// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Http.Description;
namespace System.Web.Http.ApiExplorer
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class HiddenController : ApiController
    {
        public string Get(int id)
        {
            return "visible action";
        }

        [HttpPost]
        public void AddData()
        {
        }

        public int Get()
        {
            return 0;
        }

        [NonAction]
        public string GetHiddenAction()
        {
            return "Hidden action";
        }
    }
}
