// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Owin
{
    public class HostAuthenticationAttributeTest
    {
        [Fact]
        public void AttributeUsageValidOn_IsClassOrMethod()
        {
            // Act
            AttributeUsageAttribute usage = (AttributeUsageAttribute)Attribute.GetCustomAttribute(
                typeof(HostAuthenticationAttribute), typeof(AttributeUsageAttribute));

            // Assert
            Assert.NotNull(usage);
            Assert.Equal(AttributeTargets.Class | AttributeTargets.Method, usage.ValidOn);
        }

        [Fact]
        public void AttributeUsageAllowMultiple_IsTrue()
        {
            // Act
            AttributeUsageAttribute usage = (AttributeUsageAttribute)Attribute.GetCustomAttribute(
                typeof(HostAuthenticationAttribute), typeof(AttributeUsageAttribute));

            // Assert
            Assert.NotNull(usage);
            Assert.True(usage.AllowMultiple);
        }

        [Fact]
        public void Constructor_ThrowsWhenInnerFilterIsNull()
        {
            // Arrange
            IAuthenticationFilter innerFilter = null;

            // Act & Assert
            Assert.ThrowsArgumentNull(() => { var ignore = CreateProductUnderTest(innerFilter); }, "innerFilter");
        }

        [Fact]
        public void InnerFilter_ReturnsSpecifiedInstance()
        {
            // Arrange
            IAuthenticationFilter expectedInnerFilter = CreateDummyFilter();
            HostAuthenticationAttribute product = CreateProductUnderTest(expectedInnerFilter);

            // Act
            IAuthenticationFilter innerFilter = product.InnerFilter;

            // Assert
            Assert.Same(expectedInnerFilter, innerFilter);
        }

        [Fact]
        public void AllowMultiple_ReturnsTrue()
        {
            // Arrange
            IAuthenticationFilter innerFilter = CreateDummyFilter();
            IAuthenticationFilter product = CreateProductUnderTest(innerFilter);

            // Act
            bool allowMultiple = product.AllowMultiple;

            // Assert
            Assert.True(allowMultiple);
        }

        [Fact]
        public void AuthenticateAsync_DelegatesToInnerFilter()
        {
            // Arrange
            Task expectedTask = CreateTask();
            HttpAuthenticationContext context = CreateAuthenticationContext();
            CancellationToken cancellationToken = CreateCancellationToken();
            Mock<IAuthenticationFilter> spyMock = new Mock<IAuthenticationFilter>(MockBehavior.Strict);
            spyMock.Setup(f => f.AuthenticateAsync(context, cancellationToken)).Returns(expectedTask);
            IAuthenticationFilter spy = spyMock.Object;
            IAuthenticationFilter product = CreateProductUnderTest(spy);

            // Act
            Task task = product.AuthenticateAsync(context, cancellationToken);

            // Assert
            Assert.Same(expectedTask, task);
        }

        [Fact]
        public void ChallengeAsync_DelegatesToInnerFilter()
        {
            // Arrange
            Task expectedTask = CreateTask();
            HttpAuthenticationChallengeContext context = CreateChallengeContext();
            CancellationToken cancellationToken = CreateCancellationToken();
            Mock<IAuthenticationFilter> spyMock = new Mock<IAuthenticationFilter>(MockBehavior.Strict);
            spyMock.Setup(f => f.ChallengeAsync(context, cancellationToken)).Returns(expectedTask);
            IAuthenticationFilter spy = spyMock.Object;
            IAuthenticationFilter product = CreateProductUnderTest(spy);

            // Act
            Task task = product.ChallengeAsync(context, cancellationToken);

            // Assert
            Assert.Same(expectedTask, task);
        }

        [Fact]
        public void ConstructorWithString_ThrowsWhenAuthenticationTypeIsNull()
        {
            // Arrange
            string authenticationType = null;

            // Act & Assert
            Assert.ThrowsArgumentNull(() => { var ignore = CreateProductUnderTest(authenticationType); },
                "authenticationType");
        }

        [Fact]
        public void InnerFilter_IsHostAuthenticationFilter_WhenUsingConstructorWithString()
        {
            // Arrange
            string expectedAuthenticationType = "Ignore";
            HostAuthenticationAttribute product = CreateProductUnderTest(expectedAuthenticationType);

            // Act
            IAuthenticationFilter innerFilter = product.InnerFilter;

            // Assert
            HostAuthenticationFilter typedInnerFilter = Assert.IsType<HostAuthenticationFilter>(innerFilter);
            Assert.Same(expectedAuthenticationType, typedInnerFilter.AuthenticationType);
        }

        [Fact]
        public void AuthenticationType_IsSpecifiedInstance_WhenUsingConstructorWithString()
        {
            // Arrange
            string expectedAuthenticationType = "Ignore";
            HostAuthenticationAttribute product = CreateProductUnderTest(expectedAuthenticationType);

            // Act
            string authenticationType = product.AuthenticationType;

            // Assert
            Assert.Same(expectedAuthenticationType, authenticationType);
        }

        private static HttpAuthenticationContext CreateAuthenticationContext()
        {
            return new HttpAuthenticationContext(new HttpActionContext(), CreateDummyPrincipal());
        }

        private static CancellationToken CreateCancellationToken()
        {
            return new CancellationToken(true);
        }

        private static HttpAuthenticationChallengeContext CreateChallengeContext()
        {
            return new HttpAuthenticationChallengeContext(new HttpActionContext(), CreateDummyActionResult());
        }

        private static IHttpActionResult CreateDummyActionResult()
        {
            return new Mock<IHttpActionResult>(MockBehavior.Strict).Object;
        }

        private static IAuthenticationFilter CreateDummyFilter()
        {
            return new Mock<IAuthenticationFilter>(MockBehavior.Strict).Object;
        }

        private static IPrincipal CreateDummyPrincipal()
        {
            return new Mock<IPrincipal>(MockBehavior.Strict).Object;
        }

        private static HostAuthenticationAttribute CreateProductUnderTest(string authenticationType)
        {
            return new HostAuthenticationAttribute(authenticationType);
        }

        private static HostAuthenticationAttribute CreateProductUnderTest(IAuthenticationFilter innerFilter)
        {
            return new HostAuthenticationAttribute(innerFilter);
        }

        private static Task CreateTask()
        {
            return Task.FromResult<object>(null);
        }
    }
}
