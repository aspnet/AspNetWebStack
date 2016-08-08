// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Linq;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web.Mvc;

namespace Microsoft.Web.Mvc.ModelBinding
{
    // This is a single provider that can work with both byte[] and Binary models.
    public sealed class BinaryDataModelBinderProvider : ModelBinderProvider
    {
        private static readonly ModelBinderProvider[] _providers = new ModelBinderProvider[]
        {
            new SimpleModelBinderProvider(typeof(byte[]), new ByteArrayExtensibleModelBinder()),
            new SimpleModelBinderProvider(typeof(Binary), new LinqBinaryExtensibleModelBinder())
        };

        public override IExtensibleModelBinder GetBinder(ControllerContext controllerContext, ExtensibleModelBindingContext bindingContext)
        {
            return (from provider in _providers
                    let binder = provider.GetBinder(controllerContext, bindingContext)
                    where binder != null
                    select binder).FirstOrDefault();
        }

        // This is essentially a clone of the ByteArrayModelBinder from core
        private class ByteArrayExtensibleModelBinder : IExtensibleModelBinder
        {
            [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We want to ignore when the data is corrupted")]
            [SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo", MessageId = "System.Web.Mvc.ValueProviderResult.ConvertTo(System.Type)", Justification = "The target object should make the correct culture determination, not this method.")]
            public bool BindModel(ControllerContext controllerContext, ExtensibleModelBindingContext bindingContext)
            {
                ModelBinderUtil.ValidateBindingContext(bindingContext);
                ValueProviderResult valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

                // case 1: there was no <input ... /> element containing this data
                if (valueProviderResult == null)
                {
                    return false;
                }

                string base64String = (string)valueProviderResult.ConvertTo(typeof(string));

                // case 2: there was an <input ... /> element but it was left blank
                if (String.IsNullOrEmpty(base64String))
                {
                    return false;
                }

                // Future proofing. If the byte array is actually an instance of System.Data.Linq.Binary
                // then we need to remove these quotes put in place by the ToString() method.
                string realValue = base64String.Replace("\"", String.Empty);
                try
                {
                    bindingContext.Model = ConvertByteArray(Convert.FromBase64String(realValue));
                    return true;
                }
                catch
                {
                    // corrupt data - just ignore
                    return false;
                }
            }

            protected virtual object ConvertByteArray(byte[] originalModel)
            {
                return originalModel;
            }
        }

        // This is essentially a clone of the LinqBinaryModelBinder from core
        private class LinqBinaryExtensibleModelBinder : ByteArrayExtensibleModelBinder
        {
            protected override object ConvertByteArray(byte[] originalModel)
            {
                return new Binary(originalModel);
            }
        }
    }
}
