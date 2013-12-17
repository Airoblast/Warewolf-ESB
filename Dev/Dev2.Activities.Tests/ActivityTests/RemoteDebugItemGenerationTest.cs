﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using ActivityUnitTests;
using Dev2.Diagnostics;
using Dev2.Runtime.ESB.Management.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityTests
{
    /// <summary>
    /// Summary description for RemoteDebugItemGenerationTest
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class RemoteDebugItemGenerationTest : BaseActivityUnitTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void CanGenerateRemoteDebugItems()
        {
            DsfCountRecordsetActivity act = new DsfCountRecordsetActivity { RecordsetName = "[[Customers()]]", CountNumber = "[[res]]" };

            List<DebugItem> inRes;
            List<DebugItem> outRes;

            var obj = CheckActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes, true);

            IDSFDataObject dObj = (obj as IDSFDataObject);
            Guid id;
            Guid.TryParse(dObj.RemoteInvokerID, out id);
            var msgs = RemoteDebugMessageRepo.Instance.FetchDebugItems(id);
            // remove test datalist ;)
            DataListRemoval(dObj.DataListID);
            Assert.AreEqual(1, msgs.Count);
        }

        [TestMethod]
        public void CanSerializeRemoteDebugItems()
        {
            DsfCountRecordsetActivity act = new DsfCountRecordsetActivity { RecordsetName = "[[Customers()]]", CountNumber = "[[res]]" };

            List<DebugItem> inRes;
            List<DebugItem> outRes;

            var obj = CheckActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes, true);

            IDSFDataObject dObj = (obj as IDSFDataObject);
            Guid id;
            Guid.TryParse(dObj.RemoteInvokerID, out id);
            var msgs = RemoteDebugMessageRepo.Instance.FetchDebugItems(id);

            var tmp = JsonConvert.SerializeObject(msgs);

            var tmp2 = JsonConvert.DeserializeObject<IList<DebugState>>(tmp);

            // remove test datalist ;)
            DataListRemoval(dObj.DataListID);

            Assert.AreEqual(1, tmp2.Count);
        }

        [TestMethod]
        public void CanFetchRemoteDebugItemsViaSystemService()
        {
            DsfCountRecordsetActivity act = new DsfCountRecordsetActivity { RecordsetName = "[[Customers()]]", CountNumber = "[[res]]" };

            List<DebugItem> inRes;
            List<DebugItem> outRes;

            var obj = CheckActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes, true);

            IDSFDataObject dObj = (obj as IDSFDataObject);
            Guid id;
            Guid.TryParse(dObj.RemoteInvokerID, out id);

            FetchRemoteDebugMessages frm = new FetchRemoteDebugMessages();

            Dictionary<string, StringBuilder> d = new Dictionary<string, StringBuilder>();
            d["InvokerID"] = new StringBuilder(id.ToString());

            var str = frm.Execute(d, null);

            var tmp2 = JsonConvert.DeserializeObject<IList<DebugState>>(str.ToString());

            // remove test datalist ;)
            DataListRemoval(dObj.DataListID);

            Assert.AreEqual(1, tmp2.Count);
        }

    }
}
