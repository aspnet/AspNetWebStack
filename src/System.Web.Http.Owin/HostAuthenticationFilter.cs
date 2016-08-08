// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using System.Web.Http.Owin.Properties;
using Microsoft.Owin;
using Microsoft.Owin.Security;

namespace System.Web.Http
{
    /// <summary>Represents an authentication filter that authenticates via OWIN middleware.</summary>
    public class HostAuthenticationFilter : IAuthenticationFilter
    {
        private readonly string _authenticationType;

        /// <summary>Initializes a new instance of the <see cref="HostAuthenticationFilter"/> class.</summary>
        /// <param name="authenticationType">The authentication type of the OWIN middleware to use.</param>
        public HostAuthenticationFilter(string authenticationType)
        {
            if (authenticationType == null)
            {
                throw new ArgumentNullException("authenticationType");
            }

            _authenticationType = authenticationType;
        }

        /// <summary>Gets the authentication type of the OWIN middleware to use.</summary>
        public string AuthenticationType
        {
            get { return _authenticationType; }
        }

        /// <inheritdoc />
        public async Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            HttpRequestMessage request = context.Request;

            if (request == null)
            {
                throw new InvalidOperationException(OwinResources.HttpAuthenticationContext_RequestMustNotBeNull);
            }

            IAuthenticationManager authenticationManager = GetAuthenticationManagerOrThrow(request);

            cancellationToken.ThrowIfCancellationRequested();
            AuthenticateResult result = await authenticationManager.AuthenticateAsync(_authenticationType);

            if (result != null)
            {
                IIdentity identity = result.Identity;

                if (identity != null)
                {
                    context.Principal = new ClaimsPrincipal(identity);
                }
            }
        }

        /// <inheritdoc />
        public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            HttpRequestMessage request = context.Request;

            if (request == null)
            {
                throw new InvalidOperationException(OwinResources.HttpAuthenticationChallengeContext_RequestMustNotBeNull);
            }

            IAuthenticationManager authenticationManager = GetAuthenticationManagerOrThrow(request);

            // Control the challenges that OWIN middleware adds later.
            authenticationManager.AuthenticationResponseChallenge = AddChallengeAuthenticationType(
                authenticationManager.AuthenticationResponseChallenge, _authenticationType);

            return TaskHelpers.Completed();
        }

        /// <inheritdoc />
        public bool AllowMultiple
        {
            get { return true; }
        }

        private static AuthenticationResponseChallenge AddChallengeAuthenticationType(
            AuthenticationResponseChallenge challenge, string authenticationType)
        {
            Contract.Assert(authenticationType != null);

            List<string> authenticationTypes = new List<string>();
            AuthenticationProperties properties;

            if (challenge != null)
            {
                string[] currentAuthenticationTypes = challenge.AuthenticationTypes;

                if (currentAuthenticationTypes != null)
                {
                    authenticationTypes.AddRange(currentAuthenticationTypes);
                }

                properties = challenge.Properties;
            }
            else
            {
                properties = new AuthenticationProperties();
            }

            authenticationTypes.Add(authenticationType);

            return new AuthenticationResponseChallenge(authenticationTypes.ToArray(), properties);
        }

        private static IAuthenticationManager GetAuthenticationManagerOrThrow(HttpRequestMessage request)
        {
            Contract.Assert(request != null);

            IAuthenticationManager authenticationManager = request.GetAuthenticationManager();

            if (authenticationManager == null)
            {
                throw new InvalidOperationException(OwinResources.IAuthenticationManagerNotAvailable);
            }

            return authenticationManager;
        }
    }
}
