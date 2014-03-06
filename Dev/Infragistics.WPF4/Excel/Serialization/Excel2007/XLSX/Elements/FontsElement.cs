using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;




using Infragistics.Documents.Excel.Serialization.Excel2007.SharedContentTypes;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements

{
    class FontsElement : XLSXElementBase
    {
        #region Constants

        public const string LocalName = "fonts";

        public const string QualifiedName =
            XLSXElementBase.DefaultXmlNamespace +
            XmlElementBase.NamespaceSeparator +
            FontsElement.LocalName;

        #endregion Constants

        #region Base Class Overrides

        #region Type






        public override XLSXElementType Type
        {
            get { return XLSXElementType.fonts; }
        }
        #endregion Type

        #region Load



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
        {
			// MD 1/19/11 - TFS62268
			// The <font> element handlers need to add items to the FontColorInfos collection on the manager, 
			// so push it on th econtext stack.
			manager.ContextStack.Push(new ListContext<ColorInfo>(manager.FontColorInfos));
        }

        #endregion Load

        #region Save



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
        {
            List<WorkbookFontData> fonts = manager.Fonts;
            int count = fonts.Count;
            XLSXElementBase.AddAttribute(element, "count", XLSXElementBase.GetXmlString(count, DataType.Int32));

            manager.ContextStack.Push(new ListContext<WorkbookFontData>(fonts));
            for (int i = 0; i < count; i++)
            {
                XLSXElementBase.AddElement(element, FontElement.QualifiedName);
            }
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