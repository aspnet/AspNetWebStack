// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Configuration.Provider;
using System.Reflection;
using System.Web.Security;
using System.Web.WebPages.TestUtils;
using Microsoft.TestCommon;
using Moq;

namespace WebMatrix.WebData.Test
{
    public class WebSecurityTest
    {
        [Fact]
        public void CreateUserAndAccount_WillAcceptBothObjectsAndDictionariesForExtendedParameters()
        {
            // since it is a static helper - you gotta love 'em
            AppDomainUtils.RunInSeparateAppDomain(() =>
                {
                    // we need that in order to make sure the Membership static class is initialized correctly
                    var discard = Membership.Provider as ProviderBase;

                    // Arrange
                    var providerMock = new Mock<ExtendedMembershipProvider>();
                    providerMock.Setup(
                        p => p.CreateUserAndAccount(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<IDictionary<string, object>>()))
                                .Returns((string username, string password, bool requireConfirmation, IDictionary<string, object> values) => "foo = " + values["foo"]);
                    typeof(Membership).GetField("s_Provider", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, providerMock.Object);

                    // Act
                    var resultWithObject = WebSecurity.CreateUserAndAccount("name", "pass", new { foo = "bar" });
                    var resultWithDictionary = WebSecurity.CreateUserAndAccount("name", "pass", new Dictionary<string, object> { { "foo", "baz" } });

                    // Assert
                    Assert.Equal("foo = bar", resultWithObject);
                    Assert.Equal("foo = baz", resultWithDictionary);
                });
        }

        [Fact]
        public void VerifyExtendedMembershipProviderMethodsThrowWithInvalidProvider()
        {
            const string errorString = "To call this method, the \"Membership.Provider\" property must be an instance of \"ExtendedMembershipProvider\".";
            Assert.Throws<InvalidOperationException>(() => WebSecurity.ConfirmAccount(""), errorString);
            Assert.Throws<InvalidOperationException>(() => WebSecurity.GeneratePasswordResetToken(""), errorString);
            Assert.Throws<InvalidOperationException>(() => WebSecurity.GetUserIdFromPasswordResetToken(""), errorString);
            Assert.Throws<InvalidOperationException>(() => WebSecurity.ResetPassword("", "whatever"), errorString);
            Assert.Throws<InvalidOperationException>(() => WebSecurity.CreateUserAndAccount("", "whatever"), errorString);
            Assert.Throws<InvalidOperationException>(() => WebSecurity.CreateAccount("", "whatever"), errorString);
            Assert.Throws<InvalidOperationException>(() => WebSecurity.IsConfirmed("whatever"), errorString);
            Assert.Throws<InvalidOperationException>(() => WebSecurity.GetPasswordFailuresSinceLastSuccess("whatever"), errorString);
            Assert.Throws<InvalidOperationException>(() => WebSecurity.GetCreateDate("whatever"), errorString);
            Assert.Throws<InvalidOperationException>(() => WebSecurity.GetLastPasswordFailureDate("whatever"), errorString);
            Assert.Throws<InvalidOperationException>(() => WebSecurity.GetPasswordChangedDate("whatever"), errorString);
        }
    }
}
