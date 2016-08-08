// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Helpers.AntiXsrf
{
    // Provides configuration information about the anti-forgery system.
    internal interface IAntiForgeryConfig
    {
        // Provides additional data to go into the tokens.
        IAntiForgeryAdditionalDataProvider AdditionalDataProvider { get; }

        // Name of the cookie to use.
        string CookieName { get; }

        // Name of the form field to use.
        string FormFieldName { get; }

        // Whether SSL is mandatory for this request.
        bool RequireSSL { get; }

        // Skip ClaimsIdentity & related logic.
        bool SuppressIdentityHeuristicChecks { get; }

        // ClaimType to use for ClaimsIdentity.
        string UniqueClaimTypeIdentifier { get; }

        // Skip X-FRAME-OPTIONS header.
        bool SuppressXFrameOptionsHeader { get; }
    }
}
