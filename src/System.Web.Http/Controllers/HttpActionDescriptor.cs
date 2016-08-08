// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using System.Web.Http.Internal;
using System.Web.Http.Properties;

namespace System.Web.Http.Controllers
{
    public abstract class HttpActionDescriptor
    {
        private readonly ConcurrentDictionary<object, object> _properties = new ConcurrentDictionary<object, object>();

        private IActionResultConverter _converter;
        private readonly Lazy<Collection<FilterInfo>> _filterPipeline;
        private FilterGrouping _filterGrouping;
        private Collection<FilterInfo> _filterPipelineForGrouping;

        private HttpConfiguration _configuration;
        private HttpControllerDescriptor _controllerDescriptor;
        private readonly Collection<HttpMethod> _supportedHttpMethods = new Collection<HttpMethod>();

        private HttpActionBinding _actionBinding;

        private static readonly ResponseMessageResultConverter _responseMessageResultConverter = new ResponseMessageResultConverter();
        private static readonly VoidResultConverter _voidResultConverter = new VoidResultConverter();

        protected HttpActionDescriptor()
        {
            _filterPipeline = new Lazy<Collection<FilterInfo>>(InitializeFilterPipeline);
        }

        protected HttpActionDescriptor(HttpControllerDescriptor controllerDescriptor)
            : this()
        {
            if (controllerDescriptor == null)
            {
                throw Error.ArgumentNull("controllerDescriptor");
            }

            _controllerDescriptor = controllerDescriptor;
            _configuration = _controllerDescriptor.Configuration;
        }

        public abstract string ActionName { get; }

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

        public virtual HttpActionBinding ActionBinding
        {
            get
            {
                if (_actionBinding == null)
                {
                    ServicesContainer controllerServices = _controllerDescriptor.Configuration.Services;
                    IActionValueBinder actionValueBinder = controllerServices.GetActionValueBinder();
                    HttpActionBinding actionBinding = actionValueBinder.GetBinding(this);
                    _actionBinding = actionBinding;
                }
                return _actionBinding;
            }
            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }
                _actionBinding = value;
            }
        }

        public HttpControllerDescriptor ControllerDescriptor
        {
            get { return _controllerDescriptor; }
            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }
                _controllerDescriptor = value;
            }
        }

        /// <summary>
        /// The return type of the method or <c>null</c> if the method does not return a value (e.g. a method returning
        /// <c>void</c>).
        /// </summary>
        /// <remarks>
        /// This property should describe the type of the value contained by the result of executing the action
        /// via the <see cref="ExecuteAsync(HttpControllerContext, IDictionary{string, object}, CancellationToken)"/>.
        /// </remarks>
        public abstract Type ReturnType { get; }

        /// <summary>
        /// Gets the converter for correctly transforming the result of calling
        /// <see cref="ExecuteAsync(HttpControllerContext, IDictionary{string, object}, CancellationToken)"/> into an instance of
        /// <see cref="HttpResponseMessage"/>. 
        /// </summary>
        /// <remarks>
        /// <para>This converter is not used when the runtime return value of an action is an <see cref="IHttpActionResult"/>.</para>
        /// <para>
        /// This value is <see langword="null" /> when the declared <see cref="ReturnType"/> is an <see cref="IHttpActionResult"/>.
        /// </para>
        /// <para>
        /// The behavior of the returned converter should align with the action's declared <see cref="ReturnType"/>.
        /// </para>
        /// </remarks>
        public virtual IActionResultConverter ResultConverter
        {
            get
            {
                // This initialization is not thread safe but that's fine since the converters do not have
                // any interesting state. If 2 threads get 2 different instances of the same converter type
                // we don't really care.
                if (_converter == null)
                {
                    _converter = GetResultConverter(ReturnType);
                }
                return _converter;
            }
        }

        public virtual Collection<HttpMethod> SupportedHttpMethods
        {
            get { return _supportedHttpMethods; }
        }

        /// <summary>
        /// Gets the properties associated with this instance.
        /// </summary>
        public virtual ConcurrentDictionary<object, object> Properties
        {
            get { return _properties; }
        }

        /// <summary>
        /// Gets the custom attributes.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual Collection<T> GetCustomAttributes<T>() where T : class
        {
            return GetCustomAttributes<T>(inherit: true);
        }

        /// <summary>
        /// Gets the custom attributes for the action.
        /// </summary>
        /// <typeparam name="T">The type of attribute to search for.</typeparam>
        /// <param name="inherit"><c>true</c> to search this action's inheritance chain to find the attributes; otherwise, <c>false</c>.</param>
        /// <returns>The collection of custom attributes applied to this action.</returns>
        public virtual Collection<T> GetCustomAttributes<T>(bool inherit) where T : class
        {
            return new Collection<T>();
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Filters can be built dynamically")]
        public virtual Collection<IFilter> GetFilters()
        {
            return new Collection<IFilter>();
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Parameters can be built dynamically")]
        public abstract Collection<HttpParameterDescriptor> GetParameters();

        internal static IActionResultConverter GetResultConverter(Type type)
        {
            if (type != null && type.IsGenericParameter)
            {
                // This can happen if somebody declares an action method as:
                // public T Get<T>() { }
                throw Error.InvalidOperation(SRResources.HttpActionDescriptor_NoConverterForGenericParamterTypeExists, type);
            }

            if (type == null)
            {
                return _voidResultConverter;
            }
            else if (typeof(HttpResponseMessage).IsAssignableFrom(type))
            {
                return _responseMessageResultConverter;
            }
            else if (typeof(IHttpActionResult).IsAssignableFrom(type))
            {
                return null;
            }
            else
            {
                Type valueConverterType = typeof(ValueResultConverter<>).MakeGenericType(type);
                return TypeActivator.Create<IActionResultConverter>(valueConverterType).Invoke();
            }
        }

        /// <summary>
        /// Executes the described action and returns a <see cref="Task{T}"/> that once completed will
        /// contain the return value of the action.
        /// </summary>
        /// <param name="controllerContext">The context.</param>
        /// <param name="arguments">The arguments.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="Task{T}"/> that once completed will contain the return value of the action.</returns>
        public abstract Task<object> ExecuteAsync(HttpControllerContext controllerContext, IDictionary<string, object> arguments, CancellationToken cancellationToken);

        /// <summary>
        /// Returns the filters for the given configuration and action. The filter collection is ordered
        /// according to the FilterScope (in order from least specific to most specific: First, Global, Controller, Action).
        /// 
        /// If a given filter disallows duplicates (AllowMultiple=False) then the most specific filter is maintained
        /// and less specific filters get removed (e.g. if there is a Authorize filter with a Controller scope and another
        /// one with an Action scope then the one with the Action scope will be maintained and the one with the Controller
        /// scope will be discarded).
        /// </summary>
        /// <returns>A <see cref="Collection{T}"/> of all filters associated with this <see cref="HttpActionDescriptor"/>.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Filter pipeline can be built dynamically")]
        public virtual Collection<FilterInfo> GetFilterPipeline()
        {
            return _filterPipeline.Value;
        }

        internal FilterGrouping GetFilterGrouping()
        {
            // Performance-sensitive
            // Filter grouping is expensive so cache whenever possible
            // For compatibility, the virtual method must be called
            Collection<FilterInfo> currentFilterPipeline = GetFilterPipeline();
            if (_filterGrouping == null || _filterPipelineForGrouping != currentFilterPipeline)
            {
                _filterGrouping = new FilterGrouping(currentFilterPipeline);
                _filterPipelineForGrouping = currentFilterPipeline;
            }
            return _filterGrouping;
        }

        private Collection<FilterInfo> InitializeFilterPipeline()
        {
            IEnumerable<IFilterProvider> filterProviders = _configuration.Services.GetFilterProviders();

            IEnumerable<FilterInfo> filters = filterProviders.SelectMany(fp => fp.GetFilters(_configuration, this)).OrderBy(f => f, FilterInfoComparer.Instance);

            // Need to discard duplicate filters from the end, so that most specific ones get kept (Action scope) and
            // less specific ones get removed (Global)
            filters = RemoveDuplicates(filters.Reverse()).Reverse();

            return new Collection<FilterInfo>(filters.ToList());
        }

        private static IEnumerable<FilterInfo> RemoveDuplicates(IEnumerable<FilterInfo> filters)
        {
            Contract.Assert(filters != null);

            HashSet<Type> visitedTypes = new HashSet<Type>();

            foreach (FilterInfo filter in filters)
            {
                object filterInstance = filter.Instance;
                Type filterInstanceType = filterInstance.GetType();

                if (!visitedTypes.Contains(filterInstanceType) || AllowMultiple(filterInstance))
                {
                    yield return filter;
                    visitedTypes.Add(filterInstanceType);
                }
            }
        }

        private static bool AllowMultiple(object filterInstance)
        {
            IFilter filter = filterInstance as IFilter;
            return filter == null || filter.AllowMultiple;
        }
    }
}
