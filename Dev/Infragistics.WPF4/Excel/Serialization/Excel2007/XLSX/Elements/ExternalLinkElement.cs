using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
    internal class ExternalLinkElement : XLSXElementBase
    {
        #region XML Schema Fragment

        //<complexType name="CT_ExternalLink"> 
        //  <choice> 
        //      <element name="externalBook" type="CT_ExternalBook" minOccurs="0" maxOccurs="1"/> 
        //      <element name="ddeLink" type="CT_DdeLink" minOccurs="0" maxOccurs="1"/> 
        //      <element name="oleLink" type="CT_OleLink" minOccurs="0" maxOccurs="1"/> 
        //      <element name="extLst" minOccurs="0" type="CT_ExtensionList"/> 
        //  </choice> 
        //</complexType> 

        #endregion //XML Schema Fragment

        #region Constants






        public const string LocalName = "externalLink";






        public const string QualifiedName =
            XLSXElementBase.DefaultXmlNamespace +
            XmlElementBase.NamespaceSeparator +
            ExternalLinkElement.LocalName;

        #endregion //Constants

        #region Base Class Overrides

        #region Type

        public override XLSXElementType Type
        {
            get { return XLSXElementType.externalLink; }
        }
        #endregion //Type

        #region Load



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
        {
            // We don't need to do anything here because this element does not have any attributes

            // Roundtrip
            // NOTE: This element can have one of 4 different possibilities, only one of which we support (externalBook).
            // Since we won't add these child elements to the manager, the manager will not be aware that it needs
            // to save out this file again, so we need to keep track of that somehow
        }
        #endregion //Load

        #region Save



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
        {
            ExternalWorkbookReference workbook = manager.ContextStack[typeof(ExternalWorkbookReference)] as ExternalWorkbookReference;
            if (workbook != null)
            {
                // Add the 'externalBook' element
                XmlElementBase.AddElement(element, ExternalBookElement.QualifiedName);
                return;
            }

            // Roundtrip - Remove this failure as we could be writing out one of the other elements
            // See the notes in the Load method
            Utilities.DebugFail("No external workbook on the context stack, not enough information to fully serialize element.");            
        }
        #endregion //Save

        #endregion //Base Class Overrides
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