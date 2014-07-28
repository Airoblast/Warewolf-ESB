﻿using System.Windows;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.Core.Controller
// ReSharper restore CheckNamespace
{

    public interface IPopupController
    {
        string Header { get; set; }
        string Description { get; set; }
        string Question { get; set; }
        MessageBoxImage ImageType { get; set; }
        MessageBoxButton Buttons { get; set; }
        string DontShowAgainKey { get; set; }
        MessageBoxResult Show();
        MessageBoxResult Show(string description, string header, MessageBoxButton buttons, MessageBoxImage image, string dontShowAgainKey);
        MessageBoxResult ShowNotConnected();
        MessageBoxResult ShowDeleteConfirmation(string nameOfItemBeingDeleted);
        MessageBoxResult ShowNameChangedConflict(string oldName, string newName);
        MessageBoxResult ShowSettingsCloseConfirmation();
        MessageBoxResult ShowSchedulerCloseConfirmation();
        MessageBoxResult ShowNoInputsSelectedWhenClickLink();
        MessageBoxResult ShowSaveErrorDialog(string errorMessage);
    }
}
