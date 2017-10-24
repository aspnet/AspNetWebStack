// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Cors;
using System.Web.Http.Tracing;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Cors.Tracing
{
    public class CorsPolicyProviderFactoryTracerTest
    {
        [Fact]
        public void GetCorsPolicyProvider_CallsInner()
        {
            bool innerIsCalled = false;
            Mock<ITraceWriter> traceWriterMock = new Mock<ITraceWriter>();
            Mock<ICorsPolicyProviderFactory> policyProviderFactoryMock = new Mock<ICorsPolicyProviderFactory>();
            policyProviderFactoryMock
                .Setup(f => f.GetCorsPolicyProvider(It.IsAny<HttpRequestMessage>()))
                .Returns(() =>
                {
                    innerIsCalled = true;
                    return new Mock<ICorsPolicyProvider>().Object;
                });
            CorsPolicyProviderFactoryTracer tracer = new CorsPolicyProviderFactoryTracer(policyProviderFactoryMock.Object, traceWriterMock.Object);

            tracer.GetCorsPolicyProvider(new HttpRequestMessage());

            Assert.True(innerIsCalled);
        }

        [Fact]
        public void GetCorsPolicyProvider_ReturnsCorsPolicyProviderTracer()
        {
            Mock<ITraceWriter> traceWriterMock = new Mock<ITraceWriter>();
            ICorsPolicyProvider expectedPolicyProvider = new Mock<ICorsPolicyProvider>().Object;
            Mock<ICorsPolicyProviderFactory> policyProviderFactoryMock = new Mock<ICorsPolicyProviderFactory>();
            policyProviderFactoryMock
                .Setup(f => f.GetCorsPolicyProvider(It.IsAny<HttpRequestMessage>()))
                .Returns(expectedPolicyProvider);
            CorsPolicyProviderFactoryTracer tracer = new CorsPolicyProviderFactoryTracer(policyProviderFactoryMock.Object, traceWriterMock.Object);

            ICorsPolicyProvider policyProvider = tracer.GetCorsPolicyProvider(new HttpRequestMessage());

            Assert.IsType<CorsPolicyProviderTracer>(policyProvider);
        }

        [Fact]
        public void GetCorsPolicyProvider_ReturnsNullWhenInnerReturnsNull()
        {
            Mock<ITraceWriter> traceWriterMock = new Mock<ITraceWriter>();
            ICorsPolicyProvider expectedPolicyProvider = new Mock<ICorsPolicyProvider>().Object;
            Mock<ICorsPolicyProviderFactory> policyProviderFactoryMock = new Mock<ICorsPolicyProviderFactory>();
            policyProviderFactoryMock
                .Setup(f => f.GetCorsPolicyProvider(It.IsAny<HttpRequestMessage>()))
                .Returns((ICorsPolicyProvider)null);
            CorsPolicyProviderFactoryTracer tracer = new CorsPolicyProviderFactoryTracer(policyProviderFactoryMock.Object, traceWriterMock.Object);

            ICorsPolicyProvider policyProvider = tracer.GetCorsPolicyProvider(new HttpRequestMessage());

            Assert.Null(policyProvider);
        }

        [Fact]
        public void GetCorsPolicyProvider_EmitTraces()
        {
            TraceRecord beginTrace = null;
            TraceRecord endTrace = null;
            Mock<ITraceWriter> traceWriterMock = new Mock<ITraceWriter>();
            traceWriterMock
                .Setup(t => t.Trace(It.IsAny<HttpRequestMessage>(), It.IsAny<string>(), It.IsAny<TraceLevel>(), It.IsAny<Action<TraceRecord>>()))
                .Callback<HttpRequestMessage, string, TraceLevel, Action<TraceRecord>>((request, category, level, traceAction) =>
                {
                    TraceRecord traceRecord = new TraceRecord(request, category, level);
                    traceAction(traceRecord);
                    if (traceRecord.Kind == TraceKind.Begin)
                    {
                        beginTrace = traceRecord;
                    }
                    else if (traceRecord.Kind == TraceKind.End)
                    {
                        endTrace = traceRecord;
                    }
                });
            Mock<ICorsPolicyProviderFactory> policyProviderFactoryMock = new Mock<ICorsPolicyProviderFactory>();
            policyProviderFactoryMock
                .Setup(f => f.GetCorsPolicyProvider(It.IsAny<HttpRequestMessage>()))
                .Returns(new EnableCorsAttribute(origins: "*", headers: "*", methods: "*"));
            CorsPolicyProviderFactoryTracer tracer = new CorsPolicyProviderFactoryTracer(policyProviderFactoryMock.Object, traceWriterMock.Object);
            HttpRequestMessage requestMessage = new HttpRequestMessage();
            requestMessage.Method = HttpMethod.Get;
            requestMessage.Headers.Add(CorsConstants.Origin, "http://example.com");

            tracer.GetCorsPolicyProvider(requestMessage);

            Assert.NotNull(beginTrace);
            Assert.Equal(TraceCategories.CorsCategory, beginTrace.Category);
            Assert.Equal(TraceLevel.Info, beginTrace.Level);
            Assert.Equal("GetCorsPolicyProvider", beginTrace.Operation);
            Assert.Equal(
                @"CorsRequestContext: 'Origin: http://example.com, HttpMethod: GET, IsPreflight: False, Host: , AccessControlRequestMethod: null, RequestUri: , AccessControlRequestHeaders: {}'",
                beginTrace.Message);

            Assert.NotNull(endTrace);
            Assert.Equal(TraceCategories.CorsCategory, endTrace.Category);
            Assert.Equal(TraceLevel.Info, endTrace.Level);
            Assert.Equal("GetCorsPolicyProvider", endTrace.Operation);
            Assert.Equal(
                @"CorsPolicyProvider selected: 'System.Web.Http.Cors.EnableCorsAttribute'",
                endTrace.Message);
        }
    }
}