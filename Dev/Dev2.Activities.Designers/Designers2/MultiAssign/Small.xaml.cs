﻿
using System.Windows;
using System.Windows.Controls;

namespace Dev2.Activities.Designers2.MultiAssign
{
    public partial class Small
    {
        public Small()
        {
            InitializeComponent();
            DataGrid = SmallDataGrid;
        }

        protected override IInputElement GetInitialFocusElement()
        {
            return DataGrid.GetFocusElement(0);
        }

        private void DataGrid_LoadingRow(System.Object sender, DataGridRowEventArgs e)
        {
            e.Row.Tag = e.Row.GetIndex();
        }
    }
}
