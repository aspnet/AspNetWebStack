// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Http.Controllers;
using System.Web.Http.Metadata;

namespace System.Web.Http.ModelBinding
{
    public delegate string ModelBinderErrorMessageProvider(HttpActionContext actionContext, ModelMetadata modelMetadata, object incomingValue);
}
