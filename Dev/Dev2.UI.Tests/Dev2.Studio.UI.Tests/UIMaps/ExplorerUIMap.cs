﻿using System;
using System.Linq;
using System.Threading;
using Dev2.Studio.UI.Tests.Utils;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.CodedUI.Tests.UIMaps.ExplorerUIMapClasses
{
    using Microsoft.VisualStudio.TestTools.UITesting;
    using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Windows.Input;
    using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;
    using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
    using MouseButtons = System.Windows.Forms.MouseButtons;


    public partial class ExplorerUIMap
    {
        private UITestControl _explorerTree;
        private UITestControl _explorerSearch;
        private UITestControl _explorerNewConnectionControl;
        private UITestControl _explorerRefresh;

        public ExplorerUIMap()
        {
            var vstw = new VisualTreeWalker();

            _explorerTree = vstw.GetControlFromRoot(0, false, 1, "Uia.SplitPane", "Zf1166e575b5d43bb89f15f346eccb7b1", "Z3d0e8544bdbd4fbc8b0369ecfce4e928", "Explorer", "UI_ExplorerPane_AutoID", "Explorer", "TheNavigationView", "Navigation");
            _explorerSearch = vstw.GetControlFromRoot(0, false, 1, "Uia.SplitPane", "Zf1166e575b5d43bb89f15f346eccb7b1", "Z3d0e8544bdbd4fbc8b0369ecfce4e928", "Explorer", "UI_ExplorerPane_AutoID", "Explorer", "TheNavigationView", "FilterTextBox", "UI_DataListSearchtxt_AutoID");
            _explorerNewConnectionControl = vstw.GetControlFromRoot(0, false, 1, "Uia.SplitPane", "Zf1166e575b5d43bb89f15f346eccb7b1", "Z3d0e8544bdbd4fbc8b0369ecfce4e928", "Explorer", "UI_ExplorerPane_AutoID", "Explorer", "ConnectUserControl");
            _explorerRefresh = vstw.GetControlFromRoot(0, false, 1, "Uia.SplitPane", "Zf1166e575b5d43bb89f15f346eccb7b1", "Z3d0e8544bdbd4fbc8b0369ecfce4e928", "Explorer", "UI_ExplorerPane_AutoID", "Explorer", "TheNavigationView", "UI_SourceServerRefreshbtn_AutoID");
        }

        public UITestControlCollection GetCategoryItems()
        {
            return GetNavigationItemCategories();
        }

        /// <summary>
        /// Gets the navigation item categories.
        /// </summary>
        /// <returns></returns>
        public UITestControlCollection GetNavigationItemCategories()
        {
            UITestControlCollection categories = _explorerTree.GetChildren();

            UITestControlCollection categoryCollection = new UITestControlCollection();

            foreach(UITestControl category in categories)
            {
                if(category.ControlType.ToString() == "TreeItem")
                {
                    categoryCollection.Add(category);
                }
            }

            return categoryCollection;
        }

        /// <summary>
        /// Gets the navigation items.
        /// </summary>
        /// <returns></returns>
        public UITestControlCollection GetServiceItems()
        {
            UITestControlCollection categories = _explorerTree.GetChildren();

            UITestControlCollection categoryCollection = new UITestControlCollection();

            foreach(UITestControl category in categories)
            {
                if(category.ControlType.ToString() == "TreeItem")
                {
                    var kids = category.GetChildren();
                    foreach (var kid in kids)
                    {
                        if (kid.ControlType.ToString() == "TreeItem")
    {
                            categoryCollection.Add(kid);
                        }
                    }
                    
                }
            }

            return categoryCollection;
        }


        public UITestControl GetConnectControl(string controlType)
        {
            var kids = _explorerNewConnectionControl.GetChildren();

            if (kids != null)
            {
                return kids.FirstOrDefault(c => c.ControlType.Name == controlType);
            }

            return null;
            }

        public UITestControl GetLocalServer()
        {
            var firstTreeChild = _explorerTree.GetChildren()[0];
            return firstTreeChild.GetChildren()[3];
        }


        public void ClickRefreshButton()
        {
            UITestControl window = UIBusinessDesignStudioWindow;
            var findDocManager = window.GetChildren();
            UITestControl docManager = findDocManager[3];
            var findSplitPane = docManager.GetChildren();
            UITestControl splitPane = findSplitPane[3];
            var findExplorerPane = splitPane.GetChildren()[0].GetChildren()[0].GetChildren();
            UITestControl explorerPane = findExplorerPane[3].GetChildren()[6];
            var findRefreshButton = explorerPane.GetChildren()[1].GetChildren();
            UITestControl refreshButton = findRefreshButton[2];
            Mouse.Click(refreshButton, new Point(5, 5));
        }

        public void ClickConnectDropdown()
        {
            UITestControl window = UIBusinessDesignStudioWindow;
            var findDocManager = window.GetChildren();
            UITestControl docManager = findDocManager[3];
            var findSplitPane = docManager.GetChildren();
            UITestControl splitPane = findSplitPane[3];
            var findExplorerPane = splitPane.GetChildren()[0].GetChildren()[0].GetChildren();
            UITestControl explorerPane = findExplorerPane[3].GetChildren()[6];
            var findNewServerButton = explorerPane.GetChildren()[0].GetChildren();
            UITestControl connectDropdown = findNewServerButton[1];
            Mouse.Click(connectDropdown, new Point(5, 5));
        }

        public void DoubleClickOpenProject(string serverName, string serviceType, string folderName, string projectName)
        {
            UITestControl theControl = GetServiceItem(serverName, serviceType, folderName, projectName);
            if (theControl != null)
            {
                Point p = new Point(theControl.BoundingRectangle.X + 60, theControl.BoundingRectangle.Y + 7);
                Playback.Wait(50);
                Mouse.Click(p);
                Playback.Wait(50);
                Mouse.DoubleClick(p);
                Playback.PlaybackSettings.WaitForReadyLevel = WaitForReadyLevel.AllThreads;
                theControl.WaitForControlReady();
                Playback.PlaybackSettings.WaitForReadyLevel = WaitForReadyLevel.UIThreadOnly;
            }
        }

        public bool ValidateServiceExists(string serverName, string serviceType, string folderName, string projectName)
        {
            UITestControl theControl = GetServiceItem(serverName, serviceType, folderName, projectName);
            Point p = new Point(theControl.BoundingRectangle.X + 50, theControl.BoundingRectangle.Y + 5);
            Mouse.Click(p);

            var kids = theControl.GetChildren();

            if (kids != null && kids.Count > 0)
            {
                return kids.Any(kid => kid.Name == projectName);
            }

            return false;
        }

        public void RightClickDeployProject(string serverName, string serviceType, string folderName, string projectName)
        {
            UITestControl theControl = GetServiceItem(serverName, serviceType, folderName, projectName);
            Point p = new Point(theControl.BoundingRectangle.X + 50, theControl.BoundingRectangle.Y + 5);
            Mouse.Move(p);
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            Thread.Sleep(1500);
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            Thread.Sleep(1500);
            SendKeys.SendWait("{DOWN}");
            Thread.Sleep(100);
            SendKeys.SendWait("{DOWN}");
            Thread.Sleep(100);
            SendKeys.SendWait("{ENTER}");

            // Add some focus, and dleay to make sure everything is fine
            Thread.Sleep(2500);
            Mouse.Click(new Point(Screen.PrimaryScreen.WorkingArea.Width - 10, Screen.PrimaryScreen.WorkingArea.Height / 2));
            Thread.Sleep(2500);
        }

        /// <summary>
        /// Clicks the server information server DDL.
        /// </summary>
        /// <param name="serverName">Name of the server.</param>
        public void ClickServerInServerDDL(string serverName)
        {
            // Get the base control
            UITestControl ddlBase = GetServerDDL();

            // Click it to expand it
            Mouse.Click(ddlBase, new Point(10, 10));
            Playback.Wait(500);

            VisualTreeWalker vsw = new VisualTreeWalker();
            var item = vsw.GetChildByAutomationIDPath(ddlBase, "U_UI_ExplorerServerCbx_AutoID_" + serverName);

            Mouse.Click(item, new Point(5, 5));
        }

        /// <summary>
        /// Selecteds the name of the sever.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        public string GetSelectedSeverName()
        {
            UITestControl ddlBase = GetServerDDL();
          
            var theDdl = ddlBase as WpfComboBox;
            var firstItemInDdl = theDdl.Items[theDdl.SelectedIndex] as WpfListItem;

            if(firstItemInDdl == null)
            {
                string message = string.Format("No selected server name where found on the explorer server combo box");
                throw new Exception(message);
            }

            return firstItemInDdl.AutomationId.Replace("U_UI_ExplorerServerCbx_AutoID_", "");
        }

        /// <summary>
        /// Counts the servers.
        /// </summary>
        /// <returns></returns>
        public int CountServers()
        {
            WpfTree uITvExplorerTree = this.UIBusinessDesignStudioWindow.UIExplorerCustom.UINavigationViewUserCoCustom.UITvExplorerTree;
            return uITvExplorerTree.GetChildren().Count;
        }

        public void RightClickDeleteProject(string serverName, string serviceType, string folderName, string projectName)
        {
            UITestControl theControl = GetServiceItem(serverName, serviceType, folderName, projectName);
            Point p = new Point(theControl.BoundingRectangle.X + 50, theControl.BoundingRectangle.Y + 5);
            Mouse.Move(p);
            Playback.Wait(500);
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            Playback.Wait(1000);
            SendKeys.SendWait("{DOWN}");
            Playback.Wait(100);
            SendKeys.SendWait("{DOWN}");
            Playback.Wait(100);
            SendKeys.SendWait("{DOWN}");
            Playback.Wait(100);
            SendKeys.SendWait("{DOWN}");
            Playback.Wait(100);
            SendKeys.SendWait("{ENTER}");
            PopupDialogUIMap.WaitForDialog();
            Playback.Wait(100);
            var confirmationDialog = UIBusinessDesignStudioWindow.GetChildren()[0];
            var yesButton = confirmationDialog.GetChildren().FirstOrDefault(c => c.FriendlyName == "Yes");
            Mouse.Click(yesButton, new Point(10, 10));
        }

        public bool ServiceExists(string serverName, string serviceType, string folderName, string projectName)
        {
            UITestControl theControl = GetServiceItem(serverName, serviceType, folderName, projectName);
            return theControl != null && theControl.Exists;
        }

        /// <summary>
        /// Navigates to a Workflow Item in the Explorer, Right Clicks it, and clicks Properties
        /// </summary>
        /// <param name="serverName">The name of the server (EG: localhost)</param>
        /// <param name="serviceType">The name of the service (EG: WORKFLOWS)</param>
        /// <param name="folderName">The name of the folder (AKA: Category - EG: BARNEY (Or CODEDUITESTCATEGORY for the CodedUI Test Default))</param>
        /// <param name="projectName">The name of the project (EG: MyWorkflow)</param>
        public void RightClickProperties(string serverName, string serviceType, string folderName, string projectName)
        {
            UITestControl theControl = GetServiceItem(serverName, serviceType, folderName, projectName);
            Point p = new Point(theControl.BoundingRectangle.X + 50, theControl.BoundingRectangle.Y + 5);
            Mouse.Move(p);
            Playback.Wait(500);
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            Playback.Wait(1500);
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            Playback.Wait(1500);
            SendKeys.SendWait("{UP}");
            Playback.Wait(100);
            SendKeys.SendWait("{ENTER}");
            Playback.Wait(100);
        }

        public void Server_RightClick_Disconnect(string serverName)
        {
            UITestControl theServer = GetServer(serverName);
            Mouse.Move(theServer, new Point(50, 5));
            Playback.Wait(500);
            Mouse.Click(MouseButtons.Right);
            Playback.Wait(500);
            Keyboard.SendKeys("{Down}");
            Playback.Wait(500);
            Keyboard.SendKeys("{Enter}");
            Playback.Wait(500);
        }

        public void Server_RightClick_Connect(string serverName)
        {
            UITestControl theServer = GetServer(serverName);
            Point p = new Point(theServer.BoundingRectangle.X + 50, theServer.BoundingRectangle.Y + 5);
            Mouse.Move(p);
            Playback.Wait(500);
            Mouse.Move(theServer, new Point(50, 5));
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            Playback.Wait(1500);
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            Playback.Wait(1500);
            Keyboard.SendKeys("{Down}");
            Playback.Wait(500);
            Keyboard.SendKeys("{Enter}");
            Playback.Wait(500);
        }
        
        public void Server_RightClick_NewWorkflow(string serverName)
        {
            UITestControl theServer = GetServer(serverName);
            Point p = new Point(theServer.BoundingRectangle.X + 50, theServer.BoundingRectangle.Y + 5);
            Mouse.Move(p);
            Playback.Wait(500);
            Mouse.Move(theServer, new Point(50, 5));
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            Playback.Wait(1500);
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            Playback.Wait(1500);
            Keyboard.SendKeys("{Down}");
            Playback.Wait(500);
            Keyboard.SendKeys("{Right}");
            Playback.Wait(500);
            Keyboard.SendKeys("{Enter}");
        }
        public void Server_RightClick_NewDatabaseService(string serverName)
        {
            UITestControl theServer = GetServer(serverName);
            Point p = new Point(theServer.BoundingRectangle.X + 50, theServer.BoundingRectangle.Y + 5);
            Mouse.Move(p);
            Playback.Wait(500);
            Mouse.Move(theServer, new Point(50, 5));
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            Playback.Wait(1500);
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            Playback.Wait(1500);
            Keyboard.SendKeys("{Down}");
            Playback.Wait(500);
            Keyboard.SendKeys("{Right}");
            Playback.Wait(500);
            Keyboard.SendKeys("{Down}");
            Playback.Wait(500);
            Keyboard.SendKeys("{Right}");
            Playback.Wait(500);
            Keyboard.SendKeys("{Enter}");
        }
        
        public void Server_RightClick_NewPluginService(string serverName)
        {
            UITestControl theServer = GetServer(serverName);
            Point p = new Point(theServer.BoundingRectangle.X + 50, theServer.BoundingRectangle.Y + 5);
            Mouse.Move(p);
            Playback.Wait(500);
            Mouse.Move(theServer, new Point(50, 5));
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            Playback.Wait(1500);
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            Playback.Wait(1500);
            Keyboard.SendKeys("{Down}");
            Playback.Wait(500);
            Keyboard.SendKeys("{Right}");
            Playback.Wait(500);
            Keyboard.SendKeys("{Down}");
            Playback.Wait(500);
            Keyboard.SendKeys("{Right}");
            Playback.Wait(500);
            Keyboard.SendKeys("{Down}");
            Playback.Wait(500);
            Keyboard.SendKeys("{Enter}");
        }

        public void Server_RightClick_Delete(string serverName)
        {
            UITestControl theServer = GetServer(serverName);
            Point p = new Point(theServer.BoundingRectangle.X + 50, theServer.BoundingRectangle.Y + 5);
            Mouse.Move(p);
            Playback.Wait(500);
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            Playback.Wait(1500);
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            Playback.Wait(1500);
            Keyboard.SendKeys("{Down}");
            Playback.Wait(500);
            Keyboard.SendKeys("{Down}");
            Playback.Wait(500);
            Keyboard.SendKeys("{Enter}");
            Playback.Wait(500);
        }

        public void ConnectedServer_RightClick_Delete(string serverName)
        {
            UITestControl theServer = GetServer(serverName);
            Point p = new Point(theServer.BoundingRectangle.X + 50, theServer.BoundingRectangle.Y + 5);
            Mouse.Move(p);
            Playback.Wait(500);
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            Playback.Wait(2500);
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            Playback.Wait(2500);
            Keyboard.SendKeys("{Down}");
            Playback.Wait(500);
            Keyboard.SendKeys("{Down}");
            Playback.Wait(500);
            Keyboard.SendKeys("{Down}");
            Playback.Wait(500);
            Keyboard.SendKeys("{Enter}");
            Playback.Wait(500);
        }

        [Ignore]
        // Not functioning right now ;)
        public void RightClickShowProjectDependancies(string serverName, string serviceType, string folderName, string projectName)
        {
            UITestControl theControl = GetServiceItem(serverName, serviceType, folderName, projectName);
            theControl.WaitForControlEnabled();
            Point p = new Point(theControl.BoundingRectangle.X + 50, theControl.BoundingRectangle.Y + 5);
            Mouse.Move(p);
            Playback.Wait(500);
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            Playback.Wait(500);
            Keyboard.SendKeys("{Down}");
            Keyboard.SendKeys("{Down}");
            Keyboard.SendKeys("{Down}");
            Keyboard.SendKeys("{Down}");
            Keyboard.SendKeys("{Down}");
            Keyboard.SendKeys("{Enter}");
            Playback.Wait(100);
        }

        public void RightClickHelp(string serverName, string serviceType, string folderName, string projectName)
        {
            UITestControl theControl = GetServiceItem(serverName, serviceType, folderName, projectName);
            Point p = new Point(theControl.BoundingRectangle.X + 50, theControl.BoundingRectangle.Y + 5);
            Mouse.Move(p);
            Thread.Sleep(500);
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            Thread.Sleep(500);
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            Thread.Sleep(500);
            Keyboard.SendKeys("{Down}");
            Keyboard.SendKeys("{Down}");
            Keyboard.SendKeys("{Down}");
            Keyboard.SendKeys("{Down}");
            Keyboard.SendKeys("{Down}");
            Keyboard.SendKeys("{Down}");
            Keyboard.SendKeys("{Down}");
            Keyboard.SendKeys("{Down}");
            Keyboard.SendKeys("{Down}");
            Keyboard.SendKeys("{Enter}");
        }

        public UITestControl GetServer(string serverName)
        {
            return GetConnectedServer(serverName);
        }

        public UITestControl GetServiceType(string serverName, string serviceType)
        {
            return GetServiceTypeControl(serverName, serviceType);
        }

        /// <summary>
        /// Drags the control automatic workflow designer.
        /// </summary>
        /// <param name="serverName">Name of the server.</param>
        /// <param name="serviceType">Type of the service.</param>
        /// <param name="folderName">Name of the folder.</param>
        /// <param name="projectName">Name of the project.</param>
        /// <param name="p">The application.</param>
        /// <param name="overrideDblClickBehavior">if set to <c>true</c> [override double click behavior].</param>
        public void DragControlToWorkflowDesigner(string serverName, string serviceType, string folderName, string projectName, Point p, bool overrideDblClickBehavior = false)
        {
            UITestControl theControl = GetServiceItem(serverName, serviceType, folderName, projectName,overrideDblClickBehavior);
            Mouse.StartDragging(theControl);
            Mouse.StopDragging(p);
            Playback.Wait(100);
        }

        /// <summary>
        /// Rights the click rename project.
        /// </summary>
        /// <param name="serverName">Name of the server.</param>
        /// <param name="serviceType">Type of the service.</param>
        /// <param name="folderName">Name of the folder.</param>
        /// <param name="projectName">Name of the project.</param>
        public void RightClickRenameProject(string serverName, string serviceType, string folderName, string projectName)
        {
            UITestControl theControl = GetServiceItem(serverName, serviceType, folderName, projectName);
            Point p = new Point(theControl.BoundingRectangle.X + 50, theControl.BoundingRectangle.Y + 5);
            Mouse.Move(p);
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            Playback.Wait(500);
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            Playback.Wait(500);
            SendKeys.SendWait("{DOWN}");
            Playback.Wait(100);
            SendKeys.SendWait("{DOWN}");
            Playback.Wait(100);
            SendKeys.SendWait("{DOWN}");
            Playback.Wait(100);
            SendKeys.SendWait("{ENTER}");
            Playback.Wait(500);
        }

        public void ClickNewServerButton()
        {
            var firstConnectControlButton = GetConnectControl("Button");
            var nextConnectControlButtonPosition = new Point(firstConnectControlButton.Left + 35, firstConnectControlButton.Top + 10);
            Mouse.Click(nextConnectControlButtonPosition);
            Playback.Wait(200);
        }
    }
}
