// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;

namespace System.Web.Http.ModelBinding
{
    // Supports JQuery schema on FormURL. 
    public class JQueryMvcFormUrlEncodedFormatter : FormUrlEncodedMediaTypeFormatter
    {
        private readonly HttpConfiguration _configuration = null;

        public JQueryMvcFormUrlEncodedFormatter()
        {
        }

        public JQueryMvcFormUrlEncodedFormatter(HttpConfiguration config)
        {
            _configuration = config;
        }

        public override bool CanReadType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            return true;
        }

        public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (readStream == null)
            {
                throw new ArgumentNullException("readStream");
            }

            // For simple types, defer to base class
            if (base.CanReadType(type))
            {
                return base.ReadFromStreamAsync(type, readStream, content, formatterLogger);
            }

            return ReadFromStreamAsyncCore(type, readStream, content, formatterLogger);
        }

        private async Task<object> ReadFromStreamAsyncCore(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
        {
            object obj = await base.ReadFromStreamAsync(typeof(FormDataCollection), readStream, content, formatterLogger);
            FormDataCollection fd = (FormDataCollection)obj;

            try
            {
                return fd.ReadAs(type, String.Empty, RequiredMemberSelector, formatterLogger, _configuration);
            }
            catch (Exception e)
            {
                if (formatterLogger == null)
                {
                    throw;
                }
                formatterLogger.LogError(String.Empty, e);
                return GetDefaultValueForType(type);
            }
        }
    }
}
