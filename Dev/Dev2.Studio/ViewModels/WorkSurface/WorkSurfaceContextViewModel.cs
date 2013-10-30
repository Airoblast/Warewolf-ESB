﻿using System;
using System.Activities.Presentation.View;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using Dev2.Common;
using Dev2.Communication;
using Dev2.Composition;
using Dev2.Data.ServiceModel.Messages;
using Dev2.Diagnostics;
using Dev2.Messages;
using Dev2.Providers.Errors;
using Dev2.Providers.Logs;
using Dev2.Services.Events;
using Dev2.Studio.AppResources.Comparers;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.InterfaceImplementors;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Utils;
using Dev2.Studio.Core.ViewModels;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Core.Workspaces;
using Dev2.Studio.Factory;
using Dev2.Studio.InterfaceImplementors.WizardResourceKeys;
using Dev2.Studio.ViewModels.Diagnostics;
using Dev2.Studio.ViewModels.Workflow;
using Dev2.Studio.Views.ResourceManagement;
using Dev2.Studio.Webs;
using Dev2.Studio.Webs.Callbacks;
using Dev2.Utils;
using Newtonsoft.Json;
using Unlimited.Framework;

namespace Dev2.Studio.ViewModels.WorkSurface
{
    /// <summary>
    ///     Class used as unified context across the studio - coordination across different regions
    /// </summary>
    /// <author>Jurie.smit</author>
    /// <date>2/27/2013</date>
    public class WorkSurfaceContextViewModel : BaseViewModel,
                                 IHandle<SaveResourceMessage>, IHandle<DebugResourceMessage>,
                                 IHandle<ExecuteResourceMessage>, 
                                 IHandle<UpdateWorksurfaceDisplayName>
    {
        #region private fields

        IDataListViewModel _dataListViewModel;
        IWorkSurfaceViewModel _workSurfaceViewModel;
        DebugOutputViewModel _debugOutputViewModel;
        IContextualResourceModel _contextualResourceModel;

        readonly IWindowManager _windowManager;
        IFrameworkSecurityContext _securityContext;
        IWorkspaceItemRepository _workspaceItemRepository;

        ICommand _viewInBrowserCommand;
        ICommand _debugCommand;
        ICommand _runCommand;
        ICommand _saveCommand;
        ICommand _editResourceCommand;
        bool _hasMappingChange;
        IEnvironmentModel _environmentModel;

        #endregion private fields

        #region public properties

        public WorkSurfaceKey WorkSurfaceKey { get; private set; }

        public IEnvironmentModel Environment
        {
            get
            {
                if(ContextualResourceModel == null)
                {
                    return null;
                }
                return ContextualResourceModel.Environment;
            }
        }

        public DebugOutputViewModel DebugOutputViewModel
        {
            get
            {
                return _debugOutputViewModel;
            }
            set { _debugOutputViewModel = value; }
        } 

        public bool DeleteRequested { get; set; }

        public IDataListViewModel DataListViewModel
        {
            get
            {
                return _dataListViewModel;
            }
            set
            {
                if(_dataListViewModel == value)
                {
                    return;
                }

                _dataListViewModel = value;
                if(_dataListViewModel != null)
                {
                    _dataListViewModel.ConductWith(this);
                }

                NotifyOfPropertyChange(() => DataListViewModel);
            }
        }

        public IWorkSurfaceViewModel WorkSurfaceViewModel
        {
            get
            {
                return _workSurfaceViewModel;
            }
            set
            {
                if(_workSurfaceViewModel == value)
                {
                    return;
                }

                _workSurfaceViewModel = value;
                if(_workSurfaceViewModel == null)
                {
                    if(ContextualResourceModel != null)
                    {
                        ContextualResourceModel.OnDesignValidationReceived -= ValidationMemoReceived;
                    }
                }
                NotifyOfPropertyChange(() => WorkSurfaceViewModel);

                var isWorkFlowDesigner = _workSurfaceViewModel is IWorkflowDesignerViewModel;
                if(isWorkFlowDesigner)
                {
                    var workFlowDesignerViewModel = (IWorkflowDesignerViewModel)_workSurfaceViewModel;
                    _contextualResourceModel = workFlowDesignerViewModel.ResourceModel;
                    if(ContextualResourceModel != null)
                    {
                        ContextualResourceModel.OnDesignValidationReceived += ValidationMemoReceived;
                }
                }

                if(WorkSurfaceViewModel != null)
                {
                    WorkSurfaceViewModel.ConductWith(this);
            }
        }
        }

        void ValidationMemoReceived(object sender, DesignValidationMemo designValidationMemo)
        {
            if(designValidationMemo.IsValid)
            {
                return;
            }
            if(designValidationMemo.Errors.Find(info => info.FixType == FixType.ReloadMapping) != null)
            {
                _hasMappingChange = true;
            }
        }

        public bool CanExecute
        {
            get
            {
                return ContextualResourceModel != null && IsEnvironmentConnected() && !DebugOutputViewModel.IsProcessing;
        }
        }

        public ICommand EditCommand
        {
            get
            {
                return _editResourceCommand ??
                       (_editResourceCommand =
                           new RelayCommand(param =>
                           {
                               Logger.TraceInfo("Publish message of type - " + typeof(ShowEditResourceWizardMessage));
                               EventPublisher.Publish(new ShowEditResourceWizardMessage(ContextualResourceModel));
                           }
                            , param => CanExecute));
            }
        }

        public ICommand SaveCommand
        {
            get
            {
                return _saveCommand ??
                       (_saveCommand = new RelayCommand(param => Save(), param => CanDebug()));
            }
        }

        #endregion public properties

        #region ctors

        public WorkSurfaceContextViewModel(WorkSurfaceKey workSurfaceKey, IWorkSurfaceViewModel workSurfaceViewModel)
            : this(EventPublishers.Aggregator, workSurfaceKey, workSurfaceViewModel)
        {
        }

        public WorkSurfaceContextViewModel(IEventAggregator eventPublisher, WorkSurfaceKey workSurfaceKey, IWorkSurfaceViewModel workSurfaceViewModel)
            : base(eventPublisher)
        {
            if(workSurfaceKey == null)
            {
                throw new ArgumentNullException("workSurfaceKey");
            }
            if(workSurfaceViewModel == null)
            {
                throw new ArgumentNullException("workSurfaceViewModel");
            }
            WorkSurfaceKey = workSurfaceKey;
            WorkSurfaceViewModel = workSurfaceViewModel;

             ImportService.TryGetExportValue(out _windowManager);
             ImportService.TryGetExportValue(out _securityContext);
             _workspaceItemRepository = WorkspaceItemRepository.Instance;

            var model = WorkSurfaceViewModel as IWorkflowDesignerViewModel;
            if(model != null)
            {
                _environmentModel = model.EnvironmentModel;
                if(_environmentModel != null)
                {
                    // MUST use connection server event publisher - debug events are published from the server!
                    DebugOutputViewModel = new DebugOutputViewModel(_environmentModel.Connection.ServerEvents, EnvironmentRepository.Instance);
                    _environmentModel.IsConnectedChanged += EnvironmentModelOnIsConnectedChanged();
            }
            }
        }

        EventHandler<ConnectedEventArgs> EnvironmentModelOnIsConnectedChanged()
        {
            return (sender, args) =>
            {
                if(args.IsConnected == false)
                {
                    SetDebugStatus(DebugStatus.Finished);
                }
            };
        }

        #endregion

        #region IHandle

        public void Handle(DebugResourceMessage message)
        {
            Logger.TraceInfo(message.GetType().Name);
            Debug(message.Resource, true);
        }

        public void Handle(ExecuteResourceMessage message)
        {
            Logger.TraceInfo(message.GetType().Name);
            Debug(message.Resource, false);
        }

        public void Handle(SaveResourceMessage message)
        {
            Logger.TraceInfo(message.GetType().Name);
            if(ContextualResourceModel != null)
            {
                if(ContextualResourceModel.ResourceName == message.Resource.ResourceName)
                {
                    Save(message.Resource, message.IsLocalSave, message.AddToTabManager);
                }
            }
            else
            {
                Save(message.Resource, message.IsLocalSave, message.AddToTabManager);
            }
        }

        public void Handle(UpdateWorksurfaceDisplayName message)
        {
            Logger.TraceInfo(message.GetType().Name);
            if(ContextualResourceModel != null && ContextualResourceModel.ID == message.WorksurfaceResourceID)
            {
                //tab title
                ContextualResourceModel.ResourceName = message.NewName;
                _workSurfaceViewModel.NotifyOfPropertyChange("DisplayName");
            }
        }

        public void Handle(UpdateWorksurfaceFlowNodeDisplayName message)
        {
            Logger.TraceInfo(message.GetType().Name);
            ContextualResourceModel.ServiceDefinition = ContextualResourceModel.ServiceDefinition
                    .Replace("x:Class=\"" + ContextualResourceModel.ResourceName, "x:Class=\"" + message.NewName)
                    .Replace("Name=\"" + ContextualResourceModel.ResourceName, "Name=\"" + message.NewName)
                    .Replace("ToolboxFriendlyName=\"" + ContextualResourceModel.ResourceName, "ToolboxFriendlyName=\"" + message.NewName)
                    .Replace("<DisplayName>" + ContextualResourceModel.ResourceName + "</DisplayName>", "<DisplayName>" + message.NewName + "</DisplayName>")
                    .Replace("DisplayName=\"" + ContextualResourceModel.ResourceName, "DisplayName=\"" + message.NewName);
            NotifyOfPropertyChange("ContextualResourceModel");
        }

        #endregion IHandle

        #region commands

        public ICommand RunCommand
        {
            get
            {
                return _runCommand ??
                       (_runCommand = new RelayCommand(param => Debug(ContextualResourceModel, false),
                                                       param => CanExecute));
            }
        }

        public ICommand ViewInBrowserCommand
        {
            get
            {
                return _viewInBrowserCommand ??
                       (_viewInBrowserCommand = new RelayCommand(param => ViewInBrowser(),
                                                    param => CanDebug()));
            }
        }

        public ICommand DebugCommand
        {
            get
            {
                return _debugCommand ??
                       (_debugCommand =
                        new RelayCommand(param => Debug(), param => CanDebug()));
            }
        }

        public IContextualResourceModel ContextualResourceModel
        {
            get
            {
                return _contextualResourceModel;
            }
        }

        #endregion commands

        #region public methods

        bool CanDebug()
        {
            return IsEnvironmentConnected()
                && !DebugOutputViewModel.IsStopping
                && !DebugOutputViewModel.IsConfiguring;
        }

        public void SetDebugStatus(DebugStatus debugStatus)
        {
            if(debugStatus == DebugStatus.Finished)
            {
                CommandManager.InvalidateRequerySuggested();
            }

            if(debugStatus == DebugStatus.Configure)
            {
                DebugOutputViewModel.Clear();
            }

            DebugOutputViewModel.DebugStatus = debugStatus;
        }

        public void GetServiceInputDataFromUser(IServiceDebugInfoModel input)
        {
            var inputDataViewModel = new WorkflowInputDataViewModel(input, DebugOutputViewModel) { Parent = this };
            _windowManager.ShowDialog(inputDataViewModel);
        }

        public void Debug(IContextualResourceModel resourceModel, bool isDebug)
        {
            if(resourceModel == null || resourceModel.Environment == null || !resourceModel.Environment.IsConnected)
            {
                return;
            }

            SetDebugStatus(DebugStatus.Configure);

            Save(resourceModel, true);
            var mode = isDebug ? DebugMode.DebugInteractive : DebugMode.Run;
            IServiceDebugInfoModel debugInfoModel =
                ServiceDebugInfoModelFactory.CreateServiceDebugInfoModel(resourceModel, string.Empty, mode);
            GetServiceInputDataFromUser(debugInfoModel);
        }

        public void StopExecution()
        {
            SetDebugStatus(DebugStatus.Stopping);

            CommandManager.InvalidateRequerySuggested();

            dynamic buildRequest = new UnlimitedObject();

            buildRequest.Service = "TerminateExecutionService";
            buildRequest.Roles = String.Join(",", _securityContext.Roles);

            buildRequest.ResourceID = ContextualResourceModel.ID;

            Guid workspaceID = ((IStudioClientContext)ContextualResourceModel.Environment.DsfChannel).WorkspaceID;

            string result =
                ContextualResourceModel.Environment.DsfChannel.
                      ExecuteCommand(buildRequest.XmlString, workspaceID, GlobalConstants.NullDataListID) ??
                string.Format(GlobalConstants.NetworkCommunicationErrorTextFormat, buildRequest.Service);

            DispatchServerDebugMessage(result, ContextualResourceModel);

            SetDebugStatus(DebugStatus.Finished);
        }

        public void ViewInBrowser()
        {
            FindMissing();
            Logger.TraceInfo("Publish message of type - " + typeof(SaveAllOpenTabsMessage));
            EventPublisher.Publish(new SaveAllOpenTabsMessage());

            if(ContextualResourceModel == null || ContextualResourceModel.Environment == null ||
               ContextualResourceModel.Environment.Connection == null)
            {
                return;
            }

            Process.Start(StudioToWizardBridge.GetWorkflowUrl(ContextualResourceModel).AbsoluteUri);
        }

        public void BindToModel()
        {
            var vm = WorkSurfaceViewModel as IWorkflowDesignerViewModel;
            if(vm != null)
            {
                vm.BindToModel();
            }
        }

        public void ShowSaveDialog(IContextualResourceModel resourceModel, bool addToTabManager)
        {
            RootWebSite.ShowNewWorkflowSaveDialog(resourceModel, null, addToTabManager);
        }

        public void Save(bool isLocalSave = false)
        {
            Save(ContextualResourceModel, isLocalSave);
            if (WorkSurfaceViewModel != null)
            {
                WorkSurfaceViewModel.NotifyOfPropertyChange("DisplayName");
            }
        }

        public bool IsEnvironmentConnected()
        {
            return Environment != null && Environment.IsConnected;
        }

        public void FindMissing()
        {
            if(WorkSurfaceViewModel is WorkflowDesignerViewModel)
            {
                var vm = (WorkflowDesignerViewModel)WorkSurfaceViewModel;
                vm.AddMissingWithNoPopUpAndFindUnusedDataListItems();
            }
        }

        #endregion

        #region private methods

        void Save(IContextualResourceModel resource, bool isLocalSave, bool addToTabManager = true)
        {
            if(resource == null)
            {
                return;
            }

            if(resource.IsNewWorkflow && !isLocalSave)
            {
                ShowSaveDialog(resource, addToTabManager);
                return;
            }

            FindMissing();
            BindToModel();

            var result = _workspaceItemRepository.UpdateWorkspaceItem(resource, isLocalSave);
            resource.Environment.ResourceRepository.Save(resource);
            DisplaySaveResult(result, resource);
            if(!isLocalSave)
            {
                if(_hasMappingChange)
                {
                    CheckForServerMessages(resource);
                    _hasMappingChange = false;
                }
                var saveResult = resource.Environment.ResourceRepository.SaveToServer(resource);
                DispatchServerDebugMessage(saveResult, resource);
                resource.IsWorkflowSaved = true;
            }
            Logger.TraceInfo("Publish message of type - " + typeof(UpdateDeployMessage));
            EventPublisher.Publish(new UpdateDeployMessage());
        }

        void CheckForServerMessages(IContextualResourceModel resource)
        {
            if(resource == null)
            {
                return;
            }
            var compileMessagesFromServer = StudioCompileMessageRepo.GetCompileMessagesFromServer(resource);
            if(string.IsNullOrEmpty(compileMessagesFromServer))
            {
                return;
            }
            if(compileMessagesFromServer.Contains("<Error>"))
            {
                return;
            }
            var compileMessageList = JsonConvert.DeserializeObject<CompileMessageList>(compileMessagesFromServer);
            if(compileMessageList.Count == 0)
            {
                return;
            }
            var showResourceChangedUtil = new ResourceChangeHandler(EventPublisher);
            showResourceChangedUtil.ShowResourceChanged(resource, compileMessageList.Dependants);            
        }

        void DisplaySaveResult(string result, IContextualResourceModel resource)
        {
            var sb = new StringBuilder();
            sb.AppendLine(String.Format("<Save StartDate=\"{0}\">",
                                        DateTime.Now.ToString(CultureInfo.InvariantCulture)));
            sb.AppendLine(result);
            sb.AppendLine(String.Format("</Save>"));

            DispatchServerDebugMessage(sb.ToString(), resource);
        }

        void DispatchServerDebugMessage(string message, IContextualResourceModel resource)
        {
            var debugstate = DebugStateFactory.Create(message, resource);
            if(_debugOutputViewModel != null)
            {
                debugstate.SessionID = _debugOutputViewModel.SessionID;
                _debugOutputViewModel.Append(debugstate);
            }
        }

        void Debug()
        {
            if(DebugOutputViewModel.IsProcessing)
            {
                StopExecution();
            }
            else
            {
                Debug(ContextualResourceModel, true);
            }
        }

        #endregion

        #region overrides

        protected override void OnActivate()
        {
            base.OnActivate();
            DataListSingleton.SetDataList(DataListViewModel);

            var workflowDesignerViewModel = WorkSurfaceViewModel as WorkflowDesignerViewModel;
            if(workflowDesignerViewModel != null)
            {
                //workflowDesignerViewModel.AddMissingWithNoPopUpAndFindUnusedDataListItems();
                //2013.07.03: Ashley Lewis for bug 9637 - set focus to allow ctrl+a
                if(!workflowDesignerViewModel.Designer.Context.Items.GetValue<Selection>().SelectedObjects.Any() || workflowDesignerViewModel.Designer.Context.Items.GetValue<Selection>().SelectedObjects.Any(c => c.ItemType.Name == "StartNode" || c.ItemType.Name == "Flowchart" || c.ItemType.Name == "ActivityBuilder"))
                {
                    workflowDesignerViewModel.FocusActivityBuilder();
                }
            }
        }

        #region Overrides of BaseViewModel

        /// <summary>
        /// Child classes can override this method to perform 
        ///  clean-up logic, such as removing event handlers.
        /// </summary>
        protected override void OnDispose()
        {
            if(_environmentModel != null)
            {
                _environmentModel.IsConnectedChanged -= EnvironmentModelOnIsConnectedChanged();
            }

            if(DebugOutputViewModel != null)
            {
                DebugOutputViewModel.Dispose();
            }
            if(DataListViewModel != null)
            {
                ((SimpleBaseViewModel)DataListViewModel).Dispose();
            }
            if(ContextualResourceModel != null)
            {
                ContextualResourceModel.OnDesignValidationReceived -= ValidationMemoReceived;
            }

            if(DataListViewModel != null)
            {
                DataListViewModel.Dispose();
            }

            if(DebugOutputViewModel != null)
            {
                DebugOutputViewModel.Dispose();
            }

            base.OnDispose();
        }

        #endregion

        #endregion
    }
}
