// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using Microsoft.TestCommon;

namespace System.Web.Http.ModelBinding
{
    /// <summary>
    /// End to end functional tests for model binding via routes
    /// </summary>
    public class RouteBindingTests : ModelBindingTests
    {
        [Fact]
        public void Route_Binds_Simple_Types_Get()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage()
            {
                RequestUri = new Uri(BaseAddress + "ModelBinding/GetStringFromRoute"),
                Method = HttpMethod.Get
            };

            // Act
            HttpResponseMessage response = Client.SendAsync(request).Result;

            // Assert
            string responseString = response.Content.ReadAsStringAsync().Result;
            Assert.Equal<string>("\"ModelBinding:GetStringFromRoute\"", responseString);
        }
    }
}