// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace System.Web.Http.Dependencies
{
    internal class EmptyResolver : IDependencyResolver
    {
        private static readonly IDependencyResolver _instance = new EmptyResolver();

        private EmptyResolver()
        {
        }

        public static IDependencyResolver Instance
        {
            get { return _instance; }
        }

        public IDependencyScope BeginScope()
        {
            return this;
        }

        public void Dispose()
        {
        }

        public object GetService(Type serviceType)
        {
            return null;
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return Enumerable.Empty<object>();
        }
    }
}
