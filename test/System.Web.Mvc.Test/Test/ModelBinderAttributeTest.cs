// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    public class ModelBinderAttributeTest
    {
        [Fact]
        public void ConstructorWithInvalidBinderTypeThrows()
        {
            // Arrange
            Type badType = typeof(string);

            // Act & Assert
            Assert.Throws<ArgumentException>(
                delegate { new ModelBinderAttribute(badType); },
                "The type 'System.String' does not implement the IModelBinder interface.\r\nParameter name: binderType");
        }

        [Fact]
        public void ConstructorWithNullBinderTypeThrows()
        {
            // Act & Assert
            Assert.ThrowsArgumentNull(
                delegate { new ModelBinderAttribute(null); }, "binderType");
        }

        [Fact]
        public void BinderTypeProperty()
        {
            // Arrange
            Type binderType = typeof(GoodConverter);
            ModelBinderAttribute attr = new ModelBinderAttribute(binderType);

            // Act & Assert
            Assert.Same(binderType, attr.BinderType);
        }

        [Fact]
        public void GetBinder()
        {
            // Arrange
            ModelBinderAttribute attr = new ModelBinderAttribute(typeof(GoodConverter));

            // Act
            IModelBinder binder = attr.GetBinder();

            // Assert
            Assert.IsType<GoodConverter>(binder);
        }

        [Fact]
        public void GetBinderWithBadConstructorThrows()
        {
            // Arrange
            ModelBinderAttribute attr = new ModelBinderAttribute(typeof(BadConverter));

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                delegate { attr.GetBinder(); },
                "An error occurred when trying to create the IModelBinder 'System.Web.Mvc.Test.ModelBinderAttributeTest+BadConverter'. Make sure that the binder has a public parameterless constructor.");
        }

        private class GoodConverter : IModelBinder
        {
            public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
            {
                throw new NotImplementedException();
            }
        }

        private class BadConverter : IModelBinder
        {
            // no public parameterless constructor
            public BadConverter(string s)
            {
            }

            public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
            {
                throw new NotImplementedException();
            }
        }
    }
}
