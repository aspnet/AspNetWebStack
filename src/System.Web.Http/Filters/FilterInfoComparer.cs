// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace System.Web.Http.Filters
{
    internal sealed class FilterInfoComparer : IComparer<FilterInfo>
    {
        private static readonly FilterInfoComparer _instance = new FilterInfoComparer();

        public static FilterInfoComparer Instance
        {
            get { return _instance; }
        }

        public int Compare(FilterInfo x, FilterInfo y)
        {
            if (x == null && y == null)
            {
                return 0;
            }
            else if (x == null)
            {
                return -1;
            }
            else if (y == null)
            {
                return 1;
            }
            else
            {
                var r = x.Scope - y.Scope;
                return r;
            }
        }
    }
}
