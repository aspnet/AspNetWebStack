// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace WebMatrix.WebData
{
    /// <summary>
    /// Defines key names for use in a web.config &lt;appSettings&gt; section to override default settings.
    /// </summary>
    public static class FormsAuthenticationSettings
    {
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Login", Justification = "The term Login is used more frequently in ASP.Net")]
        public static readonly string LoginUrlKey = "loginUrl";

        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Login", Justification = "The term Login is used more frequently in ASP.Net")]
        public static readonly string DefaultLoginUrl = "~/Account/Login";

        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Login", Justification = "The term Login is used more frequently in ASP.Net")]
        public static readonly string PreserveLoginUrlKey = "PreserveLoginUrl";
    }
}
