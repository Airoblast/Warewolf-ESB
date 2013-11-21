﻿using System.Drawing;
using System.Linq;
using System.Threading;
using Dev2.Studio.UI.Tests;
using Dev2.Studio.UI.Tests.UIMaps.DebugUIMapClasses;
using Microsoft.VisualStudio.TestTools.UITesting;
using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;

namespace Dev2.CodedUI.Tests.UIMaps.RibbonUIMapClasses
{
    public partial class RibbonUIMap : UIMapBase
    {
        #region Mappings

        private DebugUIMap _debugUIMap;

        public DebugUIMap DebugUIMap
        {
            get
            {
                if((_debugUIMap == null))
                {
                    _debugUIMap = new DebugUIMap();
                }
                return _debugUIMap;
            }
            set { _debugUIMap = value; }
        }

        #endregion

        int loopCount = 0;
        WpfTabList uIRibbonTabList;

        public void ClickRibbonMenu(string menuAutomationId)
        {
            // This needs some explaining :)
            // Due to the way the WpfRibbon works, unless it has been used, the default tab will not be properly initialised.
            // This will cause problems if you need to use it (Automation wise)
            // To combat this, we check if the tab we have got is actually a valid tab (Check its bounding)
            // If it's not a valid tab, we click an alternate tab - This validates our original tab.
            // We then recusrsively call the method again with the now validated tab, and it works as itended.
            // Note: This recursive call will only happen the first time the ribbon is used, as it will subsequently be initialised correctly.

            if(uIRibbonTabList == null)
            {
                uIRibbonTabList = this.UIBusinessDesignStudioWindow.UIRibbonTabList;
            }

            UITestControlCollection tabList = uIRibbonTabList.Tabs;
            UITestControl theControl = new UITestControl();
            foreach(WpfTabPage tabPage in tabList)
            {
                if(tabPage.Name == menuAutomationId)
                {
                    theControl = tabPage;
                    Point p = new Point(theControl.BoundingRectangle.X + 5, theControl.BoundingRectangle.Y + 5);
                    p = new Point(theControl.GetChildren()[0].BoundingRectangle.X + 20, theControl.GetChildren()[0].BoundingRectangle.Y + 10);
                    Mouse.Click(p);
                    return;
                }
                else
                {
                    theControl = tabPage;
                    Point p = new Point(theControl.BoundingRectangle.X + 5, theControl.BoundingRectangle.Y + 5);
                    if(p.X > 5)
                    {
                        Mouse.Click(p);
                        break;
                    }
                }
            }

            // Somethign has gone wrong - Retry!

            loopCount++; // This was added due to the infinite loop happening if the ribbon was totally unclickable due to a crash
            if(loopCount < 10)
            {
                ClickRibbonMenu(menuAutomationId);
            }
        }

        public void ClickRibbonMenuItem(string itemName)
        {
            var ribbonButtons = UIBusinessDesignStudioWindow.GetChildren();
            var control = ribbonButtons.FirstOrDefault(c => c.FriendlyName == itemName || c.GetChildren().Any(child => child.FriendlyName == itemName));
            var p = new Point(control.BoundingRectangle.X + 5, control.BoundingRectangle.Y + 5);
            Mouse.Click(p);
            Playback.Wait(1000);
        }

        public void CreateNewWorkflow()
        {
            var uiTestControlCollection = UIBusinessDesignStudioWindow.GetChildren();
            var control = uiTestControlCollection.FirstOrDefault(c => c.FriendlyName == "UI_RibbonHomeTabWorkflowBtn_AutoID");
            var p = new Point(control.BoundingRectangle.Left + 5, control.BoundingRectangle.Top + 5);
            Mouse.Click(p);
            Playback.Wait(500);

        }

        public UITestControl GetControlByName(string name)
        {
            var children = UIBusinessDesignStudioWindow.GetChildren();
            var control = children.FirstOrDefault(c => c.FriendlyName == name || c.GetChildren().Any(child => child.FriendlyName == name));

            return control;
        }
    }
}
