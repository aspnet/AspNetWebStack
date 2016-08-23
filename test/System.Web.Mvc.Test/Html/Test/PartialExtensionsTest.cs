// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using Microsoft.TestCommon;

namespace System.Web.Mvc.Html.Test
{
    [Xunit.Collection("Uses ScopeStorage or ViewEngines.Engines")]
    public class PartialExtensionsTest
    {
        [Fact]
        public void PartialWithViewName()
        {
            // Arrange
            RenderPartialExtensionsTest.SpyHtmlHelper helper = RenderPartialExtensionsTest.SpyHtmlHelper.Create();

            // Act
            MvcHtmlString result = helper.Partial("partial-view");

            // Assert
            Assert.Equal("partial-view", helper.RenderPartialInternal_PartialViewName);
            Assert.Same(helper.ViewData, helper.RenderPartialInternal_ViewData);
            Assert.Null(helper.RenderPartialInternal_Model);
            Assert.IsType<StringWriter>(helper.RenderPartialInternal_Writer);
            Assert.Same(ViewEngines.Engines, helper.RenderPartialInternal_ViewEngineCollection);
            Assert.Equal("This is the result of the view", result.ToHtmlString());
        }

        [Fact]
        public void PartialWithViewNameAndViewData()
        {
            // Arrange
            RenderPartialExtensionsTest.SpyHtmlHelper helper = RenderPartialExtensionsTest.SpyHtmlHelper.Create();
            ViewDataDictionary viewData = new ViewDataDictionary();

            // Act
            MvcHtmlString result = helper.Partial("partial-view", viewData);

            // Assert
            Assert.Equal("partial-view", helper.RenderPartialInternal_PartialViewName);
            Assert.Same(viewData, helper.RenderPartialInternal_ViewData);
            Assert.Null(helper.RenderPartialInternal_Model);
            Assert.IsType<StringWriter>(helper.RenderPartialInternal_Writer);
            Assert.Same(ViewEngines.Engines, helper.RenderPartialInternal_ViewEngineCollection);
            Assert.Equal("This is the result of the view", result.ToHtmlString());
        }

        [Fact]
        public void PartialWithViewNameAndModel()
        {
            // Arrange
            RenderPartialExtensionsTest.SpyHtmlHelper helper = RenderPartialExtensionsTest.SpyHtmlHelper.Create();
            object model = new object();

            // Act
            MvcHtmlString result = helper.Partial("partial-view", model);

            // Assert
            Assert.Equal("partial-view", helper.RenderPartialInternal_PartialViewName);
            Assert.Same(helper.ViewData, helper.RenderPartialInternal_ViewData);
            Assert.Same(model, helper.RenderPartialInternal_Model);
            Assert.IsType<StringWriter>(helper.RenderPartialInternal_Writer);
            Assert.Same(ViewEngines.Engines, helper.RenderPartialInternal_ViewEngineCollection);
            Assert.Equal("This is the result of the view", result.ToHtmlString());
        }

        [Fact]
        public void PartialWithViewNameAndModelAndViewData()
        {
            // Arrange
            RenderPartialExtensionsTest.SpyHtmlHelper helper = RenderPartialExtensionsTest.SpyHtmlHelper.Create();
            object model = new object();
            ViewDataDictionary viewData = new ViewDataDictionary();

            // Act
            MvcHtmlString result = helper.Partial("partial-view", model, viewData);

            // Assert
            Assert.Equal("partial-view", helper.RenderPartialInternal_PartialViewName);
            Assert.Same(viewData, helper.RenderPartialInternal_ViewData);
            Assert.Same(model, helper.RenderPartialInternal_Model);
            Assert.IsType<StringWriter>(helper.RenderPartialInternal_Writer);
            Assert.Same(ViewEngines.Engines, helper.RenderPartialInternal_ViewEngineCollection);
            Assert.Equal("This is the result of the view", result.ToHtmlString());
        }
    }
}
