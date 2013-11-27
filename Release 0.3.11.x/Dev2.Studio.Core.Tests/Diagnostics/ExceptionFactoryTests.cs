﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Dev2.Composition;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Diagnostics;
using Dev2.Studio.Factory;
using Microsoft.VisualStudio.TestTools.UnitTesting;using System.Diagnostics.CodeAnalysis;
using Moq;

namespace Dev2.Core.Tests.Diagnostics
{
    [TestClass][ExcludeFromCodeCoverage]
    public class ExceptionFactoryTests
    {
        Mock<IEnvironmentModel> _contextModel;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _contextModel = new Mock<IEnvironmentModel>();
            _contextModel.Setup(f => f.Connection.ExecuteCommand(It.IsAny<String>(),Guid.Empty,Guid.Empty));
            ImportService.CurrentContext = CompositionInitializer.InitializeForMeflessBaseViewModel();
        }

        #region Create Exception

        [TestMethod]
        public void ExceptionFactoryCreateDefaultExceptionsExpectedCorrectExceptionsReturned()
        {
            //Initialization
            var e = GetException();

            //Execute
            var vm = ExceptionFactory.Create(e);

            //Assert
            Assert.AreEqual(StringResources.ErrorPrefix + "Test Exception", vm.Message, "Exception view model is displaying an incorrect default exception message");
            Assert.AreEqual(1, vm.Exception.Count, "Wrong number of exceptions displayed by exception view model");
            Assert.AreEqual(StringResources.ErrorPrefix + "Test inner Exception", vm.Exception[0].Message, "Exception view model is displaying the wrong default inner exception message");
        }

        [TestMethod]
        public void ExceptionFactoryCreateCriticalExceptionsExpectedCorrectExceptionsReturned()
        {
            //Initialization
            var e = GetException();

            //Execute
            var vm = ExceptionFactory.Create(e, true);

            //Assert
            Assert.AreEqual(StringResources.CriticalExceptionMessage, vm.Message, "Exception view model is displaying an incorrect critical exception message");
            Assert.AreEqual(2, vm.Exception.Count, "Wrong number of exceptions displayed by exception view model");
            Assert.AreEqual(StringResources.ErrorPrefix + "Test Exception", vm.Exception[0].Message, "Exception view model is displaying the wrong exception message");
            Assert.AreEqual(StringResources.ErrorPrefix + "Test inner Exception", vm.Exception[1].Message, "Exception view model is displaying the wrong inner exception message");
        }

        #endregion

        #region Create Exception View Model

        [TestMethod]
        public void ExceptionFactoryCreateDefaultExceptionViewModelExpectedNonCriticalModelCreated()
        {
            //Initialization
            var e = GetException();

            //Execute
            var vm = ExceptionFactory.CreateViewModel(e, _contextModel.Object);

            //Assert
            Assert.IsFalse(vm.Critical, "Critical error view model created for non critical error");
        }

        [TestMethod]
        public void ExceptionFactoryCreateCriticalExceptionViewModelExpectedCriticalModelCreated()
        {
            //Initialization
            var e = GetException();

            //Execute
            var vm = ExceptionFactory.CreateViewModel(e, _contextModel.Object, ErrorSeverity.Critical);

            //Assert
            Assert.IsTrue(vm.Critical, "Non critical error view model created for critical error");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        public void ExceptionFactoryCreateCriticalExceptionViewModel_LogFilesExists_EnsureThatTheLogFilesAreAlsoInitialized()
        {
            //Initialization
            var e = GetException();
            const string studioLog = "Studio.log";
            ExceptionFactory.GetStudioLogTempPath = () => studioLog;
            const string uniqueTxt = "Unique.txt";
            ExceptionFactory.GetUniqueOutputPath = (ext) => uniqueTxt;
            const string severTxt = "Sever.txt";
            ExceptionFactory.GetServerLogTempPath = (evn) => severTxt; 
            //Execute
            var vm = ExceptionFactory.CreateViewModel(e, _contextModel.Object, ErrorSeverity.Critical);
            //Assert
            Assert.AreEqual(vm.StudioLogTempPath, studioLog);
            Assert.AreEqual(vm.ServerLogTempPath, severTxt);
            Assert.AreEqual(vm.OutputPath, uniqueTxt);
        }

        #endregion

        #region Create String Value

        // 14th Feb 2013
        // Created by Michael to verify additional trace info is included with the sent exception for Bug 8839
        [TestMethod]
        public void GetExceptionExpectedAdditionalTraceInfo()
        {
            string exceptionResult = ExceptionFactory.CreateStringValue(GetException()).ToString();
            StringAssert.Contains(exceptionResult, "Additional Trace Info", "Error - Additional Trace Info is missing from the exception!");
        }

        [TestMethod]
        public void GetExceptionWithCricalExceptionExpectedCriticalInfoIncluded()
        {
            string exceptionResult = ExceptionFactory.CreateStringValue(GetException(), null, true).ToString();
            StringAssert.Contains(exceptionResult, StringResources.CriticalExceptionMessage, "Error - Additional Trace Info is missing from the exception!");
        }
        
        #endregion

        #region Private Test Methods

        private static Exception GetException()
        {
            return new Exception("Test Exception", new Exception("Test inner Exception"));
        }

        #endregion
    }
}
