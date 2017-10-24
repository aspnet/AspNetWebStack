// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using System.Web.Http.Controllers;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Description
{
    public class ApiParameterDescriptionTest
    {
        [Fact]
        public void GetBindableProperties_WorksOnlyFor_PublicInstanceProperties_WithPublicGettersAndSetters()
        {
            // Arrange
            ApiParameterDescription parameter = new ApiParameterDescription();
            Mock<HttpParameterDescriptor> parameterDescriptorMock = new Mock<HttpParameterDescriptor>();
            parameterDescriptorMock.SetupGet(p => p.ParameterType).Returns(typeof(ClassWithAllKindsOfProperties));
            parameter.ParameterDescriptor = parameterDescriptorMock.Object;

            // Act
            IEnumerable<PropertyInfo> bindableProperties = parameter.GetBindableProperties();

            // Assert
            PropertyInfo bindableProperty = Assert.Single(bindableProperties);
            Assert.Equal("ValidProperty", bindableProperty.Name);
        }
    }

    internal class ClassWithAllKindsOfProperties
    {
        public int ValidProperty { get; set; }
        public static string InvalidProperty { get; set; }
        public int PropertyWithPrivateSetter { get; private set; }
        public int PropertyWithPrivateGetter { private get; set; }
        internal int InternalProperty { get; set; }
        protected int ProtectedProperty { get; private set; }
        private int PrivateProperty { get; set; }
        public int PropertyWithoutSetter { get { return 0; } }
        public int PropertyWithoutGetter { set { return; } }
    }
}
