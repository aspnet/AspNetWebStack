// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Http.Controllers;

namespace System.Web.Http.ModelBinding
{
    /// <summary>
    /// Interface for model binding.
    /// </summary>
    public interface IModelBinder
    {
        bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext);
    }
}
