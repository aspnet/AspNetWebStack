// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Http.ApiExplorer
{
    public class MixedController : ApiController
    {
        public string Get(string name, int series)
        {
            return null;
        }

        [Route("attribute/mixed")]
        public string Post([FromBody]string values)
        {
            return values;
        }

        [HttpDelete]
        public void RemoveItem(int id)
        {
        }
    }

    public class AttributedController : ApiController
    {
        [Route("controller/{id:int}")]
        public string Get(int id)
        {
            return "Get" + id;
        }

        [Route("controller/{name}")]
        public string GetByName(string name)
        {
            return "GetByName" + name;
        }

        [Route("controller/{id}")]
        public string Put(string id)
        {
            return "Put" + id;
        }

        [Route("controller/{id}")]
        public string Put(string id, string name)
        {
            return "Put" + id + name;
        }

        [HttpGet]
        [Route("optional/{opt1?}/{opt2?}")]
        public string Optional(int opt1, string opt2)
        {
            return "Optional" + opt1 + opt2;
        }

        [HttpGet]
        [Route("optionalwconstraint/{opt:int?}")]
        public string OptionalWithConstraint(string opt)
        {
            return "OptionalWithConstraint" + opt;
        }

        [HttpGet]
        [Route("default/{default1=D1}/{default2=D2}")]
        public string Default(string default1, string default2)
        {
            return "Default" + default1 + default2;
        }

        [HttpGet]
        [Route("wildcard/{*wildcard}")]
        public string Wildcard(string wildcard)
        {
            return "Wildcard" + wildcard;
        }

        [HttpGet]
        [HttpPut]
        [Route("multiverb")]
        public string MultiVerbs()
        {
            return Request.Method.ToString();
        }

        [HttpDelete] // Pick a unique verb
        [Route("multi1")]
        [Route("multi2")]
        public string MultiRoute()
        {
            return "multi";
        }
    }

    [RoutePrefix("prefix")]
    public class PrefixedController : ApiController
    {
        // Should not be reachable be our routes since there's no route attribute.
        public void Post()
        {
        }

        [Route("")]
        public string Get()
        {
            return "PrefixedGet";
        }

        [Route] // same behavior as Route("")
        public string Put()
        {
            return "PrefixedPut";
        }

        [HttpGet]
        [Route("{id}")]
        public string GetById(int id)
        {
            return "PrefixedGetById" + id;
        }
    }

    [RoutePrefix("prefix2")]
    [Route("defaultroute/{id:int}")]
    public class DefaultRouteController : ApiController
    {
        // This gets default route
        public string Get(int id)
        {
            return "get" + id;
        }

        [Route]
        public string Post()
        {
            return "post";
        }

        [Route("defaultrouteoverride/{id}")]
        public string Put(int id)
        {
            return "put" + id;
        }
    }

    [Route("api/default2/{action}")]
    public class RpcController : ApiController
    {
        public string GetAllCustomers1()
        {
            return "GetAllCustomers1";
        }

        public string GetAllCustomers2()
        {
            return "GetAllCustomers2";
        }

        // Have a REST api on a RPC controller. Has unique URL
        [Route("api/resource/{id}")]
        public string GetById(string id)
        {
            return id;
        }
    }

    [Route("partial/{action}")]
    public class PartlyResourcePartlyRpcController : ApiController
    {
        // Normal RPC methods
        [HttpGet]
        public string DoOp1()
        {
            return "op1";
        }

        // Some non-RPC methods.  Has overlapping URL
        [Route("partial/{id:int}")]
        public string GetById(int id)
        {
            return id.ToString();
        }
    }

    [RoutePrefix("apioptional")]
    [Route("{id?}")]
    public class OptionalController : ApiController
    {
        public string GetAllCustomers()
        {
            return "GetAllCustomers";
        }

        public string GetCustomer(int id)
        {
            return "GetCustomer:" + id;
        }
    }

    [RoutePrefix("apioverload")]
    public class OverloadController : ApiController
    {
        [Route("{name}")]
        public string GetAge(string name, int age)
        {
            return "GetAge:" + name + age;
        }

        [Route("{id}")]
        public string GetScore(string id, int score)
        {
            return "GetScore:" + id + score;
        }
    }

    [Route("baseclass", Name = "Base")]
    public class BaseClassController : ApiController
    {
        public string Get(int id)
        {
            return "Get:" + id;
        }
    }

    [Route("subclass", Name = "Sub")]
    public class SubClassController : BaseClassController
    {
        public string Post(string name)
        {
            return "Post:" + name;
        }
    }

    public class SubClassNoRouteController : BaseClassController
    {
        public string Post(string name)
        {
            return "Post:" + name;
        }
    }

    [RoutePrefix("baseclassprefix")]
    public class BaseClassPrefixController : ApiController
    {
        [Route]
        public string GetAll()
        {
            return "Get";
        }

        [Route("base/{id}", Name = "GetById")]
        public string GetById(int id)
        {
            return "Get:" + id;
        }
    }

    public class SubClassNoPrefixController : BaseClassPrefixController
    {
        public string Post(string name)
        {
            return "Post:" + name;
        }
    }

    [RoutePrefix("subclassprefix")]
    public class SubClassPrefixController : BaseClassPrefixController
    {
        [Route]
        public string Post(string name)
        {
            return "Post:" + name;
        }
    }

    [Route("subclassroute")]
    public class SubClassRouteController : BaseClassPrefixController
    {
        public string Post(string name)
        {
            return "Post:" + name;
        }
    }
}