// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Linq;
using System.Web.Mvc.Properties;

namespace System.Web.Mvc
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public abstract class FilterAttribute : Attribute, IMvcFilter
    {
        private static readonly ConcurrentDictionary<Type, bool> _multiuseAttributeCache = new ConcurrentDictionary<Type, bool>();
        private int _order = Filter.DefaultOrder;

        public bool AllowMultiple
        {
            get { return AllowsMultiple(GetType()); }
        }

        public int Order
        {
            get { return _order; }
            set
            {
                if (value < Filter.DefaultOrder)
                {
                    throw new ArgumentOutOfRangeException("value", MvcResources.FilterAttribute_OrderOutOfRange);
                }
                _order = value;
            }
        }

        private static bool AllowsMultiple(Type attributeType)
        {
            return _multiuseAttributeCache.GetOrAdd(
                attributeType,
                type => type.GetCustomAttributes(typeof(AttributeUsageAttribute), true)
                            .Cast<AttributeUsageAttribute>()
                            .First()
                            .AllowMultiple);
        }
    }
}
