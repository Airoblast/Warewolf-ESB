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
using System.Collections.ObjectModel;

namespace Infragistics.Controls.Grids.Primitives
{
	/// <summary>
	/// A cell that represents the border seperating pinned columns and unpinned columns.
	/// </summary>
	public class FixedBorderCell : CellBase
	{
		#region Static

		static Type _recyclingElementType = typeof(FixedBorderCellControl);

		#endregion // Static

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="FixedBorderCell"/> class.
		/// </summary>
		/// <param propertyName="row">The <see cref="RowBase"/> object that owns the <see cref="FixedBorderCell"/></param>
		/// <param propertyName="column">The <see cref="Column"/> object that the <see cref="FixedBorderCell"/> represents.</param>
		protected internal FixedBorderCell(RowBase row, Column column)
			: base(row, column)
		{
		}

		#endregion // Constructor

		#region Overrides

		#region ResolveStyle

		/// <summary>
		/// Gets the Style that should be applied to the <see cref="FixedBorderCellControl"/> when it's attached.
		/// </summary>
		protected override Style ResolveStyle
		{
			get
			{
				if (this.Style != null)
					return this.Style;

				return this.Row.ColumnLayout.FixedColumnSettings.FixedBorderStyleResolved;
			}
		}

		#endregion // ResolveStyle

		#region RecyclingElementType
		/// <summary>
		/// Gets the Type of control that should be created for the <see cref="FixedBorderCell"/>.
		/// </summary>
		protected override Type RecyclingElementType
		{
			get
			{
				return FixedBorderCell._recyclingElementType;
			}
		}
		#endregion // RecyclingElementType

		#region CreateInstanceOfRecyclingElement

		/// <summary>
		/// Creates a new instance of a <see cref="FixedBorderCellControl"/> for the <see cref="FixedBorderCell"/>.
		/// </summary>
		/// <returns>A new <see cref="FixedBorderCellControl"/></returns>
		/// <remarks>This method should only be used by the <see cref="Infragistics.RecyclingManager"/></remarks>
		protected override CellControlBase CreateInstanceOfRecyclingElement()
		{		
			return new FixedBorderCellControl();
		}

		#endregion // CreateInstanceOfRecyclingElement

		#region EnsureCurrentState

		/// <summary>
		/// Ensures that <see cref="CellBase"/> is in the correct state.
		/// </summary>
		protected internal override void EnsureCurrentState()
		{
			base.EnsureCurrentState();
		}

		#endregion // EnsureCurrentState

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