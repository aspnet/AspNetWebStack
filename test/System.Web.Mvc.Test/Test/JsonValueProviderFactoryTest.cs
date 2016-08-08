// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Globalization;
using System.IO;
using System.Text;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class JsonValueProviderFactoryTest
    {
        [Fact]
        public void GetValueProvider_NullControllerContext_ThrowsException()
        {
            JsonValueProviderFactory factory = new JsonValueProviderFactory();

            Assert.ThrowsArgumentNull(delegate() { factory.GetValueProvider(controllerContext: null); }, "controllerContext");
        }

        [Fact]
        public void GetValueProvider_SimpleArrayJsonObject()
        {
            const string jsonString = @"
[ ""abc"", null, ""foobar"" ]
";
            ControllerContext cc = GetJsonEnabledControllerContext(jsonString);
            JsonValueProviderFactory factory = new JsonValueProviderFactory();

            // Act & assert
            IValueProvider valueProvider = factory.GetValueProvider(cc);
            Assert.True(valueProvider.ContainsPrefix("[0]"));
            Assert.True(valueProvider.ContainsPrefix("[2]"));
            Assert.False(valueProvider.ContainsPrefix("[3]"));

            ValueProviderResult vpResult1 = valueProvider.GetValue("[0]");
            Assert.Equal("abc", vpResult1.AttemptedValue);
            Assert.Equal(CultureInfo.CurrentCulture, vpResult1.Culture);

            // null values should exist in the backing store as actual entries
            ValueProviderResult vpResult2 = valueProvider.GetValue("[1]");
            Assert.NotNull(vpResult2);
            Assert.Null(vpResult2.RawValue);
        }

        [Fact]
        public void GetValueProvider_SimpleDictionaryJsonObject()
        {
            const string jsonString = @"
{   ""FirstName"":""John"",
    ""LastName"": ""Doe""
}";

            ControllerContext cc = GetJsonEnabledControllerContext(jsonString);
            JsonValueProviderFactory factory = new JsonValueProviderFactory();

            // Act & assert
            IValueProvider valueProvider = factory.GetValueProvider(cc);
            Assert.True(valueProvider.ContainsPrefix("firstname"));

            ValueProviderResult vpResult1 = valueProvider.GetValue("firstname");
            Assert.Equal("John", vpResult1.AttemptedValue);
            Assert.Equal(CultureInfo.CurrentCulture, vpResult1.Culture);
        }

        [Fact]
        public void GetValueProvider_ComplexJsonObject()
        {
            // Arrange
            const string jsonString = @"
[
  { 
    ""BillingAddress"": {
      ""Street"": ""1 Microsoft Way"",
      ""City"": ""Redmond"",
      ""State"": ""WA"",
      ""ZIP"": 98052 },
    ""ShippingAddress"": { 
      ""Street"": ""123 Anywhere Ln"",
      ""City"": ""Anytown"",
      ""State"": ""ZZ"",
      ""ZIP"": 99999 }
  },
  { 
    ""Enchiladas"": [ ""Delicious"", ""Nutritious""]
  }
]
";

            ControllerContext cc = GetJsonEnabledControllerContext(jsonString);
            JsonValueProviderFactory factory = new JsonValueProviderFactory();

            // Act & assert
            IValueProvider valueProvider = factory.GetValueProvider(cc);
            Assert.NotNull(valueProvider);

            Assert.True(valueProvider.ContainsPrefix("[0].billingaddress"));
            Assert.Null(valueProvider.GetValue("[0].billingaddress"));

            Assert.True(valueProvider.ContainsPrefix("[0].billingaddress.street"));
            Assert.NotNull(valueProvider.GetValue("[0].billingaddress.street"));

            ValueProviderResult vpResult1 = valueProvider.GetValue("[1].enchiladas[0]");
            Assert.NotNull(vpResult1);
            Assert.Equal("Delicious", vpResult1.AttemptedValue);
            Assert.Equal(CultureInfo.CurrentCulture, vpResult1.Culture);
        }

        [Fact]
        public void GetValueProvider_NoJsonBody_ReturnsNull()
        {
            // Arrange
            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.Setup(o => o.HttpContext.Request.ContentType).Returns("application/json");
            mockControllerContext.Setup(o => o.HttpContext.Request.InputStream).Returns(new MemoryStream());

            JsonValueProviderFactory factory = new JsonValueProviderFactory();

            // Act
            IValueProvider valueProvider = factory.GetValueProvider(mockControllerContext.Object);

            // Assert
            Assert.Null(valueProvider);
        }

        [Fact]
        public void GetValueProvider_NotJsonRequest_ReturnsNull()
        {
            // Arrange
            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.Setup(o => o.HttpContext.Request.ContentType).Returns("not JSON");

            JsonValueProviderFactory factory = new JsonValueProviderFactory();

            // Act
            IValueProvider valueProvider = factory.GetValueProvider(mockControllerContext.Object);

            // Assert
            Assert.Null(valueProvider);
        }

        private static ControllerContext GetJsonEnabledControllerContext(string jsonString)
        {
            byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonString);
            MemoryStream jsonStream = new MemoryStream(jsonBytes);

            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.Setup(o => o.HttpContext.Request.ContentType).Returns("application/json");
            mockControllerContext.Setup(o => o.HttpContext.Request.InputStream).Returns(jsonStream);
            return mockControllerContext.Object;
        }
    }
}
