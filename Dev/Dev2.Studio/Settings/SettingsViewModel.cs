﻿using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Windows.Input;
using Caliburn.Micro;
using Dev2.Common;
using Dev2.Services.Events;
using Dev2.Settings.Logging;
using Dev2.Settings.Security;
using Dev2.Studio.Controller;
using Dev2.Studio.Core.Controller;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.ViewModels.WorkSurface;
using Dev2.Threading;

namespace Dev2.Settings
{
    public class SettingsViewModel : BaseWorkSurfaceViewModel
    {
        bool _isLoading;
        bool _isDirty;
        bool _selectionChanging;
        bool _hasErrors;
        string _errors;
        bool _isSaved;

        bool _showLogging;
        bool _showSecurity = true;

        readonly IPopupController _popupController;
        readonly IAsyncWorker _asyncWorker;
        readonly IWin32Window _parentWindow;

        SecurityViewModel _securityViewModel;
        LoggingViewModel _loggingViewModel;

        public SettingsViewModel()
            : this(EventPublishers.Aggregator, new PopupController(), new AsyncWorker(), (IWin32Window)System.Windows.Application.Current.MainWindow)
        {
        }

        public SettingsViewModel(IEventAggregator eventPublisher, IPopupController popupController, IAsyncWorker asyncWorker, IWin32Window parentWindow)
            : base(eventPublisher)
        {
            Settings = new Data.Settings.Settings();
            VerifyArgument.IsNotNull("popupController", popupController);
            _popupController = popupController;
            VerifyArgument.IsNotNull("asyncWorker", asyncWorker);
            _asyncWorker = asyncWorker;
            VerifyArgument.IsNotNull("parentWindow", parentWindow);
            _parentWindow = parentWindow;

            SaveCommand = new RelayCommand(o => SaveSettings(), o => IsDirty);
            ServerChangedCommand = new RelayCommand(OnServerChanged, o => true);
        }

        public ICommand SaveCommand { get; private set; }

        public ICommand ServerChangedCommand { get; private set; }

        public IEnvironmentModel CurrentEnvironment { get; private set; }

        public bool IsSavedSuccessVisible { get { return !HasErrors && !IsDirty && IsSaved; } }

        public bool IsErrorsVisible { get { return HasErrors || (IsDirty && !IsSaved); } }

        public bool HasErrors
        {
            get { return _hasErrors; }
            set
            {
                if(value.Equals(_hasErrors))
                {
                    return;
                }
                _hasErrors = value;
                NotifyOfPropertyChange(() => HasErrors);
                NotifyOfPropertyChange(() => IsSavedSuccessVisible);
                NotifyOfPropertyChange(() => IsErrorsVisible);
            }
        }

        public string Errors
        {
            get { return _errors; }
            set
            {
                if(value == _errors)
                {
                    return;
                }
                _errors = value;
                NotifyOfPropertyChange(() => Errors);
            }
        }

        public bool IsSaved
        {
            get { return _isSaved; }
            set
            {
                if(value.Equals(_isSaved))
                {
                    return;
                }
                _isSaved = value;
                NotifyOfPropertyChange(() => IsSaved);
                NotifyOfPropertyChange(() => IsSavedSuccessVisible);
                NotifyOfPropertyChange(() => IsErrorsVisible);
            }
        }

        public bool IsDirty
        {
            get { return _isDirty; }
            set
            {
                if(value.Equals(_isDirty))
                {
                    return;
                }
                _isDirty = value;
                NotifyOfPropertyChange(() => IsDirty);
                NotifyOfPropertyChange(() => IsSavedSuccessVisible);
                NotifyOfPropertyChange(() => IsErrorsVisible);
            }
        }

        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                if(value.Equals(_isLoading))
                {
                    return;
                }
                _isLoading = value;
                NotifyOfPropertyChange(() => IsLoading);
            }
        }

        public bool ShowLogging
        {
            get { return _showLogging; }
            set
            {
                if(value.Equals(_showLogging))
                {
                    return;
                }
                _showLogging = value;
                OnSelectionChanged();
                NotifyOfPropertyChange(() => ShowLogging);
            }
        }

        public bool ShowSecurity
        {
            get { return _showSecurity; }
            set
            {
                if(value.Equals(_showSecurity))
                {
                    return;
                }
                _showSecurity = value;
                OnSelectionChanged();
                NotifyOfPropertyChange(() => ShowSecurity);
            }
        }

        public Data.Settings.Settings Settings { get; private set; }

        public SecurityViewModel SecurityViewModel
        {
            get { return _securityViewModel; }
            private set
            {
                if(Equals(value, _securityViewModel))
                {
                    return;
                }
                _securityViewModel = value;
                NotifyOfPropertyChange(() => SecurityViewModel);
            }
        }

        public LoggingViewModel LoggingViewModel
        {
            get { return _loggingViewModel; }
            private set
            {
                if(Equals(value, _loggingViewModel))
                {
                    return;
                }
                _loggingViewModel = value;
                NotifyOfPropertyChange(() => LoggingViewModel);
            }
        }

        void OnSelectionChanged([CallerMemberName] string propertyName = null)
        {
            if(_selectionChanging)
            {
                return;
            }

            _selectionChanging = true;
            switch(propertyName)
            {
                case "ShowLogging":
                    ShowSecurity = !ShowLogging;
                    break;

                case "ShowSecurity":
                    // TODO: Remove this when logging is enabled!
                    if(!ShowSecurity)
                    {
                        ShowSecurity = true;
                    }
                    // TODO: Add this when logging is enabled!
                    //ShowLogging = !ShowSecurity;
                    break;
            }
            _selectionChanging = false;
        }

        void OnServerChanged(object obj)
        {
            var server = obj as IEnvironmentModel;
            if(server == null)
            {
                return;
            }

            if(!server.IsConnected)
            {
                server.CanStudioExecute = false;
                _popupController.ShowNotConnected();
                return;
            }

            CurrentEnvironment = server;
            LoadSettings();
        }

        void LoadSettings()
        {
            ClearErrors();
            IsSaved = false;
            IsDirty = false;
            IsLoading = true;

            _asyncWorker.Start(() =>
            {
                Settings = ReadSettings();
            }, () =>
            {
                IsLoading = false;

                if(Settings == null)
                {
                    return;
                }

                SecurityViewModel = CreateSecurityViewModel();
                LoggingViewModel = CreateLoggingViewModel();

                AddPropertyChangedHandlers();

                if(Settings.HasError)
                {
                    ShowError("Load Error", Settings.Error);
                }
            });
        }

        protected virtual SecurityViewModel CreateSecurityViewModel()
        {
            return new SecurityViewModel(Settings.Security, _parentWindow, CurrentEnvironment);
        }

        protected virtual LoggingViewModel CreateLoggingViewModel()
        {
            return new LoggingViewModel();
        }

        void AddPropertyChangedHandlers()
        {
            var isDirtyProperty = DependencyPropertyDescriptor.FromProperty(SettingsItemViewModel.IsDirtyProperty, typeof(SettingsItemViewModel));

            isDirtyProperty.AddValueChanged(SecurityViewModel, OnIsDirtyPropertyChanged);
            isDirtyProperty.AddValueChanged(LoggingViewModel, OnIsDirtyPropertyChanged);
        }

        void OnIsDirtyPropertyChanged(object sender, EventArgs eventArgs)
        {
            IsDirty = SecurityViewModel.IsDirty;
            ClearErrors();
        }

        void ResetIsDirtyForChildren()
        {
            SecurityViewModel.IsDirty = false;
        }

        void SaveSettings()
        {
            // Need to reset sub view models so that selecting something in them fires our OnIsDirtyPropertyChanged()
            ResetIsDirtyForChildren();
            ClearErrors();

            SecurityViewModel.Save(Settings.Security);

            var isWritten = WriteSettings();
            if(isWritten)
            {
                IsSaved = true;
                IsDirty = false;
            }
            else
            {
                IsSaved = false;
                IsDirty = true;
            }
        }

        bool WriteSettings()
        {
            var payload = CurrentEnvironment.ResourceRepository.WriteSettings(CurrentEnvironment, Settings);
            if(payload == null)
            {
                ShowError("Network Error", string.Format(GlobalConstants.NetworkCommunicationErrorTextFormat, "WriteSettings"));
                return false;
            }
            if(payload.HasError)
            {
                ShowError("Save Error", payload.Message.ToString());
                return false;
            }
            return true;
        }

        Data.Settings.Settings ReadSettings()
        {
            var payload = CurrentEnvironment.ResourceRepository.ReadSettings(CurrentEnvironment);
            if(payload == null)
            {
                ShowError("Network Error", string.Format(GlobalConstants.NetworkCommunicationErrorTextFormat, "ReadSettings"));
            }

            return payload;
        }


        protected void ClearErrors()
        {
            HasErrors = false;
            Errors = null;
        }

        protected virtual void ShowError(string header, string description)
        {
            HasErrors = true;
            Errors = description;
            //throw new Exception(string.Format("{0} : {1}", header, description));
        }
    }
}

