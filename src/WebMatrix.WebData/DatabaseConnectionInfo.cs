// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using WebMatrix.Data;

namespace WebMatrix.WebData
{
    internal class DatabaseConnectionInfo
    {
        private string _connectionStringName;
        private string _connectionString;

        private enum ConnectionType
        {
            ConnectionStringName = 0,
            ConnectionString = 1
        }

        public string ConnectionString
        {
            get { return _connectionString; }
            set
            {
                _connectionString = value;
                Type = ConnectionType.ConnectionString;
            }
        }

        public string ConnectionStringName
        {
            get { return _connectionStringName; }
            set
            {
                _connectionStringName = value;
                Type = ConnectionType.ConnectionStringName;
            }
        }

        public string ProviderName { get; set; }

        private ConnectionType Type { get; set; }

        public Database Connect()
        {
            switch (Type)
            {
                case ConnectionType.ConnectionString:
                    return Database.OpenConnectionString(ConnectionString, ProviderName);
                case ConnectionType.ConnectionStringName:
                    return Database.Open(ConnectionStringName);
                default:
                    return null;
            }
        }
    }
}
