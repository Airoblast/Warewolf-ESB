﻿using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Dev2.Integration.Tests.Dev2.Activities.Tests
{
    /// <summary>
    /// Summary description for DsfDataSplitActivityWFTests
    /// </summary>
    [TestClass][Ignore]//Ashley: round 2 hunting the evil test
    public class DsfReplaceActivityWFTests
    {
        string WebserverURI = ServerSettings.WebserverURI;
        public DsfReplaceActivityWFTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        [TestMethod]
        public void ReplaceToolUsingRecordsetWithStar()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "ReplaceToolUsingRecordsetWithStar");
            string expected =
                @"<ReplacementCount>3</ReplacementCount><People><Name>Wallis Buchan</Name><Province>Kwa-Zulu Natal</Province></People><People><Name>Barney Buchan</Name><Province>Kwa-Zulu Natal</Province></People><People><Name>Jurie Smit</Name><Province>GP</Province></People><People><Name>Massimo Guerrera</Name><Province>Kwa-Zulu Natal</Province></People>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            Assert.IsTrue(ResponseData.Contains(expected));
        }


        [TestMethod]
        public void ReplaceToolWithScalar()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "ReplaceToolWithScalar");

            string expected = @"<Document>To whom it may concern
I would like to inform you that the following document is for the purpose
of integration testing and not for the amusment of Dr Page. Dr Page will be
adament in telling you different but Dr Page is missinformed. Aswell as I believe Dr Page will be doing some user interface development during this sprint and I would like to wish Dr Page the best of luck.
Best Wishes
King of Tools
Dr Guerrera</Document><ReplaceCount>5</ReplaceCount>";
            expected = TestHelper.CleanUp(expected);
            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            ResponseData = TestHelper.CleanUp(ResponseData);

            StringAssert.Contains(ResponseData, expected);
        }
    }
}
