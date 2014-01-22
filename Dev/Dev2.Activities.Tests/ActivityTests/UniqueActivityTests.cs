﻿using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ActivityUnitTests;
using Dev2.Activities;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Activities.ActivityTests
{
    /// <summary>
    /// Summary description for DataSplitActivityTest
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class UniqueActivityTests : BaseActivityUnitTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }


        [TestMethod]
        public void EmptyInFieldsStringExpectedNoUnique()
        {
            const string dataList = "<ADL><recset1>\r\n\t\t<field1/>\r\n\t</recset1>\r\n\t<recset2>\r\n\t\t<field2/>\r\n\t</recset2>\r\n\t<OutVar1/>\r\n\t<OutVar2/>\r\n\t<OutVar3/>\r\n\t<OutVar4/>\r\n\t<OutVar5/>\r\n</ADL>";
            SetupArguments("<root>" + dataList + "</root>", dataList, "", "[[recset1().field1]]", "[[OutVar1]]");
            IDSFDataObject result = ExecuteProcess();

            string actual;
            string error;
            GetScalarValueFromDataList(result.DataListID, "OutVar1", out actual, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(string.Empty, actual);
        }

        [TestMethod]
        public void RecordsetWithWithNoRecordsInRecSetExpectedUniqueAndAppendRecords()
        {
            const string dataList = "<ADL><recset1><field1/><field2/><field3/></recset1><recset2><id/><value/></recset2><OutVar1/></ADL>";
            const string dataListWithData = "<ADL>" +
                                            "<recset1>" +
                                            "<field1>1</field1><field2>a</field2><field3>Test1</field3>" +
                                            "</recset1>" +
                                            "<recset1>" +
                                            "<field1>2</field1><field2>b</field2><field3>Test2</field3>" +
                                            "</recset1>" +
                                            "<recset1>" +
                                            "<field1>3</field1><field2>a</field2><field3>Test3</field3>" +
                                            "</recset1>" +
                                            "<recset1>" +
                                            "<field1>4</field1><field2>a</field2><field3>Test4</field3>" +
                                            "</recset1>" +
                                            "<recset1>" +
                                            "<field1>5</field1><field2>c</field2><field3>Test5</field3>" +
                                            "</recset1>" +
                                            "<OutVar1/></ADL>";
            SetupArguments("<root>" + dataListWithData + "</root>"
                , dataList
                , "[[recset1().field2]]"
                , "[[recset1().field1]]", "[[recset2().id]]");
            List<string> expected = new List<string> { "1", "2", "5" };

            IDSFDataObject result = ExecuteProcess();

            IList<IBinaryDataListItem> actual;
            string error;

            GetRecordSetFieldValueFromDataList(result.DataListID, "recset2", "id", out actual, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            List<string> actualRet = new List<string>();
            actual.ToList().ForEach(d => actualRet.Add(d.TheValue));
            var comparer = new ActivityUnitTests.Utils.StringComparer();
            CollectionAssert.AreEqual(expected, actualRet, comparer);
        }

        [TestMethod]
        public void ScalarExpectedUniqueAsCsv()
        {
            const string dataList = "<ADL><recset1><field1/><field2/><field3/></recset1><recset2><id/><value/></recset2><OutVar1/></ADL>";
            const string dataListWithData = "<ADL>" +
                                            "<recset1>" +
                                            "<field1>1</field1><field2>a</field2><field3>Test1</field3>" +
                                            "</recset1>" +
                                            "<recset1>" +
                                            "<field1>2</field1><field2>b</field2><field3>Test2</field3>" +
                                            "</recset1>" +
                                            "<recset1>" +
                                            "<field1>3</field1><field2>a</field2><field3>Test3</field3>" +
                                            "</recset1>" +
                                            "<recset1>" +
                                            "<field1>4</field1><field2>a</field2><field3>Test4</field3>" +
                                            "</recset1>" +
                                            "<recset1>" +
                                            "<field1>5</field1><field2>c</field2><field3>Test5</field3>" +
                                            "</recset1>" +
                                            "<OutVar1/></ADL>";
            SetupArguments("<root>" + dataListWithData + "</root>"
                , dataList
                , "[[recset1().field2]]"
                , "[[recset1().field1]]", "[[OutVar1]]");
            const string expected = "1,2,5";

            IDSFDataObject result = ExecuteProcess();

            string actual;
            string error;

            GetScalarValueFromDataList(result.DataListID, "OutVar1", out actual, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, actual, "Got " + actual + " expected " + expected);
        }


        [TestMethod]
        public void RecordsetWithWithRecordsInRecSetExpectedUniqueAndAppendRecords()
        {
            const string dataList = "<ADL><recset1><field1/><field2/><field3/></recset1><recset2><id/><value/></recset2><OutVar1/></ADL>";
            const string dataListWithData = "<ADL>" +
                                            "<recset1>" +
                                            "<field1>1</field1><field2>a</field2><field3>Test1</field3>" +
                                            "</recset1>" +
                                            "<recset1>" +
                                            "<field1>2</field1><field2>b</field2><field3>Test2</field3>" +
                                            "</recset1>" +
                                            "<recset1>" +
                                            "<field1>3</field1><field2>a</field2><field3>Test3</field3>" +
                                            "</recset1>" +
                                            "<recset1>" +
                                            "<field1>4</field1><field2>a</field2><field3>Test4</field3>" +
                                            "</recset1>" +
                                            "<recset1>" +
                                            "<field1>5</field1><field2>c</field2><field3>Test5</field3>" +
                                            "</recset1>" +
                                            "<recset2>" +
                                            "<id>10</id><value>zz</value>" +
                                            "</recset2>" +
                                            "<OutVar1/></ADL>";
            SetupArguments("<root>" + dataListWithData + "</root>"
                , dataList
                , "[[recset1().field2]]"
                , "[[recset1().field1]]", "[[recset2().id]]");
            List<string> expected = new List<string> { "10", "1", "2", "5" };

            IDSFDataObject result = ExecuteProcess();

            IList<IBinaryDataListItem> actual;
            string error;

            GetRecordSetFieldValueFromDataList(result.DataListID, "recset2", "id", out actual, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            List<string> actualRet = new List<string>();
            actual.ToList().ForEach(d => actualRet.Add(d.TheValue));
            var comparer = new ActivityUnitTests.Utils.StringComparer();
            CollectionAssert.AreEqual(expected, actualRet, comparer);
        }

        [TestMethod]
        public void RecordsetWithWithMulitpleRecordsInRecSetExpectedUniqueAndAppendRecords()
        {
            const string dataList = "<ADL><recset1><field1/><field2/><field3/></recset1><recset2><id/><value/></recset2><OutVar1/></ADL>";
            const string dataListWithData = "<ADL>" +
                                            "<recset1>" +
                                            "<field1>1</field1><field2>a</field2><field3>Test1</field3>" +
                                            "</recset1>" +
                                            "<recset1>" +
                                            "<field1>2</field1><field2>b</field2><field3>Test2</field3>" +
                                            "</recset1>" +
                                            "<recset1>" +
                                            "<field1>3</field1><field2>a</field2><field3>Test3</field3>" +
                                            "</recset1>" +
                                            "<recset1>" +
                                            "<field1>4</field1><field2>a</field2><field3>Test4</field3>" +
                                            "</recset1>" +
                                            "<recset1>" +
                                            "<field1>5</field1><field2>c</field2><field3>Test5</field3>" +
                                            "</recset1>" +
                                            "<OutVar1/></ADL>";
            SetupArguments("<root>" + dataListWithData + "</root>"
                , dataList
                , "[[recset1().field2]]"
                , "[[recset1().field1]],[[recset1().field3]]", "[[recset2().id]],[[recset2().value]]");
            List<string> expectedID = new List<string> { "1", "2", "5" };
            List<string> expectedValue = new List<string> { "Test1", "Test2", "Test5" };

            IDSFDataObject result = ExecuteProcess();

            IList<IBinaryDataListItem> actual;
            string error;

            GetRecordSetFieldValueFromDataList(result.DataListID, "recset2", "id", out actual, out error);
            List<string> actualRet = new List<string>();
            actual.ToList().ForEach(d => actualRet.Add(d.TheValue));
            var comparer = new ActivityUnitTests.Utils.StringComparer();
            CollectionAssert.AreEqual(expectedID, actualRet, comparer);
            GetRecordSetFieldValueFromDataList(result.DataListID, "recset2", "value", out actual, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            actualRet = new List<string>();
            actual.ToList().ForEach(d => actualRet.Add(d.TheValue));
            comparer = new ActivityUnitTests.Utils.StringComparer();
            CollectionAssert.AreEqual(expectedValue, actualRet, comparer);
        }


        [TestMethod]
        [Owner("Travis")]
        [Description("Ensure we can use the star notation in the unique tool!")]
        [TestCategory("UniqueTool,UnitTest")]
        public void CanUniqueToolUseStarNotationInResultsFields()
        {
            const string dataList = "<ADL><recset1><field1/><field2/><field3/></recset1><recset2><id/><value/></recset2><OutVar1/></ADL>";
            const string dataListWithData = "<ADL>" +
                                            "<recset1>" +
                                            "<field1>1</field1><field2>a</field2><field3>Test1</field3>" +
                                            "</recset1>" +
                                            "<recset1>" +
                                            "<field1>2</field1><field2>b</field2><field3>Test2</field3>" +
                                            "</recset1>" +
                                            "<recset1>" +
                                            "<field1>3</field1><field2>a</field2><field3>Test3</field3>" +
                                            "</recset1>" +
                                            "<recset1>" +
                                            "<field1>4</field1><field2>a</field2><field3>Test4</field3>" +
                                            "</recset1>" +
                                            "<recset1>" +
                                            "<field1>5</field1><field2>c</field2><field3>Test5</field3>" +
                                            "</recset1>" +
                                            "<OutVar1/></ADL>";
            SetupArguments("<root>" + dataListWithData + "</root>"
                , dataList
                , "[[recset1().field2]]"
                , "[[recset1().field1]],[[recset1().field3]]", "[[recset2(*).id]],[[recset2(*).value]]");
            List<string> expectedID = new List<string> { "1", "2", "5" };
            List<string> expectedValue = new List<string> { "Test1", "Test2", "Test5" };

            IDSFDataObject result = ExecuteProcess();

            IList<IBinaryDataListItem> actual;
            string error;

            GetRecordSetFieldValueFromDataList(result.DataListID, "recset2", "id", out actual, out error);
            List<string> actualRet = new List<string>();
            actual.ToList().ForEach(d => actualRet.Add(d.TheValue));
            var comparer = new ActivityUnitTests.Utils.StringComparer();
            CollectionAssert.AreEqual(expectedID, actualRet, comparer);
            GetRecordSetFieldValueFromDataList(result.DataListID, "recset2", "value", out actual, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            actualRet = new List<string>();
            actual.ToList().ForEach(d => actualRet.Add(d.TheValue));
            comparer = new ActivityUnitTests.Utils.StringComparer();
            CollectionAssert.AreEqual(expectedValue, actualRet, comparer);
        }


        [TestMethod]
        [Owner("Travis")]
        [Description("Ensure we can use mixed star and append notation in the unique tool!")]
        [TestCategory("UniqueTool,UnitTest")]
        public void CanUniqueToolUseStarAndAppendNotationInResultsFields()
        {
            const string dataList = "<ADL><recset1><field1/><field2/><field3/></recset1><recset2><id/><value/></recset2><OutVar1/></ADL>";
            const string dataListWithData = "<ADL>" +
                                            "<recset1>" +
                                            "<field1>1</field1><field2>a</field2><field3>Test1</field3>" +
                                            "</recset1>" +
                                            "<recset1>" +
                                            "<field1>2</field1><field2>b</field2><field3>Test2</field3>" +
                                            "</recset1>" +
                                            "<recset1>" +
                                            "<field1>3</field1><field2>a</field2><field3>Test3</field3>" +
                                            "</recset1>" +
                                            "<recset1>" +
                                            "<field1>4</field1><field2>a</field2><field3>Test4</field3>" +
                                            "</recset1>" +
                                            "<recset1>" +
                                            "<field1>5</field1><field2>c</field2><field3>Test5</field3>" +
                                            "</recset1>" +
                                            "<recset2><id>99</id></recset2>" +
                                            "<OutVar1/></ADL>";
            SetupArguments("<root>" + dataListWithData + "</root>"
                , dataList
                , "[[recset1().field2]]"
                , "[[recset1().field1]],[[recset1().field3]]", "[[recset2().id]],[[recset2(*).value]]");
            List<string> expectedID = new List<string> { "99", "1", "2", "5" };
            List<string> expectedValue = new List<string> { "Test1", "Test2", "Test5", "" };

            IDSFDataObject result = ExecuteProcess();

            IList<IBinaryDataListItem> actual;
            string error;

            GetRecordSetFieldValueFromDataList(result.DataListID, "recset2", "id", out actual, out error);
            List<string> actualRet = new List<string>();
            actual.ToList().ForEach(d => actualRet.Add(d.TheValue));
            var comparer = new ActivityUnitTests.Utils.StringComparer();
            CollectionAssert.AreEqual(expectedID, actualRet, comparer);
            GetRecordSetFieldValueFromDataList(result.DataListID, "recset2", "value", out actual, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            actualRet = new List<string>();
            actual.ToList().ForEach(d => actualRet.Add(d.TheValue));
            comparer = new ActivityUnitTests.Utils.StringComparer();
            CollectionAssert.AreEqual(expectedValue, actualRet, comparer);
        }


        [TestMethod]
        public void UniqueGetDebugInputOutputWithScalarsExpectedPass()
        {
            DsfUniqueActivity act = new DsfUniqueActivity { InFields = "[[recset1().field2]]", ResultFields = "[[recset1().field1]]", Result = "[[OutVar1]]" };

            List<DebugItem> inRes;
            List<DebugItem> outRes;

            const string dataList = "<ADL><recset1><field1/><field2/><field3/></recset1><recset2><id/><value/></recset2><OutVar1/></ADL>";
            const string dataListWithData = "<ADL>" +
                                            "<recset1>" +
                                            "<field1>1</field1><field2>a</field2><field3>Test1</field3>" +
                                            "</recset1>" +
                                            "<recset1>" +
                                            "<field1>2</field1><field2>b</field2><field3>Test2</field3>" +
                                            "</recset1>" +
                                            "<recset1>" +
                                            "<field1>3</field1><field2>a</field2><field3>Test3</field3>" +
                                            "</recset1>" +
                                            "<recset1>" +
                                            "<field1>4</field1><field2>a</field2><field3>Test4</field3>" +
                                            "</recset1>" +
                                            "<recset1>" +
                                            "<field1>5</field1><field2>c</field2><field3>Test5</field3>" +
                                            "</recset1>" +
                                            "<OutVar1/></ADL>";

            var result = CheckActivityDebugInputOutput(act, dataList,
                 dataListWithData, out inRes, out outRes);


            // remove test datalist ;)
            DataListRemoval(result.DataListID);


            Assert.AreEqual(2, inRes.Count);
            IList<DebugItemResult> fetchResultsList = inRes[0].FetchResultsList();
            Assert.AreEqual(18, fetchResultsList.Count);
            Assert.AreEqual("1", fetchResultsList[0].Value);
            Assert.AreEqual(DebugItemResultType.Label, fetchResultsList[0].Type);
            Assert.AreEqual("In Fields", fetchResultsList[1].Value);
            Assert.AreEqual(DebugItemResultType.Label, fetchResultsList[1].Type);
            Assert.AreEqual("=", fetchResultsList[2].Value);
            Assert.AreEqual(DebugItemResultType.Label, fetchResultsList[2].Type);

            Assert.AreEqual("[[recset1(1).field2]]", fetchResultsList[3].Value);
            Assert.AreEqual(DebugItemResultType.Variable, fetchResultsList[3].Type);
            Assert.AreEqual("=", fetchResultsList[4].Value);
            Assert.AreEqual(DebugItemResultType.Label, fetchResultsList[4].Type);
            Assert.AreEqual("a", fetchResultsList[5].Value);
            Assert.AreEqual(DebugItemResultType.Value, fetchResultsList[5].Type);

            Assert.AreEqual("[[recset1(2).field2]]", fetchResultsList[6].Value);
            Assert.AreEqual(DebugItemResultType.Variable, fetchResultsList[6].Type);
            Assert.AreEqual("=", fetchResultsList[7].Value);
            Assert.AreEqual(DebugItemResultType.Label, fetchResultsList[7].Type);
            Assert.AreEqual("b", fetchResultsList[8].Value);
            Assert.AreEqual(DebugItemResultType.Value, fetchResultsList[8].Type);


            Assert.AreEqual("[[recset1(3).field2]]", fetchResultsList[9].Value);
            Assert.AreEqual(DebugItemResultType.Variable, fetchResultsList[9].Type);
            Assert.AreEqual("=", fetchResultsList[10].Value);
            Assert.AreEqual(DebugItemResultType.Label, fetchResultsList[10].Type);
            Assert.AreEqual("a", fetchResultsList[11].Value);
            Assert.AreEqual(DebugItemResultType.Value, fetchResultsList[11].Type);

            Assert.AreEqual("[[recset1(4).field2]]", fetchResultsList[12].Value);
            Assert.AreEqual(DebugItemResultType.Variable, fetchResultsList[12].Type);
            Assert.AreEqual("=", fetchResultsList[13].Value);
            Assert.AreEqual(DebugItemResultType.Label, fetchResultsList[13].Type);
            Assert.AreEqual("a", fetchResultsList[14].Value);
            Assert.AreEqual(DebugItemResultType.Value, fetchResultsList[14].Type);

            Assert.AreEqual("[[recset1(5).field2]]", fetchResultsList[15].Value);
            Assert.AreEqual(DebugItemResultType.Variable, fetchResultsList[15].Type);
            Assert.AreEqual("=", fetchResultsList[16].Value);
            Assert.AreEqual(DebugItemResultType.Label, fetchResultsList[16].Type);
            Assert.AreEqual("c", fetchResultsList[17].Value);
            Assert.AreEqual(DebugItemResultType.Value, fetchResultsList[17].Type);

            IList<DebugItemResult> debugItemResults = inRes[1].FetchResultsList();
            Assert.AreEqual(2, debugItemResults.Count);
            Assert.AreEqual("Return Fields", debugItemResults[0].Value);
            Assert.AreEqual(DebugItemResultType.Label, debugItemResults[0].Type);
            Assert.AreEqual("[[recset1().field1]]", debugItemResults[1].Value);
            Assert.AreEqual(DebugItemResultType.Value, debugItemResults[1].Type);
            Assert.AreEqual(1, outRes.Count);
            IList<DebugItemResult> debugOutput = outRes[0].FetchResultsList();
            Assert.AreEqual(4, debugOutput.Count);
            Assert.AreEqual("1", debugOutput[0].Value);
            Assert.AreEqual(DebugItemResultType.Label, debugOutput[0].Type);
            Assert.AreEqual("[[OutVar1]]", debugOutput[1].Value);
            Assert.AreEqual(DebugItemResultType.Variable, debugOutput[1].Type);
            Assert.AreEqual("=", debugOutput[2].Value);
            Assert.AreEqual(DebugItemResultType.Label, debugOutput[2].Type);
            Assert.AreEqual("1,2,5", debugOutput[3].Value);
            Assert.AreEqual(DebugItemResultType.Value, debugOutput[3].Type);
        }

        [TestMethod]
        public void UniqueGetDebugInputOutputWithRecordsetExpectedPass()
        {
            DsfUniqueActivity act = new DsfUniqueActivity { InFields = "[[recset1().field2]]", ResultFields = "[[recset1().field1]]", Result = "[[recset2().id]]" };

            List<DebugItem> inRes;
            List<DebugItem> outRes;

            const string dataList = "<ADL><recset1><field1/><field2/><field3/></recset1><recset2><id/><value/></recset2><OutVar1/></ADL>";
            const string dataListWithData = "<ADL>" +
                                            "<recset1>" +
                                            "<field1>1</field1><field2>a</field2><field3>Test1</field3>" +
                                            "</recset1>" +
                                            "<recset1>" +
                                            "<field1>2</field1><field2>b</field2><field3>Test2</field3>" +
                                            "</recset1>" +
                                            "<recset1>" +
                                            "<field1>3</field1><field2>a</field2><field3>Test3</field3>" +
                                            "</recset1>" +
                                            "<recset1>" +
                                            "<field1>4</field1><field2>a</field2><field3>Test4</field3>" +
                                            "</recset1>" +
                                            "<recset1>" +
                                            "<field1>5</field1><field2>c</field2><field3>Test5</field3>" +
                                            "</recset1>" +
                                            "<OutVar1/></ADL>";

            var result = CheckActivityDebugInputOutput(act, dataList,
                dataListWithData, out inRes, out outRes);
            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(2, inRes.Count);
            IList<DebugItemResult> fetchResultsList = inRes[0].FetchResultsList();
            Assert.AreEqual(18, fetchResultsList.Count);
            Assert.AreEqual("1", fetchResultsList[0].Value);
            Assert.AreEqual(DebugItemResultType.Label, fetchResultsList[0].Type);
            Assert.AreEqual("In Fields", fetchResultsList[1].Value);
            Assert.AreEqual(DebugItemResultType.Label, fetchResultsList[1].Type);
            Assert.AreEqual("=", fetchResultsList[2].Value);
            Assert.AreEqual(DebugItemResultType.Label, fetchResultsList[2].Type);

            Assert.AreEqual("[[recset1(1).field2]]", fetchResultsList[3].Value);
            Assert.AreEqual(DebugItemResultType.Variable, fetchResultsList[3].Type);
            Assert.AreEqual("=", fetchResultsList[4].Value);
            Assert.AreEqual(DebugItemResultType.Label, fetchResultsList[4].Type);
            Assert.AreEqual("a", fetchResultsList[5].Value);
            Assert.AreEqual(DebugItemResultType.Value, fetchResultsList[5].Type);

            Assert.AreEqual("[[recset1(2).field2]]", fetchResultsList[6].Value);
            Assert.AreEqual(DebugItemResultType.Variable, fetchResultsList[6].Type);
            Assert.AreEqual("=", fetchResultsList[7].Value);
            Assert.AreEqual(DebugItemResultType.Label, fetchResultsList[7].Type);
            Assert.AreEqual("b", fetchResultsList[8].Value);
            Assert.AreEqual(DebugItemResultType.Value, fetchResultsList[8].Type);


            Assert.AreEqual("[[recset1(3).field2]]", fetchResultsList[9].Value);
            Assert.AreEqual(DebugItemResultType.Variable, fetchResultsList[9].Type);
            Assert.AreEqual("=", fetchResultsList[10].Value);
            Assert.AreEqual(DebugItemResultType.Label, fetchResultsList[10].Type);
            Assert.AreEqual("a", fetchResultsList[11].Value);
            Assert.AreEqual(DebugItemResultType.Value, fetchResultsList[11].Type);

            Assert.AreEqual("[[recset1(4).field2]]", fetchResultsList[12].Value);
            Assert.AreEqual(DebugItemResultType.Variable, fetchResultsList[12].Type);
            Assert.AreEqual("=", fetchResultsList[13].Value);
            Assert.AreEqual(DebugItemResultType.Label, fetchResultsList[13].Type);
            Assert.AreEqual("a", fetchResultsList[14].Value);
            Assert.AreEqual(DebugItemResultType.Value, fetchResultsList[14].Type);

            Assert.AreEqual("[[recset1(5).field2]]", fetchResultsList[15].Value);
            Assert.AreEqual(DebugItemResultType.Variable, fetchResultsList[15].Type);
            Assert.AreEqual("=", fetchResultsList[16].Value);
            Assert.AreEqual(DebugItemResultType.Label, fetchResultsList[16].Type);
            Assert.AreEqual("c", fetchResultsList[17].Value);
            Assert.AreEqual(DebugItemResultType.Value, fetchResultsList[17].Type);

            IList<DebugItemResult> debugItemResults = inRes[1].FetchResultsList();
            Assert.AreEqual(2, debugItemResults.Count);
            Assert.AreEqual("Return Fields", debugItemResults[0].Value);
            Assert.AreEqual(DebugItemResultType.Label, debugItemResults[0].Type);
            Assert.AreEqual("[[recset1().field1]]", debugItemResults[1].Value);
            Assert.AreEqual(DebugItemResultType.Value, debugItemResults[1].Type);
            Assert.AreEqual(1, outRes.Count);
            IList<DebugItemResult> debugOutput = outRes[0].FetchResultsList();
            Assert.AreEqual(10, debugOutput.Count);

            Assert.AreEqual("1", debugOutput[0].Value);
            Assert.AreEqual(DebugItemResultType.Label, debugOutput[0].Type);
            Assert.AreEqual("[[recset2(1).id]]", debugOutput[1].Value);
            Assert.AreEqual(DebugItemResultType.Variable, debugOutput[1].Type);
            Assert.AreEqual("=", debugOutput[2].Value);
            Assert.AreEqual(DebugItemResultType.Label, debugOutput[2].Type);
            Assert.AreEqual("1", debugOutput[3].Value);
            Assert.AreEqual(DebugItemResultType.Value, debugOutput[3].Type);

            Assert.AreEqual("[[recset2(2).id]]", debugOutput[4].Value);
            Assert.AreEqual(DebugItemResultType.Variable, debugOutput[4].Type);
            Assert.AreEqual("=", debugOutput[5].Value);
            Assert.AreEqual(DebugItemResultType.Label, debugOutput[5].Type);
            Assert.AreEqual("2", debugOutput[6].Value);
            Assert.AreEqual(DebugItemResultType.Value, debugOutput[6].Type);

            Assert.AreEqual("[[recset2(3).id]]", debugOutput[7].Value);
            Assert.AreEqual(DebugItemResultType.Variable, debugOutput[7].Type);
            Assert.AreEqual("=", debugOutput[8].Value);
            Assert.AreEqual(DebugItemResultType.Label, debugOutput[8].Type);
            Assert.AreEqual("5", debugOutput[9].Value);
            Assert.AreEqual(DebugItemResultType.Value, debugOutput[9].Type);
        }

        // ReSharper disable InconsistentNaming

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfUniqueActivity_UpdateForEachInputs")]
        public void DsfUniqueActivity_UpdateForEachInputs_NullUpdates_DoesNothing()
        {
            //------------Setup for test--------------------------
            const string InFields = "[[Numeric(1).num]]";
            const string ResultFields = "Up";
            const string Result = "[[res]]";
            var act = new DsfUniqueActivity { InFields = InFields, ResultFields = ResultFields, Result = Result };

            //------------Execute Test---------------------------
            act.UpdateForEachInputs(null, null);
            //------------Assert Results-------------------------
            Assert.AreEqual(InFields, act.InFields);
            Assert.AreEqual(ResultFields, act.ResultFields);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfUniqueActivity_UpdateForEachInputs")]
        public void DsfUniqueActivity_UpdateForEachInputs_MoreThan1Updates_Updates()
        {
            //------------Setup for test--------------------------
            const string InFields = "[[Numeric(1).num]]";
            const string ResultFields = "Up";
            const string Result = "[[res]]";
            var act = new DsfUniqueActivity { InFields = InFields, ResultFields = ResultFields, Result = Result };

            var tuple1 = new Tuple<string, string>(ResultFields, "Test");
            var tuple2 = new Tuple<string, string>(InFields, "Test2");
            //------------Execute Test---------------------------
            act.UpdateForEachInputs(new List<Tuple<string, string>> { tuple1, tuple2 }, null);
            //------------Assert Results-------------------------
            Assert.AreEqual("Test2", act.InFields);
            Assert.AreEqual("Test", act.ResultFields);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfUniqueActivity_UpdateForEachOutputs")]
        public void DsfUniqueActivity_UpdateForEachOutputs_NullUpdates_DoesNothing()
        {
            //------------Setup for test--------------------------
            const string InFields = "[[Numeric(1).num]]";
            const string ResultFields = "Up";
            const string Result = "[[res]]";
            var act = new DsfUniqueActivity { InFields = InFields, ResultFields = ResultFields, Result = Result };

            act.UpdateForEachOutputs(null, null);
            //------------Assert Results-------------------------
            Assert.AreEqual(Result, act.Result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfUniqueActivity_UpdateForEachOutputs")]
        public void DsfUniqueActivity_UpdateForEachOutputs_MoreThan1Updates_DoesNothing()
        {
            //------------Setup for test--------------------------
            const string InFields = "[[Numeric(1).num]]";
            const string ResultFields = "Up";
            const string Result = "[[res]]";
            var act = new DsfUniqueActivity { InFields = InFields, ResultFields = ResultFields, Result = Result };

            var tuple1 = new Tuple<string, string>("Test", "Test");
            var tuple2 = new Tuple<string, string>("Test2", "Test2");
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1, tuple2 }, null);
            //------------Assert Results-------------------------
            Assert.AreEqual(Result, act.Result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfUniqueActivity_UpdateForEachOutputs")]
        public void DsfUniqueActivity_UpdateForEachOutputs_1Updates_UpdateCommandResult()
        {
            //------------Setup for test--------------------------
            const string InFields = "[[Numeric(1).num]]";
            const string ResultFields = "Up";
            const string Result = "[[res]]";
            var act = new DsfUniqueActivity { InFields = InFields, ResultFields = ResultFields, Result = Result };

            var tuple1 = new Tuple<string, string>("Test", "Test");
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1 }, null);
            //------------Assert Results-------------------------
            Assert.AreEqual("Test", act.Result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfUniqueActivity_GetForEachInputs")]
        public void DsfUniqueActivity_GetForEachInputs_WhenHasExpression_ReturnsInputList()
        {
            //------------Setup for test--------------------------
            const string InFields = "[[Numeric(1).num]]";
            const string ResultFields = "Up";
            const string Result = "[[res]]";
            var act = new DsfUniqueActivity { InFields = InFields, ResultFields = ResultFields, Result = Result };

            //------------Execute Test---------------------------
            var dsfForEachItems = act.GetForEachInputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(2, dsfForEachItems.Count);
            Assert.AreEqual(InFields, dsfForEachItems[0].Name);
            Assert.AreEqual(InFields, dsfForEachItems[0].Value);
            Assert.AreEqual(ResultFields, dsfForEachItems[1].Name);
            Assert.AreEqual(ResultFields, dsfForEachItems[1].Value);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfUniqueActivity_GetForEachOutputs")]
        public void DsfUniqueActivity_GetForEachOutputs_WhenHasResult_ReturnsOutputList()
        {
            //------------Setup for test--------------------------
            const string InFields = "[[Numeric(1).num]]";
            const string ResultFields = "Up";
            const string Result = "[[res]]";
            var act = new DsfUniqueActivity { InFields = InFields, ResultFields = ResultFields, Result = Result };

            //------------Execute Test---------------------------
            var dsfForEachItems = act.GetForEachOutputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, dsfForEachItems.Count);
            Assert.AreEqual(Result, dsfForEachItems[0].Name);
            Assert.AreEqual(Result, dsfForEachItems[0].Value);
        }


        #region Private Test Methods

        private void SetupArguments(string currentDL, string testData, string inFields, string resultFields, string result)
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfUniqueActivity { InFields = inFields, ResultFields = resultFields, Result = result }
            };

            CurrentDl = testData;
            TestData = currentDL;
        }

        #endregion Private Test Methods
    }
}