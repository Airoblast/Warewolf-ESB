﻿using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.Data.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Specs.Toolbox.Recordset.Count
{
    [Binding]
    public class CountSteps : RecordSetBases
    {
        private void BuildDataList()
        {
            List<Tuple<string, string>> variableList;
            ScenarioContext.Current.TryGetValue("variableList", out variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                ScenarioContext.Current.Add("variableList", variableList);
            }

            variableList.Add(new Tuple<string, string>(ResultVariable, ""));
            BuildShapeAndTestData();

            string recordSetName;
            ScenarioContext.Current.TryGetValue("recordset", out recordSetName);
            
            var count = new DsfCountRecordsetActivity
                {
                    RecordsetName = string.IsNullOrEmpty(recordSetName) ? "" : recordSetName + "()",
                    CountNumber = ResultVariable
                };

            TestStartNode = new FlowStep
                {
                    Action = count
                };
        }

        [Given(@"I have a recordset with this shape")]
        public void GivenIHaveARecordsetWithThisShape(Table table)
        {
            List<TableRow> tableRows = table.Rows.ToList();

            if (tableRows.Count == 0)
            {
                var rs = table.Header.ToArray()[0];

                List<Tuple<string, string>> emptyRecordset;

                bool isAdded = ScenarioContext.Current.TryGetValue("rs", out emptyRecordset);
                if (!isAdded)
                {
                    emptyRecordset = new List<Tuple<string, string>>();
                     ScenarioContext.Current.Add("rs", emptyRecordset);
                }
                emptyRecordset.Add(new Tuple<string, string>(rs, "row"));
            }

            foreach (TableRow t in tableRows)
            {
                List<Tuple<string, string>> variableList;
                ScenarioContext.Current.TryGetValue("variableList", out variableList);

                if (variableList == null)
                {
                    variableList = new List<Tuple<string, string>>();
                    ScenarioContext.Current.Add("variableList", variableList);
                }
                variableList.Add(new Tuple<string, string>(t[0], ""));
            }
        }

        [When(@"the count tool is executed")]
        public void WhenTheCountToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(throwException:false);
            ScenarioContext.Current.Add("result", result);
        }

        [Then(@"the result count should be (.*)")]
        public void ThenTheResultCountShouldBe(string expectedResult)
        {
            string error;
            string actualValue;
            expectedResult = expectedResult.Replace("\"\"", "");
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            GetScalarValueFromDataList(result.DataListID, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                       out actualValue, out error);
            actualValue = string.IsNullOrEmpty(actualValue) ? "0" : actualValue;
            Assert.AreEqual(expectedResult, actualValue);
        }

        [Then(@"the count execution has ""(.*)"" error")]
        public void ThenTheCountExecutionHasError(string anError)
        {
            bool expected = anError.Equals("NO");
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            string fetchErrors = FetchErrors(result.DataListID);
            bool actual = string.IsNullOrEmpty(fetchErrors);
            string message = string.Format("expected {0} error but it {1}", anError.ToLower(),
                                           actual ? "did not occur" : "did occur" + fetchErrors);
             Assert.IsTrue(expected == actual, message);
        }
    }
}