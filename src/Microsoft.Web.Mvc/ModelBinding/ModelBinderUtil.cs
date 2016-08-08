// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Microsoft.Web.Mvc.Properties;

namespace Microsoft.Web.Mvc.ModelBinding
{
    internal static class ModelBinderUtil
    {
        public static TModel CastOrDefault<TModel>(object model)
        {
            return (model is TModel) ? (TModel)model : default(TModel);
        }

        public static string CreateIndexModelName(string parentName, int index)
        {
            return CreateIndexModelName(parentName, index.ToString(CultureInfo.InvariantCulture));
        }

        public static string CreateIndexModelName(string parentName, string index)
        {
            return (parentName.Length == 0) ? "[" + index + "]" : parentName + "[" + index + "]";
        }

        public static string CreatePropertyModelName(string prefix, string propertyName)
        {
            if (String.IsNullOrEmpty(prefix))
            {
                return propertyName ?? String.Empty;
            }
            else if (String.IsNullOrEmpty(propertyName))
            {
                return prefix ?? String.Empty;
            }
            else
            {
                return prefix + "." + propertyName;
            }
        }

        public static IExtensibleModelBinder GetPossibleBinderInstance(Type closedModelType, Type openModelType, Type openBinderType)
        {
            Type[] typeArguments = TypeHelpers.GetTypeArgumentsIfMatch(closedModelType, openModelType);
            return (typeArguments != null) ? (IExtensibleModelBinder)Activator.CreateInstance(openBinderType.MakeGenericType(typeArguments)) : null;
        }

        public static object[] RawValueToObjectArray(object rawValue)
        {
            // precondition: rawValue is not null

            // Need to special-case String so it's not caught by the IEnumerable check which follows
            if (rawValue is string)
            {
                return new[] { rawValue };
            }

            object[] rawValueAsObjectArray = rawValue as object[];
            if (rawValueAsObjectArray != null)
            {
                return rawValueAsObjectArray;
            }

            IEnumerable rawValueAsEnumerable = rawValue as IEnumerable;
            if (rawValueAsEnumerable != null)
            {
                return rawValueAsEnumerable.Cast<object>().ToArray();
            }

            // fallback
            return new[] { rawValue };
        }

        public static void ReplaceEmptyStringWithNull(ModelMetadata modelMetadata, ref object model)
        {
            if (modelMetadata.ConvertEmptyStringToNull && StringIsEmptyOrWhitespace(model as string))
            {
                model = null;
            }
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
                    MvcResources.ModelBinderUtil_CannotCreateInstance,
                    originalException.Message,
                    fullTypeName);
                replacementException = new MissingMethodException(message, originalException);
            }

            return replacementException;
        }

        // Based on String.IsNullOrWhitespace
        private static bool StringIsEmptyOrWhitespace(string s)
        {
            if (s == null)
            {
                return false;
            }

            if (s.Length != 0)
            {
                for (int i = 0; i < s.Length; i++)
                {
                    if (!Char.IsWhiteSpace(s[i]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static void ValidateBindingContext(ExtensibleModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException("bindingContext");
            }

            if (bindingContext.ModelMetadata == null)
            {
                throw Error.ModelBinderUtil_ModelMetadataCannotBeNull();
            }
        }

        public static void ValidateBindingContext(ExtensibleModelBindingContext bindingContext, Type requiredType, bool allowNullModel)
        {
            ValidateBindingContext(bindingContext);

            if (bindingContext.ModelType != requiredType)
            {
                throw Error.ModelBinderUtil_ModelTypeIsWrong(bindingContext.ModelType, requiredType);
            }

            if (!allowNullModel && bindingContext.Model == null)
            {
                throw Error.ModelBinderUtil_ModelCannotBeNull(requiredType);
            }

            if (bindingContext.Model != null && !requiredType.IsInstanceOfType(bindingContext.Model))
            {
                throw Error.ModelBinderUtil_ModelInstanceIsWrong(bindingContext.Model.GetType(), requiredType);
            }
        }
    }
}
