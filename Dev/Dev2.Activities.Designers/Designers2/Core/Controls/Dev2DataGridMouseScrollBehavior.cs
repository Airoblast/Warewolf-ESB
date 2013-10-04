﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using Dev2.UI;

namespace Dev2.Activities.Designers2.Core.Controls
{
    public class Dev2DataGridMouseScrollBehavior : Behavior<Dev2DataGrid>
    {
        #region Override Methods

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.SelectionChanged -= AssociatedObject_SelectionChanged;
            AssociatedObject.PreviewMouseWheel -= AssociatedObject_doCustomScroll;
            AssociatedObject.Unloaded -= AssociatedObjectOnUnloaded;

            AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;
            AssociatedObject.PreviewMouseWheel += AssociatedObject_doCustomScroll;
            AssociatedObject.Unloaded += AssociatedObjectOnUnloaded;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.SelectionChanged -= AssociatedObject_SelectionChanged;
            AssociatedObject.PreviewMouseWheel -= AssociatedObject_doCustomScroll;
            AssociatedObject.Unloaded -= AssociatedObjectOnUnloaded;
        }

        #endregion Override Methods

        #region Attached Behaviours

        #region TargetElement

        public FrameworkElement TargetElement
        {
            get { return (FrameworkElement)GetValue(TargetElementProperty); }
            set { SetValue(TargetElementProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TargetElement.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TargetElementProperty =
            DependencyProperty.Register("TargetElement", typeof(FrameworkElement), typeof(Dev2DataGridMouseScrollBehavior), new PropertyMetadata(null));

        #endregion TargetElement

        #endregion Attached Behaviours

        #region Event Handlers

        private void AssociatedObjectOnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            AssociatedObject.SelectionChanged -= AssociatedObject_SelectionChanged;
            AssociatedObject.PreviewMouseWheel -= AssociatedObject_doCustomScroll;
            AssociatedObject.Unloaded -= AssociatedObjectOnUnloaded;
        }

        void AssociatedObject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(sender is Dev2DataGrid)
            {
                var grid = (sender as Dev2DataGrid);
                if(grid.SelectedItem != null && grid.SelectedIndex != -1)
                {
                    grid.Dispatcher.BeginInvoke(
                        new Action(() =>
                            {
                                grid.UpdateLayout();
                                try
                                {
                                    if(e.RemovedItems != null && e.AddedItems != null && !Equals(e.RemovedItems, e.AddedItems)) grid.ScrollIntoView(grid.SelectedItem);
                                }
                                catch(NullReferenceException)
                                {
                                    //Todo log exception
                                    //throw subtypedexception - ExpectedException or something
                                }
                            }));
                }
            }
        }

        private void AssociatedObject_doCustomScroll(object sender, MouseWheelEventArgs e)
        {
            var theGrid = (sender as Dev2DataGrid);

            if(theGrid == null)
            {
                return;
            }

            if(e.Delta > 0 && theGrid.SelectedIndex >= 0)
            {
                theGrid.Focus();
                theGrid.SelectedIndex--;
            }
            else if(e.Delta < 0 && theGrid.SelectedIndex < theGrid.Items.Count)
            {
                theGrid.Focus();
                theGrid.SelectedIndex++;
            }
            else
            {
                // Mouse was not scrolled
                return;
            }
            if(theGrid.SelectedIndex < 0) theGrid.SelectedIndex = 0;
            else if(theGrid.SelectedIndex > theGrid.Items.Count) theGrid.SelectedIndex = theGrid.Items.Count;// Out of bounds not allowed
        }

        #endregion Event Handlers
    }
}
