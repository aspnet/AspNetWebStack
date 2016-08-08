// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Http.ContentNegotiation
{
    public class ConnegController : ApiController
    {
        public ConnegItem GetItem(string name = "Fido", int age = 3)
        {
            return new ConnegItem()
            {
                Name = name,
                Age = age
            };
        }

        public ConnegItem PostItem(ConnegItem item)
        {
            return item;
        }
    }
}
