﻿using Dev2.Data.Binary_Objects;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Builders;
using Dev2.DataList.Contract.TO;
using Dev2.Diagnostics;
using System;
using System.Collections.Generic;

namespace Dev2.DataList.Contract
{
    public interface IDataListCompiler: IDisposable
    {

        // Travis.Frisinger : 29.10.2012 - Amend Compiler Interface for refactoring
        #region New External Methods

        #region Evaluation Operations
        /// <summary>
        /// Used to evalaute an expression against a given datalist
        /// </summary>
        /// <param name="curDLID">The cur DL ID.</param>
        /// <param name="typeOf">The type of evaluation.</param>
        /// <param name="expression">The expression.</param>
        /// <param name="returnExpressionIfNoMatch">if set to <c>true</c> [return expression if no match].</param>
        /// <returns></returns>
        IBinaryDataListEntry Evaluate(Guid curDLID, enActionType typeOf, string expression, bool toRoot, out ErrorResultTO errors);

        /// <summary>
        /// Evaluates the system entry.
        /// </summary>
        /// <param name="curDLID">The cur DL ID.</param>
        /// <param name="sysTag">The system tag.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        string EvaluateSystemEntry(Guid curDLID, enSystemTag sysTag, out ErrorResultTO errors);

        #endregion

        #region Internal Binary Operations

        /// <summary>
        /// Generates the defs from webpage Xml.
        /// </summary>
        /// <param name="webpageXml">The webpage XML.</param>
        /// <returns></returns>
        IList<IDev2Definition> GenerateDefsFromWebpageXMl(string webpageXml);

        /// <summary>
        /// Generates the wizard data list from defs.
        /// </summary>
        /// <param name="definitions">The definitions.</param>
        /// <param name="defType">Type of the def.</param>
        /// <param name="pushToServer">if set to <c>true</c> [push to server].</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        string GenerateWizardDataListFromDefs(string definitions, enDev2ArgumentType defType, bool pushToServer, out ErrorResultTO errors, bool withData = false);

        /// <summary>
        /// Generates the data list from defs
        /// </summary>
        /// <param name="definitions">The definitions as strings</param>
        /// <param name="pushToServer">if set to <c>true</c> [push to server]. the GUID is returned</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        string GenerateDataListFromDefs(string definitions, enDev2ArgumentType typeOf, bool pushToServer, out ErrorResultTO errors);

        /// <summary>
        /// Generates the data list from defs.
        /// </summary>
        /// <param name="definitions">The definitions as binary objects</param>
        /// <param name="pushToServer">if set to <c>true</c> [push to server]. the GUID is returned</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        string GenerateDataListFromDefs(IList<IDev2Definition> definitions, bool pushToServer, out ErrorResultTO errors, bool withData = false);


        /// <summary>
        /// Generate IO definitions from the DL
        /// </summary>
        /// <param name="dataList">The data list.</param>
        /// <returns></returns>
        IList<IDev2Definition> GenerateDefsFromDataList(string dataList);

        /// <summary>
        /// Generate IO definitions from the DL
        /// </summary>
        /// <param name="dataList">The data list.</param>
        /// <returns></returns>
        IList<IDev2Definition> GenerateDefsFromDataList(string dataList, enDev2ColumnArgumentDirection dev2ColumnArgumentDirection);

        /// <summary>
        /// Shapes the dev2 definitions to data list.
        /// </summary>
        /// <param name="definitions">The definitions as string</param>
        /// <param name="defType">Type of the def.</param>
        /// <param name="pushToServer">if set to <c>true</c> [push to server].</param>
        /// <param name="errors">The errors.</param>
        /// <param name="flipGeneration">if set to <c>true</c> [flip generation].</param>
        /// <returns></returns>
        string ShapeDev2DefinitionsToDataList(string definitions, enDev2ArgumentType defType, bool pushToServer, out ErrorResultTO errors, bool flipGeneration = false);

        /// <summary>
        /// Shapes the dev2 definitions to data list.
        /// </summary>
        /// <param name="definitions">The definitions as binary objects</param>
        /// <param name="defType">Type of the def Input or Output</param>
        /// <returns></returns>
        string ShapeDev2DefinitionsToDataList(IList<IDev2Definition> definitions, enDev2ArgumentType defType, bool pushToServer, out ErrorResultTO errors);

        /// <summary>
        /// Fetches the binary data list.
        /// </summary>
        /// <param name="curDLID">The cur DL ID.</param>
        /// <returns></returns>
        IBinaryDataList FetchBinaryDataList(Guid curDLID, out ErrorResultTO errors);

        /// <summary>
        /// Clones the data list.
        /// </summary>
        /// <param name="curDLID">The cur DLID.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        Guid CloneDataList(Guid curDLID, out ErrorResultTO errors);

         /// <summary>
        /// Gets the wizard data list for a service.
        /// </summary>
        /// <param name="serviceDefinition">The service definition.</param>
        /// <returns>
        /// The string for the data list
        /// </returns>
        /// <exception cref="System.Xml.XmlException">Inputs tag not found in the service definition</exception>
        /// <exception cref="System.Xml.XmlException">Outputs tag not found in the service definition</exception>
        string GetWizardDataListForService(string serviceDefinition);

        /// <summary>
        /// Gets the wizard data list for a workflow.
        /// </summary>
        /// <param name="serviceDefinition">The dataList.</param>
        /// <returns>
        /// The string for the data list
        /// </returns>
        /// <exception cref="System.Xml.XmlException">Inputs tag not found in the service definition</exception>
        /// <exception cref="System.Xml.XmlException">Outputs tag not found in the service definition</exception>
        string GetWizardDataListForWorkflow(string dataList);

        #endregion

        #region Manipulation Operations
        /// <summary>
        /// Upserts the value to the specified cur DL ID's expression.
        /// </summary>
        /// <param name="curDLID">The cur DLID.</param>
        /// <param name="expression">The expression.</param>
        /// <param name="value">The value.</param>
        Guid Upsert(Guid curDLID, string expression, IBinaryDataListEntry value, out ErrorResultTO errors);

        /// <summary>
        /// Upserts the specified cur DLID.
        /// </summary>
        /// <param name="curDLID">The cur DLID.</param>
        /// <param name="expressions">The expressions.</param>
        /// <param name="values">The values.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        Guid Upsert(Guid curDLID, IList<string> expressions, IList<string> values, out ErrorResultTO errors);

        /// <summary>
        /// Upserts the specified cur DLID.
        /// </summary>
        /// <param name="curDLID">The cur DLID.</param>
        /// <param name="expression">The expression.</param>
        /// <param name="value">The value.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        Guid Upsert(Guid curDLID, string expression, string value, out ErrorResultTO errors);

        /// <summary>
        /// Upserts the values against the specified cur DL ID's expression list.
        /// </summary>
        /// <param name="curDLID">The cur DLID.</param>
        /// <param name="expressions">The expressions.</param>
        /// <param name="values">The values.</param>
        /// <param name="error">The error.</param>
        /// <returns></returns>
        Guid Upsert(Guid curDLID, IList<string> expressions, IList<IBinaryDataListEntry> values, out ErrorResultTO errors);


        /// <summary>
        /// Upserts the specified cur DLID.
        /// </summary>
        /// <param name="curDLID">The cur DLID.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        Guid Upsert(Guid curDLID, IDev2DataListUpsertPayloadBuilder<string> payload, out ErrorResultTO errors);

        /// <summary>
        /// Upserts the specified cur DLID.
        /// </summary>
        /// <param name="curDLID">The cur DLID.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        Guid Upsert(Guid curDLID, IDev2DataListUpsertPayloadBuilder<IBinaryDataListEntry> payload, out ErrorResultTO errors);

        /// <summary>
        /// Shapes the definitions in string form to create/amended a DL.
        /// </summary>
        /// <param name="curDLID">The cur DL ID.</param>
        /// <param name="typeOf">The type of.</param>
        /// <param name="definitions">The definitions.</param>
        /// <returns></returns>
        Guid Shape(Guid curDLID, enDev2ArgumentType typeOf, string definitions, out ErrorResultTO errors);

        /// <summary>
        /// Shapes the definitions in binary form to create/amended a DL.
        /// </summary>
        /// <param name="curDLID">The cur DL ID.</param>
        /// <param name="typeOf">The type of.</param>
        /// <param name="definitions">The definitions.</param>
        /// <returns></returns>
        Guid Shape(Guid curDLID, enDev2ArgumentType typeOf, IList<IDev2Definition> definitions, out ErrorResultTO errors);

        /// <summary>
        /// Merges the specified left ID with the right ID
        /// </summary>
        /// <param name="leftID">The left ID.</param>
        /// <param name="rightID">The right ID.</param>
        /// <param name="mergeType">Type of the merge.</param>
        /// <param name="error">The error.</param>
        /// <returns></returns>
        Guid Merge(Guid leftID, Guid rightID, enDataListMergeTypes mergeType, enTranslationDepth depth, bool createNewList, out ErrorResultTO errors);

        void ConditionalMerge(DataListMergeFrequency conditions, Guid destinationDatalistID, Guid sourceDatalistID, DataListMergeFrequency datalistMergeFrequency, enDataListMergeTypes datalistMergeType, enTranslationDepth datalistMergeDepth);

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
        IBinaryDataList Merge(IBinaryDataList left, IBinaryDataList right, enDataListMergeTypes mergeType, enTranslationDepth depth, bool createNewList, out ErrorResultTO errors);

        /// <summary>
        /// Upserts the system tag, keep val == string.Empty to erase the tag
        /// </summary>
        /// <param name="curDLID">The cur DLID.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="val">The val.</param>
        /// <returns></returns>
        Guid UpsertSystemTag(Guid curDLID, enSystemTag tag, string val, out ErrorResultTO errors);

        /// <summary>
        /// Persists the resumable data list chain.
        /// </summary>
        /// <param name="baseChildID">The base child ID.</param>
        /// <returns></returns>
        bool PersistResumableDataListChain(Guid baseChildID);

        #endregion

        #region External Translation

        /// <summary>
        /// Translation types for conversion to and from binary
        /// </summary>
        /// <returns></returns>
        IList<DataListFormat> TranslationTypes();

        /// <summary>
        /// Converts from selected Type to binary
        /// </summary>
        /// <param name="curDLID">The cur DLID.</param>
        /// <param name="depth">The depth.</param>
        /// <param name="typeOf">The type of.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="shape">The shape.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        Guid ConvertTo(DataListFormat typeOf, string payload, string shape, out ErrorResultTO errors);

        /// <summary>
        /// Converts from selected Type to binary
        /// </summary>
        /// <param name="curDLID">The cur DLID.</param>
        /// <param name="depth">The depth.</param>
        /// <param name="typeOf">The type of.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="shape">The shape.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        Guid ConvertTo(DataListFormat typeOf, byte[] payload, string shape, out ErrorResultTO errors);

        /// <summary>
        /// Converts to selected Type from binary
        /// </summary>
        /// <param name="curDLID">The cur DLID.</param>
        /// <param name="typeOf">The type of.</param>
        /// <param name="depth">The depth.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        string ConvertFrom(Guid curDLID, DataListFormat typeOf, enTranslationDepth depth, out ErrorResultTO errors);

        /// <summary>
        /// Converts the and filter.
        /// </summary>
        /// <param name="curDlid">The cur DLID.</param>
        /// <param name="typeOf">The type of.</param>
        /// <param name="filterShape">The filter shape.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        string ConvertAndFilter(Guid curDlid, DataListFormat typeOf, string filterShape, out ErrorResultTO errors);

        /// <summary>
        /// Converts from to.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="payload">The payload.</param>
        /// <returns></returns>
        T ConvertFromJsonToModel<T>(string payload);

        /// <summary>
        /// Converts the model to json.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="payload">The payload.</param>
        /// <returns></returns>
        string ConvertModelToJson<T>(T payload);

        /// <summary>
        /// Pushes the system model to data list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model">The model.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        Guid PushSystemModelToDataList<T>(T model, out ErrorResultTO errors);

        /// <summary>
        /// Pushes the system model to data list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dlID">The dl ID.</param>
        /// <param name="model">The model.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        Guid PushSystemModelToDataList<T>(Guid dlID, T model, out ErrorResultTO errors);

        /// <summary>
        /// Pushes the system model to data list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dlID">The dl ID.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        T FetchSystemModelFromDataList<T>(Guid dlID, out ErrorResultTO errors);


        /// <summary>
        /// Fetches the system model as web model.
        /// </summary>
        /// <param name="dlID">The dl ID.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        string FetchSystemModelAsWebModel<T>(Guid dlID, out ErrorResultTO errors);

        ///// <summary>
        ///// Converts from selected Type to binary
        ///// </summary>
        ///// <param name="dlID">The dl ID.</param>
        ///// <param name="payload">The payload.</param>
        ///// <param name="errors">The errors.</param>
        ///// <returns></returns>
        //Guid PushBinaryDataList(Guid dlID, byte[] payload, out ErrorResultTO errors);

        /// <summary>
        /// Pushes the binary data list.
        /// </summary>
        /// <param name="dlID">The dl ID.</param>
        /// <param name="bdl">The BDL.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        Guid PushBinaryDataList(Guid dlID, IBinaryDataList bdl, out ErrorResultTO errors);

     

        #endregion

        #region Admin Operations

        /// <summary>
        /// Fetches the DebugItems created during a upsert
        /// </summary>
        List<KeyValuePair<string, IBinaryDataListEntry>> GetDebugData();

        /// <summary>
        /// Fetches the change log for pre ( inputs ) or post execute ( outputs )
        /// </summary>
        /// <returns></returns>
        IList<KeyValuePair<string, IBinaryDataListEntry>> FetchChanges(Guid id, StateType direction);

        /// <summary>
        /// Deletes the data list by ID.
        /// </summary>
        /// <param name="curDLID">The cur DLID.</param>
        /// <returns></returns>
        bool DeleteDataListByID(Guid curDLID);

        /// <summary>
        /// Forces the delete data list by ID.
        /// </summary>
        /// <param name="curDLID">The cur DLID.</param>
        /// <returns></returns>
        bool ForceDeleteDataListByID(Guid curDLID);

        /// <summary>
        /// Gets the max number of executions.
        /// </summary>
        /// <param name="curDLID">The cur DLID.</param>
        /// <param name="expressions">The expressions.</param>
        /// <returns></returns>
        int GetMaxNumberOfExecutions(Guid curDLID, IList<string> expressions);

        /// <summary>
        /// Fetches the parent ID.
        /// </summary>
        /// <param name="curDLID">The cur DLID.</param>
        /// <returns></returns>
        Guid FetchParentID(Guid curDLID);

        /// <summary>
        /// Determines whether the specified cur DLID has errors.
        /// </summary>
        /// <param name="curDLID">The cur DLID.</param>
        /// <returns>
        ///   <c>true</c> if the specified cur DLID has errors; otherwise, <c>false</c>.
        /// </returns>
        bool HasErrors(Guid curDLID);

        /// <summary>
        /// Fetches the errors.
        /// </summary>
        /// <param name="curDLID">The cur DLID.</param>
        /// <returns></returns>
        string FetchErrors(Guid curDLID);

        /// <summary>
        /// Clears the errors.
        /// </summary>
        /// <param name="curDLID">The cur DLID.</param>
        /// <author>Jurie.smit</author>
        /// <date>2013/02/06</date>
        void ClearErrors(Guid curDLID);

        /// <summary>
        /// Sets the parent ID.
        /// </summary>
        /// <param name="curDLID">The cur DLID.</param>
        /// <param name="newParent">The new parent.</param>
        /// <returns></returns>
        bool SetParentID(Guid curDLID, Guid newParent);


        /// <summary>
        /// Pushes the binary data list in server scope.
        /// </summary>
        /// <param name="dlID">The dl ID.</param>
        /// <param name="bdl">The BDL.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        Guid PushBinaryDataListInServerScope(Guid dlID, IBinaryDataList bdl, out ErrorResultTO errors);

        #endregion

        #region Studio Method

        /// <summary>
        /// Merges the wizard data list.
        /// </summary>
        /// <param name="wizardID">The wizard ID.</param>
        /// <param name="serviceID">The service ID.</param>
        /// <returns></returns>
        WizardDataListMergeTO MergeFixedWizardDataList(string wizardDL, string serviceDL);

        #endregion
       
        #endregion External Methods

    }
}
