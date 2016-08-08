// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace System.Web.Mvc
{
    public abstract class ParameterBindingInfo
    {
        public virtual IModelBinder Binder
        {
            get { return null; }
        }

        public virtual ICollection<string> Exclude
        {
            get { return new string[0]; }
        }

        public virtual ICollection<string> Include
        {
            get { return new string[0]; }
        }

        public virtual string Prefix
        {
            get { return null; }
        }
    }
}
