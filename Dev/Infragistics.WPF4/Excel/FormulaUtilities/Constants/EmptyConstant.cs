using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Infragistics.Documents.Excel.Serialization;
using System.Globalization;

namespace Infragistics.Documents.Excel.FormulaUtilities.Constants
{



	internal class EmptyConstant : Constant
	{
		#region Base Class Overrides

		// MD 10/22/10 - TFS36696
		// All constants are basically immutable, so we don't need to clone them.
		//public override Constant Clone()
		//{
		//    return new EmptyConstant();
		//}

		// MD 12/22/11 - 12.1 - Table Support
		public override bool Equals(object obj)
		{
			return obj is EmptyConstant;
		}

		public override int GetHashCode()
		{
			return 0;
		}

		public override void Load(BiffRecordStream stream, ref byte[] data, ref int dataIndex)
		{
			stream.ReadBytesFromBuffer(8, ref data, ref dataIndex); // not used
		}

		public override void Save(BiffRecordStream stream)
		{
			stream.Write(new byte[8]);
		}

		// MD 4/6/12 - TFS101506
		//public override string ToString()
		public override string ToString(CultureInfo culture)
		{
			return String.Empty;
		}

		public override byte ConstantCode
		{
			get { return 0; }
		}

		// MD 8/18/08 - Excel formula solving
		public override object CalcValue
		{
			get { return null; }
		}

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