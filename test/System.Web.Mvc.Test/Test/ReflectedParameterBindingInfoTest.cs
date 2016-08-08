// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    public class ReflectedParameterBindingInfoTest
    {
        [Fact]
        public void BinderProperty()
        {
            // Arrange
            ParameterInfo pInfo = typeof(MyController).GetMethod("ParameterHasSingleModelBinderAttribute").GetParameters()[0];
            ReflectedParameterBindingInfo bindingInfo = new ReflectedParameterBindingInfo(pInfo);

            // Act
            IModelBinder binder = bindingInfo.Binder;

            // Assert
            Assert.IsType<MyModelBinder>(binder);
        }

        [Fact]
        public void BinderPropertyThrowsIfMultipleBinderAttributesFound()
        {
            // Arrange
            ParameterInfo pInfo = typeof(MyController).GetMethod("ParameterHasMultipleModelBinderAttributes").GetParameters()[0];
            ReflectedParameterBindingInfo bindingInfo = new ReflectedParameterBindingInfo(pInfo);

            // Act & assert
            Assert.Throws<InvalidOperationException>(
                delegate { IModelBinder binder = bindingInfo.Binder; },
                "The parameter 'p1' on method 'Void ParameterHasMultipleModelBinderAttributes(System.Object)' contains multiple attributes that inherit from CustomModelBinderAttribute.");
        }

        [Fact]
        public void ExcludeProperty()
        {
            // Arrange
            ParameterInfo pInfo = typeof(MyController).GetMethod("ParameterHasBindAttribute").GetParameters()[0];
            ReflectedParameterBindingInfo bindingInfo = new ReflectedParameterBindingInfo(pInfo);

            // Act
            ICollection<string> excludes = bindingInfo.Exclude;

            // Assert
            Assert.IsType<ReadOnlyCollection<string>>(excludes);

            string[] excludesArray = excludes.ToArray();
            Assert.Equal(2, excludesArray.Length);
            Assert.Equal("excl_a", excludesArray[0]);
            Assert.Equal("excl_b", excludesArray[1]);
        }

        [Fact]
        public void ExcludePropertyReturnsEmptyArrayIfNoBindAttributeSpecified()
        {
            // Arrange
            ParameterInfo pInfo = typeof(MyController).GetMethod("ParameterHasNoBindAttributes").GetParameters()[0];
            ReflectedParameterBindingInfo bindingInfo = new ReflectedParameterBindingInfo(pInfo);

            // Act
            ICollection<string> excludes = bindingInfo.Exclude;

            // Assert
            Assert.NotNull(excludes);
            Assert.Empty(excludes);
        }

        [Fact]
        public void IncludeProperty()
        {
            // Arrange
            ParameterInfo pInfo = typeof(MyController).GetMethod("ParameterHasBindAttribute").GetParameters()[0];
            ReflectedParameterBindingInfo bindingInfo = new ReflectedParameterBindingInfo(pInfo);

            // Act
            ICollection<string> includes = bindingInfo.Include;

            // Assert
            Assert.IsType<ReadOnlyCollection<string>>(includes);

            string[] includesArray = includes.ToArray();
            Assert.Equal(2, includesArray.Length);
            Assert.Equal("incl_a", includesArray[0]);
            Assert.Equal("incl_b", includesArray[1]);
        }

        [Fact]
        public void IncludePropertyReturnsEmptyArrayIfNoBindAttributeSpecified()
        {
            // Arrange
            ParameterInfo pInfo = typeof(MyController).GetMethod("ParameterHasNoBindAttributes").GetParameters()[0];
            ReflectedParameterBindingInfo bindingInfo = new ReflectedParameterBindingInfo(pInfo);

            // Act
            ICollection<string> includes = bindingInfo.Include;

            // Assert
            Assert.NotNull(includes);
            Assert.Empty(includes);
        }

        [Fact]
        public void PrefixProperty()
        {
            // Arrange
            ParameterInfo pInfo = typeof(MyController).GetMethod("ParameterHasBindAttribute").GetParameters()[0];
            ReflectedParameterBindingInfo bindingInfo = new ReflectedParameterBindingInfo(pInfo);

            // Act
            string prefix = bindingInfo.Prefix;

            // Assert
            Assert.Equal("some prefix", prefix);
        }

        [Fact]
        public void PrefixPropertyReturnsNullIfNoBindAttributeSpecified()
        {
            // Arrange
            ParameterInfo pInfo = typeof(MyController).GetMethod("ParameterHasNoBindAttributes").GetParameters()[0];
            ReflectedParameterBindingInfo bindingInfo = new ReflectedParameterBindingInfo(pInfo);

            // Act
            string prefix = bindingInfo.Prefix;

            // Assert
            Assert.Null(prefix);
        }

        private class MyController : Controller
        {
            public void ParameterHasBindAttribute(
                [Bind(Prefix = "some prefix", Include = "incl_a, incl_b", Exclude = "excl_a, excl_b")] object p1)
            {
            }

            public void ParameterHasNoBindAttributes(object p1)
            {
            }

            public void ParameterHasSingleModelBinderAttribute([ModelBinder(typeof(MyModelBinder))] object p1)
            {
            }

            public void ParameterHasMultipleModelBinderAttributes([MyCustomModelBinder, MyCustomModelBinder] object p1)
            {
            }
        }

        [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true, Inherited = true)]
        private class MyCustomModelBinderAttribute : CustomModelBinderAttribute
        {
            public override IModelBinder GetBinder()
            {
                throw new NotImplementedException();
            }
        }

        private class MyModelBinder : IModelBinder
        {
            public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
            {
                throw new NotImplementedException();
            }
        }
    }
}
