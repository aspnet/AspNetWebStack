// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.Data.Linq;
using System.Linq.Expressions;
using System.Web.Mvc.Test;
using System.Web.Routing;
using System.Web.WebPages.Scope;
using Microsoft.TestCommon;
using Microsoft.Web.UnitTestUtil;

namespace System.Web.Mvc.Html.Test
{
    [Xunit.Collection("Uses ScopeStorage or ViewEngines.Engines")]
    public class InputExtensionsTest : IDisposable
    {
        // CheckBox

        [Fact]
        public void CheckBoxDictionaryOverridesImplicitParameters()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetCheckBoxViewData());

            // Act
            MvcHtmlString html = helper.CheckBox("baz", new { @checked = "checked", value = "false" });

            // Assert
            Assert.Equal(@"<input checked=""checked"" id=""baz"" name=""baz"" type=""checkbox"" value=""false"" />" +
                         @"<input name=""baz"" type=""hidden"" value=""false"" />",
                         html.ToHtmlString());
        }

        [Fact]
        public void CheckBoxExplicitParametersOverrideDictionary()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper();

            // Act
            MvcHtmlString html = helper.CheckBox("foo", true /* isChecked */, new { @checked = "unchecked", value = "false" });

            // Assert
            Assert.Equal(@"<input checked=""checked"" id=""foo"" name=""foo"" type=""checkbox"" value=""false"" />" +
                         @"<input name=""foo"" type=""hidden"" value=""false"" />",
                         html.ToHtmlString());
        }

        [Fact]
        public void CheckBoxShouldNotCopyAttributesForHidden()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper();

            // Act
            MvcHtmlString html = helper.CheckBox("foo", true /* isChecked */, new { id = "myID" });

            // Assert
            Assert.Equal(@"<input checked=""checked"" id=""myID"" name=""foo"" type=""checkbox"" value=""true"" />" +
                         @"<input name=""foo"" type=""hidden"" value=""false"" />",
                         html.ToHtmlString());
        }

        [Fact]
        public void CheckBoxWithEmptyNameThrows()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetCheckBoxViewData());

            // Act & Assert
            Assert.ThrowsArgumentNullOrEmpty(
                delegate { helper.CheckBox(String.Empty); },
                "name");
        }

        [Fact]
        public void CheckBoxWithInvalidBooleanThrows()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetCheckBoxViewData());

            // Act & Assert
            Assert.Throws<FormatException>(
                delegate { helper.CheckBox("bar"); },
                "String was not recognized as a valid Boolean.");
        }

        [Fact]
        public void CheckBoxCheckedWithOnlyName()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper();

            // Act
            MvcHtmlString html = helper.CheckBox("foo", true /* isChecked */);

            // Assert
            Assert.Equal(@"<input checked=""checked"" id=""foo"" name=""foo"" type=""checkbox"" value=""true"" />" +
                         @"<input name=""foo"" type=""hidden"" value=""false"" />",
                         html.ToHtmlString());
        }

        [Fact]
        public void CheckBoxShouldRespectModelStateAttemptedValue()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetCheckBoxViewData());
            helper.ViewData.ModelState.SetModelValue("foo", HtmlHelperTest.GetValueProviderResult("false", "false"));

            // Act
            MvcHtmlString html = helper.CheckBox("foo");

            // Assert
            Assert.Equal(@"<input id=""foo"" name=""foo"" type=""checkbox"" value=""true"" />" +
                         @"<input name=""foo"" type=""hidden"" value=""false"" />",
                         html.ToHtmlString());
        }

        [Fact]
        public void CheckBoxWithOnlyName()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetCheckBoxViewData());

            // Act
            MvcHtmlString html = helper.CheckBox("foo");

            // Assert
            Assert.Equal(@"<input checked=""checked"" id=""foo"" name=""foo"" type=""checkbox"" value=""true"" />" +
                         @"<input name=""foo"" type=""hidden"" value=""false"" />",
                         html.ToHtmlString());
        }

        [Fact]
        public void CheckBoxWithOnlyName_Unobtrusive()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetCheckBoxViewData());
            helper.ViewContext.ClientValidationEnabled = true;
            helper.ViewContext.UnobtrusiveJavaScriptEnabled = true;
            helper.ViewContext.FormContext = new FormContext();
            helper.ClientValidationRuleFactory = (name, metadata) => new[] { new ModelClientValidationRule { ValidationType = "type", ErrorMessage = "error" } };

            // Act
            MvcHtmlString html = helper.CheckBox("foo");

            // Assert
            Assert.Equal(@"<input checked=""checked"" data-val=""true"" data-val-type=""error"" id=""foo"" name=""foo"" type=""checkbox"" value=""true"" />" +
                         @"<input name=""foo"" type=""hidden"" value=""false"" />",
                         html.ToHtmlString());
        }

        [Fact]
        public void CheckBoxWithNameAndObjectAttribute()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetCheckBoxViewData());

            // Act
            MvcHtmlString html = helper.CheckBox("foo", _attributesObjectDictionary);

            // Assert
            Assert.Equal(@"<input baz=""BazObjValue"" checked=""checked"" id=""foo"" name=""foo"" type=""checkbox"" value=""true"" />" +
                         @"<input name=""foo"" type=""hidden"" value=""false"" />",
                         html.ToHtmlString());
        }

        [Fact]
        public void CheckBoxWithNameAndObjectAttributeWithUnderscores()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetCheckBoxViewData());

            // Act
            MvcHtmlString html = helper.CheckBox("foo", _attributesObjectUnderscoresDictionary);

            // Assert
            Assert.Equal(@"<input checked=""checked"" foo-baz=""BazObjValue"" id=""foo"" name=""foo"" type=""checkbox"" value=""true"" />" +
                         @"<input name=""foo"" type=""hidden"" value=""false"" />",
                         html.ToHtmlString());
        }

        [Fact]
        public void CheckBoxWithObjectAttribute()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper();

            // Act
            MvcHtmlString html = helper.CheckBox("foo", false /* isChecked */, _attributesObjectDictionary);

            // Assert
            Assert.Equal(@"<input baz=""BazObjValue"" id=""foo"" name=""foo"" type=""checkbox"" value=""true"" />" +
                         @"<input name=""foo"" type=""hidden"" value=""false"" />",
                         html.ToHtmlString());
        }

        [Fact]
        public void CheckBoxWithObjectAttributeWithUnderscores()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper();

            // Act
            MvcHtmlString html = helper.CheckBox("foo", false /* isChecked */, _attributesObjectUnderscoresDictionary);

            // Assert
            Assert.Equal(@"<input foo-baz=""BazObjValue"" id=""foo"" name=""foo"" type=""checkbox"" value=""true"" />" +
                         @"<input name=""foo"" type=""hidden"" value=""false"" />",
                         html.ToHtmlString());
        }

        [Fact]
        public void CheckBoxWithAttributeDictionary()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper();

            // Act
            MvcHtmlString html = helper.CheckBox("foo", false /* isChecked */, _attributesDictionary);

            // Assert
            Assert.Equal(@"<input baz=""BazValue"" id=""foo"" name=""foo"" type=""checkbox"" value=""true"" />" +
                         @"<input name=""foo"" type=""hidden"" value=""false"" />",
                         html.ToHtmlString());
        }

        [Fact]
        public void CheckBoxWithPrefix()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper();
            helper.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = "MyPrefix";

            // Act
            MvcHtmlString html = helper.CheckBox("foo", false /* isChecked */, _attributesDictionary);

            // Assert
            Assert.Equal(@"<input baz=""BazValue"" id=""MyPrefix_foo"" name=""MyPrefix.foo"" type=""checkbox"" value=""true"" />" +
                         @"<input name=""MyPrefix.foo"" type=""hidden"" value=""false"" />",
                         html.ToHtmlString());
        }

        [Fact]
        public void CheckBoxWithPrefixAndEmptyName()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper();
            helper.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = "MyPrefix";

            // Act
            MvcHtmlString html = helper.CheckBox("", false /* isChecked */, _attributesDictionary);

            // Assert
            Assert.Equal(@"<input baz=""BazValue"" id=""MyPrefix"" name=""MyPrefix"" type=""checkbox"" value=""true"" />" +
                         @"<input name=""MyPrefix"" type=""hidden"" value=""false"" />",
                         html.ToHtmlString());
        }

        [Theory]
        [PropertyData("AttributeEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void CheckBox_AttributeEncodes_AddedHtmlAttributes(
            string text,
            bool htmlEncode,
            string encodedText)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            viewData.ModelMetadata.HtmlEncode = htmlEncode;
            var helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            var result = helper.CheckBox(name: "name", htmlAttributes: new { attribute = text, }).ToHtmlString();

            // Assert
            Assert.Equal(
                @"<input attribute=""" +
                    encodedText +
                    @""" id=""name"" name=""name"" type=""checkbox"" value=""true"" />" +
                @"<input name=""name"" type=""hidden"" value=""false"" />",
                result);
        }

        [Theory]
        [PropertyData("AttributeEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void CheckBox_AttributeEncodes_Name(string text, bool htmlEncode, string encodedText)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            viewData.ModelMetadata.HtmlEncode = htmlEncode;
            var helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            // htmlAttributes included only to avoid special-cased renaming done for id attribute.
            var result = helper.CheckBox(text, htmlAttributes: new { id = "id", }).ToHtmlString();

            // Assert
            Assert.Equal(
                @"<input id=""id"" name=""" + encodedText + @""" type=""checkbox"" value=""true"" />" +
                @"<input name=""" + encodedText + @""" type=""hidden"" value=""false"" />",
                result);
        }

        [Theory]
        [PropertyData("AttributeEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void CheckBox_AttributeEncodes_Prefix(string text, bool htmlEncode, string encodedText)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            viewData.ModelMetadata.HtmlEncode = htmlEncode;
            viewData.TemplateInfo.HtmlFieldPrefix = text;
            var helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            // htmlAttributes included only to avoid special-cased renaming done for id attribute.
            var result = helper.CheckBox(name: String.Empty, htmlAttributes: new { id = "id", }).ToHtmlString();

            // Assert
            Assert.Equal(
                @"<input id=""id"" name=""" + encodedText + @""" type=""checkbox"" value=""true"" />" +
                @"<input name=""" + encodedText + @""" type=""hidden"" value=""false"" />",
                result);
        }

        // No need for CheckBox_AttributeEncodes_Value() because CheckBox value is always true and hidden value
        // is always false.

        // CheckBoxFor

        [Fact]
        public void CheckBoxForWitNullExpressionThrows()
        {
            // Arrange
            HtmlHelper<FooBarBazModel> helper = MvcHelper.GetHtmlHelper(GetCheckBoxViewData());

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => helper.CheckBoxFor(null),
                "expression");
        }

        [Fact]
        public void CheckBoxForWithInvalidBooleanThrows()
        {
            // Arrange
            HtmlHelper<FooBarBazModel> helper = MvcHelper.GetHtmlHelper(GetCheckBoxViewData());

            // Act & Assert
            Assert.Throws<FormatException>(
                () => helper.CheckBoxFor(m => m.bar), // "bar" in ViewData isn't a valid boolean
                "String was not recognized as a valid Boolean.");
        }

        [Fact]
        public void CheckBoxForDictionaryOverridesImplicitParameters()
        {
            // Arrange
            HtmlHelper<FooBarBazModel> helper = MvcHelper.GetHtmlHelper(GetCheckBoxViewData());

            // Act
            MvcHtmlString html = helper.CheckBoxFor(m => m.baz, new { @checked = "checked", value = "false" });

            // Assert
            Assert.Equal(@"<input checked=""checked"" id=""baz"" name=""baz"" type=""checkbox"" value=""false"" />" +
                         @"<input name=""baz"" type=""hidden"" value=""false"" />",
                         html.ToHtmlString());
        }

        [Fact]
        public void CheckBoxForShouldNotCopyAttributesForHidden()
        {
            // Arrange
            HtmlHelper<FooBarBazModel> helper = MvcHelper.GetHtmlHelper(GetCheckBoxViewData());

            // Act
            MvcHtmlString html = helper.CheckBoxFor(m => m.foo, new { id = "myID" });

            // Assert
            Assert.Equal(@"<input checked=""checked"" id=""myID"" name=""foo"" type=""checkbox"" value=""true"" />" +
                         @"<input name=""foo"" type=""hidden"" value=""false"" />",
                         html.ToHtmlString());
        }

        [Fact]
        public void CheckBoxForCheckedWithOnlyName()
        {
            // Arrange
            HtmlHelper<FooBarBazModel> helper = MvcHelper.GetHtmlHelper(GetCheckBoxViewData());

            // Act
            MvcHtmlString html = helper.CheckBoxFor(m => m.foo);

            // Assert
            Assert.Equal(@"<input checked=""checked"" id=""foo"" name=""foo"" type=""checkbox"" value=""true"" />" +
                         @"<input name=""foo"" type=""hidden"" value=""false"" />",
                         html.ToHtmlString());
        }

        [Fact]
        public void CheckBoxForCheckedWithOnlyName_Unobtrusive()
        {
            // Arrange
            HtmlHelper<FooBarBazModel> helper = MvcHelper.GetHtmlHelper(GetCheckBoxViewData());
            helper.ViewContext.ClientValidationEnabled = true;
            helper.ViewContext.UnobtrusiveJavaScriptEnabled = true;
            helper.ViewContext.FormContext = new FormContext();
            helper.ClientValidationRuleFactory = (name, metadata) => new[] { new ModelClientValidationRule { ValidationType = "type", ErrorMessage = "error" } };

            // Act
            MvcHtmlString html = helper.CheckBoxFor(m => m.foo);

            // Assert
            Assert.Equal(@"<input checked=""checked"" data-val=""true"" data-val-type=""error"" id=""foo"" name=""foo"" type=""checkbox"" value=""true"" />" +
                         @"<input name=""foo"" type=""hidden"" value=""false"" />",
                         html.ToHtmlString());
        }

        [Fact]
        public void CheckBoxForShouldRespectModelStateAttemptedValue()
        {
            // Arrange
            HtmlHelper<FooBarBazModel> helper = MvcHelper.GetHtmlHelper(GetCheckBoxViewData());
            helper.ViewContext.ViewData.ModelState.SetModelValue("foo", HtmlHelperTest.GetValueProviderResult("false", "false"));

            // Act
            MvcHtmlString html = helper.CheckBoxFor(m => m.foo);

            // Assert
            Assert.Equal(@"<input id=""foo"" name=""foo"" type=""checkbox"" value=""true"" />" +
                         @"<input name=""foo"" type=""hidden"" value=""false"" />",
                         html.ToHtmlString());
        }

        [Fact]
        public void CheckBoxForWithObjectAttribute()
        {
            // Arrange
            HtmlHelper<FooBarBazModel> helper = MvcHelper.GetHtmlHelper(GetCheckBoxViewData());

            // Act
            MvcHtmlString html = helper.CheckBoxFor(m => m.foo, _attributesObjectDictionary);

            // Assert
            Assert.Equal(@"<input baz=""BazObjValue"" checked=""checked"" id=""foo"" name=""foo"" type=""checkbox"" value=""true"" />" +
                         @"<input name=""foo"" type=""hidden"" value=""false"" />",
                         html.ToHtmlString());
        }

        [Fact]
        public void CheckBoxForWithObjectAttributeWithUnderscores()
        {
            // Arrange
            HtmlHelper<FooBarBazModel> helper = MvcHelper.GetHtmlHelper(GetCheckBoxViewData());

            // Act
            MvcHtmlString html = helper.CheckBoxFor(m => m.foo, _attributesObjectUnderscoresDictionary);

            // Assert
            Assert.Equal(@"<input checked=""checked"" foo-baz=""BazObjValue"" id=""foo"" name=""foo"" type=""checkbox"" value=""true"" />" +
                         @"<input name=""foo"" type=""hidden"" value=""false"" />",
                         html.ToHtmlString());
        }

        [Fact]
        public void CheckBoxForWithAttributeDictionary()
        {
            // Arrange
            HtmlHelper<FooBarBazModel> helper = MvcHelper.GetHtmlHelper(GetCheckBoxViewData());

            // Act
            MvcHtmlString html = helper.CheckBoxFor(m => m.foo, _attributesDictionary);

            // Assert
            Assert.Equal(@"<input baz=""BazValue"" checked=""checked"" id=""foo"" name=""foo"" type=""checkbox"" value=""true"" />" +
                         @"<input name=""foo"" type=""hidden"" value=""false"" />",
                         html.ToHtmlString());
        }

        [Fact]
        public void CheckBoxForWithPrefix()
        {
            // Arrange
            HtmlHelper<FooBarBazModel> helper = MvcHelper.GetHtmlHelper(GetCheckBoxViewData());
            helper.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = "MyPrefix";

            // Act
            MvcHtmlString html = helper.CheckBoxFor(m => m.foo, _attributesDictionary);

            // Assert
            Assert.Equal(@"<input baz=""BazValue"" id=""MyPrefix_foo"" name=""MyPrefix.foo"" type=""checkbox"" value=""true"" />" +
                         @"<input name=""MyPrefix.foo"" type=""hidden"" value=""false"" />",
                         html.ToHtmlString());
        }

        [Theory]
        [PropertyData("AttributeEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void CheckBoxFor_AttributeEncodes_AddedHtmlAttributes(
            string text,
            bool htmlEncode,
            string encodedText)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            viewData.ModelMetadata.HtmlEncode = htmlEncode;
            var helper = MvcHelper.GetHtmlHelper(viewData);
            var dummy = false;

            // Act
            var result = helper.CheckBoxFor(m => dummy, htmlAttributes: new { attribute = text, }).ToHtmlString();

            // Assert
            Assert.Equal(
                @"<input attribute=""" +
                    encodedText +
                    @""" id=""dummy"" name=""dummy"" type=""checkbox"" value=""true"" />" +
                @"<input name=""dummy"" type=""hidden"" value=""false"" />",
                result);
        }

        [Theory]
        [PropertyData("AttributeEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void CheckBoxFor_AttributeEncodes_Prefix(string text, bool htmlEncode, string encodedText)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            viewData.ModelMetadata.HtmlEncode = htmlEncode;
            viewData.TemplateInfo.HtmlFieldPrefix = text;
            var helper = MvcHelper.GetHtmlHelper(viewData);
            var dummy = false;

            // Act
            // htmlAttributes included only to avoid special-cased renaming done for id attribute.
            var result = helper.CheckBoxFor(m => dummy, htmlAttributes: new { id = "id", }).ToHtmlString();

            // Assert
            Assert.Equal(
                @"<input id=""id"" name=""" + encodedText + @".dummy"" type=""checkbox"" value=""true"" />" +
                @"<input name=""" + encodedText + @".dummy"" type=""hidden"" value=""false"" />",
                result);
        }

        // No need for CheckBoxFor_AttributeEncodes_Value() because CheckBox value is always true and hidden value
        // is always false.

        // Culture tests

        [Fact]
        [ReplaceCulture]
        public void InputHelpersUseCurrentCultureToConvertValueParameter()
        {
            // Arrange
            DateTime dt = new DateTime(1900, 1, 1, 0, 0, 0);
            HtmlHelper helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary { { "foo", dt } });

            var tests = new[]
            {
                // Hidden(name)
                new
                {
                    Html = @"<input id=""foo"" name=""foo"" type=""hidden"" value=""01/01/1900 00:00:00"" />",
                    Action = new Func<MvcHtmlString>(() => helper.Hidden("foo"))
                },
                // Hidden(name, value)
                new
                {
                    Html = @"<input id=""foo"" name=""foo"" type=""hidden"" value=""01/01/1900 00:00:00"" />",
                    Action = new Func<MvcHtmlString>(() => helper.Hidden("foo", dt))
                },
                // Hidden(name, value, htmlAttributes)
                new
                {
                    Html = @"<input id=""foo"" name=""foo"" type=""hidden"" value=""01/01/1900 00:00:00"" />",
                    Action = new Func<MvcHtmlString>(() => helper.Hidden("foo", dt, null))
                },
                // Hidden(name, value, htmlAttributes)
                new
                {
                    Html = @"<input id=""foo"" name=""foo"" type=""hidden"" value=""01/01/1900 00:00:00"" />",
                    Action = new Func<MvcHtmlString>(() => helper.Hidden("foo", dt, new RouteValueDictionary()))
                },
                // RadioButton(name, value)
                new
                {
                    Html = @"<input checked=""checked"" id=""foo"" name=""foo"" type=""radio"" value=""01/01/1900 00:00:00"" />",
                    Action = new Func<MvcHtmlString>(() => helper.RadioButton("foo", dt))
                },
                // RadioButton(name, value, isChecked)
                new
                {
                    Html = @"<input id=""foo"" name=""foo"" type=""radio"" value=""01/01/1900 00:00:00"" />",
                    Action = new Func<MvcHtmlString>(() => helper.RadioButton("foo", dt, false))
                },
                // RadioButton(name, value, htmlAttributes)
                new
                {
                    Html = @"<input checked=""checked"" id=""foo"" name=""foo"" type=""radio"" value=""01/01/1900 00:00:00"" />",
                    Action = new Func<MvcHtmlString>(() => helper.RadioButton("foo", dt, null))
                },
                // RadioButton(name, value)
                new
                {
                    Html = @"<input checked=""checked"" id=""foo"" name=""foo"" type=""radio"" value=""01/01/1900 00:00:00"" />",
                    Action = new Func<MvcHtmlString>(() => helper.RadioButton("foo", dt, new RouteValueDictionary()))
                },
                // RadioButton(name, value, isChecked, htmlAttributes)
                new
                {
                    Html = @"<input id=""foo"" name=""foo"" type=""radio"" value=""01/01/1900 00:00:00"" />",
                    Action = new Func<MvcHtmlString>(() => helper.RadioButton("foo", dt, false, null))
                },
                // RadioButton(name, value, isChecked, htmlAttributes)
                new
                {
                    Html = @"<input id=""foo"" name=""foo"" type=""radio"" value=""01/01/1900 00:00:00"" />",
                    Action = new Func<MvcHtmlString>(() => helper.RadioButton("foo", dt, false, new RouteValueDictionary()))
                },
                // TextBox(name)
                new
                {
                    Html = @"<input id=""foo"" name=""foo"" type=""text"" value=""01/01/1900 00:00:00"" />",
                    Action = new Func<MvcHtmlString>(() => helper.TextBox("foo"))
                },
                // TextBox(name, value)
                new
                {
                    Html = @"<input id=""foo"" name=""foo"" type=""text"" value=""01/01/1900 00:00:00"" />",
                    Action = new Func<MvcHtmlString>(() => helper.TextBox("foo", dt))
                },
                // TextBox(name, value, hmtlAttributes)
                new
                {
                    Html = @"<input id=""foo"" name=""foo"" type=""text"" value=""01/01/1900 00:00:00"" />",
                    Action = new Func<MvcHtmlString>(() => helper.TextBox("foo", dt, (object)null))
                },
                // TextBox(name, value, hmtlAttributes)
                new
                {
                    Html = @"<input id=""foo"" name=""foo"" type=""text"" value=""01/01/1900 00:00:00"" />",
                    Action = new Func<MvcHtmlString>(() => helper.TextBox("foo", dt, new RouteValueDictionary()))
                }
            };

            // Act && Assert
            foreach (var test in tests)
            {
                Assert.Equal(test.Html, test.Action().ToHtmlString());
            }
        }

        // Hidden

        [Fact]
        public void HiddenWithByteArrayValueRendersBase64EncodedValue()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper();

            // Act
            MvcHtmlString result = htmlHelper.Hidden("ProductName", ByteArrayModelBinderTest.Base64TestBytes);

            // Assert
            Assert.Equal("<input id=\"ProductName\" name=\"ProductName\" type=\"hidden\" value=\"Fys1\" />", result.ToHtmlString());
        }

        [Fact]
        public void HiddenWithBinaryArrayValueRendersBase64EncodedValue()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper();

            // Act
            MvcHtmlString result = htmlHelper.Hidden("ProductName", new Binary(new byte[] { 23, 43, 53 }));

            // Assert
            Assert.Equal("<input id=\"ProductName\" name=\"ProductName\" type=\"hidden\" value=\"Fys1\" />", result.ToHtmlString());
        }

        [Fact]
        public void HiddenWithEmptyNameThrows()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetHiddenViewData());

            // Act & Assert
            Assert.ThrowsArgumentNullOrEmpty(
                delegate { helper.Hidden(String.Empty); },
                "name");
        }

        [Fact]
        public void HiddenWithExplicitValue()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetHiddenViewData());

            // Act
            MvcHtmlString html = helper.Hidden("foo", "DefaultFoo", null);

            // Assert
            Assert.Equal(@"<input id=""foo"" name=""foo"" type=""hidden"" value=""DefaultFoo"" />", html.ToHtmlString());
        }

        [Fact]
        public void HiddenWithExplicitValueAndAttributesDictionary()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetHiddenViewData());

            // Act
            MvcHtmlString html = helper.Hidden("foo", "DefaultFoo", _attributesDictionary);

            // Assert
            Assert.Equal(@"<input baz=""BazValue"" id=""foo"" name=""foo"" type=""hidden"" value=""DefaultFoo"" />", html.ToHtmlString());
        }

        [Fact]
        public void HiddenWithExplicitValueAndAttributesObject()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetHiddenViewData());

            // Act
            MvcHtmlString html = helper.Hidden("foo", "DefaultFoo", _attributesObjectDictionary);

            // Assert
            Assert.Equal(@"<input baz=""BazObjValue"" id=""foo"" name=""foo"" type=""hidden"" value=""DefaultFoo"" />", html.ToHtmlString());
        }

        [Fact]
        public void HiddenWithExplicitValueAndAttributesObjectWithUnderscores()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetHiddenViewData());

            // Act
            MvcHtmlString html = helper.Hidden("foo", "DefaultFoo", _attributesObjectUnderscoresDictionary);

            // Assert
            Assert.Equal(@"<input foo-baz=""BazObjValue"" id=""foo"" name=""foo"" type=""hidden"" value=""DefaultFoo"" />", html.ToHtmlString());
        }

        [Fact]
        public void HiddenWithExplicitValueNull()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetHiddenViewData());

            // Act
            MvcHtmlString html = helper.Hidden("foo", (string)null /* value */, (object)null /* htmlAttributes */);

            // Assert
            Assert.Equal(@"<input id=""foo"" name=""foo"" type=""hidden"" value=""ViewDataFoo"" />", html.ToHtmlString());
        }

        [Fact]
        public void HiddenWithImplicitValue()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetHiddenViewData());

            // Act
            MvcHtmlString html = helper.Hidden("foo");

            // Assert
            Assert.Equal(@"<input id=""foo"" name=""foo"" type=""hidden"" value=""ViewDataFoo"" />", html.ToHtmlString());
        }

        [Fact]
        public void HiddenWithImplicitValueAndAttributesDictionary()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetHiddenViewData());

            // Act
            MvcHtmlString html = helper.Hidden("foo", null, _attributesDictionary);

            // Assert
            Assert.Equal(@"<input baz=""BazValue"" id=""foo"" name=""foo"" type=""hidden"" value=""ViewDataFoo"" />", html.ToHtmlString());
        }

        [Fact]
        public void HiddenWithImplicitValueAndAttributesDictionaryReturnsEmptyValueIfNotFound()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetHiddenViewData());

            // Act
            MvcHtmlString html = helper.Hidden("keyNotFound", null, _attributesDictionary);

            // Assert
            Assert.Equal(@"<input baz=""BazValue"" id=""keyNotFound"" name=""keyNotFound"" type=""hidden"" value="""" />", html.ToHtmlString());
        }

        [Fact]
        public void HiddenWithImplicitValueAndAttributesObject()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetHiddenViewData());

            // Act
            MvcHtmlString html = helper.Hidden("foo", null, _attributesObjectDictionary);

            // Assert
            Assert.Equal(@"<input baz=""BazObjValue"" id=""foo"" name=""foo"" type=""hidden"" value=""ViewDataFoo"" />", html.ToHtmlString());
        }

        [Fact]
        public void HiddenWithNameAndValue()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetHiddenViewData());

            // Act
            MvcHtmlString html = helper.Hidden("foo", "fooValue");

            // Assert
            Assert.Equal(@"<input id=""foo"" name=""foo"" type=""hidden"" value=""fooValue"" />", html.ToHtmlString());
        }

        [Fact]
        public void HiddenWithPrefix()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetHiddenViewData());
            helper.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = "MyPrefix";

            // Act
            MvcHtmlString html = helper.Hidden("foo", "fooValue");

            // Assert
            Assert.Equal(@"<input id=""MyPrefix_foo"" name=""MyPrefix.foo"" type=""hidden"" value=""fooValue"" />", html.ToHtmlString());
        }

        [Fact]
        public void HiddenWithPrefixAndEmptyName()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetHiddenViewData());
            helper.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = "MyPrefix";

            // Act
            MvcHtmlString html = helper.Hidden("", "fooValue");

            // Assert
            Assert.Equal(@"<input id=""MyPrefix"" name=""MyPrefix"" type=""hidden"" value=""fooValue"" />", html.ToHtmlString());
        }

        [Fact]
        public void HiddenWithNullNameThrows()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetHiddenViewData());

            // Act & Assert
            Assert.ThrowsArgumentNullOrEmpty(
                delegate { helper.Hidden(null /* name */); },
                "name");
        }

        [Fact]
        public void HiddenWithViewDataErrors()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetHiddenViewDataWithErrors());

            // Act
            MvcHtmlString html = helper.Hidden("foo", null, _attributesObjectDictionary);

            // Assert
            Assert.Equal(@"<input baz=""BazObjValue"" class=""input-validation-error"" id=""foo"" name=""foo"" type=""hidden"" value=""AttemptedValueFoo"" />", html.ToHtmlString());
        }

        [Fact]
        public void HiddenWithViewDataErrorsAndCustomClass()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetHiddenViewDataWithErrors());

            // Act
            MvcHtmlString html = helper.Hidden("foo", null, new { @class = "foo-class" });

            // Assert
            Assert.Equal(@"<input class=""input-validation-error foo-class"" id=""foo"" name=""foo"" type=""hidden"" value=""AttemptedValueFoo"" />", html.ToHtmlString());
        }

        [Theory]
        [PropertyData("AttributeEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void Hidden_AttributeEncodes_AddedHtmlAttributes(
            string text,
            bool htmlEncode,
            string encodedText)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            viewData.ModelMetadata.HtmlEncode = htmlEncode;
            var helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            var result = helper.Hidden(name: "name", value: null, htmlAttributes: new { attribute = text, })
                .ToHtmlString();

            // Assert
            Assert.Equal(
                @"<input attribute=""" + encodedText + @""" id=""name"" name=""name"" type=""hidden"" value="""" />",
                result);
        }

        [Theory]
        [PropertyData("AttributeEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void Hidden_AttributeEncodes_Name(string text, bool htmlEncode, string encodedText)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            viewData.ModelMetadata.HtmlEncode = htmlEncode;
            var helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            // htmlAttributes included only to avoid special-cased renaming done for id attribute.
            var result = helper.Hidden(text, value: null, htmlAttributes: new { id = "id", }).ToHtmlString();

            // Assert
            Assert.Equal(@"<input id=""id"" name=""" + encodedText + @""" type=""hidden"" value="""" />", result);
        }

        [Theory]
        [PropertyData("AttributeEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void Hidden_AttributeEncodes_Prefix(string text, bool htmlEncode, string encodedText)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            viewData.ModelMetadata.HtmlEncode = htmlEncode;
            viewData.TemplateInfo.HtmlFieldPrefix = text;
            var helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            // htmlAttributes included only to avoid special-cased renaming done for id attribute.
            var result = helper.Hidden(name: String.Empty, value: null, htmlAttributes: new { id = "id", })
                .ToHtmlString();

            // Assert
            Assert.Equal(@"<input id=""id"" name=""" + encodedText + @""" type=""hidden"" value="""" />", result);
        }

        [Theory]
        [PropertyData("AttributeEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void Hidden_AttributeEncodes_Value(
            string text,
            bool htmlEncode,
            string encodedText)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            viewData.ModelMetadata.HtmlEncode = htmlEncode;
            var helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            var result = helper.Hidden("name", value: text).ToHtmlString();

            // Assert
            Assert.Equal(
                @"<input id=""name"" name=""name"" type=""hidden"" value=""" + encodedText + @""" />",
                result);
        }

        // HiddenFor

        [Fact]
        public void HiddenForWithNullExpressionThrows()
        {
            // Arrange
            HtmlHelper<HiddenModel> helper = MvcHelper.GetHtmlHelper(GetHiddenViewData());

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => helper.HiddenFor<HiddenModel, object>(null),
                "expression"
                );
        }

        [Fact]
        public void HiddenForWithStringValue()
        {
            // Arrange
            HtmlHelper<HiddenModel> helper = MvcHelper.GetHtmlHelper(GetHiddenViewData());
            helper.ViewData.Model.foo = "DefaultFoo";

            // Act
            MvcHtmlString html = helper.HiddenFor(m => m.foo);

            // Assert
            Assert.Equal(@"<input id=""foo"" name=""foo"" type=""hidden"" value=""DefaultFoo"" />", html.ToHtmlString());
        }

        [Fact]
        public void HiddenForWithByteArrayValueRendersBase64EncodedValue()
        {
            // Arrange
            HtmlHelper<HiddenModel> helper = MvcHelper.GetHtmlHelper(GetHiddenViewData());
            helper.ViewData.Model.bytes = ByteArrayModelBinderTest.Base64TestBytes;

            // Act
            MvcHtmlString result = helper.HiddenFor(m => m.bytes);

            // Assert
            Assert.Equal("<input id=\"bytes\" name=\"bytes\" type=\"hidden\" value=\"Fys1\" />", result.ToHtmlString());
        }

        [Fact]
        public void HiddenForWithBinaryValueRendersBase64EncodedValue()
        {
            // Arrange
            HtmlHelper<HiddenModel> helper = MvcHelper.GetHtmlHelper(GetHiddenViewData());
            helper.ViewData.Model.binary = new Binary(new byte[] { 23, 43, 53 });

            // Act
            MvcHtmlString result = helper.HiddenFor(m => m.binary);

            // Assert
            Assert.Equal("<input id=\"binary\" name=\"binary\" type=\"hidden\" value=\"Fys1\" />", result.ToHtmlString());
        }

        [Fact]
        public void HiddenForWithAttributesDictionary()
        {
            // Arrange
            HtmlHelper<HiddenModel> helper = MvcHelper.GetHtmlHelper(GetHiddenViewData());
            helper.ViewData.Model.foo = "DefaultFoo";

            // Act
            MvcHtmlString html = helper.HiddenFor(m => m.foo, _attributesDictionary);

            // Assert
            Assert.Equal(@"<input baz=""BazValue"" id=""foo"" name=""foo"" type=""hidden"" value=""DefaultFoo"" />", html.ToHtmlString());
        }

        [Fact]
        public void HiddenForWithAttributesObject()
        {
            // Arrange
            HtmlHelper<HiddenModel> helper = MvcHelper.GetHtmlHelper(GetHiddenViewData());
            helper.ViewData.Model.foo = "DefaultFoo";

            // Act
            MvcHtmlString html = helper.HiddenFor(m => m.foo, _attributesObjectDictionary);

            // Assert
            Assert.Equal(@"<input baz=""BazObjValue"" id=""foo"" name=""foo"" type=""hidden"" value=""DefaultFoo"" />", html.ToHtmlString());
        }

        [Fact]
        public void HiddenForWithAttributesObjectWithUnderscores()
        {
            // Arrange
            HtmlHelper<HiddenModel> helper = MvcHelper.GetHtmlHelper(GetHiddenViewData());
            helper.ViewData.Model.foo = "DefaultFoo";

            // Act
            MvcHtmlString html = helper.HiddenFor(m => m.foo, _attributesObjectUnderscoresDictionary);

            // Assert
            Assert.Equal(@"<input foo-baz=""BazObjValue"" id=""foo"" name=""foo"" type=""hidden"" value=""DefaultFoo"" />", html.ToHtmlString());
        }

        [Fact]
        public void HiddenForWithPrefix()
        {
            // Arrange
            HtmlHelper<HiddenModel> helper = MvcHelper.GetHtmlHelper(GetHiddenViewData());
            helper.ViewData.Model.foo = "fooValue";
            helper.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = "MyPrefix";

            // Act
            MvcHtmlString html = helper.HiddenFor(m => m.foo);

            // Assert
            Assert.Equal(@"<input id=""MyPrefix_foo"" name=""MyPrefix.foo"" type=""hidden"" value=""fooValue"" />", html.ToHtmlString());
        }

        [Fact]
        public void HiddenForWithPrefixAndEmptyName()
        {
            // Arrange
            HtmlHelper<HiddenModel> helper = MvcHelper.GetHtmlHelper(GetHiddenViewData());
            helper.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = "MyPrefix";

            // Act
            MvcHtmlString html = helper.HiddenFor(m => m);

            // Assert
            Assert.Equal(@"<input id=""MyPrefix"" name=""MyPrefix"" type=""hidden"" value=""{ foo = (null) }"" />", html.ToHtmlString());
        }

        [Fact]
        public void HiddenForWithViewDataErrors()
        {
            // Arrange
            HtmlHelper<HiddenModel> helper = MvcHelper.GetHtmlHelper(GetHiddenViewDataWithErrors());

            // Act
            MvcHtmlString html = helper.HiddenFor(m => m.foo, _attributesObjectDictionary);

            // Assert
            Assert.Equal(@"<input baz=""BazObjValue"" class=""input-validation-error"" id=""foo"" name=""foo"" type=""hidden"" value=""AttemptedValueFoo"" />", html.ToHtmlString());
        }

        [Fact]
        public void HiddenForWithViewDataErrorsAndCustomClass()
        {
            // Arrange
            HtmlHelper<HiddenModel> helper = MvcHelper.GetHtmlHelper(GetHiddenViewDataWithErrors());

            // Act
            MvcHtmlString html = helper.HiddenFor(m => m.foo, new { @class = "foo-class" });

            // Assert
            Assert.Equal(@"<input class=""input-validation-error foo-class"" id=""foo"" name=""foo"" type=""hidden"" value=""AttemptedValueFoo"" />", html.ToHtmlString());
        }

        [Theory]
        [PropertyData("AttributeEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void HiddenFor_AttributeEncodes_AddedHtmlAttributes(
            string text,
            bool htmlEncode,
            string encodedText)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            viewData.ModelMetadata.HtmlEncode = htmlEncode;
            viewData.TemplateInfo.HtmlFieldPrefix = "name";
            var helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            var result = helper.HiddenFor(m => m, htmlAttributes: new { attribute = text, }).ToHtmlString();

            // Assert
            Assert.Equal(
                @"<input attribute=""" + encodedText + @""" id=""name"" name=""name"" type=""hidden"" value="""" />",
                result);
        }

        [Theory]
        [PropertyData("AttributeEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void HiddenFor_AttributeEncodes_Prefix(string text, bool htmlEncode, string encodedText)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            viewData.ModelMetadata.HtmlEncode = htmlEncode;
            viewData.TemplateInfo.HtmlFieldPrefix = text;
            var helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            // htmlAttributes included only to avoid special-cased renaming done for id attribute.
            var result = helper.HiddenFor(m => m, htmlAttributes: new { id = "id", }).ToHtmlString();

            // Assert
            Assert.Equal(@"<input id=""id"" name=""" + encodedText + @""" type=""hidden"" value="""" />", result);
        }

        [Theory]
        [PropertyData("AttributeEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void HiddenFor_AttributeEncodes_Value(
            string text,
            bool htmlEncode,
            string encodedText)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            viewData.ModelMetadata.HtmlEncode = htmlEncode;
            var helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            var result = helper.HiddenFor(m => text).ToHtmlString();

            // Assert
            Assert.Equal(
                @"<input id=""text"" name=""text"" type=""hidden"" value=""" + encodedText + @""" />",
                result);
        }

        // Password

        [Fact]
        public void PasswordWithEmptyNameThrows()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetPasswordViewData());

            // Act & Assert
            Assert.ThrowsArgumentNullOrEmpty(
                delegate { helper.Password(String.Empty); },
                "name");
        }

        [Fact]
        public void PasswordDictionaryOverridesImplicitParameters()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetPasswordViewData());

            // Act
            MvcHtmlString html = helper.Password("foo", "Some Value", new { type = "fooType" });

            // Assert
            Assert.Equal(@"<input id=""foo"" name=""foo"" type=""fooType"" value=""Some Value"" />", html.ToHtmlString());
        }

        [Fact]
        public void PasswordExplicitParametersOverrideDictionary()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetPasswordViewData());

            // Act
            MvcHtmlString html = helper.Password("foo", "Some Value", new { value = "Another Value", name = "bar" });

            // Assert
            Assert.Equal(@"<input id=""foo"" name=""foo"" type=""password"" value=""Some Value"" />", html.ToHtmlString());
        }

        [Fact]
        public void PasswordWithExplicitValue()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetPasswordViewData());

            // Act
            MvcHtmlString html = helper.Password("foo", "DefaultFoo", (object)null);

            // Assert
            Assert.Equal(@"<input id=""foo"" name=""foo"" type=""password"" value=""DefaultFoo"" />", html.ToHtmlString());
        }

        [Fact]
        public void PasswordWithExplicitValue_Unobtrusive()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetPasswordViewData());
            helper.ViewContext.ClientValidationEnabled = true;
            helper.ViewContext.UnobtrusiveJavaScriptEnabled = true;
            helper.ViewContext.FormContext = new FormContext();
            helper.ClientValidationRuleFactory = (name, metadata) => new[] { new ModelClientValidationRule { ValidationType = "type", ErrorMessage = "error" } };

            // Act
            MvcHtmlString html = helper.Password("foo", "DefaultFoo", (object)null);

            // Assert
            Assert.Equal(@"<input data-val=""true"" data-val-type=""error"" id=""foo"" name=""foo"" type=""password"" value=""DefaultFoo"" />", html.ToHtmlString());
        }

        [Fact]
        public void PasswordWithExplicitValueAndAttributesDictionary()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetPasswordViewData());

            // Act
            MvcHtmlString html = helper.Password("foo", "DefaultFoo", _attributesDictionary);

            // Assert
            Assert.Equal(@"<input baz=""BazValue"" id=""foo"" name=""foo"" type=""password"" value=""DefaultFoo"" />", html.ToHtmlString());
        }

        [Fact]
        public void PasswordWithExplicitValueAndAttributesObject()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetPasswordViewData());

            // Act
            MvcHtmlString html = helper.Password("foo", "DefaultFoo", _attributesObjectDictionary);

            // Assert
            Assert.Equal(@"<input baz=""BazObjValue"" id=""foo"" name=""foo"" type=""password"" value=""DefaultFoo"" />", html.ToHtmlString());
        }

        [Fact]
        public void PasswordWithExplicitValueNull()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetPasswordViewData());

            // Act
            MvcHtmlString html = helper.Password("foo", (string)null /* value */, (object)null);

            // Assert
            Assert.Equal(@"<input id=""foo"" name=""foo"" type=""password"" />", html.ToHtmlString());
        }

        [Fact]
        public void PasswordWithImplicitValue()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetPasswordViewData());

            // Act
            MvcHtmlString html = helper.Password("foo");

            // Assert
            Assert.Equal(@"<input id=""foo"" name=""foo"" type=""password"" />", html.ToHtmlString());
        }

        [Fact]
        public void PasswordWithImplicitValueAndAttributesDictionary()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetPasswordViewData());

            // Act
            MvcHtmlString html = helper.Password("foo", null, _attributesDictionary);

            // Assert
            Assert.Equal(@"<input baz=""BazValue"" id=""foo"" name=""foo"" type=""password"" />", html.ToHtmlString());
        }

        [Fact]
        public void PasswordWithImplicitValueAndAttributesDictionaryReturnsEmptyValueIfNotFound()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetPasswordViewData());

            // Act
            MvcHtmlString html = helper.Password("keyNotFound", null, _attributesDictionary);

            // Assert
            Assert.Equal(@"<input baz=""BazValue"" id=""keyNotFound"" name=""keyNotFound"" type=""password"" />", html.ToHtmlString());
        }

        [Fact]
        public void PasswordWithImplicitValueAndAttributesObject()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetPasswordViewData());

            // Act
            MvcHtmlString html = helper.Password("foo", null, _attributesObjectDictionary);

            // Assert
            Assert.Equal(@"<input baz=""BazObjValue"" id=""foo"" name=""foo"" type=""password"" />", html.ToHtmlString());
        }

        [Fact]
        public void PasswordWithImplicitValueAndAttributesObjectWithUnderscores()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetPasswordViewData());

            // Act
            MvcHtmlString html = helper.Password("foo", null, _attributesObjectUnderscoresDictionary);

            // Assert
            Assert.Equal(@"<input foo-baz=""BazObjValue"" id=""foo"" name=""foo"" type=""password"" />", html.ToHtmlString());
        }

        [Fact]
        public void PasswordWithNameAndValue()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetHiddenViewData());

            // Act
            MvcHtmlString html = helper.Password("foo", "fooValue");

            // Assert
            Assert.Equal(@"<input id=""foo"" name=""foo"" type=""password"" value=""fooValue"" />", html.ToHtmlString());
        }

        [Fact]
        public void PasswordWithPrefix()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetHiddenViewData());
            helper.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = "MyPrefix";

            // Act
            MvcHtmlString html = helper.Password("foo", "fooValue");

            // Assert
            Assert.Equal(@"<input id=""MyPrefix_foo"" name=""MyPrefix.foo"" type=""password"" value=""fooValue"" />", html.ToHtmlString());
        }

        [Fact]
        public void PasswordWithPrefixAndEmptyName()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetHiddenViewData());
            helper.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = "MyPrefix";

            // Act
            MvcHtmlString html = helper.Password("", "fooValue");

            // Assert
            Assert.Equal(@"<input id=""MyPrefix"" name=""MyPrefix"" type=""password"" value=""fooValue"" />", html.ToHtmlString());
        }

        [Fact]
        public void PasswordWithNullNameThrows()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetPasswordViewData());

            // Act & Assert
            Assert.ThrowsArgumentNullOrEmpty(
                delegate { helper.Password(null /* name */); },
                "name");
        }

        [Fact]
        public void PasswordWithViewDataErrors()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetPasswordViewDataWithErrors());

            // Act
            MvcHtmlString html = helper.Password("foo", null, _attributesObjectDictionary);

            // Assert
            Assert.Equal(@"<input baz=""BazObjValue"" class=""input-validation-error"" id=""foo"" name=""foo"" type=""password"" />", html.ToHtmlString());
        }

        [Fact]
        public void PasswordWithViewDataErrorsAndCustomClass()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetPasswordViewDataWithErrors());

            // Act
            MvcHtmlString html = helper.Password("foo", null, new { @class = "foo-class" });

            // Assert
            Assert.Equal(@"<input class=""input-validation-error foo-class"" id=""foo"" name=""foo"" type=""password"" />", html.ToHtmlString());
        }

        [Theory]
        [PropertyData("AttributeEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void Password_AttributeEncodes_AddedHtmlAttributes(
            string text,
            bool htmlEncode,
            string encodedText)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            viewData.ModelMetadata.HtmlEncode = htmlEncode;
            var helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            var result = helper.Password(name: "name", value: null, htmlAttributes: new { attribute = text, })
                .ToHtmlString();

            // Assert
            Assert.Equal(
                @"<input attribute=""" + encodedText + @""" id=""name"" name=""name"" type=""password"" />",
                result);
        }

        [Theory]
        [PropertyData("AttributeEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void Password_AttributeEncodes_Name(string text, bool htmlEncode, string encodedText)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            viewData.ModelMetadata.HtmlEncode = htmlEncode;
            var helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            // htmlAttributes included only to avoid special-cased renaming done for id attribute.
            var result = helper.Password(text, value: null, htmlAttributes: new { id = "id", }).ToHtmlString();

            // Assert
            Assert.Equal(@"<input id=""id"" name=""" + encodedText + @""" type=""password"" />", result);
        }

        [Theory]
        [PropertyData("AttributeEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void Password_AttributeEncodes_Prefix(string text, bool htmlEncode, string encodedText)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            viewData.ModelMetadata.HtmlEncode = htmlEncode;
            viewData.TemplateInfo.HtmlFieldPrefix = text;
            var helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            // htmlAttributes included only to avoid special-cased renaming done for id attribute.
            var result = helper.Password(name: String.Empty, value: null, htmlAttributes: new { id = "id", })
                .ToHtmlString();

            // Assert
            Assert.Equal(@"<input id=""id"" name=""" + encodedText + @""" type=""password"" />", result);
        }

        [Theory]
        [PropertyData("AttributeEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void Password_AttributeEncodes_Value(
            string text,
            bool htmlEncode,
            string encodedText)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            viewData.ModelMetadata.HtmlEncode = htmlEncode;
            var helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            var result = helper.Password("name", value: text).ToHtmlString();

            // Assert
            Assert.Equal(
                @"<input id=""name"" name=""name"" type=""password"" value=""" + encodedText + @""" />",
                result);
        }

        // PasswordFor

        [Fact]
        public void PasswordForWithNullExpressionThrows()
        {
            // Arrange
            HtmlHelper<FooModel> helper = MvcHelper.GetHtmlHelper(GetPasswordViewData());

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => helper.PasswordFor<FooModel, object>(null),
                "expression");
        }

        [Fact]
        public void PasswordForDictionaryOverridesImplicitParameters()
        {
            // Arrange
            HtmlHelper<FooModel> helper = MvcHelper.GetHtmlHelper(GetPasswordViewData());

            // Act
            MvcHtmlString html = helper.PasswordFor(m => m.foo, new { type = "fooType" });

            // Assert
            Assert.Equal(@"<input id=""foo"" name=""foo"" type=""fooType"" />", html.ToHtmlString());
        }

        [Fact]
        public void PasswordForExpressionNameOverridesDictionary()
        {
            // Arrange
            HtmlHelper<FooModel> helper = MvcHelper.GetHtmlHelper(GetPasswordViewData());

            // Act
            MvcHtmlString html = helper.PasswordFor(m => m.foo, new { name = "bar" });

            // Assert
            Assert.Equal(@"<input id=""foo"" name=""foo"" type=""password"" />", html.ToHtmlString());
        }

        [Fact]
        public void PasswordForWithImplicitValue()
        {
            // Arrange
            HtmlHelper<FooModel> helper = MvcHelper.GetHtmlHelper(GetPasswordViewData());

            // Act
            MvcHtmlString html = helper.PasswordFor(m => m.foo);

            // Assert
            Assert.Equal(@"<input id=""foo"" name=""foo"" type=""password"" />", html.ToHtmlString());
        }

        [Fact]
        public void PasswordForWithImplicitValue_Unobtrusive()
        {
            // Arrange
            HtmlHelper<FooModel> helper = MvcHelper.GetHtmlHelper(GetPasswordViewData());
            helper.ViewContext.ClientValidationEnabled = true;
            helper.ViewContext.UnobtrusiveJavaScriptEnabled = true;
            helper.ViewContext.FormContext = new FormContext();
            helper.ClientValidationRuleFactory = (name, metadata) => new[] { new ModelClientValidationRule { ValidationType = "type", ErrorMessage = "error" } };

            // Act
            MvcHtmlString html = helper.PasswordFor(m => m.foo);

            // Assert
            Assert.Equal(@"<input data-val=""true"" data-val-type=""error"" id=""foo"" name=""foo"" type=""password"" />", html.ToHtmlString());
        }

        [Fact]
        public void PasswordForWithDeepValueWithNullModel_Unobtrusive()
        { // Dev10 Bug #936192
            // Arrange
            HtmlHelper<DeepContainerModel> helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary<DeepContainerModel>());
            helper.ViewContext.ClientValidationEnabled = true;
            helper.ViewContext.UnobtrusiveJavaScriptEnabled = true;
            helper.ViewContext.FormContext = new FormContext();

            using (new CultureReplacer("en-US", "en-US"))
            {
                // Act
                MvcHtmlString html = helper.PasswordFor(m => m.contained.foo);

                // Assert
                Assert.Equal(@"<input data-val=""true"" data-val-required=""The foo field is required."" id=""contained_foo"" name=""contained.foo"" type=""password"" />", html.ToHtmlString());
            }
        }

        [Fact]
        public void PasswordForWithAttributesDictionary()
        {
            // Arrange
            HtmlHelper<FooModel> helper = MvcHelper.GetHtmlHelper(GetPasswordViewData());

            // Act
            MvcHtmlString html = helper.PasswordFor(m => m.foo, _attributesDictionary);

            // Assert
            Assert.Equal(@"<input baz=""BazValue"" id=""foo"" name=""foo"" type=""password"" />", html.ToHtmlString());
        }

        [Fact]
        public void PasswordForWithAttributesObject()
        {
            // Arrange
            HtmlHelper<FooModel> helper = MvcHelper.GetHtmlHelper(GetPasswordViewData());

            // Act
            MvcHtmlString html = helper.PasswordFor(m => m.foo, _attributesObjectDictionary);

            // Assert
            Assert.Equal(@"<input baz=""BazObjValue"" id=""foo"" name=""foo"" type=""password"" />", html.ToHtmlString());
        }

        [Fact]
        public void PasswordForWithAttributesObjectWithUnderscores()
        {
            // Arrange
            HtmlHelper<FooModel> helper = MvcHelper.GetHtmlHelper(GetPasswordViewData());

            // Act
            MvcHtmlString html = helper.PasswordFor(m => m.foo, _attributesObjectUnderscoresDictionary);

            // Assert
            Assert.Equal(@"<input foo-baz=""BazObjValue"" id=""foo"" name=""foo"" type=""password"" />", html.ToHtmlString());
        }

        [Fact]
        public void PasswordForWithPrefix()
        {
            // Arrange
            HtmlHelper<FooModel> helper = MvcHelper.GetHtmlHelper(GetPasswordViewData());
            helper.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = "MyPrefix";

            // Act
            MvcHtmlString html = helper.PasswordFor(m => m.foo);

            // Assert
            Assert.Equal(@"<input id=""MyPrefix_foo"" name=""MyPrefix.foo"" type=""password"" />", html.ToHtmlString());
        }

        [Fact]
        public void PasswordForWithViewDataErrors()
        {
            // Arrange
            HtmlHelper<FooModel> helper = MvcHelper.GetHtmlHelper(GetPasswordViewDataWithErrors());

            // Act
            MvcHtmlString html = helper.PasswordFor(m => m.foo, _attributesObjectDictionary);

            // Assert
            Assert.Equal(@"<input baz=""BazObjValue"" class=""input-validation-error"" id=""foo"" name=""foo"" type=""password"" />", html.ToHtmlString());
        }

        [Fact]
        public void PasswordForWithViewDataErrorsAndCustomClass()
        {
            // Arrange
            HtmlHelper<FooModel> helper = MvcHelper.GetHtmlHelper(GetPasswordViewDataWithErrors());

            // Act
            MvcHtmlString html = helper.PasswordFor(m => m.foo, new { @class = "foo-class" });

            // Assert
            Assert.Equal(@"<input class=""input-validation-error foo-class"" id=""foo"" name=""foo"" type=""password"" />", html.ToHtmlString());
        }

        [Theory]
        [PropertyData("AttributeEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void PasswordFor_AttributeEncodes_AddedHtmlAttributes(
            string text,
            bool htmlEncode,
            string encodedText)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            viewData.ModelMetadata.HtmlEncode = htmlEncode;
            viewData.TemplateInfo.HtmlFieldPrefix = "name";
            var helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            var result = helper.PasswordFor(m => m, htmlAttributes: new { attribute = text, }).ToHtmlString();

            // Assert
            Assert.Equal(
                @"<input attribute=""" + encodedText + @""" id=""name"" name=""name"" type=""password"" />",
                result);
        }

        [Theory]
        [PropertyData("AttributeEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void PasswordFor_AttributeEncodes_Prefix(string text, bool htmlEncode, string encodedText)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            viewData.ModelMetadata.HtmlEncode = htmlEncode;
            viewData.TemplateInfo.HtmlFieldPrefix = text;
            var helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            // htmlAttributes included only to avoid special-cased renaming done for id attribute.
            var result = helper.PasswordFor(m => m, htmlAttributes: new { id = "id", }).ToHtmlString();

            // Assert
            Assert.Equal(@"<input id=""id"" name=""" + encodedText + @""" type=""password"" />", result);
        }

        // No need for PasswordFor_AttributeEncodes_Value() because PasswordFor() always uses a null value.

        // RadioButton

        [Fact]
        public void RadioButtonDictionaryOverridesImplicitParameters()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetRadioButtonViewData());

            // Act
            MvcHtmlString html = helper.RadioButton("bar", "ViewDataBar", new { @checked = "chucked", value = "baz" });

            // Assert
            Assert.Equal(@"<input checked=""chucked"" id=""bar"" name=""bar"" type=""radio"" value=""ViewDataBar"" />", html.ToHtmlString());
        }

        [Fact]
        public void RadioButtonExplicitParametersOverrideDictionary()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetRadioButtonViewData());

            // Act
            MvcHtmlString html = helper.RadioButton("bar", "ViewDataBar", false, new { @checked = "checked", value = "baz" });

            // Assert
            Assert.Equal(@"<input id=""bar"" name=""bar"" type=""radio"" value=""ViewDataBar"" />", html.ToHtmlString());
        }

        [Fact]
        public void RadioButtonShouldRespectModelStateAttemptedValue()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetRadioButtonViewData());
            helper.ViewData.ModelState.SetModelValue("foo", HtmlHelperTest.GetValueProviderResult("ModelStateFoo", "ModelStateFoo"));

            // Act
            MvcHtmlString html = helper.RadioButton("foo", "ModelStateFoo", false, new { @checked = "checked", value = "baz" });

            // Assert
            Assert.Equal(@"<input checked=""checked"" id=""foo"" name=""foo"" type=""radio"" value=""ModelStateFoo"" />", html.ToHtmlString());
        }

        [Fact]
        public void RadioButtonValueParameterAlwaysRendered()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetRadioButtonViewData());

            // Act
            MvcHtmlString html = helper.RadioButton("foo", "ViewDataFoo");
            MvcHtmlString html2 = helper.RadioButton("foo", "fooValue2");

            // Assert
            Assert.Equal(@"<input checked=""checked"" id=""foo"" name=""foo"" type=""radio"" value=""ViewDataFoo"" />", html.ToHtmlString());
            Assert.Equal(@"<input id=""foo"" name=""foo"" type=""radio"" value=""fooValue2"" />", html2.ToHtmlString());
        }

        [Fact]
        public void RadioButtonWithEmptyNameThrows()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper();

            // Act & Assert
            Assert.ThrowsArgumentNullOrEmpty(
                delegate { helper.RadioButton(String.Empty, "value"); },
                "name");
        }

        [Fact]
        public void RadioButtonWithNullValueThrows()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                delegate { helper.RadioButton("foo", null); },
                "value");
        }

        [Fact]
        public void RadioButtonWithNameAndValue()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetRadioButtonViewData());

            // Act
            MvcHtmlString html = helper.RadioButton("foo", "ViewDataFoo");

            // Assert
            Assert.Equal(@"<input checked=""checked"" id=""foo"" name=""foo"" type=""radio"" value=""ViewDataFoo"" />", html.ToHtmlString());
        }

        [Fact]
        public void RadioButtonWithNameAndValue_Unobtrusive()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetRadioButtonViewData());
            helper.ViewContext.ClientValidationEnabled = true;
            helper.ViewContext.UnobtrusiveJavaScriptEnabled = true;
            helper.ViewContext.FormContext = new FormContext();
            helper.ClientValidationRuleFactory = (name, metadata) => new[] { new ModelClientValidationRule { ValidationType = "type", ErrorMessage = "error" } };

            // Act
            MvcHtmlString html = helper.RadioButton("foo", "ViewDataFoo");

            // Assert
            Assert.Equal(@"<input checked=""checked"" data-val=""true"" data-val-type=""error"" id=""foo"" name=""foo"" type=""radio"" value=""ViewDataFoo"" />", html.ToHtmlString());
        }

        [Fact]
        public void RadioButtonWithPrefix()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetRadioButtonViewData());
            helper.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = "MyPrefix";

            // Act
            MvcHtmlString html = helper.RadioButton("foo", "ViewDataFoo");

            // Assert
            Assert.Equal(@"<input checked=""checked"" id=""MyPrefix_foo"" name=""MyPrefix.foo"" type=""radio"" value=""ViewDataFoo"" />", html.ToHtmlString());
        }

        [Fact]
        public void RadioButtonWithPrefixAndEmptyName()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetRadioButtonViewData());
            helper.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = "MyPrefix";

            // Act
            MvcHtmlString html = helper.RadioButton("", "ViewDataFoo");

            // Assert
            Assert.Equal(@"<input id=""MyPrefix"" name=""MyPrefix"" type=""radio"" value=""ViewDataFoo"" />", html.ToHtmlString());
        }

        [Fact]
        public void RadioButtonWithNameAndValueNotMatched()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetRadioButtonViewData());

            // Act
            MvcHtmlString html = helper.RadioButton("foo", "fooValue");

            // Assert
            Assert.Equal(@"<input id=""foo"" name=""foo"" type=""radio"" value=""fooValue"" />", html.ToHtmlString());
        }

        [Fact]
        public void RadioButtonWithNameValueUnchecked()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetRadioButtonViewData());

            // Act
            MvcHtmlString html = helper.RadioButton("bar", "barValue", false /* isChecked */);

            // Assert
            Assert.Equal(@"<input id=""bar"" name=""bar"" type=""radio"" value=""barValue"" />", html.ToHtmlString());
        }

        [Fact]
        public void RadioButtonWithNameValueChecked()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetRadioButtonViewData());

            // Act
            MvcHtmlString html = helper.RadioButton("bar", "barValue", true /* isChecked */);

            // Assert
            Assert.Equal(@"<input checked=""checked"" id=""bar"" name=""bar"" type=""radio"" value=""barValue"" />", html.ToHtmlString());
        }

        [Fact]
        public void RadioButtonWithObjectAttribute()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetRadioButtonViewData());

            // Act
            MvcHtmlString html = helper.RadioButton("foo", "fooValue", _attributesObjectDictionary);

            // Assert
            Assert.Equal(@"<input baz=""BazObjValue"" id=""foo"" name=""foo"" type=""radio"" value=""fooValue"" />", html.ToHtmlString());
        }

        [Fact]
        public void RadioButtonWithObjectAttributeWithUnderscores()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetRadioButtonViewData());

            // Act
            MvcHtmlString html = helper.RadioButton("foo", "fooValue", _attributesObjectUnderscoresDictionary);

            // Assert
            Assert.Equal(@"<input foo-baz=""BazObjValue"" id=""foo"" name=""foo"" type=""radio"" value=""fooValue"" />", html.ToHtmlString());
        }

        [Fact]
        public void RadioButtonWithAttributeDictionary()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetRadioButtonViewData());

            // Act
            MvcHtmlString html = helper.RadioButton("bar", "barValue", _attributesDictionary);

            // Assert
            Assert.Equal(@"<input baz=""BazValue"" id=""bar"" name=""bar"" type=""radio"" value=""barValue"" />", html.ToHtmlString());
        }

        [Fact]
        public void RadioButtonWithValueUnchecked()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetRadioButtonViewData());

            // Act
            MvcHtmlString html = helper.RadioButton("foo", "bar", false /* isChecked */);

            // Assert
            Assert.Equal(@"<input id=""foo"" name=""foo"" type=""radio"" value=""bar"" />", html.ToHtmlString());
        }

        [Fact]
        public void RadioButtonWithValueAndObjectAttributeUnchecked()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetRadioButtonViewData());

            // Act
            MvcHtmlString html = helper.RadioButton("foo", "bar", false /* isChecked */, _attributesObjectDictionary);

            // Assert
            Assert.Equal(@"<input baz=""BazObjValue"" id=""foo"" name=""foo"" type=""radio"" value=""bar"" />", html.ToHtmlString());
        }

        [Fact]
        public void RadioButtonWithValueAndObjectAttributeWithUnderscoresUnchecked()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetRadioButtonViewData());

            // Act
            MvcHtmlString html = helper.RadioButton("foo", "bar", false /* isChecked */, _attributesObjectUnderscoresDictionary);

            // Assert
            Assert.Equal(@"<input foo-baz=""BazObjValue"" id=""foo"" name=""foo"" type=""radio"" value=""bar"" />", html.ToHtmlString());
        }

        [Fact]
        public void RadioButtonWithValueAndAttributeDictionaryUnchecked()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetRadioButtonViewData());

            // Act
            MvcHtmlString html = helper.RadioButton("foo", "bar", false /* isChecked */, _attributesDictionary);

            // Assert
            Assert.Equal(@"<input baz=""BazValue"" id=""foo"" name=""foo"" type=""radio"" value=""bar"" />", html.ToHtmlString());
        }

        [Theory]
        [PropertyData("AttributeEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void RadioButton_AttributeEncodes_AddedHtmlAttributes(
            string text,
            bool htmlEncode,
            string encodedText)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            viewData.ModelMetadata.HtmlEncode = htmlEncode;
            var helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            var result = helper.RadioButton(name: "name", value: "value", htmlAttributes: new { attribute = text, })
                .ToHtmlString();

            // Assert
            Assert.Equal(
                @"<input attribute=""" + encodedText + @""" id=""name"" name=""name"" type=""radio"" value=""value"" />",
                result);
        }

        [Theory]
        [PropertyData("AttributeEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void RadioButton_AttributeEncodes_Name(string text, bool htmlEncode, string encodedText)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            viewData.ModelMetadata.HtmlEncode = htmlEncode;
            var helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            // htmlAttributes included only to avoid special-cased renaming done for id attribute.
            var result = helper.RadioButton(text, value: "value", htmlAttributes: new { id = "id", })
                .ToHtmlString();

            // Assert
            Assert.Equal(@"<input id=""id"" name=""" + encodedText + @""" type=""radio"" value=""value"" />", result);
        }

        [Theory]
        [PropertyData("AttributeEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void RadioButton_AttributeEncodes_Prefix(string text, bool htmlEncode, string encodedText)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            viewData.ModelMetadata.HtmlEncode = htmlEncode;
            viewData.TemplateInfo.HtmlFieldPrefix = text;
            var helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            // htmlAttributes included only to avoid special-cased renaming done for id attribute.
            var result = helper.RadioButton(name: String.Empty, value: String.Empty, htmlAttributes: new { id = "id", })
                .ToHtmlString();

            // Assert
            Assert.Equal(@"<input id=""id"" name=""" + encodedText + @""" type=""radio"" value="""" />", result);
        }

        [Theory]
        [PropertyData("AttributeEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void RadioButton_AttributeEncodes_Value(
            string text,
            bool htmlEncode,
            string encodedText)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            viewData.ModelMetadata.HtmlEncode = htmlEncode;
            var helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            var result = helper.RadioButton("name", value: text).ToHtmlString();

            // Assert
            Assert.Equal(
                @"<input id=""name"" name=""name"" type=""radio"" value=""" + encodedText + @""" />",
                result);
        }

        // RadioButtonFor

        [Fact]
        public void RadioButtonForWithNullExpressionThrows()
        {
            // Arrange
            HtmlHelper<FooBarModel> helper = MvcHelper.GetHtmlHelper(GetRadioButtonViewData());

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => helper.RadioButtonFor<FooBarModel, object>(null, "value"),
                "expression");
        }

        [Fact]
        public void RadioButtonForWithNullValueThrows()
        {
            // Arrange
            HtmlHelper<FooBarModel> helper = MvcHelper.GetHtmlHelper(GetRadioButtonViewData());

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => helper.RadioButtonFor(m => m.foo, null),
                "value");
        }

        [Fact]
        public void RadioButtonForDictionaryOverridesImplicitParameters()
        {
            // Arrange
            HtmlHelper<FooBarModel> helper = MvcHelper.GetHtmlHelper(GetRadioButtonViewData());

            // Act
            MvcHtmlString html = helper.RadioButtonFor(m => m.bar, "ViewDataBar", new { @checked = "chucked", value = "baz" });

            // Assert
            Assert.Equal(@"<input checked=""chucked"" id=""bar"" name=""bar"" type=""radio"" value=""ViewDataBar"" />", html.ToHtmlString());
        }

        [Fact]
        public void RadioButtonForShouldRespectModelStateAttemptedValue()
        {
            // Arrange
            HtmlHelper<FooBarModel> helper = MvcHelper.GetHtmlHelper(GetRadioButtonViewData());
            helper.ViewData.ModelState.SetModelValue("foo", HtmlHelperTest.GetValueProviderResult("ModelStateFoo", "ModelStateFoo"));

            // Act
            MvcHtmlString html = helper.RadioButtonFor(m => m.foo, "ModelStateFoo", new { @checked = "checked", value = "baz" });

            // Assert
            Assert.Equal(@"<input checked=""checked"" id=""foo"" name=""foo"" type=""radio"" value=""ModelStateFoo"" />", html.ToHtmlString());
        }

        [Fact]
        public void RadioButtonForValueParameterAlwaysRendered()
        {
            // Arrange
            HtmlHelper<FooBarModel> helper = MvcHelper.GetHtmlHelper(GetRadioButtonViewData());

            // Act & Assert
            Assert.Equal(@"<input checked=""checked"" id=""foo"" name=""foo"" type=""radio"" value=""ViewDataFoo"" />",
                         helper.RadioButtonFor(m => m.foo, "ViewDataFoo").ToHtmlString());
            Assert.Equal(@"<input id=""foo"" name=""foo"" type=""radio"" value=""fooValue2"" />",
                         helper.RadioButtonFor(m => m.foo, "fooValue2").ToHtmlString());
        }

        [Fact]
        public void RadioButtonForWithNameAndValue()
        {
            // Arrange
            HtmlHelper<FooBarModel> helper = MvcHelper.GetHtmlHelper(GetRadioButtonViewData());

            // Act
            MvcHtmlString html = helper.RadioButtonFor(m => m.foo, "ViewDataFoo");

            // Assert
            Assert.Equal(@"<input checked=""checked"" id=""foo"" name=""foo"" type=""radio"" value=""ViewDataFoo"" />", html.ToHtmlString());
        }

        [Fact]
        public void RadioButtonForWithNestedNameAndValue()
        {
            // Arrange
            HtmlHelper<string> helper = MvcHelper.GetHtmlHelper(GetRadioButtonNestedAndUnsetViewData());

            // Act
            MvcHtmlString html = helper.RadioButtonFor(m => m, "ViewItemFoo");

            // Assert
            Assert.Equal(@"<input checked=""checked"" id=""foo"" name=""foo"" type=""radio"" value=""ViewItemFoo"" />", html.ToHtmlString());
        }

        [Fact]
        public void RadioButtonForWithNameAndValue_Unobtrusive()
        {
            // Arrange
            HtmlHelper<FooBarModel> helper = MvcHelper.GetHtmlHelper(GetRadioButtonViewData());
            helper.ViewContext.ClientValidationEnabled = true;
            helper.ViewContext.UnobtrusiveJavaScriptEnabled = true;
            helper.ViewContext.FormContext = new FormContext();
            helper.ClientValidationRuleFactory = (name, metadata) => new[] { new ModelClientValidationRule { ValidationType = "type", ErrorMessage = "error" } };

            // Act
            MvcHtmlString html = helper.RadioButtonFor(m => m.foo, "ViewDataFoo");

            // Assert
            Assert.Equal(@"<input checked=""checked"" data-val=""true"" data-val-type=""error"" id=""foo"" name=""foo"" type=""radio"" value=""ViewDataFoo"" />", html.ToHtmlString());
        }

        [Fact]
        public void RadioButtonForWithPrefix()
        {
            // Arrange
            HtmlHelper<FooBarModel> helper = MvcHelper.GetHtmlHelper(GetRadioButtonViewData());
            helper.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = "MyPrefix";

            // Act
            MvcHtmlString html = helper.RadioButtonFor(m => m.foo, "ViewDataFoo");

            // Assert
            Assert.Equal(@"<input id=""MyPrefix_foo"" name=""MyPrefix.foo"" type=""radio"" value=""ViewDataFoo"" />", html.ToHtmlString());
        }

        [Fact]
        public void RadioButtonForWithNameAndValueNotMatched()
        {
            // Arrange
            HtmlHelper<FooBarModel> helper = MvcHelper.GetHtmlHelper(GetRadioButtonViewData());

            // Act
            MvcHtmlString html = helper.RadioButtonFor(m => m.foo, "fooValue");

            // Assert
            Assert.Equal(@"<input id=""foo"" name=""foo"" type=""radio"" value=""fooValue"" />", html.ToHtmlString());
        }

        [Fact]
        public void RadioButtonForWithObjectAttribute()
        {
            // Arrange
            HtmlHelper<FooBarModel> helper = MvcHelper.GetHtmlHelper(GetRadioButtonViewData());

            // Act
            MvcHtmlString html = helper.RadioButtonFor(m => m.foo, "fooValue", _attributesObjectDictionary);

            // Assert
            Assert.Equal(@"<input baz=""BazObjValue"" id=""foo"" name=""foo"" type=""radio"" value=""fooValue"" />", html.ToHtmlString());
        }

        [Fact]
        public void RadioButtonForWithObjectAttributeWithUnderscores()
        {
            // Arrange
            HtmlHelper<FooBarModel> helper = MvcHelper.GetHtmlHelper(GetRadioButtonViewData());

            // Act
            MvcHtmlString html = helper.RadioButtonFor(m => m.foo, "fooValue", _attributesObjectUnderscoresDictionary);

            // Assert
            Assert.Equal(@"<input foo-baz=""BazObjValue"" id=""foo"" name=""foo"" type=""radio"" value=""fooValue"" />", html.ToHtmlString());
        }

        [Fact]
        public void RadioButtonForWithAttributeDictionary()
        {
            // Arrange
            HtmlHelper<FooBarModel> helper = MvcHelper.GetHtmlHelper(GetRadioButtonViewData());

            // Act
            MvcHtmlString html = helper.RadioButtonFor(m => m.bar, "barValue", _attributesDictionary);

            // Assert
            Assert.Equal(@"<input baz=""BazValue"" id=""bar"" name=""bar"" type=""radio"" value=""barValue"" />", html.ToHtmlString());
        }

        [Theory]
        [PropertyData("AttributeEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void RadioButtonFor_AttributeEncodes_AddedHtmlAttributes(
            string text,
            bool htmlEncode,
            string encodedText)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            viewData.ModelMetadata.HtmlEncode = htmlEncode;
            viewData.TemplateInfo.HtmlFieldPrefix = "name";
            var helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            var result = helper.RadioButtonFor(m => m, value: String.Empty, htmlAttributes: new { attribute = text, })
                .ToHtmlString();

            // Assert
            Assert.Equal(
                @"<input attribute=""" + encodedText + @""" id=""name"" name=""name"" type=""radio"" value="""" />",
                result);
        }

        [Theory]
        [PropertyData("AttributeEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void RadioButtonFor_AttributeEncodes_Prefix(string text, bool htmlEncode, string encodedText)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            viewData.ModelMetadata.HtmlEncode = htmlEncode;
            viewData.TemplateInfo.HtmlFieldPrefix = text;
            var helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            // htmlAttributes included only to avoid special-cased renaming done for id attribute.
            var result = helper.RadioButtonFor(m => m, value: String.Empty, htmlAttributes: new { id = "id", })
                .ToHtmlString();

            // Assert
            Assert.Equal(@"<input id=""id"" name=""" + encodedText + @""" type=""radio"" value="""" />", result);
        }

        [Theory]
        [PropertyData("AttributeEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void RadioButtonFor_AttributeEncodes_Value(
            string text,
            bool htmlEncode,
            string encodedText)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            viewData.ModelMetadata.HtmlEncode = htmlEncode;
            viewData.TemplateInfo.HtmlFieldPrefix = "name";
            var helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            var result = helper.RadioButtonFor(m => m, value: text).ToHtmlString();

            // Assert
            Assert.Equal(
                @"<input id=""name"" name=""name"" type=""radio"" value=""" + encodedText + @""" />",
                result);
        }

        // TextBox

        [Fact]
        public void TextBoxDictionaryOverridesImplicitValues()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetTextBoxViewData());

            // Act
            MvcHtmlString html = helper.TextBox("foo", "DefaultFoo", new { type = "fooType" });

            // Assert
            Assert.Equal(@"<input id=""foo"" name=""foo"" type=""fooType"" value=""DefaultFoo"" />", html.ToHtmlString());
        }

        [Fact]
        public void TextBoxExplicitParametersOverrideDictionaryValues()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetTextBoxViewData());

            // Act
            MvcHtmlString html = helper.TextBox("foo", "DefaultFoo", new { value = "Some other value" });

            // Assert
            Assert.Equal(@"<input id=""foo"" name=""foo"" type=""text"" value=""DefaultFoo"" />", html.ToHtmlString());
        }

        [Fact]
        public void TextBoxWithDotReplacementForId()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetTextBoxViewData());

            // Act
            MvcHtmlString html = helper.TextBox("foo.bar.baz", null);

            // Assert
            Assert.Equal(@"<input id=""foo_bar_baz"" name=""foo.bar.baz"" type=""text"" value="""" />", html.ToHtmlString());
        }

        [Fact]
        public void TextBoxWithEmptyNameThrows()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetTextBoxViewData());

            // Act & Assert
            Assert.ThrowsArgumentNullOrEmpty(
                delegate { helper.TextBox(String.Empty); },
                "name");
        }

        [Fact]
        public void TextBoxWithExplicitValue()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetTextBoxViewData());

            // Act
            MvcHtmlString html = helper.TextBox("foo", "DefaultFoo", (object)null);

            // Assert
            Assert.Equal(@"<input id=""foo"" name=""foo"" type=""text"" value=""DefaultFoo"" />", html.ToHtmlString());
        }

        [Fact]
        public void TextBoxWithExplicitValue_Unobtrusive()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetTextBoxViewData());
            helper.ViewContext.ClientValidationEnabled = true;
            helper.ViewContext.UnobtrusiveJavaScriptEnabled = true;
            helper.ViewContext.FormContext = new FormContext();
            helper.ClientValidationRuleFactory = (name, metadata) => new[] { new ModelClientValidationRule { ValidationType = "type", ErrorMessage = "error" } };

            // Act
            MvcHtmlString html = helper.TextBox("foo", "DefaultFoo", (object)null);

            // Assert
            Assert.Equal(@"<input data-val=""true"" data-val-type=""error"" id=""foo"" name=""foo"" type=""text"" value=""DefaultFoo"" />", html.ToHtmlString());
        }

        [Fact]
        public void TextBoxWithExplicitValueAndAttributesDictionary()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetTextBoxViewData());

            // Act
            MvcHtmlString html = helper.TextBox("foo", "DefaultFoo", _attributesDictionary);

            // Assert
            Assert.Equal(@"<input baz=""BazValue"" id=""foo"" name=""foo"" type=""text"" value=""DefaultFoo"" />", html.ToHtmlString());
        }

        [Fact]
        public void TextBoxWithExplicitValueAndAttributesObject()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetTextBoxViewData());

            // Act
            MvcHtmlString html = helper.TextBox("foo", "DefaultFoo", _attributesObjectDictionary);

            // Assert
            Assert.Equal(@"<input baz=""BazObjValue"" id=""foo"" name=""foo"" type=""text"" value=""DefaultFoo"" />", html.ToHtmlString());
        }

        [Fact]
        public void TextBoxWithExplicitValueNull()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetTextBoxViewData());

            // Act
            MvcHtmlString html = helper.TextBox("foo", (string)null /* value */, (object)null);

            // Assert
            Assert.Equal(@"<input id=""foo"" name=""foo"" type=""text"" value=""ViewDataFoo"" />", html.ToHtmlString());
        }

        [Fact]
        public void TextBoxWithImplicitValue()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetTextBoxViewData());

            // Act
            MvcHtmlString html = helper.TextBox("foo");

            // Assert
            Assert.Equal(@"<input id=""foo"" name=""foo"" type=""text"" value=""ViewDataFoo"" />", html.ToHtmlString());
        }

        [Fact]
        public void TextBoxWithImplicitValueAndAttributesDictionary()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetTextBoxViewData());

            // Act
            MvcHtmlString html = helper.TextBox("foo", null, _attributesDictionary);

            // Assert
            Assert.Equal(@"<input baz=""BazValue"" id=""foo"" name=""foo"" type=""text"" value=""ViewDataFoo"" />", html.ToHtmlString());
        }

        [Fact]
        public void TextBoxWithImplicitValueAndAttributesDictionaryReturnsEmptyValueIfNotFound()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetTextBoxViewData());

            // Act
            MvcHtmlString html = helper.TextBox("keyNotFound", null, _attributesDictionary);

            // Assert
            Assert.Equal(@"<input baz=""BazValue"" id=""keyNotFound"" name=""keyNotFound"" type=""text"" value="""" />", html.ToHtmlString());
        }

        [Fact]
        public void TextBoxWithImplicitValueAndAttributesObject()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetTextBoxViewData());

            // Act
            MvcHtmlString html = helper.TextBox("foo", null, _attributesObjectDictionary);

            // Assert
            Assert.Equal(@"<input baz=""BazObjValue"" id=""foo"" name=""foo"" type=""text"" value=""ViewDataFoo"" />", html.ToHtmlString());
        }

        [Fact]
        public void TextBoxWithImplicitValueAndAttributesObjectWithUnderscores()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetTextBoxViewData());

            // Act
            MvcHtmlString html = helper.TextBox("foo", null, _attributesObjectUnderscoresDictionary);

            // Assert
            Assert.Equal(@"<input foo-baz=""BazObjValue"" id=""foo"" name=""foo"" type=""text"" value=""ViewDataFoo"" />", html.ToHtmlString());
        }

        [Fact]
        public void TextBoxWithNullNameThrows()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetTextBoxViewData());

            // Act & Assert
            Assert.ThrowsArgumentNullOrEmpty(
                delegate { helper.TextBox(null /* name */); },
                "name");
        }

        [Fact]
        public void TextBoxWithNameAndValue()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetHiddenViewData());

            // Act
            MvcHtmlString html = helper.TextBox("foo", "fooValue");

            // Assert
            Assert.Equal(@"<input id=""foo"" name=""foo"" type=""text"" value=""fooValue"" />", html.ToHtmlString());
        }

        [Fact]
        public void TextBoxWithPrefix()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetHiddenViewData());
            helper.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = "MyPrefix";

            // Act
            MvcHtmlString html = helper.TextBox("foo", "fooValue");

            // Assert
            Assert.Equal(@"<input id=""MyPrefix_foo"" name=""MyPrefix.foo"" type=""text"" value=""fooValue"" />", html.ToHtmlString());
        }

        [Fact]
        public void TextBoxWithPrefixAndEmptyName()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetHiddenViewData());
            helper.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = "MyPrefix";

            // Act
            MvcHtmlString html = helper.TextBox("", "fooValue");

            // Assert
            Assert.Equal(@"<input id=""MyPrefix"" name=""MyPrefix"" type=""text"" value=""fooValue"" />", html.ToHtmlString());
        }

        [Fact]
        public void TextBoxWithViewDataErrors()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetTextBoxViewDataWithErrors());

            // Act
            MvcHtmlString html = helper.TextBox("foo", null, _attributesObjectDictionary);

            // Assert
            Assert.Equal(@"<input baz=""BazObjValue"" class=""input-validation-error"" id=""foo"" name=""foo"" type=""text"" value=""AttemptedValueFoo"" />", html.ToHtmlString());
        }

        [Fact]
        public void TextBoxWithViewDataErrorsAndCustomClass()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetTextBoxViewDataWithErrors());

            // Act
            MvcHtmlString html = helper.TextBox("foo", null, new { @class = "foo-class" });

            // Assert
            Assert.Equal(@"<input class=""input-validation-error foo-class"" id=""foo"" name=""foo"" type=""text"" value=""AttemptedValueFoo"" />", html.ToHtmlString());
        }

        [Theory]
        [PropertyData("AttributeEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void TextBox_AttributeEncodes_AddedHtmlAttributes(
            string text,
            bool htmlEncode,
            string encodedText)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            viewData.ModelMetadata.HtmlEncode = htmlEncode;
            var helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            var result = helper.TextBox(name: "name", value: null, htmlAttributes: new { attribute = text, })
                .ToHtmlString();

            // Assert
            Assert.Equal(
                @"<input attribute=""" + encodedText + @""" id=""name"" name=""name"" type=""text"" value="""" />",
                result);
        }

        [Theory]
        [PropertyData("AttributeEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void TextBox_AttributeEncodes_Format(
            string text,
            bool htmlEncode,
            string encodedText)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            viewData.ModelMetadata.HtmlEncode = htmlEncode;
            var helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            var result = helper.TextBox("name", value: String.Empty, format: text).ToHtmlString();

            // Assert
            Assert.Equal(
                @"<input id=""name"" name=""name"" type=""text"" value=""" + encodedText + @""" />",
                result);
        }

        [Theory]
        [PropertyData("AttributeEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void TextBox_AttributeEncodes_Name(string text, bool htmlEncode, string encodedText)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            viewData.ModelMetadata.HtmlEncode = htmlEncode;
            var helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            // htmlAttributes included only to avoid special-cased renaming done for id attribute.
            var result = helper.TextBox(text, value: null, htmlAttributes: new { id = "id", }).ToHtmlString();

            // Assert
            Assert.Equal(@"<input id=""id"" name=""" + encodedText + @""" type=""text"" value="""" />", result);
        }

        [Theory]
        [PropertyData("AttributeEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void TextBox_AttributeEncodes_Prefix(string text, bool htmlEncode, string encodedText)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            viewData.ModelMetadata.HtmlEncode = htmlEncode;
            viewData.TemplateInfo.HtmlFieldPrefix = text;
            var helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            // htmlAttributes included only to avoid special-cased renaming done for id attribute.
            var result = helper.TextBox(name: String.Empty, value: null, htmlAttributes: new { id = "id", })
                .ToHtmlString();

            // Assert
            Assert.Equal(@"<input id=""id"" name=""" + encodedText + @""" type=""text"" value="""" />", result);
        }

        [Theory]
        [PropertyData("AttributeEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void TextBox_AttributeEncodes_Value(
            string text,
            bool htmlEncode,
            string encodedText)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            viewData.ModelMetadata.HtmlEncode = htmlEncode;
            var helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            var result = helper.TextBox("name", value: text).ToHtmlString();

            // Assert
            Assert.Equal(
                @"<input id=""name"" name=""name"" type=""text"" value=""" + encodedText + @""" />",
                result);
        }

        // TextBoxFor

        [Fact]
        public void TextBoxForWithNullExpressionThrows()
        {
            // Arrange
            HtmlHelper<FooBarModel> helper = MvcHelper.GetHtmlHelper(GetTextBoxViewData());

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => helper.TextBoxFor<FooBarModel, object>(null /* expression */),
                "expression"
                );
        }

        [Fact]
        public void TextBoxForWithSimpleExpression()
        {
            // Arrange
            HtmlHelper<FooBarModel> helper = MvcHelper.GetHtmlHelper(GetTextBoxViewData());

            // Act
            MvcHtmlString html = helper.TextBoxFor(m => m.foo);

            // Assert
            Assert.Equal(@"<input id=""foo"" name=""foo"" type=""text"" value=""ViewItemFoo"" />", html.ToHtmlString());
        }

        [Fact]
        public void TextBoxForWithSimpleExpression_Unobtrusive()
        {
            // Arrange
            HtmlHelper<FooBarModel> helper = MvcHelper.GetHtmlHelper(GetTextBoxViewData());
            helper.ViewContext.ClientValidationEnabled = true;
            helper.ViewContext.UnobtrusiveJavaScriptEnabled = true;
            helper.ViewContext.FormContext = new FormContext();
            helper.ClientValidationRuleFactory = (name, metadata) => new[] { new ModelClientValidationRule { ValidationType = "type", ErrorMessage = "error" } };

            // Act
            MvcHtmlString html = helper.TextBoxFor(m => m.foo);

            // Assert
            Assert.Equal(@"<input data-val=""true"" data-val-type=""error"" id=""foo"" name=""foo"" type=""text"" value=""ViewItemFoo"" />", html.ToHtmlString());
        }

        [Fact]
        public void TextBoxForWithAttributesDictionary()
        {
            // Arrange
            HtmlHelper<FooBarModel> helper = MvcHelper.GetHtmlHelper(GetTextBoxViewData());

            // Act
            MvcHtmlString html = helper.TextBoxFor(m => m.foo, _attributesDictionary);

            // Assert
            Assert.Equal(@"<input baz=""BazValue"" id=""foo"" name=""foo"" type=""text"" value=""ViewItemFoo"" />", html.ToHtmlString());
        }

        [Fact]
        public void TextBoxForWithAttributesObject()
        {
            // Arrange
            HtmlHelper<FooBarModel> helper = MvcHelper.GetHtmlHelper(GetTextBoxViewData());

            // Act
            MvcHtmlString html = helper.TextBoxFor(m => m.foo, _attributesObjectDictionary);

            // Assert
            Assert.Equal(@"<input baz=""BazObjValue"" id=""foo"" name=""foo"" type=""text"" value=""ViewItemFoo"" />", html.ToHtmlString());
        }

        [Fact]
        public void TextBoxForWithAttributesObjectWithUnderscores()
        {
            // Arrange
            HtmlHelper<FooBarModel> helper = MvcHelper.GetHtmlHelper(GetTextBoxViewData());

            // Act
            MvcHtmlString html = helper.TextBoxFor(m => m.foo, _attributesObjectUnderscoresDictionary);

            // Assert
            Assert.Equal(@"<input foo-baz=""BazObjValue"" id=""foo"" name=""foo"" type=""text"" value=""ViewItemFoo"" />", html.ToHtmlString());
        }

        [Fact]
        public void TextBoxForWithPrefix()
        {
            // Arrange
            HtmlHelper<FooBarModel> helper = MvcHelper.GetHtmlHelper(GetTextBoxViewData());
            helper.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = "MyPrefix";

            // Act
            MvcHtmlString html = helper.TextBoxFor(m => m.foo);

            // Assert
            Assert.Equal(@"<input id=""MyPrefix_foo"" name=""MyPrefix.foo"" type=""text"" value=""ViewItemFoo"" />", html.ToHtmlString());
        }

        [Fact]
        public void TextBoxForWithPrefixAndEmptyName()
        {
            // Arrange
            HtmlHelper<FooBarModel> helper = MvcHelper.GetHtmlHelper(GetTextBoxViewData());
            helper.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = "MyPrefix";

            // Act
            MvcHtmlString html = helper.TextBoxFor(m => m);

            // Assert
            Assert.Equal(@"<input id=""MyPrefix"" name=""MyPrefix"" type=""text"" value=""{ foo = ViewItemFoo, bar = ViewItemBar }"" />", html.ToHtmlString());
        }

        [Fact]
        public void TextBoxForWithErrors()
        {
            // Arrange
            HtmlHelper<FooBarModel> helper = MvcHelper.GetHtmlHelper(GetTextBoxViewDataWithErrors());

            // Act
            MvcHtmlString html = helper.TextBoxFor(m => m.foo, _attributesObjectDictionary);

            // Assert
            Assert.Equal(@"<input baz=""BazObjValue"" class=""input-validation-error"" id=""foo"" name=""foo"" type=""text"" value=""AttemptedValueFoo"" />", html.ToHtmlString());
        }

        [Fact]
        public void TextBoxForWithErrorsAndCustomClass()
        {
            // Arrange
            HtmlHelper<FooBarModel> helper = MvcHelper.GetHtmlHelper(GetTextBoxViewDataWithErrors());

            // Act
            MvcHtmlString html = helper.TextBoxFor(m => m.foo, new { @class = "foo-class" });

            // Assert
            Assert.Equal(@"<input class=""input-validation-error foo-class"" id=""foo"" name=""foo"" type=""text"" value=""AttemptedValueFoo"" />", html.ToHtmlString());
        }

        [Fact]
        [ReplaceCulture]
        public void TextBoxHelpersFormatValue()
        {
            // Arrange
            DateTime dt = new DateTime(1900, 1, 1, 0, 0, 0);
            HtmlHelper helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary { { "viewDataDate", dt } });

            ViewDataDictionary<DateModel> viewData = new ViewDataDictionary<DateModel>() { Model = new DateModel { date = dt } };
            HtmlHelper<DateModel> dateModelhelper = MvcHelper.GetHtmlHelper(viewData);

            var tests = new[]
            {
                // TextBox(name, value, format)
                new
                {
                    Html = @"<input id=""viewDataDate"" name=""viewDataDate"" type=""text"" value=""-01/01/1900 00:00:00-"" />",
                    Action = new Func<MvcHtmlString>(() => helper.TextBox("viewDataDate", null, "-{0}-"))
                },
                // TextBox(name, value, format)
                new
                {
                    Html = @"<input id=""date"" name=""date"" type=""text"" value=""-01/01/1900 00:00:00-"" />",
                    Action = new Func<MvcHtmlString>(() => helper.TextBox("date", dt, "-{0}-"))
                },
                // TextBox(name, value, format, hmtlAttributes)
                new
                {
                    Html = @"<input id=""date"" name=""date"" type=""text"" value=""-01/01/1900 00:00:00-"" />",
                    Action = new Func<MvcHtmlString>(() => helper.TextBox("date", dt, "-{0}-", (object)null))
                },
                // TextBox(name, value, format, hmtlAttributes)
                new
                {
                    Html = @"<input id=""date"" name=""date"" type=""text"" value=""-01/01/1900 00:00:00-"" />",
                    Action = new Func<MvcHtmlString>(() => helper.TextBox("date", dt, "-{0}-", new RouteValueDictionary()))
                },
                // TextBoxFor(expression, format)
                new
                {
                    Html = @"<input id=""date"" name=""date"" type=""text"" value=""-01/01/1900 00:00:00-"" />",
                    Action = new Func<MvcHtmlString>(() => dateModelhelper.TextBoxFor(m => m.date, "-{0}-"))
                },
                // TextBoxFor(expression, format, hmtlAttributes)
                new
                {
                    Html = @"<input id=""date"" name=""date"" type=""text"" value=""-01/01/1900 00:00:00-"" />",
                    Action = new Func<MvcHtmlString>(() => dateModelhelper.TextBoxFor(m => m.date, "-{0}-", (object)null))
                },
                // TextBoxFor(expression, format, hmtlAttributes)
                new
                {
                    Html = @"<input id=""date"" name=""date"" type=""text"" value=""-01/01/1900 00:00:00-"" />",
                    Action = new Func<MvcHtmlString>(() => dateModelhelper.TextBoxFor(m => m.date, "-{0}-", new RouteValueDictionary()))
                }
            };

            // Act && Assert
            foreach (var test in tests)
            {
                Assert.Equal(test.Html, test.Action().ToHtmlString());
            }
        }

        [Theory]
        [PropertyData("AttributeEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void TextBoxFor_AttributeEncodes_AddedHtmlAttributes(
            string text,
            bool htmlEncode,
            string encodedText)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            viewData.ModelMetadata.HtmlEncode = htmlEncode;
            viewData.TemplateInfo.HtmlFieldPrefix = "name";
            var helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            var result = helper.TextBoxFor(m => m, htmlAttributes: new { attribute = text, }).ToHtmlString();

            // Assert
            Assert.Equal(
                @"<input attribute=""" + encodedText + @""" id=""name"" name=""name"" type=""text"" value="""" />",
                result);
        }

        [Theory]
        [PropertyData("AttributeEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void TextBoxFor_AttributeEncodes_Format(
            string text,
            bool htmlEncode,
            string encodedText)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: String.Empty);
            viewData.ModelMetadata.HtmlEncode = htmlEncode;
            viewData.TemplateInfo.HtmlFieldPrefix = "name";
            var helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            var result = helper.TextBoxFor(m => m, format: text).ToHtmlString();

            // Assert
            Assert.Equal(
                @"<input id=""name"" name=""name"" type=""text"" value=""" + encodedText + @""" />",
                result);
        }

        [Theory]
        [PropertyData("AttributeEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void TextBoxFor_AttributeEncodes_Prefix(string text, bool htmlEncode, string encodedText)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            viewData.ModelMetadata.HtmlEncode = htmlEncode;
            viewData.TemplateInfo.HtmlFieldPrefix = text;
            var helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            // htmlAttributes included only to avoid special-cased renaming done for id attribute.
            var result = helper.TextBoxFor(m => m, htmlAttributes: new { id = "id", }).ToHtmlString();

            // Assert
            Assert.Equal(@"<input id=""id"" name=""" + encodedText + @""" type=""text"" value="""" />", result);
        }

        [Theory]
        [PropertyData("AttributeEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void TextBoxFor_AttributeEncodes_Value(
            string text,
            bool htmlEncode,
            string encodedText)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            viewData.ModelMetadata.HtmlEncode = htmlEncode;
            var helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            var result = helper.TextBoxFor(m => text).ToHtmlString();

            // Assert
            Assert.Equal(
                @"<input id=""text"" name=""text"" type=""text"" value=""" + encodedText + @""" />",
                result);
        }

        // MODELS
        private class FooModel
        {
            public string foo { get; set; }

            public override string ToString()
            {
                return String.Format("{{ foo = {0} }}", foo ?? "(null)");
            }
        }

        private class FooBarModel : FooModel
        {
            public string bar { get; set; }

            public override string ToString()
            {
                return String.Format("{{ foo = {0}, bar = {1} }}", foo ?? "(null)", bar ?? "(null)");
            }
        }

        private class FooBarBazModel
        {
            public bool foo { get; set; }
            public bool bar { get; set; }
            public bool baz { get; set; }

            public override string ToString()
            {
                return String.Format("{{ foo = {0}, bar = {1}, baz = {2} }}", foo, bar, baz);
            }
        }

        private class ShallowModel
        {
            [Required]
            public string foo { get; set; }
        }

        private class DeepContainerModel
        {
            public ShallowModel contained { get; set; }
        }

        private class HiddenModel : FooModel
        {
            public byte[] bytes { get; set; }
            public Binary binary { get; set; }
        }

        private class DateModel
        {
            public DateTime date { get; set; }
        }

        // CHECKBOX
        private static ViewDataDictionary<FooBarBazModel> GetCheckBoxViewData()
        {
            ViewDataDictionary<FooBarBazModel> viewData = new ViewDataDictionary<FooBarBazModel> { { "foo", true }, { "bar", "NotTrue" }, { "baz", false } };
            return viewData;
        }

        // HIDDEN
        private static ViewDataDictionary<HiddenModel> GetHiddenViewData()
        {
            return new ViewDataDictionary<HiddenModel>(new HiddenModel()) { { "foo", "ViewDataFoo" } };
        }

        private static ViewDataDictionary<HiddenModel> GetHiddenViewDataWithErrors()
        {
            ViewDataDictionary<HiddenModel> viewData = new ViewDataDictionary<HiddenModel> { { "foo", "ViewDataFoo" } };
            viewData.Model = new HiddenModel();
            ModelState modelStateFoo = new ModelState();
            modelStateFoo.Errors.Add(new ModelError("foo error 1"));
            modelStateFoo.Errors.Add(new ModelError("foo error 2"));
            viewData.ModelState["foo"] = modelStateFoo;
            modelStateFoo.Value = HtmlHelperTest.GetValueProviderResult("AttemptedValueFoo", "AttemptedValueFoo");

            return viewData;
        }

        // PASSWORD
        private static ViewDataDictionary<FooModel> GetPasswordViewData()
        {
            return new ViewDataDictionary<FooModel> { { "foo", "ViewDataFoo" } };
        }

        private static ViewDataDictionary<FooModel> GetPasswordViewDataWithErrors()
        {
            ViewDataDictionary<FooModel> viewData = new ViewDataDictionary<FooModel> { { "foo", "ViewDataFoo" } };
            ModelState modelStateFoo = new ModelState();
            modelStateFoo.Errors.Add(new ModelError("foo error 1"));
            modelStateFoo.Errors.Add(new ModelError("foo error 2"));
            viewData.ModelState["foo"] = modelStateFoo;
            modelStateFoo.Value = HtmlHelperTest.GetValueProviderResult("AttemptedValueFoo", "AttemptedValueFoo");

            return viewData;
        }

        // RADIO
        private static ViewDataDictionary<FooBarModel> GetRadioButtonViewData()
        {
            ViewDataDictionary<FooBarModel> viewData = new ViewDataDictionary<FooBarModel> { { "foo", "ViewDataFoo" } };
            viewData.Model = new FooBarModel { foo = "ViewItemFoo", bar = "ViewItemBar" };
            ModelState modelState = new ModelState();
            modelState.Value = HtmlHelperTest.GetValueProviderResult("ViewDataFoo", "ViewDataFoo");
            viewData.ModelState["foo"] = modelState;

            return viewData;
        }

        private static ViewDataDictionary<string> GetRadioButtonNestedAndUnsetViewData()
        {
            ViewDataDictionary<FooBarModel> viewData = new ViewDataDictionary<FooBarModel> { };
            viewData.Model = new FooBarModel { foo = "ViewItemFoo", bar = "ViewItemBar" };


            Expression<Func<FooBarModel, string>> containedExpression = m => m.foo;

            var metadata = ModelMetadata.FromLambdaExpression(containedExpression, viewData);
            var htmlFieldName = ExpressionHelper.GetExpressionText(containedExpression);


            ViewDataDictionary nestedViewData = new ViewDataDictionary(viewData)
            {
                Model = metadata.Model,
                ModelMetadata = metadata,
                TemplateInfo = new TemplateInfo
                {
                    HtmlFieldPrefix = viewData.TemplateInfo.GetFullHtmlFieldName(htmlFieldName),
                }
            };

            return new ViewDataDictionary<string>(nestedViewData);
        }

        // TEXTBOX
        private static readonly RouteValueDictionary _attributesDictionary = new RouteValueDictionary(new { baz = "BazValue" });
        private static readonly object _attributesObjectDictionary = new { baz = "BazObjValue" };
        private static readonly object _attributesObjectUnderscoresDictionary = new { foo_baz = "BazObjValue" };

        private static ViewDataDictionary<FooBarModel> GetTextBoxViewData()
        {
            ViewDataDictionary<FooBarModel> viewData = new ViewDataDictionary<FooBarModel> { { "foo", "ViewDataFoo" } };
            viewData.Model = new FooBarModel { foo = "ViewItemFoo", bar = "ViewItemBar" };

            return viewData;
        }

        private static ViewDataDictionary<FooBarModel> GetTextBoxViewDataWithErrors()
        {
            ViewDataDictionary<FooBarModel> viewData = new ViewDataDictionary<FooBarModel> { { "foo", "ViewDataFoo" } };
            viewData.Model = new FooBarModel { foo = "ViewItemFoo", bar = "ViewItemBar" };
            ModelState modelStateFoo = new ModelState();
            modelStateFoo.Errors.Add(new ModelError("foo error 1"));
            modelStateFoo.Errors.Add(new ModelError("foo error 2"));
            viewData.ModelState["foo"] = modelStateFoo;
            modelStateFoo.Value = HtmlHelperTest.GetValueProviderResult(new string[] { "AttemptedValueFoo" }, "AttemptedValueFoo");

            return viewData;
        }

        public void Dispose()
        {
            // Reset ScopeStorage (written via e.g. ViewContext.ClientValidationEnabled) between tests to avoid unexpected interactions.
            ScopeStorage.CurrentProvider = new StaticScopeStorageProvider();
            ScopeStorage.GlobalScope.Clear();
        }
    }
}
