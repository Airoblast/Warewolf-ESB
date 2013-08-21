﻿using Dev2.Data.Binary_Objects;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.Models.DataList;

namespace Dev2.Studio.Core.Factories
{
    public static class DataListItemModelFactory
    {
        //public static IDataListItemModel CreateDataListItemViewModel(IDataListViewModel dataListViewModel)
        //{
        //    return new DataListItemModel(dataListViewModel);
        //}

        //public static IDataListItemModel CreateDataListItemViewModel(IDataListViewModel dataListViewModel, bool isRoot)
        //{
        //    return new DataListItemModel(dataListViewModel) { IsRoot = isRoot };
        //}

        public static IDataListItemModel CreateDataListItemViewModel(IDataListViewModel dataListViewModel, IDataListItemModel parent)
        {
            return CreateDataListItemViewModel(dataListViewModel, string.Empty, string.Empty, parent, true);
        }

        public static IDataListItemModel CreateDataListItemViewModel(IDataListViewModel dataListViewModel, string name, string description, IDataListItemModel parent)
        {
            return CreateDataListItemViewModel(dataListViewModel, name, description, parent, true);
        }

        public static IDataListItemModel CreateDataListItemViewModel(IDataListViewModel dataListViewModel, string name, string description, IDataListItemModel parent, bool isEditable = true, enDev2ColumnArgumentDirection dev2ColumnArgumentDirection = enDev2ColumnArgumentDirection.None)
        {
            IDataListItemModel dataListModel = CreateDataListModel(name);            
            dataListModel.Description = description;
            dataListModel.Parent = parent;
            dataListModel.IsExpanded = true;   
            dataListModel.IsEditable = isEditable;
            return dataListModel;
        }

        public static IDataListItemModel CreateDataListModel(string displayname, string description, enDev2ColumnArgumentDirection dev2ColumnArgumentDirection = enDev2ColumnArgumentDirection.None, IDataListItemModel parent = null, OptomizedObservableCollection<IDataListItemModel> children = null, bool hasError = false, string errorMessage = "", bool isEditable = true, bool isVisable = true, bool isSelected = false)
        {
            IDataListItemModel dataListModel = new DataListItemModel(displayname, dev2ColumnArgumentDirection, description, parent, children, hasError, errorMessage, isEditable, isVisable, isSelected);
            return dataListModel;
        }
        
        public static IDataListItemModel CreateDataListModel(string displayname, string description = "", IDataListItemModel parent = null, OptomizedObservableCollection<IDataListItemModel> children = null, bool hasError = false, string errorMessage = "", bool isEditable = true, bool isVisable = true, bool isSelected = false, enDev2ColumnArgumentDirection dev2ColumnArgumentDirection = enDev2ColumnArgumentDirection.None)
        {
            IDataListItemModel dataListModel = new DataListItemModel(displayname, dev2ColumnArgumentDirection, description, parent, children, hasError, errorMessage, isEditable, isVisable, isSelected);
            return dataListModel;
        }

        public static DataListHeaderItemModel CreateDataListHeaderItem(string displayname)
        {
            DataListHeaderItemModel dataListHeaderModel = new DataListHeaderItemModel(displayname);
            return dataListHeaderModel;
        }
    }

}
