// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Http;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Infrastructure;
using Microsoft.TestCommon;
using Moq;

public class HubControllerOfTTest
{
    [Fact]
    public void HubContext_ReturnsContextResolvedFromConnectionManager()
    {
        DefaultContextController controller = new DefaultContextController();
        controller.Configuration = new HttpConfiguration();
        Mock<IConnectionManager> mockConnectionManager = new Mock<IConnectionManager>();
        IHubContext context = new Mock<IHubContext>().Object;
        mockConnectionManager.Setup(mock => mock.GetHubContext<MyHub>()).Returns(context);
        Mock<System.Web.Http.Dependencies.IDependencyResolver> mockDependencyResolver = new Mock<System.Web.Http.Dependencies.IDependencyResolver>();
        mockDependencyResolver.Setup(mock => mock.GetService(typeof(IConnectionManager))).Returns(mockConnectionManager.Object);
        controller.Configuration.DependencyResolver = mockDependencyResolver.Object;

        Assert.Same(context, controller.GetHubContext());
    }

    public class DefaultContextController : HubController<MyHub>
    {
        public IHubContext GetHubContext()
        {
            return HubContext;
        }

        public IConnectionManager GetConnectionManager()
        {
            return ConnectionManager;
        }
    }

    public class MyHub : Hub
    {
    }
}