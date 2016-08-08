// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http.ModelBinding;
using Microsoft.TestCommon;

namespace System.Web.Http.Tracing.Diagnostics.Test
{
    public class SystemDiagnosticsTraceWriterTest
    {
        // Duplicates of constants in HttpError
        private const string MessageKey = "Message";
        private const string MessageDetailKey = "MessageDetail";
        private const string ModelStateKey = "ModelState";
        private const string ExceptionMessageKey = "ExceptionMessage";
        private const string ExceptionTypeKey = "ExceptionType";
        private const string StackTraceKey = "StackTrace";
        private const string InnerExceptionKey = "InnerException";

        [Fact]
        public void Ctor_Initializes_Properties()
        {
            // Arrange
            SystemDiagnosticsTraceWriter writer = new SystemDiagnosticsTraceWriter();

            // Act & Assert
            Assert.False(writer.IsVerbose);
            Assert.Equal(TraceLevel.Info, writer.MinimumLevel);
            Assert.Null(writer.TraceSource);
        }

        [Fact]
        public void TraceSource_Accepts_Custom_TraceSource()
        {
            // Arrange
            SystemDiagnosticsTraceWriter writer = new SystemDiagnosticsTraceWriter();
            TraceSource traceSource = new TraceSource("CustomTraceSource");

            // Act
            writer.TraceSource = traceSource;

            // Assert
            Assert.Equal(traceSource, writer.TraceSource);
        }

        [Fact]
        public void TraceSource_Accepts_Null_TraceSource()
        {
            // Arrange
            SystemDiagnosticsTraceWriter writer = new SystemDiagnosticsTraceWriter();

            // Act
            writer.TraceSource = null;

            // Assert
            Assert.Null(writer.TraceSource);
        }

        [Theory]
        [InlineData(TraceLevel.Debug)]
        [InlineData(TraceLevel.Info)]
        [InlineData(TraceLevel.Warn)]
        [InlineData(TraceLevel.Error)]
        [InlineData(TraceLevel.Fatal)]
        public void MinimumLevel_Setter_Accepts_All_Legal_Levels(TraceLevel level)
        {
            // Arrange
            SystemDiagnosticsTraceWriter writer = new SystemDiagnosticsTraceWriter();

            // Act
            writer.MinimumLevel = level;

            // Assert
            Assert.Equal(level, writer.MinimumLevel);
        }

        [Theory]
        [InlineData(TraceLevel.Off - 1)]
        [InlineData(TraceLevel.Fatal + 1)]
        public void MinimumLevel_Setter_Throws_With_Bad_Level(TraceLevel level)
        {
            // Arrange & Act & Assert
            ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(() => { new SystemDiagnosticsTraceWriter().MinimumLevel = level; });
            Assert.Equal("value", exception.ParamName);
            Assert.Contains("The TraceLevel property must be a value between TraceLevel.Off and TraceLevel.Fatal, inclusive.", exception.Message);
            Assert.Equal(level, exception.ActualValue);
        }

        [Fact]
        public void Trace_Throws_With_Null_Category()
        {
            // Arrange
            SystemDiagnosticsTraceWriter writer = new SystemDiagnosticsTraceWriter();

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
                                                () => writer.Trace(new HttpRequestMessage(),
                                                                    null,
                                                                    TraceLevel.Info,
                                                                    (tr) => { }));
            Assert.Equal("category", exception.ParamName);
        }

        [Fact]
        public void Trace_Throws_With_Null_TraceAction()
        {
            // Arrange
            SystemDiagnosticsTraceWriter writer = new SystemDiagnosticsTraceWriter();

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
                                                () => writer.Trace(new HttpRequestMessage(),
                                                                    "MyCategory",
                                                                    TraceLevel.Info,
                                                                    traceAction: null));
            Assert.Equal("traceAction", exception.ParamName);
        }

        [Theory]
        [InlineData(TraceLevel.Off - 1)]
        [InlineData(TraceLevel.Fatal + 1)]
        public void Trace_Throws_With_Illegal_Level(TraceLevel level)
        {
            // Arrange
            SystemDiagnosticsTraceWriter writer = new SystemDiagnosticsTraceWriter();

            // Act & Assert
            ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(
                                                () => writer.Trace(new HttpRequestMessage(),
                                                                    "MyCategory",
                                                                    level,
                                                                    (tr) => { }));
            Assert.Equal("level", exception.ParamName);
            Assert.Contains("The TraceLevel property must be a value between TraceLevel.Off and TraceLevel.Fatal, inclusive.", exception.Message);
            Assert.Equal(level, exception.ActualValue);
        }

        [Fact]
        public void Trace_Writes_To_Custom_TraceSource()
        {
            // Arrange
            SystemDiagnosticsTraceWriter writer = CreateTraceWriter();

            HttpRequestMessage request = new HttpRequestMessage
            {
                RequestUri = new Uri("http://localhost"),
                Method = HttpMethod.Get
            };

            // Act
            writer.Info(request, "TestCategory", "TestMessage");

            // Assert
            Assert.Equal("Message='TestMessage'", ((TestTraceListener)writer.TraceSource.Listeners[0]).Messages[0]);
        }

        [Theory]
        [InlineData(TraceLevel.Debug, TraceEventType.Verbose)]
        [InlineData(TraceLevel.Info, TraceEventType.Information)]
        [InlineData(TraceLevel.Warn, TraceEventType.Warning)]
        [InlineData(TraceLevel.Error, TraceEventType.Error)]
        [InlineData(TraceLevel.Fatal, TraceEventType.Critical)]
        public void Trace_Writes_Correct_EventType_To_TraceListeners(TraceLevel level, TraceEventType diagnosticLevel)
        {
            // Arrange
            SystemDiagnosticsTraceWriter writer = CreateTraceWriter();
            writer.MinimumLevel = level;

            HttpRequestMessage request = new HttpRequestMessage
            {
                RequestUri = new Uri("http://localhost"),
                Method = HttpMethod.Get
            };

            // Act
            writer.Trace(request, "TestCategory", level, (tr) => { tr.Message = "TestMessage"; });

            // Assert
            Assert.Equal(diagnosticLevel, ((TestTraceListener)writer.TraceSource.Listeners[0]).TraceEventType);
        }

        [Theory]
        [InlineData(TraceLevel.Debug)]
        [InlineData(TraceLevel.Info)]
        [InlineData(TraceLevel.Warn)]
        [InlineData(TraceLevel.Error)]
        [InlineData(TraceLevel.Fatal)]
        public void Trace_Verbose_Writes_Correct_Message_To_TraceListeners_With_All_Fields_Set(TraceLevel level)
        {
            // Arrange
            SystemDiagnosticsTraceWriter writer = CreateTraceWriter();
            writer.MinimumLevel = level;
            writer.IsVerbose = true;

            HttpRequestMessage request = new HttpRequestMessage
            {
                RequestUri = new Uri("http://localhost"),
                Method = HttpMethod.Get
            };

            InvalidOperationException exception = new InvalidOperationException("TestException");

            // Act
            writer.Trace(request, "TestCategory", level, (tr) =>
            {
                tr.Message = "TestMessage";
                tr.Operation = "TestOperation";
                tr.Operator = "TestOperator";
                tr.Status = HttpStatusCode.Accepted;
                tr.Exception = exception;
            });

            // Assert
            string expected = String.Format("Level={0}, Kind=Trace, Category='TestCategory', Id={1}, Message='TestMessage', Operation=TestOperator.TestOperation, Status=202 (Accepted), Exception={2}",
                                                level.ToString(),
                                                request.GetCorrelationId().ToString(),
                                                exception.ToString());

            string actual = ((TestTraceListener)writer.TraceSource.Listeners[0]).Messages[0].Trim();
            string timePrefix = "] ";
            actual = actual.Substring(actual.IndexOf(timePrefix) + timePrefix.Length);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(TraceLevel.Debug)]
        [InlineData(TraceLevel.Info)]
        [InlineData(TraceLevel.Warn)]
        [InlineData(TraceLevel.Error)]
        [InlineData(TraceLevel.Fatal)]
        public void Trace_Brief_Writes_Correct_Message_To_TraceListeners_With_All_Fields_Set(TraceLevel level)
        {
            // Arrange
            SystemDiagnosticsTraceWriter writer = CreateTraceWriter();
            writer.MinimumLevel = level;

            HttpRequestMessage request = new HttpRequestMessage
            {
                RequestUri = new Uri("http://localhost"),
                Method = HttpMethod.Get
            };

            InvalidOperationException exception = new InvalidOperationException("TestException");

            // Act
            writer.Trace(request, "TestCategory", level, (tr) =>
            {
                tr.Message = "TestMessage";
                tr.Operation = "TestOperation";
                tr.Operator = "TestOperator";
                tr.Status = HttpStatusCode.Accepted;
                tr.Exception = exception;
            });

            // Assert
            string expected = "Message='TestMessage', Operation=TestOperator.TestOperation, Status=202 (Accepted), Exception=System.InvalidOperationException: TestException";
            string actual = ((TestTraceListener)writer.TraceSource.Listeners[0]).Messages[0].Trim();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Trace_Verbose_Writes_Correct_Message_To_TraceListeners_With_Only_Required_Fields_Set()
        {
            // Arrange
            SystemDiagnosticsTraceWriter writer = CreateTraceWriter();
            writer.IsVerbose = true;

            HttpRequestMessage request = new HttpRequestMessage
            {
                RequestUri = new Uri("http://localhost"),
                Method = HttpMethod.Get
            };

            InvalidOperationException exception = new InvalidOperationException("TestException");

            // Act
            writer.Info(request, "TestCategory", String.Empty);

            // Assert
            string expected = String.Format("Level=Info, Kind=Trace, Category='TestCategory', Id={0}",
                                            request.GetCorrelationId().ToString());

            string actual = ((TestTraceListener)writer.TraceSource.Listeners[0]).Messages[0].Trim();
            string timePrefix = "] ";
            actual = actual.Substring(actual.IndexOf(timePrefix) + timePrefix.Length);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Trace_Brief_Writes_Correct_Message_To_TraceListeners_With_Only_Required_Fields_Set()
        {
            // Arrange
            SystemDiagnosticsTraceWriter writer = CreateTraceWriter();

            HttpRequestMessage request = new HttpRequestMessage
            {
                RequestUri = new Uri("http://localhost"),
                Method = HttpMethod.Get
            };

            InvalidOperationException exception = new InvalidOperationException("TestException");

            // Act
            writer.Info(request, "TestCategory", "TestMessage");

            // Assert
            string expected = "Message='TestMessage'";
            string actual = ((TestTraceListener)writer.TraceSource.Listeners[0]).Messages[0].Trim();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Trace_Brief_Trace_Does_Not_Trace_When_No_Data()
        {
            // Arrange
            SystemDiagnosticsTraceWriter writer = CreateTraceWriter();

            HttpRequestMessage request = new HttpRequestMessage
            {
                RequestUri = new Uri("http://localhost"),
                Method = HttpMethod.Get
            };

            InvalidOperationException exception = new InvalidOperationException("TestException");

            // Act
            writer.Trace(request, "TestCategory", TraceLevel.Info, (tr) => { });

            // Assert
            Assert.Equal(0, ((TestTraceListener)writer.TraceSource.Listeners[0]).Messages.Count);
        }

        [Fact]
        public void Trace_Brief_Trace_Does_Not_Trace_BeginKind()
        {
            // Arrange
            SystemDiagnosticsTraceWriter writer = CreateTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage
            {
                RequestUri = new Uri("http://localhost"),
                Method = HttpMethod.Get
            };

            InvalidOperationException exception = new InvalidOperationException("TestException");

            // Act
            writer.Trace(request, "TestCategory", TraceLevel.Info, (tr) => { tr.Kind = TraceKind.Begin; tr.Message = "TestMessage"; });

            // Assert
            Assert.Equal(0, ((TestTraceListener)writer.TraceSource.Listeners[0]).Messages.Count);
        }

        [Theory]
        [InlineData(TraceLevel.Debug)]
        [InlineData(TraceLevel.Info)]
        [InlineData(TraceLevel.Warn)]
        [InlineData(TraceLevel.Error)]
        [InlineData(TraceLevel.Fatal)]
        public void Trace_Does_Not_Trace_Below_Minimum_Level(TraceLevel level)
        {
            // Arrange
            SystemDiagnosticsTraceWriter writer = CreateTraceWriter();
            writer.MinimumLevel = level;

            HttpRequestMessage request = new HttpRequestMessage
            {
                RequestUri = new Uri("http://localhost"),
                Method = HttpMethod.Get
            };

            InvalidOperationException exception = new InvalidOperationException("TestException");

            // Act
            writer.Trace(request, "TestCategory", level - 1, (tr) => { });

            // Assert
            Assert.Equal(0, ((TestTraceListener)writer.TraceSource.Listeners[0]).Messages.Count);
        }

        [Fact]
        public void Trace_Traces_Warning_EventType_When_Translates_HttpResponseException_Error()
        {
            // Arrange
            SystemDiagnosticsTraceWriter writer = CreateTraceWriter();
            writer.IsVerbose = true;

            HttpRequestMessage request = new HttpRequestMessage
            {
                RequestUri = new Uri("http://localhost"),
                Method = HttpMethod.Get
            };

            HttpResponseMessage response = request.CreateErrorResponse(HttpStatusCode.BadRequest, "bad request");
            HttpResponseException responseException = new HttpResponseException(response);

            // Act
            writer.Error(request, "TestCategory", responseException);

            // Assert
            Assert.Equal(TraceEventType.Warning, ((TestTraceListener)writer.TraceSource.Listeners[0]).TraceEventType);
        }

        [Fact]
        void Format_Throws_With_Null_TraceRecord()
        {
            // Arrange & Act & Assert
            ArgumentNullException exception =
                Assert.Throws<ArgumentNullException>(
                () => { new SystemDiagnosticsTraceWriter().Format(null); });

            Assert.Equal("traceRecord", exception.ParamName);
        }

        [Theory]
        [InlineData(TraceLevel.Debug)]
        [InlineData(TraceLevel.Info)]
        [InlineData(TraceLevel.Warn)]
        [InlineData(TraceLevel.Error)]
        [InlineData(TraceLevel.Fatal)]
        public void Format_Verbose_Builds_Trace_With_All_TraceRecord_Properties(TraceLevel level)
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage
            {
                RequestUri = new Uri("http://localhost"),
                Method = HttpMethod.Get
            };

            InvalidOperationException exception;
            try
            {
                // Want the full stack trace in the payload
                throw new InvalidOperationException("TestException");
            }
            catch (InvalidOperationException ex)
            {
                exception = ex;
            }

            TraceRecord traceRecord = new TraceRecord(request, "TestCategory", level)
            {
                Message = "TestMessage",
                Operation = "TestOperation",
                Operator = "TestOperator",
                Status = HttpStatusCode.Accepted,
                Exception = exception
            };

            // Act
            string formattedTrace = new SystemDiagnosticsTraceWriter() { IsVerbose = true }.Format(traceRecord);

            // Assert
            AssertContainsExactly(formattedTrace,
                                new Dictionary<string, string>
                                    {
                                        { "Level", level.ToString() },
                                        { "Kind", TraceKind.Trace.ToString() },
                                        { "Category", "'TestCategory'"},
                                        { "Id", request.GetCorrelationId().ToString() },
                                        { "Message", "'TestMessage'" },
                                        { "Operation", "TestOperator.TestOperation" },
                                        { "Status", "202 (Accepted)" },
                                        { "Exception", exception.ToString() },
                                    });

        }

        [Fact]
        public void Format_Builds_Trace_With_Null_Fields()
        {
            // Arrange
            TraceRecord traceRecord = new TraceRecord(request: null, category: null, level: TraceLevel.Info)
            {
                Message = null,
                Operation = null,
                Operator = null,
            };

            // Act
            string formattedTrace = new SystemDiagnosticsTraceWriter() { IsVerbose = true }.Format(traceRecord);

            // Assert
            AssertContainsExactly(formattedTrace,
                                new Dictionary<string, string>
                                    {
                                        { "Level", TraceLevel.Info.ToString() },
                                        { "Kind", TraceKind.Trace.ToString() },
                                        { "Id", new Guid().ToString() }
                                    });

        }

        [Fact]
        public void Format_Delegates_To_FormatRequestEnvelope_Begin()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage
            {
                RequestUri = new Uri("http://localhost"),
                Method = HttpMethod.Get
            };

            TraceRecord traceRecord = new TraceRecord(request, "System.Web.Http.Request", TraceLevel.Info)
            {
                Kind = TraceKind.Begin,
            };

            // Act
            string actualTrace = new SystemDiagnosticsTraceWriter() { IsVerbose = true }.Format(traceRecord);

            // Assert
            string timePrefix = "] ";
            actualTrace = actualTrace.Substring(actualTrace.IndexOf(timePrefix) + timePrefix.Length);
            string expectedTrace = String.Format("Request received, Method=GET, Url=http://localhost/, Id={0}", traceRecord.RequestId.ToString());
            Assert.Equal(expectedTrace, actualTrace);
        }

        [Fact]
        public void Format_Verbose_Delegates_To_FormatRequestEnvelope_End()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage
            {
                RequestUri = new Uri("http://localhost"),
                Method = HttpMethod.Get
            };

            TraceRecord traceRecord = new TraceRecord(request, "System.Web.Http.Request", TraceLevel.Info)
            {
                Kind = TraceKind.End,
            };

            // Act
            string actualTrace = new SystemDiagnosticsTraceWriter() { IsVerbose = true }.Format(traceRecord);

            // Assert
            string timePrefix = "] ";
            actualTrace = actualTrace.Substring(actualTrace.IndexOf(timePrefix) + timePrefix.Length);
            string expectedTrace = String.Format("Sending response, Method=GET, Url=http://localhost/, Id={0}", traceRecord.RequestId.ToString());
            Assert.Equal(expectedTrace, actualTrace);
        }

        [Fact]
        void FormatRequestEnvelope_Throws_With_Null_TraceRecord()
        {
            // Arrange & Act & Assert
            ArgumentNullException exception =
                Assert.Throws<ArgumentNullException>(
                () => { new SystemDiagnosticsTraceWriter().FormatRequestEnvelope(null); });

            Assert.Equal("traceRecord", exception.ParamName);
        }

        [Fact]
        public void FormatRequestEnvelope_Verbose_Builds_Correct_Begin_Trace_With_Only_Required_Fields()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage
            {
                RequestUri = new Uri("http://localhost"),
                Method = HttpMethod.Get
            };

            TraceRecord traceRecord = new TraceRecord(request, "System.Web.Http.Request", TraceLevel.Info)
            {
                Kind = TraceKind.Begin,
            };

            // Act
            string actualTrace = new SystemDiagnosticsTraceWriter() { IsVerbose = true }.FormatRequestEnvelope(traceRecord);

            // Assert
            string timePrefix = "] ";
            actualTrace = actualTrace.Substring(actualTrace.IndexOf(timePrefix) + timePrefix.Length);
            string expectedTrace = String.Format("Request received, Method=GET, Url=http://localhost/, Id={0}", traceRecord.RequestId.ToString());
            Assert.Equal(expectedTrace, actualTrace);
        }

        [Fact]
        public void FormatRequestEnvelope_Verbose_Builds_Correct_Begin_Trace_With_All_Fields()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage
            {
                RequestUri = new Uri("http://localhost"),
                Method = HttpMethod.Get
            };

            Exception exception = null;
            try
            {
                throw new InvalidOperationException("ExceptionMessage");
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            TraceRecord traceRecord = new TraceRecord(request, "System.Web.Http.Request", TraceLevel.Info)
            {
                Kind = TraceKind.Begin,
                Message = "EnvelopeMessage",
                Exception = exception
            };

            // Act
            string actualTrace = new SystemDiagnosticsTraceWriter() { IsVerbose = true }.FormatRequestEnvelope(traceRecord);

            // Assert
            string timePrefix = "] ";
            actualTrace = actualTrace.Substring(actualTrace.IndexOf(timePrefix) + timePrefix.Length);
            string expectedTrace = String.Format("Request received, Method=GET, Url=http://localhost/, Id={0}, Message='EnvelopeMessage', Exception={1}",
                                                traceRecord.RequestId.ToString(),
                                                exception.ToString().Trim());
            Assert.Equal(expectedTrace, actualTrace);
        }

        [Fact]
        public void FormatRequestEnvelope_Verbose_Builds_Correct_Begin_Trace_With_Null_Fields()
        {
            // Arrange
            TraceRecord traceRecord = new TraceRecord(request: null, category: null, level: TraceLevel.Info)
            {
                Kind = TraceKind.Begin,
            };

            // Act
            string actualTrace = new SystemDiagnosticsTraceWriter() { IsVerbose = true }.FormatRequestEnvelope(traceRecord);

            // Assert
            string timePrefix = "] ";
            actualTrace = actualTrace.Substring(actualTrace.IndexOf(timePrefix) + timePrefix.Length);
            string expectedTrace = String.Format("Request received, Id={0}", traceRecord.RequestId.ToString());
            Assert.Equal(expectedTrace, actualTrace);
        }

        [Fact]
        public void FormatRequestEnvelope_Verbose_Builds_Correct_End_Trace_With_Only_Required_Fields()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage
            {
                RequestUri = new Uri("http://localhost"),
                Method = HttpMethod.Get
            };

            TraceRecord traceRecord = new TraceRecord(request, "System.Web.Http.Request", TraceLevel.Info)
            {
                Kind = TraceKind.End,
                Status = HttpStatusCode.Accepted
            };

            // Act
            string actualTrace = new SystemDiagnosticsTraceWriter() { IsVerbose = true }.FormatRequestEnvelope(traceRecord);

            // Assert
            string timePrefix = "] ";
            actualTrace = actualTrace.Substring(actualTrace.IndexOf(timePrefix) + timePrefix.Length);
            string expectedTrace = String.Format("Sending response, Status=202 (Accepted), Method=GET, Url=http://localhost/, Id={0}", traceRecord.RequestId.ToString());
            Assert.Equal(expectedTrace, actualTrace);
        }

        [Fact]
        public void FormatRequestEnvelope_Verbose_Builds_Correct_End_Trace_With_All_Fields()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage
            {
                RequestUri = new Uri("http://localhost"),
                Method = HttpMethod.Get
            };

            Exception exception = null;
            try
            {
                throw new InvalidOperationException("ExceptionMessage");
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            TraceRecord traceRecord = new TraceRecord(request, "System.Web.Http.Request", TraceLevel.Info)
            {
                Kind = TraceKind.End,
                Status = HttpStatusCode.Accepted,
                Message = "EnvelopeMessage",
                Exception = exception
            };

            // Act
            string actualTrace = new SystemDiagnosticsTraceWriter() { IsVerbose = true }.FormatRequestEnvelope(traceRecord);

            // Assert
            string timePrefix = "] ";
            actualTrace = actualTrace.Substring(actualTrace.IndexOf(timePrefix) + timePrefix.Length);
            string expectedTrace = String.Format("Sending response, Status=202 (Accepted), Method=GET, Url=http://localhost/, Id={0}, Message={1}, Exception={2}",
                                                    traceRecord.RequestId.ToString(),
                                                    "'EnvelopeMessage'",
                                                    exception.ToString().Trim());
            Assert.Equal(expectedTrace, actualTrace);
        }

        [Fact]
        public void FormatRequestEnvelope_Verbose_Builds_Correct_End_Trace_With_Null_Fields()
        {
            // Arrange
            TraceRecord traceRecord = new TraceRecord(request: null, category: null, level: TraceLevel.Info)
            {
                Kind = TraceKind.End,
            };

            // Act
            string actualTrace = new SystemDiagnosticsTraceWriter() { IsVerbose = true }.FormatRequestEnvelope(traceRecord);

            // Assert
            string timePrefix = "] ";
            actualTrace = actualTrace.Substring(actualTrace.IndexOf(timePrefix) + timePrefix.Length);
            string expectedTrace = String.Format("Sending response, Id={0}", traceRecord.RequestId.ToString());
            Assert.Equal(expectedTrace, actualTrace);
        }

        [Fact]
        void TranslateHttpResponseException_Throws_With_Null_TraceRecord()
        {
            // Arrange & Act & Assert
            ArgumentNullException exception =
                Assert.Throws<ArgumentNullException>(
                    () => { new SystemDiagnosticsTraceWriter().TranslateHttpResponseException(null); });

            Assert.Equal("traceRecord", exception.ParamName);
        }

        [Fact]
        public void TranslateHttpResponseException_Copies_Status_From_Response()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage
            {
                RequestUri = new Uri("http://localhost"),
                Method = HttpMethod.Get
            };

            TraceRecord traceRecord = new TraceRecord(request, "MyCategory", TraceLevel.Error)
            {
                Exception = new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest))
            };

            // Act
            new SystemDiagnosticsTraceWriter().TranslateHttpResponseException(traceRecord);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, traceRecord.Status);
        }

        [Fact]
        public void TranslateHttpResponseException_Maps_Client_Error_To_Warning()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage
            {
                RequestUri = new Uri("http://localhost"),
                Method = HttpMethod.Get
            };

            TraceRecord traceRecord = new TraceRecord(request, "MyCategory", TraceLevel.Error)
            {
                Exception = new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest))
            };


            // Act
            new SystemDiagnosticsTraceWriter().TranslateHttpResponseException(traceRecord);

            // Assert
            Assert.Equal(TraceLevel.Warn, traceRecord.Level);
        }

        [Fact]
        public void TranslateHttpResponseException_Maps_Non_Errors_To_Info()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage
            {
                RequestUri = new Uri("http://localhost"),
                Method = HttpMethod.Get
            };

            TraceRecord traceRecord = new TraceRecord(request, "MyCategory", TraceLevel.Error)
            {
                Exception = new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Moved))
            };


            // Act
            new SystemDiagnosticsTraceWriter().TranslateHttpResponseException(traceRecord);

            // Assert
            Assert.Equal(TraceLevel.Info, traceRecord.Level);
        }

        [Fact]
        public void TranslateHttpResponseException_Does_Not_Alter_Server_Errors()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage
            {
                RequestUri = new Uri("http://localhost"),
                Method = HttpMethod.Get
            };

            TraceRecord traceRecord = new TraceRecord(request, "MyCategory", TraceLevel.Error)
            {
                Exception = new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError))
            };


            // Act
            new SystemDiagnosticsTraceWriter().TranslateHttpResponseException(traceRecord);

            // Assert
            Assert.Equal(TraceLevel.Error, traceRecord.Level);
        }

        [Fact]
        public void TranslateHttpResponseException_Unpacks_Empty_HttpError()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage
            {
                RequestUri = new Uri("http://localhost"),
                Method = HttpMethod.Get
            };

            HttpError httpError = new HttpError();
            HttpResponseMessage errorResponse = request.CreateResponse(HttpStatusCode.BadRequest);
            errorResponse.Content = new ObjectContent<HttpError>(httpError, new JsonMediaTypeFormatter());

            TraceRecord traceRecord = new TraceRecord(request, "MyCategory", TraceLevel.Error)
            {
                Exception = new HttpResponseException(errorResponse)
            };


            // Act
            new SystemDiagnosticsTraceWriter().TranslateHttpResponseException(traceRecord);

            // Assert
            Assert.Equal(0, ParseTrace(traceRecord.Message).Count);
        }

        [Fact]
        public void TranslateHttpResponseException_Unpacks_HttpError_With_All_Fields_Set()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage
            {
                RequestUri = new Uri("http://localhost"),
                Method = HttpMethod.Get
            };

            Exception exception = null;
            try
            {
                throw new InvalidOperationException("ExpectedExceptionMessage");
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            HttpError httpError = new HttpError(exception, includeErrorDetail: true);
            httpError.Message = "ExpectedUserMessage";
            httpError[MessageDetailKey] = "ExpectedDetailMessage";

            HttpResponseMessage errorResponse = request.CreateResponse(HttpStatusCode.BadRequest);
            errorResponse.Content = new ObjectContent<HttpError>(httpError, new JsonMediaTypeFormatter());

            TraceRecord traceRecord = new TraceRecord(request, "MyCategory", TraceLevel.Error)
            {
                Exception = new HttpResponseException(errorResponse)
            };


            // Act
            new SystemDiagnosticsTraceWriter().TranslateHttpResponseException(traceRecord);

            // Assert
            Assert.Equal(TraceLevel.Warn, traceRecord.Level);

            AssertContainsExactly(traceRecord.Message, new Dictionary<string, string>()
            {
                { "UserMessage", "'ExpectedUserMessage'" },
                { "MessageDetail", "'ExpectedDetailMessage'" },
                { "ExceptionType", "'System.InvalidOperationException'" },
                { "ExceptionMessage", "'ExpectedExceptionMessage'" },
                { "StackTrace", exception.StackTrace.Trim() }
            });
        }

        [Fact]
        public void TranslateHttpResponseException_Unpacks_HttpError_With_Inner_Exceptions()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage
            {
                RequestUri = new Uri("http://localhost"),
                Method = HttpMethod.Get
            };

            Exception exception1 = null;
            Exception exception2 = null;
            Exception exception3 = null;

            try
            {
                throw new NotSupportedException("ExpectedExceptionMessage3");
            }
            catch (Exception ex)
            {
                exception3 = ex;
            }

            try
            {
                throw new NotImplementedException("ExpectedExceptionMessage2", exception3);
            }
            catch (Exception ex)
            {
                exception2 = ex;
            }

            try
            {
                throw new InvalidOperationException("ExpectedExceptionMessage1", exception2);
            }
            catch (Exception ex)
            {
                exception1 = ex;
            }

            HttpError httpError = new HttpError(exception1, includeErrorDetail: true);
            httpError.Message = "ExpectedUserMessage";
            httpError[MessageDetailKey] = "ExpectedDetailMessage";

            HttpResponseMessage errorResponse = request.CreateResponse(HttpStatusCode.BadRequest);
            errorResponse.Content = new ObjectContent<HttpError>(httpError, new JsonMediaTypeFormatter());

            TraceRecord traceRecord = new TraceRecord(request, "System.Web.Http.Request", TraceLevel.Error)
            {
                Exception = new HttpResponseException(errorResponse)
            };


            // Act
            new SystemDiagnosticsTraceWriter().TranslateHttpResponseException(traceRecord);

            // Assert
            Assert.Equal(TraceLevel.Warn, traceRecord.Level);

            AssertContainsExactly(traceRecord.Message, new Dictionary<string, string>()
            {
                { "UserMessage", "'ExpectedUserMessage'" },
                { "MessageDetail", "'ExpectedDetailMessage'" },

                { "ExceptionType", "'System.InvalidOperationException'" },
                { "ExceptionMessage", "'ExpectedExceptionMessage1'" },
                { "StackTrace", exception1.StackTrace.Trim() },

                { "ExceptionType[1]", "'System.NotImplementedException'" },
                { "ExceptionMessage[1]", "'ExpectedExceptionMessage2'" },
                { "StackTrace[1]", exception2.StackTrace.Trim() },

                { "ExceptionType[2]", "'System.NotSupportedException'" },
                { "ExceptionMessage[2]", "'ExpectedExceptionMessage3'" },
                { "StackTrace[2]", exception3.StackTrace.Trim() }

            });
        }

        [Fact]
        public void TranslateHttpResponseException_HiddenInAggregateException_UnpackTheMostSevere()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage
            {
                RequestUri = new Uri("http://localhost"),
                Method = HttpMethod.Get
            };

            var ex500 = new HttpResponseException(HttpStatusCode.InternalServerError);
            var ex503 = new HttpResponseException(HttpStatusCode.ServiceUnavailable);
            var ex401 = new HttpResponseException(HttpStatusCode.Unauthorized);
            var ex503IsWithinMe = new Exception("I have 503 inside me", ex503);

            var aggregate = new AggregateException(ex500, ex503IsWithinMe, ex401);

            HttpError httpError = new HttpError(aggregate, includeErrorDetail: true);
            HttpResponseMessage errorResponse = request.CreateResponse(HttpStatusCode.ServiceUnavailable);
            errorResponse.Content = new ObjectContent<HttpError>(httpError, new JsonMediaTypeFormatter());

            TraceRecord traceRecord = new TraceRecord(request, "System.Web.Http.Request", TraceLevel.Error)
            {
                Exception = new HttpResponseException(errorResponse)
            };

            // Act
            new SystemDiagnosticsTraceWriter().TranslateHttpResponseException(traceRecord);

            // Assert
            Assert.Equal(TraceLevel.Error, traceRecord.Level);
            Assert.Equal(HttpStatusCode.ServiceUnavailable, traceRecord.Status);
        }

        [Fact]
        public void TranslateHttpResponseException_Unpacks_ModelState_Errors()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage
            {
                RequestUri = new Uri("http://localhost"),
                Method = HttpMethod.Get
            };

            ModelStateDictionary modelStateDictionary = new ModelStateDictionary();
            ModelState modelState1 = new ModelState();
            modelState1.Errors.Add("modelState1.Error1");
            modelState1.Errors.Add("modelState1.Error2");
            modelStateDictionary["key1"] = modelState1;

            ModelState modelState2 = new ModelState();
            modelState2.Errors.Add("modelState2.Error1");
            modelState2.Errors.Add("modelState2.Error2");
            modelStateDictionary["key2"] = modelState2;

            HttpError httpError = new HttpError(modelStateDictionary, includeErrorDetail: true);
            HttpResponseMessage errorResponse = request.CreateResponse(HttpStatusCode.BadRequest);
            errorResponse.Content = new ObjectContent<HttpError>(httpError, new JsonMediaTypeFormatter());

            TraceRecord traceRecord = new TraceRecord(request, "System.Web.Http.Request", TraceLevel.Error)
            {
                Exception = new HttpResponseException(errorResponse)
            };

            // Act
            new SystemDiagnosticsTraceWriter().TranslateHttpResponseException(traceRecord);

            // Assert
            Assert.Equal(TraceLevel.Warn, traceRecord.Level);

            string message = ExtractModelStateErrorString(traceRecord.Message);

            AssertContainsExactly(message, new Dictionary<string, string>()
            {
                { "key1", "[modelState1.Error1, modelState1.Error2]" },
                { "key2", "[modelState2.Error1, modelState2.Error2]" },
            });
        }

        [Fact]
        public void TranslateHttpResponseException_Unpacks_HttpError_With_No_Fields_Set()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage
            {
                RequestUri = new Uri("http://localhost"),
                Method = HttpMethod.Get
            };

            Exception exception = null;
            try
            {
                throw new InvalidOperationException("ExpectedExceptionMessage");
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            HttpError httpError = new HttpError();
            HttpResponseMessage errorResponse = request.CreateResponse(HttpStatusCode.BadRequest);
            errorResponse.Content = new ObjectContent<HttpError>(httpError, new JsonMediaTypeFormatter());

            TraceRecord traceRecord = new TraceRecord(request, "System.Web.Http.Request", TraceLevel.Error)
            {
                Exception = new HttpResponseException(errorResponse)
            };

            // Act
            new SystemDiagnosticsTraceWriter().TranslateHttpResponseException(traceRecord);

            // Assert
            Assert.Equal(TraceLevel.Warn, traceRecord.Level);
            Assert.Equal(string.Empty, traceRecord.Message);
        }

        private static void AssertContainsExactly(string trace, IDictionary<string, string> expected)
        {
            IDictionary<string, string> actual = ParseTrace(trace);
            Assert.Equal(expected.Count, actual.Count);
            foreach (string key in expected.Keys)
            {
                Assert.True(actual.ContainsKey(key), String.Format("Missing {0} field", key));
                Assert.Equal(expected[key], actual[key]);
            }
        }

        private static IDictionary<string, string> ParseTrace(string trace)
        {
            Dictionary<string, string> traces = new Dictionary<string, string>();
            string[] splits = trace.Split('=');
            for (int i = 0; i < splits.Length - 1; i++)
            {
                // Line is either "key" or "value key" -- get the key part
                string[] nextSplit = splits[i].Split(' ');
                if (nextSplit.Length < 1)
                {
                    continue;
                }
                string key = nextSplit[nextSplit.Length - 1].Trim();

                string value;

                // Last item containing name takes entire remaining text
                if (i == splits.Length - 2)
                {
                    value = splits[i + 1].Trim();
                }
                else
                {
                    // Next line is "value, nextKey" -- get the value part
                    int lastCommaPos = splits[i + 1].LastIndexOf(", ");
                    if (lastCommaPos < 0)
                    {
                        continue;
                    }

                    value = splits[i + 1].Substring(0, lastCommaPos).Trim();
                }

                traces[key] = value;
            }

            return traces;
        }

        private static string ExtractModelStateErrorString(string message)
        {
            string modelStatePrefix = "ModelStateError=[";
            int modelStatePrefixPos = message.IndexOf(modelStatePrefix);
            Assert.True(modelStatePrefixPos >= 0);
            int lastBracketPos = message.LastIndexOf("]");
            Assert.True(lastBracketPos > modelStatePrefixPos);

            int startPos = modelStatePrefixPos + modelStatePrefix.Length;

            return message.Substring(startPos, lastBracketPos - startPos);
        }

        // Helper to create a new SystemDiagnosticsTraceWriter configured
        // to use a custom TraceSource to write its traces to a TestTraceListener.
        private static SystemDiagnosticsTraceWriter CreateTraceWriter()
        {
            TestTraceListener testTraceListener = new TestTraceListener();
            TraceSource testTraceSource = new TraceSource("TestTraceSource", SourceLevels.All);
            testTraceSource.Listeners.Clear();
            testTraceSource.Listeners.Add(testTraceListener);

            SystemDiagnosticsTraceWriter writer = new SystemDiagnosticsTraceWriter();
            writer.TraceSource = testTraceSource;
            return writer;
        }

        // Test spy used to capture test traces
        class TestTraceListener : TraceListener
        {
            private List<string> _messages = new List<string>();

            public IList<string> Messages { get { return _messages; } }

            public string SourceName { get; set; }

            public int? Id { get; set; }

            public TraceEventType TraceEventType { get; set; }

            public override void Write(string message)
            {
                _messages.Add(message);
            }

            public override void WriteLine(string message)
            {
                Write(message + Environment.NewLine);
            }

            public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
            {
                _messages.Add(message);
                SourceName = source;
                Id = id;
                TraceEventType = eventType;
            }
        }
    }
}
