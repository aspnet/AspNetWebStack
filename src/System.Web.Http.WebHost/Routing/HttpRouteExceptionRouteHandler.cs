// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Runtime.ExceptionServices;
using System.Web.Routing;

namespace System.Web.Http.WebHost.Routing
{
    /// <summary>Represents a route handler that asynchronously handles an unhandled exception from routing.</summary>
    internal class HttpRouteExceptionRouteHandler : IRouteHandler
    {
        private readonly ExceptionDispatchInfo _exceptionInfo;

        public HttpRouteExceptionRouteHandler(ExceptionDispatchInfo exceptionInfo)
        {
            Contract.Assert(exceptionInfo != null);
            _exceptionInfo = exceptionInfo;
        }

        internal ExceptionDispatchInfo ExceptionInfo
        {
            get { return _exceptionInfo; }
        }

        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            return new HttpRouteExceptionHandler(_exceptionInfo);
        }
    }
}
