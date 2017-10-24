// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Mvc.Routing;
using System.Web.Mvc.Routing.Constraints;
using Microsoft.TestCommon;

namespace System.Web.Routing
{
    public class RouteCollectionAttributeRoutingExtensionsTests
    {
        [Fact]
        public void MapMvcAttributeRoutes_MapRouteAttributes()
        {
            // Arrange
            var controllerTypes = new[] { typeof(SimpleRoutingController) };
            var routes = new RouteCollection();

            // Act
            AttributeRoutingMapper.MapAttributeRoutes(routes, controllerTypes);

            // Assert
            var expectedResults = new List<Tuple<string, string>>
             {
                 new Tuple<string, string>("getme", "GetMe"),
                 new Tuple<string, string>("postme", "PostMe"),
                 new Tuple<string, string>("getorpostme", "GetOrPostMe"),
                 new Tuple<string, string>("routeme", "RouteMe"),
                 new Tuple<string, string>("once", "FoolMe"),
                 new Tuple<string, string>("twice", "FoolMe"),
                 new Tuple<string, string>("twice", "FoolMe"),
             };

            foreach (var expected in expectedResults)
            {
                var url = expected.Item1;
                var methodName = expected.Item2;

                var attributeRoutes = GetAttributeRoutes(routes);
                Route route = attributeRoutes.Cast<Route>().Single(r => r.Url == url);
                Assert.Equal(methodName, Assert.Single(route.GetTargetActionDescriptors()).ActionName);
            }
        }

        [Fact]
        public void MapMvcAttributeRoutes_CustomConstraintResolver()
        {
            // Arrange
            var controllerTypes = new[] { typeof(FruitConstraintController) };
            var routes = new RouteCollection();

            // Act
            AttributeRoutingMapper.MapAttributeRoutes(routes, controllerTypes, new FruitConstraintResolver());

            // Assert
            var attributeRoutes = GetAttributeRoutes(routes);
            Assert.Equal(1, attributeRoutes.Count);
            Route route = (Route)attributeRoutes.Single();

            Assert.Equal("fruits/{apple}", route.Url);
            Assert.IsAssignableFrom<FruitConstraint>(route.Constraints["apple"]);
        }

        [Fact]
        public void MapMvcAttributeRoutes_WithInlineConstraints_ParseConstraintsDefaultsAndOptionals()
        {
            // Arrange
            var controllerTypes = new[] { typeof(SimpleRoutingController) };
            var routes = new RouteCollection();

            // Act
            AttributeRoutingMapper.MapAttributeRoutes(routes, controllerTypes);

            // Assert
            var attributeRoutes = GetAttributeRoutes(routes);
            Route route = attributeRoutes.Cast<Route>().Single(r => r.GetTargetActionDescriptors().Single().ActionName == "Parameterized");
            Assert.NotNull(route);

            Assert.Equal("i/{have}/{id}/{defaultsto}/{name}", route.Url);
            Assert.IsAssignableFrom<IntRouteConstraint>(route.Constraints["id"]);
            Assert.Equal("VAL", route.Defaults["defaultsto"]);
            Assert.Equal("", route.Defaults["name"].ToString());
        }

        [Fact]
        public void MapMvcAttributeRoutes_WithPrefixedController()
        {
            // Arrange
            var controllerTypes = new[] { typeof(PrefixedController) };
            var routes = new RouteCollection();

            // Act
            AttributeRoutingMapper.MapAttributeRoutes(routes, controllerTypes);

            // Assert
            var attributeRoutes = GetAttributeRoutes(routes);
            Assert.Equal(1, attributeRoutes.Count);

            Route route = (Route)attributeRoutes.Single();
            Assert.Equal("prefpref/getme", route.Url);
            Assert.Equal("GetMe", Assert.Single(route.GetTargetActionDescriptors()).ActionName);
        }

        [Fact]
        public void RoutePrefixAttribute_IsSingleInstance()
        {
            var attr = typeof(RoutePrefixAttribute);
            var attrs = attr.GetCustomAttributes(typeof(AttributeUsageAttribute), false);
            var usage = (AttributeUsageAttribute)attrs[0];

            Assert.Equal(AttributeTargets.Class, usage.ValidOn);
            Assert.False(usage.AllowMultiple); // only 1 per class
            Assert.False(usage.Inherited); // RoutePrefix is not inherited.
        }

        [Fact]
        public void MapMvcAttributeRoutes_WithArea()
        {
            // Arrange
            var controllerTypes = new[] { typeof(PugetSoundController) };
            var routes = new RouteCollection();

            // Act
            AttributeRoutingMapper.MapAttributeRoutes(routes, controllerTypes);

            // Assert
            var attributeRoutes = GetAttributeRoutes(routes);
            Assert.Equal(1, attributeRoutes.Count);

            Route route = (Route)attributeRoutes.Single();

            Assert.Equal("puget-sound/getme", route.Url);
            Assert.Equal("PugetSound", route.DataTokens["area"]);
            Assert.Equal((object)false, route.DataTokens["usenamespacefallback"]);
            Assert.Equal("GetMe", Assert.Single(route.GetTargetActionDescriptors()).ActionName);
            Assert.Equal(typeof(PugetSoundController).Namespace, ((string[])route.DataTokens["namespaces"])[0]);
        }

        [Fact]
        public void MapMvcAttributeRoutes_WithPrefixedArea()
        {
            // Arrange
            var controllerTypes = new[] { typeof(PrefixedPugetSoundController) };
            var routes = new RouteCollection();

            // Act
            AttributeRoutingMapper.MapAttributeRoutes(routes, controllerTypes);

            // Assert
            var attributeRoutes = GetAttributeRoutes(routes);
            Assert.Equal(1, attributeRoutes.Count);

            Route route = (Route)attributeRoutes.Single();

            Assert.Equal("puget-sound/prefpref/getme", route.Url);
            Assert.Equal("PugetSound", route.DataTokens["area"]);
            Assert.Equal((object)false, route.DataTokens["usenamespacefallback"]);
            Assert.Equal("GetMe", Assert.Single(route.GetTargetActionDescriptors()).ActionName);
            Assert.Equal(typeof(PrefixedPugetSoundController).Namespace, ((string[])route.DataTokens["namespaces"])[0]);
        }

        [Theory]
        [InlineData(null, null, "", "")]
        [InlineData(null, null, "whatever", "whatever")]
        [InlineData(null, "", "", "")]
        [InlineData(null, "", "whatever", "whatever")]
        [InlineData(null, "pref", "", "pref")]
        [InlineData(null, "pref", "whatever", "pref/whatever")]
        [InlineData("", null, "", "")]
        [InlineData("", null, "whatever", "whatever")]
        [InlineData("", "", "", "")]
        [InlineData("", "", "whatever", "whatever")]
        [InlineData("", "pref", "", "pref")]
        [InlineData("", "pref", "whatever", "pref/whatever")]
        [InlineData("puget-sound", null, "", "puget-sound")]
        [InlineData("puget-sound", null, "whatever", "puget-sound/whatever")]
        [InlineData("puget-sound", "", "", "puget-sound")]
        [InlineData("puget-sound", "", "whatever", "puget-sound/whatever")]
        [InlineData("puget-sound", "pref", "", "puget-sound/pref")]
        [InlineData("puget-sound", "pref", "whatever", "puget-sound/pref/whatever")]
        [InlineData(null, null, "~/whatever", "whatever")]
        [InlineData("puget-sound", "pref", "~/", "")]
        [InlineData("puget-sound", "pref", "~/whatever", "whatever")]
        [InlineData("puget-sound", null, "~/whatever", "whatever")]
        [InlineData(null, "pref", "~/whatever", "whatever")]
        public void BuildRouteTemplate(string areaPrefix, string prefix, string template, string expected)
        {
            var result = DirectRouteFactoryContext.BuildRouteTemplate(areaPrefix, prefix, template);

            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(typeof(Bad1Controller), "The route prefix '/pref' on the controller named 'Bad1' cannot begin or end with a forward slash.")]
        [InlineData(typeof(Bad2Controller), "The route prefix 'pref/' on the controller named 'Bad2' cannot begin or end with a forward slash.")]
        [InlineData(typeof(Bad3Controller), "The route template '/getme' on the action named 'GetMe' on the controller named 'Bad3' cannot begin with a forward slash.")]
        [InlineData(typeof(Bad4Controller), null)]
        [InlineData(typeof(Bad5Controller), "The route template '/puget-sound/getme' on the action named 'GetMe' on the controller named 'Bad5' cannot begin with a forward slash.")]
        [InlineData(typeof(Bad6Controller), "The prefix 'puget-sound/' of the route area named 'PugetSound' on the controller named 'Bad6' cannot end with a forward slash.")]
        public void TemplatesAreValidated(Type controllerType, string expectedErrorMessage)
        {
            // Arrange
            var controllerTypes = new[] { controllerType };
            var routes = new RouteCollection();

            // Act & Assert
            if (expectedErrorMessage == null)
            {
                Assert.DoesNotThrow(() => AttributeRoutingMapper.MapAttributeRoutes(routes, controllerTypes));
            }
            else
            {
                Assert.Throws<InvalidOperationException>(() => AttributeRoutingMapper.MapAttributeRoutes(routes, controllerTypes), expectedErrorMessage);
            }
        }

        private class SimpleRoutingController : Controller
        {
            [Route("getme")]
            [HttpGet]
            public ActionResult GetMe()
            {
                throw new NotImplementedException();
            }

            [HttpPost]
            [Route("postme")]
            public ActionResult PostMe()
            {
                throw new NotImplementedException();
            }

            [Route("routeme")]
            public ActionResult RouteMe()
            {
                throw new NotImplementedException();
            }

            [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
            [Route("getorpostme")]
            public ActionResult GetOrPostMe()
            {
                throw new NotImplementedException();
            }

            [HttpGet]
            [Route("once")]
            [Route("twice")]
            public ActionResult FoolMe()
            {
                throw new NotImplementedException("Shame on you!");
            }

            [HttpGet]
            [Route("i/{have}/{id:int}/{defaultsto=VAL}/{name?}")]
            public ActionResult Parameterized(string have, int id, string optional)
            {
                throw new NotImplementedException();
            }
        }

        [RoutePrefix("prefpref")]
        private class PrefixedController : Controller
        {
            [HttpGet]
            [Route("getme")]
            public ActionResult GetMe()
            {
                throw new NotImplementedException();
            }

            public ActionResult IDontGetARoute()
            {
                throw new NotImplementedException();
            }
        }

        [RouteArea("PugetSound", AreaPrefix = "puget-sound")]
        private class PugetSoundController : Controller
        {
            [HttpGet]
            [Route("getme")]
            public ActionResult GetMe()
            {
                throw new NotImplementedException();
            }

            public ActionResult IDontGetARoute()
            {
                throw new NotImplementedException();
            }
        }

        [RouteArea("PugetSound", AreaPrefix = "puget-sound")]
        [RoutePrefix("prefpref")]
        private class PrefixedPugetSoundController : Controller
        {
            [HttpGet]
            [Route("getme")]
            public ActionResult GetMe()
            {
                throw new NotImplementedException();
            }

            public ActionResult IDontGetARoute()
            {
                throw new NotImplementedException();
            }
        }

        [RoutePrefix("/pref")]
        private class Bad1Controller : Controller
        {
            [HttpGet]
            [Route("getme")]
            public ActionResult GetMe()
            {
                throw new NotImplementedException();
            }
        }

        [RoutePrefix("pref/")]
        private class Bad2Controller : Controller
        {
            [HttpGet]
            [Route("getme")]
            public ActionResult GetMe()
            {
                throw new NotImplementedException();
            }
        }

        private class Bad3Controller : Controller
        {
            [HttpGet]
            [Route("/getme")]
            public ActionResult GetMe()
            {
                throw new NotImplementedException();
            }
        }

        private class Bad4Controller : Controller
        {
            [HttpGet]
            [Route("getme/")]
            public ActionResult GetMe()
            {
                throw new NotImplementedException();
            }
        }

        [RouteArea("PugetSound", AreaPrefix = "/puget-sound")]
        private class Bad5Controller : Controller
        {
            [HttpGet]
            [Route("getme")]
            public ActionResult GetMe()
            {
                throw new NotImplementedException();
            }
        }

        [RouteArea("PugetSound", AreaPrefix = "puget-sound/")]
        private class Bad6Controller : Controller
        {
            [HttpGet]
            [Route("getme")]
            public ActionResult GetMe()
            {
                throw new NotImplementedException();
            }
        }

        private class FruitConstraintController : Controller
        {
            [HttpGet]
            [Route("fruits/{apple:fruit}")]
            public ActionResult Eat(string apple)
            {
                throw new NotImplementedException();
            }
        }

        class FruitConstraint : IRouteConstraint
        {
            public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values,
                              RouteDirection routeDirection)
            {
                throw new NotImplementedException();
            }
        }

        class FruitConstraintResolver : IInlineConstraintResolver
        {
            public IRouteConstraint ResolveConstraint(string inlineConstraint)
            {
                if (inlineConstraint == "fruit")
                {
                    return new FruitConstraint();
                }

                throw new InvalidOperationException();
            }
        }

        private IReadOnlyCollection<RouteBase> GetAttributeRoutes(RouteCollection routes)
        {
            return routes.OfType<IReadOnlyCollection<RouteBase>>().Single();
        }
    }
}
