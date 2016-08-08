// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using Microsoft.TestCommon;

namespace System.Web.Http.Controllers
{
    public class ResponseMessageResultConverterTest
    {
        private readonly ResponseMessageResultConverter _converter = new ResponseMessageResultConverter();
        private readonly HttpControllerContext _context = new HttpControllerContext();
        private readonly HttpRequestMessage _request = new HttpRequestMessage();

        public ResponseMessageResultConverterTest()
        {
            _context.Request = _request;
            _context.Configuration = new HttpConfiguration();
        }

        [Fact]
        public void Convert_WhenValueIsResponseMessage_ReturnsResponseMessageWithRequestAssigned()
        {
            HttpResponseMessage response = new HttpResponseMessage();

            var result = _converter.Convert(_context, response);

            Assert.Same(response, result);
            Assert.Same(_request, result.RequestMessage);
        }

        [Fact]
        public void Convert_WhenContextIsNull_Throws()
        {
            Assert.ThrowsArgumentNull(() => _converter.Convert(controllerContext: null, actionResult: new HttpResponseMessage()), "controllerContext");
        }

        [Fact]
        public void Convert_WhenValueIsNull_Throws()
        {
            Assert.Throws<InvalidOperationException>(() => _converter.Convert(_context, null),
                "A null value was returned where an instance of HttpResponseMessage was expected.");
        }

        [Fact]
        public void Convert_WhenValueIsIncompatibleType_Throws()
        {
            Assert.Throws<InvalidCastException>(() => _converter.Convert(_context, "42"),
                "Unable to cast object of type 'System.String' to type 'System.Net.Http.HttpResponseMessage'.");
        }
    }
}
