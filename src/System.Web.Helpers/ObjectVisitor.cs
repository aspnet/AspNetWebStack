// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Microsoft.Internal.Web.Utils;

namespace System.Web.Helpers
{
    internal class ObjectVisitor
    {
        private static readonly Dictionary<Type, string> _typeNames = new Dictionary<Type, string>
        {
            { typeof(string), "string" },
            { typeof(object), "object" },
            { typeof(int), "int" },
            { typeof(byte), "byte" },
            { typeof(short), "short" },
            { typeof(long), "long" },
            { typeof(decimal), "decimal" },
            { typeof(float), "float" },
            { typeof(double), "double" },
            { typeof(bool), "bool" },
            { typeof(char), "char" },
            { typeof(void), "void" }
        };

        private static readonly char[] _separators = { '&', '[', '*' };

        private readonly int _recursionLimit;
        private readonly int _enumerationLimit;
        private Dictionary<object, string> _visited = new Dictionary<object, string>();

        public ObjectVisitor(int recursionLimit, int enumerationLimit)
        {
            Debug.Assert(enumerationLimit > 0);
            Debug.Assert(recursionLimit >= 0);
            _enumerationLimit = enumerationLimit;
            _recursionLimit = recursionLimit;
        }

        protected string GetObjectId(object value)
        {
            string id;
            if (_visited.TryGetValue(value, out id))
            {
                return id;
            }
            return null;
        }

        public virtual void Visit(object value, int depth)
        {
            if (value == null || DBNull.Value.Equals(value))
            {
                VisitNull();
                return;
            }

            // Check to see if the we've already visited this object
            string id;
            if (_visited.TryGetValue(value, out id))
            {
                VisitVisitedObject(id, value);
                return;
            }

            string stringValue = value as string;
            if (stringValue != null)
            {
                VisitStringValue(stringValue);
                return;
            }

            if (TryConvertToString(value, out stringValue))
            {
                VisitConvertedValue(value, stringValue);
                return;
            }

            // This exceptin occurs when we try to access the property and it fails
            // for some reason. The actual exception is wrapped in the ObjectVisitorException
            ObjectVisitorException exception = value as ObjectVisitorException;
            if (exception != null)
            {
                VisitObjectVisitorException(exception);
                return;
            }

            // Mark the object as visited
            id = CreateObjectId(value);
            _visited.Add(value, id);

            NameValueCollection nameValueCollection = value as NameValueCollection;
            if (nameValueCollection != null)
            {
                VisitNameValueCollection(nameValueCollection, depth);
                return;
            }

            IDictionary dictionary = value as IDictionary;
            if (dictionary != null)
            {
                VisitDictionary(dictionary, depth);
                return;
            }

            IEnumerable enumerable = value as IEnumerable;
            if (enumerable != null)
            {
                VisitEnumerable(enumerable, depth);
                return;
            }

            VisitComplexObject(value, depth + 1);
        }

        public virtual void VisitObjectVisitorException(ObjectVisitorException exception)
        {
        }

        public virtual void VisitConvertedValue(object value, string convertedValue)
        {
            VisitStringValue(convertedValue);
        }

        public virtual void VisitVisitedObject(string id, object value)
        {
        }

        public virtual void VisitNull()
        {
        }

        public virtual void VisitStringValue(string stringValue)
        {
        }

        public virtual void VisitComplexObject(object value, int depth)
        {
            if (depth > _recursionLimit)
            {
                return;
            }

            Debug.Assert(value != null, "Value should not be null");

            var dynamicObject = value as IDynamicMetaObjectProvider;
            // Only look at dynamic objects that do not implement ICustomTypeDescriptor
            if (dynamicObject != null && !(dynamicObject is ICustomTypeDescriptor))
            {
                var memberNames = DynamicHelper.GetMemberNames(dynamicObject);

                if (memberNames != null)
                {
                    // Always use the runtime type for dynamic objects since there is no metadata
                    VisitMembers(memberNames,
                                 name => null,
                                 name => DynamicHelper.GetMemberValue(dynamicObject, name),
                                 depth);
                }
            }
            else
            {
                // REVIEW: We should try to filter out properties of certain types

                // Dump properties using type descriptor
                var props = TypeDescriptor.GetProperties(value);
                var propNames = from PropertyDescriptor p in props
                                select p.Name;

                VisitMembers(propNames,
                             name => props.Find(name, ignoreCase: true).PropertyType,
                             name => GetPropertyDescriptorValue(value, name, props),
                             depth);

                // Dump fields
                var fields = value.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance)
                    .ToDictionary(field => field.Name);

                VisitMembers(fields.Keys,
                             name => fields[name].FieldType,
                             name => GetFieldValue(value, name, fields),
                             depth);
            }
        }

        public virtual void VisitNameValueCollection(NameValueCollection collection, int depth)
        {
            VisitKeyValues(collection, collection.AllKeys.Cast<object>(), key => collection[(string)key], depth);
        }

        public virtual void VisitDictionary(IDictionary dictionary, int depth)
        {
            VisitKeyValues(dictionary, dictionary.Keys.Cast<object>(), key => dictionary[key], depth);
        }

        public virtual void VisitEnumerable(IEnumerable enumerable, int depth)
        {
            if (depth > _recursionLimit)
            {
                return;
            }

            Type enumerableType = enumerable.GetType();
            bool isIndexedEnumeration = ImplementsInterface(enumerableType, typeof(IList<>))
                                        || ImplementsInterface(enumerableType, typeof(IList));

            int index = 0;
            foreach (var item in enumerable)
            {
                if (index >= _enumerationLimit)
                {
                    VisitEnumeratonLimitExceeded();
                    break;
                }
                if (isIndexedEnumeration)
                {
                    VisitIndexedEnumeratedValue(index, item, depth);
                }
                else
                {
                    VisitEnumeratedValue(item, depth);
                }
                index++;
            }
        }

        public virtual void VisitEnumeratedValue(object item, int depth)
        {
            Visit(item, depth);
        }

        public virtual void VisitIndexedEnumeratedValue(int index, object item, int depth)
        {
            Visit(item, depth);
        }

        public virtual void VisitEnumeratonLimitExceeded()
        {
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We don't want to fail surface any exceptions throw from getting property accessors")]
        public virtual void VisitMembers(IEnumerable<string> names, Func<string, Type> typeSelector, Func<string, object> valueSelector, int depth)
        {
            foreach (string name in names)
            {
                Type type = null;
                object value = null;
                try
                {
                    // Get the type and value
                    type = typeSelector(name);
                    value = valueSelector(name);

                    // If the type is null try using the runtime type
                    if (value != null && type == null)
                    {
                        type = value.GetType();
                    }
                }
                catch (Exception ex)
                {
                    // Set the value as an exception we know about
                    value = new ObjectVisitorException(null, ex);
                }
                finally
                {
                    VisitMember(name, type, value, depth);
                }
            }
        }

        public virtual void VisitMember(string name, Type type, object value, int depth)
        {
            Visit(value, depth);
        }

        public virtual void VisitKeyValues(object value, IEnumerable<object> keys, Func<object, object> valueSelector, int depth)
        {
            if (depth > _recursionLimit)
            {
                return;
            }

            foreach (var key in keys)
            {
                VisitKeyValue(key, valueSelector(key), depth);
            }
        }

        public virtual void VisitKeyValue(object key, object value, int depth)
        {
            // Dump the key and value
            Visit(key, depth);
            Visit(value, depth);
        }

        protected virtual string CreateObjectId(object value)
        {
            // REVIEW: Maybe use a guid?
            return value.GetHashCode().ToString(CultureInfo.InvariantCulture);
        }

        internal static string GetTypeName(Type type)
        {
            // See if we have the type name stored
            string typeName;
            if (_typeNames.TryGetValue(type, out typeName))
            {
                return typeName;
            }

            if (type.IsGenericType)
            {
                // Get the generic type name without arguments
                string genericTypeName = GetGenericTypeName(type);
                // Create a user friendly type name
                var arguments = from argType in type.GetGenericArguments()
                                select GetTypeName(argType);

                return String.Format(CultureInfo.InvariantCulture, "{0}<{1}>", genericTypeName, String.Join(", ", arguments));
            }

            if (type.IsByRef || type.IsArray || type.IsPointer)
            {
                // Get the element type name
                string elementTypeName = GetTypeName(type.GetElementType());
                // Append the separator
                int sepIndex = type.Name.IndexOfAny(_separators);
                return elementTypeName + type.Name.Substring(sepIndex);
            }

            // Fallback to using the type name as is
            return type.Name;
        }

        private static string GetGenericTypeName(Type type)
        {
            Debug.Assert(type.IsGenericType, "Type is not a generic type");

            // Check for anonymous types
            if (IsAnonymousType(type))
            {
                return "AnonymousType";
            }

            string genericTypeDefinitionName = type.GetGenericTypeDefinition().Name;
            int index = genericTypeDefinitionName.IndexOf('`');
            Debug.Assert(index >= 0);
            // Get the generic type name without the `
            return genericTypeDefinitionName.Substring(0, index);
        }

        // Copied from System.Web.WebPages/Util/TypeHelpers.cs
        private static bool IsAnonymousType(Type type)
        {
            Debug.Assert(type != null, "Type should not be null");

            // TODO: The only way to detect anonymous types right now.
            return Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false)
                   && type.IsGenericType && type.Name.Contains("AnonymousType")
                   && (type.Name.StartsWith("<>", StringComparison.OrdinalIgnoreCase) || type.Name.StartsWith("VB$", StringComparison.OrdinalIgnoreCase))
                   && (type.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic;
        }

        private static bool ImplementsInterface(Type type, Type targetInterfaceType)
        {
            Func<Type, bool> implementsInterface = t => targetInterfaceType.IsAssignableFrom(t);
            if (targetInterfaceType.IsGenericType)
            {
                implementsInterface = t => t.IsGenericType && targetInterfaceType.IsAssignableFrom(t.GetGenericTypeDefinition());
            }
            return implementsInterface(type) || type.GetInterfaces().Any(implementsInterface);
        }

        private static object GetFieldValue(object value, string name, IDictionary<string, FieldInfo> fields)
        {
            FieldInfo fieldInfo;
            // Get the value from the dictionary
            bool result = fields.TryGetValue(name, out fieldInfo);
            Debug.Assert(result, "Entry should exist");
            return fieldInfo.GetValue(value);
        }

        private static object GetPropertyDescriptorValue(object value, string name, PropertyDescriptorCollection props)
        {
            PropertyDescriptor propertyDescriptor = props.Find(name, ignoreCase: true);
            Debug.Assert(propertyDescriptor != null, "Property descriptor shouldn't be null");
            return propertyDescriptor.GetValue(value);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We don't want to surface any exceptions while trying to convert from string")]
        private static bool TryConvertToString(object value, out string stringValue)
        {
            stringValue = null;
            try
            {
                IConvertible convertibe = value as IConvertible;
                if (convertibe != null)
                {
                    stringValue = convertibe.ToString(CultureInfo.CurrentCulture);
                    return true;
                }

                TypeConverter converter = TypeDescriptor.GetConverter(value);
                if (converter.CanConvertFrom(typeof(string)))
                {
                    stringValue = converter.ConvertToString(value);
                    return true;
                }

                Type type = value.GetType();
                if (type == typeof(object))
                {
                    stringValue = value.ToString();
                    return true;
                }
                Type valueAsType = value as Type;
                if (valueAsType != null)
                {
                    stringValue = "typeof(" + GetTypeName(valueAsType) + ")";
                    return true;
                }
            }
            catch (Exception)
            {
                // If we failed to convert the type for any reason return false
            }
            return false;
        }

        [Serializable]
        public class ObjectVisitorException : Exception
        {
            public ObjectVisitorException()
            {
            }

            public ObjectVisitorException(string message)
                : base(message)
            {
            }

            public ObjectVisitorException(string message, Exception inner)
                : base(message, inner)
            {
            }

            protected ObjectVisitorException(
                SerializationInfo info,
                StreamingContext context)
                : base(info, context)
            {
            }
        }
    }
}
