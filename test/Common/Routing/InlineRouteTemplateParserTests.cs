// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
#if ASPNETWEBAPI
using System.Web.Http.Routing.Constraints;
#else
using System.Web.Mvc.Routing.Constraints;
using System.Web.Routing;
#endif
using Microsoft.TestCommon;

#if ASPNETWEBAPI
namespace System.Web.Http.Routing
#else
namespace System.Web.Mvc.Routing
#endif
{
    public class InlineRouteTemplateParserTests
    {
#if ASPNETWEBAPI
        private static readonly RouteParameter OptionalParameter = RouteParameter.Optional;
#else
        private static readonly UrlParameter OptionalParameter = UrlParameter.Optional;
#endif

        [Fact]
        public void ParseRouteTemplate_ChainedConstraintAndDefault()
        {
            var result = Act(@"hello/{param:int=111111}");

            Assert.Equal("hello/{param}", result.RouteUrl);
            Assert.Equal("111111", result.Defaults["param"]);
            Assert.IsType<IntRouteConstraint>(result.Constraints["param"]);
        }

        [Fact]
        public void ParseRouteTemplate_ChainedConstraintWithArgumentsAndDefault()
        {
            var result = Act(@"hello/{param:regex(\d+)=111111}");

            Assert.Equal("hello/{param}", result.RouteUrl);
            Assert.Equal("111111", result.Defaults["param"]);
            var regexRouteConstraint = Assert.IsType<RegexRouteConstraint>(result.Constraints["param"]);
            Assert.Equal(@"\d+", regexRouteConstraint.Pattern);
        }

        [Fact]
        public void ParseRouteTemplate_ChainedConstraintAndOptional()
        {
            var result = Act(@"hello/{param:int?}");

            Assert.Equal("hello/{param}", result.RouteUrl);

            Assert.Equal(OptionalParameter, result.Defaults["param"]);

            var constraint = Assert.IsType<OptionalRouteConstraint>(result.Constraints["param"]);
            Assert.IsType<IntRouteConstraint>(constraint.InnerConstraint);
        }

        [Fact]
        public void ParseRouteTemplate_ChainedConstraintWithArgumentsAndOptional()
        {
            var result = Act(@"hello/{param:regex(\d+)?}");

            Assert.Equal("hello/{param}", result.RouteUrl);

            Assert.Equal(OptionalParameter, result.Defaults["param"]);

            var constraint = Assert.IsType<OptionalRouteConstraint>(result.Constraints["param"]);
            Assert.Equal(@"\d+", Assert.IsType<RegexRouteConstraint>(constraint.InnerConstraint).Pattern);
        }

        [Fact]
        public void ParseRouteTemplate_ChainedConstraints()
        {
            var result = Act(@"hello/{param:regex(\d+):regex(\w+)}");

            Assert.Equal("hello/{param}", result.RouteUrl);

            CompoundRouteConstraint constraint = Assert.IsType<CompoundRouteConstraint>(result.Constraints["param"]);
            Assert.Equal(@"\d+", Assert.IsType<RegexRouteConstraint>(constraint.Constraints.ElementAt(0)).Pattern);
            Assert.Equal(@"\w+", Assert.IsType<RegexRouteConstraint>(constraint.Constraints.ElementAt(1)).Pattern);
        }

        [Fact]
        public void ParseRouteTemplate_Constraint()
        {
            var result = Act(@"hello/{param:regex(\d+)}");

            Assert.Equal("hello/{param}", result.RouteUrl);

            var constraint = Assert.IsType<RegexRouteConstraint>(result.Constraints["param"]);
            Assert.Equal(@"\d+", constraint.Pattern);
        }

        [Fact]
        public void ParseRouteTemplate_ConstraintsDefaultsAndOptionalsInMultipleSections()
        {
            var result = Act(@"some/url-{p1:alpha:length(3)=hello}/{p2=abc}/{p3?}");

            Assert.Equal("some/url-{p1}/{p2}/{p3}", result.RouteUrl);

            Assert.Equal("hello", result.Defaults["p1"]);
            Assert.Equal("abc", result.Defaults["p2"]);
            Assert.Equal(OptionalParameter, result.Defaults["p3"]);

            CompoundRouteConstraint constraint = Assert.IsType<CompoundRouteConstraint>(result.Constraints["p1"]);
            Assert.IsType<AlphaRouteConstraint>(constraint.Constraints.ElementAt(0));
            Assert.IsType<LengthRouteConstraint>(constraint.Constraints.ElementAt(1));
        }

        [Fact]
        public void ParseRouteTemplate_NoTokens()
        {
            var result = Act("hello/world");

            Assert.Equal("hello/world", result.RouteUrl);
        }

        [Fact]
        public void ParseRouteTemplate_OptionalParam()
        {
            var result = Act("hello/{param?}");

            Assert.Equal("hello/{param}", result.RouteUrl);
            Assert.Equal(OptionalParameter, result.Defaults["param"]);
        }

        [Fact]
        public void ParseRouteTemplate_ParamDefault()
        {
            var result = Act("hello/{param=world}");

            Assert.Equal("hello/{param}", result.RouteUrl);
            Assert.Equal("world", result.Defaults["param"]);
        }

        [Fact]
        public void ParseRouteTemplate_RegexConstraintWithClosingBraceInPattern()
        {
            var result = Act(@"hello/{param:regex(\})}");

            Assert.Equal("hello/{param}", result.RouteUrl);

            var constraint = Assert.IsType<RegexRouteConstraint>(result.Constraints["param"]);
            Assert.Equal(@"\}", constraint.Pattern);
        }

        [Fact]
        public void ParseRouteTemplate_RegexConstraintWithClosingParenInPattern()
        {
            var result = Act(@"hello/{param:regex(\))}");

            Assert.Equal("hello/{param}", result.RouteUrl);

            var constraint = Assert.IsType<RegexRouteConstraint>(result.Constraints["param"]);
            Assert.Equal(@"\)", constraint.Pattern);
        }

        [Fact]
        public void ParseRouteTemplate_RegexConstraintWithColonInPattern()
        {
            var result = Act(@"hello/{param:regex(:)}");

            Assert.Equal("hello/{param}", result.RouteUrl);
            var constraint = Assert.IsType<RegexRouteConstraint>(result.Constraints["param"]);
            Assert.Equal(@":", constraint.Pattern);
        }

        [Fact]
        public void ParseRouteTemplate_RegexConstraintWithCommaInPattern()
        {
            var result = Act(@"hello/{param:regex(\w,\w)}");

            Assert.Equal("hello/{param}", result.RouteUrl);
            var constraint = Assert.IsType<RegexRouteConstraint>(result.Constraints["param"]);
            Assert.Equal(@"\w,\w", constraint.Pattern);
        }

        [Fact]
        public void ParseRouteTemplate_RegexConstraintWithEqualsSignInPattern()
        {
            var result = Act(@"hello/{param:regex(=)}");

            Assert.Equal("hello/{param}", result.RouteUrl);

            Assert.DoesNotContain("param", result.Defaults.Keys);
            var constraint = Assert.IsType<RegexRouteConstraint>(result.Constraints["param"]);
            Assert.Equal(@"=", constraint.Pattern);
        }

        [Fact]
        public void ParseRouteTemplate_RegexConstraintWithOpenBraceInPattern()
        {
            var result = Act(@"hello/{param:regex(\{)}");

            Assert.Equal("hello/{param}", result.RouteUrl);
            var constraint = Assert.IsType<RegexRouteConstraint>(result.Constraints["param"]);
            Assert.Equal(@"\{", constraint.Pattern);
        }

        [Fact]
        public void ParseRouteTemplate_RegexConstraintWithOpenParenInPattern()
        {
            var result = Act(@"hello/{param:regex(\()}");

            Assert.Equal("hello/{param}", result.RouteUrl);
            var constraint = Assert.IsType<RegexRouteConstraint>(result.Constraints["param"]);
            Assert.Equal(@"\(", constraint.Pattern);
        }

        [Fact]
        public void ParseRouteTemplate_RegexConstraintWithQuestionMarkInPattern()
        {
            var result = Act(@"hello/{param:regex(\?)}");

            Assert.Equal("hello/{param}", result.RouteUrl);
            Assert.DoesNotContain("param", result.Defaults.Keys);
            var constraint = Assert.IsType<RegexRouteConstraint>(result.Constraints["param"]);
            Assert.Equal(@"\?", constraint.Pattern);
        }


        private ParseResult Act(string template)
        {
            var result = new ParseResult();
 #if ASPNETWEBAPI
            result.Constraints = new HttpRouteValueDictionary();
            result.Defaults = new HttpRouteValueDictionary();
#else
            result.Constraints = new RouteValueDictionary();
            result.Defaults = new RouteValueDictionary();
#endif
            result.RouteUrl = InlineRouteTemplateParser.ParseRouteTemplate(template, result.Defaults, result.Constraints, new DefaultInlineConstraintResolver());
            return result;
        }

        struct ParseResult
        {
            public string RouteUrl;
 #if ASPNETWEBAPI
            public HttpRouteValueDictionary Defaults;
            public HttpRouteValueDictionary Constraints;
#else
            public RouteValueDictionary Defaults;
            public RouteValueDictionary Constraints;
#endif
        }
    }
}
