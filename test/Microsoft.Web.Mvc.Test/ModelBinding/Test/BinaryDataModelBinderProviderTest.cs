// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Linq;
using System.Web.Mvc;
using Microsoft.TestCommon;
using Microsoft.Web.UnitTestUtil;

namespace Microsoft.Web.Mvc.ModelBinding.Test
{
    public class BinaryDataModelBinderProviderTest
    {
        private static readonly byte[] _base64Bytes = new byte[] { 0x12, 0x20, 0x34, 0x40 };
        private const string _base64String = "EiA0QA==";

        [Fact]
        public void BindModel_BadValue_Fails()
        {
            // Arrange
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(byte[])),
                ModelName = "foo",
                ValueProvider = new SimpleValueProvider
                {
                    { "foo", "not base64 encoded!" }
                }
            };

            BinaryDataModelBinderProvider binderProvider = new BinaryDataModelBinderProvider();

            // Act
            IExtensibleModelBinder binder = binderProvider.GetBinder(null, bindingContext);
            bool retVal = binder.BindModel(null, bindingContext);

            // Assert
            Assert.False(retVal);
        }

        [Fact]
        public void BindModel_EmptyValue_Fails()
        {
            // Arrange
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(byte[])),
                ModelName = "foo",
                ValueProvider = new SimpleValueProvider
                {
                    { "foo", "" }
                }
            };

            BinaryDataModelBinderProvider binderProvider = new BinaryDataModelBinderProvider();

            // Act
            IExtensibleModelBinder binder = binderProvider.GetBinder(null, bindingContext);
            bool retVal = binder.BindModel(null, bindingContext);

            // Assert
            Assert.False(retVal);
        }

        [Fact]
        public void BindModel_GoodValue_ByteArray_Succeeds()
        {
            // Arrange
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(byte[])),
                ModelName = "foo",
                ValueProvider = new SimpleValueProvider
                {
                    { "foo", _base64String }
                }
            };

            BinaryDataModelBinderProvider binderProvider = new BinaryDataModelBinderProvider();

            // Act
            IExtensibleModelBinder binder = binderProvider.GetBinder(null, bindingContext);
            bool retVal = binder.BindModel(null, bindingContext);

            // Assert
            Assert.True(retVal);
            Assert.Equal(_base64Bytes, (byte[])bindingContext.Model);
        }

        [Fact]
        public void BindModel_GoodValue_LinqBinary_Succeeds()
        {
            // Arrange
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(Binary)),
                ModelName = "foo",
                ValueProvider = new SimpleValueProvider
                {
                    { "foo", _base64String }
                }
            };

            BinaryDataModelBinderProvider binderProvider = new BinaryDataModelBinderProvider();

            // Act
            IExtensibleModelBinder binder = binderProvider.GetBinder(null, bindingContext);
            bool retVal = binder.BindModel(null, bindingContext);

            // Assert
            Assert.True(retVal);
            Binary binaryModel = Assert.IsType<Binary>(bindingContext.Model);
            Assert.Equal(_base64Bytes, binaryModel.ToArray());
        }

        [Fact]
        public void BindModel_NoValue_Fails()
        {
            // Arrange
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(byte[])),
                ModelName = "foo",
                ValueProvider = new SimpleValueProvider
                {
                    { "foo.bar", _base64String }
                }
            };

            BinaryDataModelBinderProvider binderProvider = new BinaryDataModelBinderProvider();

            // Act
            IExtensibleModelBinder binder = binderProvider.GetBinder(null, bindingContext);
            bool retVal = binder.BindModel(null, bindingContext);

            // Assert
            Assert.False(retVal);
        }

        [Fact]
        public void GetBinder_WrongModelType_ReturnsNull()
        {
            // Arrange
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(object)),
                ModelName = "foo",
                ValueProvider = new SimpleValueProvider
                {
                    { "foo", _base64String }
                }
            };

            BinaryDataModelBinderProvider binderProvider = new BinaryDataModelBinderProvider();

            // Act
            IExtensibleModelBinder modelBinder = binderProvider.GetBinder(null, bindingContext);

            // Assert
            Assert.Null(modelBinder);
        }
    }
}
