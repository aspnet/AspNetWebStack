// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http.Mocks;
using Microsoft.TestCommon;

namespace System.Net.Http
{
    public class HttpClientFactoryTest
    {
        public static TheoryDataSet<IEnumerable<DelegatingHandler>> InvalidPipelines
        {
            get
            {
                return new TheoryDataSet<IEnumerable<DelegatingHandler>>
                {
                    new List<DelegatingHandler>()
                    {
                        new MockDelegatingHandler(),
                        new MockDelegatingHandler(new HttpClientHandler())
                    },
                    new List<DelegatingHandler>()
                    {
                        new MockDelegatingHandler(new HttpClientHandler()),
                        new MockDelegatingHandler(),
                    },
                    new List<DelegatingHandler>()
                    {
                        null,
                        new MockDelegatingHandler(),
                    },
                    new List<DelegatingHandler>()
                    {
                        new MockDelegatingHandler(),
                        null,
                    },
                };
            }
        }

        [Fact]
        public void Create1_SetsInnerHandler()
        {
            // Arrange
            MockDelegatingHandler handler = new MockDelegatingHandler();

            // Act
            HttpClient client = HttpClientFactory.Create(handler);

            // Assert
            Assert.IsType<HttpClientHandler>(handler.InnerHandler);
        }

        [Fact]
        public void Create2_ThrowsOnNullInnerHandler()
        {
            Assert.ThrowsArgumentNull(() => HttpClientFactory.Create(null, new DelegatingHandler[0]), "innerHandler");
        }

        [Fact]
        public void Create2_SetsInnerHandler()
        {
            // Arrange
            MockDelegatingHandler handler = new MockDelegatingHandler();
            HttpClientHandler innerHandler = new HttpClientHandler();

            // Act
            HttpClient client = HttpClientFactory.Create(innerHandler, handler);

            // Assert
            Assert.IsType<HttpClientHandler>(handler.InnerHandler);
        }

        [Theory]
        [PropertyData("InvalidPipelines")]
        public void CreatePipeline_ThrowsOnInvalidPipeline(IEnumerable<DelegatingHandler> handlers)
        {
            Assert.ThrowsArgument(() => HttpClientFactory.CreatePipeline(new HttpClientHandler(), handlers), "handlers");
        }

        [Fact]
        public void CreatePipeline_ReturnsInnerHandler()
        {
            // Arrange
            HttpClientHandler innerHandler = new HttpClientHandler();

            // Act
            HttpMessageHandler handler = HttpClientFactory.CreatePipeline(innerHandler, null);

            // Assert
            Assert.Same(innerHandler, handler);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(4)]
        [InlineData(16)]
        public void CreatePipeline_WiresUpHandlers(int maxHandlerCount)
        {
            // Arrange
            List<DelegatingHandler> handlers = new List<DelegatingHandler>();

            for (int handlerCount = 0; handlerCount < maxHandlerCount; handlerCount++)
            {
                handlers.Add(new MockDelegatingHandler());
            }

            HttpClientHandler innerHandler = new HttpClientHandler();

            // Act
            DelegatingHandler pipeline = HttpClientFactory.CreatePipeline(innerHandler, handlers) as DelegatingHandler;

            // Assert
            for (int index = 0; index < handlers.Count - 1; index++)
            {
                Assert.Same(handlers[index], pipeline);
                pipeline = pipeline.InnerHandler as DelegatingHandler;
            }
            Assert.Same(innerHandler, pipeline.InnerHandler);
        }
    }
}
