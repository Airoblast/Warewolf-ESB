﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Xml;
using Dev2.Common;
using Dev2.Common.Enums;
using Dev2.Data.Translators;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.TO;
using Dev2.DataList.Contract.Translators;

// ReSharper disable CheckNamespace
namespace Dev2.Server.DataList.Translators
// ReSharper restore CheckNamespace
{
    /// <summary>
    /// The standard DataList translator used in the system
    /// </summary>
    internal sealed class DataListXMLTranslator : IDataListTranslator
    {
        const string RootTag = "DataList";
        private readonly DataListFormat _format;
        private readonly Encoding _encoding;


        public DataListFormat Format { get { return _format; } }
        public Encoding TextEncoding { get { return _encoding; } }

        public DataListXMLTranslator()
        {
            _format = DataListFormat.CreateFormat(GlobalConstants._XML);
            _encoding = Encoding.UTF8;
        }

        public DataListFormat HandlesType()
        {
            return _format;
        }

        public DataListTranslatedPayloadTO ConvertFrom(IBinaryDataList payload, out ErrorResultTO errors)
        {
            if(payload == null)
            {
                throw new ArgumentNullException("payload");
            }

            TranslatorUtils tu = new TranslatorUtils();

            StringBuilder result = new StringBuilder("<" + RootTag + ">");
            errors = new ErrorResultTO();

            var itemKeys = payload.FetchAllKeys();

            foreach(string key in itemKeys)
            {
                IBinaryDataListEntry entry;
                string error;
                if(payload.TryGetEntry(key, out entry, out error))
                {

                    if(entry.IsRecordset)
                    {
                        var idxItr = entry.FetchRecordsetIndexes();

                        while(idxItr.HasMore() && !entry.IsEmpty())
                        {

                            while(idxItr.HasMore())
                            {

                                int i = idxItr.FetchNextIndex();

                                IList<IBinaryDataListItem> rowData = entry.FetchRecordAt(i, out error);
                                errors.AddError(error);
                                result.Append("<");
                                result.Append(entry.Namespace);
                                result.Append(">");

                                foreach(IBinaryDataListItem col in rowData)
                                {
                                    string fName = col.FieldName;

                                    result.Append("<");
                                    result.Append(fName);
                                    result.Append(">");

                                    // Travis.Frisinger 04.02.2013
                                    if(!col.IsDeferredRead)
                                    {
                                        result.Append(tu.CleanForEmit(col.TheValue));
                                    }
                                    else
                                    {
                                        // deferred read, just print the location
                                        result.Append(!string.IsNullOrEmpty(col.TheValue) ? col.FetchDeferredLocation() : string.Empty);
                                    }
                                    result.Append("</");
                                    result.Append(fName);
                                    result.Append(">");
                                }

                                result.Append("</");
                                result.Append(entry.Namespace);
                                result.Append(">");
                            }
                        }
                    }
                    else
                    {
                        string fName = entry.Namespace;
                        IBinaryDataListItem val = entry.FetchScalar();
                        if(val != null)
                        {
                            result.Append("<");
                            result.Append(fName);
                            result.Append(">");
                            // Travis.Frisinger 04.02.2013
                            if(!val.IsDeferredRead)
                            {
                                // Dev2System.FormView is our html region, pass it by ;)
                                result.Append(!entry.IsManagmentServicePayload ? tu.CleanForEmit(val.TheValue) : val.TheValue);
                            }
                            else
                            {
                                // deferred read, just print the location
                                result.Append(val.FetchDeferredLocation());
                            }
                            result.Append("</");
                            result.Append(fName);
                            result.Append(">");
                        }
                    }
                }

            }

            result.Append("</" + RootTag + ">");

            DataListTranslatedPayloadTO tmp = new DataListTranslatedPayloadTO(result.ToString());

            return tmp;
        }

        public IBinaryDataList ConvertTo(byte[] input, string targetShape, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            string payload = Encoding.UTF8.GetString(input);

            IBinaryDataList result = new BinaryDataList();
            TranslatorUtils tu = new TranslatorUtils();

            // build shape
            if(targetShape == null)
            {
                errors.AddError("Null payload or shape");
            }
            else
            {
                ErrorResultTO invokeErrors;
                result = tu.TranslateShapeToObject(targetShape, true, out invokeErrors);
                errors.MergeErrors(invokeErrors);

                // populate the shape 
                if(payload != string.Empty)
                {
                    try
                    {
                        string toLoad = DataListUtil.StripCrap(payload); // clean up the rubish ;)
                        XmlDocument xDoc = new XmlDocument();

                        // BUG 9626 - 2013.06.11 - TWR: ensure our DocumentElement
                        toLoad = string.Format("<Tmp{0}>{1}</Tmp{0}>", Guid.NewGuid().ToString("N"), toLoad);
                        xDoc.LoadXml(toLoad);

                        if(xDoc.DocumentElement != null)
                        {
                            XmlNodeList children = xDoc.DocumentElement.ChildNodes;

                            IDictionary<string, int> indexCache = new Dictionary<string, int>();

                            // BUG 9626 - 2013.06.11 - TWR: refactored for recursion
                            TryConvert(children, result, indexCache, errors);
                        }

                        //// Transfer System Tags
                        for(int i = 0; i < TranslationConstants.systemTags.Length; i++)
                        {
                            string key = TranslationConstants.systemTags.GetValue(i).ToString();
                            string query = String.Concat("//", key);
                            XmlNode n = xDoc.SelectSingleNode(query);

                            // try system namespace tags ;)
                            if(n == null)
                            {
                                var values = "//" + DataListUtil.BuildSystemTagForDataList(key, false);
                                query = values;
                                n = xDoc.SelectSingleNode(query);
                            }

                            if(n != null && !string.IsNullOrEmpty(n.InnerXml))
                            {
                                string bkey = DataListUtil.BuildSystemTagForDataList(key, false);
                                string error;
                                IBinaryDataListEntry sysEntry;
                                if(result.TryGetEntry(bkey, out sysEntry, out error))
                                {
                                    sysEntry.TryPutScalar(Dev2BinaryDataListFactory.CreateBinaryItem(n.InnerXml, bkey), out error);
                                }
                            }
                        }
                    }
                    catch(Exception e)
                    {
                        // if use passed in empty input they only wanted the shape ;)
                        if(input.Length > 0)
                        {
                            errors.AddError(e.Message);
                        }
                    }
                }
            }
            return result;
        }

        public IBinaryDataList ConvertTo(object input, string shape, out ErrorResultTO errors)
        {
            throw new NotImplementedException();
        }

        public Guid Populate(object input, Guid targetDl, string outputDefs, out ErrorResultTO errors)
        {
            throw new NotImplementedException();
        }

        // BUG 9626 - 2013.06.11 - TWR: refectored for recursion
        static void TryConvert(XmlNodeList children, IBinaryDataList result, IDictionary<string, int> indexCache, ErrorResultTO errors, int level = 0)
        {
            // spin through each element in the XML
            foreach(XmlNode c in children)
            {
                if(!DataListUtil.IsSystemTag(c.Name) && c.Name != GlobalConstants.NaughtyTextNode)
                {
                    // scalars and recordset fetch
                    IBinaryDataListEntry entry;
                    string error;
                    if(result.TryGetEntry(c.Name, out entry, out error))
                    {
                        if(entry.IsRecordset)
                        {
                            // fetch recordset index
                            int fetchIdx;
                            var idx = indexCache.TryGetValue(c.Name, out fetchIdx) ? fetchIdx : 1;
                            // process recordset
                            var nl = c.ChildNodes;
                            foreach(XmlNode subc in nl)
                            {
                                entry.TryPutRecordItemAtIndex(Dev2BinaryDataListFactory.CreateBinaryItem(subc.InnerXml, c.Name, subc.Name, idx), idx, out error);

                                if(!string.IsNullOrEmpty(error))
                                {
                                    errors.AddError(error);
                                }
                            }
                            // update this recordset index
                            indexCache[c.Name] = ++idx;
                        }
                        else
                        {
                            // process scalar
                            entry.TryPutScalar(Dev2BinaryDataListFactory.CreateBinaryItem(c.InnerXml, c.Name), out error);

                            if(!string.IsNullOrEmpty(error))
                            {
                                errors.AddError(error);
                            }
                        }
                    }
                    else
                    {
                        if(level == 0)
                        {
                            // Only recurse if we're at the first level!!
                            TryConvert(c.ChildNodes, result, indexCache, errors, ++level);
                        }
                        else
                        {
                            errors.AddError(error);
                        }
                    }
                }
            }
        }

        public string ConvertAndFilter(IBinaryDataList input, string filterShape, out ErrorResultTO errors)
        {
            throw new NotImplementedException();
        }

        public DataTable ConvertToDataTable(IBinaryDataList input, string recsetName, out ErrorResultTO errors, PopulateOptions populateOptions)
        {
            errors = null;
            throw new NotImplementedException();
        }
    }
}
