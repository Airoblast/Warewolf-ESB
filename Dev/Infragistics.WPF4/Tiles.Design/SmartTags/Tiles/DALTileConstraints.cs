using System.Text;
using System.Windows;
using System.Windows.Controls;
using Infragistics.Windows.Design.SmartTagFramework;
using Microsoft.Windows.Design;
using Microsoft.Windows.Design.Interaction;
using Microsoft.Windows.Design.Model;
using Microsoft.Windows.Design.Services;
using Infragistics.Shared;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Infragistics.Windows.Tiles;
using Infragistics.Collections;
using System;

namespace Infragistics.Windows.Design.Tiles
{
    /// <summary>
    /// A predefined DesignerActionList class. Here you can specify the smart tag items 
    /// and/or methods which will be executed from the smart tag.
    /// </summary>
	public class DALTileConstraints : DesignerActionList
    {
        #region Constructors

		/// <summary>
		/// Creates an instance of a DesignerActionList
		/// </summary>
		/// <param name="context"></param>
		/// <param name="modelItem"></param>
		/// <param name="itemsToShow"></param>
		/// <param name="alternateAdornerTitle"></param>
		public DALTileConstraints(EditingContext context, ModelItem modelItem, List<string> itemsToShow, string alternateAdornerTitle)
			: base(context, modelItem, itemsToShow, alternateAdornerTitle)
        {
			//You can set the title of the smart tag. By default it is "Tasks".
			this.GenericAdornerTitle = string.IsNullOrEmpty(alternateAdornerTitle) ? SR.GetString("SmartTag_T_TileConstraints") : alternateAdornerTitle;

			//You can specify the MaxHeight of the smart tag. The smart tag has a scrolling capability.
			//this.GenericAdornerMaxHeight = 300;

			int groupSequence = 0;

			#region Sizes Group

			DesignerActionItemGroup groupSizes	= new DesignerActionItemGroup(SR.GetString("SmartTag_G_Sizes"), groupSequence++);
			groupSizes.IsExpanded				= true;
			using (PropertyItemCreator pic = new PropertyItemCreator(typeof(TileConstraints), groupSizes))
			{
				this.Items.Add(pic.GetPropertyActionItem("MaxHeight"));
				this.Items.Add(pic.GetPropertyActionItem("MaxWidth"));
				this.Items.Add(pic.GetPropertyActionItem("MinHeight"));
				this.Items.Add(pic.GetPropertyActionItem("MinWidth"));
				this.Items.Add(pic.GetPropertyActionItem("PreferredHeight"));
				this.Items.Add(pic.GetPropertyActionItem("PreferredWidth"));
			}

			#endregion //Sizes Group

			#region MarginAndAlignments Group

			DesignerActionItemGroup groupMarginAndAlignments	= new DesignerActionItemGroup(SR.GetString("SmartTag_G_MarginAndAlignments"), groupSequence++);
			groupMarginAndAlignments.IsExpanded					= true;
			using (PropertyItemCreator pic = new PropertyItemCreator(typeof(TileConstraints), groupMarginAndAlignments))
			{
				this.Items.Add(pic.GetPropertyActionItem("HorizontalAlignment"));
				this.Items.Add(pic.GetPropertyActionItem("Margin"));
				this.Items.Add(pic.GetPropertyActionItem("VerticalAlignment"));
			}

			#endregion //MarginAndAlignments Group
		}

        #endregion //Constructors

		#region DesignerActionMethodItem Callbacks

		#endregion //DesignerActionMethodItem Callbacks
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