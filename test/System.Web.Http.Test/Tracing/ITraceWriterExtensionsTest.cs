// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.ModelBinding;
using Microsoft.TestCommon;

namespace System.Web.Http.Tracing
{
    public class ITraceWriterExtensionsTest
    {
        [Fact]
        public void Debug_With_Message_Traces()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, "testCategory", TraceLevel.Debug) { Kind = TraceKind.Trace, Message = "The formatted message" },
            };

            // Act
            traceWriter.Debug(request, "testCategory", "The {0} message", "formatted");

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public void Debug_With_Exception_Traces()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            InvalidOperationException exception = new InvalidOperationException();
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, "testCategory", TraceLevel.Debug) { Kind = TraceKind.Trace, Exception = exception },
            };

            // Act
            traceWriter.Debug(request, "testCategory", exception);

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public void Debug_With_Message_And_Exception_Traces()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            InvalidOperationException exception = new InvalidOperationException();
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, "testCategory", TraceLevel.Debug) { Kind = TraceKind.Trace, Message = "The formatted message", Exception = exception },
            };

            // Act
            traceWriter.Debug(request, "testCategory", exception, "The {0} message", "formatted");

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public void Info_With_Message_Traces()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, "testCategory", TraceLevel.Info) { Kind = TraceKind.Trace, Message = "The formatted message" },
            };

            // Act
            traceWriter.Info(request, "testCategory", "The {0} message", "formatted");

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public void Info_With_Exception_Traces()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            InvalidOperationException exception = new InvalidOperationException();
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, "testCategory", TraceLevel.Info) { Kind = TraceKind.Trace, Exception = exception },
            };

            // Act
            traceWriter.Info(request, "testCategory", exception);

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public void Info_With_Message_And_Exception_Traces()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            InvalidOperationException exception = new InvalidOperationException();
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, "testCategory", TraceLevel.Info) { Kind = TraceKind.Trace, Message = "The formatted message", Exception = exception },
            };

            // Act
            traceWriter.Info(request, "testCategory", exception, "The {0} message", "formatted");

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public void Warn_With_Message_Traces()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, "testCategory", TraceLevel.Warn) { Kind = TraceKind.Trace, Message = "The formatted message" },
            };

            // Act
            traceWriter.Warn(request, "testCategory", "The {0} message", "formatted");

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public void Warn_With_Exception_Traces()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            InvalidOperationException exception = new InvalidOperationException();
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, "testCategory", TraceLevel.Warn) { Kind = TraceKind.Trace, Exception = exception },
            };

            // Act
            traceWriter.Warn(request, "testCategory", exception);

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public void Warn_With_Message_And_Exception_Traces()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            InvalidOperationException exception = new InvalidOperationException();
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, "testCategory", TraceLevel.Warn) { Kind = TraceKind.Trace, Message = "The formatted message", Exception = exception },
            };

            // Act
            traceWriter.Warn(request, "testCategory", exception, "The {0} message", "formatted");

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public void Error_With_Message_Traces()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, "testCategory", TraceLevel.Error) { Kind = TraceKind.Trace, Message = "The formatted message" },
            };

            // Act
            traceWriter.Error(request, "testCategory", "The {0} message", "formatted");

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public void Error_With_Exception_Traces()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            InvalidOperationException exception = new InvalidOperationException();
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, "testCategory", TraceLevel.Error) { Kind = TraceKind.Trace, Exception = exception },
            };

            // Act
            traceWriter.Error(request, "testCategory", exception);

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public void Error_With_Message_And_Exception_Traces()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            InvalidOperationException exception = new InvalidOperationException();
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, "testCategory", TraceLevel.Error) { Kind = TraceKind.Trace, Message = "The formatted message", Exception = exception },
            };

            // Act
            traceWriter.Error(request, "testCategory", exception, "The {0} message", "formatted");

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public void Fatal_With_Message_Traces()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, "testCategory", TraceLevel.Fatal) { Kind = TraceKind.Trace, Message = "The formatted message" },
            };

            // Act
            traceWriter.Fatal(request, "testCategory", "The {0} message", "formatted");

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public void Fatal_With_Exception_Traces()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            InvalidOperationException exception = new InvalidOperationException();
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, "testCategory", TraceLevel.Fatal) { Kind = TraceKind.Trace, Exception = exception },
            };

            // Act
            traceWriter.Fatal(request, "testCategory", exception);

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public void Fatal_With_Message_And_Exception_Traces()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            InvalidOperationException exception = new InvalidOperationException();
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, "testCategory", TraceLevel.Fatal) { Kind = TraceKind.Trace, Message = "The formatted message", Exception = exception },
            };

            // Act
            traceWriter.Fatal(request, "testCategory", exception, "The {0} message", "formatted");

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public void TraceBeginEnd_Throws_With_Null_This()
        {
            // Arrange
            TestTraceWriter traceWriter = null;
            HttpRequestMessage request = new HttpRequestMessage();

            // Act & Assert
            Assert.ThrowsArgumentNull(() => traceWriter.TraceBeginEnd(request,
                                             "",
                                             TraceLevel.Off,
                                             "",
                                             "",
                                             beginTrace: null,
                                             execute: () => { },
                                             endTrace: null,
                                             errorTrace: null),
                                       "traceWriter");
        }

        [Fact]
        public void TraceBeginEnd_Throws_With_Null_Execute_Action()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();

            // Act & Assert
            Assert.ThrowsArgumentNull(() => traceWriter.TraceBeginEnd(request,
                                             "",
                                             TraceLevel.Off,
                                             "",
                                             "",
                                             beginTrace: null,
                                             execute: null,
                                             endTrace: null,
                                             errorTrace: null),
                                       "execute");
        }

        [Fact]
        public void TraceBeginEnd_Accepts_Null_Trace_Actions()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();

            // Act & Assert
            traceWriter.TraceBeginEnd(request,
                     "",
                     TraceLevel.Off,
                     "",
                     "",
                     beginTrace: null,
                     execute: () => { },
                     endTrace: null,
                     errorTrace: null);
        }

        [Fact]
        public void TraceBeginEnd_Invokes_BeginTrace()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            bool invoked = false;

            // Act
            traceWriter.TraceBeginEnd(request,
                                 "",
                                 TraceLevel.Fatal,
                                 "",
                                 "",
                                 beginTrace: (tr) => { invoked = true; },
                                 execute: () => { },
                                 endTrace: (tr) => { },
                                 errorTrace: (tr) => { });

            // Assert
            Assert.True(invoked);
        }

        [Fact]
        public void TraceBeginEnd_Does_Not_Invoke_BeginTrace_When_Tracing_Only_Higher_Level()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            traceWriter.TraceSelector = (rqst, category, level) => level >= TraceLevel.Error;
            HttpRequestMessage request = new HttpRequestMessage();
            bool invoked = false;

            // Act
            traceWriter.TraceBeginEnd(request,
                                 "",
                                 TraceLevel.Info,
                                 "",
                                 "",
                                 beginTrace: (tr) => { invoked = true; },
                                 execute: () => { },
                                 endTrace: (tr) => { },
                                 errorTrace: (tr) => { });

            // Assert
            Assert.False(invoked);
        }

        [Fact]
        public void TraceBeginEnd_Invokes_Execute()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            bool invoked = false;

            // Act
            traceWriter.TraceBeginEnd(request,
                                 "",
                                 TraceLevel.Fatal,
                                 "",
                                 "",
                                 beginTrace: (tr) => { },
                                 execute: () => { invoked = true; },
                                 endTrace: (tr) => { },
                                 errorTrace: (tr) => { });

            // Assert
            Assert.True(invoked);
        }

        [Fact]
        public void TraceBeginEnd_Invokes_EndTrace()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            bool invoked = false;

            // Act
            traceWriter.TraceBeginEnd(request,
                                 "",
                                 TraceLevel.Off,
                                 "",
                                 "",
                                 beginTrace: (tr) => { },
                                 execute: () => { },
                                 endTrace: (tr) => { invoked = true; },
                                 errorTrace: (tr) => { });

            // Assert
            Assert.True(invoked);
        }

        [Fact]
        public void TraceBeginEnd_Does_Not_Invoke_EndTrace_When_Error_Occurs()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            Exception exception = new InvalidOperationException();
            bool invoked = false;

            // Act & Assert
            Exception thrown = Assert.Throws<InvalidOperationException>(
                    () => traceWriter.TraceBeginEnd(request,
                                 "",
                                 TraceLevel.Off,
                                 "",
                                 "",
                                 beginTrace: (tr) => { },
                                 execute: () => { throw exception; },
                                 endTrace: (tr) => { invoked = true; },
                                 errorTrace: (tr) => { }));
            Assert.False(invoked);
            Assert.Same(exception, thrown);
        }

        [Fact]
        public void TraceBeginEnd_Does_Not_Invoke_EndTrace_When_Tracing_Only_High_Level()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            traceWriter.TraceSelector = (rqst, category, level) => level >= TraceLevel.Error;
            HttpRequestMessage request = new HttpRequestMessage();
            bool invoked = false;

            // Act
            traceWriter.TraceBeginEnd(request,
                                 "",
                                 TraceLevel.Info,
                                 "",
                                 "",
                                 beginTrace: (tr) => { },
                                 execute: () => { },
                                 endTrace: (tr) => { invoked = true; },
                                 errorTrace: (tr) => { });

            // Assert
            Assert.False(invoked);
        }

        [Fact]
        public void TraceBeginEnd_Invokes_ErrorTrace()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            Exception exception = new InvalidOperationException();
            bool invoked = false;

            // Act & Assert
            Exception thrown = Assert.Throws<InvalidOperationException>(
                    () => traceWriter.TraceBeginEnd(request,
                                 "",
                                 TraceLevel.Off,
                                 "",
                                 "",
                                 beginTrace: (tr) => { },
                                 execute: () => { throw exception; },
                                 endTrace: (tr) => { },
                                 errorTrace: (tr) => { invoked = true; }));
            Assert.True(invoked);
            Assert.Same(exception, thrown);
        }

        [Fact]
        public void TraceBeginEnd_Does_Not_Invoke_ErrorTrace_When_Tracing_Only_Higher_Level()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            traceWriter.TraceSelector = (rqst, category, level) => level >= TraceLevel.Fatal;
            HttpRequestMessage request = new HttpRequestMessage();
            Exception exception = new InvalidOperationException();
            bool invoked = false;

            // Act & Assert
            Exception thrown = Assert.Throws<InvalidOperationException>(
                    () => traceWriter.TraceBeginEnd(request,
                                 "",
                                 TraceLevel.Info,
                                 "",
                                 "",
                                 beginTrace: (tr) => { },
                                 execute: () => { throw exception; },
                                 endTrace: (tr) => { },
                                 errorTrace: (tr) => { invoked = true; }));
            Assert.False(invoked);
            Assert.Same(exception, thrown);
        }

        [Fact]
        public void TraceBeginEnd_Invokes_ErrorTrace_When_Tracing_Only_Errors()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            traceWriter.TraceSelector = (rqst, category, level) => level >= TraceLevel.Error;
            HttpRequestMessage request = new HttpRequestMessage();
            Exception exception = new InvalidOperationException();
            bool invoked = false;

            // Act & Assert
            Exception thrown = Assert.Throws<InvalidOperationException>(
                    () => traceWriter.TraceBeginEnd(request,
                                 "",
                                 TraceLevel.Off,
                                 "",
                                 "",
                                 beginTrace: (tr) => { },
                                 execute: () => { throw exception; },
                                 endTrace: (tr) => { },
                                 errorTrace: (tr) => { invoked = true; }));
            Assert.True(invoked);
            Assert.Same(exception, thrown);
        }

        [Fact]
        public void TraceBeginEnd_Does_Not_Invoke_ErrorTrace_Unless_Error_Occurs()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            bool invoked = false;

            // Act
            traceWriter.TraceBeginEnd(request,
                                 "",
                                 TraceLevel.Info,
                                 "",
                                 "",
                                 beginTrace: (tr) => { },
                                 execute: () => { },
                                 endTrace: (tr) => { },
                                 errorTrace: (tr) => { invoked = true; });

            // Assert
            Assert.False(invoked);
        }

        [Fact]
        public void TraceBeginEnd_Traces()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, "testCategory", TraceLevel.Info) { Kind = TraceKind.Begin, Operator = "tester", Operation = "testOp", Message = "beginMessage" },
                new TraceRecord(request, "testCategory", TraceLevel.Info) { Kind = TraceKind.End, Operator = "tester", Operation = "testOp", Message = "endMessage" },
            };

            // Act
            traceWriter.TraceBeginEnd(request,
                                 "testCategory",
                                 TraceLevel.Info,
                                 "tester",
                                 "testOp",
                                 beginTrace: (tr) => { tr.Message = "beginMessage"; },
                                 execute: () => { },
                                 endTrace: (tr) => { tr.Message = "endMessage"; },
                                 errorTrace: (tr) => { tr.Message = "won't happen"; });

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public void TraceBeginEnd_Traces_And_Throws_When_Execute_Throws()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            InvalidOperationException exception = new InvalidOperationException("test exception");
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, "testCategory", TraceLevel.Info) { Kind = TraceKind.Begin, Operator = "tester", Operation = "testOp", Message = "beginMessage" },
                new TraceRecord(request, "testCategory", TraceLevel.Error) { Kind = TraceKind.End, Operator = "tester", Operation = "testOp", Exception = exception, Message = "errorMessage" },
            };

            // Act & Assert
            Exception thrown = Assert.Throws<InvalidOperationException>(
                                () => traceWriter.TraceBeginEnd(request,
                                    "testCategory",
                                    TraceLevel.Info,
                                    "tester",
                                    "testOp",
                                    beginTrace: (tr) => { tr.Message = "beginMessage"; },
                                    execute: () => { throw exception; },
                                    endTrace: (tr) => { tr.Message = "won't happen"; },
                                    errorTrace: (tr) => { tr.Message = "errorMessage"; }));
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
            Assert.Same(exception, thrown);
        }

        [Fact]
        public void TraceBeginEnd_Traces_And_Throws_HttpResponseException()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            HttpResponseException exception = new HttpResponseException(Net.HttpStatusCode.NotFound);
            List<TraceRecord> expectedTraces = new List<TraceRecord>
            {
                new TraceRecord(request, "testCategory", TraceLevel.Error)
                {
                    Kind = TraceKind.Begin, Operator = "tester", Operation = "testOp", Message = "beginMessage"
                },
                new TraceRecord(request, "testCategory", TraceLevel.Warn)
                {
                    Kind = TraceKind.End, Operator = "tester", Operation = "testOp", Exception = exception,
                    Message = "errorMessage", Status = Net.HttpStatusCode.NotFound
                },
            };

            // Act & Assert
            Exception thrown = Assert.Throws<HttpResponseException>(
                                () => traceWriter.TraceBeginEnd(request,
                                    "testCategory",
                                    TraceLevel.Error,
                                    "tester",
                                    "testOp",
                                    beginTrace: (tr) => { tr.Message = "beginMessage"; },
                                    execute: () => { throw exception; },
                                    endTrace: (tr) => { tr.Message = "won't Happen"; },
                                    errorTrace: (tr) => { tr.Message = "errorMessage"; }));
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
            Assert.Same(thrown, exception);
        }

        [Fact]
        public void TraceBeginEnd_Traces_And_Throws_AggregateException()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            AggregateException aggregateException = CreateAggregateException(request);
            List<TraceRecord> expectedTraces = new List<TraceRecord>
            {
                new TraceRecord(request, "testCategory", TraceLevel.Error)
                {
                    Kind = TraceKind.Begin, Operator = "tester", Operation = "testOp", Message = "beginMessage"
                },
                new TraceRecord(request, "testCategory", TraceLevel.Warn)
                {
                    Kind = TraceKind.End, Operator = "tester", Operation = "testOp", Exception = aggregateException,
                    // In the aggregateException, only the httpResponseException with the highest status code
                    // will be reflected in the trace record's message. In this test case, it should be NotFound.
                    Message = "UserMessage='The request is invalid.', ModelStateError=[key=[error], username=[invalid]]",
                    Status = Net.HttpStatusCode.NotFound
                },
            };

            // Act & Assert
            Exception thrown = Assert.Throws<AggregateException>(
                                () => traceWriter.TraceBeginEnd(request,
                                    "testCategory",
                                    TraceLevel.Error,
                                    "tester",
                                    "testOp",
                                    beginTrace: (tr) => { tr.Message = "beginMessage"; },
                                    execute: () => { throw aggregateException; },
                                    endTrace: (tr) => { tr.Message = "won't Happen"; },
                                    errorTrace: null));
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
            Assert.Same(thrown, expectedTraces[1].Exception);
        }

        [Fact]
        public void TraceBeginEnd_Does_Not_Trace_HttpResponseException_When_Tracing_Only_Higher_Level()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            traceWriter.TraceSelector = (rqst, category, level) => level >= TraceLevel.Error;
            HttpRequestMessage request = new HttpRequestMessage();
            bool invoked = false;
            Exception exception = new HttpResponseException(Net.HttpStatusCode.NotFound);

            // Act & Assert
            Exception thrown = Assert.Throws<HttpResponseException>(
                                () => traceWriter.TraceBeginEnd(request,
                                "",
                                TraceLevel.Info,
                                "",
                                "",
                                beginTrace: (tr) => { invoked = true; },
                                execute: () => { throw exception; },
                                endTrace: (tr) => { },
                                errorTrace: (tr) => { }));
            Assert.False(invoked);
            Assert.Empty(traceWriter.Traces);
            Assert.Same(thrown, exception);
        }

        [Fact]
        public void TraceBeginEndAsync_Throws_With_Null_This()
        {
            // Arrange
            TestTraceWriter traceWriter = null;
            HttpRequestMessage request = new HttpRequestMessage();

            // Act & Assert
            Assert.ThrowsArgumentNull(() => traceWriter.TraceBeginEndAsync(request,
                                             "",
                                             TraceLevel.Off,
                                             "",
                                             "",
                                             beginTrace: null,
                                             execute: () => TaskHelpers.Completed(),
                                             endTrace: null,
                                             errorTrace: null),
                                       "traceWriter");
        }

        [Fact]
        public void TraceBeginEndAsync_Throws_With_Null_Execute_Action()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();

            // Act & Assert
            Assert.ThrowsArgumentNull(() => traceWriter.TraceBeginEndAsync(request,
                                             "",
                                             TraceLevel.Off,
                                             "",
                                             "",
                                             beginTrace: null,
                                             execute: null,
                                             endTrace: null,
                                             errorTrace: null),
                                       "execute");
        }

        [Fact]
        public Task TraceBeginEndAsync_Accepts_Null_Trace_Actions()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();

            // Act & Assert
            return traceWriter.TraceBeginEndAsync(request,
                     "",
                     TraceLevel.Off,
                     "",
                     "",
                     beginTrace: null,
                     execute: () => TaskHelpers.Completed(),
                     endTrace: null,
                     errorTrace: null);
        }

        [Fact]
        public async Task TraceBeginEndAsync_Invokes_BeginTrace()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            bool invoked = false;

            // Act
            await traceWriter.TraceBeginEndAsync(request,
                                 "",
                                 TraceLevel.Fatal,
                                 "",
                                 "",
                                 beginTrace: (tr) => { invoked = true; },
                                 execute: () => TaskHelpers.Completed(),
                                 endTrace: (tr) => { },
                                 errorTrace: (tr) => { });

            // Assert
            Assert.True(invoked);
        }

        [Fact]
        public async Task TraceBeginEndAsync_Does_Not_Invoke_BeginTrace_When_Tracing_Only_Higher_Level()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            traceWriter.TraceSelector = (rqst, category, level) => level >= TraceLevel.Error;
            HttpRequestMessage request = new HttpRequestMessage();
            bool invoked = false;

            // Act
            await traceWriter.TraceBeginEndAsync(request,
                                 "",
                                 TraceLevel.Info,
                                 "",
                                 "",
                                 beginTrace: (tr) => { invoked = true; },
                                 execute: () => TaskHelpers.Completed(),
                                 endTrace: (tr) => { },
                                 errorTrace: (tr) => { });

            // Assert
            Assert.False(invoked);
        }

        [Fact]
        public async Task TraceBeginEndAsync_Traces_And_Throws_HttpResponseException()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            HttpResponseException exception = new HttpResponseException(Net.HttpStatusCode.InternalServerError);
            List<TraceRecord> expectedTraces = new List<TraceRecord>
            {
                new TraceRecord(request, "testCategory", TraceLevel.Info)
                {
                    Kind = TraceKind.Begin, Operator = "tester", Operation = "testOp", Message = "beginMessage"
                },
                new TraceRecord(request, "testCategory", TraceLevel.Error)
                {
                    Kind = TraceKind.End, Operator = "tester", Operation = "testOp", Exception = exception,
                    Message = "errorMessage", Status = Net.HttpStatusCode.InternalServerError
                },
            };

            // Act & Assert
            Exception thrown = await Assert.ThrowsAsync<HttpResponseException>(
                                () => traceWriter.TraceBeginEndAsync(request,
                                    "testCategory",
                                    TraceLevel.Info,
                                    "tester",
                                    "testOp",
                                    beginTrace: (tr) => { tr.Message = "beginMessage"; },
                                    execute: () => { throw exception; },
                                    endTrace: (tr) => { tr.Message = "won't Happen"; },
                                    errorTrace: (tr) => { tr.Message = "errorMessage"; }));
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
            Assert.Same(thrown, exception);
        }

        [Fact]
        public async Task TraceBeginEndAsync_Does_Not_Trace_HttpResponseException_When_Tracing_Only_Higher_Level()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            traceWriter.TraceSelector = (rqst, category, level) => level >= TraceLevel.Error;
            HttpRequestMessage request = new HttpRequestMessage();
            bool invoked = false;
            Exception exception = new HttpResponseException(Net.HttpStatusCode.NotFound);

            // Act & Assert
            Exception thrown = await Assert.ThrowsAsync<HttpResponseException>(
                                () => traceWriter.TraceBeginEndAsync(request,
                                "",
                                TraceLevel.Info,
                                "",
                                "",
                                beginTrace: (tr) => { invoked = true; },
                                execute: () => TaskHelpers.FromError(exception),
                                endTrace: (tr) => { },
                                errorTrace: (tr) => { }));
            Assert.False(invoked);
            Assert.Empty(traceWriter.Traces);
            Assert.Same(thrown, exception);
        }

        [Fact]
        public async Task TraceBeginEndAsync_Invokes_Execute()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            bool invoked = false;

            // Act
            await traceWriter.TraceBeginEndAsync(request,
                                 "",
                                 TraceLevel.Fatal,
                                 "",
                                 "",
                                 beginTrace: (tr) => { },
                                 execute: () =>
                                 {
                                     invoked = true;
                                     return TaskHelpers.Completed();
                                 },
                                 endTrace: (tr) => { },
                                 errorTrace: (tr) => { });

            // Assert
            Assert.True(invoked);
        }

        [Fact]
        public async Task TraceBeginEndAsync_Invokes_EndTrace()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            bool invoked = false;

            // Act
            await traceWriter.TraceBeginEndAsync(request,
                                 "",
                                 TraceLevel.Off,
                                 "",
                                 "",
                                 beginTrace: (tr) => { },
                                 execute: () => TaskHelpers.Completed(),
                                 endTrace: (tr) => { invoked = true; },
                                 errorTrace: (tr) => { });

            // Assert
            Assert.True(invoked);
        }

        [Fact]
        public async Task TraceBeginEndAsync_Does_Not_Invoke_End_Trace_When_Error_Occurs()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            bool invoked = false;
            Exception exception = new InvalidOperationException();
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>(null);
            tcs.TrySetException(exception);

            // Act & Assert
            Exception thrown = await Assert.ThrowsAsync<InvalidOperationException>(
                        () => traceWriter.TraceBeginEndAsync(request,
                                 "",
                                 TraceLevel.Off,
                                 "",
                                 "",
                                 beginTrace: (tr) => { },
                                 execute: () => tcs.Task,
                                 endTrace: (tr) => { invoked = true; },
                                 errorTrace: (tr) => { }));
            Assert.False(invoked);
            Assert.Same(exception, thrown);
        }

        [Fact]
        public async Task TraceBeginEndAsync_Does_Not_Invoke_EndTrace_When_Tracing_Only_Higher_Level()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            traceWriter.TraceSelector = (rqst, category, level) => level >= TraceLevel.Error;
            HttpRequestMessage request = new HttpRequestMessage();
            bool invoked = false;

            // Act
            await traceWriter.TraceBeginEndAsync(request,
                                 "",
                                 TraceLevel.Info,
                                 "",
                                 "",
                                 beginTrace: (tr) => { },
                                 execute: () => TaskHelpers.Completed(),
                                 endTrace: (tr) => { invoked = true; },
                                 errorTrace: (tr) => { });

            // Assert
            Assert.False(invoked);
        }

        [Fact]
        public async Task TraceBeginEndAsync_Invokes_ErrorTrace()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            bool invoked = false;
            Exception exception = new InvalidOperationException();
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>(null);
            tcs.TrySetException(exception);

            // Act & Assert
            Exception thrown = await Assert.ThrowsAsync<InvalidOperationException>(
                        () => traceWriter.TraceBeginEndAsync(request,
                                 "",
                                 TraceLevel.Off,
                                 "",
                                 "",
                                 beginTrace: (tr) => { },
                                 execute: () => tcs.Task,
                                 endTrace: (tr) => { },
                                 errorTrace: (tr) => { invoked = true; }));
            Assert.True(invoked);
            Assert.Same(exception, thrown);
        }

        [Fact]
        public async Task TraceBeginEndAsync_Does_Not_Invoke_ErrorTrace_When_Tracing_Only_Higher_Level()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            traceWriter.TraceSelector = (rqst, category, level) => level >= TraceLevel.Fatal;
            HttpRequestMessage request = new HttpRequestMessage();
            bool invoked = false;
            Exception exception = new InvalidOperationException();
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>(null);
            tcs.TrySetException(exception);

            // Act & Assert
            Exception thrown = await Assert.ThrowsAsync<InvalidOperationException>(
                        () => traceWriter.TraceBeginEndAsync(request,
                                 "",
                                 TraceLevel.Info,
                                 "",
                                 "",
                                 beginTrace: (tr) => { },
                                 execute: () => tcs.Task,
                                 endTrace: (tr) => { },
                                 errorTrace: (tr) => { invoked = true; }));
            Assert.False(invoked);
            Assert.Same(exception, thrown);
        }

        [Fact]
        public async Task TraceBeginEndAsync_Invokes_ErrorTrace_When_Tracing_Only_Errors()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            traceWriter.TraceSelector = (rqst, category, level) => level >= TraceLevel.Error;
            HttpRequestMessage request = new HttpRequestMessage();
            bool invoked = false;
            Exception exception = new InvalidOperationException();
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>(null);
            tcs.TrySetException(exception);

            // Act & Assert
            Exception thrown = await Assert.ThrowsAsync<InvalidOperationException>(
                        () => traceWriter.TraceBeginEndAsync(request,
                                 "",
                                 TraceLevel.Info,
                                 "",
                                 "",
                                 beginTrace: (tr) => { },
                                 execute: () => tcs.Task,
                                 endTrace: (tr) => { },
                                 errorTrace: (tr) => { invoked = true; }));
            Assert.True(invoked);
            Assert.Same(exception, thrown);
        }

        [Fact]
        public async Task TraceBeginEndAsync_Does_Not_Invoke_ErrorTrace_Unless_Error_Occurs()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            bool invoked = false;
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>(null);
            tcs.TrySetResult(null);

            // Act
            await traceWriter.TraceBeginEndAsync(
                            request,
                            "",
                            TraceLevel.Off,
                            "",
                            "",
                            beginTrace: (tr) => { },
                            execute: () => tcs.Task,
                            endTrace: (tr) => { },
                            errorTrace: (tr) => { invoked = true; });

            // Assert
            Assert.False(invoked);
        }

        [Fact]
        public async Task TraceBeginEndAsync_Traces()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, "testCategory", TraceLevel.Info)
                {
                    Kind = TraceKind.Begin, Operator = "tester", Operation = "testOp", Message = "beginMessage"
                },
                new TraceRecord(request, "testCategory", TraceLevel.Info)
                {
                    Kind = TraceKind.End, Operator = "tester", Operation = "testOp", Message = "endMessage"
                },
            };

            // Act
            await traceWriter.TraceBeginEndAsync(request,
                                 "testCategory",
                                 TraceLevel.Info,
                                 "tester",
                                 "testOp",
                                 beginTrace: (tr) => { tr.Message = "beginMessage"; },
                                 execute: () => TaskHelpers.Completed(),
                                 endTrace: (tr) => { tr.Message = "endMessage"; },
                                 errorTrace: (tr) => { tr.Message = "won't happen"; });

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public async Task TraceBeginEndAsync_Traces_When_Inner_Cancels()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, "testCategory", TraceLevel.Info)
                {
                    Kind = TraceKind.Begin, Operator = "tester", Operation = "testOp", Message = "beginMessage"
                },
                new TraceRecord(request, "testCategory", TraceLevel.Warn)
                {
                    Kind = TraceKind.End, Operator = "tester", Operation = "testOp", Message = "errorMessage"
                },
            };

            // Act & Assert
            await Assert.ThrowsAsync<TaskCanceledException>(
                () => traceWriter.TraceBeginEndAsync(request,
                     "testCategory",
                     TraceLevel.Info,
                     "tester",
                     "testOp",
                     beginTrace: (tr) => { tr.Message = "beginMessage"; },
                     execute: () => TaskHelpers.Canceled(),
                     endTrace: (tr) => { tr.Message = "won't happen"; },
                     errorTrace: (tr) => { tr.Message = "errorMessage"; }));
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task TraceBeginEndAsync_Traces_And_Throws_AggregateException(bool isExThrownAtExecution)
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            AggregateException aggregateException = CreateAggregateException(request);
            List<TraceRecord> expectedTraces = new List<TraceRecord>
            {
                new TraceRecord(request, "testCategory", TraceLevel.Error)
                {
                    Kind = TraceKind.Begin, Operator = "tester", Operation = "testOp", Message = "beginMessage"
                },
                new TraceRecord(request, "testCategory", TraceLevel.Warn)
                {
                    Kind = TraceKind.End, Operator = "tester", Operation = "testOp", Exception = aggregateException,
                    // In the aggregateException, only the httpResponseException with the highest status code
                    // will be reflected in the trace record's message. In this test case, it should be NotFound.
                    Message = "UserMessage='The request is invalid.', ModelStateError=[key=[error], username=[invalid]]",
                    Status = Net.HttpStatusCode.NotFound
                },
            };

            // Act & Assert
            Exception thrown = await Assert.ThrowsAsync<AggregateException>(
                                () => traceWriter.TraceBeginEndAsync(request,
                                    "testCategory",
                                    TraceLevel.Error,
                                    "tester",
                                    "testOp",
                                    beginTrace: (tr) => { tr.Message = "beginMessage"; },
                                    execute: () =>
                                    {
                                        if (isExThrownAtExecution)
                                        {
                                            throw aggregateException;
                                        }
                                        Task task = new Task(() => { throw aggregateException; });
                                        task.Start();
                                        return task;
                                    },
                                    endTrace: (tr) => { tr.Message = "won't Happen"; },
                                    errorTrace: null));
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
            Assert.Same(thrown, expectedTraces[1].Exception);
        }

        [Fact]
        public async Task TraceBeginAsync_Traces_And_Faults_When_Inner_Faults()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            InvalidOperationException exception = new InvalidOperationException();
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, "testCategory", TraceLevel.Info)
                {
                    Kind = TraceKind.Begin, Operator = "tester", Operation = "testOp", Message = "beginMessage"
                },
                new TraceRecord(request, "testCategory", TraceLevel.Error)
                {
                    Kind = TraceKind.End, Operator = "tester", Operation = "testOp",
                    Message = "errorMessage", Exception = exception
                },
            };

            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>(null);
            tcs.TrySetException(exception);

            // Act & Assert
            InvalidOperationException thrown = await Assert.ThrowsAsync<InvalidOperationException>(
                        () => traceWriter.TraceBeginEndAsync(request,
                                     "testCategory",
                                     TraceLevel.Info,
                                     "tester",
                                     "testOp",
                                     beginTrace: (tr) => { tr.Message = "beginMessage"; },
                                     execute: () => tcs.Task,
                                     endTrace: (tr) => { tr.Message = "won't happen"; },
                                     errorTrace: (tr) => { tr.Message = "errorMessage"; }));
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
            Assert.Same(exception, thrown);
        }

        [Fact]
        public void TraceBeginEndAsyncGeneric_Throws_With_Null_This()
        {
            // Arrange
            TestTraceWriter traceWriter = null;
            HttpRequestMessage request = new HttpRequestMessage();

            // Act & Assert
            Assert.ThrowsArgumentNull(() => traceWriter.TraceBeginEndAsync<int>(request,
                                             "",
                                             TraceLevel.Off,
                                             "",
                                             "",
                                             beginTrace: null,
                                             execute: () => Task.FromResult<int>(1),
                                             endTrace: null,
                                             errorTrace: null),
                                       "traceWriter");
        }

        [Fact]
        public void TraceBeginEndAsyncGeneric_Throws_With_Null_Execute_Action()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();

            // Act & Assert
            Assert.ThrowsArgumentNull(() => traceWriter.TraceBeginEndAsync<int>(request,
                                             "",
                                             TraceLevel.Off,
                                             "",
                                             "",
                                             beginTrace: null,
                                             execute: null,
                                             endTrace: null,
                                             errorTrace: null),
                                       "execute");
        }

        [Fact]
        public Task TraceBeginEndAsyncGeneric_Accepts_Null_Trace_Actions()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();

            // Act & Assert
            return traceWriter.TraceBeginEndAsync<int>(request,
                     "",
                     TraceLevel.Off,
                     "",
                     "",
                     beginTrace: null,
                     execute: () => Task.FromResult<int>(1),
                     endTrace: null,
                     errorTrace: null);
        }

        [Fact]
        public async Task TraceBeginEndAsyncGeneric_Invokes_BeginTrace()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            bool invoked = false;

            // Act
            await traceWriter.TraceBeginEndAsync<int>(request,
                                 "",
                                 TraceLevel.Fatal,
                                 "",
                                 "",
                                 beginTrace: (tr) => { invoked = true; },
                                 execute: () => Task.FromResult<int>(1),
                                 endTrace: (tr, value) => { },
                                 errorTrace: (tr) => { });

            // Assert
            Assert.True(invoked);
        }

        [Fact]
        public async Task TraceBeginEndAsyncGeneric_Traces_And_Throws_HttpResponseException()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            HttpResponseException exception = new HttpResponseException(Net.HttpStatusCode.NotFound);
            List<TraceRecord> expectedTraces = new List<TraceRecord>
            {
                new TraceRecord(request, "testCategory", TraceLevel.Error)
                {
                    Kind = TraceKind.Begin, Operator = "tester", Operation = "testOp", Message = "beginMessage"
                },
                new TraceRecord(request, "testCategory", TraceLevel.Warn)
                {
                    Kind = TraceKind.End, Operator = "tester", Operation = "testOp", Exception = exception,
                    Message = "errorMessage", Status = Net.HttpStatusCode.NotFound
                },
            };

            // Act & Assert
            Exception thrown = await Assert.ThrowsAsync<HttpResponseException>(
                                () => traceWriter.TraceBeginEndAsync<int>(request,
                                    "testCategory",
                                    TraceLevel.Error,
                                    "tester",
                                    "testOp",
                                    beginTrace: (tr) => { tr.Message = "beginMessage"; },
                                    execute: () => { throw exception; },
                                    endTrace: (tr, result) => { tr.Message = "won't Happen"; },
                                    errorTrace: (tr) => { tr.Message = "errorMessage"; }));
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
            Assert.Same(thrown, exception);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task TraceBeginEndAsyncGeneric_Traces_And_Throws_AggregateException(bool isExThrownAtExecution)
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            AggregateException aggregateException = CreateAggregateException(request);
            List<TraceRecord> expectedTraces = new List<TraceRecord>
            {
                new TraceRecord(request, "testCategory", TraceLevel.Error)
                {
                    Kind = TraceKind.Begin, Operator = "tester", Operation = "testOp", Message = "beginMessage"
                },
                new TraceRecord(request, "testCategory", TraceLevel.Warn)
                {
                    Kind = TraceKind.End, Operator = "tester", Operation = "testOp", Exception = aggregateException,
                    // In the aggregateException, only the httpResponseException with the highest status code
                    // will be reflected in the trace record's message. In this test case, it should be NotFound.
                    Message = "UserMessage='The request is invalid.', ModelStateError=[key=[error], username=[invalid]]",
                    Status = Net.HttpStatusCode.NotFound
                },
            };

            // Act & Assert
            Exception thrown = await Assert.ThrowsAsync<AggregateException>(
                                () => traceWriter.TraceBeginEndAsync<int>(request,
                                    "testCategory",
                                    TraceLevel.Error,
                                    "tester",
                                    "testOp",
                                    beginTrace: (tr) => { tr.Message = "beginMessage"; },
                                    execute: () =>
                                    {
                                        if (isExThrownAtExecution)
                                        {
                                            throw aggregateException;
                                        }
                                        Task<int> task = new Task<int>(() => { throw aggregateException; });
                                        task.Start();
                                        return task;
                                    },
                                    endTrace: (tr, result) => { tr.Message = "won't Happen"; },
                                    errorTrace: null));
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
            Assert.Same(thrown, expectedTraces[1].Exception);
        }

        [Fact]
        public async Task TraceBeginEndAsyncGeneric_Does_Not_Trace_HttpResponseException_When_Tracing_Only_Higher_Level()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            traceWriter.TraceSelector = (rqst, category, level) => level >= TraceLevel.Error;
            HttpRequestMessage request = new HttpRequestMessage();
            bool invoked = false;

            // Act & Assert
            await Assert.ThrowsAsync<HttpResponseException>(
                () => traceWriter.TraceBeginEndAsync(request,
                "",
                TraceLevel.Info,
                "",
                "",
                beginTrace: (tr) => { invoked = true; },
                execute: () => TaskHelpers.FromError(new HttpResponseException(Net.HttpStatusCode.NotFound)),
                endTrace: (tr) => { },
                errorTrace: (tr) => { }));
            Assert.False(invoked);
            Assert.Empty(traceWriter.Traces);
        }

        [Fact]
        public async Task TraceBeginEndAsyncGeneric_Does_Not_Invoke_BeginTrace_When_Tracing_Only_Higher_Level()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            traceWriter.TraceSelector = (rqst, category, level) => level >= TraceLevel.Error;
            HttpRequestMessage request = new HttpRequestMessage();
            bool invoked = false;

            // Act
            await traceWriter.TraceBeginEndAsync<int>(request,
                                 "",
                                 TraceLevel.Info,
                                 "",
                                 "",
                                 beginTrace: (tr) => { invoked = true; },
                                 execute: () => Task.FromResult<int>(1),
                                 endTrace: (tr, value) => { },
                                 errorTrace: (tr) => { });

            // Assert
            Assert.False(invoked);
        }

        [Fact]
        public async Task TraceBeginEndAsyncGeneric_Invokes_Execute()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            bool invoked = false;

            // Act
            int result = await traceWriter.TraceBeginEndAsync<int>(request,
                                 "",
                                 TraceLevel.Fatal,
                                 "",
                                 "",
                                 beginTrace: (tr) => { },
                                 execute: () =>
                                 {
                                     invoked = true;
                                     return Task.FromResult<int>(1);
                                 },
                                 endTrace: (tr, value) => { },
                                 errorTrace: (tr) => { });

            // Assert
            Assert.True(invoked);
            Assert.Equal(1, result);
        }

        [Fact]
        public async Task TraceBeginEndAsyncGeneric_Invokes_EndTrace()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            bool invoked = false;
            int invokedValue = 0;

            // Act
            await traceWriter.TraceBeginEndAsync<int>(request,
                                 "",
                                 TraceLevel.Off,
                                 "",
                                 "",
                                 beginTrace: (tr) => { },
                                 execute: () => Task.FromResult<int>(1),
                                 endTrace: (tr, value) => { invoked = true; invokedValue = value; },
                                 errorTrace: (tr) => { });

            // Assert
            Assert.True(invoked);
            Assert.Equal(1, invokedValue);
        }

        [Fact]
        public async Task TraceBeginEndAsyncGeneric_Does_Not_Invoke_EndTrace_When_Error_Occurs()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            bool invoked = false;
            Exception exception = new InvalidOperationException();
            TaskCompletionSource<int> tcs = new TaskCompletionSource<int>(0);
            tcs.TrySetException(exception);

            // Act & Assert
            Exception thrown = await Assert.ThrowsAsync<InvalidOperationException>(
                () => traceWriter.TraceBeginEndAsync<int>(request,
                                 "",
                                 TraceLevel.Off,
                                 "",
                                 "",
                                 beginTrace: (tr) => { },
                                 execute: () => tcs.Task,
                                 endTrace: (tr, value) => { invoked = true; },
                                 errorTrace: (tr) => { }));
            Assert.False(invoked);
            Assert.Same(exception, thrown);
        }

        [Fact]
        public async Task TraceBeginEndAsyncGeneric_Does_Not_Invoke_EndTrace_When_Tracing_Only_Higher_Level()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            traceWriter.TraceSelector = (rqst, category, level) => level >= TraceLevel.Error;
            HttpRequestMessage request = new HttpRequestMessage();
            bool invoked = false;

            // Act
            await traceWriter.TraceBeginEndAsync<int>(request,
                                 "",
                                 TraceLevel.Info,
                                 "",
                                 "",
                                 beginTrace: (tr) => { },
                                 execute: () => Task.FromResult<int>(1),
                                 endTrace: (tr, value) => { invoked = true; },
                                 errorTrace: (tr) => { });

            // Assert
            Assert.False(invoked);
        }

        [Fact]
        public async Task TraceBeginEndAsyncGeneric_Invokes_ErrorTrace()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            bool invoked = false;
            Exception exception = new InvalidOperationException();
            TaskCompletionSource<int> tcs = new TaskCompletionSource<int>(0);
            tcs.TrySetException(exception);

            // Act & Assert
            Exception thrown = await Assert.ThrowsAsync<InvalidOperationException>(
                () => traceWriter.TraceBeginEndAsync<int>(request,
                                 "",
                                 TraceLevel.Off,
                                 "",
                                 "",
                                 beginTrace: (tr) => { },
                                 execute: () => tcs.Task,
                                 endTrace: (tr, value) => { },
                                 errorTrace: (tr) => { invoked = true; }));
            Assert.True(invoked);
            Assert.Same(exception, thrown);
        }

        [Fact]
        public async Task TraceBeginEndAsyncGeneric_Does_Not_Invoke_ErrorTrace_When_Tracing_Only_Higher_Level()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            traceWriter.TraceSelector = (rqst, category, level) => level >= TraceLevel.Fatal;
            HttpRequestMessage request = new HttpRequestMessage();
            bool invoked = false;
            Exception exception = new InvalidOperationException();
            TaskCompletionSource<int> tcs = new TaskCompletionSource<int>(0);
            tcs.TrySetException(exception);

            // Act & Assert
            Exception thrown = await Assert.ThrowsAsync<InvalidOperationException>(
                () => traceWriter.TraceBeginEndAsync<int>(request,
                                 "",
                                 TraceLevel.Info,
                                 "",
                                 "",
                                 beginTrace: (tr) => { },
                                 execute: () => tcs.Task,
                                 endTrace: (tr, value) => { },
                                 errorTrace: (tr) => { invoked = true; }));
            Assert.False(invoked);
            Assert.Same(exception, thrown);
        }

        [Fact]
        public async Task TraceBeginEndAsyncGeneric_Invokes_ErrorTrace_When_Tracing_Only_Errors()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            traceWriter.TraceSelector = (rqst, category, level) => level >= TraceLevel.Error;
            HttpRequestMessage request = new HttpRequestMessage();
            bool invoked = false;
            Exception exception = new InvalidOperationException();
            TaskCompletionSource<int> tcs = new TaskCompletionSource<int>(0);
            tcs.TrySetException(exception);

            // Act & Assert
            Exception thrown = await Assert.ThrowsAsync<InvalidOperationException>(
                () => traceWriter.TraceBeginEndAsync<int>(request,
                                 "",
                                 TraceLevel.Info,
                                 "",
                                 "",
                                 beginTrace: (tr) => { },
                                 execute: () => tcs.Task,
                                 endTrace: (tr, value) => { },
                                 errorTrace: (tr) => { invoked = true; }));
            Assert.True(invoked);
            Assert.Same(exception, thrown);
        }

        [Fact]
        public async Task TraceBeginEndAsyncGeneric_Does_Not_Invoke_ErrorTrace_Unless_Error_Occurs()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            bool invoked = false;
            TaskCompletionSource<int> tcs = new TaskCompletionSource<int>();
            tcs.TrySetResult(1);

            // Act
            await traceWriter.TraceBeginEndAsync<int>(
                            request,
                            "",
                            TraceLevel.Off,
                            "",
                            "",
                            beginTrace: (tr) => { },
                            execute: () => tcs.Task,
                            endTrace: (tr, value) => { },
                            errorTrace: (tr) => { invoked = true; });

            // Assert
            Assert.False(invoked);
        }

        [Fact]
        public async Task TraceBeginEndAsyncGeneric_Traces()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, "testCategory", TraceLevel.Info)
                {
                    Kind = TraceKind.Begin, Operator = "tester", Operation = "testOp", Message = "beginMessage"
                },
                new TraceRecord(request, "testCategory", TraceLevel.Info)
                {
                    Kind = TraceKind.End, Operator = "tester", Operation = "testOp", Message = "endMessage1"
                },
            };

            // Act
            await traceWriter.TraceBeginEndAsync<int>(request,
                                 "testCategory",
                                 TraceLevel.Info,
                                 "tester",
                                 "testOp",
                                 beginTrace: (tr) => { tr.Message = "beginMessage"; },
                                 execute: () => Task.FromResult<int>(1),
                                 endTrace: (tr, value) => { tr.Message = "endMessage" + value; },
                                 errorTrace: (tr) => { tr.Message = "won't happen"; });

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public async Task TraceBeginEndAsyncGeneric_Traces_When_Inner_Cancels()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, "testCategory", TraceLevel.Info)
                {
                    Kind = TraceKind.Begin, Operator = "tester", Operation = "testOp", Message = "beginMessage"
                },
                new TraceRecord(request, "testCategory", TraceLevel.Warn)
                {
                    Kind = TraceKind.End, Operator = "tester", Operation = "testOp", Message = "errorMessage"
                },
            };

            // Act & Assert
            await Assert.ThrowsAsync<TaskCanceledException>(
                () => traceWriter.TraceBeginEndAsync<int>(request,
                     "testCategory",
                     TraceLevel.Info,
                     "tester",
                     "testOp",
                     beginTrace: (tr) => { tr.Message = "beginMessage"; },
                     execute: () => TaskHelpers.Canceled<int>(),
                     endTrace: (tr, value) => { tr.Message = "won't happen"; },
                     errorTrace: (tr) => { tr.Message = "errorMessage"; }));
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public async Task TraceBeginAsyncGeneric_Traces_And_Faults_When_Inner_Faults()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            InvalidOperationException exception = new InvalidOperationException();
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, "testCategory", TraceLevel.Info)
                {
                    Kind = TraceKind.Begin, Operator = "tester", Operation = "testOp", Message = "beginMessage"
                },
                new TraceRecord(request, "testCategory", TraceLevel.Error)
                {
                    Kind = TraceKind.End, Operator = "tester", Operation = "testOp",
                    Message = "errorMessage", Exception = exception
                },
            };

            TaskCompletionSource<int> tcs = new TaskCompletionSource<int>(1);
            tcs.TrySetException(exception);

            // Act & Assert
            InvalidOperationException thrown = await Assert.ThrowsAsync<InvalidOperationException>(
                        () => traceWriter.TraceBeginEndAsync<int>(request,
                                     "testCategory",
                                     TraceLevel.Info,
                                     "tester",
                                     "testOp",
                                     beginTrace: (tr) => { tr.Message = "beginMessage"; },
                                     execute: () => tcs.Task,
                                     endTrace: (tr, value) => { tr.Message = "won't happen"; },
                                     errorTrace: (tr) => { tr.Message = "errorMessage"; }));
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
            Assert.Same(exception, thrown);
        }

        private static AggregateException CreateAggregateException(HttpRequestMessage request)
        {
            using (new CultureReplacer())
            {
                HttpError httpError = new HttpError(new ModelStateDictionary()
                {
                    { "key", new ModelState() { Errors = { new ModelError("error") } } },
                    { "username", new ModelState() { Errors = { new ModelError("invalid") } } },
                }, true);
                HttpResponseException hre = new HttpResponseException(
                    request.CreateErrorResponse(Net.HttpStatusCode.BadRequest,
                                                new HttpError("Error Message from HRE.")));
                Exception nestedHre = new Exception("Level 1",
                    new Exception("Level 2",
                        new HttpResponseException(request.CreateErrorResponse(Net.HttpStatusCode.NotFound, httpError))));
                List<Exception> exceptions = new List<Exception>();
                exceptions.Add(hre);
                exceptions.Add(nestedHre);
                return new AggregateException(exceptions);
            }
        }
    }
}
