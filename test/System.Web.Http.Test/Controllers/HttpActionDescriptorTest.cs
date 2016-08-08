// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using Microsoft.TestCommon;

namespace System.Web.Http.Controllers
{
    public class HttpActionDescriptorTest
    {
        [Theory]
        [InlineData(null, typeof(VoidResultConverter))]
        [InlineData(typeof(HttpResponseMessage), typeof(ResponseMessageResultConverter))]
        [InlineData(typeof(object), typeof(ValueResultConverter<object>))]
        [InlineData(typeof(string), typeof(ValueResultConverter<string>))]
        public void GetResultConverter_GetAppropriateConverterInstance(Type actionReturnType, Type expectedConverterType)
        {
            var result = HttpActionDescriptor.GetResultConverter(actionReturnType);

            Assert.IsType(expectedConverterType, result);
        }

        [Fact]
        public void GetResultConverter_WhenReturnTypeIsActionResult_ReturnsNull()
        {
            // Act
            IActionResultConverter converter = HttpActionDescriptor.GetResultConverter(typeof(IHttpActionResult));

            // Assert
            Assert.Null(converter);
        }

        [Fact]
        public void GetResultConverter_WhenTypeIsAnGenericParameterType_Throws()
        {
            var genericType = typeof(HttpActionDescriptorTest).GetMethod("SampleGenericMethod").ReturnType;

            Assert.Throws<InvalidOperationException>(() => HttpActionDescriptor.GetResultConverter(genericType),
                "No action result converter could be constructed for a generic parameter type 'TResult'.");
        }

        public TResult SampleGenericMethod<TResult>()
        {
            return default(TResult);
        }
    }
}
