// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Web.WebPages.Resources;

namespace System.Web.WebPages.Scope
{
    public class AspNetRequestScopeStorageProvider : IScopeStorageProvider
    {
        private static readonly object _pageScopeKey = new object();
        private static readonly object _requestScopeKey = new object();
        private readonly HttpContextBase _httpContext;
        private readonly Func<bool> _appStartExecuted;

        public AspNetRequestScopeStorageProvider()
            : this(httpContext: null, appStartExecuted: () => WebPageHttpModule.AppStartExecuteCompleted)
        {
        }

        internal AspNetRequestScopeStorageProvider(HttpContextBase httpContext, Func<bool> appStartExecuted)
        {
            _httpContext = httpContext;
            _appStartExecuted = appStartExecuted;
            ApplicationScope = new ApplicationScopeStorageDictionary();
        }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "The state storage API is designed to allow contexts to be set")]
        public IDictionary<object, object> CurrentScope
        {
            get { return PageScope ?? RequestScopeInternal ?? ApplicationScope; }
            set
            {
                if (!_appStartExecuted())
                {
                    // Disallow creating new contexts before the start page is executed. 
                    // This makes sense because our provider is scoped to a request.
                    throw new InvalidOperationException(WebPageResources.StateStorage_StorageScopesCannotBeCreated);
                }
                PageScope = value;
            }
        }

        public IDictionary<object, object> GlobalScope
        {
            get { return ApplicationScope; }
        }

        public IDictionary<object, object> ApplicationScope { get; private set; }

        public IDictionary<object, object> RequestScope
        {
            get
            {
                var requestContext = RequestScopeInternal;
                if (requestContext == null)
                {
                    throw new InvalidOperationException(WebPageResources.StateStorage_RequestScopeNotAvailable);
                }
                return requestContext;
            }
        }

        private HttpContextBase HttpContext
        {
            get
            {
                // If a http context is specifically provided, use that. Else return the value from System.Web.HttpContext.Current if its available.
                var currentHttpContext = Web.HttpContext.Current;
                return _httpContext ?? (currentHttpContext == null ? null : new HttpContextWrapper(currentHttpContext));
            }
        }

        private IDictionary<object, object> RequestScopeInternal
        {
            get
            {
                if (_appStartExecuted())
                {
                    var requestContext = (IDictionary<object, object>)HttpContext.Items[_requestScopeKey];
                    if (requestContext == null)
                    {
                        HttpContext.Items[_requestScopeKey] = requestContext = new ScopeStorageDictionary(ApplicationScope);
                    }
                    return requestContext;
                }
                return null;
            }
        }

        private IDictionary<object, object> PageScope
        {
            get
            {
                if (HttpContext == null)
                {
                    return null;
                }
                return (IDictionary<object, object>)HttpContext.Items[_pageScopeKey];
            }
            set
            {
                // This call would be guarded by the CurrentContext setter.
                Debug.Assert(HttpContext != null);
                HttpContext.Items[_pageScopeKey] = value;
            }
        }
    }
}
