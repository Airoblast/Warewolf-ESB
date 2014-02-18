﻿using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Find resources by type
    /// </summary>
    public class FindSourcesByType : IEsbManagementEndpoint
    {
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            string type = null;
            StringBuilder tmp;
            values.TryGetValue("Type", out tmp);
            if(tmp != null)
            {
                type = tmp.ToString();
            }

            if(string.IsNullOrEmpty(type))
            {
                // ReSharper disable NotResolvedInText
                throw new ArgumentNullException("type");
                // ReSharper restore NotResolvedInText
            }

            enSourceType sourceType;
            if(Enum.TryParse(type, true, out sourceType))
            {
                // TODO : Based upon the enum type return correct JSON model ;)
                // NOTE : Current types are : Email, SqlDatabase, Dev2Server

                // BUG 7850 - TWR - 2013.03.11 - ResourceCatalog re-factor
                var result = ResourceCatalog.Instance.GetModels(theWorkspace.ID, sourceType);
                if(result != null)
                {
                    Dev2JsonSerializer serializer = new Dev2JsonSerializer();
                    return serializer.SerializeToBuilder(result);
                }
            }

            return new StringBuilder();
        }

        public DynamicService CreateServiceEntry()
        {
            var findSourcesByTypeAction = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };

            var findSourcesByTypeService = new DynamicService { Name = HandlesType(), DataListSpecification = "<DataList><Type ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>" };
            findSourcesByTypeService.Actions.Add(findSourcesByTypeAction);

            return findSourcesByTypeService;
        }

        public string HandlesType()
        {
            return "FindSourcesByType";
        }
    }
}
