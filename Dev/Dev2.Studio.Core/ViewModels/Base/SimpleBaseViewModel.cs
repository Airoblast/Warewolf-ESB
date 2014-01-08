﻿using System;
using Caliburn.Micro;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.ViewModels.Base
{
    public abstract class SimpleBaseViewModel : Screen, IDisposable
    {
        #region Class Members

        private bool _closeRequested;
        private ViewModelDialogResults _viewModelResults = ViewModelDialogResults.Cancel;

        private bool _isDisposed;

        #endregion Class Members

        private ValidationController _validationController;

        public ValidationController ValidationController
        {
            get { return _validationController ?? (_validationController = new ValidationController()); }
            set
            {
                if(_validationController == value)
                    return;

                _validationController = value;
                NotifyOfPropertyChange(() => ValidationController);
            }
        }

        #region Protected Methods

        protected virtual void OnPropertyChanged(string propertyName)
        {
            NotifyOfPropertyChange(propertyName);
        }

        #endregion Protected Methods

        #region IDisposable Members



        /// <summary>
        /// Child classes can override this method to perform 
        /// clean-up logic, such as removing event handlers.
        /// </summary>
        protected virtual void OnDispose()
        {
            if(_validationController != null)
            {
                _validationController.Dispose();
            }
        }


        ~SimpleBaseViewModel()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        public void Dispose()
        {
            Dispose(true);

        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if(!_isDisposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if(disposing)
                {
                    // Dispose managed resources.
                    OnDispose();
                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                _isDisposed = true;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Requests tha the view bound to this view model closes
        /// </summary>
        public virtual void RequestClose()
        {
            RequestClose(ViewModelDialogResults.Cancel);
        }

        /// <summary>
        /// Requests tha the view bound to this view model closes
        /// </summary>
        public void RequestClose(ViewModelDialogResults dialogResult)
        {
            DialogResult = dialogResult;
            CloseRequested = true;
        }

        #endregion Methods

        #region Properties

        /// <summary>
        /// Indicates if a close has been requested
        /// </summary>
        public bool CloseRequested
        {
            get
            {
                return _closeRequested;
            }
            private set
            {
                _closeRequested = value;
                NotifyOfPropertyChange(() => CloseRequested);
            }
        }

        public ViewModelDialogResults DialogResult
        {
            get
            {
                return _viewModelResults;
            }
            set
            {
                _viewModelResults = value;
                NotifyOfPropertyChange(() => DialogResult);
            }
        }

        #endregion Properties
    }
}
