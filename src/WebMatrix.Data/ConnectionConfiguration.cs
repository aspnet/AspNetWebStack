// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;

namespace WebMatrix.Data
{
    internal class ConnectionConfiguration : IConnectionConfiguration
    {
        internal ConnectionConfiguration(string providerName, string connectionString)
            : this(new DbProviderFactoryWrapper(providerName), connectionString)
        {
        }

        internal ConnectionConfiguration(IDbProviderFactory providerFactory, string connectionString)
        {
            Debug.Assert(!String.IsNullOrEmpty(connectionString), "connectionString should not be null");

            ProviderFactory = providerFactory;
            ConnectionString = connectionString;
        }

        public IDbProviderFactory ProviderFactory { get; private set; }

        public string ConnectionString { get; private set; }
    }
}
