// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;

#if ASPNETWEBAPI
using TRouteInfoProvider = System.Web.Http.Routing.IHttpRouteInfoProvider;
#else
using System.Web.Mvc.Properties;
using TRouteInfoProvider = System.Web.Mvc.Routing.IRouteInfoProvider;
#endif

#if ASPNETWEBAPI
namespace System.Web.Http.Routing
#else
namespace System.Web.Mvc.Routing
#endif
{
    /// <remarks>
    /// This class is an adapter that turns an IHttpRouteInfoProvider into an IDirectRouteFactory. We need it because
    /// we already shipped IHttpRouteInfoProvider but want to standardize the internal implementation around the more
    /// general IDirectRouteFactory interface.
    /// We can remove this class if we ever stop supporting custom attributes that implement IHttpRouteInfoProvider.
    /// </remarks>
    internal class RouteInfoDirectRouteFactory : IDirectRouteFactory
    {
        private readonly TRouteInfoProvider _infoProvider;

        public RouteInfoDirectRouteFactory(TRouteInfoProvider infoProvider)
        {
            if (infoProvider == null)
            {
                throw new ArgumentNullException("infoProvider");
            }

            _infoProvider = infoProvider;
        }

        public RouteEntry CreateRoute(DirectRouteFactoryContext context)
        {
            Contract.Assert(context != null);

            IDirectRouteBuilder builder = context.CreateBuilder(_infoProvider.Template);
            Contract.Assert(builder != null);

            builder.Name = _infoProvider.Name;

#if ASPNETWEBAPI
            builder.Order = _infoProvider.Order;
#endif

            return builder.Build();
        }
    }
}
