using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml;
using System.Globalization;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
	// MD 12/21/11 - 12.1 - Table Support
	internal class CalculatedColumnFormulaElement : TableColumnFormulaElementBase
	{
		#region XML Schema Fragment

		//<complexType name="CT_TableFormula">
		//  <simpleContent>
		//    <extension base="ST_Formula">
		//      <attribute name="array" type="xsd:boolean" default="false"/>
		//    </extension>
		//  </simpleContent>
		//</complexType>

		#endregion // Xml Schema Fragment

		#region Constants






		public const string LocalName = "calculatedColumnFormula";






		public const string QualifiedName =
			XLSXElementBase.DefaultXmlNamespace +
			XmlElementBase.NamespaceSeparator +
			CalculatedColumnFormulaElement.LocalName;

		#endregion // Constants

		#region Base Class Overrides

		#region GetFormula

		protected override Formula GetFormula(WorksheetTableColumn column)
		{
			return column.ColumnFormula;
		}

		#endregion // GetFormula

		#region SetFormula

		protected override void SetFormula(WorksheetTableColumn column, Formula formula)
		{
			column.SetColumnFormula(formula, false);
		}

		#endregion // SetFormula

		#region Type

		public override XLSXElementType Type
		{
			get { return XLSXElementType.calculatedColumnFormula; }
		}
		
		#endregion // Type

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