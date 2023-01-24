// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dependencies;
using System.Web.Http.Hosting;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Dispatcher
{
    public class DefaultHttpControllerActivatorTest
    {
        // Create tests

        [Fact]
        public void Create_GuardClauses()
        {
            // Arrange
            var config = new HttpConfiguration();
            var request = new HttpRequestMessage();
            request.SetConfiguration(config);
            var descriptor = new HttpControllerDescriptor(config, "Simple", typeof(SimpleController));
            var activator = new DefaultHttpControllerActivator();

            // Act & assert
            Assert.ThrowsArgumentNull(() => activator.Create(request: null, controllerDescriptor: descriptor, controllerType: typeof(SimpleController)), "request");
            Assert.ThrowsArgumentNull(() => activator.Create(request, controllerDescriptor: null, controllerType: typeof(SimpleController)), "controllerDescriptor");
            Assert.ThrowsArgumentNull(() => activator.Create(request, descriptor, controllerType: null), "controllerType");
            Assert.Throws<InvalidOperationException>(
                () => activator.Create(request, descriptor, typeof(AbstractController)),
                "An error occurred when trying to create a controller of type 'AbstractController'. Make sure that the controller has a parameterless public constructor.");
            Assert.Throws<InvalidOperationException>(
                () => activator.Create(request, descriptor, typeof(ControllerWithCtorParams)),
                "An error occurred when trying to create a controller of type 'ControllerWithCtorParams'. Make sure that the controller has a parameterless public constructor.");
        }

        [Fact]
        public void Create_MakesInstanceOfController()
        {
            // Arrange
            var config = new HttpConfiguration();
            var request = new HttpRequestMessage();
            request.SetConfiguration(config);
            var descriptor = new HttpControllerDescriptor(config, "Simple", typeof(SimpleController));
            var activator = new DefaultHttpControllerActivator();

            // Act
            IHttpController result = activator.Create(request, descriptor, typeof(SimpleController));

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SimpleController>(result);
        }

        [Fact]
        public void Create_UsesControllerFromRequestLevelDependencyScope()
        {
            // Arrange
            var controller = new ControllerWithCtorParams(42);
            var mockScope = new Mock<IDependencyScope>();
            mockScope.Setup(r => r.GetService(typeof(ControllerWithCtorParams))).Returns(controller).Verifiable();
            var config = new HttpConfiguration();
            var request = new HttpRequestMessage();
            request.SetConfiguration(config);
            request.Properties[HttpPropertyKeys.DependencyScope] = mockScope.Object;
            var descriptor = new HttpControllerDescriptor(config, "Name", typeof(ControllerWithCtorParams));
            var activator = new DefaultHttpControllerActivator();

            // Act
            IHttpController result = activator.Create(request, descriptor, typeof(ControllerWithCtorParams));

            // Assert
            Assert.Same(controller, result);
            mockScope.Verify();
        }

        [Fact]
        public void Create_DoesnotCacheControllerFromRequestLevelDependencyScope()
        {
            // Arrange
            int count = 0;
            var controller = new ControllerWithCtorParams(42);
            var mockScope = new Mock<IDependencyScope>();
            mockScope.Setup(r => r.GetService(typeof(ControllerWithCtorParams))).Returns(() =>
            {
                count++;
                return new ControllerWithCtorParams(42);
            }).Verifiable();
            var config = new HttpConfiguration();
            var request = new HttpRequestMessage();
            request.SetConfiguration(config);
            request.Properties[HttpPropertyKeys.DependencyScope] = mockScope.Object;
            var descriptor = new HttpControllerDescriptor(config, "Name", typeof(ControllerWithCtorParams));
            var activator = new DefaultHttpControllerActivator();

            // Act
            IHttpController result1 = activator.Create(request, descriptor, typeof(ControllerWithCtorParams));
            IHttpController result2 = activator.Create(request, descriptor, typeof(ControllerWithCtorParams));

            // Assert
            Assert.NotEqual(result1, result2);
            mockScope.Verify();
            Assert.Equal(2, count);
        }

        [Fact]
        public void Create_MixupInstanceCreationAndDependencyScope()
        {
            // Arrange
            var controller = new ControllerWithCtorParams(42);
            var mockScope = new Mock<IDependencyScope>();
            mockScope.Setup(r => r.GetService(typeof(ControllerWithCtorParams))).Returns(controller).Verifiable();
            var config = new HttpConfiguration();
            var request = new HttpRequestMessage();
            request.SetConfiguration(config);
            request.Properties[HttpPropertyKeys.DependencyScope] = mockScope.Object;
            var descriptorControllerWithCtorParamsResult = new HttpControllerDescriptor(config, "Name", typeof(ControllerWithCtorParams));
            var descriptorSimpleController = new HttpControllerDescriptor(config, "Simple", typeof(SimpleController));
            var activator = new DefaultHttpControllerActivator();

            // Act
            IHttpController simpleController = activator.Create(request, descriptorSimpleController, typeof(SimpleController));
            IHttpController controllerWithCtorParamsResult = activator.Create(request, descriptorControllerWithCtorParamsResult, typeof(ControllerWithCtorParams));

            // Assert
            Assert.NotNull(simpleController);
            Assert.IsType<SimpleController>(simpleController);
            Assert.Same(controller, controllerWithCtorParamsResult);
            mockScope.Verify();
        }

        [Fact]
        public void Create_ThrowsForNullDependencyScope()
        {
            // Arrange
            var config = new HttpConfiguration();
            var mockResolver = new Mock<IDependencyResolver>();
            mockResolver.Setup(resolver => resolver.BeginScope()).Returns((IDependencyScope)null).Verifiable();
            config.DependencyResolver = mockResolver.Object;
            var request = new HttpRequestMessage();
            request.SetConfiguration(config);
            var descriptorSimpleController = new HttpControllerDescriptor(config, "Simple", typeof(SimpleController));
            var activator = new DefaultHttpControllerActivator();

            // Act & Assert
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
                () => activator.Create(request, descriptorSimpleController, typeof(SimpleController)));

            Assert.Equal(
                "An error occurred when trying to create a controller of type 'SimpleController'. Make sure that the controller has a parameterless public constructor.",
                exception.Message);
            Assert.NotNull(exception.InnerException);
            Assert.Matches(
                "A dependency resolver of type 'IDependencyResolverProxy' returned an invalid value of null from its " +
                "BeginScope method. If the container does not have a concept of scope, consider returning a scope " +
                "that resolves in the root of the container instead.",
                exception.InnerException.Message);

            mockResolver.Verify();
        }

        // Helper classes

        abstract class AbstractController : ApiController { }

        class SimpleController : ApiController { }

        class ControllerWithCtorParams : ApiController
        {
            public ControllerWithCtorParams(int unused) { }
        }
    }
}
