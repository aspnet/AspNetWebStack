// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Web.Mvc;

namespace Microsoft.Web.Mvc
{
    [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "MailTo", Justification = "This is correctly cased.")]
    public static class MailToExtensions
    {
        public static MvcHtmlString Mailto(this HtmlHelper helper, string linkText, string emailAddress)
        {
            return Mailto(helper, linkText, emailAddress, null, null, null, null, null);
        }

        public static MvcHtmlString Mailto(this HtmlHelper helper, string linkText, string emailAddress, object htmlAttributes)
        {
            return Mailto(helper, linkText, emailAddress, null, null, null, null, htmlAttributes);
        }

        public static MvcHtmlString Mailto(this HtmlHelper helper, string linkText, string emailAddress, IDictionary<string, object> htmlAttributes)
        {
            return Mailto(helper, linkText, emailAddress, null, null, null, null, htmlAttributes);
        }

        public static MvcHtmlString Mailto(this HtmlHelper helper, string linkText, string emailAddress, string subject)
        {
            return Mailto(helper, linkText, emailAddress, subject, null, null, null, null);
        }

        public static MvcHtmlString Mailto(this HtmlHelper helper, string linkText, string emailAddress, string subject, object htmlAttributes)
        {
            return Mailto(helper, linkText, emailAddress, subject, null, null, null, htmlAttributes);
        }

        public static MvcHtmlString Mailto(this HtmlHelper helper, string linkText, string emailAddress, string subject, IDictionary<string, object> htmlAttributes)
        {
            return Mailto(helper, linkText, emailAddress, subject, null, null, null, htmlAttributes);
        }

        public static MvcHtmlString Mailto(this HtmlHelper helper, string linkText, string emailAddress, string subject, string body, string cc, string bcc, object htmlAttributes)
        {
            return Mailto(helper, linkText, emailAddress, subject, body, cc, bcc, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        public static MvcHtmlString Mailto(this HtmlHelper helper, string linkText, string emailAddress, string subject,
                                           string body, string cc, string bcc, IDictionary<string, object> htmlAttributes)
        {
            if (emailAddress == null)
            {
                throw new ArgumentNullException("emailAddress"); // TODO: Resource message
            }
            if (linkText == null)
            {
                throw new ArgumentNullException("linkText"); // TODO: Resource message
            }

            string mailToUrl = "mailto:" + emailAddress;

            List<string> mailQuery = new List<string>();
            if (!String.IsNullOrEmpty(subject))
            {
                mailQuery.Add("subject=" + helper.Encode(subject));
            }

            if (!String.IsNullOrEmpty(cc))
            {
                mailQuery.Add("cc=" + helper.Encode(cc));
            }

            if (!String.IsNullOrEmpty(bcc))
            {
                mailQuery.Add("bcc=" + helper.Encode(bcc));
            }

            if (!String.IsNullOrEmpty(body))
            {
                string encodedBody = helper.Encode(body);
                encodedBody = encodedBody.Replace(Environment.NewLine, "%0A");
                mailQuery.Add("body=" + encodedBody);
            }

            string query = String.Empty;
            for (int i = 0; i < mailQuery.Count; i++)
            {
                query += mailQuery[i];
                if (i < mailQuery.Count - 1)
                {
                    query += "&";
                }
            }
            if (query.Length > 0)
            {
                mailToUrl += "?" + query;
            }

            TagBuilder mailtoAnchor = new TagBuilder("a");
            mailtoAnchor.MergeAttribute("href", mailToUrl);
            mailtoAnchor.MergeAttributes(htmlAttributes, true);
            mailtoAnchor.InnerHtml = linkText;
            return MvcHtmlString.Create(mailtoAnchor.ToString());
        }
    }
}
