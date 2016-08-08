// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Http.Controllers;

namespace System.Web.Http.ValueProviders
{
    public abstract class ValueProviderFactory
    {
        /// <summary>
        /// Get a value provider with values from the given <paramref name="actionContext"/>.
        /// </summary>
        /// <param name="actionContext">action context that value provider will populate from</param>
        /// <returns>a value provider instance or null</returns>
        public abstract IValueProvider GetValueProvider(HttpActionContext actionContext);
    }
}
