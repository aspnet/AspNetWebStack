// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.WebPages.Test
{
    public class RequestResourceTrackerTest
    {
        [Fact]
        public void RegisteringForDisposeDisposesObjects()
        {
            // Arrange
            var context = new Mock<HttpContextBase>();
            IDictionary items = new Hashtable();
            context.Setup(m => m.Items).Returns(items);
            var disposable = new Mock<IDisposable>();
            disposable.Setup(m => m.Dispose()).Verifiable();

            // Act
            RequestResourceTracker.RegisterForDispose(context.Object, disposable.Object);
            RequestResourceTracker.DisposeResources(context.Object);

            // Assert
            disposable.VerifyAll();
        }

        [Fact]
        public void RegisteringForDisposeExtensionMethodNullContextThrows()
        {
            // Arrange
            var disposable = new Mock<IDisposable>();

            // Act
            Assert.ThrowsArgumentNull(() => HttpContextExtensions.RegisterForDispose(null, disposable.Object), "context");
        }
    }
}
