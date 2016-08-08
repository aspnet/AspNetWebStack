// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Http.ModelBinding
{
    /// <summary>
    /// End to end functional tests for model binding
    /// </summary>
    public abstract class ModelBindingTests : HttpServerTestBase
    {
        protected ModelBindingTests()
            : base("http://localhost/")
        {
        }

        protected override void ApplyConfiguration(HttpConfiguration configuration)
        {
            configuration.Routes.MapHttpRoute("Default", "{controller}/{action}", new { controller = "ModelBinding" });
        }
    }
}