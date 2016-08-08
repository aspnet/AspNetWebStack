// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace WebMatrix.Data
{
    internal class ConfigurationManagerWrapper : IConfigurationManager
    {
        private readonly string _dataDirectory = null;
        private IDictionary<string, string> _appSettings;
        private IDictionary<string, IDbFileHandler> _handlers;

        public ConfigurationManagerWrapper(IDictionary<string, IDbFileHandler> handlers, string dataDirectory = null)
        {
            Debug.Assert(handlers != null, "handlers should not be null");
            _dataDirectory = dataDirectory ?? Database.DataDirectory;
            _handlers = handlers;
        }

        public IDictionary<string, string> AppSettings
        {
            get
            {
                if (_appSettings == null)
                {
                    _appSettings = (from string key in ConfigurationManager.AppSettings
                                    select key).ToDictionary(key => key, key => ConfigurationManager.AppSettings[key]);
                }
                return _appSettings;
            }
        }

        private static IConnectionConfiguration GetConnectionConfigurationFromConfig(string name)
        {
            ConnectionStringSettings setting = ConfigurationManager.ConnectionStrings[name];
            if (setting != null)
            {
                return new ConnectionConfiguration(setting.ProviderName, setting.ConnectionString);
            }
            return null;
        }

        public IConnectionConfiguration GetConnection(string name)
        {
            return GetConnection(name, GetConnectionConfigurationFromConfig, File.Exists);
        }

        // For unit testing
        internal IConnectionConfiguration GetConnection(string name, Func<string, IConnectionConfiguration> getConfigConnection, Func<string, bool> fileExists)
        {
            // First try config
            IConnectionConfiguration configuraitonConfig = getConfigConnection(name);
            if (configuraitonConfig != null)
            {
                return configuraitonConfig;
            }

            // Then try files under the |DataDirectory| with the supported extensions
            // REVIEW: We sort because we want to process mdf before sdf (we only have 2 entries)
            foreach (var handler in _handlers.OrderBy(h => h.Key))
            {
                string fileName = Path.Combine(_dataDirectory, name + handler.Key);
                if (fileExists(fileName))
                {
                    return handler.Value.GetConnectionConfiguration(fileName);
                }
            }

            return null;
        }
    }
}
