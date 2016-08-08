// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.TestCommon;

namespace Microsoft.AspNet.Facebook.Test
{
    public class FacebookAuthorizeAttributeTest
    {
        [Fact]
        public void Constructor_ThrowsArgumentNullException()
        {
            Assert.ThrowsArgumentNull(() => new FacebookAuthorizeAttribute(null), "permissions");
        }

        [Fact]
        public void Permissions_ReturnsExpectedValues()
        {
            string[] permissions = new[] { "email", "user_likes", "friends_likes" };
            FacebookAuthorizeAttribute authorizeAttribute = new FacebookAuthorizeAttribute(permissions);
            HashSet<string> permissionSet = new HashSet<string>(permissions);

            Assert.True(permissionSet.SetEquals(authorizeAttribute.Permissions));
        }
    }
}