// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Helpers.AntiXsrf
{
    // Represents the security token for the Anti-XSRF system.
    // The token is a random 128-bit value that correlates the session with the request body.
    internal sealed class AntiForgeryToken
    {
        internal const int SecurityTokenBitLength = 128;
        internal const int ClaimUidBitLength = 256;

        private string _additionalData;
        private BinaryBlob _securityToken;
        private string _username;

        public string AdditionalData
        {
            get
            {
                return _additionalData ?? String.Empty;
            }
            set
            {
                _additionalData = value;
            }
        }

        public BinaryBlob ClaimUid { get; set; }

        public bool IsSessionToken { get; set; }

        public BinaryBlob SecurityToken
        {
            get
            {
                if (_securityToken == null)
                {
                    _securityToken = new BinaryBlob(SecurityTokenBitLength);
                }
                return _securityToken;
            }
            set
            {
                _securityToken = value;
            }
        }

        public string Username
        {
            get
            {
                return _username ?? String.Empty;
            }
            set
            {
                _username = value;
            }
        }
    }
}
