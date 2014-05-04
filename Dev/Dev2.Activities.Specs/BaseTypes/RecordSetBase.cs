﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using ActivityUnitTests;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using TechTalk.SpecFlow;

namespace Dev2.Activities.Specs.BaseTypes
{
    [Binding]
    public abstract class RecordSetBases : BaseActivityUnitTest
    {
        protected const string ResultVariable = "[[result]]";
        private int _lastAddedIndex;

        protected abstract void BuildDataList();

        protected void BuildShapeAndTestData()
        {
            var shape = new StringBuilder();
            shape.Append("<root>");

            var data = new StringBuilder();
            data.Append("<root>");

            // ReSharper disable NotAccessedVariable
            int row = 0;
            dynamic variableList;
            ScenarioContext.Current.TryGetValue("variableList", out variableList);

            if(variableList != null)
            {
                foreach(dynamic variable in variableList)
                {
                    Build(variable, shape, data, row);
                    row++;
                }
            }

            List<Tuple<string, string>> emptyRecordset;
            bool isAdded = ScenarioContext.Current.TryGetValue("rs", out emptyRecordset);
            if(isAdded)
            {
                foreach(Tuple<string, string> emptyRecord in emptyRecordset)
                {
                    shape.Append(string.Format("<{0}>", emptyRecord.Item1));
                    shape.Append(string.Format("<{0}/>", emptyRecord.Item2));
                    shape.Append(string.Format("</{0}>", emptyRecord.Item1));
                }
            }

            shape.Append("</root>");
            data.Append("</root>");

            CurrentDl = shape.ToString();
            TestData = data.ToString();
        }

        private void Build(dynamic variable, StringBuilder shape, StringBuilder data, int rowIndex)
        {
            if(DataListUtil.IsValueRecordset(variable.Item1))
            {
                dynamic recordset = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetsOnly, variable.Item1);
                dynamic recordField = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetFields, variable.Item1);

                List<string> addedRecordsets;
                ScenarioContext.Current.TryGetValue("addedRecordsets", out addedRecordsets);

                if(addedRecordsets == null)
                {
                    addedRecordsets = new List<string>();
                    ScenarioContext.Current.Add("addedRecordsets", addedRecordsets);
                }

                List<string> addedFieldset;
                ScenarioContext.Current.TryGetValue("addedFieldset", out addedFieldset);

                if(addedFieldset == null)
                {
                    addedFieldset = new List<string>();
                    ScenarioContext.Current.Add("addedFieldset", addedFieldset);
                }

                if(!(addedRecordsets.Contains(recordset) && addedFieldset.Contains(recordField)))
                {
                    shape.Append(string.Format("<{0}>", recordset));
                    shape.Append(string.Format("<{0}/>", recordField));
                    shape.Append(string.Format("</{0}>", recordset));
                    addedRecordsets.Add(recordset);
                    addedFieldset.Add(recordField);
                }

                 var indexRegionFromRecordset = DataListUtil.ExtractIndexRegionFromRecordset(variable.Item1);

                 if(!string.IsNullOrEmpty(indexRegionFromRecordset))
                 {
                     int indexForRecset;
                     int.TryParse(indexRegionFromRecordset, out indexForRecset);

                     var blankRowsToAdd = rowIndex == 0 ? 0 : (indexForRecset -1) - _lastAddedIndex;
                     
                     if(blankRowsToAdd > 0)
                     {
                         for(int i = 0; i < blankRowsToAdd; i++)
                         {
                             if(!String.IsNullOrEmpty(variable.Item2))
                             {
                                 data.Append(string.Format("<{0}>", recordset));
                                 data.Append(string.Format("<{0}></{0}>", recordField));
                                 data.Append(string.Format("</{0}>", recordset));
                             }
                             _lastAddedIndex++;
                         }
                     }
                 }


                if(!String.IsNullOrEmpty(variable.Item2))
                {
                    data.Append(string.Format("<{0}>", recordset));
                    data.Append(string.Format("<{0}>{1}</{0}>", recordField, variable.Item2));
                    data.Append(string.Format("</{0}>", recordset));
                    _lastAddedIndex++;
                }
                string rec;
                ScenarioContext.Current.TryGetValue("recordset", out rec);

                if(string.IsNullOrEmpty(rec))
                {
                    ScenarioContext.Current.Add("recordset", recordset);
                }

                string field;
                ScenarioContext.Current.TryGetValue("recordField", out field);

                if(string.IsNullOrEmpty(field))
                {
                    ScenarioContext.Current.Add("recordField", recordField);
                }
            }
            else
            {
                string variableName = DataListUtil.RemoveLanguageBrackets(variable.Item1);
                if(!String.IsNullOrEmpty(variableName))
                {
                    shape.Append(string.Format("<{0}/>", variableName));
                    data.Append(string.Format("<{0}>{1}</{0}>", variableName, variable.Item2));
                }
            }
        }

        protected string RetrieveItemForEvaluation(enIntellisensePartType partType, string value)
        {
            return CommonSteps.RetrieveItemForEvaluation(partType, value);
        }

        protected string ReadFile(string resourceName)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using(Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if(stream == null)
                {
                    return string.Empty;
                }

                using(var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}