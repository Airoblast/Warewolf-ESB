using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
    class BgColorElement : ColorElement
    {
        #region Constants

        public new const string LocalName = "bgColor";

        public new const string QualifiedName =
            XLSXElementBase.DefaultXmlNamespace +
            XmlElementBase.NamespaceSeparator +
            BgColorElement.LocalName;

        #endregion Constants

        #region Base Class Overrides

        #region Type

        /// <summary>
        /// Returns the <see cref="XLSXElementType">XLSXElementType</see> constant which identifies this element.
        /// </summary>
        public override XLSXElementType Type
        {
            get { return XLSXElementType.bgColor; }
        }
        #endregion Type

        #region Load



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
        {
            PatternFillInfo patternFillInfo = (PatternFillInfo)manager.ContextStack[typeof(PatternFillInfo)];

            if (patternFillInfo == null)
            {
                Utilities.DebugFail("PatternFillInfo object not found on the ContextStack.");
                return;
            }

            //put a new ColorInfo on the stack for the base class (ColorElement) to retrieve.
            ColorInfo colorInfo = new ColorInfo();
            manager.ContextStack.Push(colorInfo);

            base.Load(manager, element, value, ref isReaderOnNextNode);
        }

        #endregion Load

        #region Save



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
        {
            PatternFillInfo patternFillInfo = (PatternFillInfo)manager.ContextStack[typeof(PatternFillInfo)];
            if (patternFillInfo == null)
            {
                Utilities.DebugFail("PatternFillInfo not found on the ContextStack.");
                return;
            }
            if (patternFillInfo.BackgroundColor != null)
                this.SaveColorElement(patternFillInfo.BackgroundColor, manager, element, ref value);
        }

        #endregion Save

        #region OnAfterLoadChildElements

		protected override void OnAfterLoadChildElements( Excel2007WorkbookSerializationManager manager, ElementDataCache elementCache )
        {
            PatternFillInfo patternFillInfo = (PatternFillInfo)manager.ContextStack[typeof(PatternFillInfo)];
            if (patternFillInfo == null)
            {
                Utilities.DebugFail("PatternFillInfo object not found on the ContextStack.");
                return;
            }

            ColorInfo colorInfo = (ColorInfo)manager.ContextStack[typeof(ColorInfo)];
            if (colorInfo == null)
            {
                Utilities.DebugFail("For some reason, the ColorInfo was removed from the context stack");
                return;
            }
            patternFillInfo.BackgroundColor = colorInfo;
        }

        #endregion OnAfterLoadChildElements

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