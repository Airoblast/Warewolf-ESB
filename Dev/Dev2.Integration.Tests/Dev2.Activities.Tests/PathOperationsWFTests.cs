﻿using System.IO;
using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Dev2.Integration.Tests.Dev2.Activities.Tests
{
    /// <summary>
    /// Summary description for PathOperationsWFTests
    /// </summary>
    [TestClass]
    [Ignore]//Ashley 25/11/2013 - Ignored until Tshepo is done with IO tools
    public class PathOperationsWFTests
    {
        string WebserverURI = ServerSettings.WebserverURI;
        public PathOperationsWFTests()
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

        #region Language Tests

        [TestMethod]
        public void CreateFileUsingRecordsetWithNoIndex()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "CreateFileUsingRecordsetWithNoIndexTest");
            string expected = "<CreateFileRes>Success</CreateFileRes><DeleteFileRes>Success</DeleteFileRes>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            StringAssert.Contains(ResponseData, expected);
        }

        [TestMethod]
        public void CreateFileUsingRecordsetWithRecordsetWithIndex()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "CreateFileUsingRecordsetWithRecordsetWithIndex");
            string expected = @"<Recordset><record>C:\Temp\PathOperationsTestFolder\NewFolder\NewFolderFirstInnerFolder\NewCreatedFile.txt</record></Recordset><CreateFileRes>Success</CreateFileRes><DeleteFileRes>Success</DeleteFileRes>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            StringAssert.Contains(ResponseData, expected);
        }

        [TestMethod]
        public void CreateFileUsingRecordsetWithStar()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "CreateFileUsingRecordsetWithStar");

            // Why is this using a specific IP Address?? Is this service only on this machine?
            //string PostData = String.Format("{0}{1}", "http://192.168.104.33:1234/services", "CreateFileUsingRecordsetWithStar");
            string expected = @"<Recorset><record>C:\Temp\PathOperationsTestFolder\NewFolder\NewFolderFirstInnerFolder\NewCreatedFile.txt</record></Recorset><Recorset><record>C:\Temp\PathOperationsTestFolder\NewFolder\NewFolderFirstInnerFolder\NewCreatedFile2.txt</record></Recorset><Recorset><record>C:\Temp\PathOperationsTestFolder\NewFolder\NewFolderFirstInnerFolder\NewCreatedFile3.txt</record></Recorset><Recorset><record>C:\Temp\PathOperationsTestFolder\NewFolder\NewFolderFirstInnerFolder\NewCreatedFile4.txt</record></Recorset><DeleteFileRes>Success</DeleteFileRes><RESULT><RES>Success</RES></RESULT><RESULT><RES>Success</RES></RESULT><RESULT><RES>Success</RES></RESULT><RESULT><RES>Success</RES></RESULT>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            StringAssert.Contains(ResponseData, expected);
        }

        [TestMethod]
        public void CreateFileUsingScalar()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "CreateFileUsingScalar");
            string expected = @"<Path>C:\Temp\PathOperationsTestFolder\NewFolder\NewFolderFirstInnerFolder\NewCreatedFile.txt</Path><CreateFileRes>Success</CreateFileRes><DeleteFileRes>Success</DeleteFileRes>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            StringAssert.Contains(ResponseData, expected);
        }

        #endregion Language Tests

        #region Create File Tests

        [TestMethod]
        public void CreateFile()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "CreateFileOnFileSystemTest");
            string expected = @"<CreateFileRes>Success</CreateFileRes><DeleteFileRes>Success</DeleteFileRes>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            StringAssert.Contains(ResponseData, expected);
        }

        #endregion Create File Tests

        #region Read File Tests

        [TestMethod]
        public void ReadFile()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "ReadFileOffFileSystemTest");
            string expected = @"<FileReadResult>ABC</FileReadResult><WriteFileRes>Success</WriteFileRes><DeleteFileRes>Success</DeleteFileRes>";
            // A deferred read is expected RE Travis's feedback rom The Unlimited
            //string expected = "Contents not visible because this value is a deferred read of";
            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            StringAssert.Contains(ResponseData, expected);
        }

        #endregion Read File Tests

        #region Write File Tests

        [TestMethod]
        public void WriteToFile()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "WriteToFileOnFileSysytemTest");
            string expected = @"<WriteFileRes>Success</WriteFileRes><DeleteFileRes>Success</DeleteFileRes>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            StringAssert.Contains(ResponseData, expected);
        }

        #endregion Write File Tests

        #region Create Folder Tests

        [TestMethod]
        public void CreateFolder()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "CreateFolderOnFileSystemTest");
            string expected = @"<DataList><CreateFolderRes>Success</CreateFolderRes><DeleteFolderRes>Success</DeleteFolderRes>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            StringAssert.Contains(ResponseData, expected);
        }

        #endregion Create Folder Tests

        #region Read Folder Tests

        [TestMethod]
        public void ReadFolder()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "ReadFolderFromFileSystemTest");
            string expected = @"<CreateFileRes1>Success</CreateFileRes1><CreateFileRes2>Success</CreateFileRes2><DeleteFileRes>Success</DeleteFileRes><ReadFolderResult><results>C:\Temp\PathOperationsTestFolder\OldFolder\OldFolderFirstInnerFolder\testfile1.txt</results></ReadFolderResult><ReadFolderResult><results>C:\Temp\PathOperationsTestFolder\OldFolder\OldFolderFirstInnerFolder\testfile2.txt</results></ReadFolderResult>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            StringAssert.Contains(ResponseData, expected);
        }

        [TestMethod]
        public void ReadFolderInForEachLoop_Expected_NoNotImplementedError()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "FolderReadInForEachTest");
            string notExpected = "<InnerError>The method or operation is not implemented.</InnerError>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            Assert.IsFalse(ResponseData.Contains(notExpected));            
        }


        [TestMethod]
        public void FolderReadUsingScalar()
        {
            string postData = String.Format("{0}{1}", WebserverURI, "FolderReadUsingScalar");
            string expected = ",";

            string responseData = TestHelper.PostDataToWebserver(postData);

            StringAssert.Contains(responseData, expected);
        }

        #endregion Read Folder Tests

        #region Copy Tests

        [TestMethod]
        public void CopyFile()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "CopyFileLocalToLocalTest");
            string expected = @"<CreateFileRes>Success</CreateFileRes><CopyFileRes>Success</CopyFileRes><DeleteFileRes>Success</DeleteFileRes>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            StringAssert.Contains(ResponseData, expected);
        }

        [TestMethod]
        public void CopyFileToFTP()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "CreateFileCopyToFTP");
            string expected = @"<CopyRes>Success</CopyRes><CreateRes>Success</CreateRes><FTPDeleteRes>Success</FTPDeleteRes><LocalDeleteRes>Success</LocalDeleteRes>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            StringAssert.Contains(ResponseData, expected);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("DsfCopyTool_Execution")]
        public void DsfCopyTool_Execute_RemoteToLocal_Success()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "FileCopyFromFTP");
            string expected = @"<Result>Success</Result>";

            //------------Execute Test---------------------------
            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            // Assert Success
            StringAssert.Contains(ResponseData, expected);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("DsfCopyTool_Execution")]
        public void DsfCopyTool_Execute_RemoteToRemote_Success()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "CreateFileCopyFtpToFtp");
            string expected = @"<Result>Success</Result>";

            //------------Execute Test---------------------------
            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            // Assert Success
            StringAssert.Contains(ResponseData, expected);
        }

        #endregion Copy Tests

        #region Delete Tests

        [TestMethod]
        public void DeleteFolder()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "DeleteFolderFromFileSystemTest");
            string expected = @"<DeleteFolderRes>Success</DeleteFolderRes><CreateFolderRes>Success</CreateFolderRes>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            StringAssert.Contains(ResponseData, expected);
        }

        #endregion Delete Tests

        #region Move Tests

        [TestMethod]
        public void MoveFolder()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "MoveFolderOnFileSystemTest");
            string expected = @"<DataList><MoveFileRes>Success</MoveFileRes><CreateFileRes>Success</CreateFileRes><DeleteFileRes>Success</DeleteFileRes>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            StringAssert.Contains(ResponseData, expected);
        }

        #endregion Move Tests

        #region Rename Tests

        [TestMethod]
        public void RenameFolder()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "RenameFolderOnFileSystemTest");
            string expected = @"<CreateFileRes>Success</CreateFileRes><RenameFileRes>Success</RenameFileRes><DeleteFileRes>Success</DeleteFileRes>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            StringAssert.Contains(ResponseData, expected);
        }

        #endregion Rename Tests

        #region UnZip Tests

        [TestMethod]
        public void UnZip()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "UnZipOnFileSystemTest");
            string expected = @"<CreateFileRes>Success</CreateFileRes><ZipFileRes>Success</ZipFileRes><UnZipFileRes>Success</UnZipFileRes><DeleteFileRes1>Success</DeleteFileRes1><DeleteFileRes2>Success</DeleteFileRes2>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            StringAssert.Contains(ResponseData, expected);
        }

        #endregion UnZip Tests

        #region Zip Tests

        [TestMethod]
        public void ZipFolder()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "ZipFolderOnFileSystemTest");
            string expected = @"<DeleteFileRes>Success</DeleteFileRes><ZipFileRes>Success</ZipFileRes><CreateFileRes>Success</CreateFileRes>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            StringAssert.Contains(ResponseData, expected);
        }

        #endregion Zip Tests

    }
}
