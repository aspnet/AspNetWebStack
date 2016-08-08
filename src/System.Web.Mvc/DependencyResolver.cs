// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web.Mvc.Properties;

namespace System.Web.Mvc
{
    public class DependencyResolver
    {
        private static DependencyResolver _instance = new DependencyResolver();

        private IDependencyResolver _current;

        /// <summary>
        /// Cache should always be a new CacheDependencyResolver(_current).
        /// </summary>
        private CacheDependencyResolver _currentCache;

        public DependencyResolver()
        {
            InnerSetResolver(new DefaultDependencyResolver());
        }

        public static IDependencyResolver Current
        {
            get { return _instance.InnerCurrent; }
        }

        internal static IDependencyResolver CurrentCache
        {
            get { return _instance.InnerCurrentCache; }
        }

        public IDependencyResolver InnerCurrent
        {
            get { return _current; }
        }

        /// <summary>
        /// Provides caching over results returned by Current.
        /// </summary>
        internal IDependencyResolver InnerCurrentCache
        {
            get { return _currentCache; }
        }

        public static void SetResolver(IDependencyResolver resolver)
        {
            _instance.InnerSetResolver(resolver);
        }

        public static void SetResolver(object commonServiceLocator)
        {
            _instance.InnerSetResolver(commonServiceLocator);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is an appropriate nesting of generic types.")]
        public static void SetResolver(Func<Type, object> getService, Func<Type, IEnumerable<object>> getServices)
        {
            _instance.InnerSetResolver(getService, getServices);
        }

        public void InnerSetResolver(IDependencyResolver resolver)
        {
            if (resolver == null)
            {
                throw new ArgumentNullException("resolver");
            }

            _current = resolver;
            _currentCache = new CacheDependencyResolver(_current);
        }

        public void InnerSetResolver(object commonServiceLocator)
        {
            if (commonServiceLocator == null)
            {
                throw new ArgumentNullException("commonServiceLocator");
            }

            Type locatorType = commonServiceLocator.GetType();
            MethodInfo getInstance = locatorType.GetMethod("GetInstance", new[] { typeof(Type) });
            MethodInfo getInstances = locatorType.GetMethod("GetAllInstances", new[] { typeof(Type) });

            if (getInstance == null ||
                getInstance.ReturnType != typeof(object) ||
                getInstances == null ||
                getInstances.ReturnType != typeof(IEnumerable<object>))
            {
                throw new ArgumentException(
                    String.Format(
                        CultureInfo.CurrentCulture,
                        MvcResources.DependencyResolver_DoesNotImplementICommonServiceLocator,
                        locatorType.FullName),
                    "commonServiceLocator");
            }

            var getService = (Func<Type, object>)Delegate.CreateDelegate(typeof(Func<Type, object>), commonServiceLocator, getInstance);
            var getServices = (Func<Type, IEnumerable<object>>)Delegate.CreateDelegate(typeof(Func<Type, IEnumerable<object>>), commonServiceLocator, getInstances);

            InnerSetResolver(new DelegateBasedDependencyResolver(getService, getServices));
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is an appropriate nesting of generic types.")]
        public void InnerSetResolver(Func<Type, object> getService, Func<Type, IEnumerable<object>> getServices)
        {
            if (getService == null)
            {
                throw new ArgumentNullException("getService");
            }
            if (getServices == null)
            {
                throw new ArgumentNullException("getServices");
            }

            InnerSetResolver(new DelegateBasedDependencyResolver(getService, getServices));
        }

        /// <summary>
        /// Wraps an IDependencyResolver and ensures single instance per-type.
        /// </summary>
        /// <remarks>
        /// Note it's possible for multiple threads to race and call the _resolver service multiple times.
        /// We'll pick one winner and ignore the others and still guarantee a unique instance.
        /// </remarks>
        private sealed class CacheDependencyResolver : IDependencyResolver
        {
            private readonly ConcurrentDictionary<Type, object> _cache = new ConcurrentDictionary<Type, object>();
            private readonly ConcurrentDictionary<Type, IEnumerable<object>> _cacheMultiple = new ConcurrentDictionary<Type, IEnumerable<object>>();
            private readonly Func<Type, object> _getServiceDelegate;
            private readonly Func<Type, IEnumerable<object>> _getServicesDelegate;

            private readonly IDependencyResolver _resolver;

            public CacheDependencyResolver(IDependencyResolver resolver)
            {
                _resolver = resolver;
                _getServiceDelegate = _resolver.GetService;
                _getServicesDelegate = _resolver.GetServices;
            }

            public object GetService(Type serviceType)
            {
                // Use a saved delegate to prevent per-call delegate allocation
                return _cache.GetOrAdd(serviceType, _getServiceDelegate);
            }

            public IEnumerable<object> GetServices(Type serviceType)
            {
                // Use a saved delegate to prevent per-call delegate allocation
                return _cacheMultiple.GetOrAdd(serviceType, _getServicesDelegate);
            }
        }

        private class DefaultDependencyResolver : IDependencyResolver
        {
            [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "This method might throw exceptions whose type we cannot strongly link against; namely, ActivationException from common service locator")]
            public object GetService(Type serviceType)
            {
                // Since attempting to create an instance of an interface or an abstract type results in an exception, immediately return null
                // to improve performance and the debugging experience with first-chance exceptions enabled.
                if (serviceType.IsInterface || serviceType.IsAbstract)
                {
                    return null;
                }

                try
                {
                    return Activator.CreateInstance(serviceType);
                }
                catch
                {
                    return null;
                }
            }

            public IEnumerable<object> GetServices(Type serviceType)
            {
                return Enumerable.Empty<object>();
            }
        }

        private class DelegateBasedDependencyResolver : IDependencyResolver
        {
            private Func<Type, object> _getService;
            private Func<Type, IEnumerable<object>> _getServices;

            public DelegateBasedDependencyResolver(Func<Type, object> getService, Func<Type, IEnumerable<object>> getServices)
            {
                _getService = getService;
                _getServices = getServices;
            }

            [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "This method might throw exceptions whose type we cannot strongly link against; namely, ActivationException from common service locator")]
            public object GetService(Type type)
            {
                try
                {
                    return _getService.Invoke(type);
                }
                catch
                {
                    return null;
                }
            }

            public IEnumerable<object> GetServices(Type type)
            {
                return _getServices(type);
            }
        }
    }
}
