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
    public class CheckBoxTest : IDisposable
    {
        [Fact]
        public void CheckboxWithEmptyNameThrows()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act and assert
            Assert.ThrowsArgumentNullOrEmptyString(() => helper.CheckBox(null), "name");
            Assert.ThrowsArgumentNullOrEmptyString(() => helper.CheckBox(String.Empty), "name");
        }

        [Fact]
        public void CheckboxWithDefaultArguments()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act
            var html = helper.CheckBox("foo");

            // Assert
            Assert.Equal(@"<input id=""foo"" name=""foo"" type=""checkbox"" />",
                         html.ToHtmlString());
        }

        [Fact]
        public void CheckboxWithObjectAttributes()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act
            var html = helper.CheckBox("foo", new { attr = "attr-value" });

            // Assert
            Assert.Equal(@"<input attr=""attr-value"" id=""foo"" name=""foo"" type=""checkbox"" />",
                         html.ToHtmlString());
        }

        [Fact]
        public void CheckboxWithDictionaryAttributes()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act
            var html = helper.CheckBox("foo", new Dictionary<string, object> { { "attr", "attr-value" } });

            // Assert
            Assert.Equal(@"<input attr=""attr-value"" id=""foo"" name=""foo"" type=""checkbox"" />",
                         html.ToHtmlString());
        }

        [Fact]
        public void CheckboxWithExplicitChecked()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act
            var html = helper.CheckBox("foo", true);

            // Assert
            Assert.Equal(@"<input checked=""checked"" id=""foo"" name=""foo"" type=""checkbox"" />",
                         html.ToHtmlString());
        }

        [Fact]
        public void CheckboxWithModelValue()
        {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.SetModelValue("foo", true);
            HtmlHelper helper = HtmlHelperFactory.Create(modelState);

            // Act
            var html = helper.CheckBox("foo");

            // Assert
            Assert.Equal(@"<input checked=""checked"" id=""foo"" name=""foo"" type=""checkbox"" />",
                         html.ToHtmlString());
        }

        [Fact]
        public void CheckboxWithNonBooleanModelValue()
        {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.SetModelValue("foo", Boolean.TrueString);
            HtmlHelper helper = HtmlHelperFactory.Create(modelState);

            // Act
            var html = helper.CheckBox("foo");

            // Assert
            Assert.Equal(@"<input checked=""checked"" id=""foo"" name=""foo"" type=""checkbox"" />",
                         html.ToHtmlString());

            modelState.SetModelValue("foo", new object());
            helper = HtmlHelperFactory.Create(modelState);

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => helper.CheckBox("foo"),
                                                              "The parameter conversion from type \"System.Object\" to type \"System.Boolean\" failed because no " +
                                                              "type converter can convert between these types.");
        }

        [Fact]
        public void CheckboxWithModelAndExplictValue()
        {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.SetModelValue("foo", false);
            HtmlHelper helper = HtmlHelperFactory.Create(modelState);

            // Act
            var html = helper.CheckBox("foo", true);

            // Assert
            Assert.Equal(@"<input checked=""checked"" id=""foo"" name=""foo"" type=""checkbox"" />",
                         html.ToHtmlString());

            modelState.SetModelValue("foo", true);

            // Act
            html = helper.CheckBox("foo", false);

            // Assert
            Assert.Equal(@"<input id=""foo"" name=""foo"" type=""checkbox"" />",
                         html.ToHtmlString());
        }

        [Fact]
        public void CheckBoxWithCheckedHtmlAttribute()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act
            var html = helper.CheckBox("foo", new { @checked = "checked" });

            // Assert
            Assert.Equal(@"<input checked=""checked"" id=""foo"" name=""foo"" type=""checkbox"" />",
                         html.ToHtmlString());
        }

        [Fact]
        public void CheckBoxWithExplicitCheckedOverwritesHtmlAttribute()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act
            var html = helper.CheckBox("foo", false, new { @checked = "checked" });

            // Assert
            Assert.Equal(@"<input id=""foo"" name=""foo"" type=""checkbox"" />",
                         html.ToHtmlString());
        }

        [Fact]
        public void CheckBoxWithModelStateCheckedOverwritesHtmlAttribute()
        {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.SetModelValue("foo", false);
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act
            var html = helper.CheckBox("foo", false, new { @checked = "checked" });

            // Assert
            Assert.Equal(@"<input id=""foo"" name=""foo"" type=""checkbox"" />",
                         html.ToHtmlString());
        }

        [Fact]
        public void CheckBoxWithError()
        {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.SetModelValue("foo", false);
            HtmlHelper helper = HtmlHelperFactory.Create(modelState);

            // Act
            var html = helper.CheckBox("foo", true);

            // Assert
            Assert.Equal(@"<input checked=""checked"" id=""foo"" name=""foo"" type=""checkbox"" />",
                         html.ToHtmlString());
        }

        [Fact]
        public void CheckBoxWithErrorAndCustomCss()
        {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.AddError("foo", "error");
            HtmlHelper helper = HtmlHelperFactory.Create(modelState);

            // Act
            var html = helper.CheckBox("foo", true, new { @class = "my-class" });

            // Assert
            Assert.Equal(@"<input checked=""checked"" class=""input-validation-error my-class"" id=""foo"" name=""foo"" type=""checkbox"" />",
                         html.ToHtmlString());
        }

        [Fact]
        public void CheckBoxUsesCustomErrorClass()
        {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.AddError("foo", "error");
            HtmlHelper.ValidationInputCssClassName = "my-error-class";
            HtmlHelper helper = HtmlHelperFactory.Create(modelState);

            // Act
            var html = helper.CheckBox("foo", true, new { @class = "my-class" });

            // Assert
            Assert.Equal(@"<input checked=""checked"" class=""my-error-class my-class"" id=""foo"" name=""foo"" type=""checkbox"" />",
                         html.ToHtmlString());
        }

        [Fact]
        public void CheckBoxOverwritesImplicitAttributes()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act
            var html = helper.CheckBox("foo", true, new { type = "fooType", name = "bar" });

            // Assert
            Assert.Equal(@"<input checked=""checked"" id=""foo"" name=""foo"" type=""fooType"" />",
                         html.ToHtmlString());
        }

        [Fact]
        public void CheckboxAddsUnobtrusiveValidationAttributes()
        {
            // Arrange
            const string fieldName = "name";
            var modelStateDictionary = new ModelStateDictionary();
            var validationHelper = new ValidationHelper(new Mock<HttpContextBase>().Object, modelStateDictionary);
            HtmlHelper helper = HtmlHelperFactory.Create(modelStateDictionary, validationHelper);

            // Act
            validationHelper.RequireField(fieldName, "Please specify a valid Name.");
            validationHelper.Add(fieldName, Validator.StringLength(30, errorMessage: "Name cannot exceed {0} characters"));
            var html = helper.CheckBox(fieldName, new Dictionary<string, object> { { "data-some-val", "5" } });

            // Assert
            Assert.Equal(@"<input data-some-val=""5"" data-val=""true"" data-val-length=""Name cannot exceed 30 characters"" data-val-length-max=""30"" data-val-required=""Please specify a valid Name."" id=""name"" name=""name"" type=""checkbox"" />",
                         html.ToString());
        }

        [Fact]
        public void CheckboxWithAttributesFromAnonymousObject_WithUnderscoreInName_TransformsUnderscoresToDashs()
        {
            HtmlHelperTest.AssertHelperTransformsAttributesUnderscoresToDashs((helper, attributes) =>
                helper.CheckBox("foo", attributes));

            HtmlHelperTest.AssertHelperTransformsAttributesUnderscoresToDashs((helper, attributes) =>
                helper.CheckBox("foo", true, attributes));
        }

        public void Dispose()
        {
            // Reset ScopeStorage (written via e.g. HtmlHelper.ValidationInputCssClassName) between tests to avoid unexpected interactions.
            ScopeStorage.CurrentProvider = new StaticScopeStorageProvider();
            ScopeStorage.GlobalScope.Clear();
        }
    }
}
