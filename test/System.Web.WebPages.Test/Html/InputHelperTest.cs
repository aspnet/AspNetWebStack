// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Linq;
using System.Web.WebPages.Html;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.WebPages.Test
{
    public class InputHelperTest
    {
        private static readonly IDictionary<string, object> _attributesDictionary = new Dictionary<string, object> { { "baz", "BazValue" } };
        private static readonly object _attributesObject = new { baz = "BazValue" };

        [Fact]
        public void HiddenWithBinaryArrayValueRendersBase64EncodedValue()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act
            var result = helper.Hidden("ProductName", new Binary(new byte[] { 23, 43, 53 }));

            // Assert
            Assert.Equal("<input id=\"ProductName\" name=\"ProductName\" type=\"hidden\" value=\"Fys1\" />", result.ToHtmlString());
        }

        [Fact]
        public void HiddenWithEmptyNameThrows()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act & Assert
            Assert.ThrowsArgumentNullOrEmptyString(() => helper.Hidden(String.Empty), "name");
            Assert.ThrowsArgumentNullOrEmptyString(() => helper.Hidden(null), "name");
        }

        [Fact]
        public void HiddenWithExplicitValue()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act
            var html = helper.Hidden("foo", "DefaultFoo");

            // Assert
            Assert.Equal(@"<input id=""foo"" name=""foo"" type=""hidden"" value=""DefaultFoo"" />",
                         html.ToHtmlString());
        }

        [Fact]
        public void HiddenWithExplicitValueAndAttributesDictionary()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act
            var html = helper.Hidden("foo", "DefaultFoo", new Dictionary<string, object> { { "attr", "attr-val" } });

            // Assert
            Assert.Equal(@"<input attr=""attr-val"" id=""foo"" name=""foo"" type=""hidden"" value=""DefaultFoo"" />",
                         html.ToHtmlString());
        }

        [Fact]
        public void HiddenWithExplicitValueAndObjectDictionary()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act
            var html = helper.Hidden("foo", "DefaultFoo", new { attr = "attr-val" });

            // Assert
            Assert.Equal(@"<input attr=""attr-val"" id=""foo"" name=""foo"" type=""hidden"" value=""DefaultFoo"" />",
                         html.ToHtmlString());
        }

        [Fact]
        public void HiddenWithExplicitValueNull()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act
            var html = helper.Hidden("foo", value: null);

            // Assert
            Assert.Equal(@"<input id=""foo"" name=""foo"" type=""hidden"" value="""" />", html.ToHtmlString());
        }

        [Fact]
        public void HiddenWithModelValue()
        {
            // Arrange
            var model = new ModelStateDictionary();
            model.SetModelValue("foo", "bar");
            HtmlHelper helper = HtmlHelperFactory.Create(model);

            // Act
            var html = helper.Hidden("foo");

            // Assert
            Assert.Equal(@"<input id=""foo"" name=""foo"" type=""hidden"" value=""bar"" />", html.ToHtmlString());
        }

        [Fact]
        public void HiddenWithModelValueAndAttributesDictionary()
        {
            // Arrange
            var model = new ModelStateDictionary();
            model.SetModelValue("foo", "bar");
            HtmlHelper helper = HtmlHelperFactory.Create(model);

            // Act
            var html = helper.Hidden("foo", null, new Dictionary<string, object> { { "attr", "attr-val" } });

            // Assert
            Assert.Equal(@"<input attr=""attr-val"" id=""foo"" name=""foo"" type=""hidden"" value=""bar"" />", html.ToHtmlString());
        }

        [Fact]
        public void HiddenWithImplicitValueAndAttributesObject()
        {
            // Arrange
            var model = new ModelStateDictionary();
            model.SetModelValue("foo", "bar");
            HtmlHelper helper = HtmlHelperFactory.Create(model);

            // Act
            var html = helper.Hidden("foo", null, new { attr = "attr-val" });

            // Assert
            Assert.Equal(@"<input attr=""attr-val"" id=""foo"" name=""foo"" type=""hidden"" value=""bar"" />", html.ToHtmlString());
        }

        [Fact]
        public void HiddenWithNameAndValue()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act
            var html = helper.Hidden("foo", "fooValue");

            // Assert
            Assert.Equal(@"<input id=""foo"" name=""foo"" type=""hidden"" value=""fooValue"" />", html.ToHtmlString());
        }

        [Fact]
        public void HiddenWithExplicitOverwritesAttributeValue()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act
            var html = helper.Hidden("foo", "fooValue", new { value = "barValue" });

            // Assert
            Assert.Equal(@"<input id=""foo"" name=""foo"" type=""hidden"" value=""fooValue"" />", html.ToHtmlString());
        }

        [Fact]
        public void HiddenWithModelValueOverwritesAttributeValue()
        {
            // Arrange
            var model = new ModelStateDictionary();
            model.SetModelValue("foo", "fooValue");
            HtmlHelper helper = HtmlHelperFactory.Create(model);

            // Act
            var html = helper.Hidden("foo", null, new { value = "barValue" });

            // Assert
            Assert.Equal(@"<input id=""foo"" name=""foo"" type=""hidden"" value=""fooValue"" />", html.ToHtmlString());
        }

        [Fact]
        public void HiddenAddsUnobtrusiveValidationAttributes()
        {
            // Arrange
            const string fieldName = "name";
            var modelStateDictionary = new ModelStateDictionary();
            var validationHelper = new ValidationHelper(new Mock<HttpContextBase>().Object, modelStateDictionary);
            HtmlHelper helper = HtmlHelperFactory.Create(modelStateDictionary, validationHelper);

            // Act
            validationHelper.RequireField(fieldName, "Please specify a valid Name.");
            validationHelper.Add(fieldName, Validator.StringLength(30, errorMessage: "Name cannot exceed {0} characters"));
            var html = helper.Hidden(fieldName, value: null, htmlAttributes: new Dictionary<string, object> { { "data-some-val", "5" } });

            // Assert
            Assert.Equal(@"<input data-some-val=""5"" data-val=""true"" data-val-length=""Name cannot exceed 30 characters"" data-val-length-max=""30"" data-val-required=""Please specify a valid Name."" id=""name"" name=""name"" type=""hidden"" value="""" />",
                         html.ToString());
        }

        // Password

        [Fact]
        public void PasswordWithEmptyNameThrows()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act & Assert
            Assert.ThrowsArgumentNullOrEmptyString(() => helper.Password(String.Empty), "name");
            Assert.ThrowsArgumentNullOrEmptyString(() => helper.Password(null), "name");
        }

        [Fact]
        public void PasswordDictionaryOverridesImplicitParameters()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act
            var html = helper.Password("foo", "Some Value", new { type = "fooType" });

            // Assert
            Assert.Equal(@"<input id=""foo"" name=""foo"" type=""fooType"" value=""Some Value"" />", html.ToHtmlString());
        }

        [Fact]
        public void PasswordExplicitParametersOverrideDictionary()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act
            var html = helper.Password("foo", "Some Value", new { value = "Another Value", name = "bar" });

            // Assert
            Assert.Equal(@"<input id=""foo"" name=""foo"" type=""password"" value=""Some Value"" />", html.ToHtmlString());
        }

        [Fact]
        public void PasswordWithExplicitValue()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act
            var html = helper.Password("foo", "DefaultFoo", (object)null);

            // Assert
            Assert.Equal(@"<input id=""foo"" name=""foo"" type=""password"" value=""DefaultFoo"" />", html.ToHtmlString());
        }

        [Fact]
        public void PasswordWithExplicitValueAndAttributesDictionary()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act
            var html = helper.Password("foo", "DefaultFoo", new { baz = "BazValue" });

            // Assert
            Assert.Equal(@"<input baz=""BazValue"" id=""foo"" name=""foo"" type=""password"" value=""DefaultFoo"" />",
                         html.ToHtmlString());
        }

        [Fact]
        public void PasswordWithExplicitValueAndAttributesObject()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act
            var html = helper.Password("foo", "DefaultFoo", new Dictionary<string, object> { { "baz", "BazValue" } });

            // Assert
            Assert.Equal(@"<input baz=""BazValue"" id=""foo"" name=""foo"" type=""password"" value=""DefaultFoo"" />",
                         html.ToHtmlString());
        }

        [Fact]
        public void PasswordWithExplicitValueNull()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act
            var html = helper.Password("foo", value: (string)null);

            // Assert
            Assert.Equal(@"<input id=""foo"" name=""foo"" type=""password"" />", html.ToHtmlString());
        }

        [Fact]
        public void PasswordWithImplicitValue()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act
            var html = helper.Password("foo");

            // Assert
            Assert.Equal(@"<input id=""foo"" name=""foo"" type=""password"" />", html.ToHtmlString());
        }

        [Fact]
        public void PasswordWithImplicitValueAndAttributesDictionary()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act
            var html = helper.Password("foo", null, _attributesDictionary);

            // Assert
            Assert.Equal(@"<input baz=""BazValue"" id=""foo"" name=""foo"" type=""password"" />", html.ToHtmlString());
        }

        [Fact]
        public void PasswordWithImplicitValueAndAttributesDictionaryReturnsEmptyValueIfNotFound()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act
            var html = helper.Password("keyNotFound", null, _attributesDictionary);

            // Assert
            Assert.Equal(@"<input baz=""BazValue"" id=""keyNotFound"" name=""keyNotFound"" type=""password"" />", html.ToHtmlString());
        }

        [Fact]
        public void PasswordWithImplicitValueAndAttributesObject()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act
            var html = helper.Password("foo", null, _attributesObject);

            // Assert
            Assert.Equal(@"<input baz=""BazValue"" id=""foo"" name=""foo"" type=""password"" />", html.ToHtmlString());
        }

        [Fact]
        public void PasswordWithNameAndValue()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act
            var html = helper.Password("foo", "fooValue");

            // Assert
            Assert.Equal(@"<input id=""foo"" name=""foo"" type=""password"" value=""fooValue"" />", html.ToHtmlString());
        }

        [Fact]
        public void PasswordWithNullNameThrows()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act & Assert
            Assert.ThrowsArgumentNullOrEmptyString(() => helper.Password(null), "name");
            Assert.ThrowsArgumentNullOrEmptyString(() => helper.Password(String.Empty), "name");
        }

        [Fact]
        public void PasswordAddsUnobtrusiveValidationAttributes()
        {
            // Arrange
            const string fieldName = "name";
            var modelStateDictionary = new ModelStateDictionary();
            var validationHelper = new ValidationHelper(new Mock<HttpContextBase>().Object, modelStateDictionary);
            HtmlHelper helper = HtmlHelperFactory.Create(modelStateDictionary, validationHelper);

            // Act
            validationHelper.RequireField(fieldName, "Please specify a valid Name.");
            validationHelper.Add(fieldName, Validator.StringLength(30, errorMessage: "Name cannot exceed {0} characters"));
            var html = helper.Password(fieldName, value: null, htmlAttributes: new Dictionary<string, object> { { "data-some-val", "5" } });

            // Assert
            Assert.Equal(@"<input data-some-val=""5"" data-val=""true"" data-val-length=""Name cannot exceed 30 characters"" data-val-length-max=""30"" data-val-required=""Please specify a valid Name."" id=""name"" name=""name"" type=""password"" />",
                         html.ToString());
        }

        //Input 
        [Fact]
        public void TextBoxDictionaryOverridesImplicitValues()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act
            var html = helper.TextBox("foo", "DefaultFoo", new { type = "fooType" });

            // Assert
            Assert.Equal(@"<input id=""foo"" name=""foo"" type=""fooType"" value=""DefaultFoo"" />", html.ToHtmlString());
        }

        [Fact]
        public void TextBoxExplicitParametersOverrideDictionaryValues()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act
            var html = helper.TextBox("foo", "DefaultFoo", new { value = "Some other value" });

            // Assert
            Assert.Equal(@"<input id=""foo"" name=""foo"" type=""text"" value=""DefaultFoo"" />", html.ToHtmlString());
        }

        [Fact]
        public void TextBoxWithDotReplacementForId()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act
            var html = helper.TextBox("foo.bar.baz", null);

            // Assert
            Assert.Equal(@"<input id=""foo_bar_baz"" name=""foo.bar.baz"" type=""text"" value="""" />", html.ToHtmlString());
        }

        [Fact]
        public void TextBoxWithEmptyNameThrows()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act & Assert
            Assert.ThrowsArgumentNullOrEmptyString(() => helper.TextBox(null), "name");
            Assert.ThrowsArgumentNullOrEmptyString(() => helper.TextBox(String.Empty), "name");
        }

        [Fact]
        public void TextBoxWithExplicitValue()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act
            var html = helper.TextBox("foo", "DefaultFoo", (object)null);

            // Assert
            Assert.Equal(@"<input id=""foo"" name=""foo"" type=""text"" value=""DefaultFoo"" />", html.ToHtmlString());
        }

        [Fact]
        public void TextBoxWithExplicitValueAndAttributesDictionary()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act
            var html = helper.TextBox("foo", "DefaultFoo", _attributesDictionary);

            // Assert
            Assert.Equal(@"<input baz=""BazValue"" id=""foo"" name=""foo"" type=""text"" value=""DefaultFoo"" />", html.ToHtmlString());
        }

        [Fact]
        public void TextBoxWithExplicitValueAndAttributesObject()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act
            var html = helper.TextBox("foo", "DefaultFoo", _attributesObject);

            // Assert
            Assert.Equal(@"<input baz=""BazValue"" id=""foo"" name=""foo"" type=""text"" value=""DefaultFoo"" />", html.ToHtmlString());
        }

        [Fact]
        public void TextBoxWithExplicitValueNull()
        {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.SetModelValue("foo", "fooModelValue");
            HtmlHelper helper = HtmlHelperFactory.Create(modelState);

            // Act
            var html = helper.TextBox("foo", (string)null /* value */, (object)null);

            // Assert
            Assert.Equal(@"<input id=""foo"" name=""foo"" type=""text"" value=""fooModelValue"" />", html.ToHtmlString());
        }

        [Fact]
        public void TextBoxWithImplicitValue()
        {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.SetModelValue("foo", "fooModelValue");
            HtmlHelper helper = HtmlHelperFactory.Create(modelState);

            // Act
            var html = helper.TextBox("foo");

            // Assert
            Assert.Equal(@"<input id=""foo"" name=""foo"" type=""text"" value=""fooModelValue"" />", html.ToHtmlString());
        }

        [Fact]
        public void TextBoxWithImplicitValueAndAttributesDictionary()
        {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.SetModelValue("foo", "fooModelValue");
            HtmlHelper helper = HtmlHelperFactory.Create(modelState);

            // Act
            var html = helper.TextBox("foo", null, _attributesDictionary);

            // Assert
            Assert.Equal(@"<input baz=""BazValue"" id=""foo"" name=""foo"" type=""text"" value=""fooModelValue"" />", html.ToHtmlString());
        }

        [Fact]
        public void TextBoxWithImplicitValueAndAttributesDictionaryReturnsEmptyValueIfNotFound()
        {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.SetModelValue("foo", "fooModelValue");
            HtmlHelper helper = HtmlHelperFactory.Create(modelState);

            // Act
            var html = helper.TextBox("keyNotFound", null, _attributesDictionary);

            // Assert
            Assert.Equal(@"<input baz=""BazValue"" id=""keyNotFound"" name=""keyNotFound"" type=""text"" value="""" />", html.ToHtmlString());
        }

        [Fact]
        public void TextBoxWithImplicitValueAndAttributesObject()
        {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.SetModelValue("foo", "fooModelValue");
            HtmlHelper helper = HtmlHelperFactory.Create(modelState);

            // Act
            var html = helper.TextBox("foo", null, _attributesObject);

            // Assert
            Assert.Equal(@"<input baz=""BazValue"" id=""foo"" name=""foo"" type=""text"" value=""fooModelValue"" />", html.ToHtmlString());
        }

        [Fact]
        public void TextBoxWithNameAndValue()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act
            var html = helper.TextBox("foo", "fooValue");

            // Assert
            Assert.Equal(@"<input id=""foo"" name=""foo"" type=""text"" value=""fooValue"" />", html.ToHtmlString());
        }

        [Fact]
        public void TextBoxAddsUnobtrusiveValidationAttributes()
        {
            // Arrange
            const string fieldName = "name";
            var modelStateDictionary = new ModelStateDictionary();
            var validationHelper = new ValidationHelper(new Mock<HttpContextBase>().Object, modelStateDictionary);
            HtmlHelper helper = HtmlHelperFactory.Create(modelStateDictionary, validationHelper);

            // Act
            validationHelper.RequireField(fieldName, "Please specify a valid Name.");
            validationHelper.Add(fieldName, Validator.StringLength(30, errorMessage: "Name cannot exceed {0} characters"));
            var html = helper.TextBox(fieldName, value: null, htmlAttributes: new Dictionary<string, object> { { "data-some-val", "5" } });

            // Assert
            Assert.Equal(@"<input data-some-val=""5"" data-val=""true"" data-val-length=""Name cannot exceed 30 characters"" data-val-length-max=""30"" data-val-required=""Please specify a valid Name."" id=""name"" name=""name"" type=""text"" value="""" />",
                         html.ToString());
        }

        [Fact]
        public void TextBoxWithAttributesFromAnonymousObject_WithUnderscoreInName_TransformsUnderscoresToDashs()
        {
            HtmlHelperTest.AssertHelperTransformsAttributesUnderscoresToDashs((helper, attributes) =>
                helper.TextBox("foo", "value", attributes));
        }

        [Fact]
        public void HiddenTextBoxWithAttributesFromAnonymousObject_WithUnderscoreInName_TransformsUnderscoresToDashs()
        {
            HtmlHelperTest.AssertHelperTransformsAttributesUnderscoresToDashs((helper, attributes) =>
                helper.Hidden("foo", "value", attributes));
        }

        [Fact]
        public void PasswordTextBoxWithAttributesFromAnonymousObject_WithUnderscoreInName_TransformsUnderscoresToDashs()
        {
            HtmlHelperTest.AssertHelperTransformsAttributesUnderscoresToDashs((helper, attributes) =>
                helper.Password("foo", "value", attributes));
        }
    }
}
