// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if !ASPNETWEBAPI
using System.Web.Routing;
#endif
using Microsoft.TestCommon;

#if ASPNETWEBAPI
namespace System.Web.Http.Routing
#else
namespace System.Web.Mvc.Routing
#endif
{
    public class RoutePrecedenceTests
    {
        [Theory]
        [InlineData("Employees/{id}", "Employees/{id}")]
        [InlineData("abc", "def")]
        [InlineData("{x:alpha}", "{x:int}")]
        public void Compute_IsEqual(string xTemplate, string yTemplate)
        {
            // Arrange & Act
            var xPrededence = Compute(xTemplate);
            var yPrededence = Compute(yTemplate);

            // Assert
            Assert.Equal(xPrededence, yPrededence);
        }

        [Theory]
        [InlineData("abc", "a{x}")]
        [InlineData("abc", "{x}c")]
        [InlineData("abc", "{x:int}")]
        [InlineData("abc", "{x}")]
        [InlineData("abc", "{*x}")]
        [InlineData("{x:int}", "{x}")]
        [InlineData("{x:int}", "{*x}")]
        [InlineData("a{x}", "{x}")]
        [InlineData("{x}c", "{x}")]
        [InlineData("a{x}", "{*x}")]
        [InlineData("{x}c", "{*x}")]
        [InlineData("{x}", "{*x}")]
        [InlineData("{*x:maxlength(10)}", "{*x}")]
        [InlineData("abc/def", "abc/{x:int}")]
        [InlineData("abc/def", "abc/{x}")]
        [InlineData("abc/def", "abc/{*x}")]
        [InlineData("abc/{x:int}", "abc/{x}")]
        [InlineData("abc/{x:int}", "abc/{*x}")]
        [InlineData("abc/{x}", "abc/{*x}")]
        [InlineData("{x}/{y:int}", "{x}/{y}")]
        public void Compute_IsLessThan(string xTemplate, string yTemplate)
        {
            // Arrange & Act
            var xPrededence = Compute(xTemplate);
            var yPrededence = Compute(yTemplate);

            // Assert
            Assert.True(xPrededence < yPrededence);
        }

        private static decimal Compute(string template)
        {
            DefaultInlineConstraintResolver resolver = new DefaultInlineConstraintResolver();
#if ASPNETWEBAPI
            HttpRouteValueDictionary defaults = new HttpRouteValueDictionary();
            HttpRouteValueDictionary constraints = new HttpRouteValueDictionary();
#else
            RouteValueDictionary defaults = new RouteValueDictionary();
            RouteValueDictionary constraints = new RouteValueDictionary();
#endif
            string standardRouteTemplate = InlineRouteTemplateParser.ParseRouteTemplate(template,
                defaults, constraints, new DefaultInlineConstraintResolver());
            var parsedRoute = RouteParser.Parse(standardRouteTemplate);
            return RoutePrecedence.Compute(parsedRoute, constraints);
        }
    }
}