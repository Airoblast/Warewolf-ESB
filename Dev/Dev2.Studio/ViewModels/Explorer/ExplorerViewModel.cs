﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Input;
using Caliburn.Micro;
using Dev2.AppResources.Repositories;
using Dev2.Messages;
using Dev2.Providers.Logs;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Enums;
using Dev2.Studio.ViewModels.Navigation;
using Dev2.Studio.ViewModels.WorkSurface;
using Dev2.Threading;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.ViewModels.Explorer
{
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ExplorerViewModel : BaseViewModel,
                                     IHandle<UpdateExplorerMessage>,
                                     IHandle<RemoveEnvironmentMessage>,
                                     IHandle<EnvironmentDeletedMessage>,
                                     IHandle<AddServerToExplorerMessage>,
                                     IHandle<RefreshExplorerMessage>,
                                     IHandle<SetSelectedItemInExplorerTree>
    {


        #region Class Members

        readonly IAsyncWorker _asyncWorker;
        private RelayCommand _environmentChangedCommand;
        private Guid? _context;
        System.Action _onLoadResourcesCompletedOnceOff;
        NavigationViewModel _navigationViewModel;

        #endregion Class Members

        #region Constructor

        public ExplorerViewModel(IEventAggregator eventPublisher, IAsyncWorker asyncWorker, IEnvironmentRepository environmentRepository,IStudioResourceRepository studioResourceRepository, bool isFromActivityDrop = false, enDsfActivityType activityType = enDsfActivityType.All, System.Action onLoadResourcesCompletedOnceOff = null)
            : base(eventPublisher)
        {

            VerifyArgument.IsNotNull("asyncWorker", asyncWorker);
            VerifyArgument.IsNotNull("environmentRepository", environmentRepository);
            _asyncWorker = asyncWorker;
            EnvironmentRepository = environmentRepository;
            NavigationViewModel = new NavigationViewModel(eventPublisher, asyncWorker, Context, environmentRepository,studioResourceRepository, isFromActivityDrop, activityType) { Parent = this };
            if(onLoadResourcesCompletedOnceOff != null)
            {
                _onLoadResourcesCompletedOnceOff = onLoadResourcesCompletedOnceOff;
                NavigationViewModel.LoadResourcesCompleted += LoadResourcesCompletedOnceOff;
            }
        }

        void LoadResourcesCompletedOnceOff(object sender, EventArgs e)
        {
            NavigationViewModel.LoadResourcesCompleted -= LoadResourcesCompletedOnceOff;
            try
            {
                _onLoadResourcesCompletedOnceOff();
            }
            finally
            {
                _onLoadResourcesCompletedOnceOff = null;
            }
        }

        #endregion Constructor

        #region Commands

        public ICommand EnvironmentChangedCommand
        {
            get
            {
                ICommand command = _environmentChangedCommand ?? (_environmentChangedCommand = new RelayCommand(param => AddEnvironment((IEnvironmentModel)param), param => true));
                return command;
            }
        }

        #endregion Commands

        #region Properties
        [Import]
        public IFilePersistenceProvider FilePersistenceProvider { get; set; }

        [Import]
        public IWindowManager WindowManager { get; set; }

        public IEnvironmentRepository EnvironmentRepository { get; private set; }

        public NavigationViewModel NavigationViewModel
        {
            get
            {
                return _navigationViewModel;
            }
            set
            {
                _navigationViewModel = value;
            }
        }

        public Guid? Context
        {
            get
            {
                return _context ?? (_context = Guid.NewGuid());
            }
        }

        #endregion Properties

        #region Private Methods

        private void AddEnvironment(IEnvironmentModel environmentModel, bool forceConnect = false)
        {
            if(environmentModel != null)
            {
                _asyncWorker.Start(
                    () =>
                    {
                        if(forceConnect)
                        {
                            environmentModel.Connect();
                        }
                    },
                    () =>
                    {
                    });
            }
        }

        private void SaveEnvironment(IEnvironmentModel environmentModel)
        {
            EnvironmentRepository.Save(environmentModel);
            EnvironmentRepository.WriteSession(NavigationViewModel.Environments.Select(e => e.ID));
        }

        private void RemoveEnvironment(IEnvironmentModel environment)
        {
            NavigationViewModel.RemoveEnvironment(environment);
            SaveEnvironment(environment);
        }

        /// <summary>
        ///     Reloads all connected environemnts resources
        /// </summary>
        private void RefreshEnvironments(bool addMissingEnvironments)
        {
            NavigationViewModel.RefreshEnvironments();

            //
            // Ensure all environments are added to the navigation view model
            //
            if(!addMissingEnvironments) return;

            foreach(IEnvironmentModel environment in EnvironmentRepository.All())
            {
                NavigationViewModel.AddEnvironment(environment);
            }
        }

        /// <summary>
        ///     Loads the environments from the resource repository
        /// </summary>
        public void LoadEnvironments(bool setActiveEnvironment = true)
        {
            if(EnvironmentRepository == null) return;

            //
            // Load environments from repository
            //
            if(!EnvironmentRepository.IsLoaded)
                EnvironmentRepository.Load();

            // Load the default environment
            NavigationViewModel.AddEnvironment(EnvironmentRepository.Source);
            this.TraceInfo("Publish message of type - " + typeof(SetActiveEnvironmentMessage));
            if(setActiveEnvironment)
            {
                EventPublisher.Publish(new SetActiveEnvironmentMessage(EnvironmentRepository.Source));
            }

            //
            // Add last session's environments to the navigation view model
            //
            var sessionGuids = EnvironmentRepository.ReadSession();
            if(sessionGuids != null && sessionGuids.Count > 0)
            {
                ICollection<IEnvironmentModel> environmentModels = EnvironmentRepository.All();
                if(environmentModels.Count > 0)
                {
                    foreach(var environment in environmentModels.Where(e => sessionGuids.Contains(e.ID)))
                    {
                        NavigationViewModel.AddEnvironment(environment);
                    }
                }
            }
        }

        #endregion Private Methods

        #region Dispose Handling

        protected override void OnDispose()
        {
            if(NavigationViewModel != null)
            {
                NavigationViewModel.Dispose();
                NavigationViewModel = null;
            }
            base.OnDispose();
        }

        #endregion Dispose Handling

        #region IHandle

        public void Handle(SetSelectedItemInExplorerTree message)
        {
            //            this.TraceInfo(message.GetType().Name);
            //            // ReSharper disable OperatorIsCanBeUsed
            //            List<ITreeNode> treeNodes = NavigationViewModel.Root.GetChildren(c => c.GetType() == typeof(EnvironmentTreeViewModel) && c.DisplayName.Contains(message.NodeNameToSelect)).ToList();
            //            // ReSharper restore OperatorIsCanBeUsed
            //            if(treeNodes.Count == 1)
            //            {
            //                treeNodes[0].IsSelected = true;
            //            }
        }

        public void Handle(RefreshExplorerMessage message)
        {
            this.TraceInfo(message.GetType().Name);
            NavigationViewModel.UpdateWorkspaces();
        }

        public void Handle(UpdateExplorerMessage message)
        {
            this.TraceInfo(message.GetType().Name);
            RefreshEnvironments(message.Update);
        }

        public void Handle(RemoveEnvironmentMessage message)
        {
            this.TraceInfo(message.GetType().Name);
            RemoveEnvironment(message.EnvironmentModel);
        }

        public void Handle(AddServerToExplorerMessage message)
        {
            this.TraceInfo(message.GetType().Name);
            AddEnvironment(message.EnvironmentModel, message.ForceConnect);
        }

        public void Handle(EnvironmentDeletedMessage message)
        {
            this.TraceInfo(message.GetType().Name);
            RemoveEnvironment(message.EnvironmentModel);
        }

        #endregion  IHandle

        public void BringItemIntoView(WorkSurfaceContextViewModel item)
        {
            if(NavigationViewModel != null)
            {
                if(item != null && item.ContextualResourceModel != null)
                {
                    NavigationViewModel.BringItemIntoView(item.ContextualResourceModel);
                }
            }
        }
    }
}