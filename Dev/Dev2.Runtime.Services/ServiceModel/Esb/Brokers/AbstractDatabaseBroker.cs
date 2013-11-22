﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Sql;
using Unlimited.Framework.Converters.Graph;
using Unlimited.Framework.Converters.Graph.Interfaces;

namespace Dev2.Runtime.ServiceModel.Esb.Brokers
{
    public abstract class AbstractDatabaseBroker<TDbServer>
        where TDbServer : class, IDbServer, new()
    {
        #region TheCache

        // ReSharper disable StaticFieldInGenericType
        //
        // This means that the values of 
        //      AbstractDatabaseBroker<DbServer1>.TheCache 
        //      AbstractDatabaseBroker<DbServer2>.TheCache 
        // will have completely different, independent values.
        //
        public static ConcurrentDictionary<string, ServiceMethodList> TheCache = new ConcurrentDictionary<string, ServiceMethodList>();
        //
        // ReSharper restore StaticFieldInGenericType

        #endregion
        
        public virtual List<string> GetDatabases(DbSource dbSource)
        {
            VerifyArgument.IsNotNull("dbSource", dbSource);
            using(var server = CreateDbServer())
            {
                server.Connect(dbSource.ConnectionString);
                return server.FetchDatabases();
            }
        }

        public virtual ServiceMethodList GetServiceMethods(DbSource dbSource)
        {
            VerifyArgument.IsNotNull("dbSource", dbSource);

            // Check the cache for a value ;)
            ServiceMethodList cacheResult;
            if(!dbSource.ReloadActions)
            {
                if(GetCachedResult(dbSource, out cacheResult))
                {
                    return cacheResult;
                }
            }
            // else reload actions ;)

            var serviceMethods = new ServiceMethodList();

            //
            // Function to handle procedures returned by the data broker
            //
            Func<IDbCommand, IList<IDbDataParameter>, string, bool> procedureFunc = (command, parameters, helpText) =>
            {
                var serviceMethod = CreateServiceMethod(command, parameters, helpText);
                serviceMethods.Add(serviceMethod);
                return true;
            };

            //
            // Function to handle functions returned by the data broker
            //
            Func<IDbCommand, IList<IDbDataParameter>, string, bool> functionFunc = (command, parameters, helpText) =>
            {
                var serviceMethod = CreateServiceMethod(command, parameters, helpText);
                serviceMethods.Add(serviceMethod);
                return true;
            };

            //
            // Get stored procedures and functions for this database source
            //
            using(var server = CreateDbServer())
            {
                server.Connect(dbSource.ConnectionString);
                server.FetchStoredProcedures(procedureFunc, functionFunc);
            }

            // Add to cache ;)
            TheCache.AddOrUpdate(dbSource.ConnectionString, serviceMethods,(s, list) => serviceMethods);

            return GetCachedResult(dbSource, out cacheResult) ? cacheResult : serviceMethods;
        }

        bool GetCachedResult(DbSource dbSource, out ServiceMethodList cacheResult)
        {
            TheCache.TryGetValue(dbSource.ConnectionString, out cacheResult);
            if(cacheResult != null)
            {
                return true;
            }
            return false;
        }

        public virtual IOutputDescription TestService(DbService dbService)
        {
            VerifyArgument.IsNotNull("dbService", dbService);
            VerifyArgument.IsNotNull("dbService.Source", dbService.Source);

            IOutputDescription result;
            using(var server = CreateDbServer())
            {
                server.Connect(((DbSource)dbService.Source).ConnectionString);
                server.BeginTransaction();
                try
                {
                    //
                    // Execute command and normalize XML
                    //
                    var command = CommandFromServiceMethod(server, dbService.Method);
                    var dataTable = server.FetchDataTable(command);

                    //
                    // Map shape of XML
                    //
                    result = OutputDescriptionFactory.CreateOutputDescription(OutputFormats.ShapedXML);
                    var dataSourceShape = DataSourceShapeFactory.CreateDataSourceShape();
                    result.DataSourceShapes.Add(dataSourceShape);

                    var dataBrowser = DataBrowserFactory.CreateDataBrowser();
                    dataSourceShape.Paths.AddRange(dataBrowser.Map(dataTable));
                }
                finally
                {
                    server.RollbackTransaction();
                }
            }

            return result;
        }

        protected virtual TDbServer CreateDbServer()
        {
            return new TDbServer();
        }

        protected virtual string NormalizeXmlPayload(string payload)
        {
            //
            // Unescape '<>' characters delimiting
            //
            return (payload.Replace("&lt;", "<").Replace("&gt;", ">"));
        }

        static string GetXML(DataTable dataTable)
        {
            DataTable dtCloned = dataTable.Clone();
            foreach(DataColumn dc in dtCloned.Columns)
                dc.DataType = typeof(string);
            foreach(DataRow row in dataTable.Rows)
            {
                dtCloned.ImportRow(row);
            }

            foreach(DataRow row in dtCloned.Rows)
            {
                for(int i = 0; i < dtCloned.Columns.Count; i++)
                {
                    dtCloned.Columns[i].ReadOnly = false;

                    if(string.IsNullOrEmpty(row[i].ToString()))
                        row[i] = string.Empty;
                }
            }
            dtCloned.TableName = "Table";
            using(var stringWriter = new StringWriter())
            {
                dtCloned.WriteXml(stringWriter);
                return stringWriter.ToString();
            }
        }

        static ServiceMethod CreateServiceMethod(IDbCommand command, IEnumerable<IDataParameter> parameters, string sourceCode)
        {
            return new ServiceMethod(command.CommandText, sourceCode, parameters.Select(MethodParameterFromDataParameter), null, null);
        }

        static MethodParameter MethodParameterFromDataParameter(IDataParameter parameter)
        {
            return new MethodParameter
            {
                Name = parameter.ParameterName.Replace("@", "")
            };
        }

        static IDbCommand CommandFromServiceMethod(TDbServer server, ServiceMethod serviceMethod)
        {
            var command = server.CreateCommand();

            command.CommandText = serviceMethod.Name;
            command.CommandType = CommandType.StoredProcedure;

            foreach(var methodParameter in serviceMethod.Parameters)
            {
                var dataParameter = DataParameterFromMethodParameter(command, methodParameter);
                command.Parameters.Add(dataParameter);
            }

            return command;
        }

        static IDbDataParameter DataParameterFromMethodParameter(IDbCommand command, MethodParameter methodParameter)
        {
            var parameter = command.CreateParameter();

            parameter.ParameterName = string.Format("@{0}", methodParameter.Name);
            parameter.Value = methodParameter.Value;

            return parameter;
        }

    }
}
