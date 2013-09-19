﻿using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ActivityUnitTests;
using Dev2.Activities;
using Dev2.Common;
using Dev2.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Activities.ActivityTests
{
    /// <summary>
    /// Summary description for CountRecordsTest
    /// </summary>
    [TestClass]
    public class ExecuteCommandLineTest : BaseActivityUnitTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void ExecuteCommandLineShouldHaveInputProperty()
        {
            //------------Setup for test--------------------------
            var activity = new DsfExecuteCommandLineActivity();
            var randomString = GetRandomString();
            //------------Execute Test---------------------------
            activity.CommandFileName = randomString;
            //------------Assert Results-------------------------
            Assert.AreEqual(randomString,activity.CommandFileName);
        }

        static string GetRandomString()
        {
            return new Random().Next(0, 1000).ToString(CultureInfo.InvariantCulture);
        }

        [TestMethod]
        public void ExecuteCommandLineShouldHaveCommandResultProperty()
        {
            //------------Setup for test--------------------------
            var activity = new DsfExecuteCommandLineActivity();
            var randomString = GetRandomString();
            //------------Execute Test---------------------------
            activity.CommandResult = randomString;
            //------------Assert Results-------------------------
            Assert.AreEqual(randomString, activity.CommandResult);
         }

        [TestMethod]
        public void OnExecuteWhereConsoleDoesNothingExpectNothingForResult()
        {
            //------------Setup for test--------------------------
            var activity = new DsfExecuteCommandLineActivity();
            var randomString = "\""+TestContext.DeploymentDirectory+"\\ConsoleAppToTestExecuteCommandLineActivity.exe\"";
            activity.CommandFileName = randomString;
            activity.CommandResult = "[[OutVar1]]";
            TestStartNode = new FlowStep
            {
                Action = activity
            };
            string actual;
            string error;
            TestData = "<root><OutVar1 /></root>";
            //------------Execute Test---------------------------
            var result = ExecuteProcess();
            //------------Assert Results-------------------------

            GetScalarValueFromDataList(result.DataListID, "OutVar1", out actual, out error);
            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            StringAssert.Contains(actual,"");
        }

        [TestMethod]
        public void OnExecuteWhereConsolePathHasSpacesIsNotWrappedInQuotesExpectError()
        {
            //------------Setup for test--------------------------
            var activity = new DsfExecuteCommandLineActivity();
            var randomString = TestContext.DeploymentDirectory + "\\ConsoleAppToTestExecuteCommandLineActivity.exe";
            activity.CommandFileName = randomString;
            activity.CommandResult = "[[OutVar1]]";
            TestStartNode = new FlowStep
            {
                Action = activity
            };
            TestData = "<root><OutVar1 /></root>";
            //------------Execute Test---------------------------
            var result = ExecuteProcess();
            var res = Compiler.HasErrors(result.DataListID);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);
            //------------Assert Results-------------------------
            Assert.IsTrue(res);
        }

        [TestMethod]
        public void OnExecuteWhereConsolePathHasNoSpacesIsNotWrappedInQuotesExpectSuccess()
        {
            //------------Setup for test--------------------------
            var activity = new DsfExecuteCommandLineActivity();
            const string randomString = "./ConsoleAppToTestExecuteCommandLineActivity.exe";
            activity.CommandFileName = randomString;
            activity.CommandResult = "[[OutVar1]]";
            TestStartNode = new FlowStep
            {
                Action = activity
            };
            string actual;
            string error;
            TestData = "<root><OutVar1 /></root>";
            //------------Execute Test---------------------------
            var result = ExecuteProcess();
            //------------Assert Results-------------------------
            Assert.IsFalse(Compiler.HasErrors(result.DataListID));
            GetScalarValueFromDataList(result.DataListID, "OutVar1", out actual, out error);
            
            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            StringAssert.Contains(actual, "");
        }


        [TestMethod]
        public void OnExecuteWhereConsoleOutputsWithArgsWrappedInQuotesExpectSuccess()
        {
            //------------Setup for test--------------------------
            var activity = new DsfExecuteCommandLineActivity();
            var randomString = "\"" + TestContext.DeploymentDirectory + "\\ConsoleAppToTestExecuteCommandLineActivity.exe\" \"output\"";
            activity.CommandFileName = randomString;
            activity.CommandResult = "[[OutVar1]]";
            TestStartNode = new FlowStep
            {
                Action = activity
            };
            string actual;
            string error;
            TestData = "<root><OutVar1 /></root>";
            //------------Execute Test---------------------------
            var result = ExecuteProcess();
            //------------Assert Results-------------------------
            Assert.IsFalse(Compiler.HasErrors(result.DataListID));
            GetScalarValueFromDataList(result.DataListID, "OutVar1", out actual, out error);
            
            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            StringAssert.Contains(actual, "This is output from the user");

        } 

        [TestMethod]
        public void OnExecuteWhereConsoleOutputsExpectOutputForResult()
        {
            //------------Setup for test--------------------------
            var activity = new DsfExecuteCommandLineActivity();
            var randomString = "\"" + TestContext.DeploymentDirectory + "\\ConsoleAppToTestExecuteCommandLineActivity.exe\" output";
            activity.CommandFileName = randomString;
            activity.CommandResult = "[[OutVar1]]";
            TestStartNode = new FlowStep
            {
                Action = activity
            };
            string actual;
            string error;
            TestData = "<root><OutVar1 /></root>";
            //------------Execute Test---------------------------
            var result = ExecuteProcess();
            //------------Assert Results-------------------------
            Assert.IsFalse(Compiler.HasErrors(result.DataListID));
            GetScalarValueFromDataList(result.DataListID, "OutVar1", out actual, out error);
            
            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            StringAssert.Contains(actual, "This is output from the user");
           
        } 
        
        [TestMethod]
        public void OnExecuteWhereConsoleOutputsExpectOutputForResultCmddirc()
        {
            //------------Setup for test--------------------------
            var activity = new DsfExecuteCommandLineActivity();
            const string randomString = @"cmd.exe /c dir C:\";
            activity.CommandFileName = randomString;
            activity.CommandResult = "[[OutVar1]]";
            TestStartNode = new FlowStep
            {
                Action = activity
            };
            TestData = "<root><OutVar1 /></root>";
            //------------Execute Test---------------------------
            var result = ExecuteProcess();
            //------------Assert Results-------------------------
            Assert.IsTrue(Compiler.HasErrors(result.DataListID));
            var fetchErrors = Compiler.FetchErrors(result.DataListID);
            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            StringAssert.Contains(fetchErrors, "Cannot execute CMD from tool.");
           
        }


        [TestMethod]
        public void OnExecuteWhereConsoleOutputsExpectOutputForResultExplorer()
        {
            //------------Setup for test--------------------------
            var activity = new DsfExecuteCommandLineActivity();
            const string randomString = @"C:\Windows\explorer.exe";
            activity.CommandFileName = randomString;
            activity.CommandResult = "[[OutVar1]]";
            TestStartNode = new FlowStep
            {
                Action = activity
            };
            TestData = "<root><OutVar1 /></root>";
            //------------Execute Test---------------------------
            var result = ExecuteProcess();
            //------------Assert Results-------------------------
            Assert.IsTrue(Compiler.HasErrors(result.DataListID));
            var fetchErrors = Compiler.FetchErrors(result.DataListID);
            // remove test datalist ;)
            DataListRemoval(result.DataListID);
            StringAssert.Contains(fetchErrors, "Cannot execute explorer from tool.");
        }


        [TestMethod]
        public void OnExecuteWhereConsoleErrorsExpectErrorInDatalist()
        {
           // ------------Setup for test--------------------------
            var activity = new DsfExecuteCommandLineActivity();
            var randomString = "\"" + TestContext.DeploymentDirectory + "\\ConsoleAppToTestExecuteCommandLineActivity.exe\" error";
            activity.CommandFileName = randomString;
            activity.CommandResult = "[[OutVar1]]";
            TestStartNode = new FlowStep
            {
                Action = activity
            };
            string actual;
            string error;
            TestData = "<root><OutVar1 /></root>";
            //------------Execute Test---------------------------
            var result = ExecuteProcess();
            //------------Assert Results-------------------------
            Assert.IsTrue(Compiler.HasErrors(result.DataListID));
            GetScalarValueFromDataList(result.DataListID, "OutVar1", out actual, out error);
            StringAssert.Contains(actual, "This is error");
            var fetchErrors = Compiler.FetchErrors(result.DataListID);
            // remove test datalist ;)
            DataListRemoval(result.DataListID);
            StringAssert.Contains(fetchErrors, "The console errored");
            
        }

        [TestMethod]
        public void OnExecuteWhereOutputToRecordWithNoIndexWithConsoleOutputsExpectOutputForResultAppendedToRecordsets()
        {
            //------------Setup for test--------------------------
            var activity = new DsfExecuteCommandLineActivity();
            var randomString = "\"" + TestContext.DeploymentDirectory + "\\ConsoleAppToTestExecuteCommandLineActivity.exe\" output";
            activity.CommandFileName = randomString;
            activity.CommandResult = "[[recset1().field1]]";
            TestStartNode = new FlowStep
            {
                Action = activity
            };
            var expected = new List<string> { "This is output from the user" };
            string error;
            CurrentDl = "<ADL><recset1><field1/></recset1></ADL>";
            TestData = "<root></root>";
            //------------Execute Test---------------------------
            var result = ExecuteProcess();
            //------------Assert Results-------------------------
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "recset1", "field1", out error);
            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            var actualArray = actual.ToArray();
            actual.Clear();

            actual.AddRange(actualArray.Select(s => s.Trim()));

            CollectionAssert.AreEqual(expected, actual, new ActivityUnitTests.Utils.StringComparer());
          
        }
      
    
        [TestMethod]
        public void OnExecuteWhereOutputToRecordWithStarIndexWithConsoleOutputsExpectOutputForResultOverwriteToRecordsets()
        {
            //------------Setup for test--------------------------
            var activity = new DsfExecuteCommandLineActivity();
            var randomString = "\"" + TestContext.DeploymentDirectory + "\\ConsoleAppToTestExecuteCommandLineActivity.exe\" output";
            activity.CommandFileName = randomString;
            activity.CommandResult = "[[recset1(*).field1]]";
            TestStartNode = new FlowStep
            {
                Action = activity
            };
            var expected = new List<string> { "This is output from the user" };
            string error;
            CurrentDl = "<ADL><recset1><field1/></recset1></ADL>";
            TestData = "<root></root>";
            //------------Execute Test---------------------------
            var result = ExecuteProcess();
            //------------Assert Results-------------------------
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "recset1", "field1", out error);
            
            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            var actualArray = actual.ToArray();
            actual.Clear();

            actual.AddRange(actualArray.Select(s => s.Trim()));

            CollectionAssert.AreEqual(expected, actual, new ActivityUnitTests.Utils.StringComparer());
           
        } 
        
        [TestMethod]
        public void OnExecuteWhereOutputToRecordWithSpecificIndexWithConsoleOutputsExpectOutputForResultInsertsToRecordsets()
        {
            //------------Setup for test--------------------------
            var activity = new DsfExecuteCommandLineActivity();
            var randomString = "\"" + TestContext.DeploymentDirectory + "\\ConsoleAppToTestExecuteCommandLineActivity.exe\" output";
            activity.CommandFileName = randomString;
            activity.CommandResult = "[[recset1(1).field1]]";
            SetUpForExecution(activity, "<root></root>", "<ADL><recset1><field1/></recset1></ADL>");
            var expected = new List<string> { "This is output from the user" };
            string error;
            //------------Execute Test---------------------------
            var result = ExecuteProcess();
            //------------Assert Results-------------------------
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "recset1", "field1", out error);
            
            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            var actualArray = actual.ToArray();
            actual.Clear();

            actual.AddRange(actualArray.Select(s => s.Trim()));

            CollectionAssert.AreEqual(expected, actual, new ActivityUnitTests.Utils.StringComparer());

          
        }
        
        [TestMethod]
        public void OnExecuteWhereInputFromRecordWithSpecificIndexWithConsoleOutputsExpectOutputForResultInsertsToRecordsets()
        {
            //------------Setup for test--------------------------
            var activity = new DsfExecuteCommandLineActivity();
            var randomString = "\"" + TestContext.DeploymentDirectory + "\\ConsoleAppToTestExecuteCommandLineActivity.exe\" output";
            activity.CommandFileName = "[[recset1(1).rec1]]";
            activity.CommandResult = "[[recset1(1).field1]]";
            var testData = "<root><recset1><field1></field1><rec1>" + randomString + "</rec1></recset1></root>";
            SetUpForExecution(activity, testData, "<ADL><recset1><field1></field1><rec1></rec1></recset1></ADL>");
            var expected = new List<string> { "This is output from the user" };
            string error;
            //------------Execute Test---------------------------
            var result = ExecuteProcess();
            //------------Assert Results-------------------------
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "recset1", "field1", out error);
            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            var actualArray = actual.ToArray();
            actual.Clear();
            actual.AddRange(actualArray.Select(s => s.Trim()));
            CollectionAssert.AreEqual(expected, actual, new ActivityUnitTests.Utils.StringComparer());
        }

        [TestMethod]
        public void OnExecuteWhereMultipleInputFromRecordSetWithOutputToRecordSetExpectOutputResultsToMultipleRowsInRecordSet()
        {
            //------------Setup for test--------------------------
            var activity = new DsfExecuteCommandLineActivity();
            var command1 = "\"" + TestContext.DeploymentDirectory + "\\ConsoleAppToTestExecuteCommandLineActivity.exe\" output";
            var command2 = "\"" + TestContext.DeploymentDirectory + "\\ConsoleAppToTestExecuteCommandLineActivity.exe\" differentoutput";
            activity.CommandFileName = "[[recset1(*).rec1]]";
            activity.CommandResult = "[[recset2().field1]]";
            var testData = "<root><recset1><rec1>" + command1 + "</rec1></recset1><recset1><rec1>" + command2 + "</rec1></recset1></root>";
            SetUpForExecution(activity, testData, "<ADL><recset1><rec1></rec1></recset1><recset2><field1></field1></recset2></ADL>");
            var expected = new List<string> { "This is output from the user", "This is a different output from the user" };
            string error;
            //------------Execute Test---------------------------
            var result = ExecuteProcess();
            //------------Assert Results-------------------------
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "recset2", "field1", out error);
            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            var actualArray = actual.ToArray();
            actual.Clear();
            actual.AddRange(actualArray.Select(s => s.Trim()));
            CollectionAssert.AreEqual(expected, actual, new ActivityUnitTests.Utils.StringComparer());
        }

        [TestMethod]
        public void OnExecuteWhereMultipleInputFromRecordSetWithOutputToScalarExpectOutputResultOfLastCommandinScalar()
        {
            //------------Setup for test--------------------------
            var activity = new DsfExecuteCommandLineActivity();
            var command1 = "\"" + TestContext.DeploymentDirectory + "\\ConsoleAppToTestExecuteCommandLineActivity.exe\" output";
            var command2 = "\"" + TestContext.DeploymentDirectory + "\\ConsoleAppToTestExecuteCommandLineActivity.exe\" differentoutput";
            activity.CommandFileName = "[[recset1(*).rec1]]";
            activity.CommandResult = "[[OutVar1]]";
            var testData = "<root><recset1><rec1>" + command1 + "</rec1></recset1><recset1><rec1>" + command2 + "</rec1></recset1></root>";
            SetUpForExecution(activity, testData, "<ADL><recset1><rec1></rec1></recset1><OutVar1/></ADL>");
            const string expected = "This is a different output from the user";
            string error;
            string actual;
            //------------Execute Test---------------------------
            var result = ExecuteProcess();
            //------------Assert Results-------------------------
            GetScalarValueFromDataList(result.DataListID, "OutVar1", out actual, out error);
            
            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            StringAssert.Contains(actual,expected);
        }

        [TestMethod]
        public void ExecuteCommandLineGetDebugInputOutputExpectedCorrectResults()       
        {
            var command1 = "\"" + TestContext.DeploymentDirectory + "\\ConsoleAppToTestExecuteCommandLineActivity.exe\" output";
            DsfExecuteCommandLineActivity act = new DsfExecuteCommandLineActivity { CommandFileName = command1, CommandResult = "[[OutVar1]]" };

            List<DebugItem> inRes;
            List<DebugItem> outRes;

            var result = CheckActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes);


            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(1, inRes.Count);
            IList<DebugItemResult> debugInputResults = inRes[0].FetchResultsList();
            Assert.AreEqual(2, debugInputResults.Count);
            Assert.AreEqual(DebugItemResultType.Label, debugInputResults[0].Type);
            Assert.AreEqual("Command to execute", debugInputResults[0].Value);
            Assert.AreEqual(DebugItemResultType.Value, debugInputResults[1].Type);
            StringAssert.Contains(command1, debugInputResults[1].Value);

            Assert.AreEqual(1, outRes.Count);
            IList<DebugItemResult> debugOutputResults = outRes[0].FetchResultsList();
            Assert.AreEqual(3, debugOutputResults.Count);            
            Assert.AreEqual(DebugItemResultType.Variable, debugOutputResults[0].Type);
            Assert.AreEqual("[[OutVar1]]", debugOutputResults[0].Value);             
            Assert.AreEqual(DebugItemResultType.Label, debugOutputResults[1].Type);
            Assert.AreEqual(GlobalConstants.EqualsExpression, debugOutputResults[1].Value);
            Assert.AreEqual(DebugItemResultType.Value, debugOutputResults[2].Type);
            Assert.AreEqual("This is output from the user\r\n", debugOutputResults[2].Value);            
        }

        // ReSharper disable InconsistentNaming

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfExecuteCommandLineActivity_UpdateForEachInputs")]
        public void DsfExecuteCommandLineActivity_UpdateForEachInputs_NullUpdates_DoesNothing()
        {
            //------------Setup for test--------------------------
            var command1 = "\"" + TestContext.DeploymentDirectory + "\\ConsoleAppToTestExecuteCommandLineActivity.exe\" output";
            var act = new DsfExecuteCommandLineActivity { CommandFileName = command1, CommandResult = "[[OutVar1]]" };

            //------------Execute Test---------------------------
            act.UpdateForEachInputs(null, null);
            //------------Assert Results-------------------------
            Assert.AreEqual(command1, act.CommandFileName);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfExecuteCommandLineActivity_UpdateForEachInputs")]
        public void DsfExecuteCommandLineActivity_UpdateForEachInputs_MoreThan1Updates_DoesNothing()
        {
            //------------Setup for test--------------------------
            var command1 = "\"" + TestContext.DeploymentDirectory + "\\ConsoleAppToTestExecuteCommandLineActivity.exe\" output";
            var act = new DsfExecuteCommandLineActivity { CommandFileName = command1, CommandResult = "[[OutVar1]]" };

            var tuple1 = new Tuple<string, string>("Test", "Test");
            var tuple2 = new Tuple<string, string>(command1, "Test2");
            //------------Execute Test---------------------------
            act.UpdateForEachInputs(new List<Tuple<string, string>> { tuple1, tuple2 }, null);
            //------------Assert Results-------------------------
            Assert.AreEqual("Test2", act.CommandFileName);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfExecuteCommandLineActivity_UpdateForEachInputs")]
        public void DsfExecuteCommandLineActivity_UpdateForEachInputs_UpdatesNotMatching_DoesNotUpdateRecordsetName()
        {
            //------------Setup for test--------------------------
            var command1 = "\"" + TestContext.DeploymentDirectory + "\\ConsoleAppToTestExecuteCommandLineActivity.exe\" output";
            var act = new DsfExecuteCommandLineActivity { CommandFileName = command1, CommandResult = "[[OutVar1]]" };

            var tuple1 = new Tuple<string, string>("Test", "Test");
            //------------Execute Test---------------------------
            act.UpdateForEachInputs(new List<Tuple<string, string>> { tuple1 }, null);
            //------------Assert Results-------------------------
            Assert.AreEqual(command1, act.CommandFileName);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfExecuteCommandLineActivity_UpdateForEachOutputs")]
        public void DsfExecuteCommandLineActivity_UpdateForEachOutputs_NullUpdates_DoesNothing()
        {
            //------------Setup for test--------------------------
            var command1 = "\"" + TestContext.DeploymentDirectory + "\\ConsoleAppToTestExecuteCommandLineActivity.exe\" output";
            const string result = "[[OutVar1]]";
            var act = new DsfExecuteCommandLineActivity { CommandFileName = command1, CommandResult = result };

            act.UpdateForEachOutputs(null, null);
            //------------Assert Results-------------------------
            Assert.AreEqual(result, act.CommandResult);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfExecuteCommandLineActivity_UpdateForEachOutputs")]
        public void DsfExecuteCommandLineActivity_UpdateForEachOutputs_MoreThan1Updates_DoesNothing()
        {
            //------------Setup for test--------------------------
            var command1 = "\"" + TestContext.DeploymentDirectory + "\\ConsoleAppToTestExecuteCommandLineActivity.exe\" output";
            const string result = "[[OutVar1]]";
            var act = new DsfExecuteCommandLineActivity { CommandFileName = command1, CommandResult = result };

            var tuple1 = new Tuple<string, string>("Test", "Test");
            var tuple2 = new Tuple<string, string>("Test2", "Test2");
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1, tuple2 }, null);
            //------------Assert Results-------------------------
            Assert.AreEqual(result, act.CommandResult);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfExecuteCommandLineActivity_UpdateForEachOutputs")]
        public void DsfExecuteCommandLineActivity_UpdateForEachOutputs_1Updates_UpdateCommandResult()
        {
            //------------Setup for test--------------------------
            var command1 = "\"" + TestContext.DeploymentDirectory + "\\ConsoleAppToTestExecuteCommandLineActivity.exe\" output";
            const string result = "[[OutVar1]]";
            var act = new DsfExecuteCommandLineActivity { CommandFileName = command1, CommandResult = result };

            var tuple1 = new Tuple<string, string>("Test", "Test");
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1 }, null);
            //------------Assert Results-------------------------
            Assert.AreEqual("Test", act.CommandResult);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfExecuteCommandLineActivity_GetForEachInputs")]
        public void DsfExecuteCommandLineActivity_GetForEachInputs_WhenHasExpression_ReturnsInputList()
        {
            //------------Setup for test--------------------------
            var command1 = "\"" + TestContext.DeploymentDirectory + "\\ConsoleAppToTestExecuteCommandLineActivity.exe\" output";
            var act = new DsfExecuteCommandLineActivity { CommandFileName = command1, CommandResult = "[[OutVar1]]" };

            //------------Execute Test---------------------------
            var dsfForEachItems = act.GetForEachInputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, dsfForEachItems.Count);
            Assert.AreEqual(command1, dsfForEachItems[0].Name);
            Assert.AreEqual(command1, dsfForEachItems[0].Value);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfExecuteCommandLineActivity_GetForEachOutputs")]
        public void DsfExecuteCommandLineActivity_GetForEachOutputs_WhenHasResult_ReturnsOutputList()
        {
            //------------Setup for test--------------------------
            var command1 = "\"" + TestContext.DeploymentDirectory + "\\ConsoleAppToTestExecuteCommandLineActivity.exe\" output";
            const string result = "[[OutVar1]]";
            DsfExecuteCommandLineActivity act = new DsfExecuteCommandLineActivity { CommandFileName = command1, CommandResult = result };

            //------------Execute Test---------------------------
            var dsfForEachItems = act.GetForEachOutputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, dsfForEachItems.Count);
            Assert.AreEqual(result, dsfForEachItems[0].Name);
            Assert.AreEqual(result, dsfForEachItems[0].Value);
        }

        void SetUpForExecution(DsfExecuteCommandLineActivity activity, string testData, string currentDl)
        {
            TestStartNode = new FlowStep
            {
                Action = activity
            };

            TestData = testData;
            CurrentDl = currentDl;
        }
    }

}