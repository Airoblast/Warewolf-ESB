﻿using System.Windows.Forms;
using Caliburn.Micro;
using Dev2.Settings;
using Dev2.Settings.Security;
using Dev2.Studio.Core.Controller;
using Dev2.Threading;

namespace Dev2.Core.Tests.Settings
{
    public class TestSettingsViewModel : SettingsViewModel
    {
        public TestSettingsViewModel()
        {
        }

        public TestSettingsViewModel(IEventAggregator eventPublisher, IPopupController popupController, IAsyncWorker asyncWorker, IWin32Window parentWindow)
            : base(eventPublisher, popupController, asyncWorker, parentWindow)
        {
        }

        public int ShowErrorHitCount { get; private set; }
        protected override void ShowError(string header, string description)
        {
            ShowErrorHitCount++;
            base.ShowError(header, description);
        }

        public SecurityViewModel TheSecurityViewModel { get; set; }
        protected override SecurityViewModel CreateSecurityViewModel()
        {
            return TheSecurityViewModel ?? base.CreateSecurityViewModel();
        }
    }
}
