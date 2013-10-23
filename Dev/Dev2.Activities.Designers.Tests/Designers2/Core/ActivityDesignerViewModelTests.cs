﻿using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Data;
using Dev2.Activities.Designers.Tests.Designers2.Core.Stubs;
using Dev2.Activities.Designers2.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;using System.Diagnostics.CodeAnalysis;
using Moq;
using Moq.Protected;

// ReSharper disable InconsistentNaming
namespace Dev2.Activities.Designers.Tests.Designers2.Core
{
    [TestClass][ExcludeFromCodeCoverage]
    public class ActivityDesignerViewModelTests
    {

        [TestMethod]
        [TestCategory("ActivityDesignerViewModel_UnitTest")]
        [Description("Base activity view model can initialize")]
        [Owner("Ashley Lewis")]
        public void ActivityDesignerViewModel_Constructor_EmptyModelItem_ViewModelConstructed()
        {
            //init
            var mockModel = new Mock<ModelItem>();

            //exe
            var vm = new TestActivityDesignerViewModel(mockModel.Object);

            //assert
            Assert.IsInstanceOfType(vm, typeof(ActivityDesignerViewModel), "Activity view model base cannot initialize");
        }


        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityDesignerViewModel_AddTitleBarHelpToggle")]
        public void ActivityDesignerViewModel_AddTitleBarHelpToggle_Added()
        {
            //------------Setup for test--------------------------
            var mockModelItem = GenerateMockModelItem();
            var viewModel = new TestActivityDesignerViewModel(mockModelItem.Object);

            //------------Execute Test---------------------------
            viewModel.TestAddTitleBarHelpToggle();

            //------------Assert Results-------------------------
            Assert.AreEqual(1, viewModel.TitleBarToggles.Count);

            var toggle = viewModel.TitleBarToggles[0];

            Assert.AreEqual("pack://application:,,,/Dev2.Activities.Designers;component/Images/ServiceHelp-32.png", toggle.CollapseImageSourceUri);
            Assert.AreEqual("Close Help", toggle.CollapseToolTip);
            Assert.AreEqual("pack://application:,,,/Dev2.Activities.Designers;component/Images/ServiceHelp-32.png", toggle.ExpandImageSourceUri);
            Assert.AreEqual("Open Help", toggle.ExpandToolTip);
            Assert.AreEqual("HelpToggle", toggle.AutomationID);

            var binding = BindingOperations.GetBinding(viewModel, ActivityDesignerViewModel.ShowHelpProperty);
            Assert.IsNotNull(binding);
            Assert.AreEqual(toggle, binding.Source);
            Assert.AreEqual(ActivityDesignerToggle.IsCheckedProperty.Name, binding.Path.Path);
            Assert.AreEqual(BindingMode.TwoWay, binding.Mode);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityDesignerViewModel_AddTitleBarLargeToggle")]
        public void ActivityDesignerViewModel_AddTitleBarLargeToggle_Added()
        {
            //------------Setup for test--------------------------
            var mockModelItem = GenerateMockModelItem();
            var viewModel = new TestActivityDesignerViewModel(mockModelItem.Object);

            //------------Execute Test---------------------------
            viewModel.TestAddTitleBarLargeToggle();

            //------------Assert Results-------------------------
            Assert.AreEqual(1, viewModel.TitleBarToggles.Count);

            var toggle = viewModel.TitleBarToggles[0];

            Assert.AreEqual("pack://application:,,,/Dev2.Activities.Designers;component/Images/ServiceCollapseMapping-32.png", toggle.CollapseImageSourceUri);
            Assert.AreEqual("Close Large View", toggle.CollapseToolTip);
            Assert.AreEqual("pack://application:,,,/Dev2.Activities.Designers;component/Images/ServiceExpandMapping-32.png", toggle.ExpandImageSourceUri);
            Assert.AreEqual("Open Large View", toggle.ExpandToolTip);
            Assert.AreEqual("LargeViewToggle", toggle.AutomationID);

            var binding = BindingOperations.GetBinding(viewModel, ActivityDesignerViewModel.ShowLargeProperty);
            Assert.IsNotNull(binding);
            Assert.AreEqual(toggle, binding.Source);
            Assert.AreEqual(ActivityDesignerToggle.IsCheckedProperty.Name, binding.Path.Path);
            Assert.AreEqual(BindingMode.TwoWay, binding.Mode);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityDesignerViewModel_Collapse")]
        public void ActivityDesignerViewModel_Collapse_SmallViewActive()
        {
            //------------Setup for test--------------------------
            var mockModelItem = GenerateMockModelItem();
            var viewModel = new TestActivityDesignerViewModel(mockModelItem.Object);

            viewModel.ShowLarge = true;

            //------------Execute Test---------------------------
            viewModel.Collapse();

            //------------Assert Results-------------------------
            Assert.IsTrue(viewModel.IsSmallViewActive);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityDesignerViewModel_Restore")]
        public void ActivityDesignerViewModel_RestoreFromPreviouslyViewedLargeView_LargeViewActive()
        {
            //------------Setup for test--------------------------
            var mockModelItem = GenerateMockModelItem();
            var viewModel = new TestActivityDesignerViewModel(mockModelItem.Object);


            viewModel.PreviousView = ActivityDesignerViewModel.ShowLargeProperty.Name;

            //------------Execute Test---------------------------
            viewModel.Restore();

            //------------Assert Results-------------------------
            Assert.IsTrue(viewModel.ShowLarge);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityDesignerViewModel_Expand")]
        public void ActivityDesignerViewModel_Expand_LargeViewActive()
        {
            //------------Setup for test--------------------------
            var mockModelItem = GenerateMockModelItem();
            var viewModel = new TestActivityDesignerViewModel(mockModelItem.Object);

            //------------Execute Test---------------------------
            viewModel.Expand();

            //------------Assert Results-------------------------
            Assert.IsTrue(viewModel.ShowLarge);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityDesignerViewModel_ThumbVisibility")]
        public void ActivityDesignerViewModel_ThumbVisibility_IsSelectedAndSmallViewNotActive_Visible()
        {
            //------------Setup for test--------------------------
            var mockModelItem = GenerateMockModelItem();
            var viewModel = new TestActivityDesignerViewModel(mockModelItem.Object);
            viewModel.IsSelected = true;

            //------------Execute Test---------------------------
            viewModel.ShowLarge = true;

            //------------Assert Results-------------------------
            Assert.AreEqual(Visibility.Visible, viewModel.ThumbVisibility);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityDesignerViewModel_ThumbVisibility")]
        public void ActivityDesignerViewModel_ThumbVisibility_IsSelectedAndSmallViewActive_Collapsed()
        {
            //------------Setup for test--------------------------
            var mockModelItem = GenerateMockModelItem();
            var viewModel = new TestActivityDesignerViewModel(mockModelItem.Object);
            viewModel.IsSelected = true;
            viewModel.ShowLarge = true;

            //------------Execute Test---------------------------
            viewModel.ShowLarge = false;

            //------------Assert Results-------------------------
            Assert.AreEqual(Visibility.Collapsed, viewModel.ThumbVisibility);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityDesignerViewModel_ConnectorVisibility")]
        public void ActivityDesignerViewModel_ConnectorVisibility_IsSelectedAndSmallViewNotActive_Collapsed()
        {
            //------------Setup for test--------------------------
            var mockModelItem = GenerateMockModelItem();
            var viewModel = new TestActivityDesignerViewModel(mockModelItem.Object);
            viewModel.IsSelected = true;

            //------------Execute Test---------------------------
            viewModel.ShowLarge = true;

            //------------Assert Results-------------------------
            Assert.AreEqual(Visibility.Collapsed, viewModel.ConnectorVisibility);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityDesignerViewModel_ConnectorVisibility")]
        public void ActivityDesignerViewModel_ConnectorVisibility_IsSelectedAndSmallViewActive_Visible()
        {
            //------------Setup for test--------------------------
            var mockModelItem = GenerateMockModelItem();
            var viewModel = new TestActivityDesignerViewModel(mockModelItem.Object);
            viewModel.IsSelected = true;
            viewModel.ShowLarge = true;

            //------------Execute Test---------------------------
            viewModel.ShowLarge = false;

            //------------Assert Results-------------------------
            Assert.AreEqual(Visibility.Visible, viewModel.ConnectorVisibility);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityDesignerViewModel_TitleBarTogglesVisibility")]
        public void ActivityDesignerViewModel_TitleBarTogglesVisibility_IsSelected_Visible()
        {
            //------------Setup for test--------------------------
            var mockModelItem = GenerateMockModelItem();
            var viewModel = new TestActivityDesignerViewModel(mockModelItem.Object);

            //------------Execute Test---------------------------
            viewModel.IsSelected = true;

            //------------Assert Results-------------------------
            Assert.AreEqual(Visibility.Visible, viewModel.TitleBarTogglesVisibility);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityDesignerViewModel_TitleBarTogglesVisibility")]
        public void ActivityDesignerViewModel_TitleBarTogglesVisibility_NotIsSelected_Collapsed()
        {
            //------------Setup for test--------------------------
            var mockModelItem = GenerateMockModelItem();
            var viewModel = new TestActivityDesignerViewModel(mockModelItem.Object);

            //------------Execute Test---------------------------
            viewModel.IsSelected = false;

            //------------Assert Results-------------------------
            Assert.AreEqual(Visibility.Collapsed, viewModel.TitleBarTogglesVisibility);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityDesignerViewModel_ZIndexPosition")]
        public void ActivityDesignerViewModel_ZIndexPosition_IsSelected_Front()
        {
            //------------Setup for test--------------------------
            var mockModelItem = GenerateMockModelItem();
            var viewModel = new TestActivityDesignerViewModel(mockModelItem.Object);

            //------------Execute Test---------------------------
            viewModel.IsSelected = true;

            //------------Assert Results-------------------------
            Assert.AreEqual(ZIndexPosition.Front, viewModel.ZIndexPosition);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityDesignerViewModel_ZIndexPosition")]
        public void ActivityDesignerViewModel_ZIndexPosition_NotIsSelected_Back()
        {
            //------------Setup for test--------------------------
            var mockModelItem = GenerateMockModelItem();
            var viewModel = new TestActivityDesignerViewModel(mockModelItem.Object);

            //------------Execute Test---------------------------
            viewModel.IsSelected = false;

            //------------Assert Results-------------------------
            Assert.AreEqual(ZIndexPosition.Back, viewModel.ZIndexPosition);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityDesignerViewModel_ThumbVisibility")]
        public void ActivityDesignerViewModel_ThumbVisibility_IsMouseOverAndSmallViewNotActive_Visible()
        {
            //------------Setup for test--------------------------
            var mockModelItem = GenerateMockModelItem();
            var viewModel = new TestActivityDesignerViewModel(mockModelItem.Object);
            viewModel.IsMouseOver = true;

            //------------Execute Test---------------------------
            viewModel.ShowLarge = true;

            //------------Assert Results-------------------------
            Assert.AreEqual(Visibility.Visible, viewModel.ThumbVisibility);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityDesignerViewModel_ThumbVisibility")]
        public void ActivityDesignerViewModel_ThumbVisibility_IsMouseOverAndSmallViewActive_Collapsed()
        {
            //------------Setup for test--------------------------
            var mockModelItem = GenerateMockModelItem();
            var viewModel = new TestActivityDesignerViewModel(mockModelItem.Object);
            viewModel.IsMouseOver = true;
            viewModel.ShowLarge = true;

            //------------Execute Test---------------------------
            viewModel.ShowLarge = false;

            //------------Assert Results-------------------------
            Assert.AreEqual(Visibility.Collapsed, viewModel.ThumbVisibility);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityDesignerViewModel_ConnectorVisibility")]
        public void ActivityDesignerViewModel_ConnectorVisibility_IsMouseOverAndSmallViewNotActive_Collapsed()
        {
            //------------Setup for test--------------------------
            var mockModelItem = GenerateMockModelItem();
            var viewModel = new TestActivityDesignerViewModel(mockModelItem.Object);
            viewModel.IsMouseOver = true;

            //------------Execute Test---------------------------
            viewModel.ShowLarge = true;

            //------------Assert Results-------------------------
            Assert.AreEqual(Visibility.Collapsed, viewModel.ConnectorVisibility);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityDesignerViewModel_ConnectorVisibility")]
        public void ActivityDesignerViewModel_ConnectorVisibility_IsMouseOverAndSmallViewActive_Visible()
        {
            //------------Setup for test--------------------------
            var mockModelItem = GenerateMockModelItem();
            var viewModel = new TestActivityDesignerViewModel(mockModelItem.Object);
            viewModel.IsMouseOver = true;
            viewModel.ShowLarge = true;

            //------------Execute Test---------------------------
            viewModel.ShowLarge = false;

            //------------Assert Results-------------------------
            Assert.AreEqual(Visibility.Visible, viewModel.ConnectorVisibility);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityDesignerViewModel_TitleBarTogglesVisibility")]
        public void ActivityDesignerViewModel_TitleBarTogglesVisibility_IsMouseOver_Visible()
        {
            //------------Setup for test--------------------------
            var mockModelItem = GenerateMockModelItem();
            var viewModel = new TestActivityDesignerViewModel(mockModelItem.Object);

            //------------Execute Test---------------------------
            viewModel.IsMouseOver = true;

            //------------Assert Results-------------------------
            Assert.AreEqual(Visibility.Visible, viewModel.TitleBarTogglesVisibility);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityDesignerViewModel_TitleBarTogglesVisibility")]
        public void ActivityDesignerViewModel_TitleBarTogglesVisibility_NotIsMouseOver_Collapsed()
        {
            //------------Setup for test--------------------------
            var mockModelItem = GenerateMockModelItem();
            var viewModel = new TestActivityDesignerViewModel(mockModelItem.Object);

            //------------Execute Test---------------------------
            viewModel.IsMouseOver = false;

            //------------Assert Results-------------------------
            Assert.AreEqual(Visibility.Collapsed, viewModel.TitleBarTogglesVisibility);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityDesignerViewModel_ZIndexPosition")]
        public void ActivityDesignerViewModel_ZIndexPosition_IsMouseOver_Front()
        {
            //------------Setup for test--------------------------
            var mockModelItem = GenerateMockModelItem();
            var viewModel = new TestActivityDesignerViewModel(mockModelItem.Object);

            //------------Execute Test---------------------------
            viewModel.IsMouseOver = true;

            //------------Assert Results-------------------------
            Assert.AreEqual(ZIndexPosition.Front, viewModel.ZIndexPosition);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityDesignerViewModel_ZIndexPosition")]
        public void ActivityDesignerViewModel_ZIndexPosition_NotIsMouseOver_Back()
        {
            //------------Setup for test--------------------------
            var mockModelItem = GenerateMockModelItem();
            var viewModel = new TestActivityDesignerViewModel(mockModelItem.Object);

            //------------Execute Test---------------------------
            viewModel.IsMouseOver = false;

            //------------Assert Results-------------------------
            Assert.AreEqual(ZIndexPosition.Back, viewModel.ZIndexPosition);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityDesignerViewModel_ShowItemHelpCommand")]
        public void ActivityDesignerViewModel_ShowItemHelpCommand_InvokesGivenAction()
        {
            //------------Setup for test--------------------------
            Type showExampleType = null;
            var mockModelItem = GenerateMockModelItem();

            var viewModel = new TestActivityDesignerViewModel(mockModelItem.Object, type =>
            {
                showExampleType = type;
            });

            //------------Execute Test---------------------------
            viewModel.ShowItemHelpCommand.Execute(null);

            //------------Assert Results-------------------------
            Assert.IsNotNull(showExampleType);
            Assert.AreEqual(mockModelItem.Object.ItemType, showExampleType);
        }

        static Mock<ModelItem> GenerateMockModelItem()
        {
            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var propertyCollection = new Mock<ModelPropertyCollection>();

            var displayName = new Mock<ModelProperty>();
            displayName.Setup(p => p.ComputedValue).Returns("Activity");
            properties.Add("DisplayName", displayName);
            propertyCollection.Protected().Setup<ModelProperty>("Find", "DisplayName", true).Returns(displayName.Object);

            var mockModelItem = new Mock<ModelItem>();
            mockModelItem.Setup(mi => mi.ItemType).Returns(typeof(TestActivity));
            mockModelItem.Setup(s => s.Properties).Returns(propertyCollection.Object);

            return mockModelItem;
        }
    }

}
