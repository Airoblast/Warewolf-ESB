﻿using Caliburn.Micro;
using Dev2.Composition;
using Dev2.Diagnostics;
using Dev2.Providers.Events;
using Dev2.Services.Events;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Controller;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Workspaces;
using Dev2.Studio.Diagnostics;
using Dev2.Studio.Feedback;
using Dev2.Studio.ViewModels.Diagnostics;
using Dev2.Studio.Webs;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Dev2.Core.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public partial class DebugOutputViewModelTest
    {
        static Mock<IResourceRepository> _resourceRepo = new Mock<IResourceRepository>();
        private static Mock<IEnvironmentModel> _environmentModel;
        private static IEnvironmentRepository _environmentRepo;
        private static Mock<IWorkspaceItemRepository> _mockWorkspaceRepo;
        private static Mock<IContextualResourceModel> _firstResource;
        private static string _resourceName = "TestResource";
        private static string _displayName = "test2";
        private static string _serviceDefinition = "<x/>";
        private static ImportServiceContext _importServiceContext;
        private static Guid _serverID = Guid.NewGuid();
        private static Guid _workspaceID = Guid.NewGuid();
        private static Guid _firstResourceID = Guid.NewGuid();
        private Guid _secondResourceID = Guid.NewGuid();
        public static Mock<IPopupController> _popupController;
        private static Mock<IFeedbackInvoker> _feedbackInvoker;
        private static Mock<IWebController> _webController;
        private static Mock<IWindowManager> _windowManager;

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            CreateFullExportsAndVm();
        }

        [TestMethod]
        public void DebugOutputViewModel_OpenNullLineItemDoesntStartProcess()
        {
            ImportService.CurrentContext = _importServiceContext;
            var vm = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, GetEnvironmentRepository());
            vm.OpenMoreLink(null);
            Assert.IsNull(vm.ProcessController);
        }

        [TestMethod]
        public void DebugOutputViewModel_OpenEmptyMoreLinkDoesntStartProcess()
        {
            ImportService.CurrentContext = _importServiceContext;
            var vm = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, GetEnvironmentRepository());

            var lineItem = new Mock<IDebugLineItem>();
            lineItem.SetupGet(l => l.MoreLink).Returns("");

            vm.OpenMoreLink(lineItem.Object);
            Assert.IsNull(vm.ProcessController);
        }

        [TestMethod]
        public void DebugOutputViewModel_CanOpenNonNullOrEmptyMoreLinkLineItem()
        {
            ImportService.CurrentContext = _importServiceContext;

            var lineItem = new Mock<IDebugLineItem>();
            lineItem.SetupGet(l => l.MoreLink).Returns("More");

            var vm = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, GetEnvironmentRepository());

            Assert.IsTrue(vm.CanOpenMoreLink(lineItem.Object).Equals(true));
        }

        [TestMethod]
        public void DebugOutputViewModel_CantOpenEmptyMoreLinkLineItem()
        {
            ImportService.CurrentContext = _importServiceContext;
            var vm = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, GetEnvironmentRepository());

            var lineItem = new Mock<IDebugLineItem>();
            lineItem.SetupGet(l => l.MoreLink).Returns("");
            Assert.IsTrue(vm.CanOpenMoreLink(lineItem.Object).Equals(false));
        }

        [TestMethod]
        public void DebugOutputViewModel_CantOpenNullMoreLinkLineItem()
        {
            ImportService.CurrentContext = _importServiceContext;
            var vm = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, GetEnvironmentRepository());

            Assert.IsTrue(vm.CanOpenMoreLink(null).Equals(false));
        }


        [TestMethod]
        public void DebugOutputViewModel_AppendErrorExpectErrorMessageAppended()
        {
            ImportService.CurrentContext = _importServiceContext;

            var mock1 = new Mock<IDebugState>();
            var mock2 = new Mock<IDebugState>();
            mock1.SetupGet(m => m.ID).Returns(_firstResourceID);
            mock1.SetupGet(m => m.ServerID).Returns(_serverID);
            mock1.SetupGet(m => m.WorkspaceID).Returns(_workspaceID);

            mock2.SetupGet(m => m.ServerID).Returns(_serverID);
            mock2.SetupGet(m => m.WorkspaceID).Returns(_workspaceID);
            mock2.SetupGet(m => m.ParentID).Returns(_firstResourceID);
            mock2.SetupGet(m => m.StateType).Returns(StateType.Append);
            mock2.SetupGet(m => m.HasError).Returns(true);
            mock2.SetupGet(m => m.ErrorMessage).Returns("Error Test");

            mock1.SetupSet(s => s.ErrorMessage = It.IsAny<string>()).Callback<string>(s => Assert.IsTrue(s.Equals("Error Test")));
            mock1.SetupSet(s => s.HasError = It.IsAny<bool>()).Callback<bool>(s => Assert.IsTrue(s.Equals(true)));

            var vm = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, GetEnvironmentRepository());

            mock1.Setup(m => m.SessionID).Returns(vm.SessionID);
            mock2.Setup(m => m.SessionID).Returns(vm.SessionID);

            vm.Append(mock1.Object);
            vm.Append(mock2.Object);

            Assert.AreEqual(1, vm.RootItems.Count);
            var root = vm.RootItems.First() as DebugStateTreeViewItemViewModel;
            Assert.IsNotNull(root);
            Assert.IsTrue(root.HasError.GetValueOrDefault(false));
        }

        [TestMethod]
        public void DebugOutputViewModel_AppendNestedDebugstatesExpectNestedInRootItems()
        {
            ImportService.CurrentContext = _importServiceContext;

            var mock1 = new Mock<IDebugState>();
            var mock2 = new Mock<IDebugState>();
            mock1.SetupGet(m => m.ID).Returns(_firstResourceID);
            mock1.SetupGet(m => m.ServerID).Returns(_serverID);
            mock1.SetupGet(m => m.WorkspaceID).Returns(_workspaceID);

            mock2.SetupGet(m => m.ID).Returns(Guid.NewGuid());
            mock2.SetupGet(m => m.ServerID).Returns(_serverID);
            mock2.SetupGet(m => m.WorkspaceID).Returns(_workspaceID);
            mock2.SetupGet(m => m.ParentID).Returns(_firstResourceID);

            var vm = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, GetEnvironmentRepository());

            mock1.Setup(m => m.SessionID).Returns(vm.SessionID);
            mock2.Setup(m => m.SessionID).Returns(vm.SessionID);

            vm.Append(mock1.Object);
            vm.Append(mock2.Object);
            Assert.AreEqual(1, vm.RootItems.Count);
            var root = vm.RootItems.First() as DebugStateTreeViewItemViewModel;
            Assert.IsNotNull(root);
            Assert.AreEqual(root.Content, mock1.Object, "Root item incorrectly appended");

            var firstChild = root.Children.First() as DebugStateTreeViewItemViewModel;
            Assert.IsNotNull(firstChild);
            Assert.IsTrue(firstChild.Content.ParentID.Equals(_firstResourceID));
        }

        [TestMethod]
        public void DebugOutputViewModel_AppendWhenDebugStateStoppedShouldNotWriteItems()
        {
            ImportService.CurrentContext = _importServiceContext;

            var mock1 = new Mock<IDebugState>();
            var mock2 = new Mock<IDebugState>();
            mock1.SetupGet(m => m.ID).Returns(_firstResourceID);
            mock1.SetupGet(m => m.ServerID).Returns(_serverID);
            mock1.SetupGet(m => m.WorkspaceID).Returns(_workspaceID);

            mock2.SetupGet(m => m.ServerID).Returns(_serverID);
            mock2.SetupGet(m => m.WorkspaceID).Returns(_workspaceID);
            mock2.SetupGet(m => m.ParentID).Returns(_firstResourceID);

            var vm = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, GetEnvironmentRepository());
            vm.DebugStatus = DebugStatus.Stopping;
            vm.Append(mock1.Object);
            vm.Append(mock2.Object);
            Assert.AreEqual(0, vm.RootItems.Count);
        }

        [TestMethod]
        public void DebugOutputViewModel_AppendWhenDebugStateFinishedShouldNotWriteItems()
        {
            ImportService.CurrentContext = _importServiceContext;

            var mock1 = new Mock<IDebugState>();
            var mock2 = new Mock<IDebugState>();
            mock1.SetupGet(m => m.ID).Returns(_firstResourceID);
            mock1.SetupGet(m => m.ServerID).Returns(_serverID);
            mock1.SetupGet(m => m.WorkspaceID).Returns(_workspaceID);

            mock2.SetupGet(m => m.ServerID).Returns(_serverID);
            mock2.SetupGet(m => m.WorkspaceID).Returns(_workspaceID);
            mock2.SetupGet(m => m.ParentID).Returns(_firstResourceID);

            var vm = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, GetEnvironmentRepository());
            vm.DebugStatus = DebugStatus.Finished;
            vm.Append(mock1.Object);
            vm.Append(mock2.Object);
            Assert.AreEqual(0, vm.RootItems.Count);
        }

        [TestMethod]
        [TestCategory("DebugOutputViewModel_AppendItem")]
        [Description("DebugOutputViewModel AppendItem must not append item when DebugStatus is finished.")]
        [Owner("Jurie Smit")]
        public void DebugOutputViewModel_AppendWhenDebugStateFinishedShouldNotWriteItems_ItemIsMessage()
        {
            ImportService.CurrentContext = _importServiceContext;

            var mock1 = new Mock<IDebugState>();
            var mock2 = new Mock<IDebugState>();
            mock1.SetupGet(m => m.ID).Returns(_firstResourceID);
            mock1.SetupGet(m => m.ServerID).Returns(_serverID);
            mock1.SetupGet(m => m.WorkspaceID).Returns(_workspaceID);
            mock1.SetupGet(m => m.StateType).Returns(StateType.Message);
            mock2.SetupGet(m => m.ServerID).Returns(_serverID);
            mock2.SetupGet(m => m.WorkspaceID).Returns(_workspaceID);
            mock2.SetupGet(m => m.ParentID).Returns(_firstResourceID);

            var vm = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, GetEnvironmentRepository());
            mock1.Setup(m => m.SessionID).Returns(vm.SessionID);
            mock2.Setup(m => m.SessionID).Returns(vm.SessionID);
            vm.DebugStatus = DebugStatus.Finished;
            vm.Append(mock1.Object);
            vm.Append(mock2.Object);
            Assert.AreEqual(1, vm.RootItems.Count);

            var root = vm.RootItems[0] as DebugStringTreeViewItemViewModel;

            Assert.IsNotNull(root);
        }

        [TestMethod]
        [TestCategory("DebugOutputViewModel_OpenItem")]
        [Description("DebugOutputViewModel OpenItem must set the DebugState's properties.")]
        [Owner("Trevor Williams-Ros")]
        public void DebugOutputViewModel_OpenItemWithRemoteEnvironment_OpensResourceFromRemoteEnvironment()
        {
            ImportService.CurrentContext = _importServiceContext;

            const string ResourceName = "TestResource";
            var environmentID = Guid.NewGuid();

            var envList = new List<IEnvironmentModel>();
            var envRepository = new Mock<IEnvironmentRepository>();
            envRepository.Setup(e => e.All()).Returns(envList);

            var env = new Mock<IEnvironmentModel>();
            env.Setup(e => e.ID).Returns(environmentID);
            env.Setup(e => e.IsConnected).Returns(true);

            // If we get here then we've found the environment based on the environment ID!
            env.Setup(e => e.ResourceRepository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(),false)).Verifiable();

            envList.Add(env.Object);

            var model = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, envRepository.Object);

            var debugState = new DebugState
            {
                ActivityType = ActivityType.Workflow,
                DisplayName = ResourceName,
                EnvironmentID = environmentID
            };

            model.OpenItemCommand.Execute(debugState);

            env.Verify(e => e.ResourceRepository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("DebugOutputViewModel_OpenItem")]
        public void DebugOutputViewModel_OpenItem_NullEnvironmentIDInDebugState_NoUnhandledExceptions()
        {
            //------------Execute Test---------------------------
            var envRepo = GetEnvironmentRepository();
            new DebugOutputViewModel(new Mock<IEventPublisher>().Object, envRepo).OpenItemCommand.Execute(new DebugState { ActivityType = ActivityType.Workflow });
            new DebugOutputViewModel(new Mock<IEventPublisher>().Object, envRepo).OpenItemCommand.Execute(null);

            // Assert NoUnhandledExceptions
            Assert.IsTrue(true, "There where unhandled exceptions");
        }

        [TestMethod]
        [TestCategory("DebugOutputViewModel_AppendItem")]
        [Description("DebugOutputViewModel appendItem must set Debugstatus to finished when DebugItem is final step.")]
        [Owner("Jurie Smit")]
        public void DebugOutputViewModel_AppendItemFinalStep_DebugStatusFinished()
        {
            //*********************Setup********************
            ImportService.CurrentContext = _importServiceContext;
            var mock1 = new Mock<IDebugState>();
            mock1.Setup(m => m.IsFinalStep()).Returns(true);
            var vm = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, GetEnvironmentRepository());
            mock1.Setup(m => m.SessionID).Returns(vm.SessionID);
            vm.DebugStatus = DebugStatus.Ready;

            //*********************Test********************
            vm.Append(mock1.Object);

            //*********************Assert*******************
            Assert.AreEqual(vm.DebugStatus, DebugStatus.Finished);
        }

        [TestMethod]
        [TestCategory("DebugOutputViewModel_AppendItem")]
        [Description("DebugOutputViewModel appendItem must set Debugstatus to finished when DebugItem is final step.")]
        [Owner("Jurie Smit")]
        public void DebugOutputViewModel_AppendItemNotFinalStep_DebugStatusUnchanced()
        {
            //*********************Setup********************
            ImportService.CurrentContext = _importServiceContext;
            var mock1 = new Mock<IDebugState>();
            mock1.Setup(m => m.IsFinalStep()).Returns(false);
            var vm = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, GetEnvironmentRepository());
            vm.DebugStatus = DebugStatus.Ready;

            //*********************Test********************
            vm.Append(mock1.Object);

            //*********************Assert*******************
            Assert.AreEqual(vm.DebugStatus, DebugStatus.Ready);
        }

        #region PendingQueue

        // BUG 9735 - 2013.06.22 - TWR : added
        [TestMethod]
        public void DebugOutputViewModel_PendingQueueExpectedQueuesMessagesAndFlushesWhenFinishedProcessing()
        {
            ImportService.CurrentContext = _importServiceContext;

            var envRepo = GetEnvironmentRepository();

            var vm = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, envRepo) { DebugStatus = DebugStatus.Executing };
            for(var i = 0; i < 10; i++)
            {
                var state = new Mock<IDebugState>();
                var stateType = i % 2 == 0 ? StateType.Message : StateType.After;
                state.Setup(s => s.StateType).Returns(stateType);
                state.Setup(s => s.SessionID).Returns(vm.SessionID);
                vm.Append(state.Object);
            }

            Assert.AreEqual(5, vm.PendingItemCount);
            Assert.AreEqual(5, vm.ContentItemCount);

            vm.DebugStatus = DebugStatus.Finished;

            Assert.AreEqual(0, vm.PendingItemCount);
            Assert.AreEqual(10, vm.ContentItemCount);
        }

        #endregion

        static void CreateFullExportsAndVm()
        {
            CreateEnvironmentModel();
            _environmentRepo = GetEnvironmentRepository();
            _popupController = new Mock<IPopupController>();
            _feedbackInvoker = new Mock<IFeedbackInvoker>();
            _webController = new Mock<IWebController>();
            _windowManager = new Mock<IWindowManager>();

            Mock<IWorkspaceItemRepository> mockWorkspaceItemRepository = GetworkspaceItemRespository();

            _importServiceContext =
                CompositionInitializer.InitializeMockedMainViewModel(environmentRepo: _environmentRepo,
                                                                         workspaceItemRepository: mockWorkspaceItemRepository.Object,
                //aggregator: _eventAggregator,
                                                                     popupController: _popupController,
                                                                     feedbackInvoker: _feedbackInvoker,
                                                                     webController: _webController,
                                                                     windowManager: _windowManager);

            ImportService.CurrentContext = _importServiceContext;
        }

        public static Mock<IContextualResourceModel> CreateResource(ResourceType resourceType)
        {
            var result = new Mock<IContextualResourceModel>();

            result.Setup(c => c.ResourceName).Returns(_resourceName);
            result.Setup(c => c.ResourceType).Returns(resourceType);
            result.Setup(c => c.DisplayName).Returns(_displayName);
            result.Setup(c => c.WorkflowXaml).Returns(new StringBuilder(_serviceDefinition));
            result.Setup(c => c.Category).Returns("Testing");
            result.Setup(c => c.Environment).Returns(_environmentModel.Object);
            result.Setup(c => c.ServerID).Returns(_serverID);
            result.Setup(c => c.ID).Returns(_firstResourceID);

            return result;
        }

        public static IEnvironmentRepository GetEnvironmentRepository()
        {
            var models = new List<IEnvironmentModel> { _environmentModel.Object };
            var mock = new Mock<IEnvironmentRepository>();
            mock.Setup(s => s.All()).Returns(models);
            mock.Setup(s => s.IsLoaded).Returns(true);
            mock.Setup(repository => repository.Source).Returns(_environmentModel.Object);
            _environmentRepo = mock.Object;
            return _environmentRepo;
        }

        static void CreateEnvironmentModel()
        {
            _environmentModel = CreateMockEnvironment();

            _resourceRepo = new Mock<IResourceRepository>();

            _firstResource = CreateResource(ResourceType.WorkflowService);
            var coll = new Collection<IResourceModel> { _firstResource.Object };
            _resourceRepo.Setup(c => c.All()).Returns(coll);


            _environmentModel.Setup(m => m.ResourceRepository).Returns(_resourceRepo.Object);
        }

        public static Mock<IEnvironmentModel> CreateMockEnvironment(params string[] sources)
        {
            var rand = new Random();
            var connection = CreateMockConnection(rand, sources);

            var env = new Mock<IEnvironmentModel>();
            env.Setup(e => e.Connection).Returns(connection.Object);
            env.Setup(e => e.IsConnected).Returns(true);
            env.Setup(e => e.ID).Returns(Guid.NewGuid());

            env.Setup(e => e.Name).Returns(string.Format("Server_{0}", rand.Next(1, 100)));

            return env;
        }

        public static Mock<IEnvironmentConnection> CreateMockConnection(Random rand, params string[] sources)
        {

            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(c => c.ServerID).Returns(new Guid());
            connection.Setup(c => c.AppServerUri).Returns(new Uri(string.Format("http://127.0.0.{0}:{1}/dsf", rand.Next(1, 100), rand.Next(1, 100))));
            connection.Setup(c => c.WebServerUri).Returns(new Uri(string.Format("http://127.0.0.{0}:{1}", rand.Next(1, 100), rand.Next(1, 100))));
            connection.Setup(c => c.IsConnected).Returns(true);
            connection.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new StringBuilder(string.Format("<XmlData>{0}</XmlData>", string.Join("\n", sources))));

            return connection;
        }

        public static Mock<IWorkspaceItemRepository> GetworkspaceItemRespository()
        {
            _mockWorkspaceRepo = new Mock<IWorkspaceItemRepository>();
            var list = new List<IWorkspaceItem>();
            var item = new Mock<IWorkspaceItem>();
            item.SetupGet(i => i.WorkspaceID).Returns(_workspaceID);
            item.SetupGet(i => i.ServerID).Returns(_serverID);
            item.SetupGet(i => i.ServiceName).Returns(_resourceName);
            list.Add(item.Object);
            _mockWorkspaceRepo.SetupGet(c => c.WorkspaceItems).Returns(list);
            _mockWorkspaceRepo.Setup(c => c.Remove(_firstResource.Object)).Verifiable();
            return _mockWorkspaceRepo;
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DebugOutputViewModel_IsStopping")]
        public void DebugOutputViewModel_IsStopping_ReflectsDebugStatus()
        {
            //------------Setup for test--------------------------
            var envRepo = GetEnvironmentRepository();
            var viewModel = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, envRepo);

            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            viewModel.DebugStatus = DebugStatus.Stopping;
            Assert.IsTrue(viewModel.IsStopping);

            viewModel.DebugStatus = DebugStatus.Ready;
            Assert.IsFalse(viewModel.IsStopping);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DebugOutputViewModel_IsProcessing")]
        public void DebugOutputViewModel_IsProcessing_ReflectsDebugStatus()
        {
            //------------Setup for test--------------------------
            var envRepo = GetEnvironmentRepository();
            var viewModel = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, envRepo);

            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            viewModel.DebugStatus = DebugStatus.Executing;
            Assert.IsTrue(viewModel.IsProcessing);

            viewModel.DebugStatus = DebugStatus.Configure;
            Assert.IsTrue(viewModel.IsProcessing);

            viewModel.DebugStatus = DebugStatus.Ready;
            Assert.IsFalse(viewModel.IsProcessing);

            viewModel.DebugStatus = DebugStatus.Finished;
            Assert.IsFalse(viewModel.IsProcessing);

            viewModel.DebugStatus = DebugStatus.Stopping;
            Assert.IsFalse(viewModel.IsProcessing);

        }


        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DebugOutputViewModel_DebugImage")]
        public void DebugOutputViewModel_DebugImage_ReflectsDebugStatus()
        {
            //------------Setup for test--------------------------
            var envRepo = GetEnvironmentRepository();
            var viewModel = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, envRepo);

            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            viewModel.DebugStatus = DebugStatus.Executing;
            Assert.AreEqual("pack://application:,,,/Warewolf Studio;component/Images/ExecuteDebugStop-32.png", viewModel.DebugImage);

            viewModel.DebugStatus = DebugStatus.Ready;
            Assert.AreEqual("pack://application:,,,/Warewolf Studio;component/Images/ExecuteDebugStart-32.png", viewModel.DebugImage);
        }


        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DebugOutputViewModel_DebugText")]
        public void DebugOutputViewModel_DebugText_ReflectsDebugStatus()
        {
            //------------Setup for test--------------------------
            var envRepo = GetEnvironmentRepository();
            var viewModel = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, envRepo);

            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            viewModel.DebugStatus = DebugStatus.Executing;
            Assert.AreEqual("Stop", viewModel.DebugText);

            viewModel.DebugStatus = DebugStatus.Ready;
            Assert.AreEqual("Debug", viewModel.DebugText);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DebugOutputViewModel_DebugStatus")]
        public void DebugOutputViewModel_DebugStatus_Executing_PublishesDebugSelectionChangedEventArgs()
        {
            //------------Setup for test--------------------------
            var envRepo = GetEnvironmentRepository();
            var viewModel = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, envRepo);


            var expectedProps = new[] { "IsStopping", "IsProcessing", "IsConfiguring", "DebugImage", "DebugText", "ProcessingText" };
            var actualProps = new List<string>();
            viewModel.PropertyChanged += (sender, args) => actualProps.Add(args.PropertyName);

            var events = new List<DebugSelectionChangedEventArgs>();

            var selectionChangedEvents = EventPublishers.Studio.GetEvent<DebugSelectionChangedEventArgs>();
            selectionChangedEvents.Subscribe(events.Add);

            //------------Execute Test---------------------------
            viewModel.DebugStatus = DebugStatus.Executing;

            EventPublishers.Studio.RemoveEvent<DebugSelectionChangedEventArgs>();

            //------------Assert Results-------------------------
            Assert.AreEqual(1, events.Count);
            Assert.AreEqual(ActivitySelectionType.None, events[0].SelectionType);
            Assert.IsNull(events[0].DebugState);

            CollectionAssert.AreEqual(expectedProps, actualProps);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DebugOutputViewModel_SelectAll")]
        public void DebugOutputViewModel_SelectAll_Execute_PublishesClearSelection()
        {
            //------------Setup for test--------------------------
            var envRepo = GetEnvironmentRepository();
            var viewModel = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, envRepo);

            var events = new List<DebugSelectionChangedEventArgs>();

            var selectionChangedEvents = EventPublishers.Studio.GetEvent<DebugSelectionChangedEventArgs>();
            selectionChangedEvents.Subscribe(events.Add);

            //------------Execute Test---------------------------
            viewModel.SelectAllCommand.Execute(null);

            EventPublishers.Studio.RemoveEvent<DebugSelectionChangedEventArgs>();

            //------------Assert Results-------------------------
            Assert.AreEqual(1, events.Count);
            Assert.AreEqual(ActivitySelectionType.None, events[0].SelectionType);
            Assert.IsNull(events[0].DebugState);
        }


        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DebugOutputViewModel_SelectAll")]
        public void DebugOutputViewModel_SelectAll_Execute_AllDebugStateItemsSelected()
        {
            //------------Setup for test--------------------------
            var envRepo = GetEnvironmentRepository();
            var viewModel = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, envRepo);

            viewModel.RootItems.Add(CreateItemViewModel<DebugStringTreeViewItemViewModel>(envRepo, 0, isSelected: false, parent: null));

            for(var i = 1; i < 6; i++)
            {
                var item = CreateItemViewModel<DebugStateTreeViewItemViewModel>(envRepo, i, isSelected: false, parent: null);
                for(var j = 0; j < 2; j++)
                {
                    CreateItemViewModel<DebugStateTreeViewItemViewModel>(envRepo, j, isSelected: false, parent: item);
                }
                viewModel.RootItems.Add(item);
            }

            //------------Execute Test---------------------------
            viewModel.SelectAllCommand.Execute(null);


            //------------Assert Results-------------------------

            // Items are selected only if they are DebugStateTreeViewItemViewModel's
            foreach(var item in viewModel.RootItems)
            {
                Assert.AreEqual(item is DebugStateTreeViewItemViewModel, item.IsSelected);
                foreach(var child in item.Children)
                {
                    Assert.AreEqual(child is DebugStateTreeViewItemViewModel, child.IsSelected);
                }
            }
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("DebugOutputViewModel_Constructor")]
        public void DebugOutputViewModel_Constructor_ViewModelSessionIDAndEnvironmentRepoProperlyInitialized()
        {
            var mockedEnvRepo = new Mock<IEnvironmentRepository>();

            //------------Execute Test---------------------------
            var debugOutputViewModel = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, mockedEnvRepo.Object);

            // Assert View Model Session ID And Environment Repo Properly Initialized
            Assert.AreNotEqual(Guid.Empty, debugOutputViewModel.SessionID, "Session ID not properly initialized");
            Assert.AreEqual(mockedEnvRepo.Object, debugOutputViewModel.EnvironmentRepository, "Environment Repo not initialized");
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DebugOutputViewModel_ProcessingText")]
        public void DebugOutputViewModel_ProcessingText_ShouldReturnDescriptionOfDebugStatus()
        {
            //------------Setup for test--------------------------
            var mockedEnvRepo = new Mock<IEnvironmentRepository>();
            var debugOutputViewModel = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, mockedEnvRepo.Object);
            debugOutputViewModel.DebugStatus = DebugStatus.Finished;
            //------------Execute Test---------------------------
            string processingText = debugOutputViewModel.ProcessingText;
            //------------Assert Results-------------------------
            Assert.AreEqual("Ready", processingText);
        }

        static IDebugTreeViewItemViewModel CreateItemViewModel<T>(IEnvironmentRepository envRepository, int n, bool isSelected, IDebugTreeViewItemViewModel parent)
            where T : IDebugTreeViewItemViewModel
        {
            if(typeof(T) == typeof(DebugStateTreeViewItemViewModel))
            {
                var content = new DebugState { DisplayName = "State " + n, ID = Guid.NewGuid(), ActivityType = ActivityType.Step };
                var viewModel = new DebugStateTreeViewItemViewModel(envRepository) { Parent = parent, Content = content };
                if(!isSelected)
                {
                    // viewModel.IsSelected is true when the ActivityType is Step
                    viewModel.IsSelected = false;
                }
                return viewModel;
            }

            return new DebugStringTreeViewItemViewModel
            {
                Content = "String " + n,
                IsSelected = isSelected
            };
        }


    }
}
