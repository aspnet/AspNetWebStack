// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.TestCommon;
using Newtonsoft.Json.Linq;

namespace System.Web.Http.SelfHost
{
    public class DeepNestedTypeTests
    {
        private HttpSelfHostServer server = null;
        private string baseAddress = null;
        private HttpClient httpClient = null;

        public DeepNestedTypeTests()
        {
            this.SetupHost();
        }

        private void SetupHost()
        {
            baseAddress = String.Format("http://localhost/");

            HttpSelfHostConfiguration config = new HttpSelfHostConfiguration(baseAddress);
            config.HostNameComparisonMode = HostNameComparisonMode.Exact;
            config.Routes.MapHttpRoute("Default", "{controller}/{action}", new { controller = "DeepNestedType" });

            server = new HttpSelfHostServer(config);

            httpClient = new HttpClient(server);
        }

        [Fact]
        public async Task PostDeeplyNestedTypeInXmlThrows()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = new Uri(Path.Combine(baseAddress, "DeepNestedType/PostNest"));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            request.Method = HttpMethod.Post;
            request.Content = new StringContent(GetNestedObjectInXml(8000), UTF8Encoding.UTF8, "application/xml");

            // Action
            HttpResponseMessage response = await httpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        [Fact]
        public async Task PostNotTooDeeplyNestedTypeInXmlWorks()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = new Uri(Path.Combine(baseAddress, "DeepNestedType/PostNest"));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            request.Method = HttpMethod.Post;
            request.Content = new StringContent(GetNestedObjectInXml(20), UTF8Encoding.UTF8, "application/xml");

            // Action
            HttpResponseMessage response = await httpClient.SendAsync(request);

            // Assert
            string expectedResponseValue = @"<string xmlns=""http://schemas.microsoft.com/2003/10/Serialization/"">success from PostNest</string>";
            Assert.NotNull(response.Content);
            Assert.NotNull(response.Content.Headers.ContentType);
            Assert.Equal("application/xml", response.Content.Headers.ContentType.MediaType);
            Assert.Equal(expectedResponseValue, await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task PostDeeplyNestedTypeInFormUrlEncodedThrows()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = new Uri(Path.Combine(baseAddress, "DeepNestedType/PostJToken"));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            request.Method = HttpMethod.Post;
            request.Content = new StringContent(GetNestedObjectInFormUrl(5000), UTF8Encoding.UTF8, "application/x-www-form-urlencoded");

            // Action
            HttpResponseMessage response = await httpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        [Fact]
        public async Task PostNotTooDeeplyNestedTypeInFormUrlEncodedWorks()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = new Uri(Path.Combine(baseAddress, "DeepNestedType/PostJToken"));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            request.Method = HttpMethod.Post;
            request.Content = new StringContent(GetNestedObjectInFormUrl(20), UTF8Encoding.UTF8, "application/x-www-form-urlencoded");

            // Action
            HttpResponseMessage response = await httpClient.SendAsync(request);

            // Assert
            string expectedResponseValue = @"<string xmlns=""http://schemas.microsoft.com/2003/10/Serialization/"">success from PostJToken</string>";
            Assert.NotNull(response.Content);
            Assert.NotNull(response.Content.Headers.ContentType);
            Assert.Equal("application/xml", response.Content.Headers.ContentType.MediaType);
            Assert.Equal(expectedResponseValue, await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task PostNestedListInFormUrlEncodedWorks()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = new Uri(Path.Combine(baseAddress, "DeepNestedType/PostNestedList"));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            request.Method = HttpMethod.Post;
            request.Content = new StringContent(GetBigListInFormUrl(70000), Encoding.UTF8, "application/x-www-form-urlencoded");

            // Act
            HttpResponseMessage response = await httpClient.SendAsync(request);

            // Assert
            string expectedResponseValue = @"<string xmlns=""http://schemas.microsoft.com/2003/10/Serialization/"">success from PostNestedList</string>";
            Assert.NotNull(response.Content);
            Assert.NotNull(response.Content.Headers.ContentType);
            Assert.Equal("application/xml", response.Content.Headers.ContentType.MediaType);
            Assert.Equal(expectedResponseValue, await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task PostBigArrayWorks()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = new Uri(Path.Combine(baseAddress, "DeepNestedType/PostXElement"));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            request.Method = HttpMethod.Post;

            request.Content = new StringContent(GetBigArray(5000), Encoding.UTF8, "application/xml");

            // Act
            HttpResponseMessage response = await httpClient.SendAsync(request);

            // Assert
            string expectedResponseValue = @"<string xmlns=""http://schemas.microsoft.com/2003/10/Serialization/"">success from PostXElement</string>";
            Assert.NotNull(response.Content);
            Assert.NotNull(response.Content.Headers.ContentType);
            Assert.Equal("application/xml", response.Content.Headers.ContentType.MediaType);
            Assert.Equal(expectedResponseValue, await response.Content.ReadAsStringAsync());
        }

        /*
        <?xml version="1.0"?>
        <ArrayOfString xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
        <string />
        <string />
        <string />
        </ArrayOfString>
        */
            private string GetBigArray(int arraySize)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < arraySize; i++)
                {
                    sb.Append("<string/>");
                }

                sb.Insert(0, "<ArrayOfString>");
                sb.Append("</ArrayOfString>");
                sb.Insert(0, "<?xml version=\"1.0\"?>");

                return sb.ToString();
            }

        private string GetNestedObjectInXml(int depth)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < depth; i++)
            {
                sb.Insert(0, "<A>");
                sb.Append("</A>");
            }

            sb.Insert(0, "<Nest xmlns=\"http://schemas.datacontract.org/2004/07/System.Web.Http.SelfHost\">");
            sb.Append("</Nest>");
            sb.Insert(0, "<?xml version=\"1.0\"?>");

            return sb.ToString();
        }

        private string GetNestedObjectInFormUrl(int depth)
        {
            StringBuilder sb = new StringBuilder("a");
            for (int i = 0; i < depth; i++)
            {
                sb.Append("[a]");
            }
            sb.Append("=1");
            return sb.ToString();
        }

        private string GetBigListInFormUrl(int depth)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("a");
            for (int i = 0; i < depth; i++)
            {
                sb.Append("[N]");
            }
            sb.Append("[D]=1");
            return sb.ToString();
        }
    }

    public class DeepNestedTypeController : ApiController
    {
        public string PostNest(Nest a)
        {
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError));
            }
            return "success from PostNest";
        }

        public string PostJToken(JToken token)
        {
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError));
            }
            return "success from PostJToken";
        }

        public string PostNestedList(MyList a)
        {
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError));
            }
            return "success from PostNestedList";
        }

        public string PostXElement(XElement input)
        {
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError));
            }

            return "success from PostXElement";
        }
    }

    public class Nest
    {
        public Nest A { get; set; }
    }

    public class MyList
    {
        public int D { get; set; }
        public MyList N { get; set; }
    }

}
