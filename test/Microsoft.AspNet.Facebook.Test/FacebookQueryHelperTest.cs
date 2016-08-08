// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Facebook.Client;
using Microsoft.AspNet.Facebook.Test.Types;
using Microsoft.TestCommon;

namespace Microsoft.AspNet.Facebook.Test
{
    public class FacebookQueryHelperTest
    {
        [Theory]
        [InlineData(typeof(SimpleUser), "?fields=id,name,picture.fields(url)")]
        [InlineData(typeof(UserWithFriends), "?fields=id,name,picture.fields(url),friends.fields(id,name,picture.fields(url))")]
        [InlineData(typeof(UserTypeWithIgnoredProperties), "?fields=id")]
        [InlineData(typeof(UserTypeWithRenamedProperties), "?fields=id,name,picture.fields(url)")]
        [InlineData(typeof(UserTypeWithFieldModifiers), "?fields=id,name,picture.type(large).fields(url),friends.limit(5).fields(id,name,picture.fields(url))")]
        public void GetFields_ReturnsExpectedQuery(Type modelType, string expectedQuery)
        {
            Assert.Equal(expectedQuery, FacebookQueryHelper.GetFields(modelType));
        }

        [Theory]
        [InlineData(typeof(UserWithUserFriends))]
        [InlineData(typeof(UserContainingFriendsWithCycle))]
        public void GetFields_ThrowsExceptionWhenDetectsACycle(Type modelType)
        {
            Assert.Throws<InvalidOperationException>(
                () => FacebookQueryHelper.GetFields(modelType),
                Resources.CircularReferenceNotSupported);
        }
    }
}
