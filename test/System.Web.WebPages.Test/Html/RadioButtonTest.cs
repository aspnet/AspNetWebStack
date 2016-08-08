// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.WebPages.Html;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.WebPages.Test
{
    public class RadioButtonTest
    {
        [Fact]
        public void RadioButtonWithEmptyNameThrows()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act and assert
            Assert.ThrowsArgumentNullOrEmptyString(() => helper.RadioButton(null, null), "name");
            Assert.ThrowsArgumentNullOrEmptyString(() => helper.RadioButton(String.Empty, null), "name");
        }

        [Fact]
        public void RadioButtonWithDefaultArguments()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act
            var html = helper.RadioButton("foo", "bar", true);

            // Assert
            Assert.Equal(@"<input checked=""checked"" id=""foo"" name=""foo"" type=""radio"" value=""bar"" />",
                         html.ToHtmlString());

            html = helper.RadioButton("foo", "bar", false);

            // Assert
            Assert.Equal(@"<input id=""foo"" name=""foo"" type=""radio"" value=""bar"" />",
                         html.ToHtmlString());
        }

        [Fact]
        public void RadioButtonWithObjectAttributes()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act
            var html = helper.RadioButton("foo", "bar", new { attr = "attr-value" });

            // Assert
            Assert.Equal(@"<input attr=""attr-value"" id=""foo"" name=""foo"" type=""radio"" value=""bar"" />",
                         html.ToHtmlString());
        }

        [Fact]
        public void RadioButtonWithDictionaryAttributes()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act
            var html = helper.RadioButton("foo", "bar", new Dictionary<string, object> { { "attr", "attr-value" } });

            // Assert
            Assert.Equal(@"<input attr=""attr-value"" id=""foo"" name=""foo"" type=""radio"" value=""bar"" />",
                         html.ToHtmlString());
        }

        [Fact]
        public void RadioButtonUsesModelStateToAssignChecked()
        {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.SetModelValue("foo", "bar");
            HtmlHelper helper = HtmlHelperFactory.Create(modelState);

            // Act
            var html = helper.RadioButton("foo", "bar");

            // Assert
            Assert.Equal(@"<input checked=""checked"" id=""foo"" name=""foo"" type=""radio"" value=""bar"" />",
                         html.ToHtmlString());
        }

        [Fact]
        public void RadioButtonUsesModelStateToRemoveChecked()
        {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.SetModelValue("foo", "not-a-bar");
            HtmlHelper helper = HtmlHelperFactory.Create(modelState);

            // Act
            var html = helper.RadioButton("foo", "bar", new { @checked = "checked" });

            // Assert
            Assert.Equal(@"<input id=""foo"" name=""foo"" type=""radio"" value=""bar"" />",
                         html.ToHtmlString());
        }

        [Fact]
        public void RadioButtonWithoutModelStateDoesNotAffectChecked()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act
            var html = helper.RadioButton("foo", "bar", new { @checked = "checked" });

            // Assert
            Assert.Equal(@"<input checked=""checked"" id=""foo"" name=""foo"" type=""radio"" value=""bar"" />",
                         html.ToHtmlString());
        }

        [Fact]
        public void RadioButtonWithNonStringModelValue()
        {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.SetModelValue("foo", new List<double>());
            HtmlHelper helper = HtmlHelperFactory.Create(modelState);

            // Act
            var html = helper.RadioButton("foo", "bar");

            // Assert
            Assert.Equal(@"<input id=""foo"" name=""foo"" type=""radio"" value=""bar"" />",
                         html.ToHtmlString());
        }

        [Fact]
        public void RadioButtonWithNonStringValue()
        {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.SetModelValue("foo", "bar");
            HtmlHelper helper = HtmlHelperFactory.Create(modelState);

            // Act
            var html = helper.RadioButton("foo", 2.53);

            // Assert
            Assert.Equal(@"<input id=""foo"" name=""foo"" type=""radio"" value=""2.53"" />",
                         html.ToHtmlString());
        }

        [Fact]
        public void RadioButtonWithExplicitChecked()
        {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.SetModelValue("foo", "bar");
            HtmlHelper helper = HtmlHelperFactory.Create(modelState);

            // Act
            var html = helper.RadioButton("foo", "not-bar", true);

            // Assert
            Assert.Equal(@"<input checked=""checked"" id=""foo"" name=""foo"" type=""radio"" value=""not-bar"" />",
                         html.ToHtmlString());
        }

        [Fact]
        public void RadioButtonOverwritesImplicitAttributes()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act
            var html = helper.RadioButton("foo", "foo-value", new { value = "bazValue", type = "fooType", name = "bar" });

            // Assert
            Assert.Equal(@"<input id=""foo"" name=""foo"" type=""fooType"" value=""foo-value"" />",
                         html.ToHtmlString());
        }

        [Fact]
        public void RadioButtonAddsUnobtrusiveValidationAttributes()
        {
            // Arrange
            const string fieldName = "name";
            var modelStateDictionary = new ModelStateDictionary();
            var validationHelper = new ValidationHelper(new Mock<HttpContextBase>().Object, modelStateDictionary);
            HtmlHelper helper = HtmlHelperFactory.Create(modelStateDictionary, validationHelper);

            // Act
            validationHelper.RequireField(fieldName, "Please specify a valid Name.");
            validationHelper.Add(fieldName, Validator.StringLength(30, errorMessage: "Name cannot exceed {0} characters"));
            var html = helper.RadioButton(fieldName, value: 8, htmlAttributes: new Dictionary<string, object> { { "data-some-val", "5" } });

            // Assert
            Assert.Equal(@"<input data-some-val=""5"" data-val=""true"" data-val-length=""Name cannot exceed 30 characters"" data-val-length-max=""30"" data-val-required=""Please specify a valid Name."" id=""name"" name=""name"" type=""radio"" value=""8"" />",
                         html.ToString());
        }

        [Fact]
        public void RadioButtonWithAttributesFromAnonymousObject_WithUnderscoreInName_TransformsUnderscoresToDashs()
        {
            HtmlHelperTest.AssertHelperTransformsAttributesUnderscoresToDashs((helper, attributes) =>
                helper.RadioButton("foo", true, attributes));

            HtmlHelperTest.AssertHelperTransformsAttributesUnderscoresToDashs((helper, attributes) =>
                helper.RadioButton("foo", true, true, attributes));
        }
    }
}
