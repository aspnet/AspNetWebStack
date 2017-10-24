// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.ValueProviders;
using Microsoft.TestCommon;

namespace System.Web.Http.ModelBinding
{
    public class ModelBindingController : ApiController
    {
        public string GetString(string value)
        {
            return value;
        }

        public string GetStringFromRoute(string controller = null, string action = null)
        {
            return controller + ":" + action;
        }

        public int GetInt(int value)
        {
            return value;
        }

        public int? GetOptionalNullableInt(int? value = null)
        {
            return value;
        }

        public int GetIntWithDefault(int value = -1)
        {
            return value;
        }

        public string GetStringWithDefault(string value = null)
        {
            return value;
        }

        public int GetIntFromUri([FromUri] int value)
        {
            return value;
        }

        public int GetIntPrefixed([FromUri(Name = "somePrefix")] int value)
        {
            return value;
        }

        public int GetIntCustom([ValueProvider(typeof(RequestHeadersValueProviderFactory))] int value)
        {
            return value;
        }

        public Task<int> GetIntAsync(int value, CancellationToken token)
        {
            TaskCompletionSource<int> tcs = new TaskCompletionSource<int>();
            tcs.TrySetResult(value);
            return tcs.Task;
        }

        public bool GetBool(bool value)
        {
            return value;
        }

        public ModelBindOrder GetComplexType(ModelBindOrder item)
        {
            return item;
        }

        public ModelBindOrder GetComplexTypeFromUri([FromUri] ModelBindOrder item)
        {
            return item;
        }

        public string PostString(string value)
        {
            return value;
        }

        public int PostInt(int value)
        {
            return value;
        }

        public HttpResponseMessage PostComplexWithValidation(CustomerNameMax6 customer)
        {
            string errors = String.Empty;
            foreach (var kv in this.ModelState)
            {
                int errorCount = kv.Value.Errors.Count;

                if (errorCount > 0)
                {
                    errors += String.Format("Failed to bind {0}. The errors are:\n", kv.Key);
                    for (int i = 0; i < errorCount; i++)
                    {
                        ModelError error = kv.Value.Errors[i];
                        errors += "ErrorMessage: " + error.ErrorMessage + "\n";

                        if (error.Exception != null)
                        {
                            errors += "Exception" + error.Exception + "\n";
                        }
                    }
                }
            }

            if (errors != String.Empty)
            {
                // Has validation failure
                // TODO, 334736, support HttpResponseException which takes ModelState
                // throw new HttpResponseException(this.ModelState);
                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = new StringContent(errors);
                throw new HttpResponseException(response);
            }
            else
            {
                // happy path
                return Request.CreateResponse<int>(HttpStatusCode.OK, customer.Id);
            }
        }

        public int PostIntFromUri([FromUri] int value)
        {
            return value;
        }

        public int PostIntFromBody([FromBody] int value)
        {
            return value;
        }

        public int PostIntUriPrefixed([FromUri(Name = "somePrefix")] int value)
        {
            return value;
        }

        public bool PostBool(bool value)
        {
            return value;
        }

        public int PostIntArray([FromUri] int[] value)
        {
            return value.Sum();
        }

        public ModelBindOrder PostComplexType(ModelBindOrder item)
        {
            return item;
        }

        public ModelBindOrder PostComplexTypeFromUri([FromUri] ModelBindOrder item)
        {
            return item;
        }

        public ComplexTypeWithNestedCollection PostComplexTypeFromUriWithNestedCollection([FromUri] ComplexTypeWithNestedCollection value)
        {
            return value;
        }

        public ModelBindOrder PostComplexTypeFromBody([FromBody] ModelBindOrder item)
        {
            return item;
        }

        // check if HttpRequestMessage prevents binding other parameters
        public int PostComplexTypeHttpRequestMessage(HttpRequestMessage request, ModelBindOrder order)
        {
            return Int32.Parse(order.ItemName) + order.Quantity;
        }

        public string PostWithOptionalBodyParameter([FromBody] string value = "default")
        {
            return value;
        }

        public string PostWithOptionalBodyParameterAndUriParameter([FromUri]int id, [FromBody] string value = "default")
        {
            return value;
        }
    }

    [DataContract]
    public class CustomerNameMax6
    {
        [Required]
        [StringLength(6)]
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int Id { get; set; }

        [Required]
        [DataMember(IsRequired = true)]
        public int RequiredValue { get; set; }
    }

    public class ModelBindCustomer
    {
        public string Name { get; set; }
    }

    public class ModelBindOrder
    {
        public string ItemName { get; set; }
        public int Quantity { get; set; }
        public ModelBindCustomer Customer { get; set; }
    }

    public class ComplexTypeWithNestedCollection
    {
        public IEnumerable<int> Numbers { get; set; }
    }

    public class ModelBindOrderEqualityComparer : IEqualityComparer<ModelBindOrder>
    {
        public bool Equals(ModelBindOrder x, ModelBindOrder y)
        {
            Assert.True(x != null, "Expected ModelBindOrder cannot be null.");
            Assert.True(y != null, "Actual ModelBindOrder was null.");
            Assert.Equal(x.ItemName, y.ItemName);
            Assert.Equal(x.Quantity, y.Quantity);

            if (x.Customer != null)
            {
                Assert.True(y.Customer != null, "Actual Customer was null but expected was " + x.Customer.Name);
            }
            else if (x.Customer == null)
            {
                Assert.True(y.Customer == null, "Actual Customer was not null but should have been.");
            }
            else
            {
                Assert.True(String.Equals(x.Customer.Name, y.Customer.Name, StringComparison.Ordinal), String.Format("Expected Customer.Name '{0}' but actual was '{1}'", x.Customer.Name, y.Customer.Name));
            }

            return true;
        }

        public int GetHashCode(ModelBindOrder obj)
        {
            return obj.GetHashCode();
        }
    }

    public class RequestHeadersValueProviderFactory : ValueProviderFactory
    {
        public override IValueProvider GetValueProvider(HttpActionContext actionContext)
        {
            return new RequestHeaderValueProvider(actionContext);
        }
    }

    public class RequestHeaderValueProvider : IValueProvider
    {
        HttpActionContext _actionContext;
        public RequestHeaderValueProvider(HttpActionContext actionContext)
        {
            _actionContext = actionContext;
        }

        public bool ContainsPrefix(string prefix)
        {
            return _actionContext.ControllerContext.Request.Headers.Contains(prefix);
        }

        public ValueProviderResult GetValue(string key)
        {
            string result = _actionContext.ControllerContext.Request.Headers.GetValues(key).FirstOrDefault();
            return result == null
                ? null
                : new ValueProviderResult(result, result, CultureInfo.CurrentCulture);
        }
    }
}