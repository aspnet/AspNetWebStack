// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Helpers.AntiXsrf.Test
{
    public sealed class MockAntiForgeryConfig : IAntiForgeryConfig
    {
        public IAntiForgeryAdditionalDataProvider AdditionalDataProvider
        {
            get;
            set;
        }

        public string CookieName
        {
            get;
            set;
        }

        public string FormFieldName
        {
            get;
            set;
        }

        public bool RequireSSL
        {
            get;
            set;
        }

        public bool SuppressIdentityHeuristicChecks
        {
            get;
            set;
        }

        public string UniqueClaimTypeIdentifier
        {
            get;
            set;
        }

        public bool SuppressXFrameOptionsHeader
        {
            get;
            set;
        }
    }
}
