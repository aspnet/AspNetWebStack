// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Globalization;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using System.Web.Http.Properties;
using System.Web.Http.Services;

namespace System.Web.Http.Tracing.Tracers
{
    /// <summary>Represents a tracer for an <see cref="IAuthenticationFilter"/>.</summary>
    internal class AuthenticationFilterTracer : FilterTracer, IAuthenticationFilter, IDecorator<IAuthenticationFilter>
    {
        private const string AuthenticateAsyncMethodName = "AuthenticateAsync";
        private const string ChallengeAsyncMethodName = "ChallengeAsync";

        private readonly IAuthenticationFilter _innerFilter;

        public AuthenticationFilterTracer(IAuthenticationFilter innerFilter, ITraceWriter traceWriter)
            : base(innerFilter, traceWriter)
        {
            _innerFilter = innerFilter;
        }

        public new IAuthenticationFilter Inner
        {
            get { return _innerFilter; }
        }

        public Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            IPrincipal originalPrincipal = null;
            return TraceWriter.TraceBeginEndAsync(
                request: context != null ? context.Request : null,
                category: TraceCategories.FiltersCategory,
                level: TraceLevel.Info,
                operatorName: _innerFilter.GetType().Name,
                operationName: AuthenticateAsyncMethodName,
                beginTrace: (tr) =>
                {
                    if (context != null)
                    {
                        originalPrincipal = context.Principal;
                    }
                },
                execute: () => _innerFilter.AuthenticateAsync(context, cancellationToken),
                endTrace: (tr) =>
                {
                    if (context != null)
                    {
                        if (context.ErrorResult != null)
                        {
                            tr.Message = String.Format(CultureInfo.CurrentCulture,
                                SRResources.AuthenticationFilterErrorResult,
                                context.ErrorResult);
                        }
                        else if (context.Principal != originalPrincipal)
                        {
                            if (context.Principal == null || context.Principal.Identity == null)
                            {
                                tr.Message = SRResources.AuthenticationFilterSetPrincipalToUnknownIdentity;
                            }
                            else
                            {
                                tr.Message = String.Format(CultureInfo.CurrentCulture,
                                    SRResources.AuthenticationFilterSetPrincipalToKnownIdentity,
                                    context.Principal.Identity.Name,
                                    context.Principal.Identity.AuthenticationType);
                            }
                        }
                        else
                        {
                            tr.Message = SRResources.AuthenticationFilterDidNothing;
                        }
                    }
                },
                errorTrace: null);
        }

        public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            return TraceWriter.TraceBeginEndAsync(
                request: context != null ? context.Request : null,
                category: TraceCategories.FiltersCategory,
                level: TraceLevel.Info,
                operatorName: _innerFilter.GetType().Name,
                operationName: ChallengeAsyncMethodName,
                beginTrace: null,
                execute: () => _innerFilter.ChallengeAsync(context, cancellationToken),
                endTrace: null,
                errorTrace: null);
        }
    }
}
