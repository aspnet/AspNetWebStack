// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Http.Validation
{
    /// <summary>
    /// Abstraction for creating keys used in nested validation scopes. Intended for use in
    /// <see cref="IBodyModelValidator"/> implementations, especially <see cref="DefaultBodyModelValidator"/>.
    /// </summary>
    public interface IBodyModelValidatorKeyBuilder
    {
        /// <summary>
        /// Returns the key for a nested scope within the <paramref name="prefix"/> scope.
        /// </summary>
        /// <param name="prefix">Key for the current scope.</param>
        /// <returns>Key for a nested scope. Usually appends a property name to <paramref name="prefix"/>.</returns>
        string AppendTo(string prefix);
    }
}
