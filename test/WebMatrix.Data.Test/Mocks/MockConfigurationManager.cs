// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace WebMatrix.Data.Test.Mocks
{
    internal class MockConfigurationManager : IConfigurationManager
    {
        private Dictionary<string, IConnectionConfiguration> _connectionStrings = new Dictionary<string, IConnectionConfiguration>();

        public MockConfigurationManager()
        {
            AppSettings = new Dictionary<string, string>();
        }

        public IDictionary<string, string> AppSettings { get; private set; }

        public void AddConnection(string name, IConnectionConfiguration configuration)
        {
            _connectionStrings.Add(name, configuration);
        }

        public IConnectionConfiguration GetConnection(string name)
        {
            IConnectionConfiguration configuration;
            _connectionStrings.TryGetValue(name, out configuration);
            return configuration;
        }
    }
}
