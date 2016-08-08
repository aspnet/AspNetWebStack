// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Microsoft.TestCommon;

namespace Microsoft.Web.Mvc.ModelBinding.Test
{
    public class ModelBinderUtilTest
    {
        [Fact]
        public void CastOrDefault_CorrectType_ReturnsInput()
        {
            // Act
            int retVal = ModelBinderUtil.CastOrDefault<int>(42);

            // Assert
            Assert.Equal(42, retVal);
        }

        [Fact]
        public void CastOrDefault_IncorrectType_ReturnsDefaultTModel()
        {
            // Act
            DateTime retVal = ModelBinderUtil.CastOrDefault<DateTime>(42);

            // Assert
            Assert.Equal(default(DateTime), retVal);
        }

        [Fact]
        public void CreateIndexModelName_EmptyParentName()
        {
            // Act
            string fullChildName = ModelBinderUtil.CreateIndexModelName("", 42);

            // Assert
            Assert.Equal("[42]", fullChildName);
        }

        [Fact]
        public void CreateIndexModelName_IntIndex()
        {
            // Act
            string fullChildName = ModelBinderUtil.CreateIndexModelName("parentName", 42);

            // Assert
            Assert.Equal("parentName[42]", fullChildName);
        }

        [Fact]
        public void CreateIndexModelName_StringIndex()
        {
            // Act
            string fullChildName = ModelBinderUtil.CreateIndexModelName("parentName", "index");

            // Assert
            Assert.Equal("parentName[index]", fullChildName);
        }

        [Fact]
        public void CreatePropertyModelName()
        {
            // Act
            string fullChildName = ModelBinderUtil.CreatePropertyModelName("parentName", "childName");

            // Assert
            Assert.Equal("parentName.childName", fullChildName);
        }

        [Fact]
        public void CreatePropertyModelName_EmptyParentName()
        {
            // Act
            string fullChildName = ModelBinderUtil.CreatePropertyModelName("", "childName");

            // Assert
            Assert.Equal("childName", fullChildName);
        }

        [Fact]
        public void GetPossibleBinderInstance_Match_ReturnsBinder()
        {
            // Act
            IExtensibleModelBinder binder = ModelBinderUtil.GetPossibleBinderInstance(typeof(List<int>), typeof(List<>), typeof(SampleGenericBinder<>));

            // Assert
            Assert.IsType<SampleGenericBinder<int>>(binder);
        }

        [Fact]
        public void GetPossibleBinderInstance_NoMatch_ReturnsNull()
        {
            // Act
            IExtensibleModelBinder binder = ModelBinderUtil.GetPossibleBinderInstance(typeof(ArraySegment<int>), typeof(List<>), typeof(SampleGenericBinder<>));

            // Assert
            Assert.Null(binder);
        }

        [Fact]
        public void RawValueToObjectArray_RawValueIsEnumerable_ReturnsInputAsArray()
        {
            // Assert
            List<int> original = new List<int> { 1, 2, 3, 4 };

            // Act
            object[] retVal = ModelBinderUtil.RawValueToObjectArray(original);

            // Assert
            Assert.Equal(new object[] { 1, 2, 3, 4 }, retVal);
        }

        [Fact]
        public void RawValueToObjectArray_RawValueIsObject_WrapsObjectInSingleElementArray()
        {
            // Act
            object[] retVal = ModelBinderUtil.RawValueToObjectArray(42);

            // Assert
            Assert.Equal(new object[] { 42 }, retVal);
        }

        [Fact]
        public void RawValueToObjectArray_RawValueIsObjectArray_ReturnsInputInstance()
        {
            // Assert
            object[] original = new object[2];

            // Act
            object[] retVal = ModelBinderUtil.RawValueToObjectArray(original);

            // Assert
            Assert.Same(original, retVal);
        }

        [Fact]
        public void RawValueToObjectArray_RawValueIsString_WrapsStringInSingleElementArray()
        {
            // Act
            object[] retVal = ModelBinderUtil.RawValueToObjectArray("hello");

            // Assert
            Assert.Equal(new object[] { "hello" }, retVal);
        }

        [Fact]
        public void ReplaceEmptyStringWithNull_ConvertEmptyStringToNullDisabled_ModelIsEmptyString_LeavesModelAlone()
        {
            // Arrange
            ModelMetadata modelMetadata = GetMetadata(typeof(string));
            modelMetadata.ConvertEmptyStringToNull = false;

            // Act
            object model = "";
            ModelBinderUtil.ReplaceEmptyStringWithNull(modelMetadata, ref model);

            // Assert
            Assert.Equal("", model);
        }

        [Fact]
        public void ReplaceEmptyStringWithNull_ConvertEmptyStringToNullEnabled_ModelIsEmptyString_ReplacesModelWithNull()
        {
            // Arrange
            ModelMetadata modelMetadata = GetMetadata(typeof(string));
            modelMetadata.ConvertEmptyStringToNull = true;

            // Act
            object model = "";
            ModelBinderUtil.ReplaceEmptyStringWithNull(modelMetadata, ref model);

            // Assert
            Assert.Null(model);
        }

        [Fact]
        public void ReplaceEmptyStringWithNull_ConvertEmptyStringToNullEnabled_ModelIsWhitespaceString_ReplacesModelWithNull()
        {
            // Arrange
            ModelMetadata modelMetadata = GetMetadata(typeof(string));
            modelMetadata.ConvertEmptyStringToNull = true;

            // Act
            object model = "     "; // whitespace
            ModelBinderUtil.ReplaceEmptyStringWithNull(modelMetadata, ref model);

            // Assert
            Assert.Null(model);
        }

        [Fact]
        public void ReplaceEmptyStringWithNull_ConvertEmptyStringToNullDisabled_ModelIsNotEmptyString_LeavesModelAlone()
        {
            // Arrange
            ModelMetadata modelMetadata = GetMetadata(typeof(string));
            modelMetadata.ConvertEmptyStringToNull = true;

            // Act
            object model = 42;
            ModelBinderUtil.ReplaceEmptyStringWithNull(modelMetadata, ref model);

            // Assert
            Assert.Equal(42, model);
        }

        [Fact]
        public void ValidateBindingContext_SuccessWithNonNullModel()
        {
            // Arrange
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = GetMetadata(typeof(string))
            };
            bindingContext.ModelMetadata.Model = "hello!";

            // Act
            ModelBinderUtil.ValidateBindingContext(bindingContext, typeof(string), false);

            // Assert
            // Nothing to do - if we got this far without throwing, the test succeeded
        }

        [Fact]
        public void ValidateBindingContext_SuccessWithNullModel()
        {
            // Arrange
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = GetMetadata(typeof(string))
            };

            // Act
            ModelBinderUtil.ValidateBindingContext(bindingContext, typeof(string), true);

            // Assert
            // Nothing to do - if we got this far without throwing, the test succeeded
        }

        [Fact]
        public void ValidateBindingContextThrowsIfBindingContextIsNull()
        {
            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { ModelBinderUtil.ValidateBindingContext(null, typeof(string), true); }, "bindingContext");
        }

        [Fact]
        public void ValidateBindingContextThrowsIfModelInstanceIsWrongType()
        {
            // Arrange
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = GetMetadata(typeof(string))
            };
            bindingContext.ModelMetadata.Model = 42;

            // Act & assert
            Assert.Throws<ArgumentException>(
                delegate { ModelBinderUtil.ValidateBindingContext(bindingContext, typeof(string), true); },
                "The binding context has a Model of type 'System.Int32', but this binder can only operate on models of type 'System.String'." + Environment.NewLine
              + "Parameter name: bindingContext");
        }

        [Fact]
        public void ValidateBindingContextThrowsIfModelIsNullButCannotBe()
        {
            // Arrange
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = GetMetadata(typeof(string))
            };

            // Act & assert
            Assert.Throws<ArgumentException>(
                delegate { ModelBinderUtil.ValidateBindingContext(bindingContext, typeof(string), false); },
                "The binding context has a null Model, but this binder requires a non-null model of type 'System.String'." + Environment.NewLine
              + "Parameter name: bindingContext");
        }

        [Fact]
        public void ValidateBindingContextThrowsIfModelMetadataIsNull()
        {
            // Arrange
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext();

            // Act & assert
            Assert.Throws<ArgumentException>(
                delegate { ModelBinderUtil.ValidateBindingContext(bindingContext, typeof(string), true); },
                "The binding context cannot have a null ModelMetadata." + Environment.NewLine
              + "Parameter name: bindingContext");
        }

        [Fact]
        public void ValidateBindingContextThrowsIfModelTypeIsWrong()
        {
            // Arrange
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = GetMetadata(typeof(object))
            };

            // Act & assert
            Assert.Throws<ArgumentException>(
                delegate { ModelBinderUtil.ValidateBindingContext(bindingContext, typeof(string), true); },
                "The binding context has a ModelType of 'System.Object', but this binder can only operate on models of type 'System.String'." + Environment.NewLine
              + "Parameter name: bindingContext");
        }

        private static ModelMetadata GetMetadata(Type modelType)
        {
            EmptyModelMetadataProvider provider = new EmptyModelMetadataProvider();
            return provider.GetMetadataForType(null, modelType);
        }

        private class SampleGenericBinder<T> : IExtensibleModelBinder
        {
            public bool BindModel(ControllerContext controllerContext, ExtensibleModelBindingContext bindingContext)
            {
                throw new NotImplementedException();
            }
        }
    }
}
