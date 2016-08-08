// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using System.Threading;

namespace System.Web.Http.WebHost
{
    internal static class HttpResponseBaseExtensions
    {
        private static readonly bool _isSystemWebVersion451OrGreater = IsSystemWebVersion451OrGreater();
        private static readonly bool _isClientDisconnectedTokenAvailable = IsClientDisconnectedTokenAvailable();

        public static CancellationToken GetClientDisconnectedTokenWhenFixed(this HttpResponseBase response)
        {
            if (response == null)
            {
                throw new ArgumentNullException("response");
            }

            // On some platforms/configurations, accessing response.ClientDisconnectedToken would always throw.
            // Also, on .NET 4.5 and earlier, using response.ClientDisconnectedToken can cause application crashes.
            // Gracefully degrade to CancellationToken.None unless ClientDisconnectedToken is both available and
            // reliable.
            if (!_isClientDisconnectedTokenAvailable || !_isSystemWebVersion451OrGreater)
            {
                return CancellationToken.None;
            }

            return response.ClientDisconnectedToken;
        }

        private static bool IsClientDisconnectedTokenAvailable()
        {
            // Accessing HttpResponse.ClientDisconnectedToken throws PlatformNotSupportedException unless both:
            // 1) Using IIS 7.5 or newer, and
            // 2) Using integrated pipeline
            Version iis75 = new Version(7, 5);
            Version iisVersion = HttpRuntime.IISVersion;
            return iisVersion != null && iisVersion >= iis75 && HttpRuntime.UsingIntegratedPipeline;
        }

        private static bool IsSystemWebVersion451OrGreater()
        {
            Assembly systemWeb = typeof(HttpContextBase).Assembly;
            // System.Web.AspNetEventSource only exists in .NET 4.5.1 and will not be back-ported to .NET 4.5.
            return systemWeb.GetType("System.Web.AspNetEventSource") != null;
        }
    }
}
