// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Globalization;
using System.Web.Mvc.Properties;

namespace System.Web.Mvc
{
    internal class SingleServiceResolver<TService> : IResolver<TService>
        where TService : class
    {
        private Lazy<TService> _currentValueFromResolver;
        private Func<TService> _currentValueThunk;
        private TService _defaultValue;
        private Func<IDependencyResolver> _resolverThunk;
        private string _callerMethodName;

        public SingleServiceResolver(Func<TService> currentValueThunk, TService defaultValue, string callerMethodName)
        {
            if (currentValueThunk == null)
            {
                throw new ArgumentNullException("currentValueThunk");
            }
            if (defaultValue == null)
            {
                throw new ArgumentNullException("defaultValue");
            }

            _resolverThunk = () => DependencyResolver.Current;
            _currentValueFromResolver = new Lazy<TService>(GetValueFromResolver);
            _currentValueThunk = currentValueThunk;
            _defaultValue = defaultValue;
            _callerMethodName = callerMethodName;
        }

        internal SingleServiceResolver(Func<TService> staticAccessor, TService defaultValue, IDependencyResolver resolver, string callerMethodName)
            : this(staticAccessor, defaultValue, callerMethodName)
        {
            if (resolver != null)
            {
                _resolverThunk = () => resolver;
            }
        }

        public TService Current
        {
            get { return _currentValueFromResolver.Value ?? _currentValueThunk() ?? _defaultValue; }
        }

        private TService GetValueFromResolver()
        {
            TService result = _resolverThunk().GetService<TService>();

            if (result != null && _currentValueThunk() != null)
            {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, MvcResources.SingleServiceResolver_CannotRegisterTwoInstances, typeof(TService).Name.ToString(), _callerMethodName));
            }

            return result;
        }
    }
}
