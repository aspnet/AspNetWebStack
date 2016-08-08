// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Web.WebPages.Resources;

namespace System.Web.Mvc
{
    [Serializable]
    [TypeForwardedFrom("System.Web.Mvc, Version=2.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public sealed class HttpAntiForgeryException : HttpException
    {
        public HttpAntiForgeryException()
        {
        }

        private HttpAntiForgeryException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public HttpAntiForgeryException(string message)
            : base(message)
        {
        }

        private HttpAntiForgeryException(string message, params object[] args)
            : this(String.Format(CultureInfo.CurrentCulture, message, args))
        {
        }

        public HttpAntiForgeryException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        internal static HttpAntiForgeryException CreateAdditionalDataCheckFailedException()
        {
            return new HttpAntiForgeryException(WebPageResources.AntiForgeryToken_AdditionalDataCheckFailed);
        }

        internal static HttpAntiForgeryException CreateClaimUidMismatchException()
        {
            return new HttpAntiForgeryException(WebPageResources.AntiForgeryToken_ClaimUidMismatch);
        }

        internal static HttpAntiForgeryException CreateCookieMissingException(string cookieName)
        {
            return new HttpAntiForgeryException(WebPageResources.AntiForgeryToken_CookieMissing, cookieName);
        }

        internal static HttpAntiForgeryException CreateDeserializationFailedException()
        {
            return new HttpAntiForgeryException(WebPageResources.AntiForgeryToken_DeserializationFailed);
        }

        internal static HttpAntiForgeryException CreateFormFieldMissingException(string formFieldName)
        {
            return new HttpAntiForgeryException(WebPageResources.AntiForgeryToken_FormFieldMissing, formFieldName);
        }

        internal static HttpAntiForgeryException CreateSecurityTokenMismatchException()
        {
            return new HttpAntiForgeryException(WebPageResources.AntiForgeryToken_SecurityTokenMismatch);
        }

        internal static HttpAntiForgeryException CreateTokensSwappedException(string cookieName, string formFieldName)
        {
            return new HttpAntiForgeryException(WebPageResources.AntiForgeryToken_TokensSwapped, cookieName, formFieldName);
        }

        internal static HttpAntiForgeryException CreateUsernameMismatchException(string usernameInToken, string currentUsername)
        {
            return new HttpAntiForgeryException(WebPageResources.AntiForgeryToken_UsernameMismatch, usernameInToken, currentUsername);
        }
    }
}
