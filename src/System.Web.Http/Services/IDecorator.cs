// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Http.Services
{
    /// <summary>
    /// Defines a decorator that exposes the inner decorated object.
    /// </summary>
    public interface IDecorator<out T>
    {
        /// <summary>
        /// Gets the inner object.
        /// </summary>
        T Inner { get; }
    }
}
