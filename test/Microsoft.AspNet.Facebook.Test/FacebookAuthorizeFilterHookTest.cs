// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Facebook.Authorization;
using Microsoft.AspNet.Facebook.Test.Helpers;
using Microsoft.TestCommon;

namespace Microsoft.AspNet.Facebook.Test
{
    public class FacebookAuthorizeFilterHookTest
    {
        [Theory]
        [InlineData("~/home/cannotcreatecookies", "https://apps.facebook.com/DefaultAppId/home/cannotcreatecookies")]
        [InlineData(null, "https://www.facebook.com/")]
        public void OnAuthorization_CannotCreateCookiesHookRedirectsToConfigValueOrDefault(
            string cannotCreateCookiesRedirectPath,
            string expectedRedirectPath)
        {
            // Arrange
            var config = BuildConfiguration("~/home/permissions", cannotCreateCookiesRedirectPath);
            var authorizeFilter = new FacebookAuthorizeFilter(config);
            var context = BuildSignedAuthorizationContext("http://contoso.com?__fb_mps=true", "email");

            // Act
            authorizeFilter.OnAuthorization(context);
            var result = context.Result as JavaScriptRedirectResult;

            // Assert
            Assert.Equal(result.RedirectUrl.AbsoluteUri, new Uri(expectedRedirectPath).AbsoluteUri);
        }

        [Fact]
        public void OnAuthorization_OnlyTriggersCannotCreateCookiesHook()
        {
            // Arrange
            var config = BuildConfiguration("~/home/permissions");
            var authorizeFilter = new CustomDefaultAuthorizeFilter(config);
            var context = BuildSignedAuthorizationContext("http://contoso.com?__fb_mps=true", "email");

            // Act
            authorizeFilter.OnAuthorization(context);

            // Assert
            Assert.True(authorizeFilter.CannotCreateCookiesHookTriggered);
            Assert.False(authorizeFilter.PermissionPromptHookTriggered);
            Assert.False(authorizeFilter.DeniedPermissionPromptHookTriggered);
        }

        [Theory]
        [InlineData("http://contoso.com?__fb_mps=true", "email", true)]
        [InlineData("http://contoso.com", "email", false)]
        [InlineData("http://contoso.com?__fb_mps=true", null, false)]
        public void OnAuthorization_TriggersCannotCreateCookiesHook(string requestUrl,
                                                                    string permission,
                                                                    bool expectedTrigger)
        {
            // Arrange
            var config = BuildConfiguration("~/home/permissions");
            var authorizeFilter = new CustomDefaultAuthorizeFilter(config);
            var context = BuildSignedAuthorizationContext(requestUrl, permission);

            // Act
            authorizeFilter.OnAuthorization(context);

            // Assert
            Assert.Equal(expectedTrigger, authorizeFilter.CannotCreateCookiesHookTriggered);
        }

        [Theory]
        [InlineData("http://contoso.com", "email", true)]
        [InlineData("http://contoso.com?error=access_denied", "email", false)]
        [InlineData("http://contoso.com?error=access_denied", null, false)]
        public void OnAuthorization_TriggersPreHookPriorToPermissionsDialog(string requestUrl,
                                                                            string permission,
                                                                            bool expectedTrigger)
        {
            // Arrange
            var config = BuildConfiguration("~/home/permissions");
            var authorizeFilter = new CustomDefaultAuthorizeFilter(config);
            var context = BuildSignedAuthorizationContext(requestUrl, permission);

            // Act
            authorizeFilter.OnAuthorization(context);

            // Assert
            Assert.Equal(expectedTrigger, authorizeFilter.PermissionPromptHookTriggered);
        }

        [Theory]
        [InlineData("http://contoso.com", "email", true)]
        [InlineData("http://contoso.com?error=access_denied", "email", false)]
        [InlineData("http://contoso.com?error=access_denied", null, false)]
        public void OnAuthorization_TriggersDeniedHook(string requestUrl, string permission, bool expectedTrigger)
        {
            // Arrange
            var config = BuildConfiguration("~/home/permissions");
            var authorizeFilter = new CustomDefaultAuthorizeFilter(config);
            var persistedCookies = new HttpCookieCollection();
            persistedCookies.Add(
                new HttpCookie(
                    PermissionHelper.RequestedPermissionCookieName, permission ?? string.Empty));
            var context = BuildSignedAuthorizationContext(requestUrl, permission, persistedCookies);

            // Act
            authorizeFilter.OnAuthorization(context);

            // Assert
            Assert.Equal(expectedTrigger, authorizeFilter.DeniedPermissionPromptHookTriggered);
        }

        [Theory]
        [InlineData("http://contoso.com", "email", "email", true)]
        [InlineData("http://contoso.com", "email", "foo", false)]
        [InlineData("http://contoso.com?error=access_denied", "email", "email", false)]
        [InlineData("http://contoso.com?error=access_denied", "email", "foo", false)]
        [InlineData("http://contoso.com?error=access_denied", null, "foo", false)]
        public void OnAuthorization_TriggersDeniedHookWithRevokedPermissions(string requestUrl,
                                                                             string permission,
                                                                             string permissionInStatus,
                                                                             bool expectedTrigger)
        {
            var rawPermissionsStatus = new Dictionary<string, string>
            {
                { "permission", permissionInStatus },
                { "status", "declined" },
            };

            var data = new List<IDictionary<string, string>>(new[] { rawPermissionsStatus });

            // Arrange
            var config = BuildConfiguration("~/home/permissions", userPermissionsStatus:
                new PermissionsStatus(data));
            var authorizeFilter = new CustomDefaultAuthorizeFilter(config);
            var context = BuildSignedAuthorizationContext(requestUrl, permission);

            // Act
            authorizeFilter.OnAuthorization(context);

            // Assert
            Assert.Equal(expectedTrigger, authorizeFilter.DeniedPermissionPromptHookTriggered);
        }

        [Theory]
        [InlineData("http://contoso.com", "email", true)]
        [InlineData("http://contoso.com?error=access_denied", "email", false)]
        [InlineData("http://contoso.com?error=access_denied", null, false)]
        public void OnAuthorization_TriggersDeniedHookAfterPersistingRequestedPermissions(string requestUrl,
                                                                                          string permission,
                                                                                          bool expectedTrigger)
        {
            // Arrange
            var config = BuildConfiguration("~/home/permissions");
            var authorizeFilter = new CustomDefaultAuthorizeFilter(config);
            var context = BuildSignedAuthorizationContext(requestUrl, permission);

            // Act
            authorizeFilter.OnAuthorization(context);

            // Here we're acting like a browser and adding the responses cookies to the "next" request's cookies
            var responseCookies = context.HttpContext.Response.Cookies;
            foreach (var cookieName in responseCookies.AllKeys)
            {
                context.HttpContext.Request.Cookies.Add(responseCookies[cookieName]);
            }

            // Assert
            Assert.False(authorizeFilter.DeniedPermissionPromptHookTriggered);

            // Act 2
            // We're making a "second" request essentially
            authorizeFilter.OnAuthorization(context);

            // Assert 2
            Assert.Equal(expectedTrigger, authorizeFilter.DeniedPermissionPromptHookTriggered);
        }

        [Fact]
        public void OnAuthorization_CannotCreateCookiesHookNullFlows()
        {
            // Arrange
            var config = BuildConfiguration("~/home/permissions");
            var authorizeFilter = new CustomInvalidAuthorizeFilter(config);
            var context = BuildSignedAuthorizationContext("http://contoso.com?__fb_mps=true", "email");

            // Act
            authorizeFilter.OnAuthorization(context);

            // Assert
            Assert.Null(context.Result);
        }

        [Fact]
        public void OnAuthorization_PreHookNullTreatedLikeIgnoreResult()
        {
            // Arrange
            var config = BuildConfiguration("~/home/permissions");
            var authorizeFilter = new CustomInvalidAuthorizeFilter(config);
            var context = BuildSignedAuthorizationContext("http://contoso.com", "email");

            // Act
            authorizeFilter.OnAuthorization(context);

            // Assert
            Assert.Null(context.Result);
        }

        [Fact]
        public void OnAuthorization_DeniedHookNullTreatedLikeIgnoreResult()
        {
            // Arrange
            var config = BuildConfiguration("~/home/permissions");
            var authorizeFilter = new CustomInvalidAuthorizeFilter(config);
            var persistedCookies = new HttpCookieCollection();
            persistedCookies.Add(
                new HttpCookie(
                    PermissionHelper.RequestedPermissionCookieName, "email"));
            var context = BuildSignedAuthorizationContext("http://contoso.com", "email", persistedCookies);

            // Act
            authorizeFilter.OnAuthorization(context);

            // Assert
            Assert.Null(context.Result);
        }

        [Fact]
        public void OnAuthorization_CannotCreateCookiesHookCustomActionResultIsContextsResult()
        {
            // Arrange
            var tempUrl = "http://contoso.com?__fb_mps=true";
            var config = BuildConfiguration("~/home/permissions");
            var cannotCreateCookiesHookResult = new RedirectResult(tempUrl);
            var authorizeFilter = new CustomReturningAuthorizeFilter(config,
                                                                     cannotCreateCookiesHookResult,
                                                                     new RedirectResult(tempUrl),
                                                                     new RedirectResult(tempUrl));
            var context = BuildSignedAuthorizationContext(tempUrl, "email");

            // Act
            authorizeFilter.OnAuthorization(context);

            // Assert
            Assert.Equal(cannotCreateCookiesHookResult, context.Result);
        }

        [Fact]
        public void OnAuthorization_PreHookCustomActionResultIsContextsResult()
        {
            // Arrange
            var tempUrl = "http://contoso.com";
            var config = BuildConfiguration("~/home/permissions");
            var preHookResult = new RedirectResult(tempUrl);
            var authorizeFilter = new CustomReturningAuthorizeFilter(config,
                                                                     new RedirectResult(tempUrl),
                                                                     preHookResult,
                                                                     new RedirectResult(tempUrl));
            var context = BuildSignedAuthorizationContext(tempUrl, "email");

            // Act
            authorizeFilter.OnAuthorization(context);

            // Assert
            Assert.Equal(preHookResult, context.Result);
        }

        [Fact]
        public void OnAuthorization_DeniedHookCustomActionResultIsContextsResult()
        {
            // Arrange
            var tempUrl = "http://contoso.com";
            var config = BuildConfiguration("~/home/permissions");
            var deniedHookResult = new RedirectResult(tempUrl);
            var authorizeFilter = new CustomReturningAuthorizeFilter(config,
                                                                     new RedirectResult(tempUrl),
                                                                     new RedirectResult(tempUrl),
                                                                     deniedHookResult);
            var persistedCookies = new HttpCookieCollection();
            persistedCookies.Add(
                new HttpCookie(
                    PermissionHelper.RequestedPermissionCookieName, "email"));
            var context = BuildSignedAuthorizationContext(tempUrl, "email", persistedCookies);

            // Act
            authorizeFilter.OnAuthorization(context);

            // Assert
            Assert.Equal(deniedHookResult, context.Result);
        }

        // Helper methods and classes
        private FacebookConfiguration BuildConfiguration(string authorizationRedirectPath,
                                                         string cannotCreateCookiesRedirectPath = null,
                                                         PermissionsStatus userPermissionsStatus = null)
        {
            var client = MockHelpers.CreateFacebookClient();
            var permissionService = MockHelpers.CreatePermissionService(new[] { "" }, userPermissionsStatus);
            var config = MockHelpers.CreateConfiguration(client, permissionService);
            config.AuthorizationRedirectPath = authorizationRedirectPath;

            if (cannotCreateCookiesRedirectPath != null)
            {
                config.CannotCreateCookieRedirectPath = cannotCreateCookiesRedirectPath;
            }

            return config;
        }

        private AuthorizationContext BuildSignedAuthorizationContext(string requestUrl,
                                                                     string permission,
                                                                     HttpCookieCollection requestCookies = null)
        {
            var permissions = permission == null ? new string[0] : new string[] { permission };

            var requestUri = new Uri(requestUrl);

            var context = new AuthorizationContext(
                MockHelpers.CreateControllerContext(new NameValueCollection
                {
                    {"signed_request", "exampleSignedRequest"}
                },
                HttpUtility.ParseQueryString(requestUri.Query),
                requestUri,
                requestCookies),
                MockHelpers.CreateActionDescriptor(new[] { new FacebookAuthorizeAttribute(permissions) }));

            return context;
        }

        private class CustomInvalidAuthorizeFilter : FacebookAuthorizeFilter
        {
            public CustomInvalidAuthorizeFilter(FacebookConfiguration config)
                : base(config)
            { }

            protected override void OnCannotCreateCookies(PermissionContext context)
            {
                context.Result = null;
            }

            protected override void OnPermissionPrompt(PermissionContext context)
            {
                context.Result = null;
            }

            protected override void OnDeniedPermissionPrompt(PermissionContext context)
            {
                context.Result = null;
            }
        }

        private class CustomDefaultAuthorizeFilter : FacebookAuthorizeFilter
        {
            public CustomDefaultAuthorizeFilter(FacebookConfiguration config)
                : base(config)
            { }

            public bool CannotCreateCookiesHookTriggered { get; private set; }
            public bool PermissionPromptHookTriggered { get; private set; }
            public bool DeniedPermissionPromptHookTriggered { get; private set; }

            protected override void OnCannotCreateCookies(PermissionContext context)
            {
                CannotCreateCookiesHookTriggered = true;

                base.OnCannotCreateCookies(context);
            }

            protected override void OnPermissionPrompt(PermissionContext context)
            {
                PermissionPromptHookTriggered = true;

                base.OnPermissionPrompt(context);
            }

            protected override void OnDeniedPermissionPrompt(PermissionContext context)
            {
                DeniedPermissionPromptHookTriggered = true;

                base.OnDeniedPermissionPrompt(context);
            }
        }

        private class CustomReturningAuthorizeFilter : FacebookAuthorizeFilter
        {
            private ActionResult _cannotCreateCookieResult;
            private ActionResult _promptPermissionHookResult;
            private ActionResult _deniedPermissionPromptHookResult;

            public CustomReturningAuthorizeFilter(FacebookConfiguration config,
                                                  ActionResult cannotCreateCookieResult,
                                                  ActionResult promptPermissionHookResult,
                                                  ActionResult deniedPermissionPromptHookResult)
                : base(config)
            {
                _cannotCreateCookieResult = cannotCreateCookieResult;
                _promptPermissionHookResult = promptPermissionHookResult;
                _deniedPermissionPromptHookResult = deniedPermissionPromptHookResult;
            }

            protected override void OnCannotCreateCookies(PermissionContext context)
            {
                context.Result = _cannotCreateCookieResult;
            }

            protected override void OnPermissionPrompt(PermissionContext context)
            {
                context.Result = _promptPermissionHookResult;
            }

            protected override void OnDeniedPermissionPrompt(PermissionContext context)
            {
                context.Result = _deniedPermissionPromptHookResult;
            }
        }
    }
}
