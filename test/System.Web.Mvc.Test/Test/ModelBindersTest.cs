// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.Data.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class ModelBindersTest
    {
        [Fact]
        public void BindersPropertyIsNotNull()
        {
            // Arrange & Act
            ModelBinderDictionary binders = ModelBinders.Binders;

            // Assert
            Assert.NotNull(binders);
        }

        [Fact]
        public void DefaultModelBinders()
        {
            // Act
            ModelBinderDictionary binders = ModelBinders.Binders;

            // Assert
            Assert.Equal(4, binders.Count);
            Assert.True(binders.ContainsKey(typeof(byte[])));
            Assert.IsType<ByteArrayModelBinder>(binders[typeof(byte[])]);
            Assert.True(binders.ContainsKey(typeof(HttpPostedFileBase)));
            Assert.IsType<HttpPostedFileBaseModelBinder>(binders[typeof(HttpPostedFileBase)]);
            Assert.True(binders.ContainsKey(typeof(Binary)));
            Assert.IsType<LinqBinaryModelBinder>(binders[typeof(Binary)]);
            Assert.True(binders.ContainsKey(typeof(CancellationToken)));
            Assert.IsType<CancellationTokenModelBinder>(binders[typeof(CancellationToken)]);
        }

        [Fact]
        public void GetBindersFromAttributes_ReadsModelBinderAttributeFromBuddyClass()
        {
            Action<Type> errorAction = (Type t) => { throw new InvalidOperationException(); };
            // Act
            IModelBinder binder = ModelBinders.GetBinderFromAttributes(typeof(SampleModel), errorAction);

            // Assert
            Assert.IsType<SampleModelBinder>(binder);
        }

        [Fact]
        public void GetBindersFromAttributes_NullIfGetCustomAttributesReturnsNull()
        {
            // Arrange
            var provider = new Mock<ICustomAttributeProvider>(MockBehavior.Strict);
            bool inherit = true;
            provider.Setup(p => p.GetCustomAttributes(typeof(CustomModelBinderAttribute), inherit)).Returns((object[])null);
            Action<ICustomAttributeProvider> errorAction = (ICustomAttributeProvider t) => { throw new InvalidOperationException(); };

            // Act
            IModelBinder binder = ModelBinders.GetBinderFromAttributes(provider.Object, errorAction);

            // Assert
            Assert.Null(binder);
        }

        [MetadataType(typeof(SampleModel_Buddy))]
        private class SampleModel
        {
            [ModelBinder(typeof(SampleModelBinder))]
            private class SampleModel_Buddy
            {
            }
        }

        private class SampleModelBinder : IModelBinder
        {
            public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
            {
                throw new NotImplementedException();
            }
        }
    }
}
