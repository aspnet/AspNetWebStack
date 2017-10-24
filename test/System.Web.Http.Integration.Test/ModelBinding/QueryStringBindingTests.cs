// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.TestCommon;

namespace System.Web.Http.ModelBinding
{
    /// <summary>
    /// End to end functional tests for model binding via query strings
    /// </summary>
    public class QueryStringBindingTests : ModelBindingTests
    {
        [Theory]
        [InlineData("GetString", "?value=test", "\"test\"")]
        [InlineData("GetInt", "?value=99", "99")]
        [InlineData("GetBool", "?value=false", "false")]
        [InlineData("GetBool", "?value=true", "true")]
        [InlineData("GetIntWithDefault", "?value=99", "99")]    // action has default, but we provide value
        [InlineData("GetIntWithDefault", "", "-1")]             // action has default, we provide no value
        [InlineData("GetStringWithDefault", "", "null")]        // action has null default, we provide no value
        [InlineData("GetIntFromUri", "?value=99", "99")]        // [FromUri]
        [InlineData("GetIntPrefixed", "?somePrefix=99", "99")]  // [FromUri(Prefix=somePrefix)]
        [InlineData("GetIntAsync", "?value=5", "5")]
        [InlineData("GetOptionalNullableInt", "", "null")]
        [InlineData("GetOptionalNullableInt", "?value=6", "6")]
        public async Task Query_String_Binds_Simple_Types_Get(string action, string queryString, string expectedResponse)
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage()
            {
                RequestUri = new Uri(BaseAddress + String.Format("ModelBinding/{0}{1}", action, queryString)),
                Method = HttpMethod.Get
            };

            // Act
            HttpResponseMessage response = await Client.SendAsync(request);

            // Assert
            string responseString = await response.Content.ReadAsStringAsync();
            Assert.Equal(expectedResponse, responseString);
        }

        [Theory]
        [InlineData("PostString", "?value=test", "\"test\"")]
        [InlineData("PostInt", "?value=99", "99")]
        [InlineData("PostBool", "?value=false", "false")]
        [InlineData("PostBool", "?value=true", "true")]
        [InlineData("PostIntFromUri", "?value=99", "99")]           // [FromUri]
        [InlineData("PostIntUriPrefixed", "?somePrefix=99", "99")]  // [FromUri(Prefix=somePrefix)]
        [InlineData("PostIntArray", "?value={[1,2,3]}", "0")]       // TODO: DevDiv2 333257 -- make this array real when fix JsonValue array model binding
        public async Task Query_String_Binds_Simple_Types_Post(string action, string queryString, string expectedResponse)
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage()
            {
                RequestUri = new Uri(BaseAddress + String.Format("ModelBinding/{0}{1}", action, queryString)),
                Method = HttpMethod.Post
            };

            // Act
            HttpResponseMessage response = await Client.SendAsync(request);

            // Assert
            string responseString = await response.Content.ReadAsStringAsync();
            Assert.Equal(expectedResponse, responseString);
        }

        [Theory]
        [InlineData("GetComplexTypeFromUri", "itemName=Tires&quantity=2&customer.Name=Sue", "Tires", 2, "Sue")]
        public async Task Query_String_ComplexType_Type_Get(string action, string queryString, string itemName, int quantity, string customerName)
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage()
            {
                RequestUri = new Uri(BaseAddress + String.Format("ModelBinding/{0}?{1}", action, queryString)),
                Method = HttpMethod.Get
            };

            ModelBindOrder expectedItem = new ModelBindOrder()
            {
                ItemName = itemName,
                Quantity = quantity,
                Customer = new ModelBindCustomer { Name = customerName }
            };

            // Act
            HttpResponseMessage response = await Client.SendAsync(request);

            // Assert
            ModelBindOrder actualItem = await response.Content.ReadAsAsync<ModelBindOrder>();
            Assert.Equal(expectedItem, actualItem, new ModelBindOrderEqualityComparer());
        }

        [Theory]
        [InlineData("PostComplexTypeFromUri", "itemName=Tires&quantity=2&customer.Name=Bob", "Tires", 2, "Bob")]
        public async Task Query_String_ComplexType_Type_Post(string action, string queryString, string itemName, int quantity, string customerName)
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage()
            {
                RequestUri = new Uri(BaseAddress + String.Format("ModelBinding/{0}?{1}", action, queryString)),
                Method = HttpMethod.Post
            };
            ModelBindOrder expectedItem = new ModelBindOrder()
            {
                ItemName = itemName,
                Quantity = quantity,
                Customer = new ModelBindCustomer { Name = customerName }
            };

            // Act
            HttpResponseMessage response = await Client.SendAsync(request);

            // Assert
            ModelBindOrder actualItem = await response.Content.ReadAsAsync<ModelBindOrder>();
            Assert.Equal(expectedItem, actualItem, new ModelBindOrderEqualityComparer());
        }

        [Theory]
        [InlineData("PostComplexTypeFromUriWithNestedCollection", "value.Numbers[0]=1&value.Numbers[1]=2", new[] { 1, 2 })]
        public async Task Query_String_ComplexType_Type_Post_NestedCollection(string action, string queryString, int[] expectedValues)
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage()
            {
                RequestUri = new Uri(BaseAddress + String.Format("ModelBinding/{0}?{1}", action, queryString)),
                Method = HttpMethod.Post
            };

            // Act
            HttpResponseMessage response = await Client.SendAsync(request);

            // Assert
            ComplexTypeWithNestedCollection actualResult = await response.Content.ReadAsAsync<ComplexTypeWithNestedCollection>();
            int[] actualValues = actualResult.Numbers.ToArray();
            Assert.Equal(expectedValues.Length, actualValues.Length);
            for (int i = 0; i < expectedValues.Length; i++)
            {
                Assert.Equal(expectedValues[i], actualValues[i]);
            }
        }
    }
}