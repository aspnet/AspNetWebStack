// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Web.UI;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class ViewPageTest
    {
        [Fact]
        public void ModelProperty()
        {
            // Arrange
            object model = new object();
            ViewDataDictionary viewData = new ViewDataDictionary(model);
            ViewPage viewPage = new ViewPage();
            viewPage.ViewData = viewData;

            // Act
            object viewPageModel = viewPage.Model;

            // Assert
            Assert.Equal(model, viewPageModel);
            Assert.Equal(model, viewPage.ViewData.Model);
        }

        [Fact]
        public void ModelPropertyStronglyTypedViewPage()
        {
            // Arrange
            FooModel model = new FooModel();
            ViewDataDictionary<FooModel> viewData = new ViewDataDictionary<FooModel>(model);
            ViewPage<FooModel> viewPage = new ViewPage<FooModel>();
            viewPage.ViewData = viewData;

            // Act
            object viewPageModelObject = ((ViewPage)viewPage).Model;
            FooModel viewPageModelPerson = viewPage.Model;

            // Assert
            Assert.Equal(model, viewPageModelObject);
            Assert.Equal(model, viewPageModelPerson);
        }

        [Fact]
        public void SetViewItemOnBaseClassPropagatesToDerivedClass()
        {
            // Arrange
            ViewPage<object> vpInt = new ViewPage<object>();
            ViewPage vp = vpInt;
            object o = new object();

            // Act
            vp.ViewData.Model = o;

            // Assert
            Assert.Equal(o, vpInt.ViewData.Model);
            Assert.Equal(o, vp.ViewData.Model);
        }

        [Fact]
        public void SetViewItemOnDerivedClassPropagatesToBaseClass()
        {
            // Arrange
            ViewPage<object> vpInt = new ViewPage<object>();
            ViewPage vp = vpInt;
            object o = new object();

            // Act
            vpInt.ViewData.Model = o;

            // Assert
            Assert.Equal(o, vpInt.ViewData.Model);
            Assert.Equal(o, vp.ViewData.Model);
        }

        [Fact]
        public void SetViewItemToWrongTypeThrows()
        {
            // Arrange
            ViewPage vp = new ViewPage<string>();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                delegate { vp.ViewData.Model = 50; },
                "The model item passed into the dictionary is of type 'System.Int32', but this dictionary requires a model item of type 'System.String'.");
        }

        [Fact]
        public void RenderInitsHelpersAndSetsID()
        {
            // Arrange
            ViewPageWithNoProcessRequest viewPage = new ViewPageWithNoProcessRequest();
            TextWriter writer = new StringWriter();

            Mock<ViewContext> mockViewContext = new Mock<ViewContext>();
            mockViewContext.Setup(c => c.Writer).Returns(writer);
            mockViewContext.Setup(c => c.HttpContext.Response.Output).Returns(TextWriter.Null);
            mockViewContext.Setup(c => c.HttpContext.Server.Execute(It.IsAny<IHttpHandler>(), It.IsAny<TextWriter>(), true))
                .Callback<IHttpHandler, TextWriter, bool>((_h, _w, _pf) =>
                {
                    ViewPage.SwitchWriter switchWriter = _w as ViewPage.SwitchWriter;
                    Assert.NotNull(switchWriter);
                    Assert.Same(writer, switchWriter.InnerWriter);
                })
                .Verifiable();

            // Act
            viewPage.RenderView(mockViewContext.Object);

            // Assert
            mockViewContext.Verify();
            Assert.NotNull(viewPage.Ajax);
            Assert.NotNull(viewPage.Html);
            Assert.NotNull(viewPage.Url);
        }

        [Fact]
        public void GenericPageRenderInitsHelpersAndSetsID()
        {
            // Arrange
            Mock<ViewContext> mockViewContext = new Mock<ViewContext>();
            mockViewContext.Setup(c => c.Writer).Returns(new StringWriter());
            mockViewContext.Setup(c => c.HttpContext.Response.Output).Returns(TextWriter.Null);
            mockViewContext.Setup(c => c.HttpContext.Server).Returns(new Mock<HttpServerUtilityBase>().Object);

            ViewPageWithNoProcessRequest<Controller> viewPage = new ViewPageWithNoProcessRequest<Controller>();

            // Act
            viewPage.RenderView(mockViewContext.Object);

            // Assert
            Assert.NotNull(viewPage.Ajax);
            Assert.NotNull(viewPage.Html);
            Assert.NotNull(viewPage.Url);
            Assert.NotNull(((ViewPage)viewPage).Html);
            Assert.NotNull(((ViewPage)viewPage).Url);
        }

        private static void WriterSetCorrectlyInternal(bool throwException)
        {
            // Arrange
            bool triggered = false;
            HtmlTextWriter writer = new HtmlTextWriter(TextWriter.Null);
            MockViewPage vp = new MockViewPage();
            vp.RenderCallback = delegate()
            {
                triggered = true;
                Assert.Equal(writer, vp.Writer);
                if (throwException)
                {
                    throw new CallbackException();
                }
            };

            // Act & Assert
            Assert.Null(vp.Writer);
            try
            {
                vp.RenderControl(writer);
            }
            catch (CallbackException)
            {
            }
            Assert.Null(vp.Writer);
            Assert.True(triggered);
        }

        [Fact]
        public void WriterSetCorrectly()
        {
            WriterSetCorrectlyInternal(false /* throwException */);
        }

        [Fact]
        public void WriterSetCorrectlyThrowException()
        {
            WriterSetCorrectlyInternal(true /* throwException */);
        }

        private sealed class ViewPageWithNoProcessRequest : ViewPage
        {
            public override void ProcessRequest(HttpContext context)
            {
            }
        }

        private sealed class ViewPageWithNoProcessRequest<TModel> : ViewPage<TModel>
        {
            public override void ProcessRequest(HttpContext context)
            {
            }
        }

        [Fact]
        public void ViewBagProperty_ReflectsViewData()
        {
            // Arrange
            ViewPage page = new ViewPage();
            page.ViewData["A"] = 1;

            // Act & Assert
            Assert.NotNull(page.ViewBag);
            Assert.Equal(1, page.ViewBag.A);
        }

        [Fact]
        public void ViewBagProperty_ReflectsNewViewDataInstance()
        {
            // Arrange
            ViewPage page = new ViewPage();
            page.ViewData["A"] = 1;
            page.ViewData = new ViewDataDictionary() { { "A", "bar" } };

            // Act & Assert
            Assert.Equal("bar", page.ViewBag.A);
        }

        [Fact]
        public void ViewBagProperty_PropagatesChangesToViewData()
        {
            // Arrange
            ViewPage page = new ViewPage();
            page.ViewData["A"] = 1;

            // Act
            page.ViewBag.A = "foo";
            page.ViewBag.B = 2;

            // Assert
            Assert.Equal("foo", page.ViewData["A"]);
            Assert.Equal(2, page.ViewData["B"]);
        }

        private sealed class MockViewPage : ViewPage
        {
            public MockViewPage()
            {
            }

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

        private sealed class CallbackException : Exception
        {
        }
    }
}
