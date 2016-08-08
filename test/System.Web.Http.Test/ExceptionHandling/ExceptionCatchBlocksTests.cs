// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Http.ExceptionHandling
{
    public class ExceptionCatchBlocksTests
    {
        [Fact]
        public void HttpBatchHandler_IsSpecifiedValue()
        {
            // Act
            ExceptionContextCatchBlock catchBlock = ExceptionCatchBlocks.HttpBatchHandler;

            // Assert
            ExceptionContextCatchBlock expected =
                new ExceptionContextCatchBlock("HttpBatchHandler", isTopLevel: false, callsHandler: true);
            AssertEqual(expected, catchBlock);
        }

        [Fact]
        public void HttpBatchHandler_IsSameInstance()
        {
            // Arrange
            ExceptionContextCatchBlock first = ExceptionCatchBlocks.HttpBatchHandler;

            // Act
            ExceptionContextCatchBlock second = ExceptionCatchBlocks.HttpBatchHandler;

            // Assert
            Assert.Same(first, second);
        }

        [Fact]
        public void HttpControllerDispatcher_IsSpecifiedValue()
        {
            // Act
            ExceptionContextCatchBlock catchBlock = ExceptionCatchBlocks.HttpControllerDispatcher;

            // Assert
            ExceptionContextCatchBlock expected =
                new ExceptionContextCatchBlock("HttpControllerDispatcher", isTopLevel: false, callsHandler: true);
            AssertEqual(expected, catchBlock);
        }

        [Fact]
        public void HttpControllerDispatcher_IsSameInstance()
        {
            // Arrange
            ExceptionContextCatchBlock first = ExceptionCatchBlocks.HttpControllerDispatcher;

            // Act
            ExceptionContextCatchBlock second = ExceptionCatchBlocks.HttpControllerDispatcher;

            // Assert
            Assert.Same(first, second);
        }

        [Fact]
        public void HttpServer_IsSpecifiedValue()
        {
            // Act
            ExceptionContextCatchBlock catchBlock = ExceptionCatchBlocks.HttpServer;

            // Assert
            ExceptionContextCatchBlock expected =
                new ExceptionContextCatchBlock("HttpServer", isTopLevel: true, callsHandler: true);
            AssertEqual(expected, catchBlock);
        }

        [Fact]
        public void HttpServer_IsSameInstance()
        {
            // Arrange
            ExceptionContextCatchBlock first = ExceptionCatchBlocks.HttpServer;

            // Act
            ExceptionContextCatchBlock second = ExceptionCatchBlocks.HttpServer;

            // Assert
            Assert.Same(first, second);
        }

        [Fact]
        public void IExceptionFilter_IsSpecifiedValue()
        {
            // Act
            ExceptionContextCatchBlock catchBlock = ExceptionCatchBlocks.IExceptionFilter;

            // Assert
            ExceptionContextCatchBlock expected =
                new ExceptionContextCatchBlock("IExceptionFilter", isTopLevel: false, callsHandler: true);
            AssertEqual(expected, catchBlock);
        }

        [Fact]
        public void IExceptionFilter_IsSameInstance()
        {
            // Arrange
            ExceptionContextCatchBlock first = ExceptionCatchBlocks.IExceptionFilter;

            // Act
            ExceptionContextCatchBlock second = ExceptionCatchBlocks.IExceptionFilter;

            // Assert
            Assert.Same(first, second);
        }

        private static void AssertEqual(ExceptionContextCatchBlock expected, ExceptionContextCatchBlock actual)
        {
            if (expected == null)
            {
                Assert.Null(actual);
                return;
            }

            Assert.NotNull(actual);
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.IsTopLevel, actual.IsTopLevel);
            Assert.Equal(expected.CallsHandler, actual.CallsHandler);
        }
    }
}
