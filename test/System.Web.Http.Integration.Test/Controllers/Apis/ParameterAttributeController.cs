// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace System.Web.Http
{
    public class ParameterAttributeController : ApiController
    {
        public User GetUserByMyId(int myId) { return null; }
        public User GetUser([FromUri(Name = "id")] int myId) { return null; }
        public List<User> PostUserNameFromUri(int id, [FromUri]string name) { return null; }
        public List<User> PostUserNameFromBody(int id, [FromBody] string name) { return null; }
        public void DeleteUserWithNullableIdAndName(int? id, string name) { }
        public void DeleteUser(string address) { }
    }
}
