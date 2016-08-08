// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http.Controllers;
using System.Web.Http.Internal;

namespace System.Web.Http.Description
{
    /// <summary>
    /// Describes a parameter on the API defined by relative URI path and HTTP method.
    /// </summary>
    public class ApiParameterDescription
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the documentation.
        /// </summary>
        /// <value>
        /// The documentation.
        /// </value>
        public string Documentation { get; set; }

        /// <summary>
        /// Gets or sets the source of the parameter. It may come from the request URI, request body or other places.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        public ApiParameterSource Source { get; set; }

        /// <summary>
        /// Gets or sets the parameter descriptor.
        /// </summary>
        /// <value>
        /// The parameter descriptor. <see langref="null"/> if (and only if) a <see cref="ApiParameterSource.FromUri"/>
        /// parameter is declared in route template but unused in the API. Never-<see langref="null"/> for other
        /// sources.
        /// </value>
        /// <remarks>
        /// For more information on the <see langref="null"/> case, search <see cref="ApiExplorer"/> for "undeclared"
        /// route parameter handling.
        /// </remarks>
        public HttpParameterDescriptor ParameterDescriptor { get; set; }

        internal IEnumerable<PropertyInfo> GetBindableProperties()
        {
            return GetBindableProperties(ParameterDescriptor.ParameterType);
        }

        internal bool CanConvertPropertiesFromString()
        {
            return GetBindableProperties().All(p => TypeHelper.CanConvertFromString(p.PropertyType));
        }

        internal static IEnumerable<PropertyInfo> GetBindableProperties(Type type)
        {
            return type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                       .Where(p => p.GetGetMethod() != null && p.GetSetMethod() != null);
        }
    }
}
