// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IdentityModel.Claims;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;
using System.Web.Http.SelfHost.Channels;
using System.Web.Http.SelfHost.Properties;
using System.Web.Http.SelfHost.ServiceModel.Channels;

namespace System.Web.Http.SelfHost
{
    internal class SelfHostHttpRequestContext : HttpRequestContext
    {
        private readonly RequestContext _requestContext;
        private readonly HttpRequestMessage _request;

        private HttpConfiguration _configuration;

        private X509Certificate2 _clientCertificate;
        private bool _clientCertificateSet;

        private bool _includeErrorDetail;
        private bool _includeErrorDetailSet;

        private bool _isLocal;
        private bool _isLocalSet;

        private UrlHelper _url;
        private bool _urlSet;

        private string _virtualPathRoot;
        private bool _virtualPathRootSet;

        public SelfHostHttpRequestContext(RequestContext requestContext, HttpConfiguration configuration,
            HttpRequestMessage request)
        {
            Contract.Assert(requestContext != null);
            Contract.Assert(configuration != null);
            Contract.Assert(request != null);
            _requestContext = requestContext;
            _configuration = configuration;
            _request = request;
        }

        public RequestContext RequestContext
        {
            get { return _requestContext; }
        }

        public HttpRequestMessage Request
        {
            get { return _request; }
        }

        // RouteData is not overridden; it is provided in a later point in the pipeline (HttpRoutingDispatcher).

        public override X509Certificate2 ClientCertificate
        {
            get
            {
                if (!_clientCertificateSet)
                {
                    _clientCertificate = HttpSelfHostServer.RetrieveClientCertificate(_request);
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
                return _configuration;
            }
            set
            {
                _configuration = value;
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
                    _isLocal = HttpSelfHostServer.IsLocal(_requestContext.RequestMessage);
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
                return Thread.CurrentPrincipal;
            }
            set
            {
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
                        value = "/";
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
