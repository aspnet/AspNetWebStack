// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Http.Cors
{
    public class DefaultController : ApiController
    {
        public string Get()
        {
            return "value";
        }

        [EnableCors("http://restrictedExample.com", "*", "*")]
        public string Post()
        {
            return "value created";
        }
    }
}