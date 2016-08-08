// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Globalization;
using System.Net.Http;
using System.Web.Http.Controllers;
using Microsoft.TestCommon;

namespace System.Web.Http.ValueProviders.Providers
{
    public class QueryStringValueProviderFactoryTest
    {
        private readonly QueryStringValueProviderFactory _factory = new QueryStringValueProviderFactory();

        [Fact]
        public void GetValueProvider_WhenActionContextParameterIsNull_Throws()
        {
            Assert.ThrowsArgumentNull(() => _factory.GetValueProvider(actionContext: null), "actionContext");
        }

        [Fact]
        public void GetValueProvider_ReturnsQueryStringValueProviderInstaceWithInvariantCulture()
        {
            var controllerContext = new HttpControllerContext() { Request = new HttpRequestMessage() };
            var context = new HttpActionContext() { ControllerContext = controllerContext };

            IValueProvider result = _factory.GetValueProvider(context);

            var valueProvider = Assert.IsType<QueryStringValueProvider>(result);
            Assert.Equal(CultureInfo.InvariantCulture, valueProvider.Culture);
        }
    }
}
