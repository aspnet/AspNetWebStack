// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web.Http.Controllers;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Filters
{
    public class ActionDescriptorFilterProviderTest
    {
        private readonly ActionDescriptorFilterProvider _provider = new ActionDescriptorFilterProvider();
        private static readonly HttpConfiguration _configuration = new HttpConfiguration();

        [Fact]
        public void GetFilters_IfConfigurationParameterIsNull_ThrowsException()
        {
            Assert.ThrowsArgumentNull(() =>
            {
                _provider.GetFilters(configuration: null, actionDescriptor: new Mock<HttpActionDescriptor>().Object);
            }, "configuration");
        }

        [Fact]
        public void GetFilters_IfActionDescriptorParameterIsNull_ThrowsException()
        {
            Assert.ThrowsArgumentNull(() =>
            {
                _provider.GetFilters(_configuration, actionDescriptor: null);
            }, "actionDescriptor");
        }

        [Fact]
        public void GetFilters_GetsFilterObjectsFromActionDescriptorAndItsControllerDescriptor()
        {
            // Arrange
            Mock<HttpActionDescriptor> adMock = new Mock<HttpActionDescriptor>();
            IFilter filter1 = new Mock<IFilter>().Object;
            IFilter filter2 = new Mock<IFilter>().Object;
            IFilter filter3 = new Mock<IFilter>().Object;
            adMock.Setup(ad => ad.GetFilters()).Returns(new Collection<IFilter>(new[] { filter1, filter2 })).Verifiable();

            Mock<HttpControllerDescriptor> cdMock = new Mock<HttpControllerDescriptor>();
            cdMock.Setup(cd => cd.GetFilters()).Returns(new Collection<IFilter>(new[] { filter3 })).Verifiable();

            HttpActionDescriptor actionDescriptor = adMock.Object;
            actionDescriptor.ControllerDescriptor = cdMock.Object;

            // Act
            var result = _provider.GetFilters(_configuration, actionDescriptor).ToList();

            // Assert
            adMock.Verify();
            cdMock.Verify();
            Assert.Equal(3, result.Count);
            Assert.Equal(new FilterInfo(filter3, FilterScope.Controller), result[0], new TestFilterInfoComparer());
            Assert.Equal(new FilterInfo(filter1, FilterScope.Action), result[1], new TestFilterInfoComparer());
            Assert.Equal(new FilterInfo(filter2, FilterScope.Action), result[2], new TestFilterInfoComparer());
        }

        public class TestFilterInfoComparer : IEqualityComparer<FilterInfo>
        {
            public bool Equals(FilterInfo x, FilterInfo y)
            {
                return (x == null && y == null) || (Object.ReferenceEquals(x.Instance, y.Instance) && x.Scope == y.Scope);
            }

            public int GetHashCode(FilterInfo obj)
            {
                return obj.GetHashCode();
            }
        }

    }
}
