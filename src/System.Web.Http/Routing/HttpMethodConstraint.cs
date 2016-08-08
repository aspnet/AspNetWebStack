// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;

namespace System.Web.Http.Routing
{
    public class HttpMethodConstraint : IHttpRouteConstraint
    {
        public HttpMethodConstraint(params HttpMethod[] allowedMethods)
        {
            if (allowedMethods == null)
            {
                throw Error.ArgumentNull("allowedMethods");
            }

            AllowedMethods = new Collection<HttpMethod>(allowedMethods);
        }

        public Collection<HttpMethod> AllowedMethods { get; private set; }

        protected virtual bool Match(HttpRequestMessage request, IHttpRoute route, string parameterName, IDictionary<string, object> values, HttpRouteDirection routeDirection)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            if (route == null)
            {
                throw Error.ArgumentNull("route");
            }

            if (parameterName == null)
            {
                throw Error.ArgumentNull("parameterName");
            }

            if (values == null)
            {
                throw Error.ArgumentNull("values");
            }

            switch (routeDirection)
            {
                case HttpRouteDirection.UriResolution:
                    return AllowedMethods.Contains(request.Method);

                case HttpRouteDirection.UriGeneration:
                    // We need to see if the user specified the HTTP method explicitly.  Consider these two routes:
                    //
                    // a) Route: template = "/{foo}", Constraints = { httpMethod = new HttpMethodConstraint("GET") }
                    // b) Route: template = "/{foo}", Constraints = { httpMethod = new HttpMethodConstraint("POST") }
                    //
                    // A user might know ahead of time that a URI he/she is generating might be used with a particular HTTP
                    // method.  If a URI will be used for an HTTP POST but we match on (a) while generating the URI, then
                    // the HTTP GET-specific route will be used for URI generation, which might have undesired behavior.
                    // To prevent this, a user might call RouteCollection.GetVirtualPath(..., { httpMethod = "POST" }) to
                    // signal that he is generating a URI that will be used for an HTTP POST, so he wants the URI
                    // generation to be performed by the (b) route instead of the (a) route, consistent with what would
                    // happen on incoming requests.
                    HttpMethod constraint;
                    if (!values.TryGetValue(parameterName, out constraint))
                    {
                        return true;
                    }

                    return AllowedMethods.Contains(constraint);

                default:
                    throw Error.InvalidEnumArgument(String.Empty, (int)routeDirection, typeof(HttpRouteDirection));
            }
        }

        bool IHttpRouteConstraint.Match(HttpRequestMessage request, IHttpRoute route, string parameterName, IDictionary<string, object> values, HttpRouteDirection routeDirection)
        {
            return Match(request, route, parameterName, values, routeDirection);
        }
    }
}
