// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using Microsoft.TestCommon;

namespace System.Web.Http.Controllers
{
    public class VoidResultConverterTest
    {
        private readonly VoidResultConverter _converter = new VoidResultConverter();
        private readonly HttpControllerContext _context = new HttpControllerContext();
        private readonly HttpRequestMessage _request = new HttpRequestMessage();

        public VoidResultConverterTest()
        {
            _context.Request = _request;
            _context.Configuration = new HttpConfiguration();
        }

        [Fact]
        public void Convert_WhenContextIsNull_Throws()
        {
            Assert.ThrowsArgumentNull(() => _converter.Convert(controllerContext: null, actionResult: null), "controllerContext");
        }

        [Fact]
        public void Convert_ReturnsResponseMessageWithRequestAssignedAndNoContentToReflectVoid()
        {
            var result = _converter.Convert(_context, null);

            Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
            Assert.NotNull(result.Content);
            Assert.Equal(0L, result.Content.Headers.ContentLength);
            Assert.Same(_request, result.RequestMessage);
        }
    }
}
