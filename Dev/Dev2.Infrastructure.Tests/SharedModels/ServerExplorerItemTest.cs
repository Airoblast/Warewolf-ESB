﻿using System;
using System.Collections.Generic;
using Dev2.Data.ServiceModel;
using Dev2.Explorer;
using Dev2.Interfaces;
using Dev2.Services.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Dev2.Infrastructure.Tests.SharedModels
{
    [TestClass]
    public class ServerExplorerItemTest
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ServerExplorerItem_Constructor")]
        public void ServerExplorerItem_Constructor_Construct_ExpectAllFieldsAreSetup()
        {
            //------------Setup for test--------------------------
            var guid = Guid.NewGuid();
            var name = "a";
            var explorerItemType = ResourceType.Folder;
            var children = new List<IExplorerItem>();
            var permissions = Permissions.DeployFrom;


            //------------Execute Test---------------------------
            var serverExplorerItem = new ServerExplorerItem(name, guid, explorerItemType, children, permissions, "/");
            //------------Assert Results-------------------------

            Assert.AreEqual(children, serverExplorerItem.Children);
            Assert.AreEqual(name, serverExplorerItem.DisplayName);
            Assert.AreEqual(serverExplorerItem.ResourceType, explorerItemType);
            Assert.AreEqual(permissions, serverExplorerItem.Permissions);
            Assert.AreEqual(guid, serverExplorerItem.ResourceId);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ServerExplorerItem_Constructor")]
        public void ServerExplorerItem_GetHashCode_ExpectHashCodeSameAsID()
        {
            //------------Setup for test--------------------------
            var guid = Guid.NewGuid();
            var name = "a";
            var explorerItemType = ResourceType.PluginService;
            var children = new List<IExplorerItem>();
            var permissions = Permissions.DeployFrom;


            //------------Execute Test---------------------------
            var serverExplorerItem = new ServerExplorerItem(name, guid, explorerItemType, children, permissions, "");
            //------------Assert Results-------------------------

            Assert.AreEqual(guid.GetHashCode(), serverExplorerItem.GetHashCode());

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ServerExplorerItem_Constructor")]
        public void ServerExplorerItem_Equals_ExpectEqualityOnGuidOnly()
        {
            //------------Setup for test--------------------------
            var guid = Guid.NewGuid();
            var name = "a";
            var explorerItemType = ResourceType.PluginService;
            var children = new List<IExplorerItem>();
            var permissions = Permissions.DeployFrom;


            //------------Execute Test---------------------------
            var serverExplorerItem = new ServerExplorerItem(name, guid, explorerItemType, children, permissions, "");
            var serverExplorerItem2 = new ServerExplorerItem(name, guid, explorerItemType, children, Permissions.Administrator, "");
            var serverExplorerItem3 = new ServerExplorerItem(name, Guid.NewGuid(), explorerItemType, children, permissions, "");
            //------------Assert Results-------------------------

            Assert.AreEqual(serverExplorerItem, serverExplorerItem2);
            Assert.AreNotEqual(serverExplorerItem, null);
            Assert.AreEqual(serverExplorerItem, serverExplorerItem);
            Assert.AreNotEqual(serverExplorerItem, guid);
            Assert.AreNotEqual(serverExplorerItem, serverExplorerItem3);

        }
    }
}
