﻿using Dev2.Data.Binary_Objects;
using System;
using System.Collections.Generic;

namespace Dev2.DataList.Contract.Binary_Objects
{
    public static class Dev2BinaryDataListFactory
    {
        // ReSharper disable InconsistentNaming
        private static readonly IBinaryDataListItem _blankBinarnyDataListItem = CreateBinaryItem(string.Empty, string.Empty);
        // ReSharper restore InconsistentNaming


        /// <summary>
        /// Gets the blank binary data list item.
        /// </summary>
        /// <returns></returns>
        public static IBinaryDataListItem GetBlankBinaryDataListItem()
        {
            return _blankBinarnyDataListItem;
        }

        /// <summary>
        /// Creates the looped index iterator.
        /// </summary>
        /// <param name="val">The val.</param>
        /// <param name="cnt">The CNT.</param>
        /// <returns></returns>
        public static IIndexIterator CreateLoopedIndexIterator(int val, int cnt)
        {
            return new LoopedIndexIterator(val, cnt);
        }

        /// <summary>
        /// Creates the data list.
        /// </summary>
        /// <returns></returns>
        public static IBinaryDataList CreateDataList()
        {
            return new BinaryDataList();
        }

        /// <summary>
        /// Creates the data list
        /// </summary>
        /// <param name="parentID">The parent ID.</param>
        /// <returns></returns>
        public static IBinaryDataList CreateDataList(Guid parentID)
        {
            return new BinaryDataList(parentID);
        }

        /// <summary>
        /// Creates the binary item.
        /// </summary>
        /// <param name="val">The val.</param>
        /// <param name="field">The field.</param>
        /// <returns></returns>
        public static IBinaryDataListItem CreateBinaryItem(string val, string field)
        {
            return new BinaryDataListItem(val, field);
        }

        /// <summary>
        /// Creates the binary item.
        /// </summary>
        /// <param name="val">The val.</param>
        /// <param name="ns">The namespace ( aka recordset )</param>
        /// <param name="field">The field.</param>
        /// <param name="idx">The idx.</param>
        /// <returns></returns>
        public static IBinaryDataListItem CreateBinaryItem(string val, string ns, string field, string idx)
        {
            return new BinaryDataListItem(val, ns, field, idx);
        }

        /// <summary>
        /// Creates the binary item.
        /// </summary>
        /// <param name="val">The val.</param>
        /// <param name="ns">The ns.</param>
        /// <param name="field">The field.</param>
        /// <param name="idx">The idx.</param>
        /// <returns></returns>
        public static IBinaryDataListItem CreateBinaryItem(string val, string ns, string field, int idx)
        {
            return new BinaryDataListItem(val, ns, field, idx);
        }


        /// <summary>
        /// Creates the file system item.
        /// </summary>
        /// <param name="base64Obj">The base64 obj.</param>
        /// <param name="fileLoc">The file loc.</param>
        /// <param name="ns">The ns.</param>
        /// <param name="field">The field.</param>
        /// <param name="idx">The idx.</param>
        /// <returns></returns>
        public static IBinaryDataListItem CreateFileSystemItem(string base64Obj, string fileLoc, string ns, string field, int idx)
        {
            return new BinaryDataListFileSystemItem(base64Obj, fileLoc, ns, field, idx);
        }

        /// <summary>
        /// Creates the file system item.
        /// </summary>
        /// <param name="base64Obj">The base64 obj.</param>
        /// <param name="fileLoc">The file loc.</param>
        /// <param name="field">The field.</param>
        /// <returns></returns>
        public static IBinaryDataListItem CreateFileSystemItem(string base64Obj, string fileLoc, string field)
        {
            return new BinaryDataListFileSystemItem(base64Obj, fileLoc, field);
        }


        /// <summary>
        /// Creates the column.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="desc">The desc.</param>
        /// <param name="ioDir">The io dir.</param>
        /// <returns></returns>
        public static Dev2Column CreateColumn(string name, string desc, enDev2ColumnArgumentDirection ioDir)
        {
            return new Dev2Column(name, desc, ioDir);
        }

        /// <summary>
        /// Creates the column.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="ioDir">The io dir.</param>
        /// <returns></returns>
        public static Dev2Column CreateColumn(string name, enDev2ColumnArgumentDirection ioDir)
        {
            return new Dev2Column(name, ioDir);
        }

        /// <summary>
        /// Creates the column.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="desc">The desc.</param>
        /// <returns></returns>
        public static Dev2Column CreateColumn(string name, string desc)
        {
            return new Dev2Column(name, desc);
        }


        /// <summary>
        /// Creates the column.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static Dev2Column CreateColumn(string name)
        {
            return new Dev2Column(name, string.Empty);
        }


        /// <summary>
        /// Creates the entry.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="desc">The desc.</param>
        /// <param name="dataListKey">The parent ID of the DataList used as part of the key for the items.</param>
        /// <returns></returns>
        public static IBinaryDataListEntry CreateEntry(string field, string desc, Guid dataListKey)
        {
            return new BinaryDataListEntry(field, desc, dataListKey);
        }

        public static IBinaryDataListEntry CreateEntry(string field, string desc, IList<Dev2Column> cols, Guid dataListKey)
        {
            return new BinaryDataListEntry(field, desc, cols, dataListKey);
        }

    }
}
