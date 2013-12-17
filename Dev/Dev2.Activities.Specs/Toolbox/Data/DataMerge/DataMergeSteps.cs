﻿using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.Data.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Specs.Toolbox.Data.DataMerge
{
    [Binding]
    public class DataMergeSteps : RecordSetBases
    {
        private DsfDataMergeActivity _dataMerge;

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

            _dataMerge = new DsfDataMergeActivity {Result = ResultVariable};

            List<Tuple<string, string, string, string, string>> mergeCollection;
            ScenarioContext.Current.TryGetValue("mergeCollection", out mergeCollection);

            int row = 1;
            foreach (var variable in mergeCollection)
            {
                _dataMerge.MergeCollection.Add(new DataMergeDTO(variable.Item1, variable.Item2, variable.Item3, row,
                                                                variable.Item4, variable.Item5));
                row++;
            }
            
            TestStartNode = new FlowStep
                {
                    Action = _dataMerge
                };
          
        }

        [Given(@"a merge variable ""(.*)"" equal to ""(.*)""")]
        public void GivenAMergeVariableEqualTo(string variable, string value)
        {
            List<Tuple<string, string>> variableList;
            ScenarioContext.Current.TryGetValue("variableList", out variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                ScenarioContext.Current.Add("variableList", variableList);
            }

            variableList.Add(new Tuple<string, string>(variable, value));
        }

        [Given(
            @"an Input ""(.*)"" and merge type ""(.*)"" and string at as ""(.*)"" and Padding ""(.*)"" and Alignment ""(.*)"""
            )]
        public void GivenAnInputAndMergeTypeAndStringAtAsAndPaddingAndAlignment(string input, string mergeType,
                                                                                string stringAt, string padding,
                                                                                string alignment)
        {
            List<Tuple<string, string, string, string, string>> mergeCollection;
            ScenarioContext.Current.TryGetValue("mergeCollection", out mergeCollection);

            if (mergeCollection == null)
            {
                mergeCollection = new List<Tuple<string, string, string, string, string>>();
                ScenarioContext.Current.Add("mergeCollection", mergeCollection);
            }

            mergeCollection.Add(new Tuple<string, string, string, string, string>(input, mergeType, stringAt, padding,
                                                                                   alignment));
        }

        [Given(@"a merge recordset")]
        public void GivenAMergeRecordset(Table table)
        {
            List<TableRow> records = table.Rows.ToList();

            if (records.Count == 0)
            {
                var rs = table.Header.ToArray()[0];
                var field = table.Header.ToArray()[1];

                List<Tuple<string, string>> emptyRecordset;

                bool isAdded = ScenarioContext.Current.TryGetValue("rs", out emptyRecordset);
                if (!isAdded)
                {
                    emptyRecordset = new List<Tuple<string, string>>();
                     ScenarioContext.Current.Add("rs", emptyRecordset);
                }
                emptyRecordset.Add(new Tuple<string, string>(rs, field));
            }

            foreach (TableRow record in records)
            {
                List<Tuple<string, string>> variableList;
                ScenarioContext.Current.TryGetValue("variableList", out variableList);

                if (variableList == null)
                {
                    variableList = new List<Tuple<string, string>>();
                    ScenarioContext.Current.Add("variableList", variableList);
                }
                variableList.Add(new Tuple<string, string>(record[0], record[1]));
            }
        }

        [When(@"the data merge tool is executed")]
        public void WhenTheDataMergeToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(throwException:false);
            ScenarioContext.Current.Add("result", result);
        }

        [Then(@"the merged result is ""(.*)""")]
        public void ThenTheMergedResultIs(string value)
        {
            string error;
            string actualValue;
            value = value.Replace("\"\"", "");
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            GetScalarValueFromDataList(result.DataListID, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                       out actualValue, out error);
            Assert.AreEqual(value, actualValue);
        }

        [Then(@"the data merge execution has ""(.*)"" error")]
        public void ThenTheDataMergeExecutionHasError(string anError)
        {
            bool expected = anError.Equals("NO");
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            string fetchErrors = FetchErrors(result.DataListID);
            bool actual = string.IsNullOrEmpty(fetchErrors);
            string message = string.Format("expected {0} error but it {1}", anError.ToLower(),
                                           actual ? "did not occur" : "did occur" + fetchErrors);
             Assert.IsTrue(expected == actual, message);
        }

        [Then(@"the merged result is the same as file ""(.*)""")]
        public void ThenTheMergedResultIsTheSameAsFile(string fileName)
        {
            string resourceName = string.Format("Dev2.Activities.Specs.Toolbox.Data.DataMerge.{0}",
                                                fileName);
            string value = ReadFile(resourceName);
            string error;
            string actualValue;
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            GetScalarValueFromDataList(result.DataListID, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                       out actualValue, out error);
            Assert.AreEqual(value, actualValue);
        }
    }
}