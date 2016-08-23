// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.WebPages.Html;
using System.Web.WebPages.Scope;
using Microsoft.TestCommon;

namespace System.Web.WebPages.Test
{
    [Xunit.Collection("Uses ScopeStorage or ViewEngines.Engines")]
    public class ValidationHelperTest : IDisposable
    {
        [Fact]
        public void ValidationMessageAllowsEmptyModelName()
        {
            // Arrange
            ModelStateDictionary dictionary = new ModelStateDictionary();
            dictionary.AddError("test", "some error text");
            HtmlHelper htmlHelper = HtmlHelperFactory.Create(dictionary);

            // Act
            var html = htmlHelper.ValidationMessage("test");

            // Assert
            Assert.Equal("<span class=\"field-validation-error\" data-valmsg-for=\"test\" data-valmsg-replace=\"true\">some error text</span>", html.ToHtmlString());
        }

        [Fact]
        public void ValidationMessageReturnsFirstError()
        {
            // Arrange
            HtmlHelper htmlHelper = HtmlHelperFactory.Create(GetModelStateWithErrors());

            // Act
            var html = htmlHelper.ValidationMessage("foo");

            // Assert
            Assert.Equal("<span class=\"field-validation-error\" data-valmsg-for=\"foo\" data-valmsg-replace=\"true\">foo error &lt;1&gt;</span>", html.ToHtmlString());
        }

        [Fact]
        public void ValidationMessageUsesValidCssClassIfFieldDoesNotHaveErrors()
        {
            // Arrange
            HtmlHelper htmlHelper = HtmlHelperFactory.Create(GetModelStateWithErrors());

            // Act
            var html = htmlHelper.ValidationMessage("baz");

            // Assert
            Assert.Equal("<span class=\"field-validation-valid\" data-valmsg-for=\"baz\" data-valmsg-replace=\"true\"></span>", html.ToString());
        }

        [Fact]
        public void ValidationMessageReturnsWithObjectAttributes()
        {
            // Arrange
            HtmlHelper htmlHelper = HtmlHelperFactory.Create(GetModelStateWithErrors());

            // Act
            var html = htmlHelper.ValidationMessage("foo", new { attr = "attr-value" });

            // Assert
            Assert.Equal("<span attr=\"attr-value\" class=\"field-validation-error\" data-valmsg-for=\"foo\" data-valmsg-replace=\"true\">foo error &lt;1&gt;</span>",
                         html.ToHtmlString());
        }

        [Fact]
        public void ValidationMessageReturnsWithCustomMessage()
        {
            // Arrange
            HtmlHelper htmlHelper = HtmlHelperFactory.Create(GetModelStateWithErrors());

            // Atc
            var html = htmlHelper.ValidationMessage("foo", "bar error");

            // Assert
            Assert.Equal("<span class=\"field-validation-error\" data-valmsg-for=\"foo\" data-valmsg-replace=\"false\">bar error</span>", html.ToHtmlString());
        }

        [Fact]
        public void ValidationMessageReturnsWithCustomMessageAndObjectAttributes()
        {
            // Arrange
            HtmlHelper htmlHelper = HtmlHelperFactory.Create(GetModelStateWithErrors());

            // Act
            var html = htmlHelper.ValidationMessage("foo", "bar error", new { baz = "baz" });

            // Assert
            Assert.Equal("<span baz=\"baz\" class=\"field-validation-error\" data-valmsg-for=\"foo\" data-valmsg-replace=\"false\">bar error</span>", html.ToHtmlString());
        }

        [Fact]
        public void ValidationMessageWithModelStateAndNoErrors()
        {
            // Arrange
            HtmlHelper htmlHelper = HtmlHelperFactory.Create(GetModelStateWithErrors());

            // Act
            var html = htmlHelper.ValidationMessage("baz");

            // Assert
            Assert.Equal("<span class=\"field-validation-valid\" data-valmsg-for=\"baz\" data-valmsg-replace=\"true\"></span>", html.ToString());
        }

        [Fact]
        public void ValidationMessageWithAttributesFromAnonymousObject_WithUnderscoreInName_TransformsUnderscoresToDashs()
        {
            HtmlHelperTest.AssertHelperTransformsAttributesUnderscoresToDashs((helper, attributes) =>
                helper.ValidationMessage("foo", attributes));

            HtmlHelperTest.AssertHelperTransformsAttributesUnderscoresToDashs((helper, attributes) =>
                helper.ValidationMessage("foo", "bar", attributes));
        }

        [Fact]
        public void ValidationSummary()
        {
            // Arrange
            HtmlHelper htmlHelper = HtmlHelperFactory.Create(GetModelStateWithErrors());

            // Act
            var html = htmlHelper.ValidationSummary();

            // Assert
            Assert.Equal(
                "<div class=\"validation-summary-errors\" data-valmsg-summary=\"true\"><ul>" + Environment.NewLine
              + "<li>foo error &lt;1&gt;</li>" + Environment.NewLine
              + "<li>foo error &lt;2&gt;</li>" + Environment.NewLine
              + "<li>bar error &lt;1&gt;</li>" + Environment.NewLine
              + "<li>bar error &lt;2&gt;</li>" + Environment.NewLine
              + "</ul></div>",
                html.ToHtmlString());
        }

        [Fact]
        public void ValidationSummaryWithMessage()
        {
            // Arrange
            HtmlHelper htmlHelper = HtmlHelperFactory.Create(GetModelStateWithErrors());

            // Act
            var html = htmlHelper.ValidationSummary("test message");

            // Assert
            Assert.Equal(
                "<div class=\"validation-summary-errors\" data-valmsg-summary=\"true\"><span>test message</span>" + Environment.NewLine
              + "<ul>" + Environment.NewLine
              + "<li>foo error &lt;1&gt;</li>" + Environment.NewLine
              + "<li>foo error &lt;2&gt;</li>" + Environment.NewLine
              + "<li>bar error &lt;1&gt;</li>" + Environment.NewLine
              + "<li>bar error &lt;2&gt;</li>" + Environment.NewLine
              + "</ul></div>",
                html.ToHtmlString());
        }

        [Fact]
        public void ValidationSummaryWithFormErrors()
        {
            // Arrange
            HtmlHelper htmlHelper = HtmlHelperFactory.Create(GetModelStateWithFormErrors());

            // Act
            var html = htmlHelper.ValidationSummary();

            // Assert
            Assert.Equal(
                "<div class=\"validation-summary-errors\" data-valmsg-summary=\"true\"><ul>" + Environment.NewLine
              + "<li>foo error &lt;1&gt;</li>" + Environment.NewLine
              + "<li>foo error &lt;2&gt;</li>" + Environment.NewLine
              + "<li>bar error &lt;1&gt;</li>" + Environment.NewLine
              + "<li>bar error &lt;2&gt;</li>" + Environment.NewLine
              + "<li>some form error &lt;1&gt;</li>" + Environment.NewLine
              + "<li>some form error &lt;2&gt;</li>" + Environment.NewLine
              + "</ul></div>",
                html.ToHtmlString());
        }

        [Fact]
        public void ValidationSummaryWithFormErrorsAndExcludeFieldErrors()
        {
            // Arrange
            HtmlHelper htmlHelper = HtmlHelperFactory.Create(GetModelStateWithFormErrors());

            // Act
            var html = htmlHelper.ValidationSummary(excludeFieldErrors: true);

            // Assert
            Assert.Equal(
                "<div class=\"validation-summary-errors\"><ul>" + Environment.NewLine
              + "<li>some form error &lt;1&gt;</li>" + Environment.NewLine
              + "<li>some form error &lt;2&gt;</li>" + Environment.NewLine
              + "</ul></div>",
                html.ToHtmlString());
        }

        [Fact]
        public void ValidationSummaryWithObjectProperties()
        {
            // Arrange
            HtmlHelper htmlHelper = HtmlHelperFactory.Create(GetModelStateWithErrors());

            // Act
            var html = htmlHelper.ValidationSummary(new { attr = "attr-value", @class = "my-class" });

            // Assert
            Assert.Equal(
                "<div attr=\"attr-value\" class=\"validation-summary-errors my-class\" data-valmsg-summary=\"true\"><ul>" + Environment.NewLine
              + "<li>foo error &lt;1&gt;</li>" + Environment.NewLine
              + "<li>foo error &lt;2&gt;</li>" + Environment.NewLine
              + "<li>bar error &lt;1&gt;</li>" + Environment.NewLine
              + "<li>bar error &lt;2&gt;</li>" + Environment.NewLine
              + "</ul></div>",
                html.ToHtmlString());
        }

        [Fact]
        public void ValidationSummaryWithDictionary()
        {
            // Arrange
            HtmlHelper htmlHelper = HtmlHelperFactory.Create(GetModelStateWithErrors());

            // Act
            var html = htmlHelper.ValidationSummary(new Dictionary<string, object> { { "attr", "attr-value" }, { "class", "my-class" } });

            // Assert
            Assert.Equal(
                "<div attr=\"attr-value\" class=\"validation-summary-errors my-class\" data-valmsg-summary=\"true\"><ul>" + Environment.NewLine
              + "<li>foo error &lt;1&gt;</li>" + Environment.NewLine
              + "<li>foo error &lt;2&gt;</li>" + Environment.NewLine
              + "<li>bar error &lt;1&gt;</li>" + Environment.NewLine
              + "<li>bar error &lt;2&gt;</li>" + Environment.NewLine
              + "</ul></div>",
                html.ToHtmlString());
        }

        [Fact]
        public void ValidationSummaryWithDictionaryAndMessage()
        {
            HtmlHelper htmlHelper = HtmlHelperFactory.Create(GetModelStateWithErrors());

            // Act
            var html = htmlHelper.ValidationSummary("This is a message.", new Dictionary<string, object> { { "attr", "attr-value" }, { "class", "my-class" } });

            // Assert
            Assert.Equal(
                "<div attr=\"attr-value\" class=\"validation-summary-errors my-class\" data-valmsg-summary=\"true\"><span>This is a message.</span>" + Environment.NewLine
              + "<ul>" + Environment.NewLine
              + "<li>foo error &lt;1&gt;</li>" + Environment.NewLine
              + "<li>foo error &lt;2&gt;</li>" + Environment.NewLine
              + "<li>bar error &lt;1&gt;</li>" + Environment.NewLine
              + "<li>bar error &lt;2&gt;</li>" + Environment.NewLine
              + "</ul></div>",
                html.ToHtmlString());
        }

        [Fact]
        public void ValidationSummaryWithCustomValidationSummaryClass()
        {
            // Arrange
            HtmlHelper.ValidationSummaryClass = "my-val-class";
            HtmlHelper htmlHelper = HtmlHelperFactory.Create(GetModelStateWithErrors());

            // Act
            var html = htmlHelper.ValidationSummary("This is a message.", new Dictionary<string, object> { { "attr", "attr-value" }, { "class", "my-class" } });

            // Assert
            Assert.Equal(
                "<div attr=\"attr-value\" class=\"my-val-class my-class\" data-valmsg-summary=\"true\"><span>This is a message.</span>" + Environment.NewLine
              + "<ul>" + Environment.NewLine
              + "<li>foo error &lt;1&gt;</li>" + Environment.NewLine
              + "<li>foo error &lt;2&gt;</li>" + Environment.NewLine
              + "<li>bar error &lt;1&gt;</li>" + Environment.NewLine
              + "<li>bar error &lt;2&gt;</li>" + Environment.NewLine
              + "</ul></div>",
                html.ToHtmlString());
        }

        [Fact]
        public void ValidationSummaryWithNoErrorReturnsNullIfExcludeFieldErrorsIsSetToFalse()
        {
            // Arrange
            HtmlHelper htmlHelper = HtmlHelperFactory.Create();

            // Act
            var html = htmlHelper.ValidationSummary(excludeFieldErrors: false);

            // Assert
            Assert.Equal(
                "<div class=\"validation-summary-valid\" data-valmsg-summary=\"true\"><ul>" + Environment.NewLine
              + "</ul></div>",
                html.ToString());
        }

        [Fact]
        public void ValidationSummaryWithNoErrorReturnsNull()
        {
            // Arrange
            HtmlHelper htmlHelper = HtmlHelperFactory.Create();

            // Act
            var html = htmlHelper.ValidationSummary(excludeFieldErrors: true);

            // Assert
            Assert.Null(html);
        }

        [Fact]
        public void ValidationSummaryWithNoFormErrorsAndExcludedFieldErrorsReturnsNull()
        {
            // Arrange
            ModelStateDictionary modelState = new ModelStateDictionary();
            modelState.AddError("foo", "error");
            modelState.AddError("bar", "error");

            HtmlHelper htmlHelper = HtmlHelperFactory.Create(modelState);

            // Act
            var html = htmlHelper.ValidationSummary(excludeFieldErrors: true);

            // Assert
            Assert.Null(html);
        }

        [Fact]
        public void ValidationSummaryWithMultipleFormErrorsAndExcludedFieldErrors()
        {
            // Arrange
            ModelStateDictionary modelState = new ModelStateDictionary();
            modelState.AddFormError("error <1>");
            modelState.AddFormError("error <2>");

            HtmlHelper htmlHelper = HtmlHelperFactory.Create(modelState);

            // Act
            var html = htmlHelper.ValidationSummary(excludeFieldErrors: true);

            // Assert
            Assert.Equal(
                "<div class=\"validation-summary-errors\"><ul>" + Environment.NewLine
              + "<li>error &lt;1&gt;</li>" + Environment.NewLine
              + "<li>error &lt;2&gt;</li>" + Environment.NewLine
              + "</ul></div>",
                html.ToHtmlString());
        }

        [Fact]
        public void ValidationSummaryWithAttributesFromAnonymousObject_WithUnderscoreInName_TransformsUnderscoresToDashs()
        {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.AddFormError("error");

            HtmlHelper htmlHelper = HtmlHelperFactory.Create(modelState);

            HtmlHelperTest.AssertHelperTransformsAttributesUnderscoresToDashs((helper, attributes) =>
                htmlHelper.ValidationSummary(attributes));

            HtmlHelperTest.AssertHelperTransformsAttributesUnderscoresToDashs((helper, attributes) =>
                htmlHelper.ValidationSummary("foo", attributes));

            HtmlHelperTest.AssertHelperTransformsAttributesUnderscoresToDashs((helper, attributes) =>
                htmlHelper.ValidationSummary("foo", true, attributes));
        }

        private static ModelStateDictionary GetModelStateWithErrors()
        {
            ModelStateDictionary modelState = new ModelStateDictionary();
            modelState.AddError("foo", "foo error <1>");
            modelState.AddError("foo", "foo error <2>");
            modelState.AddError("bar", "bar error <1>");
            modelState.AddError("bar", "bar error <2>");
            return modelState;
        }

        private static ModelStateDictionary GetModelStateWithFormErrors()
        {
            ModelStateDictionary modelState = GetModelStateWithErrors();
            modelState.AddFormError("some form error <1>");
            modelState.AddFormError("some form error <2>");
            return modelState;
        }

        public void Dispose()
        {
            // Reset ScopeStorage (written via e.g. HtmlHelper.ValidationSummaryClass) between tests to avoid unexpected interactions.
            ScopeStorage.CurrentProvider = new StaticScopeStorageProvider();
            ScopeStorage.GlobalScope.Clear();
        }
    }
}
