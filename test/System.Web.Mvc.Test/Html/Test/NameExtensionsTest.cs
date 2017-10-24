// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.TestCommon;
using Microsoft.Web.UnitTestUtil;

namespace System.Web.Mvc.Html.Test
{
    public class NameExtensionsTest
    {
        [Fact]
        public void NonStronglyTypedWithNoPrefix()
        {
            // Arrange
            HtmlHelper html = MvcHelper.GetHtmlHelper(new ViewDataDictionary());

            // Act & Assert
            Assert.Equal("", html.IdForModel().ToHtmlString());
            Assert.Equal("foo", html.Id("foo").ToHtmlString());
            Assert.Equal("foo_bar", html.Id("foo.bar").ToHtmlString());
            Assert.Equal(String.Empty, html.Id("<script>alert(\"XSS!\")</script>").ToHtmlString());

            Assert.Equal("", html.NameForModel().ToHtmlString());
            Assert.Equal("foo", html.Name("foo").ToHtmlString());
            Assert.Equal("foo.bar", html.Name("foo.bar").ToHtmlString());
            Assert.Equal("&lt;script>alert(&quot;XSS!&quot;)&lt;/script>", html.Name("<script>alert(\"XSS!\")</script>").ToHtmlString());
        }

        [Fact]
        public void NonStronglyTypedWithPrefix()
        {
            // Arrange
            HtmlHelper html = MvcHelper.GetHtmlHelper(new ViewDataDictionary());
            html.ViewData.TemplateInfo.HtmlFieldPrefix = "prefix";

            // Act & Assert
            Assert.Equal("prefix", html.IdForModel().ToHtmlString());
            Assert.Equal("prefix_foo", html.Id("foo").ToHtmlString());
            Assert.Equal("prefix_foo_bar", html.Id("foo.bar").ToHtmlString());

            Assert.Equal("prefix", html.NameForModel().ToHtmlString());
            Assert.Equal("prefix.foo", html.Name("foo").ToHtmlString());
            Assert.Equal("prefix.foo.bar", html.Name("foo.bar").ToHtmlString());
        }

        [Fact]
        public void StronglyTypedWithNoPrefix()
        {
            // Arrange
            HtmlHelper<OuterClass> html = MvcHelper.GetHtmlHelper(new ViewDataDictionary<OuterClass>());

            // Act & Assert
            Assert.Equal("IntValue", html.IdFor(m => m.IntValue).ToHtmlString());
            Assert.Equal("Inner_StringValue", html.IdFor(m => m.Inner.StringValue).ToHtmlString());

            Assert.Equal("IntValue", html.NameFor(m => m.IntValue).ToHtmlString());
            Assert.Equal("Inner.StringValue", html.NameFor(m => m.Inner.StringValue).ToHtmlString());
        }

        [Fact]
        public void StronglyTypedWithPrefix()
        {
            // Arrange
            HtmlHelper<OuterClass> html = MvcHelper.GetHtmlHelper(new ViewDataDictionary<OuterClass>());
            html.ViewData.TemplateInfo.HtmlFieldPrefix = "prefix";

            // Act & Assert
            Assert.Equal("prefix_IntValue", html.IdFor(m => m.IntValue).ToHtmlString());
            Assert.Equal("prefix_Inner_StringValue", html.IdFor(m => m.Inner.StringValue).ToHtmlString());

            Assert.Equal("prefix.IntValue", html.NameFor(m => m.IntValue).ToHtmlString());
            Assert.Equal("prefix.Inner.StringValue", html.NameFor(m => m.Inner.StringValue).ToHtmlString());
        }

        // Regression test for codeplex #554 - editor templates for collections would add an extra dot to the
        // generated name.
        [Fact]
        public void StronglyTypedCollectionWithPrefix()
        {
            // Arrange
            HtmlHelper<List<OuterClass>> html = MvcHelper.GetHtmlHelper(new ViewDataDictionary<List<OuterClass>>());
            html.ViewData.TemplateInfo.HtmlFieldPrefix = "prefix";

            // Act & Assert
            Assert.Equal("prefix_0__IntValue", html.IdFor(m => m[0].IntValue).ToHtmlString());
            Assert.Equal("prefix_0__Inner_StringValue", html.IdFor(m => m[0].Inner.StringValue).ToHtmlString());

            Assert.Equal("prefix[0].IntValue", html.NameFor(m => m[0].IntValue).ToHtmlString());
            Assert.Equal("prefix[0].Inner.StringValue", html.NameFor(m => m[0].Inner.StringValue).ToHtmlString());
        }

        [Theory]
        [PropertyData("IdEncodedData_NoHtmlEncode", PropertyType = typeof(EncodedDataSets))]
        public void Id_IdEncodes_PropertyName(string text, string expectedText)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            var helper = MvcHelper.GetHtmlHelper<string>(viewData);

            // Act
            var result = helper.Id(text);

            // Assert
            Assert.Equal(expectedText, result.ToHtmlString());
        }

        [Theory]
        [PropertyData("IdEncodedData_NoHtmlEncode", PropertyType = typeof(EncodedDataSets))]
        public void IdHelpers_IdEncode_Prefix(string text, string expectedText)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            viewData.TemplateInfo.HtmlFieldPrefix = text;
            var helper = MvcHelper.GetHtmlHelper<string>(viewData);

            // Act
            var idResult = helper.Id("");
            var idForResult = helper.IdFor(m => m);
            var idForModelResult = helper.IdForModel();


            // Assert
            Assert.Equal(expectedText, idResult.ToHtmlString());
            Assert.Equal(expectedText, idForResult.ToHtmlString());
            Assert.Equal(expectedText, idForModelResult.ToHtmlString());
        }

        [Theory]
        [PropertyData("AttributeEncodedData_NoHtmlEncode", PropertyType = typeof(EncodedDataSets))]
        public void Name_AttributeEncodes_PropertyName(string text, string expectedText)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            var helper = MvcHelper.GetHtmlHelper<string>(viewData);

            // Act
            var result = helper.Name(text);

            // Assert
            Assert.Equal(expectedText, result.ToHtmlString());
        }

        [Theory]
        [PropertyData("AttributeEncodedData_NoHtmlEncode", PropertyType = typeof(EncodedDataSets))]
        public void NameHelpers_AttributeEncode_Prefix(string text, string expectedText)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            viewData.TemplateInfo.HtmlFieldPrefix = text;
            var helper = MvcHelper.GetHtmlHelper<string>(viewData);

            // Act
            var nameResult = helper.Name("");
            var nameForResult = helper.NameFor(m => m);
            var nameForModelResult = helper.NameForModel();


            // Assert
            Assert.Equal(expectedText, nameResult.ToHtmlString());
            Assert.Equal(expectedText, nameForResult.ToHtmlString());
            Assert.Equal(expectedText, nameForModelResult.ToHtmlString());
        }

        private sealed class OuterClass
        {
            public InnerClass Inner { get; set; }
            public int IntValue { get; set; }
        }

        private sealed class InnerClass
        {
            public string StringValue { get; set; }
        }
    }
}
