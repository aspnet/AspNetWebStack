// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Routing;
using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    public class AreaRegistrationTest
    {
        [Fact]
        public void CreateContextAndRegister()
        {
            // Arrange
            string[] expectedNamespaces = new string[] { "System.Web.Mvc.Test.*" };

            RouteCollection routes = new RouteCollection();
            MyAreaRegistration registration = new MyAreaRegistration();

            // Act
            registration.CreateContextAndRegister(routes, "some state");

            // Assert
            Assert.Equal(expectedNamespaces, registration.Namespaces);
            Assert.Equal("some state", registration.State);
        }

        [Fact]
        public void RegisterAllAreas()
        {
            // Arrange
            string[] expectedLoadedAreas = new string[] { "AreaRegistrationTest_AreaRegistration" };
            AnnotatedRouteCollection routes = new AnnotatedRouteCollection();
            MockBuildManager buildManager = new MockBuildManager(new Assembly[] { typeof(AreaRegistrationTest).Assembly });

            // Act
            AreaRegistration.RegisterAllAreas(routes, buildManager, null);

            // Assert
            Assert.Equal(expectedLoadedAreas, routes._areasLoaded.ToArray());
        }

        private class MyAreaRegistration : AreaRegistration
        {
            public string[] Namespaces;
            public object State;

            public override string AreaName
            {
                get { return "my_area"; }
            }

            public override void RegisterArea(AreaRegistrationContext context)
            {
                Namespaces = context.Namespaces.ToArray();
                State = context.State;
            }
        }
    }

    public class AnnotatedRouteCollection : RouteCollection
    {
        public List<string> _areasLoaded = new List<string>();
    }

    public abstract class AreaRegistrationTest_AbstractAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get { return "the_area"; }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            ((AnnotatedRouteCollection)context.Routes)._areasLoaded.Add("AreaRegistrationTest_AbstractAreaRegistration");
        }
    }

    public class AreaRegistrationTest_AreaRegistration : AreaRegistrationTest_AbstractAreaRegistration
    {
        public override void RegisterArea(AreaRegistrationContext context)
        {
            ((AnnotatedRouteCollection)context.Routes)._areasLoaded.Add("AreaRegistrationTest_AreaRegistration");
        }
    }

    public class AreaRegistrationTest_NoConstructorAreaRegistration : AreaRegistrationTest_AreaRegistration
    {
        private AreaRegistrationTest_NoConstructorAreaRegistration()
        {
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            ((AnnotatedRouteCollection)context.Routes)._areasLoaded.Add("AreaRegistrationTest_NoConstructorAreaRegistration");
        }
    }
}
