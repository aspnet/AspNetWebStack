// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;

namespace System.Web.Mvc
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public sealed class NoAsyncTimeoutAttribute : AsyncTimeoutAttribute
    {
        public NoAsyncTimeoutAttribute()
            : base(Timeout.Infinite)
        {
        }
    }
}
