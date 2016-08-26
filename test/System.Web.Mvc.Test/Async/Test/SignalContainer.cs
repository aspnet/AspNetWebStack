// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;

namespace System.Web.Mvc.Async.Test
{
    public sealed class SignalContainer<T>: IDisposable
    {
        private volatile object _item;
        private readonly AutoResetEvent _waitHandle = new AutoResetEvent(false /* initialState */);

        public void Signal(T item)
        {
            _item = item;
            _waitHandle.Set();
        }

        public T Wait()
        {
            _waitHandle.WaitOne();
            return (T)_item;
        }

        public void Dispose()
        {
            _waitHandle.Dispose();
        }
    }
}
