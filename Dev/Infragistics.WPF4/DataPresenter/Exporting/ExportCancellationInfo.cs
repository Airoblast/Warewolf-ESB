using System;
using System.Collections.Generic;
using System.Text;

namespace Infragistics.Windows.DataPresenter
{
	/// <summary>
	/// Used to provide additional information to the exporter about how the operation was cancelled.
	/// </summary>
	[InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_WordWriter)]
	public class RecordExportCancellationInfo
	{
		#region Member Variables

		private Exception _exception;
		private RecordExportCancellationReason _reason;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="RecordExportCancellationInfo"/>
		/// </summary>
		/// <param name="reason">The reason for the cancellation</param>
		/// <param name="exception">The exception related to the cancellation or null if the cancel was not related to an exception being raised.</param>
		public RecordExportCancellationInfo(RecordExportCancellationReason reason, Exception exception)
		{
			_exception = exception;
			_reason = reason;
		} 
		#endregion //Constructor

		#region Properties
		/// <summary>
		/// Returns the exception involved in the cancellation or null if the cancellation was not related to an exception being raised.
		/// </summary>
		public Exception Exception
		{
			get { return _exception; }
		} 

		/// <summary>
		/// Returns an enumeration indicating the action that initiated the cancellation of the export operation.
		/// </summary>
		public RecordExportCancellationReason Reason
		{
			get { return _reason; }
		}
		#endregion //Properties
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