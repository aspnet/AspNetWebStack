// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Web.Helpers.Resources;

namespace System.Web.Helpers
{
    // REVIEW: Consider implementing ICustomTypeDescriptor and IDictionary<string, object>
    public class DynamicJsonObject : DynamicObject
    {
        private readonly IDictionary<string, object> _values;

        public DynamicJsonObject(IDictionary<string, object> values)
        {
            Debug.Assert(values != null);
            _values = values.ToDictionary(p => p.Key, p => Json.WrapObject(p.Value),
                                          StringComparer.OrdinalIgnoreCase);
        }

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            result = null;
            if (binder.Type.IsAssignableFrom(_values.GetType()))
            {
                result = _values;
            }
            else
            {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, HelpersResources.Json_UnableToConvertType, binder.Type));
            }
            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = GetValue(binder.Name);
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            _values[binder.Name] = Json.WrapObject(value);
            return true;
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            string key = GetKey(indexes);
            if (!String.IsNullOrEmpty(key))
            {
                _values[key] = Json.WrapObject(value);
            }
            return true;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            string key = GetKey(indexes);
            result = null;
            if (!String.IsNullOrEmpty(key))
            {
                result = GetValue(key);
            }
            return true;
        }

        private static string GetKey(object[] indexes)
        {
            if (indexes.Length == 1)
            {
                return (string)indexes[0];
            }
            // REVIEW: Should this throw?
            return null;
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return _values.Keys;
        }

        private object GetValue(string name)
        {
            object result;
            if (_values.TryGetValue(name, out result))
            {
                return result;
            }
            return null;
        }
    }
}
