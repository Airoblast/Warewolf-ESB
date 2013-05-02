﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.MathOperations;
using Infragistics.Calculations;
using Infragistics.Calculations.Engine;
using Dev2;
using Infragistics.Calculations.CalcManager;

namespace Unlimited.UnitTest.Framework.MathOperationTest {
    /// <summary>
    /// Summary description for FunctionTest
    /// </summary>
    [TestClass]
    public class FunctionTest {
        public FunctionTest() {
            //
            // TODO: Add constructor logic here
            //
        }
        
        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext {
            get {
                return testContextInstance;
            }
            set {
                testContextInstance = value;
            }
        }

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

        #region Ctor

        [TestMethod]
        public void Function_AllInputsValid_Expected_ValidFunctionCreated() {
            string functionName = "Test Function";
            List<string> arguments = new List<string>();
            List<string> argumentDescriptions = new List<string>();
            string description = "Some Test Function";
            
            IFunction func = MathOpsFactory.CreateFunction(functionName, arguments, argumentDescriptions, description);
            Assert.IsNotNull(func);
        }

        [TestMethod]
        public void Function_NullFunctionName_Expected_ExceptionReturned() {
            string functionName = null;
            List<string> arguments = new List<string>();
            List<string> argumentDescriptions = new List<string>();
            string description = "Some Test Function";
            try {
                IFunction func = MathOpsFactory.CreateFunction(functionName, arguments, argumentDescriptions, description);
            }
            catch(ArgumentNullException) {
                // If we get this exception, it is expected.
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void Function_NullListOfArguments_Expected_EmptyListofArguments() {
            string functionName = "Test Function";
            List<string> arguments = null ;
            List<string> argumentDescriptions = null;
            string description = "Some Test Function";

            IFunction func = MathOpsFactory.CreateFunction(functionName, arguments, argumentDescriptions, description);
            Assert.AreEqual(0, func.arguments.Count);
        }

        [TestMethod]
        public void Function_NullDescription_Expected_EmptyDescription() {
            string functionName = "Test Function";
            List<string> arguments = new List<string>() { "arg1" };
            List<string> argumentDescriptions = new List<string>() { "the first argument" };
            string description = null;

            IFunction func = MathOpsFactory.CreateFunction(functionName, arguments, argumentDescriptions, description);
            Assert.IsTrue(func.Description.Equals(string.Empty));
        }

        [TestMethod]
        public void Function_NullDescriptionAndArguments_Expected_FunctionStillCreated() {
            string functionName = "Test Function";
            List<string> arguments = null;
            List<string> argumentDescriptions = null;
            string description = null;

            IFunction func = MathOpsFactory.CreateFunction(functionName, arguments, argumentDescriptions, description);
            Assert.IsNotNull(func);
        }

        #endregion Ctor

        #region CreateCustomFunction Test


        [TestMethod]
        public void CreateCustomFunction_AllValidValues_Expected_CustomFunctionCreatedAndRegisteredWithCalcManager() {
            string functionName = "TestFunction";
            List<string> arguments = new List<string>() { "x", "y" };
            List<string> argumentDescriptions = new List<string>() { "the first argument", "the second argument" };
            string description = "My TestFunction";

            IFunction func = MathOpsFactory.CreateFunction(functionName, arguments, argumentDescriptions, description);
            IDev2CalculationManager manager = new Dev2CalculationManager();
            Func<double[], double> function = new Func<double[], double>(AddAbs);

            func.CreateCustomFunction(functionName, arguments, argumentDescriptions, description, function, manager);
            CalculationValue value = manager.CalculateFormula("TestFunction(1)");
            Assert.AreEqual(123123423423, value.ToDouble());
        }

        [TestMethod]
        public void CreateCustomFunction_NullXamCalculationManager_Expected_ExceptionReturned() {
            string functionName = "TestFunction";
            List<string> arguments = new List<string>() { "x", "y" };
            List<string> argumentDescriptions = new List<string>() { "the first argument", "the second argument" };
            string description = "My TestFunction";

            IFunction func = MathOpsFactory.CreateFunction(functionName, arguments, argumentDescriptions, description);
            IDev2CalculationManager manager = null;
            Func<double[], double> function = new Func<double[], double>(AddAbs);
            try {
                func.CreateCustomFunction(functionName, arguments, argumentDescriptions, description, function, manager);
            }
            catch(NullReferenceException) {
                // since this exception is thrown we have our answer.
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void CreateCustomFunction_NullFunc_Expected_ExceptionReturned()
        {
            string functionName = "TestFunction";
            List<string> arguments = new List<string>() { "x", "y" };
            List<string> argumentDescriptions = new List<string>() { "the first argument", "the second argument" };
            string description = "My TestFunction";

            IFunction func = MathOpsFactory.CreateFunction(functionName, arguments, argumentDescriptions, description);
            IDev2CalculationManager manager = new Dev2CalculationManager();
            Func<double[], double> function = null;
            func.CreateCustomFunction(functionName, arguments, argumentDescriptions, description, function, manager);

            Assert.AreEqual("TestFunction", func.FunctionName);


        }

        [TestMethod]
        public void CreateCustomFunction_NullArgumentDescription_Expected_ExceptionReturned()
        {
            string functionName = "TestFunction";
            List<string> arguments = new List<string>() { "x", "y" };
            List<string> argumentDescriptions = null;
            string description = "My TestFunction";

            IFunction func = MathOpsFactory.CreateFunction(functionName, arguments, argumentDescriptions, description);
            IDev2CalculationManager manager = new Dev2CalculationManager();
            Func<double[], double> function = null;
            func.CreateCustomFunction(functionName, arguments, argumentDescriptions, description, function, manager);

            Assert.AreNotEqual(null, func.ArgumentDescriptions);
        }


        #endregion CreateCustom Function Test

        #region Private Test Methods

        private static double AddAbs(double[] x) {
            return 123123423423;
        }

        #endregion Private Test Methods
    }
}
