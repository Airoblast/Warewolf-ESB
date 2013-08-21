﻿using Dev2;
using Dev2.Activities;
using Dev2.Common;
using Dev2.Common.ExtMethods;
using Dev2.Data.Enums;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using Dev2.Enums;
using System;
using System.Activities;
using System.Collections.Generic;
using Dev2.Util;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Unlimited.Applications.BusinessDesignStudio.Activities.Value_Objects;
using Unlimited.Framework;

// ReSharper disable CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities
// ReSharper restore CheckNamespace
{

    public class DsfForEachActivity : DsfActivityAbstract<bool>
    {
        string _previousParentID;
        Dev2ActivityIOIteration inputItr = new Dev2ActivityIOIteration();
        #region Variables

        private string _forEachElementName;
        private string _displayName;
        int _previousInputsIndex = -1;
        int _previousOutputsIndex = -1;
        private string _inputsToken = "*";
        private string _outputsToken = "*"; 

        // ReSharper disable InconsistentNaming
        private ForEachBootstrapTO operationalData;
        // ReSharper restore InconsistentNaming

        #endregion Variables

        #region Properties

        public enForEachType ForEachType { get; set; }

        [FindMissing]
        public string From { get; set; }

        [FindMissing]
        public string To { get; set; }

        [FindMissing]
        public string Recordset { get; set; }

        [FindMissing]
        public string CsvIndexes { get; set; }

        [FindMissing]
        public string NumOfExections { get; set; }

        [Inputs("FromDisplayName")]
        [FindMissing]
        public string FromDisplayName
        {
            get
            {
                return _displayName;
            }
            set
            {
                _displayName = value;
                ForEachElementName = value;
            }
        }

        [Inputs("ForEachElementName")]
        [FindMissing]
        public string ForEachElementName
        {
            get
            {
                return _forEachElementName;
            }
            set
            {
                _forEachElementName = value;
            }
        }

        public int ExecutionCount
        {
            get
            {
                if (operationalData != null)
                {
                    return operationalData.IterationCount;
                }

                return 0;
            }
        }
        public Variable test { get; set; }
        public ActivityFunc<string, bool> DataFunc { get; set; }

        public bool FailOnFirstError { get; set; }
        public string ElementName { private set; get; }
        public string PreservedDataList { private set; get; }

        // REMOVE : Travis.Frisinger - 28.11.2012 : The two variables below are no longer required
        private Variable<IEnumerator<UnlimitedObject>> dataTags = new Variable<IEnumerator<UnlimitedObject>>("dataTags");
        private Variable<UnlimitedObject> inputData = new Variable<UnlimitedObject>("inputData");
        private List<bool> results = new List<bool>();

        // REMOVE : No longer used
        DelegateInArgument<string> actionArgument = new DelegateInArgument<string>("explicitDataFromParent");

        // used to avoid IO mapping adjustment issues ;)
        // REMOVE : 2 variables below not used any more.....
        private Variable<string> _origInput = new Variable<string>("origInput");
        private Variable<string> _origOutput = new Variable<string>("origOutput");


        #endregion Properties

        #region Ctor

        public DsfForEachActivity()
        {
            DataFunc = new ActivityFunc<string, bool>
            {
                DisplayName = "Data Action",
                Argument = new DelegateInArgument<string>(string.Format("explicitData_{0}", DateTime.Now.ToString("yyyyMMddhhmmss")))

            };
            DisplayName = "For Each";
        }

        #endregion Ctor

        #region CacheMetaData

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            metadata.AddDelegate(DataFunc);
            metadata.AddImplementationVariable(dataTags);
            metadata.AddImplementationVariable(inputData);
            metadata.AddImplementationVariable(_origInput);
            metadata.AddImplementationVariable(_origOutput);

            base.CacheMetadata(metadata);
        }

        #endregion CacheMetaData

        #region Execute

        protected override void OnBeforeExecute(NativeActivityContext context)
        {
            var dataObject = context.GetExtension<IDSFDataObject>();
            _previousParentID = dataObject.ParentInstanceID;
        }

        protected override void OnExecute(NativeActivityContext context)
        {
            _debugInputs = new List<DebugItem>();
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

            ErrorResultTO allErrors = new ErrorResultTO();
            ErrorResultTO errors;
            Guid executionID = DataListExecutionID.Get(context);

            try
            {
                ForEachBootstrapTO exePayload = FetchExecutionType(dataObject, executionID, compiler, out errors);
                if (errors.HasErrors())
                {
                    allErrors.MergeErrors(errors);
                    return;
                }
                
                //string elmName = ForEachElementName;
                if (dataObject.IsDebug || ServerLogger.ShouldLog(dataObject.ResourceID) || dataObject.RemoteInvoke)
                {                   
                    DispatchDebugState(context, StateType.Before);
                }

                dataObject.ParentInstanceID = UniqueID;

                allErrors.MergeErrors(errors);
                string error;
                ForEachInnerActivityTO innerA = GetInnerActivity(out error);
                allErrors.AddError(error);

                exePayload.InnerActivity = innerA;

                operationalData = exePayload;
                // flag it as scoped so we can use a single DataList
                dataObject.IsDataListScoped = true;

                if (exePayload.InnerActivity != null && exePayload.IndexIterator.HasMore())
                {
                    int idx = exePayload.IndexIterator.FetchNextIndex();
                    if (exePayload.ForEachType != enForEachType.NumOfExecution)
                    {
                        // set the iteration data ;)
                        IterateIOMapping(idx, context);
                    }
                    else
                    {
                        dataObject.IsDataListScoped = false;
                    }

                    // schedule the func to execute ;)
                    dataObject.ParentInstanceID = UniqueID;                    

                    context.ScheduleFunc(DataFunc, string.Empty, ActivityCompleted);
                }

            }
            catch (Exception e)
            {
                allErrors.AddError(e.Message);
            }
            finally
            {
                // Handle Errors
                if (allErrors.HasErrors())
                {
                    DisplayAndWriteError("DsfForEachActivity", allErrors);
                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Dev2Error, allErrors.MakeDataListReady(), out errors);
                    dataObject.ParentInstanceID = _previousParentID;   
                }
                if (dataObject.IsDebug || ServerLogger.ShouldLog(dataObject.ResourceID) || dataObject.RemoteInvoke)
                {
                    DispatchDebugState(context, StateType.After);
                }
            }
        }


        /// <summary>
        /// Iterates the IO mapping.
        /// </summary>
        private void IterateIOMapping(int idx, NativeActivityContext context)
        {
            string newInputs = string.Empty;
            string newOutputs = string.Empty;
            bool updateInputToken = false;
            bool updateOutputToken = false;

            // Now mutate the mappings ;)
            //Bug 8725 do not mutate mappings
            if (operationalData.InnerActivity.OrigInnerInputMapping != null)
            {
                // (*) == ({idx}) ;)
                newInputs = operationalData.InnerActivity.OrigInnerInputMapping;
                newInputs = inputItr.IterateMapping(newInputs, idx);
                newInputs = newInputs.Replace("(*)", "(" + idx + ")");
            }
            else
            {
                // coded activity

                #region Coded Activity IO ManIP

                var tmp = (operationalData.InnerActivity.InnerActivity as DsfActivityAbstract<string>);                
                               
                if (_previousInputsIndex != -1)
                {
                    if(_inputsToken != "*")
                    {
                        _inputsToken = (_previousInputsIndex).ToString();    
                    }                    
                }

                if (_previousOutputsIndex != -1)
                {
                    if (_outputsToken != "*")
                    {
                        _outputsToken = (_previousOutputsIndex).ToString();
                    }
                }

                if (tmp != null)
                {
                    IList<DsfForEachItem> data = tmp.GetForEachInputs(context);
                    IList<Tuple<string, string>> updates = new List<Tuple<string, string>>();

                    if (AmendInputs(idx, data, _inputsToken, updates))
                    {
                        updateInputToken = true;
                    }

                    // push updates for Inputs
                    tmp.UpdateForEachInputs(updates, context);
                    if (idx == 1)
                    {
                        operationalData.InnerActivity.OrigCodedInputs = updates;
                    }

                    operationalData.InnerActivity.CurCodedInputs = updates;

                    // Process outputs
                    data = tmp.GetForEachOutputs(context);
                    updates = new List<Tuple<string, string>>();

                    if (AmendOutputs(idx, data, _outputsToken, updates))
                    {
                        updateOutputToken = true;
                    }

                    // push updates 
                    tmp.UpdateForEachOutputs(updates, context);
                    if (idx == 1)
                    {
                        operationalData.InnerActivity.OrigCodedOutputs = updates;
                    }

                    operationalData.InnerActivity.CurCodedOutputs = updates;
                }
                else if (tmp == null)
                {
                    var tmp2 = (operationalData.InnerActivity.InnerActivity as DsfActivityAbstract<bool>);

                    if (tmp2 != null && !(tmp2 is DsfForEachActivity))
                    {
                        IList<DsfForEachItem> data = tmp2.GetForEachInputs(context);
                        IList<Tuple<string, string>> updates = new List<Tuple<string, string>>();

                        if (AmendInputs(idx, data, _inputsToken, updates))
                        {
                            updateInputToken = true;
                        }

                        // push updates 
                        tmp2.UpdateForEachInputs(updates, context);
                        if (idx == 1)
                        {
                            operationalData.InnerActivity.OrigCodedInputs = updates;
                        }
                        operationalData.InnerActivity.CurCodedInputs = updates;

                        // Process outputs
                        data = tmp2.GetForEachOutputs(context);
                        updates = new List<Tuple<string, string>>();

                        if (AmendOutputs(idx, data, _outputsToken, updates))
                        {
                            updateOutputToken = true;
                        }

                        // push updates 
                        tmp2.UpdateForEachOutputs(updates, context);
                        if (idx == 1)
                        {
                            operationalData.InnerActivity.OrigCodedOutputs = updates;
                        }

                        operationalData.InnerActivity.CurCodedOutputs = updates;
                    }
                }               
                #endregion
            }

            //Bug 8725 do not mutate mappings
            if (operationalData.InnerActivity.OrigInnerOutputMapping != null)
            {
                // (*) == ({idx}) ;)
                newOutputs = operationalData.InnerActivity.OrigInnerOutputMapping;
                //newOutputs = newOutputs.Replace("(*)", "(" + idx + ")");
                newOutputs = inputItr.IterateMapping(newOutputs, idx);
            }

            var dev2ActivityIoMapping = DataFunc.Handler as IDev2ActivityIOMapping;
            if (dev2ActivityIoMapping != null)
            {
                dev2ActivityIoMapping.InputMapping = newInputs;
            }

            var activityIoMapping = DataFunc.Handler as IDev2ActivityIOMapping;
            if (activityIoMapping != null)
            {
                activityIoMapping.OutputMapping = newOutputs;
            }
            if (updateInputToken)
            {
                _inputsToken = idx.ToString();
            }
            if (updateOutputToken)
            {
                _outputsToken = idx.ToString();
            }
        }

        static bool AmendInputs(int idx, IList<DsfForEachItem> data, string token, IList<Tuple<string, string>> updates)
        {
            bool result = false;
            // amend inputs ;)
            foreach (DsfForEachItem d in data)
            {
                string input = d.Value;
                if (input.Contains("(" + token + ")"))
                {
                    input = input.Replace("(" + token + ")", "(" + idx + ")");
                    result = true;    
                }                
                updates.Add(new Tuple<string, string>(d.Value, input));
            }
            return result;
        }

        static bool AmendOutputs(int idx, IList<DsfForEachItem> data, string token, IList<Tuple<string, string>> updates)
        {
            bool result = false;
            // amend inputs ;)
            foreach (DsfForEachItem d in data)
            {
                string input = d.Value;
                if (input.Contains("(" + token + ")"))
                {
                    input = input.Replace("(" + token + ")", "(" + idx + ")");
                    result = true;
                }
                updates.Add(new Tuple<string, string>(d.Value, input));
            }
            return result;
        }
       
        /// <summary>
        /// Fetches the type of the execution.
        /// </summary>        
        /// <param name="dataObject">The data object.</param>
        /// <param name="dlID">The dl ID.</param>
        /// <param name="compiler">The compiler.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>                
        private ForEachBootstrapTO FetchExecutionType(IDSFDataObject dataObject, Guid dlID, IDataListCompiler compiler, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();

            if (dataObject.IsDebug || ServerLogger.ShouldLog(dataObject.ResourceID) || dataObject.RemoteInvoke)
            {
                DebugItem itemToAdd = new DebugItem();
                itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Value, Value = ForEachType.GetDescription() });
                if(ForEachType == enForEachType.NumOfExecution && !string.IsNullOrEmpty(NumOfExections))
                {
                    IBinaryDataListEntry numOfExectionsEntry = compiler.Evaluate(dlID, enActionType.User, NumOfExections, false, out errors);
                    itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = "Number" });
                    itemToAdd.AddRange(CreateDebugItemsFromEntry(NumOfExections,numOfExectionsEntry,dlID,enDev2ArgumentType.Input));
                }
                if (ForEachType == enForEachType.InCSV && !string.IsNullOrEmpty(CsvIndexes))
                {
                    IBinaryDataListEntry csvIndexesEntry = compiler.Evaluate(dlID, enActionType.User, CsvIndexes, false, out errors);
                    itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = "Csv Indexes" });
                    itemToAdd.AddRange(CreateDebugItemsFromEntry(CsvIndexes, csvIndexesEntry, dlID, enDev2ArgumentType.Input));
                }
                if (ForEachType == enForEachType.InRange && !string.IsNullOrEmpty(From))
                {
                    IBinaryDataListEntry fromEntry = compiler.Evaluate(dlID, enActionType.User, From, false, out errors);
                    itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = "From" });
                    itemToAdd.AddRange(CreateDebugItemsFromEntry(From, fromEntry, dlID, enDev2ArgumentType.Input));
                }
                if (ForEachType == enForEachType.InRange && !string.IsNullOrEmpty(To))
                {
                    IBinaryDataListEntry toEntry = compiler.Evaluate(dlID, enActionType.User, To, false, out errors);
                    itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = "To" });
                    itemToAdd.AddRange(CreateDebugItemsFromEntry(To, toEntry, dlID, enDev2ArgumentType.Input));
                }
                if (ForEachType == enForEachType.InRecordset && !string.IsNullOrEmpty(Recordset))
                {
                    var toEmit = Recordset.Replace("()", "(*)");
                    IBinaryDataListEntry toEntry = compiler.Evaluate(dlID, enActionType.User, toEmit, false, out errors);
                    itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = "Recordset" });
                    itemToAdd.AddRange(CreateDebugItemsFromEntry(Recordset, toEntry, dlID, enDev2ArgumentType.Input));
                }
                _debugInputs.Add(itemToAdd);
                
            }

            errors = new ErrorResultTO();
            //ForEachBootstrapTO result = new ForEachBootstrapTO(enForEachExecutionType.Scalar, 0, null);

            var result = new ForEachBootstrapTO(ForEachType, From, To, CsvIndexes, NumOfExections, Recordset, dlID,compiler, out errors);

            return result;

        }

        /// <summary>
        /// Restores the handler fn.
        /// </summary>
        private void RestoreHandlerFn(NativeActivityContext context)
        {

            var activity = (DataFunc.Handler as IDev2ActivityIOMapping);

            if (activity != null)
            {

                if (operationalData.InnerActivity.OrigCodedInputs != null)
                {

                    //MO - CHANGE:This is to be reinstanciated for restoring activies back to state with star
                    #region Coded Activity
                    var tmp = (operationalData.InnerActivity.InnerActivity as DsfActivityAbstract<string>);

                    int idx = operationalData.IterationCount;

                    if (tmp != null)
                    {
                        // Restore Inputs ;)
                        IList<DsfForEachItem> data = tmp.GetForEachInputs(context);
                        IList<Tuple<string, string>> updates = new List<Tuple<string, string>>();

                        // amend inputs ;)
                        foreach (DsfForEachItem d in data)
                        {
                            string input = d.Value;
                            input = input.Replace("(" + idx + ")", "(*)");

                            updates.Add(new Tuple<string, string>(d.Value, input));
                        }

                        // push updates for Inputs
                        tmp.UpdateForEachInputs(updates, context);


                        // Restore Outputs ;)
                        data = tmp.GetForEachInputs(context);
                        updates = new List<Tuple<string, string>>();

                        // amend inputs ;)
                        foreach (DsfForEachItem d in data)
                        {
                            string input = d.Value;
                            input = input.Replace("(" + idx + ")", "(*)");

                            updates.Add(new Tuple<string, string>(d.Value, input));
                        }

                        // push updates for Inputs
                        tmp.UpdateForEachOutputs(updates, context);

                    }
                    else
                    {
                        var tmp2 = (operationalData.InnerActivity.InnerActivity as DsfActivityAbstract<bool>);

                        // Restore Inputs ;)
                        if (tmp2 != null)
                        {
                            IList<DsfForEachItem> data = tmp2.GetForEachInputs(context);
                            IList<Tuple<string, string>> updates = new List<Tuple<string, string>>();

                            // amend inputs ;)
                            foreach (DsfForEachItem d in data)
                            {
                                string input = d.Value;
                                input = input.Replace("(" + idx + ")", "(*)");

                                updates.Add(new Tuple<string, string>(d.Value, input));
                            }

                            // push updates for Inputs
                            tmp2.UpdateForEachInputs(updates, context);


                            // Restore Outputs ;)
                            data = tmp2.GetForEachInputs(context);
                            updates = new List<Tuple<string, string>>();

                            // amend inputs ;)
                            foreach (DsfForEachItem d in data)
                            {
                                string input = d.Value;
                                input = input.Replace("(" + idx + ")", "(*)");

                                updates.Add(new Tuple<string, string>(d.Value, input));
                            }

                            // push updates for Inputs
                            tmp2.UpdateForEachOutputs(updates, context);
                        }
                    }
                    #endregion
                }
                else
                {
                    activity.InputMapping = operationalData.InnerActivity.OrigInnerInputMapping;
                    activity.OutputMapping = operationalData.InnerActivity.OrigInnerOutputMapping;
                }
            }
            else
            {
                throw new Exception("DsfForEachActivity - RestoreHandlerFunction has encountered a null Function");
            }
        }

        private ForEachInnerActivityTO GetInnerActivity(out string error)
        {
            ForEachInnerActivityTO result = null;
            error = string.Empty;

            try
            {
                var tmp = DataFunc.Handler as IDev2ActivityIOMapping;

                if (tmp == null)
                {
                    error = "Can not execute a For Each with no content";
                }
                else
                {
                    result = new ForEachInnerActivityTO(tmp);
                }
            }
            catch (Exception e)
            {
                error = e.Message;
            }


            return result;
        }

        private void ActivityCompleted(NativeActivityContext context, ActivityInstance instance, bool result)
        {

            var dataObject = context.GetExtension<IDSFDataObject>();
            if (dataObject != null && operationalData != null)
            {
                operationalData.IncIterationCount();

                if (operationalData.IndexIterator.HasMore())
                {
                    var idx = operationalData.IndexIterator.FetchNextIndex();
                    // Re-jigger the mapping ;)
                    if(operationalData.ForEachType != enForEachType.NumOfExecution)
                    {
                        IterateIOMapping(idx, context);    
                    }                    
                    dataObject.ParentInstanceID = UniqueID;
                    // ReSharper disable RedundantTypeArgumentsOfMethod
                    context.ScheduleFunc<string, bool>(DataFunc, UniqueID, ActivityCompleted);
                    // ReSharper restore RedundantTypeArgumentsOfMethod
                    return;
                }

                // that is all she wrote ;)
                dataObject.IsDataListScoped = false;
                // return it all to normal
                if (ForEachType != enForEachType.NumOfExecution)
                {
                    RestoreHandlerFn(context);
                }

                dataObject.ParentInstanceID = _previousParentID;
            }
        }

        #endregion Execute

        #region Private Methods

        private void AddDebugInputItem(string expression, string labelText, IBinaryDataListEntry valueEntry, Guid executionId)
        {
            DebugItem itemToAdd = new DebugItem();

            if (!string.IsNullOrWhiteSpace(labelText))
            {
                itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = labelText });
            }

            if (valueEntry != null)
            {
                itemToAdd.AddRange(CreateDebugItemsFromEntry(expression, valueEntry, executionId, enDev2ArgumentType.Input));
            }

            _debugInputs.Add(itemToAdd);
        }


        #endregion Private Methodss

        #region Get Debug Inputs/Outputs

        public override List<DebugItem> GetDebugInputs(IBinaryDataList dataList)
        {
            foreach (IDebugItem debugInput in _debugInputs)
            {
                debugInput.FlushStringBuilder();
            }
            return _debugInputs;
        }

        public override List<DebugItem> GetDebugOutputs(IBinaryDataList dataList)
        {
            return DebugItem.EmptyList;
        }

        #endregion Get Inputs/Outputs

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            throw new NotImplementedException();
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            throw new NotImplementedException();
        }

        public override enFindMissingType GetFindMissingType()
        {
            return enFindMissingType.ForEach;
        }

        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs(NativeActivityContext context)
        {
            return GetForEachItems(context, StateType.Before, ForEachElementName);
        }

        public override IList<DsfForEachItem> GetForEachOutputs(NativeActivityContext context)
        {
            return GetForEachItems(context, StateType.After, ForEachElementName.Replace("*", ""));
        }

        #endregion

    }
}
