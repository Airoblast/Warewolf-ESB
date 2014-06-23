﻿using System;
using System.Diagnostics.CodeAnalysis;
using Dev2.Common.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests
{
    [TestClass]    
    public class DirectoryHelperTests
    {
        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void CleanUpWhereIsCFolderExpectException()
        {
            DirectoryHelper.CleanUp(@"C:\");
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void CleanUpWhereIsWindowsFolderExpectException()
        {
            DirectoryHelper.CleanUp(@"C:\Windows");
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void CleanUpWhereIsSystem32FolderExpectException()
        {
            DirectoryHelper.CleanUp(@"C:\Windows\System32");
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void CleanUpWhereIsDesktopFolderExpectException()
        {
            DirectoryHelper.CleanUp(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void CleanUpWhereIsDesktopDirectoryFolderExpectException()
        {
            DirectoryHelper.CleanUp(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void CleanUpWhereIsSystem32ExpectException()
        {
            DirectoryHelper.CleanUp(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86));
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void CleanUpWhereIsSystemFolderExpectException()
        {
            DirectoryHelper.CleanUp(Environment.GetFolderPath(Environment.SpecialFolder.System));
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void CleanUpWhereIsUserProfileFolderExpectException()
        {
            DirectoryHelper.CleanUp(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void CleanUpWhereIsProgramFilesFolderExpectException()
        {
            DirectoryHelper.CleanUp(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void CleanUpWhereIsProgramFilesX86FolderExpectException()
        {
            DirectoryHelper.CleanUp(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86));
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void CleanUpWhereIsProgramsFolderExpectException()
        {
            DirectoryHelper.CleanUp(Environment.GetFolderPath(Environment.SpecialFolder.Programs));
        }
    }
}
