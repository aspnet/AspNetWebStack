// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace System.Web.WebPages
{
    // This class encapsulates the creation of objects from virtual paths.  The creation is either performed via BuildBanager API's, or
    // by using explicitly registered factories (which happens through ApplicationPart.Register).
    public class VirtualPathFactoryManager : IVirtualPathFactory
    {
        private static readonly Lazy<VirtualPathFactoryManager> _instance = new Lazy<VirtualPathFactoryManager>(() => new VirtualPathFactoryManager(new BuildManagerWrapper()));
        private static Func<string, bool> _instancePathExists;
        private readonly LinkedList<IVirtualPathFactory> _virtualPathFactories = new LinkedList<IVirtualPathFactory>();

        internal VirtualPathFactoryManager(IVirtualPathFactory defaultFactory)
        {
            _virtualPathFactories.AddFirst(defaultFactory);
        }

        // Get the VirtualPathFactoryManager singleton instance
        internal static VirtualPathFactoryManager Instance
        {
            get { return _instance.Value; }
        }

        internal static Func<string, bool> InstancePathExists
        {
            get 
            {
                if (_instancePathExists == null)
                {
                    _instancePathExists = Instance.Exists;
                }
                return _instancePathExists;
            }
        }

        internal IEnumerable<IVirtualPathFactory> RegisteredFactories
        {
            get { return _virtualPathFactories; }
        }

        public static void RegisterVirtualPathFactory(IVirtualPathFactory virtualPathFactory)
        {
            Instance.RegisterVirtualPathFactoryInternal(virtualPathFactory);
        }

        internal void RegisterVirtualPathFactoryInternal(IVirtualPathFactory virtualPathFactory)
        {
            _virtualPathFactories.AddBefore(_virtualPathFactories.Last, virtualPathFactory);
        }

        public bool Exists(string virtualPath)
        {
            // Performance sensitive so avoid Linq and delegates
            foreach (IVirtualPathFactory factory in _virtualPathFactories)
            {
                if (factory.Exists(virtualPath))
                {
                    return true;
                }
            }
            return false;
        }

        public object CreateInstance(string virtualPath)
        {
            return CreateInstanceOfType<object>(virtualPath);
        }

        internal T CreateInstanceOfType<T>(string virtualPath) where T : class
        {
            var virtualPathFactory = _virtualPathFactories.FirstOrDefault(f => f.Exists(virtualPath));
            if (virtualPathFactory != null)
            {
                return virtualPathFactory.CreateInstance<T>(virtualPath);
            }
            return null;
        }
    }
}
