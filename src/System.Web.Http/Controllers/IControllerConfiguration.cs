// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Http.Controllers
{
    /// <summary>
    /// If a controller is decorated with an attribute with this interface, then it gets invoked
    /// to initialize the controller settings. 
    /// </summary>
    public interface IControllerConfiguration
    {
        /// <summary>
        /// Callback invoked to set per-controller overrides for this controllerDescriptor.
        /// </summary>
        /// <param name="controllerSettings">The controller settings to initialize.</param>
        /// <param name="controllerDescriptor">The controller descriptor. Note that the <see cref="HttpControllerDescriptor"/> can be associated with the derived controller type given that <see cref="IControllerConfiguration"/> is inherited.</param>
        void Initialize(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor);
    }
}
