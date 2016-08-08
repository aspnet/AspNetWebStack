// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net.Mime;
using System.Runtime.Serialization.Json;
using System.Web.Mvc;

namespace Microsoft.Web.Mvc.Resources
{
    public class JsonFormatHandler : IRequestFormatHandler, IResponseFormatHandler
    {
        public string FriendlyName
        {
            get { return "Json"; }
        }

        public bool CanDeserialize(ContentType requestFormat)
        {
            return requestFormat != null && IsCompatibleMediaType(requestFormat.MediaType);
        }

        public object Deserialize(ControllerContext controllerContext, ModelBindingContext bindingContext, ContentType requestFormat)
        {
            DataContractJsonSerializer json = new DataContractJsonSerializer(bindingContext.ModelType);
            return json.ReadObject(controllerContext.HttpContext.Request.InputStream);
        }

        public bool CanSerialize(ContentType responseFormat)
        {
            return responseFormat != null && IsCompatibleMediaType(responseFormat.MediaType);
        }

        public void Serialize(ControllerContext context, object model, ContentType responseFormat)
        {
            DataContractJsonActionResult json = new DataContractJsonActionResult(model, responseFormat);
            json.ExecuteResult(context);
        }

        protected virtual bool IsCompatibleMediaType(string mediaType)
        {
            return (mediaType == "text/json" || mediaType == "application/json");
        }

        public bool TryMapFormatFriendlyName(string friendlyName, out ContentType contentType)
        {
            if (String.Equals(friendlyName, this.FriendlyName, StringComparison.OrdinalIgnoreCase))
            {
                contentType = new ContentType("application/json");
                return true;
            }
            contentType = null;
            return false;
        }
    }
}
