// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata;
using System.Web.Http.Validation;
using System.Web.Http.ValueProviders;
using System.Web.Http.ValueProviders.Providers;

namespace System.Web.Http.ModelBinding
{
    /// <summary>
    /// Describes a parameter that gets bound via ModelBinding.  
    /// </summary>
    public class ModelBinderParameterBinding : HttpParameterBinding, IValueProviderParameterBinding
    {
        private readonly ValueProviderFactory[] _valueProviderFactories;
        private readonly IModelBinder _binder;

        public ModelBinderParameterBinding(HttpParameterDescriptor descriptor,
            IModelBinder modelBinder,
            IEnumerable<ValueProviderFactory> valueProviderFactories)
            : base(descriptor)
        {
            if (modelBinder == null)
            {
                throw Error.ArgumentNull("modelBinder");
            }
            if (valueProviderFactories == null)
            {
                throw Error.ArgumentNull("valueProviderFactories");
            }

            _binder = modelBinder;
            _valueProviderFactories = valueProviderFactories.ToArray();
        }

        public IEnumerable<ValueProviderFactory> ValueProviderFactories
        {
            get { return _valueProviderFactories; }
        }

        public IModelBinder Binder
        {
            get { return _binder; }
        }

        public override Task ExecuteBindingAsync(ModelMetadataProvider metadataProvider, HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            ModelBindingContext ctx = GetModelBindingContext(metadataProvider, actionContext);

            bool haveResult = _binder.BindModel(actionContext, ctx);
            object model = haveResult ? ctx.Model : Descriptor.DefaultValue;
            SetValue(actionContext, model);

            return TaskHelpers.Completed();
        }

        private ModelBindingContext GetModelBindingContext(ModelMetadataProvider metadataProvider, HttpActionContext actionContext)
        {
            string name = Descriptor.ParameterName;
            Type type = Descriptor.ParameterType;

            string prefix = Descriptor.Prefix;

            IValueProvider vp = CompositeValueProviderFactory.GetValueProvider(actionContext, _valueProviderFactories);

            ModelBindingContext ctx = new ModelBindingContext()
            {
                ModelName = prefix ?? name,
                FallbackToEmptyPrefix = prefix == null, // only fall back if prefix not specified
                ModelMetadata = metadataProvider.GetMetadataForType(null, type),
                ModelState = actionContext.ModelState,
                ValueProvider = vp
            };

            return ctx;
        }
    }
}
