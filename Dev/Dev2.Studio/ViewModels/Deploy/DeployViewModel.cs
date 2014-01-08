﻿using Caliburn.Micro;
using Dev2.AppResources.DependencyInjection.EqualityComparers;
using Dev2.Messages;
using Dev2.Providers.Logs;
using Dev2.Services.Events;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.InterfaceImplementors;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Core.ViewModels.Navigation;
using Dev2.Studio.Deploy;
using Dev2.Studio.TO;
using Dev2.Studio.ViewModels.Navigation;
using Dev2.Studio.ViewModels.WorkSurface;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Input;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.ViewModels.Deploy
{
    public class DeployViewModel : BaseWorkSurfaceViewModel,
        IHandle<ResourceCheckedMessage>, IHandle<UpdateDeployMessage>,
        IHandle<SelectItemInDeployMessage>, IHandle<AddServerToDeployMessage>,
        IHandle<EnvironmentDeletedMessage>
    {
        #region Class Members

        private IDeployStatsCalculator _deployStatsCalculator;

        private IEnvironmentModelProvider _serverProvider;

        private NavigationViewModel _source;
        private NavigationViewModel _target;

        private ObservableCollection<IEnvironmentModel> _servers;
        private IEnvironmentModel _selectedSourceServer;
        private IEnvironmentModel _selectedDestinationServer;

        private ObservableCollection<DeployStatsTO> _sourceStats;
        private ObservableCollection<DeployStatsTO> _targetStats;

        private IEnvironmentModel _sourceEnvironment;
        private IEnvironmentModel _targetEnvironment;

        private Dictionary<string, Func<ITreeNode, bool>> _sourceStatPredicates;
        private Dictionary<string, Func<ITreeNode, bool>> _targetStatPredicates;

        private string _initialItemDisplayName;
        private IEnvironmentModel _initialItemEnvironment;
        private bool _isDeploying;
        private bool _deploySuccessfull;
        private bool _initialLoad = true;
        private bool _selectingAndExpandingFromNavigationItem;

        private int _sourceDeployItemCount;
        private int _destinationDeployItemCount;
        private Guid? _destinationContext;
        private Guid? _sourceContext;

        #endregion Class Members

        #region Constructor

        public DeployViewModel()
            : this(ServerProvider.Instance, Core.EnvironmentRepository.Instance, EventPublishers.Aggregator)
        {
        }

        public DeployViewModel(IEnvironmentModelProvider serverProvider, IEnvironmentRepository environmentRepository, IEventAggregator eventAggregator, IDeployStatsCalculator deployStatsCalculator = null, string displayName = null, IEnvironmentModel environment = null)
            : base(eventAggregator)
        {
            _initialItemEnvironment = environment;
            _initialItemDisplayName = displayName;
            Initialize(serverProvider, environmentRepository, deployStatsCalculator);
        }

        public DeployViewModel(string displayName, IEnvironmentModel environment)
            : this(ServerProvider.Instance, Core.EnvironmentRepository.Instance, EventPublishers.Aggregator, null, displayName, environment)
        {
        }

        #endregion

        #region Commands

        public ICommand SelectAllDependanciesCommand
        {
            get;
            private set;
        }

        public ICommand DeployCommand
        {
            get;
            private set;
        }

        public ICommand SourceServerChangedCommand
        {
            get;
            private set;
        }

        public ICommand TargetServerChangedCommand
        {
            get;
            private set;
        }

        #endregion

        #region Properties

        public Guid? SourceContext
        {
            get
            {
                return _sourceContext ?? (_sourceContext = Guid.NewGuid());
            }
        }

        public Guid? DestinationContext
        {
            get
            {
                return _destinationContext ?? (_destinationContext = Guid.NewGuid());
            }
        }

        [Import(typeof(IWindowManager))]
        public IWindowManager WindowManager { get; set; }

        public IEnvironmentRepository EnvironmentRepository { get; private set; }

        public bool CanDeploy
        {
            get
            {
                return SelectedDestinationServerIsValid() && HasItemsToDeploy() && !IsDeploying && ServersAreNotTheSame;
            }
        }

        public bool ServersAreNotTheSame
        {
            get
            {
                return (SelectedDestinationServer == null || SelectedSourceServer == null) || (SelectedDestinationServer.Connection.AppServerUri != SelectedSourceServer.Connection.AppServerUri);
            }
        }

        bool HasItemsToDeploy()
        {
            return _sourceDeployItemCount > 0 && _destinationDeployItemCount > 0;
        }

        bool SelectedDestinationServerIsValid()
        {
            return SelectedDestinationServer != null && SelectedDestinationServer.IsConnected;
        }

        public bool SourceItemsSelected
        {
            get
            {
                return _sourceDeployItemCount > 0;
            }
        }

        public IEnvironmentModel SourceEnvironment
        {
            get
            {
                return _sourceEnvironment;
            }
            private set
            {
                _sourceEnvironment = value;
                NotifyOfPropertyChange(() => SourceEnvironment);
            }
        }

        public IEnvironmentModel TargetEnvironment
        {
            get
            {
                return _targetEnvironment;
            }
            private set
            {
                _targetEnvironment = value;
                NotifyOfPropertyChange(() => TargetEnvironment);
            }
        }

        /// <summary>
        /// Used to indicate a successfull deploy has happened
        /// </summary>
        public bool DeploySuccessfull
        {
            get
            {
                return _deploySuccessfull;
            }
            private set
            {
                _deploySuccessfull = value;
                NotifyOfPropertyChange(() => DeploySuccessfull);
            }
        }

        /// <summary>
        /// Used to indicate if a deploy is in progress
        /// </summary>
        public bool IsDeploying
        {
            get
            {
                return _isDeploying;
            }
            private set
            {
                _isDeploying = value;
                NotifyOfPropertyChange(() => IsDeploying);
                NotifyOfPropertyChange(() => CanDeploy);
            }
        }

        public ObservableCollection<IEnvironmentModel> Servers
        {
            get
            {
                return _servers;
            }
        }

        public ObservableCollection<DeployStatsTO> TargetStats
        {
            get
            {
                return _targetStats;
            }
        }

        public ObservableCollection<DeployStatsTO> SourceStats
        {
            get
            {
                return _sourceStats;
            }
        }

        public NavigationViewModel Source
        {
            get
            {
                return _source;
            }
            set
            {
                if(value == _source) return;

                _source = value;
                NotifyOfPropertyChange(() => Source);
            }
        }

        public NavigationViewModel Target
        {
            get
            {
                return _target;
            }
            set
            {
                if(value == _target) return;
                _target = value;
                NotifyOfPropertyChange(() => Target);
            }
        }

        public IEnvironmentModel SelectedSourceServer
        {
            get
            {
                return _selectedSourceServer;
            }
            set
            {
                _selectedSourceServer = value;
                LoadSourceEnvironment(_selectedSourceServer);
                NotifyOfPropertyChange(() => SelectedDestinationServer);
                NotifyOfPropertyChange(() => SourceItemsSelected);
                NotifyOfPropertyChange(() => CanDeploy);
                NotifyOfPropertyChange(() => ServersAreNotTheSame);
            }
        }

        public IEnvironmentModel SelectedDestinationServer
        {
            get
            {
                return _selectedDestinationServer;
            }
            set
            {
                // ReSharper disable once PossibleUnintendedReferenceComparison
                if(value != _selectedDestinationServer)
                {
                    _selectedDestinationServer = value;
                    LoadDestinationEnvironment(_selectedDestinationServer);
                    NotifyOfPropertyChange(() => SelectedDestinationServer);
                    NotifyOfPropertyChange(() => SourceItemsSelected);
                    NotifyOfPropertyChange(() => CanDeploy);
                    NotifyOfPropertyChange(() => ServersAreNotTheSame);
                }
            }
        }

        #endregion

        #region Private Methods

        private void Initialize(IEnvironmentModelProvider serverProvider, IEnvironmentRepository environmentRepository, IDeployStatsCalculator deployStatsCalculator = null)
        {
            EnvironmentRepository = environmentRepository;

            _deployStatsCalculator = deployStatsCalculator ?? new DeployStatsCalculator();
            // ReSharper disable once ObjectCreationAsStatement
            new DeployService();
            _serverProvider = serverProvider;
            _servers = new ObservableCollection<IEnvironmentModel>();
            _targetStats = new ObservableCollection<DeployStatsTO>();
            _sourceStats = new ObservableCollection<DeployStatsTO>();

            Target = new NavigationViewModel(DestinationContext) { Parent = this };
            Source = new NavigationViewModel(SourceContext) { Parent = this };

            SetupPredicates();
            SetupCommands();
            LoadServers();
        }

        /// <summary>
        /// Refreshes the resources for all environments
        /// </summary>
        private void RefreshEnvironments()
        {
            Source.RefreshEnvironments();
            Target.RefreshEnvironments();

        }

        /// <summary>
        /// Recalculates
        /// </summary>
        private void CalculateStats()
        {
            DeploySuccessfull = false;

            var items = _source.Root.GetChildren(null).OfType<ResourceTreeViewModel>().ToList();
            _deployStatsCalculator.CalculateStats(items, _sourceStatPredicates, _sourceStats, out _sourceDeployItemCount);
            _deployStatsCalculator.CalculateStats(items, _targetStatPredicates, _targetStats, out _destinationDeployItemCount);
            NotifyOfPropertyChange(() => CanDeploy);
            NotifyOfPropertyChange(() => SourceItemsSelected);
        }


        /// <summary>
        /// Create the predicates which are to be used when generating deploy stats
        /// </summary>
        private void SetupPredicates()
        {
            var exclusionCategories = new List<string> { "Website", "Human Interface Workflow", "Webpage" };
            var websiteCategories = new List<string> { "Website" };
            var webpageCategories = new List<string> { "Human Interface Workflow", "Webpage" };
            var blankCategories = new List<string>();

            _sourceStatPredicates = new Dictionary<string, Func<ITreeNode, bool>>();
            _targetStatPredicates = new Dictionary<string, Func<ITreeNode, bool>>();

            _sourceStatPredicates.Add("Services",
                                      n => _deployStatsCalculator
                                          .SelectForDeployPredicateWithTypeAndCategories
                                          (n, ResourceType.Service, blankCategories, exclusionCategories));
            _sourceStatPredicates.Add("Workflows",
                                      n => _deployStatsCalculator.
                                               SelectForDeployPredicateWithTypeAndCategories
                                               (n, ResourceType.WorkflowService, blankCategories, exclusionCategories));
            _sourceStatPredicates.Add("Sources",
                                      n => _deployStatsCalculator
                                               .SelectForDeployPredicateWithTypeAndCategories
                                               (n, ResourceType.Source, blankCategories, exclusionCategories));
            _sourceStatPredicates.Add("Webpages",
                                      n => _deployStatsCalculator
                                               .SelectForDeployPredicateWithTypeAndCategories
                                               (n, ResourceType.WorkflowService, webpageCategories, blankCategories));
            _sourceStatPredicates.Add("Websites",
                                      n => _deployStatsCalculator
                                               .SelectForDeployPredicateWithTypeAndCategories
                                               (n, ResourceType.WorkflowService, websiteCategories, blankCategories));
            _sourceStatPredicates.Add("Unknown",
                                      n => _deployStatsCalculator.SelectForDeployPredicate(n));
            _targetStatPredicates.Add("New Resources",
                                      n => _deployStatsCalculator
                                               .DeploySummaryPredicateNew(n, TargetEnvironment));
            _targetStatPredicates.Add("Override",
                                      n => _deployStatsCalculator
                                               .DeploySummaryPredicateExisting(n, TargetEnvironment));
        }

        /// <summary>
        /// Create and assign commands
        /// </summary>
        private void SetupCommands()
        {
            DeployCommand = new RelayCommand(o => Deploy(), o => CanDeploy);
            SelectAllDependanciesCommand = new RelayCommand(SelectAllDependancies);
            SourceServerChangedCommand = new RelayCommand(s =>
            {
                SelectedSourceServer = s as IEnvironmentModel;
            });

            TargetServerChangedCommand = new RelayCommand(s =>
            {
                SelectedDestinationServer = s as IEnvironmentModel;
            });
        }

        /// <summary>
        /// Selects all dependancies of the selected resources
        /// </summary>
        /// <param name="obj"></param>
        void SelectAllDependancies(object obj)
        {
            List<ResourceTreeViewModel> resourceTreeViewModels = _source.Root.GetChildren(null).OfType<ResourceTreeViewModel>().ToList();
            List<ResourceTreeViewModel> selectedResourcesTreeViewModels = resourceTreeViewModels.Where(c => c.IsChecked == true).ToList();
            List<IContextualResourceModel> selectedResourceModels = new List<IContextualResourceModel>();
            foreach(var resourceTreeViewModel in selectedResourcesTreeViewModels)
            {
                selectedResourceModels.Add(resourceTreeViewModel.DataContext);
            }

            List<string> dependancyNames = SourceEnvironment.ResourceRepository.GetDependanciesOnList(selectedResourceModels, SourceEnvironment);

            foreach(var dependant in dependancyNames)
            {
                ITreeNode treeNode = _source.Root.GetChildren(null).FirstOrDefault(c => c.DisplayName == dependant);
                if(treeNode != null)
                {
                    treeNode.IsChecked = true;
                }
            }
        }

        /// <summary>
        /// Loads the available servers from the server provider
        /// </summary>
        private void LoadServers()
        {
            List<IEnvironmentModel> servers = _serverProvider.Load();
            Servers.Clear();

            foreach(var server in servers)
            {
                Servers.Add(server);
            }

            if(servers.Count > 0)
            {
                //
                // Find a source server to select
                //
                SelectedSourceServer = servers.FirstOrDefault(s => ServerEqualityComparer.Current.Equals(s, SelectedSourceServer));

                if(SelectedSourceServer == null && _initialLoad)
                {
                    SelectSourceServerFromInitialValue();
                    _initialLoad = false;
                }
            }
        }

        /// <summary>
        /// Adds a new server
        /// </summary>
        private void AddServer(IEnvironmentModel server, bool connectSource, bool connectTarget)
        {
            server.Connect();

            Servers.Add(server);

            if(connectSource)
            {
                SelectedSourceServer = server;
            }
            if(connectTarget)
            {
                SelectedDestinationServer = server;
            }
            EventPublisher.Publish(new UpdateSelectedServer(server, connectSource));
        }

        /// <summary>
        /// Deploys the selected items
        /// </summary>
        private void Deploy()
        {

            //
            //Get the resources to deploy
            //
            var resourcesToDeploy = Source.Root.GetChildren
                (_deployStatsCalculator.SelectForDeployPredicate)
                                          .Where(n => n is ResourceTreeViewModel).Cast<ResourceTreeViewModel>()
                                          .Select(n => n.DataContext as IResourceModel).ToList();

            var deployResourceRepo = SourceEnvironment.ResourceRepository;

            if(resourcesToDeploy.Count <= 0 || deployResourceRepo == null)
            {
                return;
            }

            //
            // Deploy the resources
            //
            var deployDto = new DeployDto { ResourceModels = resourcesToDeploy };

            try
            {
                IsDeploying = true;

                deployResourceRepo.DeployResources(SourceEnvironment, TargetEnvironment, deployDto, EventPublisher);

                //
                // Reload the environments resources & update explorer
                //
                LoadDestinationEnvironment(SelectedDestinationServer);

                DeploySuccessfull = true;
            }
            finally
            {
                IsDeploying = false;
            }
        }

        /// <summary>
        /// Loads an environment for the source navigation manager
        /// </summary>
        private void LoadSourceEnvironment(IEnvironmentModel server)
        {
            // BUG 9276 : TWR : 2013.04.19
            SourceEnvironment = EnvironmentRepository.Fetch(server);

            Source.RemoveAllEnvironments();

            if(SourceEnvironment != null)
            {
                if(_selectingAndExpandingFromNavigationItem)
                {
                    Source.LoadResourcesCompleted += OnResourcesLoaded;
                }

                Source.AddEnvironment(SourceEnvironment);
            }
        }

        void OnResourcesLoaded(object source, EventArgs args)
        {
            //2013.08.27: Ashley Lewis for bug 10225 - handle race condition and detach
            SelectAndExpandFromInitialValue();
            CalculateStats();
            Source.LoadResourcesCompleted -= OnResourcesLoaded;
        }

        /// <summary>
        /// Loads an environment for the target navigation manager
        /// </summary>
        private void LoadDestinationEnvironment(IEnvironmentModel server)
        {
            // BUG 9276 : TWR : 2013.04.19
            TargetEnvironment = EnvironmentRepository.Fetch(server);

            Target.RemoveAllEnvironments();

            if(TargetEnvironment != null)
            {
                Target.AddEnvironment(TargetEnvironment);
            }

            CalculateStats();
        }

        /// <summary>
        /// Selects the server from initial value.
        /// </summary>
        private void SelectSourceServerFromInitialValue()
        {
            _selectingAndExpandingFromNavigationItem = true;

            IEnvironmentModel environment = null;

            if(_initialItemDisplayName != null && _initialItemEnvironment != null)
            {
                environment = _initialItemEnvironment;
            }

            if(environment != null)
            {
                var server = Servers.FirstOrDefault(s => ServerEqualityComparer.Current.Equals(s, environment));
                //
                // Setting the SelectedSourceServer will run the LoadSourceEnvironment method, 
                // which takes care of selecting and expanding the correct node
                //
                SelectedSourceServer = server;
            }
            _selectingAndExpandingFromNavigationItem = false;
        }

        /// <summary>
        /// Selects the and expand the correct node from the from initial navigation item view model.
        /// </summary>
        private void SelectAndExpandFromInitialValue()
        {
            ITreeNode navigationItemViewModel = null;

            if(_initialItemDisplayName != null)
            {
                navigationItemViewModel = Source.Root.GetChildren(n => n.DisplayName == _initialItemDisplayName)
                    .FirstOrDefault();
            }

            if(navigationItemViewModel == null) return;

            //
            // Select and expand the initial node
            //
            navigationItemViewModel.IsChecked = true;

            var parent = navigationItemViewModel.TreeParent;
            if(parent != null)
            {
                parent.IsExpanded = true;
            }
        }

        #endregion Private Methods

        #region Dispose Handling

        protected override void OnDispose()
        {
            EventPublishers.Aggregator.Unsubscribe(this);
            base.OnDispose();
        }

        #endregion Dispose Handling

        #region IHandle

        public void Handle(ResourceCheckedMessage message)
        {
            Logger.TraceInfo(message.GetType().Name);
            CalculateStats();
        }

        public void Handle(UpdateDeployMessage message)
        {
            Logger.TraceInfo(message.GetType().Name);
            RefreshEnvironments();
        }

        public void Handle(SelectItemInDeployMessage message)
        {
            Logger.TraceInfo(message.GetType().Name);
            _initialItemDisplayName = message.DisplayName;
            _initialItemEnvironment = message.Environment;
            SelectSourceServerFromInitialValue();
        }

        public void Handle(AddServerToDeployMessage message)
        {
            Logger.TraceInfo(message.GetType().Name);
            if(message.Context != null)
            {
                var ctx = message.Context;
                if(ctx.Equals(SourceContext))
                {
                    AddServer(message.Server, true, false);
                }
                else if(ctx.Equals(DestinationContext))
                {
                    AddServer(message.Server, false, true);
                }
            }
            else
            {
                AddServer(message.Server, message.IsSource, message.IsDestination);
            }
        }

        public void Handle(EnvironmentDeletedMessage message)
        {
            Logger.TraceInfo(message.GetType().Name);
            if(Source != null)
            {
                Source.RemoveEnvironment(message.EnvironmentModel);
            }

            if(Target != null)
            {
                Target.RemoveEnvironment(message.EnvironmentModel);
            }
        }
        #endregion

        #region Public Methods

        public void SelectDependencies(IContextualResourceModel resource)
        {
            if(resource != null)
            {
                List<string> dependancyNames = SourceEnvironment.ResourceRepository.GetDependanciesOnList(new List<IContextualResourceModel> { resource }, SourceEnvironment);
                dependancyNames.Add(resource.ResourceName);
                foreach(var dependant in dependancyNames)
                {
                    ITreeNode treeNode = _source.Root.GetChildren(null).FirstOrDefault(c => c.DisplayName == dependant);
                    if(treeNode != null)
                    {
                        treeNode.IsChecked = true;
                    }
                }
            }
        }

        #endregion
    }


}
