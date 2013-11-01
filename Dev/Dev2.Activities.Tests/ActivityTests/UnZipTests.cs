﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using ActivityUnitTests;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityTests
{
    /// <summary>
    /// Summary description for DateTimeDifferenceTests
    /// </summary>
    [TestClass][ExcludeFromCodeCoverage]
    public class UnZipTests : BaseActivityUnitTest
    {

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Get Input/Output Tests

        [TestMethod]
        public void UnZipActivity_GetInputs_Expected_Seven_Input()
        {
            DsfUnZip testAct = new DsfUnZip();

            IBinaryDataList inputs = testAct.GetInputs();

            var res = inputs.FetchAllEntries().Count;

            // remove test datalist ;)
            DataListRemoval(inputs.UID);

            Assert.AreEqual(7, res);
        }

        [TestMethod]
        public void UnZipActivity_GetOutputs_Expected_One_Output()
        {
            DsfUnZip testAct = new DsfUnZip();

            IBinaryDataList outputs = testAct.GetOutputs();

            var res = outputs.FetchAllEntries().Count;

            // remove test datalist ;)
            DataListRemoval(outputs.UID);

            Assert.AreEqual(1, res);
        }

        #endregion Get Input/Output Tests

        #region GetDebugInputs/Outputs

        /// <summary>
        /// Author : Massimo Guerrera Bug 8104 
        /// </summary>
        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void Unzip_Get_Debug_Input_Output_With_Scalar_Expected_Pass()
        // ReSharper restore InconsistentNaming
        {
            List<string> fileNames = new List<string>();
            var guid = Guid.NewGuid();
            fileNames.Add(Path.Combine(TestContext.TestRunDirectory, guid + "Dev2.txt"));

            foreach(string fileName in fileNames)
            {
                File.WriteAllText(fileName, @"TestData");
            }

            DsfZip preact = new DsfZip { InputPath = Path.Combine(TestContext.TestRunDirectory, guid + "[[CompanyName]].txt"), OutputPath = Path.Combine(TestContext.TestRunDirectory, guid + "[[CompanyName]]Zip.zip"), Result = "[[res]]" };

            List<DebugItem> inRes;
            List<DebugItem> outRes;

            var result = CheckPathOperationActivityDebugInputOutput(preact, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            foreach(string fileName in fileNames)
            {
                File.Delete(fileName);
            }

            DsfUnZip act = new DsfUnZip { InputPath = Path.Combine(TestContext.TestRunDirectory, guid + "[[CompanyName]]Zip.zip"), OutputPath = Path.Combine(TestContext.TestRunDirectory, guid + "[[CompanyName]].txt"), Result = "[[res]]" };

            result = CheckPathOperationActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(5, inRes.Count);
            Assert.AreEqual(4, inRes[0].FetchResultsList().Count);
            Assert.AreEqual(4, inRes[1].FetchResultsList().Count);
            Assert.AreEqual(1, inRes[2].FetchResultsList().Count);
            Assert.AreEqual(1, inRes[3].FetchResultsList().Count);
            Assert.AreEqual(1, inRes[4].FetchResultsList().Count);

            Assert.AreEqual(1, outRes.Count);
            Assert.AreEqual(3, outRes[0].FetchResultsList().Count);
        }

        /// <summary>
        /// Author : Massimo Guerrera Bug 8104 
        /// </summary>
        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void Unzip_Get_Debug_Input_Output_With_Recordset_Using_Star_Notation_Expected_Pass()
        // ReSharper restore InconsistentNaming
        {
            List<string> fileNames = new List<string>
                {
                    Path.Combine(TestContext.TestRunDirectory, Guid.NewGuid() + ".txt"),
                    Path.Combine(TestContext.TestRunDirectory, Guid.NewGuid() + ".txt")
                };

            List<string> zipfileNames = new List<string>
                {
                    Path.Combine(TestContext.TestRunDirectory, Guid.NewGuid() + ".zip"),
                    Path.Combine(TestContext.TestRunDirectory, Guid.NewGuid() + ".zip")
                };

            foreach(string fileName in fileNames)
            {
                File.WriteAllText(fileName, @"TestData");
            }
            List<List<string>> recsetList = new List<List<string>>();
            recsetList.Add(fileNames);
            recsetList.Add(zipfileNames);

            List<string> recsetnames = new List<string> { "FileNames", "ZipNames" };

            List<string> fieldnames = new List<string> { "Name", "Zips" };

            string dataListWithData;
            string dataListShape;

            CreateDataListWithMultipleRecsetAndCreateShape(recsetList, recsetnames, fieldnames, out dataListShape, out dataListWithData);

            DsfZip preact = new DsfZip { InputPath = "[[FileNames(*).Name]]", OutputPath = "[[ZipNames(*).Zips]]", Result = "[[res]]" };

            List<DebugItem> inRes;
            List<DebugItem> outRes;

            var result = CheckPathOperationActivityDebugInputOutput(preact, dataListShape,
                                                                dataListWithData, out inRes, out outRes);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            foreach(string fileName in fileNames)
            {
                File.Delete(fileName);
            }

            DsfUnZip act = new DsfUnZip { InputPath = "[[ZipNames(*).Zips]]", OutputPath = "[[FileNames(*).Name]]", Result = "[[res]]" };

            result = CheckPathOperationActivityDebugInputOutput(act, dataListShape,
                                                                dataListWithData, out inRes, out outRes);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(5, inRes.Count);
            Assert.AreEqual(7, inRes[0].FetchResultsList().Count);
            Assert.AreEqual(7, inRes[1].FetchResultsList().Count);
            Assert.AreEqual(1, inRes[2].FetchResultsList().Count);
            Assert.AreEqual(1, inRes[3].FetchResultsList().Count);
            Assert.AreEqual(1, inRes[4].FetchResultsList().Count);

            Assert.AreEqual(1, outRes.Count);
            Assert.AreEqual(3, outRes[0].FetchResultsList().Count);
        }

        #endregion

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DsfUnZip_Constructor")]
        public void DsfUnZip_Constructor_DisplayName_Unzip()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var dsfUnZip = new DsfUnZip();

            //------------Assert Results-------------------------
            Assert.AreEqual("Unzip", dsfUnZip.DisplayName);
        }
    }
}
