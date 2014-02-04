﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.TOTests
{
    /// <summary>
    /// Summary description for DataMergeTOTests
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class DataMergeTOTests
    {
        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("DataMergeDTO_Constructor")]
        public void DataMergeDTO_Constructor_FullConstructor_DefaultValues()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dataMergeDTO = new DataMergeDTO(string.Empty, null, null, 1, null, null);
            //------------Assert Results-------------------------
            Assert.AreEqual("Index", dataMergeDTO.MergeType);
            Assert.AreEqual(string.Empty, dataMergeDTO.At);
            Assert.AreEqual(1, dataMergeDTO.IndexNumber);
            Assert.AreEqual(string.Empty, dataMergeDTO.Padding);
            Assert.AreEqual("Left", dataMergeDTO.Alignment);
            Assert.IsNotNull(dataMergeDTO.Errors);
        }

        #region CanAdd Tests

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("DataMergeDTO_CanAdd")]
        public void DataMergeDTO_CanAdd_WithNewLineMergeTypeAndNoOtherValues_ReturnTrue()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dataMergeDTO = new DataMergeDTO(string.Empty, "NewLine", null, 1, null, null);
            //------------Assert Results-------------------------
            Assert.IsTrue(dataMergeDTO.CanAdd());
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("DataMergeDTO_CanAdd")]
        public void DataMergeDTO_CanAdd_WithNoInputVarButValueForAt_ReturnTrue()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dataMergeDTO = new DataMergeDTO(string.Empty, null, "|", 1, null, null);
            //------------Assert Results-------------------------
            Assert.IsTrue(dataMergeDTO.CanAdd());
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("DataMergeDTO_CanAdd")]
        public void DataMergeDTO_CanAdd_WithNoInputVarAndNoAt_ReturnFalse()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dataMergeDTO = new DataMergeDTO(string.Empty, null, null, 1, null, null);
            //------------Assert Results-------------------------
            Assert.IsFalse(dataMergeDTO.CanAdd());
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("DataMergeDTO_CanAdd")]
        public void DataMergeDTO_CanAdd_WithIndexMergeTypeAndNoOtherValues_ReturnFalse()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dataMergeDTO = new DataMergeDTO(string.Empty, "Index", null, 1, null, null);
            //------------Assert Results-------------------------
            Assert.IsFalse(dataMergeDTO.CanAdd());
        }

        #endregion

        #region CanRemove Tests

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("DataMergeDTO_CanRemove")]
        public void DataMergeDTO_CanRemove_WithNoInputVarButValueForAt_ReturnFalse()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dataMergeDTO = new DataMergeDTO(string.Empty, null, "|", 1, null, null);
            //------------Assert Results-------------------------
            Assert.IsFalse(dataMergeDTO.CanRemove());
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("DataMergeDTO_CanRemove")]
        public void DataMergeDTO_CanRemove_WithNoInputVarAndNoAt_ReturnTrue()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dataMergeDTO = new DataMergeDTO(string.Empty, null, null, 1, null, null);
            //------------Assert Results-------------------------
            Assert.IsTrue(dataMergeDTO.CanRemove());
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("DataMergeDTO_CanRemove")]
        public void DataMergeDTO_CanRemove_WithNewLineTypeAndNoInputVar_ReturnFalse()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dataMergeDTO = new DataMergeDTO(string.Empty, "NewLine", null, 1, null, null);
            //------------Assert Results-------------------------
            Assert.IsFalse(dataMergeDTO.CanRemove());
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("DataMergeDTO_CanRemove")]
        public void DataMergeDTO_CanRemove_WithNewLineInputTypeAndVar_ReturnFalse()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dataMergeDTO = new DataMergeDTO("s", "NewLine", null, 1, null, null);
            //------------Assert Results-------------------------
            Assert.IsFalse(dataMergeDTO.CanRemove());
        }

        #endregion


        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DataMergeDTO_IsEmpty")]
        public void DataMergeDTO_IsEmpty_PropertiesAreEmpty_True()
        {
            Verify_IsEmpty(DataMergeDTO.MergeTypeIndex);
            Verify_IsEmpty(DataMergeDTO.MergeTypeChars);
            Verify_IsEmpty(DataMergeDTO.MergeTypeNone);
        }

        void Verify_IsEmpty(string mergeType)
        {
            //------------Setup for test--------------------------
            var dto = new DataMergeDTO { MergeType = mergeType };

            //------------Execute Test---------------------------
            var actual = dto.IsEmpty();

            //------------Assert Results-------------------------
            Assert.IsTrue(actual);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DataMergeDTO_IsEmpty")]
        public void DataMergeDTO_IsEmpty_PropertiesAreNotEmpty_False()
        {
            //------------Setup for test--------------------------
            var dto = new DataMergeDTO { InputVariable = "xxx" };

            //------------Execute Test---------------------------
            var actual = dto.IsEmpty();

            //------------Assert Results-------------------------
            Assert.IsFalse(actual);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DataMergeDTO_ClearRow")]
        public void DataMergeDTO_ClearRow_PropertiesAreEmpty()
        {
            //------------Setup for test--------------------------
            var dto = new DataMergeDTO { InputVariable = "xxx", MergeType = DataMergeDTO.MergeTypeNone, Alignment = "Right", At ="1", Padding = " "};

            Assert.IsFalse(dto.IsEmpty());

            //------------Execute Test---------------------------
            dto.ClearRow();

            //------------Assert Results-------------------------
            Assert.IsTrue(dto.IsEmpty());
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DataMergeDTO_GetRuleSet")]
        public void DataMergeDTO_GetRuleSet_IsEmptyIsTrue_ValidateRulesReturnsTrue()
        {
            //------------Setup for test--------------------------
            var dto = new DataMergeDTO();

            //------------Execute Test---------------------------
            Verify_RuleSet(dto, "Padding", null);
            Verify_RuleSet(dto, "At", null);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DataMergeDTO_GetRuleSet")]
        public void DataMergeDTO_GetRuleSet_PaddingExpressionIsInvalid_ValidateRulesReturnsFalse()
        {
            //------------Setup for test--------------------------
            var dto = new DataMergeDTO { Padding = "h]]", At = "1" };

            //------------Execute Test---------------------------
            Verify_RuleSet(dto, "Padding", "Invalid expression: opening and closing brackets don't match");
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DataMergeDTO_GetRuleSet")]
        public void DataMergeDTO_GetRuleSet_PaddingExpressionIsValid_ValidateRulesReturnsTrue()
        {
            //------------Setup for test--------------------------
            var dto = new DataMergeDTO { Padding = "[[h]]", At = "1" };

            //------------Execute Test---------------------------
            Verify_RuleSet(dto, "Padding", null);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DataMergeDTO_GetRuleSet")]
        public void DataMergeDTO_GetRuleSet_PaddingIsNotSingleChar_ValidateRulesReturnsFalse()
        {
            //------------Setup for test--------------------------
            var dto = new DataMergeDTO { Padding = "aa", At = "1" };

            //------------Execute Test---------------------------
            Verify_RuleSet(dto, "Padding", "must be a single character");
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DataMergeDTO_GetRuleSet")]
        public void DataMergeDTO_GetRuleSet_PaddingIsSingleChar_ValidateRulesReturnsTrue()
        {
            //------------Setup for test--------------------------
            var dto = new DataMergeDTO { Padding = " ", At = "1" };

            //------------Execute Test---------------------------
            Verify_RuleSet(dto, "Padding", null);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DataMergeDTO_GetRuleSet")]
        public void DataMergeDTO_GetRuleSet_PaddingIsEmpty_ValidateRulesReturnsTrue()
        {
            //------------Setup for test--------------------------
            var dto = new DataMergeDTO { Padding = "", At = "1" };

            //------------Execute Test---------------------------
            Verify_RuleSet(dto, "Padding", null);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DataMergeDTO_GetRuleSet")]
        public void DataMergeDTO_GetRuleSet_AtExpressionIsInvalid_ValidateRulesReturnsFalse()
        {
            //------------Setup for test--------------------------
            var dto = new DataMergeDTO { InputVariable = "[[a]]", At = "h]]" };

            //------------Execute Test---------------------------
            Verify_RuleSet(dto, "At", "Invalid expression: opening and closing brackets don't match");
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DataMergeDTO_GetRuleSet")]
        public void DataMergeDTO_GetRuleSet_AtExpressionIsValid_ValidateRulesReturnsTrue()
        {
            //------------Setup for test--------------------------
            var dto = new DataMergeDTO { InputVariable = "[[a]]", At = "[[h]]" };

            //------------Execute Test---------------------------
            Verify_RuleSet(dto, "At", null);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DataMergeDTO_GetRuleSet")]
        public void DataMergeDTO_GetRuleSet_AtIsNullOrEmpty_ValidateRulesReturnsFalse()
        {
            //------------Setup for test--------------------------
            var dto = new DataMergeDTO { InputVariable = "[[a]]", At = "" };

            //------------Execute Test---------------------------
            Verify_RuleSet(dto, "At", "cannot be empty or null");
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DataMergeDTO_GetRuleSet")]
        public void DataMergeDTO_GetRuleSet_AtIsNotNullOrEmpty_ValidateRulesReturnsTrue()
        {
            //------------Setup for test--------------------------
            var dto = new DataMergeDTO { InputVariable = "[[a]]", At = "1" };

            //------------Execute Test---------------------------
            Verify_RuleSet(dto, "At", null);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DataMergeDTO_GetRuleSet")]
        public void DataMergeDTO_GetRuleSet_AtIsNotPositiveNumber_ValidateRulesReturnsFalse()
        {
            //------------Setup for test--------------------------
            var dto = new DataMergeDTO { InputVariable = "[[a]]", At = "-1" };

            //------------Execute Test---------------------------
            Verify_RuleSet(dto, "At", "must be a positive whole number");
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DataMergeDTO_GetRuleSet")]
        public void DataMergeDTO_GetRuleSet_AtIsPositiveNumber_ValidateRulesReturnsTrue()
        {
            //------------Setup for test--------------------------
            var dto = new DataMergeDTO { InputVariable = "[[a]]", At = "1" };

            //------------Execute Test---------------------------
            Verify_RuleSet(dto, "At", null);
        }

        static void Verify_RuleSet(DataMergeDTO dto, string propertyName, string expectedErrorMessage)
        {

            //------------Execute Test---------------------------
            var ruleSet = dto.GetRuleSet(propertyName);
            var errors = ruleSet.ValidateRules(null, null);

            //------------Assert Results-------------------------
            if(expectedErrorMessage == null)
            {
                Assert.AreEqual(0, errors.Count);
            }
            else
            {
                var err = errors.FirstOrDefault(e => e.Message.Contains(expectedErrorMessage));
                Assert.IsNotNull(err);
            }
        }

    }
}
