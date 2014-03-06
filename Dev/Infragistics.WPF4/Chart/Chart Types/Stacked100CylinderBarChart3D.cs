
#region Using

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Windows.Media;

#endregion Using

namespace Infragistics.Windows.Chart
{
    /// <summary>
    /// This class creates 3D stacked 100% cylinder bar chart. This class is also 
    /// responsible for 3D stacked 100% cylinder bar chart animation.
    /// </summary>
    internal class Stacked100CylinderBarChart3D : CylinderBarChart3D
    {
        #region ChartTypeparameters

        /// <summary>
        /// Gets a Boolean value that specifies whether every data point of 
        /// the series have different color for this chart type.
        /// </summary>
        override internal bool IsColorFromPoint { get { return false; } }

        /// <summary>
        /// Gets a Boolean value that specifies whether this chart type has Bar type axis
        /// </summary>
        override internal bool IsBar { get { return true; } }

        /// <summary>
        /// Gets a Boolean value that specifies whether this chart type is stacked chart type
        /// </summary>
        override internal bool IsStacked { get { return true; } }

        /// <summary>
        /// Gets a Boolean value that specifies whether this chart type is 100% stacked chart type
        /// </summary>
        override internal bool IsStacked100 { get { return true; } }

        #endregion ChartTypeparameters

        #region Methods

        /// <summary>
        /// Draws data points for different 3D chart types using 3D models. Creates data points 
        /// as 3D shapes for all series which have selected chart type. Creates hit test functionality 
        /// and tooltips for data points.
        /// </summary>
        /// <param name="model3DGroup">Model 3D group which keeps all column 3D objects.</param>
        /// <param name="rotateTransform3D">3D Rotation angles of the scene.</param>
        protected override void Draw3D(Model3DGroup model3DGroup, RotateTransform3D rotateTransform3D)
        {
            SeriesCollection seriesList = GetSeries();

            int seriesNum = seriesList.Count;

            int numOfPoints = GetStackedNumberOfPoints(ChartType.Stacked100CylinderBar, seriesList);

            //Add the geometry model to the model group.
            int columnIndx = 0;
            for (int pointIndx = 0; pointIndx < numOfPoints; pointIndx++)
            {
                double sum = 0;

                // Skip point if out of range
                if (IsXOutOfRange(pointIndx + 1 - 0.5) || IsXOutOfRange(pointIndx + 1 + 0.5))
                {
                    continue;
                }

                for (int seriesIndx = 0; seriesIndx < seriesNum; seriesIndx++)
                {
                    if (seriesList[seriesIndx].ChartType != ChartType.Stacked100CylinderBar || seriesList[seriesIndx].DataPoints.Count <= pointIndx)
                    {
                        // The Data Series of a stacked chart has different number of data points.
                        continue;
                    }
                    DataPoint point = seriesList[seriesIndx].DataPoints[pointIndx];

                    //Skip Null values
                    if (point.NullValue == true)
                    {
                        continue;
                    }

                    sum += Math.Abs(point.Value);
                }

                double stackedStartPositive = 0;
                double stackedEndPositive = 0;
                double stackedStartNegative = 0;
                double stackedEndNegative = 0;

                for (int seriesIndx = 0; seriesIndx < seriesNum; seriesIndx++)
                {
                    if (seriesList[seriesIndx].ChartType != ChartType.Stacked100CylinderBar || seriesList[seriesIndx].DataPoints.Count <= pointIndx)
                    {
                        continue;
                    }

                    DataPoint point = seriesList[seriesIndx].DataPoints[pointIndx];

                    //Skip Null values
                    if (point.NullValue == true)
                    {
                        continue;
                    }

                    double x, y, z;
                    double width, depth, height;

                    double val = GetStacked100Value(point, sum);

                    if (point.Value > 0)
                    {

                        stackedEndPositive += val;

                        y = AxisX.GetPosition(pointIndx + 1);
                        x = StackedColumnChart3D.GetYValue(stackedStartPositive, AxisY);
                        z = AxisZ.GetPosition(GetZSeriesPosition(seriesIndx, seriesList) - 0.5);

                        // Get point width and depth from chart parameter
                        GetPointWidth(out width, out depth, AxisX, AxisZ, seriesList[seriesIndx]);
                        height = StackedColumnChart3D.GetHeightValue(stackedStartPositive, stackedEndPositive, AxisY);

                        stackedStartPositive += val;


                    }
                    else
                    {
                        stackedStartNegative += val;

                        y = AxisX.GetPosition(pointIndx + 1);
                        x = StackedColumnChart3D.GetYValue(stackedStartNegative, AxisY);
                        z = AxisZ.GetPosition(GetZSeriesPosition(seriesIndx, seriesList) - 0.5);

                        // Get point width and depth from chart parameter
                        GetPointWidth(out width, out depth, AxisX, AxisZ, seriesList[seriesIndx]);
                        height = StackedColumnChart3D.GetHeightValue(stackedStartNegative, stackedEndNegative, AxisY);

                        stackedEndNegative += val;
                    }

                    // Find edge size which is proportional to the 
                    // width and depth of the column.
                    double size = GetPointSize(width, depth);
                    double edge = GetEdgeSize(size, point);

                    // Get brush from point
                    Brush brush = GetBrush(point, seriesIndx, pointIndx);
                    
                    columnIndx++;

                    GeometryModel3D geometry = CreateColumnCylinder(columnIndx, new Point3D(x, y, z), size, height, brush, rotateTransform3D, point.Value < 0, point, pointIndx, edge);

                    SetHitTest3D(geometry, point);

                    model3DGroup.Children.Add(geometry);

                    if (point.GetMarker() != null)
                    {
                        GeometryModel3D label = CreateCylinderLabel(columnIndx, new Point3D(x, y, z), size, height, size, brush, rotateTransform3D, point.Value < 0, point, pointIndx, edge);

                        //Add the label model to the model group.
                        model3DGroup.Children.Add(label);
                    }
                }
            }
        }

        #endregion Methods
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