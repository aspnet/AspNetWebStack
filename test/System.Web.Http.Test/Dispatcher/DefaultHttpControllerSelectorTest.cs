// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Dispatcher
{
    public class DefaultHttpControllerSelectorTest
    {
        [Fact]
        public void Constructor_Throws_NullConfiguration()
        {
            Assert.ThrowsArgumentNull(
                () => new DefaultHttpControllerSelector(configuration: null),
                "configuration");
        }

        [Theory]
        [InlineData("controller", "abc")]
        [InlineData("Controller", "123")]
        [InlineData("ControLler", "123")]
        [InlineData("CONTROLLER", "ABC")]
        public void GetControllerName_PicksControllerNameFromRouteData(string controllerKeyName, string controllerName)
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            IHttpRouteData routeData = GetRouteData();
            routeData.Values[controllerKeyName] = controllerName;
            request.SetRouteData(routeData);
            DefaultHttpControllerSelector selector = new DefaultHttpControllerSelector(new HttpConfiguration());

            // Act
            string selectedControllerName = selector.GetControllerName(request);

            // Assert
            Assert.Equal(controllerName, selectedControllerName);
        }

        [Fact]
        public void GetControllerName_PicksNull_NoRouteData()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            DefaultHttpControllerSelector selector = new DefaultHttpControllerSelector(new HttpConfiguration());

            // Act
            string selectedControllerName = selector.GetControllerName(request);

            // Assert
            Assert.Null(selectedControllerName);
        }

        [Fact]
        public void GetControllerName_PicksNull_EmptyRouteData()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            request.SetRouteData(GetRouteData());
            DefaultHttpControllerSelector selector = new DefaultHttpControllerSelector(new HttpConfiguration());

            // Act
            string selectedControllerName = selector.GetControllerName(request);

            // Assert
            Assert.Null(selectedControllerName);
        }

        [Fact]
        public void DefaultHttpControllerSelector_Uses_IAssemblyResolverAndIHttpControllerTypeResolver()
        {
            // Arrange
            HttpConfiguration configuration = new HttpConfiguration();
            Mock<IAssembliesResolver> assemblyResolver = new Mock<IAssembliesResolver>();
            Mock<IHttpControllerTypeResolver> controllerTypeResolver = new Mock<IHttpControllerTypeResolver>();
            configuration.Services.Replace(typeof(IAssembliesResolver), assemblyResolver.Object);
            configuration.Services.Replace(typeof(IHttpControllerTypeResolver), controllerTypeResolver.Object);

            controllerTypeResolver.Setup(c => c.GetControllerTypes(assemblyResolver.Object)).Returns(new Collection<Type> { GetMockControllerType("Sample") }).Verifiable();
            DefaultHttpControllerSelector selector = new DefaultHttpControllerSelector(configuration);

            HttpRequestMessage request = new HttpRequestMessage();
            IHttpRouteData routeData = GetRouteData();
            routeData.Values["controller"] = "Sample";
            request.SetRouteData(routeData);

            // Act
            selector.SelectController(request);

            // Assert
            controllerTypeResolver.Verify();
        }

        [Theory]
        [InlineData("Sample")]
        [InlineData("SAmple")]
        [InlineData("SAMPLE")]
        public void SelectController_IsCaseInsensitive(string controllerTypeName)
        {
            HttpConfiguration configuration = new HttpConfiguration();
            Mock<IHttpControllerTypeResolver> controllerTypeResolver = new Mock<IHttpControllerTypeResolver>();
            configuration.Services.Replace(typeof(IHttpControllerTypeResolver), controllerTypeResolver.Object);

            Type controllerType = GetMockControllerType("Sample");
            controllerTypeResolver
                .Setup(c => c.GetControllerTypes(It.IsAny<IAssembliesResolver>()))
                .Returns(new Collection<Type> { controllerType });

            HttpRequestMessage request = new HttpRequestMessage();
            IHttpRouteData routeData = GetRouteData();
            routeData.Values["controller"] = controllerTypeName;
            request.SetRouteData(routeData);

            DefaultHttpControllerSelector selector = new DefaultHttpControllerSelector(configuration);

            // Act
            HttpControllerDescriptor descriptor = selector.SelectController(request);

            // Assert
            Assert.IsType<HttpControllerDescriptor>(descriptor);
            Assert.Equal(controllerType, descriptor.ControllerType);
        }

        [Fact]
        public void SelectController_DoesNotCreateNewInstances_ForSameController()
        {
            HttpConfiguration configuration = new HttpConfiguration();
            Mock<IHttpControllerTypeResolver> controllerTypeResolver = new Mock<IHttpControllerTypeResolver>();
            configuration.Services.Replace(typeof(IHttpControllerTypeResolver), controllerTypeResolver.Object);

            Type controllerType = GetMockControllerType("Sample");
            controllerTypeResolver
                .Setup(c => c.GetControllerTypes(It.IsAny<IAssembliesResolver>()))
                .Returns(new Collection<Type> { controllerType });

            HttpRequestMessage request = new HttpRequestMessage();
            IHttpRouteData routeData = GetRouteData();
            routeData.Values["controller"] = "Sample";
            request.SetRouteData(routeData);

            DefaultHttpControllerSelector selector = new DefaultHttpControllerSelector(configuration);

            // Act
            HttpControllerDescriptor descriptor1 = selector.SelectController(request);
            HttpControllerDescriptor descriptor2 = selector.SelectController(request);

            // Assert
            Assert.ReferenceEquals(descriptor1, descriptor2);
        }

        [Fact]
        public void SelectController_DoesNotCreateNewInstances_ForSameController_DiferentCasedControllerName()
        {
            HttpConfiguration configuration = new HttpConfiguration();
            Mock<IHttpControllerTypeResolver> controllerTypeResolver = new Mock<IHttpControllerTypeResolver>();
            configuration.Services.Replace(typeof(IHttpControllerTypeResolver), controllerTypeResolver.Object);

            Type controllerType = GetMockControllerType("Sample");
            controllerTypeResolver
                .Setup(c => c.GetControllerTypes(It.IsAny<IAssembliesResolver>()))
                .Returns(new Collection<Type> { controllerType });

            HttpRequestMessage request1 = new HttpRequestMessage();
            IHttpRouteData routeData1 = GetRouteData();
            routeData1.Values["controller"] = "Sample";
            request1.SetRouteData(routeData1);

            HttpRequestMessage request2 = new HttpRequestMessage();
            IHttpRouteData routeData2 = GetRouteData();
            routeData2.Values["controller"] = "SaMPle";
            request2.SetRouteData(routeData2);

            DefaultHttpControllerSelector selector = new DefaultHttpControllerSelector(configuration);

            // Act
            HttpControllerDescriptor descriptor1 = selector.SelectController(request1);
            HttpControllerDescriptor descriptor2 = selector.SelectController(request2);

            // Assert
            Assert.ReferenceEquals(descriptor1, descriptor2);
        }

        [Fact]
        public void SelectController_Throws_NullRequest()
        {
            DefaultHttpControllerSelector selector = new DefaultHttpControllerSelector(new HttpConfiguration());

            Assert.ThrowsArgumentNull(
                () => selector.SelectController(request: null),
                "request");
        }

        [Fact]
        public void SelectController_Throws_NotFound_NoControllerInRouteData()
        {
            HttpConfiguration configuration = new HttpConfiguration();
            HttpRequestMessage request = new HttpRequestMessage();
            request.SetRouteData(GetRouteData());

            DefaultHttpControllerSelector selector = new DefaultHttpControllerSelector(configuration);

            // Act
            var ex = Assert.Throws<HttpResponseException>(
                () => selector.SelectController(request));

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, ex.Response.StatusCode);
        }

        [Fact]
        public void SelectController_RespectsDirectRoutes()
        {
            HttpConfiguration configuration = new HttpConfiguration();
            HttpRequestMessage request = new HttpRequestMessage();
            var controllerDescriptor = new HttpControllerDescriptor();
            var action1Descriptor = new ReflectedHttpActionDescriptor() { ControllerDescriptor = controllerDescriptor };
            var action2Descriptor = new ReflectedHttpActionDescriptor() { ControllerDescriptor = controllerDescriptor };
            IHttpRouteData routeData = GetRouteData();
            routeData.Route.DataTokens.Add("actions", new ReflectedHttpActionDescriptor[] { action1Descriptor, action2Descriptor });
            request.SetRouteData(routeData);

            DefaultHttpControllerSelector selector = new DefaultHttpControllerSelector(configuration);

            // Act
            var selectedController = selector.SelectController(request);

            // Assert
            Assert.Same(controllerDescriptor, selectedController);
        }

        [Fact]
        public void SelectController_ThrowsOnDirectRoutesWithDifferentControllers()
        {
            var action1Descriptor = new ReflectedHttpActionDescriptor()
            {
                ControllerDescriptor = new HttpControllerDescriptor()
                {
                    ControllerType = GetMockControllerType("Controller1"),
                }
            };

            var action2Descriptor = new ReflectedHttpActionDescriptor()
            {
                ControllerDescriptor = new HttpControllerDescriptor()
                {
                    ControllerType = GetMockControllerType("Controller2"),
                }
            };

            IHttpRouteData routeData = GetRouteData();
            routeData.Route.DataTokens.Add("actions", new ReflectedHttpActionDescriptor[] { action1Descriptor, action2Descriptor });

            HttpConfiguration configuration = new HttpConfiguration();
            HttpRequestMessage request = new HttpRequestMessage();
            request.SetRouteData(routeData);

            DefaultHttpControllerSelector selector = new DefaultHttpControllerSelector(configuration);

            string expectedMessage =
                "Multiple controller types were found that match the URL. This can happen if attribute routes on multiple " +
                "controllers match the requested URL." + Environment.NewLine +
                Environment.NewLine +
                "The request has found the following matching controller types: " + Environment.NewLine +
                "FullController1Controller" + Environment.NewLine +
                "FullController2Controller";

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => selector.SelectController(request), expectedMessage);
        }

        [Fact]
        public void SelectController_Throws_NotFound_NoRouteData()
        {
            HttpConfiguration configuration = new HttpConfiguration();
            HttpRequestMessage request = new HttpRequestMessage();

            DefaultHttpControllerSelector selector = new DefaultHttpControllerSelector(configuration);

            // Act
            var ex = Assert.Throws<HttpResponseException>(
                () => selector.SelectController(request));

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, ex.Response.StatusCode);
        }

        // We ignore attribute routes when RouteData.Route is null
        // See: https://aspnetwebstack.codeplex.com/workitem/1996
        [Fact]
        public void SelectController_WhenRouteData_RouteIsNull_FindsController()
        {
            // Arrange
            HttpConfiguration configuration = new HttpConfiguration();
            Mock<IHttpControllerTypeResolver> controllerTypeResolver = new Mock<IHttpControllerTypeResolver>();
            configuration.Services.Replace(typeof(IHttpControllerTypeResolver), controllerTypeResolver.Object);

            Type controllerType = GetMockControllerType("Sample");
            controllerTypeResolver
                .Setup(c => c.GetControllerTypes(It.IsAny<IAssembliesResolver>()))
                .Returns(new Collection<Type> { controllerType });

            HttpRequestMessage request = new HttpRequestMessage();

            var routeValues = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                { "controller", "Sample" }
            };

            var routeData = new Mock<IHttpRouteData>();
            routeData.SetupGet(rd => rd.Values).Returns(routeValues);
            routeData.SetupGet(rd => rd.Route).Returns((IHttpRoute)null);
            request.SetRouteData(routeData.Object);

            DefaultHttpControllerSelector selector = new DefaultHttpControllerSelector(configuration);

            // Act
            var controller = selector.SelectController(request);

            // Assert
            Assert.Equal(controllerType, controller.ControllerType);
        }

        // We ignore attribute routes when RouteData.Route is null
        // See: https://aspnetwebstack.codeplex.com/workitem/1996
        [Fact]
        public void SelectController_WhenRouteData_RouteIsNull_DoesNotFindController()
        {
            // Arrange
            HttpConfiguration configuration = new HttpConfiguration();
            Mock<IHttpControllerTypeResolver> controllerTypeResolver = new Mock<IHttpControllerTypeResolver>();
            configuration.Services.Replace(typeof(IHttpControllerTypeResolver), controllerTypeResolver.Object);

            controllerTypeResolver
                .Setup(c => c.GetControllerTypes(It.IsAny<IAssembliesResolver>()))
                .Returns(new Collection<Type> {}); // No controllers here

            HttpRequestMessage request = new HttpRequestMessage();

            var routeValues = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                { "controller", "Sample" }
            };

            var routeData = new Mock<IHttpRouteData>();
            routeData.SetupGet(rd => rd.Values).Returns(routeValues);
            routeData.SetupGet(rd => rd.Route).Returns((IHttpRoute)null);
            request.SetRouteData(routeData.Object);

            DefaultHttpControllerSelector selector = new DefaultHttpControllerSelector(configuration);

            // Act
            var ex = Assert.Throws<HttpResponseException>(
                () => selector.SelectController(request));

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, ex.Response.StatusCode);
        }

        [Fact]
        public async Task SelectController_Throws_NotFound_NoMatchingControllerType()
        {
            HttpConfiguration configuration = new HttpConfiguration();
            Mock<IHttpControllerTypeResolver> controllerTypeResolver = new Mock<IHttpControllerTypeResolver>();
            configuration.Services.Replace(typeof(IHttpControllerTypeResolver), controllerTypeResolver.Object);
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            controllerTypeResolver
                .Setup(c => c.GetControllerTypes(It.IsAny<IAssembliesResolver>()))
                .Returns(new Collection<Type>());

            HttpRequestMessage request = new HttpRequestMessage();
            IHttpRouteData routeData1 = GetRouteData();
            routeData1.Values["controller"] = "Sample";
            request.SetRouteData(routeData1);
            request.SetConfiguration(configuration);

            DefaultHttpControllerSelector selector = new DefaultHttpControllerSelector(configuration);

            // Act
            var ex = Assert.Throws<HttpResponseException>(
                () => selector.SelectController(request));

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, ex.Response.StatusCode);
            var result = await ex.Response.Content.ReadAsAsync<HttpError>();
            string response = result["MessageDetail"] as string;
            Assert.Equal("No type was found that matches the controller named 'Sample'.", response);
        }

        [Fact]
        public void SelectController_Throws_DuplicateController()
        {
            HttpConfiguration configuration = new HttpConfiguration();
            Mock<IHttpControllerTypeResolver> controllerTypeResolver = new Mock<IHttpControllerTypeResolver>();
            configuration.Services.Replace(typeof(IHttpControllerTypeResolver), controllerTypeResolver.Object);
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            controllerTypeResolver
                .Setup(c => c.GetControllerTypes(It.IsAny<IAssembliesResolver>()))
                .Returns(new Collection<Type> { GetMockControllerType("Sample"), GetMockControllerType("SampLe"), GetMockControllerType("SAmpLE") });

            HttpRequestMessage request = new HttpRequestMessage();
            IHttpRouteData routeData1 = GetRouteData();
            routeData1.Values["controller"] = "Sample";
            request.SetRouteData(routeData1);
            request.SetConfiguration(configuration);

            DefaultHttpControllerSelector selector = new DefaultHttpControllerSelector(configuration);

            // Act
            var ex = Assert.Throws<InvalidOperationException>(
                () => selector.SelectController(request));

            // Assert
            string message = ex.Message;
            Assert.Contains(
                "Multiple types were found that match the controller named 'Sample'. This can happen if the route that services this request ('') found multiple controllers defined with the same name but differing namespaces, which is not supported.\r\n\r\nThe request for 'Sample' has found the following matching controllers:",
                message);

            var duplicateControllers = message.Split(':')[1].Split('\n').Select(str => str.Trim());
            Assert.Contains("FullSampleController", duplicateControllers);
            Assert.Contains("FullSampLeController", duplicateControllers);
            Assert.Contains("FullSAmpLEController", duplicateControllers);
        }

        private static IHttpRouteData GetRouteData()
        {
            HttpRoute route = new HttpRoute();
            HttpRouteData routeData = new HttpRouteData(route);

            return routeData;
        }

        private static Type GetMockControllerType(string controllerName)
        {
            Mock<Type> mockType = new Mock<Type>();

            mockType.Setup(t => t.Name).Returns(controllerName + "Controller");
            mockType.Setup(t => t.FullName).Returns("Full" + controllerName + "Controller");
            return mockType.Object;
        }
    }
}
