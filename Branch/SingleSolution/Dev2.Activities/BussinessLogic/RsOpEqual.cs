﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;

namespace Dev2.DataList
{
    /// <summary>
    /// Class for the "=" recordset search option 
    /// </summary>
    public class RsOpEqual : AbstractRecsetSearchValidation
    {

        public RsOpEqual()
        {

        }

        public override Func<IList<string>> BuildSearchExpression(IBinaryDataList scopingObj, IRecsetSearch to)
        {
            // Default to a null function result
            Func<IList<string>> result = () => { return null; };

            result = () => {
                ErrorResultTO err = new ErrorResultTO();
                IList<RecordSetSearchPayload> operationRange = GenerateInputRange(to, scopingObj, out err).Invoke();
                IList<string> fnResult = new List<string>();

                string toFind = to.SearchCriteria.Trim();
                string toFindLower = toFind.ToLower();

                foreach (RecordSetSearchPayload p in operationRange)
                {
                    string toMatch = p.Payload.Trim();

                    if (to.MatchCase)
                    {
                        if (toMatch.Equals(toFind, StringComparison.CurrentCulture))
                        {
                            fnResult.Add(p.Index.ToString());
                        }
                        else
                        {
                            if(to.RequireAllFieldsToMatch)
                            {
                                return new List<string>();
                            }
                        }
                    }
                    else
                    {
                        if (toMatch.ToLower().Equals(toFindLower, StringComparison.CurrentCulture))
                        {
                            fnResult.Add(p.Index.ToString());
                        }
                        else
                        {
                            if(to.RequireAllFieldsToMatch)
                            {
                                return new List<string>();
                            }
                        }
                    }
                    
                }

                return fnResult.Distinct().ToList();
            };


            return result;
        }

        public override string HandlesType()
        {
            return "=";
        }
    }
}
