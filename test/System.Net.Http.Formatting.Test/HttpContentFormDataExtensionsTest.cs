// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Specialized;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.TestCommon;
#if NETFX_CORE
using NameValueCollection = System.Net.Http.Formatting.HttpValueCollection;
#endif

namespace System.Net.Http
{
    public class HttpContentFormDataExtensionsTest
    {
        public static TheoryDataSet<string> FormDataContentTypes
        {
            get
            {
                return new TheoryDataSet<string>
                {
                    "application/x-www-form-urlencoded",
                    "APPLICATION/x-www-form-urlencoded",
                    "application/X-WWW-FORM-URLENCODED",
                    "application/x-www-form-urlencoded; charset=utf-8",
                    "application/x-www-form-urlencoded; parameter=value",
                };
            }
        }

        public static TheoryDataSet<string> NonFormDataContentTypes
        {
            get
            {
                return new TheoryDataSet<string>
                {
                    "application/xml",
                    "APPLICATION/json",
                    "text/xml",
                    "text/xml; charset=utf-8",
                    "text/xml; parameter=value",
                };
            }
        }

        public static TheoryDataSet<string> FormData
        {
            get
            {
                return new TheoryDataSet<string>
                {
                    "a=b",
                    "a+c=d+e",
                    "n1=v1&n2=v2",
                    "n1=v1a+v1b&n2=v2a+v2b",
                };
            }
        }

        public static TheoryDataSet<string> IrregularFormData
        {
            get
            {
                return new TheoryDataSet<string>
                {
                    "?data",
                    Char.ConvertFromUtf32(0x0D),
                    "Hello World",
                    "<string>Hello World</string>",
                    "{ \"Message\" : \"Hello World\"",
                };
            }
        }

        [Fact]
        public void IsFormData_ThrowsOnNull()
        {
            Assert.ThrowsArgumentNull(() => HttpContentFormDataExtensions.IsFormData(null), "content");
        }

        [Fact]
        public void IsFormData_HandlesNullContentType()
        {
            HttpContent content = new StringContent(String.Empty);
            content.Headers.ContentType = null;
            Assert.False(content.IsFormData());
        }

        [Theory]
        [PropertyData("FormDataContentTypes")]
        public void IsFormData_AcceptsFormDataMediaTypes(string mediaType)
        {
            HttpContent content = new StringContent(String.Empty);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse(mediaType);
            Assert.True(content.IsFormData());
        }

        [Theory]
        [PropertyData("NonFormDataContentTypes")]
        public void IsFormData_RejectsNonFormDataMediaTypes(string mediaType)
        {
            HttpContent content = new StringContent(String.Empty);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse(mediaType);
            Assert.False(content.IsFormData());
        }

        [Fact]
        public void ReadAsFormDataAsync_ThrowsOnNull()
        {
            Assert.ThrowsArgumentNull(() => HttpContentFormDataExtensions.ReadAsFormDataAsync(null), "content");
        }

        [Fact]
        public async Task ReadAsFromDataAsync_PassesCancellationToken()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.Cancel();
            HttpContent content = new StringContent("");
            content.Headers.ContentType = MediaTypeConstants.ApplicationFormUrlEncodedMediaType;

            await Assert.ThrowsAsync<OperationCanceledException>(() => content.ReadAsFormDataAsync(cts.Token));
        }

        [Theory]
        [PropertyData("FormData")]
        public async Task ReadAsFormDataAsync_HandlesFormData(string formData)
        {
            // Arrange
            HttpContent content = new StringContent(formData);
            content.Headers.ContentType = MediaTypeConstants.ApplicationFormUrlEncodedMediaType;

            // Act
            NameValueCollection data = await content.ReadAsFormDataAsync();

            // Assert
            Assert.Equal(formData, data.ToString());
        }

        [Fact]
        public async Task ReadAsFormDataAsync_HandlesFormData_Encoded()
        {
            // Arrange
            string formData = "N=%c3%a6%c3%b8%c3%a5";
            HttpContent content = new StringContent(formData);
            content.Headers.ContentType = MediaTypeConstants.ApplicationFormUrlEncodedMediaType;

            // Act
            NameValueCollection data = await content.ReadAsFormDataAsync();

            // Assert
            Assert.Equal(formData, data.ToString(), ignoreCase: true);
        }

        [Theory]
        [PropertyData("IrregularFormData")]
        public async Task ReadAsFormDataAsync_HandlesIrregularFormData(string irregularFormData)
        {
            // Arrange
            HttpContent content = new StringContent(irregularFormData);
            content.Headers.ContentType = MediaTypeConstants.ApplicationFormUrlEncodedMediaType;

            // Act
            NameValueCollection data = await content.ReadAsFormDataAsync();

            // Assert
            Assert.Single(data);
            Assert.NotNull(data[irregularFormData]);
        }

        [Fact]
        public Task ReadAsFormDataAsync_HandlesNonFormData()
        {
            HttpContent content = new StringContent("{}", Encoding.UTF8, "test/unknown");
            return Assert.ThrowsAsync<UnsupportedMediaTypeException>(() => content.ReadAsFormDataAsync());
        }
    }
}
