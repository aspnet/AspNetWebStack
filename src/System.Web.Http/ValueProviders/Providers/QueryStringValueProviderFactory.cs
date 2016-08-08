// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Web.Http.Controllers;

namespace System.Web.Http.ValueProviders.Providers
{
    public class QueryStringValueProviderFactory : ValueProviderFactory, IUriValueProviderFactory
    {
        private const string RequestLocalStorageKey = "{8572540D-3BD9-46DA-B112-A1E6C9086003}";

        public override IValueProvider GetValueProvider(HttpActionContext actionContext)
        {
            if (actionContext == null)
            {
                throw Error.ArgumentNull("actionContext");
            }

            // Only parse the query string once-per request. 
                        
            QueryStringValueProvider provider;
            IDictionary<string, object> storage  = actionContext.Request.Properties;

            if (!storage.TryGetValue(RequestLocalStorageKey, out provider))
            {
                provider = new QueryStringValueProvider(actionContext, CultureInfo.InvariantCulture);
                storage[RequestLocalStorageKey] = provider;
            }            
            
            return provider;
        }
    }
}
