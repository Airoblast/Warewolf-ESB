﻿using System;
using System.Text;

namespace Dev2.Studio.Diagnostics
{
    public abstract class AppExceptionHandlerAbstract : IAppExceptionHandler
    {
        string _lastExceptionSignature;
        Exception _exception;
        bool _busy;

        #region Handle

        public bool Handle(Exception e)
        {
            if(e == null)
            {
                throw new ArgumentNullException("e");
            }
            if(_busy)
            {
                return true;
            }
            try
            {
                _exception = e;
                _busy = true;

                var popupController = CreatePopupController();
                var exceptionString = ToErrorString(_exception);
                var lastExceptionSignature = _lastExceptionSignature;
                _lastExceptionSignature = exceptionString;

                // Exception is critical if it is the same as the last one
                if(lastExceptionSignature != null && exceptionString == lastExceptionSignature)
                {
                    popupController.ShowPopup(_exception, ErrorSeverity.Critical);
                    RestartApp();
                }
                else
                {
                    popupController.ShowPopup(_exception, ErrorSeverity.Default);
                }
                return true;
            }
            catch
            {
                ShutdownApp();
                return false;
            }
            finally
            {
                _busy = false;
            }
        }

        #endregion

        #region ToErrorString

        protected string ToErrorString(Exception ex)
        {
            var errors = new StringBuilder();
            while(ex != null)
            {
                errors.Append(ex.GetType());
                errors.Append(ex.Message);
                ex = ex.InnerException;
            }
            return errors.ToString();
        }

        #endregion

        protected abstract IAppExceptionPopupController CreatePopupController();

        protected abstract void ShutdownApp();

        protected abstract void RestartApp();
    }
}