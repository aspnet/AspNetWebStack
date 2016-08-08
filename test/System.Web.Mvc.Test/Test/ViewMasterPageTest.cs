// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Web.Routing;
using System.Web.UI;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class ViewMasterPageTest
    {
        [Fact]
        public void GetModelFromViewPage()
        {
            // Arrange
            ViewMasterPage vmp = new ViewMasterPage();
            ViewPage vp = new ViewPage();
            vmp.Page = vp;
            object model = new object();
            vp.ViewData = new ViewDataDictionary(model);

            // Assert
            Assert.Equal(model, vmp.Model);
        }

        [Fact]
        public void GetModelFromViewPageStronglyTyped()
        {
            // Arrange
            ViewMasterPage<FooModel> vmp = new ViewMasterPage<FooModel>();
            ViewPage vp = new ViewPage();
            vmp.Page = vp;
            FooModel model = new FooModel();
            vp.ViewData = new ViewDataDictionary(model);

            // Assert
            Assert.Equal(model, vmp.Model);
        }

        [Fact]
        public void GetViewDataFromViewPage()
        {
            // Arrange
            ViewMasterPage vmp = new ViewMasterPage();
            ViewPage vp = new ViewPage();
            vmp.Page = vp;
            vp.ViewData = new ViewDataDictionary { { "a", "123" }, { "b", "456" } };

            // Assert
            Assert.Equal("123", vmp.ViewData.Eval("a"));
            Assert.Equal("456", vmp.ViewData.Eval("b"));
        }

        [Fact]
        public void GetViewItemFromViewPageTViewData()
        {
            // Arrange
            MockViewMasterPageDummyViewData vmp = new MockViewMasterPageDummyViewData();
            MockViewPageDummyViewData vp = new MockViewPageDummyViewData();
            vmp.Page = vp;
            vp.ViewData.Model = new DummyViewData { MyInt = 123, MyString = "abc" };

            // Assert
            Assert.Equal(123, vmp.ViewData.Model.MyInt);
            Assert.Equal("abc", vmp.ViewData.Model.MyString);
        }

        [Fact]
        public void GetWriterFromViewPage()
        {
            // Arrange
            bool triggered = false;
            HtmlTextWriter writer = new HtmlTextWriter(TextWriter.Null);
            ViewMasterPage vmp = new ViewMasterPage();
            MockViewPage vp = new MockViewPage();
            vp.RenderCallback = delegate()
            {
                triggered = true;
                Assert.Equal(writer, vmp.Writer);
            };
            vmp.Page = vp;

            // Act & Assert
            Assert.Null(vmp.Writer);
            vp.RenderControl(writer);
            Assert.Null(vmp.Writer);
            Assert.True(triggered);
        }

        [Fact]
        public void GetViewDataFromPageThrows()
        {
            // Arrange
            ViewMasterPage vmp = new ViewMasterPage();
            vmp.Page = new Page();

            // Assert
            Assert.Throws<InvalidOperationException>(
                delegate { object foo = vmp.ViewData; },
                "A ViewMasterPage can be used only with content pages that derive from ViewPage or ViewPage<TModel>.");
        }

        [Fact]
        public void GetViewItemFromWrongGenericViewPageType()
        {
            // Arrange
            MockViewMasterPageDummyViewData vmp = new MockViewMasterPageDummyViewData();
            MockViewPageBogusViewData vp = new MockViewPageBogusViewData();
            vmp.Page = vp;
            vp.ViewData.Model = new SelectListItem();

            // Assert
            Assert.Throws<InvalidOperationException>(
                delegate { object foo = vmp.ViewData.Model; },
                "The model item passed into the dictionary is of type 'System.Web.Mvc.SelectListItem', but this dictionary requires a model item of type 'System.Web.Mvc.Test.ViewMasterPageTest+DummyViewData'.");
        }

        [Fact]
        public void GetViewDataFromNullPageThrows()
        {
            // Arrange
            MockViewMasterPageDummyViewData vmp = new MockViewMasterPageDummyViewData();

            // Assert
            Assert.Throws<InvalidOperationException>(
                delegate { object foo = vmp.ViewData; },
                "A ViewMasterPage can be used only with content pages that derive from ViewPage or ViewPage<TModel>.");
        }

        [Fact]
        public void GetViewDataFromRegularPageThrows()
        {
            // Arrange
            MockViewMasterPageDummyViewData vmp = new MockViewMasterPageDummyViewData();
            vmp.Page = new Page();

            // Assert
            Assert.Throws<InvalidOperationException>(
                delegate { object foo = vmp.ViewData; },
                "A ViewMasterPage can be used only with content pages that derive from ViewPage or ViewPage<TModel>.");
        }

        [Fact]
        public void GetHtmlHelperFromViewPage()
        {
            // Arrange
            ViewMasterPage vmp = new ViewMasterPage();
            ViewPage vp = new ViewPage();
            vmp.Page = vp;
            ViewContext vc = new Mock<ViewContext>().Object;

            HtmlHelper<object> htmlHelper = new HtmlHelper<object>(vc, vp);
            vp.Html = htmlHelper;

            // Assert
            Assert.Equal(vmp.Html, htmlHelper);
        }

        [Fact]
        public void GetUrlHelperFromViewPage()
        {
            // Arrange
            ViewMasterPage vmp = new ViewMasterPage();
            ViewPage vp = new ViewPage();
            vmp.Page = vp;
            RequestContext rc = new RequestContext(new Mock<HttpContextBase>().Object, new RouteData());
            UrlHelper urlHelper = new UrlHelper(rc);
            vp.Url = urlHelper;

            // Assert
            Assert.Equal(vmp.Url, urlHelper);
        }

        [Fact]
        public void ViewBagProperty_ReflectsViewData()
        {
            // Arrange
            ViewPage page = new ViewPage();
            ViewMasterPage masterPage = new ViewMasterPage();
            masterPage.Page = page;
            masterPage.ViewData["A"] = 1;

            // Act & Assert
            Assert.NotNull(masterPage.ViewBag);
            Assert.Equal(1, masterPage.ViewBag.A);
        }

        [Fact]
        public void ViewBagProperty_PropagatesChangesToViewData()
        {
            // Arrange
            ViewPage page = new ViewPage();
            ViewMasterPage masterPage = new ViewMasterPage();
            masterPage.Page = page;
            masterPage.ViewData["A"] = 1;

            // Act
            masterPage.ViewBag.A = "foo";
            masterPage.ViewBag.B = 2;

            // Assert
            Assert.Equal("foo", masterPage.ViewData["A"]);
            Assert.Equal(2, masterPage.ViewData["B"]);
        }

        // Master page types
        private sealed class MockViewMasterPageDummyViewData : ViewMasterPage<DummyViewData>
        {
        }

        // View data types
        private sealed class DummyViewData
        {
            public int MyInt { get; set; }
            public string MyString { get; set; }
        }

        // Page types
        private sealed class MockViewPageBogusViewData : ViewPage<SelectListItem>
        {
        }

        private sealed class MockViewPageDummyViewData : ViewPage<DummyViewData>
        {
        }

        private sealed class MockViewPage : ViewPage
        {
            public Action RenderCallback { get; set; }

            protected override void RenderChildren(HtmlTextWriter writer)
            {
                if (RenderCallback != null)
                {
                    RenderCallback();
                }
                base.RenderChildren(writer);
            }
        }

        private sealed class FooModel
        {
        }
    }
}
