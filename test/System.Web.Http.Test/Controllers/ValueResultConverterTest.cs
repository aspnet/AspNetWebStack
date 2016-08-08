// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using Microsoft.TestCommon;

namespace System.Web.Http.Controllers
{
    public class ValueResultConverterTest
    {
        private readonly ValueResultConverter<object> _objectValueConverter = new ValueResultConverter<object>();
        private readonly ValueResultConverter<Animal> _animalValueConverter = new ValueResultConverter<Animal>();
        private readonly HttpControllerContext _context = new HttpControllerContext();
        private readonly HttpRequestMessage _request = new HttpRequestMessage();

        public ValueResultConverterTest()
        {
            _context.Request = _request;
            _context.Configuration = new HttpConfiguration();
        }

        [Fact]
        public void Convert_WhenContextIsNull_Throws()
        {
            Assert.ThrowsArgumentNull(() => _objectValueConverter.Convert(controllerContext: null, actionResult: new object()), "controllerContext");
        }

        [Fact]
        public void Convert_WhenValueTypeIsNotCompatible_Throws()
        {
            Assert.Throws<InvalidCastException>(() => _animalValueConverter.Convert(_context, new object()),
                "Unable to cast object of type 'System.Object' to type 'Animal'.");
        }

        [Fact]
        public void Convert_WhenValueIsResponseMessage_ReturnsResponseMessageWithRequestAssigned()
        {
            HttpResponseMessage response = new HttpResponseMessage();

            var result = _objectValueConverter.Convert(_context, response);

            Assert.Same(response, result);
            Assert.Same(_request, result.RequestMessage);
        }

        [Fact]
        public void Convert_WhenValueIsAnyType_CreatesContentNegotiatedResponse()
        {
            Dog dog = new Dog();
            XmlMediaTypeFormatter formatter = new XmlMediaTypeFormatter();
            _context.Configuration.Formatters.Clear();
            _context.Configuration.Formatters.Add(formatter);

            var result = _animalValueConverter.Convert(_context, dog);

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            var content = Assert.IsType<ObjectContent<Animal>>(result.Content);
            Assert.Same(dog, content.Value);
            Assert.Same(formatter, content.Formatter);
            Assert.Same(_request, result.RequestMessage);
        }

        public class Animal { }
        public class Dog : Animal { }
    }
}
