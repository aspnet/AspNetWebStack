// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.Util;

namespace System.Web.Http
{
    public abstract class HttpServerTestBase
    {
        protected HttpServerTestBase(string baseAddress)
        {
            SetupHost(baseAddress);
        }

        protected HttpServer Server { get; private set; }

        protected HttpConfiguration Configuration { get; private set; }

        protected string BaseAddress { get; private set; }

        protected HttpClient Client { get; private set; }

        protected abstract void ApplyConfiguration(HttpConfiguration configuration);

        private void SetupHost(string baseAddress)
        {
            BaseAddress = baseAddress;

            Configuration = new HttpConfiguration();
            Configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            Configuration.MessageHandlers.Add(new ConvertToStreamMessageHandler());
            ApplyConfiguration(Configuration);

            Server = new HttpServer(Configuration);
            Client = new HttpClient(Server);
        }
    }
}
