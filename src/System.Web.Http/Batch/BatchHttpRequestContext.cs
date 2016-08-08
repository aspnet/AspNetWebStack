// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;

namespace System.Web.Http.Batch
{
    internal class BatchHttpRequestContext : HttpRequestContext
    {
        private readonly HttpRequestContext _batchContext;

        public BatchHttpRequestContext(HttpRequestContext batchContext)
        {
            if (batchContext == null)
            {
                throw new ArgumentNullException("batchContext");
            }

            _batchContext = batchContext;
        }

        public HttpRequestContext BatchContext
        {
            get { return _batchContext; }
        }

        public override X509Certificate2 ClientCertificate
        {
            get
            {
                return _batchContext.ClientCertificate;
            }
            set
            {
                _batchContext.ClientCertificate = value;
            }
        }

        public override HttpConfiguration Configuration
        {
            get
            {
                // Use separate route data for configuration (base, not _batchContext).
                return base.Configuration;
            }
            set
            {
                // Use separate route data for configuration (base, not _batchContext).
                base.Configuration = value;
            }
        }

        public override bool IncludeErrorDetail
        {
            get
            {
                return _batchContext.IncludeErrorDetail;
            }
            set
            {
                _batchContext.IncludeErrorDetail = value;
            }
        }

        public override bool IsLocal
        {
            get
            {
                return _batchContext.IsLocal;
            }
            set
            {
                _batchContext.IsLocal = value;
            }
        }

        public override IPrincipal Principal
        {
            get
            {
                return _batchContext.Principal;
            }
            set
            {
                _batchContext.Principal = value;
            }
        }

        public override IHttpRouteData RouteData
        {
            get
            {
                // Use separate route data for batching (base, not _batchContext).
                return base.RouteData;
            }
            set
            {
                // Use separate route data for batching (base, not _batchContext).
                base.RouteData = value;
            }
        }

        public override UrlHelper Url
        {
            get
            {
                // Use a separate URL factory for batching (base, not _batchContext).
                return base.Url;
            }
            set
            {
                // Use a separate URL factory for batching (base, not _batchContext).
                base.Url = value;
            }
        }

        public override string VirtualPathRoot
        {
            get
            {
                return _batchContext.VirtualPathRoot;
            }
            set
            {
                _batchContext.VirtualPathRoot = value;
            }
        }
    }
}
