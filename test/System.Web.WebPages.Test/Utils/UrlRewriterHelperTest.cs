// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Specialized;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.WebPages.Test
{
    public class UrlRewriterHelperTest
    {
        [Fact]
        public void WasRequestRewritten_FalseIfUrlRewriterIsTurnedOff()
        {
            // Arrange
            UrlRewriterHelper helper = new UrlRewriterHelper();
            UrlRewriteMocks request = CreateMockContext(isUrlRewriteOnForServer: false, isUrlRewriteOnForRequest: false);

            // Act
            bool result = helper.WasRequestRewritten(request.Context.Object);

            // Assert
            Assert.False(result);
            request.WorkerRequest.Verify();
            request.WorkerRequest.Verify(wr => wr.GetServerVariable(UrlRewriterHelper.UrlWasRewrittenServerVar), Times.Never());
        }

        [Fact]
        public void WasRequestRewritten_FalseIfUrlRewriterIsTurnedOnButRequestWasNotRewritten()
        {
            // Arrange
            UrlRewriterHelper helper = new UrlRewriterHelper();
            UrlRewriteMocks request = CreateMockContext(isUrlRewriteOnForServer: true, isUrlRewriteOnForRequest: false);

            // Act
            bool result = helper.WasRequestRewritten(request.Context.Object);

            // Assert
            Assert.False(result);
            request.WorkerRequest.Verify();
        }

        [Fact]
        public void WasRequestRewritten_TrueIfUrlRewriterIsTurnedOnAndRequestWasRewritten()
        {
            // Arrange
            UrlRewriterHelper helper = new UrlRewriterHelper();
            UrlRewriteMocks request = CreateMockContext(isUrlRewriteOnForServer: true, isUrlRewriteOnForRequest: true);

            // Act
            bool result = helper.WasRequestRewritten(request.Context.Object);

            // Assert
            Assert.True(result);
            request.WorkerRequest.Verify();
        }

        [Fact]
        public void WasRequestRewritten_ChecksIfUrlRewriterIsTurnedOnOnlyOnce()
        {
            // Arrange
            UrlRewriterHelper helper = new UrlRewriterHelper();
            UrlRewriteMocks request1 = CreateMockContext(isUrlRewriteOnForServer: false, isUrlRewriteOnForRequest: false);
            UrlRewriteMocks request2 = CreateMockContext(isUrlRewriteOnForServer: false, isUrlRewriteOnForRequest: false);

            // Act
            bool result1 = helper.WasRequestRewritten(request1.Context.Object);
            bool result2 = helper.WasRequestRewritten(request2.Context.Object);

            // Assert
            request1.WorkerRequest.Verify(c => c.GetServerVariable(UrlRewriterHelper.UrlRewriterEnabledServerVar), Times.Once());
            request2.WorkerRequest.Verify(c => c.GetServerVariable(UrlRewriterHelper.UrlRewriterEnabledServerVar), Times.Never());
            Assert.False(result1);
            Assert.False(result2);
        }

        [Fact]
        public void WasRequestRewritten_ChecksRequest_OnlyOnce_Positive()
        {
            // Arrange
            UrlRewriterHelper helper = new UrlRewriterHelper();
            UrlRewriteMocks request1 = CreateMockContext(isUrlRewriteOnForServer: true, isUrlRewriteOnForRequest: true);

            // Act
            bool result1 = helper.WasRequestRewritten(request1.Context.Object);
            bool result2 = helper.WasRequestRewritten(request1.Context.Object);

            // Assert
            request1.WorkerRequest.Verify(c => c.GetServerVariable(UrlRewriterHelper.UrlWasRewrittenServerVar), Times.Once());
            Assert.True(result1);
            Assert.True(result2);
        }

        [Fact]
        public void WasRequestRewritten_ChecksRequest_OnlyOnce_Negative()
        {
            // Arrange
            UrlRewriterHelper helper = new UrlRewriterHelper();
            UrlRewriteMocks request1 = CreateMockContext(isUrlRewriteOnForServer: true, isUrlRewriteOnForRequest: false);

            // Act
            bool result1 = helper.WasRequestRewritten(request1.Context.Object);
            bool result2 = helper.WasRequestRewritten(request1.Context.Object);

            // Assert
            request1.WorkerRequest.Verify(c => c.GetServerVariable(UrlRewriterHelper.UrlWasRewrittenServerVar), Times.Once());
            Assert.False(result1);
            Assert.False(result2);
        }

        private UrlRewriteMocks CreateMockContext(bool isUrlRewriteOnForServer, bool isUrlRewriteOnForRequest)
        {
            Mock<HttpContextBase> mockContext = new Mock<HttpContextBase>();

            Mock<HttpWorkerRequest> mockWorkerRequest = new Mock<HttpWorkerRequest>();
            mockContext.As<IServiceProvider>().Setup(sp => sp.GetService(typeof(HttpWorkerRequest))).Returns(mockWorkerRequest.Object);

            if (isUrlRewriteOnForServer)
            {
                mockWorkerRequest.Setup(wr => wr.GetServerVariable(UrlRewriterHelper.UrlRewriterEnabledServerVar)).Returns("On!").Verifiable();
            }
            else
            {
                mockWorkerRequest.Setup(wr => wr.GetServerVariable(UrlRewriterHelper.UrlRewriterEnabledServerVar)).Returns((string)null).Verifiable();
            }

            if (isUrlRewriteOnForRequest)
            {
                mockWorkerRequest.Setup(wr => wr.GetServerVariable(UrlRewriterHelper.UrlWasRewrittenServerVar)).Returns("Yup!").Verifiable();
            }
            else
            {
                // this won't be called if rewrite is off for the server
                mockWorkerRequest.Setup(wr => wr.GetServerVariable(UrlRewriterHelper.UrlWasRewrittenServerVar)).Returns((string)null);
            }

            NameValueCollection serverVars = new NameValueCollection();
            mockContext.Setup(c => c.Request.ServerVariables).Returns(serverVars);
            mockContext.Setup(c => c.Request.ApplicationPath).Returns("/myapp");

            mockContext.Setup(c => c.Items).Returns(new HybridDictionary());

            return new UrlRewriteMocks()
            {
                Context = mockContext,
                WorkerRequest = mockWorkerRequest,
            };
        }

        private class UrlRewriteMocks
        {
            public Mock<HttpContextBase> Context { get; set; }
            public Mock<HttpWorkerRequest> WorkerRequest { get; set; }
        }
    }
}
