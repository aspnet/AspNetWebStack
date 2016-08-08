// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Specialized;
using System.Web;
using System.Web.Mvc;
using Facebook;
using Microsoft.AspNet.Facebook.Authorization;
using Microsoft.AspNet.Facebook.Client;
using Microsoft.AspNet.Facebook.Providers;
using Microsoft.AspNet.Facebook.Test.Helpers;
using Microsoft.TestCommon;
using Moq;

namespace Microsoft.AspNet.Facebook.Test
{
    public class FacebookAuthorizeFilterTest
    {
        [Fact]
        public void AddCookieVerificationQuery_AddsQueryParams()
        {
            // Arrange
            var collection = new NameValueCollection();
            var filter = new CustomAuthorizeFilter();

            // Act
            filter.ExposedAddCookieVerificationQuery(collection);

            // Assert
            Assert.NotEmpty(collection);
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException()
        {
            Assert.ThrowsArgumentNull(
                () => new FacebookAuthorizeFilter(null),
                "config");
        }

        [Fact]
        public void OnAuthorization_ThrowsArgumentNullException()
        {
            FacebookConfiguration config = MockHelpers.CreateConfiguration();
            FacebookAuthorizeFilter authorizeFilter = new FacebookAuthorizeFilter(config);
            Assert.ThrowsArgumentNull(
                () => authorizeFilter.OnAuthorization(null),
                "filterContext");
        }

        [Fact]
        public void CreateRedirectResult_StringEncodesTheRedirectUrl()
        {
            Uri uri = new Uri("http://example.com?query=4'; alert('hello world')");
            FacebookConfiguration config = MockHelpers.CreateConfiguration();
            FacebookAuthorizeFilter authorizeFilter = new FacebookAuthorizeFilter(config);

            ContentResult result = Assert.IsType<JavaScriptRedirectResult>(authorizeFilter.CreateRedirectResult(uri));
            Assert.Equal("text/html", result.ContentType);
            Assert.Equal(@"<script>window.top.location = 'http://example.com/?query=4\u0027;%20alert(\u0027hello%20world\u0027)';</script>", result.Content);
        }

        [Fact]
        public void OnAuthorization_RedirectsToOAuthDialog_WhenSignedRequestIsNull()
        {
            FacebookConfiguration config = MockHelpers.CreateConfiguration();
            FacebookAuthorizeFilter authorizeFilter = new FacebookAuthorizeFilter(config);
            AuthorizationContext context = new AuthorizationContext(
                MockHelpers.CreateControllerContext(),
                MockHelpers.CreateActionDescriptor(new[] { new FacebookAuthorizeAttribute("email") }));

            authorizeFilter.OnAuthorization(context);

            ContentResult result = Assert.IsType<JavaScriptRedirectResult>(context.Result);
            Assert.Equal("text/html", result.ContentType);
            Assert.Equal(
                "<script>window.top.location = 'https://www.facebook.com/dialog/oauth?redirect_uri=https%3A%2F%2Fapps.facebook.com%2FDefaultAppId%2F\\u0026client_id=DefaultAppId';</script>",
                result.Content);
        }

        [Fact]
        public void OnAuthorization_RedirectsToOAuthDialog_ForMissingPermissions()
        {
            FacebookClient client = MockHelpers.CreateFacebookClient();
            IFacebookPermissionService permissionService = MockHelpers.CreatePermissionService(new[] { "" });
            FacebookConfiguration config = MockHelpers.CreateConfiguration(client, permissionService);
            FacebookAuthorizeFilter authorizeFilter = new FacebookAuthorizeFilter(config);
            AuthorizationContext context = new AuthorizationContext(
                MockHelpers.CreateControllerContext(new NameValueCollection
                {
                    {"signed_request", "exampleSignedRequest"}
                }),
                MockHelpers.CreateActionDescriptor(new[] { new FacebookAuthorizeAttribute("email", "user_likes") }));

            authorizeFilter.OnAuthorization(context);

            ContentResult result = Assert.IsType<ShowPromptResult>(context.Result);
            Assert.Equal("text/html", result.ContentType);
            Assert.Equal(
                "<script>window.top.location = 'https://www.facebook.com/dialog/oauth?redirect_uri=example.com';</script>",
                result.Content);
        }

        [Theory]
        [InlineData("http://example.com", "https://www.facebook.com/dialog/oauth?redirect_uri=example.com")]
        [InlineData("http://example.com?error=access_denied", "https://apps.facebook.com/DefaultAppId/home/permissions?originUrl=https%3a%2f%2fapps.facebook.com%2fDefaultAppId%2f\\u0026permissions=email")]
        public void OnAuthorization_RedirectsToAuthorizationRedirectPath_OnlyWhenUserDeniedGrantingPermissions(string requestUrl, string expectedRedirectUrl)
        {
            FacebookClient client = MockHelpers.CreateFacebookClient();
            IFacebookPermissionService permissionService = MockHelpers.CreatePermissionService(new[] { "" });
            FacebookConfiguration config = MockHelpers.CreateConfiguration(client, permissionService);
            config.AuthorizationRedirectPath = "~/home/permissions";
            FacebookAuthorizeFilter authorizeFilter = new FacebookAuthorizeFilter(config);
            AuthorizationContext context = new AuthorizationContext(
                MockHelpers.CreateControllerContext(new NameValueCollection
                {
                    {"signed_request", "exampleSignedRequest"}
                },
                null,
                new Uri(requestUrl)),
                MockHelpers.CreateActionDescriptor(new[] { new FacebookAuthorizeAttribute("email") }));

            authorizeFilter.OnAuthorization(context);

            ContentResult result = Assert.IsAssignableFrom<JavaScriptRedirectResult>(context.Result);

            Assert.Equal("text/html", result.ContentType);
            Assert.Equal(
                String.Format("<script>window.top.location = '{0}';</script>", expectedRedirectUrl),
                result.Content);
        }

        private class CustomAuthorizeFilter : FacebookAuthorizeFilter
        {
            public CustomAuthorizeFilter()
                : base(new FacebookConfiguration())
            {
            }

            public void ExposedAddCookieVerificationQuery(NameValueCollection queries)
            {
                AddCookieVerificationQuery(queries);
            }
        }
    }
}