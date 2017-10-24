// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Specialized;
using System.Web.WebPages.TestUtils;
using Microsoft.TestCommon;

namespace System.Web.Http.WebHost
{
    public class SuppressFormsAuthRedirectHelperTest
    {
        [Theory]
        [InlineData("false", false)]
        [InlineData("true", true)]
        [InlineData("", true)]
        [InlineData("foo", true)]
        public void GetDisabled_ParsesAppSettings(string setting, bool expected)
        {
            Assert.Equal(expected, SuppressFormsAuthRedirectHelper.GetEnabled(new NameValueCollection() { { SuppressFormsAuthRedirectHelper.AppSettingsSuppressFormsAuthenticationRedirectKey, setting } }));
        }

        [Fact]
        public void PreApplicationStartCode_IsValid()
        {
#pragma warning disable 0618 // System.Web.Http.WebHost.PreApplicationStartCode is obsolete
            Type preApplicationStartType = typeof(PreApplicationStartCode);
#pragma warning restore
            PreAppStartTestHelper.TestPreAppStartClass(preApplicationStartType);
            object[] attrs = preApplicationStartType.GetCustomAttributes(typeof(ObsoleteAttribute), true);
            Assert.Single(attrs);
        }
    }
}