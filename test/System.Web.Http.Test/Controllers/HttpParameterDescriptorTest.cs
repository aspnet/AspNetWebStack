// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Controllers;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http
{
    public class HttpParameterDescriptorTest
    {
        [Fact]
        public void Default_Constructor()
        {
            HttpParameterDescriptor parameterDescriptor = new Mock<HttpParameterDescriptor>().Object;

            Assert.Null(parameterDescriptor.ParameterName);
            Assert.Null(parameterDescriptor.ParameterType);
            Assert.Null(parameterDescriptor.Configuration);
            Assert.Null(parameterDescriptor.Prefix);
            Assert.Null(parameterDescriptor.ParameterBinderAttribute);
            Assert.Null(parameterDescriptor.ActionDescriptor);
            Assert.Null(parameterDescriptor.DefaultValue);
            Assert.NotNull(parameterDescriptor.Properties);
            Assert.False(parameterDescriptor.IsOptional);
        }

        [Fact]
        public void Configuration_Property()
        {
            HttpConfiguration config = new HttpConfiguration();
            HttpParameterDescriptor parameterDescriptor = new Mock<HttpParameterDescriptor> { CallBase = true }.Object;

            Assert.Reflection.Property<HttpParameterDescriptor, HttpConfiguration>(
                 instance: parameterDescriptor,
                 propertyGetter: pd => pd.Configuration,
                 expectedDefaultValue: null,
                 allowNull: false,
                 roundTripTestValue: config);
        }

        [Fact]
        public void ActionDescriptor_Property()
        {
            HttpParameterDescriptor parameterDescriptor = new Mock<HttpParameterDescriptor> { CallBase = true }.Object;
            HttpActionDescriptor actionDescriptor = new Mock<HttpActionDescriptor>().Object;

            Assert.Reflection.Property<HttpParameterDescriptor, HttpActionDescriptor>(
                 instance: parameterDescriptor,
                 propertyGetter: pd => pd.ActionDescriptor,
                 expectedDefaultValue: null,
                 allowNull: false,
                 roundTripTestValue: actionDescriptor);
        }

        [Fact]
        public void GetCustomAttributes_Returns_EmptyAttributes()
        {
            HttpParameterDescriptor parameterDescriptor = new Mock<HttpParameterDescriptor> { CallBase = true }.Object;
            IEnumerable<object> attributes = parameterDescriptor.GetCustomAttributes<object>();

            Assert.Empty(attributes);
        }

        [Fact]
        public void GetCustomAttributes_AttributeType_Returns_EmptyAttributes()
        {
            HttpParameterDescriptor parameterDescriptor = new Mock<HttpParameterDescriptor> { CallBase = true }.Object;
            IEnumerable<FromBodyAttribute> attributes = parameterDescriptor.GetCustomAttributes<FromBodyAttribute>();

            Assert.Empty(attributes);
        }
    }
}
