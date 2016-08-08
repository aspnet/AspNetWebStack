// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Web.Mvc;

namespace Microsoft.Web.Mvc
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public sealed class DeserializeAttribute : CustomModelBinderAttribute
    {
        public DeserializeAttribute()
        {
        }

        internal MvcSerializer Serializer { get; set; }

        public override IModelBinder GetBinder()
        {
            return new DeserializingModelBinder(Serializer);
        }

        private sealed class DeserializingModelBinder : IModelBinder
        {
            private readonly MvcSerializer _serializer;

            public DeserializingModelBinder(MvcSerializer serializer)
            {
                _serializer = serializer ?? new MvcSerializer();
            }

            [SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo", MessageId = "System.Web.Mvc.ValueProviderResult.ConvertTo(System.Type)", Justification = "The target object should make the correct culture determination, not this method.")]
            public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
            {
                if (bindingContext == null)
                {
                    throw new ArgumentNullException("bindingContext");
                }

                ValueProviderResult valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
                if (valueProviderResult == null)
                {
                    // nothing found
                    return null;
                }

                string serializedValue = (string)valueProviderResult.ConvertTo(typeof(string));
                return _serializer.Deserialize(serializedValue);
            }
        }
    }
}
