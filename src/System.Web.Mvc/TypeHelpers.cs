// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Web.Mvc.Properties;

namespace System.Web.Mvc
{
    internal static class TypeHelpers
    {
        private static readonly Dictionary<Type, TryGetValueDelegate> _tryGetValueDelegateCache = new Dictionary<Type, TryGetValueDelegate>();
        private static readonly ReaderWriterLockSlim _tryGetValueDelegateCacheLock = new ReaderWriterLockSlim();

        private static readonly MethodInfo _strongTryGetValueImplInfo = typeof(TypeHelpers).GetMethod("StrongTryGetValueImpl", BindingFlags.NonPublic | BindingFlags.Static);

        public static readonly Assembly MsCorLibAssembly = typeof(string).Assembly;
        public static readonly Assembly MvcAssembly = typeof(Controller).Assembly;
        public static readonly Assembly SystemWebAssembly = typeof(HttpContext).Assembly;

        // method is used primarily for lighting up new .NET Framework features even if MVC targets the previous version
        // thisParameter is the 'this' parameter if target method is instance method, should be null for static method
        public static TDelegate CreateDelegate<TDelegate>(Assembly assembly, string typeName, string methodName, object thisParameter) where TDelegate : class
        {
            // ensure target type exists
            Type targetType = assembly.GetType(typeName, false /* throwOnError */);
            if (targetType == null)
            {
                return null;
            }

            return CreateDelegate<TDelegate>(targetType, methodName, thisParameter);
        }

        public static TDelegate CreateDelegate<TDelegate>(Type targetType, string methodName, object thisParameter) where TDelegate : class
        {
            // ensure target method exists
            ParameterInfo[] delegateParameters = typeof(TDelegate).GetMethod("Invoke").GetParameters();
            Type[] argumentTypes = Array.ConvertAll(delegateParameters, pInfo => pInfo.ParameterType);
            MethodInfo targetMethod = targetType.GetMethod(methodName, argumentTypes);
            if (targetMethod == null)
            {
                return null;
            }

            TDelegate d = Delegate.CreateDelegate(typeof(TDelegate), thisParameter, targetMethod, false /* throwOnBindFailure */) as TDelegate;
            return d;
        }

        public static TryGetValueDelegate CreateTryGetValueDelegate(Type targetType)
        {
            TryGetValueDelegate result;

            _tryGetValueDelegateCacheLock.EnterReadLock();
            try
            {
                if (_tryGetValueDelegateCache.TryGetValue(targetType, out result))
                {
                    return result;
                }
            }
            finally
            {
                _tryGetValueDelegateCacheLock.ExitReadLock();
            }

            Type dictionaryType = ExtractGenericInterface(targetType, typeof(IDictionary<,>));

            // just wrap a call to the underlying IDictionary<TKey, TValue>.TryGetValue() where string can be cast to TKey
            if (dictionaryType != null)
            {
                Type[] typeArguments = dictionaryType.GetGenericArguments();
                Type keyType = typeArguments[0];
                Type returnType = typeArguments[1];

                if (keyType.IsAssignableFrom(typeof(string)))
                {
                    MethodInfo strongImplInfo = _strongTryGetValueImplInfo.MakeGenericMethod(keyType, returnType);
                    result = (TryGetValueDelegate)Delegate.CreateDelegate(typeof(TryGetValueDelegate), strongImplInfo);
                }
            }

            // wrap a call to the underlying IDictionary.Item()
            if (result == null && typeof(IDictionary).IsAssignableFrom(targetType))
            {
                result = TryGetValueFromNonGenericDictionary;
            }

            _tryGetValueDelegateCacheLock.EnterWriteLock();
            try
            {
                _tryGetValueDelegateCache[targetType] = result;
            }
            finally
            {
                _tryGetValueDelegateCacheLock.ExitWriteLock();
            }

            return result;
        }

        public static Type ExtractGenericInterface(Type queryType, Type interfaceType)
        {
            if (MatchesGenericType(queryType, interfaceType))
            {
                return queryType;
            }
            Type[] queryTypeInterfaces = queryType.GetInterfaces();
            return MatchGenericTypeFirstOrDefault(queryTypeInterfaces, interfaceType);
        }

        public static object GetDefaultValue(Type type)
        {
            return (TypeAllowsNullValue(type)) ? null : Activator.CreateInstance(type);
        }

        public static bool IsCompatibleObject<T>(object value)
        {
            return (value is T || (value == null && TypeAllowsNullValue(typeof(T))));
        }

        public static bool IsNullableValueType(Type type)
        {
            return Nullable.GetUnderlyingType(type) != null;
        }

        /// <summary>
        /// Provide a new <see cref="MissingMethodException"/> if original Message does not contain given full Type name.
        /// </summary>
        /// <param name="originalException"><see cref="MissingMethodException"/> to check.</param>
        /// <param name="fullTypeName">Full Type name which Message should contain.</param>
        /// <returns>New <see cref="MissingMethodException"/> if an update is required; null otherwise.</returns>
        public static MissingMethodException EnsureDebuggableException(
            MissingMethodException originalException,
            string fullTypeName)
        {
            MissingMethodException replacementException = null;
            if (!originalException.Message.Contains(fullTypeName))
            {
                string message = String.Format(
                    CultureInfo.CurrentCulture,
                    MvcResources.TypeHelpers_CannotCreateInstance,
                    originalException.Message,
                    fullTypeName);
                replacementException = new MissingMethodException(message, originalException);
            }

            return replacementException;
        }

        private static bool MatchesGenericType(Type type, Type matchType)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == matchType;
        }

        private static Type MatchGenericTypeFirstOrDefault(Type[] types, Type matchType)
        {
            for (int i = 0; i < types.Length; i++)
            {
                Type type = types[i];
                if (MatchesGenericType(type, matchType))
                {
                    return type;
                }
            }
            return null;
        }

        private static bool StrongTryGetValueImpl<TKey, TValue>(object dictionary, string key, out object value)
        {
            IDictionary<TKey, TValue> strongDict = (IDictionary<TKey, TValue>)dictionary;

            TValue strongValue;
            bool retVal = strongDict.TryGetValue((TKey)(object)key, out strongValue);
            value = strongValue;
            return retVal;
        }

        private static bool TryGetValueFromNonGenericDictionary(object dictionary, string key, out object value)
        {
            IDictionary weakDict = (IDictionary)dictionary;

            bool containsKey = weakDict.Contains(key);
            value = (containsKey) ? weakDict[key] : null;
            return containsKey;
        }

        public static bool TypeAllowsNullValue(Type type)
        {
            return (!type.IsValueType || IsNullableValueType(type));
        }
    }
}
