// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class AjaxHelperOfTModelTest
    {
        [Fact]
        public void ViewBagAndViewDataStayInSync()
        {
            // Arrange
            Mock<IViewDataContainer> viewDataContainer = new Mock<IViewDataContainer>();
            ViewDataDictionary viewDataDictionary = new ViewDataDictionary() { { "A", 1 } };
            viewDataContainer.Setup(container => container.ViewData).Returns(viewDataDictionary);

            // Act
            AjaxHelper<object> ajaxHelper = new AjaxHelper<object>(new Mock<ViewContext>().Object, viewDataContainer.Object);
            ajaxHelper.ViewData["B"] = 2;
            ajaxHelper.ViewBag.C = 3;

            // Assert

            // Original ViewData should not be modified by redfined ViewData and ViewBag
            AjaxHelper nonGenericAjaxHelper = ajaxHelper;
            Assert.Single(nonGenericAjaxHelper.ViewData.Keys);
            Assert.Equal(1, nonGenericAjaxHelper.ViewData["A"]);
            Assert.Equal(1, nonGenericAjaxHelper.ViewBag.A);

            // Redefined ViewData and ViewBag should be in sync
            Assert.Equal(3, ajaxHelper.ViewData.Keys.Count);

            Assert.Equal(1, ajaxHelper.ViewData["A"]);
            Assert.Equal(2, ajaxHelper.ViewData["B"]);
            Assert.Equal(3, ajaxHelper.ViewData["C"]);

            Assert.Equal(1, ajaxHelper.ViewBag.A);
            Assert.Equal(2, ajaxHelper.ViewBag.B);
            Assert.Equal(3, ajaxHelper.ViewBag.C);
        }
    }
}
