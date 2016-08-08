// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http.Controllers;
using System.Web.Http.Properties;
using System.Web.Http.Routing;

namespace System.Web.Http.Dispatcher
{
    /// <summary>
    /// Default <see cref="IHttpControllerSelector"/> instance for choosing a <see cref="HttpControllerDescriptor"/> given a <see cref="HttpRequestMessage"/>
    /// A different implementation can be registered via the <see cref="HttpConfiguration.Services"/>.
    /// </summary>
    public class DefaultHttpControllerSelector : IHttpControllerSelector
    {
        public static readonly string ControllerSuffix = "Controller";

        private const string ControllerKey = "controller";

        private readonly HttpConfiguration _configuration;
        private readonly HttpControllerTypeCache _controllerTypeCache;
        private readonly Lazy<ConcurrentDictionary<string, HttpControllerDescriptor>> _controllerInfoCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultHttpControllerSelector"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public DefaultHttpControllerSelector(HttpConfiguration configuration)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            _controllerInfoCache = new Lazy<ConcurrentDictionary<string, HttpControllerDescriptor>>(InitializeControllerInfoCache);
            _configuration = configuration;
            _controllerTypeCache = new HttpControllerTypeCache(_configuration);
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Caller is responsible for disposing of response instance.")]
        public virtual HttpControllerDescriptor SelectController(HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            IHttpRouteData routeData = request.GetRouteData();
            HttpControllerDescriptor controllerDescriptor;
            if (routeData != null)
            {   
                controllerDescriptor = GetDirectRouteController(routeData);
                if (controllerDescriptor != null)
                {
                    return controllerDescriptor;
                }                
            }

            string controllerName = GetControllerName(request);
            if (String.IsNullOrEmpty(controllerName))
            {
                throw new HttpResponseException(request.CreateErrorResponse(
                    HttpStatusCode.NotFound,
                    Error.Format(SRResources.ResourceNotFound, request.RequestUri),
                    Error.Format(SRResources.ControllerNameNotFound, request.RequestUri)));
            }

            if (_controllerInfoCache.Value.TryGetValue(controllerName, out controllerDescriptor))
            {
                return controllerDescriptor;
            }

            ICollection<Type> matchingTypes = _controllerTypeCache.GetControllerTypes(controllerName);

            // ControllerInfoCache is already initialized.
            Contract.Assert(matchingTypes.Count != 1);

            if (matchingTypes.Count == 0)
            {
                // no matching types
                throw new HttpResponseException(request.CreateErrorResponse(
                    HttpStatusCode.NotFound,
                    Error.Format(SRResources.ResourceNotFound, request.RequestUri),
                    Error.Format(SRResources.DefaultControllerFactory_ControllerNameNotFound, controllerName)));
            }
            else
            {
                // multiple matching types
                throw CreateAmbiguousControllerException(request.GetRouteData().Route, controllerName, matchingTypes);
            }
        }

        public virtual IDictionary<string, HttpControllerDescriptor> GetControllerMapping()
        {
            return _controllerInfoCache.Value.ToDictionary(c => c.Key, c => c.Value, StringComparer.OrdinalIgnoreCase);
        }

        public virtual string GetControllerName(HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            IHttpRouteData routeData = request.GetRouteData();
            if (routeData == null)
            {
                return null;
            }

            // Look up controller in route data
            string controllerName = null;
            routeData.Values.TryGetValue(ControllerKey, out controllerName);
            return controllerName;
        }

        // If routeData is from an attribute route, get the controller that can handle it. 
        // Else return null. Throws an exception if multiple controllers match
        private static HttpControllerDescriptor GetDirectRouteController(IHttpRouteData routeData)
        {
            CandidateAction[] candidates = routeData.GetDirectRouteCandidates();
            if (candidates != null)
            {
                // Set the controller descriptor for the first action descriptor
                Contract.Assert(candidates.Length > 0);
                Contract.Assert(candidates[0].ActionDescriptor != null);

                HttpControllerDescriptor controllerDescriptor = candidates[0].ActionDescriptor.ControllerDescriptor;

                // Check that all other candidate action descriptors share the same controller descriptor
                for (int i = 1; i < candidates.Length; i++)
                {
                    CandidateAction candidate = candidates[i];
                    if (candidate.ActionDescriptor.ControllerDescriptor != controllerDescriptor)
                    {
                        // We've found an ambiguity (multiple controllers matched)
                        throw CreateDirectRouteAmbiguousControllerException(candidates);
                    }
                }

                return controllerDescriptor;
            }

            return null;
        }

        private static Exception CreateDirectRouteAmbiguousControllerException(CandidateAction[] candidates)
        {
            Contract.Assert(candidates != null);
            Contract.Assert(candidates.Length > 1);

            HashSet<Type> matchingTypes = new HashSet<Type>();
            for (int i = 0; i < candidates.Length; i++)
            {
                matchingTypes.Add(candidates[i].ActionDescriptor.ControllerDescriptor.ControllerType);
            }

            // we need to generate an exception containing all the controller types
            StringBuilder typeList = new StringBuilder();
            foreach (Type matchedType in matchingTypes)
            {
                typeList.AppendLine();
                typeList.Append(matchedType.FullName);
            }

            return Error.InvalidOperation(SRResources.DirectRoute_AmbiguousController, typeList, Environment.NewLine);
        }

        private static Exception CreateAmbiguousControllerException(IHttpRoute route, string controllerName, ICollection<Type> matchingTypes)
        {
            Contract.Assert(route != null);
            Contract.Assert(controllerName != null);
            Contract.Assert(matchingTypes != null);

            // Generate an exception containing all the controller types
            StringBuilder typeList = new StringBuilder();
            foreach (Type matchedType in matchingTypes)
            {
                typeList.AppendLine();
                typeList.Append(matchedType.FullName);
            }

            string errorMessage = Error.Format(SRResources.DefaultControllerFactory_ControllerNameAmbiguous_WithRouteTemplate, controllerName, route.RouteTemplate, typeList, Environment.NewLine);
            return new InvalidOperationException(errorMessage);
        }

        private ConcurrentDictionary<string, HttpControllerDescriptor> InitializeControllerInfoCache()
        {
            var result = new ConcurrentDictionary<string, HttpControllerDescriptor>(StringComparer.OrdinalIgnoreCase);
            var duplicateControllers = new HashSet<string>();
            Dictionary<string, ILookup<string, Type>> controllerTypeGroups = _controllerTypeCache.Cache;

            foreach (KeyValuePair<string, ILookup<string, Type>> controllerTypeGroup in controllerTypeGroups)
            {
                string controllerName = controllerTypeGroup.Key;

                foreach (IGrouping<string, Type> controllerTypesGroupedByNs in controllerTypeGroup.Value)
                {
                    foreach (Type controllerType in controllerTypesGroupedByNs)
                    {
                        if (result.Keys.Contains(controllerName))
                        {
                            duplicateControllers.Add(controllerName);
                            break;
                        }
                        else
                        {
                            result.TryAdd(controllerName, new HttpControllerDescriptor(_configuration, controllerName, controllerType));
                        }
                    }
                }
            }

            foreach (string duplicateController in duplicateControllers)
            {
                HttpControllerDescriptor descriptor;
                result.TryRemove(duplicateController, out descriptor);
            }

            return result;
        }
    }
}
