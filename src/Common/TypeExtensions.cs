// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;

namespace System
{
    /// <summary>
    /// Extension methods for <see cref="Type"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class TypeExtensions
    {
        public static bool IsNullable(this Type type)
        {
            if (type.IsValueType)
            {
                // value types are only nullable if they are Nullable<T>
                return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
            }
            else
            {
                // reference types are always nullable
                return true;
            }
        }
    }
}
