// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Mime;
using System.Web.Mvc;

namespace Microsoft.Web.Mvc.Resources
{
    /// <summary>
    /// Extensibility mechanism for serializing response in
    /// additional formats. FormatManager.Current.RequestFormatHandlers contains the list of request formats
    /// supported by the web application
    /// </summary>
    public interface IResponseFormatHandler
    {
        /// <summary>
        /// The preferred friendly name for the handled format
        /// </summary>
        string FriendlyName { get; }

        /// <summary>
        /// Return true if the specified friendly name ('xml' for instance) can
        /// be mapped to a content type ('application/xml' for instance). If the mapping
        /// can be performed return the content type that the friendlyName maps to
        /// </summary>
        /// <param name="friendlyName"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        bool TryMapFormatFriendlyName(string friendlyName, out ContentType contentType);

        /// <summary>
        /// Return true if the specified response format can be serialized
        /// </summary>
        /// <param name="responseFormat"></param>
        /// <returns></returns>
        bool CanSerialize(ContentType responseFormat);

        /// <summary>
        /// Serialize the model into the response body in the specified response format
        /// </summary>
        /// <param name="context"></param>
        /// <param name="model"></param>
        /// <param name="responseFormat"></param>
        void Serialize(ControllerContext context, object model, ContentType responseFormat);
    }
}
