﻿using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.Data.Util;
using Dev2.Diagnostics;
using Dev2.Services;
using Dev2.Session;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.AppResources.Repositories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Core.Network;
using Dev2.Threading;
using Dev2.Util;
using Dev2.Utilities;
using TechTalk.SpecFlow;

namespace Dev2.Activities.Specs.Composition
{
    [Binding]
    public class WorkflowExecutionSteps : RecordSetBases
    {
        private SubscriptionService<DebugWriterWriteMessage> _debugWriterSubscriptionService;
        private readonly AutoResetEvent _resetEvt = new AutoResetEvent(false);
        protected override void BuildDataList()
        {
            BuildShapeAndTestData();
        }

        [Then(@"the workflow execution has ""(.*)"" error")]
        public void ThenTheWorkflowExecutionHasError(string p0)
        {
        }

        [Given(@"I have a workflow ""(.*)""")]
        public void GivenIHaveAWorkflow(string workflowName)
        {
            AppSettings.LocalHost = "http://localhost:3142";
            IEnvironmentModel environmentModel = EnvironmentRepository.Instance.Source;
            environmentModel.Connect();
            var resourceModel = new ResourceModel(environmentModel);
            resourceModel.Category = "Acceptance Tests";
            resourceModel.ResourceName = workflowName;
            resourceModel.ID = Guid.NewGuid();
            resourceModel.ResourceType = ResourceType.WorkflowService;
            ResourceRepository repository = new ResourceRepository(environmentModel);
            repository.Add(resourceModel);
            _debugWriterSubscriptionService = new SubscriptionService<DebugWriterWriteMessage>(environmentModel.Connection.ServerEvents);
            
            _debugWriterSubscriptionService.Subscribe(msg => Append(msg.DebugState));
            ScenarioContext.Current.Add(workflowName, resourceModel);
            ScenarioContext.Current.Add("environment", environmentModel);
            ScenarioContext.Current.Add("resourceRepo", repository);
            ScenarioContext.Current.Add("debugStates", new List<IDebugState>());
        }

        void Append(IDebugState debugState)
        {
            List<IDebugState> debugStates;
            ScenarioContext.Current.TryGetValue("debugStates", out debugStates);
           
            debugStates.Add(debugState);
            if(debugState.IsFinalStep())
                _resetEvt.Set();

        }
        //
        //        [Given(@"workflow ""(.*)"" contains an Assign ""(.*)"" as")]
        //        public void GivenWorkflowContainsAnAssignAs(string workflowName, string activityName, Table table)
        //        {
        //
        //            DsfMultiAssignActivity assignActivity = new DsfMultiAssignActivity { DisplayName = activityName };
        //
        //            foreach(var tableRow in table.Rows)
        //            {
        //                var value = tableRow["value"];
        //                var variable = tableRow["variable"];
        //
        //                value = value.Replace('"', ' ').Trim();
        //
        //                if(value.StartsWith("="))
        //                {
        //                    value = value.Replace("=", "");
        //                    value = string.Format("!~calculation~!{0}!~~calculation~!", value);
        //                }
        //
        //                List<ActivityDTO> fieldCollection;
        //                ScenarioContext.Current.TryGetValue("fieldCollection", out fieldCollection);
        //
        //                CommonSteps.AddVariableToVariableList(variable);
        //
        //                assignActivity.FieldsCollection.Add(new ActivityDTO(variable, value, 1, true));
        //            }
        //            CommonSteps.AddActivityToActivityList(activityName, assignActivity);
        //        }

        [Given(@"""(.*)"" contains a database service ""(.*)"" with mappings")]
        public void GivenContainsADatabaseServiceWithMappings(string wf, string dbServiceName, Table table)
        {
            IEnvironmentModel environmentModel = EnvironmentRepository.Instance.Source;
            ResourceRepository repository = new ResourceRepository(environmentModel);
            repository.Load();
            var resource  = repository.Find(r => r.ResourceName.Equals(dbServiceName)).ToList();

            var dbServiceActivity = new DsfDatabaseActivity();
            dbServiceActivity.ResourceID = resource[0].ID;
            dbServiceActivity.ServiceName = resource[0].ResourceName;

            CommonSteps.AddActivityToActivityList(dbServiceName, dbServiceActivity);

            var outputSb = new StringBuilder();
            outputSb.Append("<Output>");

            foreach(var tableRow in table.Rows)
            {
                var input = tableRow["Input to Service"];
                var fromVariable = tableRow["From Variable"];
                var output = tableRow["Output from Service"];
                var toVariable = tableRow["To Variable"];

                CommonSteps.AddVariableToVariableList(output);
                CommonSteps.AddVariableToVariableList(input);
                
                if(resource.Count > 0)
                {
                    var outputs = XDocument.Parse(resource[0].Outputs);
                    
                    string recordsetName;
                    string fieldName;
                   
                    if (DataListUtil.IsValueRecordset(output))
                    {
                        recordsetName =  DataListUtil.ExtractRecordsetNameFromValue(output);
                        fieldName = DataListUtil.ExtractFieldNameFromValue(output);
                    }
                    else
                    {
                        recordsetName = fieldName = output;
                    }

                    var element = (from elements in outputs.Descendants("Output")
                                  where (string)elements.Attribute("Recordset") == recordsetName &&
                                        (string)elements.Attribute("OriginalName") == fieldName
                                  select elements).SingleOrDefault();

                    if (element != null)
                    {
                        element.SetAttributeValue("Value", toVariable);
                    }

                    outputSb.Append(element);
                }
            }

            outputSb.Append("</Outputs>");
            resource[0].Outputs = outputSb.ToString();
        }

        //[Given(@"""(.*)"" contains a Count ""(.*)"" as")]
        //public void GivenContainsACountAs(string wfName, string toolName, Table table)
        //{
            
        //}
            


        [When(@"""(.*)"" is executed")]
        public void WhenIsExecuted(string workflowName)
        {
            BuildDataList();

            var activityList = CommonSteps.GetActivityList();

            var flowSteps = new List<FlowStep>();

            TestStartNode = new FlowStep();
            flowSteps.Add(TestStartNode);
          
            foreach(var activity in activityList)
            {
                if(TestStartNode.Action == null)
                {
                    TestStartNode.Action = activity.Value;
                }
                else
                {
                    var flowStep = new FlowStep();
                    flowStep.Action = activity.Value;
                    flowSteps.Last().Next = flowStep;
                    flowSteps.Add(flowStep);
                }
            }

            IContextualResourceModel resourceModel;
            IEnvironmentModel environmentModel;
            IResourceRepository repository;
            ScenarioContext.Current.TryGetValue(workflowName, out resourceModel);
            ScenarioContext.Current.TryGetValue("environment", out environmentModel);
            ScenarioContext.Current.TryGetValue("resourceRepo", out repository);

            string currentDl = CurrentDl;
            resourceModel.DataList = currentDl.Replace("root", "DataList");
            WorkflowHelper helper = new WorkflowHelper();
            StringBuilder xamlDefinition = helper.GetXamlDefinition(FlowchartActivityBuilder);
            resourceModel.WorkflowXaml = xamlDefinition;

            repository.Save(resourceModel);
            repository.SaveToServer(resourceModel);

            ExecuteWorkflow(resourceModel);
        }

        [Then(@"the '(.*)' in WorkFlow '(.*)' debug inputs as")]
        public void ThenTheInWorkFlowDebugInputsAs(string toolName, string workflowName, Table table)
        {
            Dictionary<string, Activity> activityList;
            ScenarioContext.Current.TryGetValue("activityList", out activityList);

            var debugStates = ScenarioContext.Current.Get<List<IDebugState>>("debugStates");

            var workflowId = debugStates.First(wf => wf.DisplayName.Equals(workflowName)).ID;

            var toolSpecificDebug =
                debugStates.Where(ds => ds.OriginalInstanceID == workflowId && ds.DisplayName.Equals(toolName)).ToList();
            
            var commonSteps = new CommonSteps();
            commonSteps.ThenTheDebugInputsAs(table, toolSpecificDebug
                                                    .SelectMany(s => s.Inputs)
                                                    .SelectMany(s => s.ResultsList).ToList());
        }
            
        [Then(@"the '(.*)' in Workflow '(.*)' debug outputs as")]
        public void ThenTheInWorkflowDebugOutputsAs(string toolName, string workflowName, Table table)
        {
            Dictionary<string, Activity> activityList;
            ScenarioContext.Current.TryGetValue("activityList", out activityList);
           
            var debugStates = ScenarioContext.Current.Get<List<IDebugState>>("debugStates");
            var workflowId = debugStates.First(wf => wf.DisplayName.Equals(workflowName)).ID;

            var toolSpecificDebug =
                debugStates.Where(ds => ds.OriginalInstanceID == workflowId && ds.DisplayName.Equals(toolName)).ToList();

            var commonSteps = new CommonSteps();
            commonSteps.ThenTheDebugOutputAs(table, toolSpecificDebug
                                                    .SelectMany(s => s.Outputs)
                                                    .SelectMany(s => s.ResultsList).ToList());
        }

        public void ExecuteWorkflow(IContextualResourceModel resourceModel)
        {
            if(resourceModel == null || resourceModel.Environment == null)
            {
                return;
            }

            var debugTO = new DebugTO();
            debugTO.XmlData = "<DataList></DataList>";
            debugTO.SessionID = Guid.NewGuid();
            debugTO.IsDebugMode = true;

            var clientContext = resourceModel.Environment.Connection;
            if(clientContext != null)
            {
                var dataList = XElement.Parse(debugTO.XmlData);
                dataList.Add(new XElement("BDSDebugMode", debugTO.IsDebugMode));
                dataList.Add(new XElement("DebugSessionID", debugTO.SessionID));
                dataList.Add(new XElement("EnvironmentID", resourceModel.Environment.ID));
                WebServer.Send(WebServerMethod.POST, resourceModel, dataList.ToString(), new TestAsyncWorker());
                _resetEvt.WaitOne();
            }
        }
    }
}
