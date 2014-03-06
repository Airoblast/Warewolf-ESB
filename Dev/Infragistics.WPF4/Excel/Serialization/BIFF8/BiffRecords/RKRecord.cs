using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords
{
	internal sealed class RKRecord : CellValueRecordBase
	{
		// MD 9/2/08 - Excel formula solving
		// Changed signature to allow FORMULA records to save out calculated values
		//protected override object LoadCellValue( BIFF8WorkbookSerializationManager manager )
		//{
		//    return Utilities.DecodeRKValue( manager.CurrentRecordStream.ReadUInt32() );
		//}
		protected override void LoadCellValue( BIFF8WorkbookSerializationManager manager, ref byte[] data, ref int dataIndex, out object cellValue, out object lastCalculatedCellValue )
		{
			cellValue = Utilities.DecodeRKValue( manager.CurrentRecordStream.ReadUInt32FromBuffer( ref data, ref dataIndex ) );
			lastCalculatedCellValue = cellValue;
		}

		// MD 9/2/08 - Excel formula solving
		// Changed signature to allow FORMULA records to save out calculated values
		//protected override void SaveCellValue( BIFF8WorkbookSerializationManager manager, object value )
		// MD 4/18/11 - TFS62026
		// Since the FORMULA record is the only one which needs the lastCalculatedCellValue, it is no longer passed in here. Instead, pass along the cell
		// context so the FORMULA record can ask for the calculated value directly.
		// Also, we will write the cell value records in one block if possible, so pass in the memory stream containing the initial data from the record.
		//protected override void SaveCellValue( BIFF8WorkbookSerializationManager manager, object cellValue, object lastCalculatedCellValue )
		protected override void SaveCellValue(BIFF8WorkbookSerializationManager manager, CellContext cellContext, MemoryStream initialData)
		{
			uint rkValue = 0;

			// MD 4/18/11 - TFS62026
			// Instead of writing to the record stream, write to the memory stream and we will send it to the record stream in one shot.
			//// MD 9/2/08 - Excel formula solving
			////if ( Utilities.TryEncodeRKValue( Convert.ToDouble( value, CultureInfo.CurrentCulture ), out rkValue ) )
			//if ( Utilities.TryEncodeRKValue( Convert.ToDouble( cellValue, CultureInfo.CurrentCulture ), out rkValue ) )
			//    manager.CurrentRecordStream.Write( rkValue );
			//else
			//{
			//    Utilities.DebugFail("This should not have happened");
			//    manager.CurrentRecordStream.Write( (uint)0 );
			//}
			// MD 9/2/08 - Excel formula solving
			//if ( Utilities.TryEncodeRKValue( Convert.ToDouble( value, CultureInfo.CurrentCulture ), out rkValue ) )
			// MD 4/6/12 - TFS101506
			//if (Utilities.TryEncodeRKValue(Convert.ToDouble(cellContext.Value, CultureInfo.CurrentCulture), out rkValue) == false)
			if (Utilities.TryEncodeRKValue(Convert.ToDouble(cellContext.Value, manager.Workbook.CultureResolved), out rkValue) == false)
				Utilities.DebugFail("This should not have happened");

			initialData.Write(BitConverter.GetBytes(rkValue), 0, 4);
			manager.CurrentRecordStream.Write(initialData);
		}

		public override BIFF8RecordType Type
		{
			get { return BIFF8RecordType.RK; }
		}
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