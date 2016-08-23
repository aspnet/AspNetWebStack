// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.WebPages.Html;
using System.Web.WebPages.Scope;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.WebPages.Test
{
    [Xunit.Collection("Uses ScopeStorage or ViewEngines.Engines")]
    public class TextAreaExtensionsTest : IDisposable
    {
        [Fact]
        public void TextAreaWithEmptyNameThrows()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act and assert
            Assert.ThrowsArgument(() => helper.TextArea(null), "name", "Value cannot be null or an empty string.");

            // Act and assert
            Assert.ThrowsArgument(() => helper.TextArea(String.Empty), "name", "Value cannot be null or an empty string.");
        }

        [Fact]
        public void TextAreaWithDefaultRowsAndCols()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act
            var html = helper.TextArea("foo");

            // Assert
            Assert.Equal(@"<textarea cols=""20"" id=""foo"" name=""foo"" rows=""2""></textarea>", html.ToHtmlString());
        }

        [Fact]
        public void TextAreaWithZeroRowsAndColumns()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act
            var html = helper.TextArea("foo", null, 0, 0, null);

            // Assert
            Assert.Equal(@"<textarea id=""foo"" name=""foo""></textarea>", html.ToHtmlString());
        }

        [Fact]
        public void TextAreaWithNonZeroRowsAndColumns()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act
            var html = helper.TextArea("foo", null, 4, 10, null);

            // Assert
            Assert.Equal(@"<textarea cols=""10"" id=""foo"" name=""foo"" rows=""4""></textarea>", html.ToHtmlString());
        }

        [Fact]
        public void TextAreaWithObjectAttributes()
        {
            // Arrange
            ModelStateDictionary modelState = new ModelStateDictionary();
            modelState.SetModelValue("foo", "foo-value");
            HtmlHelper helper = HtmlHelperFactory.Create(modelState);

            // Act
            var html = helper.TextArea("foo", new { attr = "value", cols = 6 });

            // Assert
            Assert.Equal(@"<textarea attr=""value"" cols=""6"" id=""foo"" name=""foo"" rows=""2"">foo-value</textarea>", html.ToHtmlString());
        }

        [Fact]
        public void TextAreaWithExplicitValue()
        {
            // Arrange
            ModelStateDictionary modelState = new ModelStateDictionary();
            modelState.SetModelValue("foo", "explicit-foo-value");
            HtmlHelper helper = HtmlHelperFactory.Create(modelState);

            // Act
            var html = helper.TextArea("foo", "explicit-foo-value", new { attr = "attr-value", cols = 6 });

            // Assert
            Assert.Equal(@"<textarea attr=""attr-value"" cols=""6"" id=""foo"" name=""foo"" rows=""2"">explicit-foo-value</textarea>",
                         html.ToHtmlString());
        }

        [Fact]
        public void TextAreaWithDictionaryAttributes()
        {
            // Arrange
            ModelStateDictionary modelState = new ModelStateDictionary();
            modelState.SetModelValue("foo", "explicit-foo-value");
            HtmlHelper helper = HtmlHelperFactory.Create(modelState);
            var attributes = new Dictionary<string, object>() { { "attr", "attr-val" }, { "rows", 15 }, { "cols", 12 } };
            // Act
            var html = helper.TextArea("foo", attributes);

            // Assert
            Assert.Equal(@"<textarea attr=""attr-val"" cols=""12"" id=""foo"" name=""foo"" rows=""15"">explicit-foo-value</textarea>",
                         html.ToHtmlString());
        }

        [Fact]
        public void TextAreaWithNoValueAndObjectAttributes()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();
            var attributes = new Dictionary<string, object>() { { "attr", "attr-val" }, { "rows", 15 }, { "cols", 12 } };
            // Act
            var html = helper.TextArea("foo", attributes);

            // Assert
            Assert.Equal(@"<textarea attr=""attr-val"" cols=""12"" id=""foo"" name=""foo"" rows=""15""></textarea>",
                         html.ToHtmlString());
        }

        [Fact]
        public void TextAreaWithNullValue()
        {
            // Arrange
            ModelStateDictionary modelState = new ModelStateDictionary();
            modelState.SetModelValue("foo", "explicit-foo-value");
            HtmlHelper helper = HtmlHelperFactory.Create(modelState);
            var attributes = new Dictionary<string, object>() { { "attr", "attr-val" }, { "rows", 15 }, { "cols", 12 } };
            // Act
            var html = helper.TextArea("foo", null, attributes);

            // Assert
            Assert.Equal(@"<textarea attr=""attr-val"" cols=""12"" id=""foo"" name=""foo"" rows=""15"">explicit-foo-value</textarea>",
                         html.ToHtmlString());
        }

        [Fact]
        public void TextAreaWithError()
        {
            // Arrange
            ModelStateDictionary modelState = new ModelStateDictionary();
            modelState.AddError("foo", "some error");
            HtmlHelper helper = HtmlHelperFactory.Create(modelState);

            // Act
            var html = helper.TextArea("foo", String.Empty);

            // Assert
            Assert.Equal(@"<textarea class=""input-validation-error"" cols=""20"" id=""foo"" name=""foo"" rows=""2""></textarea>",
                         html.ToHtmlString());
        }

        [Fact]
        public void TextAreaWithErrorAndCustomCssClass()
        {
            // Arrange
            ModelStateDictionary modelState = new ModelStateDictionary();
            modelState.AddError("foo", "some error");
            HtmlHelper helper = HtmlHelperFactory.Create(modelState);

            // Act
            var html = helper.TextArea("foo", String.Empty, new { @class = "my-css" });

            // Assert
            Assert.Equal(@"<textarea class=""input-validation-error my-css"" cols=""20"" id=""foo"" name=""foo"" rows=""2""></textarea>",
                         html.ToHtmlString());
        }

        [Fact]
        public void TextAreaWithCustomErrorClass()
        {
            // Arrange
            ModelStateDictionary modelState = new ModelStateDictionary();
            modelState.AddError("foo", "some error");
            HtmlHelper.ValidationInputCssClassName = "custom-input-validation-error";
            HtmlHelper helper = HtmlHelperFactory.Create(modelState);

            // Act
            var html = helper.TextArea("foo", String.Empty, new { @class = "my-css" });

            // Assert
            Assert.Equal(@"<textarea class=""custom-input-validation-error my-css"" cols=""20"" id=""foo"" name=""foo"" rows=""2""></textarea>",
                         html.ToHtmlString());
        }

        [Fact]
        public void TextAreaAddsUnobtrusiveValidationAttributes()
        {
            // Arrange
            const string fieldName = "name";
            var modelStateDictionary = new ModelStateDictionary();
            var validationHelper = new ValidationHelper(new Mock<HttpContextBase>().Object, modelStateDictionary);
            HtmlHelper helper = HtmlHelperFactory.Create(modelStateDictionary, validationHelper);

            // Act
            validationHelper.RequireField(fieldName, "Please specify a valid Name.");
            validationHelper.Add(fieldName, Validator.StringLength(30, errorMessage: "Name cannot exceed {0} characters"));
            var html = helper.TextArea(fieldName, htmlAttributes: new Dictionary<string, object> { { "data-some-val", "5" } });

            // Assert
            Assert.Equal(@"<textarea cols=""20"" data-some-val=""5"" data-val=""true"" data-val-length=""Name cannot exceed 30 characters"" data-val-length-max=""30"" data-val-required=""Please specify a valid Name."" id=""name"" name=""name"" rows=""2""></textarea>",
                         html.ToString());
        }

        [Fact]
        public void TextAreaWithAttributesFromAnonymousObject_WithUnderscoreInName_TransformsUnderscoresToDashs()
        {
            HtmlHelperTest.AssertHelperTransformsAttributesUnderscoresToDashs((helper, attributes) =>
                helper.TextArea("foo", attributes));

            HtmlHelperTest.AssertHelperTransformsAttributesUnderscoresToDashs((helper, attributes) =>
                helper.TextArea("foo", "value", attributes));

            HtmlHelperTest.AssertHelperTransformsAttributesUnderscoresToDashs((helper, attributes) =>
                helper.TextArea("foo", "value", 1, 1, attributes));
        }

        public void Dispose()
        {
            // Reset ScopeStorage (written via e.g. HtmlHelper.ValidationInputCssClassName) between tests to avoid unexpected interactions.
            ScopeStorage.CurrentProvider = new StaticScopeStorageProvider();
            ScopeStorage.GlobalScope.Clear();
        }
    }
}
