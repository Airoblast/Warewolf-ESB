﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Dev2.Studio.Core.ViewModels.Navigation;

namespace Dev2.CustomControls.Behavior
{
    /// <summary>
    /// Exposes attached behaviors that can be
    /// applied to TreeViewItem objects.
    /// </summary>
    public class BringIntoViewWhenSelectedBehavior : Behavior<TreeViewItem>
    {
        private Storyboard _storyBoard;
        private DispatcherTimer _timer;
        private TreeViewItem _item;

        protected override void OnAttached()
        {
            base.OnAttached();
            _storyBoard = Application.Current.Resources["TreeViewFocusOnAddStoryBoard"] as Storyboard;
            AssociatedObject.Selected += OnTreeViewItemSelected;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Selected -= OnTreeViewItemSelected;
            base.OnDetaching();
        }

        #region IsBroughtIntoViewWhenSelected

        private void OnTreeViewItemSelected(object sender, RoutedEventArgs e)
        {
            //TODO - yeah, _item not associatedObject.
            //THis has to do with the original source getting lost somewhere along the way.
            //Look into this.

            // Only react to the Selected event raised by the TreeViewItem
            // whose IsSelected property was modified. Ignore all ancestors
            // who are merely reporting that a descendant's Selected fired.
            _item = e.OriginalSource as TreeViewItem;
            if (_item == null)
            {
                return;
            }

            if (!ReferenceEquals(sender, e.OriginalSource))
            {
                if (!_item.IsLoaded)
                {
                    _item.Loaded -= ItemInitialized;
                    _item.Loaded += ItemInitialized;
                }
                return;
            }

            BringIntoView(_item);
        }

        private void ItemInitialized(object sender, EventArgs e)
        {
            //TODO - yeah, _item not associatedObject.
            //THis has to do with the original source getting lost somewhere along the way.
            //Look into this.
            _item = sender as TreeViewItem;
            if (_item == null)
            {
                return;
            }


            BringIntoView(_item);

        }
        
        private void BringIntoView(TreeViewItem item)
        {
            var treeNode = _item.DataContext as ITreeNode;
            if (treeNode == null)
            {
                return;
            }

            item.BringIntoView();

            if (!treeNode.IsNew)
            {
                return;
            }

            //TODO find a better way to do this (instead of waiting halve a second and using a dispatchertimer)
            //We need to know when the item as been scrolled into view, and then fire the animation
            _timer = new DispatcherTimer
                {
                    Interval = new TimeSpan(0, 0, 0, 0, 1000)
                };
            _timer.Tick += (sender, e) =>
                {
                    _item.BeginStoryboard(_storyBoard);
                    _timer.Stop();
                    _timer = null;
                    treeNode.IsNew = false;
                };
            _timer.Start();
        }
        #endregion IsBroughtIntoViewWhenSelected
    }
}
