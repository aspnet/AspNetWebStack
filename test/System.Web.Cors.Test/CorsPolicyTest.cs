// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Cors.Test
{
    public class CorsPolicyTest
    {
        [Fact]
        public void Default_Constructor()
        {
            CorsPolicy corsPolicy = new CorsPolicy();

            Assert.False(corsPolicy.AllowAnyHeader);
            Assert.False(corsPolicy.AllowAnyMethod);
            Assert.False(corsPolicy.AllowAnyOrigin);
            Assert.False(corsPolicy.SupportsCredentials);
            Assert.Empty(corsPolicy.ExposedHeaders);
            Assert.Empty(corsPolicy.Headers);
            Assert.Empty(corsPolicy.Methods);
            Assert.Empty(corsPolicy.Origins);
            Assert.Null(corsPolicy.PreflightMaxAge);
        }

        [Fact]
        public void SettingNegativePreflightMaxAge_Throws()
        {
            CorsPolicy corsPolicy = new CorsPolicy();
            Assert.ThrowsArgumentOutOfRange(() =>
            {
                corsPolicy.PreflightMaxAge = -2;
            },
            "value",
            "PreflightMaxAge must be greater than or equal to 0.");
        }

        [Fact]
        public void ToString_ReturnsThePropertyValues()
        {
            CorsPolicy corsPolicy = new CorsPolicy
            {
                AllowAnyHeader = true,
                AllowAnyOrigin = true,
                PreflightMaxAge = 10,
                SupportsCredentials = true
            };
            corsPolicy.Headers.Add("foo");
            corsPolicy.Headers.Add("bar");
            corsPolicy.Origins.Add("http://example.com");
            corsPolicy.Origins.Add("http://example.org");
            corsPolicy.Methods.Add("GET");

            Assert.Equal(@"AllowAnyHeader: True, AllowAnyMethod: False, AllowAnyOrigin: True, PreflightMaxAge: 10, SupportsCredentials: True, Origins: {http://example.com,http://example.org}, Methods: {GET}, Headers: {foo,bar}, ExposedHeaders: {}", corsPolicy.ToString());
        }
    }
}