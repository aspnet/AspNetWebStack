// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Http
{
    public class UsersController : ApiController
    {
        // Undecorated action, following convention
        public string GetUser() { return "GetUser"; }

        // Undecorated action, not following conventions
        public string Approve() { return "Approve"; }

        // Action decorated with Verb only, following conventions
        [AcceptVerbs("UPDATE")]
        public string PutUser() { return "PutUser"; }

        // Action decorated with Name = "" only, following conventions, not reachable by {action}
        [ActionName("")]
        public string PutUserWithEmptyName() { return "PutUserWithEmptyName"; }

        // Action decorated with Name = "" only, not following conventions, it's a POST by default and not reachable by {action}
        [ActionName("")]
        public string DefaultActionWithEmptyActionName() { return "DefaultActionWithEmptyActionName"; }

        // Action decorated with Name only, following conventions
        [ActionName("UpdateUser")]
        public string PostUser() { return "PostUser"; }

        // Action decorated with both, following conventions
        [AcceptVerbs("PATCH")]
        [ActionName("ReplaceUser")]
        public string DeleteUser() { return "DeleteUser"; }

        // Action decorated with Verb only, not following conventions
        [HttpDelete]
        public string Remove() { return "Remove"; }

        // Action decorated with Name only, not following conventions
        [ActionName("Reject")]
        public string Deny() { return "Deny"; }

        // Action decorated with both, not following conventions
        [AcceptVerbs("OPTIONS")]
        [ActionName("Help")]
        public string Assist() { return "Assist"; }
    }
}
