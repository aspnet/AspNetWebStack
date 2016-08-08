// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.IO;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Html.Test
{
    public class MvcFormTest
    {
        [Fact]
        public void ConstructorWithNullViewContextThrows()
        {
            Assert.ThrowsArgumentNull(
                delegate { new MvcForm((ViewContext)null); },
                "viewContext");
        }

        [Fact]
        public void DisposeRendersCloseFormTag()
        {
            // Arrange
            StringWriter writer = new StringWriter();
            ViewContext viewContext = GetViewContext(writer);

            MvcForm form = new MvcForm(viewContext);

            // Act
            form.Dispose();

            // Assert
            Assert.Equal("</form>", writer.ToString());
        }

        [Fact]
        public void EndFormRendersCloseFormTag()
        {
            // Arrange
            StringWriter writer = new StringWriter();
            ViewContext viewContext = GetViewContext(writer);

            MvcForm form = new MvcForm(viewContext);

            // Act
            form.EndForm();

            // Assert
            Assert.Equal("</form>", writer.ToString());
        }

        [Fact]
        public void DisposeTwiceRendersCloseFormTagOnce()
        {
            // Arrange
            StringWriter writer = new StringWriter();
            ViewContext viewContext = GetViewContext(writer);

            MvcForm form = new MvcForm(viewContext);

            // Act
            form.Dispose();
            form.Dispose();

            // Assert
            Assert.Equal("</form>", writer.ToString());
        }

        [Fact]
        public void EndFormTwiceRendersCloseFormTagOnce()
        {
            // Arrange
            StringWriter writer = new StringWriter();
            ViewContext viewContext = GetViewContext(writer);

            MvcForm form = new MvcForm(viewContext);

            // Act
            form.EndForm();
            form.EndForm();

            // Assert
            Assert.Equal("</form>", writer.ToString());
        }

        private static ViewContext GetViewContext(TextWriter writer)
        {
            Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();
            mockHttpContext.Setup(o => o.Items).Returns(new Hashtable());

            return new ViewContext()
            {
                HttpContext = mockHttpContext.Object,
                Writer = writer
            };
        }
    }
}
