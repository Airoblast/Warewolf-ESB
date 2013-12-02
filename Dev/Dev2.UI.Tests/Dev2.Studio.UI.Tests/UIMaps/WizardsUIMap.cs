﻿using System;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;

namespace Dev2.Studio.UI.Tests.UIMaps
{
    public class WizardsUIMap : UIMapBase
    {
        private const int DefaultTimeOut = 5000;
        /// <summary>
        /// Returns true if found in the timeout period.
        /// </summary>
        public void WaitForWizard(int timeOut = DefaultTimeOut, bool throwIfNotFound = true)
        {
            Playback.Wait(1500);
        }


        public bool TryWaitForWizard(int timeOut)
        {
            Playback.Wait(1500);
            var tryGetDialog = StudioWindow.GetChildren()[0].GetChildren()[0];
            var type = tryGetDialog.GetType();
            return type == typeof(WpfImage);
        }
    }
}
