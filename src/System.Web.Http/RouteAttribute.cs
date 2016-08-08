// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Web.Http.Routing;

namespace System.Web.Http
{
    /// <summary>
    /// Place on a controller or action to expose it directly via a route. 
    /// When placed on a controller, it applies to actions that do not have any <see cref="RouteAttribute"/>s on them.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public sealed class RouteAttribute : Attribute, IDirectRouteFactory, IHttpRouteInfoProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RouteAttribute" /> class.
        /// </summary>
        public RouteAttribute()
        {
            Template = String.Empty;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="RouteAttribute" /> class.
        /// </summary>
        /// <param name="template">The route template describing the URI pattern to match against.</param>
        public RouteAttribute(string template)
        {
            if (template == null)
            {
                throw Error.ArgumentNull("template");
            }
            Template = template;
        }

        /// <inheritdoc />
        public string Name { get; set; }

        /// <inheritdoc />
        public int Order { get; set; }

        /// <inheritdoc />
        public string Template { get; private set; }

        RouteEntry IDirectRouteFactory.CreateRoute(DirectRouteFactoryContext context)
        {
            Contract.Assert(context != null);

            IDirectRouteBuilder builder = context.CreateBuilder(Template);
            Contract.Assert(builder != null);

            builder.Name = Name;
            builder.Order = Order;
            return builder.Build();
        }
    }
}
