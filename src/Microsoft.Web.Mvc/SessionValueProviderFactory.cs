// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web;
using System.Web.Mvc;

namespace Microsoft.Web.Mvc
{
    public class SessionValueProviderFactory : ValueProviderFactory
    {
        public override IValueProvider GetValueProvider(ControllerContext controllerContext)
        {
            HttpSessionStateBase session = controllerContext.HttpContext.Session;
            if (session == null)
            {
                // session is disabled
                return null;
            }

            Dictionary<string, object> backingStore = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            foreach (string key in session)
            {
                if (key != null)
                {
                    backingStore[key] = session[key]; // copy to backing store
                }
            }

            // use the invariant culture since Session contains serialized objects
            return new DictionaryValueProvider<object>(backingStore, CultureInfo.InvariantCulture);
        }
    }
}
