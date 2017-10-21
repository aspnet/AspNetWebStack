// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http.Routing;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http
{
    public class HttpRouteCollectionTest
    {
        [Fact]
        public void HttpRouteCollection_Dispose_UniquifiesHandlers()
        {
            // Arrange
            var collection = new HttpRouteCollection();

            var handler1 = new Mock<HttpMessageHandler>();
            handler1.As<IDisposable>().Setup(c => c.Dispose()).Verifiable();

            var handler2 = new Mock<HttpMessageHandler>();
            handler2.As<IDisposable>().Setup(c => c.Dispose()).Verifiable();

            var route1 = new Mock<IHttpRoute>();
            route1.SetupGet(r => r.Handler).Returns(handler1.Object);

            var route2 = new Mock<IHttpRoute>();
            route2.SetupGet(r => r.Handler).Returns(handler1.Object);

            var route3 = new Mock<IHttpRoute>();
            route3.SetupGet(r => r.Handler).Returns(handler2.Object);

            collection.Add("route1", route1.Object);
            collection.Add("route2", route2.Object);
            collection.Add("route3", route3.Object);

            // Act
            collection.Dispose();

            // Assert
            handler1.As<IDisposable>().Verify(c => c.Dispose(), Times.Once());
            handler2.As<IDisposable>().Verify(c => c.Dispose(), Times.Once());
        }

        [Fact]
        public void CreateRoute_ValidatesConstraintType_IHttpRouteConstraint()
        {
            // Arrange
            var routes = new MockHttpRouteCollection();

            var constraint = new CustomConstraint();
            var constraints = new HttpRouteValueDictionary();
            constraints.Add("custom", constraint);

            // Act
            var route = routes.CreateRoute("{controller}/{id}", null, constraints);

            // Assert
            Assert.NotNull(route.Constraints["custom"]);
            Assert.Equal(1, routes.TimesValidateConstraintCalled);
        }

        [Fact]
        public void CreateRoute_ValidatesConstraintType_StringRegex()
        {
            // Arrange
            var routes = new MockHttpRouteCollection();

            var constraint = "product|products";
            var constraints = new HttpRouteValueDictionary();
            constraints.Add("custom", constraint);

            // Act
            var route = routes.CreateRoute("{controller}/{id}", null, constraints);

            // Assert
            Assert.NotNull(route.Constraints["custom"]);
            Assert.Equal(1, routes.TimesValidateConstraintCalled);
        }

        [Fact]
        public void CreateRoute_ValidatesConstraintType_InvalidType()
        {
            // Arrange
            var routes = new HttpRouteCollection();

            var constraint = new Uri("http://localhost/");
            var constraints = new HttpRouteValueDictionary();
            constraints.Add("custom", constraint);

            string expectedMessage =
                "The constraint entry 'custom' on the route with route template '{controller}/{id}' " +
                "must have a string value or be of a type which implements 'System.Web.Http.Routing.IHttpRouteConstraint'.";

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => routes.CreateRoute("{controller}/{id}", null, constraints), expectedMessage);
        }

        private class CustomConstraint : IHttpRouteConstraint
        {
            public bool Match(HttpRequestMessage request, IHttpRoute route, string parameterName, IDictionary<string, object> values, HttpRouteDirection routeDirection)
            {
                throw new NotImplementedException();
            }
        }

        private class MockHttpRouteCollection : HttpRouteCollection
        {
            public int TimesValidateConstraintCalled
            {
                get;
                private set;
            }

            protected override void ValidateConstraint(string routeTemplate, string name, object constraint)
            {
                TimesValidateConstraintCalled++;
                base.ValidateConstraint(routeTemplate, name, constraint);
            }
        }
    }
}
