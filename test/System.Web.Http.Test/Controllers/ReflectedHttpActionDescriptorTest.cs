// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http
{
    public class ReflectedHttpActionDescriptorTest
    {
        private readonly UsersRpcController _controller = new UsersRpcController();
        private readonly HttpControllerContext _context;
        private readonly Dictionary<string, object> _arguments = new Dictionary<string, object>();

        public ReflectedHttpActionDescriptorTest()
        {
            _context = ContextUtil.CreateControllerContext(instance: _controller);
            _context.Configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
        }

        [Fact]
        public void Default_Constructor()
        {
            ReflectedHttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor();

            Assert.Null(actionDescriptor.ActionName);
            Assert.Null(actionDescriptor.Configuration);
            Assert.Null(actionDescriptor.ControllerDescriptor);
            Assert.Null(actionDescriptor.MethodInfo);
            Assert.Null(actionDescriptor.ReturnType);
            Assert.NotNull(actionDescriptor.Properties);
        }

        [Fact]
        public void Parameter_Constructor()
        {
            Func<string, string, User> echoUserMethod = _controller.EchoUser;
            HttpConfiguration config = new HttpConfiguration();
            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor(config, "", typeof(UsersRpcController));
            ReflectedHttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor(controllerDescriptor, echoUserMethod.Method);

            Assert.Equal("EchoUser", actionDescriptor.ActionName);
            Assert.Equal(config, actionDescriptor.Configuration);
            Assert.Equal(typeof(UsersRpcController), actionDescriptor.ControllerDescriptor.ControllerType);
            Assert.Equal(echoUserMethod.Method, actionDescriptor.MethodInfo);
            Assert.Equal(typeof(User), actionDescriptor.ReturnType);
            Assert.NotNull(actionDescriptor.Properties);
        }

        [Fact]
        public void Constructor_Throws_IfMethodInfoIsNull()
        {
            Assert.ThrowsArgumentNull(
                () => new ReflectedHttpActionDescriptor(new HttpControllerDescriptor(), null),
                "methodInfo");
        }

        [Fact]
        public void MethodInfo_Property()
        {
            ReflectedHttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor();
            Action action = new Action(() => { });

            Assert.Reflection.Property<ReflectedHttpActionDescriptor, MethodInfo>(
                 instance: actionDescriptor,
                 propertyGetter: ad => ad.MethodInfo,
                 expectedDefaultValue: null,
                 allowNull: false,
                 roundTripTestValue: action.Method);
        }

        [Fact]
        public void ControllerDescriptor_Property()
        {
            ReflectedHttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor();
            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor();

            Assert.Reflection.Property<ReflectedHttpActionDescriptor, HttpControllerDescriptor>(
                 instance: actionDescriptor,
                 propertyGetter: ad => ad.ControllerDescriptor,
                 expectedDefaultValue: null,
                 allowNull: false,
                 roundTripTestValue: controllerDescriptor);
        }

        [Fact]
        public void Configuration_Property()
        {
            ReflectedHttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor();
            HttpConfiguration config = new HttpConfiguration();

            Assert.Reflection.Property<ReflectedHttpActionDescriptor, HttpConfiguration>(
                 instance: actionDescriptor,
                 propertyGetter: ad => ad.Configuration,
                 expectedDefaultValue: null,
                 allowNull: false,
                 roundTripTestValue: config);
        }

        [Fact]
        public void GetFilter_Returns_AttributedFilter()
        {
            Func<string, string, User> echoUserMethod = _controller.AddAdmin;
            ReflectedHttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor { MethodInfo = echoUserMethod.Method };
            _arguments["firstName"] = "test";
            _arguments["lastName"] = "unit";

            IEnumerable<IFilter> filters = actionDescriptor.GetFilters();

            Assert.NotNull(filters);
            IFilter filter = Assert.Single(filters);
            Assert.IsType<AuthorizeAttribute>(filter);
        }

        [Fact]
        public void GetFilterPipeline_Returns_ConfigurationFilters()
        {
            IActionFilter actionFilter = new Mock<IActionFilter>().Object;
            IExceptionFilter exceptionFilter = new Mock<IExceptionFilter>().Object;
            IAuthorizationFilter authorizationFilter = new AuthorizeAttribute();
            Action deleteAllUsersMethod = _controller.DeleteAllUsers;

            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor(new HttpConfiguration(), "UsersRpcController", typeof(UsersRpcController));
            controllerDescriptor.Configuration.Filters.Add(actionFilter);
            controllerDescriptor.Configuration.Filters.Add(exceptionFilter);
            controllerDescriptor.Configuration.Filters.Add(authorizationFilter);
            ReflectedHttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor(controllerDescriptor, deleteAllUsersMethod.Method);

            Collection<FilterInfo> filters = actionDescriptor.GetFilterPipeline();

            Assert.Same(actionFilter, filters[0].Instance);
            Assert.Same(exceptionFilter, filters[1].Instance);
            Assert.Same(authorizationFilter, filters[2].Instance);
        }

        [Fact]
        public void GetCustomAttributes_Returns_ActionAttributes()
        {
            Func<string, string, User> echoUserMethod = _controller.AddAdmin;
            ReflectedHttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor { MethodInfo = echoUserMethod.Method };

            IEnumerable<IFilter> filters = actionDescriptor.GetCustomAttributes<IFilter>();
            IEnumerable<HttpGetAttribute> httpGet = actionDescriptor.GetCustomAttributes<HttpGetAttribute>();

            Assert.NotNull(filters);
            IFilter filter = Assert.Single(filters);
            Assert.IsType<AuthorizeAttribute>(filter);
            Assert.NotNull(httpGet);
            Assert.Single(httpGet);
        }

        [Fact]
        public void GetParameters_Returns_ActionParameters()
        {
            Func<string, string, User> echoUserMethod = _controller.EchoUser;
            ReflectedHttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor { MethodInfo = echoUserMethod.Method };

            Collection<HttpParameterDescriptor> parameterDescriptors = actionDescriptor.GetParameters();

            Assert.Equal(2, parameterDescriptors.Count);
            Assert.Contains(parameterDescriptors, p => p.ParameterName == "firstName");
            Assert.Contains(parameterDescriptors, p => p.ParameterName == "lastName");
        }

        [Fact]
        public Task ExecuteAsync_DoesNotCallActionWhenCancelled()
        {
            var cts = new CancellationTokenSource();
            cts.Cancel();
            Action action = () => { throw new NotImplementedException(); };
            ReflectedHttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor { MethodInfo = action.Method };

            return Assert.ThrowsAsync<TaskCanceledException>(() => actionDescriptor.ExecuteAsync(_context, _arguments, cts.Token));
        }


        [Fact]
        public async Task ExecuteAsync_Returns_TaskOfNull_ForVoidAction()
        {
            Action deleteAllUsersMethod = _controller.DeleteAllUsers;
            ReflectedHttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor { MethodInfo = deleteAllUsersMethod.Method };

            object returnValue = await actionDescriptor.ExecuteAsync(_context, _arguments, CancellationToken.None);

            Assert.Null(returnValue);
        }

        [Fact]
        public async Task ExecuteAsync_Returns_Results_ForNonVoidAction()
        {
            Func<string, string, User> echoUserMethod = _controller.EchoUser;
            ReflectedHttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor { MethodInfo = echoUserMethod.Method };
            _arguments["firstName"] = "test";
            _arguments["lastName"] = "unit";

            object result = await actionDescriptor.ExecuteAsync(_context, _arguments, CancellationToken.None);

            var returnValue = Assert.IsType<User>(result);
            Assert.Equal("test", returnValue.FirstName);
            Assert.Equal("unit", returnValue.LastName);
        }

        [Fact]
        public async Task ExecuteAsync_Returns_TaskOfNull_ForTaskAction()
        {
            Func<Task> deleteAllUsersMethod = _controller.DeleteAllUsersAsync;
            ReflectedHttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor { MethodInfo = deleteAllUsersMethod.Method };

            object returnValue = await actionDescriptor.ExecuteAsync(_context, _arguments, CancellationToken.None);

            Assert.Null(returnValue);
        }

        [Fact]
        public async Task ExecuteAsync_Returns_Results_ForTaskOfTAction()
        {
            Func<string, string, Task<User>> echoUserMethod = _controller.EchoUserAsync;
            ReflectedHttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor { MethodInfo = echoUserMethod.Method };
            _arguments["firstName"] = "test";
            _arguments["lastName"] = "unit";

            object result = await actionDescriptor.ExecuteAsync(_context, _arguments, CancellationToken.None);

            var returnValue = Assert.IsType<User>(result);
            Assert.Equal("test", returnValue.FirstName);
            Assert.Equal("unit", returnValue.LastName);
        }

        [Fact]
        public void ExecuteAsync_Throws_IfContextIsNull()
        {
            Func<string, string, User> echoUserMethod = _controller.EchoUser;
            ReflectedHttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor { MethodInfo = echoUserMethod.Method };

            Assert.ThrowsArgumentNull(
                () => actionDescriptor.ExecuteAsync(null, _arguments, CancellationToken.None),
                "controllerContext");
        }

        [Fact]
        public async Task ExecuteAsync_Throws_IfArgumentsIsNull()
        {
            Func<string, string, User> echoUserMethod = _controller.EchoUser;
            ReflectedHttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor { MethodInfo = echoUserMethod.Method };

            await Assert.ThrowsAsync<ArgumentNullException>(
                () => actionDescriptor.ExecuteAsync(_context, null, CancellationToken.None),
                "arguments",
                partialMatch: true);
        }

        [Fact]
        public async Task ExecuteAsync_Throws_IfValueTypeArgumentsIsNull()
        {
            Func<int, User> retrieveUserMethod = _controller.RetriveUser;
            ReflectedHttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor { MethodInfo = retrieveUserMethod.Method };
            _arguments["id"] = null;

            var exception = await Assert.ThrowsAsync<HttpResponseException>(
                 () => actionDescriptor.ExecuteAsync(_context, _arguments, CancellationToken.None));

            Assert.Equal(HttpStatusCode.BadRequest, exception.Response.StatusCode);
            var content = Assert.IsType<ObjectContent<HttpError>>(exception.Response.Content);
            Assert.Equal("The parameters dictionary contains a null entry for parameter 'id' of non-nullable type 'System.Int32' " +
                "for method 'System.Web.Http.User RetriveUser(Int32)' in 'System.Web.Http.UsersRpcController'. An optional parameter " +
                "must be a reference type, a nullable type, or be declared as an optional parameter.",
                ((HttpError)content.Value)["MessageDetail"]);
        }

        [Fact]
        public async Task ExecuteAsync_Throws_IfArgumentNameIsWrong()
        {
            Func<int, User> retrieveUserMethod = _controller.RetriveUser;
            ReflectedHttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor { MethodInfo = retrieveUserMethod.Method };
            _arguments["otherId"] = 6;

            var exception = await Assert.ThrowsAsync<HttpResponseException>(
                () => actionDescriptor.ExecuteAsync(_context, _arguments, CancellationToken.None));

            Assert.Equal(HttpStatusCode.BadRequest, exception.Response.StatusCode);
            var content = Assert.IsType<ObjectContent<HttpError>>(exception.Response.Content);
            Assert.Equal("The parameters dictionary does not contain an entry for parameter 'id' of type 'System.Int32' " +
                "for method 'System.Web.Http.User RetriveUser(Int32)' in 'System.Web.Http.UsersRpcController'. " +
                "The dictionary must contain an entry for each parameter, including parameters that have null values.",
                ((HttpError)content.Value)["MessageDetail"]);
        }

        [Fact]
        public async Task ExecuteAsync_Throws_IfArgumentTypeIsWrong()
        {
            Func<int, User> retrieveUserMethod = _controller.RetriveUser;
            ReflectedHttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor { MethodInfo = retrieveUserMethod.Method };
            _arguments["id"] = new DateTime();

            var exception = await Assert.ThrowsAsync<HttpResponseException>(
                 () => actionDescriptor.ExecuteAsync(_context, _arguments, CancellationToken.None));

            Assert.Equal(HttpStatusCode.BadRequest, exception.Response.StatusCode);
            var content = Assert.IsType<ObjectContent<HttpError>>(exception.Response.Content);
            Assert.Equal("The parameters dictionary contains an invalid entry for parameter 'id' for method " +
                "'System.Web.Http.User RetriveUser(Int32)' in 'System.Web.Http.UsersRpcController'. " +
                "The dictionary contains a value of type 'System.DateTime', but the parameter requires a value " +
                "of type 'System.Int32'.",
                ((HttpError)content.Value)["MessageDetail"]);
        }

        [Fact]
        public async Task ExecuteAsync_IfTaskReturningMethod_ReturnsWrappedTaskInstance_Throws()
        {
            Func<Task> method = _controller.WrappedTaskReturningMethod;
            ReflectedHttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor { MethodInfo = method.Method };

            await Assert.ThrowsAsync<InvalidOperationException>(
                 () => actionDescriptor.ExecuteAsync(_context, _arguments, CancellationToken.None),
                 "The method 'WrappedTaskReturningMethod' on type 'UsersRpcController' returned an instance of 'System.Threading.Tasks.Task`1[[System.Threading.Tasks.Task, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]'. Make sure to call Unwrap on the returned value to avoid unobserved faulted Task.");
        }

        [Fact]
        public async Task ExecuteAsync_IfObjectReturningMethod_ReturnsTaskInstance_Throws()
        {
            Func<object> method = _controller.TaskAsObjectReturningMethod;
            ReflectedHttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor { MethodInfo = method.Method };

            await Assert.ThrowsAsync<InvalidOperationException>(
                 () => actionDescriptor.ExecuteAsync(_context, _arguments, CancellationToken.None),
                 "The method 'TaskAsObjectReturningMethod' on type 'UsersRpcController' returned a Task instance even though it is not an asynchronous method.");
        }

        [Theory]
        [InlineData(typeof(void), null)]
        [InlineData(typeof(string), typeof(string))]
        [InlineData(typeof(Task), null)]
        [InlineData(typeof(Task<string>), typeof(string))]
        public void GetReturnType_ReturnsUnwrappedActionType(Type methodReturnType, Type expectedReturnType)
        {
            Mock<MethodInfo> methodMock = new Mock<MethodInfo>();
            methodMock.Setup(m => m.ReturnType).Returns(methodReturnType);

            Assert.Equal(expectedReturnType, ReflectedHttpActionDescriptor.GetReturnType(methodMock.Object));
        }

        [Fact]
        public void GetHashCode_ReturnsTheSameHashCode_ForDifferentInstancesWithTheSameMethodInfo()
        {
            Func<string, string, User> echoUserMethod = _controller.EchoUser;
            HttpConfiguration config = new HttpConfiguration();
            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor(config, "", typeof(UsersRpcController));
            ReflectedHttpActionDescriptor actionDescriptor1 = new ReflectedHttpActionDescriptor(controllerDescriptor, echoUserMethod.Method);
            ReflectedHttpActionDescriptor actionDescriptor2 = new ReflectedHttpActionDescriptor(controllerDescriptor, echoUserMethod.Method);

            Assert.Equal(actionDescriptor1.GetHashCode(), actionDescriptor2.GetHashCode());
        }

        [Fact]
        public void Equals_ReturnsTrue_ForDifferentInstancesWithTheSameMethodInfo()
        {
            Func<string, string, User> echoUserMethod = _controller.EchoUser;
            HttpConfiguration config = new HttpConfiguration();
            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor(config, "", typeof(UsersRpcController));
            ReflectedHttpActionDescriptor actionDescriptor1 = new ReflectedHttpActionDescriptor(controllerDescriptor, echoUserMethod.Method);
            ReflectedHttpActionDescriptor actionDescriptor2 = new ReflectedHttpActionDescriptor(controllerDescriptor, echoUserMethod.Method);

            Assert.Equal(actionDescriptor1, actionDescriptor2);
        }
    }
}
