// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Web.Http.Controllers;

namespace System.Web.Http.Dispatcher
{
    /// <summary>
    /// Defines the methods that are required for an <see cref="IHttpController"/> factory.
    /// </summary>
    public interface IHttpControllerSelector
    {
        /// <summary>
        /// Selects a <see cref="HttpControllerDescriptor"/> for the given <see cref="HttpRequestMessage"/>.
        /// </summary>
        /// <param name="request">The request message.</param>
        /// <returns>An <see cref="HttpControllerDescriptor"/> instance.</returns>
        HttpControllerDescriptor SelectController(HttpRequestMessage request);

        /// <summary>
        /// Returns a map, keyed by controller string, of all <see cref="HttpControllerDescriptor"/> that the selector can select. 
        /// This is primarily called by <see cref="System.Web.Http.Description.IApiExplorer"/> to discover all the possible controllers in the system.
        /// </summary>
        /// <returns>A map of all <see cref="HttpControllerDescriptor"/> that the selector can select, or null if the selector does not have a well-defined mapping of <see cref="HttpControllerDescriptor"/>.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "This is better handled as a method.")]
        IDictionary<string, HttpControllerDescriptor> GetControllerMapping();
    }
}
