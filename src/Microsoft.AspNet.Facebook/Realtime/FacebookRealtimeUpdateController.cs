// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Facebook;
using Microsoft.AspNet.Facebook.Models;

namespace Microsoft.AspNet.Facebook.Realtime
{
    /// <summary>
    /// <see cref="ApiController"/> for handling Facebook Realtime Update subscriptions.
    /// </summary>
    public abstract class FacebookRealtimeUpdateController : ApiController
    {
        private const string XHubSignatureHeaderName = "X-Hub-Signature";
        private FacebookConfiguration _facebookConfiguration;

        /// <summary>
        /// Gets the verify token.
        /// </summary>
        /// <value>
        /// The verify token.
        /// </value>
        public abstract string VerifyToken { get; }

        /// <summary>
        /// Gets or sets the <see cref="FacebookConfiguration"/>.
        /// </summary>
        /// <value>
        /// The <see cref="FacebookConfiguration"/>.
        /// </value>
        public FacebookConfiguration FacebookConfiguration
        {
            get
            {
                if (_facebookConfiguration == null)
                {
                    _facebookConfiguration = GlobalFacebookConfiguration.Configuration;
                }
                return _facebookConfiguration;
            }
            set
            {
                _facebookConfiguration = value;
            }
        }

        /// <summary>
        /// Handles the update.
        /// </summary>
        /// <param name="notification">The notification.</param>
        [NonAction]
        public abstract Task HandleUpdateAsync(ChangeNotification notification);

        /// <summary>
        /// Handles the HTTP GET requests from Facebook for subscription verification.
        /// </summary>
        /// <param name="subscriptionVerification">The subscription verification.</param>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Get", Justification = "Needs to be this name to follow routing conventions")]
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "HttpResponseMessage will be disposed by Web API")]
        public virtual HttpResponseMessage Get(SubscriptionVerification subscriptionVerification)
        {
            try
            {
                FacebookClient client = FacebookConfiguration.ClientProvider.CreateClient();
                client.VerifyGetSubscription(subscriptionVerification.Mode, subscriptionVerification.Verify_Token, subscriptionVerification.Challenge, VerifyToken);
            }
            catch (ArgumentException argumentException)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, argumentException);
            }

            return new HttpResponseMessage
            {
                Content = new StringContent(subscriptionVerification.Challenge)
            };
        }

        /// <summary>
        /// Handles the HTTP POST requests from Facebook for updates.
        /// </summary>
        public virtual async Task<HttpResponseMessage> Post()
        {
            IEnumerable<string> headerValues;
            if (Request.Headers.TryGetValues(XHubSignatureHeaderName, out headerValues))
            {
                string signatureHeaderValue = headerValues.FirstOrDefault();
                try
                {
                    string contentString = await Request.Content.ReadAsStringAsync();
                    FacebookClient client = FacebookConfiguration.ClientProvider.CreateClient();
                    ChangeNotification notification = client.VerifyPostSubscription(signatureHeaderValue, contentString, typeof(ChangeNotification)) as ChangeNotification;
                    await HandleUpdateAsync(notification);
                }
                catch (ArgumentException argumentException)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, argumentException);
                }
                catch (HttpResponseException responseException)
                {
                    return responseException.Response;
                }
            }
            else
            {
                return Request.CreateErrorResponse(
                    HttpStatusCode.BadRequest,
                    String.Format(CultureInfo.CurrentCulture, Resources.MissingRequiredHeader, XHubSignatureHeaderName));
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}