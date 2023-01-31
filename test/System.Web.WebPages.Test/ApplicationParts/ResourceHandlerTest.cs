// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Text;
using System.Web.WebPages.ApplicationParts;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.WebPages.Test
{
    public class ResourceHandlerTest
    {
        private const string _fileContent = "contents of jpeg file";

        [Fact]
        public void ResourceHandlerWritesContentsOfFileToStream()
        {
            // Arrange
            var applicationPart = new ApplicationPart(BuildAssembly(), "~/my-app-assembly");
            MemoryStream stream = new MemoryStream();
            var response = new Mock<HttpResponseBase>();
            response.SetupGet(c => c.OutputStream).Returns(stream);
            response.SetupSet(c => c.ContentType = "image/jpeg").Verifiable();
            var resourceHandler = new ResourceHandler(applicationPart, "bar.foo.jpg");

            // Act
            resourceHandler.ProcessRequest(response.Object);

            // Assert
            response.Verify();
            Assert.Equal(_fileContent, Encoding.Default.GetString(stream.ToArray()));
        }

        [Fact]
        public void ResourceHandlerThrows404IfResourceNotFound()
        {
            // Arrange
            var applicationPart = new ApplicationPart(BuildAssembly(), "~/my-app-assembly");
            MemoryStream stream = new MemoryStream();
            var response = new Mock<HttpResponseBase>();
            response.SetupGet(c => c.OutputStream).Returns(stream);
            response.SetupSet(c => c.ContentType = "image/jpeg").Verifiable();
            var resourceHandler = new ResourceHandler(applicationPart, "does-not-exist");

            // Act and Assert
            Assert.Throws<HttpException>(() => resourceHandler.ProcessRequest(response.Object),
                                                  "The resource file \"does-not-exist\" could not be found.");
        }

        private static IResourceAssembly BuildAssembly(string name = "my-assembly")
        {
            Mock<TestResourceAssembly> assembly = new Mock<TestResourceAssembly>();
            assembly.SetupGet(c => c.Name).Returns("my-assembly");

            byte[] content = Encoding.Default.GetBytes(_fileContent);
            assembly.Setup(c => c.GetManifestResourceStream("my-assembly.bar.foo.jpg")).Returns(new MemoryStream(content));

            assembly.Setup(c => c.GetManifestResourceNames()).Returns(new[] { "my-assembly.bar.foo.jpg" });

            return assembly.Object;
        }
    }
}
