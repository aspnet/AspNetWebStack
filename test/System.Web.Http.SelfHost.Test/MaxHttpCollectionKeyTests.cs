// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TestCommon;
using Newtonsoft.Json.Linq;

namespace System.Web.Http.SelfHost
{
    public class MaxHttpCollectionKeyTests
    {
        private HttpServer server = null;
        private string baseAddress = null;
        private HttpClient httpClient = null;

        public MaxHttpCollectionKeyTests()
        {
            this.SetupHost();
        }

        private void SetupHost()
        {
            baseAddress = String.Format("http://localhost/");

            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute("Default", "{controller}/{action}", new { controller = "MaxHttpCollectionKeyType" });

            server = new HttpServer(config);

            httpClient = new HttpClient(server);
        }

        [Theory]
        [InlineData("PostCustomer")]
        [InlineData("PostFormData")]
        public async Task PostManyKeysInFormUrlEncodedThrows(string actionName)
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = new Uri(Path.Combine(baseAddress, "MaxHttpCollectionKeyType/" + actionName));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            request.Method = HttpMethod.Post;
            request.Content = new StringContent(GenerateHttpCollectionKeyInput(100), UTF8Encoding.UTF8, "application/x-www-form-urlencoded");
            MediaTypeFormatter.MaxHttpCollectionKeys = 99;

            // Action
            HttpResponseMessage response = await httpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            string expectedResponseValue = @"The number of keys in a NameValueCollection has exceeded the limit of '99'. You can adjust it by modifying the MaxHttpCollectionKeys property on the 'System.Net.Http.Formatting.MediaTypeFormatter' class.";
            Assert.Equal(expectedResponseValue, await response.Content.ReadAsStringAsync());
        }

        [Theory]
        [InlineData("PostCustomer")]
        [InlineData("PostFormData")]
        public async Task PostNotTooManyKeysInFormUrlEncodedWorks(string actionName)
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = new Uri(Path.Combine(baseAddress, "MaxHttpCollectionKeyType/" + actionName));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            request.Method = HttpMethod.Post;
            request.Content = new StringContent(GenerateHttpCollectionKeyInput(100), UTF8Encoding.UTF8, "application/x-www-form-urlencoded");
            MediaTypeFormatter.MaxHttpCollectionKeys = 1000;

            // Action
            HttpResponseMessage response = await httpClient.SendAsync(request);

            // Assert
            string expectedResponseValue = @"<string xmlns=""http://schemas.microsoft.com/2003/10/Serialization/"">success from " + actionName + "</string>";
            Assert.NotNull(response.Content);
            Assert.NotNull(response.Content.Headers.ContentType);
            Assert.Equal("application/xml", response.Content.Headers.ContentType.MediaType);
            Assert.Equal(expectedResponseValue, await response.Content.ReadAsStringAsync());
        }

        [Theory]
        [InlineData("PostCustomerFromUri")]
        [InlineData("GetWithQueryable")]
        public async Task PostManyKeysInUriThrows(string actionName)
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = new Uri(Path.Combine(baseAddress, "MaxHttpCollectionKeyType/" + actionName + "/?" + GenerateHttpCollectionKeyInput(100)));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));

            if (actionName.StartsWith("Post"))
            {
                request.Method = HttpMethod.Post;
                request.Content = new StringContent("", UTF8Encoding.UTF8, "application/x-www-form-urlencoded");
            }
            else
            {
                request.Method = HttpMethod.Get;
            }

            MediaTypeFormatter.MaxHttpCollectionKeys = 99;

            // Action
            HttpResponseMessage response = await httpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        [Theory]
        [InlineData("PostCustomerFromUri")]
        [InlineData("GetWithQueryable")]
        public async Task PostNotTooManyKeysInUriWorks(string actionName)
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = new Uri(Path.Combine(baseAddress, "MaxHttpCollectionKeyType/" + actionName + "/?" + GenerateHttpCollectionKeyInput(100)));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));

            if (actionName.StartsWith("Post"))
            {
                request.Method = HttpMethod.Post;
                request.Content = new StringContent("", UTF8Encoding.UTF8, "application/x-www-form-urlencoded");
            }
            else
            {
                request.Method = HttpMethod.Get;
            }

            MediaTypeFormatter.MaxHttpCollectionKeys = 1000;

            // Action
            HttpResponseMessage response = await httpClient.SendAsync(request);

            // Assert
            string expectedResponseValue = @"success from " + actionName;
            Assert.NotNull(response.Content);
            Assert.NotNull(response.Content.Headers.ContentType);
            Assert.Equal("application/xml", response.Content.Headers.ContentType.MediaType);
            Assert.Contains(expectedResponseValue, await response.Content.ReadAsStringAsync());
        }

        private static string GenerateHttpCollectionKeyInput(int num)
        {
            StringBuilder sb = new StringBuilder("a=0");

            for (int i = 0; i < num; i++)
            {
                sb.Append("&");
                sb.Append(i.ToString());
                sb.Append("=0");
            }

            return sb.ToString();
        }
    }

    public class MaxHttpCollectionKeyTypeController : ApiController
    {
        // Post strongly typed Customer
        public string PostCustomer(Customer a)
        {
            if (!ModelState.IsValid)
            {
                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                ModelBinding.ModelState value = null;
                ModelState.TryGetValue("a", out value);
                response.Content = new StringContent(value.Errors[0].Exception.Message);
                throw new HttpResponseException(response);
            }

            return "success from PostCustomer";
        }

        // Post form data
        public string PostFormData(FormDataCollection a)
        {
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest));
            }

            try
            {
                NameValueCollection collection = a.ReadAsNameValueCollection();
            }
            catch (InvalidOperationException ex)
            {
                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = new StringContent(ex.Message);
                throw new HttpResponseException(response);
            }

            return "success from PostFormData";
        }

        public string PostCustomerFromUri([FromUri]Customer a)
        {
            if (!ModelState.IsValid)
            {
                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                ModelBinding.ModelState value = null;
                ModelState.TryGetValue("a", out value);
                response.Content = new StringContent(value.Errors[0].ErrorMessage);
                throw new HttpResponseException(response);
            }

            return "success from PostCustomerFromUri";
        }

        public IQueryable<string> GetWithQueryable()
        {
            return new List<string>() { "success from GetWithQueryable" }.AsQueryable();
        }

        public string PostJToken(JToken token)
        {
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest));
            }

            return "success from PostJToken";
        }
    }

    public class Customer
    {
        public string Name { get; set; }
        public int Age { get; set; }

        public override string ToString()
        {
            return "ModelBindingItem(" + Name + "," + Age + ")";
        }
    }
}
