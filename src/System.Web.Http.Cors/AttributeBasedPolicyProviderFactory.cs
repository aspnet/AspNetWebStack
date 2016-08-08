// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Web.Cors;
using System.Web.Http.Controllers;
using System.Web.Http.Cors.Properties;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;

namespace System.Web.Http.Cors
{
    /// <summary>
    /// An implementation of <see cref="ICorsPolicyProviderFactory"/> that returns the <see cref="ICorsPolicyProvider"/> from the controller or action attribute.
    /// </summary>
    public class AttributeBasedPolicyProviderFactory : ICorsPolicyProviderFactory
    {
        private const string HttpContextBaseKey = "MS_HttpContext";

        /// <summary>
        /// Gets or sets the default <see cref="ICorsPolicyProvider"/>.
        /// </summary>
        public ICorsPolicyProvider DefaultPolicyProvider { get; set; }

        /// <summary>
        /// Gets the <see cref="ICorsPolicyProvider" /> for the request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>
        /// The <see cref="ICorsPolicyProvider" />.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">request</exception>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The object is registered for disposal when the request message is disposed.")]
        public virtual ICorsPolicyProvider GetCorsPolicyProvider(HttpRequestMessage request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            CorsRequestContext corsRequestContext = request.GetCorsRequestContext();
            HttpActionDescriptor actionDescriptor = null;
            if (corsRequestContext.IsPreflight)
            {
                HttpRequestMessage targetRequest = new HttpRequestMessage(new HttpMethod(corsRequestContext.AccessControlRequestMethod), request.RequestUri);

                request.RegisterForDispose(targetRequest);

                try
                {
                    foreach (var property in request.Properties)
                    {
                        // The RouteData and HttpContext from the preflight request properties contain information
                        // relevant to the preflight request and not the actual request, therefore we need to exclude them.
                        if (property.Key != HttpPropertyKeys.HttpRouteDataKey &&
                            property.Key != HttpContextBaseKey)
                        {
                            targetRequest.Properties.Add(property.Key, property.Value);
                        }
                    }

                    HttpConfiguration config = request.GetConfiguration();
                    if (config == null)
                    {
                        throw new InvalidOperationException(SRResources.NoConfiguration);
                    }

                    IHttpRouteData routeData = config.Routes.GetRouteData(request);
                    if (routeData == null)
                    {
                        // No route data found for selecting action with EnableCorsAttribute, thus no ICorsPolicyProvider is returned
                        // and let the CorsMessageHandler flow the request to the normal Web API pipeline.
                        return null;
                    }

                    actionDescriptor = SelectAction(targetRequest, routeData, config);
                }
                catch
                {
                    if (DefaultPolicyProvider != null)
                    {
                        return DefaultPolicyProvider;
                    }
                    throw;
                }
            }
            else
            {
                actionDescriptor = request.GetActionDescriptor();
            }

            return GetCorsPolicyProvider(actionDescriptor);
        }

        private ICorsPolicyProvider GetCorsPolicyProvider(HttpActionDescriptor actionDescriptor)
        {
            ICorsPolicyProvider policyProvider = null;
            if (actionDescriptor != null)
            {
                HttpControllerDescriptor controllerDescriptor = actionDescriptor.ControllerDescriptor;
                policyProvider = actionDescriptor.GetCustomAttributes<ICorsPolicyProvider>().FirstOrDefault();
                if (policyProvider == null && controllerDescriptor != null)
                {
                    policyProvider = controllerDescriptor.GetCustomAttributes<ICorsPolicyProvider>().FirstOrDefault();
                }
            }

            if (policyProvider == null)
            {
                policyProvider = DefaultPolicyProvider;
            }

            return policyProvider;
        }

        private static HttpActionDescriptor SelectAction(HttpRequestMessage request, IHttpRouteData routeData, HttpConfiguration config)
        {
            request.SetRouteData(routeData);

            routeData.RemoveOptionalRoutingParameters();

            HttpControllerDescriptor controllerDescriptor = config.Services.GetHttpControllerSelector().SelectController(request);

            // Get the per-controller configuration
            config = controllerDescriptor.Configuration;
            request.SetConfiguration(config);
            HttpRequestContext requestContext = request.GetRequestContext();

            if (requestContext == null)
            {
                requestContext = new HttpRequestContext
                {
                    Configuration = config,
                    RouteData = routeData,
                    Url = new UrlHelper(request),
                    VirtualPathRoot = config.VirtualPathRoot
                };
            }

            IHttpController controller = controllerDescriptor.CreateController(request);
            using (controller as IDisposable)
            {
                HttpControllerContext controllerContext = new HttpControllerContext(requestContext, request, controllerDescriptor, controller);
                return config.Services.GetActionSelector().SelectAction(controllerContext);
            }
        }
    }
}