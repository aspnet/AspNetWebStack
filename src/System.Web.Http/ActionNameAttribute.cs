// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Http
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class ActionNameAttribute : Attribute
    {
        public ActionNameAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }
    }
}
