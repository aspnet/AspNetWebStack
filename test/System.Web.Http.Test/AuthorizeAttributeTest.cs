// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Web.Http.Controllers;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http
{
    public class AuthorizeAttributeTest
    {
        private readonly Mock<HttpActionDescriptor> _actionDescriptorMock = new Mock<HttpActionDescriptor>() { CallBase = true };
        private readonly Collection<AllowAnonymousAttribute> _allowAnonymousAttributeCollection = new Collection<AllowAnonymousAttribute>(new AllowAnonymousAttribute[] { new AllowAnonymousAttribute() });
        private readonly MockableAuthorizeAttribute _attribute;
        private readonly Mock<MockableAuthorizeAttribute> _attributeMock = new Mock<MockableAuthorizeAttribute>() { CallBase = true };
        private readonly Mock<HttpControllerDescriptor> _controllerDescriptorMock = new Mock<HttpControllerDescriptor>() { CallBase = true };
        private readonly HttpControllerContext _controllerContext;
        private readonly HttpActionContext _actionContext;
        private readonly Mock<IPrincipal> _principalMock = new Mock<IPrincipal>();
        private readonly HttpRequestMessage _request = new HttpRequestMessage();

        public AuthorizeAttributeTest()
        {
            _attribute = _attributeMock.Object;
            _controllerContext = new Mock<HttpControllerContext>() { CallBase = true }.Object;
            _controllerDescriptorMock.Setup(cd => cd.GetCustomAttributes<AllowAnonymousAttribute>()).Returns(new Collection<AllowAnonymousAttribute>(Enumerable.Empty<AllowAnonymousAttribute>().ToList()));
            _actionDescriptorMock.Setup(ad => ad.GetCustomAttributes<AllowAnonymousAttribute>()).Returns(new Collection<AllowAnonymousAttribute>(Enumerable.Empty<AllowAnonymousAttribute>().ToList()));
            _controllerContext.ControllerDescriptor = _controllerDescriptorMock.Object;
            _controllerContext.Request = _request;
            _actionContext = ContextUtil.CreateActionContext(_controllerContext, _actionDescriptorMock.Object);
            _controllerContext.RequestContext.Principal = _principalMock.Object;
        }

        [Fact]
        public void Roles_Property()
        {
            AuthorizeAttribute attribute = new AuthorizeAttribute();

            Assert.Reflection.StringProperty(attribute, a => a.Roles, expectedDefaultValue: String.Empty);
        }

        [Fact]
        public void Users_Property()
        {
            AuthorizeAttribute attribute = new AuthorizeAttribute();

            Assert.Reflection.StringProperty(attribute, a => a.Users, expectedDefaultValue: String.Empty);
        }

        [Fact]
        public void AllowMultiple_ReturnsTrue()
        {
            Assert.True(_attribute.AllowMultiple);
        }

        [Fact]
        public void TypeId_ReturnsUniqueInstances()
        {
            var attribute1 = new AuthorizeAttribute();
            var attribute2 = new AuthorizeAttribute();

            Assert.NotSame(attribute1.TypeId, attribute2.TypeId);
        }

        [Fact]
        public void OnAuthorization_IfContextParameterIsNull_ThrowsException()
        {
            Assert.ThrowsArgumentNull(() =>
            {
                _attribute.OnAuthorization(actionContext: null);
            }, "actionContext");
        }

        [Fact]
        public void OnAuthorization_IfUserIsAuthenticated_DoesNotShortCircuitRequest()
        {
            _principalMock.Setup(p => p.Identity.IsAuthenticated).Returns(true);

            _attribute.OnAuthorization(_actionContext);

            Assert.Null(_actionContext.Response);
        }

        [Fact]
        public void OnAuthorization_IfRequestContextDoesNotContainPrincipal_DoesShortCircuitRequest()
        {
            _actionContext.ControllerContext.RequestContext.Principal = null;

            _attribute.OnAuthorization(_actionContext);

            AssertUnauthorizedRequestSet(_actionContext);
        }

        [Fact]
        public void OnAuthorization_IfPrincipalDoesNotContainIdentity_DoesShortCircuitRequest()
        {
            _principalMock.Setup(p => p.Identity).Returns((IIdentity)null);

            _attribute.OnAuthorization(_actionContext);

            AssertUnauthorizedRequestSet(_actionContext);
        }

        [Fact]
        public void OnAuthorization_IfUserIsNotAuthenticated_DoesShortCircuitRequest()
        {
            _principalMock.Setup(p => p.Identity.IsAuthenticated).Returns(false).Verifiable();

            _attribute.OnAuthorization(_actionContext);

            AssertUnauthorizedRequestSet(_actionContext);
            _principalMock.Verify();
        }

        [Fact]
        public void OnAuthorization_IfUserIsNotInUsersCollection_DoesShortCircuitRequest()
        {
            _attribute.Users = "John";
            _principalMock.Setup(p => p.Identity.IsAuthenticated).Returns(true).Verifiable();
            _principalMock.Setup(p => p.Identity.Name).Returns("Mary").Verifiable();

            _attribute.OnAuthorization(_actionContext);

            AssertUnauthorizedRequestSet(_actionContext);
            _principalMock.Verify();
        }

        [Fact]
        public void OnAuthorization_IfUserIsInUsersCollection_DoesNotShortCircuitRequest()
        {
            _attribute.Users = " John , Mary ";
            _principalMock.Setup(p => p.Identity.IsAuthenticated).Returns(true).Verifiable();
            _principalMock.Setup(p => p.Identity.Name).Returns("Mary").Verifiable();

            _attribute.OnAuthorization(_actionContext);

            Assert.Null(_actionContext.Response);
            _principalMock.Verify();
        }

        [Fact]
        public void OnAuthorization_IfUserIsNotInRolesCollection_DoesShortCircuitRequest()
        {
            _attribute.Users = " John , Mary ";
            _attribute.Roles = "Administrators,PowerUsers";
            _principalMock.Setup(p => p.Identity.IsAuthenticated).Returns(true).Verifiable();
            _principalMock.Setup(p => p.Identity.Name).Returns("Mary").Verifiable();
            _principalMock.Setup(p => p.IsInRole("Administrators")).Returns(false).Verifiable();
            _principalMock.Setup(p => p.IsInRole("PowerUsers")).Returns(false).Verifiable();

            _attribute.OnAuthorization(_actionContext);

            AssertUnauthorizedRequestSet(_actionContext);
            _principalMock.Verify();
        }

        [Fact]
        public void OnAuthorization_IfUserIsInRolesCollection_DoesNotShortCircuitRequest()
        {
            _attribute.Users = " John , Mary ";
            _attribute.Roles = "Administrators,PowerUsers";
            _principalMock.Setup(p => p.Identity.IsAuthenticated).Returns(true).Verifiable();
            _principalMock.Setup(p => p.Identity.Name).Returns("Mary").Verifiable();
            _principalMock.Setup(p => p.IsInRole("Administrators")).Returns(false).Verifiable();
            _principalMock.Setup(p => p.IsInRole("PowerUsers")).Returns(true).Verifiable();

            _attribute.OnAuthorization(_actionContext);

            Assert.Null(_actionContext.Response);
            _principalMock.Verify();
        }

        [Fact]
        public void OnAuthorization_IfActionDescriptorIsMarkedWithAllowAnonymousAttribute_DoesNotShortCircuitResponse()
        {
            _actionDescriptorMock.Setup(ad => ad.GetCustomAttributes<AllowAnonymousAttribute>()).Returns(_allowAnonymousAttributeCollection);
            Mock<MockableAuthorizeAttribute> authorizeAttributeMock = new Mock<MockableAuthorizeAttribute>() { CallBase = true };
            AuthorizeAttribute attribute = authorizeAttributeMock.Object;

            attribute.OnAuthorization(_actionContext);

            Assert.Null(_actionContext.Response);
        }

        [Fact]
        public void OnAuthorization_IfControllerDescriptorIsMarkedWithAllowAnonymousAttribute_DoesNotShortCircuitResponse()
        {
            _controllerDescriptorMock.Setup(ad => ad.GetCustomAttributes<AllowAnonymousAttribute>()).Returns(_allowAnonymousAttributeCollection);
            Mock<MockableAuthorizeAttribute> authorizeAttributeMock = new Mock<MockableAuthorizeAttribute>() { CallBase = true };
            AuthorizeAttribute attribute = authorizeAttributeMock.Object;

            attribute.OnAuthorization(_actionContext);

            Assert.Null(_actionContext.Response);
        }

        [Fact]
        public void OnAuthorization_IfRequestNotAuthorized_CallsHandleUnauthorizedRequest()
        {
            Mock<MockableAuthorizeAttribute> authorizeAttributeMock = new Mock<MockableAuthorizeAttribute>() { CallBase = true };
            _principalMock.Setup(p => p.Identity.IsAuthenticated).Returns(false);
            authorizeAttributeMock.Setup(a => a.HandleUnauthorizedRequestPublic(_actionContext)).Verifiable();
            AuthorizeAttribute attribute = authorizeAttributeMock.Object;

            attribute.OnAuthorization(_actionContext);

            authorizeAttributeMock.Verify();
        }

        [Fact]
        public void HandleUnauthorizedRequest_IfContextParameterIsNull_ThrowsArgumentNullException()
        {
            Assert.ThrowsArgumentNull(() =>
            {
                _attribute.HandleUnauthorizedRequestPublic(context: null);
            }, "actionContext");
        }

        [Fact]
        public void HandleUnauthorizedRequest_SetsResponseWithUnauthorizedStatusCode()
        {
            _attribute.HandleUnauthorizedRequestPublic(_actionContext);

            Assert.NotNull(_actionContext.Response);
            Assert.Equal(HttpStatusCode.Unauthorized, _actionContext.Response.StatusCode);
            Assert.Same(_request, _actionContext.Response.RequestMessage);
        }

        [Theory]
        [PropertyData("SplitStringTestData")]
        public void SplitString_SplitsOnCommaAndTrimsWhitespaceAndIgnoresEmptyStrings(string input, params string[] expectedResult)
        {
            string[] result = AuthorizeAttribute.SplitString(input);

            Assert.Equal(expectedResult, result);
        }

        public static IEnumerable<object[]> SplitStringTestData
        {
            get
            {
                return new ParamsTheoryDataSet<string, string>() {
                    { null },
                    { String.Empty },
                    { "   " },
                    { "  A  ", "A" },
                    { "  A, B  ", "A", "B" },
                    { "  , A, ,B, ", "A", "B" },
                    { "   A   B   ", "A   B" },
                };
            }
        }

        [CLSCompliant(false)]
        public class ParamsTheoryDataSet<TParam1, TParam2> : TheoryDataSet
        {
            public void Add(TParam1 p1, params TParam2[] p2)
            {
                AddItem(p1, p2);
            }
        }

        private static void AssertUnauthorizedRequestSet(HttpActionContext actionContext)
        {
            Assert.NotNull(actionContext.Response);
            Assert.Equal(HttpStatusCode.Unauthorized, actionContext.Response.StatusCode);
            Assert.Same(actionContext.ControllerContext.Request, actionContext.Response.RequestMessage);
        }

        public class MockableAuthorizeAttribute : AuthorizeAttribute
        {
            protected override void HandleUnauthorizedRequest(HttpActionContext context)
            {
                HandleUnauthorizedRequestPublic(context);
            }

            public virtual void HandleUnauthorizedRequestPublic(HttpActionContext context)
            {
                base.HandleUnauthorizedRequest(context);
            }
        }
    }
}
