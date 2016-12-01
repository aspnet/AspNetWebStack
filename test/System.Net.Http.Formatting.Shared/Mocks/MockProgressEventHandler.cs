// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Net.Http.Handlers
{
    public class MockProgressEventHandler
    {
        public bool WasInvoked { get; private set; }

        public object Sender { get; private set; }

        public HttpProgressEventArgs EventArgs { get; private set; }

        public void Handler(object sender, HttpProgressEventArgs eventArgs)
        {
            WasInvoked = true;
            Sender = sender;
            EventArgs = eventArgs;
        }

        public static ProgressMessageHandler CreateProgressMessageHandler(out MockProgressEventHandler progressEventHandler, bool sendProgress)
        {
            ProgressMessageHandler progressHandler = new ProgressMessageHandler();
            progressEventHandler = new MockProgressEventHandler();
            if (sendProgress)
            {
                progressHandler.HttpSendProgress += progressEventHandler.Handler;
            }
            else
            {
                progressHandler.HttpReceiveProgress += progressEventHandler.Handler;
            }
            return progressHandler;
        }
    }
}
