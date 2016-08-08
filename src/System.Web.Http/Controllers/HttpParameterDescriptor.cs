// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Web.Http.Internal;
using System.Web.Http.ModelBinding;
using System.Web.Http.Properties;

namespace System.Web.Http.Controllers
{
    public abstract class HttpParameterDescriptor
    {
        private readonly ConcurrentDictionary<object, object> _properties = new ConcurrentDictionary<object, object>();

        private ParameterBindingAttribute _parameterBindingAttribute;
        private bool _searchedModelBinderAttribute;
        private HttpConfiguration _configuration;
        private HttpActionDescriptor _actionDescriptor;

        protected HttpParameterDescriptor()
        {
        }

        protected HttpParameterDescriptor(HttpActionDescriptor actionDescriptor)
        {
            if (actionDescriptor == null)
            {
                throw Error.ArgumentNull("actionDescriptor");
            }

            _actionDescriptor = actionDescriptor;
            _configuration = _actionDescriptor.Configuration;
        }

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

        public HttpActionDescriptor ActionDescriptor
        {
            get { return _actionDescriptor; }
            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }
                _actionDescriptor = value;
            }
        }

        /// <summary>
        /// Gets the properties associated with this instance.
        /// </summary>
        public ConcurrentDictionary<object, object> Properties
        {
            get { return _properties; }
        }

        public virtual object DefaultValue
        {
            get { return null; }
        }

        public abstract string ParameterName { get; }

        public abstract Type ParameterType { get; }

        public virtual string Prefix
        {
            get
            {
                ParameterBindingAttribute attribute = ParameterBinderAttribute;
                ModelBinderAttribute modelAttribute = attribute as ModelBinderAttribute;
                return modelAttribute != null
                           ? modelAttribute.Name
                           : null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the parameter is optional.
        /// </summary>
        /// <value>
        /// <c>true</c> if the parameter is optional; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsOptional
        {
            get { return false; }
        }

        /// <summary>
        /// Return a <see cref="ParameterBindingAttribute"/> if present on this parameter's signature or declared type.
        /// Returns null if no attribute is specified.
        /// </summary>
        public virtual ParameterBindingAttribute ParameterBinderAttribute
        {
            get
            {
                if (_parameterBindingAttribute == null)
                {
                    if (!_searchedModelBinderAttribute)
                    {
                        _searchedModelBinderAttribute = true;
                        _parameterBindingAttribute = FindParameterBindingAttribute();
                    }
                }

                return _parameterBindingAttribute;
            }

            set { _parameterBindingAttribute = value; }
        }

        public virtual Collection<T> GetCustomAttributes<T>() where T : class
        {
            return new Collection<T>();
        }

        private ParameterBindingAttribute FindParameterBindingAttribute()
        {
            // Can be on parameter itself or on the parameter's type.  Nearest wins.
            return ChooseAttribute(GetCustomAttributes<ParameterBindingAttribute>())
                ?? ChooseAttribute(ParameterType.GetCustomAttributes<ParameterBindingAttribute>(false));
        }

        private static ParameterBindingAttribute ChooseAttribute(IList<ParameterBindingAttribute> list)
        {
            if (list.Count == 0)
            {
                return null;
            }
            if (list.Count > 1)
            {
                // Multiple attributes specified at the same level
                return new AmbiguousParameterBindingAttribute();
            }
            return list[0];
        }

        // Helper class to return an error binding if an parameter has an invalid attribute combination. 
        private sealed class AmbiguousParameterBindingAttribute : ParameterBindingAttribute
        {
            public override HttpParameterBinding GetBinding(HttpParameterDescriptor parameter)
            {
                string message = Error.Format(SRResources.ParameterBindingConflictingAttributes, parameter.ParameterName);
                return parameter.BindAsError(message);
            }
        }
    }
}
