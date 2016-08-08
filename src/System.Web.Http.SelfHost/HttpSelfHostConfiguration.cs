// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IdentityModel.Selectors;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.Web.Http.SelfHost.Channels;
using System.Web.Http.SelfHost.Properties;
using System.Web.Http.SelfHost.ServiceModel;
using System.Web.Http.SelfHost.ServiceModel.Channels;

namespace System.Web.Http.SelfHost
{
    /// <summary>
    /// The configuration class for Http Services
    /// </summary>
    public class HttpSelfHostConfiguration : HttpConfiguration
    {
        private const int DefaultMaxConcurrentRequests = 100;
        private const int DefaultMaxBufferSize = 64 * 1024;
        private const int DefaultReceivedMessageSize = 64 * 1024;

        private const int MinConcurrentRequests = 1;
        private const int MinBufferSize = 1;
        private const int MinReceivedMessageSize = 1;

        private static readonly TimeSpan DefaultReceiveTimeout = new TimeSpan(0, 10, 0);
        private static readonly TimeSpan DefaultSendTimeout = new TimeSpan(0, 1, 0);

        private Uri _baseAddress;
        private int _maxConcurrentRequests;
        private ServiceCredentials _credentials = new ServiceCredentials();
        private HttpClientCredentialType _clientCredentialType = HttpClientCredentialType.None;
        private TransferMode _transferMode;
        private int _maxBufferSize = DefaultMaxBufferSize;
        private bool _maxBufferSizeIsInitialized;
        private long _maxReceivedMessageSize = DefaultReceivedMessageSize;
        private TimeSpan _receiveTimeout = DefaultReceiveTimeout;
        private TimeSpan _sendTimeout = DefaultSendTimeout;
        private HostNameComparisonMode _hostNameComparisonMode;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpSelfHostConfiguration"/> class.
        /// </summary>
        /// <param name="baseAddress">The base address.</param>
        public HttpSelfHostConfiguration(string baseAddress)
            : this(CreateBaseAddress(baseAddress))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpSelfHostConfiguration"/> class.
        /// </summary>
        /// <param name="baseAddress">The base address.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "caller owns object")]
        public HttpSelfHostConfiguration(Uri baseAddress)
            : base(new HttpRouteCollection(ValidateBaseAddress(baseAddress).AbsolutePath))
        {
            _baseAddress = ValidateBaseAddress(baseAddress);
            _maxConcurrentRequests = MultiplyByProcessorCount(DefaultMaxConcurrentRequests);
            _maxBufferSize = TransportDefaults.MaxBufferSize;
            _maxReceivedMessageSize = TransportDefaults.MaxReceivedMessageSize;
        }

        /// <summary>
        /// Gets the base address.
        /// </summary>
        /// <value>
        /// The base address.
        /// </value>
        public Uri BaseAddress
        {
            get { return _baseAddress; }
        }

        /// <summary>
        /// Gets or sets the upper limit of how many concurrent <see cref="T:System.Net.Http.HttpRequestMessage"/> instances 
        /// can be processed at any given time. The default is 100 times the number of CPU cores.
        /// </summary>
        /// <value>
        /// The maximum concurrent <see cref="T:System.Net.Http.HttpRequestMessage"/> instances processed at any given time.
        /// </value>
        public int MaxConcurrentRequests
        {
            get { return _maxConcurrentRequests; }

            set
            {
                if (value < MinConcurrentRequests)
                {
                    throw Error.ArgumentMustBeGreaterThanOrEqualTo("value", value, MinConcurrentRequests);
                }
                _maxConcurrentRequests = value;
            }
        }

        /// <summary>
        /// Gets or sets the transfer mode.
        /// </summary>
        /// <value>
        /// The transfer mode.
        /// </value>
        public TransferMode TransferMode
        {
            get { return _transferMode; }

            set
            {
                TransferModeHelper.Validate(value, "value");
                _transferMode = value;
            }
        }

        /// <summary>
        /// Specifies how the host name should be used in URI comparisons when dispatching an incoming message.
        /// </summary>
        public HostNameComparisonMode HostNameComparisonMode
        {
            get { return _hostNameComparisonMode; }

            set
            {
                HostNameComparisonModeHelper.Validate(value, "value");
                _hostNameComparisonMode = value;
            }
        }

        /// <summary>
        /// Gets or sets the size of the max buffer.
        /// </summary>
        /// <value>
        /// The size of the max buffer.
        /// </value>
        public int MaxBufferSize
        {
            get
            {
                if (_maxBufferSizeIsInitialized || TransferMode != TransferMode.Buffered)
                {
                    return _maxBufferSize;
                }

                long maxReceivedMessageSize = MaxReceivedMessageSize;
                if (maxReceivedMessageSize > Int32.MaxValue)
                {
                    return Int32.MaxValue;
                }
                return (int)maxReceivedMessageSize;
            }

            set
            {
                if (value < MinBufferSize)
                {
                    throw Error.ArgumentMustBeGreaterThanOrEqualTo("value", value, MinBufferSize);
                }
                _maxBufferSizeIsInitialized = true;
                _maxBufferSize = value;
            }
        }

        /// <summary>
        /// Gets or sets the size of the max received message.
        /// </summary>
        /// <value>
        /// The size of the max received message.
        /// </value>
        public long MaxReceivedMessageSize
        {
            get { return _maxReceivedMessageSize; }

            set
            {
                if (value < MinReceivedMessageSize)
                {
                    throw Error.ArgumentMustBeGreaterThanOrEqualTo("value", value, MinReceivedMessageSize);
                }
                _maxReceivedMessageSize = value;
            }
        }

        /// <summary>
        /// Gets or sets the interval of time that a connection can remain inactive, during which no application messages are received, before it is dropped.
        /// </summary>
        /// <value>
        /// The interval of time that a connection can remain inactive, during which no application messages are received, before it is dropped.
        /// </value>
        public TimeSpan ReceiveTimeout
        {
            get { return _receiveTimeout; }

            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw Error.ArgumentMustBeGreaterThanOrEqualTo("value", value, TimeSpan.Zero);
                }

                _receiveTimeout = value;
            }
        }

        /// <summary>
        /// Gets or sets the interval of time provided for a write operation to complete before the transport raises an exception.
        /// </summary>
        /// <value>
        /// The interval of time provided for a write operation to complete before the transport raises an exception.
        /// </value>
        public TimeSpan SendTimeout
        {
            get { return _sendTimeout; }

            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw Error.ArgumentMustBeGreaterThanOrEqualTo("value", value, TimeSpan.Zero);
                }

                _sendTimeout = value;
            }
        }

        /// <summary>
        /// Gets or sets UserNamePasswordValidator so that it can be used to validate the username and password
        /// sent over HTTP or HTTPS
        /// </summary>
        /// <value>
        /// The server certificate.
        /// </value>
        public UserNamePasswordValidator UserNamePasswordValidator
        {
            get { return _credentials.UserNameAuthentication.CustomUserNamePasswordValidator; }

            set 
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }

                _clientCredentialType = HttpClientCredentialType.Basic;
                _credentials.UserNameAuthentication.CustomUserNamePasswordValidator = value;
                _credentials.UserNameAuthentication.UserNamePasswordValidationMode = UserNamePasswordValidationMode.Custom;
            }
        }

        /// <summary>
        /// Gets or sets X509CertificateValidator so that it can be used to validate the client certificate
        /// sent over HTTPS
        /// </summary>
        /// <value>
        /// The server certificate.
        /// </value>
        public X509CertificateValidator X509CertificateValidator
        {
            get { return _credentials.ClientCertificate.Authentication.CustomCertificateValidator; }

            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }

                _clientCredentialType = HttpClientCredentialType.Certificate;
                _credentials.ClientCertificate.Authentication.CustomCertificateValidator = value;
                _credentials.ClientCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.Custom;
            }
        }

        /// <summary>
        /// Gets/Sets the ClientCredentialType that server is expecting. 
        /// </summary>
        /// <value>
        /// The default value is HttpClientCredentialType.None.
        /// </value>
        public HttpClientCredentialType ClientCredentialType
        {
            get { return _clientCredentialType; }
            set { _clientCredentialType = value; }
        }

        /// <summary>
        /// Internal method called to configure <see cref="HttpBinding"/> settings.
        /// </summary>
        /// <param name="httpBinding">Http binding.</param>
        /// <returns>The <see cref="BindingParameterCollection"/> to use when building the <see cref="IChannelListener"/> or null if no binding parameters are present.</returns>
        internal BindingParameterCollection ConfigureBinding(HttpBinding httpBinding)
        {
            return OnConfigureBinding(httpBinding);
        }

        /// <summary>
        /// Called to apply the configuration on the endpoint level.
        /// </summary>
        /// <param name="httpBinding">Http endpoint.</param>
        /// <returns>The <see cref="BindingParameterCollection"/> to use when building the <see cref="IChannelListener"/> or null if no binding parameters are present.</returns>
        protected virtual BindingParameterCollection OnConfigureBinding(HttpBinding httpBinding)
        {
            if (httpBinding == null)
            {
                throw Error.ArgumentNull("httpBinding");
            }

            if (_clientCredentialType != HttpClientCredentialType.Basic && _credentials.UserNameAuthentication.CustomUserNamePasswordValidator != null)
            {
                throw Error.InvalidOperation(SRResources.CannotUseOtherClientCredentialTypeWithUserNamePasswordValidator);
            }

            if (_clientCredentialType != HttpClientCredentialType.Certificate && _credentials.ClientCertificate.Authentication.CustomCertificateValidator != null)
            {
                throw Error.InvalidOperation(SRResources.CannotUseOtherClientCredentialTypeWithX509CertificateValidator);
            }

            httpBinding.MaxBufferSize = MaxBufferSize;
            httpBinding.MaxReceivedMessageSize = MaxReceivedMessageSize;
            httpBinding.TransferMode = TransferMode;
            httpBinding.HostNameComparisonMode = HostNameComparisonMode;
            httpBinding.ReceiveTimeout = ReceiveTimeout;
            httpBinding.SendTimeout = SendTimeout;

            // Set up binding parameters
            if (_baseAddress.Scheme == Uri.UriSchemeHttps)
            {
                // we need to use SSL
                httpBinding.Security = new HttpBindingSecurity()
                {
                    Mode = HttpBindingSecurityMode.Transport,
                };
            }
            
            if (_clientCredentialType != HttpClientCredentialType.None)
            {       
                if (httpBinding.Security == null || httpBinding.Security.Mode == HttpBindingSecurityMode.None)
                {
                    // Basic over HTTP case
                    httpBinding.Security = new HttpBindingSecurity()
                    {
                        Mode = HttpBindingSecurityMode.TransportCredentialOnly,
                    };
                }

                httpBinding.Security.Transport.ClientCredentialType = _clientCredentialType;                
            }

            if (UserNamePasswordValidator != null || X509CertificateValidator != null)
            {
                // those are the only two things that affect service credentials
                return AddCredentialsToBindingParameters();
            }
            else
            {
                return null;
            }
        }

        private BindingParameterCollection AddCredentialsToBindingParameters()
        {
            BindingParameterCollection bindingParameters = new BindingParameterCollection();
            bindingParameters.Add(_credentials);
            return bindingParameters;
        }

        private static Uri CreateBaseAddress(string baseAddress)
        {
            if (baseAddress == null)
            {
                throw Error.ArgumentNull("baseAddress");
            }

            return new Uri(baseAddress, UriKind.RelativeOrAbsolute);
        }

        private static Uri ValidateBaseAddress(Uri baseAddress)
        {
            if (baseAddress == null)
            {
                throw Error.ArgumentNull("baseAddress");
            }

            if (!baseAddress.IsAbsoluteUri)
            {
                throw Error.ArgumentUriNotAbsolute("baseAddress", baseAddress);
            }

            if (!String.IsNullOrEmpty(baseAddress.Query) || !String.IsNullOrEmpty(baseAddress.Fragment))
            {
                throw Error.ArgumentUriHasQueryOrFragment("baseAddress", baseAddress);
            }

            if (!ReferenceEquals(baseAddress.Scheme, Uri.UriSchemeHttp) && !ReferenceEquals(baseAddress.Scheme, Uri.UriSchemeHttps))
            {
                throw Error.ArgumentUriNotHttpOrHttpsScheme("baseAddress", baseAddress);
            }

            return baseAddress;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We never want to fail here so we have to catch all exceptions.")]
        internal static int MultiplyByProcessorCount(int value)
        {
            Contract.Assert(value > 0);
            try
            {
                return Math.Max(Environment.ProcessorCount * value, value);
            }
            catch
            {
                return value;
            }
        }
    }
}
