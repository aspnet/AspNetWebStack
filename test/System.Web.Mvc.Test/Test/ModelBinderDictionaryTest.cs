// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.TestUtil;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class ModelBinderDictionaryTest
    {
        [Fact]
        public void DefaultBinderIsInstanceOfDefaultModelBinder()
        {
            // Arrange
            ModelBinderDictionary binders = new ModelBinderDictionary();

            // Act
            IModelBinder defaultBinder = binders.DefaultBinder;

            // Assert
            Assert.IsType<DefaultModelBinder>(defaultBinder);
        }

        [Fact]
        public void DefaultBinderProperty()
        {
            // Arrange
            ModelBinderDictionary binders = new ModelBinderDictionary();
            IModelBinder binder = new Mock<IModelBinder>().Object;

            // Act & assert
            MemberHelper.TestPropertyWithDefaultInstance(binders, "DefaultBinder", binder);
        }

        [Fact]
        public void DictionaryInterface()
        {
            // Arrange
            DictionaryHelper<Type, IModelBinder> helper = new DictionaryHelper<Type, IModelBinder>()
            {
                Creator = () => new ModelBinderDictionary(),
                SampleKeys = new Type[] { typeof(object), typeof(string), typeof(int), typeof(long), typeof(long) },
                SampleValues = new IModelBinder[] { new DefaultModelBinder(), new DefaultModelBinder(), new DefaultModelBinder(), new DefaultModelBinder(), new DefaultModelBinder() },
                ThrowOnKeyNotFound = false
            };

            // Act & assert
            helper.Execute();
        }

        [Fact]
        public void GetBinderConsultsProviders()
        {
            // Arrange
            Type modelType = typeof(string);
            IModelBinder expectedBinderFromProvider = new Mock<IModelBinder>().Object;

            Mock<IModelBinderProvider> locatedProvider = new Mock<IModelBinderProvider>();
            locatedProvider.Setup(p => p.GetBinder(modelType))
                .Returns(expectedBinderFromProvider);

            Mock<IModelBinderProvider> secondProvider = new Mock<IModelBinderProvider>();

            ModelBinderProviderCollection providers = new ModelBinderProviderCollection(new IModelBinderProvider[] { locatedProvider.Object, secondProvider.Object });
            ModelBinderDictionary binders = new ModelBinderDictionary(providers);

            // Act
            IModelBinder returnedBinder = binders.GetBinder(modelType);

            // Assert
            Assert.Same(expectedBinderFromProvider, returnedBinder);
        }

        [Fact]
        public void GetBinderDoesNotReturnDefaultBinderIfAskedNotTo()
        {
            // Proper order of precedence:
            // 1. Binder registered in the global table
            // 2. Binder attribute defined on the type
            // 3. <null>

            // Arrange
            IModelBinder registeredFirstBinder = new Mock<IModelBinder>().Object;
            ModelBinderDictionary binders = new ModelBinderDictionary()
            {
                { typeof(MyFirstConvertibleType), registeredFirstBinder }
            };

            // Act
            IModelBinder binder1 = binders.GetBinder(typeof(MyFirstConvertibleType), false /* fallbackToDefault */);
            IModelBinder binder2 = binders.GetBinder(typeof(MySecondConvertibleType), false /* fallbackToDefault */);
            IModelBinder binder3 = binders.GetBinder(typeof(object), false /* fallbackToDefault */);

            // Assert
            Assert.Same(registeredFirstBinder, binder1);
            Assert.IsType<MySecondBinder>(binder2);
            Assert.Null(binder3);
        }

        [Fact]
        public void GetBinderResolvesBindersWithCorrectPrecedence()
        {
            // Proper order of precedence:
            // 1. Binder registered in the global table
            // 2. Binder attribute defined on the type
            // 3. Default binder

            // Arrange
            IModelBinder registeredFirstBinder = new Mock<IModelBinder>().Object;
            ModelBinderDictionary binders = new ModelBinderDictionary()
            {
                { typeof(MyFirstConvertibleType), registeredFirstBinder }
            };

            IModelBinder defaultBinder = new Mock<IModelBinder>().Object;
            binders.DefaultBinder = defaultBinder;

            // Act
            IModelBinder binder1 = binders.GetBinder(typeof(MyFirstConvertibleType));
            IModelBinder binder2 = binders.GetBinder(typeof(MySecondConvertibleType));
            IModelBinder binder3 = binders.GetBinder(typeof(object));

            // Assert
            Assert.Same(registeredFirstBinder, binder1);
            Assert.IsType<MySecondBinder>(binder2);
            Assert.Same(defaultBinder, binder3);
        }

        [Fact]
        public void GetBinderThrowsIfModelTypeContainsMultipleAttributes()
        {
            // Arrange
            ModelBinderDictionary binders = new ModelBinderDictionary();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                delegate { binders.GetBinder(typeof(ConvertibleTypeWithSeveralBinders), true /* fallbackToDefault */); },
                "The type 'System.Web.Mvc.Test.ModelBinderDictionaryTest+ConvertibleTypeWithSeveralBinders' contains multiple attributes that inherit from CustomModelBinderAttribute.");
        }

        [Fact]
        public void GetBinderThrowsIfModelTypeIsNull()
        {
            // Arrange
            ModelBinderDictionary binders = new ModelBinderDictionary();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                delegate { binders.GetBinder(null); }, "modelType");
        }

        [ModelBinder(typeof(MyFirstBinder))]
        private class MyFirstConvertibleType
        {
        }

        private class MyFirstBinder : IModelBinder
        {
            public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
            {
                throw new NotImplementedException();
            }
        }

        [ModelBinder(typeof(MySecondBinder))]
        private class MySecondConvertibleType
        {
        }

        private class MySecondBinder : IModelBinder
        {
            public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
            {
                throw new NotImplementedException();
            }
        }

        [ModelBinder(typeof(MySecondBinder))]
        [MySubclassedBinder]
        private class ConvertibleTypeWithSeveralBinders
        {
        }

        private class MySubclassedBinderAttribute : CustomModelBinderAttribute
        {
            public override IModelBinder GetBinder()
            {
                throw new NotImplementedException();
            }
        }
    }
}
