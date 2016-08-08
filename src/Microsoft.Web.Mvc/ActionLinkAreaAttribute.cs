// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.Web.Mvc
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class ActionLinkAreaAttribute : Attribute
    {
        public ActionLinkAreaAttribute(string area)
        {
            if (area == null)
            {
                throw new ArgumentNullException("area");
            }

            Area = area;
        }

        public string Area { get; private set; }
    }
}
