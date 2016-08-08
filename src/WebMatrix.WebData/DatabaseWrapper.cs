// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using WebMatrix.Data;

namespace WebMatrix.WebData
{
    internal class DatabaseWrapper : IDatabase
    {
        private readonly Database _database;

        public DatabaseWrapper(Database database)
        {
            _database = database;
        }

        public dynamic QuerySingle(string commandText, params object[] parameters)
        {
            return _database.QuerySingle(commandText, parameters);
        }

        public IEnumerable<dynamic> Query(string commandText, params object[] parameters)
        {
            return _database.Query(commandText, parameters);
        }

        public dynamic QueryValue(string commandText, params object[] parameters)
        {
            return _database.QueryValue(commandText, parameters);
        }

        public int Execute(string commandText, params object[] parameters)
        {
            return _database.Execute(commandText, parameters);
        }

        public void Dispose()
        {
            _database.Dispose();
        }
    }
}
