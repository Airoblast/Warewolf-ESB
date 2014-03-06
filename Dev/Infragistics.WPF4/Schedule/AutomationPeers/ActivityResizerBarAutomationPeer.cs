using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Automation.Peers;
using Infragistics.Controls.Schedules.Primitives;
using Infragistics.Controls.Schedules;
using System.Windows.Automation.Provider;
using System.Windows.Automation;

namespace Infragistics.AutomationPeers
{
	/// <summary>
	/// Exposes <see cref="ActivityResizerBar"/> types to UI Automation
	/// </summary>
	public class ActivityResizerBarAutomationPeer : FrameworkElementAutomationPeer
		, ITransformProvider
	{
		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="ActivityResizerBarAutomationPeer"/>
		/// </summary>
		/// <param name="owner">The <see cref="ActivityResizerBarAutomationPeer"/> that the peer represents</param>
		public ActivityResizerBarAutomationPeer(ActivityResizerBar owner)
			: base(owner)
		{
		}
		#endregion // Constructor

		#region Base class overrides

		#region GetAutomationControlTypeCore
		/// <summary>
		/// Returns an enumeration indicating the type of control represented by the automation peer.
		/// </summary>
		/// <returns>The <b>Custom</b> enumeration value</returns>
		protected override AutomationControlType GetAutomationControlTypeCore()
		{
			return AutomationControlType.Thumb;
		}

		#endregion //GetAutomationControlTypeCore

		#region GetClassNameCore
		/// <summary>
		/// Returns the name of the <see cref="ActivityPresenter"/>
		/// </summary>
		/// <returns>A string that contains 'ActivityPresenter'</returns>
		protected override string GetClassNameCore()
		{
			return this.Owner.GetType().Name;
		}

		#endregion //GetClassNameCore

		#region GetNameCore
		/// <summary>
		/// Returns the text label of the element that is associated with this peer.
		/// </summary>
		/// <returns>The automation name of the element or the name of the associated calendar.</returns>
		protected override string GetNameCore()
		{
			string name = base.GetNameCore();

			if (string.IsNullOrEmpty(name))
			{
				if ( this.Element.IsLeading )
					name = "Leading resizer";
				else
					name = "Trailing resizer";
			}

			return name;
		}
		#endregion // GetNameCore

		#region GetPattern
		/// <summary>
		/// Returns the control pattern for the element that is associated with this peer.
		/// </summary>
		/// <param name="patternInterface">The pattern being requested</param>
		/// <returns>The pattern provider or null</returns>
		public override object GetPattern(PatternInterface patternInterface)
		{
			if (patternInterface == PatternInterface.Transform)
				return this;

			return base.GetPattern(patternInterface);
		}
		#endregion // GetPattern

		#endregion //Base class overrides

		#region Properties

		#region Element
		internal ActivityResizerBar Element
		{
			get { return (ActivityResizerBar)this.Owner; }
		}
		#endregion // Element

		#endregion // Properties

		#region ITransformProvider Members

		bool ITransformProvider.CanMove
		{
			get 
			{
				ActivityPresenter presenter = this.Element.ActivityPresenter;
				return presenter != null && presenter.CanEdit; 
			}
		}

		bool ITransformProvider.CanResize
		{
			get { return false; }
		}

		bool ITransformProvider.CanRotate
		{
			get { return false; }
		}

		void ITransformProvider.Move(double x, double y)
		{
			if (this.IsEnabled() == false)
				throw new ElementNotEnabledException();

			if (!((ITransformProvider)this).CanMove)
				throw new InvalidOperationException();

			if (double.IsInfinity(x) || double.IsNaN(x))
				throw new ArgumentOutOfRangeException("x");

			if (double.IsInfinity(y) || double.IsNaN(y))
				throw new ArgumentOutOfRangeException("y");

			ActivityPresenter presenter = this.Element.ActivityPresenter;
			ScheduleControlBase control = presenter != null ? presenter.Control : null;

			if (control != null)
				control.EditHelper.BeginResize(presenter.Activity, this.Element, true, x, y);
		}

		void ITransformProvider.Resize(double width, double height)
		{
			if (this.IsEnabled() == false)
				throw new ElementNotEnabledException();

			throw new InvalidOperationException();
		}

		void ITransformProvider.Rotate(double degrees)
		{
			if (this.IsEnabled() == false)
				throw new ElementNotEnabledException();

			throw new InvalidOperationException();
		}

		#endregion
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