// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.TestUtil;
using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    public class ViewResultBaseTest
    {
        [Fact]
        public void ExecuteResultWithNullControllerContextThrows()
        {
            // Arrange
            ViewResultBaseHelper result = new ViewResultBaseHelper();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => result.ExecuteResult(null),
                "context");
        }

        [Fact]
        public void TempDataProperty()
        {
            // Arrange
            TempDataDictionary newDict = new TempDataDictionary();
            ViewResultBaseHelper result = new ViewResultBaseHelper();

            // Act & Assert
            MemberHelper.TestPropertyWithDefaultInstance(result, "TempData", newDict);
        }

        [Fact]
        public void ViewDataProperty()
        {
            // Arrange
            ViewDataDictionary newDict = new ViewDataDictionary();
            ViewResultBaseHelper result = new ViewResultBaseHelper();

            // Act & Assert
            MemberHelper.TestPropertyWithDefaultInstance(result, "ViewData", newDict);
        }

        [Fact]
        public void ViewEngineCollectionProperty()
        {
            // Arrange
            ViewEngineCollection viewEngineCollection = new ViewEngineCollection();
            ViewResultBaseHelper result = new ViewResultBaseHelper();

            // Act & Assert
            MemberHelper.TestPropertyWithDefaultInstance(result, "ViewEngineCollection", viewEngineCollection);
        }

        [Fact]
        public void ViewNameProperty()
        {
            // Arrange
            ViewResultBaseHelper result = new ViewResultBaseHelper();

            // Act & Assert
            MemberHelper.TestStringProperty(result, "ViewName", String.Empty);
        }

        public class ViewResultBaseHelper : ViewResultBase
        {
            protected override ViewEngineResult FindView(ControllerContext context)
            {
                throw new NotImplementedException();
            }
        }
    }
}
