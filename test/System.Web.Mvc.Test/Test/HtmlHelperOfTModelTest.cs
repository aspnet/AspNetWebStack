// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class HtmlHelperOfTModelTest
    {
        [Fact]
        public void StronglyTypedViewBagAndStronglyTypedViewDataStayInSync()
        {
            // Arrange
            Mock<IViewDataContainer> viewDataContainer = new Mock<IViewDataContainer>();
            ViewDataDictionary viewDataDictionary = new ViewDataDictionary() { { "A", 1 } };
            viewDataContainer.Setup(container => container.ViewData).Returns(viewDataDictionary);

            // Act
            HtmlHelper<object> htmlHelper = new HtmlHelper<object>(new Mock<ViewContext>().Object, viewDataContainer.Object);
            htmlHelper.ViewData["B"] = 2;
            htmlHelper.ViewBag.C = 3;

            // Assert

            // Original ViewData should not be modified by redfined ViewData and ViewBag
            HtmlHelper baseHelper = (HtmlHelper)htmlHelper;
            Assert.Single(baseHelper.ViewData.Keys);
            Assert.Equal(1, baseHelper.ViewData["A"]);
            Assert.Equal(1, baseHelper.ViewBag.A);

            // Redefined ViewData and ViewBag should be in sync
            Assert.Equal(3, htmlHelper.ViewData.Keys.Count);

            Assert.Equal(1, htmlHelper.ViewData["A"]);
            Assert.Equal(2, htmlHelper.ViewData["B"]);
            Assert.Equal(3, htmlHelper.ViewData["C"]);

            Assert.Equal(1, htmlHelper.ViewBag.A);
            Assert.Equal(2, htmlHelper.ViewBag.B);
            Assert.Equal(3, htmlHelper.ViewBag.C);
        }
    }
}
