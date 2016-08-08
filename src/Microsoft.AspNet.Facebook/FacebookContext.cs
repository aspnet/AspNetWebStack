// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Facebook;
using Microsoft.AspNet.Facebook.ModelBinders;

namespace Microsoft.AspNet.Facebook
{
    /// <summary>
    /// Provides access to Facebook-specific information.
    /// </summary>
    [FacebookContextBinderAttribute]
    public class FacebookContext
    {
        /// <summary>
        /// Gets or sets the parsed signed request.
        /// </summary>
        public dynamic SignedRequest { get; set; }

        /// <summary>
        /// Gets or sets the access token.
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// Gets or sets the user ID.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets the Facebook client.
        /// </summary>
        public FacebookClient Client { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="FacebookConfiguration"/>.
        /// </summary>
        public FacebookConfiguration Configuration { get; set; }
    }
}