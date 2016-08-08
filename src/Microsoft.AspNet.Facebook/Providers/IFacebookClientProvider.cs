// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Facebook;

namespace Microsoft.AspNet.Facebook.Providers
{
    /// <summary>
    /// Provides an abstraction for creating <see cref="FacebookClient"/>.
    /// </summary>
    public interface IFacebookClientProvider
    {
        /// <summary>
        /// Creates an instance of <see cref="FacebookClient"/>.
        /// </summary>
        /// <returns>The <see cref="FacebookClient"/> instance.</returns>
        FacebookClient CreateClient();
    }
}