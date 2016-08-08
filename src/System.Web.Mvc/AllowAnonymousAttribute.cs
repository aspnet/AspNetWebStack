// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Mvc
{
    /// <summary>
    /// Actions and controllers with the AllowAnonymous attribute are skipped by the Authorize attribute
    /// on authorization. See AccountController.cs in the project template for an example.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class AllowAnonymousAttribute : Attribute
    {
    }
}
