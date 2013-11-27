﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using Dev2.DynamicServices;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;using System.Diagnostics.CodeAnalysis;
using Moq;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass][ExcludeFromCodeCoverage]
    public class DeleteLogTests
    {
        readonly static object MonitorLock = new object();
        readonly static object SyncRoot = new object();

        static string _testDir;

        #region ClassInitialize

        [ClassInitialize]
        public static void MyClassInitialize(TestContext context)
        {
            _testDir = context.DeploymentDirectory;
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

        #region Execute

        [TestMethod]
        public void DeleteLogExecuteWithNullFilePathExpectedReturnsError()
        {
            var workspace = new Mock<IWorkspace>();

            var values = new Dictionary<string, string> { { "FilePath", "" }, { "Directory", "xyz" } };
            var esb = new DeleteLog();
            var result = esb.Execute(values, workspace.Object);
            Assert.IsTrue(result.StartsWith("DeleteLog: Error"));
        }

        [TestMethod]
        public void DeleteLogExecuteWithNullDirectoryExpectedReturnsError()
        {
            var workspace = new Mock<IWorkspace>();

            var values = new Dictionary<string, string> { { "FilePath", "abc" }, { "Directory", "" } };
            var esb = new DeleteLog();
            var result = esb.Execute(values, workspace.Object);
            Assert.IsTrue(result.StartsWith("DeleteLog: Error"));
        }

        [TestMethod]
        public void DeleteLogExecuteWithNonExistingDirectoryExpectedReturnsError()
        {
            var workspace = new Mock<IWorkspace>();

            var values = new Dictionary<string, string> { { "FilePath", "abc" }, { "Directory", "xyz" } };
            var esb = new DeleteLog();
            var result = esb.Execute(values, workspace.Object);
            Assert.IsTrue(result.StartsWith("DeleteLog: Error"));
        }

        [TestMethod]
        public void DeleteLogExecuteWithNonExistingPathExpectedReturnsError()
        {
            var workspace = new Mock<IWorkspace>();

            var values = new Dictionary<string, string> { { "FilePath", Guid.NewGuid().ToString() }, { "Directory", "C:" } };
            var esb = new DeleteLog();
            var result = esb.Execute(values, workspace.Object);
            Assert.IsTrue(result.StartsWith("DeleteLog: Error"));
        }


        [TestMethod]
        public void DeleteLogExecuteWithValidPathExpectedReturnsSuccess()
        {
            //Lock because of access to file system
            lock(SyncRoot)
            {
                var fileName = Guid.NewGuid().ToString() + "_Test.log";
                var path = Path.Combine(_testDir, fileName);
                File.WriteAllText(path, "hello test");

                Assert.IsTrue(File.Exists(path));

                var workspace = new Mock<IWorkspace>();

                var values = new Dictionary<string, string> { { "FilePath", fileName }, { "Directory", _testDir } };
                var esb = new DeleteLog();
                var result = esb.Execute(values, workspace.Object);
                Assert.IsTrue(result.StartsWith("Success"));
                Assert.IsFalse(File.Exists(path));
            }
        }

        [TestMethod]
        public void DeleteLogExecuteWithValidPathAndLockedExpectedReturnsError()
        {
            //Lock because of access to file system
            lock(SyncRoot)
            {
                var fileName = Guid.NewGuid().ToString() + "_Test.log";
                var path = Path.Combine(_testDir, fileName);
                File.WriteAllText(path, "hello test");

                Assert.IsTrue(File.Exists(path));

                var fs = File.OpenRead(path);

                try
                {
                    var workspace = new Mock<IWorkspace>();

                    var values = new Dictionary<string, string> { { "FilePath", fileName }, { "Directory", _testDir } };
                    var esb = new DeleteLog();
                    var result = esb.Execute(values, workspace.Object);
                    Assert.IsTrue(result.StartsWith("DeleteLog: Error"));
                }
                finally
                {
                    fs.Close();
                }
            }
        }

        #endregion

        #region HandlesType

        [TestMethod]
        public void DeleteLogHandlesTypeExpectedReturnsDeleteLogService()
        {
            var esb = new DeleteLog();
            var result = esb.HandlesType();
            Assert.AreEqual("DeleteLogService", result);
        }

        #endregion

        #region CreateServiceEntry

        [TestMethod]
        public void DeleteLogCreateServiceEntryExpectedReturnsDynamicService()
        {
            var esb = new DeleteLog();
            var result = esb.CreateServiceEntry();
            Assert.AreEqual(esb.HandlesType(), result.Name);
            Assert.AreEqual("<DataList><Directory/><FilePath/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>", result.DataListSpecification);
            Assert.AreEqual(1, result.Actions.Count);

            var serviceAction = result.Actions[0];
            Assert.AreEqual(esb.HandlesType(), serviceAction.Name);
            Assert.AreEqual(enActionType.InvokeManagementDynamicService, serviceAction.ActionType);
            Assert.AreEqual(esb.HandlesType(), serviceAction.SourceMethod);
        }

        #endregion
    }
}
