// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Properties;

namespace System.Web.Http.Tracing
{
    /// <summary>
    /// Extension methods for <see cref="ITraceWriter"/>.
    /// </summary>
    public static class ITraceWriterExtensions
    {
        /// <summary>
        /// Writes a <see cref="TraceRecord"/> at <see cref="TraceLevel.Debug"/> with the given message.
        /// </summary>
        /// <param name="traceWriter">The <see cref="ITraceWriter"/></param>
        /// <param name="request">The <see cref="HttpRequestMessage"/> with which to correlate the request.  
        /// It may be null, but if so will not be correlated with any request.</param>
        /// <param name="category">The category for the trace.</param>
        /// <param name="messageFormat">The string to use to format a message.  It may not be null.</param>
        /// <param name="messageArguments">Optional list of arguments for the <paramref name="messageFormat"/>.</param>
        public static void Debug(this ITraceWriter traceWriter, HttpRequestMessage request, string category, string messageFormat, params object[] messageArguments)
        {
            Trace(traceWriter, request, category, TraceLevel.Debug, messageFormat, messageArguments);
        }

        /// <summary>
        /// Writes a <see cref="TraceRecord"/> at <see cref="TraceLevel.Debug"/> with the given <paramref name="exception"/>.
        /// </summary>
        /// <param name="traceWriter">The <see cref="ITraceWriter"/></param>
        /// <param name="request">The <see cref="HttpRequestMessage"/> with which to correlate the request.  
        /// It may be null, but if so will not be correlated with any request.</param>
        /// <param name="category">The category for the trace.</param>
        /// <param name="exception">The exception to trace</param>
        public static void Debug(this ITraceWriter traceWriter, HttpRequestMessage request, string category, Exception exception)
        {
            Trace(traceWriter, request, category, TraceLevel.Debug, exception);
        }

        /// <summary>
        /// Writes a <see cref="TraceRecord"/> at <see cref="TraceLevel.Debug"/> with the given message and exception.
        /// </summary>
        /// <param name="traceWriter">The <see cref="ITraceWriter"/></param>
        /// <param name="request">The <see cref="HttpRequestMessage"/> with which to correlate the request.  
        /// It may be null, but if so will not be correlated with any request.</param>
        /// <param name="category">The category for the trace.</param>
        /// <param name="exception">The exception to trace</param>
        /// <param name="messageFormat">The string to use to format a message.  It may not be null.</param>
        /// <param name="messageArguments">Optional list of arguments for the <paramref name="messageFormat"/>.</param>
        public static void Debug(this ITraceWriter traceWriter, HttpRequestMessage request, string category, Exception exception, string messageFormat, params object[] messageArguments)
        {
            Trace(traceWriter, request, category, TraceLevel.Debug, exception, messageFormat, messageArguments);
        }

        /// <summary>
        /// Writes a <see cref="TraceRecord"/> at <see cref="TraceLevel.Error"/> with the given message.
        /// </summary>
        /// <param name="traceWriter">The <see cref="ITraceWriter"/></param>
        /// <param name="request">The <see cref="HttpRequestMessage"/> with which to correlate the request.  
        /// It may be null, but if so will not be correlated with any request.</param>
        /// <param name="category">The category for the trace.</param>
        /// <param name="messageFormat">The string to use to format a message.  It may not be null.</param>
        /// <param name="messageArguments">Optional list of arguments for the <paramref name="messageFormat"/>.</param>
        public static void Error(this ITraceWriter traceWriter, HttpRequestMessage request, string category, string messageFormat, params object[] messageArguments)
        {
            Trace(traceWriter, request, category, TraceLevel.Error, messageFormat, messageArguments);
        }

        /// <summary>
        /// Writes a <see cref="TraceRecord"/> at <see cref="TraceLevel.Error"/> with the given <paramref name="exception"/>.
        /// </summary>
        /// <param name="traceWriter">The <see cref="ITraceWriter"/></param>
        /// <param name="request">The <see cref="HttpRequestMessage"/> with which to correlate the request.  
        /// It may be null, but if so will not be correlated with any request.</param>
        /// <param name="category">The category for the trace.</param>
        /// <param name="exception">The exception to trace</param>
        public static void Error(this ITraceWriter traceWriter, HttpRequestMessage request, string category, Exception exception)
        {
            Trace(traceWriter, request, category, TraceLevel.Error, exception);
        }

        /// <summary>
        /// Writes a <see cref="TraceRecord"/> at <see cref="TraceLevel.Error"/> with the given message and exception.
        /// </summary>
        /// <param name="traceWriter">The <see cref="ITraceWriter"/></param>
        /// <param name="request">The <see cref="HttpRequestMessage"/> with which to correlate the request.  
        /// It may be null, but if so will not be correlated with any request.</param>
        /// <param name="category">The category for the trace.</param>
        /// <param name="exception">The exception to trace</param>
        /// <param name="messageFormat">The string to use to format a message.  It may not be null.</param>
        /// <param name="messageArguments">Optional list of arguments for the <paramref name="messageFormat"/>.</param>
        public static void Error(this ITraceWriter traceWriter, HttpRequestMessage request, string category, Exception exception, string messageFormat, params object[] messageArguments)
        {
            Trace(traceWriter, request, category, TraceLevel.Error, exception, messageFormat, messageArguments);
        }

        /// <summary>
        /// Writes a <see cref="TraceRecord"/> at <see cref="TraceLevel.Fatal"/> with the given message.
        /// </summary>
        /// <param name="traceWriter">The <see cref="ITraceWriter"/></param>
        /// <param name="request">The <see cref="HttpRequestMessage"/> with which to correlate the request.  
        /// It may be null, but if so will not be correlated with any request.</param>
        /// <param name="category">The category for the trace.</param>
        /// <param name="messageFormat">The string to use to format a message.  It may not be null.</param>
        /// <param name="messageArguments">Optional list of arguments for the <paramref name="messageFormat"/>.</param>
        public static void Fatal(this ITraceWriter traceWriter, HttpRequestMessage request, string category, string messageFormat, params object[] messageArguments)
        {
            Trace(traceWriter, request, category, TraceLevel.Fatal, messageFormat, messageArguments);
        }

        /// <summary>
        /// Writes a <see cref="TraceRecord"/> at <see cref="TraceLevel.Fatal"/> with the given <paramref name="exception"/>.
        /// </summary>
        /// <param name="traceWriter">The <see cref="ITraceWriter"/></param>
        /// <param name="request">The <see cref="HttpRequestMessage"/> with which to correlate the request.  
        /// It may be null, but if so will not be correlated with any request.</param>
        /// <param name="category">The category for the trace.</param>
        /// <param name="exception">The exception to trace</param>
        public static void Fatal(this ITraceWriter traceWriter, HttpRequestMessage request, string category, Exception exception)
        {
            Trace(traceWriter, request, category, TraceLevel.Fatal, exception);
        }

        /// <summary>
        /// Writes a <see cref="TraceRecord"/> at <see cref="TraceLevel.Fatal"/> with the given message and exception.
        /// </summary>
        /// <param name="traceWriter">The <see cref="ITraceWriter"/></param>
        /// <param name="request">The <see cref="HttpRequestMessage"/> with which to correlate the request.  
        /// It may be null, but if so will not be correlated with any request.</param>
        /// <param name="category">The category for the trace.</param>
        /// <param name="exception">The exception to trace</param>
        /// <param name="messageFormat">The string to use to format a message.  It may not be null.</param>
        /// <param name="messageArguments">Optional list of arguments for the <paramref name="messageFormat"/>.</param>
        public static void Fatal(this ITraceWriter traceWriter, HttpRequestMessage request, string category, Exception exception, string messageFormat, params object[] messageArguments)
        {
            Trace(traceWriter, request, category, TraceLevel.Fatal, exception, messageFormat, messageArguments);
        }

        /// <summary>
        /// Writes a <see cref="TraceRecord"/> at <see cref="TraceLevel.Info"/> with the given message.
        /// </summary>
        /// <param name="traceWriter">The <see cref="ITraceWriter"/></param>
        /// <param name="request">The <see cref="HttpRequestMessage"/> with which to correlate the request.  
        /// It may be null, but if so will not be correlated with any request.</param>
        /// <param name="category">The category for the trace.</param>
        /// <param name="messageFormat">The string to use to format a message.  It may not be null.</param>
        /// <param name="messageArguments">Optional list of arguments for the <paramref name="messageFormat"/>.</param>
        public static void Info(this ITraceWriter traceWriter, HttpRequestMessage request, string category, string messageFormat, params object[] messageArguments)
        {
            Trace(traceWriter, request, category, TraceLevel.Info, messageFormat, messageArguments);
        }

        /// <summary>
        /// Writes a <see cref="TraceRecord"/> at <see cref="TraceLevel.Info"/> with the given <paramref name="exception"/>.
        /// </summary>
        /// <param name="traceWriter">The <see cref="ITraceWriter"/></param>
        /// <param name="request">The <see cref="HttpRequestMessage"/> with which to correlate the request.  
        /// It may be null, but if so will not be correlated with any request.</param>
        /// <param name="category">The category for the trace.</param>
        /// <param name="exception">The exception to trace</param>
        public static void Info(this ITraceWriter traceWriter, HttpRequestMessage request, string category, Exception exception)
        {
            Trace(traceWriter, request, category, TraceLevel.Info, exception);
        }

        /// <summary>
        /// Writes a <see cref="TraceRecord"/> at <see cref="TraceLevel.Info"/> with the given message and exception.
        /// </summary>
        /// <param name="traceWriter">The <see cref="ITraceWriter"/></param>
        /// <param name="request">The <see cref="HttpRequestMessage"/> with which to correlate the request.  
        /// It may be null, but if so will not be correlated with any request.</param>
        /// <param name="category">The category for the trace.</param>
        /// <param name="exception">The exception to trace</param>
        /// <param name="messageFormat">The string to use to format a message.  It may not be null.</param>
        /// <param name="messageArguments">Optional list of arguments for the <paramref name="messageFormat"/>.</param>
        public static void Info(this ITraceWriter traceWriter, HttpRequestMessage request, string category, Exception exception, string messageFormat, params object[] messageArguments)
        {
            Trace(traceWriter, request, category, TraceLevel.Info, exception, messageFormat, messageArguments);
        }

        /// <summary>
        /// Writes a single <see cref="TraceRecord"/> to the given <see cref="ITraceWriter"/> if the trace writer
        /// is enabled for the given <paramref name="category"/> and <paramref name="level"/>.
        /// </summary>
        /// <param name="traceWriter">The <see cref="ITraceWriter"/></param>
        /// <param name="request">The <see cref="HttpRequestMessage"/> with which to correlate the request.  
        /// It may be null, but if so cannot be correlated with any request.</param>
        /// <param name="category">The category for the trace.</param>
        /// <param name="level">The <see cref="TraceLevel"/> for the trace.</param>
        /// <param name="exception">The <see cref="Exception"/> to trace.   It may not be null.</param>
        public static void Trace(this ITraceWriter traceWriter, HttpRequestMessage request, string category, TraceLevel level, Exception exception)
        {
            if (traceWriter == null)
            {
                throw System.Web.Http.Error.ArgumentNull("traceWriter");
            }

            if (exception == null)
            {
                throw System.Web.Http.Error.ArgumentNull("exception");
            }

            traceWriter.Trace(
                request,
                category,
                level,
                (TraceRecord traceRecord) =>
                {
                    traceRecord.Exception = exception;
                });
        }

        /// <summary>
        /// Writes a single <see cref="TraceRecord"/> to the given <see cref="ITraceWriter"/> if the trace writer
        /// is enabled for the given <paramref name="category"/> and <paramref name="level"/>.
        /// </summary>
        /// <param name="traceWriter">The <see cref="ITraceWriter"/></param>
        /// <param name="request">The <see cref="HttpRequestMessage"/> with which to correlate the request.  
        /// It may be null, but if so will not be correlated with any request.</param>
        /// <param name="category">The category for the trace.</param>
        /// <param name="level">The <see cref="TraceLevel"/> for the trace.</param>
        /// <param name="exception">The <see cref="Exception"/> to trace.   It may not be null.</param>
        /// <param name="messageFormat">The string to use to format a message.  It may not be null.</param>
        /// <param name="messageArguments">Optional list of arguments for the <paramref name="messageFormat"/>.</param>
        public static void Trace(this ITraceWriter traceWriter, HttpRequestMessage request, string category, TraceLevel level, Exception exception, string messageFormat, params object[] messageArguments)
        {
            if (traceWriter == null)
            {
                throw System.Web.Http.Error.ArgumentNull("traceWriter");
            }

            if (exception == null)
            {
                throw System.Web.Http.Error.ArgumentNull("exception");
            }

            if (messageFormat == null)
            {
                throw System.Web.Http.Error.ArgumentNull("messageFormat");
            }

            traceWriter.Trace(
                request,
                category,
                level,
                (TraceRecord traceRecord) =>
                {
                    traceRecord.Exception = exception;
                    traceRecord.Message = System.Web.Http.Error.Format(messageFormat, messageArguments);
                });
        }

        /// <summary>
        /// Writes a single <see cref="TraceRecord"/> to the given <see cref="ITraceWriter"/> if the trace writer
        /// is enabled for the given <paramref name="category"/> and <paramref name="level"/>.
        /// </summary>
        /// <param name="traceWriter">The <see cref="ITraceWriter"/></param>
        /// <param name="request">The <see cref="HttpRequestMessage"/> with which to correlate the request.  
        /// It may be null, but if so will not be correlated with any request.</param>
        /// <param name="category">The category for the trace.</param>
        /// <param name="level">The <see cref="TraceLevel"/> for the trace.</param>
        /// <param name="messageFormat">The string to use to format a message.  It may not be null.</param>
        /// <param name="messageArguments">Optional list of arguments for the <paramref name="messageFormat"/>.</param>
        public static void Trace(this ITraceWriter traceWriter, HttpRequestMessage request, string category, TraceLevel level, string messageFormat, params object[] messageArguments)
        {
            if (traceWriter == null)
            {
                throw System.Web.Http.Error.ArgumentNull("traceWriter");
            }

            if (messageFormat == null)
            {
                throw System.Web.Http.Error.ArgumentNull("messageFormat");
            }

            traceWriter.Trace(
                request,
                category,
                level,
                (TraceRecord traceRecord) =>
                {
                    traceRecord.Message = System.Web.Http.Error.Format(messageFormat, messageArguments);
                });
        }

        /// <summary>
        /// Traces both a begin and an end trace around a specified operation.
        /// </summary>
        /// <param name="traceWriter">The <see cref="ITraceWriter"/>.</param>
        /// <param name="request">The <see cref="HttpRequestMessage"/> with which to associate the trace.  It may be null.</param>
        /// <param name="category">The logical category of the trace.</param>
        /// <param name="level">The <see cref="TraceLevel"/> of the trace.</param>
        /// <param name="operatorName">The name of the object performing the operation.  It may be null.</param>
        /// <param name="operationName">The name of the operation being performed.  It may be null.</param>
        /// <param name="beginTrace">The <see cref="Action"/> to invoke prior to performing the operation, 
        /// allowing the given <see cref="TraceRecord"/> to be filled in.  It may be null.</param>
        /// <param name="execute">An <see cref="Action"/> that performs the operation.</param>
        /// <param name="endTrace">The <see cref="Action"/> to invoke after successfully performing the operation, 
        /// allowing the given <see cref="TraceRecord"/> to be filled in.  It may be null.</param>
        /// <param name="errorTrace">The <see cref="Action"/> to invoke if an error was encountered performing the operation, 
        /// allowing the given <see cref="TraceRecord"/> to be filled in.  It may be null.</param>
        public static void TraceBeginEnd(this ITraceWriter traceWriter,
                                        HttpRequestMessage request,
                                        string category,
                                        TraceLevel level,
                                        string operatorName,
                                        string operationName,
                                        Action<TraceRecord> beginTrace,
                                        Action execute,
                                        Action<TraceRecord> endTrace,
                                        Action<TraceRecord> errorTrace)
        {
            if (traceWriter == null)
            {
                throw System.Web.Http.Error.ArgumentNull("traceWriter");
            }

            if (execute == null)
            {
                throw System.Web.Http.Error.ArgumentNull("execute");
            }

            traceWriter.Trace(
                request,
                category,
                level,
                (TraceRecord traceRecord) =>
                {
                    traceRecord.Kind = TraceKind.Begin;
                    traceRecord.Operator = operatorName;
                    traceRecord.Operation = operationName;
                    if (beginTrace != null)
                    {
                        beginTrace(traceRecord);
                    }
                });
            try
            {
                execute();

                traceWriter.Trace(
                    request,
                    category,
                    level,
                    (TraceRecord traceRecord) =>
                    {
                        traceRecord.Kind = TraceKind.End;
                        traceRecord.Operator = operatorName;
                        traceRecord.Operation = operationName;
                        if (endTrace != null)
                        {
                            endTrace(traceRecord);
                        }
                    });
            }
            catch (Exception exception)
            {
                traceWriter.TraceError(exception, request, category, operatorName, operationName, errorTrace);
                throw;
            }
        }

        /// <summary>
        /// Traces both a begin and an end trace around a specified asynchronous operation.
        /// </summary>
        /// <remarks>The end trace will occur when the asynchronous operation completes, either success or failure.</remarks>
        /// <typeparam name="TResult">The type of result produced by the <see cref="Task"/>.</typeparam>
        /// /// <param name="traceWriter">The <see cref="ITraceWriter"/>.</param>
        /// <param name="request">The <see cref="HttpRequestMessage"/> with which to associate the trace.  It may be null.</param>
        /// <param name="category">The logical category of the trace.</param>
        /// <param name="level">The <see cref="TraceLevel"/> of the trace.</param>
        /// <param name="operatorName">The name of the object performing the operation.  It may be null.</param>
        /// <param name="operationName">The name of the operation being performed.  It may be null.</param>
        /// <param name="beginTrace">The <see cref="Action"/> to invoke prior to performing the operation, 
        /// allowing the given <see cref="TraceRecord"/> to be filled in.  It may be null.</param>
        /// <param name="execute">An <see cref="Func{Task}"/> that returns the <see cref="Task"/> that will perform the operation.</param>
        /// <param name="endTrace">The <see cref="Action"/> to invoke after successfully performing the operation, 
        /// allowing the given <see cref="TraceRecord"/> to be filled in.  The result of the completed task will also
        /// be passed to this action.  This action may be null.</param>
        /// <param name="errorTrace">The <see cref="Action"/> to invoke if an error was encountered performing the operation, 
        /// allowing the given <see cref="TraceRecord"/> to be filled in.  It may be null.</param>
        /// <returns>The <see cref="Task"/> returned by the operation.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generic required for this method.")]
        public static Task<TResult> TraceBeginEndAsync<TResult>(this ITraceWriter traceWriter,
                                HttpRequestMessage request,
                                string category,
                                TraceLevel level,
                                string operatorName,
                                string operationName,
                                Action<TraceRecord> beginTrace,
                                Func<Task<TResult>> execute,
                                Action<TraceRecord, TResult> endTrace,
                                Action<TraceRecord> errorTrace)
        {
            if (traceWriter == null)
            {
                throw System.Web.Http.Error.ArgumentNull("traceWriter");
            }

            if (execute == null)
            {
                throw System.Web.Http.Error.ArgumentNull("execute");
            }

            traceWriter.Trace(
                request,
                category,
                level,
                (TraceRecord traceRecord) =>
                {
                    traceRecord.Kind = TraceKind.Begin;
                    traceRecord.Operator = operatorName;
                    traceRecord.Operation = operationName;
                    if (beginTrace != null)
                    {
                        beginTrace(traceRecord);
                    }
                });
            try
            {
                Task<TResult> task = execute();

                // If the operation returned a null Task, we cannot do any
                // further tracing.  Return that null to the caller as
                // would happen if tracing were not enabled.
                if (task == null)
                {
                    return task;
                }

                return traceWriter.TraceBeginEndAsyncCore<TResult>(request, category, level, operatorName, operationName, endTrace, errorTrace, task);
            }
            catch (Exception exception)
            {
                traceWriter.TraceError(exception, request, category, operatorName, operationName, errorTrace);
                throw;
            }
        }

        private static async Task<TResult> TraceBeginEndAsyncCore<TResult>(this ITraceWriter traceWriter,
            HttpRequestMessage request,
            string category, TraceLevel level,
            string operatorName,
            string operationName,
            Action<TraceRecord, TResult> endTrace,
            Action<TraceRecord> errorTrace,
            Task<TResult> task)
        {
            try
            {
                TResult result = await task;
                traceWriter.Trace(
                    request,
                    category,
                    level,
                    (TraceRecord traceRecord) =>
                    {
                        traceRecord.Kind = TraceKind.End;
                        traceRecord.Operator = operatorName;
                        traceRecord.Operation = operationName;
                        if (endTrace != null)
                        {
                            endTrace(traceRecord, result);
                        }
                    });

                return result;
            }
            catch (OperationCanceledException)
            {
                traceWriter.Trace(
                        request,
                        category,
                        TraceLevel.Warn,
                        (TraceRecord traceRecord) =>
                        {
                            traceRecord.Kind = TraceKind.End;
                            traceRecord.Operator = operatorName;
                            traceRecord.Operation = operationName;
                            traceRecord.Message = SRResources.TraceCancelledMessage;
                            if (errorTrace != null)
                            {
                                errorTrace(traceRecord);
                            }
                        });

                throw;
            }
            catch (Exception exception)
            {
                traceWriter.TraceError(exception, request, category, operatorName, operationName, errorTrace);
                throw;
            }
        }

        /// <summary>
        /// Traces both a begin and an end trace around a specified asynchronous operation.
        /// </summary>
        /// <remarks>The end trace will occur when the asynchronous operation completes, either success or failure.</remarks>
        /// <param name="traceWriter">The <see cref="ITraceWriter"/>.</param>
        /// <param name="request">The <see cref="HttpRequestMessage"/> with which to associate the trace.  It may be null.</param>
        /// <param name="category">The logical category of the trace.</param>
        /// <param name="level">The <see cref="TraceLevel"/> of the trace.</param>
        /// <param name="operatorName">The name of the object performing the operation.  It may be null.</param>
        /// <param name="operationName">The name of the operation being performed.  It may be null.</param>
        /// <param name="beginTrace">The <see cref="Action"/> to invoke prior to performing the operation, 
        /// allowing the given <see cref="TraceRecord"/> to be filled in.  It may be null.</param>
        /// <param name="execute">An <see cref="Func{Task}"/> that returns the <see cref="Task"/> that will perform the operation.</param>
        /// <param name="endTrace">The <see cref="Action"/> to invoke after successfully performing the operation, 
        /// allowing the given <see cref="TraceRecord"/> to be filled in.  It may be null.</param>
        /// <param name="errorTrace">The <see cref="Action"/> to invoke if an error was encountered performing the operation, 
        /// allowing the given <see cref="TraceRecord"/> to be filled in.  It may be null.</param>
        /// <returns>The <see cref="Task"/> returned by the operation.</returns>
        public static Task TraceBeginEndAsync(this ITraceWriter traceWriter,
                                HttpRequestMessage request,
                                string category,
                                TraceLevel level,
                                string operatorName,
                                string operationName,
                                Action<TraceRecord> beginTrace,
                                Func<Task> execute,
                                Action<TraceRecord> endTrace,
                                Action<TraceRecord> errorTrace)
        {
            if (traceWriter == null)
            {
                throw System.Web.Http.Error.ArgumentNull("traceWriter");
            }

            if (execute == null)
            {
                throw System.Web.Http.Error.ArgumentNull("execute");
            }

            traceWriter.Trace(
                request,
                category,
                level,
                (TraceRecord traceRecord) =>
                {
                    traceRecord.Kind = TraceKind.Begin;
                    traceRecord.Operator = operatorName;
                    traceRecord.Operation = operationName;
                    if (beginTrace != null)
                    {
                        beginTrace(traceRecord);
                    }
                });
            try
            {
                Task task = execute();

                // If the operation returned a null Task, we cannot do any
                // further tracing.  Return that null to the caller as
                // would happen if tracing were not enabled.
                if (task == null)
                {
                    return task;
                }

                return traceWriter.TraceBeginEndAsyncCore(request, category, level, operatorName, operationName, endTrace, errorTrace, task);
            }
            catch (Exception exception)
            {
                traceWriter.TraceError(exception, request, category, operatorName, operationName, errorTrace);
                throw;
            }
        }

        private static async Task TraceBeginEndAsyncCore(this ITraceWriter traceWriter,
            HttpRequestMessage request,
            string category,
            TraceLevel level,
            string operatorName,
            string operationName,
            Action<TraceRecord> endTrace,
            Action<TraceRecord> errorTrace,
            Task task)
        {
            try
            {
                await task;
                traceWriter.Trace(
                        request,
                        category,
                        level,
                        (TraceRecord traceRecord) =>
                        {
                            traceRecord.Kind = TraceKind.End;
                            traceRecord.Operator = operatorName;
                            traceRecord.Operation = operationName;
                            if (endTrace != null)
                            {
                                endTrace(traceRecord);
                            }
                        });
            }
            catch (OperationCanceledException)
            {
                traceWriter.Trace(
                        request,
                        category,
                        TraceLevel.Warn,
                        (TraceRecord traceRecord) =>
                        {
                            traceRecord.Kind = TraceKind.End;
                            traceRecord.Operator = operatorName;
                            traceRecord.Operation = operationName;
                            traceRecord.Message = SRResources.TraceCancelledMessage;
                            if (errorTrace != null)
                            {
                                errorTrace(traceRecord);
                            }
                        });

                throw;
            }
            catch (Exception exception)
            {
                traceWriter.TraceError(exception, request, category, operatorName, operationName, errorTrace);
                throw;
            }
        }

        /// <summary>
        /// Writes a <see cref="TraceRecord"/> at <see cref="TraceLevel.Warn"/> with the given message.
        /// </summary>
        /// <param name="traceWriter">The <see cref="ITraceWriter"/></param>
        /// <param name="request">The <see cref="HttpRequestMessage"/> with which to correlate the request.  
        /// It may be null, but if so will not be correlated with any request.</param>
        /// <param name="category">The category for the trace.</param>
        /// <param name="messageFormat">The string to use to format a message.  It may not be null.</param>
        /// <param name="messageArguments">Optional list of arguments for the <paramref name="messageFormat"/>.</param>
        public static void Warn(this ITraceWriter traceWriter, HttpRequestMessage request, string category, string messageFormat, params object[] messageArguments)
        {
            Trace(traceWriter, request, category, TraceLevel.Warn, messageFormat, messageArguments);
        }

        /// <summary>
        /// Writes a <see cref="TraceRecord"/> at <see cref="TraceLevel.Warn"/> with the given <paramref name="exception"/>.
        /// </summary>
        /// <param name="traceWriter">The <see cref="ITraceWriter"/></param>
        /// <param name="request">The <see cref="HttpRequestMessage"/> with which to correlate the request.  
        /// It may be null, but if so will not be correlated with any request.</param>
        /// <param name="category">The category for the trace.</param>
        /// <param name="exception">The exception to trace</param>
        public static void Warn(this ITraceWriter traceWriter, HttpRequestMessage request, string category, Exception exception)
        {
            Trace(traceWriter, request, category, TraceLevel.Warn, exception);
        }

        /// <summary>
        /// Writes a <see cref="TraceRecord"/> at <see cref="TraceLevel.Warn"/> with the given message and exception.
        /// </summary>
        /// <param name="traceWriter">The <see cref="ITraceWriter"/></param>
        /// <param name="request">The <see cref="HttpRequestMessage"/> with which to correlate the request.  
        /// It may be null, but if so will not be correlated with any request.</param>
        /// <param name="category">The category for the trace.</param>
        /// <param name="exception">The exception to trace</param>
        /// <param name="messageFormat">The string to use to format a message.  It may not be null.</param>
        /// <param name="messageArguments">Optional list of arguments for the <paramref name="messageFormat"/>.</param>
        public static void Warn(this ITraceWriter traceWriter, HttpRequestMessage request, string category, Exception exception, string messageFormat, params object[] messageArguments)
        {
            Trace(traceWriter, request, category, TraceLevel.Warn, exception, messageFormat, messageArguments);
        }

        private static void TraceError(this ITraceWriter traceWriter, Exception exception, HttpRequestMessage request,
            string category, string operatorName, string operationName, Action<TraceRecord> errorTrace)
        {
            TraceLevel traceLevel = TraceWriterExceptionMapper.GetMappedTraceLevel(exception) ?? TraceLevel.Error;
            traceWriter.Trace(request, category, traceLevel, (traceRecord) =>
                {
                    traceRecord.Kind = TraceKind.End;
                    traceRecord.Operator = operatorName;
                    traceRecord.Operation = operationName;
                    traceRecord.Exception = exception;
                    TraceWriterExceptionMapper.TranslateHttpResponseException(traceRecord);
                    if (errorTrace != null)
                    {
                        errorTrace(traceRecord);
                    }
                });
        }
    }
}
