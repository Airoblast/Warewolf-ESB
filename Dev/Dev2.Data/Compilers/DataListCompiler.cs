﻿using System.Data;
using Dev2.Common;
using Dev2.Common.Enums;
using Dev2.Data.Binary_Objects;
using Dev2.Data.SystemTemplates;
using Dev2.Data.SystemTemplates.Models;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Builders;
using Dev2.DataList.Contract.EqualityComparers;
using Dev2.DataList.Contract.TO;
using Dev2.Diagnostics;
using Dev2.Server.Datalist;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Dev2.DataList.Contract
{
    internal class DataListCompiler : IDataListCompiler
    {
        #region Attributes

        private object _disposeGuard = new object();
        private bool _isDisposed = false;

        // New Stuff
        private static readonly IDev2LanguageParser _outputParser = DataListFactory.CreateOutputParser();
        private static readonly IDev2LanguageParser _inputParser = DataListFactory.CreateInputParser();
        private static readonly IDev2DataLanguageParser _parser = DataListFactory.CreateLanguageParser();

        // These are tags to strip from the ADL for ExtractShapeFromADLAndCleanWithDefs used with ShapeInput ;)

        private Dictionary<IDataListVerifyPart, string> _uniqueWorkflowParts = new Dictionary<IDataListVerifyPart, string>();
        private IEnvironmentModelDataListCompiler _svrCompiler;
        #endregion

        internal DataListCompiler(IEnvironmentModelDataListCompiler svrC)
        {
            // TODO : Allow IP to be sent when using the DataList compiler...
            _svrCompiler = svrC;
        }

        // Travis.Frisinger : 29.10.2012 - New DataListCompiler Methods
        #region New Methods

        /// <summary>
        /// Clones the data list.
        /// </summary>
        /// <param name="curDLID">The cur DLID.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public Guid CloneDataList(Guid curDLID, out ErrorResultTO errors)
        {
            return _svrCompiler.CloneDataList(curDLID, out errors);
        }

        /// <summary>
        /// Used to evalaute an expression against a given datalist
        /// </summary>
        /// <param name="curDLID">The cur DL ID.</param>
        /// <param name="typeOf">The type of evaluation.</param>
        /// <param name="expression">The expression.</param>
        /// <param name="toRoot"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        public IBinaryDataListEntry Evaluate(Guid curDLID, enActionType typeOf, string expression, bool toRoot, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();

            return _svrCompiler.Evaluate(null, curDLID, typeOf, expression, out errors, toRoot);
        }

        /// <summary>
        /// Generates the wizard data list from defs.
        /// </summary>
        /// <param name="definitions">The definitions.</param>
        /// <param name="defType">Type of the def.</param>
        /// <param name="pushToServer">if set to <c>true</c> [push to server].</param>
        /// <param name="errors">The errors.</param>
        /// <param name="withData"></param>
        /// <returns></returns>
        public string GenerateWizardDataListFromDefs(string definitions, enDev2ArgumentType defType, bool pushToServer, out ErrorResultTO errors, bool withData = false)
        {
            IList<IDev2Definition> defs = new List<IDev2Definition>();
            IList<IDev2Definition> wizdefs = new List<IDev2Definition>();

            if (defType == enDev2ArgumentType.Output)
            {
                defs = _outputParser.ParseAndAllowBlanks(definitions);

                foreach (IDev2Definition def in defs)
                {
                    if (def.IsRecordSet)
                    {
                        wizdefs.Add(DataListFactory.CreateDefinition(def.RecordSetName + GlobalConstants.RecordsetJoinChar + def.Name, def.MapsTo, def.Value, def.IsEvaluated, def.DefaultValue, def.IsRequired, def.RawValue));
                    }
                    else
                    {
                        wizdefs.Add(DataListFactory.CreateDefinition(def.Name, def.MapsTo, def.Value, def.IsEvaluated, def.DefaultValue, def.IsRequired, def.RawValue));
                    }
                }
            }
            else if (defType == enDev2ArgumentType.Input)
            {
                defs = _inputParser.Parse(definitions);
                foreach (IDev2Definition def in defs)
                {
                    if (def.IsRecordSet)
                    {
                        wizdefs.Add(DataListFactory.CreateDefinition(def.RecordSetName + GlobalConstants.RecordsetJoinChar + def.Name, def.MapsTo, def.Value, def.IsEvaluated, def.DefaultValue, def.IsRequired, def.RawValue));
                    }
                    else
                    {
                        wizdefs.Add(DataListFactory.CreateDefinition(def.Name, def.MapsTo, def.Value, def.IsEvaluated, def.DefaultValue, def.IsRequired, def.RawValue));
                    }
                }
            }

            return GenerateDataListFromDefs(wizdefs, pushToServer, out errors, withData);
        }

        /// <summary>
        /// Generates the data list from defs.
        /// </summary>
        /// <param name="definitions">The definitions.</param>
        /// <param name="defType">Type of the def.</param>
        /// <param name="pushToServer">if set to <c>true</c> [push to server].</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public string GenerateDataListFromDefs(string definitions, enDev2ArgumentType defType, bool pushToServer, out ErrorResultTO errors)
        {
            IList<IDev2Definition> defs = new List<IDev2Definition>();

            if (defType == enDev2ArgumentType.Output)
            {
                defs = _outputParser.ParseAndAllowBlanks(definitions);
            }
            else if (defType == enDev2ArgumentType.Input)
            {
                defs = _inputParser.Parse(definitions);
            }

            return GenerateDataListFromDefs(defs, pushToServer, out errors);
        }

        /// <summary>
        /// Generates the data list from defs.
        /// </summary>
        /// <param name="definitions">The definitions as binary objects</param>
        /// <param name="pushToServer">if set to <c>true</c> [push to server]. the GUID is returned</param>
        /// <param name="errors">The errors.</param>
        /// <param name="withData"></param>
        /// <returns></returns>
        public string GenerateDataListFromDefs(IList<IDev2Definition> definitions, bool pushToServer, out ErrorResultTO errors, bool withData = false)
        {
            errors = new ErrorResultTO();
            string dataList = GenerateDataListFromDefs(definitions, withData);
            string result = Guid.Empty.ToString();

            if (pushToServer)
            {
                byte[] data = new byte[0];
                result = _svrCompiler.ConvertTo(null, DataListFormat.CreateFormat(GlobalConstants._XML), data, dataList, out errors).ToString();
            }
            else
            {
                result = dataList;
            }

            return result;
        }

        /// <summary>
        /// Shapes the dev2 definitions to data list.
        /// </summary>
        /// <param name="definitions">The definitions as string</param>
        /// <param name="defType">Type of the def.</param>
        /// <param name="pushToServer">if set to <c>true</c> [push to server].</param>
        /// <param name="errors">The errors.</param>
        /// <param name="flipGeneration">if set to <c>true</c> [flip generation].</param>
        /// <returns></returns>
        public string ShapeDev2DefinitionsToDataList(string definitions, enDev2ArgumentType defType, bool pushToServer, out ErrorResultTO errors,bool flipGeneration = false)
        {
            string dataList = ShapeDefinitionsToDataList(definitions, defType, out errors, flipGeneration);
            string result = Guid.Empty.ToString();

            if (pushToServer)
            {
                //  Push to server and return GUID
                byte[] data = new byte[0];
                result = _svrCompiler.ConvertTo(null, DataListFormat.CreateFormat(GlobalConstants._XML), data, dataList, out errors).ToString();
            }
            else
            {
                result = dataList; // else return the datalist as requested
            }

            return result;
        }

        // Travis.Frisinger - 29.01.2013 : Bug 8412
        /// <summary>
        /// Shapes the dev2 definitions to data list.
        /// </summary>
        /// <param name="definitions">The definitions as binary objects</param>
        /// <param name="defType">Type of the def Input or Output</param>
        /// <param name="pushToServer"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        public string ShapeDev2DefinitionsToDataList(IList<IDev2Definition> definitions, enDev2ArgumentType defType, bool pushToServer, out ErrorResultTO errors)
        {

            ErrorResultTO allErrors = new ErrorResultTO();
            string dataList = DataListUtil.ShapeDefinitionsToDataList(definitions, defType, out errors);
            allErrors.MergeErrors(errors);
            errors.ClearErrors();

            // ReSharper disable RedundantAssignment
            string result = GlobalConstants.NullDataListID.ToString();
            // ReSharper restore RedundantAssignment

            if (pushToServer)
            {
                //  Push to server and return GUID
                byte[] data = new byte[0];
                result = _svrCompiler.ConvertTo(null, DataListFormat.CreateFormat(GlobalConstants._XML), data, dataList, out errors).ToString();
            }
            else
            {
                result = dataList; // else return the datalist as requested
            }

            return result;
        }

        /// <summary>
        /// Fetches the binary data list.
        /// </summary>
        /// <param name="curDLID">The cur DL ID.</param>
        /// <param name="errors"></param>
        /// <returns></returns>
        public IBinaryDataList FetchBinaryDataList(Guid curDLID, out ErrorResultTO errors)
        {

            errors = new ErrorResultTO();
            return _svrCompiler.FetchBinaryDataList(null, curDLID, out errors);
        }

        /// <summary>
        /// Upserts the value to the specified cur DL ID's expression.
        /// </summary>
        /// <param name="curDLID">The cur DLID.</param>
        /// <param name="expression">The expression.</param>
        /// <param name="value">The value.</param>
        /// <param name="errors"></param>
        /// <returns></returns>
        public Guid Upsert(Guid curDLID, string expression, IBinaryDataListEntry value, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();

            return _svrCompiler.Upsert(null, curDLID, expression, value, out errors);
        }

        /// <summary>
        /// Upserts the values against the specified cur DL ID's expression list.
        /// </summary>
        /// <param name="curDLID">The cur DLID.</param>
        /// <param name="expressions">The expressions.</param>
        /// <param name="values">The values.</param>
        /// <param name="errors"></param>
        /// <returns></returns>
        public Guid Upsert(Guid curDLID, IList<string> expressions, IList<IBinaryDataListEntry> values, out ErrorResultTO errors)
        {

            errors = new ErrorResultTO();
            return _svrCompiler.Upsert(null, curDLID, expressions, values, out errors);
        }

        /// <summary>
        /// Upserts the specified cur DLID.
        /// </summary>
        /// <param name="curDLID">The cur DLID.</param>
        /// <param name="expressions">The expressions.</param>
        /// <param name="values">The values.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public Guid Upsert(Guid curDLID, IList<string> expressions, IList<string> values, out ErrorResultTO errors)
        {

            errors = new ErrorResultTO();
            return _svrCompiler.Upsert(null, curDLID, expressions, values, out errors);
        }

        /// <summary>
        /// Upserts the specified cur DLID.
        /// </summary>
        /// <param name="curDLID">The cur DLID.</param>
        /// <param name="expression">The expression.</param>
        /// <param name="value">The value.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public Guid Upsert(Guid curDLID, string expression, string value, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            //2013.06.03: Ashley Lewis for bug 9498 - handle multiple regions in regular upsert
            var allRegions = DataListCleaningUtils.SplitIntoRegions(expression);
            var allValues = allRegions.Select(region => value).ToList();
            return _svrCompiler.Upsert(null, curDLID, allRegions, allValues, out errors);
        }

        /// <summary>
        /// Upserts the specified cur DLID.
        /// </summary>
        /// <param name="curDLID">The cur DLID.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public Guid Upsert(Guid curDLID, IDev2DataListUpsertPayloadBuilder<string> payload, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            return _svrCompiler.Upsert(null, curDLID, payload, out errors);
        }

        /// <summary>
        /// Upserts the specified cur DLID.
        /// </summary>
        /// <param name="curDLID">The cur DLID.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public Guid Upsert(Guid curDLID, IDev2DataListUpsertPayloadBuilder<List<string>> payload, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            return _svrCompiler.Upsert(null, curDLID, payload, out errors);
        }

        /// <summary>
        /// Upserts the specified cur DLID.
        /// </summary>
        /// <param name="curDLID">The cur DLID.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public Guid Upsert(Guid curDLID, IDev2DataListUpsertPayloadBuilder<IBinaryDataListEntry> payload, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            return _svrCompiler.Upsert(null, curDLID, payload, out errors);
        }

        /// <summary>
        /// Shapes the specified current dlid.
        /// </summary>
        /// <param name="curDLID">The current dlid.</param>
        /// <param name="typeOf">The type of.</param>
        /// <param name="defs">The defs.</param>
        /// <param name="errors">The errors.</param>
        /// <param name="overrideID">The override unique identifier.</param>
        /// <returns></returns>
        public Guid Shape(Guid curDLID, enDev2ArgumentType typeOf, string defs, out ErrorResultTO errors,Guid overrideID = default(Guid))
        {
            errors = new ErrorResultTO();
            return _svrCompiler.Shape(null, curDLID, typeOf, defs, out errors, overrideID);
        }

        /// <summary>
        /// Shapes the definitions in binary form to create/amended a DL.
        /// </summary>
        /// <param name="curDLID">The cur DL ID.</param>
        /// <param name="typeOf">The type of.</param>
        /// <param name="definitions">The definitions.</param>
        /// <param name="errors"></param>
        /// <returns></returns>
        public Guid Shape(Guid curDLID, enDev2ArgumentType typeOf, IList<IDev2Definition> definitions, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            return _svrCompiler.Shape(null, curDLID, typeOf, definitions, out errors);
        }

        /// <summary>
        /// Shapes for sub execution.
        /// </summary>
        /// <param name="parentDLID">The parent dlid.</param>
        /// <param name="childDLID">The child dlid.</param>
        /// <param name="inputDefs">The input defs.</param>
        /// <param name="outputDefs">The output defs.</param>
        /// <param name="errors">The errors.</param>
        public IList<KeyValuePair<enDev2ArgumentType, IList<IDev2Definition>>> ShapeForSubExecution(Guid parentDLID, Guid childDLID, string inputDefs, string outputDefs, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            return _svrCompiler.ShapeForSubExecution(null, parentDLID, childDLID, inputDefs, outputDefs, out errors);
        }


        /// <summary>
        /// Merges the specified left ID with the right ID
        /// </summary>
        /// <param name="leftID">The left ID.</param>
        /// <param name="rightID">The right ID.</param>
        /// <param name="mergeType">Type of the merge.</param>
        /// <param name="depth"></param>
        /// <param name="createNewList"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        public Guid Merge(Guid leftID, Guid rightID, enDataListMergeTypes mergeType, enTranslationDepth depth, bool createNewList, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            return _svrCompiler.Merge(null, leftID, rightID, mergeType, depth, createNewList, out errors);
        }

        /// <summary>
        /// Merges the specified left.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <param name="mergeType">Type of the merge.</param>
        /// <param name="depth">The depth.</param>
        /// <param name="createNewList">if set to <c>true</c> [create new list].</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public IBinaryDataList Merge(IBinaryDataList left, IBinaryDataList right, enDataListMergeTypes mergeType, enTranslationDepth depth, bool createNewList, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            return (left.Merge(right, mergeType, depth, createNewList, out errors));
        }

        /// <summary>
        /// Conditionals the merge.
        /// </summary>
        /// <param name="conditions">The conditions.</param>
        /// <param name="destinationDatalistID">The destination datalist ID.</param>
        /// <param name="sourceDatalistID">The source datalist ID.</param>
        /// <param name="datalistMergeFrequency">The datalist merge frequency.</param>
        /// <param name="datalistMergeType">Type of the datalist merge.</param>
        /// <param name="datalistMergeDepth">The datalist merge depth.</param>
        public void ConditionalMerge(DataListMergeFrequency conditions,
            Guid destinationDatalistID, Guid sourceDatalistID, DataListMergeFrequency datalistMergeFrequency,
            enDataListMergeTypes datalistMergeType, enTranslationDepth datalistMergeDepth)
        {
            _svrCompiler.ConditionalMerge(null, conditions, destinationDatalistID, sourceDatalistID, datalistMergeFrequency, datalistMergeType, datalistMergeDepth);
        }

        /// <summary>
        /// Upserts the system tag, keep val == string.Empty to erase the tag
        /// </summary>
        /// <param name="curDLID">The cur DLID.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="val">The val.</param>
        /// <param name="errors"></param>
        /// <returns></returns>
        public Guid UpsertSystemTag(Guid curDLID, enSystemTag tag, string val, out ErrorResultTO errors)
        {
            return _svrCompiler.UpsertSystemTag(curDLID, tag, val, out errors);
        }

        /// <summary>
        /// Translation types for conversion to and from binary
        /// </summary>
        /// <returns></returns>
        public IList<DataListFormat> TranslationTypes()
        {
            return (_svrCompiler.FetchTranslatorTypes());
        }

        /// <summary>
        /// Converts from selected Type to binary
        /// </summary>
        /// <param name="typeOf">The type of.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="shape">The shape.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public Guid ConvertTo(DataListFormat typeOf, string payload, string shape, out ErrorResultTO errors)
        {

            errors = new ErrorResultTO();
            byte[] data = Encoding.UTF8.GetBytes(payload);
            return _svrCompiler.ConvertTo(null, typeOf, data, shape, out errors);
        }

        /// <summary>
        /// Converts from selected Type to binary
        /// </summary>
        /// <param name="typeOf">The type of.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="shape">The shape.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public Guid ConvertTo(DataListFormat typeOf, byte[] payload, string shape, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            return _svrCompiler.ConvertTo(null, typeOf, payload, shape, out errors);
        }

        /// <summary>
        /// Converts to.
        /// </summary>
        /// <param name="typeOf">The type of.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="shape">The shape.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public Guid ConvertTo(DataListFormat typeOf, object payload, string shape, out ErrorResultTO errors)
        {

            errors = new ErrorResultTO();
            return _svrCompiler.ConvertTo(null, typeOf, payload, shape, out errors);
        }

        /// <summary>
        /// Populates the data list.
        /// </summary>
        /// <param name="typeOf">The type of.</param>
        /// <param name="input">The input.</param>
        /// <param name="outputDefs">The output defs.</param>
        /// <param name="targetDLID">The target dlid.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public Guid PopulateDataList(DataListFormat typeOf, object input, string outputDefs, Guid targetDLID, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            return _svrCompiler.PopulateDataList(null, typeOf, input, outputDefs, targetDLID, out errors);
        }

        /// <summary>
        /// Pushes the binary data list.
        /// </summary>
        /// <param name="dlID">The dl ID.</param>
        /// <param name="bdl">The BDL.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public Guid PushBinaryDataList(Guid dlID, IBinaryDataList bdl, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();

            return PushBinaryDataListInServerScope(dlID, bdl, out errors);

        }

        /// <summary>
        /// Pushes the binary data list in server scope.
        /// </summary>
        /// <param name="dlID">The dl ID.</param>
        /// <param name="bdl">The BDL.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public Guid PushBinaryDataListInServerScope(Guid dlID, IBinaryDataList bdl, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            string error;

            if (_svrCompiler.TryPushDataList(bdl, out error))
            {
                errors.AddError(error);
                return bdl.UID;
            }

            errors.AddError(error);

            return GlobalConstants.NullDataListID;
        }

        /// <summary>
        /// Converts to selected Type from binary
        /// </summary>
        /// <param name="curDLID">The cur DLID.</param>
        /// <param name="typeOf">The type of.</param>
        /// <param name="depth">The depth.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public string ConvertFrom(Guid curDLID, DataListFormat typeOf, enTranslationDepth depth, out ErrorResultTO errors)
        {

            DataListTranslatedPayloadTO tmp = _svrCompiler.ConvertFrom(null, curDLID, depth, typeOf, out errors);

            if (tmp != null)
            {
                return tmp.FetchAsString();
            }

            return string.Empty;            
        }

        /// <summary>
        /// Converts the and filter.
        /// </summary>
        /// <param name="curDlid">The cur DLID.</param>
        /// <param name="typeOf">The type of.</param>
        /// <param name="filterShape">The filter shape.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public string ConvertAndFilter(Guid curDlid, DataListFormat typeOf, string filterShape, out ErrorResultTO errors)
        {
            return _svrCompiler.ConvertAndFilter(null, curDlid, filterShape, typeOf, out errors);
        }

        public DataTable ConvertToDataTable(IBinaryDataList input, string recsetName, out ErrorResultTO errors, PopulateOptions populateOptions = PopulateOptions.IgnoreBlankRows)
        {
            return _svrCompiler.ConvertToDataTable(input, recsetName, out errors, populateOptions);
        }

        /// <summary>
        /// Converts from to.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="payload">The payload.</param>
        /// <returns></returns>
        public T ConvertFromJsonToModel<T>(string payload)
        {

            T obj = JsonConvert.DeserializeObject<T>(payload);

            return obj;
        }

        /// <summary>
        /// Converts the model to json.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="payload">The payload.</param>
        /// <returns></returns>
        public string ConvertModelToJson<T>(T payload)
        {
            string result = JsonConvert.SerializeObject(payload);

            return result;
        }

        /// <summary>
        /// Pushes the system model to data list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model">The model.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public Guid PushSystemModelToDataList<T>(T model, out ErrorResultTO errors)
        {
            return PushSystemModelToDataList(GlobalConstants.NullDataListID, model, out errors);
        }

        /// <summary>
        /// Pushes the system model to data list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dlID">The dl ID.</param>
        /// <param name="model">The model.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public Guid PushSystemModelToDataList<T>(Guid dlID, T model, out ErrorResultTO errors)
        {
            // Serialize the model first ;)
            string jsonModel = ConvertModelToJson(model);
            ErrorResultTO allErrors = new ErrorResultTO();

            // Create a new DL if need be
            Guid pushID = dlID;
            if (pushID == GlobalConstants.NullDataListID)
            {
                IBinaryDataList bdl = Dev2BinaryDataListFactory.CreateDataList();
                pushID = PushBinaryDataList(bdl.UID, bdl, out errors);
                allErrors.MergeErrors(errors);
                errors.ClearErrors();
            }

            UpsertSystemTag(pushID, enSystemTag.SystemModel, jsonModel, out errors);
            allErrors.MergeErrors(errors);

            errors = allErrors;

            return pushID;
        }

        /// <summary>
        /// Pushes the system model to data list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dlID">The dl ID.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public T FetchSystemModelFromDataList<T>(Guid dlID, out ErrorResultTO errors)
        {
            string modelData = EvaluateSystemEntry(dlID, enSystemTag.SystemModel, out errors);

            T obj = Activator.CreateInstance<T>();

            if (!string.IsNullOrEmpty(modelData))
            {

                if (!String.IsNullOrEmpty(modelData))
                {
                    obj = ConvertFromJsonToModel<T>(modelData);
                }
            }
            else
            {

                errors.AddError("Failed to locate model!");
            }

            return obj;
        }

        /// <summary>
        /// Fetches the system model as web model.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dlID">The dl ID.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public string FetchSystemModelAsWebModel<T>(Guid dlID, out ErrorResultTO errors)
        {
            T model = FetchSystemModelFromDataList<T>(dlID, out errors);
            string result = "{}"; // empty data set for injection ;)

            if (!errors.HasErrors())
            {
                var dev2DataModel = model as IDev2DataModel;

                if (dev2DataModel != null) result = dev2DataModel.ToWebModel();

            }

            return result;
        }

        /// <summary>
        /// Evaluates the system entry.
        /// </summary>
        /// <param name="curDLID">The cur DL ID.</param>
        /// <param name="sysTag">The system tag.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public string EvaluateSystemEntry(Guid curDLID, enSystemTag sysTag, out ErrorResultTO errors)
        {
            IBinaryDataListEntry binaryDataListEntry = _svrCompiler.Evaluate(null, curDLID, enActionType.System, sysTag.ToString(), out errors) ?? DataListConstants.baseEntry;
            return binaryDataListEntry.FetchScalar().TheValue;
        }

        public IList<KeyValuePair<string, IBinaryDataListEntry>> FetchChanges(Guid id, StateType direction)
        {
            return _svrCompiler.FetchChanges(null, id, direction);
        }

        public bool DeleteDataListByID(Guid curDLID)
        {

            return _svrCompiler.DeleteDataListByID(curDLID, false);
        }

        public bool ForceDeleteDataListByID(Guid curDLID)
        {
            // Do nothing for now, we scope it all ;)
            return _svrCompiler.DeleteDataListByID(curDLID, true);
        }

        public int GetMaxNumberOfExecutions(Guid curDLID, IList<string> expressions)
        {

            int result = 1;
            ErrorResultTO errors = new ErrorResultTO();
            IBinaryDataList bdl = FetchBinaryDataList(curDLID, out errors);
            // Loop each expression to find the total number of executions ;)
            foreach (string exp in expressions)
            {
                IList<IIntellisenseResult> parts = _parser.ParseExpressionIntoParts(exp, bdl.FetchIntellisenseParts());
                foreach (IIntellisenseResult p in parts)
                {
                    result = Math.Max(result, FetchNumberOfExecutions(p, bdl));
                }
            }

            return result;
        }

        public Guid FetchParentID(Guid curDLID)
        {

            ErrorResultTO errors;
            return (_svrCompiler.FetchBinaryDataList(null, curDLID, out errors).ParentUID);
        }

        public bool HasErrors(Guid curDLID)
        {
            ErrorResultTO errors;
            var binaryDatalist = _svrCompiler.FetchBinaryDataList(null, curDLID, out errors);

            if (binaryDatalist != null)
            {
                return (binaryDatalist.HasErrors());
            }

            errors.AddError("No binary datalist found");
            return true;
        }

        public string FetchErrors(Guid curDLID,bool returnAsXml = false)
        {
            ErrorResultTO errors;
            var binaryDatalist = _svrCompiler.FetchBinaryDataList(null, curDLID, out errors);
            if (binaryDatalist != null) return (binaryDatalist.FetchErrors(returnAsXml));
            else
            {
                var sb = new StringBuilder();
                var count = 1;
                var errorList = errors.FetchErrors();
                foreach (var error in errorList)
                {
                    sb.AppendFormat("{0} {1}", count, error);
                    count++;
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// Clears the errors.
        /// </summary>
        /// <param name="curDLID">The cur DLID.</param>
        /// <author>Jurie.smit</author>
        /// <date>2013/02/06</date>
        public void ClearErrors(Guid curDLID)
        {
            ErrorResultTO errors = new ErrorResultTO();
            var list = _svrCompiler.FetchBinaryDataList(null, curDLID, out errors);
            if (list != null)
                list.ClearErrors();
        }

        public bool SetParentID(Guid curDLID, Guid newParent)
        {
            bool result = true;
            ErrorResultTO errors = new ErrorResultTO();

            _svrCompiler.SetParentUID(curDLID, newParent, out errors);
            if (errors.HasErrors())
            {
                result = false;
            }

            return result;
        }

        /// <summary>
        /// Merges the wizard data list.
        /// </summary>
        /// <param name="wizardDL"></param>
        /// <param name="serviceDL"></param>
        /// <returns></returns>
        /// NOTE: This method is not production safe. It does not clean up data list and since it returns entries this is a BIG ISSUE!
        public WizardDataListMergeTO MergeFixedWizardDataList(string wizardDL, string serviceDL)
        {
            WizardDataListMergeTO result = new WizardDataListMergeTO();
            ErrorResultTO errors = new ErrorResultTO();
            ErrorResultTO allErrors = new ErrorResultTO();

            Guid wizardID = ConvertTo(DataListFormat.CreateFormat(GlobalConstants._Studio_XML), string.Empty, wizardDL, out errors);
            allErrors.MergeErrors(errors);
            Guid serviceID = ConvertTo(DataListFormat.CreateFormat(GlobalConstants._Studio_XML), string.Empty, serviceDL, out errors);
            allErrors.MergeErrors(errors);

            IBinaryDataList wizardBDL = FetchBinaryDataList(wizardID, out errors);
            allErrors.MergeErrors(errors);
            IBinaryDataList serviceBDL = FetchBinaryDataList(serviceID, out errors);
            allErrors.MergeErrors(errors);


            // Added Removed refenced ParentDL
            // First find difference between parent and wizard
            IList<IBinaryDataListEntry> serviceEntries = serviceBDL.FetchAllEntries();
            IList<IBinaryDataListEntry> wizardEntries = wizardBDL.FetchAllEntries();

            // iterate each service entry
            foreach (IBinaryDataListEntry serviceEntry in serviceEntries)
            {

                // Find all new entries
                bool found = false;
                int pos = 0;
                while (pos < wizardEntries.Count && !found)
                {

                    IBinaryDataListEntry tmp = wizardEntries[pos];
                    if (tmp.Namespace == serviceEntry.Namespace && ((tmp.Columns == null && serviceEntry.Columns == null) || (tmp.Columns.SequenceEqual(serviceEntry.Columns, Dev2ColumnComparer.Instance))))
                    {
                        found = true;
                    }

                    pos++;
                }
                if (!found)
                {
                    result.AddNewRegion(serviceEntry);
                }
            }

            // iterate each service entry
            foreach (IBinaryDataListEntry wizardEntry in wizardEntries)
            {

                // Find all new entries
                bool found = false;
                int pos = 0;
                while (pos < serviceEntries.Count && !found)
                {

                    IBinaryDataListEntry tmp = serviceEntries[pos];
                    if (tmp.Namespace == wizardEntry.Namespace && ((tmp.Columns == null && wizardEntry.Columns == null) || (tmp.Columns.SequenceEqual(wizardEntry.Columns, Dev2ColumnComparer.Instance))))
                    {
                        found = true;
                    }

                    pos++;
                }
                if (!found)
                {
                    result.AddRemovedRegion(wizardEntry);
                }

            }

            // Now build the new Binary Data List
            string tmpDL = ConvertFrom(serviceID, DataListFormat.CreateFormat(GlobalConstants._FIXED_WIZARD), enTranslationDepth.Shape, out errors);
            allErrors.MergeErrors(errors);
            result.SetIntersectedDataList(tmpDL);

            // now clean up
            ForceDeleteDataListByID(serviceID);
            ForceDeleteDataListByID(wizardID);

            errors = allErrors;

            return result;
        }

        /// <summary>
        /// Gets the wizard data list for a service.
        /// </summary>
        /// <param name="serviceDefinition">The service definition.</param>
        /// <returns>
        /// The string for the data list
        /// </returns>
        /// <exception cref="System.Xml.XmlException">Inputs/Outputs tags were not found in the service definition</exception>
        public string GetWizardDataListForService(string serviceDefinition)
        {
            string result;

            ErrorResultTO errors = new ErrorResultTO();

            string inputs = string.Empty;
            string outputs = string.Empty;
            try
            {
                inputs = DataListUtil.ExtractInputDefinitionsFromServiceDefinition(serviceDefinition);
                outputs = DataListUtil.ExtractOutputDefinitionsFromServiceDefinition(serviceDefinition);
            }
            catch
            {
                throw new XmlException("Inputs/Outputs tags were not found in the service definition");
            }

            string inputDl = string.Empty;
            string outputDl = string.Empty;

            inputDl = GenerateWizardDataListFromDefs(inputs, enDev2ArgumentType.Input, false, out errors);

            outputDl = GenerateWizardDataListFromDefs(outputs, enDev2ArgumentType.Output, false, out errors);

            Guid inputDlID = ConvertTo(DataListFormat.CreateFormat(GlobalConstants._Studio_XML), string.Empty, inputDl, out errors);
            Guid outputDlID = ConvertTo(DataListFormat.CreateFormat(GlobalConstants._Studio_XML), string.Empty, outputDl, out errors);
            Guid mergedDlID = Merge(inputDlID, outputDlID, enDataListMergeTypes.Union, enTranslationDepth.Shape, true, out errors);
            result = ConvertFrom(mergedDlID, DataListFormat.CreateFormat(GlobalConstants._Studio_XML), enTranslationDepth.Shape, out errors);
            return result;
        }

        /// <summary>
        /// Gets the wizard data list for a workflow.
        /// </summary>
        /// <param name="dataList"></param>
        /// <returns>
        /// The string for the data list
        /// </returns>
        /// <exception cref="System.Exception">
        /// </exception>
        public string GetWizardDataListForWorkflow(string dataList)
        {
            IBinaryDataList newDl = Dev2BinaryDataListFactory.CreateDataList();
            ErrorResultTO errors;
            Guid dlID = ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), dataList, dataList, out errors);
            if (!errors.HasErrors())
            {
                IBinaryDataList dl = FetchBinaryDataList(dlID, out errors);
                if (!errors.HasErrors())
                {
                    IList<IBinaryDataListEntry> entries = dl.FetchAllEntries();
                    foreach (IBinaryDataListEntry entry in entries)
                    {
                        if (entry.IsRecordset)
                        {
                            if (entry.ColumnIODirection != enDev2ColumnArgumentDirection.None)
                                {
                                string tmpError;
                                newDl.TryCreateRecordsetTemplate(entry.Namespace, entry.Description, entry.Columns, true, out tmpError);

                                }
                        }
                        else
                        {
                            if (entry.ColumnIODirection != enDev2ColumnArgumentDirection.None)
                            {
                                string tmpError;
                                IBinaryDataListItem scalar = entry.FetchScalar();
                                newDl.TryCreateScalarTemplate(string.Empty, scalar.FieldName, entry.Description, true, out tmpError);
                            }
                        }
                    }
                    Guid newDlId = PushBinaryDataList(newDl.UID, newDl, out errors);
                    dataList = ConvertFrom(newDlId, DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), enTranslationDepth.Shape, out errors);
                }
                else
                {
                    throw new Exception(errors.MakeUserReady());
                }
            }
            else
            {
                throw new Exception(errors.MakeUserReady());
            }
            return dataList;
        }

        public string GenerateSerializableDefsFromDataList(string datalist, enDev2ColumnArgumentDirection direction)
        {
            DefinitionBuilder db = new DefinitionBuilder();

            if (direction == enDev2ColumnArgumentDirection.Input)
            {
                db.ArgumentType = enDev2ArgumentType.Input;    
            }else if (direction == enDev2ColumnArgumentDirection.Output)
            {
                db.ArgumentType = enDev2ArgumentType.Output;
            }
            
            db.Definitions = GenerateDefsFromDataList(datalist, direction);

            return db.Generate();
        }

        public IList<IDev2Definition> GenerateDefsFromDataList(string dataList)
        {
            return GenerateDefsFromDataList(dataList, enDev2ColumnArgumentDirection.Both);
        }

        public IList<IDev2Definition> GenerateDefsFromDataList(string dataList, enDev2ColumnArgumentDirection dev2ColumnArgumentDirection)
        {
            IList<IDev2Definition> result = new List<IDev2Definition>();

            if (!string.IsNullOrEmpty(dataList))
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml(dataList);

                XmlNodeList tmpRootNl = xDoc.ChildNodes;
                XmlNodeList nl = tmpRootNl[0].ChildNodes;

                for (int i = 0; i < nl.Count; i++)
                {
                    XmlNode tmpNode = nl[i];

                    var ioDirection = GetDev2ColumnArgumentDirection(tmpNode);

                    if (CheckIODirection(dev2ColumnArgumentDirection, ioDirection))
                    {
                        if (tmpNode.HasChildNodes)
                        {
                            // it is a record set, make it as such
                            string recordsetName = tmpNode.Name;
                            // now extract child node defs
                            XmlNodeList childNL = tmpNode.ChildNodes;
                            for (int q = 0; q < childNL.Count; q++)
                            {
                                var xmlNode = childNL[q];
                                var fieldIODirection = GetDev2ColumnArgumentDirection(xmlNode);
                                if (CheckIODirection(dev2ColumnArgumentDirection, fieldIODirection)) { 
                                result.Add(DataListFactory.CreateDefinition(xmlNode.Name, "", "", recordsetName, false, "",
                                                                            false, "", false));
                            }
                        }
                        }
                        else
                        {
                            // scalar value, make it as such
                            result.Add(DataListFactory.CreateDefinition(tmpNode.Name, "", "", false, "", false, ""));
                        }
                    }
                }
            }

            return result;
        }

        static bool CheckIODirection(enDev2ColumnArgumentDirection dev2ColumnArgumentDirection, enDev2ColumnArgumentDirection ioDirection)
        {
            return ioDirection == dev2ColumnArgumentDirection ||
                   (ioDirection == enDev2ColumnArgumentDirection.Both &&
                    (dev2ColumnArgumentDirection == enDev2ColumnArgumentDirection.Input || dev2ColumnArgumentDirection == enDev2ColumnArgumentDirection.Output));
        }

        static enDev2ColumnArgumentDirection GetDev2ColumnArgumentDirection(XmlNode tmpNode)
        {
            XmlAttribute ioDirectionAttribute = tmpNode.Attributes[GlobalConstants.DataListIoColDirection];

            enDev2ColumnArgumentDirection ioDirection;
            if(ioDirectionAttribute != null)
            {
                ioDirection = (enDev2ColumnArgumentDirection)Dev2EnumConverter.GetEnumFromStringDiscription(ioDirectionAttribute.Value, typeof(enDev2ColumnArgumentDirection));
            }
            else
            {
                ioDirection = enDev2ColumnArgumentDirection.Both;
            }
            return ioDirection;
        }

        //PBI 8435 - Massimo.Guerrera - Added for getting the debug data for the multiAssign

        public List<KeyValuePair<string,IBinaryDataListEntry>> GetDebugData()
        {
            return _svrCompiler.GetDebugItems();
        }

        #region New Private Methods

        private int FetchNumberOfExecutions(IIntellisenseResult part, IBinaryDataList bdl)
        {
            int result = 1;
            IBinaryDataListEntry entry;
            string error = string.Empty;

            if (!part.Option.IsScalar)
            {
                // process the recordset...
                enRecordsetIndexType type = DataListUtil.GetRecordsetIndexType(part.Option.DisplayValue);
                if (type == enRecordsetIndexType.Star)
                {
                    // Fetch entry and find the last index
                    if (bdl.TryGetEntry(part.Option.Recordset, out entry, out error))
                    {
                        result = entry.FetchLastRecordsetIndex();
                    }
                }
                else if (type == enRecordsetIndexType.Numeric)
                {
                    // Fetch index out
                    Int32.TryParse(part.Option.RecordsetIndex, out result);
                }
            }

            return result;
        }

        /// <summary>
        /// Generate DL shape from IO defs
        /// </summary>
        /// <param name="defs">The defs.</param>
        /// <param name="withData">if set to <c>true</c> [with data].</param>
        /// <returns></returns>
        private string GenerateDataListFromDefs(IList<IDev2Definition> defs, bool withData = false)
        {
            return DataListUtil.GenerateDataListFromDefs(defs, withData);
        }

        /// <summary>
        /// Create a DL shape as per IO mapping
        /// </summary>
        /// <param name="arguments"></param>
        /// <param name="typeOf"></param>
        /// <returns></returns>
        private string ShapeDefinitionsToDataList(string arguments, enDev2ArgumentType typeOf, out ErrorResultTO errors, bool flipGeneration = false)
        {
            errors = new ErrorResultTO();
            return DataListUtil.ShapeDefinitionsToDataList(arguments, typeOf, out errors, flipGeneration);
        }

        #region Old Webpage mapping methods

        private void BuildDataPart(string DataPartFieldData)
        {

            IDataListVerifyPart verifyPart;
            string fullyFormattedStringValue;
            string[] fieldList = DataPartFieldData.Split('.');
            if (fieldList.Count() > 1 && !String.IsNullOrEmpty(fieldList[0]))
            {  // If it's a RecordSet Containing a field
                foreach (string item in fieldList)
                {
                    if (item.EndsWith(")") && item == fieldList[0])
                    {
                        if (item.Contains("("))
                        {
                            fullyFormattedStringValue = DataListUtil.RemoveRecordsetBracketsFromValue(item);
                            verifyPart = IntellisenseFactory.CreateDataListValidationRecordsetPart(fullyFormattedStringValue, String.Empty);
                            AddDataVerifyPart(verifyPart, verifyPart.DisplayValue);
                        }
                        else
                        { // If it's a field containing a single brace
                            continue;
                        }
                    }
                    else if (item == fieldList[1] && !(item.EndsWith(")") && item.Contains(")")))
                    { // If it's a field to a record set
                        verifyPart = IntellisenseFactory.CreateDataListValidationRecordsetPart(DataListUtil.RemoveRecordsetBracketsFromValue(fieldList.ElementAt(0)), item);
                        AddDataVerifyPart(verifyPart, verifyPart.DisplayValue);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else if (fieldList.Count() == 1 && !String.IsNullOrEmpty(fieldList[0]))
            { // If the workflow field is simply a scalar or a record set without a child
                if (DataPartFieldData.EndsWith(")") && DataPartFieldData == fieldList[0])
                {
                    if (DataPartFieldData.Contains("("))
                    {
                        fullyFormattedStringValue = DataListUtil.RemoveRecordsetBracketsFromValue(fieldList[0]);
                        verifyPart = IntellisenseFactory.CreateDataListValidationRecordsetPart(fullyFormattedStringValue, String.Empty);
                        AddDataVerifyPart(verifyPart, verifyPart.DisplayValue);
                    }
                }
                else
                {
                    verifyPart = IntellisenseFactory.CreateDataListValidationScalarPart(DataListUtil.RemoveRecordsetBracketsFromValue(DataPartFieldData));
                    AddDataVerifyPart(verifyPart, verifyPart.DisplayValue);
                }
            }
        }

        private void AddDataVerifyPart(IDataListVerifyPart part, string nameOfPart)
        {
            _uniqueWorkflowParts.Add(part, nameOfPart);
        }

        private IList<String> FormatDsfActivityField(string webpage)
        {
            Dev2DataLanguageParser languageParser = new Dev2DataLanguageParser();
            try
            {
                IList<String> resultData = languageParser.ParseForActivityDataItems(webpage);
                return resultData.Where(result => (!String.IsNullOrEmpty(result.ToString()))).ToList();
            }
            catch (Dev2DataLanguageParseError ddlex)
            {
                ServerLogger.LogError(ddlex);
                return new List<String>();
            }
            catch (NullReferenceException nex)
            {
                ServerLogger.LogError(nex);
                return new List<String>();
            }
        }

        #endregion Old Webpage mapping methods

        #endregion

        #endregion

        #region Tear Down

        public void Dispose()
        {
            lock (_disposeGuard)
            {
                if (_isDisposed)
                {
                    return;
                }

                _uniqueWorkflowParts.Clear();
                _uniqueWorkflowParts = null;

                _svrCompiler = null;

                _isDisposed = true;
            }
        }

        #endregion Tear Down
    }
}
