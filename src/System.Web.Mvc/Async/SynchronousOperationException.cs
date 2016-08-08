// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Runtime.Serialization;

namespace System.Web.Mvc.Async
{
    // This exception type is thrown by the SynchronizationContextUtil helper class since the AspNetSynchronizationContext
    // type swallows exceptions. The inner exception contains the data the user cares about.

    [Serializable]
    public sealed class SynchronousOperationException : HttpException
    {
        public SynchronousOperationException()
        {
        }

        private SynchronousOperationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public SynchronousOperationException(string message)
            : base(message)
        {
        }

        public SynchronousOperationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
