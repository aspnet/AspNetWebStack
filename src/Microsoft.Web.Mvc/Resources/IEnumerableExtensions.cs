// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Web.Mvc.Resources
{
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Convenience API to allow an IEnumerable{T} (such as returned by Linq2Sql) to be serialized by DataContractSerilizer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static IEnumerable<T> AsSerializable<T>(this IEnumerable<T> collection) where T : class
        {
            return new IEnumerableWrapper<T>(collection);
        }

        // This wrapper allows IEnumerable<T> to be serialized by DataContractSerilizer
        // it implements the minimal amount of surface needed for serialization.
        private class IEnumerableWrapper<T> : IEnumerable<T>
            where T : class
        {
            private IEnumerable<T> _collection;

            // The DataContractSerilizer needs a default constructor to ensure the object can be
            // deserialized. We have a dummy one since we don't actually need deserialization.
            public IEnumerableWrapper()
            {
                throw new NotImplementedException();
            }

            internal IEnumerableWrapper(IEnumerable<T> collection)
            {
                this._collection = collection;
            }

            // The DataContractSerilizer needs an Add method to ensure the object can be
            // deserialized. We have a dummy one since we don't actually need deserialization.
            [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Needed to satisfy the deserialization contract")]
            [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "item", Justification = "Needed to satisfy the deserialization contract")]
            public void Add(T item)
            {
                throw new NotImplementedException();
            }

            public IEnumerator<T> GetEnumerator()
            {
                return this._collection.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable)this._collection).GetEnumerator();
            }
        }
    }
}
