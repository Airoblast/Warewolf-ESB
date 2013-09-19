﻿using Dev2.Common.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.Persistence
{
    /// <summary>
    /// Summary description for AvlTreeTest
    /// </summary>
    [TestClass][Ignore]//Ashley: One of these tests may be causing the server to hang in a background thread, preventing windows 7 build server from performing any more builds
    public class GetComputerNamesTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void GetComputerNamesListExpectListOfComputerNames()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            GetComputerNames.GetComputerNamesList();
            //------------Assert Results-------------------------
            Assert.IsNotNull(GetComputerNames.ComputerNames);
            Assert.IsTrue(GetComputerNames.ComputerNames.Count >= 1);
        }

    }
}