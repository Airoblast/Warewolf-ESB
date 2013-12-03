﻿using System.Collections;
using System.ComponentModel;

namespace Dev2.Services.Security
{
    public class WindowsGroupPermissionComparer : IComparer
    {
        readonly int _direction;
        readonly string _sortMemberPath;

        public WindowsGroupPermissionComparer(ListSortDirection direction, string sortMemberPath)
        {
            VerifyArgument.IsNotNull("sortMemberPath", sortMemberPath);
            _direction = direction == ListSortDirection.Ascending ? 1 : -1;
            _sortMemberPath = sortMemberPath;
        }

        public int Compare(object x, object y)
        {
            var px = x as WindowsGroupPermission;
            var py = y as WindowsGroupPermission;

            if(px == null || py == null)
            {
                return 1;
            }

            // New items must be last
            //
            if(px.IsNew)
            {
                // px is greater than py
                return int.MaxValue;
            }
            if(py.IsNew)
            {
                // px is less than py
                return int.MinValue;
            }

            // BuiltInAdministrators must be first
            if(px.IsBuiltInAdministrators)
            {
                // px is less than py
                return int.MinValue;
            }
            if(py.IsBuiltInAdministrators)
            {
                // px is greater than py
                return int.MaxValue;
            }

            var result = Compare(px, py);
            return _direction * result;
        }

        int Compare(WindowsGroupPermission px, WindowsGroupPermission py)
        {
            switch(_sortMemberPath)
            {
                case "ResourceName":
                    return System.String.Compare(px.ResourceName, py.ResourceName, System.StringComparison.InvariantCulture);
                case "WindowsGroup":
                    return System.String.Compare(px.WindowsGroup, py.WindowsGroup, System.StringComparison.InvariantCulture);
                case "View":
                    return px.View.CompareTo(py.View);
                case "Execute":
                    return px.Execute.CompareTo(py.Execute);
                case "Contribute":
                    return px.Contribute.CompareTo(py.Contribute);
                case "DeployTo":
                    return px.DeployTo.CompareTo(py.DeployTo);
                case "DeployFrom":
                    return px.DeployFrom.CompareTo(py.DeployFrom);
                case "Administrator":
                    return px.Administrator.CompareTo(py.Administrator);
            }
            return 0;
        }


    }
}