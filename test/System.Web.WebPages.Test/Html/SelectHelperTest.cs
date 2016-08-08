// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.WebPages.Html;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.WebPages.Test
{
    public class SelectExtensionsTest
    {
        [Fact]
        public void DropDownListThrowsWithNoName()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act and assert
            Assert.ThrowsArgumentNullOrEmptyString(() => helper.DropDownList(name: null, selectList: null), "name");
        }

        [Fact]
        public void DropDownListWithNoSelectedItem()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act
            var html = helper.DropDownList("foo", GetSelectList());

            // Assert
            Assert.Equal(
                "<select id=\"foo\" name=\"foo\">" + Environment.NewLine
              + "<option value=\"A\">Alpha</option>" + Environment.NewLine
              + "<option value=\"B\">Bravo</option>" + Environment.NewLine
              + "<option value=\"C\">Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void DropDownListWithDefaultOption()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act
            var html = helper.DropDownList("foo", "select-one", GetSelectList());

            // Assert
            Assert.Equal(
                "<select id=\"foo\" name=\"foo\">" + Environment.NewLine
              + "<option value=\"\">select-one</option>" + Environment.NewLine
              + "<option value=\"A\">Alpha</option>" + Environment.NewLine
              + "<option value=\"B\">Bravo</option>" + Environment.NewLine
              + "<option value=\"C\">Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void DropDownListWithAttributes()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act
            var html = helper.DropDownList("foo", GetSelectList(), new { attr = "attr-val", attr2 = "attr-val2" });

            // Assert
            Assert.Equal(
                "<select attr=\"attr-val\" attr2=\"attr-val2\" id=\"foo\" name=\"foo\">" + Environment.NewLine
              + "<option value=\"A\">Alpha</option>" + Environment.NewLine
              + "<option value=\"B\">Bravo</option>" + Environment.NewLine
              + "<option value=\"C\">Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void DropDownListWithExplicitValue()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act
            var html = helper.DropDownList("foo", null, GetSelectList(), "B", new Dictionary<string, object> { { "attr", "attr-val" } });

            // Assert
            Assert.Equal(
                "<select attr=\"attr-val\" id=\"foo\" name=\"foo\">" + Environment.NewLine
              + "<option value=\"A\">Alpha</option>" + Environment.NewLine
              + "<option selected=\"selected\" value=\"B\">Bravo</option>" + Environment.NewLine
              + "<option value=\"C\">Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void DropDownWithModelValue()
        {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.SetModelValue("foo", "C");
            HtmlHelper helper = HtmlHelperFactory.Create(modelState);

            // Act
            var html = helper.DropDownList("foo", GetSelectList(), new { attr = "attr-val" });

            // Assert
            Assert.Equal(
                "<select attr=\"attr-val\" id=\"foo\" name=\"foo\">" + Environment.NewLine
              + "<option value=\"A\">Alpha</option>" + Environment.NewLine
              + "<option value=\"B\">Bravo</option>" + Environment.NewLine
              + "<option selected=\"selected\" value=\"C\">Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void DropDownWithExplictAndModelValue()
        {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.SetModelValue("foo", "C");
            HtmlHelper helper = HtmlHelperFactory.Create(modelState);

            // Act
            var html = helper.DropDownList("foo", null, GetSelectList(), "B", new { attr = "attr-val" });

            // Assert
            Assert.Equal(
                "<select attr=\"attr-val\" id=\"foo\" name=\"foo\">" + Environment.NewLine
              + "<option value=\"A\">Alpha</option>" + Environment.NewLine
              + "<option selected=\"selected\" value=\"B\">Bravo</option>" + Environment.NewLine
              + "<option value=\"C\">Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void DropDownWithNonStringModelValue()
        {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.SetModelValue("foo", 23);
            HtmlHelper helper = HtmlHelperFactory.Create(modelState);

            // Act
            var html = helper.DropDownList("foo", null, GetSelectList(), new { attr = "attr-val" });

            // Assert
            Assert.Equal(
                "<select attr=\"attr-val\" id=\"foo\" name=\"foo\">" + Environment.NewLine
              + "<option value=\"A\">Alpha</option>" + Environment.NewLine
              + "<option value=\"B\">Bravo</option>" + Environment.NewLine
              + "<option value=\"C\">Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void DropDownWithNonStringExplicitValue()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act
            var html = helper.DropDownList("foo", null, GetSelectList(), new List<int>(), new { attr = "attr-val" });

            // Assert
            Assert.Equal(
                "<select attr=\"attr-val\" id=\"foo\" name=\"foo\">" + Environment.NewLine
              + "<option value=\"A\">Alpha</option>" + Environment.NewLine
              + "<option value=\"B\">Bravo</option>" + Environment.NewLine
              + "<option value=\"C\">Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void DropDownWithErrors()
        {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.AddError("foo", "some error");
            HtmlHelper helper = HtmlHelperFactory.Create(modelState);

            // Act
            var html = helper.DropDownList("foo", GetSelectList());

            // Assert
            Assert.Equal(
                "<select class=\"input-validation-error\" id=\"foo\" name=\"foo\">" + Environment.NewLine
              + "<option value=\"A\">Alpha</option>" + Environment.NewLine
              + "<option value=\"B\">Bravo</option>" + Environment.NewLine
              + "<option value=\"C\">Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void DropDownListWithErrorsAndCustomClass()
        {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.AddError("foo", "some error");
            HtmlHelper helper = HtmlHelperFactory.Create(modelState);

            // Act
            var html = helper.DropDownList("foo", GetSelectList(), new { @class = "my-class" });

            // Assert
            Assert.Equal(
                "<select class=\"input-validation-error my-class\" id=\"foo\" name=\"foo\">" + Environment.NewLine
              + "<option value=\"A\">Alpha</option>" + Environment.NewLine
              + "<option value=\"B\">Bravo</option>" + Environment.NewLine
              + "<option value=\"C\">Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void DropDownListWithEmptyOptionLabel()
        {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.AddError("foo", "some error");
            HtmlHelper helper = HtmlHelperFactory.Create(modelState);

            // Act
            var html = helper.DropDownList("foo", GetSelectList(), new { @class = "my-class" });

            // Assert
            Assert.Equal(
                "<select class=\"input-validation-error my-class\" id=\"foo\" name=\"foo\">" + Environment.NewLine
              + "<option value=\"A\">Alpha</option>" + Environment.NewLine
              + "<option value=\"B\">Bravo</option>" + Environment.NewLine
              + "<option value=\"C\">Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void DropDownListWithObjectDictionaryAndTitle()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act
            var html = helper.DropDownList("foo", "Select One", GetSelectList(), new { @class = "my-class" });

            // Assert
            Assert.Equal(
                "<select class=\"my-class\" id=\"foo\" name=\"foo\">" + Environment.NewLine
              + "<option value=\"\">Select One</option>" + Environment.NewLine
              + "<option value=\"A\">Alpha</option>" + Environment.NewLine
              + "<option value=\"B\">Bravo</option>" + Environment.NewLine
              + "<option value=\"C\">Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void DropDownListWithDotReplacementForId()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act
            var html = helper.DropDownList("foo.bar", "Select One", GetSelectList());

            // Assert
            Assert.Equal(
                "<select id=\"foo_bar\" name=\"foo.bar\">" + Environment.NewLine
              + "<option value=\"\">Select One</option>" + Environment.NewLine
              + "<option value=\"A\">Alpha</option>" + Environment.NewLine
              + "<option value=\"B\">Bravo</option>" + Environment.NewLine
              + "<option value=\"C\">Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void DropDownAddsUnobtrusiveValidationAttributes()
        {
            // Arrange
            const string fieldName = "name";
            var modelStateDictionary = new ModelStateDictionary();
            var validationHelper = new ValidationHelper(new Mock<HttpContextBase>().Object, modelStateDictionary);
            HtmlHelper helper = HtmlHelperFactory.Create(modelStateDictionary, validationHelper);

            // Act
            validationHelper.RequireField(fieldName, "Please specify a valid Name.");
            validationHelper.Add(fieldName, Validator.StringLength(30, errorMessage: "Name cannot exceed {0} characters"));
            var html = helper.DropDownList(fieldName, GetSelectList(), htmlAttributes: new Dictionary<string, object> { { "data-some-val", "5" } });

            // Assert
            Assert.Equal(
                "<select data-some-val=\"5\" data-val=\"true\" data-val-length=\"Name cannot exceed 30 characters\" data-val-length-max=\"30\" data-val-required=\"Please specify a valid Name.\" id=\"name\" name=\"name\">" + Environment.NewLine
              + "<option value=\"A\">Alpha</option>" + Environment.NewLine
              + "<option value=\"B\">Bravo</option>" + Environment.NewLine
              + "<option value=\"C\">Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToString());
        }

        [Fact]
        public void DropDownWithAttributesFromAnonymousObject_WithUnderscoreInName_TransformsUnderscoresToDashs()
        {
            HtmlHelperTest.AssertHelperTransformsAttributesUnderscoresToDashs((helper, attributes) =>
                helper.DropDownList("foo", GetSelectList(), attributes));

            HtmlHelperTest.AssertHelperTransformsAttributesUnderscoresToDashs((helper, attributes) =>
                helper.DropDownList("foo", "val", GetSelectList(), attributes));

            HtmlHelperTest.AssertHelperTransformsAttributesUnderscoresToDashs((helper, attributes) =>
                helper.DropDownList("foo", "val", GetSelectList(), "val", attributes));
        }

        // ListBox

        [Fact]
        public void ListBoxThrowsWithNoName()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act and assert
            Assert.ThrowsArgumentNullOrEmptyString(() => helper.ListBox(name: null, selectList: null), "name");
        }

        [Fact]
        public void ListBoxWithNoSelectedItem()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act
            var html = helper.ListBox("foo", GetSelectList());

            // Assert
            Assert.Equal(
                "<select id=\"foo\" name=\"foo\">" + Environment.NewLine
              + "<option value=\"A\">Alpha</option>" + Environment.NewLine
              + "<option value=\"B\">Bravo</option>" + Environment.NewLine
              + "<option value=\"C\">Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void ListBoxWithDefaultOption()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act
            var html = helper.ListBox("foo", "select-one", GetSelectList());

            // Assert
            Assert.Equal(
                "<select id=\"foo\" name=\"foo\">" + Environment.NewLine
              + "<option value=\"\">select-one</option>" + Environment.NewLine
              + "<option value=\"A\">Alpha</option>" + Environment.NewLine
              + "<option value=\"B\">Bravo</option>" + Environment.NewLine
              + "<option value=\"C\">Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void ListBoxWithAttributes()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act
            var html = helper.ListBox("foo", GetSelectList(), new { attr = "attr-val", attr2 = "attr-val2" });

            // Assert
            Assert.Equal(
                "<select attr=\"attr-val\" attr2=\"attr-val2\" id=\"foo\" name=\"foo\">" + Environment.NewLine
              + "<option value=\"A\">Alpha</option>" + Environment.NewLine
              + "<option value=\"B\">Bravo</option>" + Environment.NewLine
              + "<option value=\"C\">Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void ListBoxWithExplicitValue()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act
            var html = helper.ListBox("foo", null, GetSelectList(), "B", new Dictionary<string, object> { { "attr", "attr-val" } });

            // Assert
            Assert.Equal(
                "<select attr=\"attr-val\" id=\"foo\" name=\"foo\">" + Environment.NewLine
              + "<option value=\"A\">Alpha</option>" + Environment.NewLine
              + "<option selected=\"selected\" value=\"B\">Bravo</option>" + Environment.NewLine
              + "<option value=\"C\">Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void ListBoxWithModelValue()
        {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.SetModelValue("foo", "C");
            HtmlHelper helper = HtmlHelperFactory.Create(modelState);

            // Act
            var html = helper.ListBox("foo", GetSelectList(), new { attr = "attr-val" });

            // Assert
            Assert.Equal(
                "<select attr=\"attr-val\" id=\"foo\" name=\"foo\">" + Environment.NewLine
              + "<option value=\"A\">Alpha</option>" + Environment.NewLine
              + "<option value=\"B\">Bravo</option>" + Environment.NewLine
              + "<option selected=\"selected\" value=\"C\">Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void ListBoxWithExplicitMultipleValuesAndNoMultiple()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act
            var html = helper.ListBox("foo", null, GetSelectList(), new[] { "B", "C" }, new Dictionary<string, object> { { "attr", "attr-val" } });

            // Assert
            Assert.Equal(
                "<select attr=\"attr-val\" id=\"foo\" name=\"foo\">" + Environment.NewLine
              + "<option value=\"A\">Alpha</option>" + Environment.NewLine
              + "<option selected=\"selected\" value=\"B\">Bravo</option>" + Environment.NewLine
              + "<option value=\"C\">Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void ListBoxWithExplicitMultipleValuesAndMultiple()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act
            var html = helper.ListBox("foo", null, GetSelectList(), new[] { "B", "C" }, 4, true);

            // Assert
            Assert.Equal(
                "<select id=\"foo\" multiple=\"multiple\" name=\"foo\" size=\"4\">" + Environment.NewLine
              + "<option value=\"A\">Alpha</option>" + Environment.NewLine
              + "<option selected=\"selected\" value=\"B\">Bravo</option>" + Environment.NewLine
              + "<option selected=\"selected\" value=\"C\">Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void ListBoxWithMultipleModelValue()
        {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.SetModelValue("foo", new[] { "A", "C" });
            HtmlHelper helper = HtmlHelperFactory.Create(modelState);

            // Act
            var html = helper.ListBox("foo", GetSelectList(), new { attr = "attr-val" });

            // Assert
            Assert.Equal(
                "<select attr=\"attr-val\" id=\"foo\" name=\"foo\">" + Environment.NewLine
              + "<option selected=\"selected\" value=\"A\">Alpha</option>" + Environment.NewLine
              + "<option value=\"B\">Bravo</option>" + Environment.NewLine
              + "<option value=\"C\">Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void ListBoxWithModelValueAndExplicitSelectItem()
        {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.SetModelValue("foo", new[] { "C", "D" });
            HtmlHelper helper = HtmlHelperFactory.Create(modelState);
            var selectList = GetSelectList().ToList();
            selectList[1].Selected = true;

            // Act
            var html = helper.ListBox("foo", selectList, new { attr = "attr-val" });

            // Assert
            Assert.Equal(
                "<select attr=\"attr-val\" id=\"foo\" name=\"foo\">" + Environment.NewLine
              + "<option value=\"A\">Alpha</option>" + Environment.NewLine
              + "<option selected=\"selected\" value=\"B\">Bravo</option>" + Environment.NewLine
              + "<option value=\"C\">Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void ListBoxWithMultiSelectAndMultipleModelValue()
        {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.SetModelValue("foo", new[] { "A", "C" });
            HtmlHelper helper = HtmlHelperFactory.Create(modelState);

            // Act
            var html = helper.ListBox("foo", GetSelectList(), null, 4, true);

            // Assert
            Assert.Equal(
                "<select id=\"foo\" multiple=\"multiple\" name=\"foo\" size=\"4\">" + Environment.NewLine
              + "<option selected=\"selected\" value=\"A\">Alpha</option>" + Environment.NewLine
              + "<option value=\"B\">Bravo</option>" + Environment.NewLine
              + "<option selected=\"selected\" value=\"C\">Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void ListBoxWithMultiSelectAndMultipleExplicitValues()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act
            var html = helper.ListBox("foo", GetSelectList(), new[] { "A", "C" }, 4, true);

            // Assert
            Assert.Equal(
                "<select id=\"foo\" multiple=\"multiple\" name=\"foo\" size=\"4\">" + Environment.NewLine
              + "<option selected=\"selected\" value=\"A\">Alpha</option>" + Environment.NewLine
              + "<option value=\"B\">Bravo</option>" + Environment.NewLine
              + "<option selected=\"selected\" value=\"C\">Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void ListBoxWithMultiSelectAndExplitSelectValue()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();
            var selectList = GetSelectList().ToList();
            selectList.First().Selected = selectList.Last().Selected = true;

            // Act
            var html = helper.ListBox("foo", selectList, new[] { "B" }, 4, true);

            // Assert
            Assert.Equal(
                "<select id=\"foo\" multiple=\"multiple\" name=\"foo\" size=\"4\">" + Environment.NewLine
              + "<option selected=\"selected\" value=\"A\">Alpha</option>" + Environment.NewLine
              + "<option selected=\"selected\" value=\"B\">Bravo</option>" + Environment.NewLine
              + "<option selected=\"selected\" value=\"C\">Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void ListBoxWithExplictAndModelValue()
        {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.SetModelValue("foo", "C");
            HtmlHelper helper = HtmlHelperFactory.Create(modelState);

            // Act
            var html = helper.ListBox("foo", defaultOption: null, selectList: GetSelectList(),
                                      selectedValues: "B", htmlAttributes: new { attr = "attr-val" });

            // Assert
            Assert.Equal(
                "<select attr=\"attr-val\" id=\"foo\" name=\"foo\">" + Environment.NewLine
              + "<option value=\"A\">Alpha</option>" + Environment.NewLine
              + "<option selected=\"selected\" value=\"B\">Bravo</option>" + Environment.NewLine
              + "<option value=\"C\">Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void ListBoxWithErrorAndExplictAndModelState()
        {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.SetModelValue("foo", "C");
            modelState.AddError("foo", "test");
            HtmlHelper helper = HtmlHelperFactory.Create(modelState);

            // Act
            var html = helper.ListBox("foo.bar", "Select One", GetSelectList());

            // Assert
            Assert.Equal(
                "<select id=\"foo_bar\" name=\"foo.bar\">" + Environment.NewLine
              + "<option value=\"\">Select One</option>" + Environment.NewLine
              + "<option value=\"A\">Alpha</option>" + Environment.NewLine
              + "<option value=\"B\">Bravo</option>" + Environment.NewLine
              + "<option value=\"C\">Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void ListBoxWithNonStringModelValue()
        {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.SetModelValue("foo", 23);
            HtmlHelper helper = HtmlHelperFactory.Create(modelState);

            // Act
            var html = helper.ListBox("foo", null, GetSelectList(), new { attr = "attr-val" });

            // Assert
            Assert.Equal(
                "<select attr=\"attr-val\" id=\"foo\" name=\"foo\">" + Environment.NewLine
              + "<option value=\"A\">Alpha</option>" + Environment.NewLine
              + "<option value=\"B\">Bravo</option>" + Environment.NewLine
              + "<option value=\"C\">Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void ListBoxWithNonStringExplicitValue()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act
            var html = helper.ListBox("foo", null, GetSelectList(), new List<int>(), new { attr = "attr-val" });

            // Assert
            Assert.Equal(
                "<select attr=\"attr-val\" id=\"foo\" name=\"foo\">" + Environment.NewLine
              + "<option value=\"A\">Alpha</option>" + Environment.NewLine
              + "<option value=\"B\">Bravo</option>" + Environment.NewLine
              + "<option value=\"C\">Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void ListBoxWithErrors()
        {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.AddError("foo", "some error");
            HtmlHelper helper = HtmlHelperFactory.Create(modelState);

            // Act
            var html = helper.ListBox("foo", GetSelectList());

            // Assert
            Assert.Equal(
                "<select class=\"input-validation-error\" id=\"foo\" name=\"foo\">" + Environment.NewLine
              + "<option value=\"A\">Alpha</option>" + Environment.NewLine
              + "<option value=\"B\">Bravo</option>" + Environment.NewLine
              + "<option value=\"C\">Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void ListBoxWithErrorsAndCustomClass()
        {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.AddError("foo", "some error");
            HtmlHelper helper = HtmlHelperFactory.Create(modelState);

            // Act
            var html = helper.ListBox("foo", GetSelectList(), new { @class = "my-class" });

            // Assert
            Assert.Equal(
                "<select class=\"input-validation-error my-class\" id=\"foo\" name=\"foo\">" + Environment.NewLine
              + "<option value=\"A\">Alpha</option>" + Environment.NewLine
              + "<option value=\"B\">Bravo</option>" + Environment.NewLine
              + "<option value=\"C\">Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void ListBoxWithEmptyOptionLabel()
        {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.AddError("foo", "some error");
            HtmlHelper helper = HtmlHelperFactory.Create(modelState);

            // Act
            var html = helper.ListBox("foo", GetSelectList(), new { @class = "my-class" });

            // Assert
            Assert.Equal(
                "<select class=\"input-validation-error my-class\" id=\"foo\" name=\"foo\">" + Environment.NewLine
              + "<option value=\"A\">Alpha</option>" + Environment.NewLine
              + "<option value=\"B\">Bravo</option>" + Environment.NewLine
              + "<option value=\"C\">Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void ListBoxWithObjectDictionaryAndTitle()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act
            var html = helper.ListBox("foo", "Select One", GetSelectList(), new { @class = "my-class" });

            // Assert
            Assert.Equal(
                "<select class=\"my-class\" id=\"foo\" name=\"foo\">" + Environment.NewLine
              + "<option value=\"\">Select One</option>" + Environment.NewLine
              + "<option value=\"A\">Alpha</option>" + Environment.NewLine
              + "<option value=\"B\">Bravo</option>" + Environment.NewLine
              + "<option value=\"C\">Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void ListBoxWithDotReplacementForId()
        {
            // Arrange
            HtmlHelper helper = HtmlHelperFactory.Create();

            // Act
            var html = helper.ListBox("foo.bar", "Select One", GetSelectList());

            // Assert
            Assert.Equal(
                "<select id=\"foo_bar\" name=\"foo.bar\">" + Environment.NewLine
              + "<option value=\"\">Select One</option>" + Environment.NewLine
              + "<option value=\"A\">Alpha</option>" + Environment.NewLine
              + "<option value=\"B\">Bravo</option>" + Environment.NewLine
              + "<option value=\"C\">Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToHtmlString());
        }

        [Fact]
        public void ListBoxAddsUnobtrusiveValidationAttributes()
        {
            // Arrange
            const string fieldName = "name";
            var modelStateDictionary = new ModelStateDictionary();
            var validationHelper = new ValidationHelper(new Mock<HttpContextBase>().Object, modelStateDictionary);
            HtmlHelper helper = HtmlHelperFactory.Create(modelStateDictionary, validationHelper);

            // Act
            validationHelper.RequireField(fieldName, "Please specify a valid Name.");
            validationHelper.Add(fieldName, Validator.StringLength(30, errorMessage: "Name cannot exceed {0} characters"));
            var html = helper.ListBox(fieldName, GetSelectList(), htmlAttributes: new Dictionary<string, object> { { "data-some-val", "5" } });

            // Assert
            Assert.Equal(
                "<select data-some-val=\"5\" data-val=\"true\" data-val-length=\"Name cannot exceed 30 characters\" data-val-length-max=\"30\" data-val-required=\"Please specify a valid Name.\" id=\"name\" name=\"name\">" + Environment.NewLine
              + "<option value=\"A\">Alpha</option>" + Environment.NewLine
              + "<option value=\"B\">Bravo</option>" + Environment.NewLine
              + "<option value=\"C\">Charlie</option>" + Environment.NewLine
              + "</select>",
                html.ToString());
        }

        [Fact]
        public void ListBoxWithAttributesFromAnonymousObject_WithUnderscoreInName_TransformsUnderscoresToDashs()
        {
            HtmlHelperTest.AssertHelperTransformsAttributesUnderscoresToDashs((helper, attributes) =>
                helper.ListBox("foo", GetSelectList(), attributes));

            HtmlHelperTest.AssertHelperTransformsAttributesUnderscoresToDashs((helper, attributes) =>
                helper.ListBox("foo", "val", GetSelectList(), "val", attributes));

            HtmlHelperTest.AssertHelperTransformsAttributesUnderscoresToDashs((helper, attributes) =>
                helper.ListBox("foo", "val", GetSelectList(), "val", attributes));
        }

        private static IEnumerable<SelectListItem> GetSelectList()
        {
            yield return new SelectListItem() { Text = "Alpha", Value = "A" };
            yield return new SelectListItem() { Text = "Bravo", Value = "B" };
            yield return new SelectListItem() { Text = "Charlie", Value = "C" };
        }
    }
}
