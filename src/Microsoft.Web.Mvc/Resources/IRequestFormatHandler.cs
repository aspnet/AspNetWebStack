// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Mime;
using System.Web.Mvc;

namespace Microsoft.Web.Mvc.Resources
{
    /// <summary>
    /// Extensibility mechanism for deserializing data in additional formats.
    /// FormatManager.Current.RequestFormatHandlers contains the list of request formats
    /// supported by the web application
    /// </summary>
    public interface IRequestFormatHandler
    {
        /// <summary>
        /// Returns true if the handler can deserialize request's content type
        /// </summary>
        /// <param name="requestFormat"></param>
        /// <returns></returns>
        bool CanDeserialize(ContentType requestFormat);

        /// <summary>
        /// Deserialize the request body based on model binding context and return the object.
        /// Note that the URI parameters are handled by the base infrastructure.
        /// </summary>
        /// <param name="controllerContext"></param>
        /// <param name="bindingContext"></param>
        /// <param name="requestFormat"></param>
        /// <returns></returns>
        object Deserialize(ControllerContext controllerContext, ModelBindingContext bindingContext, ContentType requestFormat);
    }
}
