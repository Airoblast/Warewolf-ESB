﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Dev2.Common;

namespace Dev2.Services.Sql
{
    public class SqlServer : IDbServer
    {
        SqlConnection _connection;
        SqlCommand _command;
        SqlTransaction _transaction;

        public bool IsConnected { get { return _connection != null && _connection.State == ConnectionState.Open; } }

        public string ConnectionString { get { return _connection == null ? null : _connection.ConnectionString; } }

        #region Connect

        public bool Connect(string connectionString)
        {
            _connection = CreateConnection(connectionString);
            _connection.Open();
            return true;
        }

        public bool Connect(string connectionString, CommandType commandType, string commandText)
        {
            _connection = CreateConnection(connectionString);

            VerifyArgument.IsNotNull("commandText", commandText);
            _command = CreateCommand(_connection, commandType, commandText);

            _connection.Open();
            return true;
        }

        #endregion

        public IDbCommand CreateCommand()
        {
            VerifyConnection();
            var command = _connection.CreateCommand();
            command.Transaction = _transaction;
            return command;
        }

        public void BeginTransaction()
        {
            if(IsConnected)
            {
                _transaction = _connection.BeginTransaction();
            }
        }

        public void RollbackTransaction()
        {
            if(_transaction != null)
            {
                _transaction.Rollback();
                _transaction.Dispose();
                _transaction = null;
            }
        }

        #region FetchDatabases

        public List<string> FetchDatabases()
        {
            VerifyConnection();

            const string DatabaseColumnName = "database_name";

            var databases = GetSchema(_connection, "Databases");

            // 2013.07.10 - BUG 9933 - AL - sort database list
            var orderedRows = databases.Select("", DatabaseColumnName);

            var result = orderedRows.Select(row => (row[DatabaseColumnName] ?? string.Empty).ToString()).Distinct().ToList();

            return result;
        }

        #endregion

        #region FetchDataTable

        public DataTable FetchDataTable(params IDbDataParameter[] parameters)
        {
            VerifyConnection();
            AddParameters(_command, parameters);
            return FetchDataTable(_command);
        }

        public DataTable FetchDataTable(IDbCommand command)
        {
            VerifyArgument.IsNotNull("command", command);

            return ExecuteReader(command, (CommandBehavior.SchemaOnly & CommandBehavior.KeyInfo),
                delegate(IDataReader reader)
                {
                    var table = new DataTable();
                    table.Load(reader, LoadOption.OverwriteChanges);
                    return table;
                });
        }

        #endregion

        #region FetchDataSet

        public DataSet FetchDataSet(params SqlParameter[] parameters)
        {
            VerifyConnection();
            return FetchDataSet(_command, parameters);
        }

        public DataSet FetchDataSet(SqlCommand command, params SqlParameter[] parameters)
        {
            VerifyArgument.IsNotNull("command", command);
            AddParameters(command, parameters);

            using(var dataSet = new DataSet())
            {
                using(var adapter = new SqlDataAdapter(command))
                {
                    adapter.Fill(dataSet);
                }
                return dataSet;
            }
        }

        #endregion

        #region FetchStoredProcedures

        public void FetchStoredProcedures(Func<IDbCommand, List<IDbDataParameter>, string, bool> procedureProcessor, Func<IDbCommand, List<IDbDataParameter>, string, bool> functionProcessor, bool continueOnProcessorException = false)
        {
            VerifyArgument.IsNotNull("procedureProcessor", procedureProcessor);
            VerifyArgument.IsNotNull("functionProcessor", functionProcessor);
            VerifyConnection();

            var proceduresDataTable = GetSchema(_connection, "Procedures");
            var procedureDataColumn = GetDataColumn(proceduresDataTable, "ROUTINE_NAME");
            var procedureTypeColumn = GetDataColumn(proceduresDataTable, "ROUTINE_TYPE");
            var procedureSchemaColumn = GetDataColumn(proceduresDataTable, "SPECIFIC_SCHEMA"); // ROUTINE_CATALOG - ROUTINE_SCHEMA ,SPECIFIC_SCHEMA

            foreach(DataRow row in proceduresDataTable.Rows)
            {
                var fullProcedureName = GetFullProcedureName(row, procedureDataColumn, procedureSchemaColumn);
                
                using(var command = CreateCommand(_connection, CommandType.StoredProcedure, fullProcedureName))
                {
                    try
                    {
                        var parameters = GetProcedureParameters(command);

                        var helpText = GetHelpText(_connection, fullProcedureName);

                        if(IsStoredProcedure(row, procedureTypeColumn))
                        {
                            procedureProcessor(command, parameters, helpText);
                        }
                        else if(IsFunction(row, procedureTypeColumn))
                        {
                            functionProcessor(command, parameters, helpText);
                        }
                    }
                    catch(Exception)
                    {
                        if(!continueOnProcessorException)
                        {
                            throw;
                        }
                    }
                }
            }
        }

        #endregion

        #region VerifyConnection

        void VerifyConnection()
        {
            if(!IsConnected)
            {
                throw new Exception("Please connect first.");
            }
        }

        #endregion

        static T ExecuteReader<T>(IDbCommand command, CommandBehavior commandBehavior, Func<IDataReader, T> handler)
        {
            try
            {
                using(var reader = command.ExecuteReader(commandBehavior))
                {
                    return handler(reader);
                }
            }
            catch(SqlException e)
            {
                if(e.Message.Contains("There is no text for object "))
                {
                    var exceptionDataTable = new DataTable("Error");
                    exceptionDataTable.Columns.Add("ErrorText");
                    exceptionDataTable.LoadDataRow(new object[] { e.Message }, true);
                    return handler(new DataTableReader(exceptionDataTable));
                }
                throw;
            }
        }

        static SqlConnection CreateConnection(string connectionString)
        {
            VerifyArgument.IsNotNull("connectionString", connectionString);

            return new SqlConnection(connectionString);
        }

        static SqlCommand CreateCommand(SqlConnection connection, CommandType commandType, string commandText)
        {
            return new SqlCommand(commandText, connection)
            {
                CommandType = commandType,
                CommandTimeout = (int)GlobalConstants.TransactionTimeout.TotalSeconds,                
                
            };
        }

        static void AddParameters(IDbCommand command, ICollection<IDbDataParameter> parameters)
        {
            command.Parameters.Clear();
            if(parameters != null && parameters.Count > 0)
            {
                foreach(var parameter in parameters)
                {
                    command.Parameters.Add(parameter);
                }
            }
        }

        static DataTable GetSchema(SqlConnection connection, string collectionName)
        {
            return connection.GetSchema(collectionName);
        }

        static string GetHelpText(SqlConnection connection, string objectName)
        {
            using(var command = CreateCommand(connection, CommandType.Text, string.Format("sp_helptext '{0}'", objectName)))
            {
                return ExecuteReader(command, (CommandBehavior.SchemaOnly & CommandBehavior.KeyInfo),
                    delegate(IDataReader reader)
                    {
                        var sb = new StringBuilder();
                        while(reader.Read())
                        {
                            var value = reader.GetValue(0);
                            if(value != null)
                            {
                                sb.Append(value);
                            }
                        }
                        return sb.ToString();
                    });
            }
        }

        static DataColumn GetDataColumn(DataTable dataTable, string columnName)
        {
            var dataColumn = dataTable.Columns[columnName];
            if(dataColumn == null)
            {
                throw new Exception(string.Format("SQL Server - Unable to load '{0}' column of '{1}'.", columnName, dataTable.TableName));
            }
            return dataColumn;
        }

        static string GetFullProcedureName(DataRow row, DataColumn procedureDataColumn, DataColumn procedureSchemaColumn)
        {
            var procedureName = row[procedureDataColumn].ToString();
            var schemaName = row[procedureSchemaColumn].ToString();
            return schemaName + "." + procedureName;
        }

        List<IDbDataParameter> GetProcedureParameters(SqlCommand command)
        {
            //Please do not use SqlCommandBuilder.DeriveParameters(command); as it does not handle CLR procedures correctly.
            var originalCommandText = command.CommandText;
            var parameters = new List<IDbDataParameter>();
            var parts =command.CommandText.Split('.');
            command.CommandType = CommandType.Text;
            command.CommandText = string.Format("select * from information_schema.parameters where specific_name='{0}' and specific_schema='{1}'", parts[1], parts[0]);
            var dataTable = FetchDataTable(command);
            foreach(DataRow row in dataTable.Rows)
            {
                
                var parameterName = row["PARAMETER_NAME"] as string;
                if(String.IsNullOrEmpty(parameterName))
                {
                    continue;                    
                }
                SqlDbType sqlType;
                Enum.TryParse(row["DATA_TYPE"] as string, true, out sqlType);
                var maxLength = row["CHARACTER_MAXIMUM_LENGTH"] is int ? (int)row["CHARACTER_MAXIMUM_LENGTH"] : -1;
                var sqlParameter = new SqlParameter(parameterName, sqlType, maxLength);
                command.Parameters.Add(sqlParameter);
                if(parameterName.ToLower() == "@return_value")
                {
                    continue;
                }
                parameters.Add(sqlParameter);
            }
            command.CommandText = originalCommandText;
            return parameters;
        }

        static bool IsStoredProcedure(DataRow row, DataColumn procedureTypeColumn)
        {
            if(row == null || procedureTypeColumn == null)
            {
                return false;
            }
            return row[procedureTypeColumn].ToString().Equals("PROCEDURE");
        }

        static bool IsFunction(DataRow row, DataColumn procedureTypeColumn)
        {
            if(row == null || procedureTypeColumn == null)
            {
                return false;
            }

            return !row[procedureTypeColumn].ToString().Equals("PROCEDURE");
        }

        #region IDisposable

        ~SqlServer()
        {
            // Do not re-create Dispose clean-up code here. 
            // Calling Dispose(false) is optimal in terms of 
            // readability and maintainability.
            Dispose(false);
        }

        bool _disposed;

        // Implement IDisposable. 
        // Do not make this method virtual. 
        // A derived class should not be able to override this method. 
        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method. 
            // Therefore, you should call GC.SupressFinalize to 
            // take this object off the finalization queue 
            // and prevent finalization code for this object 
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        // Dispose(bool disposing) executes in two distinct scenarios. 
        // If disposing equals true, the method has been called directly 
        // or indirectly by a user's code. Managed and unmanaged resources 
        // can be disposed. 
        // If disposing equals false, the method has been called by the 
        // runtime from inside the finalizer and you should not reference 
        // other objects. Only unmanaged resources can be disposed. 
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called. 
            if(!_disposed)
            {
                // If disposing equals true, dispose all managed 
                // and unmanaged resources. 
                if(disposing)
                {
                    // Dispose managed resources.
                    if(_transaction != null)
                    {
                        _transaction.Dispose();
                    }

                    if(_command != null)
                    {
                        _command.Dispose();
                    }

                    if(_connection != null)
                    {
                        if(_connection.State != ConnectionState.Closed)
                        {
                            _connection.Close();
                        }
                        _connection.Dispose();
                    }
                }

                // Call the appropriate methods to clean up 
                // unmanaged resources here. 
                // If disposing is false, 
                // only the following code is executed.

                // Note disposing has been done.
                _disposed = true;
            }
        }

        #endregion

    }
}
