// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using System.Web.Http.Properties;

namespace System.Web.Http.ValueProviders
{
    /// <summary>
    /// This attribute is used to specify a custom <see cref="ValueProviderFactory"/>.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments", Justification = "property already exposed in plural form")]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Parameter, Inherited = true, AllowMultiple = false)]
    public sealed class ValueProviderAttribute : ModelBinderAttribute
    {
        private readonly Type[] _valueProviderFactoryTypes;

        // Provide CLS compliant overload
        public ValueProviderAttribute(Type valueProviderFactory)
            : this(new Type[] { valueProviderFactory })
        {
        }

        // Convenience for multiple types. This is not cls-compliant.
        public ValueProviderAttribute(params Type[] valueProviderFactories)
        {
            _valueProviderFactoryTypes = valueProviderFactories;
        }

        public IEnumerable<Type> ValueProviderFactoryTypes
        {
            get { return _valueProviderFactoryTypes; }
        }

        public override IEnumerable<ValueProviderFactory> GetValueProviderFactories(HttpConfiguration configuration)
        {
            // By default, just get all registered value provider factories
            return Array.ConvertAll(_valueProviderFactoryTypes, Instantiate);
        }

        private static ValueProviderFactory Instantiate(Type factoryType)
        {
            if (factoryType == null)
            {
                throw new ArgumentNullException("factoryType");
            }

            if (!typeof(ValueProviderFactory).IsAssignableFrom(factoryType))
            {
                throw Error.InvalidOperation(SRResources.ValueProviderFactory_Cannot_Create, typeof(ValueProviderFactory), factoryType);
            }

            return (ValueProviderFactory)Activator.CreateInstance(factoryType);
        }
    }
}
