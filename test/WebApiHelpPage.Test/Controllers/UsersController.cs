// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Web.Http;

namespace WebApiHelpPageWebHost.UnitTest.Controllers
{
    public class User
    {
        public string Name { get; set; }

        public List<Order> Orders { get; set; }

        public Address Address { get; set; }
    }

    public class Order
    {
        public int Id { get; set; }

        public Product[] Products { get; set; }
    }

    public class Product
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
    }

    public class Address
    {
        public string Street { get; set; }

        public string City { get; set; }

        public string Country { get; set; }
    }

    /// <summary>
    /// Resource for Users.
    /// </summary>
    public class UsersController : ApiController
    {
        public IEnumerable<User> Get()
        {
            return null;
        }

        public string Post(User user)
        {
            return null;
        }
    }
}