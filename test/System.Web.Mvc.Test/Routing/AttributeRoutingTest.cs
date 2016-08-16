// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Mvc.Routing;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Routing
{
    public class AttributeRoutingTest
    {
        internal const string ResultKey = "MS_Test_Result";

        [Theory]
        [InlineData(typeof(DerivedController), "~/BaseMethodWithRoute", "BaseMethodWithRoute")]
        [InlineData(typeof(DerivedController), "~/BaseVirtualMethodWithRoute", "BaseVirtualMethodWithRoute")]
        [InlineData(typeof(DerivedController), "~/BaseVirtualMethodWithRouteToBeOverridenWithRoute", "BaseVirtualMethodWithRouteToBeOverridenWithRoute")]
        [InlineData(typeof(DerivedController), "~/DerivedMethodWithRoute", "DerivedMethodWithRoute")]
        [InlineData(typeof(DerivedController), "~/BaseVirtualMethodToBeOverridenWithRoute_Derived", "BaseVirtualMethodToBeOverridenWithRoute_Derived")]
        [InlineData(typeof(DerivedController), "~/BaseVirtualMethodWithRouteToBeOverridenWithRoute_Derived", "BaseVirtualMethodWithRouteToBeOverridenWithRoute_Derived")]
        [InlineData(typeof(DerivedController), "~/BaseMethodWithRouteWithName", "BaseMethodWithRouteWithName")]
        [InlineData(typeof(DerivedPrefixController), "~/BaseMethodWithRoute", "BaseMethodWithRoute")]
        [InlineData(typeof(DerivedPrefixController), "~/BaseVirtualMethodWithRoute", "BaseVirtualMethodWithRoute")]
        [InlineData(typeof(DerivedPrefixController), "~/BaseVirtualMethodWithRouteToBeOverridenWithRoute", "BaseVirtualMethodWithRouteToBeOverridenWithRoute")]
        [InlineData(typeof(DerivedPrefixController), "~/derived/DerivedMethodWithRoute", "DerivedMethodWithRoute")]
        [InlineData(typeof(DerivedPrefixController), "~/derived/BaseVirtualMethodToBeOverridenWithRoute_Derived", "BaseVirtualMethodToBeOverridenWithRoute_Derived")]
        [InlineData(typeof(DerivedPrefixController), "~/derived/BaseVirtualMethodWithRouteToBeOverridenWithRoute_Derived", "BaseVirtualMethodWithRouteToBeOverridenWithRoute_Derived")]
        [InlineData(typeof(DerivedFromBasePrefixController), "~/base/BaseMethodWithRoute", "BaseMethodWithRoute")]
        [InlineData(typeof(DerivedFromBasePrefixController), "~/base/BaseVirtualMethodWithRoute", "BaseVirtualMethodWithRoute")]
        [InlineData(typeof(DerivedFromBasePrefixController), "~/base/BaseVirtualMethodWithRouteToBeOverridenWithRoute", "BaseVirtualMethodWithRouteToBeOverridenWithRoute")]
        [InlineData(typeof(DerivedFromBasePrefixController), "~/DerivedMethodWithRoute", "DerivedMethodWithRoute")]
        [InlineData(typeof(DerivedFromBasePrefixController), "~/BaseVirtualMethodToBeOverridenWithRoute_Derived", "BaseVirtualMethodToBeOverridenWithRoute_Derived")]
        [InlineData(typeof(DerivedFromBasePrefixController), "~/BaseVirtualMethodWithRouteToBeOverridenWithRoute_Derived", "BaseVirtualMethodWithRouteToBeOverridenWithRoute_Derived")]
        [InlineData(typeof(DerivedWithPrefixFromBasePrefixController), "~/base/BaseMethodWithRoute", "BaseMethodWithRoute")]
        [InlineData(typeof(DerivedWithPrefixFromBasePrefixController), "~/base/BaseVirtualMethodWithRoute", "BaseVirtualMethodWithRoute")]
        [InlineData(typeof(DerivedWithPrefixFromBasePrefixController), "~/base/BaseVirtualMethodWithRouteToBeOverridenWithRoute", "BaseVirtualMethodWithRouteToBeOverridenWithRoute")]
        [InlineData(typeof(DerivedWithPrefixFromBasePrefixController), "~/derived/DerivedMethodWithRoute", "DerivedMethodWithRoute")]
        [InlineData(typeof(DerivedWithPrefixFromBasePrefixController), "~/derived/BaseVirtualMethodToBeOverridenWithRoute_Derived", "BaseVirtualMethodToBeOverridenWithRoute_Derived")]
        [InlineData(typeof(DerivedWithPrefixFromBasePrefixController), "~/derived/BaseVirtualMethodWithRouteToBeOverridenWithRoute_Derived", "BaseVirtualMethodWithRouteToBeOverridenWithRoute_Derived")]
        [InlineData(typeof(DerivedFromBaseRouteController), "~/BaseMethodWithRoute", "BaseMethodWithRoute")]
        [InlineData(typeof(DerivedFromBaseRouteController), "~/base/BaseVirtualMethodToBeOverridenWithRoute", "BaseVirtualMethodToBeOverridenWithRoute")]
        [InlineData(typeof(DerivedFromBaseRouteController), "~/BaseVirtualMethodWithRoute", "BaseVirtualMethodWithRoute")]
        [InlineData(typeof(DerivedFromBaseRouteController), "~/BaseVirtualMethodWithRouteToBeOverridenWithRoute", "BaseVirtualMethodWithRouteToBeOverridenWithRoute")]
        [InlineData(typeof(DerivedFromBaseRouteController), "~/DerivedMethodWithRoute", "DerivedMethodWithRoute")]
        [InlineData(typeof(DerivedFromBaseRouteController), "~/BaseVirtualMethodToBeOverridenWithRoute_Derived", "BaseVirtualMethodToBeOverridenWithRoute_Derived")]
        [InlineData(typeof(DerivedFromBaseRouteWithRouteController), "~/derivedwithroute/BaseMethodWithRoute", "BaseMethodWithRoute")]
        [InlineData(typeof(DerivedFromBaseRouteWithRouteController), "~/derivedwithroute/BaseVirtualMethodToBeOverridenWithRoute", "BaseVirtualMethodToBeOverridenWithRoute")]
        [InlineData(typeof(DerivedFromBaseRouteWithRouteController), "~/derivedwithroute/BaseVirtualMethodWithRoute", "BaseVirtualMethodWithRoute")]
        [InlineData(typeof(DerivedFromBaseRouteWithRouteController), "~/derivedwithroute/BaseVirtualMethodWithRouteToBeOverridenWithRoute", "BaseVirtualMethodWithRouteToBeOverridenWithRoute")]
        public void AttributeRouting_WithInheritance_MethodOverrides(Type derivedController, string path, string expectedAction)
        {
            // Arrange
            var controllerTypes = new[] { derivedController, derivedController.BaseType };
            var routes = new RouteCollection();
            AttributeRoutingMapper.MapAttributeRoutes(routes, controllerTypes);

            HttpContextBase context = GetContext(path);
            RouteData routeData = routes.GetRouteData(context);
            RequestContext requestContext = new RequestContext(context, routeData);
            MvcHandler handler = new MvcHandler(requestContext);
            handler.ControllerBuilder.SetControllerFactory(GetControllerFactory(controllerTypes));

            // Act
            handler.ProcessRequest(context);

            // Assert
            ContentResult result = Assert.IsType<ContentResult>(context.Items[ResultKey]);
            Assert.Equal(expectedAction, result.Content);
        }

        [Theory]
        [InlineData(typeof(DerivedController), "~/BaseMethodWithRouteTypo")]
        [InlineData(typeof(DerivedController), "~/derived/BaseMethodWithRoute")]
        [InlineData(typeof(DerivedPrefixController), "~/derived/BaseVirtualMethodWithRoute")]
        [InlineData(typeof(DerivedWithPrefixFromBasePrefixController), "~/derived/BaseVirtualMethodWithRoute")]
        public void AttributeRouting_WithInheritance_InvalidPaths(Type derivedController, string path)
        {
            // Arrange
            var controllerTypes = new[] { derivedController, derivedController.BaseType };
            var routes = new RouteCollection();
            AttributeRoutingMapper.MapAttributeRoutes(routes, controllerTypes);
            HttpContextBase context = GetContext(path);

            // Act
            RouteData routeData = routes.GetRouteData(context);

            // Assert
            Assert.Null(routeData);
        }

        [Theory]
        [InlineData(typeof(MethodOverloadsController), "~/Get1", "Get1")]
        [InlineData(typeof(MethodOverloadsController), "~/Get2?id=42", "Get2_42")]
        public void AttributeRouting_MethodOverloads_WithDifferentActionNames(Type controllerType, string path, string expectedAction)
        {
            // Arrange
            var controllerTypes = new[] { controllerType };
            var routes = new RouteCollection();
            AttributeRoutingMapper.MapAttributeRoutes(routes, controllerTypes);

            HttpContextBase context = GetContext(path);
            RouteData routeData = routes.GetRouteData(context);
            RequestContext requestContext = new RequestContext(context, routeData);
            MvcHandler handler = new MvcHandler(requestContext);
            handler.ControllerBuilder.SetControllerFactory(GetControllerFactory(controllerTypes));

            // Act
            handler.ProcessRequest(context);

            // Assert
            ContentResult result = Assert.IsType<ContentResult>(context.Items[ResultKey]);
            Assert.Equal(expectedAction, result.Content);
        }

        [Theory]
        [InlineData(typeof(MethodOverloadsController), "~/GetAmbiguous?id=42")]
        public void AttributeRouting_AmbiguousActions_ThrowsAmbiguousException(Type controllerType, string path)
        {
            // Arrange
            var controllerTypes = new[] { controllerType };
            var routes = new RouteCollection();
            AttributeRoutingMapper.MapAttributeRoutes(routes, controllerTypes);

            HttpContextBase context = GetContext(path);
            RouteData routeData = routes.GetRouteData(context);
            RequestContext requestContext = new RequestContext(context, routeData);
            MvcHandler handler = new MvcHandler(requestContext);
            handler.ControllerBuilder.SetControllerFactory(GetControllerFactory(controllerTypes));

            Assert.Throws<AmbiguousMatchException>(() => handler.ProcessRequest(context));
        }

        [Theory]
        [InlineData(typeof(MixedRoutingController), "~/GetWithRoute", "GetWithRoute")]
        [InlineData(typeof(MixedRoutingController), "~/standard/GetWithoutRoute", "GetWithoutRoute")]
        [InlineData(typeof(MixedRoutingController), "~/standard/GetWithRoute", null)]
        [InlineData(typeof(MixedRoutingWithPrefixController), "~/prefix/GetWithRoute", "GetWithRoute")]
        [InlineData(typeof(MixedRoutingWithPrefixController), "~/standard/GetWithoutRoute", "GetWithoutRoute")]
        [InlineData(typeof(MixedRoutingWithPrefixController), "~/standard/GetWithRoute", null)]
        [InlineData(typeof(MixedRoutingWithRouteonController), "~/GetWithRoute", "GetWithRoute")]
        [InlineData(typeof(MixedRoutingWithRouteonController), "~/GetWithoutRoute", "GetWithoutRoute")]
        [InlineData(typeof(MixedRoutingWithRouteonController), "~/standard/GetWithoutRoute", null)]
        [InlineData(typeof(MixedRoutingWithRouteonController), "~/standard/GetWithRoute", null)]
        public void AttributeRouting_MixedWithGeneralRouting(Type controllerType, string path, string expectedAction)
        {
            // Arrange
            var controllerTypes = new[] { controllerType };
            var routes = new RouteCollection();
            object defaults = new { controller = controllerType.Name.Substring(0, controllerType.Name.Length - 10) };
            routes.Add(new Route("standard/{action}", new RouteValueDictionary(defaults), null));
            AttributeRoutingMapper.MapAttributeRoutes(routes, controllerTypes);

            HttpContextBase context = GetContext(path);
            RouteData routeData = routes.GetRouteData(context);
            RequestContext requestContext = new RequestContext(context, routeData);
            MvcHandler handler = new MvcHandler(requestContext);
            handler.ControllerBuilder.SetControllerFactory(GetControllerFactory(controllerTypes));

            if (expectedAction == null)
            {
                // Act & Assert
                Assert.Throws<HttpException>(() => handler.ProcessRequest(context));
            }
            else
            {
                // Act
                handler.ProcessRequest(context);

                // Assert
                ContentResult result = Assert.IsType<ContentResult>(context.Items[ResultKey]);
                Assert.Equal(expectedAction, result.Content);
            }
        }

        [Theory]
        [InlineData(typeof(ActionMethodSelectorsController), "~/Action1", "Action1(int)")]
        [InlineData(typeof(ActionMethodSelectorsController), "~/DoesntRun", null)]
        public void AttributeRouting_WithActionMethodSelectors(Type controllerType, string path, string expectedAction)
        {
            // Arrange
            var controllerTypes = new[] { controllerType };
            var routes = new RouteCollection();
            AttributeRoutingMapper.MapAttributeRoutes(routes, controllerTypes);

            HttpContextBase context = GetContext(path);
            RouteData routeData = routes.GetRouteData(context);
            RequestContext requestContext = new RequestContext(context, routeData);
            MvcHandler handler = new MvcHandler(requestContext);
            handler.ControllerBuilder.SetControllerFactory(GetControllerFactory(controllerTypes));

            if (expectedAction == null)
            {
                // Act & Assert
                Assert.Throws<HttpException>(() => handler.ProcessRequest(context));
            }
            else
            {
                // Act
                handler.ProcessRequest(context);

                // Assert
                ContentResult result = Assert.IsType<ContentResult>(context.Items[ResultKey]);
                Assert.Equal(expectedAction, result.Content);
            }
        }

        [Theory]
        [InlineData(typeof(ActionNameSelectorsController), "~/SpecialName", "Action2()")]
        [InlineData(typeof(ActionNameSelectorsController), "~/AnotherSpecialName", "Action3()")]
        public void AttributeRouting_WithActionNameSelectors(Type controllerType, string path, string expectedAction)
        {
            // Arrange
            var controllerTypes = new[] { controllerType };
            var routes = new RouteCollection();
            AttributeRoutingMapper.MapAttributeRoutes(routes, controllerTypes);

            HttpContextBase context = GetContext(path);
            RouteData routeData = routes.GetRouteData(context);
            RequestContext requestContext = new RequestContext(context, routeData);
            MvcHandler handler = new MvcHandler(requestContext);
            handler.ControllerBuilder.SetControllerFactory(GetControllerFactory(controllerTypes));

            if (expectedAction == null)
            {
                // Act & Assert
                Assert.Throws<HttpException>(() => handler.ProcessRequest(context));
            }
            else
            {
                // Act
                handler.ProcessRequest(context);

                // Assert
                ContentResult result = Assert.IsType<ContentResult>(context.Items[ResultKey]);
                Assert.Equal(expectedAction, result.Content);
            }
        }

        [Fact]
        public void AttributeRouting_OptionalParametersGetRemoved()
        {
            // Arrange
            var controllerTypes = new[] { typeof(OptionalParameterController) };
            var routes = new RouteCollection();
            AttributeRoutingMapper.MapAttributeRoutes(routes, controllerTypes);

            HttpContextBase context = GetContext("~/Create");
            RouteData routeData = routes.GetRouteData(context);
            RequestContext requestContext = new RequestContext(context, routeData);
            MvcHandler handler = new MvcHandler(requestContext);
            handler.ControllerBuilder.SetControllerFactory(GetControllerFactory(controllerTypes));

            // Act
            handler.ProcessRequest(context);

            // Assert
            ContentResult result = Assert.IsType<ContentResult>(context.Items[ResultKey]);
            Assert.Equal("Create()", result.Content);

            // The request context should be updated to to contain the routedata of the direct route
            Assert.Equal("{action}/{id}", ((Route)requestContext.RouteData.Route).Url);
            Assert.Null(requestContext.RouteData.Values["id"]);
        }

        [Theory]
        [InlineData("~/Home1/Index", "Home1.Index()")]
        [InlineData("~/Home2/Index", "Home2.Index()")]
        public void AttributeRouting_WithSameControllerName(string path, string expectedAction)
        {
            // Arrange
            var controllerTypes = new[]
            {
                typeof(ControllersWithTheSameName.NS1.HomeController),
                typeof(ControllersWithTheSameName.NS2.HomeController),
            };

            var routes = new RouteCollection();
            AttributeRoutingMapper.MapAttributeRoutes(routes, controllerTypes);

            HttpContextBase context = GetContext(path);
            RouteData routeData = routes.GetRouteData(context);
            RequestContext requestContext = new RequestContext(context, routeData);
            MvcHandler handler = new MvcHandler(requestContext);
            handler.ControllerBuilder.SetControllerFactory(GetControllerFactory(controllerTypes));

            // Act
            handler.ProcessRequest(context);

            // Assert
            ContentResult result = Assert.IsType<ContentResult>(context.Items[ResultKey]);
            Assert.Equal(expectedAction, result.Content);
        }

        [Theory]
        [InlineData("~/NS1Home/Introduction", "Home.Index()")]
        [InlineData("~/NS2Account/PeopleList", "Account.Index()")]
        [InlineData("~/Default/Unknown", "Default.Index()")]
        public void AttributeRouting_WithCustomizedRoutePrefixAttribute(string path, string expectedAction)
        {
            // Arrange
            var controllerTypes = new[]
            {
                typeof(ControllersWithCustomizedRoutePrefixAttribute.NS1.HomeController),
                typeof(ControllersWithCustomizedRoutePrefixAttribute.NS2.AccountController),
                typeof(ControllersWithCustomizedRoutePrefixAttribute.NS3.OtherController),
            };

            var routes = new RouteCollection();
            AttributeRoutingMapper.MapAttributeRoutes(routes, controllerTypes);

            HttpContextBase context = GetContext(path);
            RouteData routeData = routes.GetRouteData(context);
            RequestContext requestContext = new RequestContext(context, routeData);
            MvcHandler handler = new MvcHandler(requestContext);
            handler.ControllerBuilder.SetControllerFactory(GetControllerFactory(controllerTypes));

            // Act
            handler.ProcessRequest(context);

            // Assert
            ContentResult result = Assert.IsType<ContentResult>(context.Items[ResultKey]);
            Assert.Equal(expectedAction, result.Content);
        }

        [Fact]
        public void AttributeRouting_WithMultipleCustomizedRoutePrefixAttribute_ThrowsInvalidOperationException()
        {
            // Arrange
            var controllerTypes = new[]
            {
                typeof(ControllersWithCustomizedRoutePrefixAttribute.Invalid.HomeController)
            };

            var routes = new RouteCollection();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => AttributeRoutingMapper.MapAttributeRoutes(routes, controllerTypes),
                "Only one route prefix attribute is supported. Remove extra attributes from the controller of type 'System.Web.Routing.ControllersWithCustomizedRoutePrefixAttribute.Invalid.HomeController'.");
        }

        [Fact]
        public void AttributeRouting_WithNullPrefix_ThrowsInvalidOperationException()
        {
            // Arrange
            var controllerTypes = new[]
            {
                typeof(ControllersWithCustomizedRoutePrefixAttribute.Invalid.AccountController)
            };

            var routes = new RouteCollection();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => AttributeRoutingMapper.MapAttributeRoutes(routes, controllerTypes),
                "The property 'prefix' from route prefix attribute on controller of type 'System.Web.Routing.ControllersWithCustomizedRoutePrefixAttribute.Invalid.AccountController' cannot be null.");
        }

        private IControllerFactory GetControllerFactory(Type[] controllerTypes)
        {
            return new DefaultControllerFactory
            {
                BuildManager = new MockBuildManager(controllerTypes),
                ControllerTypeCache = new ControllerTypeCache()
            };
        }

        private HttpContextBase GetContext(string url)
        {
            Uri uri = new Uri("http://localhost/" + url.Substring(2));

            NameValueCollection queryString = HttpUtility.ParseQueryString(uri.Query);

            Mock<UnvalidatedRequestValuesBase> unvalidatedRequest = new Mock<UnvalidatedRequestValuesBase>();
            unvalidatedRequest.Setup(u => u.Form).Returns(new NameValueCollection());
            unvalidatedRequest.Setup(u => u.QueryString).Returns(queryString);

            // mock HttpRequest
            Mock<HttpRequestBase> requestMock = new Mock<HttpRequestBase>();
            requestMock.Setup(request => request.Url).Returns(uri);
            requestMock.Setup(request => request.HttpMethod).Returns("GET");
            requestMock.Setup(request => request.Form).Returns(new NameValueCollection());
            requestMock.Setup(request => request.ServerVariables).Returns(new NameValueCollection());
            requestMock.Setup(request => request.AppRelativeCurrentExecutionFilePath).Returns("~" + uri.AbsolutePath);
            requestMock.Setup(request => request.Unvalidated).Returns(unvalidatedRequest.Object);
            requestMock.Setup(request => request.ContentType).Returns("");
            requestMock.Setup(request => request.QueryString).Returns(queryString);
            requestMock.Setup(request => request.Files).Returns(new Mock<HttpFileCollectionBase>().Object);

            // mock HttpResponse
            Mock<HttpResponseBase> responseBase = new Mock<HttpResponseBase>();

            // mock HttpContext
            Mock<HttpContextBase> contextMock = new Mock<HttpContextBase>();
            contextMock.Setup(context => context.Items).Returns(new Hashtable());
            contextMock.Setup(context => context.Request).Returns(requestMock.Object);
            contextMock.Setup(context => context.Response).Returns(responseBase.Object);
            return contextMock.Object;
        }

        private class MockBuildManager : IBuildManager
        {
            private Assembly[] _assemblies;

            public MockBuildManager(Type[] types)
            {
                MockAssembly assembly = new MockAssembly(types);
                _assemblies = new Assembly[] { assembly };
            }

            public bool FileExists(string virtualPath)
            {
                throw new NotImplementedException();
            }

            public Type GetCompiledType(string virtualPath)
            {
                throw new NotImplementedException();
            }

            public ICollection GetReferencedAssemblies()
            {
                return _assemblies;
            }

            public Stream ReadCachedFile(string fileName)
            {
                return null;
            }

            public Stream CreateCachedFile(string fileName)
            {
                return null;
            }

            private sealed class MockAssembly : Assembly
            {
                Type[] _types;

                public MockAssembly(params Type[] types)
                {
                    _types = types;
                }

                public override Type[] GetTypes()
                {
                    return _types;
                }
            }
        }
    }

    // stores the response from the action on the request context for later inspection.
    public class ResponseStoringController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // use the per-instance cache to avoid timing issues.
            filterContext.ActionDescriptor.DispatcherCache = new ActionMethodDispatcherCache();
            base.OnActionExecuting(filterContext);
        }

        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            filterContext.RequestContext.HttpContext.Items[AttributeRoutingTest.ResultKey] = filterContext.Result;
        }
    }

    #region Inheritance Controllers

    public class BaseController : ResponseStoringController
    {
        [Route("BaseMethodWithRoute")]
        public string BaseMethodWithRoute()
        {
            return "BaseMethodWithRoute";
        }

        [Route("BaseMethodWithRouteWithName", Name = "BaseMethodWithRouteWithName")]
        public string BaseMethodWithRouteWithName()
        {
            return "BaseMethodWithRouteWithName";
        }

        public virtual string BaseVirtualMethodToBeOverridenWithRoute()
        {
            return "BaseVirtualMethodToBeOverridenWithRoute";
        }

        [Route("BaseVirtualMethodWithRoute")]
        public virtual string BaseVirtualMethodWithRoute()
        {
            return "BaseVirtualMethodWithRoute";
        }

        [Route("BaseVirtualMethodWithRouteToBeOverridenWithRoute")]
        public virtual string BaseVirtualMethodWithRouteToBeOverridenWithRoute()
        {
            return "BaseVirtualMethodWithRouteToBeOverridenWithRoute";
        }
    }

    [RoutePrefix("base")]
    public class BasePrefixController : ResponseStoringController
    {
        [Route("BaseMethodWithRoute")]
        public string BaseMethodWithRoute()
        {
            return "BaseMethodWithRoute";
        }

        public virtual string BaseVirtualMethodToBeOverridenWithRoute()
        {
            return "BaseVirtualMethodToBeOverridenWithRoute";
        }

        [Route("BaseVirtualMethodWithRoute")]
        public virtual string BaseVirtualMethodWithRoute()
        {
            return "BaseVirtualMethodWithRoute";
        }

        [Route("BaseVirtualMethodWithRouteToBeOverridenWithRoute")]
        public virtual string BaseVirtualMethodWithRouteToBeOverridenWithRoute()
        {
            return "BaseVirtualMethodWithRouteToBeOverridenWithRoute";
        }
    }

    [Route("base/{action}")]
    public class BaseRouteController : ResponseStoringController
    {
        [Route("BaseMethodWithRoute")]
        public string BaseMethodWithRoute()
        {
            return "BaseMethodWithRoute";
        }

        public virtual string BaseVirtualMethodToBeOverridenWithRoute()
        {
            return "BaseVirtualMethodToBeOverridenWithRoute";
        }

        [Route("BaseVirtualMethodWithRoute")]
        public virtual string BaseVirtualMethodWithRoute()
        {
            return "BaseVirtualMethodWithRoute";
        }

        [Route("BaseVirtualMethodWithRouteToBeOverridenWithRoute")]
        public virtual string BaseVirtualMethodWithRouteToBeOverridenWithRoute()
        {
            return "BaseVirtualMethodWithRouteToBeOverridenWithRoute";
        }
    }

    public class DerivedController : BaseController
    {
        [Route("DerivedMethodWithRoute")]
        public string DerivedMethodWithRoute()
        {
            return "DerivedMethodWithRoute";
        }

        public override string BaseVirtualMethodWithRoute()
        {
            return "BaseVirtualMethodWithRoute_Derived";
        }

        [Route("BaseVirtualMethodToBeOverridenWithRoute_Derived")]
        public override string BaseVirtualMethodToBeOverridenWithRoute()
        {
            return "BaseVirtualMethodToBeOverridenWithRoute_Derived";
        }

        [Route("BaseVirtualMethodWithRouteToBeOverridenWithRoute_Derived")]
        public override string BaseVirtualMethodWithRouteToBeOverridenWithRoute()
        {
            return "BaseVirtualMethodWithRouteToBeOverridenWithRoute_Derived";
        }
    }

    public class DerivedFromBasePrefixController : BasePrefixController
    {
        [Route("DerivedMethodWithRoute")]
        public string DerivedMethodWithRoute()
        {
            return "DerivedMethodWithRoute";
        }

        public override string BaseVirtualMethodWithRoute()
        {
            return "BaseVirtualMethodWithRoute_Derived";
        }

        [Route("BaseVirtualMethodToBeOverridenWithRoute_Derived")]
        public override string BaseVirtualMethodToBeOverridenWithRoute()
        {
            return "BaseVirtualMethodToBeOverridenWithRoute_Derived";
        }

        [Route("BaseVirtualMethodWithRouteToBeOverridenWithRoute_Derived")]
        public override string BaseVirtualMethodWithRouteToBeOverridenWithRoute()
        {
            return "BaseVirtualMethodWithRouteToBeOverridenWithRoute_Derived";
        }
    }

    [RoutePrefix("derived")]
    public class DerivedPrefixController : BaseController
    {
        [Route("DerivedMethodWithRoute")]
        public string DerivedMethodWithRoute()
        {
            return "DerivedMethodWithRoute";
        }

        public override string BaseVirtualMethodWithRoute()
        {
            return "BaseVirtualMethodWithRoute_Derived";
        }

        [Route("BaseVirtualMethodToBeOverridenWithRoute_Derived")]
        public override string BaseVirtualMethodToBeOverridenWithRoute()
        {
            return "BaseVirtualMethodToBeOverridenWithRoute_Derived";
        }

        [Route("BaseVirtualMethodWithRouteToBeOverridenWithRoute_Derived")]
        public override string BaseVirtualMethodWithRouteToBeOverridenWithRoute()
        {
            return "BaseVirtualMethodWithRouteToBeOverridenWithRoute_Derived";
        }
    }

    [RoutePrefix("derived")]
    public class DerivedWithPrefixFromBasePrefixController : BasePrefixController
    {
        [Route("DerivedMethodWithRoute")]
        public string DerivedMethodWithRoute()
        {
            return "DerivedMethodWithRoute";
        }

        public override string BaseVirtualMethodWithRoute()
        {
            return "BaseVirtualMethodWithRoute_Derived";
        }

        [Route("BaseVirtualMethodToBeOverridenWithRoute_Derived")]
        public override string BaseVirtualMethodToBeOverridenWithRoute()
        {
            return "BaseVirtualMethodToBeOverridenWithRoute_Derived";
        }

        [Route("BaseVirtualMethodWithRouteToBeOverridenWithRoute_Derived")]
        public override string BaseVirtualMethodWithRouteToBeOverridenWithRoute()
        {
            return "BaseVirtualMethodWithRouteToBeOverridenWithRoute_Derived";
        }
    }

    public class DerivedFromBaseRouteController : BaseRouteController
    {
        [Route("DerivedMethodWithRoute")]
        public string DerivedMethodWithRoute()
        {
            return "DerivedMethodWithRoute";
        }

        public override string BaseVirtualMethodWithRoute()
        {
            return "BaseVirtualMethodWithRoute_Derived";
        }

        [Route("BaseVirtualMethodToBeOverridenWithRoute_Derived")]
        public override string BaseVirtualMethodToBeOverridenWithRoute()
        {
            return "BaseVirtualMethodToBeOverridenWithRoute_Derived";
        }

        [Route("BaseVirtualMethodWithRouteToBeOverridenWithRoute_Derived")]
        public override string BaseVirtualMethodWithRouteToBeOverridenWithRoute()
        {
            return "BaseVirtualMethodWithRouteToBeOverridenWithRoute_Derived";
        }
    }

    [Route("derivedwithroute/{action}")]
    public class DerivedFromBaseRouteWithRouteController : BaseRouteController
    {
    }

    #endregion

    public class MethodOverloadsController : ResponseStoringController
    {
        [Route("Get1")]
        public string Get()
        {
            return "Get1";
        }

        [Route("Get2")]
        public string Get(int id)
        {
            return "Get2_" + id;
        }

        [Route("GetAmbiguous")]
        public string GetAmbiguous(int id)
        {
            return "GetAmbiguous_Int";
        }

        [Route("GetAmbiguous")]
        public string GetAmbiguous(string id)
        {
            return "GetAmbiguous_String";
        }
    }

    public class MixedRoutingController : ResponseStoringController
    {
        public string GetWithoutRoute()
        {
            return "GetWithoutRoute";
        }

        [Route("GetWithRoute")]
        public string GetWithRoute()
        {
            return "GetWithRoute";
        }
    }

    [RoutePrefix("prefix")]
    public class MixedRoutingWithPrefixController : ResponseStoringController
    {
        public string GetWithoutRoute()
        {
            return "GetWithoutRoute";
        }

        [Route("GetWithRoute")]
        public string GetWithRoute()
        {
            return "GetWithRoute";
        }
    }

    [Route("{action}")]
    public class MixedRoutingWithRouteonController : ResponseStoringController
    {
        public string GetWithoutRoute()
        {
            return "GetWithoutRoute";
        }

        [Route("GetWithRoute")]
        public string GetWithRoute()
        {
            return "GetWithRoute";
        }
    }

    [Route("{action}")]
    public class ActionMethodSelectorsController : ResponseStoringController
    {
        public string Action1()
        {
            return "Action1()";
        }

        // This is a 'better' action than Action1() because it has a selector
        [BoolActionMethodSelector(true)]
        public string Action1(int id = 0)
        {
            return "Action1(int)";
        }

        // All ActionMethodSelectors need to return true
        [BoolActionMethodSelector(true)]
        [BoolActionMethodSelector(false)]
        public string DoesntRun()
        {
            return "DoesntRun";
        }
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class BoolActionMethodSelectorAttribute : ActionMethodSelectorAttribute
    {
        public BoolActionMethodSelectorAttribute(bool value)
        {
            Value = value;
        }

        private bool Value
        {
            get;
            set;
        }

        public override bool IsValidForRequest(ControllerContext controllerContext, MethodInfo methodInfo)
        {
            return Value;
        }
    }

    [Route("{action}")]
    public class ActionNameSelectorsController : ResponseStoringController
    {
        [StringActionNameSelector("SpecialName")]
        public string Action1()
        {
            return "Action1()";
        }

        // This is 'better' because it has an action selector also
        [HttpGet]
        [StringActionNameSelector("SpecialName")]
        public string Action2()
        {
            return "Action2()";
        }

        [StringActionNameSelector("AnotherSpecialName")]
        public string Action3()
        {
            return "Action3()";
        }
    }

    /// <summary>
    /// A 'custom' implementation similar to ActionNameAttribute - using a custom attribute for tests
    /// because ActionNameAttribute is special cased by the ActionDescriptor class.
    /// </summary>
    public class StringActionNameSelectorAttribute : ActionNameSelectorAttribute
    {
        public StringActionNameSelectorAttribute(string actionName)
        {
            ActionName = actionName;
        }

        private string ActionName
        {
            get;
            set;
        }

        public override bool IsValidName(ControllerContext controllerContext, string actionName, MethodInfo methodInfo)
        {
            return String.Equals(actionName, ActionName, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Route("{action}/{id?}")]
    public class OptionalParameterController : ResponseStoringController
    {
        public string Create()
        {
            return "Create()";
        }
    }

    namespace ControllersWithTheSameName
    {
        namespace NS1
        {
            [Route("Home1/{action}")]
            public class HomeController : ResponseStoringController
            {
                public ActionResult Index()
                {
                    return Content("Home1.Index()");
                }
            }
        }

        namespace NS2
        {
            [Route("Home2/{action}")]
            public class HomeController : ResponseStoringController
            {
                public ActionResult Index()
                {
                    return Content("Home2.Index()");
                }
            }
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class CustomizedRoutePrefixAttribute : Attribute, IRoutePrefix
    {
        public CustomizedRoutePrefixAttribute(Type controller)
        {
            if (controller == null)
            {
                throw Error.ArgumentNull("prefix");
            }

            if (controller.Equals(typeof(ControllersWithCustomizedRoutePrefixAttribute.NS1.HomeController)))
            {
                Prefix = "NS1Home";
            }
            else if (controller.Equals(typeof(ControllersWithCustomizedRoutePrefixAttribute.NS2.AccountController)))
            {
                Prefix = "NS2Account";
            }
            else
            {
                Prefix = "Default";
            }
        }

        public string Prefix { get; private set; }
    }

    public class ExtendedRoutePrefixAttribute : RoutePrefixAttribute
    {
    }

    namespace ControllersWithCustomizedRoutePrefixAttribute
    {
        namespace NS1
        {
            [CustomizedRoutePrefix(typeof(HomeController))]
            public class HomeController : ResponseStoringController
            {
                [Route("Introduction")]
                public ActionResult Index()
                {
                    return Content("Home.Index()");
                }
            }
        }

        namespace NS2
        {
            [CustomizedRoutePrefix(typeof(AccountController))]
            public class AccountController : ResponseStoringController
            {
                [Route("PeopleList")]
                public ActionResult Index()
                {
                    return Content("Account.Index()");
                }
            }
        }

        namespace NS3
        {
            [CustomizedRoutePrefix(typeof(OtherController))]
            public class OtherController : ResponseStoringController
            {
                [Route("Unknown")]
                public ActionResult Index()
                {
                    return Content("Default.Index()");
                }
            }
        }

        namespace Invalid
        {
            [CustomizedRoutePrefix(typeof(HomeController))]
            [RoutePrefix("InvalidExtraPrefix")]
            public class HomeController : ResponseStoringController
            {
                [Route("Introduction")]
                public ActionResult Index()
                {
                    return Content("Home.Index()");
                }
            }

            [ExtendedRoutePrefixAttribute]
            public class AccountController : ResponseStoringController
            {
                [Route("AnyRoute")]
                public ActionResult Index()
                {
                    return Content("Account.Index()");
                }
            }
        }
    }
}