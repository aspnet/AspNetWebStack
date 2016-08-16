// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Cors;
using System.Web.Http.Tracing;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Cors.Tracing
{
    public class CorsPolicyProviderTracerTest
    {
        [Fact]
        public async Task GetCorsPolicyAsync_CallsInner()
        {
            bool innerIsCalled = false;
            Mock<ITraceWriter> traceWriterMock = new Mock<ITraceWriter>();
            Mock<ICorsPolicyProvider> policyProviderMock = new Mock<ICorsPolicyProvider>();
            policyProviderMock
                .Setup(f => f.GetCorsPolicyAsync(It.IsAny<HttpRequestMessage>(), CancellationToken.None))
                .Returns(() =>
                {
                    innerIsCalled = true;
                    return Task.FromResult(new CorsPolicy());
                });
            CorsPolicyProviderTracer tracer = new CorsPolicyProviderTracer(policyProviderMock.Object, traceWriterMock.Object);

            await tracer.GetCorsPolicyAsync(new HttpRequestMessage(), CancellationToken.None);

            Assert.True(innerIsCalled);
        }

        [Fact]
        public async Task GetCorsPolicyAsync_EmitTraces()
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
            CorsPolicyProviderTracer tracer = new CorsPolicyProviderTracer(new EnableCorsAttribute(origins: "*", headers: "*", methods: "*"), traceWriterMock.Object);
            HttpRequestMessage requestMessage = new HttpRequestMessage();
            requestMessage.Method = HttpMethod.Get;
            requestMessage.Headers.Add(CorsConstants.Origin, "http://example.com");

            await tracer.GetCorsPolicyAsync(requestMessage, CancellationToken.None);

            Assert.NotNull(beginTrace);
            Assert.Equal(TraceCategories.CorsCategory, beginTrace.Category);
            Assert.Equal(TraceLevel.Info, beginTrace.Level);
            Assert.Equal("GetCorsPolicyAsync", beginTrace.Operation);
            Assert.Equal(
                @"CorsRequestContext: 'Origin: http://example.com, HttpMethod: GET, IsPreflight: False, Host: , AccessControlRequestMethod: null, RequestUri: , AccessControlRequestHeaders: {}'",
                beginTrace.Message);

            Assert.NotNull(endTrace);
            Assert.Equal(TraceCategories.CorsCategory, endTrace.Category);
            Assert.Equal(TraceLevel.Info, endTrace.Level);
            Assert.Equal("GetCorsPolicyAsync", endTrace.Operation);
            Assert.Equal(
                @"CorsPolicy selected: 'AllowAnyHeader: True, AllowAnyMethod: True, AllowAnyOrigin: True, PreflightMaxAge: null, SupportsCredentials: False, Origins: {}, Methods: {}, Headers: {}, ExposedHeaders: {}'",
                endTrace.Message);
        }
    }
}