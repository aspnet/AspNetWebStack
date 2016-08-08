// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Facebook.Client;
using Microsoft.AspNet.Facebook.Test.Helpers;
using Microsoft.AspNet.Facebook.Test.Types;
using Microsoft.TestCommon;

namespace Microsoft.AspNet.Facebook.Test
{
    public class FacebookClientExtensionsTest
    {
        [Fact]
        public void GetCurrentUserAsyncOfT_CallsGetTaskAsyncWithTheExpectedPath()
        {
            LocalFacebookClient client = new LocalFacebookClient();
            client.GetCurrentUserAsync<SimpleUser>().Wait();

            Assert.Equal("me?fields=id,name,picture.fields(url)", client.Path);
        }

        [Fact]
        public void GetCurrentUserAsync_CallsGetTaskAsyncWithTheExpectedPath()
        {
            LocalFacebookClient client = new LocalFacebookClient();
            client.GetCurrentUserAsync().Wait();

            Assert.Equal("me", client.Path);
        }

        [Fact]
        public void GetCurrentUserFriendsAsyncOfT_CallsGetTaskAsyncWithTheExpectedPath()
        {
            LocalFacebookClient client = new LocalFacebookClient();
            client.GetCurrentUserFriendsAsync<SimpleUser>().Wait();

            Assert.Equal("me/friends?fields=id,name,picture.fields(url)", client.Path);
        }

        [Fact]
        public void GetCurrentUserFriendsAsync_CallsGetTaskAsyncWithTheExpectedPath()
        {
            LocalFacebookClient client = new LocalFacebookClient();
            client.GetCurrentUserFriendsAsync().Wait();

            Assert.Equal("me/friends", client.Path);
        }

        [Fact]
        public void GetCurrentUserPermissionsAsync_CallsGetTaskAsyncWithTheExpectedPath()
        {
            LocalFacebookClient client = new LocalFacebookClient();
            client.GetCurrentUserPermissionsAsync().Wait();

            Assert.Equal("me/permissions", client.Path);
        }

        [Fact]
        public void GetCurrentUserPhotosAsyncOfT_CallsGetTaskAsyncWithTheExpectedPath()
        {
            LocalFacebookClient client = new LocalFacebookClient();
            client.GetCurrentUserPhotosAsync<UserPhoto>().Wait();

            Assert.Equal("me/photos?fields=name,picture,source", client.Path);
        }

        [Fact]
        public void GetCurrentUserStatusesAsyncOfT_CallsGetTaskAsyncWithTheExpectedPath()
        {
            LocalFacebookClient client = new LocalFacebookClient();
            client.GetCurrentUserStatusesAsync<UserStatus>().Wait();

            Assert.Equal("me/statuses?fields=message,time", client.Path);
        }

        [Fact]
        public void GetFacebookObjectAsyncOfT_CallsGetTaskAsyncWithTheExpectedPath()
        {
            LocalFacebookClient client = new LocalFacebookClient();
            client.GetFacebookObjectAsync<FacebookConnection<FacebookPicture>>("me/picture").Wait();

            Assert.Equal("me/picture?fields=url", client.Path);
        }

        [Fact]
        public void GetFacebookObjectAsync_CallsGetTaskAsyncWithTheExpectedPath()
        {
            LocalFacebookClient client = new LocalFacebookClient();
            client.GetFacebookObjectAsync("me/notes").Wait();

            Assert.Equal("me/notes", client.Path);
        }

        [Fact]
        public void GetFacebookObjectAsyncOfT_ThrowArgumentNullExceptions()
        {
            LocalFacebookClient client = null;
            Assert.ThrowsArgumentNull(
                () => client.GetFacebookObjectAsync<SimpleUser>("me").Wait(),
                "client");

            client = new LocalFacebookClient();
            Assert.ThrowsArgumentNull(
                () => client.GetFacebookObjectAsync<SimpleUser>(null).Wait(),
                "objectPath");
        }

        [Fact]
        public void GetFacebookObjectAsync_ThrowArgumentNullExceptions()
        {
            LocalFacebookClient client = null;
            Assert.ThrowsArgumentNull(
                () => client.GetFacebookObjectAsync("me").Wait(),
                "client");

            client = new LocalFacebookClient();
            Assert.ThrowsArgumentNull(
                () => client.GetFacebookObjectAsync(null).Wait(),
                "objectPath");
        }
    }
}