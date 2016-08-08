// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Net.Mime;
using System.Web.Mvc;

namespace Microsoft.Web.Mvc.Resources
{
    /// <summary>
    /// Class that maintains a registration of handlers for
    /// request and response formats
    /// </summary>
    public class FormatManager
    {
        public const string UrlEncoded = "application/x-www-form-urlencoded";

        private static FormatManager _current = new DefaultFormatManager();

        private Collection<IRequestFormatHandler> _requestHandlers;
        private Collection<IResponseFormatHandler> _responseHandlers;
        private FormatHelper _formatHelper;

        public FormatManager()
        {
            this._requestHandlers = new Collection<IRequestFormatHandler>();
            this._responseHandlers = new Collection<IResponseFormatHandler>();
            this._formatHelper = new DefaultFormatHelper();
        }

        /// <summary>
        /// The list of handlers that can parse the request body
        /// </summary>
        public Collection<IRequestFormatHandler> RequestFormatHandlers
        {
            get { return this._requestHandlers; }
        }

        /// <summary>
        /// The list of handlers that can serialize the response body
        /// </summary>
        public Collection<IResponseFormatHandler> ResponseFormatHandlers
        {
            get { return this._responseHandlers; }
        }

        public static FormatManager Current
        {
            get { return _current; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                _current = value;
            }
        }

        // CONSIDER: the FormatHelper is an abstraction that lets users extend the content negotiation process
        // we must reconsider the FormatManager/FormatHelper factoring and provide a cleaner way of allowing this same extensibility
        public FormatHelper FormatHelper
        {
            get { return _formatHelper; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                _formatHelper = value;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate", Justification = "This is an existing API; this would be a breaking change")]
        public bool TryDeserialize(ControllerContext controllerContext, ModelBindingContext bindingContext, ContentType requestFormat, out object model)
        {
            for (int i = 0; i < this.RequestFormatHandlers.Count; ++i)
            {
                if (this.RequestFormatHandlers[i].CanDeserialize(requestFormat))
                {
                    model = this.RequestFormatHandlers[i].Deserialize(controllerContext, bindingContext, requestFormat);
                    return true;
                }
            }
            model = null;
            return false;
        }

        public bool CanDeserialize(ContentType contentType)
        {
            for (int i = 0; i < this.RequestFormatHandlers.Count; ++i)
            {
                if (this.RequestFormatHandlers[i].CanDeserialize(contentType))
                {
                    return true;
                }
            }
            return false;
        }

        public bool CanSerialize(ContentType responseFormat)
        {
            for (int i = 0; i < this.ResponseFormatHandlers.Count; ++i)
            {
                if (this.ResponseFormatHandlers[i].CanSerialize(responseFormat))
                {
                    return true;
                }
            }
            return false;
        }

        public void Serialize(ControllerContext context, object model, ContentType responseFormat)
        {
            for (int i = 0; i < this.ResponseFormatHandlers.Count; ++i)
            {
                if (this.ResponseFormatHandlers[i].CanSerialize(responseFormat))
                {
                    this.ResponseFormatHandlers[i].Serialize(context, model, responseFormat);
                    return;
                }
            }
            throw new NotSupportedException();
        }

        public bool TryMapFormatFriendlyName(string formatName, out ContentType contentType)
        {
            for (int i = 0; i < this.ResponseFormatHandlers.Count; ++i)
            {
                if (this.ResponseFormatHandlers[i].TryMapFormatFriendlyName(formatName, out contentType))
                {
                    return true;
                }
            }
            contentType = null;
            return false;
        }
    }
}
