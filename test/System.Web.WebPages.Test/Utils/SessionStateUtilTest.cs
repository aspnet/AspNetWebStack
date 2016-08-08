// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Web.Razor;
using System.Web.SessionState;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.WebPages.Test
{
    public class SessionStateUtilTest
    {
        [Fact]
        public void SetUpSessionStateDoesNotInvokeSessionStateBehaviorIfNoPageHasDirective()
        {
            // Arrange
            var page = new Mock<WebPage>(MockBehavior.Strict);
            var startPage = new Mock<StartPage>(MockBehavior.Strict);
            var webPageHttpHandler = new WebPageHttpHandler(page.Object, startPage: new Lazy<WebPageRenderingBase>(() => startPage.Object));
            var context = new Mock<HttpContextBase>(MockBehavior.Strict);

            // Act
            SessionStateUtil.SetUpSessionState(context.Object, webPageHttpHandler, new ConcurrentDictionary<Type, SessionStateBehavior?>());

            // Assert
            context.Verify(c => c.SetSessionStateBehavior(It.IsAny<SessionStateBehavior>()), Times.Never());
        }

        [Fact]
        public void SetUpSessionStateUsesSessionStateValueFromRequestingPageIfAvailable()
        {
            // Arrange
            var page = new DisabledSessionWebPage();
            var webPageHttpHandler = new WebPageHttpHandler(page, startPage: null);
            var context = new Mock<HttpContextBase>(MockBehavior.Strict);
            context.Setup(c => c.SetSessionStateBehavior(SessionStateBehavior.Disabled)).Verifiable();

            // Act
            SessionStateUtil.SetUpSessionState(context.Object, webPageHttpHandler, new ConcurrentDictionary<Type, SessionStateBehavior?>());

            // Assert
            context.Verify();
        }

        [Fact]
        public void SetUpSessionStateUsesSessionStateValueFromStartPageHierarchy()
        {
            // Arrange
            var page = new Mock<WebPage>(MockBehavior.Strict);
            var startPage = new DefaultSessionWebPage
            {
                ChildPage = new ReadOnlySessionWebPage()
            };
            var webPageHttpHandler = new WebPageHttpHandler(page.Object, startPage: new Lazy<WebPageRenderingBase>(() => startPage));
            var context = new Mock<HttpContextBase>(MockBehavior.Strict);
            context.Setup(c => c.SetSessionStateBehavior(SessionStateBehavior.ReadOnly)).Verifiable();

            // Act
            SessionStateUtil.SetUpSessionState(context.Object, webPageHttpHandler, new ConcurrentDictionary<Type, SessionStateBehavior?>());

            // Assert
            context.Verify();
        }

        [Fact]
        public void SetUpSessionStateThrowsIfSessionStateValueIsInvalid()
        {
            // Arrange
            var page = new Mock<WebPage>(MockBehavior.Strict);
            var startPage = new InvalidSessionState();
            var webPageHttpHandler = new WebPageHttpHandler(page.Object, startPage: new Lazy<WebPageRenderingBase>(() => startPage));
            var context = new Mock<HttpContextBase>(MockBehavior.Strict);

            // Act
            Assert.Throws<ArgumentException>(() => SessionStateUtil.SetUpSessionState(context.Object, webPageHttpHandler, new ConcurrentDictionary<Type, SessionStateBehavior?>()),
                "Value \"jabberwocky\" specified in \"~/_Invalid.cshtml\" is an invalid value for the SessionState directive. Possible values are: \"Default, Required, ReadOnly, Disabled\".");
        }

        [Fact]
        public void SetUpSessionStateThrowsIfMultipleSessionStateValueIsInvalid()
        {
            // Arrange
            var page = new PageWithMultipleSesionStateAttributes();
            var webPageHttpHandler = new WebPageHttpHandler(page, startPage: null);
            var context = new Mock<HttpContextBase>(MockBehavior.Strict);

            // Act
            Assert.Throws<InvalidOperationException>(() => SessionStateUtil.SetUpSessionState(context.Object, webPageHttpHandler, new ConcurrentDictionary<Type, SessionStateBehavior?>()),
                "At most one SessionState value can be declared per page.");
        }

        [Fact]
        public void SetUpSessionStateUsesCache()
        {
            // Arrange
            var page = new PageWithBadAttribute();
            var webPageHttpHandler = new WebPageHttpHandler(page, startPage: null);
            var context = new Mock<HttpContextBase>(MockBehavior.Strict);
            var dictionary = new ConcurrentDictionary<Type, SessionStateBehavior?>();
            dictionary.TryAdd(webPageHttpHandler.GetType(), SessionStateBehavior.Default);
            context.Setup(c => c.SetSessionStateBehavior(SessionStateBehavior.Default)).Verifiable();

            // Act
            SessionStateUtil.SetUpSessionState(context.Object, webPageHttpHandler, dictionary);

            // Assert
            context.Verify();
            Assert.Throws<Exception>(() => page.GetType().GetCustomAttributes(inherit: false), "Can't call me!");
        }

        [RazorDirective("sessionstate", "disabled")]
        private sealed class DisabledSessionWebPage : WebPage
        {
            public override void Execute()
            {
                throw new NotSupportedException();
            }
        }

        [RazorDirective("sessionstate", "ReadOnly")]
        private sealed class ReadOnlySessionWebPage : StartPage
        {
            public override void Execute()
            {
                throw new NotSupportedException();
            }
        }

        [RazorDirective("SessionState", "Default")]
        private sealed class DefaultSessionWebPage : StartPage
        {
            public override void Execute()
            {
                throw new NotSupportedException();
            }
        }

        [RazorDirective("SessionState", "jabberwocky")]
        private sealed class InvalidSessionState : StartPage
        {
            public override string VirtualPath
            {
                get
                {
                    return "~/_Invalid.cshtml";
                }
                set
                {
                    VirtualPath = value;
                }
            }
            public override void Execute()
            {
                throw new NotSupportedException();
            }
        }

        private sealed class BadAttribute : Attribute
        {
            public BadAttribute()
            {
                throw new Exception("Can't call me!");
            }
        }

        [RazorDirective("SessionState", "Default"), Bad]
        private sealed class PageWithBadAttribute : WebPage
        {
            public override void Execute()
            {
                throw new NotSupportedException();
            }
        }

        [RazorDirective("SessionState", "Disabled"), RazorDirective("SessionState", "ReadOnly")]
        private sealed class PageWithMultipleSesionStateAttributes : WebPage
        {
            public override void Execute()
            {
                throw new NotSupportedException();
            }
        }
    }
}
