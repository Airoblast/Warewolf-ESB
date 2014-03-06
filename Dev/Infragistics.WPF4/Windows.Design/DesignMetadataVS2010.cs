using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Windows.Design.Metadata;
using Microsoft.Windows.Design.Features;



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

[assembly: ProvideMetadata(typeof(Infragistics.Windows.Design.VS2010.DesignMetadataVS2010))]

namespace Infragistics.Windows.Design.VS2010
{
	internal class DesignMetadataVS2010 : IProvideAttributeTable

	{
		#region IProvideAttributeTable Members

		AttributeTable IProvideAttributeTable.AttributeTable
		{
			get 
			{
				AttributeTableBuilder builder = DesignMetadataHelper.GetAttributeTableBuilder();

				//VS2010 specific attributes.
				//
				// In pre-VS2010 we used DefaultInitializers that derived from our custom class 
				// DefaultInitializerDeferred so that we could initialized defaults with an EditingContext
				// which was not originally passed into the DefaultIinitializer.InitializeDefaults override.
				// Now that VS2010 does pass in the EditingContext, use the default initializers that
				// derive from the DefaultInitializer class.
				builder.AddCustomAttributes(typeof(Infragistics.Windows.Controls.XamTabControl), new FeatureAttribute(typeof(DefaultInitializerXamTabControl)));
				builder.AddCustomAttributes(typeof(Infragistics.Windows.Controls.TabItemEx), new FeatureAttribute(typeof(DefaultInitializerTabItemEx)));

				
				builder.AddCustomAttributes(typeof(Infragistics.Windows.Controls.XamTabControl), new FeatureAttribute(typeof(XamTabControlContextMenuProvider)));
				builder.AddCustomAttributes(typeof(Infragistics.Windows.Controls.TabItemEx), new FeatureAttribute(typeof(XamTabControlContextMenuProvider)));

				return builder.CreateTable();
			}
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