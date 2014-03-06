using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Data;
using System.Diagnostics;
using System.Windows.Media;
using Infragistics.AutomationPeers;

namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// Custom element that represents the portion of a <see cref="CalendarGroupPresenterBase.CalendarGroup"/> that contains the timeslots for the elements.
	/// </summary>
	public class CalendarGroupTimeslotArea : CalendarGroupItemsPresenterBase
	{
		#region Constructor
		static CalendarGroupTimeslotArea()
		{

			CalendarGroupTimeslotArea.DefaultStyleKeyProperty.OverrideMetadata(typeof(CalendarGroupTimeslotArea), new FrameworkPropertyMetadata(typeof(CalendarGroupTimeslotArea)));

		}

		/// <summary>
		/// Initializes a new <see cref="CalendarGroupTimeslotArea"/>
		/// </summary>
		public CalendarGroupTimeslotArea()
		{



		}
		#endregion //Constructor

		#region Base class overrides

		#region GetBorderThickness

		internal override Thickness GetBorderThickness(double borderSize)
		{
			return new Thickness(borderSize, 0, borderSize, borderSize);
		}

		#endregion //GetBorderThickness	

		#region OnCreateAutomationPeer
		/// <summary>
		/// Returns an automation peer that exposes the <see cref="CalendarGroupTimeslotArea"/> to UI Automation.
		/// </summary>
		/// <returns>A <see cref="CalendarGroupTimeslotAreaAutomationPeer"/></returns>
		protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
		{
			return new CalendarGroupTimeslotAreaAutomationPeer(this);
		}
		#endregion // OnCreateAutomationPeer

		#region OnItemsPanelChanged
		internal override void OnItemsPanelChanged(ScheduleItemsPanel oldPanel, ScheduleItemsPanel newPanel)
		{
			base.OnItemsPanelChanged(oldPanel, newPanel);

			if (oldPanel != null)
			{
				oldPanel.ClearValue(ScheduleItemsPanel.OrientationProperty);
			}

			if (newPanel != null)
			{
				var ctrl = ScheduleControlBase.GetControl(this);

				if (null != ctrl)
					newPanel.Orientation = ctrl.TimeslotGroupOrientation;
			}

		} 
		#endregion // OnItemsPanelChanged

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