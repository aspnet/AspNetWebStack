// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net.Mime;
using System.Runtime.Serialization;
using System.Web.Mvc;

namespace Microsoft.Web.Mvc.Resources
{
    public class XmlFormatHandler : IRequestFormatHandler, IResponseFormatHandler
    {
        public string FriendlyName
        {
            get { return "Xml"; }
        }

        public bool CanDeserialize(ContentType requestFormat)
        {
            return requestFormat != null && IsCompatibleMediaType(requestFormat.MediaType);
        }

        public object Deserialize(ControllerContext controllerContext, ModelBindingContext bindingContext, ContentType requestFormat)
        {
            DataContractSerializer xml = new DataContractSerializer(bindingContext.ModelType, null, Int32.MaxValue, true, true, null);
            return xml.ReadObject(controllerContext.HttpContext.Request.InputStream);
        }

        public bool CanSerialize(ContentType responseFormat)
        {
            return responseFormat != null && IsCompatibleMediaType(responseFormat.MediaType);
        }

        public void Serialize(ControllerContext context, object model, ContentType responseFormat)
        {
            DataContractXmlActionResult xml = new DataContractXmlActionResult(model, responseFormat);
            xml.ExecuteResult(context);
        }

        protected virtual bool IsCompatibleMediaType(string mediaType)
        {
            return (mediaType == "text/xml" || mediaType == "application/xml");
        }

        public bool TryMapFormatFriendlyName(string friendlyName, out ContentType contentType)
        {
            if (String.Equals(friendlyName, this.FriendlyName, StringComparison.OrdinalIgnoreCase))
            {
                contentType = new ContentType("application/xml");
                return true;
            }
            contentType = null;
            return false;
        }
    }
}
