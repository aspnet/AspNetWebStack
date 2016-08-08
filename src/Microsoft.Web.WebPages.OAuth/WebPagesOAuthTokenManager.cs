// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Web.Security;
using DotNetOpenAuth.AspNet.Clients;
using Microsoft.Internal.Web.Utils;
using Microsoft.Web.WebPages.OAuth.Resources;
using WebMatrix.WebData;

namespace Microsoft.Web.WebPages.OAuth
{
    /// <summary>
    /// WebPages implementation for the <see cref="IOAuthTokenManager"/> interface which store tokens into SimpleMembership database
    /// </summary>
    internal class WebPagesOAuthTokenManager : IOAuthTokenManager
    {
        private static ExtendedMembershipProvider VerifyProvider()
        {
            var provider = Membership.Provider as ExtendedMembershipProvider;
            if (provider == null)
            {
                throw new InvalidOperationException(OAuthResources.Security_NoExtendedMembershipProvider);
            }
            return provider;
        }

        /// <summary>
        /// Gets the token secret from the specified token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>
        /// The token's secret
        /// </returns>
        public string GetTokenSecret(string token)
        {
            if (String.IsNullOrEmpty(token))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "token");
            }

            return VerifyProvider().GetOAuthTokenSecret(token);
        }

        /// <summary>
        /// Replaces the request token with access token.
        /// </summary>
        /// <param name="requestToken">The request token.</param>
        /// <param name="accessToken">The access token.</param>
        /// <param name="accessTokenSecret">The access token secret.</param>
        public void ReplaceRequestTokenWithAccessToken(string requestToken, string accessToken, string accessTokenSecret)
        {
            if (String.IsNullOrEmpty(requestToken))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "requestToken");
            }

            if (String.IsNullOrEmpty(accessToken))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "accessToken");
            }

            if (String.IsNullOrEmpty(accessTokenSecret))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "accessTokenSecret");
            }

            VerifyProvider().ReplaceOAuthRequestTokenWithAccessToken(requestToken, accessToken, accessTokenSecret);
        }

        /// <summary>
        /// Stores the request token together with its secret.
        /// </summary>
        /// <param name="requestToken">The request token.</param>
        /// <param name="requestTokenSecret">The request token secret.</param>
        public void StoreRequestToken(string requestToken, string requestTokenSecret)
        {
            if (String.IsNullOrEmpty(requestToken))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "requestToken");
            }

            VerifyProvider().StoreOAuthRequestToken(requestToken, requestTokenSecret);
        }
    }
}