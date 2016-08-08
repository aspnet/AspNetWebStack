// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.TestCommon;
using Microsoft.Web.UnitTestUtil;

namespace Microsoft.Web.Mvc.Test
{
    public class RadioExtensionsTest
    {
        [Fact]
        public void RadioButtonListNothingSelected()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper(new ViewDataDictionary());

            // Act
            MvcHtmlString[] html = htmlHelper.RadioButtonList("FooList", GetRadioButtonListData(false));

            // Assert
            Assert.Equal(@"<input id=""FooList"" name=""FooList"" type=""radio"" value=""foo"" />", html[0].ToHtmlString());
            Assert.Equal(@"<input id=""FooList"" name=""FooList"" type=""radio"" value=""bar"" />", html[1].ToHtmlString());
            Assert.Equal(@"<input id=""FooList"" name=""FooList"" type=""radio"" value=""baz"" />", html[2].ToHtmlString());
        }

        [Fact]
        public void RadioButtonListItemSelected()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper(new ViewDataDictionary());

            // Act
            MvcHtmlString[] html = htmlHelper.RadioButtonList("FooList", GetRadioButtonListData(true));

            // Assert
            Assert.Equal(@"<input id=""FooList"" name=""FooList"" type=""radio"" value=""foo"" />", html[0].ToHtmlString());
            Assert.Equal(@"<input id=""FooList"" name=""FooList"" type=""radio"" value=""bar"" />", html[1].ToHtmlString());
            Assert.Equal(@"<input checked=""checked"" id=""FooList"" name=""FooList"" type=""radio"" value=""baz"" />", html[2].ToHtmlString());
        }

        [Fact]
        public void RadioButtonListItemSelectedWithValueFromViewData()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper(new ViewDataDictionary(new { foolist = "bar" }));

            // Act
            MvcHtmlString[] html = htmlHelper.RadioButtonList("FooList", GetRadioButtonListData(false));

            // Assert
            Assert.Equal(@"<input id=""FooList"" name=""FooList"" type=""radio"" value=""foo"" />", html[0].ToHtmlString());
            Assert.Equal(@"<input checked=""checked"" id=""FooList"" name=""FooList"" type=""radio"" value=""bar"" />", html[1].ToHtmlString());
            Assert.Equal(@"<input id=""FooList"" name=""FooList"" type=""radio"" value=""baz"" />", html[2].ToHtmlString());
        }

        [Fact]
        public void RadioButtonListWithObjectAttributes()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper(new ViewDataDictionary());

            // Act
            MvcHtmlString[] html = htmlHelper.RadioButtonList("FooList", GetRadioButtonListData(true), new { attr1 = "value1" });

            // Assert
            Assert.Equal(@"<input attr1=""value1"" id=""FooList"" name=""FooList"" type=""radio"" value=""foo"" />", html[0].ToHtmlString());
            Assert.Equal(@"<input attr1=""value1"" id=""FooList"" name=""FooList"" type=""radio"" value=""bar"" />", html[1].ToHtmlString());
            Assert.Equal(@"<input attr1=""value1"" checked=""checked"" id=""FooList"" name=""FooList"" type=""radio"" value=""baz"" />", html[2].ToHtmlString());
        }

        [Fact]
        public void RadioButtonListWithObjectAttributesWithUnderscores()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper(new ViewDataDictionary());

            // Act
            MvcHtmlString[] html = htmlHelper.RadioButtonList("FooList", GetRadioButtonListData(true), new { foo_bar = "baz" });

            // Assert
            Assert.Equal(@"<input foo-bar=""baz"" id=""FooList"" name=""FooList"" type=""radio"" value=""foo"" />", html[0].ToHtmlString());
            Assert.Equal(@"<input foo-bar=""baz"" id=""FooList"" name=""FooList"" type=""radio"" value=""bar"" />", html[1].ToHtmlString());
            Assert.Equal(@"<input checked=""checked"" foo-bar=""baz"" id=""FooList"" name=""FooList"" type=""radio"" value=""baz"" />", html[2].ToHtmlString());
        }

        [Fact]
        public void RadioButtonListWithDictionaryAttributes()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper(new ViewDataDictionary());

            // Act
            MvcHtmlString[] html = htmlHelper.RadioButtonList("FooList", GetRadioButtonListData(true), new RouteValueDictionary(new { attr1 = "value1" }));

            // Assert
            Assert.Equal(@"<input attr1=""value1"" id=""FooList"" name=""FooList"" type=""radio"" value=""foo"" />", html[0].ToHtmlString());
            Assert.Equal(@"<input attr1=""value1"" id=""FooList"" name=""FooList"" type=""radio"" value=""bar"" />", html[1].ToHtmlString());
            Assert.Equal(@"<input attr1=""value1"" checked=""checked"" id=""FooList"" name=""FooList"" type=""radio"" value=""baz"" />", html[2].ToHtmlString());
        }

        [Fact]
        public void RadioButtonListNothingSelectedWithSelectListFromViewData()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper(GetRadioButtonListViewData(false));

            // Act
            MvcHtmlString[] html = htmlHelper.RadioButtonList("FooList");

            // Assert
            Assert.Equal(@"<input id=""FooList"" name=""FooList"" type=""radio"" value=""foo"" />", html[0].ToHtmlString());
            Assert.Equal(@"<input id=""FooList"" name=""FooList"" type=""radio"" value=""bar"" />", html[1].ToHtmlString());
            Assert.Equal(@"<input id=""FooList"" name=""FooList"" type=""radio"" value=""baz"" />", html[2].ToHtmlString());
        }

        [Fact]
        public void RadioButtonListItemSelectedWithSelectListFromViewData()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper(GetRadioButtonListViewData(true));

            // Act
            MvcHtmlString[] html = htmlHelper.RadioButtonList("FooList");

            // Assert
            Assert.Equal(@"<input id=""FooList"" name=""FooList"" type=""radio"" value=""foo"" />", html[0].ToHtmlString());
            Assert.Equal(@"<input id=""FooList"" name=""FooList"" type=""radio"" value=""bar"" />", html[1].ToHtmlString());
            Assert.Equal(@"<input checked=""checked"" id=""FooList"" name=""FooList"" type=""radio"" value=""baz"" />", html[2].ToHtmlString());
        }

        [Fact]
        public void RadioButtonListWithObjectAttributesWithSelectListFromViewData()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper(GetRadioButtonListViewData(true));

            // Act
            MvcHtmlString[] html = htmlHelper.RadioButtonList("FooList", new { attr1 = "value1" });

            // Assert
            Assert.Equal(@"<input attr1=""value1"" id=""FooList"" name=""FooList"" type=""radio"" value=""foo"" />", html[0].ToHtmlString());
            Assert.Equal(@"<input attr1=""value1"" id=""FooList"" name=""FooList"" type=""radio"" value=""bar"" />", html[1].ToHtmlString());
            Assert.Equal(@"<input attr1=""value1"" checked=""checked"" id=""FooList"" name=""FooList"" type=""radio"" value=""baz"" />", html[2].ToHtmlString());
        }

        [Fact]
        public void RadioButtonListWithObjectAttributesWithUnderscoresWithSelectListFromViewData()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper(GetRadioButtonListViewData(true));

            // Act
            MvcHtmlString[] html = htmlHelper.RadioButtonList("FooList", new { foo_bar = "baz" });

            // Assert
            Assert.Equal(@"<input foo-bar=""baz"" id=""FooList"" name=""FooList"" type=""radio"" value=""foo"" />", html[0].ToHtmlString());
            Assert.Equal(@"<input foo-bar=""baz"" id=""FooList"" name=""FooList"" type=""radio"" value=""bar"" />", html[1].ToHtmlString());
            Assert.Equal(@"<input checked=""checked"" foo-bar=""baz"" id=""FooList"" name=""FooList"" type=""radio"" value=""baz"" />", html[2].ToHtmlString());
        }

        [Fact]
        public void RadioButtonListWithDictionaryAttributesWithSelectListFromViewData()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper(GetRadioButtonListViewData(true));

            // Act
            MvcHtmlString[] html = htmlHelper.RadioButtonList("FooList", new RouteValueDictionary(new { attr1 = "value1" }));

            // Assert
            Assert.Equal(@"<input attr1=""value1"" id=""FooList"" name=""FooList"" type=""radio"" value=""foo"" />", html[0].ToHtmlString());
            Assert.Equal(@"<input attr1=""value1"" id=""FooList"" name=""FooList"" type=""radio"" value=""bar"" />", html[1].ToHtmlString());
            Assert.Equal(@"<input attr1=""value1"" checked=""checked"" id=""FooList"" name=""FooList"" type=""radio"" value=""baz"" />", html[2].ToHtmlString());
        }

        [Fact]
        public void RadioButtonListWithDictionaryAttributesWithWrongSelectListName()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper(GetRadioButtonListViewData(true));

            // Act / Assert
            Assert.Throws<InvalidOperationException>(() => 
                htmlHelper.RadioButtonList("WrongFooList", new RouteValueDictionary(new { attr1 = "value1" })));
        }

        [Fact]
        public void RadioButtonListWithDictionaryAttributesWithInvalidSelectListFromViewData()
        {
            // Arrange
            ViewDataDictionary viewData = new ViewDataDictionary();
            viewData["FooList"] = "FOOBAR3";
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper(viewData);

            // Act / Assert
            Assert.Throws<InvalidOperationException>(() => 
                htmlHelper.RadioButtonList("FooList", new RouteValueDictionary(new { attr1 = "value1" })));
        }

        [Fact]
        public void RadioButtonListWithDictionaryAttributesWithNullSelectListNameFromSelectList()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper(GetRadioButtonListViewData(true));
            SelectList selectList = GetRadioButtonListData(true);

            // Act / Assert
            Assert.Throws<ArgumentException>(() => 
                htmlHelper.RadioButtonList("", selectList, new RouteValueDictionary(new { attr1 = "value1" })));
        }

        [Fact]
        public void RadioButtonListWithDictionaryAttributesWithSelectListNameAndNullSelectList()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper(GetRadioButtonListViewData(true));

            // Act / Assert
            Assert.Throws<ArgumentNullException>(() => 
                htmlHelper.RadioButtonList("FooBar", null, new RouteValueDictionary(new { attr1 = "value1" })));
        }

        private static SelectList GetRadioButtonListData(bool selectBaz)
        {
            List<RadioItem> list = new List<RadioItem>();
            list.Add(new RadioItem { Text = "text-foo", Value = "foo" });
            list.Add(new RadioItem { Text = "text-bar", Value = "bar" });
            list.Add(new RadioItem { Text = "text-baz", Value = "baz" });
            return new SelectList(list, "value", "TEXT", selectBaz ? "baz" : "something-else");
        }

        private static ViewDataDictionary GetRadioButtonListViewData(bool selectBaz)
        {
            ViewDataDictionary viewData = new ViewDataDictionary();
            viewData["FooList"] = GetRadioButtonListData(selectBaz);
            return viewData;
        }

        private class RadioItem
        {
            public string Text { get; set; }

            public string Value { get; set; }
        }
    }
}
