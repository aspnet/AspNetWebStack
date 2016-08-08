// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Web.Mvc;
using Facebook;
using Microsoft.AspNet.Facebook.Client;

namespace Microsoft.AspNet.Facebook.ModelBinders
{
    /// <summary>
    /// Model binds an action method parameter to a <see cref="FacebookContext"/>.
    /// </summary>
    public class FacebookContextModelBinder : IModelBinder
    {
        private FacebookConfiguration _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="FacebookContextModelBinder" /> class.
        /// </summary>
        /// <param name="config">The <see cref="FacebookConfiguration"/>.</param>
        public FacebookContextModelBinder(FacebookConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            _config = config;
        }

        /// <summary>
        /// Binds the model to a value by using the specified controller context and binding context.
        /// </summary>
        /// <param name="controllerContext">The controller context.</param>
        /// <param name="bindingContext">The binding context.</param>
        /// <returns>
        /// The bound value.
        /// </returns>
        public virtual object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            FacebookClient client = _config.ClientProvider.CreateClient();
            dynamic signedRequest = FacebookRequestHelpers.GetSignedRequest(
                controllerContext.HttpContext,
                rawSignedRequest =>
                {
                    return client.ParseSignedRequest(rawSignedRequest);
                });
            if (signedRequest != null)
            {
                string accessToken = signedRequest.oauth_token;
                string userId = signedRequest.user_id;
                client.AccessToken = accessToken;
                return new FacebookContext
                {
                    Client = client,
                    SignedRequest = signedRequest,
                    AccessToken = accessToken,
                    UserId = userId,
                    Configuration = _config
                };
            }
            else
            {
                bindingContext.ModelState.AddModelError(bindingContext.ModelName, Resources.MissingSignedRequest);
            }

            return null;
        }
    }
}