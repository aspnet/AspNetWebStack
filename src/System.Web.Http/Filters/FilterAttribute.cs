// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web.Http.Internal;

namespace System.Web.Http.Filters
{
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "We want to allow inheritance")]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public abstract class FilterAttribute : Attribute, IFilter
    {
        private static readonly ConcurrentDictionary<Type, bool> _attributeUsageCache = new ConcurrentDictionary<Type, bool>();

        public virtual bool AllowMultiple
        {
            get { return AllowsMultiple(GetType()); }
        }

        private static bool AllowsMultiple(Type attributeType)
        {
            return _attributeUsageCache.GetOrAdd(
                attributeType,
                type => type.GetCustomAttributes<AttributeUsageAttribute>(inherit: true).First().AllowMultiple);
        }
    }
}
