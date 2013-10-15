﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.DataList.Contract;

namespace Unlimited.UnitTest.Framework
{
    //<summary>
    //Summary description for Dev2DataLanguageParser
    //</summary>
    [TestClass]
    public class Dev2DataLanguageParserTest
    {
        // <summary>
        //Gets or sets the test context which provides
        //information about and functionality for the current test run.
        //</summary>
        public TestContext TestContext { get; set; }

        private IList<IIntellisenseResult> ParseDataLanguageForIntellisense(string transform, string dataList, bool IsFromIntellisense = false)
        {
            return DataListFactory.CreateLanguageParser().ParseDataLanguageForIntellisense(transform, dataList,false,null, IsFromIntellisense);
        }

        private IList<IIntellisenseResult> ParseDataLanguageForIntellisense(string transform, string dataList, bool addCompleteParts, bool IsFromIntellisense = false)
        {
            return DataListFactory.CreateLanguageParser().ParseDataLanguageForIntellisense(transform, dataList,addCompleteParts, null, IsFromIntellisense);
        }

        private IList<IIntellisenseResult> ParseDataLanguageForIntellisense(string transform, string dataList, bool addCompleteParts, IntellisenseFilterOpsTO filterOps, bool IsFromIntellisense = false)
        {
            return DataListFactory.CreateLanguageParser().ParseDataLanguageForIntellisense(transform, dataList, addCompleteParts, filterOps, IsFromIntellisense);
        }

        private IList<IIntellisenseResult> ParseForMissingDataListItems(IList<IDataListVerifyPart> parts, string dataList)
        {
            return DataListFactory.CreateLanguageParser().ParseForMissingDataListItems(parts, dataList);
        }

        #region Pos Test

        [TestMethod()]
        public void Range_Operation_ExpectedRangeOption_As_Valid_Index()
        {
            string dataList = @"<ADL><sum></sum></ADL>";
            string transform = "[[sum([[rs(1).f1:rs(5).f1]])]]";
            IList<IIntellisenseResult> result = ParseDataLanguageForIntellisense(transform, dataList, true,false);

            Assert.IsTrue(result.Count == 2 && result[0].ErrorCode == enIntellisenseErrorCode.None && result[0].Option.DisplayValue == "[[sum(rs(1).f1:rs(5).f1)]]");
        }


        [TestMethod]
        public void IntellisenseWithScalars_Expected_Recordset_With_ScalarOptions()
        {
            string dl = "<ADL> <rs><f1/></rs><myScalar/><myScalar2/> </ADL>";

            IList<IIntellisenseResult> result = ParseDataLanguageForIntellisense("[[rs(", dl, true,false);

            Assert.IsTrue(result.Count == 3 && result[0].Option.DisplayValue == "[[rs([[myScalar]])]]" && result[1].Option.DisplayValue == "[[rs([[myScalar2]])]]");
        }

        [TestMethod]
        public void FormView_Is_Part()
        {
            string dl = "<ADL> <FormView/> </ADL>";

            IList<IIntellisenseResult> result = ParseDataLanguageForIntellisense("[[FormView]]", dl, true,false);

            Assert.IsTrue(result.Count == 1);
        }

        [TestMethod]
        public void Return_Scalar_With_Open_Recordset()
        {
            string dl = "<ADL><cars><reg/><colour/><year/><regYear/></cars><pos/></ADL>";
            string payload = "[[cars([[";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);

            Assert.IsTrue(results.Count == 1 && results[0].Option.DisplayValue == "[[pos]]");
        }

        [TestMethod]
        public void Double_Open_Bracket_Returns_DataList()
        {
            string dl = "<ADL><cars><reg/><colour/><year/></cars><cool/></ADL>";
            string payload = "[[";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);

            Assert.IsTrue(results.Count == 5 && results[0].Option.DisplayValue == "[[cars()]]" && results[1].Option.DisplayValue == "[[cars().reg]]" && results[2].Option.DisplayValue == "[[cars().colour]]" && results[3].Option.DisplayValue == "[[cars().year]]" && results[4].Option.DisplayValue == "[[cool]]");
        }


        [TestMethod]
        public void Matches_One_Scalar_In_DL()
        {
            string dl = "<ADL><fName></fName><sName/></ADL>";
            string payload = "[[f";


            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);

            Assert.IsTrue(results.Count == 1 && results[0].Option.DisplayValue == "[[fName]]");
        }

        [TestMethod]
        public void Matches_Two_Scalars_In_DL()
        {
            string dl = "<ADL><fName></fName><foo></foo></ADL>";
            string payload = "[[f";


            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);

            Assert.IsTrue(results.Count == 2 && results[0].Option.DisplayValue == "[[fName]]" && results[1].Option.DisplayValue == "[[foo]]");
        }

        [TestMethod]
        public void Matches_One_Scalars_And_Recordset_Field_In_DL()
        {
            string dl = "<ADL><surname></surname><cars><topspeed></topspeed></cars></ADL>";
            string payload = "[[s";


            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);

            Assert.IsTrue(results.Count == 5 && results[0].Option.DisplayValue == "[[surname]]" && results[1].Option.DisplayValue == "[[cars(" && results[2].Option.DisplayValue == "[[cars(*)]]" && results[4].Option.DisplayValue == "[[cars().topspeed]]");
        }

        //19.09.2012: massimo.guerrera - Added for bug not showing the decription entered in the datalist.
        [TestMethod]
        public void Check_Scalars_And_Recordsets_Have_Descriptions()
        {
            string dl = @"<ADL><TestScalar Description=""this is a decription for TestScalar"" /><TestRecset Description=""this is a decription for TestRecset"">
  <TestField Description=""this is a decription for TestField"" />
</TestRecset></ADL>";
            string payload = "[[";

            IntellisenseFilterOpsTO filterTo = new IntellisenseFilterOpsTO();
            filterTo.FilterCondition = "";
            filterTo.FilterType = enIntellisensePartType.All;
            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl, false, filterTo);

            Assert.IsTrue(results.Count == 3 && results[0].Option.Description == "this is a decription for TestScalar / Select this variable" && results[1].Option.Description == "this is a decription for TestRecset / Select this record set" && results[2].Option.Description == "this is a decription for TestField / Select this record set field");
        }

        [TestMethod]
        public void Matches_Recordset_No_Close_With_Close()
        {
            string dl = "<ADL><cars><reg/><colour/><year/></cars><cool/></ADL>";
            string payload = "[[cars(";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);

            Assert.IsTrue(results.Count == 2);
        }

        [TestMethod]
        public void Matches_Recordset_Fields_With_Close_No_Number()
        {
            string dl = "<ADL><cars><reg/><colour/><year/></cars><cool/></ADL>";
            string payload = "[[cars()";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);

            Assert.IsTrue(results.Count == 4 && results[0].Option.DisplayValue == "[[cars()]]" && results[1].Option.DisplayValue == "[[cars().reg]]" && results[2].Option.DisplayValue == "[[cars().colour]]" && results[3].Option.DisplayValue == "[[cars().year]]");
        }

        [TestMethod]
        public void Matches_Recordset_Fields_With_Close_With_Number()
        {
            string dl = "<ADL><cars><reg/><colour/><year/></cars><cool/></ADL>";
            string payload = "[[cars(1)";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);

            Assert.IsTrue(results.Count == 4 && results[0].Option.DisplayValue == "[[cars(1)]]" && results[1].Option.DisplayValue == "[[cars(1).reg]]" && results[2].Option.DisplayValue == "[[cars(1).colour]]" && results[3].Option.DisplayValue == "[[cars(1).year]]");
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("Dev2DataLanguageParser_ParseDataLanguageForIntellisense")]
        public void Dev2DataLanguageParser_ParseDataLanguageForIntellisense_WhenFromIntellisenseTrue_ExpectOnlyMatchingRecordsetFields()
        {
            //------------Setup for test--------------------------
            string dl = "<ADL><cars><reg/><colour/><year/></cars><cool/></ADL>";
            string payload = "[[cars().re";

            //------------Execute Test---------------------------

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl, true, true);

            //------------Assert Results-------------------------

            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(results[0].Option.DisplayValue, "[[cars().reg]]");
            Assert.AreEqual(results[1].Option.DisplayValue, "[[cars(*).reg]]");

        }

        [TestMethod]
        public void Matches_Recordset_Fields_With_Close_R_In_Field()
        {
            string dl = "<ADL><cars><reg/><colour/><year/></cars><cool/></ADL>";
            string payload = "[[cars(1).r";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);

            Assert.IsTrue(results.Count == 3 && results[0].Option.DisplayValue == "[[cars(1).reg]]" && results[1].Option.DisplayValue == "[[cars(1).colour]]" && results[2].Option.DisplayValue == "[[cars(1).year]]");
        }

        [TestMethod]
        public void Matches_Recordset_Fields_With_Close_RE_In_Field()
        {
            string dl = "<ADL><cars><reg/><colour/><year/></cars><cool/></ADL>";
            string payload = "[[cars(1).re";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);

            Assert.IsTrue(results.Count == 1 && results[0].Option.DisplayValue == "[[cars(1).reg]]");
        }

        [TestMethod]
        public void Not_Found_Recordset_With_Closed_Variable()
        {
            string dl = "<ADL><cars><reg/><colour/><year/></cars><cool/></ADL>";
            string payload = "[[carz(1).rex]]";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);

            Assert.IsTrue(results.Count == 1 && results[0].Type == enIntellisenseResultType.Error && results[0].ErrorCode == enIntellisenseErrorCode.NeitherRecordsetNorFieldFound);
        }

        [TestMethod]
        public void Not_Found_Recordset_Field_With_Closed_Variable()
        {
            string dl = "<ADL><cars><reg/><colour/><year/></cars><cool/></ADL>";
            string payload = "[[cars(1).rex]]";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);

            Assert.IsTrue(results.Count == 1 && results[0].Type == enIntellisenseResultType.Error && results[0].ErrorCode == enIntellisenseErrorCode.FieldNotFound);
        }

        [TestMethod]
        public void Find_Recordset_Field_And_Scalar_With_Closed_Variable()
        {
            string dl = "<ADL><cars><reg/><colour/><year/></cars><cool/></ADL>";
            string payload = "[[cars(1).rex]][[def]]";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);

            Assert.IsTrue(results.Count == 2 && results[0].ErrorCode == enIntellisenseErrorCode.FieldNotFound && results[1].ErrorCode == enIntellisenseErrorCode.ScalarNotFound);
        }

        [TestMethod]
        public void Find_Recordset_Field_Embedded_For_Add()
        {
            string dl = "<ADL><cars><reg/><colour/><year/></cars><cool/></ADL>";
            string payload = "[[[[cars(1).rex]]]]";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);

            Assert.IsTrue(results.Count == 1 && results[0].ErrorCode == enIntellisenseErrorCode.FieldNotFound);
        }

        [TestMethod]
        public void Find_Recordset_Field_Embedded_For_Add_And_Find_Missing_Index()
        {
            string dl = "<ADL><cars><reg/><colour/><year/></cars><cool/></ADL>";
            string payload = "[[[[cars([[abc]]).rex]]]]";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);

            Assert.IsTrue(results.Count == 2 && results[0].ErrorCode == enIntellisenseErrorCode.FieldNotFound && results[1].ErrorCode == enIntellisenseErrorCode.ScalarNotFound);
        }

        [TestMethod]
        public void Find_Recordset_Field_And_Embedded_Scalar_Reference_For_Add()
        {
            string dl = "<ADL><cars><reg/><colour/><year/></cars><cool/></ADL>";
            string payload = "[[cars([[myPos]]).rex]]";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);

            Assert.IsTrue(results.Count == 2 && results[0].ErrorCode == enIntellisenseErrorCode.FieldNotFound && results[1].ErrorCode == enIntellisenseErrorCode.ScalarNotFound);
        }

        //

        [TestMethod]
        public void Find_Recordset_Index_Non_Closed_Recordset()
        {
            string dl = "<ADL><cars><reg/><colour/><year/></cars><cool/><myPos/><cCount/></ADL>";
            string payload = "[[cars([[cCount]])";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);

            Assert.IsTrue(results.Count == 4 && results[0].Option.DisplayValue == "[[cars([[cCount]])]]" && results[1].Option.DisplayValue == "[[cars([[cCount]]).reg]]" && results[2].Option.DisplayValue == "[[cars([[cCount]]).colour]]" && results[3].Option.DisplayValue == "[[cars([[cCount]]).year]]");
        }

        [TestMethod]
        public void Find_Recordset_Index_As_Scalar()
        {
            string dl = "<ADL><cars><reg/><colour/><year/></cars><cool/><myPos/><myCount/></ADL>";
            string payload = "[[cars([[my";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);

            Assert.IsTrue(results.Count == 2 && results[0].Option.DisplayValue == "[[myPos]]" && results[1].Option.DisplayValue == "[[myCount]]");
        }

        [TestMethod]
        public void Find_Recordset_And_Field_Closed_For_Return()
        {
            string dl = "<ADL><InjectedScript/><InjectedScript2><data/></InjectedScript2></ADL>";
            string payload = "[[InjectedScript2().data]]";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl, true,false);

            Assert.IsTrue(results.Count == 1 && results[0].Option.DisplayValue == "[[InjectedScript2().data]]" && results[0].ErrorCode == enIntellisenseErrorCode.None);

        }

        [TestMethod]
        public void Find_Scalar_Closed_For_Return()
        {
            string dl = "<ADL><InjectedScript/><InjectedScript2><data/></InjectedScript2></ADL>";
            string payload = "[[InjectedScript]]";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl, true,false);

            Assert.IsTrue(results.Count == 4);

        }

        [TestMethod]
        public void Find_Scalar_In_Recordset_Closed_For_Return()
        {
            string dl = "<ADL><InjectedScript/><InjectedScript2><data/></InjectedScript2><pos/></ADL>";
            string payload = "[[InjectedScript2([[pos]])]]";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl, true,false);

            Assert.IsTrue(results.Count == 2);

        }

        [TestMethod]
        public void Find_Recordset_In_Recordset_As_Index_Closed_For_Return()
        {
            string dl = "<ADL><InjectedScript><data/></InjectedScript><InjectedScript2><data/></InjectedScript2><pos/></ADL>";
            string payload = "[[InjectedScript2([[InjectedScript().data]]).data]]";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl,true,false);

            Assert.IsTrue(results.Count == 2 && results[0].Option.RecordsetIndex == "InjectedScript().data");

        }

        [TestMethod]
        public void Star_Index_Is_Valid_Index()
        {
            string dl = "<ADL><recset><f1/><f2/></recset></ADL>";
            string payload = "[[recset(*).f1]]";
            IList<IIntellisenseResult> result = ParseDataLanguageForIntellisense(payload, dl, true,false);

            Assert.IsTrue(result.Count == 1 && result[0].Option.Recordset == "recset" && result[0].Option.RecordsetIndex == "*" && result[0].Option.Field == "f1");

        }

        // Bug : 5793 - Travis.Frisinger : 19.10.2012
        [TestMethod]
        public void Recordset_With_DataList_Index_Returns_Correctly()
        {
            string dl = "<ADL><recset><f1/><f2/></recset><scalar/></ADL>";
            string payload = "[[recset([[s";
            IList<IIntellisenseResult> result = ParseDataLanguageForIntellisense(payload, dl, true,false);

            Assert.IsTrue(result.Count == 6 && result[0].Option.DisplayValue == "[[recset(" && result[1].Option.DisplayValue == "[[recset(*)]]" && result[5].Option.DisplayValue == "[[scalar]]");

        }

        // Bug : 5793 - Travis.Frisinger : 19.10.2012
        [TestMethod]
        public void Recordset_With_Open_DataList_Index_Returns_Correctly()
        {
            string dl = "<ADL><recset><f1/><f2/></recset><scalar/></ADL>";
            string payload = "[[recset([[scalar).f2]]";
            IList<IIntellisenseResult> result = ParseDataLanguageForIntellisense(payload, dl, true,false);

            Assert.IsTrue(result.Count == 4 && result[0].Option.DisplayValue == "[[recset([[scalar]])]]" && result[1].Option.DisplayValue == "[[recset([[scalar]]).f1]]" && result[2].Option.DisplayValue == "[[recset([[scalar]]).f2]]");

        }

        //2013.05.31: Ashley Lewis for bug 9472
        [TestMethod]
        public void RecordsetResultsExpectedReturnsCompleteRecordsetsOnlyResult()
        {
            string dl = "<ADL><recset><f1/><f2/></recset></ADL>";
            string payload = "[[rec";
            IList<IIntellisenseResult> result = ParseDataLanguageForIntellisense(payload, dl, true,false);

            Assert.IsNotNull(result.FirstOrDefault(intellisenseResults => intellisenseResults.Option.DisplayValue == "[[recset()]]"));
        }

        //2013.06.25: Ashley Lewis for bug 9801 - Variable named error shouldn't necessarily get error intellisense result at least not in the case described below
        [TestMethod]
        public void ParseWithVariableNamedErrorExpectedNoErrorResults()
        {
            string dl = "<ADL><Error/></ADL>";
            string payload = "[[Error]]";
            IList<IIntellisenseResult> result = ParseDataLanguageForIntellisense(payload, dl, true,null,false);

            Assert.AreEqual(1, result.Count, "Dev2DataLanguageParser returned an incorrect number of results");
            Assert.AreEqual("[[Error]]", result[0].Option.DisplayValue, "Dev2DataLanguageParser returned an incorrect result");
            Assert.AreEqual(enIntellisenseResultType.Selectable, result[0].Type, "Dev2DataLanguageParser returned an incorrect result type");
        }

        #endregion

        #region Negative Test
        [TestMethod]
        public void Fail_To_Find_Recordset_And_Field_With_Simular_Name()
        {
            string dl = "<ADL><InjectedScript/><InjectedScript2><data/></InjectedScript2></ADL>";
            string payload = "[[InjectedScript().data]]";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);

            Assert.IsTrue(results.Count == 1);
            Assert.IsTrue(results[0].Option.DisplayValue == "[[InjectedScript().data]]");
            Assert.IsTrue(results[0].ErrorCode == enIntellisenseErrorCode.NeitherRecordsetNorFieldFound);

        }

        [TestMethod]
        public void Throws_Exception_Invalid_Index_Not_Numeric_No_Close()
        {
            string dl = "<ADL><surname></surname><cars><topspeed></topspeed></cars></ADL>";
            string payload = "[[cars(a";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);
            Assert.IsTrue(results.Count == 1 && results[0].Type == enIntellisenseResultType.Error);
        }

        [TestMethod]
        public void Throws_Exception_Invalid_Index_Not_Numeric_With_Close()
        {
            string dl = "<ADL><surname></surname><cars><topspeed></topspeed></cars></ADL>";
            string payload = "[[cars(a)";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);
            Assert.IsTrue(results.Count == 1 && results[0].Type == enIntellisenseResultType.Error && results[0].ErrorCode == enIntellisenseErrorCode.NonNumericRecordsetIndex);
        }

        [TestMethod]
        public void Throws_Exception_Invalid_Index_Not_Greater_Zero_No_Close()
        {
            string dl = "<ADL><surname></surname><cars><topspeed></topspeed></cars></ADL>";
            string payload = "[[cars(-1";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);
            Assert.IsTrue(results.Count == 1 && results[0].Type == enIntellisenseResultType.Error);
        }

        [TestMethod]
        public void Throws_Exception_Invalid_Index_Not_Greater_Zero_With_Close()
        {
            string dl = "<ADL><surname></surname><cars><topspeed></topspeed></cars></ADL>";
            string payload = "[[cars(-1)";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);
            Assert.IsTrue(results.Count == 1 && results[0].Type == enIntellisenseResultType.Error);
        }

        [TestMethod]
        public void Throws_Exception_No_Match_For_Scalar()
        {
            string dl = "<ADL><surname></surname><cars><topspeed></topspeed></cars></ADL>";
            string payload = "[[abc";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);
            Assert.IsTrue(results.Count == 1 && results[0].Type == enIntellisenseResultType.Error && results[0].ErrorCode == enIntellisenseErrorCode.ScalarNotFound);
        }

        [TestMethod]
        public void Throws_Exception_No_Match_For_Recordset_No_Close()
        {
            string dl = "<ADL><surname></surname><cars><topspeed></topspeed></cars></ADL>";
            string payload = "[[abc(";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);
            Assert.IsTrue(results.Count == 1 && results[0].Type == enIntellisenseResultType.Error);
        }

        [TestMethod]
        public void Throws_Exception_No_Match_For_Recordset_With_Close()
        {
            string dl = "<ADL><surname></surname><cars><topspeed></topspeed></cars></ADL>";
            string payload = "[[abc()";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);
            Assert.IsTrue(results.Count == 1 && results[0].Type == enIntellisenseResultType.Error && results[0].ErrorCode == enIntellisenseErrorCode.RecordsetNotFound);
        }

        [TestMethod]
        public void Error_On_Non_Recordset_Notation_With_Valid_Recordset()
        {
            string dl = "<ADL><surname></surname><cars><topspeed></topspeed></cars></ADL>";
            string payload = "[[cars]]";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);
            Assert.IsTrue(results.Count == 1 && results[0].Type == enIntellisenseResultType.Error && results[0].ErrorCode == enIntellisenseErrorCode.InvalidRecordsetNotation);
        }

        [TestMethod]
        public void Error_On_Non_Recordset_Notation_With_Valid_Recordset_New_Open_Region()
        {
            string dl = "<ADL><surname></surname><cars><topspeed></topspeed></cars></ADL>";
            string payload = "[[cars]][[";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);
            Assert.IsTrue(results.Count == 4 && results[0].Type == enIntellisenseResultType.Selectable && results[0].Option.DisplayValue == "[[surname]]" && results[3].ErrorCode == enIntellisenseErrorCode.InvalidRecordsetNotation);
        }

        [TestMethod]
        public void Single_Open_On_Dual_Region_Does_Not_Cause_Results()
        {
            string dl = "<ADL><surname></surname><cars><topspeed></topspeed></cars></ADL>";
            string payload = "[[cars()]][";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);
            Assert.IsTrue(results.Count == 0);
        }

        //

        [TestMethod]
        public void Dual_Regions_With_RS_In_Second_Passes_Field_Validation()
        {
            string dl = "<ADL><cars><reg/><colour/></cars><recordCount/></ADL>";
            string payload = "[[cars()]][[cars().colour]]";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);
            Assert.IsTrue(results.Count == 0);
        }

        // Travis.Frisinger - 24.01.2013 : Bug 7856
        // Ashley Lewis - 06.03.2013 : Bug 6731
        [TestMethod]
        public void SpaceChars_In_DataList_Region_Expect_Error()
        {
            string dl = "<ADL><cars><reg/><colour/></cars><recordCount/></ADL>";
            string payload = "[[a b ]]";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);
            Assert.AreEqual(1, results.Count);
            Assert.IsTrue(results[0].Type == enIntellisenseResultType.Error);
            Assert.AreEqual(" [[a b ]] contains a space, this is an invalid character for a variable name", results[0].Message);
        }

        [TestMethod]
        public void MixedRegionsParseCorrectly()
        {
            string dl = "<ADL><a/><b/></ADL>";
            string payload = "[[a]] [[b]]";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl, true,false);
            Assert.AreEqual(2, results.Count, "Did not detect space between language pieces correctly");
            Assert.IsTrue(results[0].Type == enIntellisenseResultType.Selectable && results[0].Option.DisplayValue == "[[a]]");
            Assert.IsTrue(results[1].Type == enIntellisenseResultType.Selectable && results[1].Option.DisplayValue == "[[b]]");

        }


        [TestMethod]
        public void MixedRegionsParseCorrectlyWithLeadingSpace()
        {
            string dl = "<ADL><a/><b/></ADL>";
            string payload = "abc: [[a]]";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl, true,false);
            Assert.AreEqual(1, results.Count);
            Assert.IsTrue(results[0].Type == enIntellisenseResultType.Selectable && results[0].Option.DisplayValue == "[[a]]");

        }

        [TestMethod]
        public void CanDetectSpaceBetweenRecordsetAndFieldWhenBetweenDotAndField()
        {
            string dl = "<ADL><rec><val/></rec></ADL>";
            string payload = "abc: [[rec(). val]]";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl, true,false);
            Assert.AreEqual(1, results.Count);
            Assert.IsTrue(results[0].Type == enIntellisenseResultType.Error && results[0].Option.DisplayValue == "[[rec(). val]]", "Got [ " + results[0].Option.DisplayValue + " ]");

        }

        [TestMethod]
        public void CanDetectSpaceBetweenRecordsetAndFieldWhenBeforeDot()
        {
            string dl = "<ADL><rec><val/></rec></ADL>";
            string payload = "abc: [[rec() .val]]";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl, true,false);
            Assert.AreEqual(1, results.Count);
            Assert.IsTrue(results[0].Type == enIntellisenseResultType.Error && results[0].Option.DisplayValue == "[[rec() .val]]", "Got [ " + results[0].Option.DisplayValue + " ]");

        }


        #endregion

        #region FindMissing
        [TestMethod]
        public void Verify_Region_Find__Two_Missing_Scalars()
        {

            string DL = "<ADL><fname/><lname/></ADL>";

            IList<IDataListVerifyPart> parts = new List<IDataListVerifyPart>();

            parts.Add(IntellisenseFactory.CreateDataListValidationScalarPart("abc"));
            parts.Add(IntellisenseFactory.CreateDataListValidationScalarPart("def"));

            IList<IIntellisenseResult> result = ParseForMissingDataListItems(parts, DL);

            Assert.IsTrue(result.Count == 2 && result[0].ErrorCode == enIntellisenseErrorCode.ScalarNotFound && result[1].ErrorCode == enIntellisenseErrorCode.ScalarNotFound);
        }

        [TestMethod]
        public void Verify_Region_Find_Missing_Recordset()
        {

            string DL = "<ADL><fname/><lname/></ADL>";

            IList<IDataListVerifyPart> parts = new List<IDataListVerifyPart>();

            parts.Add(IntellisenseFactory.CreateDataListValidationRecordsetPart("cars", "abc"));

            IList<IIntellisenseResult> result = ParseForMissingDataListItems(parts, DL);

            Assert.IsTrue(result.Count == 1 && result[0].ErrorCode == enIntellisenseErrorCode.NeitherRecordsetNorFieldFound);
        }

        //2013.05.31: Ashley Lewis for bug 9472
        [TestMethod]
        public void RecordsetResultsWithNoRecordsetInDataListExpectedReturnsNoResults()
        {
            string dl = "<ADL></ADL>";
            string payload = "[[rec";
            IList<IIntellisenseResult> result = ParseDataLanguageForIntellisense(payload, dl, true,false);

            Assert.IsNull(result.FirstOrDefault(intellisenseResults => intellisenseResults.Option.DisplayValue == "[[recset()]]"));
        }

        /*
        [TestMethod]
        public void Verify_Region_Find_Missing_Recordset_Field()
        {

            string DL = "<ADL><fname/><lname/><cars><foo/></cars></ADL>";

            IList<IDataListVerifyPart> parts = new List<IDataListVerifyPart>();

            parts.Add(IntellisenseFactory.CreateDataListValidationRecordsetPart("cars", "bar"));

            IList<IIntellisenseResult> result = ParseForMissingDataListItems(parts, DL);

            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0].ErrorCode == enIntellisenseErrorCode.FieldNotFound);
        }*/

        #endregion

        #region ParseForActivityDataItemsTest

        /*
        [TestMethod]
        public void Find_Two_Parts_Recordset_As_Index_Of_Recordset()
        {
            string payload = "[[InjectedScript2([[InjectedScript().data]]).data]]";

            IList<string> results = ParseForActivityDataItems(payload);

            Assert.IsTrue(results.Count == 2);

        }

        [TestMethod]
        public void Find_Two_Parts_Scalar_As_Index_Of_Recordset()
        {
            string payload = "[[InjectedScript2([[pos]]).data]]";

            IList<string> results = ParseForActivityDataItems(payload);

            Assert.IsTrue(results.Count == 2);
        }

        [TestMethod]
        public void Find_Three_Parts_Recordset_As_Index_Of_Recordset()
        {
            string payload = "[[InjectedScript2([[InjectedScript([[abc]]).data]]).data]]";

            IList<string> results = ParseForActivityDataItems(payload);

            Assert.IsTrue(results.Count == 3);
        }
        */

        #endregion

        #region IntellisenseFactory Tests
        // ReSharper disable InconsistentNaming

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("IntellisenseFactory_CreateDataListValidationRecordsetPart")]
        public void IntellisenseFactory_CreateDataListValidationRecordsetPart_NullRecordSetWithFieldName_ShouldReturnFieldNameWrappedInBrackets()
        {
            //------------Setup for test--------------------------
            
            
            //------------Execute Test---------------------------
            var dataListValidationRecordsetPart = IntellisenseFactory.CreateDataListValidationRecordsetPart(null,"test","test","0");
            //------------Assert Results-------------------------
            Assert.AreEqual("[[test]]",dataListValidationRecordsetPart.DisplayValue);
        }       
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("IntellisenseFactory_CreateDataListValidationRecordsetPart")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void IntellisenseFactory_CreateDataListValidationRecordsetPart_NullRecordSetWithNullFieldName_ShouldThrowException()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            IntellisenseFactory.CreateDataListValidationRecordsetPart(null,null,"test","0");
            //------------Assert Results-------------------------
        }    
    

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("IntellisenseFactory_CreateDataListValidationRecordsetPart")]
        public void IntellisenseFactory_CreateDataListValidationRecordsetPart_RecordSetWithFieldName_ShouldReturnValidDisplayName()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dataListValidationRecordsetPart = IntellisenseFactory.CreateDataListValidationRecordsetPart("rec","f1","test","0");
            //------------Assert Results-------------------------
            Assert.AreEqual("[[rec(0).f1]]", dataListValidationRecordsetPart.DisplayValue);
        } 
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("IntellisenseFactory_CreateDataListValidationRecordsetPart")]
        public void IntellisenseFactory_CreateDataListValidationRecordsetPart_RecordSetWithFieldNameWithIndex_ShouldReturnValidDisplayName()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dataListValidationRecordsetPart = IntellisenseFactory.CreateDataListValidationRecordsetPart("rec","f1","test","3");
            //------------Assert Results-------------------------
            Assert.AreEqual("[[rec(3).f1]]", dataListValidationRecordsetPart.DisplayValue);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("IntellisenseFactory_CreateDataListValidationRecordsetPart")] // THIS DOES NOT LOOK CORRECT
        public void IntellisenseFactory_CreateDataListValidationRecordsetPart_RecordSetWithSquareAndRoundBracketsWithNoFieldName_ShouldReturnValidDisplayName()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dataListValidationRecordsetPart = IntellisenseFactory.CreateDataListValidationRecordsetPart("[[rec()]]", "", "test", "");
            //------------Assert Results-------------------------
            Assert.AreEqual("[[[[rec()]]", dataListValidationRecordsetPart.DisplayValue);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("IntellisenseFactory_CreateDataListValidationRecordsetPart")] // THIS DOES NOT LOOK CORRECT
        public void IntellisenseFactory_CreateDataListValidationRecordsetPart_RecordSetWithSquareAndRoundBracketsWithFieldName_ShouldReturnValidDisplayName()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dataListValidationRecordsetPart = IntellisenseFactory.CreateDataListValidationRecordsetPart("[[rec()]]", "f3", "test", "");
            //------------Assert Results-------------------------
            Assert.AreEqual("[[[[rec().f3]]", dataListValidationRecordsetPart.DisplayValue);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("IntellisenseFactory_CreateDataListValidationRecordsetPart")] // THIS DOES NOT LOOK CORRECT
        public void IntellisenseFactory_CreateDataListValidationRecordsetPart_RecordSetWithSquareAndRoundBracketsWithFieldNameWithIndex_ShouldReturnValidDisplayName()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dataListValidationRecordsetPart = IntellisenseFactory.CreateDataListValidationRecordsetPart("[[rec()]]", "f3", "test", "5");
            //------------Assert Results-------------------------
            Assert.AreEqual("[[[[rec(5).f3]]", dataListValidationRecordsetPart.DisplayValue);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("IntellisenseFactory_CreateDataListValidationRecordsetPart")]
        // IS THIS VALID!!!!
        public void IntellisenseFactory_CreateDataListValidationRecordsetPart_RecordSetWithRoundBracketsWithNoFieldNameWithIndex_ShouldReturnValidDisplayName()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dataListValidationRecordsetPart = IntellisenseFactory.CreateDataListValidationRecordsetPart("rec()", "", "test", "5");
            //------------Assert Results-------------------------
            Assert.AreEqual("[[rec(5)]]", dataListValidationRecordsetPart.DisplayValue);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("IntellisenseFactory_CreateDataListValidationRecordsetPart")]
        public void IntellisenseFactory_CreateDataListValidationRecordsetPart_RecordSetWithRoundBracketsWithNoFieldName_ShouldReturnValidDisplayName()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dataListValidationRecordsetPart = IntellisenseFactory.CreateDataListValidationRecordsetPart("rec()", "", "test", "");
            //------------Assert Results-------------------------
            Assert.AreEqual("[[rec()]]", dataListValidationRecordsetPart.DisplayValue);
        } 
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("IntellisenseFactory_CreateDataListValidationRecordsetPart")]
        public void IntellisenseFactory_CreateDataListValidationRecordsetPart_NoRecordSetWithRoundBracketsWithFieldNameWithStartRoundBracket_ShouldReturnValidDisplayName()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dataListValidationRecordsetPart = IntellisenseFactory.CreateDataListValidationRecordsetPart("", "f1(", "test", "");
            //------------Assert Results-------------------------
            Assert.AreEqual("[[f1(", dataListValidationRecordsetPart.DisplayValue);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("IntellisenseFactory_CreateDataListValidationRecordsetPart")]
        public void IntellisenseFactory_CreateDataListValidationRecordsetPart_RecordSetWithRoundNoBracketsWithNoFieldNameWithStartRoundBracket_ShouldReturnValidDisplayName()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dataListValidationRecordsetPart = IntellisenseFactory.CreateDataListValidationRecordsetPart("rec", "", "test", "");
            //------------Assert Results-------------------------
            Assert.AreEqual("[[rec()]]", dataListValidationRecordsetPart.DisplayValue);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("IntellisenseFactory_CreateDataListValidationRecordsetPart")]
        public void IntellisenseFactory_CreateDataListValidationRecordsetPart_RecordSetWithSquareBracketsNoRound_ShouldReturnValidDisplayName()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dataListValidationRecordsetPart = IntellisenseFactory.CreateDataListValidationRecordsetPart("[[rec]]", "", "test", "");
            //------------Assert Results-------------------------
            Assert.AreEqual("rec()", dataListValidationRecordsetPart.DisplayValue);
        }


        #endregion
    }
}
