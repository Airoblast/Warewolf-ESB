﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:1.9.0.77
//      SpecFlow Generator Version:1.9.0.0
//      Runtime Version:4.0.30319.18063
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace Dev2.Studio.UI.Specs.Scheduler
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "1.9.0.77")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [Microsoft.VisualStudio.TestTools.UITesting.CodedUITestAttribute()]
    public partial class SchedulerSpecFlowFeatureFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "SchedulerUISpecFlow.feature"
#line hidden
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.ClassInitializeAttribute()]
        public static void FeatureSetup(Microsoft.VisualStudio.TestTools.UnitTesting.TestContext testContext)
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "SchedulerSpecFlowFeature", "In order to Create a Scheduled Workflow Tasks in warewolf studio\r\nAs a Warewolf U" +
                    "ser\r\nI want to be able to Create Tasks in Warewolf Scheduler Tab", ProgrammingLanguage.CSharp, ((string[])(null)));
            testRunner.OnFeatureStart(featureInfo);
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.ClassCleanupAttribute()]
        public static void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute()]
        public virtual void TestInitialize()
        {
            if (((TechTalk.SpecFlow.FeatureContext.Current != null) 
                        && (TechTalk.SpecFlow.FeatureContext.Current.FeatureInfo.Title != "SchedulerSpecFlowFeature")))
            {
                Dev2.Studio.UI.Specs.Scheduler.SchedulerSpecFlowFeatureFeature.FeatureSetup(null);
            }
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute()]
        public virtual void ScenarioTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public virtual void ScenarioSetup(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioStart(scenarioInfo);
        }
        
        public virtual void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Creating Scheduler Task Without password and expected error")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "SchedulerSpecFlowFeature")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Scheduler")]
        public virtual void CreatingSchedulerTaskWithoutPasswordAndExpectedError()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Creating Scheduler Task Without password and expected error", new string[] {
                        "Scheduler"});
#line 7
this.ScenarioSetup(scenarioInfo);
#line 8
     testRunner.Given("I have Warewolf running", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 9
     testRunner.Given("all tabs are closed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 10
  testRunner.And("I click \"EXPLORER,UI_localhost_AutoID\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 12
  testRunner.And("I click \"RIBBONSCHEDULE\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 14
  testRunner.And("I click \"SCHEDULERNEWBUTTON\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 15
  testRunner.Then("\"SCHEDULERNEWBUTTON\" is disabled", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 17
  testRunner.And("I click \"SCHEDULERWORKFLOWSELECTORBUTTON\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 18
  testRunner.And("I double click \"RESOURCEPICKERFOLDERS,UI_Examples_AutoID,UI_File and Folder - Wri" +
                    "te File_AutoID\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 19
  testRunner.And("I click \"UI_SelectServiceWindow_AutoID,UI_SelectServiceOKButton_AutoID\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 20
     testRunner.And("I type \"IntegrationTester\" in \"SCHEDULERUSERNAMEINPUT\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 21
  testRunner.And("I type \"CIScheduleTest\" in \"SCHEDULERNAMEINPUT\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 23
  testRunner.When("I click \"SCHEDULEREDITTRIGGERBUTTON\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 24
     testRunner.Then("\"TriggerEditDialog\" is visible", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 25
  testRunner.And("I click point \"524,508\" on \"TriggerEditDialog\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 26
  testRunner.And("I type \"20\" in \"SCHEDULERHISTORYTOKEEPINPUT\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 27
  testRunner.And("I send \"{TAB}{ENTER}\" to \"\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 32
  testRunner.Given("I click \"SCHEDULERSAVEBUTTON\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 33
  testRunner.And("I click point \"199,264\" on \"\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 34
  testRunner.And("\"UI_MessageBox_AutoID,UI_OkButton_AutoID\" is visible within \"2\" seconds", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 35
  testRunner.Given("I click \"UI_MessageBox_AutoID,UI_OkButton_AutoID\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 36
  testRunner.And("I click point \"362,106\" on \"UI_MessageBox_AutoID\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 38
  testRunner.Then("\"SCHEDULERNEWBUTTON\" is disabled", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 40
 testRunner.Given("I click \"UI_DocManager_AutoID,UI_SplitPane_AutoID,UI_TabManager_AutoID,closeBtn\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 41
  testRunner.And("I click \"UI_MessageBox_AutoID,UI_YesButton_AutoID\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 42
  testRunner.And("I click point \"199,264\" on \"\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 43
  testRunner.And("\"UI_MessageBox_AutoID,UI_OkButton_AutoID\" is visible within \"2\" seconds", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 44
  testRunner.Given("I click \"UI_MessageBox_AutoID,UI_OkButton_AutoID\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 45
  testRunner.And("I click \"SCHEDULERTAB,UI_SchedulerTabControl_AutoID,UI_SchedulerSettingsTab_AutoI" +
                    "D\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 46
  testRunner.And("\"SCHEDULERNAMEINPUT\" contains text \"CIScheduleTest\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 48
  testRunner.And("I click \"SCHEDULERSAVEBUTTON\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 49
  testRunner.Given("I send \"I73573r0\" to \"\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 50
  testRunner.And("I click point \"199,264\" on \"\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 52
  testRunner.And("\"SCHEDULERSAVEBUTTON\" is disabled", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 53
  testRunner.And("I click \"SCHEDULERDELETEBUTTON\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 54
  testRunner.And("I click \"SCHEDULERDELETECONFIRMATIONYESBUTTON\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
