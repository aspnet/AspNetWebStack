// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Routing;
using Microsoft.TestCommon;
using Microsoft.Web.UnitTestUtil;

namespace System.Web.Mvc.Html.Test
{
    public class LinkExtensionsTest
    {
        private const string AppPathModifier = MvcHelper.AppPathModifier;

        [Fact]
        public void ActionLink()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper();

            // Act
            MvcHtmlString html = htmlHelper.ActionLink("linktext", "newaction");

            // Assert
            Assert.Equal(@"<a href=""" + AppPathModifier + @"/app/home/newaction"">linktext</a>", html.ToHtmlString());
        }

        [Theory]
        [PropertyData("UrlEncodedData_NoHtmlEncode", PropertyType = typeof(EncodedDataSets))]
        public void ActionLink_UrlEncodesAction(string text, string expectedText)
        {
            // Arrange
            var helper = MvcHelper.GetHtmlHelper();

            // Act
            var result = helper.ActionLink(linkText: "link", actionName: text);

            // Assert
            Assert.Equal(
                @"<a href=""" + AppPathModifier + @"/app/home/" + expectedText + @""">link</a>",
                result.ToHtmlString());
        }

        [Fact]
        public void ActionLinkDictionaryOverridesImplicitValues()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper();

            // Act
            MvcHtmlString html = htmlHelper.ActionLink("linktext", "newaction", null, new { href = "http://foo.com" });

            // Assert
            Assert.Equal(@"<a href=""http://foo.com"">linktext</a>", html.ToHtmlString());
        }

        [Fact]
        public void ActionLinkExplictValuesOverrideDictionary()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper();

            // Act
            MvcHtmlString html = htmlHelper.ActionLink("linktext", "explicitAction", new { action = "dictionaryAction" }, null);

            // Assert
            Assert.Equal(@"<a href=""" + AppPathModifier + @"/app/home/explicitAction"">linktext</a>", html.ToHtmlString());
        }

        [Fact]
        public void ActionLinkParametersNeedEscaping()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper();

            // Act
            MvcHtmlString html = htmlHelper.ActionLink("linktext<&>\"", "new action<&>\"");

            // Assert
            Assert.Equal(@"<a href=""" + AppPathModifier + @"/app/home/new%20action%3C%26%3E%22"">linktext&lt;&amp;&gt;&quot;</a>", html.ToHtmlString());
        }

        [Fact]
        public void ActionLinkWithActionNameAndValueDictionary()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper();

            // Act
            MvcHtmlString html = htmlHelper.ActionLink("linktext", "newaction", new RouteValueDictionary(new { controller = "home2" }));

            // Assert
            Assert.Equal(@"<a href=""" + AppPathModifier + @"/app/home2/newaction"">linktext</a>", html.ToHtmlString());
        }

        [Fact]
        public void ActionLinkWithActionNameAndValueObject()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper();

            // Act
            MvcHtmlString html = htmlHelper.ActionLink("linktext", "newaction", new { controller = "home2" });

            // Assert
            Assert.Equal(@"<a href=""" + AppPathModifier + @"/app/home2/newaction"">linktext</a>", html.ToHtmlString());
        }

        [Fact]
        public void ActionLinkWithControllerName()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper();

            // Act
            MvcHtmlString html = htmlHelper.ActionLink("linktext", "newaction", "home2");

            // Assert
            Assert.Equal(@"<a href=""" + AppPathModifier + @"/app/home2/newaction"">linktext</a>", html.ToHtmlString());
        }

        [Theory]
        [PropertyData("UrlEncodedData_NoHtmlEncode", PropertyType = typeof(EncodedDataSets))]
        public void ActionLinkWithControllerName_UrlEncodesController(string text, string expectedText)
        {
            // Arrange
            var helper = MvcHelper.GetHtmlHelper();

            // Act
            var result = helper.ActionLink(linkText: "link", actionName: "newAction", controllerName: text);

            // Assert
            Assert.Equal(
                @"<a href=""" + AppPathModifier + @"/app/" + expectedText + @"/newAction"">link</a>",
                result.ToHtmlString());
        }

        [Fact]
        public void ActionLinkWithControllerNameAndDictionary()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper();

            // Act
            MvcHtmlString html = htmlHelper.ActionLink("linktext", "newaction", "home2", new RouteValueDictionary(new { id = "someid" }), new RouteValueDictionary(new { baz = "baz" }));

            // Assert
            Assert.Equal(@"<a baz=""baz"" href=""" + AppPathModifier + @"/app/home2/newaction/someid"">linktext</a>", html.ToHtmlString());
        }

        [Fact]
        public void ActionLinkWithControllerNameAndObjectProperties()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper();

            // Act
            MvcHtmlString html = htmlHelper.ActionLink("linktext", "newaction", "home2", new { id = "someid" }, new { baz = "baz" });

            // Assert
            Assert.Equal(@"<a baz=""baz"" href=""" + AppPathModifier + @"/app/home2/newaction/someid"">linktext</a>", html.ToHtmlString());
        }

        [Theory]
        [PropertyData("AttributeEncodedData_NoHtmlEncode", PropertyType = typeof(EncodedDataSets))]
        public void ActionLinkWithControllerNameAndObjectProperties_AttributeEncodes_AddedHtmlAttributes(
            string text,
            string expectedText)
        {
            // Arrange
            var helper = MvcHelper.GetHtmlHelper();

            // Act
            var result =
                helper.ActionLink("text", "action", "controller", routeValues: null, htmlAttributes: new { attribute = text });

            // Assert
            Assert.Equal(
                @"<a attribute=""" + expectedText + @""" href=""" + AppPathModifier + @"/app/controller/action"">text</a>",
                result.ToHtmlString());
        }

        [Fact]
        public void ActionLinkWithControllerNameAndObjectPropertiesWithUnderscores()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper();

            // Act
            MvcHtmlString html = htmlHelper.ActionLink("linktext", "newaction", "home2", new { id = "someid" }, new { foo_baz = "baz" });

            // Assert
            Assert.Equal(@"<a foo-baz=""baz"" href=""" + AppPathModifier + @"/app/home2/newaction/someid"">linktext</a>", html.ToHtmlString());
        }

        [Fact]
        public void ActionLinkWithDictionary()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper();

            // Act
            MvcHtmlString html = htmlHelper.ActionLink("linktext", "newaction", new RouteValueDictionary(new { Controller = "home2", id = "someid" }), new RouteValueDictionary(new { baz = "baz" }));

            // Assert
            Assert.Equal(@"<a baz=""baz"" href=""" + AppPathModifier + @"/app/home2/newaction/someid"">linktext</a>", html.ToHtmlString());
        }

        [Fact]
        public void ActionLinkWithFragment()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper();

            // Act
            MvcHtmlString html = htmlHelper.ActionLink("linktext", "newaction", "home2", "http", "foo.bar.com", "foo", new { id = "someid" }, new { baz = "baz" });

            // Assert
            Assert.Equal(@"<a baz=""baz"" href=""http://foo.bar.com" + AppPathModifier + @"/app/home2/newaction/someid#foo"">linktext</a>", html.ToHtmlString());
        }

        [Fact]
        public void ActionLinkWithFragmentAndAttributesWithUnderscores()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper();

            // Act
            MvcHtmlString html = htmlHelper.ActionLink("linktext", "newaction", "home2", "http", "foo.bar.com", "foo", new { id = "someid" }, new { foo_baz = "baz" });

            // Assert
            Assert.Equal(@"<a foo-baz=""baz"" href=""http://foo.bar.com" + AppPathModifier + @"/app/home2/newaction/someid#foo"">linktext</a>", html.ToHtmlString());
        }

        [Fact]
        public void ActionLinkWithNullHostname()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper();

            // Act
            MvcHtmlString html = htmlHelper.ActionLink("linktext", "newaction", "home2", "https", null /* hostName */, "foo", new { id = "someid" }, new { baz = "baz" });

            // Assert
            Assert.Equal(@"<a baz=""baz"" href=""https://localhost" + AppPathModifier + @"/app/home2/newaction/someid#foo"">linktext</a>", html.ToHtmlString());
        }

        [Fact]
        public void ActionLinkWithNullProtocolAndFragment()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper();

            // Act
            MvcHtmlString html = htmlHelper.ActionLink("linktext", "newaction", "home2", null /* protocol */, "foo.bar.com", null /* fragment */, new { id = "someid" }, new { baz = "baz" });

            // Assert
            Assert.Equal(@"<a baz=""baz"" href=""http://foo.bar.com" + AppPathModifier + @"/app/home2/newaction/someid"">linktext</a>", html.ToHtmlString());
        }

        [Fact]
        public void ActionLinkWithNullProtocolNullHostNameAndNullFragment()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper();

            // Act
            MvcHtmlString html = htmlHelper.ActionLink("linktext", "newaction", "home2", null /* protocol */, null /* hostName */, null /* fragment */, new { id = "someid" }, new { baz = "baz" });

            // Assert
            Assert.Equal(@"<a baz=""baz"" href=""" + AppPathModifier + @"/app/home2/newaction/someid"">linktext</a>", html.ToHtmlString());
        }

        [Fact]
        public void ActionLinkWithObjectProperties()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper();

            // Act
            MvcHtmlString html = htmlHelper.ActionLink("linktext", "newaction", new { Controller = "home2", id = "someid" }, new { baz = "baz" });

            // Assert
            Assert.Equal(@"<a baz=""baz"" href=""" + AppPathModifier + @"/app/home2/newaction/someid"">linktext</a>", html.ToHtmlString());
        }

        [Fact]
        public void ActionLinkWithObjectPropertiesWithUnderscores()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper();

            // Act
            MvcHtmlString html = htmlHelper.ActionLink("linktext", "newaction", new { Controller = "home2", id = "someid" }, new { foo_baz = "baz" });

            // Assert
            Assert.Equal(@"<a foo-baz=""baz"" href=""" + AppPathModifier + @"/app/home2/newaction/someid"">linktext</a>", html.ToHtmlString());
        }

        [Fact]
        public void ActionLinkWithProtocol()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper();

            // Act
            MvcHtmlString html = htmlHelper.ActionLink("linktext", "newaction", "home2", "https", "foo.bar.com", null /* fragment */, new { id = "someid" }, new { baz = "baz" });

            // Assert
            Assert.Equal(@"<a baz=""baz"" href=""https://foo.bar.com" + AppPathModifier + @"/app/home2/newaction/someid"">linktext</a>", html.ToHtmlString());
        }

        [Fact]
        public void ActionLinkWithProtocolAndFragment()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper();

            // Act
            MvcHtmlString html = htmlHelper.ActionLink("linktext", "newaction", "home2", "https", "foo.bar.com", "foo", new { id = "someid" }, new { baz = "baz" });

            // Assert
            Assert.Equal(@"<a baz=""baz"" href=""https://foo.bar.com" + AppPathModifier + @"/app/home2/newaction/someid#foo"">linktext</a>", html.ToHtmlString());
        }

        [Fact]
        public void ActionLinkWithDefaultPort()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper(Uri.UriSchemeHttps, -1);

            // Act
            MvcHtmlString html = htmlHelper.ActionLink("linktext", "newaction", "home2", "https", "foo.bar.com", "foo", new { id = "someid" }, new { baz = "baz" });

            // Assert
            Assert.Equal(@"<a baz=""baz"" href=""https://foo.bar.com" + AppPathModifier + @"/app/home2/newaction/someid#foo"">linktext</a>", html.ToHtmlString());
        }

        [Fact]
        public void ActionLinkWithDifferentPortProtocols()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper(Uri.UriSchemeHttp, -1);

            // Act
            MvcHtmlString html = htmlHelper.ActionLink("linktext", "newaction", "home2", "https", "foo.bar.com", "foo", new { id = "someid" }, new { baz = "baz" });

            // Assert
            Assert.Equal(@"<a baz=""baz"" href=""https://foo.bar.com" + AppPathModifier + @"/app/home2/newaction/someid#foo"">linktext</a>", html.ToHtmlString());
        }

        [Fact]
        public void ActionLinkWithNonDefaultPortAndDifferentProtocol()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper(Uri.UriSchemeHttp, 32768);

            // Act
            MvcHtmlString html = htmlHelper.ActionLink("linktext", "newaction", "home2", "https", "foo.bar.com", "foo", new { id = "someid" }, new { baz = "baz" });

            // Assert
            Assert.Equal(@"<a baz=""baz"" href=""https://foo.bar.com" + AppPathModifier + @"/app/home2/newaction/someid#foo"">linktext</a>", html.ToHtmlString());
        }

        [Fact]
        public void ActionLinkWithNonDefaultPortAndSameProtocol()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper(Uri.UriSchemeHttp, 32768);

            // Act
            MvcHtmlString html = htmlHelper.ActionLink("linktext", "newaction", "home2", "http", "foo.bar.com", "foo", new { id = "someid" }, new { baz = "baz" });

            // Assert
            Assert.Equal(@"<a baz=""baz"" href=""http://foo.bar.com:32768" + AppPathModifier + @"/app/home2/newaction/someid#foo"">linktext</a>", html.ToHtmlString());
        }

        [Fact]
        public void LinkGenerationDoesNotChangeProvidedDictionary()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper();
            RouteValueDictionary valuesDictionary = new RouteValueDictionary();

            // Act
            htmlHelper.ActionLink("linkText", "actionName", valuesDictionary, new RouteValueDictionary());

            // Assert
            Assert.Empty(valuesDictionary);
            Assert.False(valuesDictionary.ContainsKey("action"));
        }

        [Fact]
        public void ActionLinkProducesLowercaseUrls()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper();
            htmlHelper.RouteCollection.LowercaseUrls = true;

            // Act
            MvcHtmlString html = htmlHelper.ActionLink("about", "About", "Home");

            // Assert
            Assert.True(html.ToHtmlString() == html.ToHtmlString().ToLowerInvariant());
        }

        [Fact]
        public void ActionLinkProducesLowercaseUrlsAfterRegisteringAnArea()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper();
            MyAreaRegistration myArea = new MyAreaRegistration();
            myArea.CreateContextAndRegister(htmlHelper.RouteCollection, null);
            htmlHelper.RouteCollection.LowercaseUrls = true;

            // Act
            MvcHtmlString html = htmlHelper.ActionLink("about", "About", "Home");

            // Assert
            Assert.True(html.ToHtmlString() == html.ToHtmlString().ToLowerInvariant());
        }

        [Fact]
        public void NullOrEmptyStringParameterThrows()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper();
            var tests = new[]
            {
                // ActionLink(string linkText, string actionName)
                new { Parameter = "linkText", Action = new Action(() => htmlHelper.ActionLink(String.Empty, "actionName")) },
                // ActionLink(string linkText, string actionName, object routeValues, object htmlAttributes)
                new { Parameter = "linkText", Action = new Action(() => htmlHelper.ActionLink(String.Empty, "actionName", new Object(), null /* htmlAttributes */)) },
                // ActionLink(string linkText, string actionName, RouteValueDictionary routeValues, IDictionary<string, object> htmlAttributes)
                new { Parameter = "linkText", Action = new Action(() => htmlHelper.ActionLink(String.Empty, "actionName", new RouteValueDictionary(), new RouteValueDictionary())) },
                // ActionLink(string linkText, string actionName, string controllerName)
                new { Parameter = "linkText", Action = new Action(() => htmlHelper.ActionLink(String.Empty, "actionName", "controllerName")) },
                // ActionLink(string linkText, string actionName, string controllerName, object routeValues, object htmlAttributes)
                new { Parameter = "linkText", Action = new Action(() => htmlHelper.ActionLink(String.Empty, "actionName", "controllerName", new Object(), null /* htmlAttributes */)) },
                // ActionLink(string linkText, string actionName, string controllerName, RouteValueDictionary routeValues, IDictionary<string, object> htmlAttributes)
                new { Parameter = "linkText", Action = new Action(() => htmlHelper.ActionLink(String.Empty, "actionName", "controllerName", new RouteValueDictionary(), new RouteValueDictionary())) },
                // ActionLink(string linkText, string actionName, string controllerName, string protocol, string hostName, string fragment, RouteValueDictionary routeValues, IDictionary<string, object> htmlAttributes)
                new { Parameter = "linkText", Action = new Action(() => htmlHelper.ActionLink(String.Empty, "actionName", "controllerName", null, null, null, new RouteValueDictionary(), new RouteValueDictionary())) },
                // RouteLink(string linkText, object routeValues, object htmlAttributes)
                new { Parameter = "linkText", Action = new Action(() => htmlHelper.RouteLink(String.Empty, new Object(), null /* htmlAttributes */)) },
                // RouteLink(string linkText, RouteValueDictionary routeValues, IDictionary<string, object> htmlAttributes)
                new { Parameter = "linkText", Action = new Action(() => htmlHelper.RouteLink(String.Empty, new RouteValueDictionary(), new RouteValueDictionary())) },
                // RouteLink(string linkText, string routeName, object routeValues)
                new { Parameter = "linkText", Action = new Action(() => htmlHelper.RouteLink(String.Empty, "routeName", null /* routeValues */)) },
                // RouteLink(string linkText, string routeName, RouteValueDictionary routeValues)
                new { Parameter = "linkText", Action = new Action(() => htmlHelper.RouteLink(String.Empty, "routeName", new RouteValueDictionary() /* routeValues */)) },
                // RouteLink(string linkText, string routeName)
                new { Parameter = "linkText", Action = new Action(() => htmlHelper.RouteLink(String.Empty, (string)null /* routeName */)) },
                // RouteLink(string linkText, object routeValues)
                new { Parameter = "linkText", Action = new Action(() => htmlHelper.RouteLink(String.Empty, (object)null /* routeValues */)) },
                // RouteLink(string linkText, RouteValueDictionary routeValues)
                new { Parameter = "linkText", Action = new Action(() => htmlHelper.RouteLink(String.Empty, new RouteValueDictionary() /* routeValues */)) },
                // RouteLink(string linkText, string routeName, object routeValues, object htmlAttributes)
                new { Parameter = "linkText", Action = new Action(() => htmlHelper.RouteLink(String.Empty, "routeName", new Object(), null /* htmlAttributes */)) },
                // RouteLink(string linkText, string routeName, RouteValueDictionary routeValues, IDictionary<string, object> htmlAttributes)
                new { Parameter = "linkText", Action = new Action(() => htmlHelper.RouteLink(String.Empty, "routeName", new RouteValueDictionary(), new RouteValueDictionary())) },
                // RouteLink(string linkText, string routeName, string protocol, string hostName, string fragment, RouteValueDictionary routeValues, IDictionary<string, object> htmlAttributes)
                new { Parameter = "linkText", Action = new Action(() => htmlHelper.RouteLink(String.Empty, "routeName", null, null, null, new RouteValueDictionary(), new RouteValueDictionary())) },
            };

            // Act & Assert
            foreach (var test in tests)
            {
                Assert.ThrowsArgumentNullOrEmpty(test.Action, test.Parameter);
            }
        }

        [Fact]
        public void RouteLinkCanUseNamedRouteWithoutSpecifyingDefaults()
        {
            // DevDiv 217072: Non-mvc specific helpers should not give default values for controller and action

            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper();
            htmlHelper.RouteCollection.MapRoute("MyRouteName", "any/url", new { controller = "Charlie" });

            // Act
            MvcHtmlString html = htmlHelper.RouteLink("linktext", "MyRouteName", null /* routeValues */);

            // Assert
            Assert.Equal(@"<a href=""" + AppPathModifier + @"/app/any/url"">linktext</a>", html.ToHtmlString());
        }

        [Fact]
        public void RouteLinkWithDictionary()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper();

            // Act
            MvcHtmlString html = htmlHelper.RouteLink("linktext", new RouteValueDictionary(new { Action = "newaction", Controller = "home2", id = "someid" }), new RouteValueDictionary(new { baz = "baz" }));

            // Assert
            Assert.Equal(@"<a baz=""baz"" href=""" + AppPathModifier + @"/app/home2/newaction/someid"">linktext</a>", html.ToHtmlString());
        }

        [Fact]
        public void RouteLinkWithFragment()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper();

            // Act
            MvcHtmlString html = htmlHelper.RouteLink("linktext", "namedroute", "http", "foo.bar.com", "foo", new { Action = "newaction", Controller = "home2", id = "someid" }, new { baz = "baz" });

            // Assert
            Assert.Equal(@"<a baz=""baz"" href=""http://foo.bar.com" + AppPathModifier + @"/app/named/home2/newaction/someid#foo"">linktext</a>", html.ToHtmlString());
        }

        [Fact]
        public void RouteLinkWithFragmentAndAttributesWithUnderscores()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper();

            // Act
            MvcHtmlString html = htmlHelper.RouteLink("linktext", "namedroute", "http", "foo.bar.com", "foo", new { Action = "newaction", Controller = "home2", id = "someid" }, new { foo_baz = "baz" });

            // Assert
            Assert.Equal(@"<a foo-baz=""baz"" href=""http://foo.bar.com" + AppPathModifier + @"/app/named/home2/newaction/someid#foo"">linktext</a>", html.ToHtmlString());
        }

        [Fact]
        public void RouteLinkWithLinkTextAndRouteName()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper();
            htmlHelper.RouteCollection.MapRoute("MyRouteName", "any/url", new { controller = "Charlie" });

            // Act
            MvcHtmlString html = htmlHelper.RouteLink("linktext", "MyRouteName");

            // Assert
            Assert.Equal(@"<a href=""" + AppPathModifier + @"/app/any/url"">linktext</a>", html.ToHtmlString());
        }

        [Fact]
        public void RouteLinkWithObjectProperties()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper();

            // Act
            MvcHtmlString html = htmlHelper.RouteLink("linktext", new { Action = "newaction", Controller = "home2", id = "someid" }, new { baz = "baz" });

            // Assert
            Assert.Equal(@"<a baz=""baz"" href=""" + AppPathModifier + @"/app/home2/newaction/someid"">linktext</a>", html.ToHtmlString());
        }

        [Theory]
        [PropertyData("AttributeEncodedData_NoHtmlEncode", PropertyType = typeof(EncodedDataSets))]
        public void RouteLinkWithObjectProperties_AttributeEncodes_AddedHtmlAttributes(
            string text,
            string expectedText)
        {
            // Arrange
            var helper = MvcHelper.GetHtmlHelper();

            // Act
            var result = helper.RouteLink("text", routeValues: null, htmlAttributes: new { attribute = text });

            // Assert
            Assert.Equal(
                @"<a attribute=""" + expectedText + @""" href=""" + AppPathModifier + @"/app/home/oldaction"">text</a>",
                result.ToHtmlString());
        }

        [Fact]
        public void RouteLinkWithObjectPropertiesWithUnderscores()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper();

            // Act
            MvcHtmlString html = htmlHelper.RouteLink("linktext", new { Action = "newaction", Controller = "home2", id = "someid" }, new { foo_baz = "baz" });

            // Assert
            Assert.Equal(@"<a foo-baz=""baz"" href=""" + AppPathModifier + @"/app/home2/newaction/someid"">linktext</a>", html.ToHtmlString());
        }

        [Fact]
        public void RouteLinkWithProtocol()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper();

            // Act
            MvcHtmlString html = htmlHelper.RouteLink("linktext", "namedroute", "https", "foo.bar.com", null /* fragment */, new { Action = "newaction", Controller = "home2", id = "someid" }, new { baz = "baz" });

            // Assert
            Assert.Equal(@"<a baz=""baz"" href=""https://foo.bar.com" + AppPathModifier + @"/app/named/home2/newaction/someid"">linktext</a>", html.ToHtmlString());
        }

        [Fact]
        public void RouteLinkWithProtocolAndFragment()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper();

            // Act
            MvcHtmlString html = htmlHelper.RouteLink("linktext", "namedroute", "https", "foo.bar.com", "foo", new { Action = "newaction", Controller = "home2", id = "someid" }, new { baz = "baz" });

            // Assert
            Assert.Equal(@"<a baz=""baz"" href=""https://foo.bar.com" + AppPathModifier + @"/app/named/home2/newaction/someid#foo"">linktext</a>", html.ToHtmlString());
        }

        [Fact]
        public void RouteLinkWithRouteNameAndDefaults()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper();

            // Act
            MvcHtmlString html = htmlHelper.RouteLink("linktext", "namedroute", new { Action = "newaction" });

            // Assert
            Assert.Equal(@"<a href=""" + AppPathModifier + @"/app/named/home/newaction"">linktext</a>", html.ToHtmlString());
        }

        [Fact]
        public void RouteLinkWithRouteNameAndDictionary()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper();

            // Act
            MvcHtmlString html = htmlHelper.RouteLink("linktext", "namedroute", new RouteValueDictionary(new { Action = "newaction", Controller = "home2", id = "someid" }), new RouteValueDictionary());

            // Assert
            Assert.Equal(@"<a href=""" + AppPathModifier + @"/app/named/home2/newaction/someid"">linktext</a>", html.ToHtmlString());
        }

        [Fact]
        public void RouteLinkWithRouteNameAndObjectProperties()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper();

            // Act
            MvcHtmlString html = htmlHelper.RouteLink("linktext", "namedroute", new { Action = "newaction", Controller = "home2", id = "someid" }, new { baz = "baz" });

            // Assert
            Assert.Equal(@"<a baz=""baz"" href=""" + AppPathModifier + @"/app/named/home2/newaction/someid"">linktext</a>", html.ToHtmlString());
        }

        [Fact]
        public void RouteLinkWithRouteNameAndObjectPropertiesWithUnderscores()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper();

            // Act
            MvcHtmlString html = htmlHelper.RouteLink("linktext", "namedroute", new { Action = "newaction", Controller = "home2", id = "someid" }, new { foo_baz = "baz" });

            // Assert
            Assert.Equal(@"<a foo-baz=""baz"" href=""" + AppPathModifier + @"/app/named/home2/newaction/someid"">linktext</a>", html.ToHtmlString());
        }

        // Class for the ActionLinkProducesLowercaseUrlsAfterRegisteringAnArea test
        private class MyAreaRegistration : AreaRegistration
        {
            public override string AreaName
            {
                get
                {
                    return "MyArea";
                }
            }

            public override void RegisterArea(AreaRegistrationContext context)
            {
                context.MapRoute(
                    "MyArea_default",
                    "MyArea/{controller}/{action}/{id}",
                    new { action = "Index", id = UrlParameter.Optional }
                );
            }
        }
    }
}
