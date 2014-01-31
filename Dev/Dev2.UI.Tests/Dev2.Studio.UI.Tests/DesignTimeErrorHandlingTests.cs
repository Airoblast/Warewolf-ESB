﻿using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Studio.UI.Tests
{
    [CodedUITest]
    public class DesignTimeErrorHandlingTests : UIMapBase
    {
        #region Fields

        const string ExplorerTab = "Explorer";

        #endregion

        #region Cleanup

        [ClassInitialize]
        public static void ClassInit(TestContext tctx)
        {
            Playback.Initialize();
            Playback.PlaybackSettings.ContinueOnError = true;
            Playback.PlaybackSettings.ShouldSearchFailFast = true;
            Playback.PlaybackSettings.SmartMatchOptions = SmartMatchOptions.None;
            Playback.PlaybackSettings.MatchExactHierarchy = true;
            Playback.PlaybackSettings.DelayBetweenActions = 1;

            // make the mouse quick ;)
            Mouse.MouseMoveSpeed = 10000;
            Mouse.MouseDragSpeed = 10000;
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            TabManagerUIMap.CloseAllTabs();
        }

        #endregion

        [TestMethod]
        [TestCategory("UITest")]
        [Description("Test for 'Fix Errors' db service activity adorner: A workflow involving a db service is openned, the mappings on the service are changed and hitting the fix errors adorner should change the activity instance's mappings")]
        [Owner("Ashley")]
        public void DesignTimeErrorHandling_DesignTimeErrorHandlingUITest_FixErrorsButton_DbServiceMappingsFixed()
        {
            const string workflowToUse = "Bug_10011";
            const string serviceToUse = "Bug_10011_DbService";
            Clipboard.Clear();

            // Open the Workflow
            ExplorerUIMap.EnterExplorerSearchText(workflowToUse);
            ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "BUGS", workflowToUse);
            var theTab = TabManagerUIMap.GetActiveTab();

            // Edit the DbService
            ExplorerUIMap.EnterExplorerSearchText(serviceToUse);
            ExplorerUIMap.DoubleClickOpenProject("localhost", "SERVICES", "UTILITY", serviceToUse);

            // Get wizard window
            WizardsUIMap.WaitForWizard();
            // Tab to mappings
            DatabaseServiceWizardUIMap.TabToOutputMappings();
            // Remove column 1+2's mapping
            Playback.Wait(200);
            SendKeys.SendWait("{TAB}");
            SendKeys.SendWait("{TAB}");
            SendKeys.SendWait("{TAB}");
            SendKeys.SendWait("{TAB}");
            SendKeys.SendWait("{DEL}");
            SendKeys.SendWait("{TAB}");
            SendKeys.SendWait("{DEL}");

            // Save
            DatabaseServiceWizardUIMap.ClickOK();
            if(ResourceChangedPopUpUIMap.WaitForDialog(5000))
            {
                ResourceChangedPopUpUIMap.ClickCancel();
            }

            // Fix Errors
            if(WorkflowDesignerUIMap.Adorner_ClickFixErrors(theTab, serviceToUse + "(ServiceDesigner)"))
            {
                // Assert mapping does not exist
                Assert.IsFalse(
                    WorkflowDesignerUIMap.DoesActivityDataMappingContainText(
                        WorkflowDesignerUIMap.FindControlByAutomationId(theTab, serviceToUse + "(ServiceDesigner)"),
                        "[[get_Rows().Column2]]"), "Mappings not fixed, removed mapping still in use");
            }
            else
            {
                Assert.Fail("'Fix Errors' button not visible");
            }
        }

        [TestMethod]
        [TestCategory("UITest")]
        [Description("Test for 'Fix Errors' db service activity adorner: A workflow involving a db service is openned, mappings on the service are set to required and hitting the fix errors adorner should prompt the user to add required mappings to the activity instance's mappings")]
        [Owner("Ashley")]
        [Ignore]
        public void DesignTimeErrorHandling_DesignTimeErrorHandlingUITest_FixErrorsButton_UserIsPromptedToAddRequiredDbServiceMappings()
        {
            const string workflowResourceName = "DesignTimeErrorHandlingRequiredMappingUITest";
            const string dbResourceName = "UserIsPromptedToAddRequiredDbServiceMappingsTest";

            // Open the Workflow
            ExplorerUIMap.EnterExplorerSearchText(workflowResourceName);
            ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "UI TEST", workflowResourceName);
            var theTab = TabManagerUIMap.GetActiveTab();

            // Edit the DbService
            ExplorerUIMap.EnterExplorerSearchText(dbResourceName);
            ExplorerUIMap.DoubleClickOpenProject("localhost", "SERVICES", "INTEGRATION TEST SERVICES", dbResourceName);

            // Get wizard window
            WizardsUIMap.WaitForWizard();
            DatabaseServiceWizardUIMap.TabToInputMappings();

            //set the first input to required
            var wizard = StudioWindow.GetChildren()[0].GetChildren()[0];
            wizard.WaitForControlReady();
            Keyboard.SendKeys(wizard, "{TAB}");
            Playback.Wait(150);
            Keyboard.SendKeys(" ");

            // Save
            DatabaseServiceWizardUIMap.ClickOK();
            ResourceChangedPopUpUIMap.ClickCancel();

            // Fix Errors
            if(WorkflowDesignerUIMap.Adorner_ClickFixErrors(theTab, dbResourceName))
            {
                //Assert mappings are prompting the user to add required mapping
                var getCloseMappingToggle = WorkflowDesignerUIMap.Adorner_GetButton(theTab, dbResourceName,
                                                                                    "Close Mapping");
                Assert.IsNotNull(getCloseMappingToggle,
                                    "Fix Error does not prompt the user to input required mappings");
            }
            else
            {
                Assert.Fail("'Fix Errors' button not visible");
            }
        }
    }
}
