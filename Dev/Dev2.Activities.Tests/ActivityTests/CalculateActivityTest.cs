﻿using System;
using System.Activities;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ActivityUnitTests;
using Dev2.Activities;
using Dev2.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;using System.Diagnostics.CodeAnalysis;
using System.Activities.Statements;
using System.Collections.Generic;
using Unlimited.Applications.BusinessDesignStudio.Activities;
// ReSharper disable InconsistentNaming
namespace Dev2.Tests.Activities.ActivityTests
{
    /// <summary>
    /// Summary description for CalculateActivityTest
    /// </summary>
    [TestClass][ExcludeFromCodeCoverage]
    public class CalculateActivityTest : BaseActivityUnitTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void CalculateActivity_ValidFunction_Expected_EvalPerformed()
        {

            TestStartNode = new FlowStep
            {
                Action = new DsfCalculateActivity { Expression = @"Sum([[scalar]], 10)", Result = "[[result]]" }
            };

            CurrentDl = "<ADL><RecordSet><Field></Field></RecordSet><scalar></scalar><result></result></ADL>";
            TestData = "<root><ADL><RecordSet><Field>10</Field></RecordSet><RecordSet><Field>20</Field></RecordSet><scalar>2</scalar><result></result></ADL></root>";
            IDSFDataObject result = ExecuteProcess();
            string error;
            string entry;

            GetScalarValueFromDataList(result.DataListID, "result", out entry, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(entry, "12");

        }

        [TestMethod]
        public void CalculateActivity_SimpleFunctionHandling_Expected_EvalPerformed()
        {

            TestStartNode = new FlowStep
            {
                Action = new DsfCalculateActivity { Expression = @"sum(10,20)", Result = "[[scalar]]" }
            };

            TestData = @"<ADL><scalar></scalar></ADL>";
            //TestData = ActivityStrings.CalculateActivityDataList;
            IDSFDataObject result = ExecuteProcess();
            string error;
            string entry;

            GetScalarValueFromDataList(result.DataListID, "scalar", out entry, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(entry, "30");
        }

        [TestMethod]
        public void CalculateActivity_ErrorHandeling_Expected_ErrorTag()
        {

            TestStartNode = new FlowStep
            {
                Action = new DsfCalculateActivity { Expression = @"sum(10,20)", Result = "[[//().rec]]" }
            };


            TestData = @"<ADL><scalar></scalar></ADL>";
            IDSFDataObject result = ExecuteProcess();

            string error;
            string entry;

            GetScalarValueFromDataList(result.DataListID, "//().rec", out entry, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.IsTrue(!string.IsNullOrEmpty(error));
        }

        // SN - 07-09-2012 - Commented out until intellisense issue is patched up

        [TestMethod]
        public void CalculateActivity_InValidFunction_Expected_Error()
        {

            TestStartNode = new FlowStep
            {
                Action = new DsfCalculateActivity { Expression = @"Sum([[RecordSet(1).Field]];[[RecordSet().Field]])", Result = "[[result]]" }
            };

            CurrentDl = "<ADL><RecordSet><Field></Field></RecordSet><scalar></scalar><result></result></ADL>";
            TestData = ActivityStrings.CalculateActivityADL;
            IDSFDataObject result = ExecuteProcess();

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.IsTrue(Compiler.HasErrors(result.DataListID));

        }


        [TestMethod]
        public void CalculateActivity_CommaSeperatedArgs_Expected_EvalPerformed()
        {

            TestStartNode = new FlowStep
            {
                Action = new DsfCalculateActivity { Expression = @"Sum([[scalar]],[[RecordSet(1).Field]],[[RecordSet(2).Field]])", Result = "[[result]]" }
            };

            CurrentDl = "<ADL><RecordSet><Field></Field></RecordSet><scalar></scalar><result></result></ADL>";
            TestData = "<root><ADL><RecordSet><Field>10</Field></RecordSet><RecordSet><Field>20</Field></RecordSet><scalar>2</scalar><result></result></ADL></root>";
            IDSFDataObject result = ExecuteProcess();
            const string expected = "32";
            string error;
            string actual;

            GetScalarValueFromDataList(result.DataListID, "result", out actual, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void CalculateActivity_RangedArgs_Expected_EvalPerformed()
        {

            TestStartNode = new FlowStep
            {
                Action = new DsfCalculateActivity { Expression = @"Sum([[RecordSet(1).Field]]:[[RecordSet(2).Field]])", Result = "[[result]]" }
            };

            CurrentDl = "<ADL><RecordSet><Field></Field></RecordSet><scalar></scalar><result></result></ADL>";
            TestData = "<root><ADL><RecordSet><Field>10</Field></RecordSet><RecordSet><Field>20</Field></RecordSet><scalar>2</scalar><result></result></ADL></root>";
            IDSFDataObject result = ExecuteProcess();
            const string expected = "30";
            string error;
            string actual;

            GetScalarValueFromDataList(result.DataListID, "result", out actual, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, actual);

        }

        //Bug 6438
        [TestMethod]
        public void CalculateActivity_ConcatenateScalar_Expected_EvalPerformed()
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfCalculateActivity { Expression = "Concatenate([[testVar]], \"moreText\")", Result = "[[NewTestVar]]" }
            };

            CurrentDl = "<ADL><testVar></testVar><NewTestVar></NewTestVar></ADL>";
            TestData = "<root><ADL><testVar>ATest</testVar><NewTestVar></NewTestVar></ADL></root>";
            IDSFDataObject result = ExecuteProcess();
            const string expected = "ATestmoreText";
            string error;
            string actual;

            GetScalarValueFromDataList(result.DataListID, "NewTestVar", out actual, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, actual);
        }
        [TestMethod]
        public void CalculateActivity_RightScalar_Expected_EvalPerformed()
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfCalculateActivity { Expression = "Right([[testVar]], 2)", Result = "[[NewTestVar]]" }
            };

            CurrentDl = "<ADL><testVar></testVar><NewTestVar></NewTestVar></ADL>";
            TestData = "<root><ADL><testVar>ATest</testVar><NewTestVar></NewTestVar></ADL></root>";
            IDSFDataObject result = ExecuteProcess();
            const string expected = "st";
            string error;
            string actual;

            GetScalarValueFromDataList(result.DataListID, "NewTestVar", out actual, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, actual);
        }
        [TestMethod]
        public void CalculateActivity_LeftScalar_Expected_EvalPerformed()
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfCalculateActivity { Expression = "Left([[testVar]], 2)", Result = "[[NewTestVar]]" }
            };

            CurrentDl = "<ADL><testVar></testVar><NewTestVar></NewTestVar></ADL>";
            TestData = "<root><ADL><testVar>ATest</testVar><NewTestVar></NewTestVar></ADL></root>";
            IDSFDataObject result = ExecuteProcess();
            const string expected = "AT";
            string error;
            string actual;

            GetScalarValueFromDataList(result.DataListID, "NewTestVar", out actual, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void CalculateActivity_ConcatenateRecSet_Expected_EvalPerformed()
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfCalculateActivity { Expression = "Concatenate([[testRecSet(1).testField]], \"moreText\")", Result = "[[NewTestVar]]" }
            };

            CurrentDl = "<ADL><testRecSet><testField></testField></testRecSet><NewTestVar></NewTestVar></ADL>";
            TestData = "<root><ADL><testRecSet><testField>ATest</testField></testRecSet><NewTestVar></NewTestVar></ADL></root>";
            IDSFDataObject result = ExecuteProcess();
            const string expected = "ATestmoreText";
            string error;
            string actual;

            GetScalarValueFromDataList(result.DataListID, "NewTestVar", out actual, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, actual);
        }

        // Bug 8467 - Travis.Frisinger
        [TestMethod]
        public void CalculateActivity_RecordsetWithStar_Expected_SumOf10()
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfCalculateActivity { Expression = "sum([[rec(*).val]])", Result = "[[sumResult]]" }
            };

            CurrentDl = "<ADL><rec><val></val></rec><sumResult></sumResult></ADL>";
            TestData = "<root><ADL><rec><val>1</val></rec><rec><val>2</val></rec><rec><val>3</val></rec><rec><val>4</val></rec></ADL></root>";
            IDSFDataObject result = ExecuteProcess();
            const string expected = "10";
            string error;
            string actual;

            GetScalarValueFromDataList(result.DataListID, "sumResult", out actual, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, actual);
        }

        // Bug 8467 - Travis.Frisinger
        [TestMethod]
        public void CalculateActivity_MultRecordsetWithStar_Expected_SumOf20()
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfCalculateActivity { Expression = "sum([[rec(*).val]],[[rec(*).val2]])", Result = "[[sumResult]]" }
            };

            CurrentDl = "<ADL><rec><val></val><val2/></rec><sumResult></sumResult></ADL>";
            TestData = "<root><ADL><rec><val>1</val><val2>10</val2></rec><rec><val>2</val></rec><rec><val>3</val></rec><rec><val>4</val></rec></ADL></root>";
            IDSFDataObject result = ExecuteProcess();
            const string expected = "20";
            string error;
            string actual;

            GetScalarValueFromDataList(result.DataListID, "sumResult", out actual, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, actual);
        }

        #region Get Debug Input/Output Tests

        /// <summary>
        /// Author : Massimo Guerrera Bug 8104 
        /// </summary>
        [TestMethod]
        public void Calculate_Get_Debug_Input_Output_With_Recordsets_Expected_Pass()
        {
            DsfCalculateActivity act = new DsfCalculateActivity { Expression = "sum([[Numeric(1).num]],[[Numeric(2).num]])", Result = "[[res]]" };

            List<DebugItem> inRes;
            List<DebugItem> outRes;

            var result = CheckActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(1, inRes.Count);
            Assert.AreEqual(4, inRes[0].FetchResultsList().Count);
            Assert.AreEqual(1, outRes.Count);
            Assert.AreEqual(3, outRes[0].FetchResultsList().Count);
        }

        /// <summary>
        /// Author : Massimo Guerrera Bug 8104 
        /// </summary>
        [TestMethod]
        public void Calculate_Get_Debug_Input_Output_With_Recordsets_Using_Star_Expected_Pass()
        {
            DsfCalculateActivity act = new DsfCalculateActivity { Expression = "sum([[Numeric(*).num]])", Result = "[[res]]" };

            List<DebugItem> inRes;
            List<DebugItem> outRes;

            var result = CheckActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape, ActivityStrings.DebugDataListWithData, out inRes, out outRes);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(1, inRes.Count);
            Assert.AreEqual(4, inRes[0].FetchResultsList().Count);
            Assert.AreEqual(1, outRes.Count);
            Assert.AreEqual(3, outRes[0].FetchResultsList().Count);
        }

        #endregion

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfCalculateActivity_GetForEachInputs")]
        public void DsfCalculateActivity_GetForEachInputs_NullContext_EmptyList()
        {
            //------------Setup for test--------------------------
            var dsfCalculateActivity = new DsfCalculateActivity();
            //------------Execute Test---------------------------
            var dsfForEachItems = dsfCalculateActivity.GetForEachInputs();
            //------------Assert Results-------------------------
            Assert.IsFalse(dsfForEachItems.Any());
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfCalculateActivity_GetForEachInputs")]
        public void DsfCalculateActivity_GetForEachInputs_WhenHasExpression_ReturnsInputList()
        {
            //------------Setup for test--------------------------
            const string expression = "sum([[Numeric(1).num]],[[Numeric(2).num]])";
            var act = new DsfCalculateActivity { Expression = expression, Result = "[[res]]" };
            //------------Execute Test---------------------------
            var dsfForEachItems = act.GetForEachInputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1,dsfForEachItems.Count);
            Assert.AreEqual(expression,dsfForEachItems[0].Name);
            Assert.AreEqual(expression,dsfForEachItems[0].Value);
        }
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfCalculateActivity_GetForEachOutputs")]
        public void DsfCalculateActivity_GetForEachOutputs_WhenHasResult_ReturnsInputList()
        {
            //------------Setup for test--------------------------
            const string expression = "sum([[Numeric(1).num]],[[Numeric(2).num]])";
            const string result = "[[res]]";
            var act = new DsfCalculateActivity { Expression = expression, Result = result };
            //------------Execute Test---------------------------
            var dsfForEachItems = act.GetForEachOutputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1,dsfForEachItems.Count);
            Assert.AreEqual(result, dsfForEachItems[0].Name);
            Assert.AreEqual(result, dsfForEachItems[0].Value);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfCalculateActivity_UpdateForEachInputs")]
        public void DsfCalculateActivity_UpdateForEachInputs_GivenNullUpdates_DoNothing()
        {
            //------------Setup for test--------------------------
            const string expression = "sum([[Numeric(1).num]],[[Numeric(2).num]])";
            const string result = "[[res]]";
            var act = new DsfCalculateActivity { Expression = expression, Result = result };
            //------------Execute Test---------------------------
            act.UpdateForEachInputs(null,null);
            //------------Assert Results-------------------------
            Assert.AreEqual(expression,act.Expression);
        }      
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfCalculateActivity_UpdateForEachInputs")]
        public void DsfCalculateActivity_UpdateForEachInputs_GivenMoreThanOneUpdates_DoNothing()
        {
            //------------Setup for test--------------------------
            const string expression = "sum([[Numeric(1).num]],[[Numeric(2).num]])";
            const string result = "[[res]]";
            var act = new DsfCalculateActivity { Expression = expression, Result = result };
            //------------Execute Test---------------------------
            var tuple1 = new Tuple<string, string>("Test","Test");
            var tuple2 = new Tuple<string, string>("Test2","Test2");
            act.UpdateForEachInputs(new List<Tuple<string, string>>{tuple1,tuple2}, null);
            //------------Assert Results-------------------------
            Assert.AreEqual(expression,act.Expression);
        }
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfCalculateActivity_UpdateForEachInputs")]
        public void DsfCalculateActivity_UpdateForEachInputs_GivenOneUpdate_UpdatesExpressionToItem2InTuple()
        {
            //------------Setup for test--------------------------
            const string expression = "sum([[Numeric(1).num]],[[Numeric(2).num]])";
            const string result = "[[res]]";
            var act = new DsfCalculateActivity { Expression = expression, Result = result };
            //------------Execute Test---------------------------
            var tuple1 = new Tuple<string, string>("Test1","Test");
            act.UpdateForEachInputs(new List<Tuple<string, string>>{tuple1}, null);
            //------------Assert Results-------------------------
            Assert.AreEqual("Test",act.Expression);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfCalculateActivity_UpdateForEachOutputs")]
        public void DsfCalculateActivity_UpdateForEachOutputs_GivenNullUpdates_DoNothing()
        {
            //------------Setup for test--------------------------
            const string expression = "sum([[Numeric(1).num]],[[Numeric(2).num]])";
            const string result = "[[res]]";
            var act = new DsfCalculateActivity { Expression = expression, Result = result };
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(null,null);
            //------------Assert Results-------------------------
            Assert.AreEqual(expression,act.Expression);
        }      
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfCalculateActivity_UpdateForEachOutputs")]
        public void DsfCalculateActivity_UpdateForEachOutputs_GivenMoreThanOneUpdates_DoNothing()
        {
            //------------Setup for test--------------------------
            const string expression = "sum([[Numeric(1).num]],[[Numeric(2).num]])";
            const string result = "[[res]]";
            var act = new DsfCalculateActivity { Expression = expression, Result = result };
            var tuple1 = new Tuple<string, string>("Test","Test");
            var tuple2 = new Tuple<string, string>("Test2","Test2");
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1, tuple2 }, null);
            //------------Assert Results-------------------------
            Assert.AreEqual(expression,act.Expression);
        }
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfCalculateActivity_UpdateForEachOutputs")]
        public void DsfCalculateActivity_UpdateForEachOutputs_GivenOneUpdate_UpdatesExpressionToItem2InTuple()
        {
            //------------Setup for test--------------------------
            const string expression = "sum([[Numeric(1).num]],[[Numeric(2).num]])";
            const string result = "[[res]]";
            var act = new DsfCalculateActivity { Expression = expression, Result = result };
            var tuple1 = new Tuple<string, string>("Test1","Test");
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1 }, null);
            //------------Assert Results-------------------------
            Assert.AreEqual("Test",act.Result);
        }
    }
}
