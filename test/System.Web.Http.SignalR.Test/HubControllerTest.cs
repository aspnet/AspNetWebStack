// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Infrastructure;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http
{
    public class HubControllerTest
    {
        [Fact]
        public void Constructor_ThrowsArgumentNullForNullHubName()
        {
            Assert.ThrowsArgumentNull(() => new NullHubNameController(), "hubName");
        }

        [Fact]
        public void HubContext_ReturnsContextResolvedFromConnectionManager()
        {
            DefaultContextController controller = new DefaultContextController("hub");
            controller.Configuration = new HttpConfiguration();
            Mock<IConnectionManager> mockConnectionManager = new Mock<IConnectionManager>();
            IHubContext context = new Mock<IHubContext>().Object;
            mockConnectionManager.Setup(mock => mock.GetHubContext("hub")).Returns(context);
            Mock<System.Web.Http.Dependencies.IDependencyResolver> mockDependencyResolver = new Mock<Dependencies.IDependencyResolver>();
            mockDependencyResolver.Setup(mock => mock.GetService(typeof(IConnectionManager))).Returns(mockConnectionManager.Object);
            controller.Configuration.DependencyResolver = mockDependencyResolver.Object;

            Assert.Same(context, controller.GetHubContext());
        }

        public class NullHubNameController : HubController
        {
            public NullHubNameController() : base(null)
            {
            }
        }

        public class DefaultContextController : HubController
        {
            public DefaultContextController(string hubName) : base(hubName)
            {
            }

            public IHubContext GetHubContext()
            {
                return HubContext;
            }

            public IConnectionManager GetConnectionManager()
            {
                return ConnectionManager;
            }
        }
    }
}
