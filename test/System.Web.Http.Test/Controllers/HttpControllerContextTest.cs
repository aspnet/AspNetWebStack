// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http
{
    public class HttpControllerContextTest
    {
        [Fact]
        public void Default_Constructor()
        {
            HttpControllerContext controllerContext = new HttpControllerContext();

            Assert.Null(controllerContext.Configuration);
            Assert.Null(controllerContext.Controller);
            Assert.Null(controllerContext.ControllerDescriptor);
            Assert.Null(controllerContext.Request);
            Assert.Null(controllerContext.RouteData);
        }

        [Fact]
        public void Parameter_Constructor()
        {
            HttpConfiguration config = new HttpConfiguration();
            IHttpRouteData routeData = new Mock<IHttpRouteData>().Object;
            HttpRequestMessage request = new HttpRequestMessage();
            HttpControllerContext controllerContext = new HttpControllerContext(config, routeData, request);

            Assert.Same(config, controllerContext.Configuration);
            Assert.Same(request, controllerContext.Request);
            Assert.Same(routeData, controllerContext.RouteData);
            Assert.Null(controllerContext.Controller);
            Assert.Null(controllerContext.ControllerDescriptor);
        }

        [Fact]
        public void Constructor_Throws_IfConfigurationIsNull()
        {
            Assert.ThrowsArgumentNull(
                () => new HttpControllerContext(null, new Mock<IHttpRouteData>().Object, new HttpRequestMessage()),
                "configuration");
        }

        [Fact]
        public void Constructor_Throws_IfRouteDataIsNull()
        {
            Assert.ThrowsArgumentNull(
                () => new HttpControllerContext(new HttpConfiguration(), null, new HttpRequestMessage()),
                "routeData");
        }

        [Fact]
        public void Constructor_Throws_IfRequestIsNull()
        {
            Assert.ThrowsArgumentNull(
                () => new HttpControllerContext(new HttpConfiguration(), new Mock<IHttpRouteData>().Object, null),
                "request");
        }

        [Fact]
        public void Configuration_Property()
        {
            Assert.Reflection.Property<HttpControllerContext, HttpConfiguration>(
                instance: new HttpControllerContext(),
                propertyGetter: cc => cc.Configuration,
                expectedDefaultValue: null,
                allowNull: false,
                roundTripTestValue: new HttpConfiguration());
        }

        [Fact]
        public void Controller_Property()
        {
            Assert.Reflection.Property<HttpControllerContext, IHttpController>(
                instance: new HttpControllerContext(),
                propertyGetter: cc => cc.Controller,
                expectedDefaultValue: null,
                allowNull: false,
                roundTripTestValue: new Mock<IHttpController>().Object);
        }

        [Fact]
        public void ControllerDescriptor_Property()
        {
            Assert.Reflection.Property<HttpControllerContext, HttpControllerDescriptor>(
                instance: new HttpControllerContext(),
                propertyGetter: cc => cc.ControllerDescriptor,
                expectedDefaultValue: null,
                allowNull: false,
                roundTripTestValue: new HttpControllerDescriptor());
        }

        [Fact]
        public void RouteData_Property()
        {
            Assert.Reflection.Property<HttpControllerContext, IHttpRouteData>(
                instance: new HttpControllerContext(),
                propertyGetter: cc => cc.RouteData,
                expectedDefaultValue: null,
                allowNull: false,
                roundTripTestValue: new Mock<IHttpRouteData>().Object);
        }

    }
}
