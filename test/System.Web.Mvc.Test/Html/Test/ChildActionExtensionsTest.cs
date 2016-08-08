// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Web.Mvc.Properties;
using System.Web.Mvc.Routing;
using System.Web.Routing;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Html.Test
{
    public class ChildActionExtensionsTest
    {
        private Mock<HtmlHelper> _htmlHelper;
        private Mock<HttpContextBase> _httpContext;
        private Mock<RouteBase> _route;
        private Mock<IViewDataContainer> _viewDataContainer;
         
        private RouteData _originalRouteData;
        private RouteCollection _routes;
        private ViewContext _viewContext;
        private VirtualPathData _virtualPathData;

        public ChildActionExtensionsTest()
        {
            _route = new Mock<RouteBase>();
            _route.Setup(r => r.GetVirtualPath(It.IsAny<RequestContext>(), It.IsAny<RouteValueDictionary>()))
                .Returns(() => _virtualPathData);

            _virtualPathData = new VirtualPathData(_route.Object, "~/VirtualPath");

            _routes = new RouteCollection();
            _routes.Add(_route.Object);

            _originalRouteData = new RouteData();

            string returnValue = "";
            _httpContext = new Mock<HttpContextBase>();
            _httpContext.Setup(hc => hc.Request.ApplicationPath).Returns("~");
            _httpContext.Setup(hc => hc.Response.ApplyAppPathModifier(It.IsAny<string>()))
                .Callback<string>(s => returnValue = s)
                .Returns(() => returnValue);
            _httpContext.Setup(hc => hc.Server.Execute(It.IsAny<IHttpHandler>(), It.IsAny<TextWriter>(), It.IsAny<bool>()));

            _viewContext = new ViewContext
            {
                RequestContext = new RequestContext(_httpContext.Object, _originalRouteData)
            };

            _viewDataContainer = new Mock<IViewDataContainer>();

            _htmlHelper = new Mock<HtmlHelper>(_viewContext, _viewDataContainer.Object, _routes);
        }

        [Fact]
        public void GuardClauses()
        {
            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => ChildActionExtensions.ActionHelper(null /* htmlHelper */, "abc", null /* controllerName */, null /* routeValues */, null /* textWriter */),
                "htmlHelper"
                );

            Assert.ThrowsArgumentNullOrEmpty(
                () => ChildActionExtensions.ActionHelper(_htmlHelper.Object, null /* actionName */, null /* controllerName */, null /* routeValues */, null /* textWriter */),
                "actionName"
                );

            Assert.ThrowsArgumentNullOrEmpty(
                () => ChildActionExtensions.ActionHelper(_htmlHelper.Object, String.Empty /* actionName */, null /* controllerName */, null /* routeValues */, null /* textWriter */),
                "actionName"
                );
        }

        [Fact]
        public void ServerExecuteCalledWithWrappedChildActionMvcHandler()
        {
            // Arrange
            IHttpHandler callbackHandler = null;
            TextWriter callbackTextWriter = null;
            bool callbackPreserveForm = false;
            _httpContext.Setup(hc => hc.Server.Execute(It.IsAny<IHttpHandler>(), It.IsAny<TextWriter>(), It.IsAny<bool>()))
                .Callback<IHttpHandler, TextWriter, bool>(
                    (handler, textWriter, preserveForm) =>
                    {
                        callbackHandler = handler;
                        callbackTextWriter = textWriter;
                        callbackPreserveForm = preserveForm;
                    });
            TextWriter stringWriter = new StringWriter();

            // Act
            ChildActionExtensions.ActionHelper(_htmlHelper.Object, "actionName", null /* controllerName */, null /* routeValues */, stringWriter);

            // Assert
            Assert.NotNull(callbackHandler);
            HttpHandlerUtil.ServerExecuteHttpHandlerWrapper wrapper = callbackHandler as HttpHandlerUtil.ServerExecuteHttpHandlerWrapper;
            Assert.NotNull(wrapper);
            Assert.NotNull(wrapper.InnerHandler);
            ChildActionExtensions.ChildActionMvcHandler childHandler = wrapper.InnerHandler as ChildActionExtensions.ChildActionMvcHandler;
            Assert.NotNull(childHandler);
            Assert.Same(stringWriter, callbackTextWriter);
            Assert.True(callbackPreserveForm);
        }

        [Fact]
        public void RouteDataTokensIncludesParentActionViewContext()
        {
            // Arrange
            MvcHandler mvcHandler = null;
            _httpContext.Setup(hc => hc.Server.Execute(It.IsAny<IHttpHandler>(), It.IsAny<TextWriter>(), It.IsAny<bool>()))
                .Callback<IHttpHandler, TextWriter, bool>((handler, _, __) => mvcHandler = (MvcHandler)((HttpHandlerUtil.ServerExecuteHttpHandlerWrapper)handler).InnerHandler);

            // Act
            ChildActionExtensions.ActionHelper(_htmlHelper.Object, "actionName", null /* controllerName */, null /* routeValues */, null /* textWriter */);

            // Assert
            Assert.Same(_viewContext, mvcHandler.RequestContext.RouteData.DataTokens[ControllerContext.ParentActionViewContextToken]);
        }

        [Fact]
        public void RouteValuesIncludeNewActionName()
        {
            // Arrange
            MvcHandler mvcHandler = null;
            _httpContext.Setup(hc => hc.Server.Execute(It.IsAny<IHttpHandler>(), It.IsAny<TextWriter>(), It.IsAny<bool>()))
                .Callback<IHttpHandler, TextWriter, bool>((handler, _, __) => mvcHandler = (MvcHandler)((HttpHandlerUtil.ServerExecuteHttpHandlerWrapper)handler).InnerHandler);

            // Act
            ChildActionExtensions.ActionHelper(_htmlHelper.Object, "actionName", null /* controllerName */, null /* routeValues */, null /* textWriter */);

            // Assert
            RouteData routeData = mvcHandler.RequestContext.RouteData;
            Assert.Equal("actionName", routeData.Values["action"]);
        }

        [Fact]
        public void RouteValuesIncludeOldControllerNameWhenControllerNameIsNullOrEmpty()
        {
            // Arrange
            _originalRouteData.Values["controller"] = "oldController";
            MvcHandler mvcHandler = null;
            _httpContext.Setup(hc => hc.Server.Execute(It.IsAny<IHttpHandler>(), It.IsAny<TextWriter>(), It.IsAny<bool>()))
                .Callback<IHttpHandler, TextWriter, bool>((handler, _, __) => mvcHandler = (MvcHandler)((HttpHandlerUtil.ServerExecuteHttpHandlerWrapper)handler).InnerHandler);

            // Act
            ChildActionExtensions.ActionHelper(_htmlHelper.Object, "actionName", null /* controllerName */, null /* routeValues */, null /* textWriter */);

            // Assert
            RouteData routeData = mvcHandler.RequestContext.RouteData;
            Assert.Equal("oldController", routeData.Values["controller"]);
        }

        [Fact]
        public void RouteValuesIncludeNewControllerNameWhenControllNameIsNotEmpty()
        {
            // Arrange
            _originalRouteData.Values["controller"] = "oldController";
            MvcHandler mvcHandler = null;
            _httpContext.Setup(hc => hc.Server.Execute(It.IsAny<IHttpHandler>(), It.IsAny<TextWriter>(), It.IsAny<bool>()))
                .Callback<IHttpHandler, TextWriter, bool>((handler, _, __) => mvcHandler = (MvcHandler)((HttpHandlerUtil.ServerExecuteHttpHandlerWrapper)handler).InnerHandler);

            // Act
            ChildActionExtensions.ActionHelper(_htmlHelper.Object, "actionName", "newController", null /* routeValues */, null /* textWriter */);

            // Assert
            RouteData routeData = mvcHandler.RequestContext.RouteData;
            Assert.Equal("newController", routeData.Values["controller"]);
        }

        [Fact]
        public void PassedRouteValuesOverrideParentRequestRouteValues()
        {
            // Arrange
            _originalRouteData.Values["name1"] = "value1";
            _originalRouteData.Values["name2"] = "value2";
            MvcHandler mvcHandler = null;
            _httpContext.Setup(hc => hc.Server.Execute(It.IsAny<IHttpHandler>(), It.IsAny<TextWriter>(), It.IsAny<bool>()))
                .Callback<IHttpHandler, TextWriter, bool>((handler, _, __) => mvcHandler = (MvcHandler)((HttpHandlerUtil.ServerExecuteHttpHandlerWrapper)handler).InnerHandler);

            // Act
            ChildActionExtensions.ActionHelper(_htmlHelper.Object, "actionName", null /* controllerName */, new RouteValueDictionary { { "name2", "newValue2" } }, null /* textWriter */);

            // Assert
            RouteData routeData = mvcHandler.RequestContext.RouteData;
            Assert.Equal("value1", routeData.Values["name1"]);
            Assert.Equal("newValue2", routeData.Values["name2"]);

            Assert.Equal("newValue2", (routeData.Values[ChildActionValueProvider.ChildActionValuesKey] as DictionaryValueProvider<object>).GetValue("name2").RawValue);
        }

        [Fact]
        public void NoChildActionValuesDictionaryCreatedIfNoRouteValuesPassed()
        {
            // Arrange
            MvcHandler mvcHandler = null;
            _httpContext.Setup(hc => hc.Server.Execute(It.IsAny<IHttpHandler>(), It.IsAny<TextWriter>(), It.IsAny<bool>()))
                .Callback<IHttpHandler, TextWriter, bool>((handler, _, __) => mvcHandler = (MvcHandler)((HttpHandlerUtil.ServerExecuteHttpHandlerWrapper)handler).InnerHandler);

            // Act
            ChildActionExtensions.ActionHelper(_htmlHelper.Object, "actionName", null /* controllerName */, null, null /* textWriter */);

            // Assert
            RouteData routeData = mvcHandler.RequestContext.RouteData;
            Assert.Null(routeData.Values[ChildActionValueProvider.ChildActionValuesKey]);
        }

        [Fact]
        public void RouteValuesDoesNotIncludeExplicitlyPassedAreaName()
        {
            // Arrange
            Route route = _routes.MapRoute("my-area", "my-area");
            route.DataTokens["area"] = "myArea";
            MvcHandler mvcHandler = null;
            _httpContext.Setup(hc => hc.Server.Execute(It.IsAny<IHttpHandler>(), It.IsAny<TextWriter>(), It.IsAny<bool>()))
                .Callback<IHttpHandler, TextWriter, bool>((handler, _, __) => mvcHandler = (MvcHandler)((HttpHandlerUtil.ServerExecuteHttpHandlerWrapper)handler).InnerHandler);

            // Act
            ChildActionExtensions.ActionHelper(_htmlHelper.Object, "actionName", null /* controllerName */, new RouteValueDictionary { { "area", "myArea" } }, null /* textWriter */);

            // Assert
            RouteData routeData = mvcHandler.RequestContext.RouteData;
            Assert.False(routeData.Values.ContainsKey("area"));
            Assert.Null((routeData.Values[ChildActionValueProvider.ChildActionValuesKey] as DictionaryValueProvider<object>).GetValue("area"));
        }

        [Fact]
        public void RouteValuesIncludeExplicitlyPassedAreaNameIfAreasNotInUse()
        {
            // Arrange
            Route route = _routes.MapRoute("my-area", "my-area");
            MvcHandler mvcHandler = null;
            _httpContext.Setup(hc => hc.Server.Execute(It.IsAny<IHttpHandler>(), It.IsAny<TextWriter>(), It.IsAny<bool>()))
                .Callback<IHttpHandler, TextWriter, bool>((handler, _, __) => mvcHandler = (MvcHandler)((HttpHandlerUtil.ServerExecuteHttpHandlerWrapper)handler).InnerHandler);

            // Act
            ChildActionExtensions.ActionHelper(_htmlHelper.Object, "actionName", null /* controllerName */, new RouteValueDictionary { { "area", "myArea" } }, null /* textWriter */);

            // Assert
            RouteData routeData = mvcHandler.RequestContext.RouteData;
            Assert.True(routeData.Values.ContainsKey("area"));
            Assert.Equal("myArea", (routeData.Values[ChildActionValueProvider.ChildActionValuesKey] as DictionaryValueProvider<object>).GetValue("area").RawValue);
        }

        [Fact]
        public void NoMatchingRouteThrows()
        {
            // Arrange
            _routes.Clear();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => ChildActionExtensions.ActionHelper(_htmlHelper.Object, "actionName", null /* controllerName */, null /* routeValues */, null /* textWriter */),
                MvcResources.Common_NoRouteMatched
                );
        }

        [Fact]
        public void ActionHelper_ChildAction_WithControllerDirectRoute()
        {
            // Arrange
            AttributeRoutingMapper.MapAttributeRoutes(_routes, new Type[] { typeof(DirectRouteController) });
            
            MvcHandler mvcHandler = null;
            _httpContext.Setup(hc => hc.Server.Execute(It.IsAny<IHttpHandler>(), It.IsAny<TextWriter>(), It.IsAny<bool>()))
                .Callback<IHttpHandler, TextWriter, bool>((handler, _, __) => mvcHandler = (MvcHandler)((HttpHandlerUtil.ServerExecuteHttpHandlerWrapper)handler).InnerHandler);

            // Act
            ChildActionExtensions.ActionHelper(_htmlHelper.Object, "Action", null /* controllerName */, null /* routeValues */, null /* textWriter */);

            // Assert
            RouteData routeData = mvcHandler.RequestContext.RouteData;
            Assert.Equal("Action", routeData.Values["action"]);
        }

        [Fact]
        public void ActionHelper_ChildAction_WithActionDirectRoute()
        {
            // Arrange
            AttributeRoutingMapper.MapAttributeRoutes(_routes, new Type[] { typeof(DirectRouteActionController) });

            MvcHandler mvcHandler = null;
            _httpContext.Setup(hc => hc.Server.Execute(It.IsAny<IHttpHandler>(), It.IsAny<TextWriter>(), It.IsAny<bool>()))
                .Callback<IHttpHandler, TextWriter, bool>((handler, _, __) => mvcHandler = (MvcHandler)((HttpHandlerUtil.ServerExecuteHttpHandlerWrapper)handler).InnerHandler);

            // Act
            ChildActionExtensions.ActionHelper(_htmlHelper.Object, "Action", null /* controllerName */, null /* routeValues */, null /* textWriter */);

            // Assert
            RouteData routeData = mvcHandler.RequestContext.RouteData;
            Assert.Equal("Action", routeData.Values["action"]);
        }

        [Route("controller/{action}")]
        private class DirectRouteController : Controller
        {
            public void Action()
            {
            }
        }
        
        private class DirectRouteActionController : Controller
        {
            [Route("controller/Action")]
            public void Action()
            {
            }
        }
    }
}
