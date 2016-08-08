// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Http
{
    public class ActionAttributeTestController : ApiController
    {
        [HttpGet]
        public void RetriveUsers() { }

        [HttpPost]
        public void AddUsers(int id) { }

        [HttpPut]
        public void UpdateUsers(User user) { }

        [HttpDelete]
        [ActionName("DeleteUsers")]
        public void RemoveUsers(string name = null) { }

        [HttpOptions]
        public void Help(int id) { }

        [HttpHead]
        public void Ping(int id) { }

        [HttpPatch]
        public void Update(int id) { }

        [AcceptVerbs("PATCH", "HEAD")]
        [CLSCompliant(false)]
        public void Users(double key) { }

        [NonAction]
        public void NonAction() { }

        [NonAction]
        [AcceptVerbs("ACTION")]
        public void NonActionWitHttpMethod() { }

        public void Options() { }

        public void Head() { }

        public void PatchUsers() { }
    }
}
