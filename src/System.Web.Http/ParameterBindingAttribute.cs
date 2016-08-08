// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Http.Controllers;

namespace System.Web.Http
{
    /// <summary>
    /// Attribute on a parameter or type that produces a <see cref="HttpParameterBinding"/>. 
    /// If the attribute is on a type-declaration, then it's as if that attribute is present on all action parameters 
    /// of that type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Parameter, Inherited = true, AllowMultiple = false)]
    public abstract class ParameterBindingAttribute : Attribute
    {
        public abstract HttpParameterBinding GetBinding(HttpParameterDescriptor parameter);
    }
}