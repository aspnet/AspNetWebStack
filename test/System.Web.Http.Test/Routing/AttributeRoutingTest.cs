// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using Microsoft.TestCommon;

namespace System.Web.Http.Routing
{
    public class AttributeRoutingTest
    {
        [Theory]
        [InlineData("GET", "controller/42", "Get42")]
        // Tests inline route constraints
        [InlineData("GET", "controller/Ethan", "GetByNameEthan")]
        // Tests the HTTP method constraint
        [InlineData("PUT", "controller/42", "Put42")]
        // Tests route consolidation (or lack thereof)
        [InlineData("PUT", "controller/42?name=foo", "Put42foo")]
        // Tests optional parameters
        [InlineData("GET", "optional/1/2", "Optional12")]
        [InlineData("GET", "optional/1", "Optional1opt")]
        [InlineData("GET", "optional", "Optional8opt")]
        [InlineData("GET", "optionalwconstraint", "OptionalWithConstraintx")]
        [InlineData("GET", "optionalwnullable/12", "Optional12")]
        [InlineData("GET", "optionalwnullable", "Optional")]
        [InlineData("GET", "apibadcontrollerx/int/12", "GetInt12")]
        [InlineData("GET", "apibadcontrollerx/nullableint/12", "GetNullable12")]
        [InlineData("GET", "apibadcontrollerx/string/12", "GetString12")]
        // Tests default values
        [InlineData("GET", "default/1/2", "Default12")]
        [InlineData("GET", "default/1", "Default1D2")]
        [InlineData("GET", "default", "DefaultD1D2")]
        // Test wildcard parameters
        [InlineData("GET", "wildcard/a/b/c", "Wildcarda/b/c")]
        // Test prefixes
        [InlineData("GET", "prefix", "PrefixedGet")]
        [InlineData("GET", "prefix/123", "PrefixedGetById123")]
        [InlineData("PUT", "prefix", "PrefixedPut")]
        // Test multiple controllerRouteFactories to same action
        [InlineData("DELETE", "multi1", "multi")]
        [InlineData("DELETE", "multi2", "multi")]
        // Test multiple verbs on the same route
        [InlineData("GET", "multiverb", "GET")]
        [InlineData("PUT", "multiverb", "PUT")]
        // Test with default route
        [InlineData("GET", "prefix2/defaultroute/12", "get12")]
        [InlineData("PUT", "prefix2/defaultrouteoverride/12", "put12")]
        [InlineData("POST", "prefix2", "post")]
        // {action} values
        [InlineData("GET", "api/default2/GetAllCustomers1", "GetAllCustomers1")]
        [InlineData("GET", "api/resource/12", "12")]
        [InlineData("GET", "apiactionstress/ActionY/ActionX?useX=5", "XActionY5")]
        [InlineData("GET", "apiactionstress/ActionY/ActionX?useY=7", "YActionX7")]
        // Mixing {action} with REST
        [InlineData("GET", "partial/DoOp1", "op1")]
        [InlineData("GET", "partial/154", "154")]
        // Optional on controller [Route]
        [InlineData("GET", "apioptional", "GetAllCustomers")]
        [InlineData("GET", "apioptional/57", "GetCustomer:57")]
        // Overload resolution
        [InlineData("GET", "apioverload/Fred?age=12", "GetAge:Fred12")]
        [InlineData("GET", "apioverload/Fred?score=12", "GetScore:Fred12")]
        // Controller route attribute inheritance
        [InlineData("GET", "subclass?id=8", "Get:8")]
        [InlineData("POST", "subclass?name=foo", "Post:foo")]
        [InlineData("GET", "api/subclassnoroute?id=8", "Get:8")]
        [InlineData("POST", "api/subclassnoroute?name=foo", "Post:foo")]
        [InlineData("GET", "baseclass?id=9", "Get:9")]
        [InlineData("GET", "baseclassprefix", "Get")]
        [InlineData("GET", "baseclassprefix/base/8", "Get:8")]
        [InlineData("GET", "api/subclassnoprefix", "Get")]
        [InlineData("GET", "api/subclassnoprefix?id=9", "Get:9")]
        [InlineData("POST", "api/subclassnoprefix?name=foo", "Post:foo")]
        [InlineData("POST", "subclassprefix?name=foo", "Post:foo")]
        [InlineData("GET", "api/subclassprefix", "Get")]
        [InlineData("GET", "api/subclassprefix?id=3", "Get:3")]
        [InlineData("GET", "subclassroute", "Get")]
        [InlineData("GET", "subclassroute?id=9", "Get:9")]
        [InlineData("POST", "subclassroute?name=foo", "Post:foo")]
        // Route order
        [InlineData("GET", "routeorder/11", "GetById:11")]
        [InlineData("GET", "routeorder/name", "GetByName:name")]
        [InlineData("GET", "routeorder/literal", "GetLiteral")]
        [InlineData("GET", "routeorderoverload", "Get")]
        [InlineData("GET", "routeorderoverload?name=name&id=1", "GetByNameAndId:name1")]
        [InlineData("GET", "routeorderoverload?name=name", "GetByName:name")]
        // Route precedence
        [InlineData("GET", "routeprecedence/11", "GetById:11")]
        [InlineData("GET", "routeprecedence/name", "GetByName:name")]
        [InlineData("GET", "routeprecedence/literal", "GetLiteral")]
        [InlineData("GET", "routeprecedence/name?id=20", "GetByNameAndId:name20")]
        [InlineData("GET", "constraint", "pass")]
        public async Task AttributeRouting_RoutesToAction(string httpMethod, string uri, string responseBody)
        {
            var request = new HttpRequestMessage(new HttpMethod(httpMethod), "http://localhost/" + uri);

            var response = await SubmitRequestAsync(request);

            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(responseBody, GetContentValue<string>(response));
        }

        [Theory]
        // default controllerRouteFactories
        [InlineData("GET", "prefix2/defaultroute/name", HttpStatusCode.NotFound)] // miss route constraint
        [InlineData("PUT", "prefix2/defaultroute/12", HttpStatusCode.MethodNotAllowed)] // override, different url
        [InlineData("POST", "prefix", HttpStatusCode.MethodNotAllowed)]
        // wrong verb, 405
        [InlineData("MISSING", "controller/42", HttpStatusCode.MethodNotAllowed)]
        [InlineData("MISSING", "default/1/2", HttpStatusCode.MethodNotAllowed)]
        [InlineData("MISSING", "controller/Ethan", HttpStatusCode.MethodNotAllowed)]
        // accessing attribute routed method via standard route
        [InlineData("GET", "api/Attributed?id=42", HttpStatusCode.NotFound)]
        [InlineData("GET", "api/DefaultRoute?id=42", HttpStatusCode.NotFound)]
        [InlineData("GET", "api/Default2/GetById", HttpStatusCode.NotFound)]
        [InlineData("GET", "api/Default2/MethodNotFound", HttpStatusCode.NotFound)]
        // Ambiguous match
        [InlineData("GET", "apioverload/Fred?score=12&age=23", HttpStatusCode.InternalServerError)]
        [InlineData("GET", "apiactionstress/ActionY/ActionX?useY=7&useX=8", HttpStatusCode.InternalServerError)]
        // Unreachable inherited controllerRouteFactories
        [InlineData("GET", "api/subclassroute", HttpStatusCode.NotFound)]
        [InlineData("GET", "api/subclassroute?id=9", HttpStatusCode.NotFound)]
        [InlineData("POST", "api/subclassroute?name=foo", HttpStatusCode.NotFound)]
        [InlineData("GET", "api/baseclass?id=2", HttpStatusCode.NotFound)]
        [InlineData("GET", "api/baseclassprefix", HttpStatusCode.NotFound)]
        [InlineData("GET", "api/baseclassprefix?id=2", HttpStatusCode.NotFound)]
        // Default value is required, 500 would be a better error, but important thing is we fail
        [InlineData("GET", "apibadcontrollerx/int", HttpStatusCode.NotFound)]
        [InlineData("GET", "apibadcontrollerx/nullableint", HttpStatusCode.NotFound)]
        [InlineData("GET", "apibadcontrollerx/string", HttpStatusCode.NotFound)]
        public async Task AttributeRouting_Failures(string httpMethod, string uri, HttpStatusCode failureCode)
        {
            var request = new HttpRequestMessage(new HttpMethod(httpMethod), "http://localhost/" + uri);

            var response = await SubmitRequestAsync(request);

            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(failureCode, response.StatusCode);
        }

        [Fact]
        public async Task AttributeRouting_MultipleControllerMatches()
        {
            var request = new HttpRequestMessage(new HttpMethod("GET"), "http://localhost/ambiguousmatch");

            var response = await SubmitRequestAsync(request);

            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        [Fact]
        public void RoutePrefixAttribute_IsSingleInstance()
        {
            var attr = typeof(RoutePrefixAttribute);
            var attrs = attr.GetCustomAttributes(typeof(AttributeUsageAttribute), false);
            var usage = (AttributeUsageAttribute)attrs[0];

            Assert.Equal(AttributeTargets.Class, usage.ValidOn);
            Assert.False(usage.AllowMultiple); // only 1 per class
            Assert.True(usage.Inherited); // RoutePrefix is not inherited.
        }

        [Theory]
        [InlineData("GET", "NS1Home/Introduction", "Home.Index()")]
        [InlineData("GET", "NS2Account/PeopleList", "Account.Index()")]
        [InlineData("GET", "CustomizedDefaultPrefix/Unknown", "Default.Index()")]
        public async Task AttributeRouting_RoutesToAction_WithCustomizedRoutePrefix(string httpMethod, string uri, string responseBody)
        {
            var request = new HttpRequestMessage(new HttpMethod(httpMethod), "http://localhost/" + uri);

            var response = await SubmitRequestAsync(request);

            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(responseBody, GetContentValue<string>(response));
        }

        [Fact]
        public async Task AttributeRouting_DirectRouteProvider_ControllerRoute()
        {
            var controllerRoutes = new Dictionary<Type, IEnumerable<IDirectRouteFactory>>()
            {
                { typeof(DirectRouteProviderController), new[] { new RouteAttribute("CoolRouteBro") } }
            };

            var routeProvider = new DirectRouteProvider(controllerRoutes, null);


            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/CoolRouteBro");

            var response = await SubmitRequestAsync(request, routeProvider);

            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("DirectRouteProviderController.Get239303030()", GetContentValue<string>(response));
        }

        [Fact]
        public async Task AttributeRouting_DirectRouteProvider_ControllerRoute_TraditionalRouteDoesntMatch()
        {
            var controllerRoutes = new Dictionary<Type, IEnumerable<IDirectRouteFactory>>()
            {
                { typeof(DirectRouteProviderController), new[] { new RouteAttribute("CoolRouteBro") } }
            };

            var routeProvider = new DirectRouteProvider(controllerRoutes, null);


            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/DirectRouteProvider");

            var response = await SubmitRequestAsync(request, routeProvider);

            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task AttributeRouting_DirectRouteProvider_ActionRoute()
        {
            var actionRoutes = new Dictionary<string, IEnumerable<IDirectRouteFactory>>()
            {
                { "Get239303030", new[] { new RouteAttribute("CoolRouteBro") } }
            };

            var routeProvider = new DirectRouteProvider(null, actionRoutes);


            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/CoolRouteBro");

            var response = await SubmitRequestAsync(request, routeProvider);

            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("DirectRouteProviderController.Get239303030()", GetContentValue<string>(response));
        }

        private static async Task<HttpResponseMessage> SubmitRequestAsync(HttpRequestMessage request, IDirectRouteProvider routeProvider = null)
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute("DefaultApi", "api/{controller}");
            if (routeProvider == null)
            {
                config.MapHttpAttributeRoutes();
            }
            else
            {
                config.MapHttpAttributeRoutes(routeProvider);
            }

            HttpResponseMessage response;
            HttpServer server = new HttpServer(config);
            using (HttpMessageInvoker client = new HttpMessageInvoker(server))
            {
                response = await client.SendAsync(request, CancellationToken.None);
            }

            return response;
        }

        private static T GetContentValue<T>(HttpResponseMessage response)
        {
            T value;
            response.TryGetContentValue<T>(out value);
            return value;
        }

        private class DirectRouteProvider : DefaultDirectRouteProvider
        {
            private readonly IDictionary<Type, IEnumerable<IDirectRouteFactory>> _controllerRouteFactories;
            private readonly IDictionary<string, IEnumerable<IDirectRouteFactory>> _actionRouteFactories;

            public DirectRouteProvider(
                IDictionary<Type, IEnumerable<IDirectRouteFactory>> controllerRouteFactories,
                IDictionary<string, IEnumerable<IDirectRouteFactory>> actionRouteFactories)
            {
                _controllerRouteFactories = controllerRouteFactories ?? new Dictionary<Type, IEnumerable<IDirectRouteFactory>>();
                _actionRouteFactories = actionRouteFactories ?? new Dictionary<string, IEnumerable<IDirectRouteFactory>>();
            }

            protected override IReadOnlyList<IDirectRouteFactory> GetControllerRouteFactories(HttpControllerDescriptor controllerDescriptor)
            {
                IEnumerable<IDirectRouteFactory> factories;
                _controllerRouteFactories.TryGetValue(controllerDescriptor.ControllerType, out factories);
                return factories == null ? null : factories.ToList();
            }

            protected override IReadOnlyList<IDirectRouteFactory> GetActionRouteFactories(HttpActionDescriptor actionDescriptor)
            {
                IEnumerable<IDirectRouteFactory> factories;
                _actionRouteFactories.TryGetValue(actionDescriptor.ActionName, out factories);
                return factories == null ? null : factories.ToList();
            }
        }
    }

    public class DirectRouteProviderController : ApiController
    {
        public string Get239303030()
        {
            return "DirectRouteProviderController.Get239303030()";
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

        // Optional route parameters still require a default value in the signature.
        [HttpGet]
        [Route("optional/{opt1?}/{opt2?}")]
        public string Optional(int opt1 = 8, string opt2 = "opt")
        {
            return "Optional" + opt1 + opt2;
        }

        [HttpGet]
        [Route("optionalwnullable/{opt1?}")]
        public string Optional(int? opt1 = null)
        {
            return "Optional" + opt1;
        }

        [HttpGet]
        [Route("optionalwconstraint/{opt:int?}")]
        public string OptionalWithConstraint(string opt = "x")
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

    // Routes have optional parameters, but signature says it's required.
    // Try with value-type, reference type, and nullable.
    public class OptionalParameterController : ApiController
    {
        // Optional in route, required in signature.
        [Route("apibadcontrollerx/int/{id?}")]
        public string Get(int id)
        {
            return "GetInt" + id;
        }

        [Route("apibadcontrollerx/nullableint/{id?}")]
        public string GetNullable(int? id)
        {
            return "GetNullable" + id;
        }

        [Route("apibadcontrollerx/string/{id?}")]
        public string GetString(string id)
        {
            return "GetString" + id;
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

    [RoutePrefix("apitokens")]
    public class TokensController : ApiController
    {
        [Route("{id:int}")]
        public string GetById(int id)
        {
            return "id" + id;
        }

        [Route("{name}")]
        public string GetByName(string name)
        {
            return "name" + name;
        }

        [Route("{id:int}")]
        public string GetDetails(int id, string name)
        {
            return "id" + id + "name" + name;
        }
    }

    // Stress test for action selection. This stresses that the union route really keeps the various
    // sub routes separate and properly elevates the correct one.
    // Uses query string parameters to disambiguate.
    [RoutePrefix("apiactionstress")]
    [Route("{x}/{action}")]
    [Route("{action}/{y}")]
    public class ActionStressController : ApiController
    {
        [HttpGet]
        public string ActionX(string x, int useX)
        {
            return "X" + x + useX;
        }

        [HttpGet]
        public string ActionY(string y, int useY)
        {
            return "Y" + y + useY;
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

    public class RouteOrderController : ApiController
    {
        [Route("routeorder/{id:int}", Order = 1)]
        public string GetById(int id)
        {
            return "GetById:" + id;
        }

        [Route("routeorder/{name}", Order = 2)]
        public string GetByName(string name)
        {
            return "GetByName:" + name;
        }

        [Route("routeorder/literal", Order = 0)]
        public string GetLiteral()
        {
            return "GetLiteral";
        }
    }

    public class RouteOrderOverloadController : ApiController
    {
        [Route("routeorderoverload", Order = 1)]
        public string GetByNameAndId(string name, int id)
        {
            return "GetByNameAndId:" + name + id;
        }

        [Route("routeorderoverload", Order = 2)]
        public string GetByName(string name)
        {
            return "GetByName:" + name;
        }

        [Route("routeorderoverload", Order = 3)]
        public string Get()
        {
            return "Get";
        }
    }

    public class RoutePrecedenceController : ApiController
    {
        [Route("routeprecedence/{id:int}")]
        public string GetById(int id)
        {
            return "GetById:" + id;
        }

        [Route("routeprecedence/{name}")]
        public string GetByName(string name)
        {
            return "GetByName:" + name;
        }

        [Route("routeprecedence/{name}")]
        public string GetByNameAndId(string name, int id)
        {
            return "GetByNameAndId:" + name + id;
        }

        [Route("routeprecedence/literal")]
        public string GetLiteral()
        {
            return "GetLiteral";
        }
    }

    [RoutePrefix("constraint")]
    public class ConstraintController : ApiController
    {
        [ConstrainedRoute(Order = 1, ConstraintMatches = true)]
        public string GetHigherOrderWithMatchingContsraint()
        {
            return "pass";
        }

        [ConstrainedRoute(Order = 0, ConstraintMatches = false)]
        public string GetLowerOrderWithNonMatchingConstraint()
        {
            return "fail";
        }

        private class ConstrainedRouteAttribute : RouteFactoryAttribute
        {
            public ConstrainedRouteAttribute()
                : base(null)
            {
            }

            public bool ConstraintMatches { get; set; }

            public override IDictionary<string, object> Constraints
            {
                get
                {
                    return new HttpRouteValueDictionary()
                    {
                        { String.Empty, new Constraint(ConstraintMatches) }
                    };
                }
            }

            private class Constraint : IHttpRouteConstraint
            {
                private readonly bool _matches;

                public Constraint(bool matches)
                {
                    _matches = matches;
                }

                public bool Match(HttpRequestMessage request, IHttpRoute route, string parameterName,
                    IDictionary<string, object> values, HttpRouteDirection routeDirection)
                {
                    return _matches;
                }
            }
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class CustomizedRoutePrefixAttribute : Attribute, IRoutePrefix
    {
        public CustomizedRoutePrefixAttribute(Type controller)
        {
            if (controller == null)
            {
                throw Error.ArgumentNull("prefix");
            }

            if (controller.Equals(typeof(HomeWithCustomizedRoutePrefixController)))
            {
                Prefix = "NS1Home";
            }
            else if (controller.Equals(typeof(AccountWithCustomizedRoutePrefixController)))
            {
                Prefix = "NS2Account";
            }
            else
            {
                Prefix = "CustomizedDefaultPrefix";
            }
        }

        public string Prefix { get; private set; }
    }

    [CustomizedRoutePrefix(typeof(HomeWithCustomizedRoutePrefixController))]
    public class HomeWithCustomizedRoutePrefixController : ApiController
    {
        [Route("Introduction")]
        public string Get()
        {
            return "Home.Index()";
        }
    }

    [CustomizedRoutePrefix(typeof(AccountWithCustomizedRoutePrefixController))]
    public class AccountWithCustomizedRoutePrefixController : ApiController
    {
        [Route("PeopleList")]
        public string Get()
        {
            return "Account.Index()";
        }
    }

    [CustomizedRoutePrefix(typeof(OtherWithCustomizedRoutePrefixController))]
    public class OtherWithCustomizedRoutePrefixController : ApiController
    {
        [Route("Unknown")]
        public String Get()
        {
            return "Default.Index()";
    }
    }

    [Route("ambiguousmatch")]
    public class AmbiguousMatch1Controller : ApiController
    {
        public string Get()
        {
            return "Get()";
        }
    }

    [Route("ambiguousmatch")]
    public class AmbiguousMatch2Controller : ApiController
    {
        public string Get()
        {
            return "Get()";
        }
    }
}
