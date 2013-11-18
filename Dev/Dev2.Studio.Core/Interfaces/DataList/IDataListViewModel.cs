﻿using System;
using System.Windows.Input;
using Caliburn.Micro;
using Dev2.DataList.Contract;
using System.Collections.Generic;

namespace Dev2.Studio.Core.Interfaces.DataList
{
    public interface IDataListViewModel : IScreen, IChild, IDisposable
    {
        IResourceModel Resource { get; }
        ICommand FindUnusedAndMissingCommand { get; }

        OptomizedObservableCollection<IDataListItemModel> ScalarCollection { get;}

        OptomizedObservableCollection<IDataListItemModel> RecsetCollection { get;}

        OptomizedObservableCollection<IDataListItemModel> DataList { get; }

        /// <summary>
        /// Removes the data list item.
        /// </summary>
        /// <param name="itemToRemove">The item to remove.</param>
        /// <author>Massimo.Guerrera</author>
        /// <date>2/21/2013</date>
        void RemoveDataListItem(IDataListItemModel itemToRemove);

        /// <summary>
        /// Sets the unused data list items.
        /// </summary>
        /// <param name="parts">The parts.</param>
        /// <author>Massimo.Guerrera</author>
        /// <date>2/20/2013</date>
        void SetUnusedDataListItems(IList<IDataListVerifyPart> parts);

        /// <summary>
        /// Initializes the data list view model.
        /// </summary>
        /// <param name="resourceModel">The resource model.</param>
        void InitializeDataListViewModel(IResourceModel resourceModel);

        void InitializeDataListViewModel();
      
        /// <summary>
        /// Adds the blank row.
        /// </summary>
        /// <param name="item">The item.</param>
        void AddBlankRow(IDataListItemModel item);

        /// <summary>
        /// Removes the blank rows.
        /// </summary>
        /// <param name="item">The item.</param>
        void RemoveBlankRows(IDataListItemModel item);

        /// <summary>
        /// Writes the data list to the resource model.
        /// </summary>
        string WriteToResourceModel();

       
        /// <summary>
        /// Adds the recordset names if missing.
        /// </summary>
        void AddRecordsetNamesIfMissing();

        /// <summary>
        /// Adds the missing data list items.
        /// </summary>
        /// <param name="parts">The parts.</param>
        void AddMissingDataListItems(IList<IDataListVerifyPart> parts);

        /// <summary>
        /// Removes the unused data list items.
        /// </summary>     
        void RemoveUnusedDataListItems();

        /// <summary>
        /// Validates the names.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="addItem"></param>
        void ValidateNames(IDataListItemModel item);

        /// <summary>
        /// Finds the missing workflow data regions.
        /// </summary>
        /// <param name="partsToVerify">The parts to verify.</param>
        /// <returns></returns>
        List<IDataListVerifyPart> MissingWorkflowItems(IList<IDataListVerifyPart> partsToVerify, bool excludeUnusedItems = false);

        List<IDataListVerifyPart> MissingDataListParts(IList<IDataListVerifyPart> partsToVerify);
        List<IDataListVerifyPart> UpdateDataListItems(IResourceModel contextualResourceModel, IList<IDataListVerifyPart> workflowFields);

        /// <summary>
        ///     Creates the list of data list item view model to bind to.
        /// </summary>
        /// <param name="errorString">The error string.</param>
        /// <returns></returns>
        void CreateListsOfIDataListItemModelToBindTo(out string errorString);

        void ClearCollections();
    }
}
