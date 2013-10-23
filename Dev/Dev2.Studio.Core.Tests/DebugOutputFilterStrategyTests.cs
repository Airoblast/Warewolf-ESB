﻿using Dev2.Diagnostics;
using Dev2.Studio.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;using System.Diagnostics.CodeAnalysis;
using System;


namespace Dev2.Core.Tests
{
    [TestClass][ExcludeFromCodeCoverage]
    public class DebugOutputFilterStrategyTests
    {
        #region Class Members

        private static DebugOutputFilterStrategy _debugOutputFilterStrategy;


        #endregion Class Members

        #region Initialization

        [ClassInitialize()]
        public static void MyTestClassInitialize(TestContext testContext)
        {
            _debugOutputFilterStrategy = new DebugOutputFilterStrategy();
        }

        [TestInitialize()]
        public void TestInitialize()
        {
            //MediatorMessageTrapper.DeregUserInterfaceLayoutProvider();
        }

        #endregion Initialization

        #region Tests

        [TestMethod]
        public void Filter_Where_ContentIsNull_Expected_False()
        {
            bool expected = false;
            bool actual = _debugOutputFilterStrategy.Filter(null, "");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Filter_Where_FilterTextIsNull_Expected_False()
        {
            bool expected = false;
            bool actual = _debugOutputFilterStrategy.Filter("", null);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Filter_Where_ContentIsString_And_FilterTextContainsMatch_Expected_True()
        {
            bool expected = true;
            bool actual = _debugOutputFilterStrategy.Filter("cake", "ak");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Filter_Where_ContentIsString_And_FilterTextDoesntContainMatch_Expected_false()
        {
            bool expected = false;
            bool actual = _debugOutputFilterStrategy.Filter("cake", "123");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Filter_Where_ContentIsDebugState_And_FilterTextMatchesNothing_Expected_False()
        {
            DebugState debugState = new DebugState();

            bool expected = false;
            bool actual = _debugOutputFilterStrategy.Filter(debugState, "cake");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Filter_Where_ContentIsDebugState_And_FilterTextMatchesActivityType_Expected_True()
        {
            DebugState debugState = new DebugState();
            debugState.ActivityType = ActivityType.Workflow;

            bool expected = true;
            bool actual = _debugOutputFilterStrategy.Filter(debugState, "work");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Filter_Where_ContentIsDebugState_And_FilterTextMatchesDisplayName_Expected_True()
        {
            DebugState debugState = new DebugState();
            debugState.DisplayName = "Cake";

            bool expected = true;
            bool actual = _debugOutputFilterStrategy.Filter(debugState, "ak");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Filter_Where_ContentIsDebugState_And_ActivityTypeIsStep_And_FilterTextMatchesName_Expected_True()
        {
            DebugState debugState = new DebugState();
            debugState.ActivityType = ActivityType.Step;
            debugState.DisplayName = "Cake";

            bool expected = true;
            bool actual = _debugOutputFilterStrategy.Filter(debugState, "ak");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Filter_Where_ContentIsDebugState_And_ActivityTypeIsWorkflow_And_FilterTextMatchesServer_Expected_True()
        {
            DebugState debugState = new DebugState();
            debugState.ActivityType = ActivityType.Workflow;
            debugState.DisplayName = "Cake";

            bool expected = true;
            bool actual = _debugOutputFilterStrategy.Filter(debugState, "ak");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Filter_Where_ContentIsDebugState_And_FilterTextMatchesVersion_Expected_True()
        {
            DebugState debugState = new DebugState();
            debugState.DisplayName = "Cake";

            bool expected = true;
            bool actual = _debugOutputFilterStrategy.Filter(debugState, "ak");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Filter_Where_ContentIsDebugState_And_ActivityTypeIsStep_And_FilterTextMatchesDurration_Expected_True()
        {
            DebugState debugState = new DebugState();
            debugState.ActivityType = ActivityType.Step;
            debugState.StartTime = new DateTime(2012, 01, 02, 1, 2, 3);
            debugState.EndTime = new DateTime(2012, 01, 02, 2, 2, 3);

            bool expected = true;
            bool actual = _debugOutputFilterStrategy.Filter(debugState, "01:");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Filter_Where_ContentIsDebugState_And_ActivityTypeIsWorkflow_And_FilterTextMatchesStartTime_Expected_True()
        {
            DebugState debugState = new DebugState();
            debugState.ActivityType = ActivityType.Workflow;
            debugState.StateType = StateType.Before;
            debugState.StartTime = new DateTime(2012, 01, 02, 1, 2, 3);

            bool expected = true;
            bool actual = _debugOutputFilterStrategy.Filter(debugState, "2012");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Filter_Where_ContentIsDebugState_And_ActivityTypeIsWorkflow_And_FilterTextMatchesEndTime_Expected_True()
        {
            DebugState debugState = new DebugState();
            debugState.ActivityType = ActivityType.Workflow;
            debugState.StateType = StateType.After;
            debugState.EndTime = new DateTime(2012, 01, 02, 2, 2, 3);

            bool expected = true;
            bool actual = _debugOutputFilterStrategy.Filter(debugState, "2012");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Filter_Where_ContentIsDebugState_And_FilterTextMatchesInputOnName_Expected_True()
        {
            var debugState = new DebugState();
            DebugItem itemToAdd = new DebugItem();
            itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Variable, Value = "cake" });
            debugState.Inputs.Add(itemToAdd);

            const bool Expected = true;
            var actual = _debugOutputFilterStrategy.Filter(debugState, "ak");

            Assert.AreEqual(Expected, actual);
        }

        [TestMethod]
        public void Filter_Where_ContentIsDebugState_And_FilterTextMatchesOuputOnValue_Expected_True()
        {
            var debugState = new DebugState();
            DebugItem itemToAdd = new DebugItem();
            itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Variable, Value = "cake" });
            debugState.Outputs.Add(itemToAdd);

            const bool Expected = true;
            var actual = _debugOutputFilterStrategy.Filter(debugState, "ak");

            Assert.AreEqual(Expected, actual);
        }

        #endregion Tests
    }
}
