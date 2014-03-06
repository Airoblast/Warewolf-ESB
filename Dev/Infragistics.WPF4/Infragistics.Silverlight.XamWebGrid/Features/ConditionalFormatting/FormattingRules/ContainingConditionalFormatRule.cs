using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// A conditional formatting rule for strings which detects if a string contains a particular string.
	/// </summary>
	public class ContainingConditionalFormatRule : StringConditionalFormatRule
	{
		#region Overrides
		/// <summary>
		/// Creates a new instance of the class which will execute the logic to populate and execute the conditional formatting logic for this <see cref="IConditionalFormattingRule"/>.
		/// </summary>
		/// <returns></returns>
		protected internal override IConditionalFormattingRuleProxy CreateProxy()
		{
			return new ContainingConditionalFormatRuleProxy();
		}
		#endregion // Overrides
	}

	/// <summary>
	/// The execution proxy for the <see cref="ContainingConditionalFormatRule"/>.
	/// </summary>
	public class ContainingConditionalFormatRuleProxy : StringConditionalFormatRuleProxy
	{
		#region Overrides

		#region EvaluateCondition
		/// <summary>
		/// Determines whether the inputted value meets the condition of <see cref="ConditionalFormattingRuleBase"/> and returns the style 
		/// that should be applied.
		/// </summary>
		/// <param name="sourceDataObject"></param>
		/// <param name="sourceDataValue"></param>
		/// <returns></returns>
		protected override Style EvaluateCondition(object sourceDataObject, object sourceDataValue)
		{
			string dataValue = sourceDataValue as string;

			StringConditionalFormatRule baseObj = this.Parent as StringConditionalFormatRule;

			if (baseObj == null || string.IsNullOrEmpty(dataValue) || string.IsNullOrEmpty(baseObj.Value))
				return null;

			return (dataValue.Contains(baseObj.Value) ? baseObj.StyleToApply : null);
		}

		#endregion // EvaluateCondition

		#endregion // Overrides
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