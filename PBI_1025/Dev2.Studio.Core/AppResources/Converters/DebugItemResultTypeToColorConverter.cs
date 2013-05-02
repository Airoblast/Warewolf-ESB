﻿using Dev2.Diagnostics;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Dev2.Studio.Core.AppResources.Converters
{
    public class DebugItemResultTypeToColorConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var resultType = (DebugItemResultType)value;
            switch(resultType)
            {
                case DebugItemResultType.Variable:
                    return Application.Current.Resources["DebugItemVariableBrush"];

                case DebugItemResultType.Value:
                    return Application.Current.Resources["DebugItemValueBrush"];

                default: // DebugItemResultType.Label:
                    return Application.Current.Resources["DebugItemLabelBrush"];
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }

        #endregion
    }
}
