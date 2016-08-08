// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;

namespace System.Web.Http
{
    /// <summary>Represents an authentication attribute that authenticates via OWIN middleware.</summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public sealed class HostAuthenticationAttribute : Attribute, IAuthenticationFilter
    {
        private readonly IAuthenticationFilter _innerFilter;
        private readonly string _authenticationType;

        /// <summary>Initializes a new instance of the <see cref="HostAuthenticationAttribute"/> class.</summary>
        /// <param name="authenticationType">The authentication type of the OWIN middleware to use.</param>
        public HostAuthenticationAttribute(string authenticationType)
            : this(new HostAuthenticationFilter(authenticationType))
        {
            _authenticationType = authenticationType;
        }

        internal HostAuthenticationAttribute(IAuthenticationFilter innerFilter)
        {
            if (innerFilter == null)
            {
                throw new ArgumentNullException("innerFilter");
            }

            _innerFilter = innerFilter;
        }

        /// <inheritdoc />
        public bool AllowMultiple
        {
            get { return true; }
        }

        /// <summary>Gets the authentication type of the OWIN middleware to use.</summary>
        public string AuthenticationType
        {
            get { return _authenticationType; }
        }

        internal IAuthenticationFilter InnerFilter
        {
            get
            {
                return _innerFilter;
            }
        }

        /// <inheritdoc />
        public Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            return _innerFilter.AuthenticateAsync(context, cancellationToken);
        }

        /// <inheritdoc />
        public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            return _innerFilter.ChallengeAsync(context, cancellationToken);
        }
    }
}
