﻿Feature: Designer
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@DesignerUISpecs
Scenario: ChangeWorkflowMappingsAlertsAffectedOnSave
	Given I have Warewolf running
	And all tabs are closed
	And I click "EXPLORERFILTERCLEARBUTTON"
	And I click "EXPLORER,UI_localhost_AutoID"
	And I send "InnerWF" to "EXPLORERFILTER"
	And I double click "EXPLORERFOLDERS,UI_TestCategory_AutoID,UI_InnerWF_AutoID"
	And I click "VARIABLESCALAR,UI_Variable_result_AutoID,UI_IsInputCheckbox_AutoID"
	When I click "RIBBONSAVE"
	Then "UI_DeleteResourceNoBtn_AutoID" is visible within "2" seconds
	When I click "UI_ResourceChangedWarningDialog_AutoID,UI_ShowAffectedWorkflowsButton_AutoID"
	Given I double click point "500,96" on "UI_DocManager_AutoID,UI_SplitPane_AutoID,UI_TabManager_AutoID,myScrollViewer"
	Given I double click point "482,121" on "UI_DocManager_AutoID,UI_SplitPane_AutoID,UI_TabManager_AutoID,myScrollViewer"
	#Given I double click point "280,240" on "UI_DocManager_AutoID,UI_SplitPane_AutoID,UI_TabManager_AutoID,myScrollViewer"
	Then "WORKFLOWDESIGNER,ServiceExecutionTest(FlowchartDesigner)" is visible within "4" seconds
	#Then InnerWf1 should have error icon
	#Then "WORKFLOWDESIGNER,ServiceExecutionTest(FlowchartDesigner),InnerWF(ServiceDesigner)[0],SmallViewContent,UI_FixErrors_AutoID,UI_ErrorsAdorner_AutoID" is visible within "2" seconds
	#And "WORKFLOWDESIGNER,ServiceExecutionTest(FlowchartDesigner),InnerWF(ServiceDesigner)[1],SmallViewContent,UI_FixErrors_AutoID,UI_ErrorsAdorner_AutoID" is visible within "2" seconds
	#And "WORKFLOWDESIGNER,ServiceExecutionTest(FlowchartDesigner),InnerWF(ServiceDesigner)[2],SmallViewContent,UI_FixErrors_AutoID,UI_ErrorsAdorner_AutoID" is visible within "2" seconds
	And "WORKFLOWDESIGNER,ServiceExecutionTest(FlowchartDesigner),InnerWF(ServiceDesigner)[3],SmallViewContent,UI_FixErrors_AutoID,UI_ErrorsAdorner_AutoID" is visible within "2" seconds
	


##Test will be Opne once Ashley Setup an Automation ID's for Grid Rows
Scenario: DeleteFirstDatagridRow_Expected_RowIsNotDeleted12
	Given I have Warewolf running
	Then restart the Studio and Server
	#And all tabs are closed
	#And I click "RIBBONNEWENDPOINT"
	#And I double click "TOOLBOX,PART_SearchBox"
	#And I send "Assign" to ""
	#And I drag "TOOLASSIGN" onto "WORKSURFACE,StartSymbol"
	#And "WORKSURFACE,Assign (1)(MultiAssignDesigner),SmallViewContent,SmallDataGrid,UI_ActivityGridRow_0_AutoID,UI_TextBox_AutoID" is visible within "1" seconds
	##Adding ROW 1
	#Given I type "Delete1" in "WORKSURFACE,Assign (1)(MultiAssignDesigner),SmallViewContent,SmallDataGrid,UI_ActivityGridRow_0_AutoID,UI_TextBox_AutoID"
	#And I send "{TAB}" to ""
	#And "VARIABLESCALAR,UI_Variable_Delete1_AutoID,UI_NameTextBox_AutoID" is visible within "1" seconds
	#And I send "ROW1" to ""
	##Adding ROW 2
	#And I send "{TAB}" to ""
	#And I send "Delete2{TAB}" to ""
	#And I send "Row2{TAB}" to ""
	##Deleting Row 1
	#Given I right click "UI_DocManager_AutoID,UI_SplitPane_AutoID,UI_TabManager_AutoID,UI_WorkflowDesigner_AutoID,UserControl_1,Unsaved 1(FlowchartDesigner),Assign (2)(MultiAssignDesigner),SmallViewContent,SmallDataGrid,UI_ActivityGridRow_0_AutoID,UI_DataGridCell_AutoID[2]"
	#And I send "{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}{TAB}{ENTER}" to ""
	##Checking Expected Row Deleted
	#Given "WORKSURFACE,Assign (1)(MultiAssignDesigner),SmallViewContent,SmallDataGrid,UI_ActivityGridRow_0_AutoID,UI_TextBox_AutoID" contains text "Delete2"
	##Checking Delete and Inser Row Options are disabled when Grid contains one row only
	Given I right click "UI_DocManager_AutoID,UI_SplitPane_AutoID,UI_TabManager_AutoID,UI_WorkflowDesigner_AutoID,UserControl_1,Unsaved 1(FlowchartDesigner),Assign (1)(MultiAssignDesigner),SmallViewContent,SmallDataGrid,UI_ActivityGridRow_0_AutoID,UI_DataGridCell_AutoID[2]" 
	And I send "{TAB}{TAB}" to ""
	Given "UI_DeleteRowMenuItem_AutoID" is disabled
	Then "UI_InsertRowMenuItem_AutoID" is disabled within "2" seconds	



Scenario: Drag resource multiple times from explorer and expected mappings are not changing
	Given I have Warewolf running
	And all tabs are closed
	And I click "EXPLORER,UI_localhost_AutoID"
	And I click new "Workflow"
	And I send "Utility - Assign" to "EXPLORERFILTER"
	Given I drag "EXPLORER,UI_localhost_AutoID,UI_Examples_AutoID,UI_Utility - Assign_AutoID" onto "WORKSURFACE,StartSymbol"
	Given "WORKSURFACE,Examples\Utility - Assign(ServiceDesigner),LargeViewContent" is visible
	##Testing Row1 Vriable
	#Given "WORKSURFACE,Examples\Utility - Assign(ServiceDesigner),LargeViewContent,OutputsDataGrid,UI_DataGridCell_AutoID" contains text "[[rec().set]]"
	##Testing Row1 Vriable
	#Given "WORKSURFACE,Examples\Utility - Assign(ServiceDesigner),LargeViewContent,OutputsDataGrid,UI_DataGridCell_AutoID" contains text "[[rec().set]]"
	##Testing Row1 Vriable
	#Given "WORKSURFACE,Examples\Utility - Assign(ServiceDesigner),LargeViewContent,OutputsDataGrid,UI_DataGridCell_AutoID" contains text "[[rec().set]]"
	Given I right click "WORKSURFACE,Examples\Utility - Assign(ServiceDesigner)"
	And I send "{TAB}{TAB}{TAB}{TAB}{ENTER}" to ""
	Given I drag "EXPLORER,UI_localhost_AutoID,UI_Examples_AutoID,UI_Utility - Assign_AutoID" onto "WORKSURFACE,StartSymbol"
	Given "WORKSURFACE,Examples\Utility - Assign(ServiceDesigner),LargeViewContent" is visible
	##Testing Row1 Vriable
	#Given "WORKSURFACE,Examples\Utility - Assign(ServiceDesigner),LargeViewContent,OutputsDataGrid,UI_DataGridCell_AutoID" contains text "[[rec().set]]"
	##Testing Row1 Vriable
	#Given "WORKSURFACE,Examples\Utility - Assign(ServiceDesigner),LargeViewContent,OutputsDataGrid,UI_DataGridCell_AutoID" contains text "[[rec().set]]"
	##Testing Row1 Vriable
	#Given "WORKSURFACE,Examples\Utility - Assign(ServiceDesigner),LargeViewContent,OutputsDataGrid,UI_DataGridCell_AutoID" contains text "[[rec().set]]"