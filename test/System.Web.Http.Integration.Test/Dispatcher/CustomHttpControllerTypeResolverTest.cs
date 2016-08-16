// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.TestCommon;

namespace System.Web.Http.Dispatcher
{
    public class CustomControllerTypeResolverTest : HttpServerTestBase
    {
        internal static readonly string ExpectedContent = "Hello World!";

        public CustomControllerTypeResolverTest()
            : base("http://localhost/")
        {
        }

        protected override void ApplyConfiguration(HttpConfiguration configuration)
        {
            // Add default route
            configuration.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // Set our own assembly resolver where we add the assemblies we need
            CustomControllerTypeResolver customHttpControllerTypeResolver = new CustomControllerTypeResolver();
            configuration.Services.Replace(typeof(IHttpControllerTypeResolver), customHttpControllerTypeResolver);
        }

        [Fact]
        public async Task CustomControllerTypeResolver_ReplacesControllerTypeAndNameConvention()
        {
            // Arrange
            string address = BaseAddress + "api/custominternal";

            // Act
            HttpResponseMessage response = await Client.GetAsync(address);
            string content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(ExpectedContent, content);
        }
    }

    internal class CustomControllerTypeResolver : IHttpControllerTypeResolver
    {
        public ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver)
        {
            return new List<Type> { typeof(CustomInternalController) };
        }
    }

    /// <summary>
    /// Test controller which is declared internal so wouldn't not get picked up by
    /// <see cref="DefaultHttpControllerTypeResolver"/>.
    /// </summary>
    internal class CustomInternalController : ApiController
    {
        public HttpResponseMessage Get()
        {
            HttpResponseMessage response = Request.CreateResponse();
            response.Content = new StringContent(CustomControllerTypeResolverTest.ExpectedContent);
            return response;
        }
    }
}
