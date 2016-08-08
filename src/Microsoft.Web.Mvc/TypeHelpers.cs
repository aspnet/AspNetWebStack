// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;

namespace Microsoft.Web.Mvc
{
    internal static class TypeHelpers
    {
        public static Type ExtractGenericInterface(Type queryType, Type interfaceType)
        {
            Func<Type, bool> matchesInterface = t => t.IsGenericType && t.GetGenericTypeDefinition() == interfaceType;
            return (matchesInterface(queryType)) ? queryType : queryType.GetInterfaces().FirstOrDefault(matchesInterface);
        }

        public static Type[] GetTypeArgumentsIfMatch(Type closedType, Type matchingOpenType)
        {
            if (!closedType.IsGenericType)
            {
                return null;
            }

            Type openType = closedType.GetGenericTypeDefinition();
            return (matchingOpenType == openType) ? closedType.GetGenericArguments() : null;
        }

        public static bool IsCompatibleObject(Type type, object value)
        {
            return ((value == null && TypeAllowsNullValue(type)) || type.IsInstanceOfType(value));
        }

        public static bool IsNullableValueType(Type type)
        {
            return Nullable.GetUnderlyingType(type) != null;
        }

        public static bool TypeAllowsNullValue(Type type)
        {
            return (!type.IsValueType || IsNullableValueType(type));
        }
    }
}
