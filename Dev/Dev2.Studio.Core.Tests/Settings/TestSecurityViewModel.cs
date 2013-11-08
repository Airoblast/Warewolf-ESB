﻿using System.Collections.Generic;
using System.Windows.Forms;
using CubicOrange.Windows.Forms.ActiveDirectory;
using Dev2.Data.Settings.Security;
using Dev2.Dialogs;
using Dev2.Settings.Security;
using Dev2.Studio.Core.Interfaces;
using Moq;

namespace Dev2.Core.Tests.Settings
{
    public class TestSecurityViewModel : SecurityViewModel
    {
        public TestSecurityViewModel()
            : base(new List<WindowsGroupPermission>(), new Mock<IResourcePickerDialog>().Object, new Mock<IDirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IEnvironmentModel>().Object)
        {
        }

        public int SaveHitCount { get; private set; }
        public override void Save(List<WindowsGroupPermission> permissions)
        {
            SaveHitCount++;
            base.Save(permissions);
        }
    }
}