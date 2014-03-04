﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.Serialization;
using System.Text;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;
using Newtonsoft.Json;

namespace Dev2.Runtime.ESB.Management.Services
{
    // NOTE: Only use for design time in studio as errors will NOT be forwarded!
    public class GetDatabaseTables : IEsbManagementEndpoint
    {
        #region Implementation of ISpookyLoadable<string>

        public string HandlesType()
        {
            return "GetDatabaseTablesService";
        }

        #endregion

        #region Implementation of IEsbManagementEndpoint

        /// <summary>
        /// Executes the service
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="theWorkspace">The workspace.</param>
        /// <returns></returns>
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();

            if(values == null)
            {
                throw new InvalidDataContractException("No parameter values provided.");
            }
            string database = null;
            StringBuilder tmp;
            values.TryGetValue("Database", out tmp);
            if(tmp != null)
            {
                database = tmp.ToString();
            }

            if(string.IsNullOrEmpty(database))
            {
                var res = new DbTableList("No database set.");
                return serializer.SerializeToBuilder(res);
            }

            DbSource dbSource;
            try
            {
                dbSource = JsonConvert.DeserializeObject<DbSource>(database);
                if(dbSource.ResourceID != Guid.Empty)
                {
                    dbSource = ResourceCatalog.Instance.GetResource<DbSource>(theWorkspace.ID, dbSource.ResourceID);
                }
            }
            catch(Exception e)
            {
                var res = new DbTableList("Invalid JSON data for Database parameter. Exception: {0}", e.Message);
                return serializer.SerializeToBuilder(res);
            }

            if(string.IsNullOrEmpty(dbSource.DatabaseName) || string.IsNullOrEmpty(dbSource.Server))
            {
                var res = new DbTableList("Invalid database sent {0}.", database);
                return serializer.SerializeToBuilder(res);
            }

            try
            {
                var tables = new DbTableList();
                DataTable columnInfo;
                using(var connection = new SqlConnection(dbSource.ConnectionString))
                {
                    connection.Open();
                    columnInfo = connection.GetSchema("Tables");
                }
                if(columnInfo != null)
                {
                    foreach(DataRow row in columnInfo.Rows)
                    {
                        var tableName = row["TABLE_NAME"] as string;
                        var dbTable = tables.Items.Find(table => table.TableName == tableName);
                        if(dbTable == null)
                        {
                            dbTable = new DbTable { TableName = tableName, Columns = new List<DbColumn>() };
                            tables.Items.Add(dbTable);
                        }
                    }
                }
                if(tables.Items.Count == 0)
                {
                    tables.HasErrors = true;
                    const string ErrorFormat = "The login provided in the database source uses {0} and most probably does not have permissions to perform the following query: "
                                          + "\r\n\r\n{1}SELECT * FROM INFORMATION_SCHEMA.TABLES;{2}";

                    if(dbSource.AuthenticationType == AuthenticationType.User)
                    {
                        tables.Errors = string.Format(ErrorFormat,
                            "SQL Authentication (User: '" + dbSource.UserID + "')",
                            "EXECUTE AS USER = '" + dbSource.UserID + "';\r\n",
                            "\r\nREVERT;");
                    }
                    else
                    {
                        tables.Errors = string.Format(ErrorFormat, "Windows Authentication", "", "");
                    }
                }
                return serializer.SerializeToBuilder(tables);
            }
            catch(Exception ex)
            {
                var tables = new DbTableList(ex);
                return serializer.SerializeToBuilder(tables);
            }
        }

        /// <summary>
        /// Creates the service entry.
        /// </summary>
        /// <returns></returns>
        public DynamicService CreateServiceEntry()
        {
            var ds = new DynamicService
            {
                Name = HandlesType(),
                DataListSpecification = "<DataList><Database ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>"
            };

            var sa = new ServiceAction
            {
                Name = HandlesType(),
                ActionType = enActionType.InvokeManagementDynamicService,
                SourceMethod = HandlesType()
            };

            ds.Actions.Add(sa);

            return ds;
        }

        #endregion
    }
}
