﻿using Dev2.Common;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.BinaryDataList.Converters
{
    /// <summary>
    /// Summary description for DataListXMLTranslatorTest
    /// </summary>
    [TestClass]
    public class DataListXMLTranslatorTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DataListXMLTranslator_ConvertAndOnlyMapInputs")]
        public void DataListXMLTranslatorWithOutSystemTags_ConvertAndOnlyMapInputs_WhenMappingScalars_ExpectInputsMapped()
        {
            //------------Setup for test--------------------------
            var compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;

            const string dataList = @"<DataList>
  <result Description="""" IsEditable=""True"" ColumnIODirection=""Output"" />
  <val1 Description="""" IsEditable=""True"" ColumnIODirection=""Input"" />
  <val2 Description="""" IsEditable=""True"" ColumnIODirection=""Input"" />
</DataList>";

            const string payload = "<DataList><val1>1</val1><val2>2</val2><result>5</result></DataList>";

            //------------Execute Test---------------------------
            var result = compiler.ConvertAndOnlyMapInputs(DataListFormat.CreateFormat(GlobalConstants._XML), payload, dataList, out errors);

            var dl = compiler.FetchBinaryDataList(result, out errors);

            var scalarEntries = dl.FetchScalarEntries();

            // Examine for correct data ;)

            var scalar = scalarEntries[0].FetchScalar();

            Assert.AreEqual(string.Empty, scalar.TheValue);
            Assert.AreEqual("result", scalar.FieldName);

            scalar = scalarEntries[1].FetchScalar();
            Assert.AreEqual("1", scalar.TheValue);
            Assert.AreEqual("val1", scalar.FieldName);

            scalar = scalarEntries[2].FetchScalar();
            Assert.AreEqual("2", scalar.TheValue);
            Assert.AreEqual("val2", scalar.FieldName);

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DataListXMLTranslator_ConvertAndOnlyMapInputs")]
        public void DataListXMLTranslatorWithOutSystemTags_ConvertAndOnlyMapInputs_WhenMappingRecordsets_ExpectInputsMapped()
        {
            //------------Setup for test--------------------------
            var compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;

            const string dataList = @"<DataList>
  <result Description="""" IsEditable=""True"" ColumnIODirection=""Output"" />
  <rs Description="""" IsEditable=""True"" ColumnIODirection=""Input"" >
        <val1 Description="""" IsEditable=""True"" ColumnIODirection=""Input"" />
        <val2 Description="""" IsEditable=""True"" ColumnIODirection=""Input"" />
  </rs>
</DataList>";

            const string payload = "<DataList><rs><val1>1</val1><val2>2</val2></rs><rs><val1>3</val1><val2>4</val2></rs></DataList>";

            //------------Execute Test---------------------------
            var result = compiler.ConvertAndOnlyMapInputs(DataListFormat.CreateFormat(GlobalConstants._XML), payload, dataList, out errors);

            var dl = compiler.FetchBinaryDataList(result, out errors);

            var rsEntries = dl.FetchRecordsetEntries();

            // Examine for correct data ;)

            string error;
            var rsEntry = rsEntries[0].FetchRecordAt(1, out error);

            // row 1 validation
            Assert.AreEqual(2, rsEntry.Count);
            Assert.AreEqual("val1", rsEntry[0].FieldName);
            Assert.AreEqual("1", rsEntry[0].TheValue);
            Assert.AreEqual("val2", rsEntry[1].FieldName);
            Assert.AreEqual("2", rsEntry[1].TheValue);

            // row 2 validation
            rsEntry = rsEntries[0].FetchRecordAt(2, out error);
            Assert.AreEqual(2, rsEntry.Count);
            Assert.AreEqual("val1", rsEntry[0].FieldName);
            Assert.AreEqual("3", rsEntry[0].TheValue);
            Assert.AreEqual("val2", rsEntry[1].FieldName);
            Assert.AreEqual("4", rsEntry[1].TheValue);

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DataListXMLTranslator_ConvertAndOnlyMapInputs")]
        public void DataListXMLTranslatorWithOutSystemTags_ConvertAndOnlyMapInputs_WhenMappingRecordsetsAndScalars_ExpectInputsMapped()
        {
            //------------Setup for test--------------------------
            var compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;

            const string dataList = @"<DataList>
  <result Description="""" IsEditable=""True"" ColumnIODirection=""Input"" />
  <rs Description="""" IsEditable=""True"" ColumnIODirection=""Input"" >
        <val1 Description="""" IsEditable=""True"" ColumnIODirection=""Input"" />
        <val2 Description="""" IsEditable=""True"" ColumnIODirection=""Input"" />
  </rs>
</DataList>";

            const string payload = "<DataList><rs><val1>1</val1><val2>2</val2></rs><rs><val1>3</val1><val2>4</val2></rs><result>99</result></DataList>";

            //------------Execute Test---------------------------
            var result = compiler.ConvertAndOnlyMapInputs(DataListFormat.CreateFormat(GlobalConstants._XML), payload, dataList, out errors);

            var dl = compiler.FetchBinaryDataList(result, out errors);

            var rsEntries = dl.FetchRecordsetEntries();

            // Examine for correct data ;)

            string error;
            var rsEntry = rsEntries[0].FetchRecordAt(1, out error);

            // row 1 validation
            Assert.AreEqual(2, rsEntry.Count);
            Assert.AreEqual("val1", rsEntry[0].FieldName);
            Assert.AreEqual("1", rsEntry[0].TheValue);
            Assert.AreEqual("val2", rsEntry[1].FieldName);
            Assert.AreEqual("2", rsEntry[1].TheValue);

            // row 2 validation
            rsEntry = rsEntries[0].FetchRecordAt(2, out error);
            Assert.AreEqual(2, rsEntry.Count);
            Assert.AreEqual("val1", rsEntry[0].FieldName);
            Assert.AreEqual("3", rsEntry[0].TheValue);
            Assert.AreEqual("val2", rsEntry[1].FieldName);
            Assert.AreEqual("4", rsEntry[1].TheValue);

            // check scalar value
            var scalarEntries = dl.FetchScalarEntries();
            var scalar = scalarEntries[0].FetchScalar();

            Assert.AreEqual("99", scalar.TheValue);
            Assert.AreEqual("result", scalar.FieldName);

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DataListXMLTranslator_ConvertAndOnlyMapInputs")]
        public void DataListXMLTranslatorWithOutSystemTags_ConvertAndOnlyMapInputs_WhenMappingRecordsetsAndScalarsMarkedAsBoth_ExpectInputsMapped()
        {
            //------------Setup for test--------------------------
            var compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;

            const string dataList = @"<DataList>
  <result Description="""" IsEditable=""True"" ColumnIODirection=""Both"" />
  <rs Description="""" IsEditable=""True"" ColumnIODirection=""Both"" >
        <val1 Description="""" IsEditable=""True"" ColumnIODirection=""Both"" />
        <val2 Description="""" IsEditable=""True"" ColumnIODirection=""Both"" />
  </rs>
</DataList>";

            const string payload = "<DataList><rs><val1>1</val1><val2>2</val2></rs><rs><val1>3</val1><val2>4</val2></rs><result>99</result></DataList>";

            //------------Execute Test---------------------------
            var result = compiler.ConvertAndOnlyMapInputs(DataListFormat.CreateFormat(GlobalConstants._XML), payload, dataList, out errors);

            var dl = compiler.FetchBinaryDataList(result, out errors);

            var rsEntries = dl.FetchRecordsetEntries();

            // Examine for correct data ;)

            string error;
            var rsEntry = rsEntries[0].FetchRecordAt(1, out error);

            // row 1 validation
            Assert.AreEqual(2, rsEntry.Count);
            Assert.AreEqual("val1", rsEntry[0].FieldName);
            Assert.AreEqual("1", rsEntry[0].TheValue);
            Assert.AreEqual("val2", rsEntry[1].FieldName);
            Assert.AreEqual("2", rsEntry[1].TheValue);

            // row 2 validation
            rsEntry = rsEntries[0].FetchRecordAt(2, out error);
            Assert.AreEqual(2, rsEntry.Count);
            Assert.AreEqual("val1", rsEntry[0].FieldName);
            Assert.AreEqual("3", rsEntry[0].TheValue);
            Assert.AreEqual("val2", rsEntry[1].FieldName);
            Assert.AreEqual("4", rsEntry[1].TheValue);

            // check scalar value
            var scalarEntries = dl.FetchScalarEntries();
            var scalar = scalarEntries[0].FetchScalar();

            Assert.AreEqual("99", scalar.TheValue);
            Assert.AreEqual("result", scalar.FieldName);

        }


        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DataListXMLTranslator_ConvertAndOnlyMapInputs")]
        public void DataListXMLTranslatorWithOutSystemTags_ConvertAndOnlyMapInputs_WhenMappingRecordsetsWithSelectColumnsMapped_ExpectInputsMapped()
        {
            //------------Setup for test--------------------------
            var compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;

            const string dataList = @"<DataList>
  <result Description="""" IsEditable=""True"" ColumnIODirection=""Input"" />
  <rs Description="""" IsEditable=""True"" ColumnIODirection=""Input"" >
        <val1 Description="""" IsEditable=""True"" ColumnIODirection=""Input"" />
        <val2 Description="""" IsEditable=""True"" ColumnIODirection=""Output"" />
  </rs>
</DataList>";

            const string payload = "<DataList><rs><val1>1</val1><val2>2</val2></rs><rs><val1>3</val1><val2>4</val2></rs></DataList>";

            //------------Execute Test---------------------------
            var result = compiler.ConvertAndOnlyMapInputs(DataListFormat.CreateFormat(GlobalConstants._XML), payload, dataList, out errors);

            var dl = compiler.FetchBinaryDataList(result, out errors);

            var rsEntries = dl.FetchRecordsetEntries();

            // Examine for correct data ;)

            string error;
            var rsEntry = rsEntries[0].FetchRecordAt(1, out error);

            // row 1 validation
            Assert.AreEqual(2, rsEntry.Count);
            Assert.AreEqual("val1", rsEntry[0].FieldName);
            Assert.AreEqual("1", rsEntry[0].TheValue);
            Assert.AreEqual("val2", rsEntry[1].FieldName);
            Assert.AreEqual(string.Empty, rsEntry[1].TheValue);

            // row 2 validation
            rsEntry = rsEntries[0].FetchRecordAt(2, out error);
            Assert.AreEqual(2, rsEntry.Count);
            Assert.AreEqual("val1", rsEntry[0].FieldName);
            Assert.AreEqual("3", rsEntry[0].TheValue);
            Assert.AreEqual("val2", rsEntry[1].FieldName);
            Assert.AreEqual(string.Empty, rsEntry[1].TheValue);

        }
    }
}
