// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ServiceModel;

namespace System.Web.Http.SelfHost.Channels
{
    /// <summary>
    /// Specifies the types of security available to a service endpoint configured to use an
    /// <see cref="HttpBinding"/> binding.
    /// </summary>
    public sealed class HttpBindingSecurity
    {
        internal const HttpBindingSecurityMode DefaultMode = HttpBindingSecurityMode.None;

        private HttpBindingSecurityMode _mode;
        private HttpTransportSecurity _transportSecurity;

        /// <summary>
        /// Creates a new instance of the <see cref="HttpBindingSecurity"/> class.
        /// </summary>
        public HttpBindingSecurity()
        {
            _mode = DefaultMode;
            _transportSecurity = new HttpTransportSecurity();
        }

        /// <summary>
        /// Gets or sets the mode of security that is used by an endpoint configured to use an
        /// <see cref="HttpBinding"/> binding.
        /// </summary>
        public HttpBindingSecurityMode Mode
        {
            get { return _mode; }

            set
            {
                HttpBindingSecurityModeHelper.Validate(value, "value");
                _mode = value;
            }
        }

        /// <summary>
        /// Gets or sets an object that contains the transport-level security settings for the 
        /// <see cref="HttpBinding"/> binding.
        /// </summary>
        public HttpTransportSecurity Transport
        {
            get { return _transportSecurity; }

            set { _transportSecurity = value ?? new HttpTransportSecurity(); }
        }
    }
}
