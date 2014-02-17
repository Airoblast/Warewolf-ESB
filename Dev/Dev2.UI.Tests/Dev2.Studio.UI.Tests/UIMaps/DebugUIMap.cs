﻿namespace Dev2.Studio.UI.Tests.UIMaps.DebugUIMapClasses
{
    using System;
    using System.Drawing;
    using Microsoft.VisualStudio.TestTools.UITesting;
    using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
    using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
    using System.Windows.Forms;


    public partial class DebugUIMap : UIMapBase
    {
        public void ClickItem(int row)
        {
            WpfRow theRow = GetRow(row);
            WpfEdit requiredEdit = (WpfEdit)theRow.GetChildren()[2].GetChildren()[0];
            Mouse.Click(requiredEdit, new Point(5, 5));

        }

        public int CountRows()
        {
            WpfWindow debugWindow = GetDebugWindow();
            UITestControlCollection rowList = debugWindow.GetChildren()[1].GetChildren()[0].GetChildren()[1].GetChildren();
            return rowList.Count;
        }



        public void CloseDebugWindow_ByCancel()
        {
            WpfWindow debugWindow = GetDebugWindow();
            UITestControl theControl = new UITestControl(debugWindow);
            theControl.SearchProperties.Add("AutomationId", "UI_Cancelbtn_AutoID");
            theControl.Find();
            Mouse.Click(theControl, new Point(5, 5));
        }

        public void CloseDebugWindow()
        {
            SendKeys.SendWait("{ESCAPE}");
        }

        public void ClickExecute()
        {
            var debugWindow = GetDebugWindow();
            int counter = 0;
            while(debugWindow == null && counter < 5)
            {
                Playback.Wait(1000);
                debugWindow = GetDebugWindow();
                counter++;
            }
            if(debugWindow == null)
            {
                throw new Exception("The debug popup couldnt be found");
            }
            foreach(var child in debugWindow.GetChildren())
            {
                if(child.GetProperty("AutomationId").ToString() == "UI_Executebtn_AutoID")
                {
                    Mouse.Click(child);
                    break;
                }
            }
        }

        public bool DebugWindowExists()
        {
            WpfWindow uIDebugWindow = UIDebugWindow;
            Point p = new Point();
            if(uIDebugWindow.TryGetClickablePoint(out p))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void ClickXMLTab()
        {
            WpfWindow uIDebugWindow = this.UIDebugWindow;
            WpfTabPage XMLTabPage = (WpfTabPage)uIDebugWindow.GetChildren()[1].GetChildren()[1];
            Mouse.Click(XMLTabPage, new Point(5, 5));
        }

        public void ClickInputDataTab()
        {
            WpfWindow uIDebugWindow = this.UIDebugWindow;
            WpfTabPage TabPage = (WpfTabPage)uIDebugWindow.GetChildren()[1].GetChildren()[0];
            Mouse.Click(TabPage, new Point(5, 5));
        }

        /// <summary>
        /// Returns true if found in the timeout period.
        /// </summary>
        public bool WaitForDebugWindow(int timeOut)
        {
            const int waitLength = 100;
            Type type = null;
            var timeNow = 0;
            while(type != typeof(WpfWindow) & timeNow < timeOut)
            {
                timeNow = timeNow + waitLength;
                Playback.Wait(waitLength);
                var tryGetDialog = StudioWindow.GetChildren()[0];
                type = tryGetDialog.GetType();
                if(timeNow > timeOut)
                {
                    break;
                }
            }
            return type == typeof(WpfWindow);
        }
    }
}
