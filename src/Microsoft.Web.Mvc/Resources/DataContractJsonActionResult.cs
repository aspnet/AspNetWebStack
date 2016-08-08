// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Net;
using System.Net.Mime;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using Microsoft.Web.Mvc.Properties;

namespace Microsoft.Web.Mvc.Resources
{
    /// <summary>
    /// An ActionResult that can render an object to the json format using the DataContractJsonSerializer
    /// </summary>
    public class DataContractJsonActionResult : ActionResult
    {
        private ContentType contentType;
        private object data;

        /// <summary>
        /// The default content type is application/json
        /// </summary>
        /// <param name="data"></param>
        public DataContractJsonActionResult(object data)
            : this(data, new ContentType("application/json"))
        {
        }

        public DataContractJsonActionResult(object data, ContentType contentType)
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
            DataContractJsonSerializer dcs = new DataContractJsonSerializer(this.Data.GetType());
            this.ContentType.CharSet = encoding.HeaderName;
            context.HttpContext.Response.ContentType = this.ContentType.ToString();
            using (XmlWriter writer = JsonReaderWriterFactory.CreateJsonWriter(context.HttpContext.Response.OutputStream, encoding))
            {
                dcs.WriteObject(writer, this.Data);
                writer.Flush();
            }
        }
    }
}
