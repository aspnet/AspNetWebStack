// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.ExceptionHandling;
using Microsoft.TestCommon;

namespace System.Web.Http.Owin
{
    public class OwinExceptionCatchBlocksTests
    {
        [Fact]
        public void HttpMessageHandlerAdapterBufferContent_IsSpecifiedValue()
        {
            // Act
            ExceptionContextCatchBlock catchBlock = OwinExceptionCatchBlocks.HttpMessageHandlerAdapterBufferContent;

            // Assert
            ExceptionContextCatchBlock expected =
                new ExceptionContextCatchBlock("HttpMessageHandlerAdapter.BufferContent", isTopLevel: true,
                    callsHandler: true);
            AssertEqual(expected, catchBlock);
        }

        [Fact]
        public void HttpMessageHandlerAdapterBufferContent_IsSameInstance()
        {
            // Arrange
            ExceptionContextCatchBlock first = OwinExceptionCatchBlocks.HttpMessageHandlerAdapterBufferContent;

            // Act
            ExceptionContextCatchBlock second = OwinExceptionCatchBlocks.HttpMessageHandlerAdapterBufferContent;

            // Assert
            Assert.Same(first, second);
        }

        [Fact]
        public void HttpMessageHandlerAdapterBufferError_IsSpecifiedValue()
        {
            // Act
            ExceptionContextCatchBlock catchBlock = OwinExceptionCatchBlocks.HttpMessageHandlerAdapterBufferError;

            // Assert
            ExceptionContextCatchBlock expected =
                new ExceptionContextCatchBlock("HttpMessageHandlerAdapter.BufferError", isTopLevel: true,
                    callsHandler: false);
            AssertEqual(expected, catchBlock);
        }

        [Fact]
        public void HttpMessageHandlerAdapterBufferError_IsSameInstance()
        {
            // Arrange
            ExceptionContextCatchBlock first = OwinExceptionCatchBlocks.HttpMessageHandlerAdapterBufferError;

            // Act
            ExceptionContextCatchBlock second = OwinExceptionCatchBlocks.HttpMessageHandlerAdapterBufferError;

            // Assert
            Assert.Same(first, second);
        }

        [Fact]
        public void HttpMessageHandlerAdapterComputeContentLength_IsSpecifiedValue()
        {
            // Act
            ExceptionContextCatchBlock catchBlock =
                OwinExceptionCatchBlocks.HttpMessageHandlerAdapterComputeContentLength;

            // Assert
            ExceptionContextCatchBlock expected =
                new ExceptionContextCatchBlock("HttpMessageHandlerAdapter.ComputeContentLength", isTopLevel: true,
                    callsHandler: false);
            AssertEqual(expected, catchBlock);
        }

        [Fact]
        public void HttpMessageHandlerAdapterComputeContentLength_IsSameInstance()
        {
            // Arrange
            ExceptionContextCatchBlock first = OwinExceptionCatchBlocks.HttpMessageHandlerAdapterComputeContentLength;

            // Act
            ExceptionContextCatchBlock second = OwinExceptionCatchBlocks.HttpMessageHandlerAdapterComputeContentLength;

            // Assert
            Assert.Same(first, second);
        }

        [Fact]
        public void HttpMessageHandlerAdapterStreamContent_IsSpecifiedValue()
        {
            // Act
            ExceptionContextCatchBlock catchBlock = OwinExceptionCatchBlocks.HttpMessageHandlerAdapterStreamContent;

            // Assert
            ExceptionContextCatchBlock expected =
                new ExceptionContextCatchBlock("HttpMessageHandlerAdapter.StreamContent", isTopLevel: true,
                    callsHandler: false);
            AssertEqual(expected, catchBlock);
        }

        [Fact]
        public void HttpMessageHandlerAdapterStreamContent_IsSameInstance()
        {
            // Arrange
            ExceptionContextCatchBlock first = OwinExceptionCatchBlocks.HttpMessageHandlerAdapterStreamContent;

            // Act
            ExceptionContextCatchBlock second = OwinExceptionCatchBlocks.HttpMessageHandlerAdapterStreamContent;

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
