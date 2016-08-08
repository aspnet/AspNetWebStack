// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Http.ExceptionHandling;
using Microsoft.TestCommon;

namespace System.Web.Http.WebHost
{
    public class WebHostExceptionCatchBlocksTests
    {
        [Fact]
        public void HttpControllerHandlerBufferContent_IsSpecifiedValue()
        {
            // Act
            ExceptionContextCatchBlock catchBlock = WebHostExceptionCatchBlocks.HttpControllerHandlerBufferContent;

            // Assert
            ExceptionContextCatchBlock expected =
                new ExceptionContextCatchBlock("HttpControllerHandler.BufferContent", isTopLevel: true,
                    callsHandler: true);
            AssertEqual(expected, catchBlock);
        }

        [Fact]
        public void HttpControllerHandlerBufferContent_IsSameInstance()
        {
            // Arrange
            ExceptionContextCatchBlock first = WebHostExceptionCatchBlocks.HttpControllerHandlerBufferContent;

            // Act
            ExceptionContextCatchBlock second = WebHostExceptionCatchBlocks.HttpControllerHandlerBufferContent;

            // Assert
            Assert.Same(first, second);
        }

        [Fact]
        public void HttpControllerHandlerBufferError_IsSpecifiedValue()
        {
            // Act
            ExceptionContextCatchBlock catchBlock = WebHostExceptionCatchBlocks.HttpControllerHandlerBufferError;

            // Assert
            ExceptionContextCatchBlock expected =
                new ExceptionContextCatchBlock("HttpControllerHandler.BufferError", isTopLevel: true,
                    callsHandler: false);
            AssertEqual(expected, catchBlock);
        }

        [Fact]
        public void HttpControllerHandlerBufferError_IsSameInstance()
        {
            // Arrange
            ExceptionContextCatchBlock first = WebHostExceptionCatchBlocks.HttpControllerHandlerBufferError;

            // Act
            ExceptionContextCatchBlock second = WebHostExceptionCatchBlocks.HttpControllerHandlerBufferError;

            // Assert
            Assert.Same(first, second);
        }

        [Fact]
        public void HttpControllerHandlerComputeContentLength_IsSpecifiedValue()
        {
            // Act
            ExceptionContextCatchBlock catchBlock = WebHostExceptionCatchBlocks.HttpControllerHandlerComputeContentLength;

            // Assert
            ExceptionContextCatchBlock expected =
                new ExceptionContextCatchBlock("HttpControllerHandler.ComputeContentLength", isTopLevel: true,
                    callsHandler: false);
            AssertEqual(expected, catchBlock);
        }

        [Fact]
        public void HttpControllerHandlerComputeContentLength_IsSameInstance()
        {
            // Arrange
            ExceptionContextCatchBlock first = WebHostExceptionCatchBlocks.HttpControllerHandlerComputeContentLength;

            // Act
            ExceptionContextCatchBlock second = WebHostExceptionCatchBlocks.HttpControllerHandlerComputeContentLength;

            // Assert
            Assert.Same(first, second);
        }

        [Fact]
        public void HttpControllerHandlerStreamContent_IsSpecifiedValue()
        {
            // Act
            ExceptionContextCatchBlock catchBlock = WebHostExceptionCatchBlocks.HttpControllerHandlerStreamContent;

            // Assert
            ExceptionContextCatchBlock expected =
                new ExceptionContextCatchBlock("HttpControllerHandler.StreamContent", isTopLevel: true,
                    callsHandler: false);
            AssertEqual(expected, catchBlock);
        }

        [Fact]
        public void HttpControllerHandlerStreamContent_IsSameInstance()
        {
            // Arrange
            ExceptionContextCatchBlock first = WebHostExceptionCatchBlocks.HttpControllerHandlerStreamContent;

            // Act
            ExceptionContextCatchBlock second = WebHostExceptionCatchBlocks.HttpControllerHandlerStreamContent;

            // Assert
            Assert.Same(first, second);
        }

        [Fact]
        public void HttpWebRoute_IsSpecifiedValue()
        {
            // Act
            ExceptionContextCatchBlock catchBlock = WebHostExceptionCatchBlocks.HttpWebRoute;

            // Assert
            ExceptionContextCatchBlock expected =
                new ExceptionContextCatchBlock("HttpWebRoute", isTopLevel: true, callsHandler: true);
            AssertEqual(expected, catchBlock);
        }

        [Fact]
        public void HttpWebRoute_IsSameInstance()
        {
            // Arrange
            ExceptionContextCatchBlock first = WebHostExceptionCatchBlocks.HttpWebRoute;

            // Act
            ExceptionContextCatchBlock second = WebHostExceptionCatchBlocks.HttpWebRoute;

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
