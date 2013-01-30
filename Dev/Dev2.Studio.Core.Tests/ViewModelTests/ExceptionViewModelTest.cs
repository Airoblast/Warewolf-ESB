﻿using System;
using System.IO;
using System.Threading;
using Dev2.Composition;
using Dev2.Studio.Factory;
using Dev2.Studio.Feedback;
using Dev2.Studio.ViewModels.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.ViewModels.Base;

namespace Dev2.Core.Tests.ViewModelTests
{
    [TestClass]
    public class ExceptionViewModelTest
    {
        #region Class Members
        private static string _tempTestFolder;
        private static readonly object _testLock = new object();
        private TestContext testContextInstance;
        #endregion Class Members

        #region Properties
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }
        #endregion Properties
        
        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize]
        public static void MyClassInitialize(TestContext testContext) 
        {
            _tempTestFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempTestFolder);
        }
        
        // Use ClassCleanup to run code after all tests in a class have run
        [ClassCleanup]
        public static void MyClassCleanup() 
        {
            DeleteTempTestFolder();
        }
        
        // Use TestInitialize to run code before running each test 
        [TestInitialize]
        public void MyTestInitialize() 
        {
            Monitor.Enter(_testLock);
        }
        
        // Use TestCleanup to run code after each test has run
        [TestCleanup]
        public void MyTestCleanup() 
        {
            Monitor.Exit(_testLock);
        }
    
        #endregion

        [TestMethod]
        public void Send_Where_OutputPathDoesntExist_Expected_PathCreatedAndActionIvoked()
        {

            var tmpOutputPath = new FileInfo(GetUniqueOutputPath(".txt"));
            string newOutputFolder = Path.Combine(tmpOutputPath.Directory.FullName, Guid.NewGuid().ToString());
            string newOutputpath = Path.Combine(newOutputFolder, tmpOutputPath.Name);

            var vm = new ExceptionViewModel();

            vm.OutputPath = newOutputpath; 
            vm.OutputText = ExceptionFactory.Create(GetException()).ToString();

            var mockEmailAction = new Mock<IFeedbackAction>();
            mockEmailAction.Setup(c => c.StartFeedback()).Verifiable();

            var mockInvoker = new Mock<IFeedbackInvoker>();
            mockInvoker.Setup(i => i.InvokeFeedback(It.IsAny<IFeedbackAction>())).Verifiable();

            vm.FeedbackAction = mockEmailAction.Object;
            vm.FeedbackInvoker = mockInvoker.Object; 

            vm.SendReport();

            mockInvoker.Verify(a => a.InvokeFeedback(It.IsAny<IFeedbackAction>()), Times.Once());
            Assert.AreEqual(Directory.Exists(newOutputFolder), true);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Send_Where_OutputPathIsntZipOrXml_Expected_InvalidOperationException()
        {
            string outputPath = GetUniqueOutputPath(".cake");
            var vm = new ExceptionViewModel();
            vm.OutputPath = outputPath;
            vm.SendReport();
        }

        [TestMethod]
        public void ShowExceptionDialog_Expected_WindowManagerInvokedForViewModel()
        {
            string outputPath = GetUniqueOutputPath(".cake");
            var vm = new ExceptionViewModel();

            Mock<IDev2WindowManager> mockWinManager = new Mock<IDev2WindowManager>();
            mockWinManager.Setup(c => c.ShowDialog(It.IsAny<BaseViewModel>())).Verifiable();

            vm.WindowNavigation = mockWinManager.Object;
            vm.Show();

            mockWinManager.Verify(mgr => mgr.ShowDialog(It.IsAny<ExceptionViewModel>()), Times.Once());
        }

        [TestMethod]
        [ExpectedException(typeof(IOException))]
        public void Send_Where_OutputPathAlreadyExists_Expected_FileIOException()
        {             
            //Create file which is to conflict with the output path of the recorder
            FileInfo conflictingPath = new FileInfo(GetUniqueOutputPath(".txt"));
            conflictingPath.Create().Close();

            var vm = new ExceptionViewModel();
            vm.OutputPath = conflictingPath.FullName;

            vm.SendReport();
        }

        private static Exception GetException()
        {
            return new Exception("Test Exception", new Exception("Test inner Exception"));
        }

        private static void DeleteTempTestFolder()
        {
            try
            {
                Directory.Delete(_tempTestFolder, true);
            }
            catch (Exception)
            {
                //Fail silently if folder couldn't be deleted.
            }
        }

        private static string GetUniqueOutputPath(string extension)
        {
            return Path.Combine(_tempTestFolder, Guid.NewGuid().ToString() + extension);
        }
    }
}
