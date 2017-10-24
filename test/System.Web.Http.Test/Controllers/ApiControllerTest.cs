// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Filters;
using System.Web.Http.Hosting;
using System.Web.Http.Metadata;
using System.Web.Http.ModelBinding;
using System.Web.Http.Routing;
using System.Web.Http.Services;
using System.Web.Http.Validation;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http
{
    public class ApiControllerTest
    {
        private readonly HttpActionContext _actionContextInstance = ContextUtil.CreateActionContext();
        private readonly HttpConfiguration _configurationInstance = new HttpConfiguration();
        private readonly HttpActionDescriptor _actionDescriptorInstance = new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

        [Fact]
        public async Task Setting_CustomActionInvoker()
        {
            // Arrange
            ApiController api = new UsersController();
            string responseText = "Hello World";
            HttpControllerContext controllerContext = ContextUtil.CreateControllerContext();

            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor(controllerContext.Configuration, "test", typeof(UsersController));
            controllerContext.ControllerDescriptor = controllerDescriptor;

            Mock<IHttpActionInvoker> mockInvoker = new Mock<IHttpActionInvoker>();
            mockInvoker
                .Setup(invoker => invoker.InvokeActionAsync(It.IsAny<HttpActionContext>(), It.IsAny<CancellationToken>()))
                .Returns(() =>
                {
                    TaskCompletionSource<HttpResponseMessage> tcs = new TaskCompletionSource<HttpResponseMessage>();
                    tcs.TrySetResult(new HttpResponseMessage() { Content = new StringContent(responseText) });
                    return tcs.Task;
                });
            controllerDescriptor.Configuration.Services.Replace(typeof(IHttpActionInvoker), mockInvoker.Object);

            // Act
            HttpResponseMessage message = await api.ExecuteAsync(
                controllerContext,
                CancellationToken.None);

            // Assert
            Assert.Equal(responseText, await message.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Setting_CustomActionSelector()
        {
            // Arrange
            var api = new UsersController();
            HttpControllerContext controllerContext = ContextUtil.CreateControllerContext(instance: api);

            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor(controllerContext.Configuration, "test", typeof(UsersController));
            controllerContext.ControllerDescriptor = controllerDescriptor;

            Mock<IHttpActionSelector> mockSelector = new Mock<IHttpActionSelector>();
            mockSelector
                .Setup(invoker => invoker.SelectAction(It.IsAny<HttpControllerContext>()))
                .Returns(() =>
                {
                    Func<HttpResponseMessage> testDelegate = api.Delete;
                    return new ReflectedHttpActionDescriptor
                    {
                        Configuration = controllerContext.Configuration,
                        ControllerDescriptor = controllerDescriptor,
                        MethodInfo = testDelegate.Method
                    };
                });
            controllerDescriptor.Configuration.Services.Replace(typeof(IHttpActionSelector), mockSelector.Object);

            // Act
            HttpResponseMessage message = await api.ExecuteAsync(
                controllerContext,
                CancellationToken.None);

            // Assert
            Assert.Equal("User Deleted", await message.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Default_Get()
        {
            // Arrange
            ApiController api = new UsersController();
            HttpControllerContext controllerContext = ContextUtil.CreateControllerContext(instance: api, request: new HttpRequestMessage() { Method = HttpMethod.Get });
            controllerContext.ControllerDescriptor = new HttpControllerDescriptor(controllerContext.Configuration, "test", typeof(UsersController));

            // Act
            HttpResponseMessage message = await api.ExecuteAsync(
                controllerContext,
                CancellationToken.None);

            // Assert
            Assert.Equal("Default User", await message.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Default_Post()
        {
            // Arrange
            ApiController api = new UsersController();
            HttpControllerContext controllerContext = ContextUtil.CreateControllerContext(instance: api, request: new HttpRequestMessage() { Method = HttpMethod.Post });
            controllerContext.ControllerDescriptor = new HttpControllerDescriptor(controllerContext.Configuration, "test", typeof(UsersController));

            // Act
            HttpResponseMessage message = await api.ExecuteAsync(
                controllerContext,
                CancellationToken.None);

            // Assert
            Assert.Equal("User Posted", await message.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Default_Put()
        {
            // Arrange
            ApiController api = new UsersController();
            HttpControllerContext controllerContext = ContextUtil.CreateControllerContext(instance: api, request: new HttpRequestMessage() { Method = HttpMethod.Put });
            controllerContext.ControllerDescriptor = new HttpControllerDescriptor(controllerContext.Configuration, "test", typeof(UsersController));

            // Act
            HttpResponseMessage message = await api.ExecuteAsync(
                controllerContext,
                CancellationToken.None);

            // Assert
            Assert.Equal("User Updated", await message.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Default_Delete()
        {
            // Arrange
            ApiController api = new UsersController();
            HttpControllerContext controllerContext = ContextUtil.CreateControllerContext(instance: api, request: new HttpRequestMessage() { Method = HttpMethod.Delete });
            controllerContext.ControllerDescriptor = new HttpControllerDescriptor(controllerContext.Configuration, "test", typeof(UsersController));

            // Act
            HttpResponseMessage message = await api.ExecuteAsync(
                controllerContext,
                CancellationToken.None);

            // Assert
            Assert.Equal("User Deleted", await message.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Route_ActionName()
        {
            // Arrange
            ApiController api = new UsersRpcController();
            HttpRouteData route = new HttpRouteData(new HttpRoute());
            route.Values.Add("action", "Admin");
            HttpControllerContext controllerContext = ContextUtil.CreateControllerContext(instance: api, routeData: route, request: new HttpRequestMessage() { Method = HttpMethod.Post });
            controllerContext.ControllerDescriptor = new HttpControllerDescriptor(controllerContext.Configuration, "test", typeof(UsersRpcController));

            // Act
            HttpResponseMessage message = await api.ExecuteAsync(controllerContext, CancellationToken.None);
            User user = await message.Content.ReadAsAsync<User>();

            // Assert
            Assert.Equal("Yao", user.FirstName);
            Assert.Equal("Huang", user.LastName);
        }

        [Fact]
        public async Task Route_Get_Action_With_Route_Parameters()
        {
            // Arrange
            ApiController api = new UsersRpcController();
            HttpRouteData route = new HttpRouteData(new HttpRoute());
            route.Values.Add("action", "EchoUser");
            route.Values.Add("firstName", "RouteFirstName");
            route.Values.Add("lastName", "RouteLastName");
            HttpControllerContext controllerContext = ContextUtil.CreateControllerContext(instance: api, routeData: route, request: new HttpRequestMessage() { Method = HttpMethod.Post });
            controllerContext.ControllerDescriptor = new HttpControllerDescriptor(controllerContext.Configuration, "test", typeof(UsersRpcController));

            // Act
            HttpResponseMessage message = await api.ExecuteAsync(controllerContext, CancellationToken.None);
            User user = await message.Content.ReadAsAsync<User>();

            // Assert
            Assert.Equal("RouteFirstName", user.FirstName);
            Assert.Equal("RouteLastName", user.LastName);
        }

        [Fact]
        public async Task Route_Get_Action_With_Query_Parameters()
        {
            // Arrange
            ApiController api = new UsersRpcController();
            HttpRouteData route = new HttpRouteData(new HttpRoute());
            route.Values.Add("action", "EchoUser");

            Uri requestUri = new Uri("http://localhost/?firstName=QueryFirstName&lastName=QueryLastName");
            HttpControllerContext controllerContext = ContextUtil.CreateControllerContext(instance: api, routeData: route, request: new HttpRequestMessage()
                {
                    Method = HttpMethod.Post,
                    RequestUri = requestUri
                });
            controllerContext.ControllerDescriptor = new HttpControllerDescriptor(controllerContext.Configuration, "test", typeof(UsersRpcController));

            // Act
            HttpResponseMessage message = await api.ExecuteAsync(controllerContext, CancellationToken.None);
            User user = await message.Content.ReadAsAsync<User>();

            // Assert
            Assert.Equal("QueryFirstName", user.FirstName);
            Assert.Equal("QueryLastName", user.LastName);
        }

        [Fact]
        public async Task Route_Post_Action_With_Content_Parameter()
        {
            // Arrange
            ApiController api = new UsersRpcController();
            HttpRouteData route = new HttpRouteData(new HttpRoute());
            route.Values.Add("action", "EchoUserObject");
            User postedUser = new User()
            {
                FirstName = "SampleFirstName",
                LastName = "SampleLastName"
            };

            HttpRequestMessage request = new HttpRequestMessage() { Method = HttpMethod.Post };

            // Create a serialized request because this test directly calls the controller
            // which would have normally been working with a serialized request content.
            string serializedUserAsString = null;
            using (HttpRequestMessage tempRequest = new HttpRequestMessage() { Content = new ObjectContent<User>(postedUser, new XmlMediaTypeFormatter()) })
            {
                serializedUserAsString = await tempRequest.Content.ReadAsStringAsync();
            }

            StringContent stringContent = new StringContent(serializedUserAsString);
            stringContent.Headers.ContentType = new MediaTypeHeaderValue("application/xml");
            request.Content = stringContent;

            HttpControllerContext controllerContext = ContextUtil.CreateControllerContext(instance: api, routeData: route, request: request);
            controllerContext.ControllerDescriptor = new HttpControllerDescriptor(controllerContext.Configuration, "test", typeof(UsersRpcController));

            // Act
            HttpResponseMessage message = await api.ExecuteAsync(
                controllerContext,
                CancellationToken.None);
            User user = await message.Content.ReadAsAsync<User>();

            // Assert
            Assert.Equal(postedUser.FirstName, user.FirstName);
            Assert.Equal(postedUser.LastName, user.LastName);
        }

        [Fact]
        public async Task Invalid_Action_In_Route()
        {
            // Arrange
            ApiController api = new UsersController();
            HttpRouteData route = new HttpRouteData(new HttpRoute());
            string actionName = "invalidOp";
            route.Values.Add("action", actionName);
            HttpControllerContext controllerContext = ContextUtil.CreateControllerContext(instance: api, routeData: route, request: new HttpRequestMessage() { Method = HttpMethod.Get });
            Type controllerType = typeof(UsersController);
            controllerContext.ControllerDescriptor = new HttpControllerDescriptor(controllerContext.Configuration, controllerType.Name, controllerType);
            controllerContext.Configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<HttpResponseException>(
                () => api.ExecuteAsync(controllerContext, CancellationToken.None));

            Assert.Equal(HttpStatusCode.NotFound, exception.Response.StatusCode);
            var content = Assert.IsType<ObjectContent<HttpError>>(exception.Response.Content);
            Assert.Equal("No action was found on the controller 'UsersController' that matches the name 'invalidOp'.",
                ((HttpError)content.Value)["MessageDetail"]);
        }

        [Fact]
        public async Task ExecuteAsync_InvokesAuthenticationFilters_ThenInvokesAuthorizationFilters_ThenInvokesModelBinding_ThenInvokesActionFilters_ThenInvokesAction()
        {
            List<string> log = new List<string>();
            Mock<ApiController> controllerMock = new Mock<ApiController>() { CallBase = true };
            var controllerContextMock = new Mock<HttpControllerContext>();

            Mock<IActionValueBinder> binderMock = new Mock<IActionValueBinder>();
            Mock<HttpActionBinding> actionBindingMock = new Mock<HttpActionBinding>();
            actionBindingMock.Setup(b => b.ExecuteBindingAsync(It.IsAny<HttpActionContext>(), It.IsAny<CancellationToken>())).Returns(() => Task.Factory.StartNew(() => { log.Add("model binding"); }));
            binderMock.Setup(b => b.GetBinding(It.IsAny<HttpActionDescriptor>())).Returns(actionBindingMock.Object);
            HttpConfiguration configuration = new HttpConfiguration();

            HttpControllerContext controllerContext = controllerContextMock.Object;
            controllerContext.Configuration = configuration;
            controllerContext.ControllerDescriptor = new HttpControllerDescriptor(configuration, "test", typeof(object));
            var actionFilterMock = CreateActionFilterMock((ac, ct, cont) =>
            {
                log.Add("action filters");
                return cont();
            });
            var authorizationFilterMock = CreateAuthorizationFilterMock((ac, ct, cont) =>
            {
                log.Add("authZ filters");
                return cont();
            });
            Mock<IAuthenticationFilter> authenticationFilterMock = new Mock<IAuthenticationFilter>();
            authenticationFilterMock.Setup(f => f.AuthenticateAsync(It.IsAny<HttpAuthenticationContext>(),
                It.IsAny<CancellationToken>())).Callback(() =>
                    {
                        log.Add("authN filters authenticate");
                    }).Returns(() => Task.FromResult<object>(null));
            IHttpActionResult innerResult = null;
            Mock<IHttpActionResult> challengeResultMock = new Mock<IHttpActionResult>();
            challengeResultMock.Setup(r => r.ExecuteAsync(It.IsAny<CancellationToken>())).Returns(async () =>
            {
                HttpResponseMessage response = await innerResult.ExecuteAsync(CancellationToken.None);
                log.Add("authN filters challenge");
                return response;
            });
            authenticationFilterMock.Setup(f => f.ChallengeAsync(It.IsAny<HttpAuthenticationChallengeContext>(),
                It.IsAny<CancellationToken>()))
                .Callback<HttpAuthenticationChallengeContext, CancellationToken>((c, t) =>
                {
                    innerResult = c.Result;
                    c.Result = challengeResultMock.Object;
                })
                .Returns(() => Task.FromResult<object>(null));

            var selectorMock = new Mock<IHttpActionSelector>();

            Mock<HttpActionDescriptor> actionDescriptorMock = new Mock<HttpActionDescriptor>();
            actionDescriptorMock.Setup(ad => ad.ActionBinding).Returns(actionBindingMock.Object);
            actionDescriptorMock.Setup(ad => ad.GetFilterPipeline())
                .Returns(new Collection<FilterInfo>(new List<FilterInfo>() { new FilterInfo(actionFilterMock.Object, FilterScope.Action), new FilterInfo(authorizationFilterMock.Object, FilterScope.Action), new FilterInfo(authenticationFilterMock.Object, FilterScope.Action) }));

            selectorMock.Setup(s => s.SelectAction(controllerContext)).Returns(actionDescriptorMock.Object);

            ApiController controller = controllerMock.Object;
            var invokerMock = new Mock<IHttpActionInvoker>();
            invokerMock.Setup(i => i.InvokeActionAsync(It.IsAny<HttpActionContext>(), It.IsAny<CancellationToken>()))
                       .Returns(() => Task.Factory.StartNew(() =>
                       {
                           log.Add("action");
                           return new HttpResponseMessage();
                       }));
            controllerContext.Configuration.Services.Replace(typeof(IHttpActionInvoker), invokerMock.Object);
            controllerContext.Configuration.Services.Replace(typeof(IHttpActionSelector), selectorMock.Object);
            controllerContext.Configuration.Services.Replace(typeof(IActionValueBinder), binderMock.Object);

            await controller.ExecuteAsync(controllerContext, CancellationToken.None);

            Assert.Equal(new string[] { "authN filters authenticate", "authZ filters", "model binding", "action filters", "action", "authN filters challenge" }, log.ToArray());
        }

        [Fact]
        public void GetFilters_QueriesFilterProvidersFromServices()
        {
            // Arrange
            Mock<DefaultServices> servicesMock = new Mock<DefaultServices> { CallBase = true };
            Mock<IFilterProvider> filterProviderMock = new Mock<IFilterProvider>();
            servicesMock.Setup(r => r.GetServices(typeof(IFilterProvider))).Returns(new object[] { filterProviderMock.Object }).Verifiable();
            _configurationInstance.Services = servicesMock.Object;

            HttpActionDescriptor actionDescriptorMock = new Mock<HttpActionDescriptor>() { CallBase = true }.Object;
            actionDescriptorMock.Configuration = _configurationInstance;

            // Act
            actionDescriptorMock.GetFilterPipeline();

            // Assert
            servicesMock.Verify();
        }

        [Fact]
        public void GetFilters_UsesFilterProvidersToGetFilters()
        {
            // Arrange
            Mock<DefaultServices> servicesMock = new Mock<DefaultServices> { CallBase = true };
            Mock<IFilterProvider> filterProviderMock = new Mock<IFilterProvider>();
            servicesMock.Setup(r => r.GetServices(typeof(IFilterProvider))).Returns(new[] { filterProviderMock.Object });
            _configurationInstance.Services = servicesMock.Object;

            HttpActionDescriptor actionDescriptorMock = new Mock<HttpActionDescriptor>() { CallBase = true }.Object;
            actionDescriptorMock.Configuration = _configurationInstance;

            // Act
            actionDescriptorMock.GetFilterPipeline().ToList();

            // Assert
            filterProviderMock.Verify(fp => fp.GetFilters(_configurationInstance, actionDescriptorMock));
        }

        [Fact]
        public void RequestPropertyGetterSetterWorks()
        {
            Assert.Reflection.Property(new Mock<ApiController>().Object,
                c => c.Request, expectedDefaultValue: null, allowNull: false,
                roundTripTestValue: new HttpRequestMessage());
        }

        [Fact]
        public void ConfigurationPropertyGetterSetterWorks()
        {
            Assert.Reflection.Property(new Mock<ApiController>().Object,
                c => c.Configuration, expectedDefaultValue: null, allowNull: false,
                roundTripTestValue: new HttpConfiguration());
        }

        [Fact]
        public void ModelStatePropertyGetterWorks()
        {
            // Arrange
            ApiController controller = new Mock<ApiController>().Object;

            // Act
            ModelStateDictionary expected = new ModelStateDictionary();
            expected.Add("a", new ModelState() { Value = new ValueProviders.ValueProviderResult("result", "attempted", CultureInfo.InvariantCulture) });

            controller.ModelState.Add("a", new ModelState() { Value = new ValueProviders.ValueProviderResult("result", "attempted", CultureInfo.InvariantCulture) });

            // Assert
            Assert.Equal(expected.Count, controller.ModelState.Count);
        }

        // TODO: Move these tests to ActionDescriptorTest
        [Fact]
        public void GetFilters_OrdersFilters()
        {
            // Arrange
            HttpActionDescriptor actionDescriptorMock = new Mock<HttpActionDescriptor>() { CallBase = true }.Object;
            actionDescriptorMock.Configuration = _configurationInstance;

            var globalFilter = new FilterInfo(new TestMultiFilter(), FilterScope.Global);
            var actionFilter = new FilterInfo(new TestMultiFilter(), FilterScope.Action);
            var controllerFilter = new FilterInfo(new TestMultiFilter(), FilterScope.Controller);
            Mock<DefaultServices> servicesMock = BuildFilterProvidingServicesMock(_configurationInstance, actionDescriptorMock, globalFilter, actionFilter, controllerFilter);
            _configurationInstance.Services = servicesMock.Object;

            // Act
            var result = actionDescriptorMock.GetFilterPipeline().ToArray();

            // Assert
            Assert.Equal(new[] { globalFilter, controllerFilter, actionFilter }, result);
        }

        [Fact]
        public void GetFilters_RemovesDuplicateUniqueFiltersKeepingMostSpecificScope()
        {
            // Arrange
            HttpActionDescriptor actionDescriptorMock = new Mock<HttpActionDescriptor>() { CallBase = true }.Object;
            actionDescriptorMock.Configuration = _configurationInstance;

            var multiActionFilter = new FilterInfo(new TestMultiFilter(), FilterScope.Action);
            var multiGlobalFilter = new FilterInfo(new TestMultiFilter(), FilterScope.Global);
            var uniqueControllerFilter = new FilterInfo(new TestUniqueFilter(), FilterScope.Controller);
            var uniqueActionFilter = new FilterInfo(new TestUniqueFilter(), FilterScope.Action);
            Mock<DefaultServices> servicesMock = BuildFilterProvidingServicesMock(
                _configurationInstance, actionDescriptorMock,
                multiActionFilter, multiGlobalFilter, uniqueControllerFilter, uniqueActionFilter);
            _configurationInstance.Services = servicesMock.Object;

            // Act
            var result = actionDescriptorMock.GetFilterPipeline().ToArray();

            // Assert
            Assert.Equal(new[] { multiGlobalFilter, multiActionFilter, uniqueActionFilter }, result);
        }

        [Fact]
        public Task ExecuteAsync_RunsExceptionFilter_WhenActionThrowsException()
        {
            // Arrange
            Exception expectedException = new NotImplementedException();
            ApiController controller = new ExceptionController(expectedException);

            // Act & Assert
            return TestExceptionFilterAsync(controller, expectedException, configure: null);
        }

        [Fact]
        public Task ExecuteAsync_RunsExceptionFilter_WhenActionFilterThrowsException()
        {
            // Arrange
            Exception expectedException = new NotImplementedException();
            ApiController controller = new ExceptionlessController();
            Mock<IActionFilter> filterMock = new Mock<IActionFilter>();
            filterMock.Setup(f => f.ExecuteActionFilterAsync(It.IsAny<HttpActionContext>(),
                It.IsAny<CancellationToken>(), It.IsAny<Func<Task<HttpResponseMessage>>>())).Callback(() =>
            {
                throw expectedException;
            });
            IActionFilter filter = filterMock.Object;

            // Act & Assert
            return TestExceptionFilterAsync(controller, expectedException, (configuration) =>
                { configuration.Filters.Add(filter); });
        }

        [Fact]
        public Task ExecuteAsync_RunsExceptionFilter_WhenAuthorizationFilterThrowsException()
        {
            // Arrange
            Exception expectedException = new NotImplementedException();
            ApiController controller = new ExceptionlessController();
            Mock<IAuthorizationFilter> filterMock = new Mock<IAuthorizationFilter>();
            filterMock.Setup(f => f.ExecuteAuthorizationFilterAsync(It.IsAny<HttpActionContext>(),
                It.IsAny<CancellationToken>(), It.IsAny<Func<Task<HttpResponseMessage>>>())).Callback(() =>
            {
                throw expectedException;
            });
            IAuthorizationFilter filter = filterMock.Object;

            // Act & Assert
            return TestExceptionFilterAsync(controller, expectedException, (configuration) =>
                { configuration.Filters.Add(filter); });
        }

        [Fact]
        public Task ExecuteAsync_RunsExceptionFilter_WhenAuthenticationFilterAuthenticateThrowsException()
        {
            // Arrange
            Exception expectedException = new NotImplementedException();
            ApiController controller = new ExceptionlessController();
            Mock<IAuthenticationFilter> filterMock = new Mock<IAuthenticationFilter>();
            filterMock.Setup(f => f.AuthenticateAsync(It.IsAny<HttpAuthenticationContext>(),
                It.IsAny<CancellationToken>())).Callback(() =>
                {
                    throw expectedException;
                });
            IAuthenticationFilter filter = filterMock.Object;

            // Act & Assert
            return TestExceptionFilterAsync(controller, expectedException, (configuration) =>
                { configuration.Filters.Add(filter); });
        }

        [Fact]
        public Task ExecuteAsync_RunsExceptionFilter_WhenAuthenticationFilterChallengeThrowsException()
        {
            // Arrange
            Exception expectedException = new NotImplementedException();
            ApiController controller = new ExceptionlessController();
            Mock<IAuthenticationFilter> filterMock = new Mock<IAuthenticationFilter>();
            filterMock.Setup(f => f.AuthenticateAsync(It.IsAny<HttpAuthenticationContext>(),
                It.IsAny<CancellationToken>())).Returns(() => Task.FromResult<object>(null));
            filterMock.Setup(f => f.ChallengeAsync(It.IsAny<HttpAuthenticationChallengeContext>(),
                It.IsAny<CancellationToken>())).Callback(() =>
                {
                    throw expectedException;
                });
            IAuthenticationFilter filter = filterMock.Object;

            // Act & Assert
            return TestExceptionFilterAsync(controller, expectedException, (configuration) =>
                { configuration.Filters.Add(filter); });
        }

        private static async Task TestExceptionFilterAsync(ApiController controller, Exception expectedException,
            Action<HttpConfiguration> configure)
        {
            // Arrange
            Exception actualException = null;
            IHttpRouteData routeData = new HttpRouteData(new HttpRoute());

            using (HttpRequestMessage request = new HttpRequestMessage())
            using (HttpConfiguration configuration = new HttpConfiguration())
            using (HttpResponseMessage response = new HttpResponseMessage())
            {
                HttpControllerContext context = new HttpControllerContext(configuration, routeData, request);
                HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor(configuration,
                    "Ignored", controller.GetType());
                context.Controller = controller;
                context.ControllerDescriptor = controllerDescriptor;

                if (configure != null)
                {
                    configure.Invoke(configuration);
                }

                Mock<IExceptionFilter> spy = new Mock<IExceptionFilter>();
                spy.Setup(f => f.ExecuteExceptionFilterAsync(It.IsAny<HttpActionExecutedContext>(),
                    It.IsAny<CancellationToken>())).Returns<HttpActionExecutedContext, CancellationToken>(
                    (c, i) =>
                    {
                        actualException = c.Exception;
                        c.Response = response;
                        return Task.FromResult<object>(null);
                    });

                configuration.Filters.Add(spy.Object);

                // Act
                HttpResponseMessage ignore = await controller.ExecuteAsync(context, CancellationToken.None);
            }

            // Assert
            Assert.Same(expectedException, actualException);
        }

        [Fact]
        public async Task ExecuteAsync_IfActionThrows_CallsExceptionServicesFromConfiguration()
        {
            List<string> log = new List<string>();
            Exception expectedException = new Exception();
            ExceptionController controller = new ExceptionController(expectedException);

            Mock<IExceptionLogger> exceptionLoggerMock = new Mock<IExceptionLogger>(MockBehavior.Strict);
            exceptionLoggerMock
                .Setup(h => h.LogAsync(It.IsAny<ExceptionLoggerContext>(), It.IsAny<CancellationToken>()))
                .Returns<ExceptionLoggerContext, CancellationToken>((c, i) =>
                {
                    log.Add("logger");
                    return Task.FromResult(0);
                });
            IExceptionLogger exceptionLogger = exceptionLoggerMock.Object;

            Mock<IExceptionHandler> exceptionHandlerMock = new Mock<IExceptionHandler>(MockBehavior.Strict);
            exceptionHandlerMock
                .Setup(h => h.HandleAsync(It.IsAny<ExceptionHandlerContext>(), It.IsAny<CancellationToken>()))
                .Returns<ExceptionHandlerContext, CancellationToken>((c, i) =>
                {
                    log.Add("handler");
                    return Task.FromResult(0);
                });
            IExceptionHandler exceptionHandler = exceptionHandlerMock.Object;

            HttpControllerContext controllerContext = ContextUtil.CreateControllerContext();

            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor(
                controllerContext.Configuration, "Get", typeof(ExceptionController));
            controllerContext.ControllerDescriptor = controllerDescriptor;
            controllerContext.Controller = controller;
            controllerContext.Configuration.Services.Add(typeof(IExceptionLogger), exceptionLogger);
            controllerContext.Configuration.Services.Replace(typeof(IExceptionHandler), exceptionHandler);
            controllerContext.Configuration.Filters.Add(CreateStubExceptionFilter());

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => controller.ExecuteAsync(controllerContext, CancellationToken.None));

            Assert.Same(expectedException, exception);
            Assert.Equal(new string[] { "logger", "handler" }, log.ToArray());
        }

        [Fact]
        public async Task ApiControllerCannotBeReused()
        {
            // Arrange
            var config = new HttpConfiguration() { IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always };
            var singletonController = new Mock<ApiController> { CallBase = true }.Object;
            var mockDescriptor = new Mock<HttpControllerDescriptor>(config, "MyMock", typeof(ApiController)) { CallBase = true };
            mockDescriptor.Setup(d => d.CreateController(It.IsAny<HttpRequestMessage>())).Returns(singletonController);
            var mockSelector = new Mock<DefaultHttpControllerSelector>(config) { CallBase = true };
            mockSelector.Setup(s => s.SelectController(It.IsAny<HttpRequestMessage>())).Returns(mockDescriptor.Object);
            config.Routes.MapHttpRoute("default", "", new { controller = "MyMock" });
            config.Services.Replace(typeof(IHttpControllerSelector), mockSelector.Object);
            var server = new HttpServer(config);
            var invoker = new HttpMessageInvoker(server);

            // Act
            HttpResponseMessage response1 = await invoker.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://localhost/"), CancellationToken.None);
            HttpResponseMessage response2 = await invoker.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://localhost/"), CancellationToken.None);

            // Assert
            Assert.NotEqual(HttpStatusCode.InternalServerError, response1.StatusCode);
            Assert.Equal(HttpStatusCode.InternalServerError, response2.StatusCode);
            Assert.Contains("Cannot reuse an 'ApiController' instance. 'ApiController' has to be constructed per incoming message.", await response2.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task ApiControllerPutsSelfInRequestResourcesToBeDisposed()
        {
            // Arrange
            var config = new HttpConfiguration();
            config.Routes.MapHttpRoute("default", "", new { controller = "SpyDispose" });
            var server = new HttpServer(config);
            var invoker = new HttpMessageInvoker(server);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/");
            await invoker.SendAsync(request, CancellationToken.None);

            // Act
            request.DisposeRequestResources();

            // Assert
            Assert.True(SpyDisposeController.DisposeWasCalled);
        }

        [Fact]
        public void ControllerContextDefault_IsNonNull()
        {
            // Arrange
            ApiController controller = CreateFakeController();

            // Act
            HttpControllerContext context = controller.ControllerContext;

            // Assert
            Assert.NotNull(context);
        }

        [Fact]
        public void RequestGet_ReturnsControllerContextRequest()
        {
            // Arrange
            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                ApiController controller = CreateFakeController();
                controller.ControllerContext = new HttpControllerContext
                {
                    Request = expectedRequest
                };

                // Act
                HttpRequestMessage request = controller.Request;

                // Assert
                Assert.Same(expectedRequest, request);
            }
        }

        [Fact]
        public void RequestSet_Throws_WhenNull()
        {
            // Arrange
            ApiController controller = CreateFakeController();

            // Act & Assert
            Assert.ThrowsArgumentNull(() => { controller.Request = null; }, "value");
        }

        [Fact]
        public void RequestSet_UpdatesControllerContextRequest()
        {
            // Arrange
            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                ApiController controller = CreateFakeController();
                HttpControllerContext controllerContext = CreateControllerContext();
                controller.ControllerContext = controllerContext;

                // Act
                controller.Request = expectedRequest;

                // Assert
                Assert.Same(expectedRequest, controllerContext.Request);
            }
        }

        [Fact]
        public void RequestSet_UpdatesRequestRequestContext()
        {
            // Arrange
            using (HttpRequestMessage request = CreateRequest())
            {
                ApiController controller = CreateFakeController();
                HttpRequestContext expectedRequestContext = CreateRequestContext();
                controller.RequestContext = expectedRequestContext;

                // Act
                controller.Request = request;

                // Assert
                Assert.Same(expectedRequestContext, request.GetRequestContext());
            }
        }

        [Fact]
        public void RequestSet_Throws_WhenRequestHasConflictingRequestContext()
        {
            // Arrange
            using (HttpRequestMessage request = CreateRequest())
            {
                ApiController controller = CreateFakeController();
                HttpRequestContext otherRequestContext = CreateRequestContext();
                request.SetRequestContext(otherRequestContext);
                Assert.NotSame(controller.RequestContext, otherRequestContext); // Guard

                // Act & Assert
                Assert.Throws<InvalidOperationException>(() => { controller.Request = request; },
                    "The request context property on the request must be null or match ApiController.RequestContext.");
            }
        }

        [Fact]
        public void RequestSet_UpdatesRequestRequestContext_WhenRequestHasNoContext()
        {
            // Arrange
            using (HttpRequestMessage request = CreateRequest())
            {
                ApiController controller = CreateFakeController();
                HttpRequestContext expectedRequestContext = CreateRequestContext();
                controller.RequestContext = expectedRequestContext;
                Assert.Null(request.GetRequestContext()); // Guard

                // Act
                controller.Request = request;

                // Assert
                Assert.Same(expectedRequestContext, request.GetRequestContext());
            }
        }

        [Fact]
        public void RequestSet_WithConfigurationPropertyAndDefaultRequestContext_UpdatesConfiguration()
        {
            // Arrange
            using (HttpRequestMessage request = CreateRequest())
            using (HttpConfiguration expectedConfiguration = new HttpConfiguration())
            {
                ApiController controller = CreateFakeController();
                request.SetConfiguration(expectedConfiguration);
                controller.Request = request;

                // Act
                HttpConfiguration configuration = controller.Configuration;

                // Assert
                Assert.Same(expectedConfiguration, configuration);
            }
        }

        [Fact]
        public void RequestContextSet_LeavesRequestRequestContextUnchanged_WhenRequestHasControllerRequestContext()
        {
            // Arrange
            using (HttpRequestMessage request = CreateRequest())
            {
                ApiController controller = CreateFakeController();
                HttpRequestContext expectedRequestContext = controller.RequestContext;
                request.SetRequestContext(expectedRequestContext);

                // Act
                controller.Request = request;

                // Assert
                HttpRequestContext requestContext = request.GetRequestContext();
                Assert.Same(expectedRequestContext, requestContext);
            }
        }

        [Fact]
        public void RequestSet_UpdatesRequestBackedContextRequest()
        {
            // Arrange
            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                ApiController controller = CreateFakeController();
                RequestBackedHttpRequestContext context = Assert.IsType<RequestBackedHttpRequestContext>(controller.RequestContext); // Guard

                // Act
                controller.Request = expectedRequest;

                // Assert
                Assert.Same(expectedRequest, context.Request);
            }
        }

        [Fact]
        public void RequestContextGet_ReturnsControllerContextRequestContext()
        {
            // Arrange
            HttpControllerContext controllerContext = CreateControllerContext();
            HttpRequestContext expectedRequestContext = CreateRequestContext();
            controllerContext.RequestContext = expectedRequestContext;
            ApiController controller = CreateFakeController();
            controller.ControllerContext = controllerContext;

            // Act
            HttpRequestContext requestContext = controller.RequestContext;

            // Assert
            Assert.Same(expectedRequestContext, requestContext);
        }

        [Fact]
        public void RequestContextSet_Throws_WhenNull()
        {
            // Arrange
            ApiController controller = CreateFakeController();

            // Act & Assert
            Assert.ThrowsArgumentNull(() => { controller.RequestContext = null; }, "value");
        }

        [Fact]
        public void RequestContextSet_UpdatesControllerContextRequestContext()
        {
            // Arrange
            ApiController controller = CreateFakeController();
            HttpControllerContext controllerContext = CreateControllerContext();
            controller.ControllerContext = controllerContext;
            HttpRequestContext expectedRequestContext = CreateRequestContext();

            // Act
            controller.RequestContext = expectedRequestContext;

            // Assert
            Assert.Same(expectedRequestContext, controllerContext.RequestContext);
        }

        [Fact]
        public void RequestContextSet_UpdatesRequestRequestContext_WhenRequestIsPresent()
        {
            // Arrange
            using (HttpRequestMessage request = CreateRequest())
            {
                ApiController controller = CreateFakeController();
                HttpRequestContext expectedRequestContext = CreateRequestContext();
                controller.Request = request;

                // Act
                controller.RequestContext = expectedRequestContext;

                // Assert
                HttpRequestContext requestContext = request.GetRequestContext();
                Assert.Same(expectedRequestContext, requestContext);
            }
        }

        [Fact]
        public void RequestContextSet_Throws_WhenRequestIsPresentWithConflictingRequestContext()
        {
            // Arrange
            using (HttpRequestMessage request = CreateRequest())
            {
                ApiController controller = CreateFakeController();
                HttpRequestContext otherRequestContext = CreateRequestContext();
                controller.Request = request;
                request.SetRequestContext(otherRequestContext);
                HttpRequestContext requestContext = CreateRequestContext();

                // Act & Assert
                Assert.Throws<InvalidOperationException>(() => { controller.RequestContext = requestContext; },
                    "The request context property on the request must be null or match ApiController.RequestContext.");
            }
        }

        [Fact]
        public void RequestContextSet_UpdatesRequestRequestContext_WhenRequestIsPresentWithNoContext()
        {
            // Arrange
            using (HttpRequestMessage request = CreateRequest())
            {
                ApiController controller = CreateFakeController();
                HttpRequestContext expectedRequestContext = CreateRequestContext();
                controller.Request = request;
                request.Properties.Remove(HttpPropertyKeys.RequestContextKey);

                // Act
                controller.RequestContext = expectedRequestContext;

                // Assert
                HttpRequestContext requestContext = request.GetRequestContext();
                Assert.Same(expectedRequestContext, requestContext);
            }
        }

        [Fact]
        public void RequestContextSet_UpdatesRequestRequestContext_WhenRequestIsPresentWithExistingContext()
        {
            // Arrange
            using (HttpRequestMessage request = CreateRequest())
            {
                ApiController controller = CreateFakeController();
                HttpRequestContext expectedRequestContext = CreateRequestContext();
                controller.Request = request;
                request.SetRequestContext(controller.RequestContext);

                // Act
                controller.RequestContext = expectedRequestContext;

                // Assert
                HttpRequestContext requestContext = request.GetRequestContext();
                Assert.Same(expectedRequestContext, requestContext);
            }
        }

        [Fact]
        public void RequestContextSet_LeavesRequestRequestContextUnchanged_WhenRequestIsPresentWithNewContext()
        {
            // Arrange
            using (HttpRequestMessage request = CreateRequest())
            {
                ApiController controller = CreateFakeController();
                HttpRequestContext expectedRequestContext = CreateRequestContext();
                controller.Request = request;
                request.SetRequestContext(expectedRequestContext);

                // Act
                controller.RequestContext = expectedRequestContext;

                // Assert
                HttpRequestContext requestContext = request.GetRequestContext();
                Assert.Same(expectedRequestContext, requestContext);
            }
        }

        [Fact]
        public void RequestContextDefault_IsRequestBacked()
        {
            // Arrange
            ApiController controller = CreateFakeController();

            // Act
            HttpRequestContext context = controller.RequestContext;

            // Assert
            Assert.IsType<RequestBackedHttpRequestContext>(context);
        }

        [Fact]
        public void UserGet_ReturnsContextPrincipal()
        {
            // Arrange
            ApiController controller = CreateFakeController();
            IPrincipal expectedPrincipal = CreateDummyPrincipal();
            controller.RequestContext = new HttpRequestContext
            {
                Principal = expectedPrincipal
            };

            // Act
            IPrincipal principal = controller.User;

            // Assert
            Assert.Same(expectedPrincipal, principal);
        }

        [Fact]
        public void UserSet_UpdatesContextPrincipal()
        {
            // Arrange
            ApiController controller = CreateFakeController();
            IPrincipal expectedPrincipal = CreateDummyPrincipal();
            HttpRequestContext context = new HttpRequestContext();
            controller.RequestContext = context;

            // Act
            controller.User = expectedPrincipal;

            // Assert
            Assert.Same(expectedPrincipal, context.Principal);
        }

        [Fact]
        public void Validate_ThrowsInvalidOperationException_IfConfigurationIsNull()
        {
            // Arrange
            TestController controller = new TestController();
            TestEntity entity = new TestEntity();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => controller.Validate(entity),
                "ApiController.Configuration must not be null.");
        }

        [Fact]
        public void Validate_DoesNothing_IfValidatorIsNull()
        {
            // Arrange
            HttpConfiguration configuration = new HttpConfiguration();
            configuration.Services.Replace(typeof(IBodyModelValidator), null);
            TestEntity entity = new TestEntity { ID = 9999999 };

            TestController controller = new TestController { Configuration = configuration };

            // Act
            controller.Validate(entity);

            // Assert
            Assert.True(controller.ModelState.IsValid);
        }

        [Fact]
        [ReplaceCulture]
        public void Validate_CallsValidateOnConfiguredValidator_UsingConfiguredMetadataProvider()
        {
            // Arrange
            Mock<IBodyModelValidator> validator = new Mock<IBodyModelValidator>();
            Mock<ModelMetadataProvider> metadataProvider = new Mock<ModelMetadataProvider>();

            HttpConfiguration configuration = new HttpConfiguration();
            configuration.Services.Replace(typeof(IBodyModelValidator), validator.Object);
            configuration.Services.Replace(typeof(ModelMetadataProvider), metadataProvider.Object);

            TestController controller = new TestController { Configuration = configuration };
            TestEntity entity = new TestEntity { ID = 42 };

            // Act
            controller.Validate(entity);

            // Assert
            validator.Verify(
                v => v.Validate(entity, typeof(TestEntity), metadataProvider.Object, controller.ActionContext, String.Empty),
                Times.Once());
            Assert.True(controller.ModelState.IsValid);
        }

        [Fact]
        [ReplaceCulture]
        public void Validate_SetsModelStateErrors_ForInvalidModels()
        {
            // Arrange
            HttpConfiguration configuration = new HttpConfiguration();
            TestController controller = new TestController { Configuration = configuration };
            TestEntity entity = new TestEntity { ID = -1 };

            // Act
            controller.Validate(entity);

            // Assert
            Assert.False(controller.ModelState.IsValid);
            Assert.Equal("The field ID must be between 0 and 100.", controller.ModelState["ID"].Errors[0].ErrorMessage);
        }

        [Fact]
        [ReplaceCulture]
        public void Validate_SetsModelStateErrorsUnderRightPrefix_ForInvalidModels()
        {
            // Arrange
            HttpConfiguration configuration = new HttpConfiguration();
            TestController controller = new TestController { Configuration = configuration };
            TestEntity entity = new TestEntity { ID = -1 };

            // Act
            controller.Validate(entity, keyPrefix: "prefix");

            // Assert
            Assert.False(controller.ModelState.IsValid);
            Assert.Equal("The field ID must be between 0 and 100.",
                controller.ModelState["prefix.ID"].Errors[0].ErrorMessage);
        }

        [Fact]
        public void Validate_DoesNotThrow_ForValidModels()
        {
            // Arrange
            HttpConfiguration configuration = new HttpConfiguration();
            TestController controller = new TestController { Configuration = configuration };
            TestEntity entity = new TestEntity { ID = 42 };

            // Act && Assert
            Assert.DoesNotThrow(() => controller.Validate(entity));
        }

        private class TestController : ApiController
        {
        }

        private class TestEntity
        {
            [Range(0, 100)]
            public int ID { get; set; }
        }

        private Mock<IAuthorizationFilter> CreateAuthorizationFilterMock(Func<HttpActionContext, CancellationToken, Func<Task<HttpResponseMessage>>, Task<HttpResponseMessage>> implementation)
        {
            Mock<IAuthorizationFilter> filterMock = new Mock<IAuthorizationFilter>();
            filterMock.Setup(f => f.ExecuteAuthorizationFilterAsync(It.IsAny<HttpActionContext>(),
                                                                    CancellationToken.None,
                                                                    It.IsAny<Func<Task<HttpResponseMessage>>>()))
                      .Returns(implementation)
                      .Verifiable();
            return filterMock;
        }

        private Mock<IActionFilter> CreateActionFilterMock(Func<HttpActionContext, CancellationToken, Func<Task<HttpResponseMessage>>, Task<HttpResponseMessage>> implementation)
        {
            Mock<IActionFilter> filterMock = new Mock<IActionFilter>();
            filterMock.Setup(f => f.ExecuteActionFilterAsync(It.IsAny<HttpActionContext>(),
                                                             CancellationToken.None,
                                                             It.IsAny<Func<Task<HttpResponseMessage>>>()))
                      .Returns(implementation)
                      .Verifiable();
            return filterMock;
        }

        private static HttpControllerContext CreateControllerContext()
        {
            return new HttpControllerContext();
        }

        private static IPrincipal CreateDummyPrincipal()
        {
            return new Mock<IPrincipal>(MockBehavior.Strict).Object;
        }

        private static ApiController CreateFakeController()
        {
            return new ExceptionlessController();
        }

        private static HttpRequestMessage CreateRequest()
        {
            return new HttpRequestMessage();
        }

        private static HttpRequestContext CreateRequestContext()
        {
            return new HttpRequestContext();
        }

        private static IExceptionFilter CreateStubExceptionFilter()
        {
            Mock<IExceptionFilter> mock = new Mock<IExceptionFilter>(MockBehavior.Strict);
            mock
                .Setup(f => f.ExecuteExceptionFilterAsync(It.IsAny<HttpActionExecutedContext>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(0));
            return mock.Object;
        }

        private static Mock<DefaultServices> BuildFilterProvidingServicesMock(HttpConfiguration configuration, HttpActionDescriptor action, params FilterInfo[] filters)
        {
            var servicesMock = new Mock<DefaultServices> { CallBase = true };
            var filterProviderMock = new Mock<IFilterProvider>();
            servicesMock.Setup(r => r.GetServices(typeof(IFilterProvider))).Returns(new[] { filterProviderMock.Object });
            filterProviderMock.Setup(fp => fp.GetFilters(configuration, action)).Returns(filters);
            return servicesMock;
        }

        public class ExceptionController : ApiController
        {
            private readonly Exception _exception;

            public ExceptionController(Exception exception)
            {
                _exception = exception;
            }

            public void Get()
            {
                throw _exception;
            }
        }

        public class ExceptionlessController : ApiController
        {
            public void Get()
            {
            }
        }

        public class SpyDisposeController : ApiController
        {
            public static bool DisposeWasCalled = false;

            public SpyDisposeController()
            {
            }

            public void Get() { }

            protected override void Dispose(bool disposing)
            {
                DisposeWasCalled = true;
                base.Dispose(disposing);
            }
        }

        /// <summary>
        /// Simple IFilter implementation with AllowMultiple = true
        /// </summary>
        public class TestMultiFilter : IFilter
        {
            public bool AllowMultiple
            {
                get { return true; }
            }
        }

        /// <summary>
        /// Simple IFilter implementation with AllowMultiple = false
        /// </summary>
        public class TestUniqueFilter : IFilter
        {
            public bool AllowMultiple
            {
                get { return false; }
            }
        }
    }
}
