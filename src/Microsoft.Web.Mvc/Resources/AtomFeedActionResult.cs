// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Net;
using System.Net.Mime;
using System.ServiceModel.Syndication;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using Microsoft.Web.Mvc.Properties;

namespace Microsoft.Web.Mvc.Resources
{
    /// <summary>
    /// An ActionResult that can render a SyndicationFeed to the Atom 1.0 feed format
    /// </summary>
    public class AtomFeedActionResult : ActionResult
    {
        private ContentType contentType;
        private SyndicationFeed feed;

        /// <summary>
        /// The content type defaults to application/atom+xml
        /// </summary>
        /// <param name="feed"></param>
        public AtomFeedActionResult(SyndicationFeed feed)
            : this(feed, new ContentType("application/atom+xml"))
        {
        }

        public AtomFeedActionResult(SyndicationFeed feed, ContentType contentType)
        {
            if (feed == null)
            {
                throw new ArgumentNullException("feed");
            }
            if (contentType == null)
            {
                throw new ArgumentNullException("contentType");
            }
            this.feed = feed;
            this.contentType = contentType;
        }

        public ContentType ContentType
        {
            get { return this.contentType; }
        }

        public SyndicationFeed Feed
        {
            get { return this.feed; }
        }

        public override void ExecuteResult(ControllerContext context)
        {
            Encoding encoding = Encoding.UTF8;
            if (!String.IsNullOrEmpty(this.ContentType.CharSet))
            {
                try
                {
                    encoding = Encoding.GetEncoding(this.ContentType.CharSet);
                }
                catch (ArgumentException)
                {
                    throw new HttpException((int)HttpStatusCode.NotAcceptable, String.Format(CultureInfo.CurrentCulture, MvcResources.Resources_UnsupportedFormat, this.ContentType));
                }
            }
            XmlWriterSettings settings = new XmlWriterSettings { Encoding = encoding };
            this.ContentType.CharSet = settings.Encoding.HeaderName;
            context.HttpContext.Response.ContentType = this.ContentType.ToString();
            using (XmlWriter writer = XmlWriter.Create(context.HttpContext.Response.OutputStream, settings))
            {
                this.Feed.GetAtom10Formatter().WriteTo(writer);
                writer.Flush();
            }
        }
    }
}
