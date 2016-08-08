// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Web.Http.Filters;
using System.Web.Http.Owin;

namespace System.Web.Http
{
    /// <summary>Provides extension methods for the <see cref="HttpConfiguration"/> class.</summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class OwinHttpConfigurationExtensions
    {
        /// <summary>Enables suppression of the host's default authentication.</summary>
        /// <param name="configuration">The server configuration.</param>
        /// <remarks>
        /// When the host's default authentication is suppressed, the current principal is set to anonymous upon
        /// entering the <see cref="HttpServer"/>'s first message handler. As a result, any default authentication
        /// performed by the host is ignored. The remaining pipeline within the <see cref="HttpServer"/>, including
        /// <see cref="IAuthenticationFilter"/>s, is then the exclusive authority for authentication.
        /// </remarks>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope",
            Justification = "Message handler should be disposed with parent configuration.")]
        public static void SuppressDefaultHostAuthentication(this HttpConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            Contract.Assert(configuration.MessageHandlers != null);
            configuration.MessageHandlers.Insert(0, new PassiveAuthenticationMessageHandler());
        }
    }
}
