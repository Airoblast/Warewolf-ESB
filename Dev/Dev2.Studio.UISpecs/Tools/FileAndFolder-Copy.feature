﻿Feature: FileAndFolder-Copy
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@Copy
Scenario:CopyTool Large View And Invalid Variables Expected Error On Done Button
	Given I have Warewolf running
	And all tabs are closed	
	And I click "EXPLORER,UI_localhost_AutoID"
	And I click "RIBBONNEWENDPOINT"
	#Dragging Copy Tool From Tool Box
	Given I send "Create" to "TOOLBOX,PART_SearchBox"
    Given I drag "TOOLCREATE" onto "WORKSURFACE,StartSymbol"
	#Opening Copy Large View
	Given I double click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Create(CreateDesigner)"
	#BuG12561 Passing Invalid Recordset Variable in  File or Folder Field And Checking Validation on Done
	Given I type "[[rec@(1).a]]" in "WORKSURFACE,Create(CreateDesigner),LargeViewContent,UI__FileNametxt_AutoID"
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Create(CreateDesigner),DoneButton"
	#BUg 12561 Given "WORKSURFACE,UI_Error0_AutoID" is visible