// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Http.Tracing
{
    /// <summary>
    /// Interface to initialize the tracing layer.
    /// </summary>
    /// <remarks>
    /// This is an extensibility interface that may be inserted into
    /// <see cref="HttpConfiguration.Services"/> to provide a replacement for the
    /// entire tracing layer.
    /// </remarks>
    public interface ITraceManager
    {
        void Initialize(HttpConfiguration configuration);
    }
}
