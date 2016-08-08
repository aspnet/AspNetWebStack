// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using DotNetOpenAuth.AspNet;

namespace Microsoft.Web.WebPages.OAuth
{
    /// <summary>
    /// Store display name and extra data of an IAuthenticationClient instance.
    /// </summary>
    public class AuthenticationClientData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationClientData"/> class.
        /// </summary>
        /// <param name="authenticationClient">The authentication client.</param>
        /// <param name="displayName">The display name.</param>
        /// <param name="extraData">The data bag used to store extra data about this client</param>
        public AuthenticationClientData(IAuthenticationClient authenticationClient, string displayName, IDictionary<string, object> extraData)
        {
            if (authenticationClient == null)
            {
                throw new ArgumentNullException("authenticationClient");
            }

            AuthenticationClient = authenticationClient;
            DisplayName = displayName;
            ExtraData = extraData;
        }

        /// <summary>
        /// Gets the authentication client.
        /// </summary>
        public IAuthenticationClient AuthenticationClient { get; private set; }

        /// <summary>
        /// Gets the display name.
        /// </summary>
        public string DisplayName { get; private set; }

        /// <summary>
        /// The data bag used to store extra data about this client.
        /// </summary>
        public IDictionary<string, object> ExtraData { get; private set; }
    }
}