﻿using System.Collections.Generic;

// ReSharper disable CheckNamespace
namespace Dev2.Runtime.ServiceModel.Data
{
    public class DbTable
    {
        public string Schema { get; set; }
        public string TableName { get; set; }
        public string FullName
        {
            get
            {
                if(string.IsNullOrEmpty(Schema))
                {
                    return TableName;
                }
                if(string.IsNullOrEmpty(TableName))
                {
                    return "";
                }
                return string.Format("{0}.{1}", Schema, TableName);
            }
        }
        public List<DbColumn> Columns { get; set; }
    }
}