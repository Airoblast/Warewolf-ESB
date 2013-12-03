﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading;
using Dev2;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using Dev2.Tests.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;using System.Diagnostics.CodeAnalysis;
using System.Activities.Statements;
using System.Collections.Generic;
using Unlimited.Applications.BusinessDesignStudio.Activities;

// ReSharper disable CheckNamespace
namespace ActivityUnitTests.ActivityTests
// ReSharper restore CheckNamespace
{
    /// <summary>
    /// Summary description for DateTimeDifferenceTests
    /// </summary>
    [TestClass][ExcludeFromCodeCoverage]
    public class DateTimeDifferenceTests : BaseActivityUnitTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Positive Test Cases

        [TestMethod]
        public void Positive_With_Normal_Params_Expected_Positive()
        {
            SetupArguments(
                           "<root>" + ActivityStrings.DateTimeDiff_DataListShape + "</root>"
                         , ActivityStrings.DateTimeDiff_DataListShape
                         , "2012/03/05 09:20:30 AM"
                         , "2012/10/01 07:15:50 AM"
                         , "yyyy/mm/dd 12h:min:ss am/pm"
                         , "Days"
                         , "[[Result]]"
                         );

            IDSFDataObject result = ExecuteProcess();
            const string expected = "209";
            string actual;
            string error;
            GetScalarValueFromDataList(result.DataListID, "Result", out actual, out error);
            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Positive_UsingRecorsetWithStar_Expected_Positive()
        {
            SetupArguments(
                           ActivityStrings.DateTimeDifferenceDataListWithData
                         , ActivityStrings.DateTimeDifferenceDataListShape
                         , "[[recset1(*).f1]]"
                         , "[[recset2(*).f2]]"
                         , "dd/mm/yyyy"
                         , "Days"
                         , "[[resCol(*).res]]"
                         );

            IDSFDataObject result = ExecuteProcess();
            string error;
            IList<IBinaryDataListItem> results;
            GetRecordSetFieldValueFromDataList(result.DataListID, "resCol", "res", out results, out error);
            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual("8847", results[0].TheValue);
            Assert.AreEqual("9477", results[1].TheValue);
            Assert.AreEqual("9090", results[2].TheValue);
        }

        //2013.03.11: Ashley Lewis - PBI 9167 Moved to positive tests
        [TestMethod]
        public void Blank_InputFormat_Expected_Error()
        {
            SetupArguments(
                              "<root>" + ActivityStrings.DateTimeDiff_DataListShape + "</root>"
                            , ActivityStrings.DateTimeDiff_DataListShape
                            , DateTime.Now.ToString(CultureInfo.InvariantCulture)
                            , DateTime.Now.AddDays(209).ToString(CultureInfo.InvariantCulture)
                            , ""
                            , "Days"
                            , "[[Result]]"
                            );
            IDSFDataObject result = ExecuteProcess();
            const string expected = "209";
            string actual;
            string error;
            GetScalarValueFromDataList(result.DataListID, "Result", out actual, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [TestCategory("DateTimeDifferenceUnitTest")]
        [Owner("Massimo Guerrera")]
        // ReSharper disable InconsistentNaming
        public void DateTimeDifference_DateTimeDifferenceUnitTest_ExecuteWithBlankInput_DateTimeNowIsUsed()
        // ReSharper restore InconsistentNaming
        {
            DateTime now = DateTime.Now;

            const string currDL = @"<root><MyTestResult></MyTestResult></root>";
            SetupArguments(currDL
                         , currDL
                         , ""
                         ,""
                         , ""
                         , "Seconds"                       
                         , "[[MyTestResult]]");

            IDSFDataObject result = ExecuteProcess();

            string actual;
            string error;
            GetScalarValueFromDataList(result.DataListID, "MyTestResult", out actual, out error);           

            Assert.AreEqual("0",actual);
        }       

        #endregion Positive Test Cases

        #region Error Test Cases

        [TestMethod]
        public void Input1_Not_Matching_InputFormat_Expected_Error()
        {
            SetupArguments(
                           "<root>" + ActivityStrings.DateTimeDiff_DataListShape + "</root>"
                         , ActivityStrings.DateTimeDiff_DataListShape
                         , "2012 09:20:30 AM"
                         , "2012/10/01 07:15:50 AM"
                         , "yyyy/mm/dd 12h:min:ss am/pm"
                         , "Days"
                         , "[[Result]]"
                         );
            IDSFDataObject result = ExecuteProcess();

            var res = Compiler.HasErrors(result.DataListID);
            DataListRemoval(result.DataListID);
            
            Assert.IsTrue(res);
        }

        [TestMethod]
        public void Input2_Not_Matching_InputFormat_Expected_Error()
        {

            SetupArguments(
                            "<root>" + ActivityStrings.DateTimeDiff_DataListShape + "</root>"
                          , ActivityStrings.DateTimeDiff_DataListShape
                          , "2012/03/05 09:20:30 AM"
                          , "2012 07:15:50 AM"
                          , "yyyy/mm/dd 12h:min:ss am/pm"
                          , "Days"
                          , "[[Result]]"
                          );

            IDSFDataObject result = ExecuteProcess();

            var res = Compiler.HasErrors(result.DataListID);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);


            Assert.IsTrue(res);
        }

        [TestMethod]
        public void Invalid_InputFormat_Expected_Error()
        {
            SetupArguments(
                           "<root>" + ActivityStrings.DateTimeDiff_DataListShape + "</root>"
                         , ActivityStrings.DateTimeDiff_DataListShape
                         , "2012/03/05 09:20:30 AM"
                         , "2012/10/01 07:15:50 AM"
                         , "yyyy/wrongFromat/dd 12h:min:ss am/pm"
                         , "Days"
                         , "[[Result]]"
                         );
            IDSFDataObject result = ExecuteProcess();

            var res = Compiler.HasErrors(result.DataListID);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.IsTrue(res);
        }

        //[TestMethod]
        //[Ignore]
        //Because hugs said so.
        //public void Blank_Input1_Expected_NoError()
        //{
        //    var dateTime = new DateTime(2012, 10, 01, 7, 15, 50);
        //    IDSFDataObject result;
        //    using(ShimsContext.Create())
        //    {
        //        System.Fakes.ShimDateTime.NowGet = () => dateTime;
        //        SetupArguments(
        //            "<root>" + ActivityStrings.DateTimeDiff_DataListShape + "</root>"
        //            , ActivityStrings.DateTimeDiff_DataListShape
        //            , ""
        //            , "2012/10/01 07:15:50 AM"
        //            , "yyyy/mm/dd 12h:min:ss am/pm"
        //            , "Days"
        //            , "[[Result]]"
        //            );

        //        result = ExecuteProcess();
        //    }
        //    string expected = "0";
        //    string actual = string.Empty;
        //    string error = string.Empty;
        //    GetScalarValueFromDataList(result.DataListID, "Result", out actual, out error);

        //    Assert.AreEqual(expected, actual);
        //}

        //[TestMethod]
        //[Ignore]
        //Because hugs said so.
        //public void Blank_Input2_Expected_Error()
        //{
        //    var dateTime = new DateTime(2012, 10, 01, 7, 15, 50);
        //    IDSFDataObject result;
        //    using(ShimsContext.Create())
        //    {
        //        System.Fakes.ShimDateTime.NowGet = () => dateTime;
        //        SetupArguments(
        //            "<root>" + ActivityStrings.DateTimeDiff_DataListShape + "</root>"
        //            , ActivityStrings.DateTimeDiff_DataListShape
        //            , "2012/10/01 07:15:50 AM"
        //            , ""
        //            , "yyyy/mm/dd 12h:min:ss am/pm"
        //            , "Days"
        //            , "[[Result]]"
        //            );

        //        result = ExecuteProcess();
        //    }
        //    string expected = "0";
        //    string actual = string.Empty;
        //    string error = string.Empty;
        //    GetScalarValueFromDataList(result.DataListID, "Result", out actual, out error);

        //    Assert.AreEqual(expected, actual);
        //}

        [TestMethod]
        public void ErrorHandeling_Expected_ErrorTags()
        {
            SetupArguments(
                            "<root>" + ActivityStrings.DateTimeDiff_DataListShape + "</root>"
                          , ActivityStrings.DateTimeDiff_DataListShape
                          , "2012/10/01 07:15:50 AM"
                          , "2012/10/01 07:15:50 AM"
                          , "yyyy/mm/dd 12h:min:ss am/pm"
                          , "Days"
                          , "[[//().rec]]"
                          );

            IDSFDataObject result = ExecuteProcess();

            var res = Compiler.HasErrors(result.DataListID);
            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.IsTrue(res);
        }

        #endregion Error Test Cases

        #region Get Debug Input/Output Tests

        /// <summary>
        /// Author : Massimo Guerrera Bug 8104 
        /// </summary>
        [TestMethod]
        public void DateTimeDiffernce_Get_Debug_Input_Output_With_Scalars_Expected_Pass()
        {
            //Used recordset with a numeric index as a scalar because it the only place were i had date values and it evalues to a scalar 
            DsfDateTimeDifferenceActivity act = new DsfDateTimeDifferenceActivity { Input1 = "[[Customers(1).DOB]]", Input2 = "[[Customers(2).DOB]]", InputFormat = "yyyy/mm/dd", OutputType = "Days", Result = "[[res]]" };

            List<DebugItem> inRes;
            List<DebugItem> outRes;

            var result = CheckActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes);
            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(4, inRes.Count);
            Assert.AreEqual(4, inRes[0].FetchResultsList().Count);
            Assert.AreEqual(4, inRes[1].FetchResultsList().Count);
            Assert.AreEqual(2, inRes[2].FetchResultsList().Count);
            Assert.AreEqual(2, inRes[3].FetchResultsList().Count);
            Assert.AreEqual(1, outRes.Count);
            Assert.AreEqual(3, outRes[0].FetchResultsList().Count);
        }

        /// <summary>
        /// Author : Massimo Guerrera Bug 8104 
        /// </summary>
        [TestMethod]
        public void DateTimeDiffernce_Get_Debug_Input_Output_With_Recordsets_Expected_Pass()
        {
            DsfDateTimeDifferenceActivity act = new DsfDateTimeDifferenceActivity { Input1 = "[[Customers(*).DOB]]", Input2 = "[[Customers(2).DOB]]", InputFormat = "yyyy/mm/dd", OutputType = "Days", Result = "[[Numeric(*).num]]" };

            List<DebugItem> inRes;
            List<DebugItem> outRes;

            var result = CheckActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(4, inRes.Count);
            Assert.AreEqual(31, inRes[0].FetchResultsList().Count);
            Assert.AreEqual(4, inRes[1].FetchResultsList().Count);
            Assert.AreEqual(2, inRes[2].FetchResultsList().Count);
            Assert.AreEqual(2, inRes[3].FetchResultsList().Count);
            Assert.AreEqual(10, outRes.Count);
            Assert.AreEqual(3, outRes[0].FetchResultsList().Count);
        }

        #endregion

        #region Get Input/Output Tests

        [TestMethod]
        public void DateTimeDifference_GetInputs_Expected_Four_Input()
        {
            DsfDateTimeDifferenceActivity testAct = new DsfDateTimeDifferenceActivity { Input1 = "27-10-2012", Input2 = "28-10-2012", InputFormat = "dd-mm-yyyy", OutputType = "Years", Result = "[[result]]" };

            IBinaryDataList inputs = testAct.GetInputs();

            // remove test datalist ;)
            DataListRemoval(inputs.UID);

            Assert.AreEqual(4,inputs.FetchAllEntries().Count);
        }

        [TestMethod]
        public void DateTimeDifference_GetOutputs_Expected_One_Output()
        {
            DsfDateTimeDifferenceActivity testAct = new DsfDateTimeDifferenceActivity { Input1 = "27-10-2012", Input2 = "28-10-2012", InputFormat = "dd-mm-yyyy", OutputType = "Years", Result = "[[result]]" };

            IBinaryDataList outputs = testAct.GetOutputs();

            // remove test datalist ;)
            DataListRemoval(outputs.UID);

            Assert.AreEqual(1,outputs.FetchAllEntries().Count);
        }

        #endregion Get Input/Output Tests

        #region Private Test Methods

        private void SetupArguments(string currentDL, string testData, string input1, string input2, string inputFormat, string outputType, string result)
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfDateTimeDifferenceActivity { Input1 = input1, Input2 = input2, InputFormat = inputFormat, OutputType = outputType, Result = result }
            };
            CurrentDl = testData;
            TestData = currentDL;
        }

        #endregion Private Test Methods
    }
}
