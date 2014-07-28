﻿using Caliburn.Micro;
using Dev2.Composition;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.ViewModels.Base
{
    public enum ViewModelDialogResults
    {
        Okay,
        Cancel,
    }

    /// <summary>
    /// Base class for all ViewModel classes in the application.
    /// It provides support for property change notifications 
    /// and has a DisplayName property.  This class is abstract.
    /// </summary>
    public abstract class BaseViewModel : SimpleBaseViewModel
    {
        readonly IEventAggregator _eventPublisher;

        #region Constructor

        protected BaseViewModel(IEventAggregator eventPublisher)
        {
            VerifyArgument.IsNotNull("eventPublisher", eventPublisher);
            _eventPublisher = eventPublisher;
            _eventPublisher.Subscribe(this);

            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            SatisfyImports();
        }

        public IEventAggregator EventPublisher
        {
            get
            {
                return _eventPublisher;
            }
        }

        #endregion // Constructor

        protected override void OnDispose()
        {
            _eventPublisher.Unsubscribe(this);
            base.OnDispose();
        }

        #region Protected Virtual Methods

        protected virtual void SatisfyImports()
        {
            //For testing scenarios - ability to fail silently when everythings not imported
            ImportService.TrySatisfyImports(this);
        }

        #endregion Protected Virtual Methods
    }
}
