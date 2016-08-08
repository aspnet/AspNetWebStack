// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Threading;
using System.Web.Http.Controllers;
using System.Web.Http.Internal;
using System.Web.Http.Properties;

namespace System.Web.Http.Dispatcher
{
    /// <summary>
    /// Default implementation of an <see cref="IHttpControllerActivator"/>.
    /// A different implementation can be registered via the <see cref="T:System.Web.Http.Services.DependencyResolver"/>.
    /// We optimize for the case where we have an <see cref="Controllers.ApiControllerActionInvoker"/> 
    /// instance per <see cref="HttpControllerDescriptor"/> instance but can support cases where there are
    /// many <see cref="HttpControllerDescriptor"/> instances for one <see cref="System.Web.Http.Controllers.ApiControllerActionInvoker"/> 
    /// as well. In the latter case the lookup is slightly slower because it goes through the 
    /// <see cref="P:HttpControllerDescriptor.Properties"/> dictionary.
    /// </summary>
    public class DefaultHttpControllerActivator : IHttpControllerActivator
    {
        private Tuple<HttpControllerDescriptor, Func<IHttpController>> _fastCache;
        private object _cacheKey = new object();

        /// <summary>
        /// Creates the <see cref="IHttpController"/> specified by <paramref name="controllerType"/> using the given <paramref name="request"/>
        /// </summary>
        /// <param name="request">The request message.</param>
        /// <param name="controllerType">Type of the controller.</param>
        /// <param name="controllerDescriptor">The controller descriptor</param>
        /// <returns>An instance of type <paramref name="controllerType"/>.</returns>
        public IHttpController Create(HttpRequestMessage request, HttpControllerDescriptor controllerDescriptor, Type controllerType)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            if (controllerDescriptor == null)
            {
                throw Error.ArgumentNull("controllerDescriptor");
            }

            if (controllerType == null)
            {
                throw Error.ArgumentNull("controllerType");
            }

            try
            {
                Func<IHttpController> activator;

                // First check in the local fast cache and if not a match then look in the broader 
                // HttpControllerDescriptor.Properties cache
                if (_fastCache == null)
                {
                    IHttpController controller = GetInstanceOrActivator(request, controllerType, out activator);
                    if (controller != null)
                    {
                        // we have a controller registered with the dependency resolver for this controller type
                        return controller;
                    }
                    else
                    {
                        Tuple<HttpControllerDescriptor, Func<IHttpController>> cacheItem = Tuple.Create(controllerDescriptor, activator);
                        Interlocked.CompareExchange(ref _fastCache, cacheItem, null);
                    }
                }
                else if (_fastCache.Item1 == controllerDescriptor)
                {
                    // If the key matches and we already have the delegate for creating an instance.
                    activator = _fastCache.Item2;
                }
                else
                {
                    // If the key doesn't match then lookup/create delegate in the HttpControllerDescriptor.Properties for
                    // that HttpControllerDescriptor instance
                    object value;
                    if (controllerDescriptor.Properties.TryGetValue(_cacheKey, out value))
                    {
                        activator = (Func<IHttpController>)value;
                    }
                    else
                    {
                        IHttpController controller = GetInstanceOrActivator(request, controllerType, out activator);
                        if (controller != null)
                        {
                            // we have a controller registered with the dependency resolver for this controller type
                            return controller;
                        }
                        else
                        {
                            controllerDescriptor.Properties.TryAdd(_cacheKey, activator);
                        }
                    }
                }

                return activator();
            }
            catch (Exception ex)
            {
                throw Error.InvalidOperation(ex, SRResources.DefaultControllerFactory_ErrorCreatingController, controllerType.Name);
            }
        }

        // Returns the controller instance from the dependency resolver if there is one registered
        // else returns the activator that calls the default ctor for the give controllerType.
        private static IHttpController GetInstanceOrActivator(HttpRequestMessage request, Type controllerType, out Func<IHttpController> activator)
        {
            Contract.Assert(request != null);
            Contract.Assert(controllerType != null);

            // If dependency resolver returns controller object then use it.
            IHttpController instance = (IHttpController)request.GetDependencyScope().GetService(controllerType);
            if (instance != null)
            {
                activator = null;
                return instance;
            }

            // Otherwise create a delegate for creating a new instance of the type
            activator = TypeActivator.Create<IHttpController>(controllerType);
            return null;
        }
    }
}
