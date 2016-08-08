// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Web.Http
{
    internal class EmptyReadOnlyDictionary<TKey, TValue>
    {
        private static readonly ReadOnlyDictionary<TKey, TValue> _value = new ReadOnlyDictionary<TKey, TValue>(new Dictionary<TKey, TValue>());

        public static IDictionary<TKey, TValue> Value
        {
            get { return _value; }
        }        
    }
}
