// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text;
using System.Web.Mvc.Properties;
using System.Web.Script.Serialization;

namespace System.Web.Mvc
{
    public class JsonResult : ActionResult
    {
        public JsonResult()
        {
            JsonRequestBehavior = JsonRequestBehavior.DenyGet;
        }

        public Encoding ContentEncoding { get; set; }

        public string ContentType { get; set; }

        public object Data { get; set; }

        public JsonRequestBehavior JsonRequestBehavior { get; set; }

        /// <summary>
        /// When set MaxJsonLength passed to the JavaScriptSerializer.
        /// </summary>
        public int? MaxJsonLength { get; set; }

        /// <summary>
        /// When set RecursionLimit passed to the JavaScriptSerializer.
        /// </summary>
        public int? RecursionLimit { get; set; }

        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if (JsonRequestBehavior == JsonRequestBehavior.DenyGet &&
                String.Equals(context.HttpContext.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(MvcResources.JsonRequest_GetNotAllowed);
            }

            HttpResponseBase response = context.HttpContext.Response;

            if (!String.IsNullOrEmpty(ContentType))
            {
                response.ContentType = ContentType;
            }
            else
            {
                response.ContentType = "application/json";
            }
            if (ContentEncoding != null)
            {
                response.ContentEncoding = ContentEncoding;
            }
            if (Data != null)
            {
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                if (MaxJsonLength.HasValue)
                {
                    serializer.MaxJsonLength = MaxJsonLength.Value;
                }
                if (RecursionLimit.HasValue)
                {
                    serializer.RecursionLimit = RecursionLimit.Value;
                }
                response.Write(serializer.Serialize(Data));
            }
        }
    }
}
