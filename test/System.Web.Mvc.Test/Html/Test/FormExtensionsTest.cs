// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Web.Routing;
using System.Web.WebPages.Scope;
using Microsoft.TestCommon;
using Microsoft.Web.UnitTestUtil;
using Moq;

namespace System.Web.Mvc.Html.Test
{
    [Xunit.Collection("Uses ScopeStorage or ViewEngines.Engines")]
    public class FormExtensionsTest : IDisposable
    {
        private static void BeginFormHelper(Func<HtmlHelper, MvcForm> beginForm, string expectedFormTag)
        {
            // Arrange
            StringWriter writer;
            HtmlHelper htmlHelper = GetFormHelper(out writer);

            // Act
            IDisposable formDisposable = beginForm(htmlHelper);
            formDisposable.Dispose();

            // Assert
            Assert.Equal(expectedFormTag + "</form>", writer.ToString());
        }

        [Fact]
        public void BeginFormParameterDictionaryMerging()
        {
            BeginFormHelper(
                htmlHelper => htmlHelper.BeginForm("bar", "foo", FormMethod.Get, new RouteValueDictionary(new { method = "post" })),
                @"<form action=""" + MvcHelper.AppPathModifier + @"/foo/bar"" method=""get"">");
        }

        [Fact]
        public void BeginFormSetsAndRestoresToDefault()
        {
            // Arrange
            StringWriter writer;
            HtmlHelper htmlHelper = GetFormHelper(out writer);

            htmlHelper.ViewContext.FormContext = null;
            FormContext defaultFormContext = htmlHelper.ViewContext.FormContext;

            // Act & assert - push
            MvcForm theForm = htmlHelper.BeginForm();
            Assert.NotNull(htmlHelper.ViewContext.FormContext);
            Assert.NotEqual(defaultFormContext, htmlHelper.ViewContext.FormContext);

            // Act & assert - pop
            theForm.Dispose();
            Assert.Equal(defaultFormContext, htmlHelper.ViewContext.FormContext);
            Assert.Equal(@"<form action=""/some/path"" method=""post""></form>", writer.ToString());
        }

        [Fact]
        public void BeginFormWithClientValidationEnabled()
        {
            // Arrange
            StringWriter writer;
            HtmlHelper htmlHelper = GetFormHelper(out writer);

            htmlHelper.ViewContext.ClientValidationEnabled = true;
            htmlHelper.ViewContext.FormContext = null;
            FormContext defaultFormContext = htmlHelper.ViewContext.FormContext;

            // Act & assert - push
            MvcForm theForm = htmlHelper.BeginForm();
            Assert.NotNull(htmlHelper.ViewContext.FormContext);
            Assert.NotEqual(defaultFormContext, htmlHelper.ViewContext.FormContext);
            Assert.Equal("form_id", htmlHelper.ViewContext.FormContext.FormId);

            // Act & assert - pop
            theForm.Dispose();
            Assert.Equal(defaultFormContext, htmlHelper.ViewContext.FormContext);
            Assert.Equal(@"<form action=""/some/path"" id=""form_id"" method=""post""></form><script type=""text/javascript"">
//<![CDATA[
if (!window.mvcClientValidationMetadata) { window.mvcClientValidationMetadata = []; }
window.mvcClientValidationMetadata.push({""Fields"":[],""FormId"":""form_id"",""ReplaceValidationSummary"":false});
//]]>
</script>", writer.ToString());
        }

        [Fact]
        public void BeginFormWithClientValidationAndUnobtrusiveJavaScriptEnabled()
        {
            // Arrange
            StringWriter writer;
            HtmlHelper htmlHelper = GetFormHelper(out writer);

            htmlHelper.ViewContext.ClientValidationEnabled = true;
            htmlHelper.ViewContext.UnobtrusiveJavaScriptEnabled = true;

            // Act & assert - push
            MvcForm theForm = htmlHelper.BeginForm();
            Assert.Null(htmlHelper.ViewContext.FormContext.FormId);

            // Act & assert - pop
            theForm.Dispose();
            Assert.Equal(@"<form action=""/some/path"" method=""post""></form>", writer.ToString());
        }

        [Fact]
        public void BeginFormWithActionControllerInvalidFormMethodHtmlValues()
        {
            BeginFormHelper(
                htmlHelper => htmlHelper.BeginForm("bar", "foo", (FormMethod)2, new RouteValueDictionary(new { baz = "baz" })),
                @"<form action=""" + MvcHelper.AppPathModifier + @"/foo/bar"" baz=""baz"" method=""post"">");
        }

        [Fact]
        public void BeginFormWithActionController()
        {
            BeginFormHelper(
                htmlHelper => htmlHelper.BeginForm("bar", "foo"),
                @"<form action=""" + MvcHelper.AppPathModifier + @"/foo/bar"" method=""post"">");
        }

        [Theory]
        [PropertyData("UrlEncodedData_NoHtmlEncode", PropertyType = typeof(EncodedDataSets))]
        public void BeginFormWithActionController_UrlEncodesAction(
            string text,
            string expectedText)
        {
            BeginFormHelper(
                helper => helper.BeginForm(text, "controller"),
                @"<form action=""" + MvcHelper.AppPathModifier + "/controller/" + expectedText + @""" method=""post"">");
        }

        [Theory]
        [PropertyData("UrlEncodedData_NoHtmlEncode", PropertyType = typeof(EncodedDataSets))]
        public void BeginFormWithActionController_UrlEncodesController(
            string text,
            string expectedText)
        {
            BeginFormHelper(
                helper => helper.BeginForm("action", text),
                @"<form action=""" + MvcHelper.AppPathModifier + "/" + expectedText + @"/action"" method=""post"">");
        }

        [Fact]
        public void BeginFormWithActionControllerFormMethodHtmlDictionary()
        {
            BeginFormHelper(
                htmlHelper => htmlHelper.BeginForm("bar", "foo", FormMethod.Get, new RouteValueDictionary(new { baz = "baz" })),
                @"<form action=""" + MvcHelper.AppPathModifier + @"/foo/bar"" baz=""baz"" method=""get"">");
        }

        [Fact]
        public void BeginFormWithActionControllerFormMethodHtmlValues()
        {
            BeginFormHelper(
                htmlHelper => htmlHelper.BeginForm("bar", "foo", FormMethod.Get, new { baz = "baz" }),
                @"<form action=""" + MvcHelper.AppPathModifier + @"/foo/bar"" baz=""baz"" method=""get"">");
        }

        [Theory]
        [PropertyData("AttributeEncodedData_NoHtmlEncode", PropertyType = typeof(EncodedDataSets))]
        public void BeginFormWithActionControllerFormMethodHtmlValues_AttributeEncodes_AddedHtmlAttributes(
            string text,
            string expectedText)
        {
            BeginFormHelper(
                helper => helper.BeginForm("action", "controller", FormMethod.Get, new { attribute = text }),
                @"<form action=""" + MvcHelper.AppPathModifier + @"/controller/action"" attribute=""" +
                    expectedText +
                    @""" method=""get"">");
        }

        [Fact]
        public void BeginFormWithActionControllerFormMethodHtmlValuesWithUnderscores()
        {
            BeginFormHelper(
                htmlHelper => htmlHelper.BeginForm("bar", "foo", FormMethod.Get, new { data_test = "value" }),
                @"<form action=""" + MvcHelper.AppPathModifier + @"/foo/bar"" data-test=""value"" method=""get"">");
        }

        [Fact]
        public void BeginFormWithActionControllerRouteDictionaryFormMethodHtmlDictionary()
        {
            BeginFormHelper(
                htmlHelper => htmlHelper.BeginForm("bar", "foo", new RouteValueDictionary(new { id = "id" }), FormMethod.Get, new RouteValueDictionary(new { baz = "baz" })),
                @"<form action=""" + MvcHelper.AppPathModifier + @"/foo/bar/id"" baz=""baz"" method=""get"">");
        }

        [Fact]
        public void BeginFormWithActionControllerRouteValuesFormMethodHtmlValues()
        {
            BeginFormHelper(
                htmlHelper => htmlHelper.BeginForm("bar", "foo", new { id = "id" }, FormMethod.Get, new { baz = "baz" }),
                @"<form action=""" + MvcHelper.AppPathModifier + @"/foo/bar/id"" baz=""baz"" method=""get"">");
        }

        [Fact]
        public void BeginFormWithActionControllerRouteValuesFormMethodHtmlValuesWithUnderscores()
        {
            BeginFormHelper(
                htmlHelper => htmlHelper.BeginForm("bar", "foo", new { id = "id" }, FormMethod.Get, new { foo_baz = "baz" }),
                @"<form action=""" + MvcHelper.AppPathModifier + @"/foo/bar/id"" foo-baz=""baz"" method=""get"">");
        }

        [Fact]
        public void BeginFormWithActionControllerNullRouteValuesFormMethodNullHtmlValues()
        {
            BeginFormHelper(
                htmlHelper => htmlHelper.BeginForm("bar", "foo", null, FormMethod.Get, null),
                @"<form action=""" + MvcHelper.AppPathModifier + @"/foo/bar"" method=""get"">");
        }

        [Fact]
        public void BeginFormWithRouteValues()
        {
            BeginFormHelper(
                htmlHelper => htmlHelper.BeginForm(new { action = "someOtherAction", id = "id" }),
                @"<form action=""" + MvcHelper.AppPathModifier + @"/home/someOtherAction/id"" method=""post"">");
        }

        [Fact]
        public void BeginFormWithRouteDictionary()
        {
            BeginFormHelper(
                htmlHelper => htmlHelper.BeginForm(new RouteValueDictionary { { "action", "someOtherAction" }, { "id", "id" } }),
                @"<form action=""" + MvcHelper.AppPathModifier + @"/home/someOtherAction/id"" method=""post"">");
        }

        [Fact]
        public void BeginFormWithActionControllerRouteValues()
        {
            BeginFormHelper(
                htmlHelper => htmlHelper.BeginForm("myAction", "myController", new { id = "id", pageNum = "123" }),
                @"<form action=""" + MvcHelper.AppPathModifier + @"/myController/myAction/id?pageNum=123"" method=""post"">");
        }

        [Fact]
        public void BeginFormWithActionControllerRouteDictionary()
        {
            BeginFormHelper(
                htmlHelper => htmlHelper.BeginForm("myAction", "myController", new RouteValueDictionary { { "pageNum", "123" }, { "id", "id" } }),
                @"<form action=""" + MvcHelper.AppPathModifier + @"/myController/myAction/id?pageNum=123"" method=""post"">");
        }

        [Fact]
        public void BeginFormWithActionControllerMethod()
        {
            BeginFormHelper(
                htmlHelper => htmlHelper.BeginForm("myAction", "myController", FormMethod.Get),
                @"<form action=""" + MvcHelper.AppPathModifier + @"/myController/myAction"" method=""get"">");
        }

        [Fact]
        public void BeginFormWithActionControllerRouteValuesMethod()
        {
            BeginFormHelper(
                htmlHelper => htmlHelper.BeginForm("myAction", "myController", new { id = "id", pageNum = "123" }, FormMethod.Get),
                @"<form action=""" + MvcHelper.AppPathModifier + @"/myController/myAction/id?pageNum=123"" method=""get"">");
        }

        [Fact]
        public void BeginFormWithActionControllerRouteDictionaryMethod()
        {
            BeginFormHelper(
                htmlHelper => htmlHelper.BeginForm("myAction", "myController", new RouteValueDictionary { { "pageNum", "123" }, { "id", "id" } }, FormMethod.Get),
                @"<form action=""" + MvcHelper.AppPathModifier + @"/myController/myAction/id?pageNum=123"" method=""get"">");
        }

        [Fact]
        public void BeginFormWithNoParams()
        {
            BeginFormHelper(
                htmlHelper => htmlHelper.BeginForm(),
                @"<form action=""/some/path"" method=""post"">");
        }

        [Fact]
        public void BeginRouteFormWithRouteNameInvalidFormMethodHtmlValues()
        {
            BeginFormHelper(
                htmlHelper => htmlHelper.BeginRouteForm("namedroute", (FormMethod)2, new RouteValueDictionary(new { baz = "baz" })),
                @"<form action=""" + MvcHelper.AppPathModifier + @"/named/home/oldaction"" baz=""baz"" method=""post"">");
        }

        [Fact]
        public void BeginRouteFormWithRouteName()
        {
            BeginFormHelper(
                htmlHelper => htmlHelper.BeginRouteForm("namedroute"),
                @"<form action=""" + MvcHelper.AppPathModifier + @"/named/home/oldaction"" method=""post"">");
        }

        [Fact]
        public void BeginRouteFormWithRouteNameFormMethodHtmlDictionary()
        {
            BeginFormHelper(
                htmlHelper => htmlHelper.BeginRouteForm("namedroute", FormMethod.Get, new RouteValueDictionary(new { baz = "baz" })),
                @"<form action=""" + MvcHelper.AppPathModifier + @"/named/home/oldaction"" baz=""baz"" method=""get"">");
        }

        [Fact]
        public void BeginRouteFormWithRouteNameFormMethodHtmlValues()
        {
            BeginFormHelper(
                htmlHelper => htmlHelper.BeginRouteForm("namedroute", FormMethod.Get, new { baz = "baz" }),
                @"<form action=""" + MvcHelper.AppPathModifier + @"/named/home/oldaction"" baz=""baz"" method=""get"">");
        }

        [Theory]
        [PropertyData("AttributeEncodedData_NoHtmlEncode", PropertyType = typeof(EncodedDataSets))]
        public void BeginRouteFormWithRouteNameFormMethodHtmlValues_AttributeEncodes_AddedHtmlAttributes(
            string text,
            string expectedText)
        {
            BeginFormHelper(
                helper => helper.BeginRouteForm("namedroute", FormMethod.Get, new { attribute = text }),
                @"<form action=""" + MvcHelper.AppPathModifier + @"/named/home/oldaction"" attribute=""" +
                    expectedText +
                    @""" method=""get"">");
        }

        [Fact]
        public void BeginRouteFormWithRouteNameFormMethodHtmlValuesWithUnderscores()
        {
            BeginFormHelper(
                htmlHelper => htmlHelper.BeginRouteForm("namedroute", FormMethod.Get, new { foo_baz = "baz" }),
                @"<form action=""" + MvcHelper.AppPathModifier + @"/named/home/oldaction"" foo-baz=""baz"" method=""get"">");
        }

        [Fact]
        public void BeginRouteFormWithRouteNameRouteDictionaryFormMethodHtmlDictionary()
        {
            BeginFormHelper(
                htmlHelper => htmlHelper.BeginRouteForm("namedroute", new RouteValueDictionary(new { id = "id" }), FormMethod.Get, new RouteValueDictionary(new { baz = "baz" })),
                @"<form action=""" + MvcHelper.AppPathModifier + @"/named/home/oldaction/id"" baz=""baz"" method=""get"">");
        }

        [Fact]
        public void BeginRouteFormWithRouteNameRouteValuesFormMethodHtmlValues()
        {
            BeginFormHelper(
                htmlHelper => htmlHelper.BeginRouteForm("namedroute", new { id = "id" }, FormMethod.Get, new { baz = "baz" }),
                @"<form action=""" + MvcHelper.AppPathModifier + @"/named/home/oldaction/id"" baz=""baz"" method=""get"">");
        }

        [Fact]
        public void BeginRouteFormWithRouteNameRouteValuesFormMethodHtmlValuesWithUnderscores()
        {
            BeginFormHelper(
                htmlHelper => htmlHelper.BeginRouteForm("namedroute", new { id = "id" }, FormMethod.Get, new { foo_baz = "baz" }),
                @"<form action=""" + MvcHelper.AppPathModifier + @"/named/home/oldaction/id"" foo-baz=""baz"" method=""get"">");
        }

        [Fact]
        public void BeginRouteFormWithRouteNameNullRouteValuesFormMethodNullHtmlValues()
        {
            BeginFormHelper(
                htmlHelper => htmlHelper.BeginRouteForm("namedroute", null, FormMethod.Get, null),
                @"<form action=""" + MvcHelper.AppPathModifier + @"/named/home/oldaction"" method=""get"">");
        }

        [Fact]
        public void BeginRouteFormWithRouteValues()
        {
            BeginFormHelper(
                htmlHelper => htmlHelper.BeginRouteForm(new { action = "someOtherAction", id = "id" }),
                @"<form action=""" + MvcHelper.AppPathModifier + @"/home/someOtherAction/id"" method=""post"">");
        }

        [Fact]
        public void BeginRouteFormWithRouteDictionary()
        {
            BeginFormHelper(
                htmlHelper => htmlHelper.BeginRouteForm(new RouteValueDictionary { { "action", "someOtherAction" }, { "id", "id" } }),
                @"<form action=""" + MvcHelper.AppPathModifier + @"/home/someOtherAction/id"" method=""post"">");
        }

        [Fact]
        public void BeginRouteFormWithRouteNameRouteValues()
        {
            BeginFormHelper(
                htmlHelper => htmlHelper.BeginRouteForm("namedroute", new { id = "id", pageNum = "123" }),
                @"<form action=""" + MvcHelper.AppPathModifier + @"/named/home/oldaction/id?pageNum=123"" method=""post"">");
        }

        [Fact]
        public void BeginRouteFormWithActionControllerRouteDictionary()
        {
            BeginFormHelper(
                htmlHelper => htmlHelper.BeginRouteForm("namedroute", new RouteValueDictionary { { "pageNum", "123" }, { "id", "id" } }),
                @"<form action=""" + MvcHelper.AppPathModifier + @"/named/home/oldaction/id?pageNum=123"" method=""post"">");
        }

        [Fact]
        public void BeginRouteFormCanUseNamedRouteWithoutSpecifyingDefaults()
        {
            // DevDiv 217072: Non-mvc specific helpers should not give default values for controller and action

            BeginFormHelper(
                htmlHelper =>
                {
                    htmlHelper.RouteCollection.MapRoute("MyRouteName", "any/url", new { controller = "Charlie" });
                    return htmlHelper.BeginRouteForm("MyRouteName");
                }, @"<form action=""" + MvcHelper.AppPathModifier + @"/any/url"" method=""post"">");
        }

        [Fact]
        public void BeginRouteFormWithActionControllerMethod()
        {
            BeginFormHelper(
                htmlHelper => htmlHelper.BeginRouteForm("namedroute", FormMethod.Get),
                @"<form action=""" + MvcHelper.AppPathModifier + @"/named/home/oldaction"" method=""get"">");
        }

        [Fact]
        public void BeginRouteFormWithActionControllerRouteValuesMethod()
        {
            BeginFormHelper(
                htmlHelper => htmlHelper.BeginRouteForm("namedroute", new { id = "id", pageNum = "123" }, FormMethod.Get),
                @"<form action=""" + MvcHelper.AppPathModifier + @"/named/home/oldaction/id?pageNum=123"" method=""get"">");
        }

        [Fact]
        public void BeginRouteFormWithActionControllerRouteDictionaryMethod()
        {
            BeginFormHelper(
                htmlHelper => htmlHelper.BeginRouteForm("namedroute", new RouteValueDictionary { { "pageNum", "123" }, { "id", "id" } }, FormMethod.Get),
                @"<form action=""" + MvcHelper.AppPathModifier + @"/named/home/oldaction/id?pageNum=123"" method=""get"">");
        }

        [Fact]
        public void EndFormWritesCloseTag()
        {
            // Arrange
            StringWriter writer;
            HtmlHelper htmlHelper = GetFormHelper(out writer);

            // Act
            htmlHelper.EndForm();

            // Assert
            Assert.Equal("</form>", writer.ToString());
        }

        private static HtmlHelper GetFormHelper(out StringWriter writer)
        {
            Mock<ViewContext> mockViewContext = new Mock<ViewContext>() { CallBase = true };
            mockViewContext.Setup(c => c.HttpContext.Request.Url).Returns(new Uri("http://www.contoso.com/some/path"));
            mockViewContext.Setup(c => c.HttpContext.Request.RawUrl).Returns("/some/path");
            mockViewContext.Setup(c => c.HttpContext.Request.ApplicationPath).Returns("/");
            mockViewContext.Setup(c => c.HttpContext.Request.Path).Returns("/");
            mockViewContext.Setup(c => c.HttpContext.Request.ServerVariables).Returns((NameValueCollection)null);
            mockViewContext.Setup(c => c.HttpContext.Response.Write(It.IsAny<string>())).Throws(new Exception("Should not be called"));
            mockViewContext.Setup(c => c.HttpContext.Items).Returns(new Hashtable());
            mockViewContext.Setup(c => c.HttpContext.GetService(It.IsAny<Type>())).Returns(null);

            writer = new StringWriter();
            mockViewContext.Setup(c => c.Writer).Returns(writer);

            mockViewContext.Setup(c => c.HttpContext.Response.ApplyAppPathModifier(It.IsAny<string>())).Returns<string>(r => MvcHelper.AppPathModifier + r);

            RouteCollection rt = new RouteCollection();
            rt.Add(new Route("{controller}/{action}/{id}", null) { Defaults = new RouteValueDictionary(new { id = "defaultid" }) });
            rt.Add("namedroute", new Route("named/{controller}/{action}/{id}", null) { Defaults = new RouteValueDictionary(new { id = "defaultid" }) });
            RouteData rd = new RouteData();
            rd.Values.Add("controller", "home");
            rd.Values.Add("action", "oldaction");

            mockViewContext.Setup(c => c.RouteData).Returns(rd);
            HtmlHelper helper = new HtmlHelper(mockViewContext.Object, new Mock<IViewDataContainer>().Object, rt);
            helper.ViewContext.FormIdGenerator = () => "form_id";
            return helper;
        }

        public void Dispose()
        {
            // Reset ScopeStorage (written via e.g. ViewContext.ClientValidationEnabled) between tests to avoid unexpected interactions.
            ScopeStorage.CurrentProvider = new StaticScopeStorageProvider();
            ScopeStorage.GlobalScope.Clear();
        }
    }
}
