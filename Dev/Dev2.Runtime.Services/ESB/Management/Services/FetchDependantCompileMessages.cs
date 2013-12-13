﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Dev2.Communication;
using Dev2.Data.Enums;
using Dev2.Data.ServiceModel.Messages;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Internal service to fetch compile time messages
    /// </summary>
    public class FetchDependantCompileMessages : IEsbManagementEndpoint
    {
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            string serviceID = null;
            string workspaceID = null;
            string filterList = null;

            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            var result = new ExecuteMessage() { HasError = false };

            StringBuilder tmp;
            values.TryGetValue("ServiceID", out tmp);
            if(tmp != null)
            {
                serviceID = tmp.ToString();
            }
            values.TryGetValue("WorkspaceID", out tmp);
            if(tmp != null)
            {
                workspaceID = tmp.ToString();
            }
            values.TryGetValue("FilterList", out tmp);
            if(tmp != null)
            {
                filterList = tmp.ToString();
            }

            if(string.IsNullOrEmpty(serviceID) || string.IsNullOrEmpty(workspaceID))
            {
                throw new InvalidDataContractException("Null or empty ServiceID or WorkspaceID");
            }

            Guid wGuid;
            Guid sGuid;

            Guid.TryParse(workspaceID, out wGuid);
            Guid.TryParse(serviceID, out sGuid);


            var thisService = ResourceCatalog.Instance.GetResource(wGuid, sGuid);

            if(thisService != null)
            {
                var deps = ResourceCatalog.Instance.GetDependants(wGuid, thisService.ResourceName);

                CompileMessageType[] filters = null; // TODO : Convert string list to enum array ;)
                if(deps.Count > 0)
                {
                    CompileMessageList msgs = CompileMessageRepo.Instance.FetchMessages(wGuid, sGuid, deps, filters);
                    return serializer.SerializeToBuilder(msgs);

                }
            }
            else
            {
                result.Message.Append("Could not locate service with ID [ " + sGuid + " ]");
            }

            return serializer.SerializeToBuilder(result);
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService newDs = new DynamicService();
            newDs.Name = HandlesType();
            newDs.DataListSpecification = "<DataList><ServiceID/><WorkspaceID/><FilterList/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>";
            ServiceAction sa = new ServiceAction();
            sa.Name = HandlesType();
            sa.ActionType = enActionType.InvokeManagementDynamicService;
            sa.SourceMethod = HandlesType();
            newDs.Actions.Add(sa);

            return newDs;
        }

        public string HandlesType()
        {
            return "FetchDependantCompileMessagesService";
        }
    }
}