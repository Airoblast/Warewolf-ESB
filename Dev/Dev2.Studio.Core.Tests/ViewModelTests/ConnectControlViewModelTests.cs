﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics.CodeAnalysis;
using Caliburn.Micro;
using Dev2.AppResources.Enums;
using Dev2.Composition;
using Dev2.Core.Tests.Environments;
using Dev2.Messages;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Threading;
using Dev2.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
// ReSharper disable InconsistentNaming
namespace Dev2.Core.Tests.ViewModelTests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ConnectControlViewModelTests
    {

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ConnectControlViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConnectControlViewModel_Constructor_ActiveEnvironmentIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------
            var testEnvRepo = new TestEnvironmentRespository(new Mock<IEnvironmentModel>().Object);
            new EnvironmentRepository(testEnvRepo);
            //------------Execute Test---------------------------
            // ReSharper disable ObjectCreationAsStatement
            new ConnectControlViewModel(null, null);
            // ReSharper restore ObjectCreationAsStatement
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ConnectControlViewModel_Constructor")]
        public void ConnectControlViewModel_Constructor_ActiveEnvironmentIsNotNull_SetsProperty()
        {
            //------------Setup for test--------------------------
            var testEnvRepo = new TestEnvironmentRespository(new Mock<IEnvironmentModel>().Object);
            new EnvironmentRepository(testEnvRepo);
            var env = new Mock<IEnvironmentModel>();
            //------------Execute Test---------------------------
            var connectViewModel = new ConnectControlViewModel(env.Object, new EventAggregator());

            //------------Assert Results-------------------------
            Assert.IsNotNull(connectViewModel.ActiveEnvironment);
            Assert.AreSame(env.Object, connectViewModel.ActiveEnvironment);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ConnectControlViewModel_LoadServers")]
        public void ConnectControlViewModel_LoadServers_NullEnvironmentModel_ShouldLoadServerNotSetSelectedServer()
        {
            //------------Setup for test--------------------------
            SetupMef();
            var testEnvRepo = new TestEnvironmentRespository(new Mock<IEnvironmentModel>().Object);
            new EnvironmentRepository(testEnvRepo);
            var localhostServer = CreateServer("localhost", true);
            var remoteServer = CreateServer("remote", false);
            var otherServer = CreateServer("disconnected", false);
            var connectViewModel = new ConnectControlViewModel(localhostServer, new EventAggregator());
            var mockEnvironmentRepository = new TestEnvironmentRespository(localhostServer, remoteServer, otherServer);
            // ReSharper disable ObjectCreationAsStatement
            new EnvironmentRepository(mockEnvironmentRepository);
            // ReSharper restore ObjectCreationAsStatement
            //------------Execute Test---------------------------
            connectViewModel.LoadServers();
            //------------Assert Results-------------------------
            Assert.AreEqual("New Remote Server...", connectViewModel.Servers[0].Name);
            Assert.AreEqual(4, connectViewModel.Servers.Count);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ConnectControlViewModel_LoadServers")]
        public void ConnectControlViewModel_LoadServers_WithEnvironmentModel_ShouldLoadServerNotSetSelectedServer()
        {
            //------------Setup for test--------------------------
            SetupMef();
            var testEnvRepo = new TestEnvironmentRespository(new Mock<IEnvironmentModel>().Object);
            new EnvironmentRepository(testEnvRepo);
            var localhostServer = CreateServer("localhost", true);
            var remoteServer = CreateServer("remote", false);
            var otherServer = CreateServer("disconnected", false);
            var connectViewModel = new ConnectControlViewModel(localhostServer, new EventAggregator());
            var mockEnvironmentRepository = new TestEnvironmentRespository(localhostServer, remoteServer, otherServer);
            // ReSharper disable ObjectCreationAsStatement
            new EnvironmentRepository(mockEnvironmentRepository);
            // ReSharper restore ObjectCreationAsStatement
            //------------Execute Test---------------------------
            connectViewModel.LoadServers(remoteServer);
            //------------Assert Results-------------------------
            Assert.AreEqual(4, connectViewModel.Servers.Count);
            Assert.AreEqual(null, connectViewModel.SelectedServer);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ConnectControlViewModel_LoadServers")]
        public void ConnectControlViewModel_LoadServers_WithEnvironmentModelNotFound_ShouldLoadServerNotSetSelectedServer()
        {
            //------------Setup for test--------------------------
            SetupMef();
            var testEnvRepo = new TestEnvironmentRespository(new Mock<IEnvironmentModel>().Object);
            new EnvironmentRepository(testEnvRepo);
            var localhostServer = CreateServer("localhost", true);
            var remoteServer = CreateServer("remote", false);
            var otherServer = CreateServer("disconnected", false);
            var connectViewModel = new ConnectControlViewModel(localhostServer, new EventAggregator());
            var mockEnvironmentRepository = new TestEnvironmentRespository(localhostServer, otherServer);
            // ReSharper disable ObjectCreationAsStatement
            new EnvironmentRepository(mockEnvironmentRepository);
            // ReSharper restore ObjectCreationAsStatement
            //------------Execute Test---------------------------
            connectViewModel.LoadServers(remoteServer);
            //------------Assert Results-------------------------
            Assert.AreEqual(3, connectViewModel.Servers.Count);
            Assert.IsNull(connectViewModel.SelectedServer);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ConnectControlViewModel_LoadServers")]
        public void ConnectControlViewModel_LoadServers_NullEnvironmentModelWithMutlipleServers_ShouldLoadServerNotSetSelectedServer()
        {
            //------------Setup for test--------------------------
            SetupMef();
            var testEnvRepo = new TestEnvironmentRespository(new Mock<IEnvironmentModel>().Object);
            new EnvironmentRepository(testEnvRepo);
            var localhostServer = CreateServer("localhost", true);
            var connectViewModel = new ConnectControlViewModel(localhostServer, new EventAggregator());
            var mockEnvironmentRepository = new TestEnvironmentRespository(localhostServer);
            // ReSharper disable ObjectCreationAsStatement
            new EnvironmentRepository(mockEnvironmentRepository);
            // ReSharper restore ObjectCreationAsStatement
            //------------Execute Test---------------------------
            connectViewModel.LoadServers();
            //------------Assert Results-------------------------
            Assert.AreEqual(2, connectViewModel.Servers.Count);
            Assert.AreEqual(localhostServer.Name, connectViewModel.Servers[1].Name);
            Assert.IsNull(connectViewModel.SelectedServer);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ConnectControl_SelectionChanged")]
        public void ConnectControlViewModel_SelectionChanged_BoundToActiveEnvironment_SelectNewServer_NewServerAdded()
        {
            //------------Setup for test--------------------------
            var testEnvRepo = new TestEnvironmentRespository(new Mock<IEnvironmentModel>().Object);
            new EnvironmentRepository(testEnvRepo);
            var localhostServer = CreateServer("localhost", true);
            var remoteServer = CreateServer("remote", false);
            var otherServer = CreateServer("disconnected", false);
            var mockEventAggregator = new Mock<IEventAggregator>();
            mockEventAggregator.Setup(aggregator => aggregator.Publish(It.IsAny<SetSelectedItemInExplorerTree>())).Verifiable();
            mockEventAggregator.Setup(aggregator => aggregator.Publish(It.IsAny<SetActiveEnvironmentMessage>())).Verifiable();
            mockEventAggregator.Setup(aggregator => aggregator.Publish(It.IsAny<ServerSelectionChangedMessage>())).Verifiable();
            var connectControlViewModel = new TestConnectControlViewModel(localhostServer, mockEventAggregator.Object);
            var mockEnvironmentRepository = new TestEnvironmentRespository(localhostServer, remoteServer, otherServer);
            // ReSharper disable ObjectCreationAsStatement
            new EnvironmentRepository(mockEnvironmentRepository);
            // ReSharper restore ObjectCreationAsStatement
            connectControlViewModel.BindToActiveEnvironment = true;
            connectControlViewModel.LoadServers();
            var environmentModel = connectControlViewModel.Servers[0];
            //------------Execute Test---------------------------
            connectControlViewModel.SelectedServer = environmentModel;
            connectControlViewModel.SelectedServerHasChanged(environmentModel, connectControlViewModel);
            //------------Assert Results-------------------------
            Assert.AreEqual(1, connectControlViewModel.openNewConnectionWizardHitCount);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ConnectControl_SelectionChanged")]
        public void ConnectControlViewModel_SelectionChanged_WhenHasItem_NotBoundToActiveEnvironmentAndInstanceTypeDeployTarget_ShouldNotFireMessages()
        {
            //------------Setup for test--------------------------
            var testEnvRepo = new TestEnvironmentRespository(new Mock<IEnvironmentModel>().Object);
            new EnvironmentRepository(testEnvRepo);
            var localhostServer = CreateServer("localhost", true);
            var remoteServer = CreateServer("remote", false);
            var otherServer = CreateServer("disconnected", false);
            var mockEventAggregator = new Mock<IEventAggregator>();
            mockEventAggregator.Setup(aggregator => aggregator.Publish(It.IsAny<SetSelectedItemInExplorerTree>())).Verifiable();
            mockEventAggregator.Setup(aggregator => aggregator.Publish(It.IsAny<SetActiveEnvironmentMessage>())).Verifiable();
            mockEventAggregator.Setup(aggregator => aggregator.Publish(It.IsAny<ServerSelectionChangedMessage>())).Verifiable();
            var connectControlViewModel = new ConnectControlViewModel(localhostServer, mockEventAggregator.Object) { ConnectControlInstanceType = ConnectControlInstanceType.DeployTarget, Servers = new ObservableCollection<IEnvironmentModel> { localhostServer, remoteServer, otherServer }, BindToActiveEnvironment = false, SelectedServer = remoteServer };
            //------------Execute Test---------------------------
            connectControlViewModel.SelectedServerHasChanged(remoteServer, connectControlViewModel);
            //------------Assert Results-------------------------
            mockEventAggregator.Verify(aggregator => aggregator.Publish(It.IsAny<SetSelectedItemInExplorerTree>()), Times.Never());
            mockEventAggregator.Verify(aggregator => aggregator.Publish(It.IsAny<SetActiveEnvironmentMessage>()), Times.Never());
            mockEventAggregator.Verify(aggregator => aggregator.Publish(It.IsAny<ServerSelectionChangedMessage>()), Times.Once());
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ConnectControl_SelectionChanged")]
        public void ConnectControlViewModel_SelectionChanged_WhenHasItem_NotBoundToActiveEnvironmentAndInstanceTypeExplorer_ShouldFireMessages()
        {
            //------------Setup for test--------------------------
            var testEnvRepo = new TestEnvironmentRespository(new Mock<IEnvironmentModel>().Object);
            new EnvironmentRepository(testEnvRepo);
            var localhostServer = CreateServer("localhost", true);
            var remoteServer = CreateServer("remote", false);
            var otherServer = CreateServer("disconnected", false);
            var mockEventAggregator = new Mock<IEventAggregator>();
            mockEventAggregator.Setup(aggregator => aggregator.Publish(It.IsAny<SetSelectedItemInExplorerTree>())).Verifiable();
            mockEventAggregator.Setup(aggregator => aggregator.Publish(It.IsAny<SetActiveEnvironmentMessage>())).Verifiable();
            mockEventAggregator.Setup(aggregator => aggregator.Publish(It.IsAny<ServerSelectionChangedMessage>())).Verifiable();
            var connectControlViewModel = new ConnectControlViewModel(localhostServer, mockEventAggregator.Object) { ConnectControlInstanceType = ConnectControlInstanceType.Explorer, Servers = new ObservableCollection<IEnvironmentModel> { localhostServer, remoteServer, otherServer }, BindToActiveEnvironment = false, SelectedServer = remoteServer };
            //------------Execute Test---------------------------
            connectControlViewModel.SelectedServerHasChanged(remoteServer, connectControlViewModel);
            //------------Assert Results-------------------------
            mockEventAggregator.Verify(aggregator => aggregator.Publish(It.IsAny<SetSelectedItemInExplorerTree>()), Times.Once());
            mockEventAggregator.Verify(aggregator => aggregator.Publish(It.IsAny<SetActiveEnvironmentMessage>()), Times.Once());
            mockEventAggregator.Verify(aggregator => aggregator.Publish(It.IsAny<AddServerToExplorerMessage>()), Times.Once());
            mockEventAggregator.Verify(aggregator => aggregator.Publish(It.IsAny<ServerSelectionChangedMessage>()), Times.Once());
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ConnectControl_SelectionChanged")]
        public void ConnectControlViewModel_SelectionChanged_WhenHasItem_NotBoundToActiveEnvironmentAndInstanceTypeDeploySource_ShouldNotFireMessages()
        {
            //------------Setup for test--------------------------
            var testEnvRepo = new TestEnvironmentRespository(new Mock<IEnvironmentModel>().Object);
            new EnvironmentRepository(testEnvRepo);
            var localhostServer = CreateServer("localhost", true);
            var remoteServer = CreateServer("remote", false);
            var otherServer = CreateServer("disconnected", false);
            var mockEventAggregator = new Mock<IEventAggregator>();
            mockEventAggregator.Setup(aggregator => aggregator.Publish(It.IsAny<SetSelectedItemInExplorerTree>())).Verifiable();
            mockEventAggregator.Setup(aggregator => aggregator.Publish(It.IsAny<SetActiveEnvironmentMessage>())).Verifiable();
            mockEventAggregator.Setup(aggregator => aggregator.Publish(It.IsAny<ServerSelectionChangedMessage>())).Verifiable();
            var connectControlViewModel = new ConnectControlViewModel(localhostServer, mockEventAggregator.Object) { ConnectControlInstanceType = ConnectControlInstanceType.DeploySource, Servers = new ObservableCollection<IEnvironmentModel> { localhostServer, remoteServer, otherServer }, BindToActiveEnvironment = false, SelectedServer = remoteServer };
            //------------Execute Test---------------------------
            connectControlViewModel.SelectedServerHasChanged(remoteServer, connectControlViewModel);
            //------------Assert Results-------------------------
            mockEventAggregator.Verify(aggregator => aggregator.Publish(It.IsAny<SetSelectedItemInExplorerTree>()), Times.Never());
            mockEventAggregator.Verify(aggregator => aggregator.Publish(It.IsAny<SetActiveEnvironmentMessage>()), Times.Never());
            mockEventAggregator.Verify(aggregator => aggregator.Publish(It.IsAny<ServerSelectionChangedMessage>()), Times.Once());
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ConnectControl_SelectionChanged")]
        public void ConnectControlViewModel_SelectionChanged_SettingSelectedItemToAlreadySelectedItem_ShouldNotFireMessages()
        {
            //------------Setup for test--------------------------
            var testEnvRepo = new TestEnvironmentRespository(new Mock<IEnvironmentModel>().Object);
            new EnvironmentRepository(testEnvRepo);
            var localhostServer = CreateServer("localhost", true);
            var remoteServer = CreateServer("remote", false);
            var otherServer = CreateServer("disconnected", false);
            var mockEventAggregator = new Mock<IEventAggregator>();
            mockEventAggregator.Setup(aggregator => aggregator.Publish(It.IsAny<SetSelectedItemInExplorerTree>())).Verifiable();
            mockEventAggregator.Setup(aggregator => aggregator.Publish(It.IsAny<SetActiveEnvironmentMessage>())).Verifiable();
            mockEventAggregator.Setup(aggregator => aggregator.Publish(It.IsAny<ServerSelectionChangedMessage>())).Verifiable();
            var connectControlViewModel = new ConnectControlViewModel(localhostServer, mockEventAggregator.Object) { SelectedServer = remoteServer };
            connectControlViewModel.SelectedServerHasChanged(remoteServer, connectControlViewModel);
            mockEventAggregator.Verify(aggregator => aggregator.Publish(It.IsAny<SetSelectedItemInExplorerTree>()), Times.Once());
            mockEventAggregator.Verify(aggregator => aggregator.Publish(It.IsAny<ServerSelectionChangedMessage>()), Times.Once());
            var serverDtos = new List<IEnvironmentModel> { localhostServer, remoteServer, otherServer };
            var observableCollection = new ObservableCollection<IEnvironmentModel>(serverDtos);
            connectControlViewModel.Servers = observableCollection;
            //------------Execute Test---------------------------
            connectControlViewModel.SelectedServer = remoteServer;
            //------------Assert Results-------------------------
            mockEventAggregator.Verify(aggregator => aggregator.Publish(It.IsAny<SetSelectedItemInExplorerTree>()), Times.Once());
            mockEventAggregator.Verify(aggregator => aggregator.Publish(It.IsAny<ServerSelectionChangedMessage>()), Times.Once());
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ConnectControl_SelectionChanged")]
        public void ConnectControlViewModel_SelectionChanged_FromRemoteServerToLocalServerWithRemoteServerNotConnected_ShouldFireMessagesTwice()
        {
            //------------Setup for test--------------------------
            var testEnvRepo = new TestEnvironmentRespository(new Mock<IEnvironmentModel>().Object);
            new EnvironmentRepository(testEnvRepo);
            var localhostServer = CreateServer("localhost", true);
            var remoteServer = CreateServer("remote", false);
            var otherServer = CreateServer("disconnected", false);
            var mockEventAggregator = new Mock<IEventAggregator>();
            mockEventAggregator.Setup(aggregator => aggregator.Publish(It.IsAny<SetSelectedItemInExplorerTree>())).Verifiable();
            mockEventAggregator.Setup(aggregator => aggregator.Publish(It.IsAny<SetActiveEnvironmentMessage>())).Verifiable();
            mockEventAggregator.Setup(aggregator => aggregator.Publish(It.IsAny<ServerSelectionChangedMessage>())).Verifiable();
            var connectControlViewModel = new ConnectControlViewModel(localhostServer, mockEventAggregator.Object) { BindToActiveEnvironment = true };
            var serverDtos = new List<IEnvironmentModel> { localhostServer, remoteServer, otherServer };
            var observableCollection = new ObservableCollection<IEnvironmentModel>(serverDtos);
            connectControlViewModel.Servers = observableCollection;
            //------------Execute Test---------------------------
            connectControlViewModel.SelectedServer = remoteServer;
            connectControlViewModel.SelectedServerHasChanged(remoteServer, connectControlViewModel);
            connectControlViewModel.SelectedServer = localhostServer;
            connectControlViewModel.SelectedServerHasChanged(localhostServer, connectControlViewModel);
            //------------Assert Results-------------------------
            mockEventAggregator.Verify(aggregator => aggregator.Publish(It.IsAny<SetSelectedItemInExplorerTree>()), Times.Exactly(2));
            mockEventAggregator.Verify(aggregator => aggregator.Publish(It.IsAny<ServerSelectionChangedMessage>()), Times.Exactly(2));
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ConnectControl_HandleUpdateActiveEnvironmentMessage")]
        public void ConnectControlViewModel_HandleUpdateActiveEnvironmentMessageWhenBoundToActiveEnvironmentTrue_SelectedServerIsUpdated()
        {
            //------------Setup for test--------------------------
            var testEnvRepo = new TestEnvironmentRespository(new Mock<IEnvironmentModel>().Object);
            new EnvironmentRepository(testEnvRepo);
            var localhostServer = CreateServer("localhost", true);
            var remoteServer = CreateServer("remote", false);
            var otherServer = CreateServer("disconnected", false);
            var mockEventAggregator = new Mock<IEventAggregator>();
            var connectControlViewModel = new ConnectControlViewModel(localhostServer, mockEventAggregator.Object) { LabelText = "Connect", BindToActiveEnvironment = true };
            var serverDtos = new List<IEnvironmentModel> { localhostServer, remoteServer, otherServer };
            var observableCollection = new ObservableCollection<IEnvironmentModel>(serverDtos);
            connectControlViewModel.Servers = observableCollection;
            connectControlViewModel.SelectedServer = localhostServer;
            var updateSelectedServerMessage = new UpdateActiveEnvironmentMessage(remoteServer);
            //------------Execute Test---------------------------
            connectControlViewModel.Handle(updateSelectedServerMessage);
            //------------Assert Results-------------------------
            Assert.AreEqual(remoteServer, connectControlViewModel.SelectedServer);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ConnectControl_HandleUpdateActiveEnvironmentMessage")]
        public void ConnectControlViewModel_HandleUpdateActiveEnvironmentMessageWhenBoundToActiveEnvironmentFalse_SelectedServerIsNotUpdated()
        {
            //------------Setup for test--------------------------
            var testEnvRepo = new TestEnvironmentRespository(new Mock<IEnvironmentModel>().Object);
            new EnvironmentRepository(testEnvRepo);
            var localhostServer = CreateServer("localhost", true);
            var remoteServer = CreateServer("remote", false);
            var otherServer = CreateServer("disconnected", false);
            var mockEventAggregator = new Mock<IEventAggregator>();
            var connectControlViewModel = new ConnectControlViewModel(localhostServer, mockEventAggregator.Object) { LabelText = "Connect", BindToActiveEnvironment = false };
            var serverDtos = new List<IEnvironmentModel> { localhostServer, remoteServer, otherServer };
            var observableCollection = new ObservableCollection<IEnvironmentModel>(serverDtos);
            connectControlViewModel.Servers = observableCollection;
            connectControlViewModel.SelectedServer = localhostServer;
            var updateSelectedServerMessage = new UpdateActiveEnvironmentMessage(remoteServer);
            //------------Execute Test---------------------------
            connectControlViewModel.Handle(updateSelectedServerMessage);
            //------------Assert Results-------------------------
            Assert.AreEqual(localhostServer, connectControlViewModel.SelectedServer);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ConnectControl_HandleSetConnectControlSelectedServerMessage")]
        public void ConnectControlViewModel_SetConnectControlSelectedServerMessage_TypeIsCorrect_SetSelectedServer()
        {
            //------------Setup for test--------------------------
            var testEnvRepo = new TestEnvironmentRespository(new Mock<IEnvironmentModel>().Object);
            new EnvironmentRepository(testEnvRepo);
            var localhostServer = CreateServer("localhost", true);
            var remoteServer = CreateServer("remote", false);
            var otherServer = CreateServer("disconnected", false);
            var mockEventAggregator = new Mock<IEventAggregator>();
            var connectControlViewModel = new ConnectControlViewModel(localhostServer, mockEventAggregator.Object) { LabelText = "Connect", BindToActiveEnvironment = false };
            var serverDtos = new List<IEnvironmentModel> { localhostServer, remoteServer, otherServer };
            var observableCollection = new ObservableCollection<IEnvironmentModel>(serverDtos);
            connectControlViewModel.Servers = observableCollection;
            connectControlViewModel.SelectedServer = localhostServer;
            connectControlViewModel.ConnectControlInstanceType = ConnectControlInstanceType.Settings;
            var updateSelectedServerMessage = new SetConnectControlSelectedServerMessage(remoteServer, ConnectControlInstanceType.Settings);
            //------------Execute Test---------------------------
            connectControlViewModel.Handle(updateSelectedServerMessage);
            //------------Assert Results-------------------------
            Assert.AreEqual(remoteServer, connectControlViewModel.SelectedServer);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ConnectControl_HandleSetConnectControlSelectedServerMessage")]
        public void ConnectControlViewModel_SetConnectControlSelectedServerMessage_TypeIsNotCorrect_DontSetSelectedServer()
        {
            //------------Setup for test--------------------------
            var testEnvRepo = new TestEnvironmentRespository(new Mock<IEnvironmentModel>().Object);
            new EnvironmentRepository(testEnvRepo);
            var localhostServer = CreateServer("localhost", true);
            var remoteServer = CreateServer("remote", false);
            var otherServer = CreateServer("disconnected", false);
            var mockEventAggregator = new Mock<IEventAggregator>();
            var connectControlViewModel = new ConnectControlViewModel(localhostServer, mockEventAggregator.Object) { LabelText = "Connect", BindToActiveEnvironment = false };
            var serverDtos = new List<IEnvironmentModel> { localhostServer, remoteServer, otherServer };
            var observableCollection = new ObservableCollection<IEnvironmentModel>(serverDtos);
            connectControlViewModel.Servers = observableCollection;
            connectControlViewModel.SelectedServer = localhostServer;
            connectControlViewModel.ConnectControlInstanceType = ConnectControlInstanceType.Settings;
            var updateSelectedServerMessage = new SetConnectControlSelectedServerMessage(remoteServer, ConnectControlInstanceType.Explorer);
            //------------Execute Test---------------------------
            connectControlViewModel.Handle(updateSelectedServerMessage);
            //------------Assert Results-------------------------
            Assert.AreEqual(localhostServer, connectControlViewModel.SelectedServer);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ConnectControl_Connect")]
        public void ConnectControlViewModel_ConnectWithDisconnectedEnvironment_ConnectIsHit()
        {
            //------------Setup for test--------------------------
            var testEnvRepo = new TestEnvironmentRespository(new Mock<IEnvironmentModel>().Object);
            new EnvironmentRepository(testEnvRepo);

            var localhostServer = CreateMockEnvironmentModel("localhost", true);
            var remoteServer = CreateMockEnvironmentModel("remote", false);
            var otherServer = CreateMockEnvironmentModel("disconnected", false);
            var mockEventAggregator = new Mock<IEventAggregator>();
            var connectControlViewModel = new ConnectControlViewModel(localhostServer.Object, mockEventAggregator.Object, new TestAsyncWorker()) { LabelText = "Connect", BindToActiveEnvironment = false };
            var serverDtos = new List<IEnvironmentModel> { localhostServer.Object, remoteServer.Object, otherServer.Object };
            var observableCollection = new ObservableCollection<IEnvironmentModel>(serverDtos);
            connectControlViewModel.Servers = observableCollection;
            connectControlViewModel.SelectedServer = remoteServer.Object;
            //------------Execute Test---------------------------
            connectControlViewModel.ConnectCommand.Execute(null);
            //------------Assert Results-------------------------
            remoteServer.Verify(e => e.Connect(), Times.Once());
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ConnectControl_Connect")]
        public void ConnectControlViewModel_ConnectWithConnectedEnvironment_DisconnectIsHit()
        {
            //------------Setup for test--------------------------
            var testEnvRepo = new TestEnvironmentRespository(new Mock<IEnvironmentModel>().Object);
            new EnvironmentRepository(testEnvRepo);
            var localhostServer = CreateMockEnvironmentModel("localhost", true);
            var remoteServer = CreateMockEnvironmentModel("remote", true);
            var otherServer = CreateMockEnvironmentModel("disconnected", false);
            var mockEventAggregator = new Mock<IEventAggregator>();
            var connectControlViewModel = new ConnectControlViewModel(localhostServer.Object, mockEventAggregator.Object, new TestAsyncWorker()) { LabelText = "Connect", BindToActiveEnvironment = false };
            var serverDtos = new List<IEnvironmentModel> { localhostServer.Object, remoteServer.Object, otherServer.Object };
            var observableCollection = new ObservableCollection<IEnvironmentModel>(serverDtos);
            connectControlViewModel.Servers = observableCollection;
            connectControlViewModel.SelectedServer = remoteServer.Object;
            //------------Execute Test---------------------------
            connectControlViewModel.ConnectCommand.Execute(null);
            //------------Assert Results-------------------------
            remoteServer.Verify(e => e.Disconnect(), Times.Once());
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ConnectControl_EditConnection")]
        public void ConnectControlViewModel_EditConnection_EnvironmentReloaded()
        {
            //------------Setup for test--------------------------
            var testEnvRepo = new TestEnvironmentRespository(new Mock<IEnvironmentModel>().Object);
            new EnvironmentRepository(testEnvRepo);
            var localhostServer = CreateMockEnvironmentModel("localhost", true);
            var remoteServer = CreateMockEnvironmentModel("remote", false);
            var otherServer = CreateMockEnvironmentModel("disconnected", false);
            var mockEventAggregator = new Mock<IEventAggregator>();
            var connectControlViewModel = new TestConnectControlViewModel(localhostServer.Object, mockEventAggregator.Object) { LabelText = "Connect", BindToActiveEnvironment = false };
            var serverDtos = new List<IEnvironmentModel> { localhostServer.Object, remoteServer.Object, otherServer.Object };
            var mockEnvironmentRepository = new TestEnvironmentRespository(localhostServer.Object, remoteServer.Object, otherServer.Object);
            // ReSharper disable ObjectCreationAsStatement
            new EnvironmentRepository(mockEnvironmentRepository);
            // ReSharper restore ObjectCreationAsStatement
            var observableCollection = new ObservableCollection<IEnvironmentModel>(serverDtos);
            connectControlViewModel.Servers = observableCollection;
            connectControlViewModel.SelectedServer = localhostServer.Object;
            //------------Execute Test---------------------------
            connectControlViewModel.EditCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.AreEqual(1, connectControlViewModel.openEditConnectionWizardHitCount);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ConnectControlViewModel_AddMissingServers")]
        public void ConnectControlViewModel_AddMissingServers_WhenEnvironmentRepositoryItemAdded_ServerIsAddedToServerCollection()
        {
            //------------Setup for test--------------------------
            var mockEventAggregator = new Mock<IEventAggregator>();
            var testEnvRepo = new TestEnvironmentRespository(new Mock<IEnvironmentModel>().Object);
            new EnvironmentRepository(testEnvRepo);
            var localhostServer = CreateMockEnvironmentModel("localhost", true);
            var remoteServer = CreateMockEnvironmentModel("remote", false);
            var connectControlViewModel = new TestConnectControlViewModel(localhostServer.Object, mockEventAggregator.Object) { SelectedServer = localhostServer.Object };
            //------------PreConditions--------------------------
            Assert.AreEqual(1, connectControlViewModel.Servers.Count);
            Assert.AreEqual(localhostServer.Object, connectControlViewModel.SelectedServer);
            //------------Execute Test---------------------------
            EnvironmentRepository.Instance.Save(remoteServer.Object);
            //------------Assert Results-------------------------
            Assert.AreEqual(3, connectControlViewModel.Servers.Count); //The New Remote Server is also in the list
            Assert.AreEqual(localhostServer.Object, connectControlViewModel.SelectedServer);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ConnectControlViewModel_AddMissingServers")]
        public void ConnectControlViewModel_AddMissingServers_WhenBindToActiveEnvironment_ServerIsSelected()
        {
            //------------Setup for test--------------------------
            var mockEventAggregator = new Mock<IEventAggregator>();
            var testEnvRepo = new TestEnvironmentRespository(new Mock<IEnvironmentModel>().Object);
            new EnvironmentRepository(testEnvRepo);
            var localhostServer = CreateMockEnvironmentModel("localhost", true);
            var remoteServer = CreateMockEnvironmentModel("remote", false);
            var connectControlViewModel = new TestConnectControlViewModel(localhostServer.Object, mockEventAggregator.Object) { SelectedServer = localhostServer.Object, BindToActiveEnvironment = true };
            //------------PreConditions--------------------------
            Assert.AreEqual(1, connectControlViewModel.Servers.Count);
            Assert.AreEqual(localhostServer.Object, connectControlViewModel.SelectedServer);
            //------------Execute Test---------------------------
            EnvironmentRepository.Instance.Save(remoteServer.Object);
            //------------Assert Results-------------------------
            Assert.AreEqual(3, connectControlViewModel.Servers.Count); //The New Remote Server is also in the list
            Assert.AreEqual(remoteServer.Object, connectControlViewModel.SelectedServer);
        }

        #region Testing Methods
        public static ImportServiceContext EnviromentRepositoryImportServiceContext;
        static void SetupMef()
        {

            var eventAggregator = new Mock<IEventAggregator>();

            var importServiceContext = new ImportServiceContext();
            ImportService.CurrentContext = importServiceContext;
            ImportService.Initialize(new List<ComposablePartCatalog>());
            ImportService.AddExportedValueToContainer(eventAggregator.Object);

            EnviromentRepositoryImportServiceContext = importServiceContext;
        }


        static Mock<IEnvironmentModel> CreateMockEnvironmentModel(string name, bool isConnected)
        {
            var isLocalhost = name == "localhost";

            var env = new Mock<IEnvironmentModel>();
            env.SetupProperty(e => e.Name, name);
            env.Setup(e => e.ID).Returns(isLocalhost ? Guid.Empty : Guid.NewGuid());
            env.Setup(e => e.IsConnected).Returns(isConnected);
            env.Setup(e => e.IsLocalHost).Returns(isLocalhost);
            env.Setup(e => e.Connect()).Verifiable();
            env.Setup(e => e.Disconnect()).Verifiable();

            return env;
        }

        static IEnvironmentModel CreateServer(string name, bool isConnected)
        {
            var isLocalhost = name == "localhost";

            var env = new Mock<IEnvironmentModel>();
            env.SetupProperty(e => e.Name, name);
            env.Setup(e => e.ID).Returns(isLocalhost ? Guid.Empty : Guid.NewGuid());
            env.Setup(e => e.IsConnected).Returns(isConnected);
            env.Setup(e => e.IsLocalHost).Returns(isLocalhost);

            return env.Object;
        }
        #endregion
    }

    internal class TestConnectControlViewModel : ConnectControlViewModel
    {
        public int openNewConnectionWizardHitCount = 0;
        public int openEditConnectionWizardHitCount = 0;

        public TestConnectControlViewModel(IEnvironmentModel activeEnvironment, IEventAggregator eventAggregator)
            : base(activeEnvironment, eventAggregator)
        {
        }

        #region Overrides of ConnectControlViewModel

        public override void OpenNewConnectionWizard(IEnvironmentModel localHost)
        {
            openNewConnectionWizardHitCount++;
        }

        public override void OpenEditConnectionWizard(IEnvironmentModel localHost)
        {
            openEditConnectionWizardHitCount++;
        }

        #endregion

    }
}
