using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;






using Infragistics.Documents.Excel.Serialization.Excel2007.SharedContentTypes;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.SharedElements.LegacyDrawing

{
    internal class PathLegacyElement : XmlElementBase
	{
		#region XML Schema Fragment

		//<complexType name="CT_Path">
		//    <attributeGroup ref="AG_Id"/>
		//    <attribute name="v" type="xsd:string" use="optional"/>
		//    <attribute name="limo" type="xsd:string" use="optional"/>
		//    <attribute name="textboxrect" type="xsd:string" use="optional"/>
		//    <attribute name="fillok" type="ST_TrueFalse" use="optional"/>
		//    <attribute name="strokeok" type="ST_TrueFalse" use="optional"/>
		//    <attribute name="shadowok" type="ST_TrueFalse" use="optional"/>
		//    <attribute name="arrowok" type="ST_TrueFalse" use="optional"/>
		//    <attribute name="gradientshapeok" type="ST_TrueFalse" use="optional"/>
		//    <attribute name="textpathok" type="ST_TrueFalse" use="optional"/>
		//    <attribute name="insetpenok" type="ST_TrueFalse" use="optional"/>
		//    <attribute ref="o:connecttype"/>
		//    <attribute ref="o:connectlocs"/>
		//    <attribute ref="o:connectangles"/>
		//    <attribute ref="o:extrusionok"/>
		//</complexType>

		#endregion //XML Schema Fragment

		#region Constants






		public const string LocalName = "path";






		public const string QualifiedName =
			LegacyDrawingsPart.VmlNamespace +
			XmlElementBase.NamespaceSeparator +
			PathLegacyElement.LocalName;

		private const string GradientShapeOkAttributeName = "gradientshapeok";
		private const string ConnecttypeAttributeName =
			LegacyDrawingsPart.OfficeNamespace +
			XmlElementBase.NamespaceSeparator +
			"connecttype";

		#endregion Constants

		#region Base class overrides

		#region ElementName

		public override string ElementName
		{
			get { return PathLegacyElement.QualifiedName; }
		}

		#endregion ElementName

		#region Load



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		protected override void Load( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode )
		{
		}

		#endregion Load

		#region Save



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		protected override void Save( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value )
		{
			// Save out a path element with the standard values since we only support this
			// element with the use of cell comments

			// Add the 'gradientshapeok' attribute
			string attributeValue = "f";
			XmlElementBase.AddAttribute( element, PathLegacyElement.GradientShapeOkAttributeName, attributeValue );

			// Add the 'connecttype' attribute
			attributeValue = "segments";
			XmlElementBase.AddAttribute( element, PathLegacyElement.ConnecttypeAttributeName, attributeValue );
		}

		#endregion Save

		#endregion Base class overrides
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