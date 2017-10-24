// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.Controllers;
using Microsoft.TestCommon;

namespace System.Web.Http.Batch
{
    public class BatchHttpRequestMessageExtensionsTest
    {
        [Theory]
        [InlineData("MS_HttpRouteData")]
        [InlineData("MS_DisposableRequestResources")]
        [InlineData("MS_SynchronizationContext")]
        [InlineData("MS_HttpConfiguration")]
        [InlineData("MS_HttpBatchContext")]
        [InlineData("MS_RoutingContext")]
        public void CopyBatchRequestProperties_IgnoresSpecialProperties(string specialPropertyName)
        {
            // Arrange
            string notSpecialPropertyName = "SomeProperty";
            HttpRequestMessage baseRequest = new HttpRequestMessage();
            HttpRequestMessage childRequest = new HttpRequestMessage();

            baseRequest.Properties.Add(notSpecialPropertyName, 42);
            baseRequest.Properties.Add(specialPropertyName, 42);

            // Act
            childRequest.CopyBatchRequestProperties(baseRequest);

            // Assert
            Assert.Contains(notSpecialPropertyName, childRequest.Properties.Keys);
            Assert.DoesNotContain(specialPropertyName, childRequest.Properties.Keys);
        }

        [Fact]
        public void CopyBatchRequestProperties_AddsBatchHttpRequestContext()
        {
            using (HttpRequestMessage subRequest = new HttpRequestMessage())
            using (HttpRequestMessage batchRequest = new HttpRequestMessage())
            {
                HttpRequestContext expectedOriginalContext = new HttpRequestContext();
                subRequest.SetRequestContext(expectedOriginalContext);

                // Act
                BatchHttpRequestMessageExtensions.CopyBatchRequestProperties(subRequest, batchRequest);

                // Assert
                HttpRequestContext context = subRequest.GetRequestContext();
                BatchHttpRequestContext typedContext = Assert.IsType<BatchHttpRequestContext>(context);
                Assert.Same(expectedOriginalContext, typedContext.BatchContext);
            }
        }

        [Fact]
        public void CopyBatchRequestProperties_SetsRequestContextWithUrlHelperForSubRequest()
        {
            // Arrange
            using (HttpRequestMessage subRequest = new HttpRequestMessage())
            using (HttpRequestMessage batchRequest = new HttpRequestMessage())
            {
                subRequest.SetRequestContext(new HttpRequestContext());

                // Act
                BatchHttpRequestMessageExtensions.CopyBatchRequestProperties(subRequest, batchRequest);

                // Assert
                HttpRequestContext context = subRequest.GetRequestContext();
                Assert.NotNull(context);
                Assert.NotNull(context.Url);
                Assert.Same(subRequest, context.Url.Request);
            }
        }
    }
}
