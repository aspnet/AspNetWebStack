// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace System.Web.Http
{
    public static class ScenarioHelper
    {
        public static string BaseAddress = "http://localhost";
        public static async Task RunTestAsync(
            string controllerName,
            string routeSuffix,
            HttpRequestMessage request,
            Func<HttpResponseMessage, Task> assert,
            Action<HttpConfiguration> configurer = null)
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration() { IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always };

            config.Routes.MapHttpRoute("Default", "{controller}" + routeSuffix, new { controller = controllerName });
            if (configurer != null)
            {
                configurer(config);
            }
            HttpServer server = new HttpServer(config);
            HttpMessageInvoker invoker = new HttpMessageInvoker(server);
            HttpResponseMessage response = null;
            try
            {
                // Act
                response = await invoker.SendAsync(request, CancellationToken.None);

                // Assert
                await assert(response);
            }
            finally
            {
                request.Dispose();
                if (response != null)
                {
                    response.Dispose();
                }
            }
        }
    }
}
