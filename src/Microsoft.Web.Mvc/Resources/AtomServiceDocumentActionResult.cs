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
    /// An ActionResult that can render a ServiceDocument to the Atom 1.0 ServiceDocument format
    /// </summary>
    public class AtomServiceDocumentActionResult : ActionResult
    {
        private ContentType contentType;
        private ServiceDocument document;

        /// <summary>
        /// The content type defaults to application/atomsvc+xml
        /// </summary>
        /// <param name="document"></param>
        public AtomServiceDocumentActionResult(ServiceDocument document)
            : this(document, new ContentType("application/atomsvc+xml"))
        {
        }

        public AtomServiceDocumentActionResult(ServiceDocument document, ContentType contentType)
        {
            if (document == null)
            {
                throw new ArgumentNullException("document");
            }
            if (contentType == null)
            {
                throw new ArgumentNullException("contentType");
            }
            this.document = document;
            this.contentType = contentType;
        }

        public ContentType ContentType
        {
            get { return this.contentType; }
        }

        public ServiceDocument Document
        {
            get { return this.document; }
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
                this.Document.GetFormatter().WriteTo(writer);
                writer.Flush();
            }
        }
    }
}
