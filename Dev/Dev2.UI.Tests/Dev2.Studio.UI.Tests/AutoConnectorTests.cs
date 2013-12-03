﻿using System.Collections.Generic;
using System;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Studio.UI.Tests
{
    /// <summary>
    ///    These are UI tests around the auto connectors
    /// </summary>
    [CodedUITest]
    public class AutoConnectorTests : UIMapBase
    {

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
        
        #region Tests

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("AutoConnectorTests")]
        public void AutoConnectorTests_DragActivityOnStartAutoConnectorNode_AConnectorIsCreated()
        {
            CreateWorkflow();

            ExplorerUIMap.EnterExplorerSearchText("email service");
            Point point = WorkflowDesignerUIMap.GetStartNodeBottomAutoConnectorPoint();
            ExplorerUIMap.DragControlToWorkflowDesigner("localhost", "SERVICES", "COMMUNICATION", "Email Service", point);
            //If the screen resolution is low or if the studio is windowed this point can jump as soon as the control is dragged over the work surface, the control might need to be re-dragged to hit the connector line
            Point newPoint = WorkflowDesignerUIMap.GetStartNodeBottomAutoConnectorPoint();
            if (point != newPoint)
            {
                WorkflowDesignerUIMap.DragControl("Email Service", newPoint);
            }
            List<UITestControl> connectors = WorkflowDesignerUIMap.GetAllConnectors();
            //Assert start auto connector worked
            Assert.AreEqual(1, connectors.Count, "Start auto connector was not created");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("AutoConnectorTests")]
        public void AutoConnectorTests_DragAToolOnStartAutoConnectorNode_AConnectorIsCreated()
        {
            CreateWorkflow();

            Point point = WorkflowDesignerUIMap.GetStartNodeBottomAutoConnectorPoint();
            //Drag a control to the design surface
            ToolboxUIMap.DragControlToWorkflowDesigner("Assign", point);
            //If the screen resolution is low or if the studio is windowed this point can jump as soon as the control is dragged over the work surface, the control might need to be re-dragged to hit the connector line
            Point newPoint = WorkflowDesignerUIMap.GetStartNodeBottomAutoConnectorPoint();
            if(point != newPoint)
            {
                WorkflowDesignerUIMap.DragControl("Assign", newPoint);
            }
            List<UITestControl> connectors = WorkflowDesignerUIMap.GetAllConnectors();
            //Assert start auto connector worked
            Assert.AreEqual(1, connectors.Count, "Start auto connector was not created");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("AutoConnectorTests")]
        public void AutoConnectorTests_DragAToolOnALineBetweenConnectors_ASecondConnectorIsCreated()
        {
            //Drag a tool to the design surface
            ExplorerUIMap.EnterExplorerSearchText("AutoConnectorResource");
            ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "BUGS", "AutoConnectorResource");
            var theTab = TabManagerUIMap.GetActiveTab();
            UITestControl control = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "MultiAssignDesigner");

            //Drag a tool to the design surface
            //Note that this point is a position relative to the multi assign on the design surface. This is to ensure that the tool is dropped exactly on the line
            if (control != null)
            {
                var point = new Point(control.BoundingRectangle.X + 120, control.BoundingRectangle.Y - 150);
                ToolboxUIMap.DragControlToWorkflowDesigner("Assign", point);
                //If the screen resolution is low or if the studio is windowed this point can jump as soon as the control is dragged over the work surface, the control might need to be re-dragged to hit the connector line
                var newPoint = new Point(control.BoundingRectangle.X + 120, control.BoundingRectangle.Y - 150);
                if(point != newPoint)
                {
                    var theControl = WorkflowDesignerUIMap.GetAllControlsOnDesignSurface(theTab)[5];
                    Mouse.StartDragging(theControl, new Point(10, 10));
                    Mouse.StopDragging(newPoint);
                }
            }
            else
            {
                throw new Exception("MultiAssignDesigner not found on active tab");
            }
            var connectors = WorkflowDesignerUIMap.GetAllConnectors();
            //Assert that the line was split
            Assert.IsTrue(connectors.Count >= 2, "Connector line wasn't split");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("AutoConnectorTests")]
        public void AutoConnectorTests_DragAnActivityOnALineBetweenConnectors_ASecondConnectorIsCreated()
        {
            //Drag an activity to the design surface
            ExplorerUIMap.EnterExplorerSearchText("AutoConnectorResource");
            ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "BUGS", "AutoConnectorResource");
            var control = WorkflowDesignerUIMap.FindControlByAutomationId(TabManagerUIMap.GetActiveTab(), "MultiAssignDesigner");
            // Drag another service to over the line between two connectors
            ExplorerUIMap.EnterExplorerSearchText("email service");
            //Note that this point is a position relative to the multi assign on the design surface. This is to ensure that the tool is dropped exactly on the line
            if (control != null)
            {
                var point = new Point(control.BoundingRectangle.X + 120, control.BoundingRectangle.Y - 150);
                ExplorerUIMap.DragControlToWorkflowDesigner("localhost", "SERVICES", "COMMUNICATION", "Email Service", point);
                if (WorkflowDesignerUIMap.TryCloseMappings("Email Service"))
                {
                    //If the screen resolution is low or if the studio is windowed this point can jump as soon as the control is dragged over the work surface, the control might need to be re-dragged to hit the connector line
                    var newPoint = new Point(control.BoundingRectangle.X + 120, control.BoundingRectangle.Y - 150);
                    if(point != newPoint)
                    {
                        WorkflowDesignerUIMap.DragControl("Email Service", new Point(control.BoundingRectangle.X + 120, control.BoundingRectangle.Y - 50));
                    }
                }
                else
                {
                    //If the screen resolution is low or if the studio is windowed this point can jump as soon as the control is dragged over the work surface, the control might need to be re-dragged to hit the connector line
                    var newPoint = new Point(control.BoundingRectangle.X + 120, control.BoundingRectangle.Y - 150);
                    if (point != newPoint)
                    {
                        WorkflowDesignerUIMap.DragControl("Email Service", newPoint);
                    }
                }
            }
            else
            {
                throw new Exception("MultiAssignDesigner not found on active tab");
            }
            var connectors = WorkflowDesignerUIMap.GetAllConnectors();
            //Assert start auto connector worked
            Assert.IsTrue(connectors.Count >= 2, "Connector line wasn't split");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("AutoConnectorTests")]
        public void AutoConnectorTests_DragADecisionOnALineBetweenConnectors_ASecondConnectorIsCreated()
        {

            ExplorerUIMap.EnterExplorerSearchText("AutoConnectorResource");
            ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "BUGS", "AutoConnectorResource");
            var control = WorkflowDesignerUIMap.FindControlByAutomationId(TabManagerUIMap.GetActiveTab(), "MultiAssignDesigner");

            //Drag a decision to the design surface
            //Note that this point is a position relative to the multi assign on the design surface. This is to ensure that the tool is dropped exactly on the line
            if (control != null)
            {
                var point = new Point(control.BoundingRectangle.X + 120, control.BoundingRectangle.Y - 150);
                ToolboxUIMap.DragControlToWorkflowDesigner("Decision", point);
            }
            else
            {
                throw new Exception("MultiAssignDesigner not found on active tab");
            }

            Playback.Wait(3500);
    
            DecisionWizardUIMap.ClickCancel();
            var connectors = WorkflowDesignerUIMap.GetAllConnectors();
            //Assert start auto connector worked
            Assert.IsTrue(connectors.Count == 2, "Connector line wasn't split");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("AutoConnectorTests")]
        public void AutoConnectorTests_DragADecisionOnStartAutoConnectorNode_ASecondConnectorIsCreated()
        {
            CreateWorkflow();

            Point point = WorkflowDesignerUIMap.GetStartNodeBottomAutoConnectorPoint();
            //Drag a control to the design surface
            ToolboxUIMap.DragControlToWorkflowDesigner("Decision", point);
            Playback.Wait(3500);
            DecisionWizardUIMap.ClickCancel();
            //If the screen resolution is low or if the studio is windowed this point can jump as soon as the control is dragged over the work surface, the control might need to be re-dragged to hit the connector line
            Point newPoint = WorkflowDesignerUIMap.GetStartNodeBottomAutoConnectorPoint();
            if (point != newPoint)
            {
                WorkflowDesignerUIMap.DragControl("Decision", newPoint);
            }
            var connectors = WorkflowDesignerUIMap.GetAllConnectors();
            //Assert start auto connector worked
            Assert.AreEqual(1, connectors.Count, "Start auto connector doesnt work");
        }

        #endregion

        #region Utils

        public void CreateWorkflow()
        {
            RibbonUIMap.CreateNewWorkflow();
        }

        #endregion
    }
}