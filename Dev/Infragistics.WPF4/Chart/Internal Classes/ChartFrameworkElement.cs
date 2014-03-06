
#region Using

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Media.Effects;
using System.Windows.Input;

#endregion Using

namespace Infragistics.Windows.Chart
{
    internal class ChartFrameworkElement : FrameworkElement
    {
        #region Properties

        /// <summary>
        /// Gets or sets the width of the element. 
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new double Width 
        {
            get
            {
                return base.Width;
            }
            set
            {
                base.Width = value;
            }
        }

        /// <summary>
        /// Gets or sets the suggested height of the element.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new double Height
        {
            get
            {
                return base.Height;
            }
            set
            {
                base.Height = value;
            }
        }

        /// <summary>
        /// Gets or sets the horizontal alignment characteristics applied 
        /// to this element when it is composed within a parent element, 
        /// such as a panel or items control. 
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new HorizontalAlignment HorizontalAlignment
        {
            get
            {
                return base.HorizontalAlignment;
            }
            set
            {
                base.HorizontalAlignment = value;
            }
        }

        /// <summary>
        /// Gets or sets the vertical alignment characteristics applied to 
        /// this element when it is composed within a parent element such as 
        /// a panel or items control. 
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new VerticalAlignment VerticalAlignment
        {
            get
            {
                return base.VerticalAlignment;
            }
            set
            {
                base.VerticalAlignment = value;
            }
        }

        /// <summary>
        /// Gets or sets the outer margin of an element. 
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Thickness Margin 
        {
            get
            {
                return base.Margin;
            }
            set
            {
                base.Margin = value;
            }
        }

        /// <summary>
        /// Gets or sets the opacity factor applied to the entire UIElement 
        /// when it is rendered in the user interface (UI). 
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new double Opacity
        {
            get
            {
                return base.Opacity;
            }
            set
            {
                base.Opacity = value;
            }
        }

        /// <summary>
        /// Gets or sets an opacity mask, as a Brush implementation that is 
        /// applied to any alpha-channel masking for the rendered content 
        /// of this element.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Brush OpacityMask
        {
            get
            {
                return base.OpacityMask;
            }
            set
            {
                base.OpacityMask = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to clip the content 
        /// of this element (or content coming from the child elements of 
        /// this element) to fit into the size of the containing element.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new bool ClipToBounds 
        {
            get
            {
                return base.ClipToBounds;
            }
            set
            {
                base.ClipToBounds = value;
            }
        }

        /// <summary>
        /// Gets or sets a value that determines whether rendering for this element 
        /// should use device-specific pixel settings during rendering. 
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new bool SnapsToDevicePixels
        {
            get
            {
                return base.SnapsToDevicePixels;
            }
            set
            {
                base.SnapsToDevicePixels = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this element 
        /// can be used as the target of a drag-and-drop operation. 
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new bool AllowDrop 
        {
            get
            {
                return base.AllowDrop;
            }
            set
            {
                base.AllowDrop = value;
            }
        }

#pragma warning disable 618
        /// <summary>
        /// Gets or sets a bitmap effect that applies directly to the rendered content for this element. 
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new BitmapEffect BitmapEffect 
        {
            get
            {
                return base.BitmapEffect;
            }
            set
            {
                base.BitmapEffect = value;
            }
        }

		/// <summary>
        /// Gets or sets an input source for the bitmap effect that 
        /// applies directly to the rendered content for this element. 
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new BitmapEffectInput BitmapEffectInput 
        {
            get
            {
                return base.BitmapEffectInput;
            }
            set
            {
                base.BitmapEffectInput = value;
            }
        }
#pragma warning restore 618

		/// <summary>
        /// Gets or sets the geometry used to define the outline of 
        /// the contents of an element. 
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Geometry Clip 
        {
            get
            {
                return base.Clip;
            }
            set
            {
                base.Clip = value;
            }
        }

        /// <summary>
        /// Gets or sets the context menu element that should appear 
        /// whenever the context menu is requested through user 
        /// interface (UI) from within this element. 
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new ContextMenu ContextMenu 
        {
            get
            {
                return base.ContextMenu;
            }
            set
            {
                base.ContextMenu = value;
            }
        }

        /// <summary>
        /// Gets or sets transform information that affects the rendering 
        /// position of this element.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Transform RenderTransform
        {
            get
            {
                return base.RenderTransform;
            }
            set
            {
                base.RenderTransform = value;
            }
        }

        /// <summary>
        /// Gets or sets a graphics transformation that should apply to this 
        /// element when layout is performed. 
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Transform LayoutTransform 
        {
            get
            {
                return base.LayoutTransform;
            }
            set
            {
                base.LayoutTransform = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum height constraint of the element. 
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new double MaxHeight
        {
            get
            {
                return base.MaxHeight;
            }
            set
            {
                base.MaxHeight = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum width constraint of the element.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new double MaxWidth
        {
            get
            {
                return base.MaxWidth;
            }
            set
            {
                base.MaxWidth = value;
            }
        }

        /// <summary>
        /// Gets or sets the minimum height constraint of the element.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new double MinHeight 
        {
            get
            {
                return base.MinHeight;
            }
            set
            {
                base.MinHeight = value;
            }
        }

        /// <summary>
        /// Gets or sets the minimum width constraint of the element. 
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new double MinWidth 
        {
            get
            {
                return base.MinWidth;
            }
            set
            {
                base.MinWidth = value;
            }
        }

        /// <summary>
        /// Gets or sets the user interface (UI) visibility of this element.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Visibility Visibility
        {
            get
            {
                return base.Visibility;
            }
            set
            {
                base.Visibility = value;
            }
        }

        /// <summary>
        /// Gets or sets the cursor that displays when the mouse pointer is over this element.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Cursor Cursor
        {
            get
            {
                return base.Cursor;
            }
            set
            {
                base.Cursor = value;
            }
        }

        /// <summary>
        /// Gets or sets the data context for an element when it participates in data binding.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Object DataContext 
        {
            get
            {
                return base.DataContext;
            }
            set
            {
                base.DataContext = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this element is enabled in the user interface (UI).
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new bool IsEnabled
        {
            get
            {
                return base.IsEnabled;
            }
            set
            {
                base.IsEnabled = value;
            }
        }

        /// <summary>
        /// Gets or sets the tool-tip object that is displayed for this element in the user interface (UI). 
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Object ToolTip 
        {
            get
            {
                return base.ToolTip;
            }
            set
            {
                base.ToolTip = value;
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the element can receive focus. 
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new bool Focusable
        {
            get
            {
                return base.Focusable;
            }
            set
            {
                base.Focusable = value;
            }
        }

        /// <summary>
        /// Gets or sets a value that declares whether this element can possibly be 
        /// returned as a hit test result from some portion of its rendered content.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new bool IsHitTestVisible
        {
            get
            {
                return base.IsHitTestVisible;
            }
            set
            {
                base.IsHitTestVisible = value;
            }
        }

        /// <summary>
        /// Gets or sets an arbitrary object value that can be used to 
        /// store custom information about this element. 
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Object Tag
        {
            get
            {
                return base.Tag;
            }
            set
            {
                base.Tag = value;
            }
        }

        /// <summary>
        /// Gets or sets a property that enables customization of appearance, 
        /// effects, or other style characteristics that will apply to this 
        /// element when it captures keyboard focus. 
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Style FocusVisualStyle 
        {
            get
            {
                return base.FocusVisualStyle;
            }
            set
            {
                base.FocusVisualStyle = value;
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether this FrameworkElement should force 
        /// the user interface (UI) to render the cursor as declared by the Cursor property. 
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new bool ForceCursor 
        {
            get
            {
                return base.ForceCursor;
            }
            set
            {
                base.ForceCursor = value;
            }
        }

        /// <summary>
        /// Gets or sets the context for input used by this FrameworkElement. 
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new InputScope InputScope 
        {
            get
            {
                return base.InputScope;
            }
            set
            {
                base.InputScope = value;
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether this element incorporates style 
        /// properties from theme styles.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new bool OverridesDefaultStyle 
        {
            get
            {
                return base.OverridesDefaultStyle;
            }
            set
            {
                base.OverridesDefaultStyle = value;
            }
        }
               

        /// <summary>
        /// Gets or sets the center point of any possible render transform declared by 
        /// RenderTransform, relative to the bounds of the element.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Point RenderTransformOrigin
        {
            get
            {
                return base.RenderTransformOrigin;
            }
            set
            {
                base.RenderTransformOrigin = value;
            }
        }

        /// <summary>
        /// Gets or sets the locally-defined resource dictionary.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new ResourceDictionary Resources 
        {
            get
            {
                return base.Resources;
            }
            set
            {
                base.Resources = value;
            }
        }

        #endregion Properties
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