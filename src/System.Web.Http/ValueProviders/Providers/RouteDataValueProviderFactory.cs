// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Web.Http.Controllers;

namespace System.Web.Http.ValueProviders.Providers
{
    public class RouteDataValueProviderFactory : ValueProviderFactory, IUriValueProviderFactory
    {
        private const string RequestLocalStorageKey = "{C0E50671-A1D4-429E-93C9-2AA63779924F}";

        public override IValueProvider GetValueProvider(HttpActionContext actionContext)
        {
            if (actionContext == null)
            {
                throw Error.ArgumentNull("actionContext");
            }

            // cache the route provider across requests so that we don't recompute on every parameter.
            RouteDataValueProvider provider;
            IDictionary<string, object> storage = actionContext.Request.Properties;

            if (!storage.TryGetValue(RequestLocalStorageKey, out provider))
            {
                provider = new RouteDataValueProvider(actionContext, CultureInfo.InvariantCulture);
                storage[RequestLocalStorageKey] = provider;
            }

            return provider;       
        }
    }
}
