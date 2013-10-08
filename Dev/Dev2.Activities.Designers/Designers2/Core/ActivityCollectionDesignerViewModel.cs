﻿using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Data;
using Dev2.Activities.Designers2.Core.Converters;
using Dev2.Activities.Designers2.Core.QuickVariableInput;

namespace Dev2.Activities.Designers2.Core
{
    public abstract class ActivityCollectionDesignerViewModel : ActivityDesignerViewModel
    {
        bool _isToggleCheckedChanged;

        protected ActivityCollectionDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            QuickVariableInputViewModel = new QuickVariableInputViewModel(AddToCollection);

            BindingOperations.SetBinding(QuickVariableInputViewModel, QuickVariableInputViewModel.IsClosedProperty, new Binding(ShowQuickVariableInputProperty.Name)
            {
                Source = this,
                Mode = BindingMode.TwoWay,
                Converter = new NegateBooleanConverter()
            });

            BindingOperations.SetBinding(this, ErrorsProperty, new Binding(QuickVariableInputViewModel.ErrorsProperty.Name)
            {
                Source = QuickVariableInputViewModel,
                Mode = BindingMode.TwoWay
            });
        }

        public ModelItemCollection ModelItemCollection { get; protected set; }

        public abstract void OnSelectionChanged(ModelItem oldItem, ModelItem newItem);

        public abstract void UpdateDisplayName();

        public abstract string CollectionName { get; }

        public abstract bool CanRemoveAt(int indexNumber);

        public abstract bool CanInsertAt(int indexNumber);

        public abstract void RemoveAt(int indexNumber);

        public abstract void InsertAt(int indexNumber);

        protected abstract void AddToCollection(IEnumerable<string> source, bool overWrite);

        public QuickVariableInputViewModel QuickVariableInputViewModel
        {
            get { return (QuickVariableInputViewModel)GetValue(QuickVariableInputViewModelProperty); }
            set { SetValue(QuickVariableInputViewModelProperty, value); }
        }

        public static readonly DependencyProperty QuickVariableInputViewModelProperty =
            DependencyProperty.Register("QuickVariableInputViewModel", typeof(QuickVariableInputViewModel), typeof(ActivityCollectionDesignerViewModel), new PropertyMetadata(null));

        public bool ShowQuickVariableInput
        {
            get { return (bool)GetValue(ShowQuickVariableInputProperty); }
            set { SetValue(ShowQuickVariableInputProperty, value); }
        }

        public static readonly DependencyProperty ShowQuickVariableInputProperty =
            DependencyProperty.Register("ShowQuickVariableInput", typeof(bool), typeof(ActivityCollectionDesignerViewModel), new PropertyMetadata(false, OnTitleBarToggleChanged));

        public override bool ShowSmall
        {
            get
            {
                return base.ShowSmall && !ShowQuickVariableInput;
            }
        }

        public override void Collapse()
        {
            ShowQuickVariableInput = false;
            base.Collapse();
        }

        public override void Restore()
        {
            if(PreviousView == ShowQuickVariableInputProperty.Name)
            {
                ShowLarge = false;
                ShowQuickVariableInput = true;
            }
            else
            {
                base.Restore();
            }
        }

        protected override void OnToggleCheckedChanged(string propertyName, bool isChecked)
        {
            if(!_isToggleCheckedChanged)
            {
                if(propertyName == ShowLargeProperty.Name)
                {
                    _isToggleCheckedChanged = true;
                    ShowQuickVariableInput = false;
                }
                else if(propertyName == ShowQuickVariableInputProperty.Name)
                {
                    if(PreviousView == ShowLargeProperty.Name)
                    {
                        _isToggleCheckedChanged = true;
                        ShowLarge = false;
                    }
                }
                _isToggleCheckedChanged = false;

                base.OnToggleCheckedChanged(propertyName, isChecked);
            }
        }

        protected void AddTitleBarQuickVariableInputToggle()
        {
            var toggle = ActivityDesignerToggle.Create(
                collapseImageSourceUri: "pack://application:,,,/Dev2.Activities.Designers;component/Images/ServiceQuickVariableInput-32.png",
                collapseToolTip: "Close Quick Variable Input",
                expandImageSourceUri: "pack://application:,,,/Dev2.Activities.Designers;component/Images/ServiceQuickVariableInput-32.png",
                expandToolTip: "Open Quick Variable Input",
                automationID: "QuickVariableInputToggle",
                target: this,
                dp: ShowQuickVariableInputProperty
                );
            TitleBarToggles.Add(toggle);
        }
    }
}
