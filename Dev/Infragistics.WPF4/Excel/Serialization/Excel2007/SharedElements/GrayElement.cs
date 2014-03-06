using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;







using System.Drawing;
using Infragistics.Documents.Excel.Serialization.Excel2007.SharedContentTypes;
namespace Infragistics.Documents.Excel.Serialization.Excel2007.SharedElements

{



    internal class GrayElement : XmlElementBase
    {
        #region Constants

        public const string LocalName = "gray";

        public const string QualifiedName =
            ThemePart.DefaultNamespace +
            XmlElementBase.NamespaceSeparator +
            GrayElement.LocalName;

        #endregion Constants

        #region Base Class Overrides

        #region ElementName

        public override string ElementName
        {
            get { return GrayElement.QualifiedName; }
        }

        #endregion ElementName

        #region Load

        protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
        {

            ChildDataItem dataItem = (ChildDataItem)manager.ContextStack[typeof(ChildDataItem)] as ChildDataItem;

            if (dataItem == null)
            {
                Utilities.DebugFail("ChildDataItem object not found on the ContextStack.");
                return;
            }

            if (dataItem.Data is Color == false)
            {
                Utilities.DebugFail("Color object not found in ChildDataItem.");
                return;
            }

            Color color = (Color)dataItem.Data;

            // common grayscale formula is: Luminance=0.3Red+0.59Green+0.11Blue
            int luma = (int)((color.R * 0.3) + (color.G * 0.59) + (color.B * 0.11));
            color = Color.FromArgb(0xFF, (byte)luma, (byte)luma, (byte)luma);

            dataItem.Data = color;
        }

        #endregion Load

        #region Save

        protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
        {
            Utilities.DebugFail("Save() method for gray element not yet implemented.");
        }

        #endregion Save

        #endregion Base Class Overrides
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