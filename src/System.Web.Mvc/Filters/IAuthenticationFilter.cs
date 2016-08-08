// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Mvc.Filters
{
    /// <summary>Defines a filter that performs authentication.</summary>
    public interface IAuthenticationFilter
    {
        /// <summary>Authenticates the request.</summary>
        /// <param name="filterContext">The context to use for authentication.</param>
        void OnAuthentication(AuthenticationContext filterContext);

        /// <summary>Adds an authentication challenge to the current <see cref="ActionResult"/>.</summary>
        /// <param name="filterContext">The context to use for the authentication challenge.</param>
        void OnAuthenticationChallenge(AuthenticationChallengeContext filterContext);
    }
}
