// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace Microsoft.AspNet.Facebook.Test
{
    public class FacebookConfigurationTest
    {
        [Fact]
        public void Default_Constructor()
        {
            FacebookConfiguration config = new FacebookConfiguration();
            Assert.Null(config.AppId);
            Assert.Null(config.AppNamespace);
            Assert.Null(config.AppSecret);
            Assert.NotNull(config.AppUrl);
            Assert.Null(config.AuthorizationRedirectPath);
            Assert.Null(config.CannotCreateCookieRedirectPath);
            Assert.Null(config.ClientProvider);
            Assert.Null(config.PermissionService);
            Assert.NotNull(config.Properties);
        }

        [Fact]
        public void AppUrl_FromAppId()
        {
            FacebookConfiguration config = new FacebookConfiguration();
            config.AppId = "654321";
            Assert.Equal("https://apps.facebook.com/654321", config.AppUrl);
        }

        [Fact]
        public void AppUrl_FromAppNamespace()
        {
            FacebookConfiguration config = new FacebookConfiguration();
            config.AppId = "654321";
            config.AppNamespace = "MyCustomApp";
            Assert.Equal("https://apps.facebook.com/MyCustomApp", config.AppUrl);
        }

        [Fact]
        public void AppUrl_FromCustomUrl()
        {
            FacebookConfiguration config = new FacebookConfiguration();
            config.AppId = "654321";
            config.AppNamespace = "MyCustomApp";
            config.AppUrl = "http://apps.example.com/myapp";
            Assert.Equal("http://apps.example.com/myapp", config.AppUrl);
        }

        [Fact]
        public void LoadFromAppSettings_ReadsFromAppConfig()
        {
            FacebookConfiguration config = new FacebookConfiguration();
            config.LoadFromAppSettings();
            Assert.Equal("123456", config.AppId);
            Assert.Equal("abcdefg", config.AppSecret);
            Assert.Equal("MyApp", config.AppNamespace);
            Assert.Equal("~/Authorize/Index", config.AuthorizationRedirectPath);
            Assert.Equal("~/NoCookies/Index", config.CannotCreateCookieRedirectPath);
            Assert.Equal("https://apps.newfacebook.example.com/myapp", config.AppUrl);
        }

        [Fact]
        public void AuthorizationRedirectPath_ThrowsArgumentException()
        {
            FacebookConfiguration config = new FacebookConfiguration();
            Assert.ThrowsArgument(() => config.AuthorizationRedirectPath = "Home/Permissions", "value");
        }

        [Fact]
        public void CannotCreateCookieRedirectPath_ThrowsArgumentException()
        {
            FacebookConfiguration config = new FacebookConfiguration();
            Assert.ThrowsArgument(() => config.CannotCreateCookieRedirectPath = "Home/Permissions", "value");
        }
    }
}