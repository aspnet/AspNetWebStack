// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Web.UI;

namespace Microsoft.Web.Mvc.Controls
{
    public class RouteValues : IAttributeAccessor
    {
        private IDictionary<string, string> _attributes;

        public IDictionary<string, string> Attributes
        {
            get
            {
                EnsureAttributes();
                return _attributes;
            }
        }

        private void EnsureAttributes()
        {
            if (_attributes == null)
            {
                _attributes = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }
        }

        protected virtual string GetAttribute(string key)
        {
            EnsureAttributes();
            string value;
            _attributes.TryGetValue(key, out value);
            return value;
        }

        protected virtual void SetAttribute(string key, string value)
        {
            EnsureAttributes();
            _attributes[key] = value;
        }

        #region IAttributeAccessor Members

        string IAttributeAccessor.GetAttribute(string key)
        {
            return GetAttribute(key);
        }

        void IAttributeAccessor.SetAttribute(string key, string value)
        {
            SetAttribute(key, value);
        }

        #endregion
    }
}
