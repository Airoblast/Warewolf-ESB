﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using ActivityUnitTests;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityTests
{
    /// <summary>
    /// Summary description for DateTimeDifferenceTests
    /// </summary>
    [TestClass][ExcludeFromCodeCoverage]
    public class PathDeleteTests : BaseActivityUnitTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }


        #region Get Input/Output Tests

        [TestMethod]
        public void PathDeleteActivity_GetInputs_Expected_Four_Input()
        {
            DsfPathDelete testAct = new DsfPathDelete();

            IBinaryDataList inputs = testAct.GetInputs();

            var res = inputs.FetchAllEntries().Count;

            // remove test datalist ;)
            DataListRemoval(inputs.UID);

            Assert.AreEqual(4,res);
        }

        [TestMethod]
        public void PathDeleteActivity_GetOutputs_Expected_One_Output()
        {
            DsfPathDelete testAct = new DsfPathDelete();

            IBinaryDataList outputs = testAct.GetOutputs();

            var res = outputs.FetchAllEntries().Count;

            // remove test datalist ;)
            DataListRemoval(outputs.UID);

            Assert.AreEqual(1,res);
        }

        #endregion Get Input/Output Tests

        #region GetDebugInputs/Outputs

        /// <summary>
        /// Author : Massimo Guerrera Bug 8104 
        /// </summary>
        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void Delete_Get_Debug_Input_Output_With_Scalar_Expected_Pass()
        // ReSharper restore InconsistentNaming
        {
            File.WriteAllText(Path.Combine(TestContext.TestRunDirectory, "Dev2.txt"), "TestData");

            DsfPathDelete act = new DsfPathDelete { InputPath = string.Concat(TestContext.TestRunDirectory, "\\", "[[CompanyName]].txt"), Result = "[[res]]" };

            List<DebugItem> inRes;
            List<DebugItem> outRes;

            var result = CheckPathOperationActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(3, inRes.Count);
            Assert.AreEqual(4, inRes[0].FetchResultsList().Count);
            Assert.AreEqual(1, inRes[1].FetchResultsList().Count);
            Assert.AreEqual(1, inRes[2].FetchResultsList().Count);            

            Assert.AreEqual(1, outRes.Count);
            Assert.AreEqual(3, outRes[0].FetchResultsList().Count);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfDelete_Execute")]
        public void DsfDelete_Execute_WhenHasNullAsResultValue_ShouldStillCompleteExecution()
        {
            //------------Setup for test--------------------------
            File.WriteAllText(Path.Combine(TestContext.TestRunDirectory, "Dev2.txt"), "TestData");

            DsfPathDelete act = new DsfPathDelete { InputPath = string.Concat(TestContext.TestRunDirectory, "\\", "[[CompanyName]].txt"), Result = null };

            List<DebugItem> inRes;
            List<DebugItem> outRes;
            
            //------------Execute Test---------------------------
            var result = CheckPathOperationActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes);

            //------------Assert Results-------------------------
            DataListRemoval(result.DataListID);

            Assert.AreEqual(3, inRes.Count);
            Assert.AreEqual(4, inRes[0].FetchResultsList().Count);
            Assert.AreEqual(1, inRes[1].FetchResultsList().Count);
            Assert.AreEqual(1, inRes[2].FetchResultsList().Count);

            Assert.AreEqual(0, outRes.Count);
        }

        /// <summary>
        /// Author : Massimo Guerrera Bug 8104 
        /// </summary>
        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void Delete_Get_Debug_Input_Output_With_Recordset_Using_Star_Notation_Expected_Pass()
        // ReSharper restore InconsistentNaming
        {
            List<string> fileNames = new List<string>();
            fileNames.Add(Path.Combine(TestContext.TestRunDirectory, Guid.NewGuid() + ".txt"));
            fileNames.Add(Path.Combine(TestContext.TestRunDirectory, Guid.NewGuid() + ".txt"));
            fileNames.Add(Path.Combine(TestContext.TestRunDirectory, Guid.NewGuid() + ".txt"));
            fileNames.Add(Path.Combine(TestContext.TestRunDirectory, Guid.NewGuid() + ".txt"));

            foreach(string fileName in fileNames)
            {
                File.WriteAllText(fileName,@"TestData");
            }

            string dataListWithData;
            string dataListShape;

            CreateDataListWithRecsetAndCreateShape(fileNames, "FileNames", "Name", out dataListShape, out dataListWithData);

            DsfPathDelete act = new DsfPathDelete { InputPath = "[[FileNames(*).Name]]", Result = "[[res]]" };

            List<DebugItem> inRes;
            List<DebugItem> outRes;

            var result = CheckPathOperationActivityDebugInputOutput(act, dataListShape,
                                                                dataListWithData, out inRes, out outRes);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(3, inRes.Count);
            Assert.AreEqual(13, inRes[0].FetchResultsList().Count);
            Assert.AreEqual(1, inRes[1].FetchResultsList().Count);
            Assert.AreEqual(1, inRes[2].FetchResultsList().Count);            

            Assert.AreEqual(1, outRes.Count);
            Assert.AreEqual(3, outRes[0].FetchResultsList().Count);            
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("DsfPathDelete_Execution")]
        public void DsfPathDelete_Execution_FileNotFound_DebugOutputErrorMessageRelevant()
        {
            var dsfPathDelete = new DsfPathDelete{ InputPath = TestContext.TestRunDirectory + "\\some file that doesnt exist.txt", Result = "[[res]]" };
            List<DebugItem> inRes;
            List<DebugItem> outRes;
            string actual;
            string error;

            //------------Execute Test---------------------------
            var result = CheckPathOperationActivityDebugInputOutput(dsfPathDelete, "<ADL><FileNames><Name></Name></FileNames><res></res></ADL>",
                                                                "<ADL><FileNames><Name></Name></FileNames><res></res></ADL>", out inRes, out outRes);
            GetScalarValueFromDataList(result.DataListID, "Dev2System.Dev2Error", out actual, out error);

            // Assert Debug Output Error Message Relevant
            Assert.IsTrue(string.IsNullOrEmpty(actual) || !actual.Contains("null reference"), "Irrelevent error displayed for file not found.");

            //clean
            DataListRemoval(result.DataListID);
        }

        #endregion


        // ReSharper disable InconsistentNaming

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfPathDelete_UpdateForEachInputs")]
        public void DsfPathDelete_UpdateForEachInputs_NullUpdates_DoesNothing()
        {
            //------------Setup for test--------------------------
            var newGuid = Guid.NewGuid();
            var inputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]]2.txt");
            var act = new DsfPathDelete { InputPath = inputPath, Result = "[[CompanyName]]" };

            //------------Execute Test---------------------------
            act.UpdateForEachInputs(null, null);
            //------------Assert Results-------------------------
            Assert.AreEqual(inputPath, act.InputPath);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfPathDelete_UpdateForEachInputs")]
        public void DsfPathDelete_UpdateForEachInputs_MoreThan1Updates_DoesNotUpdates()
        {
            //------------Setup for test--------------------------
            var newGuid = Guid.NewGuid();
            var path = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]].txt");
            var inputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]]2.txt");
            var act = new DsfPathDelete { InputPath = inputPath, Result = "[[CompanyName]]" };

            var tuple1 = new Tuple<string, string>(inputPath, "Test");
            var tuple2 = new Tuple<string, string>(path, "Test2");
            //------------Execute Test---------------------------
            act.UpdateForEachInputs(new List<Tuple<string, string>> { tuple1, tuple2 }, null);
            //------------Assert Results-------------------------
            Assert.AreEqual(inputPath, act.InputPath);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfPathDelete_UpdateForEachInputs")]
        public void DsfPathDelete_UpdateForEachInputs_1Update_Updates()
        {
            //------------Setup for test--------------------------
            var newGuid = Guid.NewGuid();
            var inputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]]2.txt");
            var act = new DsfPathDelete { InputPath = inputPath, Result = "[[CompanyName]]" };

            var tuple1 = new Tuple<string, string>(inputPath, "Test");
            //------------Execute Test---------------------------
            act.UpdateForEachInputs(new List<Tuple<string, string>> { tuple1 }, null);
            //------------Assert Results-------------------------
            Assert.AreEqual("Test", act.InputPath);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfPathDelete_UpdateForEachOutputs")]
        public void DsfPathDelete_UpdateForEachOutputs_NullUpdates_DoesNothing()
        {
            //------------Setup for test--------------------------
            var newGuid = Guid.NewGuid();
            const string result = "[[CompanyName]]";
            var act = new DsfPathDelete { InputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]]2.txt"), Result = result };

            act.UpdateForEachOutputs(null, null);
            //------------Assert Results-------------------------
            Assert.AreEqual(result, act.Result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfPathDelete_UpdateForEachOutputs")]
        public void DsfPathDelete_UpdateForEachOutputs_MoreThan1Updates_DoesNothing()
        {
            //------------Setup for test--------------------------
            var newGuid = Guid.NewGuid();
            const string result = "[[CompanyName]]";
            var act = new DsfPathDelete { InputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]]2.txt"), Result = result };

            var tuple1 = new Tuple<string, string>("Test", "Test");
            var tuple2 = new Tuple<string, string>("Test2", "Test2");
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1, tuple2 }, null);
            //------------Assert Results-------------------------
            Assert.AreEqual(result, act.Result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfPathDelete_UpdateForEachOutputs")]
        public void DsfPathDelete_UpdateForEachOutputs_1Updates_UpdateResult()
        {
            //------------Setup for test--------------------------
            var newGuid = Guid.NewGuid();
            var inputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]]2.txt");
            var act = new DsfPathDelete { InputPath = inputPath, Result = "[[CompanyName]]" };

            var tuple1 = new Tuple<string, string>("Test", "Test");
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1 }, null);
            //------------Assert Results-------------------------
            Assert.AreEqual("Test", act.Result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfPathDelete_GetForEachInputs")]
        public void DsfPathDelete_GetForEachInputs_WhenHasExpression_ReturnsInputList()
        {
            //------------Setup for test--------------------------
            var newGuid = Guid.NewGuid();
            var inputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]]2.txt");
            var act = new DsfPathDelete { InputPath = inputPath, Result = "[[CompanyName]]" };

            //------------Execute Test---------------------------
            var dsfForEachItems = act.GetForEachInputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, dsfForEachItems.Count);
            Assert.AreEqual(inputPath, dsfForEachItems[0].Name);
            Assert.AreEqual(inputPath, dsfForEachItems[0].Value);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfPathDelete_GetForEachOutputs")]
        public void DsfPathDelete_GetForEachOutputs_WhenHasResult_ReturnsOutputList()
        {
            //------------Setup for test--------------------------
            var newGuid = Guid.NewGuid();
            const string result = "[[CompanyName]]";
            var act = new DsfPathDelete { InputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]]2.txt"), Result = result };

            //------------Execute Test---------------------------
            var dsfForEachItems = act.GetForEachOutputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, dsfForEachItems.Count);
            Assert.AreEqual(result, dsfForEachItems[0].Name);
            Assert.AreEqual(result, dsfForEachItems[0].Value);
        }
    }
}
