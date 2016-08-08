// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.WebPages.Html;
using Moq;

namespace System.Web.WebPages.Test
{
    public static class HtmlHelperFactory
    {
        internal static HtmlHelper Create(ModelStateDictionary modelStateDictionary = null, ValidationHelper validationHelper = null)
        {
            modelStateDictionary = modelStateDictionary ?? new ModelStateDictionary();
            var httpContext = new Mock<HttpContextBase>();
            validationHelper = validationHelper ?? new ValidationHelper(httpContext.Object, modelStateDictionary);
            return new HtmlHelper(modelStateDictionary, validationHelper);
        }
    }
}
