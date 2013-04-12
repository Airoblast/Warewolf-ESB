﻿using System;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Common.ServiceModel;
using Dev2.DynamicServices;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Webs.Callbacks;

namespace Dev2.Studio.Webs
{
    public static class RootWebSite
    {
        public const string SiteName = "wwwroot";

        #region ShowDialog(IContextualResourceModel resourceModel)

        //
        // TWR: 2013.02.14 - refactored to accommodate new requests
        // PBI: 801
        // BUG: 8477
        //
        public static bool ShowDialog(IContextualResourceModel resourceModel)
        {
            if(resourceModel == null)
            {
                return false;
            }

            string resourceID;
            var resourceType = ResourceType.Unknown;

            if(string.IsNullOrEmpty(resourceModel.ServiceDefinition))
            {
                resourceID = Guid.Empty.ToString();
                if(resourceModel.IsDatabaseService)
                {
                    resourceType = ResourceType.DbService;
                }
                else
                {
                    Enum.TryParse(resourceModel.DisplayName,out resourceType);
                }
            }
            else
            {
                var resourceXml = XElement.Parse(resourceModel.ServiceDefinition);

                resourceID = resourceXml.AttributeSafe("ID");
                if(string.IsNullOrEmpty(resourceID))
                {
                    resourceID = resourceXml.AttributeSafe("Name");
                }
                Enum.TryParse(resourceXml.AttributeSafe("ResourceType"), out resourceType);

                if(resourceType == ResourceType.Unknown)
                {
                    #region Try determine resourceType of 'old' resources

                    if(resourceXml.Name.LocalName.Equals("Source", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if(resourceXml.AttributeSafe("Type").Equals(enSourceType.Dev2Server.ToString(), StringComparison.InvariantCultureIgnoreCase))
                        {
                            resourceType = ResourceType.Server;
                        }
                        else if (resourceXml.AttributeSafe("Type").Equals(enSourceType.SqlDatabase.ToString(), StringComparison.InvariantCultureIgnoreCase))
                        {
                            // Since the resource is outdated we use the name instead                            
                            resourceID = resourceXml.AttributeSafe("Name");
                            resourceType = ResourceType.DbSource;
                        }
                    }
                    else if(resourceXml.Name.LocalName.Equals("Service", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if(resourceXml.ElementSafe("TypeOf").Equals(enActionType.InvokeStoredProc.ToString(), StringComparison.InvariantCultureIgnoreCase))
                        {
                            // DynamicServicesHost auto-adds an ID to each service if it doesn't exist in the persisted XML
                            // Since the resource is read from disk rather than DynamicServicesHost we use the name instead                            
                            resourceID = resourceXml.AttributeSafe("Name");
                            resourceType = ResourceType.DbService;
                        }
                    }

                    #endregion

                }
            }

            return ShowDialog(resourceModel.Environment, resourceType, resourceID);
        }

        #endregion

        #region ShowDialog(IEnvironmentModel environment, ResourceType resourceType, string resourceID = null)

        public static bool ShowDialog(IEnvironmentModel environment, ResourceType resourceType, string resourceID = null)
        {
            if(environment == null)
            {
                throw new ArgumentNullException("environment");
            }

            string pageName;
            WebsiteCallbackHandler pageHandler;
            double width;
            double height;
            var workspaceID = ((IStudioClientContext)environment.DsfChannel).AccountID;

            switch(resourceType)
            {
                case ResourceType.Server:
                    workspaceID = GlobalConstants.ServerWorkspaceID; // MUST always save to the server!
                    pageName = "sources/server";
                    pageHandler = new ConnectCallbackHandler();
                    width = 705;
                    height = 492;
                    break;

                case ResourceType.DbService:
                    pageName = "services/dbservice";
                    pageHandler = new DbServiceCallbackHandler();
                    width = 941;
                    height = 562;
                    break;

                case ResourceType.DbSource:
                    pageName = "sources/dbsource";
                    pageHandler = new DbSourceCallbackHandler();
                    width = 705;
                    height = 455;
                    break;

                default:
                    return false;
            }

            environment.ShowWebPageDialog(SiteName, string.Format("{0}?wid={1}&rid={2}", pageName, workspaceID, resourceID), pageHandler, width, height);
            return true;
        }

        #endregion
    }
}
