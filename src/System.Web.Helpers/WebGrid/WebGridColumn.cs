// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Helpers
{
    public class WebGridColumn
    {
        public bool CanSort { get; set; }

        public string ColumnName { get; set; }

        public Func<dynamic, object> Format { get; set; }

        public string Header { get; set; }

        public string Style { get; set; }
    }
}
