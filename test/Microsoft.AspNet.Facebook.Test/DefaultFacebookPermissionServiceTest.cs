// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Facebook.Providers;
using Microsoft.AspNet.Facebook.Test.Helpers;
using Microsoft.TestCommon;

namespace Microsoft.AspNet.Facebook.Test
{
    public class DefaultFacebookPermissionServiceTest
    {
        [Fact]
        public void Constructor_ThrowsArgumentNullException()
        {
            Assert.ThrowsArgumentNull(() => new DefaultFacebookPermissionService(null), "configuration");
        }

        [Fact]
        public void GetUserPermissions_ThrowsArgumentNullException()
        {
            FacebookConfiguration config = new FacebookConfiguration();
            config.ClientProvider = new DefaultFacebookClientProvider(config);
            DefaultFacebookPermissionService permissionService = new DefaultFacebookPermissionService(config);

            Assert.ThrowsArgumentNull(() => permissionService.GetUserPermissions(null, "accessToken"), "userId");
            Assert.ThrowsArgumentNull(() => permissionService.GetUserPermissions("userId", null), "accessToken");
        }

        [Fact]
        public void GetUserPermissions_CallsGetOnFacebookClientWithExpectedPath()
        {
            LocalFacebookClient localClient = new LocalFacebookClient();
            FacebookConfiguration config = MockHelpers.CreateConfiguration(localClient);
            DefaultFacebookPermissionService permissionService = new DefaultFacebookPermissionService(config);

            permissionService.GetUserPermissions("123456", "sampleAccessToken");

            Assert.Equal("me/permissions", localClient.Path);
        }
    }
}