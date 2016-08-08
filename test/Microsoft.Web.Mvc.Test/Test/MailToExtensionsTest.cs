// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.TestCommon;
using Microsoft.Web.UnitTestUtil;

namespace Microsoft.Web.Mvc.Test
{
    public class MailToExtensionsTest
    {
        [Fact]
        public void MailToWithoutEmailThrowsArgumentNullException()
        {
            HtmlHelper html = MvcHelper.GetHtmlHelperWithPath(new ViewDataDictionary());
            Assert.ThrowsArgumentNull(() => html.Mailto("link text", null), "emailAddress");
        }

        [Fact]
        public void MailToWithoutLinkTextThrowsArgumentNullException()
        {
            HtmlHelper html = MvcHelper.GetHtmlHelperWithPath(new ViewDataDictionary());
            Assert.ThrowsArgumentNull(() => html.Mailto(null, "somebody@example.com"), "linkText");
        }

        [Fact]
        public void MailToWithLinkTextAndEmailRendersProperElement()
        {
            HtmlHelper html = MvcHelper.GetHtmlHelperWithPath(new ViewDataDictionary());
            MvcHtmlString result = html.Mailto("This is a test", "test@example.com");
            Assert.Equal("<a href=\"mailto:test@example.com\">This is a test</a>", result.ToHtmlString());
        }

        [Fact]
        public void MailToWithLinkTextEmailAndHtmlAttributesRendersAttributes()
        {
            HtmlHelper html = MvcHelper.GetHtmlHelperWithPath(new ViewDataDictionary());
            MvcHtmlString result = html.Mailto("This is a test", "test@example.com", new { title = "this is a test" });
            Assert.Equal("<a href=\"mailto:test@example.com\" title=\"this is a test\">This is a test</a>", result.ToHtmlString());
        }

        [Fact]
        public void MailToWithLinkTextEmailAndHtmlAttributesDictionaryRendersAttributes()
        {
            HtmlHelper html = MvcHelper.GetHtmlHelperWithPath(new ViewDataDictionary());
            MvcHtmlString result = html.Mailto("This is a test", "test@example.com", new RouteValueDictionary(new { title = "this is a test" }));
            Assert.Equal("<a href=\"mailto:test@example.com\" title=\"this is a test\">This is a test</a>", result.ToHtmlString());
        }

        [Fact]
        public void MailToWithSubjectAndHtmlAttributesRendersAttributes()
        {
            HtmlHelper html = MvcHelper.GetHtmlHelperWithPath(new ViewDataDictionary());
            MvcHtmlString result = html.Mailto("This is a test", "test@example.com", "The subject", new { title = "this is a test" });
            Assert.Equal("<a href=\"mailto:test@example.com?subject=The subject\" title=\"this is a test\">This is a test</a>", result.ToHtmlString());
        }

        [Fact]
        public void MailToWithSubjectAndHtmlAttributesDictionaryRendersAttributes()
        {
            HtmlHelper html = MvcHelper.GetHtmlHelperWithPath(new ViewDataDictionary());
            MvcHtmlString result = html.Mailto("This is a test", "test@example.com", "The subject", new RouteValueDictionary(new { title = "this is a test" }));
            Assert.Equal("<a href=\"mailto:test@example.com?subject=The subject\" title=\"this is a test\">This is a test</a>", result.ToHtmlString());
        }

        [Fact]
        public void MailToAttributeEncodesEmail()
        {
            HtmlHelper html = MvcHelper.GetHtmlHelperWithPath(new ViewDataDictionary());
            MvcHtmlString result = html.Mailto("This is a test", "te\">st@example.com");
            Assert.Equal("<a href=\"mailto:te&quot;>st@example.com\">This is a test</a>", result.ToHtmlString());
        }

        [Fact]
        public void MailToWithMultipleRecipientsRendersWithCommas()
        {
            HtmlHelper html = MvcHelper.GetHtmlHelperWithPath(new ViewDataDictionary());
            MvcHtmlString result = html.Mailto("This is a test", "te\">st@example.com,test2@example.com");
            Assert.Equal("<a href=\"mailto:te&quot;>st@example.com,test2@example.com\">This is a test</a>", result.ToHtmlString());
        }

        [Fact]
        public void MailToWithSubjectAppendsSubjectQuery()
        {
            HtmlHelper html = MvcHelper.GetHtmlHelperWithPath(new ViewDataDictionary());
            MvcHtmlString result = html.Mailto("This is a test", "test@example.com", "This is the subject");
            Assert.Equal("<a href=\"mailto:test@example.com?subject=This is the subject\">This is a test</a>", result.ToHtmlString());
        }

        [Fact]
        public void MailToWithCopyOnlyAppendsCopyQuery()
        {
            HtmlHelper html = MvcHelper.GetHtmlHelperWithPath(new ViewDataDictionary());
            MvcHtmlString result = html.Mailto("This is a test", "test@example.com", null, null, "cctest@example.com", null, null);
            Assert.Equal("<a href=\"mailto:test@example.com?cc=cctest@example.com\">This is a test</a>", result.ToHtmlString());
        }

        [Fact]
        public void MailToWithMultipartBodyRendersProperMailtoEncoding()
        {
            HtmlHelper html = MvcHelper.GetHtmlHelperWithPath(new ViewDataDictionary());
            string body = "Line one" + Environment.NewLine
                        + "Line two" + Environment.NewLine
                        + "Line three";

            MvcHtmlString result = html.Mailto("email me", "test@example.com", null, body, null, null, null);
            Assert.Equal("<a href=\"mailto:test@example.com?body=Line one%0ALine two%0ALine three\">email me</a>", result.ToHtmlString());
        }

        [Fact]
        public void MailToWithAllValuesProvidedRendersCorrectTag()
        {
            HtmlHelper html = MvcHelper.GetHtmlHelperWithPath(new ViewDataDictionary());
            string body = "Line one" + Environment.NewLine
                        + "Line two" + Environment.NewLine
                        + "Line three";

            MvcHtmlString result = html.Mailto("email me", "test@example.com", "the subject", body, "cc@example.com", "bcc@example.com", new { title = "email test" });
            string expected = @"<a href=""mailto:test@example.com?subject=the subject&amp;cc=cc@example.com&amp;bcc=bcc@example.com&amp;body=Line one%0ALine two%0ALine three"" title=""email test"">email me</a>";
            Assert.Equal(expected, result.ToHtmlString());
        }

        [Fact]
        public void MailToWithAttributesWithUnderscores()
        {
            HtmlHelper html = MvcHelper.GetHtmlHelperWithPath(new ViewDataDictionary());
            string body = "Line one" + Environment.NewLine
                        + "Line two" + Environment.NewLine
                        + "Line three";

            MvcHtmlString result = html.Mailto("email me", "test@example.com", "the subject", body, "cc@example.com", "bcc@example.com", new { foo_bar = "baz" });
            string expected = @"<a foo-bar=""baz"" href=""mailto:test@example.com?subject=the subject&amp;cc=cc@example.com&amp;bcc=bcc@example.com&amp;body=Line one%0ALine two%0ALine three"">email me</a>";
            Assert.Equal(expected, result.ToHtmlString());
        }
    }
}
