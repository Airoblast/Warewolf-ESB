﻿using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Dev2.Infrastructure.Tests
{
    [TestClass]
    public class DbTableTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DbTable_FullName")]
        public void DbTable_FullName_HasSchema_ShouldContainSchemaDotTableName()
        {
            //------------Setup for test--------------------------
            var dbTable = new DbTable();
            dbTable.TableName = "Test";
            dbTable.Schema = "dbo";
            //------------Execute Test---------------------------
            var fullName = dbTable.FullName;
            //------------Assert Results-------------------------
            Assert.AreEqual("dbo.Test", fullName);
        }
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DbTable_FullName")]
        public void DbTable_FullName_HasEmptySchema_ShouldContainTableName()
        {
            //------------Setup for test--------------------------
            var dbTable = new DbTable();
            dbTable.TableName = "Test";
            dbTable.Schema = "";
            //------------Execute Test---------------------------
            var fullName = dbTable.FullName;
            //------------Assert Results-------------------------
            Assert.AreEqual("Test", fullName);
        }        

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DbTable_FullName")]
        public void DbTable_FullName_HasNullSchema_ShouldContainTableName()
        {
            //------------Setup for test--------------------------
            var dbTable = new DbTable();
            dbTable.TableName = "Test";
            dbTable.Schema = null;
            //------------Execute Test---------------------------
            var fullName = dbTable.FullName;
            //------------Assert Results-------------------------
            Assert.AreEqual("Test", fullName);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DbTable_FullName")]
        public void DbTable_FullName_HasEmptyTableName_ShouldEmptyString()
        {
            //------------Setup for test--------------------------
            var dbTable = new DbTable();
            dbTable.Schema = "Test";
            dbTable.TableName = "";
            //------------Execute Test---------------------------
            var fullName = dbTable.FullName;
            //------------Assert Results-------------------------
            Assert.AreEqual("", fullName);
        }        

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DbTable_FullName")]
        public void DbTable_FullName_HasTableNameNull_ShouldEmptyString()
        {
            //------------Setup for test--------------------------
            var dbTable = new DbTable();
            dbTable.Schema = "Test";
            dbTable.TableName = null;
            //------------Execute Test---------------------------
            var fullName = dbTable.FullName;
            //------------Assert Results-------------------------
            Assert.AreEqual("", fullName);
        }
    }
}
