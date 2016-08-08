// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Mvc
{
    /// <summary>
    /// Used to create an <see cref="ITempDataProvider"/> instance for the controller.
    /// </summary>
    public interface ITempDataProviderFactory
    {
        /// <summary>
        /// Creates an instance of <see cref="ITempDataProvider"/> for the controller.
        /// </summary>
        /// <returns>The created <see cref="ITempDataProvider"/>.</returns>
        ITempDataProvider CreateInstance();
    }
}
