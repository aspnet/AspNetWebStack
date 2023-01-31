// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http.Controllers;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Routing
{
    public class DirectRouteProviderContextTests
    {
        [Fact]
        public void CreateBuilderWithoutResolverAndBuild_SetsActionsDataToken()
        {
            var actions = new HttpActionDescriptor[] { new ReflectedHttpActionDescriptor() };

            var route = BuildWithoutResolver("route", actions);

            var actualActions = route.DataTokens[RouteDataTokenKeys.Actions];
            Assert.Equal(actions, actualActions);
        }

        [Fact]
        public void CreateBuilderWithoutResolverAndBuild_AddsDefaultValuesAsOptional()
        {
            var actions = new ReflectedHttpActionDescriptor[] { new ReflectedHttpActionDescriptor() };
            var route = BuildWithoutResolver("movies/{id}", actions);
            route.Defaults.Add("id", RouteParameter.Optional);

            var routeData = route.GetRouteData("", new HttpRequestMessage(HttpMethod.Get, "http://localhost/movies"));

            Assert.Equal(RouteParameter.Optional, routeData.Values["id"]);
        }

        [Fact]
        public void CreateBuilderWithResolverAndBuild_Throws_WhenConstraintResolverReturnsNull()
        {
            Mock<IInlineConstraintResolver> constraintResolver = new Mock<IInlineConstraintResolver>();
            constraintResolver.Setup(r => r.ResolveConstraint("constraint")).Returns<IHttpRouteConstraint>(null);

            var ex = Assert.Throws<InvalidOperationException>(
                () => BuildWithResolver(@"hello/{param:constraint}", constraintResolver: constraintResolver.Object));
            Assert.Matches(
                "The inline constraint resolver of type 'IInlineConstraintResolverProxy' was unable to resolve the following inline constraint: 'constraint'.",
                ex.Message);
        }

        [Fact]
        public void CreateBuilderWithResolverAndBuild_ResolvesConstraintUsingConstraintResolver()
        {
            IHttpRouteConstraint routeConstraint = new Mock<IHttpRouteConstraint>().Object;
            Mock<IInlineConstraintResolver> constraintResolver = new Mock<IInlineConstraintResolver>();
            constraintResolver.Setup(r => r.ResolveConstraint("constraint")).Returns(routeConstraint);

            var route = BuildWithResolver(@"hello/{param:constraint}", constraintResolver: constraintResolver.Object);

            Assert.Equal("hello/{param}", route.RouteTemplate);
            Assert.Equal(routeConstraint, route.Constraints["param"]);
        }

        private static IHttpRoute BuildWithoutResolver(string template,
            IReadOnlyCollection<HttpActionDescriptor> actions)
        {
            DirectRouteFactoryContext context = new DirectRouteFactoryContext(null, actions,
                new Mock<IInlineConstraintResolver>(MockBehavior.Strict).Object, targetIsAction: true);
            IDirectRouteBuilder builder = context.CreateBuilder(template, constraintResolver: null);
            return builder.Build().Route;
        }

        private static IHttpRoute BuildWithResolver(string template, IInlineConstraintResolver constraintResolver)
        {
            HttpActionDescriptor[] actions = new HttpActionDescriptor[] { new ReflectedHttpActionDescriptor() };
            DirectRouteFactoryContext context = new DirectRouteFactoryContext(null, actions, constraintResolver, targetIsAction: true);

            // Act
            IDirectRouteBuilder builder = context.CreateBuilder(template);
            IHttpRoute route = builder.Build().Route;

            // Assertions for default, unspecified behavior:
            Assert.NotNull(route);
            Assert.Equal(actions, route.DataTokens["actions"]);

            return route;
        }

    }
}
