// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;

namespace System.Web.Helpers
{
    public static class Json
    {
        private static readonly JavaScriptSerializer _serializer = CreateSerializer();

        public static string Encode(object value)
        {
            // Serialize our dynamic array type as an array
            DynamicJsonArray jsonArray = value as DynamicJsonArray;
            if (jsonArray != null)
            {
                return _serializer.Serialize((object[])jsonArray);
            }

            return _serializer.Serialize(value);
        }

        public static void Write(object value, TextWriter writer)
        {
            writer.Write(_serializer.Serialize(value));
        }

        public static dynamic Decode(string value)
        {
            return WrapObject(_serializer.DeserializeObject(value));
        }

        public static dynamic Decode(string value, Type targetType)
        {
            return WrapObject(_serializer.Deserialize(value, targetType));
        }

        public static T Decode<T>(string value)
        {
            return _serializer.Deserialize<T>(value);
        }

        private static JavaScriptSerializer CreateSerializer()
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializer.RegisterConverters(new[] { new DynamicJavaScriptConverter() });
            return serializer;
        }

        internal static dynamic WrapObject(object value)
        {
            // The JavaScriptSerializer returns IDictionary<string, object> for objects
            // and object[] for arrays, so we wrap those in different dynamic objects
            // so we can access the object graph using dynamic
            var dictionaryValues = value as IDictionary<string, object>;
            if (dictionaryValues != null)
            {
                return new DynamicJsonObject(dictionaryValues);
            }

            var arrayValues = value as object[];
            if (arrayValues != null)
            {
                return new DynamicJsonArray(arrayValues);
            }

            return value;
        }
    }
}
