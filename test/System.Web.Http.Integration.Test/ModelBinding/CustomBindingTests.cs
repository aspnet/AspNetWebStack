// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.TestCommon;

namespace System.Web.Http.ModelBinding
{
    /// <summary>
    /// End to end functional tests for model binding via custom providers
    /// </summary>
    public class CustomBindingTests : ModelBindingTests
    {
        [Fact]
        public async Task Custom_ValueProvider_Binds_Simple_Types_Get()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage()
            {
                RequestUri = new Uri(BaseAddress + String.Format("ModelBinding/{0}", "GetIntCustom")),
                Method = HttpMethod.Get
            };

            request.Headers.Add("value", "5");

            // Act
            HttpResponseMessage response = await Client.SendAsync(request);

            // Assert
            string responseString = await response.Content.ReadAsStringAsync();
            Assert.Equal("5", responseString);
        }

    }
}