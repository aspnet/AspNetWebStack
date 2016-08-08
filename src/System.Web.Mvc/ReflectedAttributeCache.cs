// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;

namespace System.Web.Mvc
{
    internal static class ReflectedAttributeCache
    {
        private static readonly ConcurrentDictionary<MethodInfo, ReadOnlyCollection<ActionMethodSelectorAttribute>> _actionMethodSelectorAttributeCache = new ConcurrentDictionary<MethodInfo, ReadOnlyCollection<ActionMethodSelectorAttribute>>();
        private static readonly ConcurrentDictionary<MethodInfo, ReadOnlyCollection<ActionNameSelectorAttribute>> _actionNameSelectorAttributeCache = new ConcurrentDictionary<MethodInfo, ReadOnlyCollection<ActionNameSelectorAttribute>>();
        private static readonly ConcurrentDictionary<MethodInfo, ReadOnlyCollection<FilterAttribute>> _methodFilterAttributeCache = new ConcurrentDictionary<MethodInfo, ReadOnlyCollection<FilterAttribute>>();

        private static readonly ConcurrentDictionary<Type, ReadOnlyCollection<FilterAttribute>> _typeFilterAttributeCache = new ConcurrentDictionary<Type, ReadOnlyCollection<FilterAttribute>>();

        public static ReadOnlyCollection<FilterAttribute> GetTypeFilterAttributes(Type type)
        {
            return GetAttributes(_typeFilterAttributeCache, type);
        }

        public static ReadOnlyCollection<FilterAttribute> GetMethodFilterAttributes(MethodInfo methodInfo)
        {
            return GetAttributes(_methodFilterAttributeCache, methodInfo);
        }

        public static ReadOnlyCollection<ActionMethodSelectorAttribute> GetActionMethodSelectorAttributesCollection(MethodInfo methodInfo)
        {
            return GetAttributes(_actionMethodSelectorAttributeCache, methodInfo);
        }

        public static ReadOnlyCollection<ActionNameSelectorAttribute> GetActionNameSelectorAttributes(MethodInfo methodInfo)
        {
            return GetAttributes(_actionNameSelectorAttributeCache, methodInfo);
        }

        private static ReadOnlyCollection<TAttribute> GetAttributes<TMemberInfo, TAttribute>(ConcurrentDictionary<TMemberInfo, ReadOnlyCollection<TAttribute>> lookup, TMemberInfo memberInfo)
            where TAttribute : Attribute
            where TMemberInfo : MemberInfo
        {
            Debug.Assert(memberInfo != null);
            Debug.Assert(lookup != null);
            // Frequently called, so use a static delegate
            // An inline delegate cannot be used because the C# compiler does not cache inline delegates that reference generic method arguments
            return lookup.GetOrAdd(
                memberInfo,
                CachedDelegates<TMemberInfo, TAttribute>.GetCustomAttributes);
        }

        private static class CachedDelegates<TMemberInfo, TAttribute>
            where TAttribute : Attribute
            where TMemberInfo : MemberInfo
        {
            internal static Func<TMemberInfo, ReadOnlyCollection<TAttribute>> GetCustomAttributes = (TMemberInfo memberInfo) =>
            {
                return new ReadOnlyCollection<TAttribute>((TAttribute[])memberInfo.GetCustomAttributes(typeof(TAttribute), inherit: true));
            };
        }
    }
}
