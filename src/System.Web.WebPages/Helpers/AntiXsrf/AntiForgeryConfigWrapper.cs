// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Helpers.AntiXsrf
{
    internal sealed class AntiForgeryConfigWrapper : IAntiForgeryConfig
    {
        public IAntiForgeryAdditionalDataProvider AdditionalDataProvider
        {
            get
            {
                return AntiForgeryConfig.AdditionalDataProvider;
            }
        }

        public string CookieName
        {
            get { return AntiForgeryConfig.CookieName; }
        }

        public string FormFieldName
        {
            get { return AntiForgeryConfig.AntiForgeryTokenFieldName; }
        }

        public bool RequireSSL
        {
            get { return AntiForgeryConfig.RequireSsl; }
        }

        public bool SuppressIdentityHeuristicChecks
        {
            get { return AntiForgeryConfig.SuppressIdentityHeuristicChecks; }
        }

        public string UniqueClaimTypeIdentifier
        {
            get { return AntiForgeryConfig.UniqueClaimTypeIdentifier; }
        }

        public bool SuppressXFrameOptionsHeader
        {
            get { return AntiForgeryConfig.SuppressXFrameOptionsHeader; }
        }
    }
}
