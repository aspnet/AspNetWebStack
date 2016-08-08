// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
#if ASPNETWEBAPI
using System.Net.Http;
using System.Web.Http.Routing.Constraints;
#else
using System.Web.Mvc.Routing.Constraints;
#endif
using Microsoft.TestCommon;
using Moq;

#if ASPNETWEBAPI
using TActionDescriptor = System.Web.Http.Controllers.HttpActionDescriptor;
using TParsedRoute = System.Web.Http.Routing.HttpParsedRoute;
using TRouteValueDictionary = System.Web.Http.Routing.HttpRouteValueDictionary;
#else
using TActionDescriptor = System.Web.Mvc.ActionDescriptor;
using TParsedRoute = System.Web.Mvc.Routing.ParsedRoute;
using TRouteValueDictionary = System.Web.Routing.RouteValueDictionary;
#endif

#if ASPNETWEBAPI
namespace System.Web.Http.Routing
#else
namespace System.Web.Mvc.Routing
#endif
{
    public class DirectRouteBuilderTests
    {
        [Fact]
        public void CreateRoute_ValidatesConstraintType_TRouteConstraint()
        {
            // Arrange
            var actions = GetActions();
            var builder = new DirectRouteBuilder(actions, targetIsAction: true);

            var constraint = new AlphaRouteConstraint();
            var constraints = new TRouteValueDictionary();
            constraints.Add("custom", constraint);
            builder.Constraints = constraints;

            // Act
            var routeEntry = builder.Build();

            // Assert
            Assert.NotNull(routeEntry.Route.Constraints["custom"]);
        }

        [Fact]
        public void BuildRoute_ValidatesConstraintType_StringRegex()
        {
            // Arrange
            var actions = GetActions();
            var builder = new DirectRouteBuilder(actions, targetIsAction: true);

            var constraint = "product|products";
            var constraints = new TRouteValueDictionary();
            constraints.Add("custom", constraint);
            builder.Constraints = constraints;

            // Act
            var routeEntry = builder.Build();

            // Assert
            Assert.NotNull(routeEntry.Route.Constraints["custom"]);
        }

        [Fact]
        public void BuildRoute_ValidatesConstraintType_InvalidType()
        {
            // Arrange
            var actions = GetActions();
            var builder = new DirectRouteBuilder(actions, targetIsAction: true);

            var constraint = new Uri("http://localhost/");
            var constraints = new TRouteValueDictionary();
            constraints.Add("custom", constraint);

            builder.Constraints = constraints;
            builder.Template = "c/{id}";

#if ASPNETWEBAPI
            string expectedMessage =
                "The constraint entry 'custom' on the route with route template 'c/{id}' " +
                "must have a string value or be of a type which implements 'System.Web.Http.Routing.IHttpRouteConstraint'.";
#else
            string expectedMessage =
                "The constraint entry 'custom' on the route with route template 'c/{id}' " +
                "must have a string value or be of a type which implements 'System.Web.Routing.IRouteConstraint'.";
#endif

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => builder.Build(), expectedMessage);
        }

        [Fact]
        public void BuildRoute_ValidatesAllowedParameters()
        {
            // Arrange
            var actions = GetActions();
            var builder = new MockDirectRouteBuilder(actions, targetIsAction: true);
            builder.Template = "{a}/{b}";

            // Act
            RouteEntry entry = builder.Build();

            // Assert
            Assert.NotNull(entry);
            Assert.Equal(1, builder.TimesValidateParametersCalled);
        }

        [Theory]
        [InlineData("{controller}", true)]
        [InlineData("{controller}", false)]
        [InlineData("{z}-abc-{controller}", true)]
        public void BuildRoute_ControllerParameterNotAllowed(string template, bool targetIsAction)
        {
            // Arrange
            var actions = GetActions();

            var expectedMessage =
                "A direct route cannot use the parameter 'controller'. " +
                "Specify a literal path in place of this parameter to create a route to a controller.";

            var builder = new MockDirectRouteBuilder(actions, targetIsAction);
            builder.Template = template;

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => builder.Build(), expectedMessage);
            Assert.Equal(1, builder.TimesValidateParametersCalled);
        }

        [Fact]
        public void BuildRoute_ActionParameterAllowed_OnControllerRoute()
        {
            // Arrange
            var actions = GetActions();
            var builder = new MockDirectRouteBuilder(actions, targetIsAction: false);
            builder.Template = "{a}/{action}";

            // Act
            RouteEntry entry = builder.Build();

            // Assert
            Assert.NotNull(entry);
            Assert.Equal(1, builder.TimesValidateParametersCalled);
        }

        [Theory]
        [InlineData("{action}")]
        [InlineData("api/yy-{action}")]
        public void BuildRoute_ActionNotAllowed_OnActionRoute(string template)
        {
            // Arrange
            var actions = GetActions();

            var expectedMessage =
                "A direct route for an action method cannot use the parameter 'action'. " +
                "Specify a literal path in place of this parameter to create a route to the action.";

            var builder = new MockDirectRouteBuilder(actions, targetIsAction: true);
            builder.Template = template;

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => builder.Build(), expectedMessage);
            Assert.Equal(1, builder.TimesValidateParametersCalled);
        }

#if ASPNETWEBAPI
        private IReadOnlyCollection<TActionDescriptor> GetActions()
        {
            var actions = new List<TActionDescriptor>()
            {
                new Mock<TActionDescriptor>().Object,
            };

            return actions.AsReadOnly();
        }
#else
        private IReadOnlyCollection<TActionDescriptor> GetActions()
        {
            var action = new Mock<ActionDescriptor>();
            action.SetupGet(a => a.ControllerDescriptor).Returns(new Mock<ControllerDescriptor>().Object);
            var actions = new List<ActionDescriptor>()
            {
                action.Object,
            };

            return actions.AsReadOnly();
        }
#endif

        private class MockDirectRouteBuilder : DirectRouteBuilder
        {
            public MockDirectRouteBuilder(IReadOnlyCollection<TActionDescriptor> actionDescriptors, bool targetIsAction)
                : base(actionDescriptors, targetIsAction)
            {
            }

            public int TimesValidateParametersCalled
            {
                get;
                private set;
            }

            internal override void ValidateParameters(TParsedRoute parsedRoute)
            {
                TimesValidateParametersCalled++;
                base.ValidateParameters(parsedRoute);
            }
        }
    }
}
