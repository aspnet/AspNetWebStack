// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Http;

namespace System.Web.Http
{
    public class DuplicateController : ApiController
    {
        public string GetAction()
        {
            return "dup";
        }
    }
}

namespace System.Web.Http2
{
    public class DuplicateController : ApiController
    {
        public string GetAction()
        {
            return "dup2";
        }
    }
}
