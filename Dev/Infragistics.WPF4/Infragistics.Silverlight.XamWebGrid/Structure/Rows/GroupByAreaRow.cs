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

namespace Infragistics.Controls.Grids.Primitives
{
	/// <summary>
	/// An object that represents a row that represents data of a <see cref="Column"/> that the underlying data has been grouped by.
	/// </summary>
	public class GroupByAreaRow : RowBase
	{
		#region Members

		SingleCellBaseCollection<GroupByAreaCell> _cells;

		#endregion // Members

		#region Constructor

		/// <summary>
		/// Creates a new instance of the <see cref="GroupByAreaRow"/> object.
		/// </summary>
		/// <param propertyName="manager"></param>
		public GroupByAreaRow(RowsManagerBase manager)
			: base(manager)
		{
			this.FixedPositionSortOrder = -1;
			this.MinimumRowHeight = 0;
		}

		#endregion // Constructor

		#region Overrides

		#region RowType

		/// <summary>
		/// Gets the <see cref="RowType"/> of this <see cref="GroupByAreaRow"/>
		/// </summary>
		public override RowType RowType
		{
			get 
			{
				return RowType.GroupByAreaRow;
			}
		}
		#endregion // RowType

		#region Cells

		/// <summary>
		/// Gets the <see cref="CellBaseCollection"/> that belongs to the <see cref="ChildBand"/>.
		/// </summary>
		public override CellBaseCollection Cells
		{
			get
			{
				if (this._cells == null)
					this._cells = new SingleCellBaseCollection<GroupByAreaCell>(((XamGridRowsManager)this.Manager).GroupByAreaColumn, this.Columns, this);
				return this._cells;
			}
		}

		#endregion // Cells

		#region RecyclingElementType

		/// <summary>
		/// Gets the Type of control that should be created for the <see cref="GroupByAreaRow"/>.
		/// </summary>
		protected override Type RecyclingElementType
		{
			get
			{
				return typeof(GroupByAreaRowCellsPanel);
			}
		}
		#endregion // RecyclingElementType

		#region CreateInstanceOfRecyclingElement

		/// <summary>
		/// Creates a new instance of the <see cref="GroupByAreaRowCellsPanel"/>. 
		/// Note: this method is only meant to be invoked via the RecyclingManager.
		/// </summary>
		/// <returns>A new <see cref="GroupByAreaRowCellsPanel"/></returns>
		protected override CellsPanel CreateInstanceOfRecyclingElement()
		{
			return new GroupByAreaRowCellsPanel();
		}
		#endregion // CreateInstanceOfRecyclingElement

		#region HeightResolved

		/// <summary>
		/// Resolves the <see cref="RowBase.Height"/> property for this Row.
		/// </summary>
		public override RowHeight HeightResolved
		{
			get
			{
				if (this.Height != null)
					return (RowHeight)this.Height;
				else
					return this.ColumnLayout.GroupBySettings.GroupByAreaRowHeightResolved;
			}
		}
		#endregion // HeightResolved

        #region CanScrollHorizontally
        /// <summary>
        /// Gets whether or not a row will ever need to scroll horizontally. 
        /// </summary>
        protected internal override bool CanScrollHorizontally
        {
            get { return false; }
        }

        #endregion // CanScrollHorizontally

		#endregion // Overrides
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