// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Web.Http.SelfHost.ServiceModel;
using System.Web.Http.SelfHost.ServiceModel.Channels;

namespace System.Web.Http.SelfHost.Channels
{   
    /// <summary>
    /// A binding used with endpoints for web services that use strongly-type HTTP request 
    /// and response messages.
    /// </summary>
    public class HttpBinding : Binding, IBindingRuntimePreferences
    {
        internal const string CollectionElementName = "httpBinding";
        internal const TransferMode DefaultTransferMode = System.ServiceModel.TransferMode.Buffered;
        
        private HttpsTransportBindingElement _httpsTransportBindingElement;
        private HttpTransportBindingElement _httpTransportBindingElement;
        private HttpBindingSecurity _security;
        private HttpMessageEncodingBindingElement _httpMessageEncodingBindingElement;
        private Action<HttpTransportBindingElement> _configureTransportBindingElement;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpBinding"/> class.
        /// </summary>
        public HttpBinding()
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpBinding"/> class with the 
        /// type of security used by the binding explicitly specified.
        /// </summary>
        /// <param name="securityMode">The value of <see cref="HttpBindingSecurityMode"/> that 
        /// specifies the type of security that is used to configure a service endpoint using the
        /// <see cref="HttpBinding"/> binding.
        /// </param>
        public HttpBinding(HttpBindingSecurityMode securityMode)
            : this()
        {
            _security.Mode = securityMode;
        }

        /// <summary>
        /// Gets the envelope version that is used by endpoints that are configured to use an 
        /// <see cref="HttpBinding"/> binding.  Always returns <see cref="System.ServiceModel.EnvelopeVersion.None"/>.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "This is existing public API")]
        public EnvelopeVersion EnvelopeVersion
        {
            get { return EnvelopeVersion.None; }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the hostname is used to reach the 
        /// service when matching the URI.
        /// </summary>
        [DefaultValue(HttpTransportDefaults.HostNameComparisonMode)]
        public HostNameComparisonMode HostNameComparisonMode
        {
            get { return _httpTransportBindingElement.HostNameComparisonMode; }

            set
            {
                _httpTransportBindingElement.HostNameComparisonMode = value;
                _httpsTransportBindingElement.HostNameComparisonMode = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum amount of memory allocated for the buffer manager that manages the buffers 
        /// required by endpoints that use this binding.
        /// </summary>
        [DefaultValue(TransportDefaults.MaxBufferPoolSize)]
        public long MaxBufferPoolSize
        {
            get { return _httpTransportBindingElement.MaxBufferPoolSize; }

            set
            {
                _httpTransportBindingElement.MaxBufferPoolSize = value;
                _httpsTransportBindingElement.MaxBufferPoolSize = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum amount of memory that is allocated for use by the manager of the message 
        /// buffers that receive messages from the channel.
        /// </summary>
        [DefaultValue(TransportDefaults.MaxBufferSize)]
        public int MaxBufferSize
        {
            get { return _httpTransportBindingElement.MaxBufferSize; }

            set
            {
                _httpTransportBindingElement.MaxBufferSize = value;
                _httpsTransportBindingElement.MaxBufferSize = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum size for a message that can be processed by the binding.
        /// </summary>
        [DefaultValue(TransportDefaults.MaxReceivedMessageSize)]
        public long MaxReceivedMessageSize
        {
            get { return _httpTransportBindingElement.MaxReceivedMessageSize; }

            set
            {
                _httpTransportBindingElement.MaxReceivedMessageSize = value;
                _httpsTransportBindingElement.MaxReceivedMessageSize = value;
            }
        }

        /// <summary>
        /// Gets or sets the delegate which can configure the <see cref="HttpTransportBindingElement"/> that this binding creates.
        /// </summary>
        public Action<HttpTransportBindingElement> ConfigureTransportBindingElement
        {
            get { return _configureTransportBindingElement; }

            set 
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }

                _configureTransportBindingElement = value;              
            }
        }

        /// <summary>
        /// Gets the URI transport scheme for the channels and listeners that are configured 
        /// with this binding. (Overrides <see cref="System.ServiceModel.Channels.Binding.Scheme">
        /// Binding.Scheme</see>.)
        /// </summary>
        public override string Scheme
        {
            get { return GetTransport().Scheme; }
        }

        /// <summary>
        /// Gets or sets the security settings used with this binding. 
        /// </summary>
        public HttpBindingSecurity Security
        {
            get { return _security; }

            set
            {
                if (value == null)
                {
                    throw Error.ArgumentNull("value");
                }

                _security = value;
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the service configured with the 
        /// binding uses streamed or buffered (or both) modes of message transfer.
        /// </summary>
        [DefaultValue(HttpTransportDefaults.TransferMode)]
        public TransferMode TransferMode
        {
            get { return _httpTransportBindingElement.TransferMode; }

            set
            {
                _httpTransportBindingElement.TransferMode = value;
                _httpsTransportBindingElement.TransferMode = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether incoming requests can be handled more efficiently synchronously or asynchronously.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "This is the pattern used by all standard bindings.")]
        bool IBindingRuntimePreferences.ReceiveSynchronously
        {
            get { return false; }
        }

        /// <summary>
        /// Returns an ordered collection of binding elements contained in the current binding. 
        /// (Overrides <see cref="System.ServiceModel.Channels.Binding.CreateBindingElements">
        /// Binding.CreateBindingElements</see>.)
        /// </summary>
        /// <returns>
        /// An ordered collection of binding elements contained in the current binding.
        /// </returns>
        public override BindingElementCollection CreateBindingElements()
        {
            BindingElementCollection bindingElements = new BindingElementCollection();

            bindingElements.Add(_httpMessageEncodingBindingElement);
            bindingElements.Add(GetTransport());

            return bindingElements.Clone();
        }

        private TransportBindingElement GetTransport()
        {
            HttpTransportBindingElement result = null;

            if (_security.Mode == HttpBindingSecurityMode.Transport)
            {
                _security.Transport.ConfigureTransportProtectionAndAuthentication(_httpsTransportBindingElement);
                result = _httpsTransportBindingElement;
            }
            else if (_security.Mode == HttpBindingSecurityMode.TransportCredentialOnly)
            {
                _security.Transport.ConfigureTransportAuthentication(_httpTransportBindingElement);
                result = _httpTransportBindingElement;
            }
            else
            {
                _security.Transport.DisableTransportAuthentication(_httpTransportBindingElement);
                result = _httpTransportBindingElement;
            }

            // give customer final chance to customize the auth scheme
            if (_configureTransportBindingElement != null)
            {
                _configureTransportBindingElement(result);
            }

            return result;
        }

        private void Initialize()
        {
            _security = new HttpBindingSecurity();

            _httpTransportBindingElement = new HttpTransportBindingElement();
            _httpTransportBindingElement.ManualAddressing = true;

            _httpsTransportBindingElement = new HttpsTransportBindingElement();
            _httpsTransportBindingElement.ManualAddressing = true;

            _httpMessageEncodingBindingElement = new HttpMessageEncodingBindingElement();
        }
    }
}
