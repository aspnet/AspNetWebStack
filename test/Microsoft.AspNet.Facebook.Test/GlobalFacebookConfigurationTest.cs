// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace Microsoft.AspNet.Facebook.Test
{
    public class GlobalFacebookConfigurationTest
    {
        [Fact]
        public void Default_Configuration()
        {
            FacebookConfiguration config = GlobalFacebookConfiguration.Configuration;
            Assert.Null(config.AppId);
            Assert.Null(config.AppNamespace);
            Assert.Null(config.AppSecret);
            Assert.NotNull(config.AppUrl);
            Assert.Null(config.AuthorizationRedirectPath);
            Assert.NotNull(config.ClientProvider);
            Assert.NotNull(config.PermissionService);
            Assert.NotNull(config.Properties);
        }
    }
}