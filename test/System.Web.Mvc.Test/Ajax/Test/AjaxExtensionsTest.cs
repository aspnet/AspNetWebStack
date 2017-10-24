// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Web.Mvc.Html;
using System.Web.Routing;
using System.Web.WebPages.Scope;
using Microsoft.TestCommon;
using Microsoft.Web.UnitTestUtil;
using Moq;

namespace System.Web.Mvc.Ajax.Test
{
    [Xunit.Collection("Uses ScopeStorage or ViewEngines.Engines")]
    public class AjaxExtensionsTest : IDisposable
    {
        // Guards

        [Fact]
        public void ActionLinkWithNullOrEmptyLinkTextThrows()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper();

            // Act & Assert
            Assert.ThrowsArgumentNullOrEmpty(
                delegate { MvcHtmlString actionLink = ajaxHelper.ActionLink(String.Empty, String.Empty, null, null, null, null); },
                "linkText");
        }

        [Fact]
        public void RouteLinkWithNullOrEmptyLinkTextThrows()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper();

            // Act & Assert
            Assert.ThrowsArgumentNullOrEmpty(
                delegate { MvcHtmlString actionLink = ajaxHelper.RouteLink(String.Empty, String.Empty, null, null, null); },
                "linkText");
        }

        // Form context setup and cleanup

        [Fact]
        public void BeginFormSetsAndRestoresToDefault()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper();

            ajaxHelper.ViewContext.FormContext = null;
            FormContext defaultFormContext = ajaxHelper.ViewContext.FormContext;

            // Act & assert - push
            MvcForm theForm = ajaxHelper.BeginForm(new AjaxOptions());
            Assert.NotNull(ajaxHelper.ViewContext.FormContext);
            Assert.NotEqual(defaultFormContext, ajaxHelper.ViewContext.FormContext);

            // Act & assert - pop
            theForm.Dispose();
            Assert.Equal(defaultFormContext, ajaxHelper.ViewContext.FormContext);
        }

        [Fact]
        public void DisposeWritesClosingFormTag()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper();
            AjaxOptions ajaxOptions = new AjaxOptions { UpdateTargetId = "some-id" };
            StringWriter writer = new StringWriter();
            ajaxHelper.ViewContext.Writer = writer;

            // Act
            IDisposable form = ajaxHelper.BeginForm("Action", "Controller", ajaxOptions);
            form.Dispose();

            // Assert
            Assert.EndsWith("</form>", writer.ToString());
        }

        // GlobalizationScript

        [Fact]
        public void GlobalizationScriptWithNullCultureInfoThrows()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                delegate { ajaxHelper.GlobalizationScript(null); },
                "cultureInfo");
        }

        [Fact]
        public void GlobalizationScriptUsesCurrentCultureAsDefault()
        {
            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;

            try
            {
                // Arrange
                AjaxHelper ajaxHelper = GetAjaxHelper();
                AjaxHelper.GlobalizationScriptPath = null;
                Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-GB");

                // Act
                MvcHtmlString globalizationScript = ajaxHelper.GlobalizationScript();

                // Assert
                Assert.Equal(@"<script src=""~/Scripts/Globalization/en-GB.js"" type=""text/javascript""></script>", globalizationScript.ToHtmlString());
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = currentCulture;
            }
        }

        [Fact]
        public void GlobalizationScriptWithCultureInfo()
        {
            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;

            try
            {
                // Arrange
                AjaxHelper ajaxHelper = GetAjaxHelper();
                AjaxHelper.GlobalizationScriptPath = null;
                Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-GB");

                // Act
                MvcHtmlString globalizationScript = ajaxHelper.GlobalizationScript(CultureInfo.GetCultureInfo("en-CA"));

                // Assert
                Assert.Equal(@"<script src=""~/Scripts/Globalization/en-CA.js"" type=""text/javascript""></script>", globalizationScript.ToHtmlString());
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = currentCulture;
            }
        }

        [Fact]
        public void GlobalizationScriptEncodesSource()
        {
            // Arrange
            Mock<CultureInfo> xssCulture = new Mock<CultureInfo>("en-US");
            xssCulture.Setup(culture => culture.Name).Returns("evil.example.com/<script>alert('XSS!')</script>");
            string globalizationPath = "~/Scripts&Globalization";
            string expectedScriptTag = @"<script src=""~/Scripts&amp;Globalization/evil.example.com%2f%3cscript%3ealert(%27XSS!%27)%3c%2fscript%3e.js"" type=""text/javascript""></script>";

            // Act
            MvcHtmlString globalizationScript = AjaxExtensions.GlobalizationScriptHelper(globalizationPath, xssCulture.Object);

            // Assert
            Assert.Equal(expectedScriptTag, globalizationScript.ToHtmlString());
        }

        [Fact]
        public void GlobalizationScriptWithNullCultureName()
        {
            // Arrange
            Mock<CultureInfo> xssCulture = new Mock<CultureInfo>("en-US");
            xssCulture.Setup(culture => culture.Name).Returns((string)null);

            AjaxHelper ajaxHelper = GetAjaxHelper();
            AjaxHelper.GlobalizationScriptPath = null;

            // Act
            MvcHtmlString globalizationScript = ajaxHelper.GlobalizationScript(xssCulture.Object);

            // Assert
            Assert.Equal(@"<script src=""~/Scripts/Globalization/.js"" type=""text/javascript""></script>", globalizationScript.ToHtmlString());
        }

        // ActionLink (traditional JavaScript)

        [Fact]
        public void ActionLinkWithNullActionName()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: false);

            // Act
            MvcHtmlString actionLink = ajaxHelper.ActionLink("linkText", null, new AjaxOptions());

            // Assert
            Assert.Equal(@"<a href=""" + MvcHelper.AppPathModifier + @"/app/home/oldaction"" onclick=""Sys.Mvc.AsyncHyperlink.handleClick(this, new Sys.UI.DomEvent(event), { insertionMode: Sys.Mvc.InsertionMode.replace });"">linkText</a>", actionLink.ToHtmlString());
        }

        [Fact]
        public void ActionLinkWithNullActionName_Unobtrusive()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: true);

            // Act
            MvcHtmlString actionLink = ajaxHelper.ActionLink("linkText", null, new AjaxOptions());

            // Assert
            Assert.Equal(@"<a data-ajax=""true"" href=""" + MvcHelper.AppPathModifier + @"/app/home/oldaction"">linkText</a>", actionLink.ToHtmlString());
        }

        [Fact]
        public void ActionLinkWithNullActionNameAndNullOptions()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: false);

            // Act
            MvcHtmlString actionLink = ajaxHelper.ActionLink("linkText", null, null);

            // Assert
            Assert.Equal(@"<a href=""" + MvcHelper.AppPathModifier + @"/app/home/oldaction"" onclick=""Sys.Mvc.AsyncHyperlink.handleClick(this, new Sys.UI.DomEvent(event), { insertionMode: Sys.Mvc.InsertionMode.replace });"">linkText</a>", actionLink.ToHtmlString());
        }

        [Fact]
        public void ActionLinkWithNullActionNameAndNullOptions_Unobtrusive()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: true);

            // Act
            MvcHtmlString actionLink = ajaxHelper.ActionLink("linkText", null, null);

            // Assert
            Assert.Equal(@"<a data-ajax=""true"" href=""" + MvcHelper.AppPathModifier + @"/app/home/oldaction"">linkText</a>", actionLink.ToHtmlString());
        }

        [Fact]
        public void ActionLink()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: false);

            // Act
            MvcHtmlString actionLink = ajaxHelper.ActionLink("linkText", "Action", new AjaxOptions());

            // Assert
            Assert.Equal(@"<a href=""" + MvcHelper.AppPathModifier + @"/app/home/Action"" onclick=""Sys.Mvc.AsyncHyperlink.handleClick(this, new Sys.UI.DomEvent(event), { insertionMode: Sys.Mvc.InsertionMode.replace });"">linkText</a>", actionLink.ToHtmlString());
        }

        [Fact]
        public void ActionLink_Unobtrusive()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: true);

            // Act
            MvcHtmlString actionLink = ajaxHelper.ActionLink("linkText", "Action", new AjaxOptions());

            // Assert
            Assert.Equal(@"<a data-ajax=""true"" href=""" + MvcHelper.AppPathModifier + @"/app/home/Action"">linkText</a>", actionLink.ToHtmlString());
        }

        [Fact]
        public void ActionLinkAnonymousValues()
        {
            // Arrange
            AjaxHelper helper = GetAjaxHelper(unobtrusiveJavaScript: false);
            object values = new { controller = "Controller" };
            AjaxOptions options = new AjaxOptions { UpdateTargetId = "update-div" };

            // Act
            MvcHtmlString actionLink = helper.ActionLink("Some Text", "Action", values, options);

            // Assert
            Assert.Equal(@"<a href=""" + MvcHelper.AppPathModifier + @"/app/Controller/Action"" onclick=""Sys.Mvc.AsyncHyperlink.handleClick(this, new Sys.UI.DomEvent(event), { insertionMode: Sys.Mvc.InsertionMode.replace, updateTargetId: &#39;update-div&#39; });"">Some Text</a>", actionLink.ToHtmlString());
        }

        [Fact]
        public void ActionLinkAnonymousValues_Unobtrusive()
        {
            // Arrange
            AjaxHelper helper = GetAjaxHelper(unobtrusiveJavaScript: true);
            object values = new { controller = "Controller" };
            AjaxOptions options = new AjaxOptions { UpdateTargetId = "update-div" };

            // Act
            MvcHtmlString actionLink = helper.ActionLink("Some Text", "Action", values, options);

            // Assert
            Assert.Equal(@"<a data-ajax=""true"" data-ajax-mode=""replace"" data-ajax-update=""#update-div"" href=""" + MvcHelper.AppPathModifier + @"/app/Controller/Action"">Some Text</a>", actionLink.ToHtmlString());
        }

        [Fact]
        public void ActionLinkAnonymousValuesAndAttributes()
        {
            // Arrange
            AjaxHelper helper = GetAjaxHelper(unobtrusiveJavaScript: false);
            object htmlAttributes = new { foo = "bar", baz = "quux", foo_bar = "baz_quux" };
            object values = new { controller = "Controller" };
            AjaxOptions options = new AjaxOptions { UpdateTargetId = "update-div" };

            // Act
            MvcHtmlString actionLink = helper.ActionLink("Some Text", "Action", values, options, htmlAttributes);

            // Assert
            Assert.Equal(@"<a baz=""quux"" foo=""bar"" foo-bar=""baz_quux"" href=""" + MvcHelper.AppPathModifier + @"/app/Controller/Action"" onclick=""Sys.Mvc.AsyncHyperlink.handleClick(this, new Sys.UI.DomEvent(event), { insertionMode: Sys.Mvc.InsertionMode.replace, updateTargetId: &#39;update-div&#39; });"">Some Text</a>", actionLink.ToHtmlString());
        }

        [Fact]
        public void ActionLinkAnonymousValuesAndAttributes_Unobtrusive()
        {
            // Arrange
            AjaxHelper helper = GetAjaxHelper(unobtrusiveJavaScript: true);
            object htmlAttributes = new { foo = "bar", baz = "quux", foo_bar = "baz_quux" };
            object values = new { controller = "Controller" };
            AjaxOptions options = new AjaxOptions { UpdateTargetId = "update-div" };

            // Act
            MvcHtmlString actionLink = helper.ActionLink("Some Text", "Action", values, options, htmlAttributes);

            // Assert
            Assert.Equal(@"<a baz=""quux"" data-ajax=""true"" data-ajax-mode=""replace"" data-ajax-update=""#update-div"" foo=""bar"" foo-bar=""baz_quux"" href=""" + MvcHelper.AppPathModifier + @"/app/Controller/Action"">Some Text</a>", actionLink.ToHtmlString());
        }

        [Fact]
        public void ActionLinkTypedValues()
        {
            // Arrange
            AjaxHelper helper = GetAjaxHelper(unobtrusiveJavaScript: false);
            RouteValueDictionary values = new RouteValueDictionary
            {
                { "controller", "Controller" }
            };

            AjaxOptions options = new AjaxOptions { UpdateTargetId = "update-div" };

            // Act
            MvcHtmlString actionLink = helper.ActionLink("Some Text", "Action", values, options);

            // Assert
            Assert.Equal(@"<a href=""" + MvcHelper.AppPathModifier + @"/app/Controller/Action"" onclick=""Sys.Mvc.AsyncHyperlink.handleClick(this, new Sys.UI.DomEvent(event), { insertionMode: Sys.Mvc.InsertionMode.replace, updateTargetId: &#39;update-div&#39; });"">Some Text</a>", actionLink.ToHtmlString());
        }

        [Fact]
        public void ActionLinkTypedValues_Unobtrusive()
        {
            // Arrange
            AjaxHelper helper = GetAjaxHelper(unobtrusiveJavaScript: true);
            RouteValueDictionary values = new RouteValueDictionary
            {
                { "controller", "Controller" }
            };

            AjaxOptions options = new AjaxOptions { UpdateTargetId = "update-div" };

            // Act
            MvcHtmlString actionLink = helper.ActionLink("Some Text", "Action", values, options);

            // Assert
            Assert.Equal(@"<a data-ajax=""true"" data-ajax-mode=""replace"" data-ajax-update=""#update-div"" href=""" + MvcHelper.AppPathModifier + @"/app/Controller/Action"">Some Text</a>", actionLink.ToHtmlString());
        }

        [Fact]
        public void ActionLinkTypedValuesAndAttributes()
        {
            // Arrange
            AjaxHelper helper = GetAjaxHelper(unobtrusiveJavaScript: false);
            RouteValueDictionary values = new RouteValueDictionary
            {
                { "controller", "Controller" }
            };
            Dictionary<string, object> htmlAttributes = new Dictionary<string, object>
            {
                { "foo", "bar" },
                { "baz", "quux" },
                { "foo_bar", "baz_quux" }
            };
            AjaxOptions options = new AjaxOptions { UpdateTargetId = "update-div" };

            // Act
            MvcHtmlString actionLink = helper.ActionLink("Some Text", "Action", values, options, htmlAttributes);

            // Assert
            Assert.Equal(@"<a baz=""quux"" foo=""bar"" foo_bar=""baz_quux"" href=""" + MvcHelper.AppPathModifier + @"/app/Controller/Action"" onclick=""Sys.Mvc.AsyncHyperlink.handleClick(this, new Sys.UI.DomEvent(event), { insertionMode: Sys.Mvc.InsertionMode.replace, updateTargetId: &#39;update-div&#39; });"">Some Text</a>", actionLink.ToHtmlString());
        }

        [Fact]
        public void ActionLinkTypedValuesAndAttributes_Unobtrusive()
        {
            // Arrange
            AjaxHelper helper = GetAjaxHelper(unobtrusiveJavaScript: true);
            RouteValueDictionary values = new RouteValueDictionary
            {
                { "controller", "Controller" }
            };
            Dictionary<string, object> htmlAttributes = new Dictionary<string, object>
            {
                { "foo", "bar" },
                { "baz", "quux" },
                { "foo_bar", "baz_quux" }
            };
            AjaxOptions options = new AjaxOptions { UpdateTargetId = "update-div" };

            // Act
            MvcHtmlString actionLink = helper.ActionLink("Some Text", "Action", values, options, htmlAttributes);

            // Assert
            Assert.Equal(@"<a baz=""quux"" data-ajax=""true"" data-ajax-mode=""replace"" data-ajax-update=""#update-div"" foo=""bar"" foo_bar=""baz_quux"" href=""" + MvcHelper.AppPathModifier + @"/app/Controller/Action"">Some Text</a>", actionLink.ToHtmlString());
        }

        [Fact]
        public void ActionLinkController()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: false);

            // Act
            MvcHtmlString actionLink = ajaxHelper.ActionLink("linkText", "Action", "Controller", new AjaxOptions());

            // Assert
            Assert.Equal(@"<a href=""" + MvcHelper.AppPathModifier + @"/app/Controller/Action"" onclick=""Sys.Mvc.AsyncHyperlink.handleClick(this, new Sys.UI.DomEvent(event), { insertionMode: Sys.Mvc.InsertionMode.replace });"">linkText</a>", actionLink.ToHtmlString());
        }

        [Fact]
        public void ActionLinkController_Unobtrusive()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: true);

            // Act
            MvcHtmlString actionLink = ajaxHelper.ActionLink("linkText", "Action", "Controller", new AjaxOptions());

            // Assert
            Assert.Equal(@"<a data-ajax=""true"" href=""" + MvcHelper.AppPathModifier + @"/app/Controller/Action"">linkText</a>", actionLink.ToHtmlString());
        }

        [Fact]
        public void ActionLinkControllerAnonymousValues()
        {
            // Arrange
            AjaxHelper helper = GetAjaxHelper(unobtrusiveJavaScript: false);
            object values = new { id = 5 };
            AjaxOptions options = new AjaxOptions { UpdateTargetId = "update-div" };

            // Act
            MvcHtmlString actionLink = helper.ActionLink("Some Text", "Action", "Controller", values, options);

            // Assert
            Assert.Equal(@"<a href=""" + MvcHelper.AppPathModifier + @"/app/Controller/Action/5"" onclick=""Sys.Mvc.AsyncHyperlink.handleClick(this, new Sys.UI.DomEvent(event), { insertionMode: Sys.Mvc.InsertionMode.replace, updateTargetId: &#39;update-div&#39; });"">Some Text</a>", actionLink.ToHtmlString());
        }

        [Fact]
        public void ActionLinkControllerAnonymousValues_Unobtrusive()
        {
            // Arrange
            AjaxHelper helper = GetAjaxHelper(unobtrusiveJavaScript: true);
            object values = new { id = 5 };
            AjaxOptions options = new AjaxOptions { UpdateTargetId = "update-div" };

            // Act
            MvcHtmlString actionLink = helper.ActionLink("Some Text", "Action", "Controller", values, options);

            // Assert
            Assert.Equal(@"<a data-ajax=""true"" data-ajax-mode=""replace"" data-ajax-update=""#update-div"" href=""" + MvcHelper.AppPathModifier + @"/app/Controller/Action/5"">Some Text</a>", actionLink.ToHtmlString());
        }

        [Fact]
        public void ActionLinkControllerAnonymousValuesAndAttributes()
        {
            // Arrange
            AjaxHelper helper = GetAjaxHelper(unobtrusiveJavaScript: false);
            object htmlAttributes = new { foo = "bar", baz = "quux", foo_bar = "baz_quux" };
            object values = new { id = 5 };
            AjaxOptions options = new AjaxOptions { UpdateTargetId = "update-div" };

            // Act
            MvcHtmlString actionLink = helper.ActionLink("Some Text", "Action", "Controller", values, options, htmlAttributes);

            // Assert
            Assert.Equal(@"<a baz=""quux"" foo=""bar"" foo-bar=""baz_quux"" href=""" + MvcHelper.AppPathModifier + @"/app/Controller/Action/5"" onclick=""Sys.Mvc.AsyncHyperlink.handleClick(this, new Sys.UI.DomEvent(event), { insertionMode: Sys.Mvc.InsertionMode.replace, updateTargetId: &#39;update-div&#39; });"">Some Text</a>", actionLink.ToHtmlString());
        }

        [Fact]
        public void ActionLinkControllerAnonymousValuesAndAttributes_Unobtrusive()
        {
            // Arrange
            AjaxHelper helper = GetAjaxHelper(unobtrusiveJavaScript: true);
            object htmlAttributes = new { foo = "bar", baz = "quux", foo_bar = "baz_quux" };
            object values = new { id = 5 };
            AjaxOptions options = new AjaxOptions { UpdateTargetId = "update-div" };

            // Act
            MvcHtmlString actionLink = helper.ActionLink("Some Text", "Action", "Controller", values, options, htmlAttributes);

            // Assert
            Assert.Equal(@"<a baz=""quux"" data-ajax=""true"" data-ajax-mode=""replace"" data-ajax-update=""#update-div"" foo=""bar"" foo-bar=""baz_quux"" href=""" + MvcHelper.AppPathModifier + @"/app/Controller/Action/5"">Some Text</a>", actionLink.ToHtmlString());
        }

        [Fact]
        public void ActionLinkControllerTypedValues()
        {
            // Arrange
            AjaxHelper helper = GetAjaxHelper(unobtrusiveJavaScript: false);
            RouteValueDictionary values = new RouteValueDictionary
            {
                { "id", 5 }
            };
            AjaxOptions options = new AjaxOptions { UpdateTargetId = "update-div" };

            // Act
            MvcHtmlString actionLink = helper.ActionLink("Some Text", "Action", "Controller", values, options);

            // Assert
            Assert.Equal(@"<a href=""" + MvcHelper.AppPathModifier + @"/app/Controller/Action/5"" onclick=""Sys.Mvc.AsyncHyperlink.handleClick(this, new Sys.UI.DomEvent(event), { insertionMode: Sys.Mvc.InsertionMode.replace, updateTargetId: &#39;update-div&#39; });"">Some Text</a>", actionLink.ToHtmlString());
        }

        [Fact]
        public void ActionLinkControllerTypedValues_Unobtrusive()
        {
            // Arrange
            AjaxHelper helper = GetAjaxHelper(unobtrusiveJavaScript: true);
            RouteValueDictionary values = new RouteValueDictionary
            {
                { "id", 5 }
            };
            AjaxOptions options = new AjaxOptions { UpdateTargetId = "update-div" };

            // Act
            MvcHtmlString actionLink = helper.ActionLink("Some Text", "Action", "Controller", values, options);

            // Assert
            Assert.Equal(@"<a data-ajax=""true"" data-ajax-mode=""replace"" data-ajax-update=""#update-div"" href=""" + MvcHelper.AppPathModifier + @"/app/Controller/Action/5"">Some Text</a>", actionLink.ToHtmlString());
        }

        [Fact]
        public void ActionLinkControllerTypedValuesAndAttributes()
        {
            // Arrange
            AjaxHelper helper = GetAjaxHelper(unobtrusiveJavaScript: false);
            RouteValueDictionary values = new RouteValueDictionary
            {
                { "id", 5 }
            };
            Dictionary<string, object> htmlAttributes = new Dictionary<string, object>
            {
                { "foo", "bar" },
                { "baz", "quux" },
                { "foo_bar", "baz_quux" }
            };
            AjaxOptions options = new AjaxOptions { UpdateTargetId = "update-div" };

            // Act
            MvcHtmlString actionLink = helper.ActionLink("Some Text", "Action", "Controller", values, options, htmlAttributes);

            // Assert
            Assert.Equal(@"<a baz=""quux"" foo=""bar"" foo_bar=""baz_quux"" href=""" + MvcHelper.AppPathModifier + @"/app/Controller/Action/5"" onclick=""Sys.Mvc.AsyncHyperlink.handleClick(this, new Sys.UI.DomEvent(event), { insertionMode: Sys.Mvc.InsertionMode.replace, updateTargetId: &#39;update-div&#39; });"">Some Text</a>", actionLink.ToHtmlString());
        }

        [Fact]
        public void ActionLinkControllerTypedValuesAndAttributes_Unobtrusive()
        {
            // Arrange
            AjaxHelper helper = GetAjaxHelper(unobtrusiveJavaScript: true);
            RouteValueDictionary values = new RouteValueDictionary
            {
                { "id", 5 }
            };
            Dictionary<string, object> htmlAttributes = new Dictionary<string, object>
            {
                { "foo", "bar" },
                { "baz", "quux" },
                { "foo_bar", "baz_quux" }
            };
            AjaxOptions options = new AjaxOptions { UpdateTargetId = "update-div" };

            // Act
            MvcHtmlString actionLink = helper.ActionLink("Some Text", "Action", "Controller", values, options, htmlAttributes);

            // Assert
            Assert.Equal(@"<a baz=""quux"" data-ajax=""true"" data-ajax-mode=""replace"" data-ajax-update=""#update-div"" foo=""bar"" foo_bar=""baz_quux"" href=""" + MvcHelper.AppPathModifier + @"/app/Controller/Action/5"">Some Text</a>", actionLink.ToHtmlString());
        }

        [Fact]
        public void ActionLinkWithOptions()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: false);

            // Act
            MvcHtmlString actionLink = ajaxHelper.ActionLink("linkText", "Action", "Controller", new AjaxOptions { UpdateTargetId = "some-id" });

            // Assert
            Assert.Equal(@"<a href=""" + MvcHelper.AppPathModifier + @"/app/Controller/Action"" onclick=""Sys.Mvc.AsyncHyperlink.handleClick(this, new Sys.UI.DomEvent(event), { insertionMode: Sys.Mvc.InsertionMode.replace, updateTargetId: &#39;some-id&#39; });"">linkText</a>", actionLink.ToHtmlString());
        }

        [Fact]
        public void ActionLinkWithOptions_Unobtrusive()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: true);
            AjaxOptions options = new AjaxOptions { UpdateTargetId = "some-id" };

            // Act
            MvcHtmlString actionLink = ajaxHelper.ActionLink("linkText", "Action", "Controller", options);

            // Assert
            Assert.Equal(@"<a data-ajax=""true"" data-ajax-mode=""replace"" data-ajax-update=""#some-id"" href=""" + MvcHelper.AppPathModifier + @"/app/Controller/Action"">linkText</a>", actionLink.ToHtmlString());
        }

        [Fact]
        public void ActionLinkWithNullHostName()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: false);

            // Act
            MvcHtmlString actionLink = ajaxHelper.ActionLink("linkText", "Action", "Controller",
                                                             null, null, null, null, new AjaxOptions { UpdateTargetId = "some-id" }, null);

            // Assert
            Assert.Equal(@"<a href=""" + MvcHelper.AppPathModifier + @"/app/Controller/Action"" onclick=""Sys.Mvc.AsyncHyperlink.handleClick(this, new Sys.UI.DomEvent(event), { insertionMode: Sys.Mvc.InsertionMode.replace, updateTargetId: &#39;some-id&#39; });"">linkText</a>", actionLink.ToHtmlString());
        }

        [Fact]
        public void ActionLinkWithNullHostName_Unobtrusive()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: true);

            // Act
            MvcHtmlString actionLink = ajaxHelper.ActionLink("linkText", "Action", "Controller",
                                                             null, null, null, null, new AjaxOptions { UpdateTargetId = "some-id" }, null);

            // Assert
            Assert.Equal(@"<a data-ajax=""true"" data-ajax-mode=""replace"" data-ajax-update=""#some-id"" href=""" + MvcHelper.AppPathModifier + @"/app/Controller/Action"">linkText</a>", actionLink.ToHtmlString());
        }

        [Fact]
        public void ActionLinkWithProtocol()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: false);

            // Act
            MvcHtmlString actionLink = ajaxHelper.ActionLink("linkText", "Action", "Controller", "https", null, null, null, new AjaxOptions { UpdateTargetId = "some-id" }, null);

            // Assert
            Assert.Equal(@"<a href=""https://foo.bar.baz" + MvcHelper.AppPathModifier + @"/app/Controller/Action"" onclick=""Sys.Mvc.AsyncHyperlink.handleClick(this, new Sys.UI.DomEvent(event), { insertionMode: Sys.Mvc.InsertionMode.replace, updateTargetId: &#39;some-id&#39; });"">linkText</a>", actionLink.ToHtmlString());
        }

        [Fact]
        public void ActionLinkWithProtocol_Unobtrusive()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: true);

            // Act
            MvcHtmlString actionLink = ajaxHelper.ActionLink("linkText", "Action", "Controller", "https", null, null, null, new AjaxOptions { UpdateTargetId = "some-id" }, null);

            // Assert
            Assert.Equal(@"<a data-ajax=""true"" data-ajax-mode=""replace"" data-ajax-update=""#some-id"" href=""https://foo.bar.baz" + MvcHelper.AppPathModifier + @"/app/Controller/Action"">linkText</a>", actionLink.ToHtmlString());
        }

        // RouteLink

        [Fact]
        public void RouteLinkWithNullOptions()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: false);

            // Act
            MvcHtmlString routeLink = ajaxHelper.RouteLink("Some Text", new RouteValueDictionary(), null);

            // Assert
            Assert.Equal(@"<a href=""" + MvcHelper.AppPathModifier + @"/app/home/oldaction"" onclick=""Sys.Mvc.AsyncHyperlink.handleClick(this, new Sys.UI.DomEvent(event), { insertionMode: Sys.Mvc.InsertionMode.replace });"">Some Text</a>", routeLink.ToHtmlString());
        }

        [Fact]
        public void RouteLinkWithNullOptions_Unobtrusive()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: true);

            // Act
            MvcHtmlString routeLink = ajaxHelper.RouteLink("Some Text", new RouteValueDictionary(), null);

            // Assert
            Assert.Equal(@"<a data-ajax=""true"" href=""" + MvcHelper.AppPathModifier + @"/app/home/oldaction"">Some Text</a>", routeLink.ToHtmlString());
        }

        [Fact]
        public void RouteLinkAnonymousValues()
        {
            // Arrange
            AjaxHelper helper = GetAjaxHelper(unobtrusiveJavaScript: false);
            object values = new
            {
                action = "Action",
                controller = "Controller"
            };
            AjaxOptions options = new AjaxOptions { UpdateTargetId = "update-div" };

            // Act
            MvcHtmlString routeLink = helper.RouteLink("Some Text", values, options);

            // Assert
            Assert.Equal(@"<a href=""" + MvcHelper.AppPathModifier + @"/app/Controller/Action"" onclick=""Sys.Mvc.AsyncHyperlink.handleClick(this, new Sys.UI.DomEvent(event), { insertionMode: Sys.Mvc.InsertionMode.replace, updateTargetId: &#39;update-div&#39; });"">Some Text</a>", routeLink.ToHtmlString());
        }

        [Fact]
        public void RouteLinkAnonymousValues_Unobtrusive()
        {
            // Arrange
            AjaxHelper helper = GetAjaxHelper(unobtrusiveJavaScript: true);
            object values = new
            {
                action = "Action",
                controller = "Controller"
            };
            AjaxOptions options = new AjaxOptions { UpdateTargetId = "update-div" };

            // Act
            MvcHtmlString routeLink = helper.RouteLink("Some Text", values, options);

            // Assert
            Assert.Equal(@"<a data-ajax=""true"" data-ajax-mode=""replace"" data-ajax-update=""#update-div"" href=""" + MvcHelper.AppPathModifier + @"/app/Controller/Action"">Some Text</a>", routeLink.ToHtmlString());
        }

        [Fact]
        public void RouteLinkAnonymousValuesAndAttributes()
        {
            // Arrange
            AjaxHelper helper = GetAjaxHelper(unobtrusiveJavaScript: false);
            object htmlAttributes = new
            {
                foo = "bar",
                baz = "quux",
                foo_bar = "baz_quux"
            };
            object values = new
            {
                action = "Action",
                controller = "Controller"
            };
            AjaxOptions options = new AjaxOptions { UpdateTargetId = "update-div" };

            // Act
            MvcHtmlString actionLink = helper.RouteLink("Some Text", values, options, htmlAttributes);

            // Assert
            Assert.Equal(@"<a baz=""quux"" foo=""bar"" foo-bar=""baz_quux"" href=""" + MvcHelper.AppPathModifier + @"/app/Controller/Action"" onclick=""Sys.Mvc.AsyncHyperlink.handleClick(this, new Sys.UI.DomEvent(event), { insertionMode: Sys.Mvc.InsertionMode.replace, updateTargetId: &#39;update-div&#39; });"">Some Text</a>", actionLink.ToHtmlString());
        }

        [Fact]
        public void RouteLinkAnonymousValuesAndAttributes_Unobtrusive()
        {
            // Arrange
            AjaxHelper helper = GetAjaxHelper(unobtrusiveJavaScript: true);
            object htmlAttributes = new
            {
                foo = "bar",
                baz = "quux",
                foo_bar = "baz_quux"
            };
            object values = new
            {
                action = "Action",
                controller = "Controller"
            };
            AjaxOptions options = new AjaxOptions { UpdateTargetId = "update-div" };

            // Act
            MvcHtmlString actionLink = helper.RouteLink("Some Text", values, options, htmlAttributes);

            // Assert
            Assert.Equal(@"<a baz=""quux"" data-ajax=""true"" data-ajax-mode=""replace"" data-ajax-update=""#update-div"" foo=""bar"" foo-bar=""baz_quux"" href=""" + MvcHelper.AppPathModifier + @"/app/Controller/Action"">Some Text</a>", actionLink.ToHtmlString());
        }

        [Fact]
        public void RouteLinkTypedValues()
        {
            // Arrange
            AjaxHelper helper = GetAjaxHelper(unobtrusiveJavaScript: false);
            RouteValueDictionary values = new RouteValueDictionary
            {
                { "controller", "Controller" },
                { "action", "Action" }
            };

            AjaxOptions options = new AjaxOptions { UpdateTargetId = "update-div" };

            // Act
            MvcHtmlString actionLink = helper.RouteLink("Some Text", values, options);

            // Assert
            Assert.Equal(@"<a href=""" + MvcHelper.AppPathModifier + @"/app/Controller/Action"" onclick=""Sys.Mvc.AsyncHyperlink.handleClick(this, new Sys.UI.DomEvent(event), { insertionMode: Sys.Mvc.InsertionMode.replace, updateTargetId: &#39;update-div&#39; });"">Some Text</a>", actionLink.ToHtmlString());
        }

        [Fact]
        public void RouteLinkTypedValues_Unobtrusive()
        {
            // Arrange
            AjaxHelper helper = GetAjaxHelper(unobtrusiveJavaScript: true);
            RouteValueDictionary values = new RouteValueDictionary
            {
                { "controller", "Controller" },
                { "action", "Action" }
            };

            AjaxOptions options = new AjaxOptions { UpdateTargetId = "update-div" };

            // Act
            MvcHtmlString actionLink = helper.RouteLink("Some Text", values, options);

            // Assert
            Assert.Equal(@"<a data-ajax=""true"" data-ajax-mode=""replace"" data-ajax-update=""#update-div"" href=""" + MvcHelper.AppPathModifier + @"/app/Controller/Action"">Some Text</a>", actionLink.ToHtmlString());
        }

        [Fact]
        public void RouteLinkTypedValuesAndAttributes()
        {
            // Arrange
            AjaxHelper helper = GetAjaxHelper(unobtrusiveJavaScript: false);
            RouteValueDictionary values = new RouteValueDictionary
            {
                { "controller", "Controller" },
                { "action", "Action" }
            };
            Dictionary<string, object> htmlAttributes = new Dictionary<string, object>
            {
                { "foo", "bar" },
                { "baz", "quux" },
                { "foo_bar", "baz_quux" }
            };
            AjaxOptions options = new AjaxOptions { UpdateTargetId = "update-div" };

            // Act
            MvcHtmlString actionLink = helper.RouteLink("Some Text", values, options, htmlAttributes);

            // Assert
            Assert.Equal(@"<a baz=""quux"" foo=""bar"" foo_bar=""baz_quux"" href=""" + MvcHelper.AppPathModifier + @"/app/Controller/Action"" onclick=""Sys.Mvc.AsyncHyperlink.handleClick(this, new Sys.UI.DomEvent(event), { insertionMode: Sys.Mvc.InsertionMode.replace, updateTargetId: &#39;update-div&#39; });"">Some Text</a>", actionLink.ToHtmlString());
        }

        [Fact]
        public void RouteLinkTypedValuesAndAttributes_Unobtrusive()
        {
            // Arrange
            AjaxHelper helper = GetAjaxHelper(unobtrusiveJavaScript: true);
            RouteValueDictionary values = new RouteValueDictionary
            {
                { "controller", "Controller" },
                { "action", "Action" }
            };
            Dictionary<string, object> htmlAttributes = new Dictionary<string, object>
            {
                { "foo", "bar" },
                { "baz", "quux" },
                { "foo_bar", "baz_quux" }
            };
            AjaxOptions options = new AjaxOptions { UpdateTargetId = "update-div" };

            // Act
            MvcHtmlString actionLink = helper.RouteLink("Some Text", values, options, htmlAttributes);

            // Assert
            Assert.Equal(@"<a baz=""quux"" data-ajax=""true"" data-ajax-mode=""replace"" data-ajax-update=""#update-div"" foo=""bar"" foo_bar=""baz_quux"" href=""" + MvcHelper.AppPathModifier + @"/app/Controller/Action"">Some Text</a>", actionLink.ToHtmlString());
        }

        [Fact]
        public void RouteLinkNamedRoute()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: false);

            // Act
            MvcHtmlString actionLink = ajaxHelper.RouteLink("linkText", "namedroute", new AjaxOptions());

            // Assert
            Assert.Equal(@"<a href=""" + MvcHelper.AppPathModifier + @"/app/named/home/oldaction"" onclick=""Sys.Mvc.AsyncHyperlink.handleClick(this, new Sys.UI.DomEvent(event), { insertionMode: Sys.Mvc.InsertionMode.replace });"">linkText</a>", actionLink.ToHtmlString());
        }

        [Fact]
        public void RouteLinkNamedRoute_Unobtrusive()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: true);

            // Act
            MvcHtmlString actionLink = ajaxHelper.RouteLink("linkText", "namedroute", new AjaxOptions());

            // Assert
            Assert.Equal(@"<a data-ajax=""true"" href=""" + MvcHelper.AppPathModifier + @"/app/named/home/oldaction"">linkText</a>", actionLink.ToHtmlString());
        }

        [Fact]
        public void RouteLinkNamedRouteAnonymousAttributes()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: false);
            object htmlAttributes = new
            {
                foo = "bar",
                baz = "quux",
                foo_bar = "baz_quux"
            };
            AjaxOptions options = new AjaxOptions { UpdateTargetId = "update-div" };

            // Act
            MvcHtmlString actionLink = ajaxHelper.RouteLink("Some Text", "namedroute", options, htmlAttributes);

            // Assert
            Assert.Equal(@"<a baz=""quux"" foo=""bar"" foo-bar=""baz_quux"" href=""" + MvcHelper.AppPathModifier + @"/app/named/home/oldaction"" onclick=""Sys.Mvc.AsyncHyperlink.handleClick(this, new Sys.UI.DomEvent(event), { insertionMode: Sys.Mvc.InsertionMode.replace, updateTargetId: &#39;update-div&#39; });"">Some Text</a>", actionLink.ToHtmlString());
        }

        [Fact]
        public void RouteLinkNamedRouteAnonymousAttributes_Unobtrusive()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: true);
            object htmlAttributes = new
            {
                foo = "bar",
                baz = "quux",
                foo_bar = "baz_quux"
            };
            AjaxOptions options = new AjaxOptions { UpdateTargetId = "update-div" };

            // Act
            MvcHtmlString actionLink = ajaxHelper.RouteLink("Some Text", "namedroute", options, htmlAttributes);

            // Assert
            Assert.Equal(@"<a baz=""quux"" data-ajax=""true"" data-ajax-mode=""replace"" data-ajax-update=""#update-div"" foo=""bar"" foo-bar=""baz_quux"" href=""" + MvcHelper.AppPathModifier + @"/app/named/home/oldaction"">Some Text</a>", actionLink.ToHtmlString());
        }

        [Fact]
        public void RouteLinkNamedRouteTypedAttributes()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: false);
            Dictionary<string, object> htmlAttributes = new Dictionary<string, object> { { "foo", "bar" }, { "baz", "quux" }, { "foo_bar", "baz_quux" } };
            AjaxOptions options = new AjaxOptions { UpdateTargetId = "update-div" };

            // Act
            MvcHtmlString actionLink = ajaxHelper.RouteLink("Some Text", "namedroute", options, htmlAttributes);

            // Assert
            Assert.Equal(@"<a baz=""quux"" foo=""bar"" foo_bar=""baz_quux"" href=""" + MvcHelper.AppPathModifier + @"/app/named/home/oldaction"" onclick=""Sys.Mvc.AsyncHyperlink.handleClick(this, new Sys.UI.DomEvent(event), { insertionMode: Sys.Mvc.InsertionMode.replace, updateTargetId: &#39;update-div&#39; });"">Some Text</a>", actionLink.ToHtmlString());
        }

        [Fact]
        public void RouteLinkNamedRouteTypedAttributes_Unobtrusive()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: true);
            Dictionary<string, object> htmlAttributes = new Dictionary<string, object> { { "foo", "bar" }, { "baz", "quux" }, { "foo_bar", "baz_quux" } };
            AjaxOptions options = new AjaxOptions { UpdateTargetId = "update-div" };

            // Act
            MvcHtmlString actionLink = ajaxHelper.RouteLink("Some Text", "namedroute", options, htmlAttributes);

            // Assert
            Assert.Equal(@"<a baz=""quux"" data-ajax=""true"" data-ajax-mode=""replace"" data-ajax-update=""#update-div"" foo=""bar"" foo_bar=""baz_quux"" href=""" + MvcHelper.AppPathModifier + @"/app/named/home/oldaction"">Some Text</a>", actionLink.ToHtmlString());
        }

        [Fact]
        public void RouteLinkNamedRouteWithAnonymousValues()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: false);
            object values = new
            {
                action = "Action",
                controller = "Controller"
            };

            // Act
            MvcHtmlString actionLink = ajaxHelper.RouteLink("linkText", "namedroute", values, new AjaxOptions());

            // Assert
            Assert.Equal(@"<a href=""" + MvcHelper.AppPathModifier + @"/app/named/Controller/Action"" onclick=""Sys.Mvc.AsyncHyperlink.handleClick(this, new Sys.UI.DomEvent(event), { insertionMode: Sys.Mvc.InsertionMode.replace });"">linkText</a>", actionLink.ToHtmlString());
        }

        [Fact]
        public void RouteLinkNamedRouteWithAnonymousValues_Unobtrusive()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: true);
            object values = new
            {
                action = "Action",
                controller = "Controller"
            };

            // Act
            MvcHtmlString actionLink = ajaxHelper.RouteLink("linkText", "namedroute", values, new AjaxOptions());

            // Assert
            Assert.Equal(@"<a data-ajax=""true"" href=""" + MvcHelper.AppPathModifier + @"/app/named/Controller/Action"">linkText</a>", actionLink.ToHtmlString());
        }

        [Fact]
        public void RouteLinkNamedRouteAnonymousValuesAndAttributes()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: false);
            object values = new
            {
                action = "Action",
                controller = "Controller"
            };

            object htmlAttributes = new
            {
                foo = "bar",
                baz = "quux",
                foo_bar = "baz_quux"
            };
            AjaxOptions options = new AjaxOptions { UpdateTargetId = "update-div" };

            // Act
            MvcHtmlString actionLink = ajaxHelper.RouteLink("Some Text", "namedroute", values, options, htmlAttributes);

            // Assert
            Assert.Equal(@"<a baz=""quux"" foo=""bar"" foo-bar=""baz_quux"" href=""" + MvcHelper.AppPathModifier + @"/app/named/Controller/Action"" onclick=""Sys.Mvc.AsyncHyperlink.handleClick(this, new Sys.UI.DomEvent(event), { insertionMode: Sys.Mvc.InsertionMode.replace, updateTargetId: &#39;update-div&#39; });"">Some Text</a>", actionLink.ToHtmlString());
        }

        [Fact]
        public void RouteLinkNamedRouteAnonymousValuesAndAttributes_Unobtrusive()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: true);
            object values = new
            {
                action = "Action",
                controller = "Controller"
            };

            object htmlAttributes = new
            {
                foo = "bar",
                baz = "quux",
                foo_bar = "baz_quux"
            };
            AjaxOptions options = new AjaxOptions { UpdateTargetId = "update-div" };

            // Act
            MvcHtmlString actionLink = ajaxHelper.RouteLink("Some Text", "namedroute", values, options, htmlAttributes);

            // Assert
            Assert.Equal(@"<a baz=""quux"" data-ajax=""true"" data-ajax-mode=""replace"" data-ajax-update=""#update-div"" foo=""bar"" foo-bar=""baz_quux"" href=""" + MvcHelper.AppPathModifier + @"/app/named/Controller/Action"">Some Text</a>", actionLink.ToHtmlString());
        }

        [Fact]
        public void RouteLinkNamedRouteWithTypedValues()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: false);
            RouteValueDictionary values = new RouteValueDictionary
            {
                { "controller", "Controller" },
                { "action", "Action" }
            };

            // Act
            MvcHtmlString actionLink = ajaxHelper.RouteLink("linkText", "namedroute", values, new AjaxOptions());

            // Assert
            Assert.Equal(@"<a href=""" + MvcHelper.AppPathModifier + @"/app/named/Controller/Action"" onclick=""Sys.Mvc.AsyncHyperlink.handleClick(this, new Sys.UI.DomEvent(event), { insertionMode: Sys.Mvc.InsertionMode.replace });"">linkText</a>", actionLink.ToHtmlString());
        }

        [Fact]
        public void RouteLinkNamedRouteWithTypedValues_Unobtrusive()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: true);
            RouteValueDictionary values = new RouteValueDictionary
            {
                { "controller", "Controller" },
                { "action", "Action" }
            };

            // Act
            MvcHtmlString actionLink = ajaxHelper.RouteLink("linkText", "namedroute", values, new AjaxOptions());

            // Assert
            Assert.Equal(@"<a data-ajax=""true"" href=""" + MvcHelper.AppPathModifier + @"/app/named/Controller/Action"">linkText</a>", actionLink.ToHtmlString());
        }

        [Fact]
        public void RouteLinkNamedRouteTypedValuesAndAttributes()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: false);
            RouteValueDictionary values = new RouteValueDictionary
            {
                { "controller", "Controller" },
                { "action", "Action" }
            };

            Dictionary<string, object> htmlAttributes = new Dictionary<string, object> { { "foo", "bar" }, { "baz", "quux" }, { "foo_bar", "baz_quux" } };
            AjaxOptions options = new AjaxOptions { UpdateTargetId = "update-div" };

            // Act
            MvcHtmlString actionLink = ajaxHelper.RouteLink("Some Text", "namedroute", values, options, htmlAttributes);

            // Assert
            Assert.Equal(@"<a baz=""quux"" foo=""bar"" foo_bar=""baz_quux"" href=""" + MvcHelper.AppPathModifier + @"/app/named/Controller/Action"" onclick=""Sys.Mvc.AsyncHyperlink.handleClick(this, new Sys.UI.DomEvent(event), { insertionMode: Sys.Mvc.InsertionMode.replace, updateTargetId: &#39;update-div&#39; });"">Some Text</a>", actionLink.ToHtmlString());
        }

        [Fact]
        public void RouteLinkNamedRouteTypedValuesAndAttributes_Unobtrusive()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: true);
            RouteValueDictionary values = new RouteValueDictionary
            {
                { "controller", "Controller" },
                { "action", "Action" }
            };

            Dictionary<string, object> htmlAttributes = new Dictionary<string, object> { { "foo", "bar" }, { "baz", "quux" }, { "foo_bar", "baz_quux" } };
            AjaxOptions options = new AjaxOptions { UpdateTargetId = "update-div" };

            // Act
            MvcHtmlString actionLink = ajaxHelper.RouteLink("Some Text", "namedroute", values, options, htmlAttributes);

            // Assert
            Assert.Equal(@"<a baz=""quux"" data-ajax=""true"" data-ajax-mode=""replace"" data-ajax-update=""#update-div"" foo=""bar"" foo_bar=""baz_quux"" href=""" + MvcHelper.AppPathModifier + @"/app/named/Controller/Action"">Some Text</a>", actionLink.ToHtmlString());
        }

        [Fact]
        public void RouteLinkNamedRouteNullValuesAndAttributes()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: false);
            Dictionary<string, object> htmlAttributes = new Dictionary<string, object> { { "foo", "bar" }, { "baz", "quux" }, { "foo_bar", "baz_quux" } };
            AjaxOptions options = new AjaxOptions { UpdateTargetId = "update-div" };

            // Act
            MvcHtmlString actionLink = ajaxHelper.RouteLink("Some Text", "namedroute", null, options, htmlAttributes);

            // Assert
            Assert.Equal(@"<a baz=""quux"" foo=""bar"" foo_bar=""baz_quux"" href=""" + MvcHelper.AppPathModifier + @"/app/named/home/oldaction"" onclick=""Sys.Mvc.AsyncHyperlink.handleClick(this, new Sys.UI.DomEvent(event), { insertionMode: Sys.Mvc.InsertionMode.replace, updateTargetId: &#39;update-div&#39; });"">Some Text</a>", actionLink.ToHtmlString());
        }

        [Fact]
        public void RouteLinkNamedRouteNullValuesAndAttributes_Unobtrusive()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: true);
            Dictionary<string, object> htmlAttributes = new Dictionary<string, object> { { "foo", "bar" }, { "baz", "quux" }, { "foo_bar", "baz_quux" } };
            AjaxOptions options = new AjaxOptions { UpdateTargetId = "update-div" };

            // Act
            MvcHtmlString actionLink = ajaxHelper.RouteLink("Some Text", "namedroute", null, options, htmlAttributes);

            // Assert
            Assert.Equal(@"<a baz=""quux"" data-ajax=""true"" data-ajax-mode=""replace"" data-ajax-update=""#update-div"" foo=""bar"" foo_bar=""baz_quux"" href=""" + MvcHelper.AppPathModifier + @"/app/named/home/oldaction"">Some Text</a>", actionLink.ToHtmlString());
        }

        [Fact]
        public void RouteLinkWithHostName()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: false);
            Dictionary<string, object> htmlAttributes = new Dictionary<string, object> { { "foo", "bar" }, { "baz", "quux" }, { "foo_bar", "baz_quux" } };
            AjaxOptions options = new AjaxOptions { UpdateTargetId = "update-div" };

            // Act
            MvcHtmlString actionLink = ajaxHelper.RouteLink("Some Text", "namedroute", null, "baz.bar.foo", null, null, options, htmlAttributes);

            // Assert
            Assert.Equal(@"<a baz=""quux"" foo=""bar"" foo_bar=""baz_quux"" href=""http://baz.bar.foo" + MvcHelper.AppPathModifier + @"/app/named/home/oldaction"" onclick=""Sys.Mvc.AsyncHyperlink.handleClick(this, new Sys.UI.DomEvent(event), { insertionMode: Sys.Mvc.InsertionMode.replace, updateTargetId: &#39;update-div&#39; });"">Some Text</a>", actionLink.ToHtmlString());
        }

        [Fact]
        public void RouteLinkWithHostName_Unobtrusive()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: true);
            Dictionary<string, object> htmlAttributes = new Dictionary<string, object> { { "foo", "bar" }, { "baz", "quux" }, { "foo_bar", "baz_quux" } };
            AjaxOptions options = new AjaxOptions { UpdateTargetId = "update-div" };

            // Act
            MvcHtmlString actionLink = ajaxHelper.RouteLink("Some Text", "namedroute", null, "baz.bar.foo", null, null, options, htmlAttributes);

            // Assert
            Assert.Equal(@"<a baz=""quux"" data-ajax=""true"" data-ajax-mode=""replace"" data-ajax-update=""#update-div"" foo=""bar"" foo_bar=""baz_quux"" href=""http://baz.bar.foo" + MvcHelper.AppPathModifier + @"/app/named/home/oldaction"">Some Text</a>", actionLink.ToHtmlString());
        }

        // BeginForm

        [Fact]
        public void BeginFormOnlyWithNullOptions()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: false);
            StringWriter writer = new StringWriter();
            ajaxHelper.ViewContext.Writer = writer;

            // Act
            IDisposable form = ajaxHelper.BeginForm(null);

            // Assert
            Assert.Equal(@"<form action=""/rawUrl"" method=""post"" onclick=""Sys.Mvc.AsyncForm.handleClick(this, new Sys.UI.DomEvent(event));"" onsubmit=""Sys.Mvc.AsyncForm.handleSubmit(this, new Sys.UI.DomEvent(event), { insertionMode: Sys.Mvc.InsertionMode.replace });"">", writer.ToString());
        }

        [Fact]
        public void BeginFormOnlyWithNullOptions_Unobtrusive()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: true);
            StringWriter writer = new StringWriter();
            ajaxHelper.ViewContext.Writer = writer;

            // Act
            IDisposable form = ajaxHelper.BeginForm(null);

            // Assert
            Assert.Equal(@"<form action=""/rawUrl"" data-ajax=""true"" method=""post"">", writer.ToString());
        }

        [Fact]
        public void BeginFormWithNullActionName()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: false);
            AjaxOptions ajaxOptions = new AjaxOptions();
            StringWriter writer = new StringWriter();
            ajaxHelper.ViewContext.Writer = writer;

            // Act
            IDisposable form = ajaxHelper.BeginForm(null, ajaxOptions);

            // Assert
            Assert.Equal(@"<form action=""" + MvcHelper.AppPathModifier + @"/app/home/oldaction"" method=""post"" onclick=""Sys.Mvc.AsyncForm.handleClick(this, new Sys.UI.DomEvent(event));"" onsubmit=""Sys.Mvc.AsyncForm.handleSubmit(this, new Sys.UI.DomEvent(event), { insertionMode: Sys.Mvc.InsertionMode.replace });"">", writer.ToString());
        }

        [Fact]
        public void BeginFormWithNullActionName_Unobtrusive()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: true);
            AjaxOptions ajaxOptions = new AjaxOptions();
            StringWriter writer = new StringWriter();
            ajaxHelper.ViewContext.Writer = writer;

            // Act
            IDisposable form = ajaxHelper.BeginForm(null, ajaxOptions);

            // Assert
            Assert.Equal(@"<form action=""" + MvcHelper.AppPathModifier + @"/app/home/oldaction"" data-ajax=""true"" method=""post"">", writer.ToString());
        }

        [Fact]
        public void BeginFormWithNullOptions()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: false);
            AjaxOptions ajaxOptions = new AjaxOptions();
            StringWriter writer = new StringWriter();
            ajaxHelper.ViewContext.Writer = writer;

            // Act
            IDisposable form = ajaxHelper.BeginForm("Action", "Controller", null);

            // Assert
            Assert.Equal(@"<form action=""" + MvcHelper.AppPathModifier + @"/app/Controller/Action"" method=""post"" onclick=""Sys.Mvc.AsyncForm.handleClick(this, new Sys.UI.DomEvent(event));"" onsubmit=""Sys.Mvc.AsyncForm.handleSubmit(this, new Sys.UI.DomEvent(event), { insertionMode: Sys.Mvc.InsertionMode.replace });"">", writer.ToString());
        }

        [Fact]
        public void BeginFormWithNullOptions_Unobtrusive()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: true);
            AjaxOptions ajaxOptions = new AjaxOptions();
            StringWriter writer = new StringWriter();
            ajaxHelper.ViewContext.Writer = writer;

            // Act
            IDisposable form = ajaxHelper.BeginForm("Action", "Controller", null);

            // Assert
            Assert.Equal(@"<form action=""" + MvcHelper.AppPathModifier + @"/app/Controller/Action"" data-ajax=""true"" method=""post"">", writer.ToString());
        }

        [Fact]
        public void BeginForm()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: false);
            AjaxOptions ajaxOptions = new AjaxOptions();
            StringWriter writer = new StringWriter();
            ajaxHelper.ViewContext.Writer = writer;

            // Act
            IDisposable form = ajaxHelper.BeginForm(ajaxOptions);

            // Assert
            Assert.Equal(@"<form action=""/rawUrl"" method=""post"" onclick=""Sys.Mvc.AsyncForm.handleClick(this, new Sys.UI.DomEvent(event));"" onsubmit=""Sys.Mvc.AsyncForm.handleSubmit(this, new Sys.UI.DomEvent(event), { insertionMode: Sys.Mvc.InsertionMode.replace });"">", writer.ToString());
        }

        [Fact]
        public void BeginForm_Unobtrusive()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: true);
            AjaxOptions ajaxOptions = new AjaxOptions();
            StringWriter writer = new StringWriter();
            ajaxHelper.ViewContext.Writer = writer;

            // Act
            IDisposable form = ajaxHelper.BeginForm(ajaxOptions);

            // Assert
            Assert.Equal(@"<form action=""/rawUrl"" data-ajax=""true"" method=""post"">", writer.ToString());
        }

        [Fact]
        public void BeginFormAction()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: false);
            AjaxOptions ajaxOptions = new AjaxOptions();
            StringWriter writer = new StringWriter();
            ajaxHelper.ViewContext.Writer = writer;

            // Act
            IDisposable form = ajaxHelper.BeginForm("Action", ajaxOptions);

            // Assert
            Assert.Equal(@"<form action=""" + MvcHelper.AppPathModifier + @"/app/home/Action"" method=""post"" onclick=""Sys.Mvc.AsyncForm.handleClick(this, new Sys.UI.DomEvent(event));"" onsubmit=""Sys.Mvc.AsyncForm.handleSubmit(this, new Sys.UI.DomEvent(event), { insertionMode: Sys.Mvc.InsertionMode.replace });"">", writer.ToString());
        }

        [Fact]
        public void BeginFormAction_Unobtrusive()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: true);
            AjaxOptions ajaxOptions = new AjaxOptions();
            StringWriter writer = new StringWriter();
            ajaxHelper.ViewContext.Writer = writer;

            // Act
            IDisposable form = ajaxHelper.BeginForm("Action", ajaxOptions);

            // Assert
            Assert.Equal(@"<form action=""" + MvcHelper.AppPathModifier + @"/app/home/Action"" data-ajax=""true"" method=""post"">", writer.ToString());
        }

        [Fact]
        public void BeginFormAnonymousValues()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: false);
            AjaxOptions ajaxOptions = new AjaxOptions();
            object values = new { controller = "Controller" };
            StringWriter writer = new StringWriter();
            ajaxHelper.ViewContext.Writer = writer;

            // Act
            IDisposable form = ajaxHelper.BeginForm("Action", values, ajaxOptions);

            // Assert
            Assert.Equal(@"<form action=""" + MvcHelper.AppPathModifier + @"/app/Controller/Action"" method=""post"" onclick=""Sys.Mvc.AsyncForm.handleClick(this, new Sys.UI.DomEvent(event));"" onsubmit=""Sys.Mvc.AsyncForm.handleSubmit(this, new Sys.UI.DomEvent(event), { insertionMode: Sys.Mvc.InsertionMode.replace });"">", writer.ToString());
        }

        [Fact]
        public void BeginFormAnonymousValues_Unobtrusive()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: true);
            AjaxOptions ajaxOptions = new AjaxOptions();
            object values = new { controller = "Controller" };
            StringWriter writer = new StringWriter();
            ajaxHelper.ViewContext.Writer = writer;

            // Act
            IDisposable form = ajaxHelper.BeginForm("Action", values, ajaxOptions);

            // Assert
            Assert.Equal(@"<form action=""" + MvcHelper.AppPathModifier + @"/app/Controller/Action"" data-ajax=""true"" method=""post"">", writer.ToString());
        }

        [Fact]
        public void BeginFormAnonymousValuesAndAttributes()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: false);
            AjaxOptions ajaxOptions = new AjaxOptions { UpdateTargetId = "some-id" };
            object values = new { controller = "Controller" };
            object htmlAttributes = new { method = "get", foo_bar = "baz_quux" };
            StringWriter writer = new StringWriter();
            ajaxHelper.ViewContext.Writer = writer;

            // Act
            IDisposable form = ajaxHelper.BeginForm("Action", values, ajaxOptions, htmlAttributes);

            // Assert
            Assert.Equal(@"<form action=""" + MvcHelper.AppPathModifier + @"/app/Controller/Action"" foo-bar=""baz_quux"" method=""get"" onclick=""Sys.Mvc.AsyncForm.handleClick(this, new Sys.UI.DomEvent(event));"" onsubmit=""Sys.Mvc.AsyncForm.handleSubmit(this, new Sys.UI.DomEvent(event), { insertionMode: Sys.Mvc.InsertionMode.replace, updateTargetId: &#39;some-id&#39; });"">", writer.ToString());
        }

        [Fact]
        public void BeginFormAnonymousValuesAndAttributes_Unobtrusive()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: true);
            AjaxOptions ajaxOptions = new AjaxOptions { UpdateTargetId = "some-id" };
            object values = new { controller = "Controller" };
            object htmlAttributes = new { method = "get", foo_bar = "baz_quux" };
            StringWriter writer = new StringWriter();
            ajaxHelper.ViewContext.Writer = writer;

            // Act
            IDisposable form = ajaxHelper.BeginForm("Action", values, ajaxOptions, htmlAttributes);

            // Assert
            Assert.Equal(@"<form action=""" + MvcHelper.AppPathModifier + @"/app/Controller/Action"" data-ajax=""true"" data-ajax-mode=""replace"" data-ajax-update=""#some-id"" foo-bar=""baz_quux"" method=""get"">", writer.ToString());
        }

        [Fact]
        public void BeginFormTypedValues()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: false);
            AjaxOptions ajaxOptions = new AjaxOptions();
            RouteValueDictionary values = new RouteValueDictionary
            {
                { "controller", "Controller" }
            };
            StringWriter writer = new StringWriter();
            ajaxHelper.ViewContext.Writer = writer;

            // Act
            IDisposable form = ajaxHelper.BeginForm("Action", values, ajaxOptions);

            // Assert
            Assert.Equal(@"<form action=""" + MvcHelper.AppPathModifier + @"/app/Controller/Action"" method=""post"" onclick=""Sys.Mvc.AsyncForm.handleClick(this, new Sys.UI.DomEvent(event));"" onsubmit=""Sys.Mvc.AsyncForm.handleSubmit(this, new Sys.UI.DomEvent(event), { insertionMode: Sys.Mvc.InsertionMode.replace });"">", writer.ToString());
        }

        [Fact]
        public void BeginFormTypedValues_Unobtrusive()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: true);
            AjaxOptions ajaxOptions = new AjaxOptions();
            RouteValueDictionary values = new RouteValueDictionary
            {
                { "controller", "Controller" }
            };
            StringWriter writer = new StringWriter();
            ajaxHelper.ViewContext.Writer = writer;

            // Act
            IDisposable form = ajaxHelper.BeginForm("Action", values, ajaxOptions);

            // Assert
            Assert.Equal(@"<form action=""" + MvcHelper.AppPathModifier + @"/app/Controller/Action"" data-ajax=""true"" method=""post"">", writer.ToString());
        }

        [Fact]
        public void BeginFormTypedValuesAndAttributes()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: false);
            AjaxOptions ajaxOptions = new AjaxOptions { UpdateTargetId = "some-id" };
            RouteValueDictionary values = new RouteValueDictionary
            {
                { "controller", "Controller" }
            };
            Dictionary<string, object> htmlAttributes = new Dictionary<string, object>
            {
                { "method", "get" },
                { "foo_bar", "baz_quux" }
            };
            StringWriter writer = new StringWriter();
            ajaxHelper.ViewContext.Writer = writer;

            // Act
            IDisposable form = ajaxHelper.BeginForm("Action", values, ajaxOptions, htmlAttributes);

            // Assert
            Assert.Equal(@"<form action=""" + MvcHelper.AppPathModifier + @"/app/Controller/Action"" foo_bar=""baz_quux"" method=""get"" onclick=""Sys.Mvc.AsyncForm.handleClick(this, new Sys.UI.DomEvent(event));"" onsubmit=""Sys.Mvc.AsyncForm.handleSubmit(this, new Sys.UI.DomEvent(event), { insertionMode: Sys.Mvc.InsertionMode.replace, updateTargetId: &#39;some-id&#39; });"">", writer.ToString());
        }

        [Fact]
        public void BeginFormTypedValuesAndAttributes_Unobtrusive()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: true);
            AjaxOptions ajaxOptions = new AjaxOptions { UpdateTargetId = "some-id" };
            RouteValueDictionary values = new RouteValueDictionary
            {
                { "controller", "Controller" }
            };
            Dictionary<string, object> htmlAttributes = new Dictionary<string, object>
            {
                { "method", "get" },
                { "foo_bar", "baz_quux" }
            };
            StringWriter writer = new StringWriter();
            ajaxHelper.ViewContext.Writer = writer;

            // Act
            IDisposable form = ajaxHelper.BeginForm("Action", values, ajaxOptions, htmlAttributes);

            // Assert
            Assert.Equal(@"<form action=""" + MvcHelper.AppPathModifier + @"/app/Controller/Action"" data-ajax=""true"" data-ajax-mode=""replace"" data-ajax-update=""#some-id"" foo_bar=""baz_quux"" method=""get"">", writer.ToString());
        }

        [Fact]
        public void BeginFormController()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: false);
            AjaxOptions ajaxOptions = new AjaxOptions();
            StringWriter writer = new StringWriter();
            ajaxHelper.ViewContext.Writer = writer;

            // Act
            IDisposable form = ajaxHelper.BeginForm("Action", "Controller", ajaxOptions);

            // Assert
            Assert.Equal(@"<form action=""" + MvcHelper.AppPathModifier + @"/app/Controller/Action"" method=""post"" onclick=""Sys.Mvc.AsyncForm.handleClick(this, new Sys.UI.DomEvent(event));"" onsubmit=""Sys.Mvc.AsyncForm.handleSubmit(this, new Sys.UI.DomEvent(event), { insertionMode: Sys.Mvc.InsertionMode.replace });"">", writer.ToString());
        }

        [Fact]
        public void BeginFormController_Unobtrusive()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: true);
            AjaxOptions ajaxOptions = new AjaxOptions();
            StringWriter writer = new StringWriter();
            ajaxHelper.ViewContext.Writer = writer;

            // Act
            IDisposable form = ajaxHelper.BeginForm("Action", "Controller", ajaxOptions);

            // Assert
            Assert.Equal(@"<form action=""" + MvcHelper.AppPathModifier + @"/app/Controller/Action"" data-ajax=""true"" method=""post"">", writer.ToString());
        }

        [Fact]
        public void BeginFormControllerAnonymousValues()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: false);
            AjaxOptions ajaxOptions = new AjaxOptions();
            object values = new { id = 5 };
            StringWriter writer = new StringWriter();
            ajaxHelper.ViewContext.Writer = writer;

            // Act
            IDisposable form = ajaxHelper.BeginForm("Action", "Controller", values, ajaxOptions);

            // Assert
            Assert.Equal(@"<form action=""" + MvcHelper.AppPathModifier + @"/app/Controller/Action/5"" method=""post"" onclick=""Sys.Mvc.AsyncForm.handleClick(this, new Sys.UI.DomEvent(event));"" onsubmit=""Sys.Mvc.AsyncForm.handleSubmit(this, new Sys.UI.DomEvent(event), { insertionMode: Sys.Mvc.InsertionMode.replace });"">", writer.ToString());
        }

        [Fact]
        public void BeginFormControllerAnonymousValues_Unobtrusive()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: true);
            AjaxOptions ajaxOptions = new AjaxOptions();
            object values = new { id = 5 };
            StringWriter writer = new StringWriter();
            ajaxHelper.ViewContext.Writer = writer;

            // Act
            IDisposable form = ajaxHelper.BeginForm("Action", "Controller", values, ajaxOptions);

            // Assert
            Assert.Equal(@"<form action=""" + MvcHelper.AppPathModifier + @"/app/Controller/Action/5"" data-ajax=""true"" method=""post"">", writer.ToString());
        }

        [Fact]
        public void BeginFormControllerAnonymousValuesAndAttributes()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: false);
            AjaxOptions ajaxOptions = new AjaxOptions();
            object values = new { id = 5 };
            object htmlAttributes = new { method = "get", foo_bar = "baz_quux" };
            StringWriter writer = new StringWriter();
            ajaxHelper.ViewContext.Writer = writer;

            // Act
            IDisposable form = ajaxHelper.BeginForm("Action", "Controller", values, ajaxOptions, htmlAttributes);

            // Assert
            Assert.Equal(@"<form action=""" + MvcHelper.AppPathModifier + @"/app/Controller/Action/5"" foo-bar=""baz_quux"" method=""get"" onclick=""Sys.Mvc.AsyncForm.handleClick(this, new Sys.UI.DomEvent(event));"" onsubmit=""Sys.Mvc.AsyncForm.handleSubmit(this, new Sys.UI.DomEvent(event), { insertionMode: Sys.Mvc.InsertionMode.replace });"">", writer.ToString());
        }

        [Fact]
        public void BeginFormControllerAnonymousValuesAndAttributes_Unobtrusive()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: true);
            AjaxOptions ajaxOptions = new AjaxOptions();
            object values = new { id = 5 };
            object htmlAttributes = new { method = "get", foo_bar = "baz_quux" };
            StringWriter writer = new StringWriter();
            ajaxHelper.ViewContext.Writer = writer;

            // Act
            IDisposable form = ajaxHelper.BeginForm("Action", "Controller", values, ajaxOptions, htmlAttributes);

            // Assert
            Assert.Equal(@"<form action=""" + MvcHelper.AppPathModifier + @"/app/Controller/Action/5"" data-ajax=""true"" foo-bar=""baz_quux"" method=""get"">", writer.ToString());
        }

        [Fact]
        public void BeginFormControllerTypedValues()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: false);
            AjaxOptions ajaxOptions = new AjaxOptions();
            RouteValueDictionary values = new RouteValueDictionary
            {
                { "id", 5 }
            };
            StringWriter writer = new StringWriter();
            ajaxHelper.ViewContext.Writer = writer;

            // Act
            IDisposable form = ajaxHelper.BeginForm("Action", "Controller", values, ajaxOptions);

            // Assert
            Assert.Equal(@"<form action=""" + MvcHelper.AppPathModifier + @"/app/Controller/Action/5"" method=""post"" onclick=""Sys.Mvc.AsyncForm.handleClick(this, new Sys.UI.DomEvent(event));"" onsubmit=""Sys.Mvc.AsyncForm.handleSubmit(this, new Sys.UI.DomEvent(event), { insertionMode: Sys.Mvc.InsertionMode.replace });"">", writer.ToString());
        }

        [Fact]
        public void BeginFormControllerTypedValues_Unobtrusive()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: true);
            AjaxOptions ajaxOptions = new AjaxOptions();
            RouteValueDictionary values = new RouteValueDictionary
            {
                { "id", 5 }
            };
            StringWriter writer = new StringWriter();
            ajaxHelper.ViewContext.Writer = writer;

            // Act
            IDisposable form = ajaxHelper.BeginForm("Action", "Controller", values, ajaxOptions);

            // Assert
            Assert.Equal(@"<form action=""" + MvcHelper.AppPathModifier + @"/app/Controller/Action/5"" data-ajax=""true"" method=""post"">", writer.ToString());
        }

        [Fact]
        public void BeginFormControllerTypedValuesAndAttributes()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: false);
            AjaxOptions ajaxOptions = new AjaxOptions();
            RouteValueDictionary values = new RouteValueDictionary
            {
                { "id", 5 }
            };
            Dictionary<string, object> htmlAttributes = new Dictionary<string, object>
            {
                { "method", "get" },
                { "foo_bar", "baz_quux" }
            };
            StringWriter writer = new StringWriter();
            ajaxHelper.ViewContext.Writer = writer;

            // Act
            IDisposable form = ajaxHelper.BeginForm("Action", "Controller", values, ajaxOptions, htmlAttributes);

            // Assert
            Assert.Equal(@"<form action=""" + MvcHelper.AppPathModifier + @"/app/Controller/Action/5"" foo_bar=""baz_quux"" method=""get"" onclick=""Sys.Mvc.AsyncForm.handleClick(this, new Sys.UI.DomEvent(event));"" onsubmit=""Sys.Mvc.AsyncForm.handleSubmit(this, new Sys.UI.DomEvent(event), { insertionMode: Sys.Mvc.InsertionMode.replace });"">", writer.ToString());
        }

        [Fact]
        public void BeginFormControllerTypedValuesAndAttributes_Unobtrusive()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: true);
            AjaxOptions ajaxOptions = new AjaxOptions();
            RouteValueDictionary values = new RouteValueDictionary
            {
                { "id", 5 }
            };
            Dictionary<string, object> htmlAttributes = new Dictionary<string, object>
            {
                { "method", "get" },
                { "foo_bar", "baz_quux" }
            };
            StringWriter writer = new StringWriter();
            ajaxHelper.ViewContext.Writer = writer;

            // Act
            IDisposable form = ajaxHelper.BeginForm("Action", "Controller", values, ajaxOptions, htmlAttributes);

            // Assert
            Assert.Equal(@"<form action=""" + MvcHelper.AppPathModifier + @"/app/Controller/Action/5"" data-ajax=""true"" foo_bar=""baz_quux"" method=""get"">", writer.ToString());
        }

        [Fact]
        public void BeginFormWithTargetId()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: false);
            AjaxOptions ajaxOptions = new AjaxOptions { UpdateTargetId = "some-id" };
            StringWriter writer = new StringWriter();
            ajaxHelper.ViewContext.Writer = writer;

            // Act
            IDisposable form = ajaxHelper.BeginForm("Action", "Controller", ajaxOptions);

            // Assert
            Assert.Equal(@"<form action=""" + MvcHelper.AppPathModifier + @"/app/Controller/Action"" method=""post"" onclick=""Sys.Mvc.AsyncForm.handleClick(this, new Sys.UI.DomEvent(event));"" onsubmit=""Sys.Mvc.AsyncForm.handleSubmit(this, new Sys.UI.DomEvent(event), { insertionMode: Sys.Mvc.InsertionMode.replace, updateTargetId: &#39;some-id&#39; });"">", writer.ToString());
        }

        [Fact]
        public void BeginFormWithTargetId_Unobtrusive()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: true);
            AjaxOptions ajaxOptions = new AjaxOptions { UpdateTargetId = "some-id" };
            StringWriter writer = new StringWriter();
            ajaxHelper.ViewContext.Writer = writer;

            // Act
            IDisposable form = ajaxHelper.BeginForm("Action", "Controller", ajaxOptions);

            // Assert
            Assert.Equal(@"<form action=""" + MvcHelper.AppPathModifier + @"/app/Controller/Action"" data-ajax=""true"" data-ajax-mode=""replace"" data-ajax-update=""#some-id"" method=""post"">", writer.ToString());
        }

        // BeginRouteForm

        [Fact]
        public void BeginRouteForm()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: false);
            AjaxOptions ajaxOptions = new AjaxOptions();
            StringWriter writer = new StringWriter();
            ajaxHelper.ViewContext.Writer = writer;

            // Act
            IDisposable form = ajaxHelper.BeginRouteForm("namedroute", ajaxOptions);

            // Assert
            Assert.Equal(@"<form action=""" + MvcHelper.AppPathModifier + @"/app/named/home/oldaction"" method=""post"" onclick=""Sys.Mvc.AsyncForm.handleClick(this, new Sys.UI.DomEvent(event));"" onsubmit=""Sys.Mvc.AsyncForm.handleSubmit(this, new Sys.UI.DomEvent(event), { insertionMode: Sys.Mvc.InsertionMode.replace });"">", writer.ToString());
        }

        [Fact]
        public void BeginRouteForm_Unobtrusive()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: true);
            AjaxOptions ajaxOptions = new AjaxOptions();
            StringWriter writer = new StringWriter();
            ajaxHelper.ViewContext.Writer = writer;

            // Act
            IDisposable form = ajaxHelper.BeginRouteForm("namedroute", ajaxOptions);

            // Assert
            Assert.Equal(@"<form action=""" + MvcHelper.AppPathModifier + @"/app/named/home/oldaction"" data-ajax=""true"" method=""post"">", writer.ToString());
        }

        [Fact]
        public void BeginRouteFormAnonymousValues()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: false);
            AjaxOptions ajaxOptions = new AjaxOptions();
            AjaxHelper poes = GetAjaxHelper(unobtrusiveJavaScript: false);
            StringWriter writer = new StringWriter();
            ajaxHelper.ViewContext.Writer = writer;

            // Act
            IDisposable form = ajaxHelper.BeginRouteForm("namedroute", null, ajaxOptions);

            // Assert
            Assert.Equal(@"<form action=""" + MvcHelper.AppPathModifier + @"/app/named/home/oldaction"" method=""post"" onclick=""Sys.Mvc.AsyncForm.handleClick(this, new Sys.UI.DomEvent(event));"" onsubmit=""Sys.Mvc.AsyncForm.handleSubmit(this, new Sys.UI.DomEvent(event), { insertionMode: Sys.Mvc.InsertionMode.replace });"">", writer.ToString());
        }

        [Fact]
        public void BeginRouteFormAnonymousValues_Unobtrusive()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: true);
            AjaxOptions ajaxOptions = new AjaxOptions();
            AjaxHelper poes = GetAjaxHelper(unobtrusiveJavaScript: true);
            StringWriter writer = new StringWriter();
            ajaxHelper.ViewContext.Writer = writer;

            // Act
            IDisposable form = ajaxHelper.BeginRouteForm("namedroute", null, ajaxOptions);

            // Assert
            Assert.Equal(@"<form action=""" + MvcHelper.AppPathModifier + @"/app/named/home/oldaction"" data-ajax=""true"" method=""post"">", writer.ToString());
        }

        [Fact]
        public void BeginRouteFormAnonymousValuesAndAttributes()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: false);
            AjaxOptions ajaxOptions = new AjaxOptions { UpdateTargetId = "some-id" };
            object htmlAttributes = new { method = "get", foo_bar = "baz_quux" };
            StringWriter writer = new StringWriter();
            ajaxHelper.ViewContext.Writer = writer;

            // Act
            IDisposable form = ajaxHelper.BeginRouteForm("namedroute", null, ajaxOptions, htmlAttributes);

            // Assert
            Assert.Equal(@"<form action=""" + MvcHelper.AppPathModifier + @"/app/named/home/oldaction"" foo-bar=""baz_quux"" method=""get"" onclick=""Sys.Mvc.AsyncForm.handleClick(this, new Sys.UI.DomEvent(event));"" onsubmit=""Sys.Mvc.AsyncForm.handleSubmit(this, new Sys.UI.DomEvent(event), { insertionMode: Sys.Mvc.InsertionMode.replace, updateTargetId: &#39;some-id&#39; });"">", writer.ToString());
        }

        [Fact]
        public void BeginRouteFormAnonymousValuesAndAttributes_Unobtrusive()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: true);
            AjaxOptions ajaxOptions = new AjaxOptions { UpdateTargetId = "some-id" };
            object htmlAttributes = new { method = "get", foo_bar = "baz_quux" };
            StringWriter writer = new StringWriter();
            ajaxHelper.ViewContext.Writer = writer;

            // Act
            IDisposable form = ajaxHelper.BeginRouteForm("namedroute", null, ajaxOptions, htmlAttributes);

            // Assert
            Assert.Equal(@"<form action=""" + MvcHelper.AppPathModifier + @"/app/named/home/oldaction"" data-ajax=""true"" data-ajax-mode=""replace"" data-ajax-update=""#some-id"" foo-bar=""baz_quux"" method=""get"">", writer.ToString());
        }

        [Fact]
        public void BeginRouteFormCanUseNamedRouteWithoutSpecifyingDefaults()
        {
            // DevDiv 217072: Non-mvc specific helpers should not give default values for controller and action

            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: false);
            ajaxHelper.RouteCollection.MapRoute("MyRouteName", "any/url", new { controller = "Charlie" });
            StringWriter writer = new StringWriter();
            ajaxHelper.ViewContext.Writer = writer;

            // Act
            IDisposable form = ajaxHelper.BeginRouteForm("MyRouteName", new AjaxOptions());

            // Assert
            Assert.Equal(@"<form action=""" + MvcHelper.AppPathModifier + @"/app/any/url"" method=""post"" onclick=""Sys.Mvc.AsyncForm.handleClick(this, new Sys.UI.DomEvent(event));"" onsubmit=""Sys.Mvc.AsyncForm.handleSubmit(this, new Sys.UI.DomEvent(event), { insertionMode: Sys.Mvc.InsertionMode.replace });"">", writer.ToString());
        }

        [Fact]
        public void BeginRouteFormCanUseNamedRouteWithoutSpecifyingDefaults_Unobtrusive()
        {
            // DevDiv 217072: Non-mvc specific helpers should not give default values for controller and action

            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: true);
            ajaxHelper.RouteCollection.MapRoute("MyRouteName", "any/url", new { controller = "Charlie" });
            StringWriter writer = new StringWriter();
            ajaxHelper.ViewContext.Writer = writer;

            // Act
            IDisposable form = ajaxHelper.BeginRouteForm("MyRouteName", new AjaxOptions());

            // Assert
            Assert.Equal(@"<form action=""" + MvcHelper.AppPathModifier + @"/app/any/url"" data-ajax=""true"" method=""post"">", writer.ToString());
        }

        [Fact]
        public void BeginRouteFormTypedValues()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: false);
            AjaxOptions ajaxOptions = new AjaxOptions();
            RouteValueDictionary values = new RouteValueDictionary();
            StringWriter writer = new StringWriter();
            ajaxHelper.ViewContext.Writer = writer;

            // Act
            IDisposable form = ajaxHelper.BeginRouteForm("namedroute", values, ajaxOptions);

            // Assert
            Assert.Equal(@"<form action=""" + MvcHelper.AppPathModifier + @"/app/named/home/oldaction"" method=""post"" onclick=""Sys.Mvc.AsyncForm.handleClick(this, new Sys.UI.DomEvent(event));"" onsubmit=""Sys.Mvc.AsyncForm.handleSubmit(this, new Sys.UI.DomEvent(event), { insertionMode: Sys.Mvc.InsertionMode.replace });"">", writer.ToString());
        }

        [Fact]
        public void BeginRouteFormTypedValues_Unobtrusive()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: true);
            AjaxOptions ajaxOptions = new AjaxOptions();
            RouteValueDictionary values = new RouteValueDictionary();
            StringWriter writer = new StringWriter();
            ajaxHelper.ViewContext.Writer = writer;

            // Act
            IDisposable form = ajaxHelper.BeginRouteForm("namedroute", values, ajaxOptions);

            // Assert
            Assert.Equal(@"<form action=""" + MvcHelper.AppPathModifier + @"/app/named/home/oldaction"" data-ajax=""true"" method=""post"">", writer.ToString());
        }

        [Fact]
        public void BeginRouteFormTypedValuesAndAttributes()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: false);
            Dictionary<string, object> htmlAttributes = new Dictionary<string, object> { { "method", "get" }, { "foo_bar", "baz_quux" } };
            AjaxOptions ajaxOptions = new AjaxOptions { UpdateTargetId = "some-id" };
            RouteValueDictionary values = new RouteValueDictionary();
            StringWriter writer = new StringWriter();
            ajaxHelper.ViewContext.Writer = writer;

            // Act
            IDisposable form = ajaxHelper.BeginRouteForm("namedroute", values, ajaxOptions, htmlAttributes);

            // Assert
            Assert.Equal(@"<form action=""" + MvcHelper.AppPathModifier + @"/app/named/home/oldaction"" foo_bar=""baz_quux"" method=""get"" onclick=""Sys.Mvc.AsyncForm.handleClick(this, new Sys.UI.DomEvent(event));"" onsubmit=""Sys.Mvc.AsyncForm.handleSubmit(this, new Sys.UI.DomEvent(event), { insertionMode: Sys.Mvc.InsertionMode.replace, updateTargetId: &#39;some-id&#39; });"">", writer.ToString());
        }

        [Fact]
        public void BeginRouteFormTypedValuesAndAttributes_Unobtrusive()
        {
            // Arrange
            AjaxHelper ajaxHelper = GetAjaxHelper(unobtrusiveJavaScript: true);
            Dictionary<string, object> htmlAttributes = new Dictionary<string, object> { { "method", "get" }, { "foo_bar", "baz_quux" } };
            AjaxOptions ajaxOptions = new AjaxOptions { UpdateTargetId = "some-id" };
            RouteValueDictionary values = new RouteValueDictionary();
            StringWriter writer = new StringWriter();
            ajaxHelper.ViewContext.Writer = writer;

            // Act
            IDisposable form = ajaxHelper.BeginRouteForm("namedroute", values, ajaxOptions, htmlAttributes);

            // Assert
            Assert.Equal(@"<form action=""" + MvcHelper.AppPathModifier + @"/app/named/home/oldaction"" data-ajax=""true"" data-ajax-mode=""replace"" data-ajax-update=""#some-id"" foo_bar=""baz_quux"" method=""get"">", writer.ToString());
        }

        // Helpers

        private static AjaxHelper GetAjaxHelper(bool unobtrusiveJavaScript = false)
        {
            var mockRequest = new Mock<HttpRequestBase>();
            mockRequest.Setup(o => o.Url).Returns(new Uri("http://foo.bar.baz"));
            mockRequest.Setup(o => o.RawUrl).Returns("/rawUrl");
            mockRequest.Setup(o => o.PathInfo).Returns(String.Empty);
            mockRequest.Setup(o => o.ApplicationPath).Returns("/app/");

            var mockResponse = new Mock<HttpResponseBase>();
            mockResponse.Setup(o => o.ApplyAppPathModifier(It.IsAny<string>())).Returns<string>(r => MvcHelper.AppPathModifier + r);

            var mockHttpContext = new Mock<HttpContextBase>();
            mockHttpContext.Setup(o => o.Request).Returns(mockRequest.Object);
            mockHttpContext.Setup(o => o.Session).Returns((HttpSessionStateBase)null);
            mockHttpContext.Setup(o => o.Items).Returns(new Hashtable());
            mockHttpContext.Setup(o => o.Response).Returns(mockResponse.Object);

            var routes = new RouteCollection();
            routes.MapRoute("default", "{controller}/{action}/{id}", new { id = "defaultid" });
            routes.MapRoute("namedroute", "named/{controller}/{action}/{id}", new { id = "defaultid" });

            var routeData = new RouteData();
            routeData.Values.Add("controller", "home");
            routeData.Values.Add("action", "oldaction");

            var viewContext = new ViewContext()
            {
                HttpContext = mockHttpContext.Object,
                RouteData = routeData,
                UnobtrusiveJavaScriptEnabled = unobtrusiveJavaScript,
                Writer = TextWriter.Null
            };

            return new AjaxHelper(viewContext, new Mock<IViewDataContainer>().Object, routes);
        }

        public void Dispose()
        {
            // Reset ScopeStorage (written via e.g. ViewContext.UnobtrusiveJavaScriptEnabled) between tests to avoid unexpected interactions.
            ScopeStorage.CurrentProvider = new StaticScopeStorageProvider();
            ScopeStorage.GlobalScope.Clear();
        }
    }
}
