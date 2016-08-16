// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNet.Facebook.Client;
using Microsoft.AspNet.Facebook.Test.Helpers;
using Microsoft.AspNet.Facebook.Test.Types;
using Microsoft.TestCommon;

namespace Microsoft.AspNet.Facebook.Test
{
    public class FacebookClientExtensionsTest
    {
        [Fact]
        public async Task GetCurrentUserAsyncOfT_CallsGetTaskAsyncWithTheExpectedPath()
        {
            LocalFacebookClient client = new LocalFacebookClient();
            await client.GetCurrentUserAsync<SimpleUser>();

            Assert.Equal("me?fields=id,name,picture.fields(url)", client.Path);
        }

        [Fact]
        public async Task GetCurrentUserAsync_CallsGetTaskAsyncWithTheExpectedPath()
        {
            LocalFacebookClient client = new LocalFacebookClient();
            await client.GetCurrentUserAsync();

            Assert.Equal("me", client.Path);
        }

        [Fact]
        public async Task GetCurrentUserFriendsAsyncOfT_CallsGetTaskAsyncWithTheExpectedPath()
        {
            LocalFacebookClient client = new LocalFacebookClient();
            await client.GetCurrentUserFriendsAsync<SimpleUser>();

            Assert.Equal("me/friends?fields=id,name,picture.fields(url)", client.Path);
        }

        [Fact]
        public async Task GetCurrentUserFriendsAsync_CallsGetTaskAsyncWithTheExpectedPath()
        {
            LocalFacebookClient client = new LocalFacebookClient();
            await client.GetCurrentUserFriendsAsync();

            Assert.Equal("me/friends", client.Path);
        }

        [Fact]
        public async Task GetCurrentUserPermissionsAsync_CallsGetTaskAsyncWithTheExpectedPath()
        {
            LocalFacebookClient client = new LocalFacebookClient();
            await client.GetCurrentUserPermissionsAsync();

            Assert.Equal("me/permissions", client.Path);
        }

        [Fact]
        public async Task GetCurrentUserPhotosAsyncOfT_CallsGetTaskAsyncWithTheExpectedPath()
        {
            LocalFacebookClient client = new LocalFacebookClient();
            await client.GetCurrentUserPhotosAsync<UserPhoto>();

            Assert.Equal("me/photos?fields=name,picture,source", client.Path);
        }

        [Fact]
        public async Task GetCurrentUserStatusesAsyncOfT_CallsGetTaskAsyncWithTheExpectedPath()
        {
            LocalFacebookClient client = new LocalFacebookClient();
            await client.GetCurrentUserStatusesAsync<UserStatus>();

            Assert.Equal("me/statuses?fields=message,time", client.Path);
        }

        [Fact]
        public async Task GetFacebookObjectAsyncOfT_CallsGetTaskAsyncWithTheExpectedPath()
        {
            LocalFacebookClient client = new LocalFacebookClient();
            await client.GetFacebookObjectAsync<FacebookConnection<FacebookPicture>>("me/picture");

            Assert.Equal("me/picture?fields=url", client.Path);
        }

        [Fact]
        public async Task GetFacebookObjectAsync_CallsGetTaskAsyncWithTheExpectedPath()
        {
            LocalFacebookClient client = new LocalFacebookClient();
            await client.GetFacebookObjectAsync("me/notes");

            Assert.Equal("me/notes", client.Path);
        }

        [Fact]
        public async Task GetFacebookObjectAsyncOfT_ThrowArgumentNullExceptions()
        {
            LocalFacebookClient client = null;
            await Assert.ThrowsArgumentNullAsync(() => client.GetFacebookObjectAsync<SimpleUser>("me"), "client");

            client = new LocalFacebookClient();
            await Assert.ThrowsArgumentNullAsync(() => client.GetFacebookObjectAsync<SimpleUser>(null), "objectPath");
        }

        [Fact]
        public async Task GetFacebookObjectAsync_ThrowArgumentNullExceptions()
        {
            LocalFacebookClient client = null;
            await Assert.ThrowsArgumentNullAsync(() => client.GetFacebookObjectAsync("me"), "client");

            client = new LocalFacebookClient();
            await Assert.ThrowsArgumentNullAsync(() => client.GetFacebookObjectAsync(null), "objectPath");
        }
    }
}