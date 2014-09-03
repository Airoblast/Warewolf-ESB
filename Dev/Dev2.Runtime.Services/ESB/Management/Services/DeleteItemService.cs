﻿using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Hosting;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Explorer;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class DeleteItemService : IEsbManagementEndpoint
    {
        private IExplorerServerResourceRepository _serverExplorerRepository;

        public string HandlesType()
        {
            return "DeleteItemService";
        }

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            IExplorerRepositoryResult item;
            var serializer = new Dev2JsonSerializer();
            try
            {
                if(values == null)
                {
                    throw new ArgumentNullException("values");
                }               
                StringBuilder itemBeingDeleted;
                if(!values.TryGetValue("itemToDelete", out itemBeingDeleted))
                {
                    throw new ArgumentException("itemToDelete value not supplied.");
                }
                var itemToDelete = serializer.Deserialize<ServerExplorerItem>(itemBeingDeleted);
                Dev2Logger.Log.Info("Delete Item Service."+itemToDelete);
                item = ServerExplorerRepo.DeleteItem(itemToDelete, GlobalConstants.ServerWorkspaceID);
            }
            catch(Exception e)
            {
                Dev2Logger.Log.Error("Delete Item Error" ,e);
                item = new ExplorerRepositoryResult(ExecStatus.Fail, e.Message);
            }
            return serializer.SerializeToBuilder(item);
        }

        public DynamicService CreateServiceEntry()
        {
            var findServices = new DynamicService { Name = HandlesType(), DataListSpecification = "<DataList><itemToAdd ColumnIODirection=\"itemToDelete\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>" };

            var fetchItemsAction = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };

            findServices.Actions.Add(fetchItemsAction);

            return findServices;
        }
        public IExplorerServerResourceRepository ServerExplorerRepo
        {
            get { return _serverExplorerRepository ?? ServerExplorerRepository.Instance; }
            set { _serverExplorerRepository = value; }
        }
    }
}
