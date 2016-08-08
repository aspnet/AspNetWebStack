// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Web.Script.Serialization;
using Microsoft.Internal.Web.Utils;

namespace System.Web.Helpers
{
    /// <summary>
    /// Converter that knows how to get the member values from a dynamic object.
    /// </summary>
    internal class DynamicJavaScriptConverter : JavaScriptConverter
    {
        public override IEnumerable<Type> SupportedTypes
        {
            get
            {
                // REVIEW: For some reason the converters don't pick up interfaces
                yield return typeof(IDynamicMetaObjectProvider);
                yield return typeof(DynamicObject);
            }
        }

        public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
        {
            throw new NotSupportedException();
        }

        public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
        {
            var values = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            var memberNames = DynamicHelper.GetMemberNames(obj);

            // This should never happen
            Debug.Assert(memberNames != null);

            // Get the value for each member in the dynamic object
            foreach (string memberName in memberNames)
            {
                object value = DynamicHelper.GetMemberValue(obj, memberName);
                var jsonValue = value as DynamicJsonArray;
                if (jsonValue != null)
                {
                    value = (object[])jsonValue;
                }
                values[memberName] = value;
            }

            return values;
        }
    }
}
