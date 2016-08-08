// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Web.Http.Description;

namespace System.Web.Http.ApiExplorer
{
    public class ResponseTypeController : ApiController
    {
        [ResponseType(typeof(User))]
        public IHttpActionResult Get()
        {
            return Content<User>(HttpStatusCode.OK, new User { FirstName = "foo", LastName = "bar" });
        }

        [ResponseType(typeof(User))]
        public HttpResponseMessage Post(User user)
        {
            return Request.CreateResponse<User>(user);
        }

        public string Delete(int id)
        {
            return "User deleted";
        }

        [ResponseType(typeof(void))]
        public IHttpActionResult Head()
        {
            return StatusCode(HttpStatusCode.OK);
        }
    }
}