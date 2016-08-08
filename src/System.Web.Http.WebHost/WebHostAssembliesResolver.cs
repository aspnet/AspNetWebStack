// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Compilation;
using System.Web.Http.Dispatcher;

namespace System.Web.Http.WebHost
{
    /// <summary>
    /// Provides an implementation of <see cref="IAssembliesResolver"/> using <see cref="BuildManager"/>.
    /// </summary>
    internal sealed class WebHostAssembliesResolver : IAssembliesResolver
    {
        /// <summary>
        /// Returns a list of assemblies that will be searched for types that implement IHttpController, such as ApiController.
        /// </summary>
        /// <returns>An <see cref="ICollection{Assembly}" /> of assemblies.</returns>
        ICollection<Assembly> IAssembliesResolver.GetAssemblies()
        {
            return BuildManager.GetReferencedAssemblies().OfType<Assembly>().ToList();
        }
    }
}
