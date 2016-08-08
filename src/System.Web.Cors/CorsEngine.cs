// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Web.Cors.Properties;

namespace System.Web.Cors
{
    /// <summary>
    /// An implementation of <see cref="ICorsEngine"/> based on the CORS specifications.
    /// </summary>
    public class CorsEngine : ICorsEngine
    {
        /// <summary>
        /// Evaluates the policy.
        /// </summary>
        /// <param name="requestContext">The <see cref="CorsRequestContext" />.</param>
        /// <param name="policy">The <see cref="CorsPolicy" />.</param>
        /// <returns>
        /// The <see cref="CorsResult" />
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// requestContext
        /// or
        /// policy
        /// </exception>
        public virtual CorsResult EvaluatePolicy(CorsRequestContext requestContext, CorsPolicy policy)
        {
            if (requestContext == null)
            {
                throw new ArgumentNullException("requestContext");
            }
            if (policy == null)
            {
                throw new ArgumentNullException("policy");
            }

            CorsResult result = new CorsResult();

            if (!TryValidateOrigin(requestContext, policy, result))
            {
                return result;
            }

            result.SupportsCredentials = policy.SupportsCredentials;

            if (requestContext.IsPreflight)
            {
                if (!TryValidateMethod(requestContext, policy, result))
                {
                    return result;
                }

                if (!TryValidateHeaders(requestContext, policy, result))
                {
                    return result;
                }

                result.PreflightMaxAge = policy.PreflightMaxAge;
            }
            else
            {
                AddHeaderValues(result.AllowedExposedHeaders, policy.ExposedHeaders);
            }

            return result;
        }

        /// <summary>
        /// Try to validate the requested method based on <see cref="CorsPolicy"/>.
        /// </summary>
        /// <param name="requestContext">The <see cref="CorsRequestContext"/>.</param>
        /// <param name="policy">The <see cref="CorsPolicy"/>.</param>
        /// <param name="result">The <see cref="CorsResult"/>.</param>
        /// <returns><c>true</c> if the requested method is valid; otherwise, <c>false</c>. </returns>
        /// <exception cref="System.ArgumentNullException">
        /// requestContext
        /// or
        /// policy
        /// or
        /// result
        /// </exception>
        public virtual bool TryValidateMethod(CorsRequestContext requestContext, CorsPolicy policy, CorsResult result)
        {
            if (requestContext == null)
            {
                throw new ArgumentNullException("requestContext");
            }
            if (policy == null)
            {
                throw new ArgumentNullException("policy");
            }
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (policy.AllowAnyMethod ||
                policy.Methods.Contains(requestContext.AccessControlRequestMethod))
            {
                result.AllowedMethods.Add(requestContext.AccessControlRequestMethod);
            }
            else
            {
                result.ErrorMessages.Add(String.Format(
                    CultureInfo.CurrentCulture,
                    SRResources.MethodNotAllowed,
                    requestContext.AccessControlRequestMethod));
            }

            return result.IsValid;
        }

        /// <summary>
        /// Try to validate the requested headers based on <see cref="CorsPolicy"/>.
        /// </summary>
        /// <param name="requestContext">The <see cref="CorsRequestContext"/>.</param>
        /// <param name="policy">The <see cref="CorsPolicy"/>.</param>
        /// <param name="result">The <see cref="CorsResult"/>.</param>
        /// <returns><c>true</c> if the requested headers are valid; otherwise, <c>false</c>. </returns>
        /// <exception cref="System.ArgumentNullException">
        /// requestContext
        /// or
        /// policy
        /// or
        /// result
        /// </exception>
        public virtual bool TryValidateHeaders(CorsRequestContext requestContext, CorsPolicy policy, CorsResult result)
        {
            if (requestContext == null)
            {
                throw new ArgumentNullException("requestContext");
            }
            if (policy == null)
            {
                throw new ArgumentNullException("policy");
            }
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (policy.AllowAnyHeader ||
                requestContext.AccessControlRequestHeaders.IsSubsetOf(policy.Headers))
            {
                AddHeaderValues(result.AllowedHeaders, requestContext.AccessControlRequestHeaders);
            }
            else
            {
                result.ErrorMessages.Add(String.Format(
                    CultureInfo.CurrentCulture,
                    SRResources.HeadersNotAllowed,
                    String.Join(",", requestContext.AccessControlRequestHeaders)));
            }

            return result.IsValid;
        }

        /// <summary>
        /// Try to validate the request origin based on <see cref="CorsPolicy"/>.
        /// </summary>
        /// <param name="requestContext">The <see cref="CorsRequestContext"/>.</param>
        /// <param name="policy">The <see cref="CorsPolicy"/>.</param>
        /// <param name="result">The <see cref="CorsResult"/>.</param>
        /// <returns><c>true</c> if the request origin is valid; otherwise, <c>false</c>. </returns>
        /// <exception cref="System.ArgumentNullException">
        /// requestContext
        /// or
        /// policy
        /// or
        /// result
        /// </exception>
        public virtual bool TryValidateOrigin(CorsRequestContext requestContext, CorsPolicy policy, CorsResult result)
        {
            if (requestContext == null)
            {
                throw new ArgumentNullException("requestContext");
            }
            if (policy == null)
            {
                throw new ArgumentNullException("policy");
            }
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (requestContext.Origin != null)
            {
                if (policy.AllowAnyOrigin)
                {
                    if (policy.SupportsCredentials)
                    {
                        result.AllowedOrigin = requestContext.Origin;
                    }
                    else
                    {
                        result.AllowedOrigin = CorsConstants.AnyOrigin;
                    }
                }
                else if (policy.Origins.Contains(requestContext.Origin))
                {
                    result.AllowedOrigin = requestContext.Origin;
                }
                else
                {
                    result.ErrorMessages.Add(String.Format(
                        CultureInfo.CurrentCulture,
                        SRResources.OriginNotAllowed,
                        requestContext.Origin));
                }
            }
            else
            {
                result.ErrorMessages.Add(SRResources.NoOriginHeader);
            }

            return result.IsValid;
        }

        private static void AddHeaderValues(IList<string> target, IEnumerable<string> headerValues)
        {
            foreach (string headerValue in headerValues)
            {
                target.Add(headerValue);
            }
        }
    }
}