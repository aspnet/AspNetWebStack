// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Web.Mvc.Async;
using System.Web.Profile;
using System.Web.Routing;
using System.Web.TestUtil;
using Microsoft.TestCommon;
using Microsoft.Web.UnitTestUtil;
using Moq;
using Moq.Protected;

namespace System.Web.Mvc.Test
{
    [Xunit.Collection("Uses ScopeStorage or ViewEngines.Engines")]
    public class ControllerTest
    {
        [Fact]
        public void ActionInvokerProperty()
        {
            // Arrange
            Controller controller = new EmptyController();

            // Act & Assert
            MemberHelper.TestPropertyWithDefaultInstance(controller, "ActionInvoker", new ControllerActionInvoker());
        }

        [Fact]
        public void ContentWithContentString()
        {
            // Arrange
            Controller controller = new EmptyController();
            string content = "Some content";

            // Act
            ContentResult result = controller.Content(content);

            // Assert
            Assert.Equal(content, result.Content);
        }

        [Fact]
        public void ContentWithContentStringAndContentType()
        {
            // Arrange
            Controller controller = new EmptyController();
            string content = "Some content";
            string contentType = "Some content type";

            // Act
            ContentResult result = controller.Content(content, contentType);

            // Assert
            Assert.Equal(content, result.Content);
            Assert.Equal(contentType, result.ContentType);
        }

        [Fact]
        public void ContentWithContentStringAndContentTypeAndEncoding()
        {
            // Arrange
            Controller controller = new EmptyController();
            string content = "Some content";
            string contentType = "Some content type";
            Encoding contentEncoding = Encoding.UTF8;

            // Act
            ContentResult result = controller.Content(content, contentType, contentEncoding);

            // Assert
            Assert.Equal(content, result.Content);
            Assert.Equal(contentType, result.ContentType);
            Assert.Same(contentEncoding, result.ContentEncoding);
        }

        [Fact]
        public void ContextProperty()
        {
            var controller = new EmptyController();
            MemberHelper.TestPropertyValue(controller, "ControllerContext", new Mock<ControllerContext>().Object);
        }

        [Fact]
        public void HttpContextProperty()
        {
            var c = new EmptyController();
            Assert.Null(c.HttpContext);

            Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();

            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.Setup(cc => cc.Controller).Returns(c);
            mockControllerContext.Setup(cc => cc.HttpContext).Returns(mockHttpContext.Object);

            c.ControllerContext = mockControllerContext.Object;
            Assert.Equal(mockHttpContext.Object, c.HttpContext);
        }

        [Fact]
        public void HttpNotFound()
        {
            // Arrange
            var c = new EmptyController();

            // Act
            HttpNotFoundResult result = c.HttpNotFound();

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.StatusDescription);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public void HttpNotFoundWithNullStatusDescription()
        {
            // Arrange
            var c = new EmptyController();

            // Act
            HttpNotFoundResult result = c.HttpNotFound(statusDescription: null);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.StatusDescription);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public void HttpNotFoundWithStatusDescription()
        {
            // Arrange
            var c = new EmptyController();

            // Act
            HttpNotFoundResult result = c.HttpNotFound(statusDescription: "I lost it");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("I lost it", result.StatusDescription);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public void ModelStateProperty()
        {
            // Arrange
            Controller controller = new EmptyController();

            // Act & assert
            Assert.Same(controller.ViewData.ModelState, controller.ModelState);
        }

        [Fact]
        public void ProfileProperty()
        {
            var c = new EmptyController();
            Assert.Null(c.Profile);

            Mock<ProfileBase> mockProfile = new Mock<ProfileBase>();

            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.Setup(cc => cc.Controller).Returns(c);
            mockControllerContext.Setup(cc => cc.HttpContext.Profile).Returns(mockProfile.Object);

            c.ControllerContext = mockControllerContext.Object;
            Assert.Equal(mockProfile.Object, c.Profile);
        }

        [Fact]
        public void RequestProperty()
        {
            var c = new EmptyController();
            Assert.Null(c.Request);

            Mock<HttpRequestBase> mockHttpRequest = new Mock<HttpRequestBase>();

            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.Setup(cc => cc.Controller).Returns(c);
            mockControllerContext.Setup(cc => cc.HttpContext.Request).Returns(mockHttpRequest.Object);

            c.ControllerContext = mockControllerContext.Object;
            Assert.Equal(mockHttpRequest.Object, c.Request);
        }

        [Fact]
        public void ResponseProperty()
        {
            var c = new EmptyController();
            Assert.Null(c.Request);

            Mock<HttpResponseBase> mockHttpResponse = new Mock<HttpResponseBase>();

            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.Setup(cc => cc.Controller).Returns(c);
            mockControllerContext.Setup(cc => cc.HttpContext.Response).Returns(mockHttpResponse.Object);

            c.ControllerContext = mockControllerContext.Object;
            Assert.Equal(mockHttpResponse.Object, c.Response);
        }

        [Fact]
        public void ServerProperty()
        {
            var c = new EmptyController();
            Assert.Null(c.Request);

            Mock<HttpServerUtilityBase> mockServerUtility = new Mock<HttpServerUtilityBase>();

            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.Setup(cc => cc.Controller).Returns(c);
            mockControllerContext.Setup(cc => cc.HttpContext.Server).Returns(mockServerUtility.Object);

            c.ControllerContext = mockControllerContext.Object;
            Assert.Equal(mockServerUtility.Object, c.Server);
        }

        [Fact]
        public void SessionProperty()
        {
            var c = new EmptyController();
            Assert.Null(c.Request);

            Mock<HttpSessionStateBase> mockSessionState = new Mock<HttpSessionStateBase>();

            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.Setup(cc => cc.Controller).Returns(c);
            mockControllerContext.Setup(cc => cc.HttpContext.Session).Returns(mockSessionState.Object);

            c.ControllerContext = mockControllerContext.Object;
            Assert.Same(mockSessionState.Object, c.Session);
        }

        [Fact]
        public void UrlProperty()
        {
            // Arrange
            EmptyController controller = new EmptyController();
            RequestContext requestContext = new RequestContext(new Mock<HttpContextBase>().Object, new RouteData());

            // Act
            controller.PublicInitialize(requestContext);

            // Assert
            Assert.NotNull(controller.Url);
        }

        [Fact]
        public void UserProperty()
        {
            var c = new EmptyController();
            Assert.Null(c.Request);

            Mock<IPrincipal> mockUser = new Mock<IPrincipal>();

            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.Setup(cc => cc.Controller).Returns(c);
            mockControllerContext.Setup(cc => cc.HttpContext.User).Returns(mockUser.Object);

            c.ControllerContext = mockControllerContext.Object;
            Assert.Equal(mockUser.Object, c.User);
        }

        [Fact]
        public void RouteDataProperty()
        {
            var c = new EmptyController();
            Assert.Null(c.Request);

            RouteData rd = new RouteData();

            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.Setup(cc => cc.Controller).Returns(c);
            mockControllerContext.Setup(cc => cc.RouteData).Returns(rd);

            c.ControllerContext = mockControllerContext.Object;
            Assert.Equal(rd, c.RouteData);
        }

        [Fact]
        public void ControllerMethodsDoNotHaveNonActionAttribute()
        {
            var methods = typeof(Controller).GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var method in methods)
            {
                var attrs = method.GetCustomAttributes(typeof(NonActionAttribute), true /* inherit */);
                Assert.True(attrs.Length == 0, "Methods on the Controller class should not be marked [NonAction]: " + method);
            }
        }

        [Fact]
        public void DisposeCallsProtectedDisposingMethod()
        {
            // Arrange
            Mock<Controller> mockController = new Mock<Controller>();
            mockController.Protected().Setup("Dispose", true).Verifiable();
            Controller controller = mockController.Object;

            // Act
            controller.Dispose();

            // Assert
            mockController.Verify();
        }

        [Fact]
        public void ExecuteWithUnknownAction()
        {
            // Arrange
            UnknownActionController controller = new UnknownActionController();
            // We need a provider since Controller.Execute is called
            controller.TempDataProvider = new EmptyTempDataProvider();
            ControllerContext context = GetControllerContext("Foo");

            Mock<IActionInvoker> mockInvoker = new Mock<IActionInvoker>();
            mockInvoker.Setup(o => o.InvokeAction(context, "Foo")).Returns(false);
            controller.ActionInvoker = mockInvoker.Object;

            // Act
            ((IController)controller).Execute(context.RequestContext);

            // Assert
            Assert.True(controller.WasCalled);
        }

        [Fact]
        public void FileWithContents()
        {
            // Arrange
            EmptyController controller = new EmptyController();
            byte[] fileContents = new byte[0];

            // Act
            FileContentResult result = controller.File(fileContents, "someContentType");

            // Assert
            Assert.NotNull(result);
            Assert.Same(fileContents, result.FileContents);
            Assert.Equal("someContentType", result.ContentType);
            Assert.Equal(String.Empty, result.FileDownloadName);
        }

        [Fact]
        public void FileWithContentsAndFileDownloadName()
        {
            // Arrange
            EmptyController controller = new EmptyController();
            byte[] fileContents = new byte[0];

            // Act
            FileContentResult result = controller.File(fileContents, "someContentType", "someDownloadName");

            // Assert
            Assert.NotNull(result);
            Assert.Same(fileContents, result.FileContents);
            Assert.Equal("someContentType", result.ContentType);
            Assert.Equal("someDownloadName", result.FileDownloadName);
        }

        [Fact]
        public void FileWithPath()
        {
            // Arrange
            EmptyController controller = new EmptyController();

            // Act
            FilePathResult result = controller.File("somePath", "someContentType");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("somePath", result.FileName);
            Assert.Equal("someContentType", result.ContentType);
            Assert.Equal(String.Empty, result.FileDownloadName);
        }

        [Fact]
        public void FileWithPathAndFileDownloadName()
        {
            // Arrange
            EmptyController controller = new EmptyController();

            // Act
            FilePathResult result = controller.File("somePath", "someContentType", "someDownloadName");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("somePath", result.FileName);
            Assert.Equal("someContentType", result.ContentType);
            Assert.Equal("someDownloadName", result.FileDownloadName);
        }

        [Fact]
        public void FileWithStream()
        {
            // Arrange
            EmptyController controller = new EmptyController();
            Stream fileStream = Stream.Null;

            // Act
            FileStreamResult result = controller.File(fileStream, "someContentType");

            // Assert
            Assert.NotNull(result);
            Assert.Same(fileStream, result.FileStream);
            Assert.Equal("someContentType", result.ContentType);
            Assert.Equal(String.Empty, result.FileDownloadName);
        }

        [Fact]
        public void FileWithStreamAndFileDownloadName()
        {
            // Arrange
            EmptyController controller = new EmptyController();
            Stream fileStream = Stream.Null;

            // Act
            FileStreamResult result = controller.File(fileStream, "someContentType", "someDownloadName");

            // Assert
            Assert.NotNull(result);
            Assert.Same(fileStream, result.FileStream);
            Assert.Equal("someContentType", result.ContentType);
            Assert.Equal("someDownloadName", result.FileDownloadName);
        }

        [Fact]
        public void HandleUnknownActionThrows()
        {
            var controller = new EmptyController();
            Assert.Throws<HttpException>(
                delegate { controller.HandleUnknownAction("UnknownAction"); },
                "A public action method 'UnknownAction' was not found on controller 'System.Web.Mvc.Test.ControllerTest+EmptyController'.");
        }

        /// <summary>
        /// This is a scenario for attribute routing, we don't have a single action nameat this point
        /// </summary>
        [Fact]
        public void HandleUnknownAction_NoActionName_Throws()
        {
            var controller = new EmptyController();
            Assert.Throws<HttpException>(
                delegate { controller.HandleUnknownAction(null); },
                "No matching action was found on controller 'System.Web.Mvc.Test.ControllerTest+EmptyController'. " +
                "This can happen when a controller uses RouteAttribute for routing, but no action on that controller matches the request.");
        }

        [Fact]
        public void JavaScript()
        {
            // Arrange
            Controller controller = GetEmptyController();
            string script = "alert('foo');";

            // Act
            JavaScriptResult result = controller.JavaScript(script);

            // Assert
            Assert.Equal(script, result.Script);
        }

        [Fact]
        public void PartialView()
        {
            // Arrange
            Controller controller = GetEmptyController();

            // Act
            PartialViewResult result = controller.PartialView();

            // Assert
            Assert.Same(controller.TempData, result.TempData);
            Assert.Same(controller.ViewData, result.ViewData);
            Assert.Same(ViewEngines.Engines, result.ViewEngineCollection);
        }

        [Fact]
        public void PartialView_Model()
        {
            // Arrange
            Controller controller = GetEmptyController();
            object model = new object();

            // Act
            PartialViewResult result = controller.PartialView(model);

            // Assert
            Assert.Same(model, result.ViewData.Model);
            Assert.Same(controller.TempData, result.TempData);
            Assert.Same(controller.ViewData, result.ViewData);
        }

        [Fact]
        public void PartialView_ViewName()
        {
            // Arrange
            Controller controller = GetEmptyController();

            // Act
            PartialViewResult result = controller.PartialView("Some partial view");

            // Assert
            Assert.Equal("Some partial view", result.ViewName);
            Assert.Same(controller.TempData, result.TempData);
            Assert.Same(controller.ViewData, result.ViewData);
        }

        [Fact]
        public void PartialView_ViewName_Model()
        {
            // Arrange
            Controller controller = GetEmptyController();
            object model = new object();

            // Act
            PartialViewResult result = controller.PartialView("Some partial view", model);

            // Assert
            Assert.Equal("Some partial view", result.ViewName);
            Assert.Same(model, result.ViewData.Model);
            Assert.Same(controller.TempData, result.TempData);
            Assert.Same(controller.ViewData, result.ViewData);
        }

        [Fact]
        public void PartialView_ViewEngineCollection()
        {
            // Arrange
            Controller controller = GetEmptyController();
            ViewEngineCollection viewEngines = new ViewEngineCollection();
            controller.ViewEngineCollection = viewEngines;

            // Act
            PartialViewResult result = controller.PartialView();

            // Assert
            Assert.Same(viewEngines, result.ViewEngineCollection);
        }

        [Fact]
        public void RedirectToActionClonesRouteValueDictionary()
        {
            // The RedirectToAction() method should clone the provided dictionary, then operate on the clone.
            // The original dictionary should remain unmodified throughout the helper's execution.

            // Arrange
            Controller controller = GetEmptyController();
            RouteValueDictionary values = new RouteValueDictionary(new { Action = "SomeAction", Controller = "SomeController" });

            // Act
            controller.RedirectToAction("SomeOtherAction", "SomeOtherController", values);

            // Assert
            Assert.Equal(2, values.Count);
            Assert.Equal("SomeAction", values["action"]);
            Assert.Equal("SomeController", values["controller"]);
        }

        [Fact]
        public void RedirectToActionOverwritesActionDictionaryKey()
        {
            // Arrange
            Controller controller = GetEmptyController();
            object values = new { Action = "SomeAction" };

            // Act
            RedirectToRouteResult result = controller.RedirectToAction("SomeOtherAction", values);
            RouteValueDictionary newValues = result.RouteValues;

            // Assert
            Assert.Equal("SomeOtherAction", newValues["action"]);
        }

        [Fact]
        public void RedirectToActionOverwritesControllerDictionaryKeyIfSpecified()
        {
            // Arrange
            Controller controller = GetEmptyController();
            object values = new { Action = "SomeAction", Controller = "SomeController" };

            // Act
            RedirectToRouteResult result = controller.RedirectToAction("SomeOtherAction", "SomeOtherController", values);
            RouteValueDictionary newValues = result.RouteValues;

            // Assert
            Assert.Equal("SomeOtherController", newValues["controller"]);
        }

        [Fact]
        public void RedirectToActionPreservesControllerDictionaryKeyIfNotSpecified()
        {
            // Arrange
            Controller controller = GetEmptyController();
            object values = new { Controller = "SomeController" };

            // Act
            RedirectToRouteResult result = controller.RedirectToAction("SomeOtherAction", values);
            RouteValueDictionary newValues = result.RouteValues;

            // Assert
            Assert.Equal("SomeController", newValues["controller"]);
        }

        [Fact]
        public void RedirectToActionWithActionName()
        {
            // Arrange
            Controller controller = GetEmptyController();

            // Act
            RedirectToRouteResult result = controller.RedirectToAction("SomeOtherAction");

            // Assert
            Assert.Equal("", result.RouteName);
            Assert.Equal("SomeOtherAction", result.RouteValues["action"]);
            Assert.False(result.Permanent);
        }

        [Fact]
        public void RedirectToActionWithActionNameAndControllerName()
        {
            // Arrange
            Controller controller = GetEmptyController();

            // Act
            RedirectToRouteResult result = controller.RedirectToAction("SomeOtherAction", "SomeOtherController");

            // Assert
            Assert.Equal("", result.RouteName);
            Assert.Equal("SomeOtherAction", result.RouteValues["action"]);
            Assert.Equal("SomeOtherController", result.RouteValues["controller"]);
            Assert.False(result.Permanent);
        }

        [Fact]
        public void RedirectToActionWithActionNameAndControllerNameAndValuesDictionary()
        {
            // Arrange
            Controller controller = GetEmptyController();
            RouteValueDictionary values = new RouteValueDictionary(new { Foo = "SomeFoo" });

            // Act
            RedirectToRouteResult result = controller.RedirectToAction("SomeOtherAction", "SomeOtherController", values);

            // Assert
            Assert.Equal("", result.RouteName);
            Assert.Equal("SomeOtherAction", result.RouteValues["action"]);
            Assert.Equal("SomeOtherController", result.RouteValues["controller"]);
            Assert.Equal("SomeFoo", result.RouteValues["foo"]);
            Assert.False(result.Permanent);
        }

        [Fact]
        public void RedirectToActionWithActionNameAndControllerNameAndValuesObject()
        {
            // Arrange
            Controller controller = GetEmptyController();
            object values = new { Foo = "SomeFoo" };

            // Act
            RedirectToRouteResult result = controller.RedirectToAction("SomeOtherAction", "SomeOtherController", values);

            // Assert
            Assert.Equal("", result.RouteName);
            Assert.Equal("SomeOtherAction", result.RouteValues["action"]);
            Assert.Equal("SomeOtherController", result.RouteValues["controller"]);
            Assert.Equal("SomeFoo", result.RouteValues["foo"]);
            Assert.False(result.Permanent);
        }

        [Fact]
        public void RedirectToActionSelectsCurrentControllerByDefault()
        {
            // Arrange
            TestRouteController controller = new TestRouteController();
            controller.ControllerContext = GetControllerContext("SomeAction", "TestRoute");

            // Act
            RedirectToRouteResult route = controller.Index() as RedirectToRouteResult;

            // Assert
            Assert.Equal("SomeAction", route.RouteValues["action"]);
            Assert.Equal("TestRoute", route.RouteValues["controller"]);
        }

        [Fact]
        public void RedirectToActionDictionaryOverridesDefaultControllerName()
        {
            // Arrange
            TestRouteController controller = new TestRouteController();
            object values = new { controller = "SomeOtherController" };
            controller.ControllerContext = GetControllerContext("SomeAction", "TestRoute");

            // Act
            RedirectToRouteResult route = controller.RedirectToAction("SomeOtherAction", values);

            // Assert
            Assert.Equal("SomeOtherAction", route.RouteValues["action"]);
            Assert.Equal("SomeOtherController", route.RouteValues["controller"]);
        }

        [Fact]
        public void RedirectToActionSimpleOverridesCallLegacyMethod()
        {
            // The simple overrides need to call RedirectToAction(string, string, RouteValueDictionary) to maintain backwards compat

            // Arrange
            int invocationCount = 0;
            Mock<Controller> controllerMock = new Mock<Controller>();
            controllerMock.Setup(c => c.RedirectToAction(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<RouteValueDictionary>())).Callback(() => { invocationCount++; });

            Controller controller = controllerMock.Object;

            // Act
            controller.RedirectToAction("SomeAction");
            controller.RedirectToAction("SomeAction", (object)null);
            controller.RedirectToAction("SomeAction", (RouteValueDictionary)null);
            controller.RedirectToAction("SomeAction", "SomeController");
            controller.RedirectToAction("SomeAction", "SomeController", (object)null);

            // Assert
            Assert.Equal(5, invocationCount);
        }

        [Fact]
        public void RedirectToActionWithActionNameAndValuesDictionary()
        {
            // Arrange
            Controller controller = GetEmptyController();
            RouteValueDictionary values = new RouteValueDictionary(new { Foo = "SomeFoo" });

            // Act
            RedirectToRouteResult result = controller.RedirectToAction("SomeOtherAction", values);

            // Assert
            Assert.Equal("", result.RouteName);
            Assert.Equal("SomeOtherAction", result.RouteValues["action"]);
            Assert.Equal("SomeFoo", result.RouteValues["foo"]);
            Assert.False(result.Permanent);
        }

        [Fact]
        public void RedirectToActionWithActionNameAndValuesObject()
        {
            // Arrange
            Controller controller = GetEmptyController();
            object values = new { Foo = "SomeFoo" };

            // Act
            RedirectToRouteResult result = controller.RedirectToAction("SomeOtherAction", values);

            // Assert
            Assert.Equal("", result.RouteName);
            Assert.Equal("SomeOtherAction", result.RouteValues["action"]);
            Assert.Equal("SomeFoo", result.RouteValues["foo"]);
            Assert.False(result.Permanent);
        }

        [Fact]
        public void RedirectToActionWithNullRouteValueDictionary()
        {
            // Arrange
            Controller controller = GetEmptyController();

            // Act
            RedirectToRouteResult result = controller.RedirectToAction("SomeOtherAction", (RouteValueDictionary)null);
            RouteValueDictionary newValues = result.RouteValues;

            // Assert
            Assert.Single(newValues);
            Assert.Equal("SomeOtherAction", newValues["action"]);
            Assert.False(result.Permanent);
        }

        [Fact]
        public void RedirectToActionPermanent()
        {
            // Arrange
            Controller controller = GetEmptyController();

            // Act
            RedirectToRouteResult result = controller.RedirectToActionPermanent("SomeOtherAction");

            // Assert
            Assert.True(result.Permanent);
            Assert.Equal("SomeOtherAction", result.RouteValues["action"]);
            Assert.Null(result.RouteValues["controller"]);
        }

        [Fact]
        public void RedirectToActionPermanentWithObjectDictionary()
        {
            // Arrange
            Controller controller = GetEmptyController();

            // Act
            RedirectToRouteResult result = controller.RedirectToActionPermanent("SomeOtherAction", controllerName: "SomeController", routeValues: new { foo = "bar" });

            // Assert
            Assert.True(result.Permanent);
            Assert.Equal("SomeOtherAction", result.RouteValues["action"]);
            Assert.Equal("bar", result.RouteValues["foo"]);
            Assert.Equal("SomeController", result.RouteValues["controller"]);
        }

        [Fact]
        public void RedirectToActionPermanentWithRouteValueDictionary()
        {
            // Arrange
            Controller controller = GetEmptyController();

            // Act
            RedirectToRouteResult result = controller.RedirectToActionPermanent("SomeOtherAction", routeValues: new RouteValueDictionary(new { foo = "bar" }));

            // Assert
            Assert.True(result.Permanent);
            Assert.Equal("SomeOtherAction", result.RouteValues["action"]);
            Assert.Equal("bar", result.RouteValues["foo"]);
        }

        [Fact]
        public void RedirectToRouteSimpleOverridesCallLegacyMethod()
        {
            // The simple overrides need to call RedirectToRoute(string, RouteValueDictionary) to maintain backwards compat

            // Arrange
            int invocationCount = 0;
            Mock<Controller> controllerMock = new Mock<Controller>();
            controllerMock.Setup(c => c.RedirectToRoute(It.IsAny<string>(), It.IsAny<RouteValueDictionary>())).Callback(() => { invocationCount++; });

            Controller controller = controllerMock.Object;

            // Act
            controller.RedirectToRoute("SomeRoute");
            controller.RedirectToRoute("SomeRoute", (object)null);
            controller.RedirectToRoute((object)null);
            controller.RedirectToRoute((RouteValueDictionary)null);

            // Assert
            Assert.Equal(4, invocationCount);
        }

        [Fact]
        public void RedirectToRouteWithNullRouteValueDictionary()
        {
            // Arrange
            Controller controller = GetEmptyController();

            // Act
            RedirectToRouteResult result = controller.RedirectToRoute((RouteValueDictionary)null);

            // Assert
            Assert.Empty(result.RouteValues);
            Assert.False(result.Permanent);
        }

        [Fact]
        public void RedirectToRouteWithObjectDictionary()
        {
            // Arrange
            Controller controller = GetEmptyController();
            var values = new { Foo = "MyFoo" };

            // Act
            RedirectToRouteResult result = controller.RedirectToRoute(values);

            // Assert
            Assert.Single(result.RouteValues);
            Assert.Equal("MyFoo", result.RouteValues["Foo"]);
            Assert.False(result.Permanent);
        }

        [Fact]
        public void RedirectToRouteWithRouteValueDictionary()
        {
            // Arrange
            Controller controller = GetEmptyController();
            RouteValueDictionary values = new RouteValueDictionary() { { "Foo", "MyFoo" } };

            // Act
            RedirectToRouteResult result = controller.RedirectToRoute(values);

            // Assert
            Assert.Single(result.RouteValues);
            Assert.Equal("MyFoo", result.RouteValues["Foo"]);
            Assert.NotSame(values, result.RouteValues);
            Assert.False(result.Permanent);
        }

        [Fact]
        public void RedirectToRouteWithName()
        {
            // Arrange
            Controller controller = GetEmptyController();

            // Act
            RedirectToRouteResult result = controller.RedirectToRoute("foo");

            // Assert
            Assert.Empty(result.RouteValues);
            Assert.Equal("foo", result.RouteName);
            Assert.False(result.Permanent);
        }

        [Fact]
        public void RedirectToRouteWithNameAndNullRouteValueDictionary()
        {
            // Arrange
            Controller controller = GetEmptyController();

            // Act
            RedirectToRouteResult result = controller.RedirectToRoute("foo", (RouteValueDictionary)null);

            // Assert
            Assert.Empty(result.RouteValues);
            Assert.Equal("foo", result.RouteName);
            Assert.False(result.Permanent);
        }

        [Fact]
        public void RedirectToRouteWithNullNameAndNullRouteValueDictionary()
        {
            // Arrange
            Controller controller = GetEmptyController();

            // Act
            RedirectToRouteResult result = controller.RedirectToRoute(null, (RouteValueDictionary)null);

            // Assert
            Assert.Empty(result.RouteValues);
            Assert.Equal(String.Empty, result.RouteName);
            Assert.False(result.Permanent);
        }

        [Fact]
        public void RedirectToRouteWithNameAndObjectDictionary()
        {
            // Arrange
            Controller controller = GetEmptyController();
            var values = new { Foo = "MyFoo" };

            // Act
            RedirectToRouteResult result = controller.RedirectToRoute("foo", values);

            // Assert
            Assert.Single(result.RouteValues);
            Assert.Equal("MyFoo", result.RouteValues["Foo"]);
            Assert.Equal("foo", result.RouteName);
            Assert.False(result.Permanent);
        }

        [Fact]
        public void RedirectToRouteWithNameAndRouteValueDictionary()
        {
            // Arrange
            Controller controller = GetEmptyController();
            RouteValueDictionary values = new RouteValueDictionary() { { "Foo", "MyFoo" } };

            // Act
            RedirectToRouteResult result = controller.RedirectToRoute("foo", values);

            // Assert
            Assert.Single(result.RouteValues);
            Assert.Equal("MyFoo", result.RouteValues["Foo"]);
            Assert.NotSame(values, result.RouteValues);
            Assert.Equal("foo", result.RouteName);
            Assert.False(result.Permanent);
        }

        [Fact]
        public void RedirectToRoutePermanentWithObjectDictionary()
        {
            // Arrange
            Controller controller = GetEmptyController();

            // Act
            RedirectToRouteResult result = controller.RedirectToRoutePermanent(routeValues: new { Foo = "Bar" });

            // Assert
            Assert.True(result.Permanent);
            Assert.Equal("Bar", result.RouteValues["Foo"]);
        }

        [Fact]
        public void RedirectToRoutePermanentWithRouteValueDictionary()
        {
            // Arrange
            Controller controller = GetEmptyController();

            // Act
            RedirectToRouteResult result = controller.RedirectToRoutePermanent(routeValues: new RouteValueDictionary(new { Foo = "Bar" }));

            // Assert
            Assert.True(result.Permanent);
            Assert.Equal("Bar", result.RouteValues["Foo"]);
        }

        [Fact]
        public void RedirectReturnsCorrectActionResult()
        {
            // Arrange
            Controller controller = GetEmptyController();

            // Act & Assert
            var result = controller.Redirect("http://www.contoso.com/");

            // Assert
            Assert.Equal("http://www.contoso.com/", result.Url);
            Assert.False(result.Permanent);
        }

        [Fact]
        public void RedirectPermanentReturnsCorrectActionResult()
        {
            // Arrange
            Controller controller = GetEmptyController();

            // Act & Assert
            var result = controller.RedirectPermanent("http://www.contoso.com/");

            // Assert
            Assert.Equal("http://www.contoso.com/", result.Url);
            Assert.True(result.Permanent);
        }

        [Fact]
        public void RedirectWithEmptyUrlThrows()
        {
            // Arrange
            Controller controller = GetEmptyController();

            // Act & Assert
            Assert.ThrowsArgumentNullOrEmpty(
                delegate { controller.Redirect(String.Empty); },
                "url");
        }

        [Fact]
        public void RedirectPermanentWithEmptyUrlThrows()
        {
            // Arrange
            Controller controller = GetEmptyController();

            // Act & Assert
            Assert.ThrowsArgumentNullOrEmpty(
                delegate { controller.RedirectPermanent(String.Empty); },
                "url");
        }

        [Fact]
        public void RedirectWithNullUrlThrows()
        {
            // Arrange
            Controller controller = GetEmptyController();

            // Act & Assert
            Assert.ThrowsArgumentNullOrEmpty(
                delegate { controller.Redirect(url: null); },
                "url");
        }

        [Fact]
        public void RedirectPermanentWithNullUrlThrows()
        {
            // Arrange
            Controller controller = GetEmptyController();

            // Act & Assert
            Assert.ThrowsArgumentNullOrEmpty(
                delegate { controller.RedirectPermanent(url: null); },
                "url");
        }

        [Fact]
        public void View()
        {
            // Arrange
            Controller controller = GetEmptyController();

            // Act
            ViewResult result = controller.View();

            // Assert
            Assert.Same(controller.ViewData, result.ViewData);
            Assert.Same(controller.TempData, result.TempData);
            Assert.Same(ViewEngines.Engines, result.ViewEngineCollection);
        }

        [Fact]
        public void View_Model()
        {
            // Arrange
            Controller controller = GetEmptyController();
            object viewItem = new object();

            // Act
            ViewResult result = controller.View(viewItem);

            // Assert
            Assert.Same(viewItem, result.ViewData.Model);
            Assert.Same(controller.TempData, result.TempData);
        }

        [Fact]
        public void View_ViewName()
        {
            // Arrange
            Controller controller = GetEmptyController();

            // Act
            ViewResult result = controller.View("Foo");

            // Assert
            Assert.Equal("Foo", result.ViewName);
            Assert.Same(controller.ViewData, result.ViewData);
            Assert.Same(controller.TempData, result.TempData);
        }

        [Fact]
        public void View_ViewName_Model()
        {
            // Arrange
            Controller controller = GetEmptyController();
            object viewItem = new object();

            // Act
            ViewResult result = controller.View("Foo", viewItem);

            // Assert
            Assert.Equal("Foo", result.ViewName);
            Assert.Same(viewItem, result.ViewData.Model);
            Assert.Same(controller.TempData, result.TempData);
        }

        [Fact]
        public void View_ViewName_MasterViewName()
        {
            // Arrange
            Controller controller = GetEmptyController();

            // Act
            ViewResult result = controller.View("Foo", "Bar");

            // Assert
            Assert.Equal("Foo", result.ViewName);
            Assert.Equal("Bar", result.MasterName);
            Assert.Same(controller.ViewData, result.ViewData);
            Assert.Same(controller.TempData, result.TempData);
        }

        [Fact]
        public void View_ViewName_MasterViewName_Model()
        {
            // Arrange
            Controller controller = GetEmptyController();
            object viewItem = new object();

            // Act
            ViewResult result = controller.View("Foo", "Bar", viewItem);

            // Assert
            Assert.Equal("Foo", result.ViewName);
            Assert.Equal("Bar", result.MasterName);
            Assert.Same(viewItem, result.ViewData.Model);
            Assert.Same(controller.TempData, result.TempData);
        }

        [Fact]
        public void View_View()
        {
            // Arrange
            Controller controller = GetEmptyController();
            IView view = new Mock<IView>().Object;

            // Act
            ViewResult result = controller.View(view);

            // Assert
            Assert.Same(result.View, view);
            Assert.Same(controller.ViewData, result.ViewData);
            Assert.Same(controller.TempData, result.TempData);
        }

        [Fact]
        public void View_View_Model()
        {
            // Arrange
            Controller controller = GetEmptyController();
            IView view = new Mock<IView>().Object;
            object model = new object();

            // Act
            ViewResult result = controller.View(view, model);

            // Assert
            Assert.Same(result.View, view);
            Assert.Same(controller.ViewData, result.ViewData);
            Assert.Same(controller.TempData, result.TempData);
            Assert.Same(model, result.ViewData.Model);
        }

        [Fact]
        public void View_ViewEngineCollection()
        {
            // Arrange
            Controller controller = GetEmptyController();
            ViewEngineCollection viewEngines = new ViewEngineCollection();
            controller.ViewEngineCollection = viewEngines;

            // Act
            ViewResult result = controller.View();

            // Assert
            Assert.Same(viewEngines, result.ViewEngineCollection);
        }

        internal static void AddRequestParams(Mock<HttpRequestBase> requestMock, object paramValues)
        {
            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(paramValues);
            foreach (PropertyDescriptor prop in props)
            {
                requestMock.Setup(o => o[It.Is<string>(item => String.Equals(prop.Name, item, StringComparison.OrdinalIgnoreCase))]).Returns((string)prop.GetValue(paramValues));
            }
        }

        [Fact]
        public void TempDataGreetUserWithNoUserIDRedirects()
        {
            // Arrange
            TempDataHomeController tempDataHomeController = new TempDataHomeController();

            // Act
            RedirectToRouteResult result = tempDataHomeController.GreetUser() as RedirectToRouteResult;
            RouteValueDictionary values = result.RouteValues;

            // Assert
            Assert.True(values.ContainsKey("action"));
            Assert.Equal("ErrorPage", values["action"]);
            Assert.Empty(tempDataHomeController.TempData);
        }

        [Fact]
        public void TempDataGreetUserWithUserIDCopiesToViewDataAndRenders()
        {
            // Arrange
            TempDataHomeController tempDataHomeController = new TempDataHomeController();
            tempDataHomeController.TempData["UserID"] = "TestUserID";

            // Act
            ViewResult result = tempDataHomeController.GreetUser() as ViewResult;
            ViewDataDictionary viewData = tempDataHomeController.ViewData;

            // Assert
            Assert.Equal("GreetUser", result.ViewName);
            Assert.NotNull(viewData);
            Assert.True(viewData.ContainsKey("NewUserID"));
            Assert.Equal("TestUserID", viewData["NewUserID"]);
        }

        [Fact]
        public void TempDataIndexSavesUserIDAndRedirects()
        {
            // Arrange
            TempDataHomeController tempDataHomeController = new TempDataHomeController();

            // Act
            RedirectToRouteResult result = tempDataHomeController.Index() as RedirectToRouteResult;
            RouteValueDictionary values = result.RouteValues;

            // Assert
            Assert.True(values.ContainsKey("action"));
            Assert.Equal("GreetUser", values["action"]);

            Assert.True(tempDataHomeController.TempData.ContainsKey("UserID"));
            Assert.Equal("user123", tempDataHomeController.TempData["UserID"]);
        }

        [Fact]
        public void TempDataSavedWhenControllerThrows()
        {
            // Arrange
            BrokenController controller = new BrokenController() { ValidateRequest = false };
            Mock<HttpContextBase> mockContext = HttpContextHelpers.GetMockHttpContext();
            HttpSessionStateBase session = GetEmptySession();
            mockContext.Setup(o => o.Session).Returns(session);
            RouteData rd = new RouteData();
            rd.Values.Add("action", "Crash");
            controller.ControllerContext = new ControllerContext(mockContext.Object, rd, controller);

            // Assert
            Assert.Throws<InvalidOperationException>(
                delegate { ((IController)controller).Execute(controller.ControllerContext.RequestContext); });
            Assert.NotNull(mockContext.Object.Session[SessionStateTempDataProvider.TempDataSessionStateKey]);
            TempDataDictionary tempData = new TempDataDictionary();
            tempData.Load(controller.ControllerContext, controller.TempDataProvider);
            Assert.Equal("Value1", tempData["Key1"]);
        }

        [Fact]
        public void TempDataMovedToPreviousTempDataInDestinationController()
        {
            // Arrange
            Mock<Controller> mockController = new Mock<Controller>() { CallBase = true };
            Mock<HttpContextBase> mockContext = new Mock<HttpContextBase>();
            HttpSessionStateBase session = GetEmptySession();
            mockContext.Setup(o => o.Session).Returns(session);
            mockController.Object.ControllerContext = new ControllerContext(mockContext.Object, new RouteData(), mockController.Object);

            // Act
            mockController.Object.TempData.Add("Key", "Value");
            mockController.Object.TempData.Save(mockController.Object.ControllerContext, mockController.Object.TempDataProvider);

            // Assert
            Assert.True(mockController.Object.TempData.ContainsKey("Key"));
            Assert.True(mockController.Object.TempData.ContainsValue("Value"));

            // Instantiate "destination" controller with the same session state and see that it gets the temp data
            Mock<Controller> mockDestinationController = new Mock<Controller>() { CallBase = true };
            Mock<HttpContextBase> mockDestinationContext = new Mock<HttpContextBase>();
            mockDestinationContext.Setup(o => o.Session).Returns(session);
            mockDestinationController.Object.ControllerContext = new ControllerContext(mockDestinationContext.Object, new RouteData(), mockDestinationController.Object);
            mockDestinationController.Object.TempData.Load(mockDestinationController.Object.ControllerContext, mockDestinationController.Object.TempDataProvider);

            // Assert
            Assert.True(mockDestinationController.Object.TempData.ContainsKey("Key"));

            // Act
            mockDestinationController.Object.TempData["NewKey"] = "NewValue";
            Assert.True(mockDestinationController.Object.TempData.ContainsKey("NewKey"));
            mockDestinationController.Object.TempData.Save(mockDestinationController.Object.ControllerContext, mockDestinationController.Object.TempDataProvider);

            // Instantiate "second destination" controller with the same session state and see that it gets the temp data
            Mock<Controller> mockSecondDestinationController = new Mock<Controller>() { CallBase = true };
            Mock<HttpContextBase> mockSecondDestinationContext = new Mock<HttpContextBase>();
            mockSecondDestinationContext.Setup(o => o.Session).Returns(session);
            mockSecondDestinationController.Object.ControllerContext = new ControllerContext(mockSecondDestinationContext.Object, new RouteData(), mockSecondDestinationController.Object);
            mockSecondDestinationController.Object.TempData.Load(mockSecondDestinationController.Object.ControllerContext, mockSecondDestinationController.Object.TempDataProvider);

            // Assert
            Assert.True(mockSecondDestinationController.Object.TempData.ContainsKey("Key"));
            Assert.True(mockSecondDestinationController.Object.TempData.ContainsKey("NewKey"));
        }

        [Fact]
        public void TempDataRemovesKeyWhenRead()
        {
            // Arrange
            Mock<Controller> mockController = new Mock<Controller>() { CallBase = true };
            Mock<HttpContextBase> mockContext = new Mock<HttpContextBase>();
            HttpSessionStateBase session = GetEmptySession();
            mockContext.Setup(o => o.Session).Returns(session);
            mockController.Object.ControllerContext = new ControllerContext(mockContext.Object, new RouteData(), mockController.Object);

            // Act
            mockController.Object.TempData.Add("Key", "Value");
            mockController.Object.TempData.Save(mockController.Object.ControllerContext, mockController.Object.TempDataProvider);

            // Assert
            Assert.True(mockController.Object.TempData.ContainsKey("Key"));
            Assert.True(mockController.Object.TempData.ContainsValue("Value"));

            // Instantiate "destination" controller with the same session state and see that it gets the temp data
            Mock<Controller> mockDestinationController = new Mock<Controller>() { CallBase = true };
            Mock<HttpContextBase> mockDestinationContext = new Mock<HttpContextBase>();
            mockDestinationContext.Setup(o => o.Session).Returns(session);
            mockDestinationController.Object.ControllerContext = new ControllerContext(mockDestinationContext.Object, new RouteData(), mockDestinationController.Object);
            mockDestinationController.Object.TempData.Load(mockDestinationController.Object.ControllerContext, mockDestinationController.Object.TempDataProvider);

            // Assert
            Assert.True(mockDestinationController.Object.TempData.ContainsKey("Key"));

            // Act
            object value = mockDestinationController.Object.TempData["Key"];
            mockDestinationController.Object.TempData.Save(mockDestinationController.Object.ControllerContext, mockDestinationController.Object.TempDataProvider);

            // Instantiate "second destination" controller with the same session state and see that it gets the temp data
            Mock<Controller> mockSecondDestinationController = new Mock<Controller>() { CallBase = true };
            Mock<HttpContextBase> mockSecondDestinationContext = new Mock<HttpContextBase>();
            mockSecondDestinationContext.Setup(o => o.Session).Returns(session);
            mockSecondDestinationController.Object.ControllerContext = new ControllerContext(mockSecondDestinationContext.Object, new RouteData(), mockSecondDestinationController.Object);
            mockSecondDestinationController.Object.TempData.Load(mockSecondDestinationController.Object.ControllerContext, mockSecondDestinationController.Object.TempDataProvider);

            // Assert
            Assert.False(mockSecondDestinationController.Object.TempData.ContainsKey("Key"));
        }

        [Fact]
        public void TempDataValidForSingleControllerWhenSessionStateDisabled()
        {
            // Arrange
            Mock<Controller> mockController = new Mock<Controller>();
            Mock<HttpContextBase> mockContext = new Mock<HttpContextBase>();
            HttpSessionStateBase session = null;
            mockContext.Setup(o => o.Session).Returns(session);
            mockController.Object.ControllerContext = new ControllerContext(mockContext.Object, new RouteData(), mockController.Object);
            mockController.Object.TempData = new TempDataDictionary();

            // Act
            mockController.Object.TempData["Key"] = "Value";

            // Assert
            Assert.True(mockController.Object.TempData.ContainsKey("Key"));
        }

        [Fact]
        public void TryUpdateModelCallsModelBinderForModel()
        {
            // Arrange
            MyModel myModel = new MyModelSubclassed();
            IValueProvider valueProvider = new SimpleValueProvider();

            Controller controller = new EmptyController();
            controller.ControllerContext = GetControllerContext("someAction");

            // Act
            bool returned = controller.TryUpdateModel(myModel, "somePrefix", new[] { "prop1", "prop2" }, null, valueProvider);

            // Assert
            Assert.True(returned);
            Assert.Equal(valueProvider, myModel.BindingContext.ValueProvider);
            Assert.Equal("somePrefix", myModel.BindingContext.ModelName);
            Assert.Equal(controller.ModelState, myModel.BindingContext.ModelState);
            Assert.Equal(typeof(MyModel), myModel.BindingContext.ModelType);
            Assert.True(myModel.BindingContext.PropertyFilter("prop1"));
            Assert.True(myModel.BindingContext.PropertyFilter("prop2"));
            Assert.False(myModel.BindingContext.PropertyFilter("prop3"));
        }

        [Fact]
        public void TryUpdateModelReturnsFalseIfModelStateInvalid()
        {
            // Arrange
            MyModel myModel = new MyModelSubclassed();

            Controller controller = new EmptyController();
            controller.ModelState.AddModelError("key", "some exception message");

            // Act
            bool returned = controller.TryUpdateModel(myModel, new SimpleValueProvider());

            // Assert
            Assert.False(returned);
        }

        [Fact]
        public void TryUpdateModelSuppliesControllerValueProviderIfNoValueProviderSpecified()
        {
            // Arrange
            MyModel myModel = new MyModelSubclassed();
            IValueProvider valueProvider = new SimpleValueProvider();

            Controller controller = new EmptyController();
            controller.ValueProvider = valueProvider;

            // Act
            bool returned = controller.TryUpdateModel(myModel, "somePrefix", new[] { "prop1", "prop2" });

            // Assert
            Assert.True(returned);
            Assert.Equal(valueProvider, myModel.BindingContext.ValueProvider);
        }

        [Fact]
        public void TryUpdateModelSuppliesEmptyModelNameIfNoPrefixSpecified()
        {
            // Arrange
            MyModel myModel = new MyModelSubclassed();
            Controller controller = new EmptyController();

            // Act
            bool returned = controller.TryUpdateModel(myModel, new[] { "prop1", "prop2" }, new SimpleValueProvider());

            // Assert
            Assert.True(returned);
            Assert.Equal(String.Empty, myModel.BindingContext.ModelName);
        }

        [Fact]
        public void TryUpdateModelThrowsIfModelIsNull()
        {
            // Arrange
            Controller controller = new EmptyController();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                delegate { controller.TryUpdateModel<object>(null, new SimpleValueProvider()); }, "model");
        }

        [Fact]
        public void TryUpdateModelThrowsIfValueProviderIsNull()
        {
            // Arrange
            Controller controller = new EmptyController();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                delegate { controller.TryUpdateModel(new object(), null, null, null, null); }, "valueProvider");
        }

        [Fact]
        public void UpdateModelReturnsIfModelStateValid()
        {
            // Arrange
            MyModel myModel = new MyModelSubclassed();
            Controller controller = new EmptyController();

            // Act
            controller.UpdateModel(myModel, new SimpleValueProvider());

            // Assert
            // nothing to do - if we got here, the test passed
        }

        [Fact]
        public void TryUpdateModelWithoutBindPropertiesImpliesAllPropertiesAreUpdateable()
        {
            // Arrange
            MyModel myModel = new MyModelSubclassed();
            Controller controller = new EmptyController();

            // Act
            bool returned = controller.TryUpdateModel(myModel, "somePrefix", new SimpleValueProvider());

            // Assert
            Assert.True(returned);
            Assert.True(myModel.BindingContext.PropertyFilter("prop1"));
            Assert.True(myModel.BindingContext.PropertyFilter("prop2"));
            Assert.True(myModel.BindingContext.PropertyFilter("prop3"));
        }

        [Fact]
        public void UpdateModelSuppliesControllerValueProviderIfNoValueProviderSpecified()
        {
            // Arrange
            MyModel myModel = new MyModelSubclassed();
            IValueProvider valueProvider = new SimpleValueProvider();

            Controller controller = new EmptyController() { ValueProvider = valueProvider };

            // Act
            controller.UpdateModel(myModel, "somePrefix", new[] { "prop1", "prop2" });

            // Assert
            Assert.Equal(valueProvider, myModel.BindingContext.ValueProvider);
        }

        [Fact]
        public void UpdateModelThrowsIfModelStateInvalid()
        {
            // Arrange
            MyModel myModel = new MyModelSubclassed();

            Controller controller = new EmptyController();
            controller.ModelState.AddModelError("key", "some exception message");

            // Act & assert
            Assert.Throws<InvalidOperationException>(
                delegate { controller.UpdateModel(myModel, new SimpleValueProvider()); },
                "The model of type 'System.Web.Mvc.Test.ControllerTest+MyModel' could not be updated.");
        }

        [Fact]
        public void UpdateModelWithoutBindPropertiesImpliesAllPropertiesAreUpdateable()
        {
            // Arrange
            MyModel myModel = new MyModelSubclassed();
            Controller controller = new EmptyController();

            // Act
            controller.UpdateModel(myModel, "somePrefix", new SimpleValueProvider());

            // Assert
            Assert.True(myModel.BindingContext.PropertyFilter("prop1"));
            Assert.True(myModel.BindingContext.PropertyFilter("prop2"));
            Assert.True(myModel.BindingContext.PropertyFilter("prop3"));
        }

        [Fact]
        public void Json()
        {
            // Arrange
            MyModel model = new MyModel();
            Controller controller = new EmptyController();

            // Act
            JsonResult result = controller.Json(model);

            // Assert
            Assert.Same(model, result.Data);
            Assert.Null(result.ContentType);
            Assert.Null(result.ContentEncoding);
            Assert.Equal(JsonRequestBehavior.DenyGet, result.JsonRequestBehavior);
        }

        [Fact]
        public void JsonWithContentType()
        {
            // Arrange
            MyModel model = new MyModel();
            Controller controller = new EmptyController();

            // Act
            JsonResult result = controller.Json(model, "text/xml");

            // Assert
            Assert.Same(model, result.Data);
            Assert.Equal("text/xml", result.ContentType);
            Assert.Null(result.ContentEncoding);
            Assert.Equal(JsonRequestBehavior.DenyGet, result.JsonRequestBehavior);
        }

        [Fact]
        public void JsonWithContentTypeAndEncoding()
        {
            // Arrange
            MyModel model = new MyModel();
            Controller controller = new EmptyController();

            // Act
            JsonResult result = controller.Json(model, "text/xml", Encoding.UTF32);

            // Assert
            Assert.Same(model, result.Data);
            Assert.Equal("text/xml", result.ContentType);
            Assert.Equal(Encoding.UTF32, result.ContentEncoding);
            Assert.Equal(JsonRequestBehavior.DenyGet, result.JsonRequestBehavior);
        }

        [Fact]
        public void JsonWithBehavior()
        {
            // Arrange
            MyModel model = new MyModel();
            Controller controller = new EmptyController();

            // Act
            JsonResult result = controller.Json(model, JsonRequestBehavior.AllowGet);

            // Assert
            Assert.Same(model, result.Data);
            Assert.Null(result.ContentType);
            Assert.Null(result.ContentEncoding);
            Assert.Equal(JsonRequestBehavior.AllowGet, result.JsonRequestBehavior);
        }

        [Fact]
        public void JsonWithContentTypeAndBehavior()
        {
            // Arrange
            MyModel model = new MyModel();
            Controller controller = new EmptyController();

            // Act
            JsonResult result = controller.Json(model, "text/xml", JsonRequestBehavior.AllowGet);

            // Assert
            Assert.Same(model, result.Data);
            Assert.Equal("text/xml", result.ContentType);
            Assert.Null(result.ContentEncoding);
            Assert.Equal(JsonRequestBehavior.AllowGet, result.JsonRequestBehavior);
        }

        [Fact]
        public void JsonWithContentTypeAndEncodingAndBehavior()
        {
            // Arrange
            MyModel model = new MyModel();
            Controller controller = new EmptyController();

            // Act
            JsonResult result = controller.Json(model, "text/xml", Encoding.UTF32, JsonRequestBehavior.AllowGet);

            // Assert
            Assert.Same(model, result.Data);
            Assert.Equal("text/xml", result.ContentType);
            Assert.Equal(Encoding.UTF32, result.ContentEncoding);
            Assert.Equal(JsonRequestBehavior.AllowGet, result.JsonRequestBehavior);
        }

        [Fact]
        public void ExecuteDoesNotCallTempDataLoadOrSave()
        {
            // Arrange
            TempDataDictionary tempData = new TempDataDictionary();
            ViewContext viewContext = new ViewContext { TempData = tempData };
            RouteData routeData = new RouteData();
            routeData.DataTokens[ControllerContext.ParentActionViewContextToken] = viewContext;
            routeData.Values["action"] = "SimpleAction";
            RequestContext requestContext = new RequestContext(HttpContextHelpers.GetMockHttpContext().Object, routeData);
            // Strict == no default implementations == calls to Load & Save are not allowed
            Mock<ITempDataProvider> tempDataProvider = new Mock<ITempDataProvider>(MockBehavior.Strict);
            SimpleController controller = new SimpleController();
            controller.ValidateRequest = false;

            // Act
            ((IController)controller).Execute(requestContext);

            // Assert
            tempDataProvider.Verify();
        }

        // Model validation

        [Fact]
        public void TryValidateModelGuardClauses()
        {
            // Arrange
            Controller controller = new SimpleController();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => controller.TryValidateModel(null),
                "model");
        }

        [Fact]
        public void TryValidateModelWithValidModel()
        {
            // Arrange
            Controller controller = new SimpleController();
            TryValidateModelModel model = new TryValidateModelModel { IntegerProperty = 15 };

            // Act
            bool result = controller.TryValidateModel(model);

            // Assert
            Assert.True(result);
            Assert.True(controller.ModelState.IsValid);
        }

        [Fact]
        public void TryValidateModelWithInvalidModel()
        {
            // Arrange
            Controller controller = new SimpleController();
            TryValidateModelModel model = new TryValidateModelModel { IntegerProperty = 5 };

            // Act
            bool result = controller.TryValidateModel(model, "Prefix");

            // Assert
            Assert.False(result);
            Assert.Equal("Out of range!", controller.ModelState["Prefix.IntegerProperty"].Errors[0].ErrorMessage);
        }

        [Fact]
        public void ValidateModelGuardClauses()
        {
            // Arrange
            Controller controller = new SimpleController();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => controller.ValidateModel(null),
                "model");
        }

        [Fact]
        public void ValidateModelWithValidModel()
        {
            // Arrange
            Controller controller = new SimpleController();
            TryValidateModelModel model = new TryValidateModelModel { IntegerProperty = 15 };

            // Act
            controller.ValidateModel(model);

            // Assert
            Assert.True(controller.ModelState.IsValid);
        }

        [Fact]
        public void ValidateModelWithInvalidModel()
        {
            // Arrange
            Controller controller = new SimpleController();
            TryValidateModelModel model = new TryValidateModelModel { IntegerProperty = 5 };

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => controller.ValidateModel(model, "Prefix"),
                "The model of type '" + model.GetType().FullName + "' is not valid.");

            Assert.Equal("Out of range!", controller.ModelState["Prefix.IntegerProperty"].Errors[0].ErrorMessage);
        }

        [Fact]
        public void ValidateControllerUsesCachedResolver()
        {
            var controller = new EmptyController();

            var resolver = controller.Resolver;

            Assert.Equal(DependencyResolver.CurrentCache.GetType(), resolver.GetType());
        }


        [Fact]
        public void CreateActionInvokerCallsAsyncActionInvokerFactoryCreateInstance()
        {
            // Controller uses an IDependencyResolver and an IAsyncActionInvokerFactory
            // to create an IAsyncActionInvoker.
            // Arrange
            var controller = new EmptyController();
            Mock<IDependencyResolver> resolverMock = new Mock<IDependencyResolver>();
            Mock<IAsyncActionInvoker> asyncActionInvokerMock = new Mock<IAsyncActionInvoker>();
            Mock<IAsyncActionInvokerFactory> asyncActionInvokerFactoryMock = new Mock<IAsyncActionInvokerFactory>();
            asyncActionInvokerFactoryMock.Setup(a => a.CreateInstance()).Returns(asyncActionInvokerMock.Object);
            resolverMock.Setup(r => r.GetService(typeof(IAsyncActionInvokerFactory)))
                .Returns(asyncActionInvokerFactoryMock.Object);
            resolverMock.Setup(r => r.GetService(typeof(IActionInvokerFactory)))
                .Returns((IActionInvokerFactory)null);
            controller.Resolver = resolverMock.Object;

            // Act
            var ai = controller.CreateActionInvoker();

            // Assert
            resolverMock.Verify(r => r.GetService(typeof(IAsyncActionInvokerFactory)), Times.Once());
            resolverMock.Verify(r => r.GetService(typeof(IActionInvokerFactory)), Times.Never());
            Assert.Same(asyncActionInvokerMock.Object, ai);
        }

        [Fact]
        public void CreateActionInvokerCallsActionInvokerFactoryCreateInstance()
        {
            // Controller uses an IDependencyResolver and an IActionInvokerFactory to create an IActionInvoker.
            // Arrange
            var controller = new EmptyController();
            Mock<IDependencyResolver> resolverMock = new Mock<IDependencyResolver>();
            Mock<IActionInvoker> actionInvokerMock = new Mock<IActionInvoker>();
            Mock<IActionInvokerFactory> actionInvokerFactoryMock = new Mock<IActionInvokerFactory>();
            actionInvokerFactoryMock.Setup(a => a.CreateInstance()).Returns(actionInvokerMock.Object);
            resolverMock.Setup(r => r.GetService(typeof(IAsyncActionInvokerFactory)))
                .Returns((IAsyncActionInvokerFactory)null);
            resolverMock.Setup(r => r.GetService(typeof(IActionInvokerFactory)))
                .Returns(actionInvokerFactoryMock.Object);
            controller.Resolver = resolverMock.Object;

            // Act
            var ai = controller.CreateActionInvoker();

            // Assert
            resolverMock.Verify(r => r.GetService(typeof(IAsyncActionInvokerFactory)), Times.Once());
            resolverMock.Verify(r => r.GetService(typeof(IActionInvokerFactory)), Times.Once());
            Assert.Same(actionInvokerMock.Object, ai);
        }

        [Fact]
        public void CreateActionInvokerCallsIntoResolverInstance()
        {
            // Controller uses an IDependencyResolver to create an IActionInvoker.
            var controller = new EmptyController();
            Mock<IDependencyResolver> resolverMock = new Mock<IDependencyResolver>();
            Mock<IAsyncActionInvoker> actionInvokerMock = new Mock<IAsyncActionInvoker>();
            resolverMock.Setup(r => r.GetService(typeof(IAsyncActionInvoker))).Returns(actionInvokerMock.Object);
            controller.Resolver = resolverMock.Object;

            var ai = controller.CreateActionInvoker();

            resolverMock.Verify(r => r.GetService(typeof(IAsyncActionInvoker)), Times.Once());
            Assert.Same(actionInvokerMock.Object, ai);
        }

        [Fact]
        public void CreateActionInvokerCallsIntoResolverInstanceAndCreatesANewOneIfNecessary()
        {
            // If IDependencyResolver is set, but empty, falls back and still creates.
            var controller = new EmptyController();
            Mock<IDependencyResolver> resolverMock = new Mock<IDependencyResolver>();
            resolverMock.Setup(r => r.GetService(typeof(IAsyncActionInvoker))).Returns(null);
            controller.Resolver = resolverMock.Object;

            IActionInvoker ai = controller.CreateActionInvoker();

            resolverMock.Verify(r => r.GetService(typeof(IAsyncActionInvoker)), Times.Once());
            Assert.NotNull(ai);
        }

        [Fact]
        public void CreateTempProviderWithResolver()
        {
            // Controller uses an IDependencyResolver to create an ITempDataProvider.
            var controller = new EmptyController();
            Mock<IDependencyResolver> resolverMock = new Mock<IDependencyResolver>();
            Mock<ITempDataProvider> tempMock = new Mock<ITempDataProvider>();
            resolverMock.Setup(r => r.GetService(typeof(ITempDataProvider))).Returns(tempMock.Object);
            controller.Resolver = resolverMock.Object;

            ITempDataProvider temp = controller.CreateTempDataProvider();

            resolverMock.Verify(r => r.GetService(typeof(ITempDataProvider)), Times.Once());
            Assert.Same(tempMock.Object, temp);
        }

        [Fact]
        public void CreateTempProviderWithResolver_ITempDataProviderFactory()
        {
            // Controller uses an IDependencyResolver and an ITempDataProviderFactory to create an ITempDataProvider.
            // Arrange
            var controller = new EmptyController();
            Mock<IDependencyResolver> resolverMock = new Mock<IDependencyResolver>();
            Mock<ITempDataProvider> tempMock = new Mock<ITempDataProvider>();
            Mock<ITempDataProviderFactory> factoryMock = new Mock<ITempDataProviderFactory>();
            factoryMock.Setup(f => f.CreateInstance()).Returns(tempMock.Object);
            resolverMock.Setup(r => r.GetService(typeof(ITempDataProvider))).Returns(tempMock.Object);
            resolverMock.Setup(r => r.GetService(typeof(ITempDataProviderFactory))).Returns(factoryMock.Object);
            controller.Resolver = resolverMock.Object;

            // Act
            ITempDataProvider temp = controller.CreateTempDataProvider();

            // Assert
            // Times.Once() and Times.Never() confirm the order.
            resolverMock.Verify(r => r.GetService(typeof(ITempDataProviderFactory)), Times.Once());
            resolverMock.Verify(r => r.GetService(typeof(ITempDataProvider)), Times.Never());
            Assert.Same(tempMock.Object, temp);
        }

        [Fact]
        public void CanMock_UrlHelper()
        {
            // Arrange
            var controller = new UrlHelperController();
            Mock<UrlHelper> urlHelper = new Mock<UrlHelper>();
            urlHelper.Setup(u => u.Action("SimpleAction", new { ID = 42 })).Verifiable();
            controller.Url = urlHelper.Object;

            // Act
            controller.SimpleAction();

            // Assert
            urlHelper.Verify();
        }

        private class TryValidateModelModel
        {
            [Range(10, 20, ErrorMessage = "Out of range!")]
            public int IntegerProperty { get; set; }
        }

        // Helpers

        private class UrlHelperController : Controller
        {
            public string SimpleAction()
            {
                return Url.Action("SimpleAction", new { ID = 42 });
            }
        }

        private class SimpleController : Controller
        {
            public SimpleController()
            {
                ControllerContext = new ControllerContext { Controller = this };
            }

            public void SimpleAction()
            {
            }
        }

        private static ControllerContext GetControllerContext(string actionName)
        {
            RouteData rd = new RouteData();
            rd.Values["action"] = actionName;

            Mock<HttpContextBase> mockHttpContext = HttpContextHelpers.GetMockHttpContext();
            mockHttpContext.Setup(c => c.Session).Returns((HttpSessionStateBase)null);

            return new ControllerContext(mockHttpContext.Object, rd, new Mock<Controller>().Object);
        }

        private static ControllerContext GetControllerContext(string actionName, string controllerName)
        {
            RouteData rd = new RouteData();
            rd.Values["action"] = actionName;
            rd.Values["controller"] = controllerName;

            Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();
            mockHttpContext.Setup(c => c.Session).Returns((HttpSessionStateBase)null);

            return new ControllerContext(mockHttpContext.Object, rd, new Mock<Controller>().Object);
        }

        private static Controller GetEmptyController()
        {
            ControllerContext context = GetControllerContext("Foo");
            var controller = new EmptyController()
            {
                ControllerContext = context,
                RouteCollection = new RouteCollection(),
                TempData = new TempDataDictionary(),
                TempDataProvider = new SessionStateTempDataProvider()
            };
            return controller;
        }

        private static HttpSessionStateBase GetEmptySession()
        {
            HttpSessionStateMock mockSession = new HttpSessionStateMock();
            return mockSession;
        }

        private sealed class HttpSessionStateMock : HttpSessionStateBase
        {
            private Hashtable _sessionData = new Hashtable(StringComparer.OrdinalIgnoreCase);

            public override void Remove(string name)
            {
                Assert.Equal(SessionStateTempDataProvider.TempDataSessionStateKey, name);
                _sessionData.Remove(name);
            }

            public override object this[string name]
            {
                get
                {
                    Assert.Equal(SessionStateTempDataProvider.TempDataSessionStateKey, name);
                    return _sessionData[name];
                }
                set
                {
                    Assert.Equal(SessionStateTempDataProvider.TempDataSessionStateKey, name);
                    _sessionData[name] = value;
                }
            }
        }

        public class Person
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }

        private class EmptyController : Controller
        {
            public new void HandleUnknownAction(string actionName)
            {
                base.HandleUnknownAction(actionName);
            }

            public void PublicInitialize(RequestContext requestContext)
            {
                base.Initialize(requestContext);
            }

            // Test can expose protected method as public.
            public new IActionInvoker CreateActionInvoker()
            {
                return base.CreateActionInvoker();
            }

            public new ITempDataProvider CreateTempDataProvider()
            {
                return base.CreateTempDataProvider();
            }
        }


        private sealed class UnknownActionController : Controller
        {
            public bool WasCalled;

            protected override void HandleUnknownAction(string actionName)
            {
                WasCalled = true;
            }
        }

        private sealed class TempDataHomeController : Controller
        {
            public ActionResult Index()
            {
                // Save UserID into TempData and redirect to greeting page
                TempData["UserID"] = "user123";
                return RedirectToAction("GreetUser");
            }

            public ActionResult GreetUser()
            {
                // Check that the UserID is present. If it's not
                // there, redirect to error page. If it is, show
                // the greet user view.
                if (!TempData.ContainsKey("UserID"))
                {
                    return RedirectToAction("ErrorPage");
                }
                ViewData["NewUserID"] = TempData["UserID"];
                return View("GreetUser");
            }
        }

        public class BrokenController : Controller
        {
            public BrokenController()
            {
                ActionInvoker = new ControllerActionInvoker()
                {
                    DescriptorCache = new ControllerDescriptorCache()
                };
            }

            public ActionResult Crash()
            {
                TempData["Key1"] = "Value1";
                throw new InvalidOperationException("Crashing....");
            }
        }

        private sealed class TestRouteController : Controller
        {
            public ActionResult Index()
            {
                return RedirectToAction("SomeAction");
            }
        }

        [ModelBinder(typeof(MyModelBinder))]
        private class MyModel
        {
            public ControllerContext ControllerContext;
            public ModelBindingContext BindingContext;
        }

        private class MyModelSubclassed : MyModel
        {
        }

        private class MyModelBinder : IModelBinder
        {
            public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
            {
                MyModel myModel = (MyModel)bindingContext.Model;
                myModel.ControllerContext = controllerContext;
                myModel.BindingContext = bindingContext;
                return myModel;
            }
        }
    }

    internal class EmptyTempDataProvider : ITempDataProvider
    {
        public void SaveTempData(ControllerContext controllerContext, IDictionary<string, object> values)
        {
        }

        public IDictionary<string, object> LoadTempData(ControllerContext controllerContext)
        {
            return new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }
    }
}
