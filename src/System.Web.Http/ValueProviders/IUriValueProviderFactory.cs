// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace System.Web.Http.ValueProviders
{
    /// <summary>
    /// This interface is implemented by any <see cref="ValueProviderFactory"/> that supports
    /// the creation of a <see cref="IValueProvider"/> to access the <see cref="T:System.Uri"/> of
    /// an incoming <see cref="T:System.Net.Http.HttpRequestMessage"/>.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces", Justification = "Tagging interface is intentional to allow Linq TypeOf")]
    public interface IUriValueProviderFactory
    {
    }
}
