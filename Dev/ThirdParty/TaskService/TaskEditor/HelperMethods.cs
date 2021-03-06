
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using CubicOrange.Windows.Forms.ActiveDirectory;
using System;
using System.Collections;
using System.Security.Principal;

namespace Microsoft.Win32.TaskScheduler
{
	internal static class HelperMethods
	{
		public static bool SelectAccount(System.Windows.Forms.IWin32Window parent, string targetComputerName, ref string acctName, out bool isGroup, out bool isService, out string sid)
		{
			DirectoryObjectPickerDialog dlg = new DirectoryObjectPickerDialog() { TargetComputer = targetComputerName, MultiSelect = false, SkipDomainControllerCheck = true };
			dlg.AllowedObjectTypes = ObjectTypes.Users; // | ObjectTypes.WellKnownPrincipals | ObjectTypes.Computers;
			if (NativeMethods.AccountUtils.CurrentUserIsAdmin(targetComputerName)) dlg.AllowedObjectTypes |= ObjectTypes.BuiltInGroups | ObjectTypes.Groups;
			dlg.AttributesToFetch.Add("objectSid");
			if (dlg.ShowDialog(parent) == System.Windows.Forms.DialogResult.OK)
			{
				if (dlg.SelectedObject != null)
				{
					try
					{
						if (!String.IsNullOrEmpty(dlg.SelectedObject.Upn))
							acctName = NameTranslator.TranslateUpnToDownLevel(dlg.SelectedObject.Upn);
						else
							acctName = dlg.SelectedObject.Name;
					}
					catch
					{
						acctName = dlg.SelectedObject.Name;
					}
					sid = AttrToString(dlg.SelectedObject.FetchedAttributes[0]);
					isGroup = dlg.SelectedObject.SchemaClassName.Equals("Group", StringComparison.OrdinalIgnoreCase);
					isService = NativeMethods.AccountUtils.UserIsServiceAccount(acctName);
					return true;
				}
			}
			isGroup = isService = false;
			sid = null;
			return false;
		}

		private static string AttrToString(object attr)
		{
			object multivaluedAttribute = attr;
			if (!(multivaluedAttribute is IEnumerable) || multivaluedAttribute is byte[] || multivaluedAttribute is string)
				multivaluedAttribute = new Object[1] { multivaluedAttribute };

			System.Collections.Generic.List<string> list = new System.Collections.Generic.List<string>();

			foreach (object attribute in (IEnumerable)multivaluedAttribute)
			{
				if (attribute == null)
				{
					list.Add(string.Empty);
				}
				else if (attribute is byte[])
				{
					byte[] bytes = (byte[])attribute;
					list.Add(BytesToString(bytes));
				}
				else
				{
					list.Add(attribute.ToString());
				}
			}

			return string.Join("|", list.ToArray());
		}

		private static string BytesToString(byte[] bytes)
		{
			try { return new Guid(bytes).ToString("D"); }
			catch { }

			try { return new SecurityIdentifier(bytes, 0).ToString(); }
			catch { }

			return "0x" + BitConverter.ToString(bytes).Replace('-', ' ');
		}
	}
}
