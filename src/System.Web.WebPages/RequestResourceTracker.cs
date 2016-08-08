// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace System.Web.WebPages
{
    internal static class RequestResourceTracker
    {
        private static readonly object _resourcesKey = new object();

        private static List<SecureWeakReference> GetResources(HttpContextBase context)
        {
            var resources = (List<SecureWeakReference>)context.Items[_resourcesKey];
            if (resources == null)
            {
                resources = new List<SecureWeakReference>();
                context.Items[_resourcesKey] = resources;
            }

            return resources;
        }

        internal static void DisposeResources(HttpContextBase context)
        {
            var resources = GetResources(context);
            if (resources != null)
            {
                resources.ForEach(resource => resource.Dispose());
                resources.Clear();
            }
        }

        internal static void RegisterForDispose(HttpContextBase context, IDisposable resource)
        {
            var resources = GetResources(context);
            if (resources != null)
            {
                resources.Add(new SecureWeakReference(resource));
            }
        }

        internal static void RegisterForDispose(IDisposable resource)
        {
            var context = HttpContext.Current;
            if (context != null)
            {
                RegisterForDispose(new HttpContextWrapper(context), resource);
            }
        }

        private sealed class SecureWeakReference
        {
            private readonly WeakReference _reference;

            public SecureWeakReference(IDisposable reference)
            {
                _reference = new WeakReference(reference);
            }

            internal void Dispose()
            {
                var disposable = (IDisposable)_reference.Target;
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }
        }
    }
}
