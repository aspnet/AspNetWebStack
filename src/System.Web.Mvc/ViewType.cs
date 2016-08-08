// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;
using System.Web.UI;

namespace System.Web.Mvc
{
    [ControlBuilder(typeof(ViewTypeControlBuilder))]
    [NonVisualControl]
    public class ViewType : Control
    {
        private string _typeName;

        [DefaultValue("")]
        public string TypeName
        {
            get { return _typeName ?? String.Empty; }
            set { _typeName = value; }
        }
    }
}
