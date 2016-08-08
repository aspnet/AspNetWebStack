// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Net.Http.Formatting.Mocks
{
    public class MockAsyncCallback
    {
        public bool WasInvoked { get; private set; }

        public IAsyncResult AsyncResult { get; private set; }

        public void Handler(IAsyncResult result)
        {
            WasInvoked = true;
            AsyncResult = result;
        }
    }
}
