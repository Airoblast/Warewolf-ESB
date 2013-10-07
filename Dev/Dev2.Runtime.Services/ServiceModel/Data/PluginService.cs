﻿using System;
using System.Linq;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Data.ServiceModel;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;

namespace Dev2.Runtime.ServiceModel.Data
{
    public class PluginService : Service
    {
        // BUG 9500 - 2013.05.31 - TWR : removed Recordset property
        public RecordsetList Recordsets { get; set; }

        // BUG 9500 - 2013.05.31 - TWR : added
        public string Namespace { get; set; }

        #region CTOR

        public PluginService()
        {
            ResourceID = Guid.Empty;
            ResourceType = ResourceType.PluginService;
            Source = new PluginSource();
            Recordsets = new RecordsetList();
            Method = new ServiceMethod();
        }

        public PluginService(XElement xml)
            : base(xml)
        {
            ResourceType = ResourceType.PluginService;
            var action = xml.Descendants("Action").FirstOrDefault();
            if(action == null)
            {
                return;
            }

            // BUG 9500 - 2013.05.31 - TWR : added
            Namespace = action.AttributeSafe("Namespace");

            // Handle old service this is not set
            // We also need to redo wizards to correctly return defaults and mappings ;)
            if (string.IsNullOrEmpty(Namespace))
            {
                var mySource = action.AttributeSafe("SourceName");

                // Now look up the old source and fetch namespace ;)
                var services = ResourceCatalog.Instance.GetDynamicObjects<Source>(GlobalConstants.ServerWorkspaceID, mySource);

                var tmp = services.FirstOrDefault();

                if (tmp != null)
                {
                    Namespace = tmp.AssemblyName;
                }
            }

            Source = CreateSource<PluginSource>(action);
            Method = CreateInputsMethod(action);
            Recordsets = CreateOutputsRecordsetList(action);
        }

        #endregion

        #region ToXml

        // BUG 9500 - 2013.05.31 - TWR : refactored
        public override XElement ToXml()
        {
            var result = CreateXml(enActionType.Plugin, Source, Recordsets,
                new XAttribute("Namespace", Namespace ?? string.Empty)
                );
            return result;
        }

        #endregion
    }
}