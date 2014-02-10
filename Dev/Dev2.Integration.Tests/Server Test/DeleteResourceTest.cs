﻿using System;
using Dev2.Common.Common;
using Dev2.Communication;
using Dev2.Controller;
using Dev2.Integration.Tests.Helpers;
using Dev2.Network;
using Dev2.Studio.Core.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests.Dev2.Application.Server.Tests
{
    [TestClass]
    public class DeleteResourceTest
    {
        private readonly string _webserverUri = ServerSettings.DsfAddress;

        public TestContext TestContext { get; set; }


        [TestMethod]
        public void DeleteWorkflowExpectsSuccessResponse()
        {
            //---------Setup-------------------------------
            IEnvironmentConnection connection = new ServerProxy(new Uri(_webserverUri));
            connection.Connect();
            const string ServiceName = "DeleteWorkflowTest";
            const string ResourceType = "WorkflowService";
            //----------Execute-----------------------------

            var coms = new CommunicationController { ServiceName = "DeleteResourceService" };

            coms.AddPayloadArgument("ResourceName", ServiceName);
            coms.AddPayloadArgument("ResourceType", ResourceType);

            var result = coms.ExecuteCommand<ExecuteMessage>(connection, Guid.Empty);

            Assert.IsTrue(result.Message.Contains("Success"), "Got [ " + result.Message + " ]");
        }

        [TestMethod]
        public void DeleteWorkflowSuccessCantDeleteDeletedWorkflow()
        {
            //---------Setup-------------------------------
            IEnvironmentConnection connection = new ServerProxy(new Uri(_webserverUri));
            connection.Connect();
            const string ServiceName = "DeleteWorkflowTest2";
            const string ResourceType = "WorkflowService";
            //----------Execute-----------------------------

            var coms = new CommunicationController { ServiceName = "DeleteResourceService" };

            coms.AddPayloadArgument("ResourceName", ServiceName);
            coms.AddPayloadArgument("ResourceType", ResourceType);

            // Execute
            var result = coms.ExecuteCommand<ExecuteMessage>(connection, Guid.Empty);

            // Assert
            Assert.IsTrue(result.Message.Contains("Success"), "Got [ " + result.Message + " ]");

            result = coms.ExecuteCommand<ExecuteMessage>(connection, Guid.Empty);
            StringAssert.Contains(result.Message.ToString(), "WorkflowService 'DeleteWorkflowTest2' was not found.");
        }

        [TestMethod]
        public void DeleteWorkflowSuccessCantCallDeletedWorkflow()
        {
            //---------Setup-------------------------------
            IEnvironmentConnection connection = new ServerProxy(new Uri(_webserverUri));
            connection.Connect();
            const string ServiceName = "DeleteWorkflowTest3";
            const string ResourceType = "WorkflowService";
            //----------Execute-----------------------------

            var coms = new CommunicationController { ServiceName = "DeleteResourceService" };

            coms.AddPayloadArgument("ResourceName", ServiceName);
            coms.AddPayloadArgument("ResourceType", ResourceType);

            var result = coms.ExecuteCommand<ExecuteMessage>(connection, Guid.Empty);

            //---------Call Workflow Failure-------
            const string serviceName = "DeleteWorkflowTest3";
            var servicecall = String.Format("{0}{1}", ServerSettings.WebserverURI, serviceName);
            var result2 = TestHelper.PostDataToWebserver(servicecall);
            Assert.IsTrue(result2.Contains("Service [ DeleteWorkflowTest3 ] not found."), "Got [ " + result + " ]");

        }

        private string BuildDeleteRequestString(string resourceName, string resourceType, string roles = "Domain Admins,Domain Users,Windows SBS Remote Web Workplace Users,Windows SBS Fax Users,Windows SBS Folder Redirection Accounts,All Users,Windows SBS SharePoint_MembersGroup,Windows SBS Link Users,Business Design Studio Developers,Build Configuration Engineers,Test Engineers,DEV2 Limited Internet Access")
        {
            return string.Format("ResourceName={0}&ResourceType={1}&Roles={2}", resourceName, resourceType, roles);
        }
    }
}
