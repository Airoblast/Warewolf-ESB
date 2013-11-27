﻿using System;
using System.Activities;
using System.Collections.Generic;
using Dev2.Common;
using Dev2.Common.Enums;
using Dev2.Common.ExtMethods;
using Dev2.Data.Factories;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Builders;
using Dev2.DataList.Contract.Value_Objects;
using Dev2.Diagnostics;
using Dev2.Enums;
using Dev2.Util;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;

namespace Dev2.Activities
{
    public class DsfRandomActivity : DsfActivityAbstract<string>
    {

        #region Properties

        [FindMissing]
        [Inputs("Length")]
        public string Length { get; set; }

        public enRandomType RandomType { get; set; }       

        [FindMissing]
        [Inputs("From")]
        public string From { get; set; }

        [FindMissing]
        [Inputs("To")]
        public string To { get; set; }

        [FindMissing]
        [Outputs("Result")]
        public string Result { get; set; }

        #endregion

        #region Ctor

        public DsfRandomActivity()
            : base("Random")
        {
            Length = string.Empty;
            RandomType = enRandomType.Numbers;
            Result = string.Empty;         
            From = string.Empty;
            To = string.Empty;
        }

        #endregion

        #region Overrides of DsfNativeActivity<string>

        /// <summary>
        /// When overridden runs the activity's execution logic 
        /// </summary>
        /// <param name="context">The context to be used.</param>
        protected override void OnExecute(NativeActivityContext context)
        {
            _debugInputs = new List<DebugItem>();
            _debugOutputs = new List<DebugItem>();
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();

            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler(); 

            Guid dlID = dataObject.DataListID;
            ErrorResultTO allErrors = new ErrorResultTO();
            ErrorResultTO errors = new ErrorResultTO();
            Guid executionId = dlID;         
            allErrors.MergeErrors(errors);
            

            try
            {
                if (!errors.HasErrors())
                {
                    IDev2DataListUpsertPayloadBuilder<string> toUpsert = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder(true);
                    IDev2IteratorCollection colItr = Dev2ValueObjectFactory.CreateIteratorCollection();

                    IDev2DataListEvaluateIterator lengthItr = CreateDataListEvaluateIterator(Length, executionId, compiler, colItr, allErrors);
                    IBinaryDataListEntry lengthEntry = compiler.Evaluate(executionId, enActionType.User, Length, false, out errors);                    

                    IDev2DataListEvaluateIterator fromItr = CreateDataListEvaluateIterator(From, executionId, compiler, colItr, allErrors);
                    IBinaryDataListEntry fromEntry = compiler.Evaluate(executionId, enActionType.User, From, false, out errors);

                    IDev2DataListEvaluateIterator toItr = CreateDataListEvaluateIterator(To, executionId, compiler, colItr, allErrors);
                    IBinaryDataListEntry toEntry = compiler.Evaluate(executionId, enActionType.User, To, false, out errors);

                    if (dataObject.IsDebug || ServerLogger.ShouldLog(dataObject.ResourceID) || dataObject.RemoteInvoke)
                    {
                        AddDebugInputItem(Length, From, To, fromEntry, toEntry, lengthEntry, executionId, RandomType);
                    }
                    Dev2Random dev2Random = new Dev2Random();
                    int iterationCounter = 0;
                    while(colItr.HasMoreData())
                    {                       
                        int lengthNum = -1;                       
                        int fromNum = -1;
                        int toNum = -1;
                   
                        string fromValue = colItr.FetchNextRow(fromItr).TheValue;
                        string toValue = colItr.FetchNextRow(toItr).TheValue;
                        string lengthValue = colItr.FetchNextRow(lengthItr).TheValue;

                        if(RandomType != enRandomType.Guid)
                        {                           
                            if(RandomType == enRandomType.Numbers)
                            {
                                #region Getting the From

                                fromNum = GetFromValue(fromValue, out errors);
                                if(errors.HasErrors())
                                {
                                    allErrors.MergeErrors(errors);
                                    continue;
                                }

                                #endregion

                                #region Getting the To

                                toNum = GetToValue(toValue, out errors);
                                if(errors.HasErrors())
                                {
                                    allErrors.MergeErrors(errors);
                                    continue;
                                }

                                #endregion
                            }
                            else
                            {
                                #region Getting the Length

                                lengthNum = GetLengthValue(lengthValue, out errors);
                                if(errors.HasErrors())
                                {
                                    allErrors.MergeErrors(errors);
                                    continue;
                                }                                

                                #endregion    
                            }                            
                        }                                                                    
                        string value = dev2Random.GetRandom(RandomType, lengthNum, fromNum, toNum);

                        //2013.06.03: Ashley Lewis for bug 9498 - handle multiple regions in result
                        foreach (var region in DataListCleaningUtils.SplitIntoRegions(Result))
                        {
                            toUpsert.Add(region, value);
                            toUpsert.FlushIterationFrame();
                            if (dataObject.IsDebug || ServerLogger.ShouldLog(dataObject.ResourceID) || dataObject.RemoteInvoke)
                            {
                                AddDebugOutputItem(region, value, executionId, iterationCounter);
                            }
                            iterationCounter++;
                        }
                    }
                    compiler.Upsert(executionId, toUpsert, out errors);
                    allErrors.MergeErrors(errors);
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
                    DisplayAndWriteError("DsfRandomActivity", allErrors);
                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Dev2Error, allErrors.MakeDataListReady(), out errors);
                }
                if (dataObject.IsDebug || ServerLogger.ShouldLog(dataObject.ResourceID) || dataObject.RemoteInvoke)
                {
                    DispatchDebugState(context, StateType.Before);
                    DispatchDebugState(context, StateType.After);
                }
            }
        }                  

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if(updates != null)
            {
                foreach (Tuple<string, string> t in updates)
                {

                    if (t.Item1 == From)
                    {
                        From = t.Item2;
                    }

                    if (t.Item1 == To)
                    {
                        To = t.Item2;
                    }

                    if (t.Item1 == Length)
                    {
                        Length = t.Item2;
                    }                
                }
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if (updates != null && updates.Count == 1)
            {
                Result = updates[0].Item2;
            }
        }

        #endregion

        #region Private Methods        

        private int GetFromValue(string fromValue, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            int fromNum;
            if (string.IsNullOrEmpty(fromValue))
            {
                errors.AddError("Please ensure that you have entered an integer for Start.");
                return -1;
            }
            if (!int.TryParse(fromValue, out fromNum))
            {
                errors.AddError("Please ensure that the Start is an integer.");
                return -1;
            }
            return fromNum;
        }

        private int GetToValue(string toValue, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            int toNum;
            if (string.IsNullOrEmpty(toValue))
            {
                errors.AddError("Please ensure that you have entered an integer for End.");
                return -1;
            }
            if (!int.TryParse(toValue, out toNum))
            {
                errors.AddError("Please ensure that the End is an integer.");
                return -1;
            }
            return toNum;
        }

        private int GetLengthValue(string lengthValue, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            int lengthNum;
            if (string.IsNullOrEmpty(lengthValue))
            {
                errors.AddError("Please ensure that you have entered an integer for Length.");
                return -1;
            }

            if (!int.TryParse(lengthValue, out lengthNum))
            {
                errors.AddError("Please ensure that the Length is an integer value.");
                return -1;
            }

            if(lengthNum < 1)
            {
                errors.AddError("Please enter a positive integer for the Length.");
                return -1;
            }

            return lengthNum;
        }

        private void AddDebugInputItem(string lengthExpression, string fromExpression, string toExpression, IBinaryDataListEntry fromEntry, IBinaryDataListEntry toEntry, IBinaryDataListEntry lengthEntry, Guid executionId, enRandomType randomType)
        {
            DebugItem itemToAdd = new DebugItem();
            itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = "Generate Random" });
            itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Value, Value = randomType.GetDescription() });
            _debugInputs.Add(itemToAdd);

            itemToAdd = new DebugItem();
            if (randomType == enRandomType.Guid)
            {
               return;
            }
            if (randomType == enRandomType.Numbers)
            {
                itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = "Between" });
                itemToAdd.AddRange(CreateDebugItemsFromEntry(fromExpression, fromEntry, executionId, enDev2ArgumentType.Input));
                itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = "And" });
                itemToAdd.AddRange(CreateDebugItemsFromEntry(toExpression, toEntry, executionId, enDev2ArgumentType.Input));
            }
            else
            {
                itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = "Length" });
                itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = GlobalConstants.EqualsExpression });
                itemToAdd.AddRange(CreateDebugItemsFromEntry(lengthExpression, lengthEntry, executionId, enDev2ArgumentType.Input));
            }

            
            _debugInputs.Add(itemToAdd);
        }

        private void AddDebugOutputItem(string result, string value, Guid dlId, int iterationCounter)
        {
            DebugItem itemToAdd = new DebugItem();
            itemToAdd.AddRange(CreateDebugItemsFromString(result, value, dlId, iterationCounter, enDev2ArgumentType.Output));            
            _debugOutputs.Add(itemToAdd);
        }

        #endregion

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
            foreach (IDebugItem debugOutput in _debugOutputs)
            {
                debugOutput.FlushStringBuilder();
            }
            return _debugOutputs;
        }

        #endregion Get Inputs/Outputs

        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            return GetForEachItems(To,From,Length);
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return GetForEachItems(Result);
        }

        #endregion
    }
}
