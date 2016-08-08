// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Http.ApiExplorer
{
    public class AmbiguousActionController : ApiController
    {
        // The actions GetItem and Get and potentially be ambiguous under
        // route "api/{controller}" because they're both GET api/AmbiguousAction

        public void GetItem()
        {
        }

        public void Get()
        {
        }

        public bool Post(int id)
        {
            return true;
        }
    }
}