// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Http.Batch;
using System.Web.Http.Dispatcher;
using System.Web.Http.Filters;

namespace System.Web.Http.ExceptionHandling
{
    /// <summary>Provides the catch blocks used within this assembly.</summary>
    public static class ExceptionCatchBlocks
    {
        private static readonly ExceptionContextCatchBlock _httpBatchHandler =
            new ExceptionContextCatchBlock(typeof(HttpBatchHandler).Name, isTopLevel: false, callsHandler: true);
        private static readonly ExceptionContextCatchBlock _httpControllerDispatcher =
            new ExceptionContextCatchBlock(typeof(HttpControllerDispatcher).Name, isTopLevel: false, callsHandler: true);
        private static readonly ExceptionContextCatchBlock _httpServer =
            new ExceptionContextCatchBlock(typeof(HttpServer).Name, isTopLevel: true, callsHandler: true);
        private static readonly ExceptionContextCatchBlock _exceptionFilter =
            new ExceptionContextCatchBlock(typeof(IExceptionFilter).Name, isTopLevel: false, callsHandler: true);

        /// <summary>Gets the catch block in <see cref="HttpBatchHandler"/>.SendAsync.</summary>
        public static ExceptionContextCatchBlock HttpBatchHandler
        {
            get { return _httpBatchHandler; }
        }

        /// <summary>Gets the catch block in <see cref="HttpControllerDispatcher"/>.SendAsync.</summary>
        public static ExceptionContextCatchBlock HttpControllerDispatcher
        {
            get { return _httpControllerDispatcher; }
        }

        /// <summary>Gets the catch block in <see cref="HttpServer"/>.SendAsync</summary>
        public static ExceptionContextCatchBlock HttpServer
        {
            get { return _httpServer; }
        }

        /// <summary>
        /// Gets the catch block in <see cref="ApiController"/>.ExecuteAsync when using <see cref="IExceptionFilter"/>.
        /// </summary>
        public static ExceptionContextCatchBlock IExceptionFilter
        {
            get { return _exceptionFilter; }
        }
    }
}
