using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml;
using Infragistics.Documents.Excel.Serialization.Excel2007.SharedContentTypes;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.SharedElements
{
	// MD 2/9/12 - TFS89375
	internal class MajorFontElement : XmlElementBase
	{
		#region XML Schema Fragment

		//<complexType name="CT_MajorFont">
		//  <sequence>
		//    <element name="latin" type="CT_TextFont" minOccurs="1" maxOccurs="1"/>
		//    <element name="ea" type="CT_TextFont" minOccurs="1" maxOccurs="1"/>
		//    <element name="cs" type="CT_TextFont" minOccurs="1" maxOccurs="1"/>
		//    <element name="font" type="CT_SupplementalFont" minOccurs="0" maxOccurs="unbounded"/>
		//    <element name="extLst" type="CT_OfficeArtExtensionList" minOccurs="0" maxOccurs="1"/>
		//  </sequence>
		//</complexType>

		#endregion // Xml Schema Fragment

		#region Constants






		public const string LocalName = "majorFont";






		public const string QualifiedName =
			ThemePart.DefaultNamespace +
			XmlElementBase.NamespaceSeparator +
			MajorFontElement.LocalName;


		#endregion // Constants

		#region Base Class Overrides

		#region ElementName

		public override string ElementName
		{
			get { return MajorFontElement.QualifiedName; }
		}

		#endregion ElementName

		#region Load

		protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
		{

			#region Load Attribute Values

			foreach (ExcelXmlAttribute attribute in element.Attributes)
			{
				string attributeName = XmlElementBase.GetQualifiedAttributeName(attribute);

				switch (attributeName)
				{

					default:
						Utilities.DebugFail("Unknown attribute " + attributeName);
						break;
				}
			}

			#endregion // Load Attribute Values

			manager.ContextStack.Push(manager.MajorFonts);
		}

		#endregion // Load

		#region Save

		protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
		{
			string attributeValue = String.Empty;


			// Add the 'latin' element
			//XmlElementBase.AddElement(element, LatinElement.QualifiedName);

			// Add the 'ea' element
			//XmlElementBase.AddElement(element, EaElement.QualifiedName);

			// Add the 'cs' element
			//XmlElementBase.AddElement(element, CsElement.QualifiedName);

			// Add the 'font' element
			//XmlElementBase.AddElement(element, FontElement.QualifiedName);

			// Add the 'extLst' element
			//XmlElementBase.AddElement(element, ExtLstElement.QualifiedName);


		}

		#endregion // Save

		#endregion // Base Class Overrides
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