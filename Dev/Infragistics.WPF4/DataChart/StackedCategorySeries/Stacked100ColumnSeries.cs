using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using Infragistics.Controls.Charts.Util;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents a XamDataChart stacked100 column series.
    /// </summary>
    public class Stacked100ColumnSeries:StackedColumnSeries, IStacked100Series
    {
        #region C'tor & Initialization
        /// <summary>
        /// Initializes a new instance of a Stacked100ColumnSeries class.
        /// </summary>
        public Stacked100ColumnSeries()
        {
            DefaultStyleKey = typeof(Stacked100ColumnSeries);
        }

        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal processes
        /// call ApplyTemplate.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            RenderSeries(false);
        }
        #endregion

        #region View-related
        /// <summary>
        /// Called to create the view for the series.
        /// </summary>
        /// <returns>The created view.</returns>
        protected override SeriesView CreateView()
        {
            return new Stacked100ColumnSeriesView(this);
        }

        internal Stacked100ColumnSeriesView Stacked100ColumnView { get; set; }
        
        /// <summary>
        /// Called when the view has been created.
        /// </summary>
        /// <param name="view">The view class for the current series</param>
        protected internal override void OnViewCreated(SeriesView view)
        {
            base.OnViewCreated(view);
            Stacked100ColumnView = (Stacked100ColumnSeriesView)view;
        }
        #endregion

        #region Method Overrides

        internal override CategorySeriesView GetSeriesView()
        {
            return Stacked100ColumnView;
        }

        /// <summary>
        /// Calculates the value column and min and max values.
        /// </summary>
        protected override void PrepareData()
        {
            base.PrepareData();

            if (FastItemsSource == null)
                return;

            //calculate Minimum and Maximum based on percentages.
            double min = double.PositiveInfinity;
            double max = double.NegativeInfinity;

            for (int i = 0; i < FastItemsSource.Count; i++)
            {
                double total = Math.Abs(Lows[i]) + Highs[i];

                if (total == 0)
                {
                    min = Math.Min(min, 0);
                    max = Math.Max(max, 0);
                    continue;
                }

                min = Math.Min(min, Lows[i] / total * 100);
                max = Math.Max(max, Highs[i] / total * 100);
            }

            Minimum = min;
            Maximum = max;
        }
        #endregion

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