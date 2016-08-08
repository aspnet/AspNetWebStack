// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.WebPages.Scope;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.WebPages.Test
{
    public class AspNetRequestStorageProvider
    {
        [Fact]
        public void AspNetStorageProviderReturnsApplicationStateBeforeAppStart()
        {
            // Arrange
            var provider = GetProvider(() => false);

            // Act and Assert
            Assert.NotNull(provider.ApplicationScope);
            Assert.NotNull(provider.GlobalScope);
            Assert.Equal(provider.ApplicationScope, provider.GlobalScope);
        }

        [Fact]
        public void AspNetStorageProviderThrowsWhenAccessingRequestScopeBeforeAppStart()
        {
            // Arrange
            var provider = GetProvider(() => false);

            // Act and Assert
            Assert.Throws<InvalidOperationException>(
                () => { var x = provider.RequestScope; },
                "RequestScope cannot be created when _AppStart is executing.");
        }

        [Fact]
        public void AspNetStorageProviderThrowsWhenAssigningScopeBeforeAppStart()
        {
            // Arrange
            var provider = GetProvider(() => false);

            // Act and Assert
            Assert.Throws<InvalidOperationException>(
                () => { provider.CurrentScope = new ScopeStorageDictionary(); },
                "Storage scopes cannot be created when _AppStart is executing.");
        }

        [Fact]
        public void AspNetStorageProviderReturnsRequestScopeAfterAppStart()
        {
            // Arrange
            var provider = GetProvider();

            // Act and Assert 
            Assert.NotNull(provider.RequestScope);
            Assert.Equal(provider.RequestScope, provider.CurrentScope);
        }

        [Fact]
        public void AspNetStorageRetrievesRequestScopeAfterSettingAnonymousScopes()
        {
            // Arrange
            var provider = GetProvider();

            // Act 
            var requestScope = provider.RequestScope;

            var Scope = new ScopeStorageDictionary();
            provider.CurrentScope = Scope;

            Assert.Equal(provider.CurrentScope, Scope);
            Assert.Equal(provider.RequestScope, requestScope);
        }

        [Fact]
        public void AspNetStorageUsesApplicationScopeAsGlobalScope()
        {
            // Arrange
            var provider = GetProvider();

            // Act and Assert
            Assert.Equal(provider.GlobalScope, provider.ApplicationScope);
        }

        private AspNetRequestScopeStorageProvider GetProvider(Func<bool> appStartExecuted = null)
        {
            Mock<HttpContextBase> context = new Mock<HttpContextBase>();
            context.Setup(c => c.Items).Returns(new Dictionary<object, object>());
            appStartExecuted = appStartExecuted ?? (() => true);

            return new AspNetRequestScopeStorageProvider(context.Object, appStartExecuted);
        }
    }
}
