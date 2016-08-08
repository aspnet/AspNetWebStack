// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;

namespace System.Web.Http.SelfHost.ServiceModel.Channels
{
    //An AsyncResult that completes as soon as it is instantiated.
    internal class CompletedAsyncResult : AsyncResult
    {
        public CompletedAsyncResult(AsyncCallback callback, object state)
            : base(callback, state)
        {
            Complete(true);
        }

        public static void End(IAsyncResult result)
        {
            Contract.Assert(result != null, "CompletedAsyncResult was null.");
            Contract.Assert(result.IsCompleted, "CompletedAsyncResult was not completed!");
            AsyncResult.End<CompletedAsyncResult>(result);
        }
    }
}
