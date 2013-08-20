﻿#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Caliburn.Micro;
using Dev2.Composition;
using Dev2.Services.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.DependencyInjection.EqualityComparers;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Core.ViewModels.Navigation;
using Dev2.Studio.Core.Wizards.Interfaces;
using Unlimited.Applications.BusinessDesignStudio.Views;

#endregion

namespace Dev2.Studio.ViewModels.Navigation
{
    /// <summary>
    /// The viewmodel for a treenode representing an environment
    /// </summary>
    /// <date>2013/01/23</date>
    /// <author>
    /// Jurie.smit
    /// </author>
    public sealed class EnvironmentTreeViewModel : AbstractTreeViewModel
    //,IHandle<UpdateActiveEnvironmentMessage>
    {
        #region private fields

        private RelayCommand _connectCommand;
        private RelayCommand _disconnectCommand;
        private IEnvironmentModel _environmentModel;
        private RelayCommand _removeCommand;
        private RelayCommand<string> _newResourceCommand;
        private RelayCommand _refreshCommand;
        WebPropertyEditorWindow _win;

        #endregion

        #region ctor + init
        //, ImportService.GetExportValue<IWizardEngine>()
        public EnvironmentTreeViewModel(ITreeNode parent, IEnvironmentModel environmentModel)
            : this(parent, environmentModel, EventPublishers.Aggregator)
        {
        }
        //, IWizardEngine wizardEngine
        public EnvironmentTreeViewModel(ITreeNode parent, IEnvironmentModel environmentModel, IEventAggregator eventPublisher)
            : base(null, eventPublisher)
        {
            EnvironmentModel = environmentModel;
            IsExpanded = true;
            if(parent != null)
            {
                parent.Add(this);
            }
        }

        #endregion

        #region public properties

        public override bool CanRefresh
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets the icon path.
        /// </summary>
        /// <value>
        /// The icon path.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public override string IconPath
        {
            get { return StringResources.Navigation_Environment_Icon_Pack_Uri; }
        }

        /// <summary>
        /// Gets the display name.
        /// </summary>
        /// <value>
        /// The display name.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public override string DisplayName
        {
            get
            {
                return EnvironmentModel == null
                           ? String.Empty
                           : string.Format("{0} ({1})", EnvironmentModel.Name,
                                           (EnvironmentModel.Connection.AppServerUri == null)
                                               ? String.Empty
                                               : EnvironmentModel.Connection.AppServerUri.AbsoluteUri);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is connected.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is connected; otherwise, <c>false</c>.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public override bool IsConnected
        {
            get { return (EnvironmentModel != null) && EnvironmentModel.IsConnected; }
        }

        /// <summary>
        /// Gets or sets the environment model for this instance.
        /// </summary>
        /// <value>
        /// The environment model.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public override sealed IEnvironmentModel EnvironmentModel
        {
            get { return _environmentModel; }
            protected set
            {
                if(_environmentModel != null)
                {
                    // BUG 9940 - 2013.07.29 - TWR - added
                    _environmentModel.IsConnectedChanged -= OnEnvironmentModelIsConnectedChanged;
                }
                _environmentModel = value;
                if(_environmentModel != null)
                {
                    // BUG 9940 - 2013.07.29 - TWR - added
                    _environmentModel.IsConnectedChanged += OnEnvironmentModelIsConnectedChanged;
                }
                NotifyOfPropertyChange(() => EnvironmentModel);
                NotifyOfPropertyChange(() => IsConnected);
            }
        }

        void OnEnvironmentModelIsConnectedChanged(object sender, ConnectedEventArgs args)
        {
            // BUG 9940 - 2013.07.29 - TWR - added
            NotifyOfPropertyChange(() => IsConnected);

            if(IsRefreshing && !args.IsConnected)
            {
                var rootNavigationViewModel = FindRootNavigationViewModel() as NavigationViewModel;
                if(rootNavigationViewModel != null && !rootNavigationViewModel.IsRefreshing)
                {
                    IsRefreshing = false;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance can connect.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance can connect; otherwise, <c>false</c>.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public override bool CanConnect
        {
            get
            {
                return EnvironmentModel != null
                    && (!EnvironmentModel.IsConnected || !EnvironmentModel.CanStudioExecute);
            }
        }

        public override bool HasFileMenu
        {
            get
            {
                return EnvironmentModel != null
                    && EnvironmentModel.IsConnected;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has executable commands.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has executable commands; otherwise, <c>false</c>.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public override bool HasExecutableCommands
        {
            get
            {
                return base.HasExecutableCommands ||
                    CanConnect || CanDisconnect || CanRemove;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance can disconnect.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance can disconnect; otherwise, <c>false</c>.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public override bool CanDisconnect
        {
            get
            {
                return EnvironmentModel != null &&
                       EnvironmentModel.Connection != null &&
                       EnvironmentModel.IsConnected &&
                       EnvironmentModel.Name != StringResources.DefaultEnvironmentName;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether this instance can be removed.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance can remove; otherwise, <c>false</c>.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public override bool CanRemove
        {
            get { return EnvironmentModel.ID != Guid.Empty; }
        }

        public override ObservableCollection<ITreeNode> Children
        {
            get
            {
                if(_children == null)
                {
                    _children = new SortedObservableCollection<ITreeNode>();
                    _children.CollectionChanged += ChildrenOnCollectionChanged;
                }
                return _children;
            }
            set
            {
                if(_children == value) return;

                _children = value;
                _children.CollectionChanged -= ChildrenOnCollectionChanged;
                _children.CollectionChanged += ChildrenOnCollectionChanged;
            }
        }

        public override ICommand RenameCommand
        {
            get
            {
                //not implimented
                return null;
            }
        }

        #endregion public properties

        #region Commands

        /// <summary>
        /// Gets the refresh command.
        /// </summary>
        /// <value>
        /// The refresh command.
        /// </value>
        /// <author>Massimo.Guerrera</author>
        /// <date>2013/06/20</date>
        public override ICommand RefreshCommand
        {
            get
            {
                return _refreshCommand ??
                        (_refreshCommand = new RelayCommand(param => RefreshEnvironment(), o => CanRefresh));
            }
        }

        /// <summary>
        /// Gets the connect command.
        /// </summary>
        /// <value>
        /// The connect command.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public override ICommand ConnectCommand
        {
            get
            {
                return _connectCommand ??
                       (_connectCommand = new RelayCommand(param => Connect(), o => CanConnect));
            }
        }

        public override ICommand NewResourceCommand
        {
            get
            {
                return _newResourceCommand ??
                       (_newResourceCommand = new RelayCommand<string>((s)
                           => _eventPublisher.Publish(new ShowNewResourceWizard(s)), o => HasFileMenu));
            }
        }

        /// <summary>
        /// Gets the disconnect command.
        /// </summary>
        /// <value>
        /// The disconnect command.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public override ICommand DisconnectCommand
        {
            get
            {
                return _disconnectCommand ?? (_disconnectCommand = new RelayCommand(param =>
                                                                       Disconnect(), param => CanDisconnect));
            }
        }

        /// <summary>
        /// Gets the remove command.
        /// </summary>
        /// <value>
        /// The remove command.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public override ICommand RemoveCommand
        {
            get { return _removeCommand ?? (_removeCommand = new RelayCommand(parm => Remove(), o => CanRemove)); }
        }

        #endregion Commands

        #region public methods

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is filtered from the tree.
        ///     Always false for environment node
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is filtered; otherwise, <c>false</c>.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public override bool IsFiltered
        {
            get
            {
                return false;
            }
            set
            {
                //Do Nothing
            }
        }

        /// <summary>
        ///     Finds the environmentmodel for the treeparent
        /// </summary>
        /// <typeparam name="T">Type of the resource to find</typeparam>
        /// <param name="resourceToFind">The resource to find.</param>
        /// <returns></returns>
        /// <date>2013/01/23</date>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public override ITreeNode FindChild<T>(T resourceToFind)
        {
            if(resourceToFind is IEnvironmentModel)
                if(EnvironmentModelEqualityComparer.Current.Equals(EnvironmentModel, resourceToFind))
                    return this;
            return base.FindChild(resourceToFind);
        }

        #endregion

        #region private methods

        bool _isRefreshValid = true;

        /// <summary>
        /// Refreshes the environment.
        /// </summary>
        /// <author>Massimo.Guerrera</author>
        /// <date>2013/06/20</date>
        void RefreshEnvironment()
        {
            var rootNavigationViewModel = FindRootNavigationViewModel() as NavigationViewModel;
            if(rootNavigationViewModel != null)
            {
                rootNavigationViewModel.LoadEnvironmentResources(EnvironmentModel);
            }
        }

        /// <summary>
        /// Removes this instance.
        /// </summary>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        private void Remove()
        {
            if(EnvironmentModel == null) return;
            Disconnect();
            var rootVM = FindRootNavigationViewModel();
            var ctx = (rootVM == null) ? null : rootVM.Context;

            _eventPublisher.Publish(new RemoveEnvironmentMessage(EnvironmentModel, ctx));
            RaisePropertyChangedForCommands();
        }

        /// <summary>
        /// Connects this instance.
        /// </summary>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        private void Connect()
        {
            if(EnvironmentModel.IsConnected && EnvironmentModel.CanStudioExecute) return;

            EnvironmentModel.CanStudioExecute = true;
            EnvironmentModel.Connect();
            EnvironmentModel.ForceLoadResources();
            var vm = FindRootNavigationViewModel();
            vm.LoadEnvironmentResources(EnvironmentModel);
            RaisePropertyChangedForCommands();
        }

        /// <summary>
        /// Disconnects this instance.
        /// </summary>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        private void Disconnect()
        {
            if(!EnvironmentModel.IsConnected) return;

            EnvironmentModel.Disconnect();
            NotifyOfPropertyChange(() => IsConnected);
            RaisePropertyChangedForCommands();

            NavigationViewModel vm = FindRootNavigationViewModel() as NavigationViewModel;
            if(vm != null)
            {
                List<ITreeNode> treeNodes = vm.Root.GetChildren(c => c.DisplayName.Contains("localhost")).ToList();
                if(treeNodes.Count == 1 && treeNodes[0] is EnvironmentTreeViewModel)
                {
                    treeNodes[0].IsSelected = true;
                    _eventPublisher.Publish(new SetActiveEnvironmentMessage(treeNodes[0].EnvironmentModel));
                }
            }
        }
        #endregion

        public void Handle(CloseWizardMessage message)
        {
            throw new NotImplementedException();
        }

        //#region Implementation of IHandle<UpdateActiveEnvironmentMessage>

        //Juries Removed, we shouldnt need to set the active environment 
        //as selected just because its currently active?
        //public void Handle(UpdateActiveEnvironmentMessage message)
        //{
        //    if(Equals(_environmentModel, message.EnvironmentModel))
        //    {
        //        IsSelected = true;
        //    }
        //}


        protected override ITreeNode CreateParent(string displayName)
        {
            throw new NotImplementedException();
        }
    }


}
