﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Deploy a resource
    /// </summary>
    public class DeployResource : IEsbManagementEndpoint
    {
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            StringBuilder resourceDefinition;

            values.TryGetValue("ResourceDefinition", out resourceDefinition);
            Dev2Logger.Log.Info(String.Format("Deploy Resource."));
            if(resourceDefinition == null || resourceDefinition.Length == 0)
            {
                Dev2Logger.Log.Info(String.Format("Roles or ResourceDefinition missing"));
                throw new InvalidDataContractException("Roles or ResourceDefinition missing");
            }

            var msg = ResourceCatalog.Instance.SaveResource(WorkspaceRepository.ServerWorkspaceID, resourceDefinition,null,"Deploy","unknown");
            WorkspaceRepository.Instance.RefreshWorkspaces();

            var result = new ExecuteMessage { HasError = false };
            result.SetMessage(msg.Message);
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            return serializer.SerializeToBuilder(result);
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService deployResourceDynamicService = new DynamicService { Name = HandlesType(), DataListSpecification = "<DataList><ResourceDefinition ColumnIODirection=\"Input\"/><Roles ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>" };

            ServiceAction deployResourceServiceAction = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };

            deployResourceDynamicService.Actions.Add(deployResourceServiceAction);

            return deployResourceDynamicService;
        }

        public string HandlesType()
        {
            return "DeployResourceService";
        }
    }
}
