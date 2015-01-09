// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.TestCommon;
using Newtonsoft.Json;

namespace System.Web.Http.ModelBinding
{
    /// <summary>
    /// End-to-end tests for model binding.
    /// </summary>
    public class ModelBindingEndToEndTests
    {
        [Fact]
        public async Task BindModel_BindsValuesFromUrl()
        {
            // Arrange
            string value = "some-value";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get,
                            "http://localhost/ModelBinding/Url?somekey=" + value);

            // Act
            HttpResponseMessage response = await SubmitRequestAsync(request);

            // Assert
            Assert.Equal(value, await ReadAsJson<string>(response));
        }

        [Fact]
        public async Task BindModel_BindsSimpleTypesFromBody()
        {
            // Arrange
            Dictionary<string, string> bodyParameters = new Dictionary<string, string>
            {
                { "Name", "PersonName" },
                { "Sibling.Name", "SiblingName" },
            };
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post,
                            "http://localhost/ModelBinding/SimpleType")
            {
                Content = new FormUrlEncodedContent(bodyParameters)
            };

            // Act
            HttpResponseMessage response = await SubmitRequestAsync(request);

            // Assert
            Person result = await ReadAsJson<Person>(response);
            Assert.Equal("PersonName", result.Name);
            Assert.Equal("SiblingName", result.Sibling.Name);
        }

        [Fact]
        public async Task BindModel_WithCollection()
        {
            // Arrange
            Dictionary<string, string> bodyParameters = new Dictionary<string, string>
            {
                { "AddressLines[0].Line", "Street Address 0" },
                { "AddressLines[1].Line", "Street Address 1" },
                { "ZipCode", "98052" },
            };
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put,
                            "http://localhost/ModelBinding/CollectionType")
            {
                Content = new FormUrlEncodedContent(bodyParameters)
            };

            // Act
            HttpResponseMessage response = await SubmitRequestAsync(request);

            // Assert
            Address address = await ReadAsJson<Address>(response);
            Assert.Equal(2, address.AddressLines.Count);
            Assert.Equal("Street Address 0", address.AddressLines[0].Line);
            Assert.Equal("Street Address 1", address.AddressLines[1].Line);
            Assert.Equal("98052", address.ZipCode);
        }

        [Fact]
        public async Task BindModel_WithCollection_SpecifyingIndex()
        {
            // Arrange
            IEnumerable<KeyValuePair<string, string>> bodyParameters = new[]
            {
                new KeyValuePair<string, string>("AddressLines.index", "3"),
                new KeyValuePair<string, string>("AddressLines.index", "10000"),
                new KeyValuePair<string, string>("AddressLines[3].Line", "Street Address 0"),
                new KeyValuePair<string, string>("AddressLines[10000].Line", "Street Address 1"),
            };
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put,
                            "http://localhost/ModelBinding/CollectionType")
            {
                Content = new FormUrlEncodedContent(bodyParameters)
            };

            // Act
            HttpResponseMessage response = await SubmitRequestAsync(request);

            // Assert
            Address address = await ReadAsJson<Address>(response);
            Assert.Equal(2, address.AddressLines.Count);
            Assert.Equal("Street Address 0", address.AddressLines[0].Line);
            Assert.Equal("Street Address 1", address.AddressLines[1].Line);
        }

        [Fact]
        public async Task BindModel_WithNestedCollection()
        {
            // Arrange
            Dictionary<string, string> bodyParameters = new Dictionary<string, string>
            {
                { "Addresses[0].AddressLines[0].Line", "Street Address 00" },
                { "Addresses[0].AddressLines[1].Line", "Street Address 01" },
                { "Addresses[0].ZipCode", "98052" },
                { "Addresses[1].AddressLines[0].Line", "Street Address 10" },
                { "Addresses[1].AddressLines[3].Line", "Street Address 13" },
            };
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post,
                            "http://localhost/ModelBinding/NestedCollection")
            {
                Content = new FormUrlEncodedContent(bodyParameters)
            };

            // Act
            HttpResponseMessage response = await SubmitRequestAsync(request);

            // Assert
            UserWithAddress result = await ReadAsJson<UserWithAddress>(response);
            Assert.Equal(2, result.Addresses.Count);
            Address address = result.Addresses[0];
            Assert.Equal(2, address.AddressLines.Count);
            Assert.Equal("Street Address 00", address.AddressLines[0].Line);
            Assert.Equal("Street Address 01", address.AddressLines[1].Line);
            Assert.Equal("98052", address.ZipCode);

            address = result.Addresses[1];
            Assert.Single(address.AddressLines);
            Assert.Equal("Street Address 10", address.AddressLines[0].Line);
            Assert.Null(address.ZipCode);
        }

        [Fact]
        public async Task BindModel_WithIncorrectlyFormattedNestedCollectionValue()
        {
            // Arrange
            Dictionary<string, string> bodyParameters = new Dictionary<string, string>
            {
                { "Addresses", "Street Address 00" },
            };
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post,
                            "http://localhost/ModelBinding/NestedCollection")
            {
                Content = new FormUrlEncodedContent(bodyParameters)
            };

            // Act
            HttpResponseMessage response = await SubmitRequestAsync(request);

            // Assert
            UserWithAddress result = await ReadAsJson<UserWithAddress>(response);
            Address address = Assert.Single(result.Addresses);
            Assert.Null(address.AddressLines);
            Assert.Null(address.ZipCode);
        }

        [Fact]
        public async Task BindModel_WithNestedCollectionContainingARecursiveRelation()
        {
            // Arrange
            Dictionary<string, string> bodyParameters = new Dictionary<string, string>
            {
                { "People[0].Name", "Person 0" },
                { "People[0].Sibling.Name", "Person 0 Sibling" },
                { "People[1].Sibling.Name", "Person 1 Sibling" },
                { "People[2].Sibling", "Person 2 Sibling" },
                { "People[1000].Name", "Person 1000 Sibling" },
            };
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post,
                            "http://localhost/ModelBinding/NestedCollectionOfRecursiveTypes")
            {
                Content = new FormUrlEncodedContent(bodyParameters)
            };

            // Act
            HttpResponseMessage response = await SubmitRequestAsync(request);

            // Assert
            PeopleModel result = await ReadAsJson<PeopleModel>(response);
            Assert.Equal(3, result.People.Count);
            Person person = result.People[0];

            Assert.Equal("Person 0", person.Name);
            Assert.Equal("Person 0 Sibling", person.Sibling.Name);
            Assert.Null(person.Sibling.Sibling);

            person = result.People[1];
            Assert.Equal("Person 1 Sibling", person.Sibling.Name);
            Assert.Null(person.Sibling.Sibling);

            person = result.People[2];
            Assert.Null(person.Name);
            Assert.NotNull(person.Sibling);
            Assert.Null(person.Sibling.Name);
        }

        [Fact]
        public async Task BindModel_WithNestedCollectionContainingRecursiveRelation_WithMalformedValue()
        {
            // Arrange
            Dictionary<string, string> bodyParameters = new Dictionary<string, string>
            {
                { "People", "Person 0" },
            };
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post,
                            "http://localhost/ModelBinding/NestedCollectionOfRecursiveTypes")
            {
                Content = new FormUrlEncodedContent(bodyParameters)
            };

            // Act
            HttpResponseMessage response = await SubmitRequestAsync(request);

            // Assert
            PeopleModel result = await ReadAsJson<PeopleModel>(response);
            Person person = Assert.Single(result.People);
            Assert.Null(person.Name);
            Assert.Null(person.Sibling);
        }

        private static async Task<HttpResponseMessage> SubmitRequestAsync(HttpRequestMessage request)
        {
            HttpConfiguration config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();
            HttpServer server = new HttpServer(config);
            using (HttpMessageInvoker client = new HttpMessageInvoker(server))
            {
                HttpResponseMessage response = await client.SendAsync(request, CancellationToken.None);
                response.EnsureSuccessStatusCode();

                return response;
            }
        }

        private static async Task<TVal> ReadAsJson<TVal>(HttpResponseMessage response)
        {
            Assert.Equal(MediaTypeHeaderValue.Parse("application/json; charset=utf-8"),
                         response.Content.Headers.ContentType);
            string content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<TVal>(content);
        }
    }

    [RoutePrefix("ModelBinding")]
    public class ModelBindingController : ApiController
    {
        [HttpGet]
        [Route("Url")]
        public string UrlBinding([FromUri] string someKey)
        {
            return someKey;
        }

        [HttpPost]
        [Route("SimpleType")]
        public Person SimpleType([FromBody] Person model)
        {
            return model;
        }

        [HttpPut]
        [Route("CollectionType")]
        public Address CollectionType([FromBody] Address address)
        {
            return address;
        }

        [HttpPost]
        [Route("NestedCollection")]
        public UserWithAddress NestedCollectionType([FromBody] UserWithAddress user)
        {
            return user;
        }

        [HttpPost]
        [Route("NestedCollectionOfRecursiveTypes")]
        public PeopleModel NestedCollectionType([FromBody] PeopleModel model)
        {
            return model;
        }
    }
}
