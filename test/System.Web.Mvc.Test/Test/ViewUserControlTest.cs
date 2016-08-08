// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Web.Routing;
using System.Web.TestUtil;
using System.Web.UI;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class ViewUserControlTest
    {
        [Fact]
        public void ModelProperty()
        {
            // Arrange
            object model = new object();
            ViewDataDictionary viewData = new ViewDataDictionary(model);
            ViewUserControl viewUserControl = new ViewUserControl();
            viewUserControl.ViewData = viewData;

            // Act
            object viewPageModel = viewUserControl.Model;

            // Assert
            Assert.Equal(model, viewPageModel);
            Assert.Equal(model, viewUserControl.ViewData.Model);
        }

        [Fact]
        public void ModelPropertyStronglyTyped()
        {
            // Arrange
            FooModel model = new FooModel();
            ViewDataDictionary<FooModel> viewData = new ViewDataDictionary<FooModel>(model);
            ViewUserControl<FooModel> viewUserControl = new ViewUserControl<FooModel>();
            viewUserControl.ViewData = viewData;

            // Act
            object viewPageModelObject = ((ViewUserControl)viewUserControl).Model;
            FooModel viewPageModelPerson = viewUserControl.Model;

            // Assert
            Assert.Equal(model, viewPageModelObject);
            Assert.Equal(model, viewPageModelPerson);
        }

        [Fact]
        public void RenderViewAndRestoreContentType()
        {
            // Arrange
            Mock<ViewContext> mockViewContext = new Mock<ViewContext>();
            mockViewContext.SetupProperty(c => c.HttpContext.Response.ContentType);
            ViewContext vc = mockViewContext.Object;

            Mock<ViewPage> mockViewPage = new Mock<ViewPage>();
            mockViewPage.Setup(vp => vp.RenderView(vc)).Callback(() => vc.HttpContext.Response.ContentType = "newContentType");

            // Act
            vc.HttpContext.Response.ContentType = "oldContentType";
            ViewUserControl.RenderViewAndRestoreContentType(mockViewPage.Object, vc);
            string postContentType = vc.HttpContext.Response.ContentType;

            // Assert
            Assert.Equal("oldContentType", postContentType);
        }

        [Fact]
        public void SetViewItem()
        {
            // Arrange
            ViewUserControl vuc = new ViewUserControl();
            object viewItem = new object();
            vuc.ViewData = new ViewDataDictionary(viewItem);

            // Act
            vuc.ViewData.Model = viewItem;
            object newViewItem = vuc.ViewData.Model;

            // Assert
            Assert.Same(viewItem, newViewItem);
        }

        [Fact]
        public void SetViewItemOnBaseClassPropagatesToDerivedClass()
        {
            // Arrange
            ViewUserControl<object> vucInt = new ViewUserControl<object>();
            ViewUserControl vuc = vucInt;
            vuc.ViewData = new ViewDataDictionary();
            object o = new object();

            // Act
            vuc.ViewData.Model = o;

            // Assert
            Assert.Equal(o, vucInt.ViewData.Model);
            Assert.Equal(o, vuc.ViewData.Model);
        }

        [Fact]
        public void SetViewItemOnDerivedClassPropagatesToBaseClass()
        {
            // Arrange
            ViewUserControl<object> vucInt = new ViewUserControl<object>();
            ViewUserControl vuc = vucInt;
            vucInt.ViewData = new ViewDataDictionary<object>();
            object o = new object();

            // Act
            vucInt.ViewData.Model = o;

            // Assert
            Assert.Equal(o, vucInt.ViewData.Model);
            Assert.Equal(o, vuc.ViewData.Model);
        }

        [Fact]
        public void SetViewItemToWrongTypeThrows()
        {
            // Arrange
            ViewUserControl<string> vucString = new ViewUserControl<string>();
            vucString.ViewData = new ViewDataDictionary<string>();
            ViewUserControl vuc = vucString;

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                delegate { vuc.ViewData.Model = 50; },
                "The model item passed into the dictionary is of type 'System.Int32', but this dictionary requires a model item of type 'System.String'.");
        }

        [Fact]
        public void GetViewDataWhenNoPageSetThrows()
        {
            ViewUserControl vuc = new ViewUserControl();
            vuc.AppRelativeVirtualPath = "~/Foo.ascx";

            Assert.Throws<InvalidOperationException>(
                delegate { var foo = vuc.ViewData["Foo"]; },
                "The ViewUserControl '~/Foo.ascx' cannot find an IViewDataContainer object. The ViewUserControl must be inside a ViewPage, a ViewMasterPage, or another ViewUserControl.");
        }

        [Fact]
        public void GetViewDataWhenRegularPageSetThrows()
        {
            Page p = new Page();
            p.Controls.Add(new Control());
            ViewUserControl vuc = new ViewUserControl();
            p.Controls[0].Controls.Add(vuc);
            vuc.AppRelativeVirtualPath = "~/Foo.ascx";

            Assert.Throws<InvalidOperationException>(
                delegate { var foo = vuc.ViewData["Foo"]; },
                "The ViewUserControl '~/Foo.ascx' cannot find an IViewDataContainer object. The ViewUserControl must be inside a ViewPage, a ViewMasterPage, or another ViewUserControl.");
        }

        [Fact]
        public void GetViewDataFromViewPage()
        {
            // Arrange
            ViewPage p = new ViewPage();
            p.Controls.Add(new Control());
            ViewUserControl vuc = new ViewUserControl();
            p.Controls[0].Controls.Add(vuc);
            p.ViewData = new ViewDataDictionary { { "FirstName", "Joe" }, { "LastName", "Schmoe" } };

            // Act
            object firstName = vuc.ViewData.Eval("FirstName");
            object lastName = vuc.ViewData.Eval("LastName");

            // Assert
            Assert.Equal("Joe", firstName);
            Assert.Equal("Schmoe", lastName);
        }

        [Fact]
        public void GetViewDataFromViewPageWithViewDataKeyPointingToObject()
        {
            // Arrange
            ViewDataDictionary vdd = new ViewDataDictionary()
            {
                { "Foo", "FooParent" },
                { "Bar", "BarParent" },
                { "Child", new object() }
            };

            ViewPage p = new ViewPage();
            p.Controls.Add(new Control());
            ViewUserControl vuc = new ViewUserControl() { ViewDataKey = "Child" };
            p.Controls[0].Controls.Add(vuc);
            p.ViewData = vdd;

            // Act
            object oFoo = vuc.ViewData.Eval("Foo");
            object oBar = vuc.ViewData.Eval("Bar");

            // Assert
            Assert.Equal(vdd["Child"], vuc.ViewData.Model);
            Assert.Equal("FooParent", oFoo);
            Assert.Equal("BarParent", oBar);
        }

        [Fact]
        public void GetViewDataFromViewPageWithViewDataKeyPointingToViewDataDictionary()
        {
            // Arrange
            ViewDataDictionary vdd = new ViewDataDictionary()
            {
                { "Foo", "FooParent" },
                { "Bar", "BarParent" },
                {
                    "Child",
                    new ViewDataDictionary()
                    {
                        { "Foo", "FooChild" },
                        { "Bar", "BarChild" }
                    }
                    }
            };

            ViewPage p = new ViewPage();
            p.Controls.Add(new Control());
            ViewUserControl vuc = new ViewUserControl() { ViewDataKey = "Child" };
            p.Controls[0].Controls.Add(vuc);
            p.ViewData = vdd;

            // Act
            object oFoo = vuc.ViewData.Eval("Foo");
            object oBar = vuc.ViewData.Eval("Bar");

            // Assert
            Assert.Equal(vdd["Child"], vuc.ViewData);
            Assert.Equal("FooChild", oFoo);
            Assert.Equal("BarChild", oBar);
        }

        [Fact]
        public void GetViewDataFromViewUserControl()
        {
            // Arrange
            ViewPage p = new ViewPage();
            p.Controls.Add(new Control());
            ViewUserControl outerVuc = new ViewUserControl();
            p.Controls[0].Controls.Add(outerVuc);
            outerVuc.Controls.Add(new Control());
            ViewUserControl vuc = new ViewUserControl();
            outerVuc.Controls[0].Controls.Add(vuc);

            p.ViewData = new ViewDataDictionary { { "FirstName", "Joe" }, { "LastName", "Schmoe" } };

            // Act
            object firstName = vuc.ViewData.Eval("FirstName");
            object lastName = vuc.ViewData.Eval("LastName");

            // Assert
            Assert.Equal("Joe", firstName);
            Assert.Equal("Schmoe", lastName);
        }

        [Fact]
        public void GetViewDataFromViewUserControlWithViewDataKeyOnInnerControl()
        {
            // Arrange
            ViewPage p = new ViewPage();
            p.Controls.Add(new Control());
            ViewUserControl outerVuc = new ViewUserControl();
            p.Controls[0].Controls.Add(outerVuc);
            outerVuc.Controls.Add(new Control());
            ViewUserControl vuc = new ViewUserControl() { ViewDataKey = "SubData" };
            outerVuc.Controls[0].Controls.Add(vuc);

            p.ViewData = new ViewDataDictionary { { "FirstName", "Joe" }, { "LastName", "Schmoe" } };
            p.ViewData["SubData"] = new ViewDataDictionary { { "FirstName", "SubJoe" }, { "LastName", "SubSchmoe" } };

            // Act
            object firstName = vuc.ViewData.Eval("FirstName");
            object lastName = vuc.ViewData.Eval("LastName");

            // Assert
            Assert.Equal("SubJoe", firstName);
            Assert.Equal("SubSchmoe", lastName);
        }

        [Fact]
        public void GetViewDataFromViewUserControlWithViewDataKeyOnOuterControl()
        {
            // Arrange
            ViewPage p = new ViewPage();
            p.Controls.Add(new Control());
            ViewUserControl outerVuc = new ViewUserControl() { ViewDataKey = "SubData" };
            p.Controls[0].Controls.Add(outerVuc);
            outerVuc.Controls.Add(new Control());
            ViewUserControl vuc = new ViewUserControl();
            outerVuc.Controls[0].Controls.Add(vuc);

            p.ViewData = new ViewDataDictionary { { "FirstName", "Joe" }, { "LastName", "Schmoe" } };
            p.ViewData["SubData"] = new ViewDataDictionary { { "FirstName", "SubJoe" }, { "LastName", "SubSchmoe" } };

            // Act
            object firstName = vuc.ViewData.Eval("FirstName");
            object lastName = vuc.ViewData.Eval("LastName");

            // Assert
            Assert.Equal("SubJoe", firstName);
            Assert.Equal("SubSchmoe", lastName);
        }

        [Fact]
        public void ViewDataKeyProperty()
        {
            MemberHelper.TestStringProperty(new ViewUserControl(), "ViewDataKey", String.Empty, testDefaultValueAttribute: true);
        }

        [Fact]
        public void GetWrongGenericViewItemTypeThrows()
        {
            // Arrange
            ViewPage p = new ViewPage();
            p.ViewData = new ViewDataDictionary();
            p.ViewData["Foo"] = new DummyViewData { MyInt = 123, MyString = "Whatever" };

            MockViewUserControl<MyViewData> vuc = new MockViewUserControl<MyViewData>() { ViewDataKey = "FOO" };
            vuc.AppRelativeVirtualPath = "~/Foo.aspx";
            p.Controls.Add(new Control());
            p.Controls[0].Controls.Add(vuc);

            // Act
            Assert.Throws<InvalidOperationException>(
                delegate { var foo = vuc.ViewData.Model.IntProp; },
                @"The model item passed into the dictionary is of type 'System.Web.Mvc.Test.ViewUserControlTest+DummyViewData', but this dictionary requires a model item of type 'System.Web.Mvc.Test.ViewUserControlTest+MyViewData'.");
        }

        [Fact]
        public void GetGenericViewItemType()
        {
            // Arrange
            ViewPage p = new ViewPage();
            p.Controls.Add(new Control());
            MockViewUserControl<MyViewData> vuc = new MockViewUserControl<MyViewData>() { ViewDataKey = "FOO" };
            p.Controls[0].Controls.Add(vuc);
            p.ViewData = new ViewDataDictionary();
            p.ViewData["Foo"] = new MyViewData { IntProp = 123, StringProp = "miao" };

            // Act
            int intProp = vuc.ViewData.Model.IntProp;
            string stringProp = vuc.ViewData.Model.StringProp;

            // Assert
            Assert.Equal(123, intProp);
            Assert.Equal("miao", stringProp);
        }

        [Fact]
        public void GetHtmlHelperFromViewPage()
        {
            // Arrange
            ViewUserControl vuc = new ViewUserControl();
            ViewPage containerPage = new ViewPage();
            containerPage.Controls.Add(vuc);
            ViewContext vc = new Mock<ViewContext>().Object;
            vuc.ViewContext = vc;

            // Act
            HtmlHelper htmlHelper = vuc.Html;

            // Assert
            Assert.Equal(vc, htmlHelper.ViewContext);
            Assert.Equal(vuc, htmlHelper.ViewDataContainer);
        }

        [Fact]
        public void GetHtmlHelperFromRegularPage()
        {
            // Arrange
            ViewUserControl vuc = new ViewUserControl();
            Page containerPage = new Page();
            containerPage.Controls.Add(vuc);

            // Assert
            Assert.Throws<InvalidOperationException>(
                delegate { HtmlHelper foo = vuc.Html; },
                "A ViewUserControl can be used only in pages that derive from ViewPage or ViewPage<TModel>.");
        }

        [Fact]
        public void GetUrlHelperFromViewPage()
        {
            // Arrange
            ViewUserControl vuc = new ViewUserControl();
            ViewPage containerPage = new ViewPage();
            containerPage.Controls.Add(vuc);
            RequestContext rc = new RequestContext(new Mock<HttpContextBase>().Object, new RouteData());
            UrlHelper urlHelper = new UrlHelper(rc);
            containerPage.Url = urlHelper;

            // Assert
            Assert.Equal(vuc.Url, urlHelper);
        }

        [Fact]
        public void GetUrlHelperFromRegularPage()
        {
            // Arrange
            ViewUserControl vuc = new ViewUserControl();
            Page containerPage = new Page();
            containerPage.Controls.Add(vuc);

            // Assert
            Assert.Throws<InvalidOperationException>(
                delegate { UrlHelper foo = vuc.Url; },
                "A ViewUserControl can be used only in pages that derive from ViewPage or ViewPage<TModel>.");
        }

        [Fact]
        public void GetWriterFromViewPage()
        {
            // Arrange
            MockViewUserControl vuc = new MockViewUserControl();
            MockViewUserControlContainerPage containerPage = new MockViewUserControlContainerPage(vuc);
            bool triggered = false;
            HtmlTextWriter writer = new HtmlTextWriter(TextWriter.Null);
            containerPage.RenderCallback = delegate()
            {
                triggered = true;
                Assert.Equal(writer, vuc.Writer);
            };

            // Act & Assert
            Assert.Null(vuc.Writer);
            containerPage.RenderControl(writer);
            Assert.Null(vuc.Writer);
            Assert.True(triggered);
        }

        [Fact]
        public void GetWriterFromRegularPageThrows()
        {
            // Arrange
            MockViewUserControl vuc = new MockViewUserControl();
            Page containerPage = new Page();
            containerPage.Controls.Add(vuc);

            // Act
            Assert.Throws<InvalidOperationException>(
                delegate { HtmlTextWriter writer = vuc.Writer; },
                "A ViewUserControl can be used only in pages that derive from ViewPage or ViewPage<TModel>.");
        }

        [Fact]
        public void ViewBagProperty_ReflectsViewData()
        {
            // Arrange
            ViewPage containerPage = new ViewPage();
            ViewUserControl userControl = new ViewUserControl();
            containerPage.Controls.Add(userControl);
            userControl.ViewData["A"] = 1;

            // Act & Assert
            Assert.NotNull(userControl.ViewBag);
            Assert.Equal(1, userControl.ViewBag.A);
        }

        [Fact]
        public void ViewBagProperty_ReflectsNewViewDataInstance()
        {
            // Arrange
            ViewPage containerPage = new ViewPage();
            ViewUserControl userControl = new ViewUserControl();
            containerPage.Controls.Add(userControl);
            userControl.ViewData["A"] = 1;
            userControl.ViewData = new ViewDataDictionary() { { "A", "bar" } };

            // Act & Assert
            Assert.Equal("bar", userControl.ViewBag.A);
        }

        [Fact]
        public void ViewBagProperty_PropagatesChangesToViewData()
        {
            // Arrange
            ViewPage containerPage = new ViewPage();
            ViewUserControl userControl = new ViewUserControl();
            containerPage.Controls.Add(userControl);
            userControl.ViewData["A"] = 1;

            // Act
            userControl.ViewBag.A = "foo";
            userControl.ViewBag.B = 2;

            // Assert
            Assert.Equal("foo", userControl.ViewData["A"]);
            Assert.Equal(2, userControl.ViewData["B"]);
        }

        private sealed class DummyViewData
        {
            public int MyInt { get; set; }
            public string MyString { get; set; }
        }

        private sealed class MockViewUserControlContainerPage : ViewPage
        {
            public Action RenderCallback { get; set; }

            public MockViewUserControlContainerPage(ViewUserControl userControl)
            {
                Controls.Add(userControl);
            }

            protected override void RenderChildren(HtmlTextWriter writer)
            {
                if (RenderCallback != null)
                {
                    RenderCallback();
                }
                base.RenderChildren(writer);
            }
        }

        private sealed class MockViewUserControl : ViewUserControl
        {
        }

        private sealed class MockViewUserControl<TViewData> : ViewUserControl<TViewData>
        {
        }

        private sealed class MyViewData
        {
            public int IntProp { get; set; }
            public string StringProp { get; set; }
        }

        private sealed class FooModel
        {
        }
    }
}
