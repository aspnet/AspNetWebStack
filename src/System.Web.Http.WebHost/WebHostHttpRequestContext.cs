// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Threading;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;

namespace System.Web.Http.WebHost
{
    internal class WebHostHttpRequestContext : HttpRequestContext
    {
        private readonly HttpContextBase _contextBase;
        private readonly HttpRequestBase _requestBase;
        private readonly HttpRequestMessage _request;

        private X509Certificate2 _clientCertificate;
        private bool _clientCertificateSet;

        private HttpConfiguration _configuration;
        private bool _configurationSet;

        private bool _includeErrorDetail;
        private bool _includeErrorDetailSet;

        private bool _isLocal;
        private bool _isLocalSet;

        private UrlHelper _url;
        private bool _urlSet;

        private string _virtualPathRoot;
        private bool _virtualPathRootSet;

        public WebHostHttpRequestContext(HttpContextBase contextBase, HttpRequestBase requestBase,
            HttpRequestMessage request)
        {
            Contract.Assert(contextBase != null);
            Contract.Assert(requestBase != null);
            Contract.Assert(request != null);
            _contextBase = contextBase;
            _requestBase = requestBase;
            _request = request;
        }

        public HttpContextBase Context
        {
            get { return _contextBase; }
        }

        public HttpRequestBase WebRequest
        {
            get { return _requestBase; }
        }

        public HttpRequestMessage Request
        {
            get { return _request; }
        }

        // RouteData is not overridden; it is provided by a later point in the pipeline
        //  (HttpControllerHandler.ProcessRequestAsyncCore).

        public override X509Certificate2 ClientCertificate
        {
            get
            {
                if (!_clientCertificateSet)
                {
                    X509Certificate2 value;

                    if (_requestBase.ClientCertificate != null && _requestBase.ClientCertificate.Certificate != null
                        && _requestBase.ClientCertificate.Certificate.Length > 0)
                    {
                        value = new X509Certificate2(_requestBase.ClientCertificate.Certificate);
                    }
                    else
                    {
                        value = null;
                    }

                    _clientCertificate = value;
                    _clientCertificateSet = true;
                }

                return _clientCertificate;
            }
            set
            {
                _clientCertificate = value;
                _clientCertificateSet = true;
            }
        }

        public override HttpConfiguration Configuration
        {
            get
            {
                if (!_configurationSet)
                {
                    _configuration = GlobalConfiguration.Configuration;
                    _configurationSet = true;
                }

                return _configuration;
            }
            set
            {
                _configuration = value;
                _configurationSet = true;
            }
        }

        public override bool IncludeErrorDetail
        {
            get
            {
                if (!_includeErrorDetailSet)
                {
                    IncludeErrorDetailPolicy includeErrorDetailPolicy;

                    if (_configuration != null)
                    {
                        includeErrorDetailPolicy = _configuration.IncludeErrorDetailPolicy;
                    }
                    else
                    {
                        includeErrorDetailPolicy = IncludeErrorDetailPolicy.Default;
                    }

                    bool value;

                    switch (includeErrorDetailPolicy)
                    {
                        case IncludeErrorDetailPolicy.Default:
                            value = !_contextBase.IsCustomErrorEnabled;
                            break;

                        case IncludeErrorDetailPolicy.LocalOnly:
                            value = IsLocal;
                            break;

                        case IncludeErrorDetailPolicy.Always:
                            value = true;
                            break;

                        case IncludeErrorDetailPolicy.Never:
                        default:
                            value = false;
                            break;
                    }

                    _includeErrorDetail = value;
                    _includeErrorDetailSet = true;
                }

                return _includeErrorDetail;
            }
            set
            {
                _includeErrorDetail = value;
                _includeErrorDetailSet = true;
            }
        }

        public override bool IsLocal
        {
            get
            {
                if (!_isLocalSet)
                {
                    _isLocal = _requestBase.IsLocal;
                    _isLocalSet = true;
                }

                return _isLocal;
            }
            set
            {
                _isLocal = value;
                _isLocalSet = true;
            }
        }

        public override IPrincipal Principal
        {
            get
            {
                return _contextBase.User;
            }
            set
            {
                _contextBase.User = value;
                Thread.CurrentPrincipal = value;
            }
        }

        public override UrlHelper Url
        {
            get
            {
                if (!_urlSet)
                {
                    _url = new UrlHelper(_request);
                    _urlSet = true;
                }

                return _url;
            }
            set
            {
                _url = value;
                _urlSet = true;
            }
        }

        public override string VirtualPathRoot
        {
            get
            {
                if (!_virtualPathRootSet)
                {
                    string value;

                    if (_configuration != null)
                    {
                        value = _configuration.VirtualPathRoot;
                    }
                    else
                    {
                        value = null;
                    }

                    _virtualPathRoot = value;
                    _virtualPathRootSet = true;
                }

                return _virtualPathRoot;
            }
            set
            {
                _virtualPathRoot = value;
                _virtualPathRootSet = true;
            }
        }
    }
}
