// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.WebPages.Test
{
    public class CultureUtilTest
    {
        [Fact]
        [ReplaceCulture(Culture = "es-PR", UICulture = "es-PR")]
        public void SetAutoCultureWithNoUserLanguagesDoesNothing()
        {
            // Arrange
            var context = GetContextForSetCulture(null);
            Thread thread = Thread.CurrentThread;
            CultureInfo culture = thread.CurrentCulture;

            // Act
            CultureUtil.SetCulture(thread, context, "auto");

            // Assert
            Assert.Equal(culture, thread.CurrentCulture);
        }

        [Fact]
        [ReplaceCulture(Culture = "es-PR", UICulture = "es-PR")]
        public void SetAutoUICultureWithNoUserLanguagesDoesNothing()
        {
            // Arrange
            var context = GetContextForSetCulture(null);
            Thread thread = Thread.CurrentThread;
            CultureInfo culture = thread.CurrentUICulture;

            // Act
            CultureUtil.SetUICulture(thread, context, "auto");

            // Assert
            Assert.Equal(culture, thread.CurrentUICulture);
        }

        [Fact]
        [ReplaceCulture(Culture = "es-PR", UICulture = "es-PR")]
        public void SetAutoCultureWithEmptyUserLanguagesDoesNothing()
        {
            // Arrange
            var context = GetContextForSetCulture(Enumerable.Empty<string>());
            Thread thread = Thread.CurrentThread;
            CultureInfo culture = thread.CurrentCulture;

            // Act
            CultureUtil.SetCulture(thread, context, "auto");

            // Assert
            Assert.Equal(culture, thread.CurrentCulture);
        }

        [Fact]
        [ReplaceCulture(Culture = "es-PR", UICulture = "es-PR")]
        public void SetAutoUICultureWithEmptyUserLanguagesDoesNothing()
        {
            // Arrange
            var context = GetContextForSetCulture(Enumerable.Empty<string>());
            Thread thread = Thread.CurrentThread;
            CultureInfo culture = thread.CurrentUICulture;

            // Act
            CultureUtil.SetUICulture(thread, context, "auto");

            // Assert
            Assert.Equal(culture, thread.CurrentUICulture);
        }

        [Fact]
        [ReplaceCulture(Culture = "es-PR", UICulture = "es-PR")]
        public void SetAutoCultureWithBlankUserLanguagesDoesNothing()
        {
            // Arrange
            var context = GetContextForSetCulture(new[] { " " });
            Thread thread = Thread.CurrentThread;
            CultureInfo culture = thread.CurrentCulture;

            // Act
            CultureUtil.SetCulture(thread, context, "auto");

            // Assert
            Assert.Equal(culture, thread.CurrentCulture);
        }

        [Fact]
        [ReplaceCulture(Culture = "es-PR", UICulture = "es-PR")]
        public void SetAutoUICultureWithBlankUserLanguagesDoesNothing()
        {
            // Arrange
            var context = GetContextForSetCulture(new[] { " " });
            Thread thread = Thread.CurrentThread;
            CultureInfo culture = thread.CurrentUICulture;

            // Act
            CultureUtil.SetUICulture(thread, context, "auto");

            // Assert
            Assert.Equal(culture, thread.CurrentUICulture);
        }

        [Fact]
        [ReplaceCulture(Culture = "es-PR", UICulture = "es-PR")]
        public void SetAutoCultureWithInvalidLanguageDoesNothing()
        {
            // Arrange
            // "sans-culture" is an invalid culture name everywhere -- even on Windows 10.
            var context = GetContextForSetCulture(new[] { "sans-culture", "bb-BB", "cc-CC" });
            Thread thread = Thread.CurrentThread;
            CultureInfo culture = thread.CurrentCulture;

            // Act
            CultureUtil.SetCulture(thread, context, "auto");

            // Assert
            Assert.Equal(culture, thread.CurrentCulture);
        }

        [Fact]
        [ReplaceCulture(Culture = "es-PR", UICulture = "es-PR")]
        public void SetAutoUICultureWithInvalidLanguageDoesNothing()
        {
            // Arrange
            // "sans-culture" is an invalid culture name everywhere -- even on Windows 10.
            var context = GetContextForSetCulture(new[] { "sans-culture", "bb-BB", "cc-CC" });
            Thread thread = Thread.CurrentThread;
            CultureInfo culture = thread.CurrentUICulture;

            // Act
            CultureUtil.SetUICulture(thread, context, "auto");

            // Assert
            Assert.Equal(culture, thread.CurrentUICulture);
        }

        [Fact]
        [ReplaceCulture(Culture = "es-PR", UICulture = "es-PR")]
        public void SetAutoCultureDetectsUserLanguageCulture()
        {
            // Arrange
            var context = GetContextForSetCulture(new[] { "en-GB", "en-US", "ar-eg" });
            Thread thread = Thread.CurrentThread;

            // Act
            CultureUtil.SetCulture(thread, context, "auto");

            // Assert
            Assert.Equal(CultureInfo.GetCultureInfo("en-GB"), thread.CurrentCulture);
            Assert.Equal("05/01/1979", new DateTime(1979, 1, 5).ToString("d", thread.CurrentCulture));
        }

        [Fact]
        [ReplaceCulture(Culture = "es-PR", UICulture = "es-PR")]
        public void SetAutoUICultureDetectsUserLanguageCulture()
        {
            // Arrange
            var context = GetContextForSetCulture(new[] { "en-GB", "en-US", "ar-eg" });
            Thread thread = Thread.CurrentThread;

            // Act
            CultureUtil.SetUICulture(thread, context, "auto");

            // Assert
            Assert.Equal(CultureInfo.GetCultureInfo("en-GB"), thread.CurrentUICulture);
            Assert.Equal("05/01/1979", new DateTime(1979, 1, 5).ToString("d", thread.CurrentUICulture));
        }

        [Fact]
        [ReplaceCulture(Culture = "es-PR", UICulture = "es-PR")]
        public void SetAutoCultureUserLanguageWithQParameterCulture()
        {
            // Arrange
            var context = GetContextForSetCulture(new[] { "en-GB;q=0.3", "en-US", "ar-eg;q=0.5" });
            Thread thread = Thread.CurrentThread;

            // Act
            CultureUtil.SetCulture(thread, context, "auto");

            // Assert
            Assert.Equal(CultureInfo.GetCultureInfo("en-GB"), thread.CurrentCulture);
            Assert.Equal("05/01/1979", new DateTime(1979, 1, 5).ToString("d", thread.CurrentCulture));
        }

        [Fact]
        [ReplaceCulture(Culture = "es-PR", UICulture = "es-PR")]
        public void SetAutoUICultureDetectsUserLanguageWithQParameterCulture()
        {
            // Arrange
            var context = GetContextForSetCulture(new[] { "en-GB;q=0.3", "en-US", "ar-eg;q=0.5" });
            Thread thread = Thread.CurrentThread;

            // Act
            CultureUtil.SetUICulture(thread, context, "auto");

            // Assert
            Assert.Equal(CultureInfo.GetCultureInfo("en-GB"), thread.CurrentUICulture);
            Assert.Equal("05/01/1979", new DateTime(1979, 1, 5).ToString("d", thread.CurrentUICulture));
        }

        [Fact]
        [ReplaceCulture(Culture = "es-PR", UICulture = "es-PR")]
        public void SetCultureWithInvalidCultureThrows()
        {
            // Arrange
            var context = GetContextForSetCulture();
            Thread thread = Thread.CurrentThread;

            // Act and Assert
            Assert.Throws<CultureNotFoundException>(() => CultureUtil.SetCulture(thread, context, "sans-culture"));
        }

        [Fact]
        [ReplaceCulture(Culture = "es-PR", UICulture = "es-PR")]
        public void SetUICultureWithInvalidCultureThrows()
        {
            // Arrange
            var context = GetContextForSetCulture();
            Thread thread = Thread.CurrentThread;

            // Act and Assert
            Assert.Throws<CultureNotFoundException>(() => CultureUtil.SetUICulture(thread, context, "sans-culture"));
        }

        [Fact]
        [ReplaceCulture(Culture = "es-PR", UICulture = "es-PR")]
        public void SetCultureWithValidCulture()
        {
            // Arrange
            var context = GetContextForSetCulture();
            Thread thread = Thread.CurrentThread;

            // Act
            CultureUtil.SetCulture(thread, context, "en-GB");

            // Assert
            Assert.Equal(CultureInfo.GetCultureInfo("en-GB"), thread.CurrentCulture);
            Assert.Equal("05/01/1979", new DateTime(1979, 1, 5).ToString("d", thread.CurrentCulture));
        }

        [Fact]
        [ReplaceCulture(Culture = "es-PR", UICulture = "es-PR")]
        public void SetUICultureWithValidCulture()
        {
            // Arrange
            var context = GetContextForSetCulture();
            Thread thread = Thread.CurrentThread;

            // Act
            CultureUtil.SetUICulture(thread, context, "en-GB");

            // Assert
            Assert.Equal(CultureInfo.GetCultureInfo("en-GB"), thread.CurrentUICulture);
            Assert.Equal("05/01/1979", new DateTime(1979, 1, 5).ToString("d", thread.CurrentUICulture));
        }

        private static HttpContextBase GetContextForSetCulture(IEnumerable<string> userLanguages = null)
        {
            Mock<HttpContextBase> contextMock = new Mock<HttpContextBase>();
            contextMock.Setup(context => context.Request.UserLanguages).Returns(userLanguages == null ? null : userLanguages.ToArray());
            return contextMock.Object;
        }
    }
}
