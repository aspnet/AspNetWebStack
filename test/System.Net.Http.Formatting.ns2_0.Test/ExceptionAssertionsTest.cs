// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Runtime.CompilerServices;
using Xunit;
using Xunit.Sdk;
using TestCommonAssert = Microsoft.TestCommon.Assert;

namespace System.Net.Http
{
    public class ExceptionAssertionsTest
    {
        [Fact]
        public void Throws_NoException_ReportsFilteredAssertionFailure()
        {
            var exception = Assert.IsAssignableFrom<XunitException>(
                Record.Exception(() => TestCommonAssert.Throws<InvalidOperationException>(() => { })));

            Assert.IsAssignableFrom<IAssertionException>(exception);
            Assert.Equal(ThrowsException.ForNoException(typeof(InvalidOperationException)).Message, exception.Message);
            Assert.Null(exception.InnerException);
            Assert.NotNull(exception.StackTrace);
            Assert.Contains(nameof(Throws_NoException_ReportsFilteredAssertionFailure), exception.StackTrace);
            Assert.DoesNotContain("at Microsoft.TestCommon.Assert.", exception.StackTrace);
        }

        [Fact]
        public void Throws_WrongExceptionType_PreservesInnerExceptionAndOriginalStackTrace()
        {
            var actual = new InvalidOperationException("Actual exception message");
            var exception = Assert.IsAssignableFrom<XunitException>(
                Record.Exception(() => TestCommonAssert.Throws<ArgumentException>(() => ThrowException(actual))));

            Assert.IsAssignableFrom<IAssertionException>(exception);
            Assert.Equal(ThrowsException.ForIncorrectExceptionType(typeof(ArgumentException), actual).Message, exception.Message);
            Assert.Same(actual, exception.InnerException);
            Assert.NotNull(actual.StackTrace);
            Assert.Equal(Microsoft.TestCommon.ExceptionUtility.FilterStackTrace(actual.StackTrace), exception.StackTrace);
            Assert.Contains(nameof(ThrowException), exception.StackTrace);
            Assert.DoesNotContain("at Microsoft.TestCommon.Assert.", exception.StackTrace);
        }

        [Fact]
        public void Throws_ExpectedExceptionType_ReturnsOriginalException()
        {
            var expected = new InvalidOperationException();

            var actual = TestCommonAssert.Throws<InvalidOperationException>(() => ThrowException(expected));

            Assert.Same(expected, actual);
        }

        [Fact]
        public void Throws_DerivedExceptionType_ReportsAssertionFailure()
        {
            var actual = new ArgumentNullException("parameter");
            var exception = Assert.IsAssignableFrom<XunitException>(
                Record.Exception(() => TestCommonAssert.Throws<ArgumentException>(() => ThrowException(actual))));

            Assert.Equal(ThrowsException.ForIncorrectExceptionType(typeof(ArgumentException), actual).Message, exception.Message);
            Assert.Same(actual, exception.InnerException);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowException(Exception exception)
        {
            throw exception;
        }
    }
}
