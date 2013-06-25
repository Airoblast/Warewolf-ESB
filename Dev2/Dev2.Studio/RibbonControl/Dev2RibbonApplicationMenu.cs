﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using Microsoft.Windows.Controls.Ribbon;

namespace Dev2.Studio.RibbonControl
{
    public class Dev2RibbonApplicationMenu : RibbonApplicationMenu
    {
        public bool ShowAuxilaryPanel
        {
            get { return (bool)GetValue(ShowAuxilaryPanelProperty); }
            set { SetValue(ShowAuxilaryPanelProperty, value); }
        }

        public static readonly DependencyProperty ShowAuxilaryPanelProperty =
            DependencyProperty.Register("ShowAuxilaryPanel", typeof(bool),
            typeof(Dev2RibbonApplicationMenu), new UIPropertyMetadata(true));

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.DropDownOpened += Dev2RibbonApplicationMenu_DropDownOpened;
        }

        void Dev2RibbonApplicationMenu_DropDownOpened(object sender, EventArgs e)
        {
            DependencyObject popupObj = GetTemplateChild("PART_Popup");
            Popup panel = (Popup)popupObj;
            if (panel != null)
            {
                var width = panel.GetBindingExpression(Popup.WidthProperty);

                if (!ShowAuxilaryPanel && width == null)
                {
                    ScrollViewer panelArea = (ScrollViewer)base.GetTemplateChild("PART_SubMenuScrollViewer");
                    //DependencyObject auxPanel = base.GetTemplateChild("PART_AuxiliaryPaneContentPresenter");

                    //if (auxPanel != null)
                    //{
                    //    ContentPresenter cp = (ContentPresenter)auxPanel;

                    //    cp.VerticalAlignment = VerticalAlignment.Top;
                    //    cp.HorizontalAlignment = HorizontalAlignment.Left;
                    //    Binding vis = new Binding("VerticalAlignment") { Source = cp, Mode = BindingMode.OneTime };
                    //    Binding vis1 = new Binding("HorizontalAlignment") { Source = cp, Mode = BindingMode.OneTime };
                    //    cp.SetBinding(ContentPresenter.VisibilityProperty, vis);
                    //    cp.SetBinding(ContentPresenter.VerticalAlignmentProperty, vis);
                    //    cp.SetBinding(ContentPresenter.HorizontalAlignmentProperty, vis1);
                    //    panel.SetBinding(Popup.VisibilityProperty, vis);
                    //}

                    var panelBindingWidth = new Binding("ActualWidth")
                    {
                        Source = panelArea,
                        Mode = BindingMode.OneWay
                    };

                    panel.SetBinding(Popup.WidthProperty, panelBindingWidth);
                }
                else if (ShowAuxilaryPanel && width != null)
                {
                    BindingOperations.ClearBinding(panel, Popup.WidthProperty);
                }
                
            }
        }
    }
}
