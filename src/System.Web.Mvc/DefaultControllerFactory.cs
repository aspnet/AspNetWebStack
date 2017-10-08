// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Mvc.Properties;
using System.Web.Mvc.Routing;
using System.Web.Routing;
using System.Web.SessionState;

namespace System.Web.Mvc
{
    public class DefaultControllerFactory : IControllerFactory
    {
        private static readonly ConcurrentDictionary<Type, SessionStateBehavior> _sessionStateCache = new ConcurrentDictionary<Type, SessionStateBehavior>();
        private static ControllerTypeCache _staticControllerTypeCache = new ControllerTypeCache();
        private IBuildManager _buildManager;
        private IResolver<IControllerActivator> _activatorResolver;
        private IControllerActivator _controllerActivator;
        private ControllerBuilder _controllerBuilder;
        private ControllerTypeCache _instanceControllerTypeCache;

        public DefaultControllerFactory()
            : this(null, null, null)
        {
        }

        public DefaultControllerFactory(IControllerActivator controllerActivator)
            : this(controllerActivator, null, null)
        {
        }

        internal DefaultControllerFactory(IControllerActivator controllerActivator, IResolver<IControllerActivator> activatorResolver, IDependencyResolver dependencyResolver)
        {
            if (controllerActivator != null)
            {
                _controllerActivator = controllerActivator;
            }
            else
            {
                _activatorResolver = activatorResolver ?? new SingleServiceResolver<IControllerActivator>(
                                                              () => null,
                                                              new DefaultControllerActivator(dependencyResolver),
                                                              "DefaultControllerFactory constructor");
            }
        }

        private IControllerActivator ControllerActivator
        {
            get
            {
                if (_controllerActivator != null)
                {
                    return _controllerActivator;
                }
                _controllerActivator = _activatorResolver.Current;
                return _controllerActivator;
            }
        }

        internal IBuildManager BuildManager
        {
            get
            {
                if (_buildManager == null)
                {
                    _buildManager = new BuildManagerWrapper();
                }
                return _buildManager;
            }
            set { _buildManager = value; }
        }

        internal ControllerBuilder ControllerBuilder
        {
            get { return _controllerBuilder ?? ControllerBuilder.Current; }
            set { _controllerBuilder = value; }
        }

        internal ControllerTypeCache ControllerTypeCache
        {
            get { return _instanceControllerTypeCache ?? _staticControllerTypeCache; }
            set { _instanceControllerTypeCache = value; }
        }

        internal static InvalidOperationException CreateAmbiguousControllerException(RouteBase route, string controllerName, ICollection<Type> matchingTypes)
        {
            // we need to generate an exception containing all the controller types
            StringBuilder typeList = new StringBuilder();
            foreach (Type matchedType in matchingTypes)
            {
                typeList.AppendLine();
                typeList.Append(matchedType.FullName);
            }

            string errorText;
            Route castRoute = route as Route;
            if (castRoute != null)
            {
                errorText = String.Format(CultureInfo.CurrentCulture, MvcResources.DefaultControllerFactory_ControllerNameAmbiguous_WithRouteUrl,
                                          controllerName, castRoute.Url, typeList, Environment.NewLine);
            }
            else
            {
                errorText = String.Format(CultureInfo.CurrentCulture, MvcResources.DefaultControllerFactory_ControllerNameAmbiguous_WithoutRouteUrl,
                                          controllerName, typeList, Environment.NewLine);
            }

            return new InvalidOperationException(errorText);
        }

        private static InvalidOperationException CreateDirectRouteAmbiguousControllerException(ICollection<Type> matchingTypes)
        {
            // we need to generate an exception containing all the controller types
            StringBuilder typeList = new StringBuilder();
            foreach (Type matchedType in matchingTypes)
            {
                typeList.AppendLine();
                typeList.Append(matchedType.FullName);
            }

            string errorText = String.Format(
                CultureInfo.CurrentCulture,
                MvcResources.DefaultControllerFactory_DirectRouteAmbiguous,
                typeList,
                Environment.NewLine);

            return new InvalidOperationException(errorText);
        }

        public virtual IController CreateController(RequestContext requestContext, string controllerName)
        {
            if (requestContext == null)
            {
                throw new ArgumentNullException("requestContext");
            }

            if (String.IsNullOrEmpty(controllerName) &&
                (requestContext.RouteData == null || !requestContext.RouteData.HasDirectRouteMatch()))
            {
                throw new ArgumentException(MvcResources.Common_NullOrEmpty, "controllerName");
            }

            Type controllerType = GetControllerType(requestContext, controllerName);
            IController controller = GetControllerInstance(requestContext, controllerType);
            return controller;
        }

        protected internal virtual IController GetControllerInstance(RequestContext requestContext, Type controllerType)
        {
            if (requestContext == null)
            {
                throw new ArgumentNullException("requestContext");
            }
            if (controllerType == null)
            {
                throw new HttpException(404,
                                        String.Format(
                                            CultureInfo.CurrentCulture,
                                            MvcResources.DefaultControllerFactory_NoControllerFound,
                                            requestContext.HttpContext.Request.Path));
            }
            if (!typeof(IController).IsAssignableFrom(controllerType))
            {
                throw new ArgumentException(
                    String.Format(
                        CultureInfo.CurrentCulture,
                        MvcResources.DefaultControllerFactory_TypeDoesNotSubclassControllerBase,
                        controllerType),
                    "controllerType");
            }

            return ControllerActivator.Create(requestContext, controllerType);
        }

        protected internal virtual SessionStateBehavior GetControllerSessionBehavior(RequestContext requestContext, Type controllerType)
        {
            if (controllerType == null)
            {
                return SessionStateBehavior.Default;
            }

            return _sessionStateCache.GetOrAdd(
                controllerType,
                type =>
                {
                    var attr = type.GetCustomAttributes(typeof(SessionStateAttribute), inherit: true)
                        .OfType<SessionStateAttribute>()
                        .FirstOrDefault();

                    return (attr != null) ? attr.Behavior : SessionStateBehavior.Default;
                });
        }

        protected internal virtual Type GetControllerType(RequestContext requestContext, string controllerName)
        {
            if (requestContext == null)
            {
                throw new ArgumentNullException("requestContext");
            }

            if (String.IsNullOrEmpty(controllerName) &&
                (requestContext.RouteData == null || !requestContext.RouteData.HasDirectRouteMatch()))
            {
                throw new ArgumentException(MvcResources.Common_NullOrEmpty, "controllerName");
            }

            RouteData routeData = requestContext.RouteData;
            if (routeData != null && routeData.HasDirectRouteMatch())
            {
                return GetControllerTypeFromDirectRoute(routeData);
            }

            // first search in the current route's namespace collection
            object routeNamespacesObj;
            Type match;
            if (routeData != null && routeData.DataTokens.TryGetValue(RouteDataTokenKeys.Namespaces, out routeNamespacesObj))
            {
                IEnumerable<string> routeNamespaces = routeNamespacesObj as IEnumerable<string>;
                if (routeNamespaces != null && routeNamespaces.Any())
                {
                    HashSet<string> namespaceHash = new HashSet<string>(routeNamespaces, StringComparer.OrdinalIgnoreCase);
                    match = GetControllerTypeWithinNamespaces(routeData.Route, controllerName, namespaceHash);

                    // the UseNamespaceFallback key might not exist, in which case its value is implicitly "true"
                    if (match != null || false.Equals(routeData.DataTokens[RouteDataTokenKeys.UseNamespaceFallback]))
                    {
                        // got a match or the route requested we stop looking
                        return match;
                    }
                }
            }

            // then search in the application's default namespace collection
            RouteBase route = routeData == null ? null : routeData.Route;
            if (ControllerBuilder.DefaultNamespaces.Count > 0)
            {
                HashSet<string> namespaceDefaults = new HashSet<string>(ControllerBuilder.DefaultNamespaces, StringComparer.OrdinalIgnoreCase);
                match = GetControllerTypeWithinNamespaces(route, controllerName, namespaceDefaults);
                if (match != null)
                {
                    return match;
                }
            }

            // if all else fails, search every namespace
            return GetControllerTypeWithinNamespaces(route, controllerName, null /* namespaces */);
        }

        private static Type GetControllerTypeFromDirectRoute(RouteData routeData)
        {
            Contract.Assert(routeData != null);

            var matchingRouteDatas = routeData.GetDirectRouteMatches();

            List<Type> controllerTypes = new List<Type>();
            foreach (var directRouteData in matchingRouteDatas)
            {
                if (directRouteData != null)
                {
                    Type controllerType = directRouteData.GetTargetControllerType();
                    if (controllerType == null)
                    {
                        // We don't expect this to happen, but it could happen if some code messes with the 
                        // route data tokens and removes the key we're looking for. 
                        throw new InvalidOperationException(MvcResources.DirectRoute_MissingControllerType);
                    }

                    if (!controllerTypes.Contains(controllerType))
                    {
                        controllerTypes.Add(controllerType);
                    }
                }
            }

            // We only want to handle the case where all matched direct routes refer to the same controller.
            // Handling the multiple-controllers case would put attribute routing down a totally different
            // path than traditional routing.
            if (controllerTypes.Count == 0)
            {
                return null;
            }
            else if (controllerTypes.Count == 1)
            {
                return controllerTypes[0];
            }
            else
            {
                throw CreateDirectRouteAmbiguousControllerException(controllerTypes);
            }
        }

        private Type GetControllerTypeWithinNamespaces(RouteBase route, string controllerName, HashSet<string> namespaces)
        {
            // Once the master list of controllers has been created we can quickly index into it
            ControllerTypeCache.EnsureInitialized(BuildManager);

            ICollection<Type> matchingTypes = ControllerTypeCache.GetControllerTypes(controllerName, namespaces);
            switch (matchingTypes.Count)
            {
                case 0:
                    // no matching types
                    return null;

                case 1:
                    // single matching type
                    return matchingTypes.First();

                default:
                    // multiple matching types
                    throw CreateAmbiguousControllerException(route, controllerName, matchingTypes);
            }
        }

        public virtual void ReleaseController(IController controller)
        {
            IDisposable disposable = controller as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }

        internal IReadOnlyList<Type> GetControllerTypes()
        {
            ControllerTypeCache.EnsureInitialized(BuildManager);
            return ControllerTypeCache.GetControllerTypes();
        }

        SessionStateBehavior IControllerFactory.GetControllerSessionBehavior(RequestContext requestContext, string controllerName)
        {
            if (requestContext == null)
            {
                throw new ArgumentNullException("requestContext");
            }
            if (String.IsNullOrEmpty(controllerName))
            {
                throw new ArgumentException(MvcResources.Common_NullOrEmpty, "controllerName");
            }

            Type controllerType = GetControllerType(requestContext, controllerName);
            return GetControllerSessionBehavior(requestContext, controllerType);
        }

        private class DefaultControllerActivator : IControllerActivator
        {
            private Func<IDependencyResolver> _resolverThunk;

            public DefaultControllerActivator()
                : this(null)
            {
            }

            public DefaultControllerActivator(IDependencyResolver resolver)
            {
                if (resolver == null)
                {
                    _resolverThunk = () => DependencyResolver.Current;
                }
                else
                {
                    _resolverThunk = () => resolver;
                }
            }

            public IController Create(RequestContext requestContext, Type controllerType)
            {
                try
                {
                    return (IController)(_resolverThunk().GetService(controllerType) ?? Activator.CreateInstance(controllerType));
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(
                        String.Format(
                            CultureInfo.CurrentCulture,
                            MvcResources.DefaultControllerFactory_ErrorCreatingController,
                            controllerType),
                        ex);
                }
            }
        }
    }
}
