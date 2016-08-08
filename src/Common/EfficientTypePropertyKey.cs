// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Http
{
    // When the key is cached, it will be more efficient that a normal tuple, because the hash code call
    // is rather expensive particularly for T as Type or T as long string.
    internal class EfficientTypePropertyKey<T1, T2> : Tuple<T1, T2>
    {
        private int _hashCode;

        public EfficientTypePropertyKey(T1 item1, T2 item2)
            : base(item1, item2)
        {
            _hashCode = base.GetHashCode();
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }
    }
}
