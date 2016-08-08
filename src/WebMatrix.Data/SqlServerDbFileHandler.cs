// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.IO;

namespace WebMatrix.Data
{
    internal class SqlServerDbFileHandler : IDbFileHandler
    {
        private const string SqlServerConnectionStringFormat = @"Data Source=.\SQLEXPRESS;AttachDbFilename={0};Initial Catalog={1};Integrated Security=True;User Instance=True;MultipleActiveResultSets=True";
        private const string SqlServerProviderName = "System.Data.SqlClient";

        public IConnectionConfiguration GetConnectionConfiguration(string fileName)
        {
            return new ConnectionConfiguration(SqlServerProviderName, GetConnectionString(fileName, Database.DataDirectory));
        }

        internal static string GetConnectionString(string fileName, string dataDirectory)
        {
            if (Path.IsPathRooted(fileName))
            {
                // Attach the db as the file name if it is rooted
                return String.Format(CultureInfo.InvariantCulture, SqlServerConnectionStringFormat, fileName, fileName);
            }

            // Use |DataDirectory| if the path isn't rooted
            string dataSource = @"|DataDirectory|\" + Path.GetFileName(fileName);
            // Set the full path for the initial catalog so we attach as that
            string initialCatalog = Path.Combine(dataDirectory, Path.GetFileName(fileName));
            return String.Format(CultureInfo.InvariantCulture, SqlServerConnectionStringFormat, dataSource, initialCatalog);
        }
    }
}
