// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Web.Http.Routing;

namespace System.Web.Http
{
    /// <summary>
    /// Annotates a controller with a route prefix that applies to actions that have any <see cref="RouteAttribute"/>s on them.
    /// </summary>
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "This attribute is intended to be extended by the user.")]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class RoutePrefixAttribute : Attribute, IRoutePrefix
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RoutePrefixAttribute" /> class without specifying any parameters.
        /// </summary>
        protected RoutePrefixAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RoutePrefixAttribute" /> class.
        /// </summary>
        /// <param name="prefix">The route prefix for the controller.</param>
        public RoutePrefixAttribute(string prefix)
        {
            if (prefix == null)
            {
                throw Error.ArgumentNull("prefix");
            }

            Prefix = prefix;
        }

        /// <summary>
        /// Gets the route prefix.
        /// </summary>
        public virtual string Prefix { get; private set; }
    }
}