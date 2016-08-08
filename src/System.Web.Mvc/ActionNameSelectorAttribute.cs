// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;

namespace System.Web.Mvc
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public abstract class ActionNameSelectorAttribute : Attribute
    {
        public abstract bool IsValidName(ControllerContext controllerContext, string actionName, MethodInfo methodInfo);
    }
}
