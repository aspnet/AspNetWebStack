// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Web.Http.Dispatcher;
using System.Web.Http.Filters;
using System.Web.Http.Internal;

namespace System.Web.Http.Controllers
{
    /// <summary>
    /// Description and configuration for a controller.
    /// </summary>
    public class HttpControllerDescriptor
    {
        private readonly ConcurrentDictionary<object, object> _properties = new ConcurrentDictionary<object, object>();

        private HttpConfiguration _configuration;

        private string _controllerName;
        private Type _controllerType;

        private object[] _attributeCache;
        private object[] _declaredOnlyAttributeCache;

        public HttpControllerDescriptor(HttpConfiguration configuration, string controllerName, Type controllerType)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            if (controllerName == null)
            {
                throw Error.ArgumentNull("controllerName");
            }

            if (controllerType == null)
            {
                throw Error.ArgumentNull("controllerType");
            }

            _configuration = configuration;
            _controllerName = controllerName;
            _controllerType = controllerType;

            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpControllerDescriptor"/> class.
        /// </summary>
        /// <remarks>The default constructor is intended for use by unit testing only.</remarks>
        public HttpControllerDescriptor()
        {
        }

        // For unit testing purposes. 
        internal HttpControllerDescriptor(HttpConfiguration configuration)
        {
            Initialize(configuration);
        }

        /// <summary>
        /// Gets the properties associated with this instance.
        /// </summary>
        public virtual ConcurrentDictionary<object, object> Properties
        {
            get { return _properties; }
        }

        /// <summary>
        /// Gets the configuration associated with the <see cref="HttpControllerDescriptor"/>. 
        /// </summary>
        public HttpConfiguration Configuration
        {
            get { return _configuration; }
            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }
                _configuration = value;
            }
        }

        public string ControllerName
        {
            get { return _controllerName; }
            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }
                _controllerName = value;
            }
        }

        public Type ControllerType
        {
            get { return _controllerType; }
            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }
                _controllerType = value;
            }
        }

        /// <summary>
        /// Creates a controller instance for the given <see cref="HttpRequestMessage"/>
        /// </summary>
        /// <param name="request">The request message</param>
        /// <returns></returns>
        public virtual IHttpController CreateController(HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            // Invoke the controller activator
            IHttpControllerActivator activator = Configuration.Services.GetHttpControllerActivator();
            IHttpController instance = activator.Create(request, this, ControllerType);
            return instance;
        }

        /// <summary>
        /// Returns the collection of <see cref="IFilter">filters</see> associated with this descriptor's controller.
        /// </summary>
        /// <remarks>The default implementation calls <see cref="GetCustomAttributes{IFilter}()"/>.</remarks>
        /// <returns>A collection of filters associated with this controller.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Filters can be built dynamically")]
        public virtual Collection<IFilter> GetFilters()
        {
            return GetCustomAttributes<IFilter>();
        }

        /// <summary>
        /// Returns a collection of attributes that can be assigned to <typeparamref name="T"/> for this descriptor's controller.
        /// </summary>
        /// <remarks>The default implementation retrieves the matching set of attributes declared on <see cref="ControllerType"/>.</remarks>
        /// <typeparam name="T">Used to filter the collection of attributes. Use a value of <see cref="Object"/> to retrieve all attributes.</typeparam>
        /// <returns>A collection of attributes associated with this controller.</returns>
        public virtual Collection<T> GetCustomAttributes<T>() where T : class
        {
            return GetCustomAttributes<T>(inherit: true);
        }

        /// <summary>
        /// Returns a collection of attributes that can be assigned to <typeparamref name="T"/> for this descriptor's controller.
        /// </summary>
        /// <remarks>The default implementation retrieves the matching set of attributes declared on <see cref="ControllerType"/>.</remarks>
        /// <typeparam name="T">Used to filter the collection of attributes. Use a value of <see cref="Object"/> to retrieve all attributes.</typeparam>
        /// <param name="inherit"><c>true</c> to search this controller's inheritance chain to find the attributes; otherwise, <c>false</c>.</param>
        /// <returns>A collection of attributes associated with this controller.</returns>
        public virtual Collection<T> GetCustomAttributes<T>(bool inherit) where T : class
        {
            object[] attributes;
            // Getting custom attributes via reflection is slow. 
            // But iterating over a object[] to pick out specific types is fast. 
            // Furthermore, many different services may call to ask for different attributes, so we have multiple callers. 
            // That means there's not a single cache for the callers, which means there's some value caching here.
            if (inherit)
            {
                if (_attributeCache == null)
                {
                    // Even in a race, we'll just ask for the custom attributes twice.
                    _attributeCache = ControllerType.GetCustomAttributes(inherit: true);
                }
                attributes = _attributeCache;
            }
            else
            {
                if (_declaredOnlyAttributeCache == null)
                {
                    // Even in a race, we'll just ask for the custom attributes twice.
                    _declaredOnlyAttributeCache = ControllerType.GetCustomAttributes(inherit: false);
                }
                attributes = _declaredOnlyAttributeCache;
            }

            return new Collection<T>(TypeHelper.OfType<T>(attributes));
        }

        // For unit tests for initializing mock objects. Controller may not have a type, so we can't do the normal Initialize() path. 
        internal void Initialize(HttpConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Initialize the Controller Descriptor. This invokes all IControllerConfiguration attributes
        // on the controller type (and its base types)
        private void Initialize()
        {
            InvokeAttributesOnControllerType(this, ControllerType);
        }

        // Helper to invoke any Controller config attributes on this controller type or its base classes.
        private static void InvokeAttributesOnControllerType(HttpControllerDescriptor controllerDescriptor, Type type)
        {
            Contract.Assert(controllerDescriptor != null);

            if (type == null)
            {
                return;
            }
            // Initialize base class before derived classes (same order as ctors).
            InvokeAttributesOnControllerType(controllerDescriptor, type.BaseType);

            // Check for attribute
            object[] attrs = type.GetCustomAttributes(inherit: false);
            foreach (object attr in attrs)
            {
                var controllerConfig = attr as IControllerConfiguration;
                if (controllerConfig != null)
                {
                    var originalConfig = controllerDescriptor.Configuration;
                    var controllerSettings = new HttpControllerSettings(originalConfig);
                    controllerConfig.Initialize(controllerSettings, controllerDescriptor);
                    controllerDescriptor.Configuration = HttpConfiguration.ApplyControllerSettings(controllerSettings, originalConfig);
                }
            }
        }
    }
}
