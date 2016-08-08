// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Net.Http.Internal;

namespace System.Net.Http.Mocks
{
    internal class MockNonClosingDelegatingStream : NonClosingDelegatingStream
    {
        public MockNonClosingDelegatingStream(Stream innerStream)
            : base(innerStream)
        {
        }
    }
}
