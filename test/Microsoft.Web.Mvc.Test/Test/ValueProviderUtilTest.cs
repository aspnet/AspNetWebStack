// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace Microsoft.Web.Mvc.Test
{
    public class ValueProviderUtilTest
    {
        [Fact]
        public void IsPrefixMatch_Misses()
        {
            // Arrange
            var tests = new[]
            {
                new { Prefix = "Prefix", TestString = (string)null, Reason = "Null test string shouldn't match anything." },
                new { Prefix = "Foo", TestString = "NotFoo", Reason = "Prefix 'foo' doesn't match 'notfoo'." },
                new { Prefix = "Foo", TestString = "FooBar", Reason = "Prefix 'foo' was not followed by a delimiter in the test string." }
            };

            // Act & assert
            foreach (var test in tests)
            {
                bool retVal = ValueProviderUtil.IsPrefixMatch(test.Prefix, test.TestString);
                Assert.False(retVal, test.Reason);
            }
        }

        [Fact]
        public void IsPrefixMatch_Hits()
        {
            // Arrange
            var tests = new[]
            {
                new { Prefix = "", TestString = "SomeTestString", Reason = "Empty prefix should match any non-null test string." },
                new { Prefix = "SomeString", TestString = "SomeString", Reason = "This was an exact match." },
                new { Prefix = "Foo", TestString = "foo.bar", Reason = "Prefix 'foo' matched." },
                new { Prefix = "Foo", TestString = "foo[bar]", Reason = "Prefix 'foo' matched." },
            };

            // Act & assert
            foreach (var test in tests)
            {
                bool retVal = ValueProviderUtil.IsPrefixMatch(test.Prefix, test.TestString);
                Assert.True(retVal, test.Reason);
            }
        }
    }
}
