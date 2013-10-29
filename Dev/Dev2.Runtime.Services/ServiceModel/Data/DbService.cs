﻿using System;
using System.Linq;
using System.Xml.Linq;
using Dev2.Data.ServiceModel;
using Dev2.DynamicServices;

namespace Dev2.Runtime.ServiceModel.Data
{
    public class DbService : Service
    {
        public Recordset Recordset { get; set; }

        #region CTOR

        public DbService()
        {
            ResourceType = ResourceType.DbService;
            Source = new DbSource();
            Recordset = new Recordset();
        }

        public DbService(XElement xml)
            : base(xml)
        {
            ResourceType = ResourceType.DbService;
            var action = xml.Descendants("Action").FirstOrDefault();
            if(action == null)
            {
                return;
            }

            Source = CreateSource<WebSource>(action);
            Method = CreateInputsMethod(action);

            var recordSets = CreateOutputsRecordsetList(action);
            Recordset = recordSets.FirstOrDefault() ?? new Recordset { Name = action.AttributeSafe("Name") };

            if(String.IsNullOrEmpty(Recordset.Name))
            {
                Recordset.Name = Method.Name;
            }
        }

        #endregion

        #region ToXml

        public override XElement ToXml()
        {
            var actionName = Recordset == null || Recordset.Name == null ? string.Empty : Recordset.Name;
            var result = CreateXml(enActionType.InvokeStoredProc, actionName, Source, new RecordsetList { Recordset });
            return result;
        }

        #endregion

        #region Create

        public static DbService Create()
        {
            var result = new DbService
            {
                ResourceID = Guid.Empty,
                Source = { ResourceID = Guid.Empty },
            };
            return result;
        }

        #endregion


    }
}