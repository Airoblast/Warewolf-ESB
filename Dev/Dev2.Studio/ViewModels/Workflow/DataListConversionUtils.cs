using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dev2.Data.Binary_Objects;
using Dev2.Data.Interfaces;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Studio.Core;

namespace Dev2.ViewModels.Workflow
{
    public class DataListConversionUtils
    {
        public OptomizedObservableCollection<IDataListItem> CreateListToBindTo(IBinaryDataList dataList)
        {
            var result = new OptomizedObservableCollection<IDataListItem>();

            if(dataList != null)
            {
                var listOfEntries = dataList.FetchAllEntries();

                foreach(var entry in listOfEntries
                    .Where(e => (e.ColumnIODirection == enDev2ColumnArgumentDirection.Input ||
                                 e.ColumnIODirection == enDev2ColumnArgumentDirection.Both)))
                {
                    result.AddRange(ConvertIBinaryDataListEntryToIDataListItem(entry));
                }
            }

            return result;
        }

        IList<IDataListItem> ConvertIBinaryDataListEntryToIDataListItem(IBinaryDataListEntry dataListEntry)
        {
            IList<IDataListItem> result = new List<IDataListItem>();
            if(dataListEntry.IsRecordset)
            {
                var sizeOfCollection = dataListEntry.ItemCollectionSize();
                if(sizeOfCollection == 0) { sizeOfCollection++; }
                var count = 0;

                while(count < sizeOfCollection)
                {
                    string error;
                    var items = dataListEntry.FetchRecordAt(count + 1, out error);
                    foreach(var item in items)
                    {
                        IDataListItem singleRes = new DataListItem();
                        singleRes.IsRecordset = true;
                        singleRes.Recordset = item.Namespace;
                        singleRes.Field = item.FieldName;
                        singleRes.RecordsetIndex = (count + 1).ToString(CultureInfo.InvariantCulture);
                        singleRes.Value = item.TheValue;

                        singleRes.DisplayValue = item.DisplayValue;
                        var desc = dataListEntry.Columns.FirstOrDefault(c => c.ColumnName == item.FieldName);
                        singleRes.Description = desc == null ? null : desc.ColumnDescription;
                        result.Add(singleRes);
                    }
                    count++;
                }
            }
            else
            {
                var item = dataListEntry.FetchScalar();
                if(item != null)
                {
                    IDataListItem singleRes = new DataListItem();
                    singleRes.IsRecordset = false;
                    singleRes.Field = item.FieldName;
                    singleRes.DisplayValue = item.FieldName;
                    singleRes.Value = item.TheValue;
                    var desc = dataListEntry.Description;
                    singleRes.Description = string.IsNullOrWhiteSpace(desc) ? null : desc;
                    result.Add(singleRes);
                }
            }
            return result;
        }
    }
}