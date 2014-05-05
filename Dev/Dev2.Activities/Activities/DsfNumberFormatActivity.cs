﻿using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using Dev2;
using Dev2.Activities;
using Dev2.Activities.Debug;
using Dev2.Common.ExtMethods;
using Dev2.Data.Factories;
using Dev2.Data.Operations;
using Dev2.Data.TO;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Builders;
using Dev2.DataList.Contract.Value_Objects;
using Dev2.Diagnostics;
using Dev2.Util;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;

// ReSharper disable CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities
// ReSharper restore CheckNamespace
{

    public class DsfNumberFormatActivity : DsfActivityAbstract<string>
    {
        #region Class Members

        // ReSharper disable InconsistentNaming
        private static readonly IDev2NumberFormatter _numberFormatter; //  REVIEW : Should this not be an instance variable....
        // ReSharper restore InconsistentNaming

        #endregion Class Members

        #region Constructors

        public DsfNumberFormatActivity()
            : base("Format Number")
        {
            RoundingType = "None";
        }

        static DsfNumberFormatActivity()
        {
            _numberFormatter = new Dev2NumberFormatter(); // REVIEW : Please use a factory method to create

        }
        #endregion Constructors

        #region Properties

        [Inputs("Expression")]
        [FindMissing]
        public string Expression { get; set; }

        [Inputs("RoundingType")]
        public string RoundingType { get; set; }

        [Inputs("RoundingDecimalPlaces")]
        [FindMissing]
        public string RoundingDecimalPlaces { get; set; }

        [Inputs("DecimalPlacesToShow")]
        [FindMissing]
        public string DecimalPlacesToShow { get; set; }

        [Outputs("Result")]
        [FindMissing]
        public new string Result { get; set; }

        protected override bool CanInduceIdle
        {
            get
            {
                return true;
            }
        }

        #endregion Properties

        #region Override Methods

        protected override void OnExecute(NativeActivityContext context)
        {
            _debugInputs = new List<DebugItem>();
            _debugOutputs = new List<DebugItem>();
            var dataObject = context.GetExtension<IDSFDataObject>();
            var compiler = DataListFactory.CreateDataListCompiler();

            var allErrors = new ErrorResultTO();
            ErrorResultTO errors;

            var executionId = DataListExecutionID.Get(context);
            var toUpsert = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder(true);
            toUpsert.IsDebug = dataObject.IsDebugMode();
            InitializeDebug(dataObject);
            try
            {
                var expression = Expression ?? string.Empty;
                var roundingDecimalPlaces = RoundingDecimalPlaces ?? string.Empty;
                var decimalPlacesToShow = DecimalPlacesToShow ?? string.Empty;
                var colItr = Dev2ValueObjectFactory.CreateIteratorCollection();
                var expressionIterator = CreateDataListEvaluateIterator(expression, executionId, compiler, colItr, allErrors);
                var roundingDecimalPlacesIterator = CreateDataListEvaluateIterator(roundingDecimalPlaces, executionId, compiler, colItr, allErrors);
                var decimalPlacesToShowIterator = CreateDataListEvaluateIterator(decimalPlacesToShow, executionId, compiler, colItr, allErrors);

                if(dataObject.IsDebugMode())
                {
                    AddDebugInputItem(expression, "Number", expressionIterator.FetchEntry(), executionId);
                    if(!String.IsNullOrEmpty(RoundingType))
                    {
                        AddDebugInputItem(new DebugItemStaticDataParams(RoundingType, "Rounding"));
                    }
                    AddDebugInputItem(roundingDecimalPlaces, "Rounding Value", roundingDecimalPlacesIterator.FetchEntry(), executionId);
                    AddDebugInputItem(decimalPlacesToShow, "Decimals to show", decimalPlacesToShowIterator.FetchEntry(), executionId);
                }
                // Loop data ;)
                while(colItr.HasMoreData())
                {
                    int decimalPlacesToShowValue;
                    var tmpDecimalPlacesToShow = colItr.FetchNextRow(decimalPlacesToShowIterator).TheValue;
                    var adjustDecimalPlaces = tmpDecimalPlacesToShow.IsRealNumber(out decimalPlacesToShowValue);
                    if(!string.IsNullOrEmpty(tmpDecimalPlacesToShow) && !adjustDecimalPlaces)
                    {
                        throw new Exception("Decimals to show is not valid");
                    }


                    var tmpDecimalPlaces = colItr.FetchNextRow(roundingDecimalPlacesIterator).TheValue;
                    var roundingDecimalPlacesValue = 0;
                    if(!string.IsNullOrEmpty(tmpDecimalPlaces) && !tmpDecimalPlaces.IsRealNumber(out roundingDecimalPlacesValue))
                    {
                        throw new Exception("Rounding decimal places is not valid");
                    }

                    var binaryDataListItem = colItr.FetchNextRow(expressionIterator);
                    var val = binaryDataListItem.TheValue;
                    FormatNumberTO formatNumberTo = new FormatNumberTO(val, RoundingType, roundingDecimalPlacesValue, adjustDecimalPlaces, decimalPlacesToShowValue);
                    var result = _numberFormatter.Format(formatNumberTo);

                    UpdateResultRegions(toUpsert, result);
                }
                compiler.Upsert(executionId, toUpsert, out errors);
                allErrors.MergeErrors(errors);
                if(!allErrors.HasErrors())
                {
                    foreach(var debugOutputTO in toUpsert.DebugOutputs)
                    {
                        AddDebugOutputItem(new DebugItemVariableParams(debugOutputTO));
                    }
                }
            }
            catch(Exception e)
            {
                allErrors.AddError(e.Message);
            }
            finally
            {
                if(allErrors.HasErrors())
                {
                    if(dataObject.IsDebugMode())
                    {
                        AddDebugOutputItem(new DebugItemStaticDataParams("", Result, ""));
                    }
                    DisplayAndWriteError("DsfNumberFormatActivity", allErrors);
                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Dev2Error, allErrors.MakeDataListReady(), out errors);
                }

                if(dataObject.IsDebugMode())
                {
                    DispatchDebugState(context, StateType.Before);
                    DispatchDebugState(context, StateType.After);
                }
            }
        }

        void UpdateResultRegions(IDev2DataListUpsertPayloadBuilder<string> toUpsert, string result)
        {
            foreach(var region in DataListCleaningUtils.SplitIntoRegions(Result))
            {
                toUpsert.Add(region, result);
                toUpsert.FlushIterationFrame();
            }
        }

        #endregion

        #region Private Methods

        private void AddDebugInputItem(string expression, string labelText, IBinaryDataListEntry valueEntry, Guid executionId)
        {
            DebugItem itemToAdd = new DebugItem();
            if(valueEntry != null)
            {
                AddDebugItem(new DebugItemVariableParams(expression, labelText, valueEntry, executionId), itemToAdd);
            }
            else
            {
                AddDebugItem(new DebugItemStaticDataParams("", labelText, expression), itemToAdd);
            }

            _debugInputs.Add(itemToAdd);
        }

        #endregion

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

        #endregion

        #region Update ForEach Inputs/Outputs

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if(updates != null)
            {
                foreach(Tuple<string, string> t in updates)
                {

                    if(t.Item1 == Expression)
                    {
                        Expression = t.Item2;
                    }

                    if(t.Item1 == RoundingType)
                    {
                        RoundingType = t.Item2;
                    }

                    if(t.Item1 == RoundingDecimalPlaces)
                    {
                        RoundingDecimalPlaces = t.Item2;
                    }

                    if(t.Item1 == DecimalPlacesToShow)
                    {
                        DecimalPlacesToShow = t.Item2;
                    }
                }
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if(updates != null)
            {
                var itemUpdate = updates.FirstOrDefault(tuple => tuple.Item1 == Result);
                if(itemUpdate != null)
                {
                    Result = itemUpdate.Item2;
                }
            }
        }

        #endregion

        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            return GetForEachItems(Expression, RoundingType, RoundingDecimalPlaces, DecimalPlacesToShow);
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return GetForEachItems(Result);
        }

        #endregion

    }
}
