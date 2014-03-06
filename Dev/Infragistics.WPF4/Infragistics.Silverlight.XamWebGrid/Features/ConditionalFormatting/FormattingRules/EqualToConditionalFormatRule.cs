using System.Windows;

namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// A conditional formatting rule that evaluates if the value equals an inputted value.
	/// </summary>
	public class EqualToConditionalFormatRule : DiscreetRuleBase
	{

        #region Members
        private object _valueResolved;
        private bool _valueResolvedExecuted;
        #endregion // Members

		#region Properties

		#region Value

		/// <summary>
		/// Identifies the <see cref="Value"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(object), typeof(EqualToConditionalFormatRule), new PropertyMetadata(new PropertyChangedCallback(ValueChanged)));

		/// <summary>
		/// Get / set the value to be compared.
		/// </summary>
		public object Value
		{
			get { return (object)this.GetValue(ValueProperty); }
			set { this.SetValue(ValueProperty, value); }
		}

		private static void ValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			EqualToConditionalFormatRule rule = (EqualToConditionalFormatRule)obj;
            rule._valueResolvedExecuted = false;
			rule.OnPropertyChanged("Value");
		}

		#endregion // Value

		#region ResolvedValue

		/// <summary>		
		/// Gets the value resolved to the datatype of the column
		/// </summary>
		protected object ResolvedValue
		{
            get
            {
                
#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)

                if (this._valueResolvedExecuted)
                    return this._valueResolved;

                object value = this.Value;

                if (this.Column != null)
                {
                    if (value != null)
                    {
                        if (value.GetType() == this.Column.DataType)
                        {
                            this._valueResolved = this.Value;
                            this._valueResolvedExecuted = true;
                        }
                    }
                    try
                    {
                        _valueResolved = this.Column.ResolveValue(value, System.Globalization.CultureInfo.InvariantCulture);
                        this._valueResolvedExecuted = true;
                    }
                    catch
                    {
                    }
                }
                else
                {
                    _valueResolved = value;
                }
                return _valueResolved;
            }
		}

		#endregion // ResolvedValue

		#endregion // Properties

		#region Overrides
		
		/// <summary>
		/// Creates a new instance of the class which will execute the logic to populate and execute the conditional formatting logic for this <see cref="IConditionalFormattingRule"/>.
		/// </summary>
		/// <returns></returns>
		protected internal override IConditionalFormattingRuleProxy CreateProxy()
		{
			return new EqualToConditionalFormatRuleProxy() { Value = this.Value };
		}

		#endregion // Overrides
	}

	/// <summary>
	/// The execution proxy for the <see cref="EqualToConditionalFormatRule"/>.
	/// </summary>
	public class EqualToConditionalFormatRuleProxy : DiscreetRuleBaseProxy
    {
        #region Members
        private object _valueResolved;
        private bool _valueResolvedExecuted;
        #endregion // Members

        #region Properties

        #region Value

        /// <summary>
		/// Get / set the value to be compared.
		/// </summary>
		public object Value
		{
            get
            {
                return _valueResolved;
            }
            set
            {
                if (this._valueResolved != value)
                {                    
                    this._valueResolvedExecuted = false;
                    this._valueResolved = value;
                }
            }
		}

		#endregion // Value

		#region ResolvedValue

		/// <summary>		
		/// Gets the value resolved to the datatype of the column
		/// </summary>
		protected object ResolvedValue
		{
			get
			{
                
#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)

                if (this._valueResolvedExecuted)
                    return this._valueResolved;

				object value = this.Value;

                if (this.Parent.Column != null)
                {
                    if (value != null)
                    {
                        if (value.GetType() == this.Parent.Column.DataType)
                        {
                            this._valueResolved = this.Value;
                            this._valueResolvedExecuted = true;
                        }
                    }
                    try
                    {
                        _valueResolved = this.Parent.Column.ResolveValue(value, System.Globalization.CultureInfo.InvariantCulture);
                        this._valueResolvedExecuted = true;
                    }
                    catch
                    {
                    }
                }
                else
                {
                    _valueResolved = value;
                }
				return _valueResolved;
			}
		}

		#endregion // ResolvedValue

		#endregion // Properties

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
			object value = this.ResolvedValue;

			if (sourceDataValue == value)
			{
				return ((DiscreetRuleBase)this.Parent).StyleToApply;
			}
			if (sourceDataValue != null)
			{
				if (sourceDataValue.Equals(value))
				{
					return ((DiscreetRuleBase)this.Parent).StyleToApply;
				}
			}
			return null;
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