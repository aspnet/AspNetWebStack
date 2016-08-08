// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Http.Controllers;

namespace System.Web.Http.Description
{
    /// <summary>
    /// Defines the provider responsible for documenting the service.
    /// </summary>
    public interface IDocumentationProvider
    {
        /// <summary>
        /// Gets the documentation based on <see cref="HttpControllerDescriptor"/>.
        /// </summary>
        /// <param name="controllerDescriptor">The controller descriptor.</param>
        /// <returns>Documentation for the controller.</returns>
        string GetDocumentation(HttpControllerDescriptor controllerDescriptor);

        /// <summary>
        /// Gets the documentation based on <see cref="HttpActionDescriptor"/>.
        /// </summary>
        /// <param name="actionDescriptor">The action descriptor.</param>
        /// <returns>Documentation for the action.</returns>
        string GetDocumentation(HttpActionDescriptor actionDescriptor);

        /// <summary>
        /// Gets the documentation based on <see cref="HttpParameterDescriptor"/>.
        /// </summary>
        /// <param name="parameterDescriptor">The parameter descriptor.</param>
        /// <returns>Documentation for the parameter.</returns>
        string GetDocumentation(HttpParameterDescriptor parameterDescriptor);

        /// <summary>
        /// Gets the response documentation based on <see cref="HttpActionDescriptor"/>.
        /// </summary>
        /// <param name="actionDescriptor">The action descriptor.</param>
        /// <returns>Documentation for the action response.</returns>
        string GetResponseDocumentation(HttpActionDescriptor actionDescriptor);
    }
}
