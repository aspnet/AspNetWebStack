// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Web.Http;

namespace Microsoft.AspNet.Facebook.Models
{
    /// <summary>
    /// Subscription verification data from Facebook as part of Realtime Updates.
    /// </summary>
    [FromUri(Name = "hub")]
    public class SubscriptionVerification
    {
        /// <summary>
        /// Gets or sets the mode.
        /// </summary>
        /// <value>
        /// The mode.
        /// </value>
        public string Mode { get; set; }

        /// <summary>
        /// Gets or sets the verify_token.
        /// </summary>
        /// <value>
        /// The verify_token.
        /// </value>
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "This is a shipped API")]
        public string Verify_Token { get; set; }

        /// <summary>
        /// Gets or sets the challenge string.
        /// </summary>
        /// <value>
        /// The challenge string.
        /// </value>
        public string Challenge { get; set; }
    }
}