// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Http.ModelBinding;

namespace System.Web.Http
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class HttpBindRequiredAttribute : HttpBindingBehaviorAttribute
    {
        public HttpBindRequiredAttribute()
            : base(HttpBindingBehavior.Required)
        {
        }
    }
}
