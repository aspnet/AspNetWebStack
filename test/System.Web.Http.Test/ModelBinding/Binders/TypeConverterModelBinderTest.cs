// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;
using System.Globalization;
using System.Web.Http.Metadata.Providers;
using System.Web.Http.Util;
using Microsoft.TestCommon;

namespace System.Web.Http.ModelBinding.Binders
{
    public class TypeConverterModelBinderTest
    {
        [Fact]
        public void BindModel_Error_FormatExceptionsTurnedIntoStringsInModelState()
        {
            // Arrange
            ModelBindingContext bindingContext = GetBindingContext(typeof(int));
            bindingContext.ValueProvider = new SimpleHttpValueProvider
            {
                { "theModelName", "not an integer" }
            };

            TypeConverterModelBinder binder = new TypeConverterModelBinder();

            // Act
            bool retVal = binder.BindModel(null, bindingContext);

            // Assert
            Assert.False(retVal);
            Assert.Equal("The value 'not an integer' is not valid for Int32.", bindingContext.ModelState["theModelName"].Errors[0].ErrorMessage);
        }

        [Fact]
        public void BindModel_Error_FormatExceptionsTurnedIntoStringsInModelState_ErrorNotAddedIfCallbackReturnsNull()
        {
            // Arrange
            ModelBindingContext bindingContext = GetBindingContext(typeof(int));
            bindingContext.ValueProvider = new SimpleHttpValueProvider
            {
                { "theModelName", "not an integer" }
            };

            TypeConverterModelBinder binder = new TypeConverterModelBinder();

            // Act
            ModelBinderErrorMessageProvider originalProvider = ModelBinderConfig.TypeConversionErrorMessageProvider;
            bool retVal;
            try
            {
                ModelBinderConfig.TypeConversionErrorMessageProvider = delegate { return null; };
                retVal = binder.BindModel(null, bindingContext);
            }
            finally
            {
                ModelBinderConfig.TypeConversionErrorMessageProvider = originalProvider;
            }

            // Assert
            Assert.False(retVal);
            Assert.Null(bindingContext.Model);
            Assert.True(bindingContext.ModelState.IsValid);
        }

        [Fact]
        public void BindModel_Error_GeneralExceptionsSavedInModelState()
        {
            // Arrange
            ModelBindingContext bindingContext = GetBindingContext(typeof(Dummy));
            bindingContext.ValueProvider = new SimpleHttpValueProvider
            {
                { "theModelName", "foo" }
            };

            TypeConverterModelBinder binder = new TypeConverterModelBinder();

            // Act
            bool retVal = binder.BindModel(null, bindingContext);

            // Assert
            Assert.False(retVal);
            Assert.Null(bindingContext.Model);
            Assert.Equal("The parameter conversion from type 'System.String' to type 'System.Web.Http.ModelBinding.Binders.TypeConverterModelBinderTest+Dummy' failed. See the inner exception for more information.", bindingContext.ModelState["theModelName"].Errors[0].Exception.Message);
            Assert.Equal("From DummyTypeConverter: foo", bindingContext.ModelState["theModelName"].Errors[0].Exception.InnerException.Message);
        }

        [Fact]
        public void BindModel_NullValueProviderResult_ReturnsFalse()
        {
            // Arrange
            ModelBindingContext bindingContext = GetBindingContext(typeof(int));

            TypeConverterModelBinder binder = new TypeConverterModelBinder();

            // Act
            bool retVal = binder.BindModel(null, bindingContext);

            // Assert
            Assert.False(retVal, "BindModel should have returned null.");
            Assert.Empty(bindingContext.ModelState);
        }

        [Fact]
        public void BindModel_ValidValueProviderResult_ConvertEmptyStringsToNull()
        {
            // Arrange
            ModelBindingContext bindingContext = GetBindingContext(typeof(string));
            bindingContext.ValueProvider = new SimpleHttpValueProvider
            {
                { "theModelName", "" }
            };

            TypeConverterModelBinder binder = new TypeConverterModelBinder();

            // Act
            bool retVal = binder.BindModel(null, bindingContext);

            // Assert
            Assert.True(retVal);
            Assert.Null(bindingContext.Model);
            Assert.True(bindingContext.ModelState.ContainsKey("theModelName"));
        }

        [Fact]
        public void BindModel_ValidValueProviderResult_ReturnsModel()
        {
            // Arrange
            ModelBindingContext bindingContext = GetBindingContext(typeof(int));
            bindingContext.ValueProvider = new SimpleHttpValueProvider
            {
                { "theModelName", "42" }
            };

            TypeConverterModelBinder binder = new TypeConverterModelBinder();

            // Act
            bool retVal = binder.BindModel(null, bindingContext);

            // Assert
            Assert.True(retVal);
            Assert.Equal(42, bindingContext.Model);
            Assert.True(bindingContext.ModelState.ContainsKey("theModelName"));
        }

        private static ModelBindingContext GetBindingContext(Type modelType)
        {
            return new ModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, modelType),
                ModelName = "theModelName",
                ValueProvider = new SimpleHttpValueProvider() // empty
            };
        }

        [TypeConverter(typeof(DummyTypeConverter))]
        private struct Dummy
        {
        }

        private sealed class DummyTypeConverter : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                return (sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                throw new InvalidOperationException(String.Format("From DummyTypeConverter: {0}", value));
            }
        }
    }
}
