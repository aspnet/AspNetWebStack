// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Net.Mime;
using System.Web;
using System.Web.Mvc;
using Microsoft.Web.Mvc.Properties;

namespace Microsoft.Web.Mvc.Resources
{
    /// <summary>
    /// ModelBinder implementation that augments the inner model binder with support for binding to other formats -
    /// XML and JSON by default.
    /// </summary>
    public class ResourceModelBinder : IModelBinder
    {
        private IModelBinder _inner;

        /// <summary>
        /// Wraps the ModelBinders.Binders.DefaultBinder
        /// </summary>
        public ResourceModelBinder()
            : this(ModelBinders.Binders.DefaultBinder)
        {
        }

        public ResourceModelBinder(IModelBinder inner)
        {
            this._inner = inner;
        }

        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            if (WebApiEnabledAttribute.IsDefined(controllerContext.Controller))
            {
                if (!controllerContext.RouteData.Values.ContainsKey(bindingContext.ModelName) && controllerContext.HttpContext.Request.HasBody())
                {
                    ContentType requestFormat = controllerContext.RequestContext.GetRequestFormat();
                    object model;
                    if (TryBindModel(controllerContext, bindingContext, requestFormat, out model))
                    {
                        bindingContext.ModelMetadata.Model = model;
                        MyDefaultModelBinder dmb = new MyDefaultModelBinder();
                        dmb.CallOnModelUpdated(controllerContext, bindingContext);
                        if (!MyDefaultModelBinder.IsModelValid(bindingContext))
                        {
                            List<ModelError> details = new List<ModelError>();
                            foreach (ModelState ms in bindingContext.ModelState.Values)
                            {
                                foreach (ModelError me in ms.Errors)
                                {
                                    details.Add(me);
                                }
                            }
                            HttpException failure = new HttpException((int)HttpStatusCode.ExpectationFailed, "Invalid Model");
                            failure.Data["details"] = details;
                            throw failure;
                        }
                        return model;
                    }
                    throw new HttpException((int)HttpStatusCode.UnsupportedMediaType, String.Format(CultureInfo.CurrentCulture, MvcResources.Resources_UnsupportedMediaType, (requestFormat == null ? String.Empty : requestFormat.MediaType)));
                }
            }
            return this._inner.BindModel(controllerContext, bindingContext);
        }

        [SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate", Justification = "This is an existing API; this would be a breaking change")]
        public bool TryBindModel(ControllerContext controllerContext, ModelBindingContext bindingContext, ContentType requestFormat, out object model)
        {
            if (requestFormat != null && String.Compare(requestFormat.MediaType, FormatManager.UrlEncoded, StringComparison.OrdinalIgnoreCase) == 0)
            {
                model = this._inner.BindModel(controllerContext, bindingContext);
                return true;
            }
            if (!FormatManager.Current.TryDeserialize(controllerContext, bindingContext, requestFormat, out model))
            {
                model = null;
                return false;
            }
            return true;
        }

        private class MyDefaultModelBinder : DefaultModelBinder
        {
            public void CallOnModelUpdated(ControllerContext controllerContext, ModelBindingContext bindingContext)
            {
                OnModelUpdated(controllerContext, bindingContext);
            }

            internal static new bool IsModelValid(ModelBindingContext bindingContext)
            {
                return DefaultModelBinder.IsModelValid(bindingContext);
            }
        }
    }
}
