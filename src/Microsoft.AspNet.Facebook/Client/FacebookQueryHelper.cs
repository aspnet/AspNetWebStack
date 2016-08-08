// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.AspNet.Facebook.Client
{
    /// <summary>
    /// Helper for constructing Facebook Graph API queries.
    /// </summary>
    public static class FacebookQueryHelper
    {
        /// <summary>
        /// Gets the appropriate "fields" query parameter for the Facebook Graph API based on the public properties of the model type.
        /// </summary>
        /// <param name="modelType">Type of the model.</param>
        /// <returns>The "fields" query parameter.</returns>
        public static string GetFields(Type modelType)
        {
            IList<string> fieldNames = GetFieldNames(modelType, typesVisited: new HashSet<Type>());
            if (fieldNames.Count > 0 && modelType != typeof(object))
            {
                return "?fields=" + String.Join(",", fieldNames);
            }

            return String.Empty;
        }

        private static string GetConnectionFields(Type modelType, HashSet<Type> typesVisited)
        {
            if (typesVisited.Contains(modelType))
            {
                throw new InvalidOperationException(Resources.CircularReferenceNotSupported);
            }

            IList<string> fieldNames = GetFieldNames(modelType, typesVisited);
            if (fieldNames.Count > 0 && modelType != typeof(object))
            {
                return ".fields(" + String.Join(",", fieldNames) + ")";
            }

            return String.Empty;
        }

        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "Lowercase is intended for field names.")]
        private static IList<string> GetFieldNames(Type modelType, HashSet<Type> typesVisited)
        {
            Type connectionType;
            if (TryUnwrapConnectionType(modelType, out connectionType))
            {
                modelType = connectionType;
            }

            PropertyInfo[] properties = modelType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            List<string> facebookFields = new List<string>();
            foreach (PropertyInfo property in properties)
            {
                string propertyName = property.Name;
                AttributeCollection attributes =
                   TypeDescriptor.GetProperties(modelType)[propertyName].Attributes;

                JsonIgnoreAttribute jsonIgnoreAttribute =
                   (JsonIgnoreAttribute)attributes[typeof(JsonIgnoreAttribute)];

                if (jsonIgnoreAttribute == null)
                {
                    JsonPropertyAttribute jsonPropertyAttribute =
                        (JsonPropertyAttribute)attributes[typeof(JsonPropertyAttribute)];
                    FacebookFieldModifierAttribute modifierAttribute =
                        (FacebookFieldModifierAttribute)attributes[typeof(FacebookFieldModifierAttribute)];

                    StringBuilder fieldName = new StringBuilder(
                        jsonPropertyAttribute != null ?
                            jsonPropertyAttribute.PropertyName :
                            propertyName);

                    if (modifierAttribute != null)
                    {
                        fieldName.AppendFormat(CultureInfo.InvariantCulture, ".{0}", modifierAttribute.FieldModifier);
                    }

                    Type propertyType = property.PropertyType;
                    if (TryUnwrapConnectionType(propertyType, out connectionType))
                    {
                        typesVisited.Add(property.DeclaringType);
                        fieldName.Append(GetConnectionFields(connectionType, typesVisited));
                    }

                    facebookFields.Add(fieldName.ToString().ToLowerInvariant());
                }
            }

            return facebookFields;
        }

        private static bool TryUnwrapConnectionType(Type modelType, out Type connectionType)
        {
            if (modelType.IsGenericType)
            {
                Type genericTypeDefinition = modelType.GetGenericTypeDefinition();
                if (genericTypeDefinition == typeof(FacebookConnection<>) ||
                    genericTypeDefinition == typeof(FacebookGroupConnection<>))
                {
                    Type genericArgumentType = modelType.GetGenericArguments()[0];
                    connectionType = genericArgumentType;
                    return true;
                }
            }
            connectionType = null;
            return false;
        }
    }
}