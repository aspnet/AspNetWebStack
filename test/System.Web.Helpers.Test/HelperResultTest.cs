// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Web.WebPages;
using Microsoft.TestCommon;

namespace System.Web.Helpers.Test
{
    /// <summary>
    /// This is a test class for Util is intended
    /// to contain all HelperResult Unit Tests
    /// </summary>
    public class HelperResultTest
    {
        [Fact]
        public void HelperResultConstructorNullTest()
        {
            Assert.ThrowsArgumentNull(() => { var helper = new HelperResult(null); }, "action");
        }

        [Fact]
        public void ToStringTest()
        {
            var text = "Hello";
            Action<TextWriter> action = tw => tw.Write(text);
            var helper = new HelperResult(action);
            Assert.Equal(text, helper.ToString());
        }

        [Fact]
        public void WriteToTest()
        {
            var text = "Hello";
            Action<TextWriter> action = tw => tw.Write(text);
            var helper = new HelperResult(action);
            var writer = new StringWriter();
            helper.WriteTo(writer);
            Assert.Equal(text, writer.ToString());
        }

        [Fact]
        public void ToHtmlStringDoesNotEncode()
        {
            // Arrange
            string text = "<strong>This is a test & it uses html.</strong>";
            Action<TextWriter> action = writer => writer.Write(text);
            HelperResult helperResult = new HelperResult(action);

            // Act
            string result = helperResult.ToHtmlString();

            // Assert
            Assert.Equal(result, text);
        }

        [Fact]
        public void ToHtmlStringReturnsSameResultAsWriteTo()
        {
            // Arrange
            string text = "<strong>This is a test & it uses html.</strong>";
            Action<TextWriter> action = writer => writer.Write(text);
            HelperResult helperResult = new HelperResult(action);
            StringWriter stringWriter = new StringWriter();

            // Act
            string htmlString = helperResult.ToHtmlString();
            helperResult.WriteTo(stringWriter);

            // Assert
            Assert.Equal(htmlString, stringWriter.ToString());
        }
    }
}
