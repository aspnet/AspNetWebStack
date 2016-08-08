// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Web.Mvc.Routing;

namespace System.Web.Mvc
{
    /// <summary>
    /// Annotates a controller with a route prefix that applies to all actions within the controller.
    /// </summary>
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "This attribute is intended to be extended by the user.")]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
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
                throw new ArgumentNullException("prefix");
            }

            Prefix = prefix;
        }

        /// <summary>
        /// Gets the route prefix.
        /// </summary>
        public virtual string Prefix { get; private set; }
    }
}