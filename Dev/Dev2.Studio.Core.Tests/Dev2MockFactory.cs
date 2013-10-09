﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Network;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Dev2.Core.Tests.Utils;
using Dev2.Data.Interfaces;
using Dev2.DataList.Contract;
using Dev2.Network;
using Dev2.Network.Execution;
using Dev2.Network.Messaging;
using Dev2.Providers.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Controller;
using Dev2.Studio.Core.Helpers;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Enums;
using Dev2.Studio.ViewModels;
using Dev2.Studio.ViewModels.Navigation;
using Dev2.Threading;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Action = System.Action;

namespace Dev2.Core.Tests
{
    public static class Dev2MockFactory
    {
        private static Mock<IMainViewModel> _mockIMainViewModel;
        private static Mock<MainViewModel> _mockMainViewModel;
        private static Mock<IEnvironmentModel> _mockEnvironmentModel;
        private static Mock<IContextualResourceModel> _mockResourceModel;
        private static Mock<IStudioClientContext> _mockFrameworkDataChannel;
        private static Mock<IFilePersistenceProvider> _mockFilePersistenceProvider;



        /*
         * Travis.Frisinger : 04-04-2012
         * 
         * The stuff for ILayoutObjectViewModel is ugly, but given that I need to return from a callback???
         * It was the easiest method I would find to correctly result the LayoutGrid without taking another day to get it right
         * http://stackoverflow.com/questions/2494930/settings-variable-values-in-a-moq-callback-call
         * sites a method of returning inline, but it does not take paramers into the callback method and I could not get the ordering right
         * hence this not-so-nice workaround
         * 
         */
        // required for IsSelected Property of LayoutGridObjectViewModel
        private static bool[,] isSelected = new bool[,] { { false, false, false, false }, { false, false, false, false }, { false, false, false, false }, { false, false, false, false } };
        private static ILayoutObjectViewModel _sillyMoqResult;
        private static ILayoutGridViewModel _model;


        public static void printIsSelected()
        {

            for(int i = 0; i < 4; i++)
            {
                for(int q = 0; q < 4; q++)
                {
                    Console.WriteLine(i + "," + q + " " + isSelected[i, q]);
                }
            }
        }

        public static ILayoutObjectViewModel FetchCallbackResult()
        {
            return _sillyMoqResult;
        }

        public static ILayoutObjectViewModel LayoutObjectFromMock(ILayoutGridViewModel grid, int row, int col)
        {
            _sillyMoqResult = LayoutObject(grid, row, col).Object;
            _model = grid;
            return _sillyMoqResult;
        }

        public static Mock<ILayoutObjectViewModel> LayoutObject(ILayoutGridViewModel grid, int row, int col)
        {
            Mock<ILayoutObjectViewModel> result = new Mock<ILayoutObjectViewModel>();

            result.SetupAllProperties();

            // Setup IsSelected Properties for LayoutGridObjectViewModel
            result.SetupGet(c => c.IsSelected).Returns(() => isSelected[row, col]);
            result.SetupSet(c => c.IsSelected = It.IsAny<bool>()).Callback<bool>(value => isSelected[row, col] = value);

            result.Setup(c => c.AddColumnLeftCommand);
            result.Setup(c => c.LayoutObjectGrid).Returns(grid);

            result.Setup(c => c.GridRow).Returns(row);
            result.Setup(c => c.GridColumn).Returns(col);
            result.Setup(c => c.LayoutObjectGrid).Returns(grid);
            result.Setup(c => c.CopyCommand);
            result.Setup(c => c.ClearAllCommand);

            return result;
        }

        static public Mock<IEnvironmentModel> EnvironmentModel
        {
            get
            {
                if(_mockEnvironmentModel == null)
                {
                    _mockEnvironmentModel = SetupEnvironmentModel();
                    return _mockEnvironmentModel;
                }
                else
                    return _mockEnvironmentModel;
            }
            set
            {
                if(_mockEnvironmentModel == null)
                {
                    _mockEnvironmentModel = value;
                }
            }
        }

        static public Mock<IMainViewModel> IMainViewModel
        {
            get
            {
                if(_mockIMainViewModel == null)
                {
                    _mockIMainViewModel = SetupMainViewModel();
                    return _mockIMainViewModel;
                }
                else
                {
                    return _mockIMainViewModel;
                }
            }
            set
            {
                _mockIMainViewModel = value;
            }
        }

        static public Mock<MainViewModel> MainViewModel
        {
            get
            {
                if (_mockMainViewModel == null)
                {
                    CompositionInitializer.InitializeForMeflessBaseViewModel();
                    var eventPublisher = new Mock<IEventAggregator>();
                    var environmentRepository = new Mock<IEnvironmentRepository>();
                    var environmentModel = new Mock<IEnvironmentModel>();
                    environmentModel.Setup(c => c.CanStudioExecute).Returns(false);
                    environmentRepository.Setup(c => c.ReadSession()).Returns(new[] {Guid.NewGuid()});
                    environmentRepository.Setup(c => c.All()).Returns(new[] {environmentModel.Object});
                    environmentRepository.Setup(c => c.Source).Returns(environmentModel.Object);
                    var versionChecker = new Mock<IVersionChecker>();
                    var asyncWorker = AsyncWorkerTests.CreateSynchronousAsyncWorker();
                    _mockMainViewModel = new Mock<MainViewModel>(eventPublisher.Object, asyncWorker.Object, environmentRepository.Object,
                        versionChecker.Object, false, null,null,null,null,null,null);
                }
                return _mockMainViewModel;
            }
            set
            {
                _mockMainViewModel = value;
            }
        }

        static public Mock<IFilePersistenceProvider> FilePersistenceProvider
        {
            get
            {
                if(_mockFilePersistenceProvider == null)
                {
                    _mockFilePersistenceProvider = SetupFilePersistenceProviderMock();
                    return _mockFilePersistenceProvider;
                }
                else
                {
                    return _mockFilePersistenceProvider;
                }
            }
            set
            {
                _mockFilePersistenceProvider = value;
            }
        }

        static public Mock<IContextualResourceModel> ResourceModel
        {
            get
            {
                if(_mockResourceModel == null)
                {
                    _mockResourceModel = SetupResourceModelMock();
                    return _mockResourceModel;
                }
                else
                {
                    return _mockResourceModel;
                }
            }
            set
            {
                _mockResourceModel = value;
            }
        }

        static public Mock<IContextualResourceModel> ResourceModelWithOnlyInputs
        {
            get
            {
                if(_mockResourceModel == null)
                {
                    _mockResourceModel = SetupResourceModelWithOnlyInputsMock();
                    return _mockResourceModel;
                }
                else
                {
                    return _mockResourceModel;
                }
            }
            set
            {
                _mockResourceModel = value;
            }
        }

        static public Mock<IContextualResourceModel> ResourceModelWithOnlyOutputs
        {
            get
            {
                if(_mockResourceModel == null)
                {
                    _mockResourceModel = SetupResourceModelWithOnlyOuputsMock();
                    return _mockResourceModel;
                }
                else
                {
                    return _mockResourceModel;
                }
            }
            set
            {
                _mockResourceModel = value;
            }
        }


        static public Mock<IMainViewModel> SetupMainViewModel()
        {
            // MainViewModel Setup
            // Dependancies on the EnvironmentRepository and the Data Channel
            _mockIMainViewModel = new Mock<IMainViewModel>();
            //5559 Check if the removal of the below lines impacted any tests
            //_mockMainViewModel.Setup(mainVM => mainVM.EnvironmentRepository.Save(SetupEnvironmentModel().Object)).Verifiable();
            //_mockMainViewModel.Setup(mainVM => mainVM.DsfChannel).Returns(SetupIFrameworkDataChannel().Object);

            return _mockIMainViewModel;

        }

        public static Mock<IStudioClientContext> SetupIFrameworkDataChannel_EmptyReturn()
        {
            Mock<IStudioClientContext> mockDataChannel = new Mock<IStudioClientContext>();
            mockDataChannel.Setup(dataChannel => dataChannel.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns("");

            return mockDataChannel;
        }

        static public Mock<IStudioClientContext> SetupIFrameworkDataChannel()
        {
            _mockFrameworkDataChannel = new Mock<IStudioClientContext>();
            _mockFrameworkDataChannel.Setup(dataChannel => dataChannel.ExecuteCommand("<x>String</x>", Guid.Empty, Guid.Empty)).Returns("success");

            return _mockFrameworkDataChannel;
        }

        static public Mock<IEnvironmentConnection> SetupIEnvironmentConnection(INetworkMessage resultMessage)
        {
            Mock<IEnvironmentConnection> mockIEnvironmentConnection = new Mock<IEnvironmentConnection>();
            mockIEnvironmentConnection.Setup(e => e.ServerEvents).Returns(new EventPublisher());
            mockIEnvironmentConnection.Setup(c => c.SendReceiveNetworkMessage(It.IsAny<INetworkMessage>())).Returns(resultMessage);

            // PBI 9598 - 2013.06.10 - TWR : added FetchCurrentServerLogService return value
            mockIEnvironmentConnection.Setup(c => c.ExecuteCommand(It.Is<string>(s => s.Contains("FetchCurrentServerLogService")), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(
                "Test log line1\nTest log line2\nTest log line3");
            return mockIEnvironmentConnection;
        }

        static public Mock<IStudioClientContext> SetupIFrameworkDataChannel<T>(INetworkMessage resultMessage) where T : INetworkMessage, new()
        {
            Mock<IStudioClientContext> mockFrameworkDataChannel = new Mock<IStudioClientContext>();
            mockFrameworkDataChannel.Setup(dataChannel => dataChannel.ExecuteCommand("<x>String</x>", Guid.Empty, Guid.Empty)).Returns("success");
            mockFrameworkDataChannel.Setup(dataChannel => dataChannel.SendMessage(It.IsAny<T>())).Returns(resultMessage);

            return mockFrameworkDataChannel;
        }

        static public Mock<IEnvironmentConnection> SetupIEnvironmentConnection(Exception messageSendingException)
        {
            Mock<IEnvironmentConnection> mockIEnvironmentConnection = new Mock<IEnvironmentConnection>();

            mockIEnvironmentConnection.Setup(c => c.SendReceiveNetworkMessage(It.IsAny<INetworkMessage>()))
                .Throws(messageSendingException);
            return mockIEnvironmentConnection;
        }

        static public Mock<IStudioClientContext> SetupIFrameworkDataChannel<T>(Exception messageSendingException) where T : INetworkMessage, new()
        {
            Mock<IStudioClientContext> mockFrameworkDataChannel = new Mock<IStudioClientContext>();
            mockFrameworkDataChannel.Setup(dataChannel => dataChannel.ExecuteCommand("<x>String</x>", Guid.Empty, Guid.Empty)).Returns("success");
            mockFrameworkDataChannel.Setup(dataChannel => dataChannel.SendMessage(It.IsAny<T>())).Throws(messageSendingException);

            return mockFrameworkDataChannel;
        }

        static public Mock<IEnvironmentModel> SetupEnvironmentModel<T>(INetworkMessage resultMessage) where T : INetworkMessage, new()
        {
            Mock<IEnvironmentModel> mockEnvironmentModel = new Mock<IEnvironmentModel>();
            mockEnvironmentModel.Setup(e => e.Connect()).Verifiable();
            mockEnvironmentModel.Setup(e => e.LoadResources()).Verifiable();
            mockEnvironmentModel.Setup(e => e.ResourceRepository).Returns(SetupFrameworkRepositoryResourceModelMock().Object);
            mockEnvironmentModel.Setup(e => e.Connection.WebServerUri).Returns(new Uri(StringResources.Uri_WebServer));
            mockEnvironmentModel.Setup(e => e.Connection.AppServerUri).Returns(new Uri(StringResources.Uri_WebServer));
            mockEnvironmentModel.Setup(e => e.Connection.ServerEvents).Returns(new EventPublisher());

            mockEnvironmentModel.SetupGet(c => c.Connection).Returns(SetupIEnvironmentConnection(resultMessage).Object);
            return mockEnvironmentModel;
        }

        static public Mock<IEnvironmentModel> SetupEnvironmentModel<T>(Exception messageSendingException) where T : INetworkMessage, new()
        {
            Mock<IEnvironmentModel> mockEnvironmentModel = new Mock<IEnvironmentModel>();
            mockEnvironmentModel.Setup(e => e.Connect()).Verifiable();
            mockEnvironmentModel.Setup(e => e.LoadResources()).Verifiable();
            mockEnvironmentModel.Setup(e => e.ResourceRepository).Returns(SetupFrameworkRepositoryResourceModelMock().Object);
            mockEnvironmentModel.Setup(e => e.Connection.WebServerUri).Returns(new Uri(StringResources.Uri_WebServer));
            mockEnvironmentModel.Setup(e => e.Connection.AppServerUri).Returns(new Uri(StringResources.Uri_WebServer));
            mockEnvironmentModel.Setup(e => e.Connection.ServerEvents).Returns(new EventPublisher());

            mockEnvironmentModel.SetupGet(c => c.Connection).Returns(SetupIEnvironmentConnection(messageSendingException).Object);
            return mockEnvironmentModel;
        }

        static public Mock<IEnvironmentModel> SetupEnvironmentModel()
        {
            var _mockEnvironmentModel = new Mock<IEnvironmentModel>();
            _mockEnvironmentModel.Setup(environmentModel => environmentModel.Connect()).Verifiable();
            _mockEnvironmentModel.Setup(environmentModel => environmentModel.LoadResources()).Verifiable();
            _mockEnvironmentModel.Setup(environmentModel => environmentModel.ResourceRepository).Returns(SetupFrameworkRepositoryResourceModelMock().Object);
            _mockEnvironmentModel.Setup(environmentModel => environmentModel.Connection.WebServerUri).Returns(new Uri(StringResources.Uri_WebServer));
            _mockEnvironmentModel.Setup(environmentModel => environmentModel.Connection.AppServerUri).Returns(new Uri(StringResources.Uri_WebServer));
            _mockEnvironmentModel.Setup(environmentModel => environmentModel.DsfChannel).Returns(SetupIFrameworkDataChannel_EmptyReturn().Object);
            _mockEnvironmentModel.Setup(environmentModel => environmentModel.Connection.ServerEvents).Returns(new EventPublisher());
            return _mockEnvironmentModel;
        }

        static public Mock<IEnvironmentModel> SetupEnvironmentModel(Mock<IContextualResourceModel> returnResource, List<IResourceModel> resourceRepositoryFakeBacker)
        {
            var _mockEnvironmentModel = new Mock<IEnvironmentModel>();
            _mockEnvironmentModel.Setup(environmentModel => environmentModel.Connect()).Verifiable();
            _mockEnvironmentModel.Setup(environmentModel => environmentModel.LoadResources()).Verifiable();
            _mockEnvironmentModel.Setup(environmentModel => environmentModel.ResourceRepository).Returns(SetupFrameworkRepositoryResourceModelMock(returnResource, resourceRepositoryFakeBacker).Object);
            _mockEnvironmentModel.Setup(environmentModel => environmentModel.DsfChannel.ExecuteCommand("", Guid.Empty, Guid.Empty)).Returns("");
            _mockEnvironmentModel.Setup(environmentModel => environmentModel.Connection.WebServerUri).Returns(new Uri(StringResources.Uri_WebServer));
            _mockEnvironmentModel.Setup(environmentModel => environmentModel.Connection.ServerEvents).Returns(new EventPublisher());

            return _mockEnvironmentModel;
        }

        static public Mock<IEnvironmentModel> SetupEnvironmentModel(Mock<IContextualResourceModel> returnResource, List<IResourceModel> returnResources, List<IResourceModel> resourceRepositoryFakeBacker)
        {
            var _mockEnvironmentModel = new Mock<IEnvironmentModel>();
            _mockEnvironmentModel.Setup(environmentModel => environmentModel.Connect()).Verifiable();
            _mockEnvironmentModel.Setup(environmentModel => environmentModel.LoadResources()).Verifiable();
            _mockEnvironmentModel.Setup(environmentModel => environmentModel.ResourceRepository).Returns(SetupFrameworkRepositoryResourceModelMock(returnResource, returnResources, resourceRepositoryFakeBacker).Object);
            _mockEnvironmentModel.Setup(environmentModel => environmentModel.DsfChannel.ExecuteCommand("", Guid.Empty, Guid.Empty)).Returns("");
            _mockEnvironmentModel.Setup(environmentModel => environmentModel.Connection.WebServerUri).Returns(new Uri(StringResources.Uri_WebServer));
            _mockEnvironmentModel.Setup(environmentModel => environmentModel.Connection.ServerEvents).Returns(new EventPublisher());

            return _mockEnvironmentModel;
        }

        static public Mock<IResourceRepository> SetupFrameworkRepositoryResourceModelMock()
        {
            var mockResourceModel = new Mock<IResourceRepository>();
            mockResourceModel.Setup(r => r.All()).Returns(new List<IResourceModel>());
            //mockResourceModel.Setup(resource => resource.Save().Verifiable();
            //mockResourceModel.Setup(resource => resource.ResourceType).Returns(enResourceType.Service);

            return mockResourceModel;
        }

        static public Mock<IResourceRepository> SetupFrameworkRepositoryResourceModelMock(Mock<IContextualResourceModel> returnResource, List<IResourceModel> resourceRepositoryFakeBacker)
        {
            var mockResourceModel = new Mock<IResourceRepository>();
            mockResourceModel.Setup(r => r.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>())).Returns(returnResource.Object);
            mockResourceModel.Setup(r => r.Save(It.IsAny<IResourceModel>())).Callback<IResourceModel>(r => resourceRepositoryFakeBacker.Add(r));
            mockResourceModel.Setup(r => r.ReloadResource(It.IsAny<Guid>(), It.IsAny<ResourceType>(), It.IsAny<IEqualityComparer<IResourceModel>>())).Returns(new List<IResourceModel>() { returnResource.Object });
            mockResourceModel.Setup(r => r.All()).Returns(new List<IResourceModel>() { returnResource.Object });

            return mockResourceModel;
        }

        static public Mock<IResourceRepository> SetupFrameworkRepositoryResourceModelMock(Mock<IContextualResourceModel> returnResource, List<IResourceModel> returnResources, List<IResourceModel> resourceRepositoryFakeBacker)
        {
            var mockResourceModel = new Mock<IResourceRepository>();
            mockResourceModel.Setup(r => r.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>())).Returns(returnResource.Object);
            mockResourceModel.Setup(r => r.Save(It.IsAny<IResourceModel>())).Callback<IResourceModel>(r => resourceRepositoryFakeBacker.Add(r));
            mockResourceModel.Setup(r => r.ReloadResource(It.IsAny<Guid>(), It.IsAny<ResourceType>(), It.IsAny<IEqualityComparer<IResourceModel>>())).Returns(new List<IResourceModel>() { returnResource.Object });
            mockResourceModel.Setup(r => r.All()).Returns(returnResources);

            return mockResourceModel;
        }

        static public Mock<IContextualResourceModel> SetupResourceModelMock()
        {
            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(res => res.DataList).Returns(StringResourcesTest.xmlDataList);
            mockResourceModel.Setup(res => res.ServiceDefinition).Returns(StringResources.xmlServiceDefinition);
            mockResourceModel.Setup(resModel => resModel.ResourceName).Returns("Test");
            mockResourceModel.Setup(resModel => resModel.DisplayName).Returns("TestResource");
            mockResourceModel.Setup(resModel => resModel.Category).Returns("Testing");
            mockResourceModel.Setup(resModel => resModel.IconPath).Returns("");
            mockResourceModel.Setup(resModel => resModel.ResourceType).Returns(ResourceType.WorkflowService);
            mockResourceModel.Setup(resModel => resModel.DataTags).Returns("WFI1,WFI2,WFI3");
            mockResourceModel.Setup(resModel => resModel.Environment).Returns(SetupEnvironmentModel(mockResourceModel, new List<IResourceModel>()).Object);

            return mockResourceModel;
        }

        static public Mock<IContextualResourceModel> SetupResourceModelWithOnlyInputsMock()
        {
            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(res => res.DataList).Returns(StringResourcesTest.xmlDataListInputOnly);
            mockResourceModel.Setup(res => res.ServiceDefinition).Returns(StringResourcesTest.xmlServiceDefinitionWithInputsOnly);
            mockResourceModel.Setup(resModel => resModel.ResourceName).Returns("Test");
            mockResourceModel.Setup(resModel => resModel.DisplayName).Returns("TestResource");
            mockResourceModel.Setup(resModel => resModel.Category).Returns("Testing");
            mockResourceModel.Setup(resModel => resModel.IconPath).Returns("");
            mockResourceModel.Setup(resModel => resModel.ResourceType).Returns(ResourceType.WorkflowService);
            mockResourceModel.Setup(resModel => resModel.DataTags).Returns("WFI1,WFI2,WFI3");
            mockResourceModel.Setup(resModel => resModel.Environment).Returns(SetupEnvironmentModel(mockResourceModel, new List<IResourceModel>()).Object);

            return mockResourceModel;
        }

        static public Mock<IContextualResourceModel> SetupResourceModelWithOnlyOuputsMock()
        {
            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(res => res.DataList).Returns(StringResourcesTest.xmlDataListOutputOnly);
            mockResourceModel.Setup(res => res.ServiceDefinition).Returns(StringResourcesTest.xmlServiceDefinitionWithOutputsOnly);
            mockResourceModel.Setup(resModel => resModel.ResourceName).Returns("Test");
            mockResourceModel.Setup(resModel => resModel.DisplayName).Returns("TestResource");
            mockResourceModel.Setup(resModel => resModel.Category).Returns("Testing");
            mockResourceModel.Setup(resModel => resModel.IconPath).Returns("");
            mockResourceModel.Setup(resModel => resModel.ResourceType).Returns(ResourceType.WorkflowService);
            mockResourceModel.Setup(resModel => resModel.DataTags).Returns("WFI1,WFI2,WFI3");
            mockResourceModel.Setup(resModel => resModel.Environment).Returns(SetupEnvironmentModel(mockResourceModel, new List<IResourceModel>()).Object);

            return mockResourceModel;
        }

        static public Mock<IContextualResourceModel> SetupResourceModelMock(ResourceType resourceType)
        {
            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(res => res.DataList).Returns(StringResourcesTest.xmlDataList);
            mockResourceModel.Setup(res => res.ServiceDefinition).Returns(StringResources.xmlServiceDefinition);
            mockResourceModel.Setup(resModel => resModel.ResourceName).Returns("Test");
            mockResourceModel.Setup(resModel => resModel.DisplayName).Returns("TestResource");
            mockResourceModel.Setup(resModel => resModel.Category).Returns("Testing");
            mockResourceModel.Setup(resModel => resModel.IconPath).Returns("");
            mockResourceModel.Setup(resModel => resModel.ResourceType).Returns(resourceType);
            mockResourceModel.Setup(resModel => resModel.DataTags).Returns("WFI1,WFI2,WFI3");
            mockResourceModel.Setup(resModel => resModel.Environment).Returns(SetupEnvironmentModel(mockResourceModel, new List<IResourceModel>()).Object);

            return mockResourceModel;
        }

        static public Mock<IContextualResourceModel> SetupResourceModelMock(ResourceType resourceType, string resourceName, bool returnSelf = true)
        {
            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(res => res.DataList).Returns(StringResourcesTest.xmlDataList);
            mockResourceModel.Setup(res => res.ServiceDefinition).Returns(StringResources.xmlServiceDefinition);
            mockResourceModel.Setup(resModel => resModel.ResourceName).Returns(resourceName);
            mockResourceModel.Setup(resModel => resModel.DisplayName).Returns(resourceName);
            mockResourceModel.Setup(resModel => resModel.Category).Returns("Testing");
            mockResourceModel.Setup(resModel => resModel.IconPath).Returns("");
            mockResourceModel.Setup(resModel => resModel.ResourceType).Returns(resourceType);
            mockResourceModel.Setup(resModel => resModel.DataTags).Returns("WFI1,WFI2,WFI3");

            if(returnSelf)
            {
                mockResourceModel.Setup(resModel => resModel.Environment).Returns(SetupEnvironmentModel(mockResourceModel, new List<IResourceModel>()).Object);
            }
            else
            {
                mockResourceModel.Setup(resModel => resModel.Environment).Returns(SetupEnvironmentModel().Object);
            }

            return mockResourceModel;
        }

        static public Mock<IContextualResourceModel> SetupResourceModelMock(ResourceType resourceType, string resourceName, Mock<IContextualResourceModel> findSingleReturn)
        {
            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(res => res.DataList).Returns(StringResourcesTest.xmlDataList);
            mockResourceModel.Setup(res => res.ServiceDefinition).Returns(StringResources.xmlServiceDefinition);
            mockResourceModel.Setup(resModel => resModel.ResourceName).Returns(resourceName);
            mockResourceModel.Setup(resModel => resModel.DisplayName).Returns(resourceName);
            mockResourceModel.Setup(resModel => resModel.Category).Returns("Testing");
            mockResourceModel.Setup(resModel => resModel.IconPath).Returns("");
            mockResourceModel.Setup(resModel => resModel.ResourceType).Returns(resourceType);
            mockResourceModel.Setup(resModel => resModel.DataTags).Returns("WFI1,WFI2,WFI3");
            mockResourceModel.Setup(resModel => resModel.Environment).Returns(SetupEnvironmentModel(findSingleReturn, new List<IResourceModel>()).Object);

            return mockResourceModel;
        }

        static public Mock<IContextualResourceModel> SetupResourceModelMock(List<IResourceModel> resourceRepositoryFakeBacker)
        {
            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(res => res.DataList).Returns(StringResourcesTest.xmlDataList);
            mockResourceModel.Setup(res => res.ServiceDefinition).Returns(StringResources.xmlServiceDefinition);
            mockResourceModel.Setup(resModel => resModel.ResourceName).Returns("Test");
            mockResourceModel.Setup(resModel => resModel.DisplayName).Returns("TestResource");
            mockResourceModel.Setup(resModel => resModel.Category).Returns("Testing");
            mockResourceModel.Setup(resModel => resModel.IconPath).Returns("");
            mockResourceModel.Setup(resModel => resModel.ResourceType).Returns(ResourceType.WorkflowService);
            mockResourceModel.Setup(resModel => resModel.DataTags).Returns("WFI1,WFI2,WFI3");
            mockResourceModel.Setup(resModel => resModel.Environment).Returns(SetupEnvironmentModel(mockResourceModel, resourceRepositoryFakeBacker).Object);

            return mockResourceModel;
        }

        static public Mock<IFilePersistenceProvider> SetupFilePersistenceProviderMock()
        {
            var mockFilePersistenceProvider = new Mock<IFilePersistenceProvider>();
            mockFilePersistenceProvider.Setup(filepro => filepro.Write(String.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, StringResources.DebugData_FilePath), "<xmlData/>")).Verifiable();
            mockFilePersistenceProvider.Setup(filepro => filepro.Read(String.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, StringResources.DebugData_FilePath))).Returns("<xmlData/>").Verifiable();
            mockFilePersistenceProvider.Setup(filepro => filepro.Delete(String.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, StringResources.DebugData_FilePath))).Verifiable();

            return mockFilePersistenceProvider;
        }

        public static Mock<IWebActivity> SetupWebActivityMock()
        {
            var _mockWebActivity = new Mock<IWebActivity>();

            _mockWebActivity.SetupAllProperties();
            _mockWebActivity.Setup(activity => activity.XMLConfiguration).Returns(StringResourcesTest.WebActivity_XmlConfig);
            _mockWebActivity.Object.SavedInputMapping = StringResourcesTest.WebActivity_SavedInputMapping;
            _mockWebActivity.Object.SavedOutputMapping = StringResourcesTest.WebActivity_SavedOutputMapping;
            _mockWebActivity.Object.LiveInputMapping = StringResourcesTest.WebActivity_LiveInputMapping;
            _mockWebActivity.Object.LiveOutputMapping = StringResourcesTest.WebActivity_LiveOutputMapping;
            _mockWebActivity.Object.ServiceName = "MyTestActivity";
            _mockWebActivity.Object.ResourceModel = SetupResourceModelMock().Object;
            return _mockWebActivity;
        }

        public static Mock<IDataListViewModel> SetupDataListViewModel()
        {
            var mockDataListViewModel = new Mock<IDataListViewModel>();
            return mockDataListViewModel;
        }

        public static Mock<IDataListItemModel> SetupDataListItemModel()
        {
            var mockDataListItemViewModel = new Mock<IDataListItemModel>();
            mockDataListItemViewModel.Setup(itemVM => itemVM.Name).Returns("UnitTestDataListItem");
            ObservableCollection<IDataListItemModel> children = new ObservableCollection<IDataListItemModel>();
            return mockDataListItemViewModel;
        }

        public static Mock<IExecutionStatusCallbackDispatcher> SetupExecutionStatusCallbackDispatcher(bool addResult = true, bool removeResult = true)
        {
            Mock<IExecutionStatusCallbackDispatcher> mockExecutionStatusCallbackDispatcher = new Mock<IExecutionStatusCallbackDispatcher>();
            mockExecutionStatusCallbackDispatcher.Setup(e => e.Add(It.IsAny<Guid>(), It.IsAny<Action<ExecutionStatusCallbackMessage>>())).Verifiable();
            mockExecutionStatusCallbackDispatcher.Setup(e => e.Add(It.IsAny<Guid>(), It.IsAny<Action<ExecutionStatusCallbackMessage>>())).Returns(addResult);

            mockExecutionStatusCallbackDispatcher.Setup(e => e.Remove(It.IsAny<Guid>())).Verifiable();
            mockExecutionStatusCallbackDispatcher.Setup(e => e.Remove(It.IsAny<Guid>())).Returns(removeResult);

            mockExecutionStatusCallbackDispatcher.Setup(e => e.RemoveRange(It.IsAny<IList<Guid>>())).Verifiable();

            mockExecutionStatusCallbackDispatcher.Setup(e => e.Post(It.IsAny<ExecutionStatusCallbackMessage>())).Verifiable();
            mockExecutionStatusCallbackDispatcher.Setup(e => e.Send(It.IsAny<ExecutionStatusCallbackMessage>())).Verifiable();

            return mockExecutionStatusCallbackDispatcher;
        }

        public static Mock<INetworkMessageBroker> SetupNetworkMessageBroker<T>(bool sendThrowsException = false) where T : INetworkMessage, new()
        {
            Mock<INetworkMessageBroker> mockNetworkMessageBroker = new Mock<INetworkMessageBroker>();

            if(sendThrowsException)
            {
                mockNetworkMessageBroker.Setup(e => e.Send<T>(It.IsAny<T>(), It.IsAny<INetworkOperator>())).Throws(new Exception());
            }
            else
            {
                mockNetworkMessageBroker.Setup(e => e.Send<ExecutionStatusCallbackMessage>(It.IsAny<ExecutionStatusCallbackMessage>(), It.IsAny<INetworkOperator>())).Verifiable();
            }

            return mockNetworkMessageBroker;
        }

        public static Mock<IStudioNetworkChannelContext> SetupStudioNetworkChannelContext()
        {
            Mock<IStudioNetworkChannelContext> mockSetupServerNetworkChannelContext = new Mock<IStudioNetworkChannelContext>();
            mockSetupServerNetworkChannelContext.Setup(s => s.Account).Returns(Guid.Empty);
            mockSetupServerNetworkChannelContext.Setup(s => s.Server).Returns(Guid.Empty);

            return mockSetupServerNetworkChannelContext;
        }

        public static Mock<IStudioNetworkMessageAggregator> SetupStudioNetworkMessageAggregator()
        {
            Mock<IStudioNetworkMessageAggregator> mockStudioNetworkMessageAggregator = new Mock<IStudioNetworkMessageAggregator>();

            return mockStudioNetworkMessageAggregator;
        }

        public static Mock<IDataListItemModel> SetupDataListItemViewModel()
        {
            var mockDataListItemViewModel = new Mock<IDataListItemModel>();
            mockDataListItemViewModel.Setup(itemVM => itemVM.Name).Returns("UnitTestDataListItem");
            ObservableCollection<IDataListItemModel> children = new ObservableCollection<IDataListItemModel>();
            return mockDataListItemViewModel;
        }

        public static Mock<IDev2Definition> SetupIDev2Definition(string name, string value, string recordSetName, string rawValue, string mapsTo, bool isRequired, bool isRecordSet, bool isEvaluated, string defaultValue)
        {
            Mock<IDev2Definition> _mockDev2Def = new Mock<IDev2Definition>();
            _mockDev2Def.Setup(devDef => devDef.Name).Returns(name);
            _mockDev2Def.Setup(devDef => devDef.Value).Returns(value);
            _mockDev2Def.Setup(devDef => devDef.RecordSetName).Returns(recordSetName);
            _mockDev2Def.Setup(devDef => devDef.RawValue).Returns(rawValue);
            _mockDev2Def.Setup(devDef => devDef.MapsTo).Returns(mapsTo);
            _mockDev2Def.Setup(devDef => devDef.IsRequired).Returns(isRequired);
            _mockDev2Def.Setup(devDef => devDef.IsRecordSet).Returns(isRecordSet);
            _mockDev2Def.Setup(devDef => devDef.IsEvaluated).Returns(isEvaluated);
            _mockDev2Def.Setup(devDef => devDef.DefaultValue).Returns(defaultValue);
            return _mockDev2Def;
        }

        public static Mock<IInputOutputViewModel> SetupIInputOutputViewModel(string name, string value, string recordSetName, bool isSelected, string mapsTo, bool isRequired, string displayName, string defaultValue)
        {
            Mock<IInputOutputViewModel> _mockInOut = new Mock<IInputOutputViewModel>();
            _mockInOut.SetupAllProperties();
            _mockInOut.Setup(devDef => devDef.Name).Returns(name);
            _mockInOut.Setup(devDef => devDef.Value).Returns(value);
            _mockInOut.Setup(devDef => devDef.RecordSetName).Returns(recordSetName);
            _mockInOut.Setup(devDef => devDef.IsSelected).Returns(isSelected);
            _mockInOut.Setup(devDef => devDef.MapsTo).Returns(mapsTo);
            _mockInOut.Setup(devDef => devDef.Required).Returns(isRequired);
            _mockInOut.Setup(devDef => devDef.DisplayName).Returns(displayName);
            _mockInOut.Setup(devDef => devDef.DefaultValue).Returns(defaultValue);
            ObservableCollection<IDataListItemModel> dataList = new ObservableCollection<IDataListItemModel>();
            dataList.Add(SetupDataListItemViewModel().Object);
            //_mockInOut.Setup(inOut => inOut.DataList).Returns(dataList);

            return _mockInOut;
        }

        public static Mock<IDataMappingViewModel> SetupIDataMappingViewModel()
        {
            Mock<IDataMappingViewModel> _mockDataMappingViewModel = new Mock<IDataMappingViewModel>();

            //Mock<IDataMappingListFactory> _mockDataMappingListFactory = new Mock<IDataMappingListFactory>();
            //Mock<IDataListViewModelFactory> _mockDataListViewModelFactory = new Mock<IDataListViewModelFactory>();
            Mock<IWebActivity> _mockWebActivity = SetupWebActivityMock();
            Mock<IContextualResourceModel> _mockresource = SetupResourceModelMock();
            Mock<IDataListViewModel> _mockDataListViewModel = new Mock<IDataListViewModel>();
            IList<IDev2Definition> inputDev2defList = new List<IDev2Definition>();

            Mock<IDev2Definition> _mockDev2DefIn1 = SetupIDev2Definition("reg", "", "", "", "", true, false, true, "NUD2347");
            Mock<IDev2Definition> _mockDev2DefIn2 = SetupIDev2Definition("asdfsad", "registration223", "", "", "registration223", true, false, true, "w3rt24324");
            Mock<IDev2Definition> _mockDev2DefIn3 = SetupIDev2Definition("number", "", "", "", "", false, false, true, "");

            inputDev2defList.Add(_mockDev2DefIn1.Object);
            inputDev2defList.Add(_mockDev2DefIn2.Object);
            inputDev2defList.Add(_mockDev2DefIn3.Object);

            IList<IDev2Definition> outputDev2defList = new List<IDev2Definition>();

            Mock<IDev2Definition> _mockDev2DefOut1 = SetupIDev2Definition("vehicleVin", "", "", "", "VIN", false, false, true, "");
            Mock<IDev2Definition> _mockDev2DefOut2 = SetupIDev2Definition("vehicleColor", "", "", "", "vehicleColor", false, false, true, "");
            Mock<IDev2Definition> _mockDev2DefOut3 = SetupIDev2Definition("Fines", "", "", "", "", false, false, true, "");
            Mock<IDev2Definition> _mockDev2DefOut4 = SetupIDev2Definition("speed", "", "Fines", "", "speed", false, false, true, "");
            Mock<IDev2Definition> _mockDev2DefOut5 = SetupIDev2Definition("date", "Fines.Date", "", "", "date", false, false, true, "");
            Mock<IDev2Definition> _mockDev2DefOut6 = SetupIDev2Definition("location", "", "Fines", "", "location", false, false, true, "");

            outputDev2defList.Add(_mockDev2DefOut1.Object);
            outputDev2defList.Add(_mockDev2DefOut2.Object);
            outputDev2defList.Add(_mockDev2DefOut3.Object);
            outputDev2defList.Add(_mockDev2DefOut4.Object);
            outputDev2defList.Add(_mockDev2DefOut5.Object);
            outputDev2defList.Add(_mockDev2DefOut6.Object);

            IList<IInputOutputViewModel> inputInOutList = new List<IInputOutputViewModel>();
            Mock<IInputOutputViewModel> _mockInOutVm1 = SetupIInputOutputViewModel("reg", "", "", false, "", true, "reg", "NUD2347");
            Mock<IInputOutputViewModel> _mockInOutVm2 = SetupIInputOutputViewModel("asdfsad", "registration223", "", false, "registration223", true, "asdfsad", "w3rt24324");
            Mock<IInputOutputViewModel> _mockInOutVm3 = SetupIInputOutputViewModel("number", "", "", false, "", false, "number", "");
            inputInOutList.Add(_mockInOutVm1.Object);
            inputInOutList.Add(_mockInOutVm2.Object);
            inputInOutList.Add(_mockInOutVm3.Object);

            IList<IInputOutputViewModel> outputInOutList = new List<IInputOutputViewModel>();
            Mock<IInputOutputViewModel> _mockInOutVm4 = SetupIInputOutputViewModel("vehicleVin", "", "", false, "VIN", false, "vehicleVin", "");
            Mock<IInputOutputViewModel> _mockInOutVm5 = SetupIInputOutputViewModel("vehicleColor", "", "", false, "", true, "vehicleColor", "");
            Mock<IInputOutputViewModel> _mockInOutVm6 = SetupIInputOutputViewModel("Fines", "", "", false, "", false, "Fines", "");
            Mock<IInputOutputViewModel> _mockInOutVm7 = SetupIInputOutputViewModel("speed", "", "Fines", false, "", false, "speed", "NUD2347");
            Mock<IInputOutputViewModel> _mockInOutVm8 = SetupIInputOutputViewModel("date", "Fines.Date", "Fines", false, "registration223", true, "date", "w3rt24324");
            Mock<IInputOutputViewModel> _mockInOutVm9 = SetupIInputOutputViewModel("location", "", "Fines", false, "", false, "location", "");
            outputInOutList.Add(_mockInOutVm4.Object);
            outputInOutList.Add(_mockInOutVm5.Object);
            outputInOutList.Add(_mockInOutVm6.Object);
            outputInOutList.Add(_mockInOutVm7.Object);
            outputInOutList.Add(_mockInOutVm8.Object);
            outputInOutList.Add(_mockInOutVm9.Object);


            _mockDataListViewModel.Setup(dlvm => dlvm.Resource).Returns(_mockresource.Object);


            _mockWebActivity.Setup(activity => activity.UnderlyingWebActivityObjectType).Returns(typeof(DsfWebPageActivity));
            _mockDataMappingViewModel.Setup(dmvm => dmvm.Activity).Returns(_mockWebActivity.Object);

            _mockDataMappingViewModel.Setup(dmvm => dmvm.Inputs).Returns(inputInOutList.ToObservableCollection());
            _mockDataMappingViewModel.Setup(dmvm => dmvm.Outputs).Returns(outputInOutList.ToObservableCollection());



            return _mockDataMappingViewModel;
        }


        public static Mock<IDev2ConfigurationProvider> CreateConfigurationProvider(string[] key, string[] val)
        {
            Mock<IDev2ConfigurationProvider> result = new Mock<IDev2ConfigurationProvider>();
            IDictionary d = new Dictionary<string, string>();
            int pos = 0;

            foreach(string k in key)
            {
                d.Add(k, val[pos]);
            }

            // result.SetupSet(c => c.IsSelected = It.IsAny<bool>()).Callback<bool>(value => isSelected[row,col] = value);
            result.Setup(c => c.ReadKey(It.IsAny<string>())).Returns<string>((string kk) => (string)d[kk]);

            return result;
        }

        public static Mock<IPopupController> CreateIPopup(MessageBoxResult returningResult)
        {
            Mock<IPopupController> result = new Mock<IPopupController>();
            result.Setup(moq => moq.Show()).Returns(returningResult).Verifiable();

            return result;
        }

        public static Mock<IEventAggregator> SetupMockEventAggregator()
        {
            Mock<IEventAggregator> mock = new Mock<IEventAggregator>();
            mock.Setup(m => m.Publish(It.IsAny<IMessage>())).Verifiable();
            return mock;
        }
    }
}
