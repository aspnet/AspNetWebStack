// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Web.Http.SelfHost;
using Microsoft.TestCommon;

namespace System.Web.Http
{
    [Xunit.Collection("PortReserver Collection")] // Avoid conflicts between different PortReserver consumers.
    public class BasicOverHttpTest
    {
        [Fact]
        public Task AuthenticateWithUsernameTokenSucceed()
        {
            return RunBasicAuthTest("Sample", "", new NetworkCredential("username", "password"),
                (response) => Assert.Equal(HttpStatusCode.OK, response.StatusCode)
                );
        }

        [Fact]
        public Task AuthenticateWithWrongPasswordFail()
        {
            return RunBasicAuthTest("Sample", "", new NetworkCredential("username", "wrong password"),
                (response) => Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode)
                );
        }

        [Fact]
        public Task AuthenticateWithNoCredentialFail()
        {
            return RunBasicAuthTest("Sample", "", null,
                (response) => Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)
                );
        }

        private static async Task RunBasicAuthTest(string controllerName, string routeSuffix, NetworkCredential credential, Action<HttpResponseMessage> assert)
        {
            using (var port = new PortReserver())
            {
                // Arrange
                HttpSelfHostConfiguration config = new HttpSelfHostConfiguration(port.BaseUri);
                config.HostNameComparisonMode = HostNameComparisonMode.Exact;
                config.Routes.MapHttpRoute("Default", "{controller}" + routeSuffix, new { controller = controllerName });
                config.UserNamePasswordValidator = new CustomUsernamePasswordValidator();
                config.MessageHandlers.Add(new CustomMessageHandler());
                HttpSelfHostServer server = new HttpSelfHostServer(config);

                await server.OpenAsync();

                // Create a GET request with correct username and password
                HttpClientHandler handler = new HttpClientHandler();
                handler.Credentials = credential;
                HttpClient client = new HttpClient(handler);

                HttpResponseMessage response = null;
                try
                {
                    // Act
                    response = await client.GetAsync(port.BaseUri);

                    // Assert
                    assert(response);
                }
                finally
                {
                    if (response != null)
                    {
                        response.Dispose();
                    }
                    client.Dispose();
                }

                await server.CloseAsync();
            }
        }

    }
}
