// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    [Xunit.Collection("Uses ScopeStorage or ViewEngines.Engines")]
    public class ViewEnginesTest
    {
        [Fact]
        public void EnginesProperty()
        {
            // Act
            ViewEngineCollection collection = ViewEngines.Engines;

            // Assert
            Assert.Equal(2, collection.Count);
            Assert.IsType<WebFormViewEngine>(collection[0]);
            Assert.IsType<RazorViewEngine>(collection[1]);
        }
    }
}
