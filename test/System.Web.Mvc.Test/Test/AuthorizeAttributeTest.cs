// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Web.TestUtil;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class AuthorizeAttributeTest
    {
        [Fact]
        public void AuthorizeAttributeReturnsUniqueTypeIDs()
        {
            // Arrange
            AuthorizeAttribute attr1 = new AuthorizeAttribute();
            AuthorizeAttribute attr2 = new AuthorizeAttribute();

            // Assert
            Assert.NotEqual(attr1.TypeId, attr2.TypeId);
        }

        [Authorize(Roles = "foo")]
        [Authorize(Roles = "bar")]
        private class ClassWithMultipleAuthorizeAttributes
        {
        }

        [Fact]
        public void CanRetrieveMultipleAuthorizeAttributesFromOneClass()
        {
            // Arrange
            ClassWithMultipleAuthorizeAttributes @class = new ClassWithMultipleAuthorizeAttributes();

            // Act
            IEnumerable<AuthorizeAttribute> attributes = TypeDescriptor.GetAttributes(@class).OfType<AuthorizeAttribute>();

            // Assert
            Assert.Equal(2, attributes.Count());
            Assert.Contains(attributes, a => a.Roles == "foo");
            Assert.Contains(attributes, a => a.Roles == "bar");
        }

        [Fact]
        public void AuthorizeCoreReturnsFalseIfNameDoesNotMatch()
        {
            // Arrange
            AuthorizeAttributeHelper helper = new AuthorizeAttributeHelper() { Users = "SomeName" };

            Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();
            mockHttpContext.Setup(c => c.User.Identity.IsAuthenticated).Returns(true);
            mockHttpContext.Setup(c => c.User.Identity.Name).Returns("SomeOtherName");

            // Act
            bool retVal = helper.PublicAuthorizeCore(mockHttpContext.Object);

            // Assert
            Assert.False(retVal);
        }

        [Fact]
        public void AuthorizeCoreReturnsFalseIfRoleDoesNotMatch()
        {
            // Arrange
            AuthorizeAttributeHelper helper = new AuthorizeAttributeHelper() { Roles = "SomeRole" };

            Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();
            mockHttpContext.Setup(c => c.User.Identity.IsAuthenticated).Returns(true);
            mockHttpContext.Setup(c => c.User.IsInRole("SomeRole")).Returns(false).Verifiable();

            // Act
            bool retVal = helper.PublicAuthorizeCore(mockHttpContext.Object);

            // Assert
            Assert.False(retVal);
            mockHttpContext.Verify();
        }

        [Fact]
        public void AuthorizeCoreReturnsFalseIfUserIsUnauthenticated()
        {
            // Arrange
            AuthorizeAttributeHelper helper = new AuthorizeAttributeHelper();

            Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();
            mockHttpContext.Setup(c => c.User.Identity.IsAuthenticated).Returns(false);

            // Act
            bool retVal = helper.PublicAuthorizeCore(mockHttpContext.Object);

            // Assert
            Assert.False(retVal);
        }

        [Fact]
        public void AuthorizeCoreReturnsTrueIfUserIsAuthenticatedAndNamesOrRolesSpecified()
        {
            // Arrange
            AuthorizeAttributeHelper helper = new AuthorizeAttributeHelper() { Users = "SomeUser, SomeOtherUser", Roles = "SomeRole, SomeOtherRole" };

            Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();
            mockHttpContext.Setup(c => c.User.Identity.IsAuthenticated).Returns(true);
            mockHttpContext.Setup(c => c.User.Identity.Name).Returns("SomeUser");
            mockHttpContext.Setup(c => c.User.IsInRole("SomeRole")).Returns(false).Verifiable();
            mockHttpContext.Setup(c => c.User.IsInRole("SomeOtherRole")).Returns(true).Verifiable();

            // Act
            bool retVal = helper.PublicAuthorizeCore(mockHttpContext.Object);

            // Assert
            Assert.True(retVal);
            mockHttpContext.Verify();
        }

        [Fact]
        public void AuthorizeCoreReturnsTrueIfUserIsAuthenticatedAndNoNamesOrRolesSpecified()
        {
            // Arrange
            AuthorizeAttributeHelper helper = new AuthorizeAttributeHelper();

            Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();
            mockHttpContext.Setup(c => c.User.Identity.IsAuthenticated).Returns(true);

            // Act
            bool retVal = helper.PublicAuthorizeCore(mockHttpContext.Object);

            // Assert
            Assert.True(retVal);
        }

        [Fact]
        public void AuthorizeCoreThrowsIfHttpContextIsNull()
        {
            // Arrange
            AuthorizeAttributeHelper helper = new AuthorizeAttributeHelper();

            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { helper.PublicAuthorizeCore((HttpContextBase)null); }, "httpContext");
        }

        [Fact]
        public void OnAuthorizationCallsHandleUnauthorizedRequestIfUserUnauthorized()
        {
            // Arrange
            CustomFailAuthorizeAttribute attr = new CustomFailAuthorizeAttribute();

            Mock<AuthorizationContext> mockAuthContext = new Mock<AuthorizationContext>();
            mockAuthContext.Setup(c => c.HttpContext.User.Identity.IsAuthenticated).Returns(false);
            mockAuthContext.Setup(c => c.HttpContext.Items).Returns(new Hashtable());
            mockAuthContext.Setup(c => c.ActionDescriptor.ControllerDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            AuthorizationContext authContext = mockAuthContext.Object;

            // Act
            attr.OnAuthorization(authContext);

            // Assert
            Assert.Equal(CustomFailAuthorizeAttribute.ExpectedResult, authContext.Result);
        }

        [Fact]
        public void OnAuthorizationFailedSetsHttpUnauthorizedResultIfUserUnauthorized()
        {
            // Arrange
            Mock<AuthorizeAttributeHelper> mockHelper = new Mock<AuthorizeAttributeHelper>() { CallBase = true };
            mockHelper.Setup(h => h.PublicAuthorizeCore(It.IsAny<HttpContextBase>())).Returns(false);
            AuthorizeAttributeHelper helper = mockHelper.Object;

            AuthorizationContext filterContext = new Mock<AuthorizationContext>() { DefaultValue = DefaultValue.Mock }.Object;

            // Act
            helper.OnAuthorization(filterContext);

            // Assert
            Assert.IsType<HttpUnauthorizedResult>(filterContext.Result);
        }

        [Fact]
        public void OnAuthorizationHooksCacheValidationIfUserAuthorized()
        {
            // Arrange
            Mock<AuthorizeAttributeHelper> mockHelper = new Mock<AuthorizeAttributeHelper>() { CallBase = true };
            mockHelper.Setup(h => h.PublicAuthorizeCore(It.IsAny<HttpContextBase>())).Returns(true);
            AuthorizeAttributeHelper helper = mockHelper.Object;

            MethodInfo callbackMethod = typeof(AuthorizeAttribute).GetMethod("CacheValidateHandler", BindingFlags.Instance | BindingFlags.NonPublic);
            Mock<AuthorizationContext> mockFilterContext = new Mock<AuthorizationContext>();
            mockFilterContext.Setup(c => c.HttpContext.Response.Cache.SetProxyMaxAge(new TimeSpan(0))).Verifiable();
            mockFilterContext.Setup(c => c.HttpContext.Items).Returns(new Hashtable());
            mockFilterContext
                .Setup(c => c.HttpContext.Response.Cache.AddValidationCallback(It.IsAny<HttpCacheValidateHandler>(), null /* data */))
                .Callback(
                    delegate(HttpCacheValidateHandler handler, object data)
                    {
                        Assert.Equal(helper, handler.Target);
                        Assert.Equal(callbackMethod, handler.Method);
                    })
                .Verifiable();
            mockFilterContext.Setup(c => c.ActionDescriptor.ControllerDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            AuthorizationContext filterContext = mockFilterContext.Object;

            // Act
            helper.OnAuthorization(filterContext);

            // Assert
            mockFilterContext.Verify();
        }

        [Fact]
        public void OnAuthorizationThrowsIfFilterContextIsNull()
        {
            // Arrange
            AuthorizeAttribute attr = new AuthorizeAttribute();

            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { attr.OnAuthorization(null); }, "filterContext");
        }

        [Fact]
        public void OnAuthorizationReturnsWithNoResultIfAllowAnonymousAttributeIsDefinedOnAction()
        {
            // Arrange
            Mock<AuthorizeAttributeHelper> mockHelper = new Mock<AuthorizeAttributeHelper>() { CallBase = true };
            AuthorizeAttributeHelper helper = mockHelper.Object;

            Mock<AuthorizationContext> mockFilterContext = new Mock<AuthorizationContext>();
            mockFilterContext.Setup(c => c.HttpContext.Items).Returns(new Hashtable());
            mockFilterContext.Setup(c => c.ActionDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(true);

            // Act
            helper.OnAuthorization(mockFilterContext.Object);

            // Assert
            Assert.Null(mockFilterContext.Object.Result);
            mockHelper.Verify(h => h.PublicAuthorizeCore(It.IsAny<HttpContextBase>()), Times.Never());
        }

        [Fact]
        public void OnAuthorizationReturnsWithNoResultIfAllowAnonymousAttributeIsDefinedOnController()
        {
            // Arrange
            Mock<AuthorizeAttributeHelper> mockHelper = new Mock<AuthorizeAttributeHelper>() { CallBase = true };
            AuthorizeAttributeHelper helper = mockHelper.Object;

            Mock<AuthorizationContext> mockFilterContext = new Mock<AuthorizationContext>();
            mockFilterContext.Setup(c => c.HttpContext.Items).Returns(new Hashtable());
            mockFilterContext.Setup(c => c.ActionDescriptor.ControllerDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(true);

            // Act
            helper.OnAuthorization(mockFilterContext.Object);

            // Assert
            Assert.Null(mockFilterContext.Object.Result);
            mockHelper.Verify(h => h.PublicAuthorizeCore(It.IsAny<HttpContextBase>()), Times.Never());
        }

        [Fact]
        public void OnCacheAuthorizationReturnsIgnoreRequestIfUserIsUnauthorized()
        {
            // Arrange
            Mock<AuthorizeAttributeHelper> mockHelper = new Mock<AuthorizeAttributeHelper>() { CallBase = true };
            mockHelper.Setup(h => h.PublicAuthorizeCore(It.IsAny<HttpContextBase>())).Returns(false);
            AuthorizeAttributeHelper helper = mockHelper.Object;

            Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();
            mockHttpContext.Setup(c => c.User).Returns(new Mock<IPrincipal>().Object);

            // Act
            HttpValidationStatus validationStatus = helper.PublicOnCacheAuthorization(mockHttpContext.Object);

            // Assert
            Assert.Equal(HttpValidationStatus.IgnoreThisRequest, validationStatus);
        }

        [Fact]
        public void OnCacheAuthorizationReturnsValidIfUserIsAuthorized()
        {
            // Arrange
            Mock<AuthorizeAttributeHelper> mockHelper = new Mock<AuthorizeAttributeHelper>() { CallBase = true };
            mockHelper.Setup(h => h.PublicAuthorizeCore(It.IsAny<HttpContextBase>())).Returns(true);
            AuthorizeAttributeHelper helper = mockHelper.Object;

            Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();
            mockHttpContext.Setup(c => c.User).Returns(new Mock<IPrincipal>().Object);

            // Act
            HttpValidationStatus validationStatus = helper.PublicOnCacheAuthorization(mockHttpContext.Object);

            // Assert
            Assert.Equal(HttpValidationStatus.Valid, validationStatus);
        }

        [Fact]
        public void OnCacheAuthorizationThrowsIfHttpContextIsNull()
        {
            // Arrange
            AuthorizeAttributeHelper helper = new AuthorizeAttributeHelper();

            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { helper.PublicOnCacheAuthorization(null); }, "httpContext");
        }

        [Fact]
        public void RolesProperty()
        {
            // Arrange
            AuthorizeAttribute attr = new AuthorizeAttribute();

            // Act & assert
            MemberHelper.TestStringProperty(attr, "Roles", String.Empty);
        }

        [Fact]
        public void UsersProperty()
        {
            // Arrange
            AuthorizeAttribute attr = new AuthorizeAttribute();

            // Act & assert
            MemberHelper.TestStringProperty(attr, "Users", String.Empty);
        }

        public class AuthorizeAttributeHelper : AuthorizeAttribute
        {
            public virtual bool PublicAuthorizeCore(HttpContextBase httpContext)
            {
                return base.AuthorizeCore(httpContext);
            }

            protected override bool AuthorizeCore(HttpContextBase httpContext)
            {
                return PublicAuthorizeCore(httpContext);
            }

            public virtual HttpValidationStatus PublicOnCacheAuthorization(HttpContextBase httpContext)
            {
                return base.OnCacheAuthorization(httpContext);
            }

            protected override HttpValidationStatus OnCacheAuthorization(HttpContextBase httpContext)
            {
                return PublicOnCacheAuthorization(httpContext);
            }
        }

        public class CustomFailAuthorizeAttribute : AuthorizeAttribute
        {
            public static readonly ActionResult ExpectedResult = new ContentResult();

            protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
            {
                filterContext.Result = ExpectedResult;
            }
        }
    }
}
