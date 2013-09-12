﻿using System;
using System.Threading;
using Dev2;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using Dev2.Tests.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

// ReSharper disable CheckNamespace
namespace ActivityUnitTests.ActivityTests
// ReSharper restore CheckNamespace
{
    /// <summary>
    /// Summary description for BaseConvertActivityTests
    /// </summary>
    [TestClass]
    public class BaseConvertActivityTests : BaseActivityUnitTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Base Convert Cases

        [TestMethod]
        public void BaseConvert_ScalarToBase64_Expected_stringToBase64()
        {
            IList<BaseConvertTO> convertCollection = new List<BaseConvertTO> { new BaseConvertTO("[[testVar]]", "Text", "Base 64", "[[testVar]]", 1) };
            SetupArguments(@"<root><testVar>change this to base64</testVar></root>"
                          , ActivityStrings.CaseConvert_DLShape
                          , convertCollection
                          );
            IDSFDataObject result = ExecuteProcess();

            const string expected = @"Y2hhbmdlIHRoaXMgdG8gYmFzZTY0";
            string actual;
            string error;
            GetScalarValueFromDataList(result.DataListID, "testVar", out actual, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void BaseConvert_RecsetWithIndexToHex_Expected_stringToHex()
        {
            IList<BaseConvertTO> convertCollection = new List<BaseConvertTO> { new BaseConvertTO("[[Recset(1).Field]]", "Text", "Hex", "[[Recset(1).Field]]", 1) };
            SetupArguments(
                            @"<root><Recset><Field>CHANGE THIS TO HEX</Field></Recset></root>"
                          , ActivityStrings.BaseConvert_DLShape
                          , convertCollection
                          );
            IDSFDataObject result = ExecuteProcess();

            const string expected = @"0x4348414e4745205448495320544f20484558";
            string error;
            string actual = RetrieveAllRecordSetFieldValues(result.DataListID, "Recset", "Field", out error).First();

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void BaseConvert_RecsetWithNoIndexToBinary_Expected_stringToBinary()
        {
            IList<BaseConvertTO> convertCollection = new List<BaseConvertTO> { new BaseConvertTO("[[Recset().Field]]", "Text", "Binary", "[[Recset().Field]]", 1) };
            SetupArguments(
                            @"<root><Recset><Field>CHANGE THIS TO BINARY</Field></Recset></root>"
                          , ActivityStrings.BaseConvert_DLShape
                          , convertCollection
                          );
            IDSFDataObject result = ExecuteProcess();

            const string expected = @"010000110100100001000001010011100100011101000101001000000101010001001000010010010101001100100000010101000100111100100000010000100100100101001110010000010101001001011001";
            string error;
            string actual = RetrieveAllRecordSetFieldValues(result.DataListID, "Recset", "Field", out error)[1];

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, actual);
        }

        //2013.02.13: Ashley Lewis - Bug 8725, Task 8836 DONE
        [TestMethod]
        public void BaseConvertScalarNumberToBase64ExpectedStringToBase64()
        {
            IList<BaseConvertTO> convertCollection = new List<BaseConvertTO> { new BaseConvertTO("[[testVar]]", "Text", "Base 64", "[[testVar]]", 1) };
            SetupArguments(@"<root><testVar>1</testVar></root>"
                          , ActivityStrings.CaseConvert_DLShape
                          , convertCollection
                          );
            IDSFDataObject result = ExecuteProcess();

            const string expected = @"MQ==";
            string actual;
            string error;
            GetScalarValueFromDataList(result.DataListID, "testVar", out actual, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, actual);
        }
        [TestMethod]
        public void BaseConvertRecsetWithStarIndexToBinaryExpectedOutputToCorrectRecords()
        {
            SetupArguments(
                            @"<root></root>"
                          , ActivityStrings.BaseConvert_DLShape.Replace("<ADL>", "<ADL><setup/>")
                          , new List<BaseConvertTO> { new BaseConvertTO("", "Text", "Binary", "[[setup]]", 1) }
                          );
            IDSFDataObject result = ExecuteProcess();
            ErrorResultTO errorResult;
            IBinaryDataList bdl = Compiler.FetchBinaryDataList(result.DataListID, out errorResult);

            IBinaryDataListItem isolatedRecord = Dev2BinaryDataListFactory.CreateBinaryItem("CONVERT THIS TO BINARY", "Field");
            string error;
            IBinaryDataListEntry entry;
            bdl.TryGetEntry("Recset", out entry, out error);
            entry.TryPutRecordItemAtIndex(isolatedRecord, 5, out error);

            IList<BaseConvertTO> convertCollection = new List<BaseConvertTO> { new BaseConvertTO("[[Recset(*).Field]]", "Text", "Binary", "[[Recset(*).Field]]", 1) };
            TestStartNode = new FlowStep
            {
                Action = new DsfBaseConvertActivity { ConvertCollection = convertCollection }
            };
            result = ExecuteProcess();

            IList<IBinaryDataListEntry> actual = bdl.FetchRecordsetEntries();
            var index = actual[0].FetchRecordAt(5, out error)[0].ItemCollectionIndex;
            var count = actual.Count();
            var actualValue = actual[0].FetchRecordAt(5, out error)[0].TheValue;

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual("01000011010011110100111001010110010001010101001001010100001000000101010001001000010010010101001100100000010101000100111100100000010000100100100101001110010000010101001001011001", actualValue);
            Assert.AreEqual(1, count); // still only one record
            Assert.AreEqual(5, index); // and that record has not moved
        }

        #endregion Base Convert Cases

        #region Language Tests

        [TestMethod]
        public void BaseConvert_RecsetWithStarToBinary_Expected_stringToBinary()
        {
            IList<BaseConvertTO> convertCollection = new List<BaseConvertTO> { new BaseConvertTO("[[Recset(*).Field]]", "Text", "Binary", "[[Recset(*).Field]]", 1) };

            SetupArguments(
                @"<root><Recset><Field>CHANGE THIS TO BINARY</Field></Recset><Recset><Field>New Text to change</Field></Recset><Recset><Field>Other new text to change</Field></Recset></root>"
              , ActivityStrings.BaseConvert_DLShape
              , convertCollection
              );
            IDSFDataObject result = ExecuteProcess();

            List<string> expected = new List<string> 
                { "010000110100100001000001010011100100011101000101001000000101010001001000010010010101001100100000010101000100111100100000010000100100100101001110010000010101001001011001"
                , "010011100110010101110111001000000101010001100101011110000111010000100000011101000110111100100000011000110110100001100001011011100110011101100101"
                , "010011110111010001101000011001010111001000100000011011100110010101110111001000000111010001100101011110000111010000100000011101000110111100100000011000110110100001100001011011100110011101100101"
                };
            string error;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "Recset", "Field", out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            CollectionAssert.AreEqual(expected, actual, new Utils.StringComparer());

        }

        // Travis.Frisinger - 24.01.2013 - Bug 7916
        [TestMethod]
        public void Sclar_To_Base64_Back_To_Text_Expect_Original()
        {

            IList<BaseConvertTO> convertCollection = new List<BaseConvertTO> { new BaseConvertTO("[[test]]", "Text", "Base64", "[[test]]", 1), new BaseConvertTO("[[test]]", "Base64", "Text", "[[test]]", 2) };
            SetupArguments(
                @"<root><test>data</test></root>"
              , @"<root><test/></root>"
              , convertCollection
              );
            IDSFDataObject result = ExecuteProcess();

            string error;
            string actual;
            GetScalarValueFromDataList(result.DataListID, "test", out actual, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual("data", actual, "Got " + actual);
        }

        #endregion Language Tests

        #region Negative Tests
        [TestMethod]
        public void BaseConvert_Convert_Binary_To_Text_With_Base64_Value_Expected_ErrorTag()
        {
            IList<BaseConvertTO> convertCollection = new List<BaseConvertTO> { new BaseConvertTO("[[testVar]]", "Binary", "Text", "[[testVar]]", 1) };

            SetupArguments(
                            @"<root><testVar>SGkgdGhpcyBpcyB0ZXh0 </testVar></root>"
                          , ActivityStrings.CaseConvert_DLShape
                          , convertCollection
                          );

            IDSFDataObject result = ExecuteProcess();
            string actual;
            string error;
            GetScalarValueFromDataList(result.DataListID, "Dev2System.Dev2Error", out actual, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(@"<InnerError>Base Conversion Broker was expecting [ Binary ] but the data was not in this format</InnerError>", actual);
        }

        #endregion Negative Tests

        #region Get Debug Input/Output Tests

        /// <summary>
        /// Author : Massimo Guerrera Bug 8104 
        /// </summary>
        [TestMethod]
        public void BaseConvert_Get_Debug_Input_Output_With_Scalars_Expected_Pass()
        {
            IList<BaseConvertTO> convertCollection = new List<BaseConvertTO> { new BaseConvertTO("[[CompanyName]]", "Text", "Binary", "[[CompanyName]]", 1) };
            DsfBaseConvertActivity act = new DsfBaseConvertActivity { ConvertCollection = convertCollection };

            List<DebugItem> inRes;
            List<DebugItem> outRes;

            var result = CheckActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(1, inRes.Count);
            Assert.AreEqual(9, inRes[0].ResultsList.Count);
            Assert.AreEqual("1", inRes[0].ResultsList[0].Value);
            Assert.AreEqual("Convert", inRes[0].ResultsList[1].Value);
            Assert.AreEqual("[[CompanyName]]", inRes[0].ResultsList[2].Value);
            Assert.AreEqual("=", inRes[0].ResultsList[3].Value);
            Assert.AreEqual("Dev2", inRes[0].ResultsList[4].Value);
            Assert.AreEqual("From", inRes[0].ResultsList[5].Value);
            Assert.AreEqual("Text", inRes[0].ResultsList[6].Value);
            Assert.AreEqual("To", inRes[0].ResultsList[7].Value);
            Assert.AreEqual("Binary", inRes[0].ResultsList[8].Value);            
            Assert.AreEqual(1, outRes.Count);
            Assert.AreEqual(4, outRes[0].ResultsList.Count);
            Assert.AreEqual("1", outRes[0].ResultsList[0].Value);
            Assert.AreEqual("[[CompanyName]]", outRes[0].ResultsList[1].Value);
            Assert.AreEqual("=", outRes[0].ResultsList[2].Value);
            Assert.AreEqual("01000100011001010111011000110010", outRes[0].ResultsList[3].Value);            
        }

        /// <summary>
        /// Author : Massimo Guerrera Bug 8104 
        /// </summary>
        [TestMethod]
        public void BaseConvert_Get_Debug_Input_Output_With_Recordsets_Expected_Pass()
        {
            IList<BaseConvertTO> convertCollection = new List<BaseConvertTO> { new BaseConvertTO("[[Customers(*).FirstName]]", "Text", "Binary", "[[Customers(*).FirstName]]", 1) };
            DsfBaseConvertActivity act = new DsfBaseConvertActivity { ConvertCollection = convertCollection };

            List<DebugItem> inRes;
            List<DebugItem> outRes;

            var result = CheckActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(1, inRes.Count);
            Assert.AreEqual(36, inRes[0].ResultsList.Count);
            Assert.AreEqual("1", inRes[0].ResultsList[0].Value);
            Assert.AreEqual("Convert", inRes[0].ResultsList[1].Value);
            Assert.AreEqual("[[Customers(1).FirstName]]", inRes[0].ResultsList[2].Value);
            Assert.AreEqual("=", inRes[0].ResultsList[3].Value);
            Assert.AreEqual("Wallis", inRes[0].ResultsList[4].Value);
            Assert.AreEqual("[[Customers(2).FirstName]]", inRes[0].ResultsList[5].Value);
            Assert.AreEqual("=", inRes[0].ResultsList[6].Value);
            Assert.AreEqual("Barney", inRes[0].ResultsList[7].Value);
            Assert.AreEqual("[[Customers(3).FirstName]]", inRes[0].ResultsList[8].Value);
            Assert.AreEqual("=", inRes[0].ResultsList[9].Value);
            Assert.AreEqual("Trevor", inRes[0].ResultsList[10].Value);
            Assert.AreEqual("[[Customers(4).FirstName]]", inRes[0].ResultsList[11].Value);
            Assert.AreEqual("=", inRes[0].ResultsList[12].Value);
            Assert.AreEqual("Travis", inRes[0].ResultsList[13].Value);            
            Assert.AreEqual(10, outRes.Count);
            Assert.AreEqual(4, outRes[0].ResultsList.Count);
            Assert.AreEqual("1", outRes[0].ResultsList[0].Value);
            Assert.AreEqual("[[Customers(1).FirstName]]", outRes[0].ResultsList[1].Value);
            Assert.AreEqual("=", outRes[0].ResultsList[2].Value);
            Assert.AreEqual("010101110110000101101100011011000110100101110011", outRes[0].ResultsList[3].Value);            
        }

        #endregion

        #region GetWizardData Tests

        [TestMethod]
        public void GetWizardData_Expected_Correct_IBinaryDataList()
        {
            bool passTest = true;
            IList<BaseConvertTO> convertCollection = new List<BaseConvertTO> { new BaseConvertTO("[[testVar]]", "Text", "Base 64", "[[testVar]]", 1), new BaseConvertTO("[[testVar1]]", "Text", "Base 64", "[[testVar]]", 2) };

            DsfBaseConvertActivity testAct = new DsfBaseConvertActivity { ConvertCollection = convertCollection };

            IBinaryDataList binaryDL = testAct.GetWizardData();
            var recsets = binaryDL.FetchRecordsetEntries();
            if (recsets.Count != 1)
            {
                passTest = false;
            }
            else
            {
                if (recsets[0].Columns.Count != 4)
                {
                    passTest = false;
                }
            }

            // remove test datalist ;)
            DataListRemoval(binaryDL.UID);

            Assert.IsTrue(passTest);
        }

        #endregion GetWizardData Tests


        #region ForEach Update/Get Inputs/Outputs

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DsfBaseConvert_UpdateForEachInputs")]
        public void DsfBaseConvert_UpdateForEachInputs_WhenContainsMatchingStarAndOtherData_UpdateSuccessful()
        {
            //------------Setup for test--------------------------
            List<BaseConvertTO> fieldsCollection = new List<BaseConvertTO>
	        {
		        new BaseConvertTO( "[[rs(*).val]] [[result]]","text", "hex", "[[result]]",1)
	        };


            var dsfBaseConvert = new DsfBaseConvertActivity() { ConvertCollection = fieldsCollection };

            //------------Execute Test---------------------------
            dsfBaseConvert.UpdateForEachInputs(new List<Tuple<string, string>>()
	        {
		        new Tuple<string, string>("[[rs(*).val]]", "[[rs(1).val]]"),
	        }, null);

            //------------Assert Results-------------------------

            var collection = dsfBaseConvert.ConvertCollection;

            Assert.AreEqual("[[rs(1).val]] [[result]]", collection[0].FromExpression);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DsfBaseConvert_UpdateForEachInputs")]
        public void DsfBaseConvert_UpdateForEachInputs_WhenContainsMatchingStar_UpdateSuccessful()
        {
            //------------Setup for test--------------------------
            List<BaseConvertTO> fieldsCollection = new List<BaseConvertTO>
	        {
		        new BaseConvertTO( "[[rs(*).val]]","text", "hex", "[[result]]",1)
	        };


            var dsfBaseConvert = new DsfBaseConvertActivity() { ConvertCollection = fieldsCollection };

            //------------Execute Test---------------------------
            dsfBaseConvert.UpdateForEachInputs(new List<Tuple<string, string>>()
	        {
		        new Tuple<string, string>("[[rs(*).val]]", "[[rs(1).val]]"),
	        }, null);

            //------------Assert Results-------------------------

            var collection = dsfBaseConvert.ConvertCollection;

            Assert.AreEqual("[[rs(1).val]]", collection[0].FromExpression);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DsfBaseConvert_UpdateForEachOutputs")]
        public void DsfBaseConvert_UpdateForEachOutputs_WhenContainsMatchingStar_UpdateSuccessful()
        {
            //------------Setup for test--------------------------
            List<BaseConvertTO> fieldsCollection = new List<BaseConvertTO>
	        {
		        new BaseConvertTO( "[[rs(*).val]]","text", "hex", "abc",1)
	        };


            var dsfBaseConvert = new DsfBaseConvertActivity() { ConvertCollection = fieldsCollection };

            //------------Execute Test---------------------------
            dsfBaseConvert.UpdateForEachOutputs(new List<Tuple<string, string>>()
	        {
		        new Tuple<string, string>("[[rs(*).val]]", "[[rs(1).val]]"),
	        }, null);

            //------------Assert Results-------------------------

            var collection = dsfBaseConvert.ConvertCollection;

            Assert.AreEqual("[[rs(1).val]]", collection[0].ToExpression);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DsfBaseConvert_GetForEachInputs")]
        public void DsfBaseConvert_GetForEachInputs_Normal_UpdateSuccessful()
        {
            //------------Setup for test--------------------------
            List<BaseConvertTO> fieldsCollection = new List<BaseConvertTO>
	        {
		        // "[[result]]", "[[rs(*).val]] [[result]]", 1
		        new BaseConvertTO( "[[rs(*).val]]","text", "hex", "[[result]]",1)
	        };


            var dsfBaseConvert = new DsfBaseConvertActivity() { ConvertCollection = fieldsCollection };

            //------------Execute Test---------------------------

            var inputs = dsfBaseConvert.GetForEachInputs(null);

            //------------Assert Results-------------------------

            
            Assert.AreEqual("[[rs(*).val]]", inputs[0].Name);
    

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DsfBaseConvert_GetForEachOutputs")]
        public void DsfBaseConvert_GetForEachOutputs_Normal_UpdateSuccessful()
        {
            //------------Setup for test--------------------------
            List<BaseConvertTO> fieldsCollection = new List<BaseConvertTO>
	        {
		        // "[[result]]", "[[rs(*).val]] [[result]]", 1
		        new BaseConvertTO( "[[rs(*).val]]","text", "hex", "[[result]]",1)
	        };


            var dsfBaseConvert = new DsfBaseConvertActivity() { ConvertCollection = fieldsCollection };

            //------------Execute Test---------------------------

            var inputs = dsfBaseConvert.GetForEachOutputs(null);

            //------------Assert Results-------------------------


            Assert.AreEqual("[[rs(*).val]]", inputs[0].Value);

        }

        #endregion

        #region Private Test Methods

        private void SetupArguments(string currentDL, string testData, IList<BaseConvertTO> converCollection)
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfBaseConvertActivity { ConvertCollection = converCollection }
            };

            CurrentDl = testData;
            TestData = currentDL;
        }

        #endregion Private Test Methods

    }
}
