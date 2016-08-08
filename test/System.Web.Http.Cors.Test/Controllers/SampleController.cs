// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Http.Cors
{
    [EnableCors("*", "*", "*")]
    public class SampleController : ApiController
    {
        public bool Disposed { get; private set; }

        public string Get()
        {
            return "value";
        }

        [DisableCors]
        public string Get(int id)
        {
            return "value" + id;
        }

        [DisableCors]
        public string Post()
        {
            return "value";
        }

        public void Delete()
        {
        }

        [EnableCors("http://example.com", "*", "*")]
        public void Head()
        {
        }

        [EnableCors("http://example.com, http://localhost", "*", "*")]
        public void Put()
        {
        }

        protected override void Dispose(bool disposing)
        {
            Disposed = true;
            base.Dispose(disposing);
        }
    }
}