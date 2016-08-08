// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace System.Web.Http.Dispatcher
{
    /// <summary>
    /// Provides an abstraction for managing the controller types of an application. A different
    /// implementation can be registered via the <see cref="T:System.Web.Http.Services.DependencyResolver"/>.
    /// </summary>
    public interface IHttpControllerTypeResolver
    {
        /// <summary>
        /// Returns a list of controllers available for the application.
        /// </summary>
        /// <returns>An <see cref="ICollection{Type}"/> of controllers.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "This is better handled as a method.")]
        ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver);
    }
}
