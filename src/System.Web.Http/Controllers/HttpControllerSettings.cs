// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Formatting;
using System.Web.Http.ModelBinding;

namespace System.Web.Http.Controllers
{
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "ServicesContainer is disposed with the configuration")]
    public sealed class HttpControllerSettings
    {
        private MediaTypeFormatterCollection _formatters;
        private ParameterBindingRulesCollection _parameterBindingRules;
        private ServicesContainer _services;
        private HttpConfiguration _configuration;

        public HttpControllerSettings(HttpConfiguration configuration)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            _configuration = configuration;
        }

        public MediaTypeFormatterCollection Formatters
        {
            get
            {
                if (_formatters == null)
                {
                    _formatters = new MediaTypeFormatterCollection(_configuration.Formatters);
                }

                return _formatters;
            }
        }

        public ParameterBindingRulesCollection ParameterBindingRules
        {
            get
            {
                if (_parameterBindingRules == null)
                {
                    _parameterBindingRules = new ParameterBindingRulesCollection();
                    foreach (var parameterBindingRule in _configuration.ParameterBindingRules)
                    {
                        _parameterBindingRules.Add(parameterBindingRule);
                    }
                }

                return _parameterBindingRules;
            }
        }

        public ServicesContainer Services
        {
            get
            {
                if (_services == null)
                {
                    _services = new ControllerServices(_configuration.Services);
                }

                return _services;
            }
        }

        internal bool IsFormatterCollectionInitialized
        {
            get { return _formatters != null; }
        }

        internal bool IsParameterBindingRuleCollectionInitialized
        {
            get { return _parameterBindingRules != null; }
        }

        internal bool IsServiceCollectionInitialized
        {
            get { return _services != null; }
        }
    }
}