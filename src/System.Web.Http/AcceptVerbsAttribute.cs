// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;

namespace System.Web.Http
{
    /// <summary>
    /// Specifies what HTTP methods an action supports.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments", Justification = "The accessor is exposed as an Collection<HttpMethod>.")]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class AcceptVerbsAttribute : Attribute, IActionHttpMethodProvider
    {
        private readonly Collection<HttpMethod> _httpMethods;

        /// <summary>
        /// Initializes a new instance of the <see cref="AcceptVerbsAttribute" /> class.        
        /// </summary>
        /// <param name="method">The HTTP method the action supports.</param>
        /// <remarks>
        /// This is a CLS compliant constructor.
        /// </remarks>
        public AcceptVerbsAttribute(string method)
            : this(new string[] { method })
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="AcceptVerbsAttribute" /> class.
        /// </summary>
        /// <param name="methods">The HTTP methods the action supports.</param>
        /// <remarks>
        /// This constructor is not CLS-compliant.
        /// </remarks>
        public AcceptVerbsAttribute(params string[] methods)
        {
            _httpMethods = methods != null
                                   ? new Collection<HttpMethod>(methods.Select(method => HttpMethodHelper.GetHttpMethod(method)).ToArray())
                                   : new Collection<HttpMethod>(new HttpMethod[0]);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AcceptVerbsAttribute" /> class.
        /// </summary>
        /// <param name="methods">The HTTP methods the action supports.</param>
        internal AcceptVerbsAttribute(params HttpMethod[] methods)
        {
            _httpMethods = new Collection<HttpMethod>(methods);
        }

        /// <summary>
        /// Gets the HTTP methods the action supports.
        /// </summary>
        public Collection<HttpMethod> HttpMethods
        {
            get
            {
                return _httpMethods;
            }
        }
    }
}
