// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Web.Mvc;
using Microsoft.Web.Mvc.Properties;

namespace Microsoft.Web.Mvc
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public sealed class ContentTypeAttribute : ActionFilterAttribute
    {
        public ContentTypeAttribute(string contentType)
        {
            if (String.IsNullOrEmpty(contentType))
            {
                throw new ArgumentException(MvcResources.Common_NullOrEmpty, "contentType");
            }

            ContentType = contentType;
        }

        public string ContentType { get; private set; }

        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            filterContext.HttpContext.Response.ContentType = ContentType;
        }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            filterContext.HttpContext.Response.ContentType = ContentType;
        }
    }
}
