// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Net;
using System.Net.Mime;
using System.Runtime.Serialization;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using Microsoft.Web.Mvc.Properties;

namespace Microsoft.Web.Mvc.Resources
{
    /// <summary>
    /// An ActionResult that can render an object to the xml format using the DataContractSerializer
    /// </summary>
    public class DataContractXmlActionResult : ActionResult
    {
        private ContentType contentType;
        private object data;

        /// <summary>
        /// The content type of the response defaults to application/xml
        /// </summary>
        public DataContractXmlActionResult(object data)
            : this(data, new ContentType("application/xml"))
        {
        }

        public DataContractXmlActionResult(object data, ContentType contentType)
        {
            this.data = data;
            this.contentType = contentType;
        }

        public ContentType ContentType
        {
            get { return this.contentType; }
        }

        public object Data
        {
            get { return this.data; }
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
            DataContractSerializer dcs = new DataContractSerializer(this.Data.GetType());
            this.ContentType.CharSet = settings.Encoding.HeaderName;
            context.HttpContext.Response.ContentType = this.ContentType.ToString();
            using (XmlWriter writer = XmlWriter.Create(context.HttpContext.Response.OutputStream, settings))
            {
                dcs.WriteObject(writer, this.Data);
                writer.Flush();
            }
        }
    }
}
