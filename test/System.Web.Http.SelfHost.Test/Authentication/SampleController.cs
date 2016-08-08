// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Http
{
    /// <summary>
    /// Sample ApiControler
    /// </summary>
    public class SampleController : ApiController
    {
        [RequireAdmin]
        public string Get()
        {
            return "hello";
        }
    }
}