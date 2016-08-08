// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    public class ControllerDescriptorCacheTest
    {
        [Fact]
        public void GetDescriptorCaches()
        {
            // Arrange
            Type controllerType = typeof(object);
            ControllerDescriptorCache cache = new ControllerDescriptorCache();

            // Act
            ControllerDescriptor descriptor1 = cache.GetDescriptor(controllerType, () => new ReflectedControllerDescriptor(controllerType));
            ControllerDescriptor descriptor2 = cache.GetDescriptor(controllerType, () => new ReflectedControllerDescriptor(controllerType));

            // Assert
            Assert.Same(controllerType, descriptor1.ControllerType);
            Assert.Same(descriptor1, descriptor2);
        }

        [Fact]
        public void GetDescriptorWithArgumentCaches()
        {
            // Arrange
            Type controllerType = typeof(object);
            ControllerDescriptorCache cache = new ControllerDescriptorCache();
            object argument = new object();

            // Act
            ControllerDescriptor descriptor1 = cache.GetDescriptor(
                controllerType,
                (object innerState) =>
                {
                    Assert.Same(argument, innerState);
                    return new ReflectedControllerDescriptor(controllerType);
                },
                argument);
            ControllerDescriptor descriptor2 = cache.GetDescriptor(
                controllerType,
                (object innerState) =>
                {
                    Assert.Same(argument, innerState);
                    return new ReflectedControllerDescriptor(controllerType);
                },
                argument);

            // Assert
            Assert.Same(controllerType, descriptor1.ControllerType);
            Assert.Same(descriptor1, descriptor2);
        }
    }
}
