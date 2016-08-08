// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace System.Web.Http.Dispatcher
{
    /// <summary>
    /// Provides an implementation of <see cref="IAssembliesResolver"/> with no external dependencies.
    /// </summary>
    public class DefaultAssembliesResolver : IAssembliesResolver
    {
        /// <summary>
        /// Returns a list of assemblies available for the application.
        /// </summary>
        /// <returns>A <see cref="Collection{Assembly}"/> of assemblies.</returns>
        public virtual ICollection<Assembly> GetAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies().ToList();
        }
    }
}
