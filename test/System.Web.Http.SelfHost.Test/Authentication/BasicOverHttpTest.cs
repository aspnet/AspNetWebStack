// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Web.Http.SelfHost;
using Microsoft.TestCommon;

namespace System.Web.Http
{
    public class BasicOverHttpTest
    {
        [Fact]
        public void AuthenticateWithUsernameTokenSucceed()
        {
            RunBasicAuthTest("Sample", "", new NetworkCredential("username", "password"),
                (response) => Assert.Equal(HttpStatusCode.OK, response.StatusCode)
                );
        }

        [Fact]
        public void AuthenticateWithWrongPasswordFail()
        {
            RunBasicAuthTest("Sample", "", new NetworkCredential("username", "wrong password"),
                (response) => Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode)
                );
        }

        [Fact]
        public void AuthenticateWithNoCredentialFail()
        {
            RunBasicAuthTest("Sample", "", null,
                (response) => Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)
                );
        }

        private static void RunBasicAuthTest(string controllerName, string routeSuffix, NetworkCredential credential, Action<HttpResponseMessage> assert)
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

                server.OpenAsync().Wait();

                // Create a GET request with correct username and password
                HttpClientHandler handler = new HttpClientHandler();
                handler.Credentials = credential;
                HttpClient client = new HttpClient(handler);

                HttpResponseMessage response = null;
                try
                {
                    // Act
                    response = client.GetAsync(port.BaseUri).Result;

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

                server.CloseAsync().Wait();
            }
        }

    }
}
