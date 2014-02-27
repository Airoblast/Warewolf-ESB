﻿using Dev2;
using Dev2.Activities;
using Dev2.Activities.Debug;
using Dev2.Data.Factories;
using Dev2.Data.Util;
using Dev2.DataList;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Value_Objects;
using Dev2.Diagnostics;
using Dev2.Enums;
using Dev2.Interfaces;
using Dev2.Util;
using System;
using System.Activities;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;

// ReSharper disable CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities
// ReSharper restore CheckNamespace
{
    /// <New>
    /// Activity for finding records accoring to a search criteria that the user specifies
    /// </New>
    public class DsfFindRecordsMultipleCriteriaActivity : DsfActivityAbstract<string>, ICollectionActivity
    {
        IList<FindRecordsTO> _resultsCollection;

        #region Properties

        /// <summary>
        /// Property for holding a string the user enters into the "In Fields" box
        /// </summary>
        [Inputs("FieldsToSearch")]
        [FindMissing]
        public string FieldsToSearch { get; set; }


        /// <summary>
        /// Property for holding a string the user enters into the "Result" box
        /// </summary>
        [Outputs("Result")]
        [FindMissing]
        public new string Result { get; set; }

        /// <summary>
        /// Property for holding a string the user enters into the "Start Index" box
        /// </summary>
        [Inputs("StartIndex")]
        [FindMissing]
        public string StartIndex { get; set; }

        /// <summary>
        /// Property for holding a bool the user chooses with the "MatchCase" Checkbox
        /// </summary>
        [Inputs("MatchCase")]
        public bool MatchCase { get; set; }

        public bool RequireAllTrue { get; set; }

        public bool RequireAllFieldsToMatch { get; set; }
        #endregion Properties

        #region Ctor

        public DsfFindRecordsMultipleCriteriaActivity()
            : base("Find Record Index")
        {
            // Initialise all the properties here
            _resultsCollection = new List<FindRecordsTO>();
            FieldsToSearch = string.Empty;
            Result = string.Empty;
            StartIndex = string.Empty;
            RequireAllTrue = true;
            RequireAllFieldsToMatch = false;
        }

        #endregion Ctor

        /// <summary>
        ///     Executes the logic of the activity and calls the backend code to do the work
        ///     Also responsible for adding the results to the data list
        /// </summary>
        /// <param name="context"></param>
        protected override void OnExecute(NativeActivityContext context)
        {
            _debugInputs = new List<DebugItem>();
            _debugOutputs = new List<DebugItem>();
            var compiler = DataListFactory.CreateDataListCompiler();
            var dataObject = context.GetExtension<IDSFDataObject>();
            var errorResultTO = new ErrorResultTO();
            var allErrors = new ErrorResultTO();
            var executionID = dataObject.DataListID;

            InitializeDebug(dataObject);
            try
            {
                IList<string> toSearch = FieldsToSearch.Split(',');
                if(dataObject.IsDebug)
                {
                    AddDebugInputValues(dataObject, toSearch, compiler, executionID, ref errorResultTO);
                }

                allErrors.MergeErrors(errorResultTO);
                IEnumerable<string> results = new List<string>();
                var concatRes = string.Empty;
                var toUpsert = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder(true);
                var iterationIndex = 0;
                bool isFirstIteration = true;
                for(var i = 0; i < ResultsCollection.Count; i++)
                {
                    IDev2IteratorCollection itrCollection = Dev2ValueObjectFactory.CreateIteratorCollection();

                    IBinaryDataListEntry binaryDataListEntrySearchCrit = compiler.Evaluate(executionID, enActionType.User, ResultsCollection[i].SearchCriteria, false, out errorResultTO);
                    IDev2DataListEvaluateIterator searchCritItr = Dev2ValueObjectFactory.CreateEvaluateIterator(binaryDataListEntrySearchCrit);
                    itrCollection.AddIterator(searchCritItr);
                    allErrors.MergeErrors(errorResultTO);

                    IBinaryDataListEntry binaryDataListEntryFrom = compiler.Evaluate(executionID, enActionType.User, ResultsCollection[i].From, false, out errorResultTO);
                    IDev2DataListEvaluateIterator fromItr = Dev2ValueObjectFactory.CreateEvaluateIterator(binaryDataListEntryFrom);
                    itrCollection.AddIterator(fromItr);
                    allErrors.MergeErrors(errorResultTO);

                    IBinaryDataListEntry binaryDataListEntryTo = compiler.Evaluate(executionID, enActionType.User, ResultsCollection[i].To, false, out errorResultTO);
                    IDev2DataListEvaluateIterator toItr = Dev2ValueObjectFactory.CreateEvaluateIterator(binaryDataListEntryTo);
                    itrCollection.AddIterator(toItr);
                    allErrors.MergeErrors(errorResultTO);

                    int idx;
                    if(!Int32.TryParse(StartIndex, out idx))
                    {
                        idx = 1;
                    }
                    var toSearchList = compiler.FetchBinaryDataList(executionID, out errorResultTO);
                    allErrors.MergeErrors(errorResultTO);



                    var searchType = ResultsCollection[i].SearchType;
                    if(string.IsNullOrEmpty(searchType))
                    {
                        continue;
                    }
                    while(itrCollection.HasMoreData())
                    {
                        var currentResults = results as IList<string> ?? results.ToList();
                        var splitOn = new[] { "," };
                        var fieldsToSearch = FieldsToSearch.Split(splitOn, StringSplitOptions.RemoveEmptyEntries);

                        SearchTO searchTO;
                        IList<string> iterationResults = new List<string>();

                        if(fieldsToSearch.Length > 0)
                        {

                            foreach(var field in fieldsToSearch)
                            {
                                searchTO = DataListFactory.CreateSearchTO(field, searchType,
                                                                          itrCollection.FetchNextRow(searchCritItr)
                                                                                       .TheValue,
                                                                          idx.ToString(CultureInfo.InvariantCulture),
                                                                          Result, MatchCase,
                                                                          RequireAllFieldsToMatch,
                                                                          itrCollection.FetchNextRow(fromItr).TheValue,
                                                                          itrCollection.FetchNextRow(toItr).TheValue);
                                ValidateRequiredFields(searchTO, out errorResultTO);
                                allErrors.MergeErrors(errorResultTO);
                                (RecordsetInterrogator.FindRecords(toSearchList, searchTO,
                                                                                     out errorResultTO)).ToList().ForEach(it => iterationResults.Add(it));

                            }
                        }
                        else
                        {
                            searchTO = (SearchTO)ConvertToSearchTO(itrCollection.FetchNextRow(searchCritItr).TheValue,
                                                             searchType, idx.ToString(CultureInfo.InvariantCulture),
                                                             itrCollection.FetchNextRow(fromItr).TheValue,
                                                             itrCollection.FetchNextRow(toItr).TheValue);

                            ValidateRequiredFields(searchTO, out errorResultTO);
                            allErrors.MergeErrors(errorResultTO);
                            iterationResults = RecordsetInterrogator.FindRecords(toSearchList, searchTO,
                                                                                     out errorResultTO);

                        }

                        allErrors.MergeErrors(errorResultTO);

                        if(RequireAllTrue)
                        {
                            results = isFirstIteration ? iterationResults : currentResults.Intersect(iterationResults);
                        }
                        else
                        {
                            results = currentResults.Union(iterationResults);
                        }
                        isFirstIteration = false;
                    }
                }

                var regions = DataListCleaningUtils.SplitIntoRegions(Result);
                foreach(var region in regions)
                {
                    var allResults = results as IList<string> ?? results.ToList();
                    if(allResults.Count == 0)
                    {
                        allResults.Add("-1");
                    }

                    if(!DataListUtil.IsValueRecordset(region))
                    {
                        foreach(var r in allResults)
                        {
                            concatRes = string.Concat(concatRes, r, ",");
                        }

                        if(concatRes.EndsWith(","))
                        {
                            concatRes = concatRes.Remove(concatRes.Length - 1);
                        }
                        toUpsert.Add(region, concatRes);
                        toUpsert.FlushIterationFrame();
                    }
                    else
                    {
                        iterationIndex = 0;

                        foreach(var r in allResults)
                        {
                            toUpsert.Add(region, r);
                            toUpsert.FlushIterationFrame();
                            iterationIndex++;
                        }
                    }
                    compiler.Upsert(executionID, toUpsert, out errorResultTO);
                    allErrors.MergeErrors(errorResultTO);

                    if(dataObject.IsDebugMode() && !allErrors.HasErrors())
                    {
                        AddDebugOutputItem(new DebugOutputParams(region, "", executionID, 1));
                    }
                }
            }
            catch(Exception exception)
            {
                allErrors.AddError(exception.Message);
            }
            finally
            {
                var hasErrors = allErrors.HasErrors();
                if(hasErrors)
                {
                    DisplayAndWriteError("DsfFindRecordsMultipleCriteriaActivity", allErrors);
                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Dev2Error, allErrors.MakeDataListReady(), out errorResultTO);
                }

                if(dataObject.IsDebugMode())
                {
                    if(hasErrors)
                    {
                        var iterationIndex = 0;
                        var regions = DataListCleaningUtils.SplitIntoRegions(Result);
                        foreach(var region in regions)
                        {
                            AddDebugOutputItem(new DebugOutputParams(region, "-1", executionID, iterationIndex));
                            iterationIndex++;
                        }
                    }

                    DispatchDebugState(context, StateType.Before);
                    DispatchDebugState(context, StateType.After);
                }
            }
        }

        private void ValidateRequiredFields(SearchTO searchTO, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            if(string.IsNullOrEmpty(searchTO.FieldsToSearch))
            {
                errors.AddError("Fields to search is required");
            }

            if(string.IsNullOrEmpty(searchTO.SearchType))
            {
                errors.AddError("Search type is required");
            }

            if(searchTO.SearchType.Equals("Is Between"))
            {
                if(string.IsNullOrEmpty(searchTO.From))
                {
                    errors.AddError("From is required");
                }

                if(string.IsNullOrEmpty(searchTO.To))
                {
                    errors.AddError("To is required");
                }
            }
        }

        public override enFindMissingType GetFindMissingType()
        {
            return enFindMissingType.MixedActivity;
        }

        void AddDebugInputValues(IDSFDataObject dataObject, IEnumerable<string> toSearch, IDataListCompiler compiler, Guid executionID, ref ErrorResultTO errorTos)
        {
            if(dataObject.IsDebugMode())
            {
                var debugItem = new DebugItem();
                AddDebugItem(new DebugItemStaticDataParams("", "In Field(s)"), debugItem);
                foreach(var s in toSearch)
                {
                    var searchFields = s;
                    if(DataListUtil.IsValueRecordset(s))
                    {
                        searchFields = searchFields.Replace("()", "(*)");
                    }
                    IBinaryDataListEntry tmpEntry = compiler.Evaluate(executionID, enActionType.User, searchFields, false, out errorTos);
                    AddDebugItem(new DebugItemVariableParams(searchFields, "", tmpEntry, executionID), debugItem);
                }
                _debugInputs.Add(debugItem);
                AddResultDebugInputs(ResultsCollection, executionID, compiler);
                AddDebugInputItem(new DebugItemStaticDataParams(RequireAllFieldsToMatch ? "YES" : "NO", "Require All Fields To Match"));
                AddDebugInputItem(new DebugItemStaticDataParams(RequireAllTrue ? "YES" : "NO", "Require All Matches To Be True"));
            }
        }

        #region Private Methods

        void AddResultDebugInputs(IEnumerable<FindRecordsTO> resultsCollection, Guid executionID, IDataListCompiler compiler)
        {
            var indexCount = 1;
            foreach(var findRecordsTO in resultsCollection)
            {
                DebugItem debugItem = new DebugItem();
                if(!String.IsNullOrEmpty(findRecordsTO.SearchType))
                {
                    AddDebugItem(new DebugItemStaticDataParams("", indexCount.ToString(CultureInfo.InvariantCulture)), debugItem);
                    AddDebugItem(new DebugItemStaticDataParams(findRecordsTO.SearchType, ""), debugItem);

                    if(!string.IsNullOrEmpty(findRecordsTO.SearchCriteria))
                    {
                        var expressionsEntry = compiler.Evaluate(executionID, enActionType.User, findRecordsTO.SearchCriteria, false, out errorsTo);
                        AddDebugItem(new DebugItemVariableParams(findRecordsTO.SearchCriteria, "", expressionsEntry, executionID), debugItem);
                    }

                    if(findRecordsTO.SearchType == "Is Between" || findRecordsTO.SearchType == "Not Between")
                    {
                        var expressionsEntryFrom = compiler.Evaluate(executionID, enActionType.User, findRecordsTO.From, false, out errorsTo);
                        AddDebugItem(new DebugItemVariableParams(findRecordsTO.From, "", expressionsEntryFrom, executionID), debugItem);

                        var expressionsEntryTo = compiler.Evaluate(executionID, enActionType.User, findRecordsTO.To, false, out errorsTo);
                        AddDebugItem(new DebugItemVariableParams(findRecordsTO.To, " And", expressionsEntryTo, executionID), debugItem);
                    }

                    _debugInputs.Add(debugItem);
                    indexCount++;
                }
            }
        }

        /// <summary>
        /// Creates a new instance of the SearchTO object
        /// </summary>
        /// <returns></returns>
        private IRecsetSearch ConvertToSearchTO(string searchCriteria, string searchType, string startIndex, string from, string to)
        {
            return DataListFactory.CreateSearchTO(FieldsToSearch, searchType, searchCriteria, startIndex, Result, MatchCase, RequireAllFieldsToMatch, from, to);
        }

        void InsertToCollection(IEnumerable<string> listToAdd, ModelItem modelItem)
        {
            var modelProperty = modelItem.Properties["ResultsCollection"];
            if(modelProperty == null)
            {
                return;
            }
            var mic = modelProperty.Collection;

            if(mic == null)
            {
                return;
            }
            var listOfValidRows = ResultsCollection.Where(c => !c.CanRemove()).ToList();
            if(listOfValidRows.Count > 0)
            {
                FindRecordsTO findRecordsTO = ResultsCollection.Last(c => !c.CanRemove());
                var startIndex = ResultsCollection.IndexOf(findRecordsTO) + 1;
                foreach(var s in listToAdd)
                {
                    mic.Insert(startIndex, new FindRecordsTO(s, ResultsCollection[startIndex - 1].SearchType, startIndex + 1));
                    startIndex++;
                }
                CleanUpCollection(mic, modelItem, startIndex);
            }
            else
            {
                AddToCollection(listToAdd, modelItem);
            }
        }

        void AddToCollection(IEnumerable<string> listToAdd, ModelItem modelItem)
        {
            var modelProperty = modelItem.Properties["ResultsCollection"];
            if(modelProperty == null)
            {
                return;
            }
            var mic = modelProperty.Collection;

            if(mic == null)
            {
                return;
            }
            var startIndex = 0;
            var searchType = ResultsCollection[0].SearchType;
            mic.Clear();
            foreach(var s in listToAdd)
            {
                mic.Add(new FindRecordsTO(s, searchType, startIndex + 1));
                startIndex++;
            }
            CleanUpCollection(mic, modelItem, startIndex);
        }

        void CleanUpCollection(ModelItemCollection mic, ModelItem modelItem, int startIndex)
        {
            if(startIndex < mic.Count)
            {
                mic.RemoveAt(startIndex);
            }
            mic.Add(new XPathDTO(string.Empty, "", startIndex + 1));
            var modelProperty = modelItem.Properties["DisplayName"];
            if(modelProperty != null)
            {
                modelProperty.SetValue(CreateDisplayName(modelItem, startIndex + 1));
            }
        }

        string CreateDisplayName(ModelItem modelItem, int count)
        {
            var modelProperty = modelItem.Properties["DisplayName"];
            if(modelProperty == null)
            {
                return "";
            }
            var currentName = modelProperty.ComputedValue as string;
            if(currentName != null && (currentName.Contains("(") && currentName.Contains(")")))
            {
                currentName = currentName.Remove(currentName.Contains(" (") ? currentName.IndexOf(" (", StringComparison.Ordinal) : currentName.IndexOf("(", StringComparison.Ordinal));
            }
            currentName = currentName + " (" + (count - 1) + ")";
            return currentName;
        }

        #endregion Private Methods

        #region Get Debug Inputs/Outputs

        public override List<DebugItem> GetDebugInputs(IBinaryDataList dataList)
        {
            foreach(IDebugItem debugInput in _debugInputs)
            {
                debugInput.FlushStringBuilder();
            }
            return _debugInputs;
        }

        public override List<DebugItem> GetDebugOutputs(IBinaryDataList dataList)
        {
            foreach(IDebugItem debugOutput in _debugOutputs)
            {
                debugOutput.FlushStringBuilder();
            }
            return _debugOutputs;
        }

        #endregion Get Inputs/Outputs

        #region Get ForEach Inputs/Ouputs

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if(updates != null)
            {
                foreach(Tuple<string, string> t in updates)
                {
                    // locate all updates for this tuple
                    Tuple<string, string> t1 = t;
                    var items = ResultsCollection.Where(c => !string.IsNullOrEmpty(c.SearchCriteria) && c.SearchCriteria.Equals(t1.Item1));

                    // issues updates
                    foreach(var a in items)
                    {
                        a.SearchCriteria = t.Item2;
                    }

                    if(FieldsToSearch == t.Item1)
                    {
                        FieldsToSearch = t.Item2;
                    }
                }
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if(updates != null)
            {
                foreach(var t in updates)
                {
                    if(Result == t.Item1)
                    {
                        Result = t.Item2;
                    }
                }
            }
        }

        #endregion

        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            var items = (new[] { FieldsToSearch }).Union(ResultsCollection.Where(c => !string.IsNullOrEmpty(c.SearchCriteria)).Select(c => c.SearchCriteria)).ToArray();
            return GetForEachItems(items);
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            var items = Result;
            return GetForEachItems(items);
        }

        #endregion

        #region Implementation of ICollectionActivity

        public int GetCollectionCount()
        {
            return ResultsCollection.Count(findRecordsTO => !findRecordsTO.CanRemove());
        }

        public IList<FindRecordsTO> ResultsCollection
        {
            get
            {
                return _resultsCollection;
            }
            set
            {
                _resultsCollection = value;
            }
        }

        public void AddListToCollection(IList<string> listToAdd, bool overwrite, ModelItem modelItem)
        {
            if(!overwrite)
            {
                InsertToCollection(listToAdd, modelItem);
            }
            else
            {
                AddToCollection(listToAdd, modelItem);
            }
        }

        #endregion
    }
}