﻿using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.ComponentModel;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.Common.Enums;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;

namespace Dev2.Activities.Specs.Toolbox.Utility.Random
{
    [Binding]
    public class RandomSteps : RecordSetBases
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

            enRandomType randomType;
            ScenarioContext.Current.TryGetValue("randomType", out randomType);
            string length;
            ScenarioContext.Current.TryGetValue("length", out length);
            string rangeFrom;
            ScenarioContext.Current.TryGetValue("rangeFrom", out rangeFrom);
            string rangeTo;
            ScenarioContext.Current.TryGetValue("rangeTo", out rangeTo);

            var dsfRandom = new DsfRandomActivity
                {
                    RandomType = randomType,
                    Result = ResultVariable
                };

            if (!string.IsNullOrEmpty(length))
            {
                dsfRandom.Length = length;
            }

            if (!string.IsNullOrEmpty(rangeFrom))
            {
                dsfRandom.From = rangeFrom;
            }

            if (!string.IsNullOrEmpty(rangeTo))
            {
                dsfRandom.To = rangeTo;
            }

            TestStartNode = new FlowStep
                {
                    Action = dsfRandom
                };
        }

        [Given(@"I have a type as ""(.*)""")]
        public void GivenIHaveATypeAs(string randomType)
        {
            ScenarioContext.Current.Add("randomType", (enRandomType) Enum.Parse(typeof (enRandomType), randomType));
        }

        [Given(@"I have a length as ""(.*)""")]
        public void GivenIHaveALengthAs(string length)
        {
            ScenarioContext.Current.Add("length", length);
        }

        [Given(@"I have a range from ""(.*)"" to ""(.*)""")]
        public void GivenIHaveARangeFromTo(string rangeFrom, string rangeTo)
        {
            ScenarioContext.Current.Add("rangeFrom", rangeFrom);
            ScenarioContext.Current.Add("rangeTo", rangeTo);
        }


        [When(@"the random tool is executed")]
        public void WhenTheRandomToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(throwException:false);
            ScenarioContext.Current.Add("result", result);
        }

        [Then(@"the result from the random tool should be of type ""(.*)"" with a length of ""(.*)""")]
        public void ThenTheResultFromTheRandomToolShouldBeOfTypeWithALengthOf(string type, int length)
        {
            string error;
            string actualValue;
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            GetScalarValueFromDataList(result.DataListID, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                       out actualValue, out error);
            TypeConverter converter = TypeDescriptor.GetConverter(Type.GetType(type));
            object res = converter.ConvertFrom(actualValue);
            Assert.AreEqual(length, actualValue.Length);
        }

        [Then(@"the random value will be ""(.*)""")]
        public void ThenTheRandomValueWillBe(string value)
        {
            string error;
            string actualValue;
            value = value.Replace("\"\"", "");
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            GetScalarValueFromDataList(result.DataListID, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                       out actualValue, out error);
            Assert.AreEqual(value, actualValue);
        }

        [Then(@"random execution has ""(.*)"" error")]
        public void ThenRandomExecutionHasError(string anError)
        {
            bool expected = anError.Equals("NO");
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            bool actual = string.IsNullOrEmpty(FetchErrors(result.DataListID));
            string message = string.Format("expected {0} error but an error was {1}", anError,
                                           actual ? "not found" : "found");
            Assert.AreEqual(expected, actual, message);
        }
    }
}