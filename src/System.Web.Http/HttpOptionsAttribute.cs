// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.ObjectModel;
using System.Net.Http;
using System.Web.Http.Controllers;

namespace System.Web.Http
{
    /// <summary>
    /// Specifies that an action supports the OPTIONS HTTP method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class HttpOptionsAttribute : Attribute, IActionHttpMethodProvider
    {
        private static readonly Collection<HttpMethod> _supportedMethods = new Collection<HttpMethod>(new HttpMethod[] { HttpMethod.Options });

        public Collection<HttpMethod> HttpMethods
        {
            get
            {
                return _supportedMethods;
            }
        }
    }
}
