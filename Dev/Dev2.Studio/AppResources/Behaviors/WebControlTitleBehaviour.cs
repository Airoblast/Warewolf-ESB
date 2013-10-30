﻿
using CefSharp.Wpf;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Interactivity;

namespace Dev2.Studio.AppResources.Behaviors
{
    public class WebControlTitleBehaviour : Behavior<WebView>, IDisposable
    {
        #region Override Methods

        protected override void OnAttached()
        {
            base.OnAttached();
            SubscribeToEvents();
        }

        protected override void OnDetaching()
        {
            UnsubscribeFromEvents();
            base.OnDetaching();
        }

        #endregion

        #region Dependency Properties

        #region Title

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(WebControlTitleBehaviour), new PropertyMetadata(""));

        #endregion Title

        #endregion

        #region Private Methods

        private void SubscribeToEvents()
        {
            AssociatedObject.Unloaded += AssociatedObjectOnUnloaded;
            AssociatedObject.PropertyChanged += OnWebControlProperteryChanged;
        }

        private void UnsubscribeFromEvents()
        {
            AssociatedObject.Unloaded -= AssociatedObjectOnUnloaded;
            AssociatedObject.PropertyChanged -= OnWebControlProperteryChanged;
        }

        private void OnWebControlProperteryChanged(object sender, PropertyChangedEventArgs args)
        {
            switch(args.PropertyName)
            {
                case "Title":
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        Title = AssociatedObject.Title;
                    }));
                    break;
            }
        }

        private void AssociatedObjectOnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            UnsubscribeFromEvents();
            routedEventArgs.Handled = true;
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            UnsubscribeFromEvents();
        }

        #endregion Methods
    }
}
