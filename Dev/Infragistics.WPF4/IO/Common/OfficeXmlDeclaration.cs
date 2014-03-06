using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Infragistics.Documents.Core
{
    /// <summary>
    /// Duplicated:
    /// Infragistics.Documents.Excel.Serialization.Excel2007.ExcelXmlDeclaration
    /// </summary>
	internal class OfficeXmlDeclaration : OfficeXmlNode
	{
		private string version;
		private string encoding;
		private string standalone;

		internal OfficeXmlDeclaration( string version, string encoding, string standalone, OfficeXmlDocument ownerDocument )
			: base( ownerDocument )
		{
			this.version = version;
			this.encoding = encoding;
			this.standalone = standalone;
		}

		#region Base Class Overrides

		public override string LocalName
		{
			get { return "xml"; }
		}

		public override XmlNodeType NodeType
		{
			get { return XmlNodeType.XmlDeclaration; }
		}

		public override void WriteStart()
		{
            //  BF 9/29/10  IGWord
            //  I needed the same code for Word so I put this in a utility method
            #region Externalized
            //StringBuilder builder = new StringBuilder( "version=\"" + this.version + "\"" );
            //if ( this.encoding.Length > 0 )
            //{
            //    builder.Append( " encoding=\"" );
            //    builder.Append( this.encoding );
            //    builder.Append( "\"" );
            //}
            //if ( this.standalone.Length > 0 )
            //{
            //    builder.Append( " standalone=\"" );
            //    builder.Append( this.standalone );
            //    builder.Append( "\"" );
            //}
            #endregion Externalized

            string declaration = SerializationUtilities.GetXmlDeclaration( this.version, this.encoding, this.standalone );
			this.OwnerDocument.Writer.WriteProcessingInstruction( this.LocalName, declaration );
		}

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