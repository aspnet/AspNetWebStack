// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace System.Web.Mvc
{
    internal static class MultiServiceResolver        
    {
        internal static TService[] GetCombined<TService>(IList<TService> items, IDependencyResolver resolver = null) where TService : class
        {           
            if (resolver == null)
            {
                resolver = DependencyResolver.Current;
            }
            IEnumerable<TService> services = resolver.GetServices<TService>();
            return services.Concat(items).ToArray();
        } 
    }
}
