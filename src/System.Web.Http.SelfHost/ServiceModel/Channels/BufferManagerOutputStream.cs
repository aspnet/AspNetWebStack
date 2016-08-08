// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ServiceModel;
using System.ServiceModel.Channels;

namespace System.Web.Http.SelfHost.ServiceModel.Channels
{
    internal class BufferManagerOutputStream : BufferedOutputStream
    {
        private readonly string _quotaExceededString;

        public BufferManagerOutputStream(string quotaExceededString, int initialSize, int maxSize, BufferManager bufferManager)
            : base(initialSize, maxSize, bufferManager)
        {
            _quotaExceededString = quotaExceededString;
        }

        public void Init(int initialSize, int maxSizeQuota, BufferManager bufferManager)
        {
            Init(initialSize, maxSizeQuota, maxSizeQuota, bufferManager);
        }

        public void Init(int initialSize, int maxSizeQuota, int effectiveMaxSize, BufferManager bufferManager)
        {
            Reinitialize(initialSize, maxSizeQuota, effectiveMaxSize, bufferManager);
        }

        protected override Exception CreateQuotaExceededException(int maxSizeQuota)
        {
            string excMsg = Error.Format(_quotaExceededString, maxSizeQuota);
            return new QuotaExceededException(excMsg);
        }
    }
}
