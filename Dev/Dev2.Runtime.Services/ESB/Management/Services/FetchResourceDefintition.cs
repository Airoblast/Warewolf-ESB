﻿using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common.Common;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Fetch a service body definition
    /// </summary>
    public class FetchResourceDefintition : IEsbManagementEndpoint
    {
        private static string payloadStart = "<XamlDefinition>";
        private static string payloadEnd = "</XamlDefinition>";
        private static string altPayloadStart = "<Actions>";
        private static string altPayloadEnd = "</Actions>";

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {

            var res = new ExecuteMessage {HasError = false};

            string serviceID = null;
            StringBuilder tmp;
            values.TryGetValue("ResourceID", out tmp);
            
            if (tmp != null)
            {
                serviceID = tmp.ToString();
            }

            Guid resourceID;
            Guid.TryParse(serviceID, out resourceID);

            var result = ResourceCatalog.Instance.GetResourceContents(theWorkspace.ID, resourceID);
            var startIdx = result.IndexOf(payloadStart, 0, false);

            if(startIdx >= 0)
            {
                // remove begingin junk
                startIdx += payloadStart.Length;
                result = result.Remove(0, startIdx);

                startIdx = result.IndexOf(payloadEnd,0, false);

                if(startIdx > 0)
                {
                    var len = result.Length - startIdx;
                    result = result.Remove(startIdx, len);
                    
                    res.Message.Append(result.Unescape());
                }
            }
            else
            {
                // handle services ;)
                startIdx = result.IndexOf(altPayloadStart, 0, false);
                if (startIdx >= 0)
                {
                    // remove begingin junk
                    startIdx += altPayloadStart.Length;
                    result = result.Remove(0, startIdx);

                    startIdx = result.IndexOf(altPayloadEnd, 0, false);

                    if (startIdx > 0)
                    {
                        var len = result.Length - startIdx;
                        result = result.Remove(startIdx, len);

                        res.Message.Append(result.Unescape());
                    }
                }
                else
                {
                    // send the entire thing ;)
                    res.Message.Append(result);
                }
            }

            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            return serializer.SerializeToBuilder(res);
        }

        public DynamicService CreateServiceEntry()
        {
            var serviceAction = new ServiceAction { Name = HandlesType(), SourceMethod = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService };

            var serviceEntry = new DynamicService { Name = HandlesType(), DataListSpecification = "<DataList><ResourceID/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>" };
            serviceEntry.Actions.Add(serviceAction);

            return serviceEntry;
        }

        public string HandlesType()
        {
            return "FetchResourceDefinitionService";
        }

    }
}
