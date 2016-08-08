// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Internal;
using System.Web.Http.ValueProviders;
using System.Web.Http.ValueProviders.Providers;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.ModelBinding
{
    public class HttpParameterBindingExtensionsTest
    {
        [Fact]
        public void WillReadUri_Throws_With_Null_ParameterBinding()
        {
            // Arrange
            HttpParameterBinding binding = null;

            // Act & Assert
            Assert.ThrowsArgumentNull(() => binding.WillReadUri(), "parameterBinding");
        }

        [Fact]
        public void WillReadUri_Returns_True_For_IValueProviderParameterBinding_Containing_Only_Standard_Uri_ValueProviders()
        {
            // Arrange
            Mock<HttpParameterDescriptor> descriptorMock = new Mock<HttpParameterDescriptor>();
            HttpParameterBinding bindingMock = new HttpValueProviderParameterBindingTestDouble(
                                                    descriptorMock.Object,
                                                    new List<ValueProviderFactory>() 
                                                    {
                                                        new QueryStringValueProviderFactory(), 
                                                        new RouteDataValueProviderFactory()
                                                    });

            // Act
            bool result = bindingMock.WillReadUri();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void WillReadUri_Returns_False_For_IValueProviderParameterBinding_Containing_Non_Uri_ValueProviders()
        {
            // Arrange
            Mock<HttpParameterDescriptor> descriptorMock = new Mock<HttpParameterDescriptor>();
            Mock<ValueProviderFactory> valueProviderMock = new Mock<ValueProviderFactory>();
            HttpParameterBinding bindingMock = new HttpValueProviderParameterBindingTestDouble(
                                                    descriptorMock.Object,
                                                    new List<ValueProviderFactory>() 
                                                    {
                                                        new QueryStringValueProviderFactory(), 
                                                        new RouteDataValueProviderFactory(),
                                                        valueProviderMock.Object,
                                                    });

            // Act
            bool result = bindingMock.WillReadUri();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void WillReadUri_Returns_False_For_IValueProviderParameterBinding_Containing_No_ValueProviders()
        {
            // Arrange
            Mock<HttpParameterDescriptor> descriptorMock = new Mock<HttpParameterDescriptor>();
            HttpParameterBinding bindingMock = new HttpValueProviderParameterBindingTestDouble(
                                                    descriptorMock.Object,
                                                    new List<ValueProviderFactory>());

            // Act
            bool result = bindingMock.WillReadUri();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void WillReadUri_Returns_False_For_HttpParameterBinding_Not_Implementing_IValueProviderParameterBinding()
        {
            // Arrange
            Mock<HttpParameterDescriptor> descriptorMock = new Mock<HttpParameterDescriptor>();
            Mock<HttpParameterBinding> bindingMock = new Mock<HttpParameterBinding>(descriptorMock.Object);

            // Act
            bool result = bindingMock.Object.WillReadUri();

            // Assert
            Assert.False(result);
        }

        class HttpValueProviderParameterBindingTestDouble : HttpParameterBinding, IValueProviderParameterBinding
        {
            public IEnumerable<ValueProviderFactory> Factories { get; set; }

            public HttpValueProviderParameterBindingTestDouble(HttpParameterDescriptor descriptor, IEnumerable<ValueProviderFactory> factories) : base(descriptor)
            {
                Factories = factories;
            }

            public override Task ExecuteBindingAsync(Metadata.ModelMetadataProvider metadataProvider, HttpActionContext actionContext, Threading.CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<ValueProviderFactory> ValueProviderFactories
            {
                get { return Factories; }
            }
        }
    }
}
