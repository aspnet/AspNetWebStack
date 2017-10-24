// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Reflection;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class ReflectedControllerDescriptorTest
    {
        [Fact]
        public void ConstructorSetsControllerTypeProperty()
        {
            // Arrange
            Type controllerType = typeof(string);

            // Act
            ReflectedControllerDescriptor cd = new ReflectedControllerDescriptor(controllerType);

            // Assert
            Assert.Same(controllerType, cd.ControllerType);
        }

        [Fact]
        public void ConstructorThrowsIfControllerTypeIsNull()
        {
            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { new ReflectedControllerDescriptor(null); }, "controllerType");
        }

        [Fact]
        public void FindActionReturnsActionDescriptorIfFound()
        {
            // Arrange
            Type controllerType = typeof(MyController);
            MethodInfo targetMethod = controllerType.GetMethod("AliasedMethod");
            ReflectedControllerDescriptor cd = new ReflectedControllerDescriptor(controllerType);

            // Act
            ActionDescriptor ad = cd.FindAction(new ControllerContext(), "NewName");

            // Assert
            Assert.Equal("NewName", ad.ActionName);
            ReflectedActionDescriptor actionDescriptor = Assert.IsType<ReflectedActionDescriptor>(ad);
            Assert.Same(targetMethod, actionDescriptor.MethodInfo);
            Assert.Same(cd, ad.ControllerDescriptor);
        }

        [Fact]
        public void FindActionReturnsNullIfNoActionFound()
        {
            // Arrange
            Type controllerType = typeof(MyController);
            ReflectedControllerDescriptor cd = new ReflectedControllerDescriptor(controllerType);

            // Act
            ActionDescriptor ad = cd.FindAction(new ControllerContext(), "NonExistent");

            // Assert
            Assert.Null(ad);
        }

        [Fact]
        public void FindActionThrowsIfActionNameIsEmpty()
        {
            // Arrange
            Type controllerType = typeof(MyController);
            ReflectedControllerDescriptor cd = new ReflectedControllerDescriptor(controllerType);

            // Act & assert
            Assert.ThrowsArgumentNullOrEmpty(
                delegate { cd.FindAction(new Mock<ControllerContext>().Object, ""); }, "actionName");
        }

        [Fact]
        public void FindActionThrowsIfActionNameIsNull()
        {
            // Arrange
            Type controllerType = typeof(MyController);
            ReflectedControllerDescriptor cd = new ReflectedControllerDescriptor(controllerType);

            // Act & assert
            Assert.ThrowsArgumentNullOrEmpty(
                delegate { cd.FindAction(new Mock<ControllerContext>().Object, null); }, "actionName");
        }

        [Fact]
        public void FindActionThrowsIfControllerContextIsNull()
        {
            // Arrange
            Type controllerType = typeof(MyController);
            ReflectedControllerDescriptor cd = new ReflectedControllerDescriptor(controllerType);

            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { cd.FindAction(null, "someName"); }, "controllerContext");
        }

        [Fact]
        public void GetCanonicalActionsWrapsMethodInfos()
        {
            // Arrange
            Type controllerType = typeof(MyController);
            MethodInfo mInfo0 = controllerType.GetMethod("AliasedMethod");
            MethodInfo mInfo1 = controllerType.GetMethod("NonAliasedMethod");
            ReflectedControllerDescriptor cd = new ReflectedControllerDescriptor(controllerType);

            // Act
            ActionDescriptor[] aDescsFirstCall = cd.GetCanonicalActions();
            ActionDescriptor[] aDescsSecondCall = cd.GetCanonicalActions();

            // Assert
            Assert.NotSame(aDescsFirstCall, aDescsSecondCall);
            Assert.True(aDescsFirstCall.SequenceEqual(aDescsSecondCall));
            Assert.Equal(2, aDescsFirstCall.Length);

            ReflectedActionDescriptor aDesc0 = aDescsFirstCall[0] as ReflectedActionDescriptor;
            ReflectedActionDescriptor aDesc1 = aDescsFirstCall[1] as ReflectedActionDescriptor;

            Assert.NotNull(aDesc0);
            Assert.Equal("AliasedMethod", aDesc0.ActionName);
            Assert.Same(mInfo0, aDesc0.MethodInfo);
            Assert.Same(cd, aDesc0.ControllerDescriptor);
            Assert.NotNull(aDesc1);
            Assert.Equal("NonAliasedMethod", aDesc1.ActionName);
            Assert.Same(mInfo1, aDesc1.MethodInfo);
            Assert.Same(cd, aDesc1.ControllerDescriptor);
        }

        [Fact]
        public void GetCustomAttributesCallsTypeGetCustomAttributes()
        {
            // Arrange
            object[] expected = new object[0];
            Mock<Type> mockType = new Mock<Type>();
            mockType.Setup(t => t.GetCustomAttributes(true)).Returns(expected);
            ReflectedControllerDescriptor cd = new ReflectedControllerDescriptor(mockType.Object);

            // Act
            object[] returned = cd.GetCustomAttributes(true);

            // Assert
            Assert.Same(expected, returned);
        }

        [Fact]
        public void GetCustomAttributesWithAttributeTypeCallsTypeGetCustomAttributes()
        {
            // Arrange
            object[] expected = new object[0];
            Mock<Type> mockType = new Mock<Type>();
            mockType.Setup(t => t.GetCustomAttributes(typeof(ObsoleteAttribute), true)).Returns(expected);
            ReflectedControllerDescriptor cd = new ReflectedControllerDescriptor(mockType.Object);

            // Act
            object[] returned = cd.GetCustomAttributes(typeof(ObsoleteAttribute), true);

            // Assert
            Assert.Same(expected, returned);
        }

        [Fact]
        public void IsDefinedCallsTypeIsDefined()
        {
            // Arrange
            Mock<Type> mockType = new Mock<Type>();
            mockType.Setup(t => t.IsDefined(typeof(ObsoleteAttribute), true)).Returns(true);
            ReflectedControllerDescriptor cd = new ReflectedControllerDescriptor(mockType.Object);

            // Act
            bool isDefined = cd.IsDefined(typeof(ObsoleteAttribute), true);

            // Assert
            Assert.True(isDefined);
        }

        private class MyController : Controller
        {
            [ActionName("NewName")]
            public void AliasedMethod()
            {
            }

            public void NonAliasedMethod()
            {
            }

            public void GenericMethod<T>()
            {
            }

            private void PrivateMethod()
            {
            }
        }
    }
}
