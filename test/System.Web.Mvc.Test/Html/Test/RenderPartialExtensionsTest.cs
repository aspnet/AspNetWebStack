// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Html.Test
{
    [Xunit.Collection("Uses ScopeStorage or ViewEngines.Engines")]
    public class RenderPartialExtensionsTest
    {
        [Fact]
        public void RenderPartialWithViewName()
        {
            // Arrange
            SpyHtmlHelper helper = SpyHtmlHelper.Create();

            // Act
            helper.RenderPartial("partial-view");

            // Assert
            Assert.Equal("partial-view", helper.RenderPartialInternal_PartialViewName);
            Assert.Same(helper.ViewData, helper.RenderPartialInternal_ViewData);
            Assert.Null(helper.RenderPartialInternal_Model);
            Assert.Same(helper.ViewContext.Writer, helper.RenderPartialInternal_Writer);
            Assert.Same(ViewEngines.Engines, helper.RenderPartialInternal_ViewEngineCollection);
        }

        [Fact]
        public void RenderPartialWithViewNameAndViewData()
        {
            // Arrange
            SpyHtmlHelper helper = SpyHtmlHelper.Create();
            ViewDataDictionary viewData = new ViewDataDictionary();

            // Act
            helper.RenderPartial("partial-view", viewData);

            // Assert
            Assert.Equal("partial-view", helper.RenderPartialInternal_PartialViewName);
            Assert.Same(viewData, helper.RenderPartialInternal_ViewData);
            Assert.Null(helper.RenderPartialInternal_Model);
            Assert.Same(helper.ViewContext.Writer, helper.RenderPartialInternal_Writer);
            Assert.Same(ViewEngines.Engines, helper.RenderPartialInternal_ViewEngineCollection);
        }

        [Fact]
        public void RenderPartialWithViewNameAndModel()
        {
            // Arrange
            SpyHtmlHelper helper = SpyHtmlHelper.Create();
            object model = new object();

            // Act
            helper.RenderPartial("partial-view", model);

            // Assert
            Assert.Equal("partial-view", helper.RenderPartialInternal_PartialViewName);
            Assert.Same(helper.ViewData, helper.RenderPartialInternal_ViewData);
            Assert.Same(model, helper.RenderPartialInternal_Model);
            Assert.Same(helper.ViewContext.Writer, helper.RenderPartialInternal_Writer);
            Assert.Same(ViewEngines.Engines, helper.RenderPartialInternal_ViewEngineCollection);
        }

        [Fact]
        public void RenderPartialWithViewNameAndModelAndViewData()
        {
            // Arrange
            SpyHtmlHelper helper = SpyHtmlHelper.Create();
            object model = new object();
            ViewDataDictionary viewData = new ViewDataDictionary();

            // Act
            helper.RenderPartial("partial-view", model, viewData);

            // Assert
            Assert.Equal("partial-view", helper.RenderPartialInternal_PartialViewName);
            Assert.Same(viewData, helper.RenderPartialInternal_ViewData);
            Assert.Same(model, helper.RenderPartialInternal_Model);
            Assert.Same(helper.ViewContext.Writer, helper.RenderPartialInternal_Writer);
            Assert.Same(ViewEngines.Engines, helper.RenderPartialInternal_ViewEngineCollection);
        }

        internal class SpyHtmlHelper : HtmlHelper
        {
            public string RenderPartialInternal_PartialViewName;
            public ViewDataDictionary RenderPartialInternal_ViewData;
            public object RenderPartialInternal_Model;
            public TextWriter RenderPartialInternal_Writer;
            public ViewEngineCollection RenderPartialInternal_ViewEngineCollection;

            SpyHtmlHelper(ViewContext viewContext, IViewDataContainer viewDataContainer)
                : base(viewContext, viewDataContainer)
            {
            }

            public static SpyHtmlHelper Create()
            {
                ViewDataDictionary viewData = new ViewDataDictionary();

                Mock<ViewContext> mockViewContext = new Mock<ViewContext>();
                mockViewContext.Setup(c => c.HttpContext.Response.Output).Throws(new Exception("Response.Output should never be called."));
                mockViewContext.Setup(c => c.ViewData).Returns(viewData);
                mockViewContext.Setup(c => c.Writer).Returns(new StringWriter());

                Mock<IViewDataContainer> container = new Mock<IViewDataContainer>();
                container.Setup(c => c.ViewData).Returns(viewData);

                return new SpyHtmlHelper(mockViewContext.Object, container.Object);
            }

            internal override void RenderPartialInternal(string partialViewName, ViewDataDictionary viewData, object model,
                                                         TextWriter writer, ViewEngineCollection viewEngineCollection)
            {
                RenderPartialInternal_PartialViewName = partialViewName;
                RenderPartialInternal_ViewData = viewData;
                RenderPartialInternal_Model = model;
                RenderPartialInternal_Writer = writer;
                RenderPartialInternal_ViewEngineCollection = viewEngineCollection;

                writer.Write("This is the result of the view");
            }
        }
    }
}
