// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Net.Http.Formatting;

namespace System.Web.Http.Tracing.Tracers
{
    /// <summary>
    /// FormatterLogger to trace an error while logging.
    /// </summary>
    internal class FormatterLoggerTraceWrapper : IFormatterLogger
    {
        private readonly IFormatterLogger _formatterLogger;
        private readonly ITraceWriter _traceWriter;
        private readonly HttpRequestMessage _request;
        private readonly string _operatorName;
        private readonly string _operationName;

        public FormatterLoggerTraceWrapper(IFormatterLogger formatterLogger,
                                           ITraceWriter traceWriter,
                                           HttpRequestMessage request,
                                           string operatorName,
                                           string operationName)
        {
            Contract.Assert(formatterLogger != null);
            Contract.Assert(traceWriter != null);

            _formatterLogger = formatterLogger;
            _traceWriter = traceWriter;
            _request = request;
            _operatorName = operatorName;
            _operationName = operationName;
        }

        public void LogError(string errorPath, string errorMessage)
        {
            _traceWriter.Trace(_request, TraceCategories.FormattingCategory, TraceLevel.Error, (traceRecord) =>
                {
                    traceRecord.Kind = TraceKind.Trace;
                    traceRecord.Operator = _operatorName;
                    traceRecord.Operation = _operationName;
                    traceRecord.Message = errorMessage;
                });
            _formatterLogger.LogError(errorPath, errorMessage);
        }

        public void LogError(string errorPath, Exception exception)
        {
            _traceWriter.Trace(_request, TraceCategories.FormattingCategory, TraceLevel.Error, (traceRecord) =>
                {
                    traceRecord.Kind = TraceKind.Trace;
                    traceRecord.Operator = _operatorName;
                    traceRecord.Operation = _operationName;
                    traceRecord.Exception = exception;
                });
            _formatterLogger.LogError(errorPath, exception);
        }
    }
}
