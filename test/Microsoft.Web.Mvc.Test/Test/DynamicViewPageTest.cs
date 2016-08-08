// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace Microsoft.Web.Mvc.Test
{
    public class DynamicViewPageTest
    {
        // DynamicViewPage

        [Fact]
        public void AnonymousObjectsAreWrapped()
        {
            // Arrange
            DynamicViewPage page = new DynamicViewPage();
            page.ViewData.Model = new { foo = "Hello world!" };

            // Act & Assert
            Assert.Equal("Microsoft.Web.Mvc.DynamicReflectionObject", page.Model.GetType().FullName);
        }

        [Fact]
        public void NonAnonymousObjectsAreNotWrapped()
        {
            // Arrange
            DynamicViewPage page = new DynamicViewPage();
            page.ViewData.Model = "Hello world!";

            // Act & Assert
            Assert.Equal(typeof(string), page.Model.GetType());
        }

        [Fact]
        public void ViewDataDictionaryIsWrapped()
        {
            // Arrange
            DynamicViewPage page = new DynamicViewPage();

            // Act & Assert
            Assert.Equal("Microsoft.Web.Mvc.DynamicViewDataDictionary", page.ViewData.GetType().FullName);
        }

        // DynamicViewPage<T>

        [Fact]
        public void Generic_ViewDataDictionaryIsWrapped()
        {
            // Arrange
            DynamicViewPage<object> page = new DynamicViewPage<object>();

            // Act & Assert
            Assert.Equal("Microsoft.Web.Mvc.DynamicViewDataDictionary", page.ViewData.GetType().FullName);
        }
    }
}
