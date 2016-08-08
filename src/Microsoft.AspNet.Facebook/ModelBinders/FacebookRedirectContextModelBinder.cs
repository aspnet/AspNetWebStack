// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Web;
using System.Web.Mvc;
using Facebook;
using Microsoft.AspNet.Facebook.Client;

namespace Microsoft.AspNet.Facebook.ModelBinders
{
    /// <summary>
    /// Model binds an action method parameter to a <see cref="FacebookRedirectContext"/>.
    /// </summary>
    public class FacebookRedirectContextModelBinder : IModelBinder
    {
        private FacebookConfiguration _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="FacebookRedirectContextModelBinder" /> class.
        /// </summary>
        /// <param name="config">The <see cref="FacebookConfiguration"/>.</param>
        public FacebookRedirectContextModelBinder(FacebookConfiguration config)
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
            HttpRequestBase request = controllerContext.HttpContext.Request;
            string originUrl = request.QueryString["originUrl"];
            string permissions = request.QueryString["permissions"];

            if (!String.IsNullOrEmpty(originUrl))
            {
                if (!originUrl.StartsWith(_config.AppUrl, StringComparison.OrdinalIgnoreCase))
                {
                    bindingContext.ModelState.AddModelError(bindingContext.ModelName,
                        String.Format(CultureInfo.CurrentCulture, Resources.UrlCannotBeExternal, "originUrl", _config.AppUrl));
                }
            }
            else
            {
                bindingContext.ModelState.AddModelError(bindingContext.ModelName,
                    String.Format(CultureInfo.CurrentCulture, Resources.ParameterIsRequired, "originUrl"));
            }

            string redirectUrl = null;
            string[] requiredPermissions = permissions != null ? permissions.Split(',') : new string[0];
            if (bindingContext.ModelState.IsValid)
            {
                FacebookClient client = _config.ClientProvider.CreateClient();
                // Don't want to redirect to a permissioned URL, the action authorize filters take care of that.
                redirectUrl = client.GetLoginUrl(originUrl, _config.AppId, String.Empty).AbsoluteUri;
            }

            return new FacebookRedirectContext
            {
                OriginUrl = originUrl,
                RequiredPermissions = requiredPermissions,
                RedirectUrl = redirectUrl,
                Configuration = _config
            };
        }
    }
}