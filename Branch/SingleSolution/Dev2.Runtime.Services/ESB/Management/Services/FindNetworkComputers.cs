﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Common.Common;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Find Computers service
    /// </summary>
    class FindNetworkComputers : IEsbManagementEndpoint
    {

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            StringBuilder result = new StringBuilder();
            string json = "[";
            try
            {
                var computers = GetComputerNames.ComputerNames;
                // DirectoryEntry with WinNT: was timing out, swapped to using a NetworkBrowser...
                json = computers.Cast<object>().Aggregate(json, (current, comp) => current + (@"{""ComputerName"":""" + comp + @"""},"));
                json += "]";
                json = json.Replace(",]", "]"); // remove last comma
                result.Append("<JSON>");
                result.Append(json);
                result.Append("</JSON>");
            }
            catch(Exception ex)
            {
                result.Append(ex.Message);
            }

            return result;
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService findNetworkComputersService = new DynamicService();
            findNetworkComputersService.Name = HandlesType();
            findNetworkComputersService.DataListSpecification = "<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>";

            ServiceAction findNetworkComputersAction = new ServiceAction();
            findNetworkComputersAction.Name = HandlesType();
            findNetworkComputersAction.ActionType = enActionType.InvokeManagementDynamicService;
            findNetworkComputersAction.SourceMethod = HandlesType();
            

            findNetworkComputersService.Actions.Add(findNetworkComputersAction);

            return findNetworkComputersService;
        }

        public string HandlesType()
        {
            return "FindNetworkComputersService";
        }
    }


    
}
