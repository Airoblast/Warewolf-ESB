﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using Caliburn.Micro;
using Dev2.Composition;
using Dev2.Core.Tests.Environments;
using Dev2.Services.Events;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Webs.Callbacks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;

namespace Dev2.Core.Tests.Webs
{

    // SN : 23-01-2013 : Missing Tests 
    // 
    // Saving with Invalid connection
    // Dev2 Set with invalid environment connection

    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ConnectCallbackHandlerTests
    {
        const string ConnectionID = "1478649D-CF54-4D0D-8E26-CA9B81454B66";
        const string ConnectionCategory = "TestCategory";
        const string ConnectionName = "TestConnection";
        const string ConnectionAddress = "http://RSAKLFBRANDONPA:77/dsf";
        const int ConnectionWebServerPort = 1234;

        // Keys are case-sensitive!
        static readonly string ConnectionJson =
            "{\"ResourceID\":\"" + ConnectionID +
            "\",\"ResourceName\":\"" + ConnectionName +
            "\",\"ResourcePath\":\"TEST\",\"ResourceType\":\"Dev2Server\",\"Address\":\"" + ConnectionAddress +
            "\",\"AuthenticationType\":\"Windows\",\"UserName\":\"\",\"Password\":\"\",\"WebServerPort\":" + ConnectionWebServerPort + "}";

        static ImportServiceContext ImportContext;

        #region Class/TestInitialize

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            ImportContext = new ImportServiceContext();
            ImportService.CurrentContext = ImportContext;

            ImportService.Initialize(new List<ComposablePartCatalog>
            {
                new FullTestAggregateCatalog()
            });
            new Mock<IEventAggregator>();
        }

        [TestInitialize]
        public void TestInitialize()
        {
            ImportService.CurrentContext = ImportContext;
        }

        #endregion

        #region Save

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        // ReSharper disable InconsistentNaming - Unit Tests
        public void Save_WithNullParameters_Expected_ThrowsArgumentNullException()
        // ReSharper restore InconsistentNaming - Unit Tests
        {
            var handler = new ConnectCallbackHandler();
            handler.Save(null, null, null, null, 0, null);
            handler.Save(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(UriFormatException))]
        // ReSharper disable InconsistentNaming - Unit Tests
        public void Save_WithInValidConnectionUri_Expected_ThrowsUriFormatException()
        // ReSharper restore InconsistentNaming - Unit Tests
        {
            var handler = new ConnectCallbackHandler();
            handler.Save(ConnectionID, "xxx", "xxx", "xxx", 0, null);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        // ReSharper disable InconsistentNaming - Unit Tests
        public void Save_WithInValidConnectionID_Expected_ThrowsFormatException()
        // ReSharper restore InconsistentNaming - Unit Tests
        {
            var handler = new ConnectCallbackHandler();
            handler.Save("xxx", "xxx", "xxx", "xxx", 0, null);
        }

        [TestMethod]
        // ReSharper disable InconsistentNaming - Unit Tests
        public void Save_WithValidConnection_Expected_InvokesAddResourceService()
        // ReSharper restore InconsistentNaming - Unit Tests
        {
            var targetEnv = new Mock<IEnvironmentModel>();
            var resRepo = new Mock<IResourceRepository>();

            bool addCalled = false;

            resRepo.Setup(r => r.AddEnvironment(It.IsAny<IEnvironmentModel>(), It.IsAny<IEnvironmentModel>()))
                   .Callback(() =>
                       {
                           addCalled = true;
                       });
            
            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new StringBuilder(string.Format("<XmlData>{0}</XmlData>", string.Join("\n", new { }))));
            targetEnv.Setup(e => e.Connection).Returns(connection.Object);

            var repo = new TestEnvironmentRespository();
            repo.ActiveEnvironment = targetEnv.Object;
            targetEnv.Setup(e => e.ResourceRepository).Returns(resRepo.Object);


            var handler = new ConnectCallbackHandler(repo);
            
            handler.Save(ConnectionID, ConnectionCategory, ConnectionAddress, ConnectionName, ConnectionWebServerPort, targetEnv.Object);

            Assert.IsTrue(addCalled);
        }

        [TestMethod]
        // ReSharper disable InconsistentNaming - Unit Tests
        public void Save_WithValidConnection_Expected_InvokesSaveEnvironment()
        // ReSharper restore InconsistentNaming - Unit Tests
        {
            
            var mockConnection = new Mock<IEnvironmentConnection>();
            var mockResourceRepo = new Mock<IResourceRepository>();
            var mockEventAgg = new Mock<IEventAggregator>();
            var Env = new EnvironmentModel(mockEventAgg.Object,Guid.NewGuid(),mockConnection.Object,mockResourceRepo.Object);

            var currentRepository = new Mock<IEnvironmentRepository>();
            currentRepository.Setup(c => c.ActiveEnvironment).Returns(Env);
            currentRepository.Setup(e => e.Save(It.IsAny<IEnvironmentModel>())).Verifiable();
            currentRepository.Setup(e => e.Fetch(It.IsAny<IEnvironmentModel>())).Returns(new Mock<IEnvironmentModel>().Object);

            var handler = new ConnectCallbackHandler(currentRepository.Object);
            handler.Save(ConnectionID, ConnectionCategory, ConnectionAddress, ConnectionName, ConnectionWebServerPort, null);

            // ReSharper disable PossibleUnintendedReferenceComparison - expected to be the same instance
            currentRepository.Verify(r => r.Save(It.IsAny<IEnvironmentModel>()));
            // ReSharper restore PossibleUnintendedReferenceComparison
        }
        
        [TestMethod]
        public void SaveWithValidConnectionExpectsAddServerToExplorerMessage()
        {

            var mockConnection = new Mock<IEnvironmentConnection>();
            var mockResourceRepo = new Mock<IResourceRepository>();
            var mockEventAgg = new Mock<IEventAggregator>();
            var enviro = new EnvironmentModel(mockEventAgg.Object, Guid.NewGuid(), mockConnection.Object, mockResourceRepo.Object);

            var aggregator = new Mock<IEventAggregator>();

            EventPublishers.Aggregator = aggregator.Object;

            var currentRepository = new Mock<IEnvironmentRepository>();
            currentRepository.Setup(e => e.Save(It.IsAny<IEnvironmentModel>())).Verifiable();
            currentRepository.Setup(c => c.ActiveEnvironment).Returns(enviro);

            var environmentId = Guid.NewGuid();
            var environment = new Mock<IEnvironmentModel>();
            environment.Setup(e => e.ID).Returns(environmentId);
            currentRepository.Setup(e => e.Fetch(It.IsAny<IEnvironmentModel>())).Returns(environment.Object);
            
            var ctx = Guid.NewGuid();
            var handler = new ConnectCallbackHandler(currentRepository.Object, ctx);

            aggregator.Setup(e => e.Publish(It.IsAny<AddServerToExplorerMessage>()))
                            .Callback<Object>(m =>
                                {
                                    var msg = (AddServerToExplorerMessage) m;
                                    Assert.IsTrue(msg.Context.Equals(ctx));                                    
                                })
                             .Verifiable();

            handler.Save(ConnectionID, ConnectionCategory, ConnectionAddress, ConnectionName, ConnectionWebServerPort, null);

            aggregator.Verify(e => e.Publish(It.IsAny<AddServerToExplorerMessage>()), Times.Once());
        }        

        [TestMethod]
        public void SaveWithValidConnectionExpectsAddServerToDeployMessage()
        {
            var mockConnection = new Mock<IEnvironmentConnection>();
            var mockResourceRepo = new Mock<IResourceRepository>();
            var mockEventAgg = new Mock<IEventAggregator>();
            var enviro = new EnvironmentModel(mockEventAgg.Object, Guid.NewGuid(), mockConnection.Object, mockResourceRepo.Object);

            var aggregator = new Mock<IEventAggregator>();

            EventPublishers.Aggregator = aggregator.Object;

            var currentRepository = new Mock<IEnvironmentRepository>();
            currentRepository.Setup(e => e.Save(It.IsAny<IEnvironmentModel>())).Verifiable();
            currentRepository.Setup(c => c.ActiveEnvironment).Returns(enviro);

            var environment = new Mock<IEnvironmentModel>();
            currentRepository.Setup(e => e.Fetch(It.IsAny<IEnvironmentModel>())).Returns(environment.Object);

            var ctx = Guid.NewGuid();
            var handler = new ConnectCallbackHandler(currentRepository.Object, ctx);

            aggregator.Setup(e => e.Publish(It.IsAny<AddServerToDeployMessage>()))
                            .Callback<Object>(m =>
                            {
                                var msg = (AddServerToDeployMessage)m;
                                Assert.IsTrue(msg.Context.Equals(ctx));
                                Assert.IsTrue(msg.Server.ID.ToString()
                                                 .Equals(ConnectionID.ToString(CultureInfo.InvariantCulture),
                                                         StringComparison.InvariantCultureIgnoreCase));
                            })
                             .Verifiable();

            handler.Save(ConnectionID, ConnectionCategory, ConnectionAddress, ConnectionName, ConnectionWebServerPort, null);

            aggregator.Verify(e => e.Publish(It.IsAny<AddServerToDeployMessage>()), Times.Once());
        }

        [TestMethod]
        // ReSharper disable InconsistentNaming - Unit Tests
        public void Save_WithValidConnection_Expected_InvokesSaveEnvironmentWithCategory()
        // ReSharper restore InconsistentNaming - Unit Tests
        {
            // BUG: 8786 - TWR - 2013.02.20
            var mockConnection = new Mock<IEnvironmentConnection>();
            var mockResourceRepo = new Mock<IResourceRepository>();
            var mockEventAgg = new Mock<IEventAggregator>();
            var enviro = new EnvironmentModel(mockEventAgg.Object, Guid.NewGuid(), mockConnection.Object, mockResourceRepo.Object);

            var currentRepository = new Mock<IEnvironmentRepository>();
            currentRepository.Setup(e => e.Save(It.IsAny<IEnvironmentModel>())).Verifiable();
            currentRepository.Setup(c => c.ActiveEnvironment).Returns(enviro);

            var env = new Mock<IEnvironmentModel>();
            env.SetupProperty(e => e.Category); // start tracking sets/gets to this property

            currentRepository.Setup(e => e.Fetch(It.IsAny<IEnvironmentModel>())).Returns(env.Object);

            var handler = new ConnectCallbackHandler(new Mock<IEventAggregator>().Object, currentRepository.Object);
            handler.Save(ConnectionID, ConnectionCategory, ConnectionAddress, ConnectionName, ConnectionWebServerPort, null);

            // ReSharper disable PossibleUnintendedReferenceComparison - expected to be the same instance
            currentRepository.Verify(r => r.Save(It.IsAny<IEnvironmentModel>()));
            // ReSharper restore PossibleUnintendedReferenceComparison
        }


        #endregion

        #region Save

        [TestMethod]
        [ExpectedException(typeof(JsonReaderException))]
        // ReSharper disable InconsistentNaming - Unit Tests
        public void Save_WithInvalidJson_Expected_ThrowsJsonReaderException()
        // ReSharper restore InconsistentNaming - Unit Tests
        {
            var handler = new ConnectCallbackHandler();
            handler.Save("xxxxx", null);
        }

        [TestMethod]
        // ReSharper disable InconsistentNaming - Unit Tests
        public void Save_WithValidJson_Expected_InvokesSave()
        // ReSharper restore InconsistentNaming - Unit Tests
        {
            var mockConnection = new Mock<IEnvironmentConnection>();
            var mockResourceRepo = new Mock<IResourceRepository>();
            var mockEventAgg = new Mock<IEventAggregator>();
            var enviro = new EnvironmentModel(mockEventAgg.Object, Guid.NewGuid(), mockConnection.Object, mockResourceRepo.Object);

            var currentRepository = new Mock<IEnvironmentRepository>();
            currentRepository.Setup(e => e.Save(It.IsAny<IEnvironmentModel>())).Verifiable();
            currentRepository.Setup(e => e.Fetch(It.IsAny<IEnvironmentModel>())).Returns(new Mock<IEnvironmentModel>().Object);
            currentRepository.Setup(c => c.ActiveEnvironment).Returns(enviro);

            var handler = new ConnectCallbackHandler(new Mock<IEventAggregator>().Object, currentRepository.Object);
            handler.Save(ConnectionJson, null);

            // ReSharper disable PossibleUnintendedReferenceComparison - expected to be the same instance
            currentRepository.Verify(r => r.Save(It.IsAny<IEnvironmentModel>()));
            // ReSharper restore PossibleUnintendedReferenceComparison
        }

        #endregion

    }
}
