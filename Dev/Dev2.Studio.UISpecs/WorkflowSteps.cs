﻿using Dev2.Studio.UI.Tests;
using Dev2.Studio.UI.Tests.Extensions;
using Dev2.Studio.UI.Tests.UIMaps.Activities;
using Dev2.Studio.UI.Tests.Utils;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using TechTalk.SpecFlow;
using System.Threading;

namespace Dev2.Studio.UI.Specs
{
    [Binding]
    public class WorkflowSteps : UIMapBase
    {
        // ReSharper disable ConvertToConstant.Local
        // ReSharper disable UnusedMember.Local
#pragma warning disable 414
        static readonly string Explorer = "UI_DocManager_AutoID,UI_ExplorerPane_AutoID,UI_ExplorerControl_AutoID,UI_NavigationViewUserControl_AutoID";
        static readonly string ExplorerConnectControl = "UI_DocManager_AutoID,UI_ExplorerPane_AutoID,UI_ExplorerControl_AutoID,ConnectUserControl,UI_ExplorerServerCbx_AutoID";
        static readonly string ExplorerConnectProgress = "UI_DocManager_AutoID,UI_ExplorerPane_AutoID,UI_ExplorerControl_AutoID,ConnectUserControl,UI_IndicatorConnecting_AutoID";
        static readonly string Toolbox = "UI_DocManager_AutoID,UI_ToolboxPane_AutoID,UI_ToolboxControl_AutoID";
        static readonly string WorkflowDesigner = "UI_DocManager_AutoID,UI_SplitPane_AutoID,UI_TabManager_AutoID,UI_WorkflowDesigner_AutoID,UserControl_1,ActivityTypeDesigner,WorkflowItemPresenter";
        static readonly string Worksurface = WorkflowDesigner + ",Unsaved 1(FlowchartDesigner)";
        static readonly string DebugOutput = "MainViewWindow,UI_DocManager_AutoID,OutputPane,DebugOutput,DebugOutputTree";
        static readonly string ToolBoxSearch = Toolbox + ",PART_SearchBox";
        static readonly string TabActive = "ACTIVETAB,UI_WorkflowDesigner_AutoID,UserControl_1,scrollViewer,ActivityTypeDesigner,WorkflowItemPresenter";
        
        //Explorer
        static readonly string ExplorerFilter = Explorer + ",UI_DatalistFilterTextBox_AutoID,UI_TextBox_AutoID";
        static readonly string ExplorerFilterClearButton = Explorer + ",UI_DatalistFilterTextBox_AutoID,UI_FilterButton_AutoID";
        static readonly string ExplorerFolders = Explorer + ",UI_ExplorerTree_AutoID,UI_localhost_AutoID";
        //Tools
        static readonly string ToolMultiAssign = Toolbox + ",PART_Tools,Data,Unlimited.Applications.BusinessDesignStudio.Activities.DsfMultiAssignActivity";
        static readonly string ToolDataMerge = Toolbox + ",PART_Tools,Data,Unlimited.Applications.BusinessDesignStudio.Activities.DsfDataMergeActivity";
        static readonly string ToolBaseConvert = Toolbox + ",PART_Tools,Data,Unlimited.Applications.BusinessDesignStudio.Activities.DsfDataMergeActivity";
        static readonly string ToolCaseConvert = Toolbox + ",PART_Tools,Data,Unlimited.Applications.BusinessDesignStudio.Activities.DsfDataMergeActivity";
        static readonly string ToolFindIndex = Toolbox + ",PART_Tools,Data,Unlimited.Applications.BusinessDesignStudio.Activities.DsfDataMergeActivity";
        static readonly string ToolReplace = Toolbox + ",PART_Tools,Data,Unlimited.Applications.BusinessDesignStudio.Activities.DsfDataMergeActivity";
        static readonly string ToolCalculate = Toolbox + ",PART_Tools,Data,Unlimited.Applications.BusinessDesignStudio.Activities.DsfDataMergeActivity";
        static readonly string ToolFormatnumber = Toolbox + ",PART_Tools,Data,Unlimited.Applications.BusinessDesignStudio.Activities.DsfDataMergeActivity";
        static readonly string ToolRandom = Toolbox + ",PART_Tools,Data,Unlimited.Applications.BusinessDesignStudio.Activities.DsfDataMergeActivity";
        static readonly string ToolDateandtime = Toolbox + ",PART_Tools,Data,Unlimited.Applications.BusinessDesignStudio.Activities.DsfDataMergeActivity";
        static readonly string ToolDateandTimeDifference = Toolbox + ",PART_Tools,Data,Unlimited.Applications.BusinessDesignStudio.Activities.DsfDataMergeActivity";
        static readonly string ToolEmail = Toolbox + ",PART_Tools,Data,Unlimited.Applications.BusinessDesignStudio.Activities.DsfDataMergeActivity";
        static readonly string ToolSystemInfo = Toolbox + ",PART_Tools,Data,Unlimited.Applications.BusinessDesignStudio.Activities.DsfDataMergeActivity";
        static readonly string ToolXpath = Toolbox + ",PART_Tools,Data,Unlimited.Applications.BusinessDesignStudio.Activities.DsfDataMergeActivity";
        static readonly string ToolComment = Toolbox + ",PART_Tools,Data,Unlimited.Applications.BusinessDesignStudio.Activities.DsfDataMergeActivity";
        static readonly string ToolWebRequest = Toolbox + ",PART_Tools,Data,Unlimited.Applications.BusinessDesignStudio.Activities.DsfDataMergeActivity";
        static readonly string ToolCount = Toolbox + ",PART_Tools,Data,Unlimited.Applications.BusinessDesignStudio.Activities.DsfDataMergeActivity";
        //Settings Tab
        static readonly string SettingsTab = "ACTIVETAB,Dev2.Studio.ViewModels.WorkSurface.WorkSurfaceContextViewModel,UI_SettingsView_AutoID";
        static readonly string SettingsServerPermissionsGrid = "ACTIVETAB,UI_SettingsView_AutoID,SecurityViewContent,ServerPermissionsDataGrid";
        static readonly string SettingsResoureceselect = SettingsTab + ",SecurityViewContent,ResourcePermissionsDataGrid,UI_PermissionsGrid_Row_0_AutoID,UI__AddResourceButton_AutoID";
        static readonly string SettingsResourCeExecute = "UI_DocManager_AutoID,UI_SplitPane_AutoID,UI_TabManager_AutoID,UI_SettingsView_AutoID,SecurityViewContent,ResourcePermissionsDataGrid,UI_PermissionsGrid_Row_0_AutoID";
        static readonly string SettingsForGroup = "ServerPermissionsDataGrid,UI_ServerPermissionsGrid_Row_2_AutoID,UI_ServerPermissionsColumn_Row_2_Cell_AutoID,UI__AddWindowsGroupTextBox_AutoID";
        static readonly string SecurityViewContent = "ACTIVETAB,UI_SettingsView_AutoID,SecurityViewContent,ServerPermissionsDataGrid,UI_ServerPermissionsGrid_Row_2_AutoID,UI_ServerPermissionsColumn_Row_2_Cell_AutoID,UI_Public_ViewPermissionCheckBox_AutoID";
        static readonly string SecurityPublicView = "ACTIVETAB,UI_SettingsView_AutoID,SecurityViewContent,ServerPermissionsDataGrid,UI_ServerPermissionsGrid_Row_1_AutoID,UI_ServerViewPermissions_Row_1_Cell_AutoID,UI_Public_ViewPermissionCheckBox_AutoID";
        static readonly string SecurityPublicExecute = "ACTIVETAB,UI_SettingsView_AutoID,SecurityViewContent,ServerPermissionsDataGrid,UI_ServerPermissionsGrid_Row_1_AutoID,UI_ServerExecutePermissions_Row_1_Cell_AutoID,UI_Public_ExecutePermissionCheckBox_AutoID";
        static readonly string SecurityPublicContribute = "ACTIVETAB,UI_SettingsView_AutoID,SecurityViewContent,ServerPermissionsDataGrid,UI_ServerPermissionsGrid_Row_1_AutoID,UI_ServerContributePermissions_Row_1_Cell_AutoID,UI_Public_ContributePermissionCheckBox_AutoID";
        static readonly string SecurityPublicAdministrator = "ACTIVETAB,UI_SettingsView_AutoID,SecurityViewContent,ServerPermissionsDataGrid,UI_ServerPermissionsGrid_Row_1_AutoID,UI_ServerAdministratorPermissions_Row_1_Cell_AutoID,UI_Public_AdministratorPermissionCheckBox_AutoID";
        static readonly string SecurityPublicDeployTo = "ACTIVETAB,UI_SettingsView_AutoID,SecurityViewContent,ServerPermissionsDataGrid,UI_ServerPermissionsGrid_Row_1_AutoID,UI_ServerDeployToPermissions_Row_1_Cell_AutoID,UI_Public_DeployToPermissionCheckBox_AutoID";
        static readonly string SecurityPublicDeployFrom = "ACTIVETAB,UI_SettingsView_AutoID,SecurityViewContent,ServerPermissionsDataGrid,UI_ServerPermissionsGrid_Row_1_AutoID,UI_ServerDeployFromPermissions_Row_1_Cell_AutoID,UI_Public_DeployFromPermissionCheckBox_AutoID";
        static readonly string SecurityPublic = "ACTIVETAB,UI_SettingsView_AutoID,SecurityViewContent,ServerPermissionsDataGrid,UI_ServerPermissionsGrid_Row_1_AutoID,UI_ServerDeployFromPermissions_Row_1_Cell_AutoID,UI_Public_DeployFromPermissionCheckBox_AutoID";

        //Deploy Tab
        static readonly string DeploySource = "UI_DocManager_AutoID,UI_SplitPane_AutoID,UI_TabManager_AutoID,DeployUserControl,SourceNavigationView,UI_ExplorerTree_AutoID";
        static readonly string DeployDestination = "UI_DocManager_AutoID,UI_SplitPane_AutoID,UI_TabManager_AutoID,DeployUserControl";

        static readonly string SecuritySave = "ACTIVETAB,UI_SettingsView_AutoID,UI_SaveSettingsbtn_AutoID";
        static readonly string SecurityDelete = SettingsTab + ",UI_AddRemovebtn_AutoID";
        static readonly string SecurityResourcePicker = SettingsTab + ",UI_AddResourceToSecuritySettingsbtn_AutoID";
        static readonly string SecurityWindowsGrouppicker = SettingsTab + "UI__AddWindowsGroupButton_AutoID";
        static readonly string SecurityWindowsGroupInput = SettingsTab + ",UI_AddWindowsGroupsTextBox_AutoID";
        static readonly string ResourceGroupPicker = SettingsTab + ",UI__AddWindowsGroupsButton_AutoID";
        static readonly string SecurityView = SettingsTab + ",UI_SecuritySettingResourceViewchk_AutoID";
        static readonly string SecurityExecute = SettingsTab + ",UI_SecuritySettingResourceExecutechk_AutoID";
        static readonly string SecurityContribute = SettingsTab + ",UI_SecuritySettingResourceContributechk_AutoID";
        static readonly string SecurityAdministrator = SettingsTab + ",UI_SecuritySettingServerAdministratorchk_AutoID";
        static readonly string SecurityDeployTo = SettingsTab + ",UI_SecuritySettingServerDeployTochk_AutoID";
        static readonly string SecurityDeployFrom = SettingsTab + ",UI_SecuritySettingServerDeployFromchk_AutoID";
        static readonly string SecurityConnectDropdown = SettingsTab + ",UI_SettingsServerComboBox_AutoID";
        static readonly string SecurityConnectEdit = SettingsTab + ",UI_SettingsServerEditButton_AutoID";
        static readonly string SecurityConnectButton = SettingsTab + ",UI_SettingsServerConnectButton_AutoID";
        static readonly string SecurityHelp = SettingsTab + ",ServerHelpToggleButton";

        static readonly string SchedulerTab = "ACTIVETAB,UI_SchedulerView_AutoID";
        static readonly string SchedulerConnectDropDown = SchedulerTab + ",ConnectUserControl,UI_SettingsServerComboBox_AutoID";
        static readonly string SchedulerConnectEditButton = SchedulerTab + ",ConnectUserControl,UI_SettingsServerEditButton_AutoID";
        static readonly string SchedulerConnectConnectButton = SchedulerTab + ",ConnectUserControl,UI_SettingsServerConnectButton_AutoID";
        static readonly string SchedulerNewButton = SchedulerTab + ",UI_NewTaskButton_AutoID";
        static readonly string SchedulerSaveButton = SchedulerTab + ",UI_SaveTaskButton_AutoID";
        static readonly string SchedulerDeleteButton = SchedulerTab + ",UI_DeleteTaskButton_AutoID";
        static readonly string SchedulerNameInput = SchedulerTab + ",UI_SchedulerTabControl_AutoID,UI_SchedulerSettingsTab_AutoID,UI_NameTextbox";
        static readonly string SchedulerStatusDisabledRadio = SchedulerTab + ",UI_SchedulerTabControl_AutoID,UI_SchedulerSettingsTab_AutoID,UI_DisabledRadioButton";
        static readonly string SchedulerStatusEnabledRadio = SchedulerTab + ",UI_SchedulerTabControl_AutoID,UI_SchedulerSettingsTab_AutoID,UI_EnabledRadioButton";
        static readonly string SchedulerWorkflowEdit = SchedulerTab + ",UI_SchedulerTabControl_AutoID,UI_SchedulerSettingsTab_AutoID,UI_WorkflowNameTextBox";
        static readonly string SchedulerWorkflowSelectorButton = SchedulerTab + ",UI_SchedulerTabControl_AutoID,UI_SchedulerSettingsTab_AutoID,UI_WorkflowSelectorButton_AutoID";
        static readonly string SchedulerRunTaskAsSoonAsPossibleCheckbox = SchedulerTab + ",UI_SchedulerTabControl_AutoID,UI_SchedulerSettingsTab_AutoID,UI_RunAsapCheckBox";
        static readonly string SchedulerHistoryToKeepInput = SchedulerTab + ",UI_SchedulerTabControl_AutoID,UI_SchedulerSettingsTab_AutoID,UI_NumOfHisTextBox";
        static readonly string SchedulerUsernameInput = SchedulerTab + ",UI_SchedulerTabControl_AutoID,UI_SchedulerSettingsTab_AutoID,UI_UserNameTextBox";
        static readonly string SchedulerPasswordInput = SchedulerTab + ",UI_SchedulerTabControl_AutoID,UI_SchedulerSettingsTab_AutoID,UI_PasswordBox";
        static readonly string SchedulerHistoryTab = SchedulerTab + ",UI_SchedulerTabControl_AutoID,UI_SchedulerHistoryTab_AutoID";
        static readonly string SchedulerSavingErrorOkButton = "MainViewWindow,UI_MessageBox_AutoID,UI_OkButton_AutoID";
        static readonly string SchedulerEditTriggerButton = SchedulerTab + ",UI_SchedulerTabControl_AutoID,UI_SchedulerSettingsTab_AutoID,UI_EditTriggerButton_AutoID";
        static readonly string SchedulerSetupTriggerOkButton = "MainViewWindow,TriggerEditDialog,okBtn";
        static readonly string SchedulerDeleteConfirmationYesButton = "MainViewWindow,UI_MessageBox_AutoID,UI_YesButton_AutoID";
        static readonly string SchedulerDeleteConfirmationNoButton = "MainViewWindow,UI_MessageBox_AutoID,UI_NoButton_AutoID";
        static readonly string SchedulerHelpButton = SchedulerTab + ",UI_SchedulerHelpButton_AutoID";

        static readonly string RibbonDeploy = "UI_RibbonHomeTabDeployBtn_AutoID";
        static readonly string RibbonSettings = "UI_RibbonHomeManageSecuritySettingsBtn_AutoID";
        static readonly string RibbonSchedule = "UI_RibbonHomeTabSchedulerBtn_AutoID";
        static readonly string RibbonDebug = "UI_RibbonDebugBtn_AutoID";
        static readonly string WindowDebug = "UI_DebugInputWindow_AutoID";
        static readonly string WindowDebugButton = WindowDebug + ",UI_Executebtn_AutoID";
        static readonly string WindowViewInBrowser = WindowDebug + ",UI_Browserbtn_AutoID";
        static readonly string WindowCancel = WindowDebug + ",UI_Cancelbtn_AutoID";
        static readonly string RibbonNewEndPoint = "UI_RibbonHomeTabWorkflowBtn_AutoID";
        static readonly string RibbonSave = "UI_RibbonHomeTabSaveBtn_AutoID";
        static readonly string RibbonNewDatabaseConnector = "UI_RibbonHomeTabDBServiceBtn_AutoID";
        static readonly string RibbonNewPluginConnector = "UI_RibbonHomeTabPluginServiceBtn_AutoID";
        static readonly string RibbonNewWebConnector = "UI_RibbonHomeTabWebServiceBtn_AutoID";

        static readonly string ResourcePicker = "UI_SelectServiceWindow_AutoID";
        static readonly string ResourcePickerFilter = ResourcePicker + ",UI_NavigationViewUserControl_AutoID,FilterTextBox,UI_DataListSearchtxt_AutoID";
        static readonly string ResourcePickerFolders = ResourcePicker + ",UI_NavigationViewUserControl_AutoID,UI_ExplorerTree_AutoID,UI_localhost_AutoID";
        static readonly string ResourcePickerOkButton = ResourcePicker + ",UI_SelectServiceOKButton_AutoID";


        int _retryCount;
#pragma warning restore 414

        [BeforeTestRun]
        public static void SetupForTest()
        {
            // ReSharper disable PossiblyMistakenUseOfParamsMethod
            Bootstrap.ResolvePathsToTestAgainst();
            // ReSharper restore PossiblyMistakenUseOfParamsMethod
            Playback.Initialize();

        }

        [When(@"I debug ""(.*)"" in ""(.*)""")]
        public void WhenIDebugIn(string workflowName, string folderName)
        {
            ExplorerUIMap.DoubleClickWorkflow(workflowName, "TRAV");
            RibbonUIMap.ClickDebug();
            DebugUIMap.ClickExecute();
            OutputUIMap.WaitForExecution(100);
        }

        [Then(@"debug contains")]
        public void ThenDebugContains(Table table)
        {
            UITestControl lastStep = OutputUIMap.GetLastStep();
            string workflowStepName = OutputUIMap.GetStepName(lastStep);
            Assert.AreEqual("TravsTestFlow", workflowStepName);
            var tableRow = table.Rows[0];
            var labelValue = tableRow["Label"];
            var debugValues = tableRow["Value"].Split(' ');
            List<string> outputs = new[] { labelValue }.ToList();
            outputs.AddRange(debugValues);
            Assert.IsTrue(OutputUIMap.AssertDebugOutputContains(lastStep, outputs.ToArray()));

        }


        [When(@"I click new ""(.*)""")]
        public void WhenIClickNew(string type)
        {
            switch(type)
            {
                case "Workflow":
                    RibbonUIMap.CreateNewWorkflow();
                    break;
                case "Database Service":
                    RibbonUIMap.ClickNewDbWebService();
                    break;
            }
        }

        [Given(@"I click new ""(.*)""")]
        public void GivenIClickNew(string p0)
        {
            WhenIClickNew(p0);
        }

        [When(@"I drag on a ""(.*)""")]
        public void WhenIDragOnA(string p0)
        {
            var dsfActivityUiMap = new DsfMultiAssignUiMap(false, false) { TheTab = TabManagerUIMap.GetActiveTab() };
            dsfActivityUiMap.DragToolOntoDesigner();
            ScenarioContext.Current.Add(p0, dsfActivityUiMap);
        }

        [When(@"I enter into the ""(.*)""")]
        public void WhenIEnterIntoThe(string p0, Table table)
        {
            var dsfActivityUiMap = ScenarioContext.Current.Get<DsfMultiAssignUiMap>(p0);
            int index = 0;
            foreach(var tableRow in table.Rows)
            {
                dsfActivityUiMap.EnterTextIntoVariable(index, tableRow["Variable"]);
                dsfActivityUiMap.EnterTextIntoValue(index, tableRow["Value"]);
                index++;
            }
        }

        [Then(@"""(.*)"" contains")]
        public void ThenContains(string p0, Table table)
        {
            var dsfActivityUiMap = ScenarioContext.Current.Get<DsfMultiAssignUiMap>(p0);
            int index = 0;

            foreach(var tableRow in table.Rows)
            {
                Assert.AreEqual(tableRow["Variable"], dsfActivityUiMap.GetTextFromVariable(index));
                Assert.AreEqual(tableRow["Value"], dsfActivityUiMap.GetTextFromValue(index));
                index++;
            }
        }

        [When(@"the new Database Service wizard is visible")]
        public void WhenTheNewDatabaseServiceWizardIsVisible()
        {
            if(!WizardsUIMap.TryWaitForWizard(2500))
            {
                Assert.Fail("Database Service wizard not visible");
            }
            else
            {
                Assert.AreEqual("localhost (http://localhost:3142/dsf)", WizardsUIMap.GetRightTitleText());
            }
        }

        [When(@"I create a new Database Source ""(.*)""")]
        public void WhenICreateANewDatabaseSource(string sourceName)
        {
            DatabaseServiceWizardUIMap.ClickNewDbSource();
            DatabaseServiceWizardUIMap.CreateDbSource("RSAKLFSVRGENDEV", sourceName);
        }

        [When(@"I create a database service ""(.*)""")]
        public void WhenICreateADatabaseService(string serviceName)
        {
            DatabaseServiceWizardUIMap.CreateDbService(serviceName);
        }

        [Given(@"""(.*)"" is in the explorer")]
        [Then(@"""(.*)"" is in the explorer")]
        [When(@"""(.*)"" is in the explorer")]
        public void ThenIsInTheExplorer(string resourceName)
        {
            Assert.IsTrue(ExplorerUIMap.ValidateHasResource(resourceName));
            ExplorerUIMap.RightClickDeleteResource(resourceName, "UNASSIGNED", "localhost");
            Bootstrap.DeleteService(resourceName);
        }

        [Given(@"a new tab is created")]
        [Then(@"a new tab is created")]
        [When(@"a new tab is created")]
        public void ThenANewTabIsCreated()
        {
            var uiTestControl = TabManagerUIMap.GetActiveTab();
            Assert.IsNotNull(uiTestControl);
        }

        [Given(@"the tab name contains ""(.*)""")]
        [Then(@"the tab name contains ""(.*)""")]
        [When(@"the tab name contains ""(.*)""")]
        public void ThenTheTabNameContains(string p0)
        {
            var activeTabName = TabManagerUIMap.GetActiveTabName();
            StringAssert.Contains(activeTabName, p0);
        }

        [Given(@"start node is visible")]
        [Then(@"start node is visible")]
        [When(@"start node is visible")]
        public void ThenStartNodeIsVisible()
        {
            var uiTestControl = TabManagerUIMap.GetActiveTab();
            var findStartNode = WorkflowDesignerUIMap.FindStartNode(uiTestControl);
            Assert.IsNotNull(findStartNode, "Start node is not visible");
        }

        [Given(@"I wait for ""(.*)"" seconds")]
        [Then(@"I wait for ""(.*)"" seconds")]
        [When(@"I wait for ""(.*)"" seconds")]
        public void GivenIWaitForSeconds(int seconds)
        {
            Thread.Sleep(seconds * 1000);
        }


        [When(@"I send ""(.*)"" to ""(.*)""")]
        [Given(@"I send ""(.*)"" to ""(.*)""")]
        [Then(@"I send ""(.*)"" to ""(.*)""")]
        public void WhenISendTo(string textToSend, string automationIds)
        {
            var correctedAutoIds = GetCorrect(automationIds).Split(',');
            var startControl = GetStartUiTestControl(ref correctedAutoIds);
            var controlToSendData = VisualTreeWalker.GetControlFromRoot(true, 0, startControl,false, correctedAutoIds);
            if(!string.IsNullOrEmpty(automationIds))
            {
                controlToSendData.Click();
                // controlToSendData.EnterText("");
            }

            Playback.Wait(0);
            var dataToSend = GetCorrect(textToSend).Split('|');
            foreach(var text in dataToSend)
            {
                Keyboard.SendKeys(text);
            }

        }

        [Given(@"I type ""(.*)"" in ""(.*)""")]
        [When(@"I type ""(.*)"" in ""(.*)""")]
        [Then(@"I type ""(.*)"" in ""(.*)""")]
        public void GivenITypeIn(string textToSend, string automationIds)
        {
            var correctedAutoIds = GetCorrect(automationIds).Split(',');
            var startControl = GetStartUiTestControl(ref correctedAutoIds);
            var controlToSendData = VisualTreeWalker.GetControlFromRoot(true, 0, startControl, false, correctedAutoIds);
            controlToSendData.EnterText(textToSend);
        }


        UITestControl GetStartUiTestControl(ref string[] correctedAutoIds)
        {
            UITestControl startControl = null;
            if(correctedAutoIds.Any())
            {
                if(correctedAutoIds[0] == "ACTIVETAB")
                {
                    startControl = TabManagerUIMap.GetActiveTab();
                    var listOfIds = correctedAutoIds.ToList();
                    listOfIds.RemoveAt(0);
                    correctedAutoIds = listOfIds.ToArray();
                }
            }
            return startControl;
        }

        [Given(@"I click ""(.*)""")]
        [When(@"I click ""(.*)""")]
        [Then(@"I click ""(.*)""")]
        [Given(@"I click")]
        public void GivenIClick(string automationIds)
        {
            var controlToClick = GetAControlStrict(automationIds);
            controlToClick.Click();
        }

        [Then(@"Wait for ""(.*)"" seconds")]
        public void ThenWaitForSeconds(int seconds)
        {
            Thread.Sleep(seconds * 1000);
        }


        [Given(@"I right click ""(.*)""")]
        [When(@"I right click ""(.*)""")]
        [Then(@"I right click ""(.*)""")]
        public void ThenIRightClick(string automationIds)
        {
            var controlToClick = GetAControlStrict(automationIds);
            controlToClick.RightClick();
        }

        [Given(@"""(.*)"" is checked")]
        [When(@"""(.*)"" is checked")]
        [Then(@"""(.*)"" is checked")]
        public void GivenIsChecked(string automationIds)
        {
            var checkBox = GetAControlStrict(automationIds) as WpfCheckBox;
            Assert.IsNotNull(checkBox, "Control must be a combobox");
            checkBox.Check(true);
        }

        [Given(@"""(.*)"" is unchecked")]
        [When(@"""(.*)"" is unchecked")]
        [Then(@"""(.*)"" is unchecked")]
        public void GivenIsUnchecked(string automationIds)
        {
            var checkBox = GetAControlStrict(automationIds) as WpfCheckBox;
            Assert.IsNotNull(checkBox, "Control must be a check box");
            checkBox.Check(false);
        }

        UITestControl GetAControlStrict(string automationIds)
        {
            var automationIDs = GetCorrect(automationIds).Split(',');
            var startControl = GetStartUiTestControl(ref automationIDs);
            var controlToClick = VisualTreeWalker.GetControlFromRoot(true, 0, startControl, false, automationIDs);
            return controlToClick;
        }

        UITestControl GetAControlRelaxed(string automationIds)
            {
            var automationIDs = GetCorrect(automationIds).Split(',');
            var startControl = GetStartUiTestControl(ref automationIDs);
            var controlToClick = VisualTreeWalker.GetControlFromRoot(true, 0, startControl, true, automationIDs);
            return controlToClick;
        }

        [Given(@"I create a new remote connection ""(.*)"" as")]
        [When(@"I create a new remote connection ""(.*)"" as")]
        public void GivenICreateANewRemoteConnectionAs(string serverName, Table table)
        {
            GivenIClick("UI_DocManager_AutoID,UI_ExplorerPane_AutoID,UI_ExplorerControl_AutoID,ConnectUserControl,UI_ExplorerServerCbx_AutoID,U_UI_ExplorerServerCbx_AutoID_New Remote Server...");
            Playback.Wait(3500);
            var serverDetailsRow = table.Rows[0];
            NewServerUIMap.ClearServerAddress();
            NewServerUIMap.EnterServerAddress(serverDetailsRow["Address"]);
            var authType = serverDetailsRow["AuthType"];
            NewServerUIMap.SelectAuthenticationType(authType);
            if(authType == "User")
            {
                NewServerUIMap.EnterPassword(serverDetailsRow["Password"]);
                NewServerUIMap.EnterUserName(serverDetailsRow["UserName"]);
            }
            NewServerUIMap.ClickTestConnection();
            Playback.Wait(1500);
            NewServerUIMap.ClickSave();
            Playback.Wait(1500);
            NewServerUIMap.SaveNameInDialog(serverName);
            Playback.Wait(1500);
            NewServerUIMap.ClickSave();
            Playback.Wait(1500);
            var spinnerControl = GetAControlStrict(ExplorerConnectProgress);
            Assert.IsNotNull(spinnerControl, "Server is not connecting after creating a source ...");
            var canExit = false;
            while(!canExit)
            {
                Playback.Wait(500);
                spinnerControl = GetAControlStrict(ExplorerConnectProgress);
                canExit = spinnerControl.State == ControlStates.Offscreen;
            }
        }

        [Given(@"I close Studio")]
        [Then(@"I close Studio")]
        public void GivenICloseStudio()
        {
            Bootstrap.KillStudio();
        }

        [Given(@"I close Server")]
        [Then(@"I close Server")]
        public void GivenICloseServer()
        {
            Bootstrap.KillServer();
        }

        [Given(@"I start Server as ""(.*)"" with password ""(.*)""")]
        public void GivenIStartServerAsWithPassword(string userName, string password)
        {
            RunSpecifiedFileWithUserNameAndPassword(userName, password, Bootstrap.ServerLocation);
        }

        static void RunSpecifiedFileWithUserNameAndPassword(string userName, string password, string fileLocation)
        {
            var sspw = new SecureString();

            foreach(var c in password)
            {
                sspw.AppendChar(c);
            }

            var proc = new Process
            {
                StartInfo =
                            {
                                UseShellExecute = false,
                                UserName = userName,
                                Password = sspw,
                                Domain = Environment.MachineName,
                                LoadUserProfile = false
                            }
            };

            var workingDirectory = Path.GetDirectoryName(fileLocation);
            if(workingDirectory != null)
            {
                proc.StartInfo.WorkingDirectory = workingDirectory;
            }
            var fileName = Path.GetFileName(fileLocation);
            if(fileName != null)
            {
                proc.StartInfo.FileName = fileLocation;
            }

            //proc.StartInfo.Domain = Environment.MachineName;
            //proc.StartInfo.Arguments = "";

            proc.Start();
        }

        [Given(@"I start Studio as ""(.*)"" with password ""(.*)""")]
        [Then(@"I start Studio as ""(.*)"" with password ""(.*)""")]
        [When(@"I start Studio as ""(.*)"" with password ""(.*)""")]
        public void GivenIStartStudioAsWithPassword(string userName, string password)
        {
            RunSpecifiedFileWithUserNameAndPassword(userName, password, Bootstrap.StudioLocation);
        }


        [When(@"I drag ""(.*)"" onto ""(.*)""")]
        [Given(@"I drag ""(.*)"" onto ""(.*)""")]
        [Then(@"I drag ""(.*)"" onto ""(.*)""")]
        public void WhenIDragOnto(string dragItemAutoIds, string dragDestinationAutoIds)
        {
            var correcteddDragItemAutoIds = GetCorrect(dragItemAutoIds).Split(',');
            var startControlDragItem = GetStartUiTestControl(ref correcteddDragItemAutoIds);
            var correctedDragDestinationAutoIds = GetCorrect(dragDestinationAutoIds).Split(',');
            var startControlDragDestination = GetStartUiTestControl(ref correctedDragDestinationAutoIds);


            var itemToDrag = VisualTreeWalker.GetControlFromRoot(true, 0, startControlDragItem,false, correcteddDragItemAutoIds);
            var dragDestinationItem = VisualTreeWalker.GetControlFromRoot(true, 0, startControlDragDestination,false, correctedDragDestinationAutoIds);

            itemToDrag.Click();
            //Mouse.Click(itemToDrag, new Point(15, 15));
            var clickablePoint = itemToDrag.GetClickablePoint();
            Mouse.StartDragging(itemToDrag, clickablePoint);


            var boundingRect = dragDestinationItem.BoundingRectangle;
            var pointToDrag = new Point(boundingRect.X + boundingRect.Width / 2, boundingRect.Bottom + 10);

            Mouse.StopDragging(pointToDrag);
            //Playback.PlaybackSettings.WaitForReadyLevel = WaitForReadyLevel.AllThreads;
            Playback.Wait(100);
        }
      
        [When(@"I double click ""(.*)""")]
        [Given(@"I double click ""(.*)""")]
        [Given(@"I double click")]
        [Then(@"I double click ""(.*)""")]
        public void WhenIDoubleClick(string itemToDoubleClickAutoIds)
        {
            var correcteddItemToDoubleClickAutoIds = GetCorrect(itemToDoubleClickAutoIds).Split(',');
            var startControl = GetStartUiTestControl(ref correcteddItemToDoubleClickAutoIds);
            var itemToDoubleClick = VisualTreeWalker.GetControlFromRoot(true, 0, startControl, false, correcteddItemToDoubleClickAutoIds);
            itemToDoubleClick.DoubleClick();
        }

        [Then(@"""(.*)"" is visible within ""(.*)"" seconds")]
        [Given(@"""(.*)"" is visible within ""(.*)"" seconds")]
        [When(@"""(.*)"" is visible within ""(.*)"" seconds")]
        public void ThenIsVisibleWithinSeconds(string itemToFindAutoIds, int seconds)
        {
            long timetolookup = seconds * 1000;
            var stopWatch = Stopwatch.StartNew();
            while(timetolookup > 0)
            {
                Playback.Wait(1000);
                var control = GetAControlRelaxed(itemToFindAutoIds);
                if(control != null)
                {
                    if (control.State.HasFlag(ControlStates.Offscreen))
                    {
                        control.EnsureClickable();
                    }

                    var isInvisible = control.State.HasFlag(ControlStates.Invisible);
                    
                    if (!isInvisible)
                    {
                        break;
                    }
                }
                timetolookup -= stopWatch.ElapsedMilliseconds;
            }
            stopWatch.Stop();
            if(timetolookup <= 0)
            {
                string message = string.Format("{0} - Was not visible within {1} seconds", itemToFindAutoIds, seconds);
                Assert.Fail(message);
            }
        }

        [Given(@"""(.*)"" is invisible within ""(.*)"" seconds")]
        [Then(@"""(.*)"" is invisible within ""(.*)"" seconds")]
        [When(@"""(.*)"" is invisible within ""(.*)"" seconds")]
        public void ThenIsInvisibleWithinSeconds(string itemToFindAutoIds, int seconds)
        {
            long timetolookup = seconds * 1000;
            var stopWatch = Stopwatch.StartNew();
            while(timetolookup > 0)
            {
                Playback.Wait(1000);
                var control = GetAControlRelaxed(itemToFindAutoIds);
                if(control == null)
                {
                    break;
                }

                var isInvisible = control.State.HasFlag(ControlStates.Invisible) ||
                            control.State.HasFlag(ControlStates.Offscreen);

                if(isInvisible)
                {
                    break;
                }

                timetolookup -= stopWatch.ElapsedMilliseconds;
            }
            stopWatch.Stop();
            if(timetolookup <= 0)
            {
                string message = string.Format("{0} - Was not invisible within {1} seconds", itemToFindAutoIds, seconds);
                Assert.Fail(message);
            }
        }

        [Given(@"""(.*)"" is enabled within ""(.*)"" seconds")]
        [Then(@"""(.*)"" is enabled within ""(.*)"" seconds")]
        [When(@"""(.*)"" is enabled within ""(.*)"" seconds")]
        public void ThenIsEnabledWithinSeconds(string itemToFindAutoIds, int seconds)
        {            
            ValidateEnableState(itemToFindAutoIds, seconds, true);
        }
        
        [Given(@"""(.*)"" is disabled within ""(.*)"" seconds")]
        [Then(@"""(.*)"" is disabled within ""(.*)"" seconds")]
        [When(@"""(.*)"" is disabled within ""(.*)"" seconds")]
        public void ThenIsDisabledWithinSeconds(string itemToFindAutoIds, int seconds)
        {
            ValidateEnableState(itemToFindAutoIds, seconds, false);
        }

        void ValidateEnableState(string itemToFindAutoIds, int seconds, bool enableState)
        {
            long timetolookup = seconds * 1000;
            var stopWatch = Stopwatch.StartNew();
            while(timetolookup > 0)
            {
                Playback.Wait(1000);
                var control = GetAControlRelaxed(itemToFindAutoIds);
                Assert.IsNotNull(control, "Could not find control to determine it's enable state");
                if(control.IsEnabled() == enableState)
                {
                    break;
                }
                timetolookup -= stopWatch.ElapsedMilliseconds;
            }
            stopWatch.Stop();
            if(timetolookup <= 0)
            {
                string message = string.Format("{0} - Was not {1} within {2} seconds", itemToFindAutoIds, enableState ? "enabled" : "disbaled", seconds);
                Assert.Fail(message);
            }
        }

        [Given(@"all tabs are closed")]
        [When(@"all tabs are closed")]
        [Then(@"all tabs are closed")]
        public void GivenAllTabsAreClosed()
        {
            StudioWindow.SetFocus();
            var tabManager = TabManagerUIMap.GetManager();

            if(tabManager != null)
            {
                var tabs = tabManager.GetChildren();
                foreach(var tab in tabs)
                {
                    var closeButton = tab.FindByAutomationId("closeBtn",false);   
                    closeButton.Click();
                }
            }
        }


        [Given(@"""(.*)"" is visible")]
        [Then(@"""(.*)"" is visible")]
        [Then(@"""(.*)"" is visible")]
        [Then(@"""(.*)"" is visible")]
        public void ThenIsVisible(string itemToFindAutoIds)
        {
            var correctedditemToFindAutoIds = GetCorrect(itemToFindAutoIds).Split(',');
            var itemFound = VisualTreeWalker.GetControlFromRoot(true, 0, null, false, correctedditemToFindAutoIds);
            var message = string.Format("Control with Auto ID : [{0}] is not visible", correctedditemToFindAutoIds[correctedditemToFindAutoIds.Length - 1]);
            Assert.IsNotNull(itemFound, message);

            var isInvisible = itemFound.State.HasFlag(ControlStates.Invisible) ||
                              itemFound.State.HasFlag(ControlStates.Offscreen);

            Assert.IsFalse(isInvisible, message);
        }

        [Given(@"""(.*)"" is not visible")]
        [Then(@"""(.*)"" is not visible")]
        [When(@"""(.*)"" is not visible")]
        public void ThenIsNotVisible(string itemToFindAutoIds)
        {
            var correctedditemToFindAutoIds = GetCorrect(itemToFindAutoIds).Split(',');
            var itemFound = VisualTreeWalker.GetControlFromRoot(true, 0, null,false, correctedditemToFindAutoIds);
            var message = string.Format("Control with Auto ID : [{0}] is visible", correctedditemToFindAutoIds[correctedditemToFindAutoIds.Length - 1]);
            if (itemFound != null)
            {
                var isInvisible = itemFound.State.HasFlag(ControlStates.Invisible) ||
                                  itemFound.State.HasFlag(ControlStates.Offscreen);

                Assert.IsTrue(isInvisible, message);
            }
        }

        [Given(@"""(.*)"" is enabled")]
        [Then(@"""(.*)"" is enabled")]
        [When(@"""(.*)"" is enabled")]
        public void ThenIsEnabled(string itemToFindAutoIds)
        {
            var correctedditemToFindAutoIds = GetCorrect(itemToFindAutoIds).Split(',');
            var itemFound = VisualTreeWalker.GetControlFromRoot(true, 0, null,false, correctedditemToFindAutoIds);
            Assert.IsNotNull(itemFound);
            var message = string.Format("Control with Auto ID : [{0}] is not enabled", correctedditemToFindAutoIds[correctedditemToFindAutoIds.Length - 1]);
            Assert.IsTrue(itemFound.IsEnabled(), message);
        }

        [When(@"""(.*)"" is disabled")]
        [Then(@"""(.*)"" is disabled")]
        [Given(@"""(.*)"" is disabled")]
        public void ThenIsDisabled(string itemToFindAutoIds)
        {
            var correctedditemToFindAutoIds = GetCorrect(itemToFindAutoIds).Split(',');
            var itemFound = VisualTreeWalker.GetControlFromRoot(true, 0, null, false, correctedditemToFindAutoIds);
            Assert.IsNotNull(itemFound);
            var message = string.Format("Control with Auto ID : [{0}] is not disabled", correctedditemToFindAutoIds[correctedditemToFindAutoIds.Length - 1]);
            Assert.IsFalse(itemFound.IsEnabled(), message);
        }

        //[Given(@"I wait till ""(.*)"" is not visible")]
        //[Then(@"I wait till ""(.*)"" is not visible")]
        //[When(@"I wait till ""(.*)"" is not visible")]
        //public void GivenIWaitTillIsNotVisible(string itemAutoIds)
        //{
        //    var correctedditemToFindAutoIds = GetCorrect(itemAutoIds).Split(',');
        //    var itemFound = VisualTreeWalker.GetControlFromRoot(true, 0, null, false, correctedditemToFindAutoIds);
        //    while(itemFound != null && itemFound.Exists)
        //    {
        //        Playback.Wait(100);
        //    }
        //}

        //[Given(@"I wait till ""(.*)"" is visible")]
        //[Then(@"I wait till ""(.*)"" is visible")]
        //[When(@"I wait till ""(.*)"" is visible")]
        //public void GivenIWaitTillIsVisible(string itemAutoIds)
        //{
        //    var correctedditemToFindAutoIds = GetCorrect(itemAutoIds).Split(',');
        //    _retryCount = 0;
        //    Playback.PlaybackError += PlaybackOnPlaybackError;

        //    var itemFound = VisualTreeWalker.GetControlFromRoot(true, 0, null, false, correctedditemToFindAutoIds);
        //    if(itemFound.State != ControlStates.Invisible)
        //    {
        //        Playback.PlaybackError -= PlaybackOnPlaybackError;
        //        _retryCount = 0;
        //    }
        //}

        void PlaybackOnPlaybackError(object sender, PlaybackErrorEventArgs playbackErrorEventArgs)
        {
            if(_retryCount >= 100)
            {
                throw playbackErrorEventArgs.Error;
            }
            Playback.Wait(100);
            playbackErrorEventArgs.Result = PlaybackErrorOptions.Retry;
            _retryCount++;
        }

        string GetCorrect(string automationIds)
        {
            var fieldInfos = typeof(WorkflowSteps).GetFields(BindingFlags.NonPublic | BindingFlags.Static);
            var consts = fieldInfos.Where(fi => fi.FieldType == typeof(String)).ToList();
            var replace = automationIds;
            foreach(var fieldInfo in consts)
            {
                string retVal = replace.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).FirstOrDefault(p => p.Equals(fieldInfo.Name.ToUpper()));
                if(retVal != null)
                {
                    var rawConstantValue = fieldInfo.GetValue(this) as string;
                    if(rawConstantValue != null)
                    {
                        replace = replace.Replace(fieldInfo.Name.ToUpper(), rawConstantValue);
                    }
                }
            }
            return replace;
        }

        [When(@"close the Studio and Server")]
        [Then(@"close the Studio and Server")]
        public void WhenCloseTheStudioAndServer()
        {
            TabManagerUIMap.CloseAllTabs();
            Bootstrap.Teardown();
            Playback.Cleanup();
        }

        [Given(@"""(.*)"" is Highlighted")]
        public void WhenIsHighlighted(string p0)
        {
            var correctedAutoIds = GetCorrect(p0).Split(',');
            var controlToHighlight = VisualTreeWalker.GetControl(correctedAutoIds);

            if(!string.IsNullOrEmpty(p0))
            {
                controlToHighlight.DrawHighlight();
            }
        }

    }
}
