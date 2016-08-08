// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.TestCommon;

namespace System.Web.Helpers.Test
{
    public class WebCacheTest
    {
        [Fact]
        public void GetReturnsExpectedValueTest()
        {
            string key = DateTime.UtcNow.Ticks.ToString() + "_GetTest";
            List<string> expected = new List<string>();
            WebCache.Set(key, expected);

            var actual = WebCache.Get(key);

            Assert.Equal(expected, actual);
            Assert.Equal(0, actual.Count);
        }

        [Fact]
        public void RemoveRemovesRightValueTest()
        {
            string key = DateTime.UtcNow.Ticks.ToString() + "_RemoveTest";
            List<string> expected = new List<string>();
            WebCache.Set(key, expected);

            var actual = WebCache.Remove(key);

            Assert.Equal(expected, actual);
            Assert.Equal(0, actual.Count);
        }

        [Fact]
        public void RemoveRemovesValueFromCacheTest()
        {
            string key = DateTime.UtcNow.Ticks.ToString() + "_RemoveTest2";
            List<string> expected = new List<string>();
            WebCache.Set(key, expected);

            var removed = WebCache.Remove(key);

            Assert.Null(WebCache.Get(key));
        }

        [Fact]
        public void SetWithAbsoluteExpirationDoesNotThrow()
        {
            string key = DateTime.UtcNow.Ticks.ToString() + "SetWithAbsoluteExpirationDoesNotThrow_SetTest";
            object expected = new object();
            int minutesToCache = 10;
            bool slidingExpiration = false;
            WebCache.Set(key, expected, minutesToCache, slidingExpiration);
            object actual = WebCache.Get(key);
            Assert.True(expected == actual);
        }

        [Fact]
        public void CanSetWithSlidingExpiration()
        {
            string key = DateTime.UtcNow.Ticks.ToString() + "_CanSetWithSlidingExpiration_SetTest";
            object expected = new object();

            WebCache.Set(key, expected, slidingExpiration: true);
            object actual = WebCache.Get(key);
            Assert.True(expected == actual);
        }

        [Fact]
        public void SetWithSlidingExpirationForNegativeTime()
        {
            string key = DateTime.UtcNow.Ticks.ToString() + "_SetWithSlidingExpirationForNegativeTime_SetTest";
            object expected = new object();
            Assert.ThrowsArgumentGreaterThan(() => WebCache.Set(key, expected, -1), "minutesToCache", "0");
        }

        [Fact]
        public void SetWithSlidingExpirationForZeroTime()
        {
            string key = DateTime.UtcNow.Ticks.ToString() + "_SetWithSlidingExpirationForZeroTime_SetTest";
            object expected = new object();
            Assert.ThrowsArgumentGreaterThan(() => WebCache.Set(key, expected, 0), "minutesToCache", "0");
        }

        [Fact]
        public void SetWithSlidingExpirationForYear()
        {
            string key = DateTime.UtcNow.Ticks.ToString() + "_SetWithSlidingExpirationForYear_SetTest";
            object expected = new object();

            WebCache.Set(key, expected, 365 * 24 * 60, true);
            object actual = WebCache.Get(key);
            Assert.True(expected == actual);
        }

        [Fact]
        public void SetWithSlidingExpirationForMoreThanYear()
        {
            string key = DateTime.UtcNow.Ticks.ToString() + "_SetWithSlidingExpirationForMoreThanYear_SetTest";
            object expected = new object();
            Assert.ThrowsArgumentLessThanOrEqualTo(() => WebCache.Set(key, expected, (365 * 24 * 60) + 1, true), "minutesToCache", (365 * 24 * 60).ToString());
        }

        [Fact]
        public void SetWithAbsoluteExpirationForMoreThanYear()
        {
            string key = DateTime.UtcNow.Ticks.ToString() + "_SetWithAbsoluteExpirationForMoreThanYear_SetTest";
            object expected = new object();

            WebCache.Set(key, expected, 365 * 24 * 60, true);
            object actual = WebCache.Get(key);
            Assert.True(expected == actual);
        }
    }
}
