// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Web.Security;
using DotNetOpenAuth.AspNet;
using Microsoft.Web.WebPages.OAuth.Resources;
using WebMatrix.WebData;

namespace Microsoft.Web.WebPages.OAuth
{
    internal class WebPagesOAuthDataProvider : IOpenAuthDataProvider
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

        public string GetUserNameFromOpenAuth(string openAuthProvider, string openAuthId)
        {
            ExtendedMembershipProvider provider = VerifyProvider();

            int userId = provider.GetUserIdFromOAuth(openAuthProvider, openAuthId);
            if (userId == -1) 
            {
                return null;
            }

            return provider.GetUserNameFromId(userId);
        }
    }
}