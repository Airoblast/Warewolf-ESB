using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Infragistics.Controls.Schedules.Primitives
{
	internal abstract class DayPresenterBaseAdapter : TimeslotBase
	{
		#region Member Variables

		private DateTime _logicalDate;
		private bool _isSelected;

		#endregion // Member Variables

		#region Constructor
		internal DayPresenterBaseAdapter(DateTime logicalDate, DateTime start, DateTime end)
			: base(start, end)
		{
			_logicalDate = logicalDate;
		}
		#endregion // Constructor

		#region Base class overrides

		#region OnElementAttached
		protected override void OnElementAttached(TimeRangePresenterBase element)
		{
			var dayElement = element as DayPresenterBase;

			if (null != dayElement)
			{
				dayElement.IsToday = _isToday;
				dayElement.IsSelected = this.IsSelected;
			}

			base.OnElementAttached(element);
		}
		#endregion // OnElementAttached

		#region RecyclingElementType

		/// <summary>
		/// Gets the Type of control that should be created for the object.
		/// </summary>
		protected override abstract Type RecyclingElementType { get; }
		#endregion // RecyclingElementType

		#endregion // Base class overrides

		#region Properties

		#region IsSelected
		/// <summary>
		/// Returns a boolean indicating if the time slot is selected.
		/// </summary>
		public bool IsSelected
		{
			get { return _isSelected; }
			internal set
			{
				if (value != _isSelected)
				{
					_isSelected = value;

					var element = this.AttachedElement as DayPresenterBase;

					if (null != element)
						element.IsSelected = value;

					this.OnPropertyChanged("IsSelected");
				}
			}
		}
		#endregion //IsSelected

		#region IsToday

		private bool _isToday;

		internal bool IsToday
		{
			get { return _isToday; }
			set
			{
				if (value != _isToday)
				{
					_isToday = value;

					var element = this.AttachedElement as DayPresenterBase;

					if (null != element)
						element.IsToday = value;
				}
			}
		}
		#endregion // IsToday

		#region LogicalDate
		internal DateTime LogicalDate
		{
			get { return _logicalDate; }
		}
		#endregion // LogicalDate

		#endregion // Properties
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