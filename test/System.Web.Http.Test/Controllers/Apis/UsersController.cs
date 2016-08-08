// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;

namespace System.Web.Http
{
    public class UsersController : ApiController
    {
        public HttpResponseMessage Get()
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("Default User")
            };
        }

        public HttpResponseMessage Post()
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("User Posted")
            };
        }

        public HttpResponseMessage Put()
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("User Updated")
            };
        }

        public HttpResponseMessage Delete()
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("User Deleted")
            };
        }
    }
}
