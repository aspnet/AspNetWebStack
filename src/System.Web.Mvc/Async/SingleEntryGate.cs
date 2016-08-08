// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;

namespace System.Web.Mvc.Async
{
    // used to synchronize access to a single-use consumable resource
    internal sealed class SingleEntryGate
    {
        private const int NotEntered = 0;
        private const int Entered = 1;

        private int _status;

        // returns true if this is the first call to TryEnter(), false otherwise
        public bool TryEnter()
        {
            int oldStatus = Interlocked.Exchange(ref _status, Entered);
            return (oldStatus == NotEntered);
        }
    }
}
