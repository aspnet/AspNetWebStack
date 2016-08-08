// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.WebPages.Test
{
    public class WebPageContextStackTest
    {
        [Fact]
        public void GetCurrentContextReturnsNullWhenStackIsEmpty()
        {
            // Arrange
            var httpContext = GetHttpContext();

            // Act
            var template = TemplateStack.GetCurrentTemplate(httpContext);

            // Assert
            Assert.Equal(1, httpContext.Items.Count);
            Assert.Null(template);
        }

        [Fact]
        public void GetCurrentContextReturnsCurrentContext()
        {
            // Arrange
            var template = GetTemplateFile();
            var httpContext = GetHttpContext();

            // Act
            TemplateStack.Push(httpContext, template);

            // Assert
            var currentTemplate = TemplateStack.GetCurrentTemplate(httpContext);
            Assert.Equal(template, currentTemplate);
        }

        [Fact]
        public void GetCurrentContextReturnsLastPushedContext()
        {
            // Arrange
            var httpContext = GetHttpContext();
            var template1 = GetTemplateFile("page1");
            var template2 = GetTemplateFile("page2");

            // Act
            TemplateStack.Push(httpContext, template1);
            TemplateStack.Push(httpContext, template2);

            // Assert
            var currentTemplate = TemplateStack.GetCurrentTemplate(httpContext);
            Assert.Equal(template2, currentTemplate);
        }

        [Fact]
        public void GetCurrentContextReturnsNullAfterPop()
        {
            // Arrange
            var httpContext = GetHttpContext();
            var template = GetTemplateFile();

            // Act
            TemplateStack.Push(httpContext, template);
            TemplateStack.Pop(httpContext);

            // Assert
            Assert.Null(TemplateStack.GetCurrentTemplate(httpContext));
        }

        private static HttpContextBase GetHttpContext()
        {
            Mock<HttpContextBase> context = new Mock<HttpContextBase>();
            context.Setup(c => c.Items).Returns(new Dictionary<object, object>());

            return context.Object;
        }

        private static ITemplateFile GetTemplateFile(string path = null)
        {
            Mock<ITemplateFile> templateFile = new Mock<ITemplateFile>();
            templateFile.Setup(f => f.TemplateInfo).Returns(new TemplateFileInfo(path));

            return templateFile.Object;
        }
    }
}
