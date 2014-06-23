﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Dev2.Composition;
using Dev2.Services.Events;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Webs.Callbacks;
using Dev2.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.Webs
{
    [TestClass]    
    public class FileChooserCallbackHandlerTests
    {
        static ImportServiceContext _importContext;

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            _importContext = new ImportServiceContext();
            ImportService.CurrentContext = _importContext;

            ImportService.Initialize(new List<ComposablePartCatalog>
            {
                new FullTestAggregateCatalog()
            });
        }

        [TestInitialize]
        public void TestInitialize()
        {
            EventPublishers.Aggregator = null;
            AppSettings.LocalHost = "http://localhost:3142";
            ImportService.CurrentContext = _importContext;
        }


        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("FileChooserCallbackHandler_Save")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FileChooserCallbackHandler_Constructor_FileChooserMessageIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var handler = new FileChooserCallbackHandler(null);

            //------------Assert Results-------------------------
        }


        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("FileChooserCallbackHandler_Save")]
        public void FileChooserCallbackHandler_Save_ValueIsEmpty_ClearsMessageSelectedFiles()
        {
            //------------Setup for test--------------------------
            var message = new FileChooserMessage { SelectedFiles = new[] { "E:\\Data\\tasks1.txt", "E:\\Data\\tasks2.txt" } };
            var handler = new FileChooserCallbackHandler(message);

            //------------Execute Test---------------------------
            handler.Save(string.Empty);

            //------------Assert Results-------------------------
            Assert.IsNull(message.SelectedFiles);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("FileChooserCallbackHandler_Save")]
        public void FileChooserCallbackHandler_Save_ValueIsNotNull_UpdatesMessageSelectedFiles()
        {
            //------------Setup for test--------------------------
            var message = new FileChooserMessage();
            var handler = new FileChooserCallbackHandler(message);

            //------------Execute Test---------------------------
            handler.Save("\"{'filePaths':['E:\\\\\\\\Data\\\\\\\\tasks1.txt','E:\\\\\\\\Data\\\\\\\\tasks2.txt']}\"");

            //------------Assert Results-------------------------
            Assert.IsNotNull(message.SelectedFiles);

            var selectedFiles = message.SelectedFiles.ToList();
            Assert.AreEqual(2, selectedFiles.Count);
            Assert.AreEqual("E:\\Data\\tasks1.txt", selectedFiles[0]);
            Assert.AreEqual("E:\\Data\\tasks2.txt", selectedFiles[1]);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("FileChooserCallbackHandler_Save")]
        public void FileChooserCallbackHandler_Save_ValueIsAnyStringAndCloseWindowIsTrue_DoesInvokeClose()
        {
            //------------Setup for test--------------------------
            var message = new FileChooserMessage { SelectedFiles = new[] { "E:\\Data\\tasks1.txt", "E:\\Data\\tasks2.txt" } };
            var handler = new TestFileChooserCallbackHandler(message);

            //------------Execute Test---------------------------
            handler.Save(It.IsAny<string>(), true);

            //------------Assert Results-------------------------
            Assert.AreEqual(1, handler.CloseHitCount);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("FileChooserCallbackHandler_Save")]
        public void FileChooserCallbackHandler_Save_ValueIsAnyStringAndCloseWindowIsFalse_DoesNotInvokeClose()
        {
            //------------Setup for test--------------------------
            var message = new FileChooserMessage { SelectedFiles = new[] { "E:\\Data\\tasks1.txt", "E:\\Data\\tasks2.txt" } };
            var handler = new TestFileChooserCallbackHandler(message);

            //------------Execute Test---------------------------
            handler.Save(It.IsAny<string>(), false);

            //------------Assert Results-------------------------
            Assert.AreEqual(0, handler.CloseHitCount);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("FileChooserCallbackHandler_Save")]
        [ExpectedException(typeof(NotImplementedException))]
        public void FileChooserCallbackHandler_Save_ValueAndEnvironmentModel_ThrowsNotImplementedException()
        {
            //------------Setup for test--------------------------
            var message = new FileChooserMessage();
            var handler = new FileChooserCallbackHandler(message);

            //------------Execute Test---------------------------
            handler.Save("aaa", new Mock<IEnvironmentModel>().Object);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("FileChooserCallbackHandler_Save")]
        [ExpectedException(typeof(NotImplementedException))]
        public void FileChooserCallbackHandler_Save_JsonObjAndEnvironmentModel_ThrowsNotImplementedException()
        {
            //------------Setup for test--------------------------
            var message = new FileChooserMessage();
            var handler = new TestFileChooserCallbackHandler(message);

            //------------Execute Test---------------------------
            handler.TestSave(new Mock<IEnvironmentModel>().Object, new object());

            //------------Assert Results-------------------------
        }
    }

    public class TestFileChooserCallbackHandler : FileChooserCallbackHandler
    {
        public TestFileChooserCallbackHandler(FileChooserMessage message)
            : base(message)
        {
        }

        public void TestSave(IEnvironmentModel environmentModel, dynamic jsonObj)
        {
            Save(environmentModel, jsonObj);
        }

        public int CloseHitCount { get; private set; }
        public override void Close()
        {
            CloseHitCount++;
            base.Close();
        }

    }
}
