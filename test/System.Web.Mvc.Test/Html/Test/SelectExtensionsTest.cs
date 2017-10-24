// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web.Mvc.Test;
using System.Web.WebPages.Scope;
using Microsoft.TestCommon;
using Microsoft.Web.UnitTestUtil;

namespace System.Web.Mvc.Html.Test
{
    [Xunit.Collection("Uses ScopeStorage or ViewEngines.Engines")]
    public class SelectExtensionsTest : IDisposable
    {
        private static readonly ViewDataDictionary<FooModel> _listBoxViewData = new ViewDataDictionary<FooModel> { { "foo", new[] { "Bravo" } } };
        private static readonly ViewDataDictionary<FooModel> _dropDownListViewData = new ViewDataDictionary<FooModel> { { "foo", "Bravo" } };
        private static readonly ViewDataDictionary<NonIEnumerableModel> _nonIEnumerableViewData = new ViewDataDictionary<NonIEnumerableModel> { { "foo", 1 } };
        private static readonly ViewDataDictionary<EnumModel> _enumDropDownListViewData = new ViewDataDictionary<EnumModel>
        {
            { "WithDisplay", EnumWithDisplay.Two },
            { "WithDuplicates", EnumWithDuplicates.Second },
            { "WithFlags", EnumWithFlags.Second },
        };
        private static readonly SelectList _selectList = new SelectList(
            new[]
            {
                new { Text = "UFO", Value = "ufo", Category = "" }, /* Empty Group */
                new { Text = "Volvo", Value = "volvo", Category = "Swedish Cars" },
                new { Text = "Mercedes-Benz", Value = "mercedes-benz", Category = "German Cars" },
                new { Text = "Saab", Value = "saab", Category = "Swedish Cars" },
                new { Text = "Audi", Value = "audi", Category = "German Cars" },
                new { Text = "Other", Value = "other", Category = (string) null }, /* Another Empty Group */
                new { Text = "Unknown", Value = "unknown", Category = " " } /* Unnamed Group */
            }, "Value", "Text", "Category", (object) "audi");
        private static readonly MultiSelectList _multiSelectList = new MultiSelectList(
            new[]
            {
                new { Text = "UFO", Value = "ufo", Category = "" }, /* Empty Group */
                new { Text = "Volvo", Value = "volvo", Category = "Swedish Cars" },
                new { Text = "Mercedes-Benz", Value = "mercedes-benz", Category = "German Cars" },
                new { Text = "Saab", Value = "saab", Category = "Swedish Cars" },
                new { Text = "Audi", Value = "audi", Category = "German Cars" },
                new { Text = "Other", Value = "other", Category = (string) null }, /* Another Empty Group */
                new { Text = "Unknown", Value = "unknown", Category = " " } /* Unnamed Group */
            }, "Value", "Text", "Category", new[] { "audi", "volvo" });

        private static ViewDataDictionary GetViewDataWithSelectList()
        {
            ViewDataDictionary viewData = new ViewDataDictionary();
            SelectList selectList = new SelectList(MultiSelectListTest.GetSampleAnonymousObjects(), "Letter", "FullWord", "C");
            viewData["foo"] = selectList;
            viewData["foo.bar"] = selectList;
            return viewData;
        }

        private static List<SelectListItem> GroupedItems_WithDisabled
        {
            get
            {
                List<SelectListItem> items = new List<SelectListItem>();
                SelectListGroup disabledGroup = new SelectListGroup { Disabled = true, Name = "DisabledGroup" };
                items.Add(new SelectListItem() { Text = "Alice", Value = "a" });
                items.Add(new SelectListItem() { Text = "Bob", Value = "b", Group = disabledGroup });
                items.Add(new SelectListItem() { Text = "Charlie", Value = "c", Group = disabledGroup });
                items.Add(new SelectListItem() { Text = "David", Value = "d", Disabled = true });

                return items;
            }
        }

        // DropDownList

        private static List<SelectListItem> GroupedItems
        {
            get
            {
                List<SelectListItem> items = new List<SelectListItem>();
                SelectListGroup swedish = new SelectListGroup { Name = "Swedish Cars" };
                SelectListGroup german = new SelectListGroup { Name = "German Cars" };
                SelectListGroup unnamed = new SelectListGroup();
                items.Add(new SelectListItem() { Text = "other1", Value = "other1" });
                items.Add(new SelectListItem() { Text = "other2", Value = "other2" });
                items.Add(new SelectListItem() { Group = swedish, Text = "Volvo", Value = "volvo" });
                items.Add(new SelectListItem() { Text = "other3", Value = "other3" });
                items.Add(new SelectListItem() { Group = unnamed, Text = "other4", Value = "other4" });
                items.Add(new SelectListItem() { Group = unnamed, Text = "other5", Value = "other5" });
                items.Add(new SelectListItem() { Group = german, Text = "Mercedes-Benz", Value = "mercedes-benz" });
                items.Add(new SelectListItem() { Group = swedish, Text = "Saab", Value = "saab", Selected = true });
                items.Add(new SelectListItem() { Group = german, Text = "Audi", Value = "audi", Disabled = true });
                items.Add(new SelectListItem() { Text = "other6", Value = "other6" });

                return items;
            }
        }

        [Fact]
        void DropDownList_WithGroups()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper();
            const string expectedDropDownListHtml = @"<select id=""List"" name=""List""><option value=""other1"">other1</option>
<option value=""other2"">other2</option>
<optgroup label=""Swedish Cars"">
<option value=""volvo"">Volvo</option>
<option selected=""selected"" value=""saab"">Saab</option>
</optgroup>
<option value=""other3"">other3</option>
<optgroup>
<option value=""other4"">other4</option>
<option value=""other5"">other5</option>
</optgroup>
<optgroup label=""German Cars"">
<option value=""mercedes-benz"">Mercedes-Benz</option>
<option disabled=""disabled"" value=""audi"">Audi</option>
</optgroup>
<option value=""other6"">other6</option>
</select>";

            // Act
            MvcHtmlString html = helper.DropDownList("List", GroupedItems);

            // Assert
            Assert.Equal(expectedDropDownListHtml, html.ToHtmlString());
        }

        [Fact]
        void DropDownList_WithGroups_WithDisabled()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper();
            const string expectedDropDownListHtml = @"<select id=""List"" name=""List""><option value=""a"">Alice</option>
<optgroup disabled=""disabled"" label=""DisabledGroup"">
<option value=""b"">Bob</option>
<option value=""c"">Charlie</option>
</optgroup>
<option disabled=""disabled"" value=""d"">David</option>
</select>";

            // Act
            MvcHtmlString html = helper.DropDownList("List", GroupedItems_WithDisabled);

            // Assert
            Assert.Equal(expectedDropDownListHtml, html.ToHtmlString());
        }


        [Fact]
        void DropDownList_SelectList_WithGroups()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper();
            const string expectedDropDownListHtml = @"<select id=""List"" name=""List""><option value=""ufo"">UFO</option>
<optgroup label=""Swedish Cars"">
<option value=""volvo"">Volvo</option>
<option value=""saab"">Saab</option>
</optgroup>
<optgroup label=""German Cars"">
<option value=""mercedes-benz"">Mercedes-Benz</option>
<option selected=""selected"" value=""audi"">Audi</option>
</optgroup>
<option value=""other"">Other</option>
<optgroup label="" "">
<option value=""unknown"">Unknown</option>
</optgroup>
</select>";

            // Act
            MvcHtmlString html = helper.DropDownList("List", _selectList);

            // Assert
            Assert.Equal(expectedDropDownListHtml, html.ToHtmlString());
        }

        [Fact]
        public void DropDownListUsesExplicitValueIfNotProvidedInViewData()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary());
            SelectList selectList = new SelectList(MultiSelectListTest.GetSampleAnonymousObjects(), "Letter", "FullWord", "C");

            // Act
            MvcHtmlString html = helper.DropDownList("foo", selectList, (string)null /* optionLabel */);

            // Assert
            Assert.Equal(
                "<select id=\"foo\" name=\"foo\"><option value=\"A\">Alpha</option>" + Environment.NewLine
              + "<option value=\"B\">Bravo</option>" + Environment.NewLine
              + "<option selected=\"selected\" value=\"C\">Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void DropDownListUsesExplicitValueIfNotProvidedInViewData_Unobtrusive()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary());
            helper.ViewContext.ClientValidationEnabled = true;
            helper.ViewContext.UnobtrusiveJavaScriptEnabled = true;
            helper.ViewContext.FormContext = new FormContext();
            helper.ClientValidationRuleFactory = (name, metadata) => new[] { new ModelClientValidationRule { ValidationType = "type", ErrorMessage = "error" } };
            SelectList selectList = new SelectList(MultiSelectListTest.GetSampleAnonymousObjects(), "Letter", "FullWord", "C");

            // Act
            MvcHtmlString html = helper.DropDownList("foo", selectList, (string)null /* optionLabel */);

            // Assert
            Assert.Equal(
                "<select data-val=\"true\" data-val-type=\"error\" id=\"foo\" name=\"foo\"><option value=\"A\">Alpha</option>" + Environment.NewLine
              + "<option value=\"B\">Bravo</option>" + Environment.NewLine
              + "<option selected=\"selected\" value=\"C\">Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void DropDownListUsesViewDataDefaultValue()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(_dropDownListViewData);
            SelectList selectList = new SelectList(MultiSelectListTest.GetSampleStrings(), "Charlie");

            // Act
            MvcHtmlString html = helper.DropDownList("foo", selectList, (string)null /* optionLabel */);

            // Assert
            Assert.Equal(
                "<select id=\"foo\" name=\"foo\"><option>Alpha</option>" + Environment.NewLine
              + "<option selected=\"selected\">Bravo</option>" + Environment.NewLine
              + "<option>Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void DropDownListUsesViewDataDefaultValueNoOptionLabel()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(_dropDownListViewData);
            SelectList selectList = new SelectList(MultiSelectListTest.GetSampleStrings(), "Charlie");

            // Act
            MvcHtmlString html = helper.DropDownList("foo", selectList);

            // Assert
            Assert.Equal(
                "<select id=\"foo\" name=\"foo\"><option>Alpha</option>" + Environment.NewLine
              + "<option selected=\"selected\">Bravo</option>" + Environment.NewLine
              + "<option>Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void DropDownListWithAttributesDictionary()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary());
            SelectList selectList = new SelectList(MultiSelectListTest.GetSampleStrings());

            // Act
            MvcHtmlString html = helper.DropDownList("foo", selectList, null /* optionLabel */, HtmlHelperTest.AttributesDictionary);

            // Assert
            Assert.Equal(
                "<select baz=\"BazValue\" id=\"foo\" name=\"foo\"><option>Alpha</option>" + Environment.NewLine
              + "<option>Bravo</option>" + Environment.NewLine
              + "<option>Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void DropDownListWithEmptyNameThrows()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary());

            // Act & Assert
            Assert.ThrowsArgumentNullOrEmpty(
                delegate { helper.DropDownList(String.Empty, (SelectList)null /* selectList */, (string)null /* optionLabel */); },
                "name");
        }

        [Fact]
        public void DropDownListWithErrors()
        {
            // Arrange
            SelectList selectList = new SelectList(MultiSelectListTest.GetSampleStrings(), new[] { "Charlie" });
            ViewDataDictionary viewData = GetViewDataWithErrors();
            HtmlHelper helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            MvcHtmlString html = helper.DropDownList("foo", selectList, null /* optionLabel */, HtmlHelperTest.AttributesObjectDictionary);

            // Assert
            Assert.Equal(
                "<select baz=\"BazObjValue\" class=\"input-validation-error\" id=\"foo\" name=\"foo\"><option>Alpha</option>" + Environment.NewLine
              + "<option selected=\"selected\">Bravo</option>" + Environment.NewLine
              + "<option>Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void DropDownListWithErrorsAndCustomClass()
        {
            // Arrange
            SelectList selectList = new SelectList(MultiSelectListTest.GetSampleStrings());
            ViewDataDictionary viewData = GetViewDataWithErrors();
            HtmlHelper helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            MvcHtmlString html = helper.DropDownList("foo", selectList, null /* optionLabel */, new { @class = "foo-class" });

            // Assert
            Assert.Equal(
                "<select class=\"input-validation-error foo-class\" id=\"foo\" name=\"foo\"><option>Alpha</option>" + Environment.NewLine
              + "<option selected=\"selected\">Bravo</option>" + Environment.NewLine
              + "<option>Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void DropDownListWithNullNameThrows()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary());

            // Act & Assert
            Assert.ThrowsArgumentNullOrEmpty(
                delegate { helper.DropDownList(null /* name */, (SelectList)null /* selectList */, (string)null /* optionLabel */); },
                "name");
        }

        [Fact]
        public void DropDownListWithNullSelectListUsesViewData()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper();
            helper.ViewData["foo"] = new MultiSelectList(MultiSelectListTest.GetSampleStrings(), new[] { "Charlie" });

            // Act
            MvcHtmlString html = helper.DropDownList("foo");

            // Assert
            Assert.Equal(
                "<select id=\"foo\" name=\"foo\"><option>Alpha</option>" + Environment.NewLine
              + "<option>Bravo</option>" + Environment.NewLine
              + "<option selected=\"selected\">Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void DropDownListWithObjectDictionary()
        {
            // Arrange
            SelectList selectList = new SelectList(MultiSelectListTest.GetSampleStrings());
            ViewDataDictionary viewData = new ViewDataDictionary();
            HtmlHelper helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            MvcHtmlString html = helper.DropDownList("foo", selectList, null /* optionLabel */, HtmlHelperTest.AttributesObjectDictionary);

            // Assert
            Assert.Equal(
                "<select baz=\"BazObjValue\" id=\"foo\" name=\"foo\"><option>Alpha</option>" + Environment.NewLine
              + "<option>Bravo</option>" + Environment.NewLine
              + "<option>Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void DropDownListWithObjectDictionaryWithUnderscores()
        {
            // Arrange
            SelectList selectList = new SelectList(MultiSelectListTest.GetSampleStrings());
            ViewDataDictionary viewData = new ViewDataDictionary();
            HtmlHelper helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            MvcHtmlString html = helper.DropDownList("foo", selectList, null /* optionLabel */, HtmlHelperTest.AttributesObjectUnderscoresDictionary);

            // Assert
            Assert.Equal(
                "<select foo-baz=\"BazObjValue\" id=\"foo\" name=\"foo\"><option>Alpha</option>" + Environment.NewLine
              + "<option>Bravo</option>" + Environment.NewLine
              + "<option>Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void DropDownListWithObjectDictionaryAndSelectList()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary());
            SelectList selectList = new SelectList(MultiSelectListTest.GetSampleStrings());

            // Act
            MvcHtmlString html = helper.DropDownList("foo", selectList, null /* optionLabel */, HtmlHelperTest.AttributesObjectDictionary);

            // Assert
            Assert.Equal(
                "<select baz=\"BazObjValue\" id=\"foo\" name=\"foo\"><option>Alpha</option>" + Environment.NewLine
              + "<option>Bravo</option>" + Environment.NewLine
              + "<option>Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void DropDownListWithObjectDictionaryAndSelectListNoOptionLabel()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary());
            SelectList selectList = new SelectList(MultiSelectListTest.GetSampleStrings());

            // Act
            MvcHtmlString html = helper.DropDownList("foo", selectList, HtmlHelperTest.AttributesObjectDictionary);

            // Assert
            Assert.Equal(
                "<select baz=\"BazObjValue\" id=\"foo\" name=\"foo\"><option>Alpha</option>" + Environment.NewLine
              + "<option>Bravo</option>" + Environment.NewLine
              + "<option>Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void DropDownListWithObjectDictionaryWithUnderscoresAndSelectListNoOptionLabel()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary());
            SelectList selectList = new SelectList(MultiSelectListTest.GetSampleStrings());

            // Act
            MvcHtmlString html = helper.DropDownList("foo", selectList, HtmlHelperTest.AttributesObjectUnderscoresDictionary);

            // Assert
            Assert.Equal(
                "<select foo-baz=\"BazObjValue\" id=\"foo\" name=\"foo\"><option>Alpha</option>" + Environment.NewLine
              + "<option>Bravo</option>" + Environment.NewLine
              + "<option>Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void DropDownListWithObjectDictionaryAndEmptyOptionLabel()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary());
            SelectList selectList = new SelectList(MultiSelectListTest.GetSampleStrings());

            // Act
            MvcHtmlString html = helper.DropDownList("foo", selectList, String.Empty /* optionLabel */, HtmlHelperTest.AttributesObjectDictionary);

            // Assert
            Assert.Equal(
                "<select baz=\"BazObjValue\" id=\"foo\" name=\"foo\"><option value=\"\"></option>" + Environment.NewLine
              + "<option>Alpha</option>" + Environment.NewLine
              + "<option>Bravo</option>" + Environment.NewLine
              + "<option>Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void DropDownListWithObjectDictionaryAndTitle()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary());
            SelectList selectList = new SelectList(MultiSelectListTest.GetSampleStrings());

            // Act
            MvcHtmlString html = helper.DropDownList("foo", selectList, "[Select Something]", HtmlHelperTest.AttributesObjectDictionary);

            // Assert
            Assert.Equal(
                "<select baz=\"BazObjValue\" id=\"foo\" name=\"foo\"><option value=\"\">[Select Something]</option>" + Environment.NewLine
              + "<option>Alpha</option>" + Environment.NewLine
              + "<option>Bravo</option>" + Environment.NewLine
              + "<option>Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void DropDownListUsesViewDataSelectList()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetViewDataWithSelectList());

            // Act
            MvcHtmlString html = helper.DropDownList("foo", (string)null /* optionLabel */);

            // Assert
            Assert.Equal(
                "<select id=\"foo\" name=\"foo\"><option value=\"A\">Alpha</option>" + Environment.NewLine
              + "<option value=\"B\">Bravo</option>" + Environment.NewLine
              + "<option selected=\"selected\" value=\"C\">Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void DropDownListUsesModelState()
        {
            // Arrange
            SelectList selectList = new SelectList(MultiSelectListTest.GetSampleStrings());
            ViewDataDictionary viewData = GetViewDataWithErrors();
            viewData["foo"] = selectList;
            HtmlHelper helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            MvcHtmlString html = helper.DropDownList("foo");

            // Assert
            Assert.Equal(
                "<select class=\"input-validation-error\" id=\"foo\" name=\"foo\"><option>Alpha</option>" + Environment.NewLine
              + "<option selected=\"selected\">Bravo</option>" + Environment.NewLine
              + "<option>Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void DropDownListUsesViewDataSelectListNoOptionLabel()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetViewDataWithSelectList());

            // Act
            MvcHtmlString html = helper.DropDownList("foo");

            // Assert
            Assert.Equal(
                "<select id=\"foo\" name=\"foo\"><option value=\"A\">Alpha</option>" + Environment.NewLine
              + "<option value=\"B\">Bravo</option>" + Environment.NewLine
              + "<option selected=\"selected\" value=\"C\">Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void DropDownListWithDotReplacementForId()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(GetViewDataWithSelectList());

            // Act
            MvcHtmlString html = helper.DropDownList("foo.bar");

            // Assert
            Assert.Equal(
                "<select id=\"foo_bar\" name=\"foo.bar\"><option value=\"A\">Alpha</option>" + Environment.NewLine
              + "<option value=\"B\">Bravo</option>" + Environment.NewLine
              + "<option selected=\"selected\" value=\"C\">Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void DropDownListWithIEnumerableSelectListItem()
        {
            // Arrange
            ViewDataDictionary vdd = new ViewDataDictionary { { "foo", MultiSelectListTest.GetSampleIEnumerableObjects() } };
            HtmlHelper helper = MvcHelper.GetHtmlHelper(vdd);

            // Act
            MvcHtmlString html = helper.DropDownList("foo");

            // Assert
            Assert.Equal(
                "<select id=\"foo\" name=\"foo\"><option value=\"123456789\">John</option>" + Environment.NewLine
              + "<option value=\"987654321\">Jane</option>" + Environment.NewLine
              + "<option selected=\"selected\" value=\"111111111\">Joe</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void DropDownListWithIEnumerableSelectListItemSelectsDefaultFromViewData()
        {
            // Arrange
            ViewDataDictionary vdd = new ViewDataDictionary { { "foo", "123456789" } };
            HtmlHelper helper = MvcHelper.GetHtmlHelper(vdd);

            // Act
            MvcHtmlString html = helper.DropDownList("foo", MultiSelectListTest.GetSampleIEnumerableObjects());

            // Assert
            Assert.Equal(
                "<select id=\"foo\" name=\"foo\"><option selected=\"selected\" value=\"123456789\">John</option>" + Environment.NewLine
              + "<option value=\"987654321\">Jane</option>" + Environment.NewLine
              + "<option value=\"111111111\">Joe</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void DropDownListWithIEnumerableOfSelectListItemMatchesEnumName()
        {
            // Arrange
            ViewDataDictionary<EnumWithDisplay> vdd = new ViewDataDictionary<EnumWithDisplay>
            {
                { "foo", EnumWithDisplay.Three }
            };
            HtmlHelper<EnumWithDisplay> helper = MvcHelper.GetHtmlHelper(vdd);

            // Act
            MvcHtmlString html =
                helper.DropDownList("foo", GetSelectListWithNamedValuesForEnumWithDisplay(includeEmpty: false));

            // Assert
            Assert.Equal(
                "<select id=\"foo\" name=\"foo\">" +
                "<option value=\"Zero\">Zero</option>" + Environment.NewLine +
                "<option value=\"One\">One</option>" + Environment.NewLine +
                "<option value=\"Two\">Two</option>" + Environment.NewLine +
                "<option selected=\"selected\" value=\"Three\">Three</option>" + Environment.NewLine +
                "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void DropDownListWithIEnumerableOfSelectListItemMatchesEnumValue()
        {
            // Arrange
            ViewDataDictionary<EnumWithDisplay> vdd = new ViewDataDictionary<EnumWithDisplay>
            {
                { "foo", EnumWithDisplay.Three }
            };
            HtmlHelper<EnumWithDisplay> helper = MvcHelper.GetHtmlHelper(vdd);

            // Act
            MvcHtmlString html =
                helper.DropDownList("foo", GetSelectListWithNumericValuesForEnumWithDisplay(includeEmpty: false));

            // Assert
            Assert.Equal(
                "<select id=\"foo\" name=\"foo\">" +
                "<option value=\"0\">Zero</option>" + Environment.NewLine +
                "<option value=\"1\">One</option>" + Environment.NewLine +
                "<option value=\"2\">Two</option>" + Environment.NewLine +
                "<option selected=\"selected\" value=\"3\">Three</option>" + Environment.NewLine +
                "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void DropDownListWithListOfSelectListItemSelectsDefaultFromViewData()
        {
            // Arrange
            ViewDataDictionary vdd = new ViewDataDictionary { { "foo", "123456789" } };
            HtmlHelper helper = MvcHelper.GetHtmlHelper(vdd);

            // Act
            MvcHtmlString html = helper.DropDownList("foo", MultiSelectListTest.GetSampleListObjects());

            // Assert
            Assert.Equal(
                "<select id=\"foo\" name=\"foo\"><option selected=\"selected\" value=\"123456789\">John</option>" + Environment.NewLine
              + "<option value=\"987654321\">Jane</option>" + Environment.NewLine
              + "<option value=\"111111111\">Joe</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void DropDownListWithListOfSelectListItem()
        {
            // Arrange
            ViewDataDictionary vdd = new ViewDataDictionary { { "foo", MultiSelectListTest.GetSampleListObjects() } };
            HtmlHelper helper = MvcHelper.GetHtmlHelper(vdd);

            // Act
            MvcHtmlString html = helper.DropDownList("foo");

            // Assert
            Assert.Equal(
                "<select id=\"foo\" name=\"foo\"><option value=\"123456789\">John</option>" + Environment.NewLine
              + "<option value=\"987654321\">Jane</option>" + Environment.NewLine
              + "<option selected=\"selected\" value=\"111111111\">Joe</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void DropDownListWithNullViewDataValueThrows()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary());

            // Act
            Assert.Throws<InvalidOperationException>(
                delegate { helper.DropDownList("foo", (string)null /* optionLabel */); },
                "There is no ViewData item of type 'IEnumerable<SelectListItem>' that has the key 'foo'.");
        }

        [Fact]
        public void DropDownListWithWrongViewDataTypeValueThrows()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary { { "foo", 123 } });

            // Act
            Assert.Throws<InvalidOperationException>(
                delegate { helper.DropDownList("foo", (string)null /* optionLabel */); },
                "The ViewData item that has the key 'foo' is of type 'System.Int32' but must be of type 'IEnumerable<SelectListItem>'.");
        }

        [Fact]
        public void DropDownListWithPrefix()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary());
            SelectList selectList = new SelectList(MultiSelectListTest.GetSampleStrings());
            helper.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = "MyPrefix";

            // Act
            MvcHtmlString html = helper.DropDownList("foo", selectList, HtmlHelperTest.AttributesObjectDictionary);

            // Assert
            Assert.Equal(
                "<select baz=\"BazObjValue\" id=\"MyPrefix_foo\" name=\"MyPrefix.foo\"><option>Alpha</option>" + Environment.NewLine
              + "<option>Bravo</option>" + Environment.NewLine
              + "<option>Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void DropDownListWithPrefixAndEmptyName()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary());
            SelectList selectList = new SelectList(MultiSelectListTest.GetSampleStrings());
            helper.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = "MyPrefix";

            // Act
            MvcHtmlString html = helper.DropDownList("", selectList, HtmlHelperTest.AttributesObjectDictionary);

            // Assert
            Assert.Equal(
                "<select baz=\"BazObjValue\" id=\"MyPrefix\" name=\"MyPrefix\"><option>Alpha</option>" + Environment.NewLine
              + "<option>Bravo</option>" + Environment.NewLine
              + "<option>Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void DropDownListWithPrefixAndNullSelectListUsesViewData()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper();
            helper.ViewData["foo"] = new MultiSelectList(MultiSelectListTest.GetSampleStrings(), new[] { "Charlie" });
            helper.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = "MyPrefix";

            // Act
            MvcHtmlString html = helper.DropDownList("foo");

            // Assert
            Assert.Equal(
                "<select id=\"MyPrefix_foo\" name=\"MyPrefix.foo\"><option>Alpha</option>" + Environment.NewLine
              + "<option>Bravo</option>" + Environment.NewLine
              + "<option selected=\"selected\">Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Theory]
        [PropertyData("AttributeEncodedData_NoHtmlEncode", PropertyType = typeof(EncodedDataSets))]
        public void DropDownList_AttributeEncodes_AddedHtmlAttributes(string text, string encodedText)
        {
            // Arrange
            var selectList = new List<SelectListItem>
            {
                new SelectListItem { Text = "text", },
            };
            var helper = MvcHelper.GetHtmlHelper();

            // Act
            var result =
                helper.DropDownList(name: "name", selectList: selectList, htmlAttributes: new { attribute = text, })
                .ToHtmlString();

            // Assert
            Assert.Equal(
                "<select attribute=\"" +
                    encodedText +
                    "\" id=\"name\" name=\"name\"><option>text</option>" +
                    Environment.NewLine +
                    "</select>",
                result);
        }

        [Theory]
        [PropertyData("AttributeEncodedData_NoHtmlEncode", PropertyType = typeof(EncodedDataSets))]
        public void DropDownList_AttributeEncodes_Name(string text, string encodedText)
        {
            // Arrange
            var selectList = new List<SelectListItem>
            {
                new SelectListItem { Text = "text", },
            };
            var helper = MvcHelper.GetHtmlHelper();

            // Act
            // htmlAttributes included only to avoid special-cased renaming done for id attribute.
            var result =
                helper.DropDownList(name: text, selectList: selectList, htmlAttributes: new { id = "id", })
                .ToHtmlString();

            // Assert
            Assert.Equal(
                "<select id=\"id\" name=\"" +
                    encodedText +
                    "\"><option>text</option>" +
                    Environment.NewLine +
                    "</select>",
                result);
        }

        [Theory]
        [PropertyData("AttributeEncodedData_NoHtmlEncode", PropertyType = typeof(EncodedDataSets))]
        public void DropDownList_AttributeEncodes_OptGroupLabel(string text, string encodedText)
        {
            // Arrange
            var selectList = new List<SelectListItem>
            {
                new SelectListItem { Group = new SelectListGroup { Name = text, }, Text = "text", },
            };
            var helper = MvcHelper.GetHtmlHelper();

            // Act
            var result = helper.DropDownList(name: "name", selectList: selectList).ToHtmlString();

            // Assert
            Assert.Equal(
                "<select id=\"name\" name=\"name\"><optgroup label=\"" +
                    encodedText +
                    "\">" +
                    Environment.NewLine +
                    "<option>text</option>" +
                    Environment.NewLine +
                    "</optgroup>" +
                    Environment.NewLine +
                    "</select>",
                result);
        }

        [Theory]
        [PropertyData("HtmlEncodedData_NoHtmlEncode", PropertyType = typeof(EncodedDataSets))]
        public void DropDownList_HtmlEncodes_OptionLabel(string text, string encodedText)
        {
            // Arrange
            var selectList = new List<SelectListItem>
            {
                new SelectListItem { Text = "text", },
            };
            var helper = MvcHelper.GetHtmlHelper();

            // Act
            var result = helper.DropDownList(name: "name", selectList: selectList, optionLabel: text).ToHtmlString();

            // Assert
            Assert.Equal(
                "<select id=\"name\" name=\"name\"><option value=\"\">" +
                    encodedText +
                    "</option>" +
                    Environment.NewLine +
                    "<option>text</option>" +
                    Environment.NewLine +
                    "</select>",
                result);
        }

        [Theory]
        [PropertyData("AttributeEncodedData_NoHtmlEncode", PropertyType = typeof(EncodedDataSets))]
        public void DropDownList_AttributeEncodes_Prefix(string text, string encodedText)
        {
            // Arrange
            var selectList = new List<SelectListItem>
            {
                new SelectListItem { Text = "text", },
            };
            var viewData = new ViewDataDictionary<string>(model: null);
            viewData.TemplateInfo.HtmlFieldPrefix = text;
            var helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            // htmlAttributes included only to avoid special-cased renaming done for id attribute.
            var result =
                helper.DropDownList(name: String.Empty, selectList: selectList, htmlAttributes: new { id = "id", })
                .ToHtmlString();

            // Assert
            Assert.Equal(
                "<select id=\"id\" name=\"" +
                    encodedText +
                    "\"><option>text</option>" +
                    Environment.NewLine +
                    "</select>",
                result);
        }

        [Theory]
        [PropertyData("HtmlEncodedData_NoHtmlEncode", PropertyType = typeof(EncodedDataSets))]
        public void DropDownList_HtmlEncodes_Text(string text, string encodedText)
        {
            // Arrange
            var selectList = new List<SelectListItem>
            {
                new SelectListItem { Text = text, },
            };
            var helper = MvcHelper.GetHtmlHelper();

            // Act
            var result = helper.DropDownList(name: "name", selectList: selectList).ToHtmlString();

            // Assert
            Assert.Equal(
                "<select id=\"name\" name=\"name\"><option>" +
                    encodedText +
                    "</option>" +
                    Environment.NewLine +
                    "</select>",
                result);
        }

        [Theory]
        [PropertyData("AttributeEncodedData_NoHtmlEncode", PropertyType = typeof(EncodedDataSets))]
        public void DropDownList_AttributeEncodes_Value(string text, string encodedText)
        {
            // Arrange
            var selectList = new List<SelectListItem>
            {
                new SelectListItem { Text = "text", Value = text },
            };
            var helper = MvcHelper.GetHtmlHelper();

            // Act
            var result = helper.DropDownList(name: "name", selectList: selectList).ToHtmlString();

            // Assert
            Assert.Equal(
                "<select id=\"name\" name=\"name\"><option value=\"" +
                    encodedText +
                    "\">text</option>" +
                    Environment.NewLine +
                    "</select>",
                result);
        }

        // DropDownListFor

        [Fact]
        void DropDownListFor_WithGroups()
        {
            // Arrange
            ViewDataDictionary<FooModel> dict = new ViewDataDictionary<FooModel>();
            dict.Add("foo", "volvo");
            HtmlHelper<FooModel> helper = MvcHelper.GetHtmlHelper(dict);
            const string expectedListBox = @"<select id=""foo"" name=""foo""><option value="""">Cars</option>
<option value=""other1"">other1</option>
<option value=""other2"">other2</option>
<optgroup label=""Swedish Cars"">
<option selected=""selected"" value=""volvo"">Volvo</option>
<option value=""saab"">Saab</option>
</optgroup>
<option value=""other3"">other3</option>
<optgroup>
<option value=""other4"">other4</option>
<option value=""other5"">other5</option>
</optgroup>
<optgroup label=""German Cars"">
<option value=""mercedes-benz"">Mercedes-Benz</option>
<option disabled=""disabled"" value=""audi"">Audi</option>
</optgroup>
<option value=""other6"">other6</option>
</select>";

            // Act
            MvcHtmlString html = helper.DropDownListFor(m => m.foo, GroupedItems, optionLabel: "Cars");

            // Assert
            Assert.Equal(expectedListBox, html.ToHtmlString());
        }

        [Fact]
        void DropDownListFor_WithGroups_WithDisabled()
        {
            // Arrange
            ViewDataDictionary<FooModel> dict = new ViewDataDictionary<FooModel>();
            dict.Add("foo", "d");
            HtmlHelper<FooModel> helper = MvcHelper.GetHtmlHelper(dict);
            const string expectedListBox = @"<select id=""foo"" name=""foo""><option value="""">Options</option>
<option value=""a"">Alice</option>
<optgroup disabled=""disabled"" label=""DisabledGroup"">
<option value=""b"">Bob</option>
<option value=""c"">Charlie</option>
</optgroup>
<option disabled=""disabled"" selected=""selected"" value=""d"">David</option>
</select>";

            // Act
            MvcHtmlString html = helper.DropDownListFor(m => m.foo, GroupedItems_WithDisabled, optionLabel: "Options");

            // Assert
            Assert.Equal(expectedListBox, html.ToHtmlString());
        }

        [Fact]
        void DropDownListFor_SelectList_WithGroups()
        {
            // Arrange
            ViewDataDictionary<FooModel> dict = new ViewDataDictionary<FooModel>();
            dict.Add("foo", "volvo");
            HtmlHelper<FooModel> helper = MvcHelper.GetHtmlHelper(dict);
            const string expectedListBox = @"<select id=""foo"" name=""foo""><option value="""">options...</option>
<option value=""ufo"">UFO</option>
<optgroup label=""Swedish Cars"">
<option selected=""selected"" value=""volvo"">Volvo</option>
<option value=""saab"">Saab</option>
</optgroup>
<optgroup label=""German Cars"">
<option value=""mercedes-benz"">Mercedes-Benz</option>
<option value=""audi"">Audi</option>
</optgroup>
<option value=""other"">Other</option>
<optgroup label="" "">
<option value=""unknown"">Unknown</option>
</optgroup>
</select>";

            // Act
            MvcHtmlString html = helper.DropDownListFor(m => m.foo, _selectList, optionLabel: "options...");

            // Assert
            Assert.Equal(expectedListBox, html.ToHtmlString());
        }

        [Fact]
        public void DropDownListForWithNullExpressionThrows()
        {
            // Arrange
            HtmlHelper<object> helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary<object>());
            SelectList selectList = new SelectList(MultiSelectListTest.GetSampleAnonymousObjects(), "Letter", "FullWord", "C");

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => helper.DropDownListFor<object, object>(null /* expression */, selectList),
                "expression"
                );
        }

        [Fact]
        public void DropDownListForUsesExplicitValueIfNotProvidedInViewData()
        {
            // Arrange
            HtmlHelper<FooModel> helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary<FooModel>());
            SelectList selectList = new SelectList(MultiSelectListTest.GetSampleAnonymousObjects(), "Letter", "FullWord", "C");

            // Act
            MvcHtmlString html = helper.DropDownListFor(m => m.foo, selectList, (string)null /* optionLabel */);

            // Assert
            Assert.Equal(
                "<select id=\"foo\" name=\"foo\"><option value=\"A\">Alpha</option>" + Environment.NewLine
              + "<option value=\"B\">Bravo</option>" + Environment.NewLine
              + "<option selected=\"selected\" value=\"C\">Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void DropDownListForUsesExplicitValueIfNotProvidedInViewData_Unobtrusive()
        {
            // Arrange
            HtmlHelper<FooModel> helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary<FooModel>());
            helper.ViewContext.ClientValidationEnabled = true;
            helper.ViewContext.UnobtrusiveJavaScriptEnabled = true;
            helper.ViewContext.FormContext = new FormContext();
            helper.ClientValidationRuleFactory = (name, metadata) => new[] { new ModelClientValidationRule { ValidationType = "type", ErrorMessage = "error" } };
            SelectList selectList = new SelectList(MultiSelectListTest.GetSampleAnonymousObjects(), "Letter", "FullWord", "C");

            // Act
            MvcHtmlString html = helper.DropDownListFor(m => m.foo, selectList, (string)null /* optionLabel */);

            // Assert
            Assert.Equal(
                "<select data-val=\"true\" data-val-type=\"error\" id=\"foo\" name=\"foo\"><option value=\"A\">Alpha</option>" + Environment.NewLine
              + "<option value=\"B\">Bravo</option>" + Environment.NewLine
              + "<option selected=\"selected\" value=\"C\">Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void DropDownListForWithEnumerableModel_Unobtrusive()
        {
            // Arrange
            HtmlHelper<IEnumerable<RequiredModel>> helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary<IEnumerable<RequiredModel>>());
            helper.ViewContext.ClientValidationEnabled = true;
            helper.ViewContext.UnobtrusiveJavaScriptEnabled = true;
            helper.ViewContext.FormContext = new FormContext();
            helper.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = "MyPrefix";

            SelectList selectList = new SelectList(MultiSelectListTest.GetSampleAnonymousObjects(), "Letter", "FullWord", "C");

            using (new CultureReplacer("en-US", "en-US"))
            {
                // Act
                MvcHtmlString html = helper.DropDownListFor(m => m.ElementAt(0).foo, selectList);

                // Assert
                Assert.Equal(
                    "<select data-val=\"true\" data-val-required=\"The foo field is required.\" id=\"MyPrefix_foo\" name=\"MyPrefix.foo\"><option value=\"A\">Alpha</option>" + Environment.NewLine
                  + "<option value=\"B\">Bravo</option>" + Environment.NewLine
                  + "<option selected=\"selected\" value=\"C\">Charlie</option>" + Environment.NewLine
                  + "</select>",
                    html.ToHtmlString());
            }
        }

        [Fact]
        public void DropDownListForUsesViewDataDefaultValue()
        {
            // Arrange
            HtmlHelper<FooModel> helper = MvcHelper.GetHtmlHelper(_dropDownListViewData);
            SelectList selectList = new SelectList(MultiSelectListTest.GetSampleStrings(), "Charlie");

            // Act
            MvcHtmlString html = helper.DropDownListFor(m => m.foo, selectList, (string)null /* optionLabel */);

            // Assert
            Assert.Equal(
                "<select id=\"foo\" name=\"foo\"><option>Alpha</option>" + Environment.NewLine
              + "<option selected=\"selected\">Bravo</option>" + Environment.NewLine
              + "<option>Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void DropDownListForUsesViewDataDefaultValueNoOptionLabel()
        {
            // Arrange
            HtmlHelper<FooModel> helper = MvcHelper.GetHtmlHelper(_dropDownListViewData);
            SelectList selectList = new SelectList(MultiSelectListTest.GetSampleStrings(), "Charlie");

            // Act
            MvcHtmlString html = helper.DropDownListFor(m => m.foo, selectList);

            // Assert
            Assert.Equal(
                "<select id=\"foo\" name=\"foo\"><option>Alpha</option>" + Environment.NewLine
              + "<option selected=\"selected\">Bravo</option>" + Environment.NewLine
              + "<option>Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void DropDownListForUsesLambdaDefaultValue()
        {
            // Arrange
            HtmlHelper<FooModel> helper = MvcHelper.GetHtmlHelper(_dropDownListViewData);
            SelectList selectList = new SelectList(MultiSelectListTest.GetSampleStrings());

            // Act
            MvcHtmlString html = helper.DropDownListFor(m => m.foo, selectList);

            // Assert
            Assert.Equal(
                "<select id=\"foo\" name=\"foo\"><option>Alpha</option>" + Environment.NewLine
              + "<option selected=\"selected\">Bravo</option>" + Environment.NewLine
              + "<option>Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void DropDownListForUsesLambdaDefaultValueWithNullSelectListUsesViewData()
        {
            // Arrange
            FooModel model = new FooModel { foo = "Bravo" };
            ViewDataDictionary<FooModel> vdd = new ViewDataDictionary<FooModel>(model)
            {
                { "foo", new SelectList(MultiSelectListTest.GetSampleStrings()) }
            };
            HtmlHelper<FooModel> helper = MvcHelper.GetHtmlHelper(vdd);

            // Act
            MvcHtmlString html = helper.DropDownListFor(m => m.foo, selectList: null);

            // Assert
            Assert.Equal(
                "<select id=\"foo\" name=\"foo\"><option>Alpha</option>" + Environment.NewLine
              + "<option selected=\"selected\">Bravo</option>" + Environment.NewLine
              + "<option>Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void DropDownListForWithAttributesDictionary()
        {
            // Arrange
            HtmlHelper<FooModel> helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary<FooModel>());
            SelectList selectList = new SelectList(MultiSelectListTest.GetSampleStrings());

            // Act
            MvcHtmlString html = helper.DropDownListFor(m => m.foo, selectList, null /* optionLabel */, HtmlHelperTest.AttributesDictionary);

            // Assert
            Assert.Equal(
                "<select baz=\"BazValue\" id=\"foo\" name=\"foo\"><option>Alpha</option>" + Environment.NewLine
              + "<option>Bravo</option>" + Environment.NewLine
              + "<option>Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void DropDownListForWithErrors()
        {
            // Arrange
            HtmlHelper<FooBarModel> helper = MvcHelper.GetHtmlHelper(GetViewDataWithErrors());
            SelectList selectList = new SelectList(MultiSelectListTest.GetSampleStrings(), new[] { "Charlie" });

            // Act
            MvcHtmlString html = helper.DropDownListFor(m => m.foo, selectList, null /* optionLabel */, HtmlHelperTest.AttributesObjectDictionary);

            // Assert
            Assert.Equal(
                "<select baz=\"BazObjValue\" class=\"input-validation-error\" id=\"foo\" name=\"foo\"><option>Alpha</option>" + Environment.NewLine
              + "<option selected=\"selected\">Bravo</option>" + Environment.NewLine
              + "<option>Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void DropDownListForWithErrorsAndCustomClass()
        {
            // Arrange
            HtmlHelper<FooBarModel> helper = MvcHelper.GetHtmlHelper(GetViewDataWithErrors());
            SelectList selectList = new SelectList(MultiSelectListTest.GetSampleStrings());

            // Act
            MvcHtmlString html = helper.DropDownListFor(m => m.foo, selectList, null /* optionLabel */, new { @class = "foo-class" });

            // Assert
            Assert.Equal(
                "<select class=\"input-validation-error foo-class\" id=\"foo\" name=\"foo\"><option>Alpha</option>" + Environment.NewLine
              + "<option selected=\"selected\">Bravo</option>" + Environment.NewLine
              + "<option>Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void DropDownListForWithObjectDictionary()
        {
            // Arrange
            HtmlHelper<FooModel> helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary<FooModel>());
            SelectList selectList = new SelectList(MultiSelectListTest.GetSampleStrings());

            // Act
            MvcHtmlString html = helper.DropDownListFor(m => m.foo, selectList, null /* optionLabel */, HtmlHelperTest.AttributesObjectDictionary);

            // Assert
            Assert.Equal(
                "<select baz=\"BazObjValue\" id=\"foo\" name=\"foo\"><option>Alpha</option>" + Environment.NewLine
              + "<option>Bravo</option>" + Environment.NewLine
              + "<option>Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void DropDownListForWithObjectDictionaryWithUnderscores()
        {
            // Arrange
            HtmlHelper<FooModel> helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary<FooModel>());
            SelectList selectList = new SelectList(MultiSelectListTest.GetSampleStrings());

            // Act
            MvcHtmlString html = helper.DropDownListFor(m => m.foo, selectList, null /* optionLabel */, HtmlHelperTest.AttributesObjectUnderscoresDictionary);

            // Assert
            Assert.Equal(
                "<select foo-baz=\"BazObjValue\" id=\"foo\" name=\"foo\"><option>Alpha</option>" + Environment.NewLine
              + "<option>Bravo</option>" + Environment.NewLine
              + "<option>Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void DropDownListForWithObjectDictionaryAndSelectListNoOptionLabel()
        {
            // Arrange
            HtmlHelper<FooModel> helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary<FooModel>());
            SelectList selectList = new SelectList(MultiSelectListTest.GetSampleStrings());

            // Act
            MvcHtmlString html = helper.DropDownListFor(m => m.foo, selectList, HtmlHelperTest.AttributesObjectDictionary);

            // Assert
            Assert.Equal(
                "<select baz=\"BazObjValue\" id=\"foo\" name=\"foo\"><option>Alpha</option>" + Environment.NewLine
              + "<option>Bravo</option>" + Environment.NewLine
              + "<option>Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void DropDownListForWithObjectDictionaryWithUnderscoresAndSelectListNoOptionLabel()
        {
            // Arrange
            HtmlHelper<FooModel> helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary<FooModel>());
            SelectList selectList = new SelectList(MultiSelectListTest.GetSampleStrings());

            // Act
            MvcHtmlString html = helper.DropDownListFor(m => m.foo, selectList, HtmlHelperTest.AttributesObjectUnderscoresDictionary);

            // Assert
            Assert.Equal(
                "<select foo-baz=\"BazObjValue\" id=\"foo\" name=\"foo\"><option>Alpha</option>" + Environment.NewLine
              + "<option>Bravo</option>" + Environment.NewLine
              + "<option>Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void DropDownListForWithObjectDictionaryAndEmptyOptionLabel()
        {
            // Arrange
            HtmlHelper<FooModel> helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary<FooModel>());
            SelectList selectList = new SelectList(MultiSelectListTest.GetSampleStrings());

            // Act
            MvcHtmlString html = helper.DropDownListFor(m => m.foo, selectList, String.Empty /* optionLabel */, HtmlHelperTest.AttributesObjectDictionary);

            // Assert
            Assert.Equal(
                "<select baz=\"BazObjValue\" id=\"foo\" name=\"foo\"><option value=\"\"></option>" + Environment.NewLine
              + "<option>Alpha</option>" + Environment.NewLine
              + "<option>Bravo</option>" + Environment.NewLine
              + "<option>Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void DropDownListForWithObjectDictionaryAndTitle()
        {
            // Arrange
            HtmlHelper<FooModel> helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary<FooModel>());
            SelectList selectList = new SelectList(MultiSelectListTest.GetSampleStrings());

            // Act
            MvcHtmlString html = helper.DropDownListFor(m => m.foo, selectList, "[Select Something]", HtmlHelperTest.AttributesObjectDictionary);

            // Assert
            Assert.Equal(
                "<select baz=\"BazObjValue\" id=\"foo\" name=\"foo\"><option value=\"\">[Select Something]</option>" + Environment.NewLine
              + "<option>Alpha</option>" + Environment.NewLine
              + "<option>Bravo</option>" + Environment.NewLine
              + "<option>Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void DropDownListForWithIEnumerableSelectListItemSelectsDefaultFromViewData()
        {
            // Arrange
            ViewDataDictionary<FooModel> vdd = new ViewDataDictionary<FooModel> { { "foo", "123456789" } };
            HtmlHelper<FooModel> helper = MvcHelper.GetHtmlHelper(vdd);

            // Act
            MvcHtmlString html = helper.DropDownListFor(m => m.foo, MultiSelectListTest.GetSampleIEnumerableObjects());

            // Assert
            Assert.Equal(
                "<select id=\"foo\" name=\"foo\"><option selected=\"selected\" value=\"123456789\">John</option>" + Environment.NewLine
              + "<option value=\"987654321\">Jane</option>" + Environment.NewLine
              + "<option value=\"111111111\">Joe</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void DropDownListForWithListOfSelectListItemSelectsDefaultFromViewData()
        {
            // Arrange
            ViewDataDictionary<FooModel> vdd = new ViewDataDictionary<FooModel> { { "foo", "123456789" } };
            HtmlHelper<FooModel> helper = MvcHelper.GetHtmlHelper(vdd);

            // Act
            MvcHtmlString html = helper.DropDownListFor(m => m.foo, MultiSelectListTest.GetSampleListObjects());

            // Assert
            Assert.Equal(
                "<select id=\"foo\" name=\"foo\"><option selected=\"selected\" value=\"123456789\">John</option>" + Environment.NewLine
              + "<option value=\"987654321\">Jane</option>" + Environment.NewLine
              + "<option value=\"111111111\">Joe</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void DropDownListForWithPrefix()
        {
            // Arrange
            HtmlHelper<FooModel> helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary<FooModel>());
            SelectList selectList = new SelectList(MultiSelectListTest.GetSampleStrings());
            helper.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = "MyPrefix";

            // Act
            MvcHtmlString html = helper.DropDownListFor(m => m.foo, selectList, HtmlHelperTest.AttributesObjectDictionary);

            // Assert
            Assert.Equal(
                "<select baz=\"BazObjValue\" id=\"MyPrefix_foo\" name=\"MyPrefix.foo\"><option>Alpha</option>" + Environment.NewLine
              + "<option>Bravo</option>" + Environment.NewLine
              + "<option>Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void DropDownListForWithPrefixAndEmptyName()
        {
            // Arrange
            HtmlHelper<FooModel> helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary<FooModel>());
            SelectList selectList = new SelectList(MultiSelectListTest.GetSampleStrings());
            helper.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = "MyPrefix";

            // Act
            MvcHtmlString html = helper.DropDownListFor(m => m, selectList, HtmlHelperTest.AttributesObjectDictionary);

            // Assert
            Assert.Equal(
                "<select baz=\"BazObjValue\" id=\"MyPrefix\" name=\"MyPrefix\"><option>Alpha</option>" + Environment.NewLine
              + "<option>Bravo</option>" + Environment.NewLine
              + "<option>Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void DropDownListForWithPrefixAndIEnumerableSelectListItemSelectsDefaultFromViewData()
        {
            // Arrange
            ViewDataDictionary<FooModel> vdd = new ViewDataDictionary<FooModel> { { "foo", "123456789" } };
            vdd.TemplateInfo.HtmlFieldPrefix = "MyPrefix";
            HtmlHelper<FooModel> helper = MvcHelper.GetHtmlHelper(vdd);

            // Act
            MvcHtmlString html = helper.DropDownListFor(m => m.foo, MultiSelectListTest.GetSampleIEnumerableObjects());

            // Assert
            Assert.Equal(
                "<select id=\"MyPrefix_foo\" name=\"MyPrefix.foo\"><option selected=\"selected\" value=\"123456789\">John</option>" + Environment.NewLine
              + "<option value=\"987654321\">Jane</option>" + Environment.NewLine
              + "<option value=\"111111111\">Joe</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        // EnumDropDownListFor

        [Fact]
        public void EnumDropDownListForWithNullExpressionThrowsArgumentNull()
        {
            // Arrange
            HtmlHelper<EnumWithDisplay> helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary<EnumWithDisplay>());

            // Act & Assert
            Assert.ThrowsArgumentNull(() => helper.EnumDropDownListFor<EnumWithDisplay, EnumWithDisplay>(null),
                "expression");
        }

        [Fact]
        public void EnumDropDownListForWithUnsupportedExpressionThrowsInvalidOperation()
        {
            // Arrange
            IEnumerable<EnumModel> model = new List<EnumModel>
            {
                new EnumModel { WithDisplay = EnumWithDisplay.One, },
                new EnumModel { WithDisplay = EnumWithDisplay.Two, },
            };
            ViewDataDictionary<IEnumerable<EnumModel>> viewData = new ViewDataDictionary<IEnumerable<EnumModel>>(model);
            HtmlHelper<IEnumerable<EnumModel>> helper = MvcHelper.GetHtmlHelper(viewData);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => helper.EnumDropDownListFor(m => m.Select((item) => item.WithDisplay).First()),
                exceptionMessage: "Templates can be used only with field access, property access, " +
                "single-dimension array index, or single-parameter custom indexer expressions.");
        }

        [Fact]
        public void EnumDropDownListForWithIntExpressionTypeThrowsArgument()
        {
            // Arrange
            ViewDataDictionary<int> viewData = new ViewDataDictionary<int>
            {
                TemplateInfo = new TemplateInfo { HtmlFieldPrefix = "MyExpression", },
            };
            HtmlHelper<int> helper = MvcHelper.GetHtmlHelper(viewData);

            // Act & Assert
            Assert.ThrowsArgument(() => helper.EnumDropDownListFor(model => model), paramName: "expression",
                exceptionMessage: "Return type 'System.Int32' is not supported." + Environment.NewLine +
                "Parameter name: expression");
        }

        [Fact]
        public void EnumDropDownListForWithUnsupportedExpressionTypeThrowsArgument()
        {
            // Arrange
            ViewDataDictionary<EnumWithFlags> viewData = new ViewDataDictionary<EnumWithFlags>
            {
                TemplateInfo = new TemplateInfo { HtmlFieldPrefix = "MyExpression", },
            };
            HtmlHelper<EnumWithFlags> helper = MvcHelper.GetHtmlHelper(viewData);

            // Act & Assert
            Assert.ThrowsArgument(() => helper.EnumDropDownListFor(model => model), paramName: "expression",
                exceptionMessage: "Return type 'System.Web.Mvc.Html.Test.SelectExtensionsTest+EnumWithFlags' is not " +
                "supported. Type must not have a 'Flags' attribute." + Environment.NewLine +
                "Parameter name: expression");
        }

        // Like EnumDropDownListForWithUnsupportedExpressionTypeThrowsArgument but using EnumModel
        [Fact]
        public void EnumDropDownListForWithUnsupportedPropertyTypeThrowsArgument()
        {
            // Arrange
            HtmlHelper<EnumModel> helper = MvcHelper.GetHtmlHelper(_enumDropDownListViewData);

            // Act & Assert
            Assert.ThrowsArgument(() => helper.EnumDropDownListFor(m => m.WithFlags), paramName: "expression",
                exceptionMessage: "Return type 'System.Web.Mvc.Html.Test.SelectExtensionsTest+EnumWithFlags' is not " +
                "supported. Type must not have a 'Flags' attribute." + Environment.NewLine +
                "Parameter name: expression");
        }

        [Fact]
        public void EnumDropDownListforIsSuccessfulIfNoValueProvided()
        {
            // Arrange
            ViewDataDictionary<EnumWithDisplay> viewData = new ViewDataDictionary<EnumWithDisplay>
            {
                TemplateInfo = new TemplateInfo { HtmlFieldPrefix = "MyExpression", },
            };
            HtmlHelper<EnumWithDisplay> helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            MvcHtmlString html = helper.EnumDropDownListFor(model => model, optionLabel: null);

            // Assert
            Assert.Equal(
                "<select id=\"MyExpression\" name=\"MyExpression\">" +
                "<option selected=\"selected\" value=\"0\">First</option>" + Environment.NewLine +
                "<option value=\"1\">Second</option>" + Environment.NewLine +
                "<option value=\"2\">Third</option>" + Environment.NewLine +
                "<option value=\"3\">Fourth</option>" + Environment.NewLine +
                "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void EnumDropDownListForIsSuccessfulIfNoValueProvidedAndEnumEmpty()
        {
            // Arrange
            ViewDataDictionary<EnumWithoutAnything> viewData = new ViewDataDictionary<EnumWithoutAnything>
            {
                TemplateInfo = new TemplateInfo { HtmlFieldPrefix = "MyExpression", },
            };
            HtmlHelper<EnumWithoutAnything> helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            MvcHtmlString html = helper.EnumDropDownListFor(model => model, optionLabel: null);

            // Assert
            Assert.Equal(
                "<select id=\"MyExpression\" name=\"MyExpression\">" +
                "<option selected=\"selected\" value=\"0\"></option>" + Environment.NewLine +
                "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void EnumDropDownListForUsesModelValueIfNotProvidedInViewData()
        {
            // Arrange
            ViewDataDictionary<EnumWithDisplay> viewData = new ViewDataDictionary<EnumWithDisplay>(EnumWithDisplay.Two)
            {
                TemplateInfo = new TemplateInfo { HtmlFieldPrefix = "MyExpression", },
            };
            HtmlHelper<EnumWithDisplay> helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            MvcHtmlString html = helper.EnumDropDownListFor(model => model, optionLabel: null);

            // Assert
            Assert.Equal(
                "<select id=\"MyExpression\" name=\"MyExpression\">" +
                "<option value=\"0\">First</option>" + Environment.NewLine +
                "<option value=\"1\">Second</option>" + Environment.NewLine +
                "<option selected=\"selected\" value=\"2\">Third</option>" + Environment.NewLine +
                "<option value=\"3\">Fourth</option>" + Environment.NewLine +
                "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void EnumDropDownListForUsesModelValueIfNotProvidedInViewData_EnumModel()
        {
            // Arrange
            EnumModel model = new EnumModel { WithDisplay = EnumWithDisplay.Two, };
            HtmlHelper<EnumModel> helper =
                MvcHelper.GetHtmlHelper(new ViewDataDictionary<EnumModel>(model));

            // Act
            MvcHtmlString html = helper.EnumDropDownListFor(m => m.WithDisplay, optionLabel: null);

            // Assert
            Assert.Equal(
                "<select id=\"WithDisplay\" name=\"WithDisplay\">" +
                "<option value=\"0\">First</option>" + Environment.NewLine +
                "<option value=\"1\">Second</option>" + Environment.NewLine +
                "<option selected=\"selected\" value=\"2\">Third</option>" + Environment.NewLine +
                "<option value=\"3\">Fourth</option>" + Environment.NewLine +
                "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void EnumDropDownListForUsesModelValueIfNotProvidedInViewData_EnumWithoutAnything()
        {
            // Arrange
            EnumModel model = new EnumModel { WithoutAnything = (EnumWithoutAnything)23, };
            HtmlHelper<EnumModel> helper =
                MvcHelper.GetHtmlHelper(new ViewDataDictionary<EnumModel>(model));

            // Act
            MvcHtmlString html = helper.EnumDropDownListFor(m => m.WithoutAnything, optionLabel: "My Label");

            // Assert
            Assert.Equal(
                "<select id=\"WithoutAnything\" name=\"WithoutAnything\">" +
                "<option selected=\"selected\" value=\"23\">My Label</option>" + Environment.NewLine +
                "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void EnumDropDownListForUsesModelValueIfNotProvidedInViewData_Unobtrusive()
        {
            // Arrange
            EnumModel model = new EnumModel { WithDisplay = EnumWithDisplay.Two, };
            HtmlHelper<EnumModel> helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary<EnumModel>(model));
            helper.ViewContext.ClientValidationEnabled = true;
            helper.ViewContext.UnobtrusiveJavaScriptEnabled = true;
            helper.ViewContext.FormContext = new FormContext();
            helper.ClientValidationRuleFactory = (name, metadata) =>
                new[] { new ModelClientValidationRule { ValidationType = "type", ErrorMessage = "error" } };

            // Act
            MvcHtmlString html = helper.EnumDropDownListFor(m => m.WithDisplay, optionLabel: null);

            // Assert
            Assert.Equal(
                "<select data-val=\"true\" data-val-type=\"error\" id=\"WithDisplay\" name=\"WithDisplay\">" +
                "<option value=\"0\">First</option>" + Environment.NewLine +
                "<option value=\"1\">Second</option>" + Environment.NewLine +
                "<option selected=\"selected\" value=\"2\">Third</option>" + Environment.NewLine +
                "<option value=\"3\">Fourth</option>" + Environment.NewLine +
                "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void EnumDropDownListForUsesViewDataDefaultValue()
        {
            // Arrange
            HtmlHelper<EnumModel> helper = MvcHelper.GetHtmlHelper(_enumDropDownListViewData);

            // Act
            MvcHtmlString html = helper.EnumDropDownListFor(m => m.WithDisplay, optionLabel: null);

            // Assert
            Assert.Equal(
                "<select id=\"WithDisplay\" name=\"WithDisplay\">" +
                "<option value=\"0\">First</option>" + Environment.NewLine +
                "<option value=\"1\">Second</option>" + Environment.NewLine +
                "<option selected=\"selected\" value=\"2\">Third</option>" + Environment.NewLine +
                "<option value=\"3\">Fourth</option>" + Environment.NewLine +
                "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void EnumDropDownListForUsesViewDataDefaultValue_Duplicates()
        {
            // Arrange
            HtmlHelper<EnumModel> helper = MvcHelper.GetHtmlHelper(_enumDropDownListViewData);

            // Act
            MvcHtmlString html = helper.EnumDropDownListFor(m => m.WithDuplicates, optionLabel: null);

            // Assert
            // TODO: https://aspnetwebstack.codeplex.com/workitem/1349 covers incorrect multi-select in this case
            Assert.Equal(
                "<select id=\"WithDuplicates\" name=\"WithDuplicates\">" +
                "<option value=\"0\">First</option>" + Environment.NewLine +
                "<option selected=\"selected\" value=\"1\">Second</option>" + Environment.NewLine +
                "<option selected=\"selected\" value=\"1\">Third</option>" + Environment.NewLine +
                "<option value=\"2\">Fourth</option>" + Environment.NewLine +
                "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void EnumDropDownListForUsesViewDataDefaultValue_Unrecognized()
        {
            // Arrange
            ViewDataDictionary<EnumModel> viewData = new ViewDataDictionary<EnumModel>(_enumDropDownListViewData);
            viewData["WithDisplay"] = (EnumWithDisplay)34;
            HtmlHelper<EnumModel> helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            MvcHtmlString html = helper.EnumDropDownListFor(m => m.WithDisplay, optionLabel: null);

            // Assert
            Assert.Equal(
                "<select id=\"WithDisplay\" name=\"WithDisplay\">" +
                "<option selected=\"selected\" value=\"34\"></option>" + Environment.NewLine +
                "<option value=\"0\">First</option>" + Environment.NewLine +
                "<option value=\"1\">Second</option>" + Environment.NewLine +
                "<option value=\"2\">Third</option>" + Environment.NewLine +
                "<option value=\"3\">Fourth</option>" + Environment.NewLine +
                "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void EnumDropDownListForUsesViewDataDefaultValue_NullableIsNull()
        {
            // Arrange
            HtmlHelper<EnumModel> helper = MvcHelper.GetHtmlHelper(_enumDropDownListViewData);

            // Act
            MvcHtmlString html = helper.EnumDropDownListFor(m => m.WithNullable, optionLabel: null);

            // Assert
            Assert.Equal(
                "<select id=\"WithNullable\" name=\"WithNullable\">" +
                "<option selected=\"selected\" value=\"\"></option>" + Environment.NewLine +
                "<option value=\"0\">First</option>" + Environment.NewLine +
                "<option value=\"1\">Second</option>" + Environment.NewLine +
                "<option value=\"2\">Third</option>" + Environment.NewLine +
                "<option value=\"3\">Fourth</option>" + Environment.NewLine +
                "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void EnumDropDownListForUsesViewDataDefaultValue_NullableNotNull()
        {
            // Arrange
            ViewDataDictionary<EnumModel> viewData = new ViewDataDictionary<EnumModel>(_enumDropDownListViewData);
            viewData["WithNullable"] = EnumWithDisplay.Three;
            HtmlHelper<EnumModel> helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            MvcHtmlString html = helper.EnumDropDownListFor(m => m.WithNullable, optionLabel: null);

            // Assert
            Assert.Equal(
                "<select id=\"WithNullable\" name=\"WithNullable\">" +
                "<option value=\"\"></option>" + Environment.NewLine +
                "<option value=\"0\">First</option>" + Environment.NewLine +
                "<option value=\"1\">Second</option>" + Environment.NewLine +
                "<option value=\"2\">Third</option>" + Environment.NewLine +
                "<option selected=\"selected\" value=\"3\">Fourth</option>" + Environment.NewLine +
                "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void EnumDropDownListForUsesViewDataDefaultValue_NullableUnrecognized()
        {
            // Arrange
            ViewDataDictionary<EnumModel> viewData = new ViewDataDictionary<EnumModel>(_enumDropDownListViewData);
            viewData["WithNullable"] = (EnumWithDisplay)34;
            HtmlHelper<EnumModel> helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            MvcHtmlString html = helper.EnumDropDownListFor(m => m.WithNullable, optionLabel: null);

            // Assert
            Assert.Equal(
                "<select id=\"WithNullable\" name=\"WithNullable\">" +
                "<option selected=\"selected\" value=\"34\"></option>" + Environment.NewLine +
                "<option value=\"0\">First</option>" + Environment.NewLine +
                "<option value=\"1\">Second</option>" + Environment.NewLine +
                "<option value=\"2\">Third</option>" + Environment.NewLine +
                "<option value=\"3\">Fourth</option>" + Environment.NewLine +
                "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void EnumDropDownListForUsesViewDataDefaultValueNoOptionLabel()
        {
            // Arrange
            HtmlHelper<EnumModel> helper = MvcHelper.GetHtmlHelper(_enumDropDownListViewData);

            // Act
            MvcHtmlString html = helper.EnumDropDownListFor(m => m.WithDisplay);

            // Assert
            Assert.Equal(
                "<select id=\"WithDisplay\" name=\"WithDisplay\">" +
                "<option value=\"0\">First</option>" + Environment.NewLine +
                "<option value=\"1\">Second</option>" + Environment.NewLine +
                "<option selected=\"selected\" value=\"2\">Third</option>" + Environment.NewLine +
                "<option value=\"3\">Fourth</option>" + Environment.NewLine +
                "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void EnumDropDownListForWithTitle()
        {
            // Arrange
            HtmlHelper<EnumModel> helper = MvcHelper.GetHtmlHelper(_enumDropDownListViewData);

            // Act
            MvcHtmlString html = helper.EnumDropDownListFor(m => m.WithDisplay, optionLabel: "[Select Something]");

            // Assert
            Assert.Equal(
                "<select id=\"WithDisplay\" name=\"WithDisplay\">" +
                "<option value=\"\">[Select Something]</option>" + Environment.NewLine +
                "<option value=\"0\">First</option>" + Environment.NewLine +
                "<option value=\"1\">Second</option>" + Environment.NewLine +
                "<option selected=\"selected\" value=\"2\">Third</option>" + Environment.NewLine +
                "<option value=\"3\">Fourth</option>" + Environment.NewLine +
                "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void EnumDropDownListForWithTitle_Unrecognized()
        {
            // Arrange
            ViewDataDictionary<EnumModel> viewData = new ViewDataDictionary<EnumModel>(_enumDropDownListViewData);
            viewData["WithDisplay"] = (EnumWithDisplay)34;
            HtmlHelper<EnumModel> helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            MvcHtmlString html = helper.EnumDropDownListFor(m => m.WithDisplay, optionLabel: "[Select Something]");

            // Assert
            Assert.Equal(
                "<select id=\"WithDisplay\" name=\"WithDisplay\">" +
                "<option selected=\"selected\" value=\"34\">[Select Something]</option>" + Environment.NewLine +
                "<option value=\"0\">First</option>" + Environment.NewLine +
                "<option value=\"1\">Second</option>" + Environment.NewLine +
                "<option value=\"2\">Third</option>" + Environment.NewLine +
                "<option value=\"3\">Fourth</option>" + Environment.NewLine +
                "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void EnumDropDownListForWithTitle_NullableNull()
        {
            // Arrange
            HtmlHelper<EnumModel> helper = MvcHelper.GetHtmlHelper(_enumDropDownListViewData);

            // Act
            MvcHtmlString html = helper.EnumDropDownListFor(m => m.WithNullable, optionLabel: "[Select Something]");

            // Assert
            Assert.Equal(
                "<select id=\"WithNullable\" name=\"WithNullable\">" +
                "<option selected=\"selected\" value=\"\">[Select Something]</option>" + Environment.NewLine +
                "<option value=\"0\">First</option>" + Environment.NewLine +
                "<option value=\"1\">Second</option>" + Environment.NewLine +
                "<option value=\"2\">Third</option>" + Environment.NewLine +
                "<option value=\"3\">Fourth</option>" + Environment.NewLine +
                "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void EnumDropDownListForWithTitle_NullableNotNull()
        {
            // Arrange
            ViewDataDictionary<EnumModel> viewData = new ViewDataDictionary<EnumModel>(_enumDropDownListViewData);
            viewData["WithNullable"] = EnumWithDisplay.Three;
            HtmlHelper<EnumModel> helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            MvcHtmlString html = helper.EnumDropDownListFor(m => m.WithNullable, optionLabel: "[Select Something]");

            // Assert
            Assert.Equal(
                "<select id=\"WithNullable\" name=\"WithNullable\">" +
                "<option value=\"\">[Select Something]</option>" + Environment.NewLine +
                "<option value=\"0\">First</option>" + Environment.NewLine +
                "<option value=\"1\">Second</option>" + Environment.NewLine +
                "<option value=\"2\">Third</option>" + Environment.NewLine +
                "<option selected=\"selected\" value=\"3\">Fourth</option>" + Environment.NewLine +
                "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void EnumDropDownListForWithTitle_NullableUnrecognized()
        {
            // Arrange
            ViewDataDictionary<EnumModel> viewData = new ViewDataDictionary<EnumModel>(_enumDropDownListViewData);
            viewData["WithNullable"] = (EnumWithDisplay)34;
            HtmlHelper<EnumModel> helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            MvcHtmlString html = helper.EnumDropDownListFor(m => m.WithNullable, optionLabel: "[Select Something]");

            // Assert
            Assert.Equal(
                "<select id=\"WithNullable\" name=\"WithNullable\">" +
                "<option selected=\"selected\" value=\"34\">[Select Something]</option>" + Environment.NewLine +
                "<option value=\"0\">First</option>" + Environment.NewLine +
                "<option value=\"1\">Second</option>" + Environment.NewLine +
                "<option value=\"2\">Third</option>" + Environment.NewLine +
                "<option value=\"3\">Fourth</option>" + Environment.NewLine +
                "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void EnumDropDownListForWithAttributesDictionary()
        {
            // Arrange
            HtmlHelper<EnumModel> helper = MvcHelper.GetHtmlHelper(_enumDropDownListViewData);

            // Act
            MvcHtmlString html = helper.EnumDropDownListFor(m => m.WithDisplay,
                htmlAttributes: HtmlHelperTest.AttributesDictionary);

            // Assert
            Assert.Equal(
                "<select baz=\"BazValue\" id=\"WithDisplay\" name=\"WithDisplay\">" +
                "<option value=\"0\">First</option>" + Environment.NewLine +
                "<option value=\"1\">Second</option>" + Environment.NewLine +
                "<option selected=\"selected\" value=\"2\">Third</option>" + Environment.NewLine +
                "<option value=\"3\">Fourth</option>" + Environment.NewLine +
                "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void EnumDropDownListForWithAttributesDictionaryAndTitle()
        {
            // Arrange
            HtmlHelper<EnumModel> helper = MvcHelper.GetHtmlHelper(_enumDropDownListViewData);

            // Act
            MvcHtmlString html = helper.EnumDropDownListFor(m => m.WithDisplay, optionLabel: "[Select Something]",
                htmlAttributes: HtmlHelperTest.AttributesDictionary);

            // Assert
            Assert.Equal(
                "<select baz=\"BazValue\" id=\"WithDisplay\" name=\"WithDisplay\">" +
                "<option value=\"\">[Select Something]</option>" + Environment.NewLine +
                "<option value=\"0\">First</option>" + Environment.NewLine +
                "<option value=\"1\">Second</option>" + Environment.NewLine +
                "<option selected=\"selected\" value=\"2\">Third</option>" + Environment.NewLine +
                "<option value=\"3\">Fourth</option>" + Environment.NewLine +
                "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void EnumDropDownListForWithErrors()
        {
            // Arrange
            ViewDataDictionary<EnumModel> viewData = new ViewDataDictionary<EnumModel>(_enumDropDownListViewData);
            ModelState modelState = new ModelState
            {
                Errors = { new ModelError("WithDisplay error 1"), new ModelError("WithDisplay error 2"), },
            };
            viewData.ModelState["WithDisplay"] = modelState;

            HtmlHelper<EnumModel> helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            MvcHtmlString html = helper.EnumDropDownListFor(m => m.WithDisplay,
                htmlAttributes: HtmlHelperTest.AttributesObjectDictionary);

            // Assert
            Assert.Equal(
                "<select baz=\"BazObjValue\" class=\"input-validation-error\" id=\"WithDisplay\" name=\"WithDisplay\">" +
                "<option value=\"0\">First</option>" + Environment.NewLine +
                "<option value=\"1\">Second</option>" + Environment.NewLine +
                "<option selected=\"selected\" value=\"2\">Third</option>" + Environment.NewLine +
                "<option value=\"3\">Fourth</option>" + Environment.NewLine +
                "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void EnumDropDownListForWithErrorsAndValue()
        {
            // Arrange
            ViewDataDictionary<EnumModel> viewData = new ViewDataDictionary<EnumModel>(_enumDropDownListViewData);
            ModelState modelState = new ModelState
            {
                Errors = { new ModelError("WithDisplay error 1"), new ModelError("WithDisplay error 2"), },
                Value = new ValueProviderResult(new string[] { "1", }, "1", null),
            };
            viewData.ModelState["WithDisplay"] = modelState;

            HtmlHelper<EnumModel> helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            MvcHtmlString html = helper.EnumDropDownListFor(m => m.WithDisplay,
                htmlAttributes: HtmlHelperTest.AttributesObjectDictionary);

            // Assert
            Assert.Equal(
                "<select baz=\"BazObjValue\" class=\"input-validation-error\" id=\"WithDisplay\" name=\"WithDisplay\">" +
                "<option value=\"0\">First</option>" + Environment.NewLine +
                "<option selected=\"selected\" value=\"1\">Second</option>" + Environment.NewLine +
                "<option value=\"2\">Third</option>" + Environment.NewLine +
                "<option value=\"3\">Fourth</option>" + Environment.NewLine +
                "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void EnumDropDownListForWithObjectDictionaryWithUnderscores()
        {
            // Arrange
            HtmlHelper<EnumModel> helper = MvcHelper.GetHtmlHelper(_enumDropDownListViewData);

            // Act
            MvcHtmlString html = helper.EnumDropDownListFor(m => m.WithDisplay, optionLabel: null,
                htmlAttributes: HtmlHelperTest.AttributesObjectUnderscoresDictionary);

            // Assert
            Assert.Equal(
                "<select foo-baz=\"BazObjValue\" id=\"WithDisplay\" name=\"WithDisplay\">" +
                "<option value=\"0\">First</option>" + Environment.NewLine +
                "<option value=\"1\">Second</option>" + Environment.NewLine +
                "<option selected=\"selected\" value=\"2\">Third</option>" + Environment.NewLine +
                "<option value=\"3\">Fourth</option>" + Environment.NewLine +
                "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void EnumDropDownListForWithObjectDictionaryWithUnderscoresNoOptionLabel()
        {
            // Arrange
            HtmlHelper<EnumModel> helper = MvcHelper.GetHtmlHelper(_enumDropDownListViewData);

            // Act
            MvcHtmlString html = helper.EnumDropDownListFor(m => m.WithDisplay,
                htmlAttributes: HtmlHelperTest.AttributesObjectUnderscoresDictionary);

            // Assert
            Assert.Equal(
                "<select foo-baz=\"BazObjValue\" id=\"WithDisplay\" name=\"WithDisplay\">" +
                "<option value=\"0\">First</option>" + Environment.NewLine +
                "<option value=\"1\">Second</option>" + Environment.NewLine +
                "<option selected=\"selected\" value=\"2\">Third</option>" + Environment.NewLine +
                "<option value=\"3\">Fourth</option>" + Environment.NewLine +
                "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void EnumDropDownListForWithObjectDictionaryAndEmptyOptionLabel()
        {
            // Arrange
            HtmlHelper<EnumModel> helper = MvcHelper.GetHtmlHelper(_enumDropDownListViewData);

            // Act
            MvcHtmlString html = helper.EnumDropDownListFor(m => m.WithDisplay, optionLabel: String.Empty,
                htmlAttributes: HtmlHelperTest.AttributesObjectDictionary);

            // Assert
            Assert.Equal(
                "<select baz=\"BazObjValue\" id=\"WithDisplay\" name=\"WithDisplay\">" +
                "<option value=\"\"></option>" + Environment.NewLine +
                "<option value=\"0\">First</option>" + Environment.NewLine +
                "<option value=\"1\">Second</option>" + Environment.NewLine +
                "<option selected=\"selected\" value=\"2\">Third</option>" + Environment.NewLine +
                "<option value=\"3\">Fourth</option>" + Environment.NewLine +
                "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void EnumDropDownListForWithObjectDictionaryAndTitle()
        {
            // Arrange
            HtmlHelper<EnumModel> helper = MvcHelper.GetHtmlHelper(_enumDropDownListViewData);

            // Act
            MvcHtmlString html = helper.EnumDropDownListFor(m => m.WithDisplay, optionLabel: "[Select Something]",
                htmlAttributes: HtmlHelperTest.AttributesObjectDictionary);

            // Assert
            Assert.Equal(
                "<select baz=\"BazObjValue\" id=\"WithDisplay\" name=\"WithDisplay\">" +
                "<option value=\"\">[Select Something]</option>" + Environment.NewLine +
                "<option value=\"0\">First</option>" + Environment.NewLine +
                "<option value=\"1\">Second</option>" + Environment.NewLine +
                "<option selected=\"selected\" value=\"2\">Third</option>" + Environment.NewLine +
                "<option value=\"3\">Fourth</option>" + Environment.NewLine +
                "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void EnumDropDownListForWithPrefix()
        {
            // Arrange
            ViewDataDictionary<EnumModel> viewData = new ViewDataDictionary<EnumModel>(_enumDropDownListViewData)
            {
                TemplateInfo = new TemplateInfo { HtmlFieldPrefix = "MyPrefix", },
            };
            HtmlHelper<EnumModel> helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            MvcHtmlString html = helper.EnumDropDownListFor(m => m.WithDisplay,
                htmlAttributes: HtmlHelperTest.AttributesObjectDictionary);

            // Assert
            Assert.Equal(
                "<select baz=\"BazObjValue\" id=\"MyPrefix_WithDisplay\" name=\"MyPrefix.WithDisplay\">" +
                "<option value=\"0\">First</option>" + Environment.NewLine +
                "<option value=\"1\">Second</option>" + Environment.NewLine +
                "<option selected=\"selected\" value=\"2\">Third</option>" + Environment.NewLine +
                "<option value=\"3\">Fourth</option>" + Environment.NewLine +
                "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void EnumDropDownListForWithPrefixAndEmptyName()
        {
            // Arrange
            ViewDataDictionary<EnumWithDisplay> viewData = new ViewDataDictionary<EnumWithDisplay>(EnumWithDisplay.Two)
            {
                TemplateInfo = new TemplateInfo { HtmlFieldPrefix = "MyPrefix", },
            };
            HtmlHelper<EnumWithDisplay> helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            MvcHtmlString html = helper.EnumDropDownListFor(m => m,
                htmlAttributes: HtmlHelperTest.AttributesObjectDictionary);

            // Assert
            Assert.Equal(
                "<select baz=\"BazObjValue\" id=\"MyPrefix\" name=\"MyPrefix\">" +
                "<option value=\"0\">First</option>" + Environment.NewLine +
                "<option value=\"1\">Second</option>" + Environment.NewLine +
                "<option selected=\"selected\" value=\"2\">Third</option>" + Environment.NewLine +
                "<option value=\"3\">Fourth</option>" + Environment.NewLine +
                "</select>",
                html.ToHtmlString());
        }

        // ListBox

        [Fact]
        void ListBox_WithGroups()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper();
            const string expectedListBox = @"<select id=""List"" multiple=""multiple"" name=""List""><option value=""other1"">other1</option>
<option value=""other2"">other2</option>
<optgroup label=""Swedish Cars"">
<option value=""volvo"">Volvo</option>
<option selected=""selected"" value=""saab"">Saab</option>
</optgroup>
<option value=""other3"">other3</option>
<optgroup>
<option value=""other4"">other4</option>
<option value=""other5"">other5</option>
</optgroup>
<optgroup label=""German Cars"">
<option value=""mercedes-benz"">Mercedes-Benz</option>
<option disabled=""disabled"" value=""audi"">Audi</option>
</optgroup>
<option value=""other6"">other6</option>
</select>";

            // Act
            MvcHtmlString html = helper.ListBox("List", GroupedItems);

            // Assert
            Assert.Equal(expectedListBox, html.ToHtmlString());
        }

        [Fact]
        void ListBox_WithGroups_WithDisabled()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper();
            const string expectedListBox = @"<select id=""List"" multiple=""multiple"" name=""List""><option value=""a"">Alice</option>
<optgroup disabled=""disabled"" label=""DisabledGroup"">
<option value=""b"">Bob</option>
<option value=""c"">Charlie</option>
</optgroup>
<option disabled=""disabled"" value=""d"">David</option>
</select>";

            // Act
            MvcHtmlString html = helper.ListBox("List", GroupedItems_WithDisabled);

            // Assert
            Assert.Equal(expectedListBox, html.ToHtmlString());
        }

        [Fact]
        void ListBox_MultiSelectList_WithGroups()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper();
            const string expectedListBox = @"<select id=""List"" multiple=""multiple"" name=""List""><option value=""ufo"">UFO</option>
<optgroup label=""Swedish Cars"">
<option selected=""selected"" value=""volvo"">Volvo</option>
<option value=""saab"">Saab</option>
</optgroup>
<optgroup label=""German Cars"">
<option value=""mercedes-benz"">Mercedes-Benz</option>
<option selected=""selected"" value=""audi"">Audi</option>
</optgroup>
<option value=""other"">Other</option>
<optgroup label="" "">
<option value=""unknown"">Unknown</option>
</optgroup>
</select>";

            // Act
            MvcHtmlString html = helper.ListBox("List", _multiSelectList);

            // Assert
            Assert.Equal(expectedListBox, html.ToHtmlString());
        }

        [Fact]
        public void ListBoxUsesExplicitValueIfNotProvidedInViewData()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary());
            MultiSelectList selectList = new MultiSelectList(MultiSelectListTest.GetSampleAnonymousObjects(), "Letter", "FullWord", new[] { "A", "C" });

            // Act
            MvcHtmlString html = helper.ListBox("foo", selectList);

            // Assert
            Assert.Equal(
                "<select id=\"foo\" multiple=\"multiple\" name=\"foo\"><option selected=\"selected\" value=\"A\">Alpha</option>" + Environment.NewLine
              + "<option value=\"B\">Bravo</option>" + Environment.NewLine
              + "<option selected=\"selected\" value=\"C\">Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void ListBoxUsesExplicitValueIfNotProvidedInViewData_Unobtrusive()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary());
            helper.ViewContext.ClientValidationEnabled = true;
            helper.ViewContext.UnobtrusiveJavaScriptEnabled = true;
            helper.ViewContext.FormContext = new FormContext();
            helper.ClientValidationRuleFactory = (name, metadata) => new[] { new ModelClientValidationRule { ValidationType = "type", ErrorMessage = "error" } };
            MultiSelectList selectList = new MultiSelectList(MultiSelectListTest.GetSampleAnonymousObjects(), "Letter", "FullWord", new[] { "A", "C" });

            // Act
            MvcHtmlString html = helper.ListBox("foo", selectList);

            // Assert
            Assert.Equal(
                "<select data-val=\"true\" data-val-type=\"error\" id=\"foo\" multiple=\"multiple\" name=\"foo\"><option selected=\"selected\" value=\"A\">Alpha</option>" + Environment.NewLine
              + "<option value=\"B\">Bravo</option>" + Environment.NewLine
              + "<option selected=\"selected\" value=\"C\">Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void ListBoxUsesViewDataDefaultValue()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(_listBoxViewData);
            MultiSelectList selectList = new MultiSelectList(MultiSelectListTest.GetSampleStrings(), new[] { "Charlie" });

            // Act
            MvcHtmlString html = helper.ListBox("foo", selectList);

            // Assert
            Assert.Equal(
                "<select id=\"foo\" multiple=\"multiple\" name=\"foo\"><option>Alpha</option>" + Environment.NewLine
              + "<option selected=\"selected\">Bravo</option>" + Environment.NewLine
              + "<option>Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void ListBoxWithErrors()
        {
            // Arrange
            ViewDataDictionary viewData = GetViewDataWithErrors();
            HtmlHelper helper = MvcHelper.GetHtmlHelper(viewData);
            MultiSelectList list = new MultiSelectList(MultiSelectListTest.GetSampleStrings(), new[] { "Charlie" });

            // Act
            MvcHtmlString html = helper.ListBox("foo", list);

            // Assert
            Assert.Equal(
                "<select class=\"input-validation-error\" id=\"foo\" multiple=\"multiple\" name=\"foo\"><option>Alpha</option>" + Environment.NewLine
              + "<option selected=\"selected\">Bravo</option>" + Environment.NewLine
              + "<option selected=\"selected\">Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void ListBoxWithErrorsAndCustomClass()
        {
            // Arrange
            ViewDataDictionary viewData = GetViewDataWithErrors();
            HtmlHelper helper = MvcHelper.GetHtmlHelper(viewData);
            MultiSelectList selectList = new MultiSelectList(MultiSelectListTest.GetSampleStrings(), new[] { "Charlie" });

            // Act
            MvcHtmlString html = helper.ListBox("foo", selectList, new { @class = "foo-class" });

            // Assert
            Assert.Equal(
                "<select class=\"input-validation-error foo-class\" id=\"foo\" multiple=\"multiple\" name=\"foo\"><option>Alpha</option>" + Environment.NewLine
              + "<option selected=\"selected\">Bravo</option>" + Environment.NewLine
              + "<option selected=\"selected\">Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void ListBoxWithNameOnly()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper();
            helper.ViewData["foo"] = new MultiSelectList(MultiSelectListTest.GetSampleStrings(), new[] { "Charlie" });

            // Act
            MvcHtmlString html = helper.ListBox("foo");

            // Assert
            Assert.Equal(
                "<select id=\"foo\" multiple=\"multiple\" name=\"foo\"><option>Alpha</option>" + Environment.NewLine
              + "<option>Bravo</option>" + Environment.NewLine
              + "<option selected=\"selected\">Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void ListBoxWithAttributesDictionary()
        {
            // Arrange
            ViewDataDictionary viewData = new ViewDataDictionary();
            MultiSelectList selectList = new MultiSelectList(MultiSelectListTest.GetSampleStrings());
            //viewData["foo"] = selectList;
            HtmlHelper helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            MvcHtmlString html = helper.ListBox("foo", selectList, HtmlHelperTest.AttributesDictionary);

            // Assert
            Assert.Equal(
                "<select baz=\"BazValue\" id=\"foo\" multiple=\"multiple\" name=\"foo\"><option>Alpha</option>" + Environment.NewLine
              + "<option>Bravo</option>" + Environment.NewLine
              + "<option>Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void ListBoxWithAttributesDictionaryAndMultiSelectList()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary());
            MultiSelectList selectList = new MultiSelectList(MultiSelectListTest.GetSampleStrings());

            // Act
            MvcHtmlString html = helper.ListBox("foo", selectList, HtmlHelperTest.AttributesDictionary);

            // Assert
            Assert.Equal(
                "<select baz=\"BazValue\" id=\"foo\" multiple=\"multiple\" name=\"foo\"><option>Alpha</option>" + Environment.NewLine
              + "<option>Bravo</option>" + Environment.NewLine
              + "<option>Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void ListBoxWithAttributesDictionaryOverridesName()
        {
            // DevDiv Bugs #217602:
            // SelectInternal() should override the user-provided 'name' attribute

            // Arrange
            ViewDataDictionary viewData = new ViewDataDictionary();
            MultiSelectList selectList = new MultiSelectList(MultiSelectListTest.GetSampleStrings());
            HtmlHelper helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            MvcHtmlString html = helper.ListBox("foo", selectList, new { myAttr = "myValue", name = "theName" });

            // Assert
            Assert.Equal(
                "<select id=\"foo\" multiple=\"multiple\" myAttr=\"myValue\" name=\"foo\"><option>Alpha</option>" + Environment.NewLine
              + "<option>Bravo</option>" + Environment.NewLine
              + "<option>Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void ListBoxWithEmptyNameThrows()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary());

            // Act & Assert
            Assert.ThrowsArgumentNullOrEmpty(
                delegate { helper.ListBox(String.Empty, (MultiSelectList)null /* selectList */); },
                "name");
        }

        [Fact]
        public void ListBoxWithNullNameThrows()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary());

            // Act & Assert
            Assert.ThrowsArgumentNullOrEmpty(
                delegate { helper.ListBox(null /* name */, (MultiSelectList)null /* selectList */); },
                "name");
        }

        [Fact]
        public void ListBoxWithNullSelectListUsesViewData()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper();
            helper.ViewData["foo"] = new MultiSelectList(MultiSelectListTest.GetSampleStrings(), new[] { "Charlie" });

            // Act
            MvcHtmlString html = helper.ListBox("foo", null);

            // Assert
            Assert.Equal(
                "<select id=\"foo\" multiple=\"multiple\" name=\"foo\"><option>Alpha</option>" + Environment.NewLine
              + "<option>Bravo</option>" + Environment.NewLine
              + "<option selected=\"selected\">Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void ListBoxWithObjectDictionary()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary());
            MultiSelectList selectList = new MultiSelectList(MultiSelectListTest.GetSampleStrings());

            // Act
            MvcHtmlString html = helper.ListBox("foo", selectList, HtmlHelperTest.AttributesObjectDictionary);

            // Assert
            Assert.Equal(
                "<select baz=\"BazObjValue\" id=\"foo\" multiple=\"multiple\" name=\"foo\"><option>Alpha</option>" + Environment.NewLine
              + "<option>Bravo</option>" + Environment.NewLine
              + "<option>Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void ListBoxWithObjectDictionaryWithUnderscores()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary());
            MultiSelectList selectList = new MultiSelectList(MultiSelectListTest.GetSampleStrings());

            // Act
            MvcHtmlString html = helper.ListBox("foo", selectList, HtmlHelperTest.AttributesObjectUnderscoresDictionary);

            // Assert
            Assert.Equal(
                "<select foo-baz=\"BazObjValue\" id=\"foo\" multiple=\"multiple\" name=\"foo\"><option>Alpha</option>" + Environment.NewLine
              + "<option>Bravo</option>" + Environment.NewLine
              + "<option>Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void ListBoxWithIEnumerableSelectListItem()
        {
            // Arrange
            ViewDataDictionary vdd = new ViewDataDictionary { { "foo", MultiSelectListTest.GetSampleIEnumerableObjects() } };
            HtmlHelper helper = MvcHelper.GetHtmlHelper(vdd);

            // Act
            MvcHtmlString html = helper.ListBox("foo");

            // Assert
            Assert.Equal(
                "<select id=\"foo\" multiple=\"multiple\" name=\"foo\"><option value=\"123456789\">John</option>" + Environment.NewLine
              + "<option value=\"987654321\">Jane</option>" + Environment.NewLine
              + "<option selected=\"selected\" value=\"111111111\">Joe</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void ListBoxWithIEnumerableOfSelectListItemMatchesEnumName()
        {
            // Arrange
            ViewDataDictionary<IEnumerable<EnumWithDisplay?>> vdd = new ViewDataDictionary<IEnumerable<EnumWithDisplay?>>
            {
                { "foo", new EnumWithDisplay?[] { EnumWithDisplay.One, null, EnumWithDisplay.Three, }}
            };
            HtmlHelper<IEnumerable<EnumWithDisplay?>> helper = MvcHelper.GetHtmlHelper(vdd);

            // Act
            MvcHtmlString html =
                helper.ListBox("foo", GetSelectListWithNamedValuesForEnumWithDisplay(includeEmpty: true));

            // Assert
            Assert.Equal(
                "<select id=\"foo\" multiple=\"multiple\" name=\"foo\">" +
                "<option selected=\"selected\" value=\"\"></option>" + Environment.NewLine +
                "<option value=\"Zero\">Zero</option>" + Environment.NewLine +
                "<option selected=\"selected\" value=\"One\">One</option>" + Environment.NewLine +
                "<option value=\"Two\">Two</option>" + Environment.NewLine +
                "<option selected=\"selected\" value=\"Three\">Three</option>" + Environment.NewLine +
                "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void ListBoxWithIEnumerableOfSelectListItemMatchesEnumValue()
        {
            // Arrange
            ViewDataDictionary<IEnumerable<EnumWithDisplay?>> vdd = new ViewDataDictionary<IEnumerable<EnumWithDisplay?>>
            {
                { "foo", new EnumWithDisplay?[] { EnumWithDisplay.One, null, EnumWithDisplay.Three, }}
            };
            HtmlHelper<IEnumerable<EnumWithDisplay?>> helper = MvcHelper.GetHtmlHelper(vdd);

            // Act
            MvcHtmlString html =
                helper.ListBox("foo", GetSelectListWithNumericValuesForEnumWithDisplay(includeEmpty: true));

            // Assert
            Assert.Equal(
                "<select id=\"foo\" multiple=\"multiple\" name=\"foo\">" +
                "<option selected=\"selected\" value=\"\"></option>" + Environment.NewLine +
                "<option value=\"0\">Zero</option>" + Environment.NewLine +
                "<option selected=\"selected\" value=\"1\">One</option>" + Environment.NewLine +
                "<option value=\"2\">Two</option>" + Environment.NewLine +
                "<option selected=\"selected\" value=\"3\">Three</option>" + Environment.NewLine +
                "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void ListBoxThrowsWhenExpressionDoesNotEvaluateToIEnumerable()
        {
            // Arrange
            ViewDataDictionary vdd = new ViewDataDictionary { { "foo", 123456789 } };
            HtmlHelper helper = MvcHelper.GetHtmlHelper(vdd);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => helper.ListBox("foo", MultiSelectListTest.GetSampleIEnumerableObjects()),
                "The parameter 'expression' must evaluate to an IEnumerable when multiple selection is allowed."
                );
        }

        [Fact]
        public void ListBoxThrowsWhenExpressionEvaluatesToString()
        {
            // Arrange
            ViewDataDictionary vdd = new ViewDataDictionary { { "foo", "123456789" } };
            HtmlHelper helper = MvcHelper.GetHtmlHelper(vdd);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => helper.ListBox("foo", MultiSelectListTest.GetSampleIEnumerableObjects()),
                "The parameter 'expression' must evaluate to an IEnumerable when multiple selection is allowed."
                );
        }

        [Fact]
        public void ListBoxWithListOfSelectListItemSelectsDefaultFromViewData()
        {
            // Arrange
            ViewDataDictionary vdd = new ViewDataDictionary { { "foo", new string[] { "123456789", "111111111" } } };
            HtmlHelper helper = MvcHelper.GetHtmlHelper(vdd);

            // Act
            MvcHtmlString html = helper.ListBox("foo", MultiSelectListTest.GetSampleListObjects());

            // Assert
            Assert.Equal(
                "<select id=\"foo\" multiple=\"multiple\" name=\"foo\"><option selected=\"selected\" value=\"123456789\">John</option>" + Environment.NewLine
              + "<option value=\"987654321\">Jane</option>" + Environment.NewLine
              + "<option selected=\"selected\" value=\"111111111\">Joe</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void ListBoxWithListOfSelectListItem()
        {
            // Arrange
            ViewDataDictionary vdd = new ViewDataDictionary { { "foo", MultiSelectListTest.GetSampleListObjects() } };
            HtmlHelper helper = MvcHelper.GetHtmlHelper(vdd);

            // Act
            MvcHtmlString html = helper.ListBox("foo");

            // Assert
            Assert.Equal(
                "<select id=\"foo\" multiple=\"multiple\" name=\"foo\"><option value=\"123456789\">John</option>" + Environment.NewLine
              + "<option value=\"987654321\">Jane</option>" + Environment.NewLine
              + "<option selected=\"selected\" value=\"111111111\">Joe</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void ListBoxWithPrefix()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary());
            MultiSelectList selectList = new MultiSelectList(MultiSelectListTest.GetSampleStrings());
            helper.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = "MyPrefix";

            // Act
            MvcHtmlString html = helper.ListBox("foo", selectList, HtmlHelperTest.AttributesObjectDictionary);

            // Assert
            Assert.Equal(
                "<select baz=\"BazObjValue\" id=\"MyPrefix_foo\" multiple=\"multiple\" name=\"MyPrefix.foo\"><option>Alpha</option>" + Environment.NewLine
              + "<option>Bravo</option>" + Environment.NewLine
              + "<option>Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void ListBoxWithPrefixAndEmptyName()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary());
            MultiSelectList selectList = new MultiSelectList(MultiSelectListTest.GetSampleStrings());
            helper.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = "MyPrefix";

            // Act
            MvcHtmlString html = helper.ListBox("", selectList, HtmlHelperTest.AttributesObjectDictionary);

            // Assert
            Assert.Equal(
                "<select baz=\"BazObjValue\" id=\"MyPrefix\" multiple=\"multiple\" name=\"MyPrefix\"><option>Alpha</option>" + Environment.NewLine
              + "<option>Bravo</option>" + Environment.NewLine
              + "<option>Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void ListBoxWithPrefixAndNullSelectListUsesViewData()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper();
            helper.ViewData["foo"] = new MultiSelectList(MultiSelectListTest.GetSampleStrings(), new[] { "Charlie" });
            helper.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = "MyPrefix";

            // Act
            MvcHtmlString html = helper.ListBox("foo");

            // Assert
            Assert.Equal(
                "<select id=\"MyPrefix_foo\" multiple=\"multiple\" name=\"MyPrefix.foo\"><option>Alpha</option>" + Environment.NewLine
              + "<option>Bravo</option>" + Environment.NewLine
              + "<option selected=\"selected\">Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        // ListBoxFor

        [Fact]
        void ListBoxFor_WithGroups()
        {
            // Arrange
            ViewDataDictionary<FooArrayModel> dict = new ViewDataDictionary<FooArrayModel>();
            dict.Add("foo", new[] { "volvo", "audi" });
            HtmlHelper<FooArrayModel> helper = MvcHelper.GetHtmlHelper(dict);
            const string expectedListBox = @"<select id=""foo"" multiple=""multiple"" name=""foo""><option value=""other1"">other1</option>
<option value=""other2"">other2</option>
<optgroup label=""Swedish Cars"">
<option selected=""selected"" value=""volvo"">Volvo</option>
<option value=""saab"">Saab</option>
</optgroup>
<option value=""other3"">other3</option>
<optgroup>
<option value=""other4"">other4</option>
<option value=""other5"">other5</option>
</optgroup>
<optgroup label=""German Cars"">
<option value=""mercedes-benz"">Mercedes-Benz</option>
<option disabled=""disabled"" selected=""selected"" value=""audi"">Audi</option>
</optgroup>
<option value=""other6"">other6</option>
</select>";

            // Act
            MvcHtmlString html = helper.ListBoxFor(m => m.foo, GroupedItems);

            // Assert
            Assert.Equal(expectedListBox, html.ToHtmlString());
        }

        [Fact]
        void ListBoxFor_WithGroups_WithDisabled()
        {
            // Arrange
            ViewDataDictionary<FooArrayModel> dict = new ViewDataDictionary<FooArrayModel>();
            dict.Add("foo", new[] { "a", "d" });
            HtmlHelper<FooArrayModel> helper = MvcHelper.GetHtmlHelper(dict);
            const string expectedListBox = @"<select id=""foo"" multiple=""multiple"" name=""foo""><option selected=""selected"" value=""a"">Alice</option>
<optgroup disabled=""disabled"" label=""DisabledGroup"">
<option value=""b"">Bob</option>
<option value=""c"">Charlie</option>
</optgroup>
<option disabled=""disabled"" selected=""selected"" value=""d"">David</option>
</select>";

            // Act
            MvcHtmlString html = helper.ListBoxFor(m => m.foo, GroupedItems_WithDisabled);

            // Assert
            Assert.Equal(expectedListBox, html.ToHtmlString());
        }

        [Fact]
        void ListBoxFor_MultiSelectList_WithGroups()
        {
            // Arrange
            ViewDataDictionary<FooArrayModel> dict = new ViewDataDictionary<FooArrayModel>();
            dict.Add("foo", new[] { "mercedes-benz", "audi" });
            HtmlHelper<FooArrayModel> helper = MvcHelper.GetHtmlHelper(dict);
            const string expectedListBox = @"<select id=""foo"" multiple=""multiple"" name=""foo""><option value=""ufo"">UFO</option>
<optgroup label=""Swedish Cars"">
<option value=""volvo"">Volvo</option>
<option value=""saab"">Saab</option>
</optgroup>
<optgroup label=""German Cars"">
<option selected=""selected"" value=""mercedes-benz"">Mercedes-Benz</option>
<option selected=""selected"" value=""audi"">Audi</option>
</optgroup>
<option value=""other"">Other</option>
<optgroup label="" "">
<option value=""unknown"">Unknown</option>
</optgroup>
</select>";

            // Act
            MvcHtmlString html = helper.ListBoxFor(m => m.foo, _multiSelectList);

            // Assert
            Assert.Equal(expectedListBox, html.ToHtmlString());
        }

        [Fact]
        public void ListBoxForWithNullExpressionThrows()
        {
            // Arrange
            HtmlHelper<FooModel> helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary<FooModel>());

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => helper.ListBoxFor<FooModel, object>(null, null),
                "expression");
        }

        [Fact]
        public void ListBoxForUsesExplicitValueIfNotProvidedInViewData()
        {
            // Arrange
            HtmlHelper<FooModel> helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary<FooModel>());
            MultiSelectList selectList = new MultiSelectList(MultiSelectListTest.GetSampleAnonymousObjects(), "Letter", "FullWord", new[] { "A", "C" });

            // Act
            MvcHtmlString html = helper.ListBoxFor(m => m.foo, selectList);

            // Assert
            Assert.Equal(
                "<select id=\"foo\" multiple=\"multiple\" name=\"foo\"><option selected=\"selected\" value=\"A\">Alpha</option>" + Environment.NewLine
              + "<option value=\"B\">Bravo</option>" + Environment.NewLine
              + "<option selected=\"selected\" value=\"C\">Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void ListBoxForUsesExplicitValueIfNotProvidedInViewData_Unobtrusive()
        {
            // Arrange
            HtmlHelper<FooModel> helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary<FooModel>());
            helper.ViewContext.ClientValidationEnabled = true;
            helper.ViewContext.UnobtrusiveJavaScriptEnabled = true;
            helper.ViewContext.FormContext = new FormContext();
            helper.ClientValidationRuleFactory = (name, metadata) => new[] { new ModelClientValidationRule { ValidationType = "type", ErrorMessage = "error" } };
            MultiSelectList selectList = new MultiSelectList(MultiSelectListTest.GetSampleAnonymousObjects(), "Letter", "FullWord", new[] { "A", "C" });

            // Act
            MvcHtmlString html = helper.ListBoxFor(m => m.foo, selectList);

            // Assert
            Assert.Equal(
                "<select data-val=\"true\" data-val-type=\"error\" id=\"foo\" multiple=\"multiple\" name=\"foo\"><option selected=\"selected\" value=\"A\">Alpha</option>" + Environment.NewLine
              + "<option value=\"B\">Bravo</option>" + Environment.NewLine
              + "<option selected=\"selected\" value=\"C\">Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        [ReplaceCulture]
        public void ListBoxForWithEnumerableModel_Unobtrusive()
        {
            // Arrange
            HtmlHelper<IEnumerable<RequiredModel>> helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary<IEnumerable<RequiredModel>>());
            helper.ViewContext.ClientValidationEnabled = true;
            helper.ViewContext.UnobtrusiveJavaScriptEnabled = true;
            helper.ViewContext.FormContext = new FormContext();
            helper.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = "MyPrefix";

            MultiSelectList selectList = new MultiSelectList(MultiSelectListTest.GetSampleAnonymousObjects(), "Letter",
                "FullWord", new[] { "C" });

            // Act
            MvcHtmlString html = helper.ListBoxFor(m => m.ElementAt(0).foo, selectList);

            // Assert
            Assert.Equal(
                "<select data-val=\"true\" data-val-required=\"The foo field is required.\" id=\"MyPrefix_foo\" multiple=\"multiple\" name=\"MyPrefix.foo\"><option value=\"A\">Alpha</option>" + Environment.NewLine
              + "<option value=\"B\">Bravo</option>" + Environment.NewLine
              + "<option selected=\"selected\" value=\"C\">Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void ListBoxForUsesViewDataDefaultValue()
        {
            // Arrange
            HtmlHelper<FooModel> helper = MvcHelper.GetHtmlHelper(_listBoxViewData);
            MultiSelectList selectList = new MultiSelectList(MultiSelectListTest.GetSampleStrings(), new[] { "Charlie" });

            // Act
            MvcHtmlString html = helper.ListBoxFor(m => m.foo, selectList);

            // Assert
            Assert.Equal(
                "<select id=\"foo\" multiple=\"multiple\" name=\"foo\"><option>Alpha</option>" + Environment.NewLine
              + "<option selected=\"selected\">Bravo</option>" + Environment.NewLine
              + "<option>Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void ListBoxForUsesLambdaDefaultValue()
        {
            // Arrange
            FooArrayModel model = new FooArrayModel { foo = new[] { "Bravo" } };
            HtmlHelper<FooArrayModel> helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary<FooArrayModel>(model));
            SelectList selectList = new SelectList(MultiSelectListTest.GetSampleStrings());

            // Act
            MvcHtmlString html = helper.ListBoxFor(m => m.foo, selectList);

            // Assert
            Assert.Equal(
                "<select id=\"foo\" multiple=\"multiple\" name=\"foo\"><option>Alpha</option>" + Environment.NewLine
              + "<option selected=\"selected\">Bravo</option>" + Environment.NewLine
              + "<option>Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void ListBoxForUsesLambdaDefaultValueWithNullSelectListUsesViewData()
        {
            // Arrange
            FooArrayModel model = new FooArrayModel { foo = new[] { "Bravo" } };
            ViewDataDictionary<FooArrayModel> vdd = new ViewDataDictionary<FooArrayModel>(model)
            {
                { "foo", new MultiSelectList(MultiSelectListTest.GetSampleStrings()) }
            };
            HtmlHelper<FooArrayModel> helper = MvcHelper.GetHtmlHelper(vdd);

            // Act
            MvcHtmlString html = helper.ListBoxFor(m => m.foo, selectList: null);

            // Assert
            Assert.Equal(
                "<select id=\"foo\" multiple=\"multiple\" name=\"foo\"><option>Alpha</option>" + Environment.NewLine
              + "<option selected=\"selected\">Bravo</option>" + Environment.NewLine
              + "<option>Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void ListBoxForWithErrors()
        {
            // Arrange
            HtmlHelper<FooBarModel> helper = MvcHelper.GetHtmlHelper(GetViewDataWithErrors());
            MultiSelectList list = new MultiSelectList(MultiSelectListTest.GetSampleStrings(), new[] { "Charlie" });

            // Act
            MvcHtmlString html = helper.ListBoxFor(m => m.foo, list);

            // Assert
            Assert.Equal(
                "<select class=\"input-validation-error\" id=\"foo\" multiple=\"multiple\" name=\"foo\"><option>Alpha</option>" + Environment.NewLine
              + "<option selected=\"selected\">Bravo</option>" + Environment.NewLine
              + "<option selected=\"selected\">Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void ListBoxForWithErrorsAndCustomClass()
        {
            // Arrange
            HtmlHelper<FooBarModel> helper = MvcHelper.GetHtmlHelper(GetViewDataWithErrors());
            MultiSelectList selectList = new MultiSelectList(MultiSelectListTest.GetSampleStrings(), new[] { "Charlie" });

            // Act
            MvcHtmlString html = helper.ListBoxFor(m => m.foo, selectList, new { @class = "foo-class" });

            // Assert
            Assert.Equal(
                "<select class=\"input-validation-error foo-class\" id=\"foo\" multiple=\"multiple\" name=\"foo\"><option>Alpha</option>" + Environment.NewLine
              + "<option selected=\"selected\">Bravo</option>" + Environment.NewLine
              + "<option selected=\"selected\">Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void ListBoxForWithAttributesDictionary()
        {
            // Arrange
            MultiSelectList selectList = new MultiSelectList(MultiSelectListTest.GetSampleStrings());
            HtmlHelper<FooModel> helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary<FooModel>());

            // Act
            MvcHtmlString html = helper.ListBoxFor(m => m.foo, selectList, HtmlHelperTest.AttributesDictionary);

            // Assert
            Assert.Equal(
                "<select baz=\"BazValue\" id=\"foo\" multiple=\"multiple\" name=\"foo\"><option>Alpha</option>" + Environment.NewLine
              + "<option>Bravo</option>" + Environment.NewLine
              + "<option>Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void ListBoxForWithAttributesDictionaryOverridesName()
        {
            // DevDiv Bugs #217602:
            // SelectInternal() should override the user-provided 'name' attribute

            // Arrange
            MultiSelectList selectList = new MultiSelectList(MultiSelectListTest.GetSampleStrings());
            HtmlHelper<FooModel> helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary<FooModel>());

            // Act
            MvcHtmlString html = helper.ListBoxFor(m => m.foo, selectList, new { myAttr = "myValue", name = "theName" });

            // Assert
            Assert.Equal(
                "<select id=\"foo\" multiple=\"multiple\" myAttr=\"myValue\" name=\"foo\"><option>Alpha</option>" + Environment.NewLine
              + "<option>Bravo</option>" + Environment.NewLine
              + "<option>Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void ListBoxForWithNullSelectListUsesViewData()
        {
            // Arrange
            HtmlHelper<FooModel> helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary<FooModel>());
            helper.ViewContext.ViewData["foo"] = new MultiSelectList(MultiSelectListTest.GetSampleStrings(), new[] { "Charlie" });

            // Act
            MvcHtmlString html = helper.ListBoxFor(m => m.foo, null);

            // Assert
            Assert.Equal(
                "<select id=\"foo\" multiple=\"multiple\" name=\"foo\"><option>Alpha</option>" + Environment.NewLine
              + "<option>Bravo</option>" + Environment.NewLine
              + "<option selected=\"selected\">Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void ListBoxForWithObjectDictionary()
        {
            // Arrange
            HtmlHelper<FooModel> helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary<FooModel>());
            MultiSelectList selectList = new MultiSelectList(MultiSelectListTest.GetSampleStrings());

            // Act
            MvcHtmlString html = helper.ListBoxFor(m => m.foo, selectList, HtmlHelperTest.AttributesObjectDictionary);

            // Assert
            Assert.Equal(
                "<select baz=\"BazObjValue\" id=\"foo\" multiple=\"multiple\" name=\"foo\"><option>Alpha</option>" + Environment.NewLine
              + "<option>Bravo</option>" + Environment.NewLine
              + "<option>Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void ListBoxForWithObjectDictionaryWithUnderscores()
        {
            // Arrange
            HtmlHelper<FooModel> helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary<FooModel>());
            MultiSelectList selectList = new MultiSelectList(MultiSelectListTest.GetSampleStrings());

            // Act
            MvcHtmlString html = helper.ListBoxFor(m => m.foo, selectList, HtmlHelperTest.AttributesObjectUnderscoresDictionary);

            // Assert
            Assert.Equal(
                "<select foo-baz=\"BazObjValue\" id=\"foo\" multiple=\"multiple\" name=\"foo\"><option>Alpha</option>" + Environment.NewLine
              + "<option>Bravo</option>" + Environment.NewLine
              + "<option>Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void ListBoxForWithIEnumerableSelectListItem()
        {
            // Arrange
            ViewDataDictionary<FooModel> vdd = new ViewDataDictionary<FooModel> { { "foo", MultiSelectListTest.GetSampleIEnumerableObjects() } };
            HtmlHelper<FooModel> helper = MvcHelper.GetHtmlHelper(vdd);

            // Act
            MvcHtmlString html = helper.ListBoxFor(m => m.foo, null);

            // Assert
            Assert.Equal(
                "<select id=\"foo\" multiple=\"multiple\" name=\"foo\"><option value=\"123456789\">John</option>" + Environment.NewLine
              + "<option value=\"987654321\">Jane</option>" + Environment.NewLine
              + "<option selected=\"selected\" value=\"111111111\">Joe</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void ListBoxForThrowsWhenExpressionDoesNotEvaluateToIEnumerable()
        {
            // Arrange
            ViewDataDictionary<NonIEnumerableModel> vdd = new ViewDataDictionary<NonIEnumerableModel> { { "foo", 123456789 } };
            HtmlHelper<NonIEnumerableModel> helper = MvcHelper.GetHtmlHelper(vdd);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => helper.ListBoxFor(m => m.foo, MultiSelectListTest.GetSampleIEnumerableObjects()),
                "The parameter 'expression' must evaluate to an IEnumerable when multiple selection is allowed."
                );
        }

        [Fact]
        public void ListBoxForThrowsWhenExpressionEvaluatesToString()
        {
            // Arrange
            ViewDataDictionary<FooModel> vdd = new ViewDataDictionary<FooModel> { { "foo", "123456789" } };
            HtmlHelper<FooModel> helper = MvcHelper.GetHtmlHelper(vdd);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => helper.ListBoxFor(m => m.foo, MultiSelectListTest.GetSampleIEnumerableObjects()),
                "The parameter 'expression' must evaluate to an IEnumerable when multiple selection is allowed."
                );
        }

        [Fact]
        public void ListBoxForWithListOfSelectListItemSelectsDefaultFromViewData()
        {
            // Arrange
            ViewDataDictionary<FooModel> vdd = new ViewDataDictionary<FooModel> { { "foo", new string[] { "123456789", "111111111" } } };
            HtmlHelper<FooModel> helper = MvcHelper.GetHtmlHelper(vdd);

            // Act
            MvcHtmlString html = helper.ListBoxFor(m => m.foo, MultiSelectListTest.GetSampleListObjects());

            // Assert
            Assert.Equal(
                "<select id=\"foo\" multiple=\"multiple\" name=\"foo\"><option selected=\"selected\" value=\"123456789\">John</option>" + Environment.NewLine
              + "<option value=\"987654321\">Jane</option>" + Environment.NewLine
              + "<option selected=\"selected\" value=\"111111111\">Joe</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void ListBoxForWithListOfSelectListItem()
        {
            // Arrange
            ViewDataDictionary<FooModel> vdd = new ViewDataDictionary<FooModel> { { "foo", MultiSelectListTest.GetSampleListObjects() } };
            HtmlHelper<FooModel> helper = MvcHelper.GetHtmlHelper(vdd);

            // Act
            MvcHtmlString html = helper.ListBoxFor(m => m.foo, null);

            // Assert
            Assert.Equal(
                "<select id=\"foo\" multiple=\"multiple\" name=\"foo\"><option value=\"123456789\">John</option>" + Environment.NewLine
              + "<option value=\"987654321\">Jane</option>" + Environment.NewLine
              + "<option selected=\"selected\" value=\"111111111\">Joe</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void ListBoxForWithPrefix()
        {
            // Arrange
            HtmlHelper<FooModel> helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary<FooModel>());
            MultiSelectList selectList = new MultiSelectList(MultiSelectListTest.GetSampleStrings());
            helper.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = "MyPrefix";

            // Act
            MvcHtmlString html = helper.ListBoxFor(m => m.foo, selectList, HtmlHelperTest.AttributesObjectDictionary);

            // Assert
            Assert.Equal(
                "<select baz=\"BazObjValue\" id=\"MyPrefix_foo\" multiple=\"multiple\" name=\"MyPrefix.foo\"><option>Alpha</option>" + Environment.NewLine
              + "<option>Bravo</option>" + Environment.NewLine
              + "<option>Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void ListBoxForWithPrefixAndListOfSelectListItemSelectsDefaultFromViewData()
        {
            // Arrange
            ViewDataDictionary<FooModel> vdd = new ViewDataDictionary<FooModel> { { "foo", new string[] { "123456789", "111111111" } } };
            HtmlHelper<FooModel> helper = MvcHelper.GetHtmlHelper(vdd);
            helper.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = "MyPrefix";

            // Act
            MvcHtmlString html = helper.ListBoxFor(m => m.foo, MultiSelectListTest.GetSampleListObjects());

            // Assert
            Assert.Equal(
                "<select id=\"MyPrefix_foo\" multiple=\"multiple\" name=\"MyPrefix.foo\"><option selected=\"selected\" value=\"123456789\">John</option>" + Environment.NewLine
              + "<option value=\"987654321\">Jane</option>" + Environment.NewLine
              + "<option selected=\"selected\" value=\"111111111\">Joe</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        // Culture tests

        [Fact]
        [ReplaceCulture]
        public void SelectHelpersUseCurrentCultureToConvertValues()
        {
            // Arrange
            HtmlHelper defaultValueHelper = MvcHelper.GetHtmlHelper(new ViewDataDictionary
            {
                { "foo", new[] { new DateTime(1900, 1, 1, 0, 0, 1) } },
                { "bar", new DateTime(1900, 1, 1, 0, 0, 1) }
            });
            HtmlHelper helper = MvcHelper.GetHtmlHelper();
            SelectList selectList = new SelectList(GetSampleCultureAnonymousObjects(), "Date", "FullWord", new DateTime(1900, 1, 1, 0, 0, 0));

            var tests = new[]
            {
                // DropDownList(name, selectList, optionLabel)
                new
                {
                    Html = "<select id=\"foo\" name=\"foo\"><option selected=\"selected\" value=\"01/01/1900 00:00:00\">Alpha</option>" + Environment.NewLine
                         + "<option value=\"01/01/1900 00:00:01\">Bravo</option>" + Environment.NewLine
                         + "<option value=\"01/01/1900 00:00:02\">Charlie</option>" + Environment.NewLine
                         + "</select>",
                    Action = new Func<MvcHtmlString>(() => helper.DropDownList("foo", selectList, (string)null))
                },
                // DropDownList(name, selectList, optionLabel) (With default value selected from ViewData)
                new
                {
                    Html = "<select id=\"bar\" name=\"bar\"><option value=\"01/01/1900 00:00:00\">Alpha</option>" + Environment.NewLine
                         + "<option selected=\"selected\" value=\"01/01/1900 00:00:01\">Bravo</option>" + Environment.NewLine
                         + "<option value=\"01/01/1900 00:00:02\">Charlie</option>" + Environment.NewLine
                         + "</select>",
                    Action = new Func<MvcHtmlString>(() => defaultValueHelper.DropDownList("bar", selectList, (string)null))
                },
                // ListBox(name, selectList)
                new
                {
                    Html = "<select id=\"foo\" multiple=\"multiple\" name=\"foo\"><option selected=\"selected\" value=\"01/01/1900 00:00:00\">Alpha</option>" + Environment.NewLine
                         + "<option value=\"01/01/1900 00:00:01\">Bravo</option>" + Environment.NewLine
                         + "<option value=\"01/01/1900 00:00:02\">Charlie</option>" + Environment.NewLine
                         + "</select>",
                    Action = new Func<MvcHtmlString>(() => helper.ListBox("foo", selectList))
                },
                // ListBox(name, selectList) (With default value selected from ViewData)
                new
                {
                    Html = "<select id=\"foo\" multiple=\"multiple\" name=\"foo\"><option value=\"01/01/1900 00:00:00\">Alpha</option>" + Environment.NewLine
                         + "<option selected=\"selected\" value=\"01/01/1900 00:00:01\">Bravo</option>" + Environment.NewLine
                         + "<option value=\"01/01/1900 00:00:02\">Charlie</option>" + Environment.NewLine
                         + "</select>",
                    Action = new Func<MvcHtmlString>(() => defaultValueHelper.ListBox("foo", selectList))
                }
            };

            // Act && Assert
            foreach (var test in tests)
            {
                Assert.Equal(test.Html, test.Action().ToHtmlString());
            }
        }

        // Helpers

        // Value and Text is constant name in all returned SelectListItem objects.
        private static IEnumerable<SelectListItem> GetSelectListWithNamedValuesForEnumWithDisplay(bool includeEmpty)
        {
            IList<SelectListItem> selectList = new List<SelectListItem>();
            if (includeEmpty)
            {
                // Similar to what we might generate for a Nullable<T>
                selectList.Add(new SelectListItem { Text = String.Empty, Value = String.Empty, });
            }

            foreach (string name in Enum.GetNames(typeof(EnumWithDisplay)))
            {
                selectList.Add(new SelectListItem { Text = name, Value = name, });
            }

            return selectList;
        }

        // Value is numeric value while Text is constant name in all returned SelectListItem objects.
        private static IEnumerable<SelectListItem> GetSelectListWithNumericValuesForEnumWithDisplay(bool includeEmpty)
        {
            IList<SelectListItem> selectList = new List<SelectListItem>();
            if (includeEmpty)
            {
                // Similar to what we might generate for a Nullable<T>
                selectList.Add(new SelectListItem { Text = String.Empty, Value = String.Empty, });
            }

            foreach (FieldInfo field in typeof(EnumWithDisplay).GetFields(
                BindingFlags.DeclaredOnly | BindingFlags.GetField | BindingFlags.Public | BindingFlags.Static))
            {
                string name = field.Name;
                object value = field.GetRawConstantValue();
                selectList.Add(new SelectListItem { Text = name, Value = value.ToString(), });
            }

            return selectList;
        }

        private class FooModel
        {
            public string foo { get; set; }
        }

        private class FooBarModel : FooModel
        {
            public string bar { get; set; }
        }

        private class FooArrayModel
        {
            public string[] foo { get; set; }
        }

        private class NonIEnumerableModel
        {
            public int foo { get; set; }
        }

        private static ViewDataDictionary<FooBarModel> GetViewDataWithErrors()
        {
            ViewDataDictionary<FooBarModel> viewData = new ViewDataDictionary<FooBarModel> { { "foo", "ViewDataFoo" } };
            viewData.Model = new FooBarModel { foo = "ViewItemFoo", bar = "ViewItemBar" };

            ModelState modelStateFoo = new ModelState();
            modelStateFoo.Errors.Add(new ModelError("foo error 1"));
            modelStateFoo.Errors.Add(new ModelError("foo error 2"));
            viewData.ModelState["foo"] = modelStateFoo;
            modelStateFoo.Value = new ValueProviderResult(new string[] { "Bravo", "Charlie" }, "Bravo", CultureInfo.InvariantCulture);

            return viewData;
        }

        internal static IEnumerable GetSampleCultureAnonymousObjects()
        {
            return new[]
            {
                new { Date = new DateTime(1900, 1, 1, 0, 0, 0), FullWord = "Alpha" },
                new { Date = new DateTime(1900, 1, 1, 0, 0, 1), FullWord = "Bravo" },
                new { Date = new DateTime(1900, 1, 1, 0, 0, 2), FullWord = "Charlie" }
            };
        }

        private class RequiredModel
        {
            [Required]
            public string foo { get; set; }
        }

        private class EnumModel
        {
            public EnumWithDisplay WithDisplay { get; set; }
            public EnumWithDuplicates WithDuplicates { get; set; }
            public EnumWithFlags WithFlags { get; set; }
            public EnumWithDisplay? WithNullable { get; set; }
            public EnumWithoutAnything WithoutAnything { get; set; }
        }

        // enum definitions

        private enum EnumWithDisplay : byte
        {
            [Display(Name = "First")]
            Zero,
            [Display(Name = "Second")]
            One,
            [Display(Name = "Third")]
            Two,
            [Display(Name = "Fourth")]
            Three,
        }

        private enum EnumWithDuplicates : byte
        {
            First,
            Second,
            Third = 1,
            Fourth,
        }

        private enum EnumWithoutAnything : byte
        {
        }

        [Flags]
        private enum EnumWithFlags : byte
        {
            First = 1,
            Second = 2,
            Third = 4,
            Fourth = 8,
        }

        public void Dispose()
        {
            // Reset ScopeStorage (written via e.g. ViewContext.ClientValidationEnabled) between tests to avoid unexpected interactions.
            ScopeStorage.CurrentProvider = new StaticScopeStorageProvider();
            ScopeStorage.GlobalScope.Clear();
        }
    }
}
