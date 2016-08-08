// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http.Formatting;

namespace System.Web.Http.ApiExplorer
{
    public class ItemFormatter : BufferedMediaTypeFormatter
    {
        public override bool CanReadType(Type type)
        {
            return typeof(System.Web.Http.ApiExplorer.ItemController.Item).IsAssignableFrom(type);
        }

        public override bool CanWriteType(Type type)
        {
            return typeof(System.Web.Http.ApiExplorer.ItemController.Item).IsAssignableFrom(type);
        }

        public override object ReadFromStream(Type type, IO.Stream stream, Net.Http.HttpContent content, IFormatterLogger formatterLogger)
        {
            return base.ReadFromStream(type, stream, content, formatterLogger);
        }

        public override void WriteToStream(Type type, object value, IO.Stream stream, Net.Http.HttpContent content)
        {
            base.WriteToStream(type, value, stream, content);
        }
    }
}
