﻿using System;
using Caliburn.Micro;
using Dev2.Common;
using Dev2.Providers.Logs;
using Dev2.Services.Events;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;

// ReSharper disable once CheckNamespace
namespace Dev2.Webs
{

    public class WebController : IHandle<CloseWizardMessage>, IWebController
    {
        public WebController()
        {
            EventPublishers.Aggregator.Subscribe(this);
        }

        public void DisplayDialogue(IContextualResourceModel resourceModel, bool includeArgs)
        {
            if(RootWebSite.ShowDialog(resourceModel))
            {
            }
        }

        public void CloseWizard()
        {
            throw new NotImplementedException();
        }

        #region IHandle

        public void Handle(ShowWebpartWizardMessage message)
        {
            Dev2Logger.Log.Info(message.GetType().Name);
        }

        public void Handle(CloseWizardMessage message)
        {
            Dev2Logger.Log.Info(message.GetType().Name);
            CloseWizard();
        }

        public void Handle(SetActivePageMessage message)
        {
            Dev2Logger.Log.Info(message.GetType().Name);
        }
        #endregion IHandle

    }
}
