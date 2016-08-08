// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace System.Web.WebPages.Html
{
    public class ModelState
    {
        private List<string> _errors = new List<string>();

        public IList<string> Errors
        {
            get { return _errors; }
        }

        public object Value { get; set; }
    }
}
