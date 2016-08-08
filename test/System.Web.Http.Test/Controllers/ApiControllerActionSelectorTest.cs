// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using Microsoft.TestCommon;

namespace System.Web.Http
{
    public class ApiControllerActionSelectorTest
    {
        [Fact]
        public void SelectAction_With_DifferentExecutionContexts()
        {
            ApiControllerActionSelector actionSelector = new ApiControllerActionSelector();
            HttpControllerContext GetContext = ContextUtil.CreateControllerContext();
            HttpControllerDescriptor usersControllerDescriptor = new HttpControllerDescriptor(GetContext.Configuration, "Users", typeof(UsersController));
            usersControllerDescriptor.Configuration.Services.Replace(typeof(IHttpActionSelector), actionSelector);
            GetContext.ControllerDescriptor = usersControllerDescriptor;
            GetContext.Request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get
                };
            HttpControllerContext PostContext = ContextUtil.CreateControllerContext();
            usersControllerDescriptor.Configuration.Services.Replace(typeof(IHttpActionSelector), actionSelector);
            PostContext.ControllerDescriptor = usersControllerDescriptor;
            PostContext.Request = new HttpRequestMessage
            {
                Method = HttpMethod.Post
            };

            HttpActionDescriptor getActionDescriptor = actionSelector.SelectAction(GetContext);
            HttpActionDescriptor postActionDescriptor = actionSelector.SelectAction(PostContext);

            Assert.Equal("Get", getActionDescriptor.ActionName);
            Assert.Equal("Post", postActionDescriptor.ActionName);
        }

        [Fact]
        public void SelectAction_RespectsDirectRoutes()
        {
            var actionSelector = new ApiControllerActionSelector();
            HttpControllerContext context = ContextUtil.CreateControllerContext();
            context.Request = new HttpRequestMessage { Method = HttpMethod.Get };
            var controllerDescriptor = new HttpControllerDescriptor(context.Configuration, "Users", typeof(UsersController));
            context.ControllerDescriptor = controllerDescriptor;
            ReflectedHttpActionDescriptor directRouteAction = (ReflectedHttpActionDescriptor)actionSelector.GetActionMapping(controllerDescriptor)["Get"].First();
            context.RouteData.Route.DataTokens.Add("actions", new ReflectedHttpActionDescriptor[] { directRouteAction });

            HttpActionDescriptor actionDescriptor = actionSelector.SelectAction(context);

            Assert.Same(directRouteAction, actionDescriptor);
        }

        [Fact]
        public void SelectAction_WithDirectRoutes_RespectsRouteOrder()
        {
            // Arrange
            var actionSelector = new ApiControllerActionSelector();
            HttpControllerContext context = ContextUtil.CreateControllerContext();
            context.Request = new HttpRequestMessage { Method = HttpMethod.Get };
            var controllerDescriptor = new HttpControllerDescriptor(context.Configuration, "MultipleGet", typeof(MultipleGetController));
            context.ControllerDescriptor = controllerDescriptor;
            ReflectedHttpActionDescriptor firstDirectRouteAction = (ReflectedHttpActionDescriptor)actionSelector.GetActionMapping(controllerDescriptor)["GetA"].Single();
            HttpRouteData[] subRouteData = new HttpRouteData[2];
            subRouteData[0] = new HttpRouteData(new HttpRoute());
            subRouteData[1] = new HttpRouteData(new HttpRoute());
            context.RouteData.Values.Add(RouteCollectionRoute.SubRouteDataKey, subRouteData);
            subRouteData[0].Route.DataTokens.Add("actions", new ReflectedHttpActionDescriptor[] { firstDirectRouteAction });
            subRouteData[0].Route.DataTokens.Add("order", 1);
            ReflectedHttpActionDescriptor secondDirectRouteAction = (ReflectedHttpActionDescriptor)actionSelector.GetActionMapping(controllerDescriptor)["GetB"].Single();
            subRouteData[1].Route.DataTokens.Add("actions", new ReflectedHttpActionDescriptor[] { secondDirectRouteAction });
            subRouteData[1].Route.DataTokens.Add("order", 2);

            // Act
            HttpActionDescriptor actionDescriptor = actionSelector.SelectAction(context);

            // Assert
            Assert.Same(firstDirectRouteAction, actionDescriptor);
        }

        [Fact]
        public void SelectAction_WithDirectRoutes_RespectsPrecedence()
        {
            // Arrange
            var actionSelector = new ApiControllerActionSelector();
            HttpControllerContext context = ContextUtil.CreateControllerContext();
            context.Request = new HttpRequestMessage { Method = HttpMethod.Get };
            var controllerDescriptor = new HttpControllerDescriptor(context.Configuration, "MultipleGet", typeof(MultipleGetController));
            context.ControllerDescriptor = controllerDescriptor;
            ReflectedHttpActionDescriptor firstDirectRouteAction = (ReflectedHttpActionDescriptor)actionSelector.GetActionMapping(controllerDescriptor)["GetA"].Single();
            HttpRouteData[] subRouteData = new HttpRouteData[2];
            subRouteData[0] = new HttpRouteData(new HttpRoute());
            subRouteData[1] = new HttpRouteData(new HttpRoute());
            context.RouteData.Values.Add(RouteCollectionRoute.SubRouteDataKey, subRouteData);
            subRouteData[0].Route.DataTokens.Add("actions", new ReflectedHttpActionDescriptor[] { firstDirectRouteAction });
            subRouteData[0].Route.DataTokens.Add("precedence", 2M);
            ReflectedHttpActionDescriptor secondDirectRouteAction = (ReflectedHttpActionDescriptor)actionSelector.GetActionMapping(controllerDescriptor)["GetB"].Single();
            subRouteData[1].Route.DataTokens.Add("actions", new ReflectedHttpActionDescriptor[] { secondDirectRouteAction });
            subRouteData[1].Route.DataTokens.Add("precedence", 1M);

            // Act
            HttpActionDescriptor actionDescriptor = actionSelector.SelectAction(context);

            // Assert
            Assert.Same(secondDirectRouteAction, actionDescriptor);
        }

        [Fact]
        public void SelectAction_Throws_IfContextIsNull()
        {
            ApiControllerActionSelector actionSelector = new ApiControllerActionSelector();

            Assert.ThrowsArgumentNull(
                () => actionSelector.SelectAction(null),
                "controllerContext");
        }

        [Fact]
        public void GetActionMapping_IgnoresNonAction()
        {
            var actionSelector = new ApiControllerActionSelector();
            HttpControllerContext context = ContextUtil.CreateControllerContext();
            context.Request = new HttpRequestMessage { Method = HttpMethod.Get };
            var controllerDescriptor = new HttpControllerDescriptor(context.Configuration, "NonAction", typeof(NonActionController));
            context.ControllerDescriptor = controllerDescriptor;

            var mapping = actionSelector.GetActionMapping(controllerDescriptor);

            Assert.False(mapping.Contains("GetA"));
            Assert.True(mapping.Contains("GetB"));            
        }

        public class NonActionController : ApiController
        {
            [NonAction]
            public HttpResponseMessage GetA() { return null; }

            public HttpResponseMessage GetB() { return null; }
        }

        public class MultipleGetController : ApiController
        {
            public HttpResponseMessage GetA() { return null; }

            public HttpResponseMessage GetB() { return null; }
        }
    }
}
