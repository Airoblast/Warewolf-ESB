﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dev2.Common;
using Dev2.DynamicServices;
using Dev2.Runtime.Configuration;
using Dev2.Runtime.Configuration.Settings;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class FindLogDirectoryTests
    {
        static string _testDir;
        readonly static object MonitorLock = new object();

        #region ClassInitialize

        [ClassInitialize]
        public static void MyClassInitialize(TestContext context)
        {
            _testDir = Path.Combine(context.DeploymentDirectory, "TestLogDirectory");
            Directory.CreateDirectory(_testDir);
        }

        #endregion

        #region TestInitialize/Cleanup

        [TestInitialize]
        public void MyTestInitialize()
        {
            Monitor.Enter(MonitorLock);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            Monitor.Exit(MonitorLock);
        }

        #endregion

        #region Execution
        
        //[TestMethod]
        //public void FindLogDirectoryWithNoWebServerUriReturnsError()
        //{
        //    var workspace = new Mock<IWorkspace>();

        //    var values = new Dictionary<string, string> {  };
        //    var esb = new FindLogDirectory();
        //    var result = esb.Execute(values, workspace.Object);
        //    Assert.IsTrue(result.Contains("Value cannot be null"));
        //}

        #endregion


        #region HandlesType

        [TestMethod]
        public void FindLogDirectoryHandlesTypeExpectedReturnsDeleteLogService()
        {
            var esb = new FindLogDirectory();
            var result = esb.HandlesType();
            Assert.AreEqual("FindLogDirectoryService", result);
        }

        #endregion

        #region CreateServiceEntry

        [TestMethod]
        public void FindLogDirectoryCreateServiceEntryExpectedReturnsDynamicService()
        {
            var esb = new FindLogDirectory();
            var result = esb.CreateServiceEntry();
            Assert.AreEqual(esb.HandlesType(), result.Name);
            Assert.AreEqual("<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>", result.DataListSpecification);
            Assert.AreEqual(1, result.Actions.Count);

            var serviceAction = result.Actions[0];
            Assert.AreEqual(esb.HandlesType(), serviceAction.Name);
            Assert.AreEqual(enActionType.InvokeManagementDynamicService, serviceAction.ActionType);
            Assert.AreEqual(esb.HandlesType(), serviceAction.SourceMethod);
        }

        #endregion
    }
}
