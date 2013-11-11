﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Windows.Forms;
using CubicOrange.Windows.Forms.ActiveDirectory;
using Dev2.Data.Settings.Security;
using Dev2.Dialogs;
using Dev2.Help;
using Dev2.Settings.Security;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.AppResources.ExtensionMethods;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.Settings
{
    [TestClass]
    public class SecurityViewModelTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SecurityViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SecurityViewModel_Constructor_PermissionsIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var viewModel = new SecurityViewModel(null, null, null, null, null);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SecurityViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SecurityViewModel_Constructor_ResourcePickerDialogIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var viewModel = new SecurityViewModel(new List<WindowsGroupPermission>(), null, null, null, null);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SecurityViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SecurityViewModel_Constructor_DirectoryObjectPickerDialogIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var viewModel = new SecurityViewModel(new List<WindowsGroupPermission>(), new Mock<IResourcePickerDialog>().Object, null, null, null);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SecurityViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SecurityViewModel_Constructor_ParentWindowIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var viewModel = new SecurityViewModel(new List<WindowsGroupPermission>(), new Mock<IResourcePickerDialog>().Object, new Mock<IDirectoryObjectPickerDialog>().Object, null, null);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SecurityViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SecurityViewModel_Constructor_EnvironmentIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var viewModel = new SecurityViewModel(new List<WindowsGroupPermission>(), new Mock<IResourcePickerDialog>().Object, new Mock<IDirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, null);

            //------------Assert Results-------------------------
        }


        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SecurityViewModel_Constructor")]
        public void SecurityViewModel_Constructor_Properties_Initialized()
        {
            //------------Setup for test--------------------------
            var permissions = new[]
            {
                new WindowsGroupPermission
                {
                    IsServer = true, WindowsGroup = "Deploy Admins",
                    View = false, Execute = false, Contribute = false, DeployTo = true, DeployFrom = true, Administrator = false
                },

                new WindowsGroupPermission
                {
                    ResourceID = Guid.NewGuid(), ResourceName = "Category1\\Workflow1",
                    WindowsGroup = "Windows Group 1", View = false, Execute = true, Contribute = false
                }
            };

            //------------Execute Test---------------------------
            var viewModel = new SecurityViewModel(permissions, new Mock<IResourcePickerDialog>().Object, new Mock<IDirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IEnvironmentModel>().Object);

            //------------Assert Results-------------------------
            Assert.IsTrue(viewModel.CloseHelpCommand.CanExecute(null));
            Assert.IsTrue(viewModel.PickResourceCommand.CanExecute(null));
            Assert.IsTrue(viewModel.PickWindowsGroupCommand.CanExecute(null));
            Assert.IsNotNull(viewModel.PickResourceCommand);
            Assert.IsNotNull(viewModel.PickWindowsGroupCommand);
            Assert.IsNotNull(viewModel.ServerPermissions);
            Assert.IsNotNull(viewModel.ResourcePermissions);

            var serverPerms = permissions.Where(p => p.IsServer).ToList();
            var resourcePerms = permissions.Where(p => !p.IsServer).ToList();

            // constructor adds an extra "new"  permission
            Assert.AreEqual(serverPerms.Count + 1, viewModel.ServerPermissions.Count);
            Assert.AreEqual(resourcePerms.Count + 1, viewModel.ResourcePermissions.Count);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SecurityViewModel_OnPermissionPropertyChanged")]
        public void SecurityViewModel_OnPermissionPropertyChanged_PermissionChanged_IsDirtyIsTrue()
        {
            //------------Setup for test--------------------------
            var permission = new WindowsGroupPermission
            {
                IsServer = true,
                WindowsGroup = "Deploy Admins",
                View = false,
                Execute = false,
                Contribute = false,
                DeployTo = true,
                DeployFrom = true,
                Administrator = false
            };
            var viewModel = new SecurityViewModel(new[] { permission }, new Mock<IResourcePickerDialog>().Object, new Mock<IDirectoryObjectPickerDialog>().Object, new Mock<System.Windows.Forms.IWin32Window>().Object, new Mock<IEnvironmentModel>().Object);

            Assert.IsFalse(viewModel.IsDirty);

            //------------Execute Test---------------------------
            permission.Administrator = true;

            //------------Assert Results-------------------------
            Assert.IsTrue(viewModel.IsDirty);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SecurityViewModel_OnPermissionPropertyChanged")]
        public void SecurityViewModel_OnPermissionPropertyChanged_ServerPermissionWindowsGroupChangedToNonEmptyAndIsNew_NewServerPermissionIsAdded()
        {
            //------------Setup for test--------------------------
            var viewModel = new SecurityViewModel(new List<WindowsGroupPermission>(), new Mock<IResourcePickerDialog>().Object, new Mock<IDirectoryObjectPickerDialog>().Object, new Mock<System.Windows.Forms.IWin32Window>().Object, new Mock<IEnvironmentModel>().Object);

            Assert.AreEqual(1, viewModel.ServerPermissions.Count);
            Assert.IsTrue(viewModel.ServerPermissions[0].IsNew);

            //------------Execute Test---------------------------
            viewModel.ServerPermissions[0].WindowsGroup = "xxx";

            //------------Assert Results-------------------------
            Assert.IsFalse(viewModel.ServerPermissions[0].IsNew);

            Assert.AreEqual(2, viewModel.ServerPermissions.Count);
            Assert.IsTrue(viewModel.ServerPermissions[1].IsNew);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SecurityViewModel_OnPermissionPropertyChanged")]
        public void SecurityViewModel_OnPermissionPropertyChanged_ServerPermissionWindowsGroupChangedToEmptyAndIsNotNew_ServerPermissionIsRemoved()
        {
            //------------Setup for test--------------------------
            var permission = new WindowsGroupPermission
            {
                IsServer = true,
                WindowsGroup = "Deploy Admins",
                View = false,
                Execute = false,
                Contribute = false,
                DeployTo = true,
                DeployFrom = true,
                Administrator = false
            };
            var viewModel = new SecurityViewModel(new[] { permission }, new Mock<IResourcePickerDialog>().Object, new Mock<IDirectoryObjectPickerDialog>().Object, new Mock<System.Windows.Forms.IWin32Window>().Object, new Mock<IEnvironmentModel>().Object);

            Assert.AreEqual(2, viewModel.ServerPermissions.Count);
            Assert.IsFalse(viewModel.ServerPermissions[0].IsNew);
            Assert.IsTrue(viewModel.ServerPermissions[1].IsNew);
            Assert.AreSame(permission, viewModel.ServerPermissions[0]);

            //------------Execute Test---------------------------
            viewModel.ServerPermissions[0].WindowsGroup = "";

            //------------Assert Results-------------------------
            Assert.AreEqual(1, viewModel.ServerPermissions.Count);

            Assert.IsTrue(viewModel.ServerPermissions[0].IsNew);
            Assert.AreNotSame(permission, viewModel.ServerPermissions[0]);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SecurityViewModel_OnPermissionPropertyChanged")]
        public void SecurityViewModel_OnPermissionPropertyChanged_ResourcePermissionWindowsGroupAndResourceNameChangedToNonEmptyAndIsNew_NewResourcePermissionIsAdded()
        {
            //------------Setup for test--------------------------
            var viewModel = new SecurityViewModel(new List<WindowsGroupPermission>(), new Mock<IResourcePickerDialog>().Object, new Mock<IDirectoryObjectPickerDialog>().Object, new Mock<System.Windows.Forms.IWin32Window>().Object, new Mock<IEnvironmentModel>().Object);

            Assert.AreEqual(1, viewModel.ResourcePermissions.Count);
            Assert.IsTrue(viewModel.ResourcePermissions[0].IsNew);

            //------------Execute Test---------------------------
            viewModel.ResourcePermissions[0].ResourceName = "xxx";
            viewModel.ResourcePermissions[0].WindowsGroup = "xxx";

            //------------Assert Results-------------------------
            Assert.IsFalse(viewModel.ResourcePermissions[0].IsNew);

            Assert.AreEqual(2, viewModel.ResourcePermissions.Count);
            Assert.IsTrue(viewModel.ResourcePermissions[1].IsNew);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SecurityViewModel_OnPermissionPropertyChanged")]
        public void SecurityViewModel_OnPermissionPropertyChanged_ResourcePermissionWindowsGroupChangedToEmptyAndIsNotNew_ResourcePermissionIsRemoved()
        {
            //------------Setup for test--------------------------
            var permission = new WindowsGroupPermission
            {
                IsServer = false,
                WindowsGroup = "Deploy Admins",
                View = false,
                Execute = false,
                Contribute = false,
                DeployTo = true,
                DeployFrom = true,
                Administrator = false
            };
            var viewModel = new SecurityViewModel(new[] { permission }, new Mock<IResourcePickerDialog>().Object, new Mock<IDirectoryObjectPickerDialog>().Object, new Mock<System.Windows.Forms.IWin32Window>().Object, new Mock<IEnvironmentModel>().Object);

            Assert.AreEqual(2, viewModel.ResourcePermissions.Count);
            Assert.IsFalse(viewModel.ResourcePermissions[0].IsNew);
            Assert.IsTrue(viewModel.ResourcePermissions[1].IsNew);
            Assert.AreSame(permission, viewModel.ResourcePermissions[0]);

            //------------Execute Test---------------------------
            viewModel.ResourcePermissions[0].WindowsGroup = "";

            //------------Assert Results-------------------------
            Assert.AreEqual(1, viewModel.ResourcePermissions.Count);

            Assert.IsTrue(viewModel.ResourcePermissions[0].IsNew);
            Assert.AreNotSame(permission, viewModel.ResourcePermissions[0]);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SecurityViewModel_PickWindowsGroupCommand")]
        public void SecurityViewModel_PickWindowsGroupCommand_DialogResultIsNotOK_DoesNothing()
        {
            //------------Setup for test--------------------------
            var resourceID = Guid.NewGuid();
            const string ResourceName = "Cat\\Resource";
            var permission = new WindowsGroupPermission
            {
                IsServer = false,
                WindowsGroup = "Deploy Admins",
                View = false,
                Execute = false,
                Contribute = false,
                DeployTo = true,
                DeployFrom = true,
                Administrator = false,
                ResourceID = resourceID,
                ResourceName = ResourceName
            };
            var picker = new Mock<IDirectoryObjectPickerDialog>();
            picker.Setup(p => p.ShowDialog(It.IsAny<System.Windows.Forms.IWin32Window>())).Returns(DialogResult.Cancel);
            picker.Setup(p => p.SelectedObjects).Returns(new[] { (DirectoryObject)null });

            var viewModel = new SecurityViewModel(new[] { permission }, new Mock<IResourcePickerDialog>().Object, new Mock<IDirectoryObjectPickerDialog>().Object, new Mock<System.Windows.Forms.IWin32Window>().Object, new Mock<IEnvironmentModel>().Object);

            //------------Execute Test---------------------------
            viewModel.PickWindowsGroupCommand.Execute(permission);

            //------------Assert Results-------------------------
            Assert.AreEqual("Deploy Admins", viewModel.ResourcePermissions[0].WindowsGroup);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SecurityViewModel_PickWindowsGroupCommand")]
        public void SecurityViewModel_PickWindowsGroupCommand_DialogResultIsOKAndNothingSelected_DoesNothing()
        {
            //------------Setup for test--------------------------
            var resourceID = Guid.NewGuid();
            const string ResourceName = "Cat\\Resource";
            var permission = new WindowsGroupPermission
            {
                IsServer = false,
                WindowsGroup = "Deploy Admins",
                View = false,
                Execute = false,
                Contribute = false,
                DeployTo = true,
                DeployFrom = true,
                Administrator = false,
                ResourceID = resourceID,
                ResourceName = ResourceName
            };
            var picker = new Mock<IDirectoryObjectPickerDialog>();
            picker.Setup(p => p.ShowDialog(It.IsAny<IWin32Window>())).Returns(DialogResult.OK);
            picker.Setup(p => p.SelectedObjects).Returns(new[] { (DirectoryObject)null });

            var viewModel = new SecurityViewModel(new[] { permission }, new Mock<IResourcePickerDialog>().Object, new Mock<IDirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IEnvironmentModel>().Object);

            //------------Execute Test---------------------------
            viewModel.PickWindowsGroupCommand.Execute(permission);

            //------------Assert Results-------------------------
            Assert.AreEqual("Deploy Admins", viewModel.ResourcePermissions[0].WindowsGroup);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SecurityViewModel_PickWindowsGroupCommand")]
        public void SecurityViewModel_PickWindowsGroupCommand_PermissionIsNull_DoesNothing()
        {
            //------------Setup for test--------------------------
            var resourceID = Guid.NewGuid();
            const string ResourceName = "Cat\\Resource";
            var permission = new WindowsGroupPermission
            {
                IsServer = false,
                WindowsGroup = "Deploy Admins",
                View = false,
                Execute = false,
                Contribute = false,
                DeployTo = true,
                DeployFrom = true,
                Administrator = false,
                ResourceID = resourceID,
                ResourceName = ResourceName
            };
            var viewModel = new SecurityViewModel(new[] { permission }, new Mock<IResourcePickerDialog>().Object, new Mock<IDirectoryObjectPickerDialog>().Object, new Mock<System.Windows.Forms.IWin32Window>().Object, new Mock<IEnvironmentModel>().Object);

            //------------Execute Test---------------------------
            viewModel.PickWindowsGroupCommand.Execute(null);

            //------------Assert Results-------------------------
            Assert.AreEqual("Deploy Admins", viewModel.ResourcePermissions[0].WindowsGroup);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SecurityViewModel_PickWindowsGroupCommand")]
        public void SecurityViewModel_PickWindowsGroupCommand_ResultIsNull_PermissionWindowsGroupIsNotUpdated()
        {
            //------------Setup for test--------------------------
            var resourceID = Guid.NewGuid();
            const string ResourceName = "Cat\\Resource";
            var permission = new WindowsGroupPermission
            {
                IsServer = false,
                WindowsGroup = "Deploy Admins",
                View = false,
                Execute = false,
                Contribute = false,
                DeployTo = true,
                DeployFrom = true,
                Administrator = false,
                ResourceID = resourceID,
                ResourceName = ResourceName
            };
            var picker = new Mock<IDirectoryObjectPickerDialog>();
            picker.Setup(p => p.ShowDialog(It.IsAny<System.Windows.Forms.IWin32Window>())).Returns(DialogResult.OK);
            picker.Setup(p => p.SelectedObjects).Returns(new[] { (DirectoryObject)null });

            var viewModel = new SecurityViewModel(new[] { permission }, new Mock<IResourcePickerDialog>().Object, picker.Object, new Mock<System.Windows.Forms.IWin32Window>().Object, new Mock<IEnvironmentModel>().Object);

            //------------Execute Test---------------------------
            viewModel.PickWindowsGroupCommand.Execute(viewModel.ResourcePermissions[0]);

            //------------Assert Results-------------------------
            Assert.AreEqual("Deploy Admins", viewModel.ResourcePermissions[0].WindowsGroup);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SecurityViewModel_PickWindowsGroupCommand")]
        public void SecurityViewModel_PickWindowsGroupCommand_ResultIsNotNull_PermissionWindowsGroupIsUpdated()
        {
            //------------Setup for test--------------------------
            var resourceID = Guid.NewGuid();
            const string ResourceName = "Cat\\Resource";
            var permission = new WindowsGroupPermission
            {
                IsServer = false,
                WindowsGroup = "Deploy Admins",
                View = false,
                Execute = false,
                Contribute = false,
                DeployTo = true,
                DeployFrom = true,
                Administrator = false,
                ResourceID = resourceID,
                ResourceName = ResourceName
            };

            var directoryObj = new DirectoryObject("Administrators", "WinNT://dev2/MyPC/Administrators", "Group", "");

            var picker = new Mock<IDirectoryObjectPickerDialog>();
            picker.Setup(p => p.ShowDialog(It.IsAny<System.Windows.Forms.IWin32Window>())).Returns(DialogResult.OK);
            picker.Setup(p => p.SelectedObjects).Returns(new[] { directoryObj });

            var viewModel = new SecurityViewModel(new[] { permission }, new Mock<IResourcePickerDialog>().Object, picker.Object, new Mock<System.Windows.Forms.IWin32Window>().Object, new Mock<IEnvironmentModel>().Object);

            //------------Execute Test---------------------------
            viewModel.PickWindowsGroupCommand.Execute(viewModel.ResourcePermissions[0]);

            //------------Assert Results-------------------------
            Assert.AreEqual(directoryObj.Name, viewModel.ResourcePermissions[0].WindowsGroup);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SecurityViewModel_PickWindowsGroupCommand")]
        public void SecurityViewModel_PickWindowsGroupCommand_ResultIsEmptyArray_DoesNothing()
        {
            //------------Setup for test--------------------------
            var resourceID = Guid.NewGuid();
            const string ResourceName = "Cat\\Resource";
            var permission = new WindowsGroupPermission
            {
                IsServer = false,
                WindowsGroup = "Deploy Admins",
                View = false,
                Execute = false,
                Contribute = false,
                DeployTo = true,
                DeployFrom = true,
                Administrator = false,
                ResourceID = resourceID,
                ResourceName = ResourceName
            };
            var picker = new Mock<IDirectoryObjectPickerDialog>();
            picker.Setup(p => p.ShowDialog(It.IsAny<System.Windows.Forms.IWin32Window>())).Returns(DialogResult.Cancel);
            picker.Setup(p => p.SelectedObjects).Returns(new DirectoryObject[0]);

            var viewModel = new SecurityViewModel(new[] { permission }, new Mock<IResourcePickerDialog>().Object, new Mock<IDirectoryObjectPickerDialog>().Object, new Mock<System.Windows.Forms.IWin32Window>().Object, new Mock<IEnvironmentModel>().Object);

            //------------Execute Test---------------------------
            viewModel.PickWindowsGroupCommand.Execute(permission);

            //------------Assert Results-------------------------
            Assert.AreEqual("Deploy Admins", viewModel.ResourcePermissions[0].WindowsGroup);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SecurityViewModel_PickResourceCommand")]
        public void SecurityViewModel_PickResourceCommand_DialogResultIsNotOK_DoesNothing()
        {
            //------------Setup for test--------------------------
            var resourceID = Guid.NewGuid();
            const string ResourceName = "Cat\\Resource";
            var permission = new WindowsGroupPermission
            {
                IsServer = false,
                WindowsGroup = "Deploy Admins",
                View = false,
                Execute = false,
                Contribute = false,
                DeployTo = true,
                DeployFrom = true,
                Administrator = false,
                ResourceID = resourceID,
                ResourceName = ResourceName
            };

            var picker = new Mock<IResourcePickerDialog>();
            picker.Setup(p => p.ShowDialog()).Returns(false);

            var viewModel = new SecurityViewModel(new[] { permission }, picker.Object, new Mock<IDirectoryObjectPickerDialog>().Object, new Mock<System.Windows.Forms.IWin32Window>().Object, new Mock<IEnvironmentModel>().Object);

            //------------Execute Test---------------------------
            viewModel.PickResourceCommand.Execute(viewModel.ResourcePermissions[0]);

            //------------Assert Results-------------------------
            Assert.AreEqual(resourceID, viewModel.ResourcePermissions[0].ResourceID);
            Assert.AreEqual(ResourceName, viewModel.ResourcePermissions[0].ResourceName);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SecurityViewModel_PickResourceCommand")]
        public void SecurityViewModel_PickResourceCommand_PermissionIsNull_DoesNothing()
        {
            //------------Setup for test--------------------------
            var resourceID = Guid.NewGuid();
            const string ResourceName = "Cat\\Resource";
            var permission = new WindowsGroupPermission
            {
                IsServer = false,
                WindowsGroup = "Deploy Admins",
                View = false,
                Execute = false,
                Contribute = false,
                DeployTo = true,
                DeployFrom = true,
                Administrator = false,
                ResourceID = resourceID,
                ResourceName = ResourceName
            };

            var picker = new Mock<IResourcePickerDialog>();

            var viewModel = new SecurityViewModel(new[] { permission }, picker.Object, new Mock<IDirectoryObjectPickerDialog>().Object, new Mock<System.Windows.Forms.IWin32Window>().Object, new Mock<IEnvironmentModel>().Object);

            //------------Execute Test---------------------------
            viewModel.PickResourceCommand.Execute(null);

            //------------Assert Results-------------------------
            Assert.AreEqual(resourceID, viewModel.ResourcePermissions[0].ResourceID);
            Assert.AreEqual(ResourceName, viewModel.ResourcePermissions[0].ResourceName);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SecurityViewModel_PickResourceCommand")]
        public void SecurityViewModel_PickResourceCommand_ResultIsNull_PermissionResourceIsNotUpdated()
        {
            //------------Setup for test--------------------------
            var resourceID = Guid.NewGuid();
            const string ResourceName = "Cat\\Resource";
            var permission = new WindowsGroupPermission
            {
                IsServer = false,
                WindowsGroup = "Deploy Admins",
                View = false,
                Execute = false,
                Contribute = false,
                DeployTo = true,
                DeployFrom = true,
                Administrator = false,
                ResourceID = resourceID,
                ResourceName = ResourceName
            };

            var picker = new Mock<IResourcePickerDialog>();
            picker.Setup(p => p.ShowDialog()).Returns(true);
            picker.Setup(p => p.SelectedResource).Returns((IResourceModel)null);

            var viewModel = new SecurityViewModel(new[] { permission }, picker.Object, new Mock<IDirectoryObjectPickerDialog>().Object, new Mock<System.Windows.Forms.IWin32Window>().Object, new Mock<IEnvironmentModel>().Object);

            //------------Execute Test---------------------------
            viewModel.PickResourceCommand.Execute(viewModel.ResourcePermissions[0]);

            //------------Assert Results-------------------------
            Assert.AreEqual(resourceID, viewModel.ResourcePermissions[0].ResourceID);
            Assert.AreEqual(ResourceName, viewModel.ResourcePermissions[0].ResourceName);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SecurityViewModel_PickResourceCommand")]
        public void SecurityViewModel_PickResourceCommand_ResultIsNotNull_PermissionResourceIsUpdated()
        {
            //------------Setup for test--------------------------
            var resourceID = Guid.NewGuid();
            const string ResourceName = "Cat\\Resource";
            var permission = new WindowsGroupPermission
            {
                IsServer = false,
                WindowsGroup = "Deploy Admins",
                View = false,
                Execute = false,
                Contribute = false,
                DeployTo = true,
                DeployFrom = true,
                Administrator = false,
                ResourceID = resourceID,
                ResourceName = ResourceName
            };
            const ResourceType ResourceType = ResourceType.WorkflowService;
            var newResourceID = Guid.NewGuid();
            var resourceModel = new Mock<IResourceModel>();
            resourceModel.Setup(r => r.ID).Returns(newResourceID);
            resourceModel.Setup(r => r.ResourceType).Returns(ResourceType);
            resourceModel.Setup(r => r.ResourceName).Returns("Resource2");
            resourceModel.Setup(r => r.Category).Returns("Category2");

            var picker = new Mock<IResourcePickerDialog>();
            picker.Setup(p => p.ShowDialog()).Returns(true);
            picker.Setup(p => p.SelectedResource).Returns(resourceModel.Object);

            var viewModel = new SecurityViewModel(new[] { permission }, picker.Object, new Mock<IDirectoryObjectPickerDialog>().Object, new Mock<System.Windows.Forms.IWin32Window>().Object, new Mock<IEnvironmentModel>().Object);

            //------------Execute Test---------------------------
            viewModel.PickResourceCommand.Execute(viewModel.ResourcePermissions[0]);

            //------------Assert Results-------------------------
            Assert.AreEqual(newResourceID, viewModel.ResourcePermissions[0].ResourceID);
            Assert.AreEqual(ResourceType.GetTreeDescription() + "\\Category2\\Resource2", viewModel.ResourcePermissions[0].ResourceName);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SecurityViewModel_PickResourceCommand")]
        public void SecurityViewModel_PickResourceCommand_PermissionHasResource_PickerShouldHavePermissionResourceAsSelectedResource()
        {
            //------------Setup for test--------------------------
            const ResourceType ResourceType = ResourceType.WorkflowService;
            const string ResourceName = "Resource2";
            var resourceID = Guid.NewGuid();
            var resourceModel = new Mock<IResourceModel>();
            resourceModel.Setup(r => r.ID).Returns(resourceID);
            resourceModel.Setup(r => r.ResourceType).Returns(ResourceType);
            resourceModel.Setup(r => r.ResourceName).Returns(ResourceName);
            resourceModel.Setup(r => r.Category).Returns("Category2");

            var permission = new WindowsGroupPermission
            {
                IsServer = false,
                WindowsGroup = "Deploy Admins",
                View = false,
                Execute = false,
                Contribute = false,
                DeployTo = true,
                DeployFrom = true,
                Administrator = false,
                ResourceID = resourceID,
                ResourceName = ResourceName
            };
            
            var picker = new Mock<IResourcePickerDialog>();
            picker.Setup(p => p.ShowDialog()).Returns(false);
            picker.SetupProperty(p => p.SelectedResource);

            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            var mockResourceRepository = new Mock<IResourceRepository>();
            mockResourceRepository.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>())).Returns(resourceModel.Object);
            mockEnvironmentModel.Setup(model => model.ResourceRepository).Returns(mockResourceRepository.Object);
            var viewModel = new SecurityViewModel(new[] { permission }, picker.Object, new Mock<IDirectoryObjectPickerDialog>().Object, new Mock<System.Windows.Forms.IWin32Window>().Object, mockEnvironmentModel.Object);
            //------------Execute Test---------------------------
            viewModel.PickResourceCommand.Execute(viewModel.ResourcePermissions[0]);
            //------------Assert Results-------------------------
            Assert.AreEqual(resourceModel.Object,picker.Object.SelectedResource);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SecurityViewModel_HelpText")]
        public void SecurityViewModel_HelpText_IsServerHelpVisibleIsTrue_ContainsServerHelpText()
        {
            //------------Setup for test--------------------------          
            var viewModel = new SecurityViewModel(new WindowsGroupPermission[0], new Mock<IResourcePickerDialog>().Object, new Mock<IDirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IEnvironmentModel>().Object);

            //------------Execute Test---------------------------
            viewModel.IsServerHelpVisible = true;

            //------------Assert Results-------------------------
            Assert.AreEqual(HelpTextResources.SettingsSecurityServerHelpDefault, viewModel.HelpText);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SecurityViewModel_HelpText")]
        public void SecurityViewModel_HelpText_IsResourceHelpVisibleIsTrue_ContainsResourceHelpText()
        {
            //------------Setup for test--------------------------          
            var viewModel = new SecurityViewModel(new WindowsGroupPermission[0], new Mock<IResourcePickerDialog>().Object, new Mock<IDirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IEnvironmentModel>().Object);

            //------------Execute Test---------------------------
            viewModel.IsResourceHelpVisible = true;

            //------------Assert Results-------------------------
            Assert.AreEqual(HelpTextResources.SettingsSecurityResourceHelpDefault, viewModel.HelpText);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SecurityViewModel_IsServerHelpVisible")]
        public void SecurityViewModel_IsServerHelpVisible_ChangedToTrueAndIsResourceHelpVisibleIsTrue_IsResourceHelpVisibleIsFalse()
        {
            //------------Setup for test--------------------------          
            var viewModel = new SecurityViewModel(new WindowsGroupPermission[0], new Mock<IResourcePickerDialog>().Object, new Mock<IDirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IEnvironmentModel>().Object);
            viewModel.IsResourceHelpVisible = true;

            //------------Execute Test---------------------------
            viewModel.IsServerHelpVisible = true;

            //------------Assert Results-------------------------
            Assert.IsTrue(viewModel.IsServerHelpVisible);
            Assert.IsFalse(viewModel.IsResourceHelpVisible);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SecurityViewModel_IsResourceHelpVisible")]
        public void SecurityViewModel_IsResourceHelpVisible_ChangedToTrueAndIsServerHelpVisibleIsTrue_IsServerHelpVisibleIsFalse()
        {
            //------------Setup for test--------------------------          
            var viewModel = new SecurityViewModel(new WindowsGroupPermission[0], new Mock<IResourcePickerDialog>().Object, new Mock<IDirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IEnvironmentModel>().Object);
            viewModel.IsServerHelpVisible = true;

            //------------Execute Test---------------------------
            viewModel.IsResourceHelpVisible = true;

            //------------Assert Results-------------------------
            Assert.IsTrue(viewModel.IsResourceHelpVisible);
            Assert.IsFalse(viewModel.IsServerHelpVisible);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SecurityViewModel_CloseHelpCommand")]
        public void SecurityViewModel_CloseHelpCommand_IsServerHelpVisibleIsTrue_IsServerHelpVisibleIsFalse()
        {
            //------------Setup for test--------------------------          
            var viewModel = new SecurityViewModel(new WindowsGroupPermission[0], new Mock<IResourcePickerDialog>().Object, new Mock<IDirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IEnvironmentModel>().Object);
            viewModel.IsServerHelpVisible = true;

            //------------Execute Test---------------------------
            viewModel.CloseHelpCommand.Execute(null);

            //------------Assert Results-------------------------
            Assert.IsFalse(viewModel.IsResourceHelpVisible);
            Assert.IsFalse(viewModel.IsServerHelpVisible);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SecurityViewModel_CloseHelpCommand")]
        public void SecurityViewModel_CloseHelpCommand_IsResourceHelpVisibleIsTrue_IsResourceHelpVisibleIsFalse()
        {
            //------------Setup for test--------------------------          
            var viewModel = new SecurityViewModel(new WindowsGroupPermission[0], new Mock<IResourcePickerDialog>().Object, new Mock<IDirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IEnvironmentModel>().Object);
            viewModel.IsResourceHelpVisible = true;

            //------------Execute Test---------------------------
            viewModel.CloseHelpCommand.Execute(null);

            //------------Assert Results-------------------------
            Assert.IsFalse(viewModel.IsResourceHelpVisible);
            Assert.IsFalse(viewModel.IsServerHelpVisible);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SecurityViewModel_Save")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SecurityViewModel_Save_NullPermissions_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------          
            var viewModel = new SecurityViewModel(new WindowsGroupPermission[0], new Mock<IResourcePickerDialog>().Object, new Mock<IDirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IEnvironmentModel>().Object);

            //------------Execute Test---------------------------
            viewModel.Save(null, null);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SecurityViewModel_Save")]
        public void SecurityViewModel_Save_InvalidPermissions_InvalidPermissionsAreRemoved()
        {
            //------------Setup for test--------------------------          
            var permissions = CreatePermissions();

            var invalidPermission = permissions[permissions.Count - 1];
            invalidPermission.WindowsGroup = "";

            var viewModel = new SecurityViewModel(permissions, new Mock<IResourcePickerDialog>().Object, new Mock<IDirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IEnvironmentModel>().Object);

            var target = new List<WindowsGroupPermission>();

            var expectedCount = permissions.Count - 1;
            var expectedResourceCount = viewModel.ResourcePermissions.Count - 1;

            //------------Execute Test---------------------------
            viewModel.Save(target, new List<string>());

            //------------Assert Results-------------------------
            Assert.AreEqual(expectedCount, target.Count);
            Assert.AreEqual(expectedResourceCount, viewModel.ResourcePermissions.Count);
            foreach(var permission in target)
            {
                Assert.IsTrue(permission.IsValid);
                Assert.IsFalse(permission.IsNew);
            }
            Assert.AreEqual(1, viewModel.ResourcePermissions.Count(p => p.IsNew));
            Assert.AreEqual(1, viewModel.ServerPermissions.Count(p => p.IsNew));
        }

        static List<WindowsGroupPermission> CreatePermissions()
        {
            return new List<WindowsGroupPermission>(new[]
            {
                new WindowsGroupPermission
                {
                    IsServer = true, WindowsGroup = "BuiltIn\\Administrators",
                    View = false, Execute = false, Contribute = true, DeployTo = true, DeployFrom = true, Administrator = true
                },
                new WindowsGroupPermission
                {
                    IsServer = true, WindowsGroup = "Deploy Admins",
                    View = false, Execute = false, Contribute = false, DeployTo = true, DeployFrom = true, Administrator = false
                },

                new WindowsGroupPermission
                {
                    ResourceID = Guid.NewGuid(), ResourceName = "Category1\\Workflow1",
                    WindowsGroup = "Windows Group 1", View = false, Execute = true, Contribute = false
                },
                new WindowsGroupPermission
                {
                    ResourceID = Guid.NewGuid(), ResourceName = "Category1\\Workflow1",
                    WindowsGroup = "Windows Group 2", View = false, Execute = false, Contribute = true
                },

                new WindowsGroupPermission
                {
                    ResourceID = Guid.NewGuid(), ResourceName = "Category1\\Workflow2",
                    WindowsGroup = "Windows Group 1", View = true, Execute = true, Contribute = false
                },

                new WindowsGroupPermission
                {
                    ResourceID = Guid.NewGuid(), ResourceName = "Category2\\Workflow3",
                    WindowsGroup = "Windows Group 3", View = true, Execute = false, Contribute = false
                },
                new WindowsGroupPermission
                {
                    ResourceID = Guid.NewGuid(), ResourceName = "Category2\\Workflow3",
                    WindowsGroup = "Windows Group 4", View = false, Execute = true, Contribute = false
                },


                new WindowsGroupPermission
                {
                    ResourceID = Guid.NewGuid(), ResourceName = "Category2\\Workflow4",
                    WindowsGroup = "Windows Group 3", View = false, Execute = false, Contribute = true
                },
                new WindowsGroupPermission
                {
                    ResourceID = Guid.NewGuid(), ResourceName = "Category1\\Workflow1",
                    WindowsGroup = "Windows Group 4", View = false, Execute = false, Contribute = true
                }
            });
        }
    }
}
