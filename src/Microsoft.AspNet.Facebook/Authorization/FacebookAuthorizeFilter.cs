// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Facebook;
using Microsoft.AspNet.Facebook.Client;

namespace Microsoft.AspNet.Facebook.Authorization
{
    /// <summary>
    /// Authorization filter that verifies the signed requests and permissions from Facebook.
    /// </summary>
    public class FacebookAuthorizeFilter : IAuthorizationFilter
    {
        private static readonly Uri FacebookUri = new Uri("https://www.facebook.com/");
        private static readonly Uri DefaultAuthorizationRedirectUrl = FacebookUri;
        private static readonly Uri DefaultCannotCreateCookiesRedirectPath = FacebookUri;
        // Facebook Missing Permissions, shortened version to not add excessively long query string parameters to the url.
        private const string MissingPermissionsQueryName = "__fb_mps";

        private FacebookConfiguration _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="FacebookAuthorizeFilter" /> class.
        /// </summary>
        /// <param name="config">The <see cref="FacebookConfiguration"/>.</param>
        public FacebookAuthorizeFilter(FacebookConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            _config = config;
        }

        /// <summary>
        /// Called when authorization is required.
        /// </summary>
        /// <param name="filterContext">The filter context.</param>
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Type references are needed for authorization")]
        public virtual void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }

            IEnumerable<FacebookAuthorizeAttribute> authorizeAttributes = filterContext.ActionDescriptor
                .GetCustomAttributes(typeof(FacebookAuthorizeAttribute), inherit: true)
                .Union(filterContext.ActionDescriptor.ControllerDescriptor
                    .GetCustomAttributes(typeof(FacebookAuthorizeAttribute), inherit: true))
                .OfType<FacebookAuthorizeAttribute>();
            if (!authorizeAttributes.Any())
            {
                return;
            }

            FacebookClient client = _config.ClientProvider.CreateClient();
            HttpRequestBase request = filterContext.HttpContext.Request;
            dynamic signedRequest = FacebookRequestHelpers.GetSignedRequest(
                filterContext.HttpContext,
                rawSignedRequest =>
                {
                    return client.ParseSignedRequest(rawSignedRequest);
                });
            string userId = null;
            string accessToken = null;
            if (signedRequest != null)
            {
                userId = signedRequest.user_id;
                accessToken = signedRequest.oauth_token;
            }

            NameValueCollection parsedQueries = HttpUtility.ParseQueryString(request.Url.Query);
            HashSet<string> requiredPermissions = PermissionHelper.GetRequiredPermissions(authorizeAttributes);

            bool handleError = !String.IsNullOrEmpty(parsedQueries["error"]);

            // This must occur AFTER the handleError calculation because it modifies the parsed queries.
            string redirectUrl = GetRedirectUrl(request, parsedQueries);

            // Check if there was an error and we should handle it.
            if (handleError)
            {
                Uri errorUrl;

                if (String.IsNullOrEmpty(_config.AuthorizationRedirectPath))
                {
                    errorUrl = DefaultAuthorizationRedirectUrl;
                }
                else
                {
                    errorUrl = GetErroredAuthorizeUri(redirectUrl, requiredPermissions);
                }

                filterContext.Result = CreateRedirectResult(errorUrl);

                // There was an error so short circuit
                return;
            }

            FacebookContext facebookContext = new FacebookContext
            {
                Client = client,
                SignedRequest = signedRequest,
                AccessToken = accessToken,
                UserId = userId,
                Configuration = _config
            };

            PermissionContext permissionContext = new PermissionContext
            {
                FacebookContext = facebookContext,
                FilterContext = filterContext,
                RequiredPermissions = requiredPermissions,
            };

            // Check if we need to prompt for default permissions.
            if (signedRequest == null || String.IsNullOrEmpty(userId) || String.IsNullOrEmpty(accessToken))
            {
                PromptDefaultPermissions(permissionContext, redirectUrl);
            }
            else if (requiredPermissions.Any())
            {
                PermissionsStatus currentPermissionsStatus = _config.PermissionService.GetUserPermissionsStatus(userId, accessToken);
                // Instead of performing another request to gather "granted" permissions just parse the status
                IEnumerable<string> currentPermissions = PermissionHelper.GetGrantedPermissions(currentPermissionsStatus);
                IEnumerable<string> missingPermissions = requiredPermissions.Except(currentPermissions);

                // If we have missing permissions than we need to present a prompt or redirect to an error 
                // page if there's an error.
                if (missingPermissions.Any())
                {
                    permissionContext.MissingPermissions = missingPermissions;
                    permissionContext.DeclinedPermissions = PermissionHelper.GetDeclinedPermissions(currentPermissionsStatus);
                    permissionContext.SkippedPermissions = PermissionHelper.GetSkippedPermissions(
                        filterContext.HttpContext.Request,
                        missingPermissions,
                        permissionContext.DeclinedPermissions);

                    // Add a query string parameter that enables us to identify if we've already prompted for missing permissions
                    // and therefore can detect cookies.
                    AddCookieVerificationQuery(parsedQueries);
                    // Rebuild the redirect Url so it contains the new query string parameter.
                    redirectUrl = GetRedirectUrl(request, parsedQueries);

                    PromptMissingPermissions(permissionContext, redirectUrl);
                }
            }
        }

        /// <summary>
        /// Adds a query string parameter to a <see cref="NameValueCollection"/> that enables a 
        /// <see cref="FacebookAuthorizeFilter"/> to detect when cookies are unavailable and then trigger the 
        /// <see cref="OnCannotCreateCookies"/> hook.
        /// </summary>
        /// <param name="queries">List of query parameters that are used to create a url.</param>
        protected static void AddCookieVerificationQuery(NameValueCollection queries)
        {
            // Add a query string parameter that enables us to identify if we've already prompted for missing permissions
            queries.Add(MissingPermissionsQueryName, "true");
        }

        /// <summary>
        /// Called when authorization fails and need to create a redirect result.
        /// </summary>
        /// <param name="redirectUrl">The redirect URL.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public virtual JavaScriptRedirectResult CreateRedirectResult(Uri redirectUrl)
        {
            if (redirectUrl == null)
            {
                throw new ArgumentNullException("redirectUrl");
            }

            return new JavaScriptRedirectResult(redirectUrl);
        }

        /// <summary>
        /// Returns an <see cref="ActionResult"/> that indicates we want to show a permission prompt.  Should only be used as a
        /// return value within the <see cref="OnPermissionPrompt"/> and <see cref="OnDeniedPermissionPrompt"/> methods.
        /// </summary>
        /// <returns>An <see cref="ActionResult"/> that indicates that we want to show a permission prompt.</returns>
        protected ShowPromptResult ShowPrompt(PermissionContext context)
        {
            FacebookClient client = context.FacebookContext.Client;
            Uri navigationUrl = client.GetLoginUrl(context.RedirectUrl,
                                                   _config.AppId,
                                                   permissions: String.Join(",", context.RequiredPermissions));

            return new ShowPromptResult(navigationUrl);
        }

        /// <summary>
        /// Invoked during <see cref="OnAuthorization"/> after determining that cookies cannot be created. Default behavior
        /// redirects to a no cookies error page.
        /// </summary>
        /// <param name="context">Provides access to permission information associated with the user.</param>
        protected virtual void OnCannotCreateCookies(PermissionContext context)
        {
            Uri redirectPath;

            if (String.IsNullOrEmpty(_config.CannotCreateCookieRedirectPath))
            {
                redirectPath = DefaultCannotCreateCookiesRedirectPath;
            }
            else
            {
                redirectPath = GetCannotCreateCookiesUri();
            }

            context.Result = CreateRedirectResult(redirectPath);
        }

        /// <summary>
        /// Invoked during <see cref="OnAuthorization"/> when a prompt requests permissions that were skipped or revoked.
        /// Set the <paramref name="context"/>'s Result property to modify login flow.
        /// </summary>
        /// <param name="context">Provides access to permission information associated with the user.</param>
        protected virtual void OnDeniedPermissionPrompt(PermissionContext context)
        {
        }

        /// <summary>
        /// Invoked during <see cref="OnAuthorization"/> prior to a permission prompt that is requesting permissions that have
        /// not been requested before. Set the <paramref name="context"/>'s Result property to modify login flow.
        /// </summary>
        /// <param name="context">Provides access to permission information associated with the user.</param>
        protected virtual void OnPermissionPrompt(PermissionContext context)
        {
            context.Result = ShowPrompt(context);
        }

        private Uri GetErroredAuthorizeUri(string originUrl, HashSet<string> requiredPermissions)
        {
            if (requiredPermissions == null)
            {
                throw new ArgumentNullException("requiredPermissions");
            }

            string requiredPermissionString = String.Join(",", requiredPermissions);
            UriBuilder authorizationUrlBuilder = new UriBuilder(new Uri(_config.AppUrl));
            authorizationUrlBuilder.Path += _config.AuthorizationRedirectPath.Substring(1);
            authorizationUrlBuilder.Query = String.Format(CultureInfo.InvariantCulture,
                "originUrl={0}&permissions={1}",
                HttpUtility.UrlEncode(originUrl),
                HttpUtility.UrlEncode(requiredPermissionString));

            return authorizationUrlBuilder.Uri;
        }

        private Uri GetCannotCreateCookiesUri()
        {
            UriBuilder noCookiesUrlBuilder = new UriBuilder(new Uri(_config.AppUrl));
            noCookiesUrlBuilder.Path += _config.CannotCreateCookieRedirectPath.Substring(1);

            return noCookiesUrlBuilder.Uri;
        }

        private string GetRedirectUrl(HttpRequestBase request)
        {
            NameValueCollection queryNameValuePair = HttpUtility.ParseQueryString(request.Url.Query);
            return GetRedirectUrl(request, queryNameValuePair);
        }

        private string GetRedirectUrl(HttpRequestBase request, NameValueCollection queryNameValuePair)
        {
            // Don't propagate query strings added by Facebook OAuth Dialog
            queryNameValuePair.Remove("code");
            queryNameValuePair.Remove("error");
            queryNameValuePair.Remove("error_reason");
            queryNameValuePair.Remove("error_description");

            string redirectUrl = String.Format(
                CultureInfo.InvariantCulture,
                "{0}/{1}",
                _config.AppUrl.TrimEnd('/'),
                request.AppRelativeCurrentExecutionFilePath.TrimStart('~', '/'));

            string query = queryNameValuePair.ToString();
            if (!String.IsNullOrEmpty(query))
            {
                redirectUrl += "?" + query;
            }

            return redirectUrl;
        }

        private void PromptDefaultPermissions(PermissionContext permissionContext, string redirectUrl)
        {
            FacebookClient client = permissionContext.FacebookContext.Client;
            // Cannot obtain user information from signed_request, redirect to Facebook OAuth dialog.
            Uri navigationUrl = client.GetLoginUrl(redirectUrl,
                                                   _config.AppId,
                                                   permissions: null);

            permissionContext.FilterContext.Result = CreateRedirectResult(navigationUrl);
        }

        private void PromptMissingPermissions(PermissionContext permissionContext, string redirectUrl)
        {
            AuthorizationContext filterContext = permissionContext.FilterContext;
            HashSet<string> requiredPermissions = permissionContext.RequiredPermissions;

            // If there were no errors it means that we will be prompted with a permission prompt.
            // Therefore, invoke the permission prompt hooks and navigate to the prompt.

            IEnumerable<string> declinedPermissions = permissionContext.DeclinedPermissions;
            IEnumerable<string> skippedPermissions = permissionContext.SkippedPermissions;
            IEnumerable<string> missingPermissions = permissionContext.MissingPermissions;

            // Declined permissions and skipped permissions can persist through multiple pages.  So we need to cross check
            // them against the current pages permissions, this will determine if we should invoke the denied permission hook.
            bool deniedPermissions = missingPermissions.Where(
                permission => declinedPermissions.Contains(permission) ||
                              skippedPermissions.Contains(permission)).Any();

            permissionContext.RedirectUrl = redirectUrl;

            // See if our persisted permissions cookie doesn't exist and if we've had missing permissions before.
            // Essentially this checks to see if we've tried to persist permissions before and were unsuccessful due to an
            // inability to create cookies.
            if (!PermissionHelper.RequestedPermissionsCookieExists(filterContext.HttpContext.Request) &&
                filterContext.HttpContext.Request.QueryString.Get(MissingPermissionsQueryName) != null)
            {
                OnCannotCreateCookies(permissionContext);
            }
            else
            {
                // The DeniedPermissionPromptHook will only be invoked if we detect there are denied permissions.
                // It is attempted instead of the permission hook to allow app creators to handle situations when a user
                // skip's or revokes previously prompted permissions. Ex: redirect to a different page.
                if (deniedPermissions)
                {
                    OnDeniedPermissionPrompt(permissionContext);
                }
                else
                {
                    OnPermissionPrompt(permissionContext);
                }

                // We persist the requested permissions in a cookie to know if a permission was denied in any way.
                // The persisted data allows us to detect skipping of permissions.
                PermissionHelper.PersistRequestedPermissions(filterContext, requiredPermissions);
            }

            filterContext.Result = permissionContext.Result;
        }
    }
}