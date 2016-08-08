// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Owin;
using Microsoft.Owin.Security;

namespace System.Net.Http
{
    /// <summary>
    /// Provides extension methods for the <see cref="HttpRequestMessage"/> class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class OwinHttpRequestMessageExtensions
    {
        private const string OwinEnvironmentKey = "MS_OwinEnvironment";
        private const string OwinContextKey = "MS_OwinContext";

        /// <summary>Gets the OWIN context for the specified request.</summary>
        /// <param name="request">The HTTP request message.</param>
        /// <returns>
        /// The OWIN environment for the specified context, if available; otherwise <see langword="null"/>.
        /// </returns>
        public static IOwinContext GetOwinContext(this HttpRequestMessage request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            IOwinContext context;
            if (!request.Properties.TryGetValue<IOwinContext>(OwinContextKey, out context))
            {
                // If the OWIN context is not available, try to create by upgrading an OWIN environment property
                // instead.
                IDictionary<string, object> environment;
                if (request.Properties.TryGetValue<IDictionary<string, object>>(OwinEnvironmentKey, out environment))
                {
                    context = new OwinContext(environment);
                    SetOwinContext(request, context);
                    request.Properties.Remove(OwinEnvironmentKey);
                }
            }
            return context;
        }

        /// <summary>Sets the OWIN context for the specified request.</summary>
        /// <param name="request">The HTTP request message.</param>
        /// <param name="context">The OWIN context to set.</param>
        public static void SetOwinContext(this HttpRequestMessage request, IOwinContext context)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            request.Properties[OwinContextKey] = context;
            // Make sure only one of the two properties exists (single source of truth).
            request.Properties.Remove(OwinEnvironmentKey);
        }

        /// <summary>Gets the OWIN environment for the specified request.</summary>
        /// <param name="request">The HTTP request message.</param>
        /// <returns>
        /// The OWIN environment for the specified request, if available; otherwise <see langword="null"/>.
        /// </returns>
        public static IDictionary<string, object> GetOwinEnvironment(this HttpRequestMessage request)
        {
            IOwinContext context = GetOwinContext(request);

            if (context == null)
            {
                return null;
            }

            return context.Environment;
        }

        /// <summary>Sets the OWIN environment for the specified request.</summary>
        /// <param name="request">The HTTP request message.</param>
        /// <param name="environment">The OWIN environment to set.</param>
        public static void SetOwinEnvironment(this HttpRequestMessage request, IDictionary<string, object> environment)
        {
            SetOwinContext(request, new OwinContext(environment));
        }

        internal static IAuthenticationManager GetAuthenticationManager(this HttpRequestMessage request)
        {
            IOwinContext context = GetOwinContext(request);

            if (context == null)
            {
                return null;
            }

            return context.Authentication;
        }
    }
}
