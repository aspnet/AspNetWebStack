// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Http.Metadata;

namespace System.Web.Http.Validation
{
    /// <summary>
    /// Defines a cache for <see cref="ModelValidator"/>s. This cache is keyed on the type or property that the
    /// metadata is associated with.
    /// </summary>
    public interface IModelValidatorCache
    {
        /// <summary>
        /// Returns the <see cref="ModelValidator"/>s for the given <paramref name="metadata"/>.
        /// </summary>
        /// <param name="metadata">The <see cref="ModelMetadata"/>.</param>
        /// <returns>An array of <see cref="ModelValidator"/>s for the given <paramref name="metadata"/>.</returns>
        ModelValidator[] GetValidators(ModelMetadata metadata);
    }
}
