// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.Http.Controllers;
using System.Web.Http.Dependencies;
using System.Web.Http.ModelBinding.Binders;
using System.Web.Http.Properties;
using System.Web.Http.ValueProviders;

namespace System.Web.Http.ModelBinding
{
    /// <summary>
    /// Specify this parameter uses a model binder. This can optionally specify the specific model binder and 
    /// value providers that drive that model binder. 
    /// Derived attributes may provide convenience settings for the model binder or value provider. 
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments", Justification = "want constructor argument shortcut")]
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "part of a class hierarchy")]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Parameter, Inherited = true, AllowMultiple = false)]
    public class ModelBinderAttribute : ParameterBindingAttribute
    {
        public ModelBinderAttribute()
            : this(null)
        {
        }

        public ModelBinderAttribute(Type binderType)
        {
            BinderType = binderType;
        }

        /// <summary>
        /// Sets the type of the model binder. 
        /// This type must be a subclass of <see cref="ModelBinderProvider"/>  or <see cref="IModelBinder"/>      
        /// If null, uses the default from the configuration. 
        /// </summary>   
        public Type BinderType { get; set; }

        /// <summary>
        /// Gets or sets the name to consider as the parameter name during model binding
        /// </summary>
        public string Name { get; set; }

        public bool SuppressPrefixCheck { get; set; }

        public override HttpParameterBinding GetBinding(HttpParameterDescriptor parameter)
        {
            HttpConfiguration config = parameter.Configuration;
            IModelBinder binder = GetModelBinder(config, parameter.ParameterType);
            IEnumerable<ValueProviderFactory> valueProviderFactories = GetValueProviderFactories(config);

            return new ModelBinderParameterBinding(parameter, binder, valueProviderFactories);
        }

        // This will get called by a parameter binding, which will cache the results. 
        public ModelBinderProvider GetModelBinderProvider(HttpConfiguration configuration)
        {
            if (BinderType != null)
            {
                object value = GetOrInstantiate(configuration, BinderType);

                if (value != null)
                {
                    VerifyBinderType(value.GetType());
                    ModelBinderProvider result = (ModelBinderProvider)value;
                    return result;
                }
            }

            // Create default over config
            IEnumerable<ModelBinderProvider> providers = configuration.Services.GetModelBinderProviders();

            if (providers.Count() == 1)
            {
                return providers.First();
            }

            return new CompositeModelBinderProvider(providers);
        }

        /// <summary>
        /// Get the IModelBinder for this type.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="modelType">model type that the binder is expected to bind.</param>
        /// <returns>
        /// a non-null model binder.
        /// </returns>
        public IModelBinder GetModelBinder(HttpConfiguration configuration, Type modelType)
        {
            if (BinderType == null)
            {
                ModelBinderProvider provider = GetModelBinderProvider(configuration);
                return provider.GetBinder(configuration, modelType);
            }

            // This may create a IModelBinder or a ModelBinderProvider
            object value = GetOrInstantiate(configuration, BinderType);

            Contract.Assert(value != null); // Activator would have thrown

            IModelBinder binder = value as IModelBinder;
            if (binder != null)
            {
                return binder;
            }
            else
            {
                ModelBinderProvider provider = value as ModelBinderProvider;
                if (provider != null)
                {
                    return provider.GetBinder(configuration, modelType);
                }
            }

            Type required = typeof(IModelBinder);
            throw Error.InvalidOperation(SRResources.ValueProviderFactory_Cannot_Create, required.Name, value.GetType().Name, required.Name);
        }

        /// <summary>
        /// Value providers that will be fed to the model binder.
        /// </summary>
        public virtual IEnumerable<ValueProviderFactory> GetValueProviderFactories(HttpConfiguration configuration)
        {
            // By default, just get all registered value provider factories
            return configuration.Services.GetValueProviderFactories();
        }

        private static void VerifyBinderType(Type attemptedType)
        {
            Type required = typeof(ModelBinderProvider);
            if (!required.IsAssignableFrom(attemptedType))
            {
                throw Error.InvalidOperation(SRResources.ValueProviderFactory_Cannot_Create, required.Name, attemptedType.Name, required.Name);
            }
        }

        private static object GetOrInstantiate(HttpConfiguration configuration, Type type)
        {
            IDependencyResolver dr = configuration.DependencyResolver;
            object value = dr.GetService(type);
            if (value != null)
            {
                return value;
            }

            return Activator.CreateInstance(type);
        }
    }
}
