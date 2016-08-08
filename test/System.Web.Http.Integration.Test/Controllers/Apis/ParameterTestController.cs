// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Http
{
    public class ParameterTestController : ApiController
    {
        public void Delete(int id)
        {
        }

        public string Get(int id = -1)
        {
            return String.Format("Get({0})", id);
        }

        public string POST(string id = null)
        {
            return String.Format("POST({0})", id ?? "null");
        }

        public string Put(int id, string value)
        {
            return String.Format("Put({0}, {1})", id, value);
        }
    }
}
