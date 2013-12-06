﻿using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Specs.Toolbox.ControlFlow.Switch
{
    [Binding]
    public class SwitchSteps : RecordSetBases
    {
        private DsfFlowSwitchActivity _flowSwitch;

        public SwitchSteps()
            : base(new List<Tuple<string, string>>())
        {
        }

        private void BuildDataList()
        {
            BuildShapeAndTestData();
            _flowSwitch = new DsfFlowSwitchActivity
                {
                    ExpressionText =
                        string.Format(
                            "Dev2.Data.Decision.Dev2DataListDecisionHandler.Instance.FetchSwitchData(\"{0}\",AmbientDataList)",
                            ((List<Tuple<string, string>>) _variableList).First().Item1)
                };

            TestStartNode = new FlowStep
                {
                    Action = _flowSwitch
                };
        }

        [Given(@"I need to switch on variable ""(.*)"" with the value ""(.*)""")]
        public void GivenINeedToSwitchOnVariableWithTheValue(string variable, string value)
        {
            _variableList.Add(new Tuple<string, string>(variable, value));
        }

        [When(@"the switch tool is executed")]
        public void WhenTheSwitchToolIsExecuted()
        {
            BuildDataList();
            _result = ExecuteProcess();
        }

        [Then(@"the variable ""(.*)"""" will evaluate to ""(.*)""")]
        public void ThenTheVariableWillEvaluateTo(string variable, string result)
        {
            string error;
            string actualValue;
            result = result.Replace("\"\"", "");
            GetScalarValueFromDataList(_result.DataListID, DataListUtil.RemoveLanguageBrackets(variable),
                                       out actualValue, out error);
            Assert.AreEqual(result, actualValue);
        }
    }
}