// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace System.Web.Http.Dispatcher
{
    /// <summary>
    /// Provides an abstraction for managing the assemblies of an application. A different
    /// implementation can be registered via the <see cref="T:System.Web.Http.Services.DependencyResolver"/>.
    /// </summary>
    public interface IAssembliesResolver
    {
        /// <summary>
        /// Returns a list of assemblies available for the application.
        /// </summary>
        /// <returns>An <see cref="ICollection{Assembly}"/> of assemblies.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "This is better handled as a method.")]
        ICollection<Assembly> GetAssemblies();
    }
}
