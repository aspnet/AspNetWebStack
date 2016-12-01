// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Moq;

namespace System.Net.Http.Formatting.Mocks
{
    public class MockCompletedAsyncResult
    {
        private MockCompletedAsyncResult()
        {
        }

        public static IAsyncResult Create(bool completedSynchronously, object userState)
        {
            Mock<IAsyncResult> mockIAsyncResult = new Mock<IAsyncResult>();
            mockIAsyncResult.Setup(ar => ar.AsyncState).Returns(userState);
            mockIAsyncResult.Setup(ar => ar.IsCompleted).Returns(true);
            mockIAsyncResult.Setup(ar => ar.CompletedSynchronously).Returns(completedSynchronously);
            return mockIAsyncResult.Object;
        }
    }
}
