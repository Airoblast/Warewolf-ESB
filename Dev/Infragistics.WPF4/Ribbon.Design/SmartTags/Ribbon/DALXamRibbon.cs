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
using Infragistics.Windows.Ribbon;
using System.Collections.ObjectModel;

namespace Infragistics.Windows.Design.Ribbon
{
    /// <summary>
    /// A predefined DesignerActionList class. Here you can specify the smart tag items 
    /// and/or methods which will be executed from the smart tag.
    /// </summary>
    public class DALXamRibbon : DesignerActionList
    {
        #region Constructors

		/// <summary>
		/// Creates an instance of a DesignerActionList
		/// </summary>
		/// <param name="context"></param>
		/// <param name="modelItem"></param>
		/// <param name="itemsToShow"></param>
		/// <param name="alternateAdornerTitle"></param>
		public DALXamRibbon(EditingContext context, ModelItem modelItem, List<string> itemsToShow, string alternateAdornerTitle)
			: base(context, modelItem, itemsToShow, alternateAdornerTitle)
        {
			//You can set the title of the smart tag. By default it is "Tasks".
			this.GenericAdornerTitle = string.IsNullOrEmpty(alternateAdornerTitle) ? SR.GetString("SmartTag_T_XamRibbon") : alternateAdornerTitle;

			//You can specify the MaxHeight of the smart tag. The smart tag has a scrolling capability.
			//this.GenericAdornerMaxHeight = 300;

			int groupSequence = 0;

			#region Tasks Group

			DesignerActionItemGroup groupTasks	= new DesignerActionItemGroup(SR.GetString("SmartTag_G_Tasks"), groupSequence++);
			groupTasks.IsExpanded				= true;
			this.Items.Add(new DesignerActionMethodItem(this, "PerformAction_AddTab", SR.GetString("SmartTag_D_AddTab"), SR.GetString("SmartTag_N_AddTab"), groupTasks, groupSequence++));

			#endregion //Tasks Group

			#region Appearance Group

			DesignerActionItemGroup groupAppearance = new DesignerActionItemGroup(SR.GetString("SmartTag_G_Appearance"), groupSequence++);
			groupAppearance.IsExpanded				= true;
			using (PropertyItemCreator pic = new PropertyItemCreator(typeof(XamRibbon), groupAppearance))
			{
				this.Items.Add(pic.GetPropertyActionItem("IsMinimized"));
				this.Items.Add(pic.GetPropertyActionItem("QuickAccessToolbarLocation"));
				this.Items.Add(pic.GetPropertyActionItem("Theme"));
			}

			#endregion //Appearance Group

			#region Behavior Group

			DesignerActionItemGroup groupBehavior = new DesignerActionItemGroup(SR.GetString("SmartTag_G_Behavior"), groupSequence++);
			groupBehavior.IsExpanded = true;
			using (PropertyItemCreator pic = new PropertyItemCreator(typeof(XamRibbon), groupBehavior))
			{
				this.Items.Add(pic.GetPropertyActionItem("AllowMinimize"));
				this.Items.Add(pic.GetPropertyActionItem("AutoHideEnabled"));
				this.Items.Add(pic.GetPropertyActionItem("AutoHideHorizontalThreshold"));
				this.Items.Add(pic.GetPropertyActionItem("AutoHideVerticalThreshold"));
				this.Items.Add(pic.GetPropertyActionItem("IsRibbonGroupResizingEnabled"));
			}

			#endregion //Behavior Group
		}

        #endregion //Constructors

		#region DesignerActionMethodItem Callbacks

			#region PerformAction_AddTab

		private static void PerformAction_AddTab(EditingContext context, ModelItem adornedControlModel, DesignerActionList designerActionList)
		{
			ModelItem tabsModelItem = adornedControlModel.Properties["Tabs"].Value;
            // JM 02-27-12 - TFS102654 For some reason in VS11 when we enter this method after having already added a RibbonTabItem
            // on a previous call, the ItemType for the Tabs property returns typeof(RibbonTabItem) instead of typeof(ObservableCollection<RibbonTabItem>).
            // This could be a bug in VS11 but it shouldn't hurt to add a check for typeof(RibbonTabItem) here.
            //if (tabsModelItem == null || tabsModelItem.ItemType != typeof(ObservableCollection<RibbonTabItem>))
            if (tabsModelItem == null || (tabsModelItem.ItemType != typeof(ObservableCollection<RibbonTabItem>)  &&
                                          tabsModelItem.ItemType != typeof(RibbonTabItem)))
            {
				// Create a ModelItem & object instance for the property.
				tabsModelItem = ModelFactory.CreateItem(context, typeof(ObservableCollection<RibbonTabItem>), null);

				// Set the value of the property to the newly create ModelItem
				adornedControlModel.Properties["Tabs"].SetValue(tabsModelItem);
			}

			// Create a new RibbonTabItem and set it's header property to a default value 
			ModelItem		newTabModelItem		= ModelFactory.CreateItem(context, typeof(RibbonTabItem), null);
			newTabModelItem.Properties["Header"].SetValue(SR.GetString("SmartTag_Default_RibbonTabItemHeader"));

			// Add the new RibbonTabItem to the Tabs collection
			ModelProperty	tabsModelProperty	= adornedControlModel.Properties["Tabs"];
			tabsModelProperty.Collection.Add(newTabModelItem);
		}

			#endregion //PerformAction_AddTab

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