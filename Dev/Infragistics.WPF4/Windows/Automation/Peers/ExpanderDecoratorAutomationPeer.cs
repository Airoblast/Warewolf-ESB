using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Automation.Peers;

namespace Infragistics.Windows.Automation.Peers
{
	// AS 9/1/09 TFS21721
	/// <summary>
	/// Exposes the <see cref="ExpanderDecorator"/> to UI Automation
	/// </summary>
	public class ExpanderDecoratorAutomationPeer : FrameworkElementAutomationPeer
	{
		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="ExpanderDecoratorAutomationPeer"/>
		/// </summary>
		/// <param name="expander">The associated element</param>
		public ExpanderDecoratorAutomationPeer(ExpanderDecorator expander)
			: base(expander)
		{
		}
		#endregion //Constructor

		#region Base class overrides

		#region GetAutomationControlTypeCore
		/// <summary>
		/// Returns an enumeration indicating the type of control represented by the automation peer.
		/// </summary>
		/// <returns>The <b>Group</b> enumeration value</returns>
		protected override AutomationControlType GetAutomationControlTypeCore()
		{
			return AutomationControlType.Group;
		} 
		#endregion //GetAutomationControlTypeCore

		#region GetClassNameCore
		/// <summary>
		/// Returns the name of the <see cref="ExpanderDecorator"/>
		/// </summary>
		/// <returns>A string that contains 'ExpanderDecorator'</returns>
		protected override string GetClassNameCore()
		{
			return "ExpanderDecorator";
		} 
		#endregion //GetClassNameCore

		#region GetChildrenCore
		/// <summary>
		/// Returns the collection of automation peers that represents the children of the tab item
		/// </summary>
		/// <returns>The collection of child elements</returns>
		protected override List<AutomationPeer> GetChildrenCore()
		{
			ExpanderDecorator expander = (ExpanderDecorator)this.Owner;

			if (!expander.IsExpanded)
				return new List<AutomationPeer>();

			return base.GetChildrenCore();
		}
		#endregion //GetChildrenCore

		#region IsControlElementCore
		/// <summary>
		/// Returns a value that indicates whether the <see cref="System.Windows.UIElement"/> that corresponds with the object that is associated with this <see cref="AutomationPeer"/> is understood by the end user as interactive.
		/// </summary>
		/// <returns><b>True</b> if the <see cref="System.Windows.UIElement"/> is a control; otherwise, <b>false</b>.</returns>
		protected override bool IsControlElementCore()
		{
			return false;
		} 
		#endregion //IsControlElementCore

		#endregion //Base class overrides
	}
}

#region Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved
/* ---------------------------------------------------------------------*
*                           Infragistics, Inc.                          *
*              Copyright (c) 2001-2012 All Rights reserved               *
*                                                                       *
*                                                                       *
* This file and its contents are protected by United States and         *
* International copyright laws.  Unauthorized reproduction and/or       *
* distribution of all or any portion of the code contained herein       *
* is strictly prohibited and will result in severe civil and criminal   *
* penalties.  Any violations of this copyright will be prosecuted       *
* to the fullest extent possible under law.                             *
*                                                                       *
* THE SOURCE CODE CONTAINED HEREIN AND IN RELATED FILES IS PROVIDED     *
* TO THE REGISTERED DEVELOPER FOR THE PURPOSES OF EDUCATION AND         *
* TROUBLESHOOTING. UNDER NO CIRCUMSTANCES MAY ANY PORTION OF THE SOURCE *
* CODE BE DISTRIBUTED, DISCLOSED OR OTHERWISE MADE AVAILABLE TO ANY     *
* THIRD PARTY WITHOUT THE EXPRESS WRITTEN CONSENT OF INFRAGISTICS, INC. *
*                                                                       *
* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *
* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *
* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY INFRAGISTICS PRODUCT.    *
*                                                                       *
* THE REGISTERED DEVELOPER ACKNOWLEDGES THAT THIS SOURCE CODE           *
* CONTAINS VALUABLE AND PROPRIETARY TRADE SECRETS OF INFRAGISTICS,      *
* INC.  THE REGISTERED DEVELOPER AGREES TO EXPEND EVERY EFFORT TO       *
* INSURE ITS CONFIDENTIALITY.                                           *
*                                                                       *
* THE END USER LICENSE AGREEMENT (EULA) ACCOMPANYING THE PRODUCT        *
* PERMITS THE REGISTERED DEVELOPER TO REDISTRIBUTE THE PRODUCT IN       *
* EXECUTABLE FORM ONLY IN SUPPORT OF APPLICATIONS WRITTEN USING         *
* THE PRODUCT.  IT DOES NOT PROVIDE ANY RIGHTS REGARDING THE            *
* SOURCE CODE CONTAINED HEREIN.                                         *
*                                                                       *
* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *
* --------------------------------------------------------------------- *
*/
#endregion Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved