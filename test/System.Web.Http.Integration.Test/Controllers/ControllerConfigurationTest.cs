// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.TestCommon;

namespace System.Web.Http
{
    public class ControllerConfigurationTest
    {
        [Theory]
        [InlineData("SpecialConfig/GetFormattersCount_ControllerConfig", 1)]
        [InlineData("RegularConfig/GetFormattersCount_ControllerConfig", 4)]
        [InlineData("SpecialConfig/GetParameterRulesCount_ControllerConfig", 0)]
        [InlineData("RegularConfig/GetParameterRulesCount_ControllerConfig", 3)]
        [InlineData("SpecialConfig/GetServicesCount_ControllerConfig", 1)]
        [InlineData("RegularConfig/GetServicesCount_ControllerConfig", 0)]
        [InlineData("SpecialConfig/GetFormattersCount_RequestConfig", 1)]
        [InlineData("RegularConfig/GetFormattersCount_RequestConfig", 4)]
        [InlineData("SpecialConfig/GetParameterRulesCount_RequestConfig", 0)]
        [InlineData("RegularConfig/GetParameterRulesCount_RequestConfig", 3)]
        [InlineData("SpecialConfig/GetServicesCount_RequestConfig", 1)]
        [InlineData("RegularConfig/GetServicesCount_RequestConfig", 0)]
        public async Task ControllerConfigurationSettings_ArePropagatedTo_ControllerAndRequest(string requestUrl, int count)
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute("Default", "{controller}/{action}");
            HttpServer server = new HttpServer(config);
            HttpClient client = new HttpClient(server);
            HttpResponseMessage response = await client.GetAsync("http://localhost/" + requestUrl);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(count, await response.Content.ReadAsAsync<int>());
        }
    }
}
