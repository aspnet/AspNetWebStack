// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata;
using System.Web.Http.Validation;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.ModelBinding
{
    public class FormatterParameterBindingTest
    {
        [Fact]
        public void ReadContentAsync_Throws_ForNoContentType()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost");
            request.Content = new StringContent("The quick, brown fox tripped and fell.");
            request.Content.Headers.ContentType = null;
            var formatters = new MediaTypeFormatterCollection();
            var descriptor = new Mock<HttpParameterDescriptor>();
            descriptor.Setup(desc => desc.IsOptional).Returns(false);
            var binding = new FormatterParameterBinding(descriptor.Object, formatters, null);

            HttpResponseException exception = Assert.Throws<HttpResponseException>(
                () => binding.ReadContentAsync(request, typeof(string), formatters, null));

            Assert.Equal(HttpStatusCode.UnsupportedMediaType, exception.Response.StatusCode);
            HttpError error;
            exception.Response.TryGetContentValue(out error);
            Assert.Equal(
                "The request contains an entity body but no Content-Type header. The inferred media type 'application/octet-stream' is not supported for this resource.",
                error.Message);
        }

        [Fact]
        public void ReadContentAsync_Throws_ForUnsupportedMediaType()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost");
            request.Content = new StringContent("The quick, brown fox tripped and fell.");
            var formatters = new MediaTypeFormatterCollection();
            var descriptor = new Mock<HttpParameterDescriptor>();
            descriptor.Setup(desc => desc.IsOptional).Returns(false);
            var binding = new FormatterParameterBinding(descriptor.Object, formatters, null);

            HttpResponseException exception = Assert.Throws<HttpResponseException>(
                () => binding.ReadContentAsync(request, typeof(string), formatters, null));

            Assert.Equal(HttpStatusCode.UnsupportedMediaType, exception.Response.StatusCode);
            HttpError error;
            exception.Response.TryGetContentValue(out error);
            Assert.Equal(
                "The request entity's media type 'text/plain' is not supported for this resource.",
                error.Message);
        }

        [Fact]
        public async Task ExecuteBindingAsync_PassesCancellationTokenTo_ReadContentAsync()
        {
            // Arrange
            var parameter = new Mock<HttpParameterDescriptor>();
            parameter.Setup(p => p.IsOptional).Returns(false);
            parameter.Setup(p => p.ParameterName).Returns("ParameterName");
            var formatters = Enumerable.Empty<MediaTypeFormatter>();
            IBodyModelValidator validator = null;
            ModelMetadataProvider metadataProvider = new Mock<ModelMetadataProvider>().Object;
            HttpRequestMessage request = new HttpRequestMessage();
            HttpActionContext actionContext = new HttpActionContext { ControllerContext = new HttpControllerContext { Request = request } };
            CancellationTokenSource cts = new CancellationTokenSource();

            Mock<FormatterParameterBinding> binding = new Mock<FormatterParameterBinding>(parameter.Object, formatters, validator);
            binding.CallBase = true;
            binding.Setup(b => b.ReadContentAsync(request, null, formatters, It.IsAny<IFormatterLogger>()))
                .Returns(Task.FromResult<object>(42))
                .Verifiable();

            // Act
            await binding.Object.ExecuteBindingAsync(metadataProvider, actionContext, cts.Token);

            // Assert
            binding.Verify();
        }

        [Fact]
        public async Task ReadContentAsync_PassesCancellationToken_Further()
        {
            // Arrange
            var parameter = new Mock<HttpParameterDescriptor>();
            parameter.Setup(p => p.IsOptional).Returns(false);
            IBodyModelValidator validator = null;
            HttpRequestMessage request = new HttpRequestMessage();
            request.Content = new StringContent("");
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("app/test");
            IFormatterLogger logger = null;
            CancellationTokenSource cts = new CancellationTokenSource();

            Mock<MediaTypeFormatter> formatter = new Mock<MediaTypeFormatter>();
            formatter.Setup(f => f.CanReadType(typeof(int))).Returns(true);
            formatter.Object.SupportedMediaTypes.Add(request.Content.Headers.ContentType);
            formatter.Setup(f => f.ReadFromStreamAsync(typeof(int), It.IsAny<Stream>(), request.Content, logger, cts.Token))
                .Returns(Task.FromResult<object>(42))
                .Verifiable();

            var formatters = new[] { formatter.Object };
            FormatterParameterBinding binding = new FormatterParameterBinding(parameter.Object, formatters, validator);

            // Act
            await binding.ReadContentAsync(request, typeof(int), formatters, logger, cts.Token);

            // Assert
            formatter.Verify();
        }
    }
}
