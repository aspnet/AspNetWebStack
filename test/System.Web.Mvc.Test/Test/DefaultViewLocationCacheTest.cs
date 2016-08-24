// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    public class DefaultViewLocationCacheTest
    {
        [Fact]
        public void TimeSpanProperty()
        {
            // Arrange
            TimeSpan timeSpan = new TimeSpan(0, 20, 0);
            DefaultViewLocationCache viewCache = new DefaultViewLocationCache(timeSpan);

            // Assert
            Assert.Equal(timeSpan.Ticks, viewCache.TimeSpan.Ticks);
        }

        [Fact]
        public void ConstructorAssignsDefaultTimeSpan()
        {
            // Arrange
            DefaultViewLocationCache viewLocationCache = new DefaultViewLocationCache();
            TimeSpan timeSpan = new TimeSpan(0, 15, 0);

            // Assert
            Assert.Equal(timeSpan.Ticks, viewLocationCache.TimeSpan.Ticks);
        }

        [Fact]
        public void ConstructorWithNegativeTimeSpanThrows()
        {
            // Assert
            Assert.Throws<InvalidOperationException>(
                delegate { new DefaultViewLocationCache(new TimeSpan(-1, 0, 0)); },
                "The number of ticks for the TimeSpan value must be greater than or equal to 0.");
        }

        [Fact]
        public void GetViewLocationThrowsWithNullHttpContext()
        {
            // Arrange
            DefaultViewLocationCache viewLocationCache = new DefaultViewLocationCache();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                delegate { string viewLocation = viewLocationCache.GetViewLocation(null /* httpContext */, "foo"); },
                "httpContext");
        }

        [Fact]
        public void InsertViewLocationThrowsWithNullHttpContext()
        {
            // Arrange
            DefaultViewLocationCache viewLocationCache = new DefaultViewLocationCache();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                delegate { viewLocationCache.InsertViewLocation(null /* httpContext */, "foo", "fooPath"); },
                "httpContext");
        }

        [Fact]
        public void NullViewLocationCacheReturnsNullLocations()
        {
            // Act
            DefaultViewLocationCache.Null.InsertViewLocation(null /* httpContext */, "foo", "fooPath");

            // Assert
            Assert.Null(DefaultViewLocationCache.Null.GetViewLocation(null /* httpContext */, "foo"));
        }
    }
}
