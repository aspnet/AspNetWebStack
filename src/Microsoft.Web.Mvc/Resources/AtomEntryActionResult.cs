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
    /// An ActionResult that can render a SyndicationItem to the Atom 1.0 entry format
    /// </summary>
    public class AtomEntryActionResult : ActionResult
    {
        private ContentType contentType;
        private SyndicationItem item;

        /// <summary>
        /// The content type defaults to application/atom+xml; type=entry
        /// </summary>
        /// <param name="item"></param>
        public AtomEntryActionResult(SyndicationItem item)
            : this(item, new ContentType("application/atom+xml;type=entry"))
        {
        }

        public AtomEntryActionResult(SyndicationItem item, ContentType contentType)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            if (contentType == null)
            {
                throw new ArgumentNullException("contentType");
            }
            this.item = item;
            this.contentType = contentType;
        }

        public ContentType ContentType
        {
            get { return this.contentType; }
        }

        public SyndicationItem Item
        {
            get { return this.item; }
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
                this.Item.GetAtom10Formatter().WriteTo(writer);
                writer.Flush();
            }
        }
    }
}
