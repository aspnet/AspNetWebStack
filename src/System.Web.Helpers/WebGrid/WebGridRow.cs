// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web.Helpers.Resources;
using Microsoft.Internal.Web.Utils;

namespace System.Web.Helpers
{
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Collection is not an appropriate suffix for this class")]
    public class WebGridRow : DynamicObject, IEnumerable<object>
    {
        private const string RowIndexMemberName = "ROW";
        private const BindingFlags BindFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.IgnoreCase;

        private WebGrid _grid;
        private IDynamicMetaObjectProvider _dynamic;
        private int _rowIndex;
        private object _value;
        private IEnumerable<dynamic> _values;

        public WebGridRow(WebGrid webGrid, object value, int rowIndex)
        {
            _grid = webGrid;
            _value = value;
            _rowIndex = rowIndex;
            _dynamic = value as IDynamicMetaObjectProvider;
        }

        public dynamic Value
        {
            get { return _value; }
        }

        public WebGrid WebGrid
        {
            get { return _grid; }
        }

        public object this[string name]
        {
            get
            {
                if (String.IsNullOrEmpty(name))
                {
                    throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "name");
                }
                object value = null;
                if (!TryGetMember(name, out value))
                {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,
                                                                      HelpersResources.WebGrid_ColumnNotFound, name));
                }
                return value;
            }
        }

        public object this[int index]
        {
            get
            {
                if ((index < 0) || (index >= _grid.ColumnNames.Count()))
                {
                    throw new ArgumentOutOfRangeException("index");
                }
                return this.Skip(index).First();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<object> GetEnumerator()
        {
            if (_values == null)
            {
                _values = _grid.ColumnNames.Select(c => WebGrid.GetMember(this, c));
            }
            return _values.GetEnumerator();
        }

        public IHtmlString GetSelectLink(string text = null)
        {
            if (String.IsNullOrEmpty(text))
            {
                text = HelpersResources.WebGrid_SelectLinkText;
            }
            return WebGridRenderer.GridLink(_grid, GetSelectUrl(), text);
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "review: I think a method is more appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings", Justification = "Strings are easier for Plan9 developer to work with")]
        public string GetSelectUrl()
        {
            NameValueCollection queryString = new NameValueCollection(1);
            queryString[WebGrid.SelectionFieldName] = (_rowIndex + 1L).ToString(CultureInfo.CurrentCulture);
            return WebGrid.GetPath(queryString);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = null;
            // Try to get the row index
            if (TryGetRowIndex(binder.Name, out result))
            {
                return true;
            }

            // Try to evaluate the dynamic member based on the binder
            if (_dynamic != null && DynamicHelper.TryGetMemberValue(_dynamic, binder, out result))
            {
                return true;
            }

            return TryGetComplexMember(_value, binder.Name, out result);
        }

        internal bool TryGetMember(string memberName, out object result)
        {
            result = null;

            // Try to get the row index
            if (TryGetRowIndex(memberName, out result))
            {
                return true;
            }

            // Try to evaluate the dynamic member based on the name
            if (_dynamic != null && DynamicHelper.TryGetMemberValue(_dynamic, memberName, out result))
            {
                return true;
            }

            // Support '.' for navigation properties
            return TryGetComplexMember(_value, memberName, out result);
        }

        public override string ToString()
        {
            return _value.ToString();
        }

        private bool TryGetRowIndex(string memberName, out object result)
        {
            result = null;
            if (String.IsNullOrEmpty(memberName))
            {
                return false;
            }

            if (memberName == RowIndexMemberName)
            {
                result = _rowIndex;
                return true;
            }

            return false;
        }

        private static bool TryGetComplexMember(object obj, string name, out object result)
        {
            result = null;

            string[] names = name.Split('.');
            for (int i = 0; i < names.Length; i++)
            {
                if ((obj == null) || !TryGetMember(obj, names[i], out result))
                {
                    result = null;
                    return false;
                }
                obj = result;
            }
            return true;
        }

        private static bool TryGetMember(object obj, string name, out object result)
        {
            PropertyInfo property = obj.GetType().GetProperty(name, BindFlags);
            if ((property != null) && (property.GetIndexParameters().Length == 0))
            {
                result = property.GetValue(obj, null);
                return true;
            }
            result = null;
            return false;
        }
    }
}
