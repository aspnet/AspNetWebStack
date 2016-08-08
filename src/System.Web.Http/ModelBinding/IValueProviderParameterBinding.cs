// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http.ValueProviders;

namespace System.Web.Http.ModelBinding
{
    /// <summary>
    /// Describes a parameter binding that uses one or more instances of <see cref="ValueProviderFactory"/>
    /// </summary>
    public interface IValueProviderParameterBinding
    {
        /// <summary>
        /// Gets the <see cref="ValueProviderFactory"/> instances used by this
        /// parameter binding.
        /// </summary>
        IEnumerable<ValueProviderFactory> ValueProviderFactories { get; }
    }
}
