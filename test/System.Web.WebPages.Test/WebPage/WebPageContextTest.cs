// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.WebPages.Test
{
    public class WebPageContextTest
    {
        [Fact]
        public void CreateNestedPageContextCopiesPropertiesFromParentPageContext()
        {
            // Arrange
            var httpContext = new Mock<HttpContextBase>();
            var pageDataDictionary = new Dictionary<object, dynamic>();
            var model = new { Hello = "World" };
            Action<TextWriter> bodyAction = writer => { };
            var sectionWritersStack = new Stack<Dictionary<string, SectionWriter>>();
            var basePageContext = new WebPageContext(httpContext.Object, null, null) { BodyAction = bodyAction, SectionWritersStack = sectionWritersStack };

            // Act
            var subPageContext = WebPageContext.CreateNestedPageContext(basePageContext, pageDataDictionary, model, isLayoutPage: false);

            // Assert
            Assert.Equal(basePageContext.HttpContext, subPageContext.HttpContext);
            Assert.Equal(basePageContext.OutputStack, subPageContext.OutputStack);
            Assert.Equal(basePageContext.Validation, subPageContext.Validation);
            Assert.Equal(basePageContext.ModelState, subPageContext.ModelState);
            Assert.Equal(pageDataDictionary, subPageContext.PageData);
            Assert.Equal(model, subPageContext.Model);
            Assert.Null(subPageContext.BodyAction);
        }

        [Fact]
        public void CreateNestedPageCopiesBodyActionAndSectionWritersWithOtherPropertiesFromParentPageContext()
        {
            // Arrange
            var httpContext = new Mock<HttpContextBase>();
            var pageDataDictionary = new Dictionary<object, dynamic>();
            var model = new { Hello = "World" };
            Action<TextWriter> bodyAction = writer => { };
            var sectionWritersStack = new Stack<Dictionary<string, SectionWriter>>();
            var basePageContext = new WebPageContext(httpContext.Object, null, null) { BodyAction = bodyAction, SectionWritersStack = sectionWritersStack };

            // Act
            var subPageContext = WebPageContext.CreateNestedPageContext(basePageContext, pageDataDictionary, model, isLayoutPage: true);

            // Assert
            Assert.Equal(basePageContext.HttpContext, subPageContext.HttpContext);
            Assert.Equal(basePageContext.OutputStack, subPageContext.OutputStack);
            Assert.Equal(basePageContext.Validation, subPageContext.Validation);
            Assert.Equal(basePageContext.ModelState, subPageContext.ModelState);
            Assert.Equal(pageDataDictionary, subPageContext.PageData);
            Assert.Equal(model, subPageContext.Model);
            Assert.Equal(sectionWritersStack, subPageContext.SectionWritersStack);
            Assert.Equal(bodyAction, subPageContext.BodyAction);
        }
    }
}
