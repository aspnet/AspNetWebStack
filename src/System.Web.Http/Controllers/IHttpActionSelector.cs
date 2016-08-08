// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
namespace System.Web.Http.Controllers
{
    public interface IHttpActionSelector
    {
        /// <summary>
        /// Selects the action.
        /// </summary>
        /// <param name="controllerContext">The controller context.</param>
        /// <returns>The selected action.</returns>
        HttpActionDescriptor SelectAction(HttpControllerContext controllerContext);

        /// <summary>
        /// Returns a map, keyed by action string, of all <see cref="HttpActionDescriptor"/> that the selector can select. 
        /// This is primarily called by <see cref="System.Web.Http.Description.IApiExplorer"/> to discover all the possible actions in the controller.
        /// </summary>
        /// <param name="controllerDescriptor">The controller descriptor.</param>
        /// <returns>A map of <see cref="HttpActionDescriptor"/> that the selector can select, or null if the selector does not have a well-defined mapping of <see cref="HttpActionDescriptor"/>.</returns>
        ILookup<string, HttpActionDescriptor> GetActionMapping(HttpControllerDescriptor controllerDescriptor);
    }
}
