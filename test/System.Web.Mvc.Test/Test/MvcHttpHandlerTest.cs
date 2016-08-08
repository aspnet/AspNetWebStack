// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class MvcHttpHandlerTest
    {
        [Fact]
        public void ConstructorDoesNothing()
        {
            new MvcHttpHandler();
        }

        [Fact]
        public void VerifyAndProcessRequestWithNullHandlerThrows()
        {
            // Arrange
            PublicMvcHttpHandler handler = new PublicMvcHttpHandler();

            // Act
            Assert.ThrowsArgumentNull(
                delegate { handler.PublicVerifyAndProcessRequest(null, null); },
                "httpHandler");
        }

        [Fact]
        public void ProcessRequestCallsExecute()
        {
            // Arrange
            PublicMvcHttpHandler handler = new PublicMvcHttpHandler();
            Mock<IHttpHandler> mockTargetHandler = new Mock<IHttpHandler>();
            mockTargetHandler.Setup(h => h.ProcessRequest(It.IsAny<HttpContext>())).Verifiable();

            // Act
            handler.PublicVerifyAndProcessRequest(mockTargetHandler.Object, null);

            // Assert
            mockTargetHandler.Verify();
        }

        private sealed class DummyHttpHandler : IHttpHandler
        {
            bool IHttpHandler.IsReusable
            {
                get { throw new NotImplementedException(); }
            }

            void IHttpHandler.ProcessRequest(HttpContext context)
            {
                throw new NotImplementedException();
            }
        }

        private sealed class PublicMvcHttpHandler : MvcHttpHandler
        {
            public void PublicVerifyAndProcessRequest(IHttpHandler httpHandler, HttpContextBase httpContext)
            {
                base.VerifyAndProcessRequest(httpHandler, httpContext);
            }
        }
    }
}
