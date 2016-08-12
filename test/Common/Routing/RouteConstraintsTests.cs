// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
#if ASPNETWEBAPI
using System.Net.Http;
using System.Web.Http.Routing.Constraints;
#else
using System.Web.Mvc.Routing.Constraints;
using System.Web.Routing;
#endif
using Microsoft.TestCommon;
using Moq;

#if ASPNETWEBAPI
namespace System.Web.Http.Routing
#else
namespace System.Web.Mvc.Routing
#endif
{
    public class RouteConstraintsTests
    {
        [Theory]
        [InlineData(42, true)]
        [InlineData("42", true)]
        [InlineData(3.14, false)]
        [InlineData("43.567", false)]
        [InlineData("42a", false)]
        public void IntRouteConstraintTests(object parameterValue, bool expected)
        {
            var constraint = new IntRouteConstraint();
            var actual = TestValue(constraint, parameterValue);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(42, true)]
        [InlineData("42", true)]
        [InlineData("9223372036854775807", true)]
        [InlineData(3.14, false)]
        [InlineData("43.567", false)]
        [InlineData("42a", false)]
        public void LongRouteConstraintTests(object parameterValue, bool expected)
        {
            var constraint = new LongRouteConstraint();
            var actual = TestValue(constraint, parameterValue);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(@"^\d{3}-\d{3}-\d{4}$", "406-555-0123", true)]
        [InlineData(@"^\d{3}$", "1234", false)]
        public void RegexRouteConstraintTests(string pattern, string parameterValue, bool expected)
        {
            var constraint = new RegexRouteConstraint(pattern);
            var actual = TestValue(constraint, parameterValue);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("alpha", true)]
        [InlineData("a1pha", false)]
        [InlineData("", true)]
        public void AlphaRouteConstraintTests(string parameterValue, bool expected)
        {
            var constraint = new AlphaRouteConstraint();
            var actual = TestValue(constraint, parameterValue);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(long.MinValue, long.MaxValue, 2, true)]
        [InlineData(3, 5, 4, true)]
        [InlineData(3, 5, 5, true)]
        [InlineData(3, 5, 3, true)]
        [InlineData(3, 5, 6, false)]
        [InlineData(3, 5, 2, false)]
        [InlineData(3, 1, 2, false)]
        public void RangeRouteConstraintTests(long min, long max, int parameterValue, bool expected)
        {
            var constraint = new RangeRouteConstraint(min, max);
            var actual = TestValue(constraint, parameterValue);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(3, 4, true)]
        [InlineData(3, 3, true)]
        [InlineData(3, 2, false)]
        public void MinRouteConstraintTests(long min, int parameterValue, bool expected)
        {
            var constraint = new MinRouteConstraint(min);
            var actual = TestValue(constraint, parameterValue);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(3, 2, true)]
        [InlineData(3, 3, true)]
        [InlineData(3, 4, false)]
        public void MaxRouteConstraintTests(long max, int parameterValue, bool expected)
        {
            var constraint = new MaxRouteConstraint(max);
            var actual = TestValue(constraint, parameterValue);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(3, "1234", true)]
        [InlineData(3, "123", true)]
        [InlineData(3, "12", false)]
        [InlineData(3, "", false)]
        public void MinLengthRouteConstraintTests(int min, string parameterValue, bool expected)
        {
            var constraint = new MinLengthRouteConstraint(min);
            var actual = TestValue(constraint, parameterValue);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(3, "", true)]
        [InlineData(3, "12", true)]
        [InlineData(3, "123", true)]
        [InlineData(3, "1234", false)]
        public void MaxLengthRouteConstraintTests(int min, string parameterValue, bool expected)
        {
            var constraint = new MaxLengthRouteConstraint(min);
            var actual = TestValue(constraint, parameterValue);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(3, "123", true)]
        [InlineData(3, "1234", false)]
        public void LengthRouteConstraint_ExactLength_Tests(int length, string parameterValue, bool expected)
        {
            var constraint = new LengthRouteConstraint(length);
            var actual = TestValue(constraint, parameterValue);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(3, 5, "12", false)]
        [InlineData(3, 5, "123", true)]
        [InlineData(3, 5, "1234", true)]
        [InlineData(3, 5, "12345", true)]
        [InlineData(3, 5, "123456", false)]
        public void LengthRouteConstraint_Range_Tests(int min, int max, string parameterValue, bool expected)
        {
            var constraint = new LengthRouteConstraint(min, max);
            var actual = TestValue(constraint, parameterValue);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("12345678-1234-1234-1234-123456789012", false, true)]
        [InlineData("12345678-1234-1234-1234-123456789012", true, true)]
        [InlineData("12345678901234567890123456789012", false, true)]
        [InlineData("not-parseable-as-guid", false, false)]
        [InlineData(12, false, false)]
        public void GuidRouteConstraintTests(object parameterValue, bool parseBeforeTest, bool expected)
        {
            if (parseBeforeTest)
            {
                parameterValue = Guid.Parse(parameterValue.ToString());
            }
            var constraint = new GuidRouteConstraint();
            var actual = TestValue(constraint, parameterValue);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("3.14", true)]
        [InlineData(3.14f, true)]
        [InlineData("not-parseable-as-float", false)]
        [InlineData(false, false)]
        [InlineData("1.79769313486232E+300", false)]
        public void FloatRouteConstraintTests(object parameterValue, bool expected)
        {
            var constraint = new FloatRouteConstraint();
            var actual = TestValue(constraint, parameterValue);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("3.14", true)]
        [InlineData(3.14f, true)]
        [InlineData("1.79769313486232E+300", true)]
        [InlineData("not-parseable-as-double", false)]
        [InlineData(false, false)]
        public void DoubleRouteConstraintTests(object parameterValue, bool expected)
        {
            var constraint = new DoubleRouteConstraint();
            var actual = TestValue(constraint, parameterValue);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("3.14", true)]
        [InlineData("9223372036854775808.9223372036854775808", true)]
        [InlineData("1.79769313486232E+300", false)]
        [InlineData("not-parseable-as-decimal", false)]
        [InlineData(false, false)]
        public void DecimalRouteConstraintTests(object parameterValue, bool expected)
        {
            var constraint = new DecimalRouteConstraint();
            var actual = TestValue(constraint, parameterValue);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("12/25/2009", true)]
        [InlineData("12/25/2009 11:45:00 PM", true)]
        [InlineData("11:45:00 PM", true)]
        [InlineData("2009-05-12T11:45:00Z", true)]
        [InlineData("not-parseable-as-date", false)]
        [InlineData(false, false)]
        public void DateTimeRouteConstraint(object parameterValue, bool expected)
        {
            var constraint = new DateTimeRouteConstraint();
            var actual = TestValue(constraint, parameterValue);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("true", true)]
        [InlineData("false", true)]
        [InlineData(true, true)]
        [InlineData(false, true)]
        [InlineData(1, false)]
        [InlineData("not-parseable-as-bool", false)]
        public void BoolRouteConstraint(object parameterValue, bool expected)
        {
            var constraint = new BoolRouteConstraint();
            var actual = TestValue(constraint, parameterValue);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(null, false, true)]
        [InlineData("pass", true, true)]
        [InlineData("fail", true, false)]
        public void OptionalRouteConstraintTests(object parameterValue, bool shouldCallInner, bool expected)
        {
            // Arrange
            var inner = MockConstraintWithResult((string)parameterValue != "fail");

            // Act
            var constraint = new OptionalRouteConstraint(inner.Object);
#if ASPNETWEBAPI
            var optionalParameter = RouteParameter.Optional;
#else
            var optionalParameter = UrlParameter.Optional;
#endif
            var actual = TestValue(constraint, parameterValue ?? optionalParameter, route =>
            {
                route.Defaults.Add("fake", optionalParameter);
            });

            // Assert
            Assert.Equal(expected, actual);

            var timeMatchShouldHaveBeenCalled = shouldCallInner
                ? Times.Once()
                : Times.Never();

            AssertMatchWasCalled(inner, timeMatchShouldHaveBeenCalled);
        }

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(true, false, false)]
        [InlineData(false, true, false)]
        [InlineData(false, false, false)]
        public void CompoundRouteConstraintTests(bool inner1Result, bool inner2Result, bool expected)
        {
            // Arrange
            var inner1 = MockConstraintWithResult(inner1Result);

            var inner2 = MockConstraintWithResult(inner2Result);

            // Act
            var constraint = new CompoundRouteConstraint(new[] { inner1.Object, inner2.Object });
            var actual = TestValue(constraint, null);

            // Assert
            Assert.Equal(expected, actual);
        }

#if ASPNETWEBAPI
        static Expression<Func<IHttpRouteConstraint, bool>> ConstraintMatchMethodExpression =
            c => c.Match(It.IsAny<HttpRequestMessage>(), It.IsAny<IHttpRoute>(), It.IsAny<string>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<HttpRouteDirection>());

        private static Mock<IHttpRouteConstraint> MockConstraintWithResult(bool result)
        {
            var mock = new Mock<IHttpRouteConstraint>();
            mock.Setup(ConstraintMatchMethodExpression)
                 .Returns(result)
                 .Verifiable();
            return mock;
        }

        private static void AssertMatchWasCalled(Mock<IHttpRouteConstraint> mock, Times times)
        {
            mock.Verify(ConstraintMatchMethodExpression, times);
        }

        private static bool TestValue(IHttpRouteConstraint constraint, object value, Action<IHttpRoute> routeConfig = null)
        {
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage();

            HttpRoute httpRoute = new HttpRoute();
            if (routeConfig != null)
            {
                routeConfig(httpRoute);
            }
            const string parameterName = "fake";
            HttpRouteValueDictionary values = new HttpRouteValueDictionary { { parameterName, value } };
            const HttpRouteDirection httpRouteDirection = HttpRouteDirection.UriResolution;

            return constraint.Match(httpRequestMessage, httpRoute, parameterName, values, httpRouteDirection);
        }
#else
        static Expression<Func<IRouteConstraint, bool>> ConstraintMatchMethodExpression =
            c => c.Match(It.IsAny<HttpContextBase>(), It.IsAny<Route>(), It.IsAny<string>(), It.IsAny<RouteValueDictionary>(), It.IsAny<RouteDirection>());

        private static Mock<IRouteConstraint> MockConstraintWithResult(bool result)
        {
            var mock = new Mock<IRouteConstraint>();
            mock.Setup(ConstraintMatchMethodExpression)
                 .Returns(result)
                 .Verifiable();
            return mock;
        }

        private static void AssertMatchWasCalled(Mock<IRouteConstraint> mock, Times times)
        {
            mock.Verify(ConstraintMatchMethodExpression, times);
        }

        private static bool TestValue(IRouteConstraint constraint, object value, Action<Route> routeConfig = null)
        {
            var context = new Mock<HttpContextBase>();

            Route route = new Route("", null);
            route.Defaults = new RouteValueDictionary();

            if (routeConfig != null)
            {
                routeConfig(route);
            }
            const string parameterName = "fake";
            RouteValueDictionary values = new RouteValueDictionary { { parameterName, value } };
            const RouteDirection routeDirection = RouteDirection.IncomingRequest;

            return constraint.Match(context.Object, route, parameterName, values, routeDirection);
        }
#endif

    }
}