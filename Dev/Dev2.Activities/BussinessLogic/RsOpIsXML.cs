﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;

namespace Dev2.DataList
{
    /// <summary>
    /// Class for the "is xml" recordset search option 
    /// </summary>
    public class RsOpIsXML : AbstractRecsetSearchValidation
    {
        public override Func<IList<string>> BuildSearchExpression(IBinaryDataList scopingObj, IRecsetSearch to)
        {
            // Default to a null function result
            Func<IList<string>> result = () => { return null; };

            result = () =>
            {
                ErrorResultTO err = new ErrorResultTO();
                IList<RecordSetSearchPayload> operationRange = GenerateInputRange(to, scopingObj, out err).Invoke();
                IList<string> fnResult = new List<string>();

                foreach(RecordSetSearchPayload p in operationRange)
                {

                    if(p.Payload.IsXml())
                    {
                        fnResult.Add(p.Index.ToString(CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        if(to.RequireAllFieldsToMatch)
                        {
                            return new List<string>();
                        }
                    }
                }

                return fnResult.Distinct().ToList();
            };


            return result;
        }

        public override string HandlesType()
        {
            return "Is XML";
        }
    }
}
