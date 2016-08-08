// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.WebPages
{
    internal static class VirtualPathFactoryExtensions
    {
        public static T CreateInstance<T>(this IVirtualPathFactory factory, string virtualPath) where T : class
        {
            var virtualPathFactoryManager = factory as VirtualPathFactoryManager;
            if (virtualPathFactoryManager != null)
            {
                return virtualPathFactoryManager.CreateInstanceOfType<T>(virtualPath);
            }
            var buildManagerFactory = factory as BuildManagerWrapper;
            if (buildManagerFactory != null)
            {
                return buildManagerFactory.CreateInstanceOfType<T>(virtualPath);
            }

            return factory.CreateInstance(virtualPath) as T;
        }
    }
}
