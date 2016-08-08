// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;

namespace System.Web.Mvc
{
    internal sealed class DynamicViewDataDictionary : DynamicObject
    {
        private readonly Func<ViewDataDictionary> _viewDataThunk;

        public DynamicViewDataDictionary(Func<ViewDataDictionary> viewDataThunk)
        {
            _viewDataThunk = viewDataThunk;
        }

        private ViewDataDictionary ViewData
        {
            get
            {
                ViewDataDictionary viewData = _viewDataThunk();
                Debug.Assert(viewData != null);
                return viewData;
            }
        }

        // Implementing this function improves the debugging experience as it provides the debugger with the list of all
        // the properties currently defined on the object
        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return ViewData.Keys;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = ViewData[binder.Name];
            // since ViewDataDictionary always returns a result even if the key does not exist, always return true
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            ViewData[binder.Name] = value;
            // you can always set a key in the dictionary so return true
            return true;
        }
    }
}
