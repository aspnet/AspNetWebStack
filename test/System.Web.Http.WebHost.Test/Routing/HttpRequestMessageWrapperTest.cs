// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Specialized;
using System.Net.Http;
using System.Web.Http.Hosting;
using Microsoft.TestCommon;

namespace System.Web.Http.WebHost.Routing
{
    public class HttpRequestMessageWrapperTest
    {
        public static TheoryDataSet<string, string, string> AppRelativeCurrentExecutionFilePathData
        {
            get
            {
                // string virtualPathRoot, string requestUri, string expectedPath
                return new TheoryDataSet<string, string, string>
                {
                    { "/", "http://localhost/path", "~/path" },
                    { "/", "http://localhost/path.ext", "~/path.ext" },
                    { "/", "http://localhost/path.ext?query=value", "~/path.ext" },

                    { "/", "http://localhost/path/", "~/path" },
                    { "/", "http://localhost/path.ext/", "~/path.ext" },
                    { "/", "http://localhost/path.ext/?query=value", "~/path.ext" },

                    { "/path1", "http://localhost/path1/path2", "~/path2" },
                    { "/path1", "http://localhost/path1/path2.ext", "~/path2.ext" },
                    { "/path1", "http://localhost/path1/path2.ext?query=value", "~/path2.ext" },

                    { "/path1", "http://localhost/path1/path2/", "~/path2" },
                    { "/path1", "http://localhost/path1/path2.ext/", "~/path2.ext" },
                    { "/path1", "http://localhost/path1/path2.ext/?query=value", "~/path2.ext" },

                    { "/path1", "http://localhost/path1/path2/path3", "~/path2/path3" },
                    { "/path1", "http://localhost/path1/path2/path3.ext", "~/path2/path3.ext" },
                    { "/path1", "http://localhost/path1/path2/path3.ext?query=value", "~/path2/path3.ext" },

                    { "/path1", "http://localhost/path1/path2/path3/", "~/path2/path3" },
                    { "/path1", "http://localhost/path1/path2/path3.ext/", "~/path2/path3.ext" },
                    { "/path1", "http://localhost/path1/path2/path3.ext/?query=value", "~/path2/path3.ext" },

                    { "/path1", "http://localhost/PATH1/path2/path3/", "~/path2/path3" },
                    { "/path1", "http://localhost/PATH1/PATH2/path3.ext/", "~/PATH2/path3.ext" },
                    { "/path1", "http://localhost/PATH1/PATH2/PATH3.ext/?query=value", "~/PATH2/PATH3.ext" },

                    // urls should be unencoded - /path1/path2 /path3.ext instead of /path1/path2%20/path3.ext
                    { "/path1", "http://localhost/PATH1/path2 /path3/", "~/path2 /path3" },
                    { "/path1", "http://localhost/PATH1/PATH2 /path3.ext/", "~/PATH2 /path3.ext" },
                    { "/path1", "http://localhost/PATH1/PATH2 /PATH3.ext/?query=value", "~/PATH2 /PATH3.ext" },
                };
            }
        }

        public static TheoryDataSet<string, string, string> FilePathData
        {
            get
            {
                // string virtualPathRoot, string requestUri, string expectedPath
                return new TheoryDataSet<string, string, string>
                {
                    { "/", "http://localhost/path", "/path" },
                    { "/", "http://localhost/path.ext", "/path.ext" },
                    { "/", "http://localhost/path.ext?query=value", "/path.ext" },

                    { "/", "http://localhost/path/", "/path" },
                    { "/", "http://localhost/path.ext/", "/path.ext" },
                    { "/", "http://localhost/path.ext/?query=value", "/path.ext" },

                    { "/path1", "http://localhost/path1/path2", "/path1/path2" },
                    { "/path1", "http://localhost/path1/path2.ext", "/path1/path2.ext" },
                    { "/path1", "http://localhost/path1/path2.ext?query=value", "/path1/path2.ext" },

                    { "/path1", "http://localhost/path1/path2/", "/path1/path2" },
                    { "/path1", "http://localhost/path1/path2.ext/", "/path1/path2.ext" },
                    { "/path1", "http://localhost/path1/path2.ext/?query=value", "/path1/path2.ext" },

                    { "/path1", "http://localhost/path1/path2/path3", "/path1/path2/path3" },
                    { "/path1", "http://localhost/path1/path2/path3.ext", "/path1/path2/path3.ext" },
                    { "/path1", "http://localhost/path1/path2/path3.ext?query=value", "/path1/path2/path3.ext" },

                    { "/path1", "http://localhost/path1/path2/path3/", "/path1/path2/path3" },
                    { "/path1", "http://localhost/path1/path2/path3.ext/", "/path1/path2/path3.ext" },
                    { "/path1", "http://localhost/path1/path2/path3.ext/?query=value", "/path1/path2/path3.ext" },

                    // urls should be unescaped - /path1/path2 /path3.ext instead of /path1/path2%20/path3.ext
                    { "/path1", "http://localhost/path1/path2 /path3/", "/path1/path2 /path3" },
                    { "/path1", "http://localhost/path1/path2 /path3.ext/", "/path1/path2 /path3.ext" },
                    { "/path1", "http://localhost/path1/path2 /path3.ext/?query=value", "/path1/path2 /path3.ext" },
                };
            }
        }

        [Fact]
        public void Constructor_GuardClauses()
        {
            HttpRequestMessage request = new HttpRequestMessage();
            Assert.ThrowsArgumentNull(() => new HttpRequestMessageWrapper(virtualPathRoot: null, httpRequest: request), "virtualPathRoot");
            Assert.ThrowsArgumentNull(() => new HttpRequestMessageWrapper(String.Empty, httpRequest: null), "httpRequest");
        }

        [Theory]
        [InlineData("")]
        [InlineData("/")]
        [InlineData("/path")]
        [InlineData("/path1/path2")]
        public void ApplicationPath_DelegatesToHttpRequestMessage(string applicationPath)
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            HttpRequestMessageWrapper wrapper = new HttpRequestMessageWrapper(applicationPath, request);

            // Act
            string actualApplicationPath = wrapper.ApplicationPath;

            // Assert
            Assert.Equal(applicationPath, actualApplicationPath);
        }

        [Theory]
        [PropertyData("AppRelativeCurrentExecutionFilePathData")]
        public void AppRelativeCurrentExecutionFilePath_DelegatesToHttpRequestMessage(string virtualPathRoot, string requestUri, string expectedPath)
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage
            {
                RequestUri = new Uri(requestUri),
            };
            HttpRequestMessageWrapper wrapper = new HttpRequestMessageWrapper(virtualPathRoot, request);

            // Act
            string actualPath = wrapper.AppRelativeCurrentExecutionFilePath;

            // Assert
            Assert.Equal(expectedPath, actualPath);
        }

        [Theory]
        [PropertyData("FilePathData")]
        public void CurrentExecutionFilePath_DelegatesToHttpRequestMessage(string virtualPathRoot, string requestUri, string expectedPath)
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage
            {
                RequestUri = new Uri(requestUri),
            };
            HttpRequestMessageWrapper wrapper = new HttpRequestMessageWrapper(virtualPathRoot, request);

            // Act
            string actualPath = wrapper.CurrentExecutionFilePath;

            // Assert
            Assert.Equal(expectedPath, actualPath);
        }

        [Theory]
        [PropertyData("FilePathData")]
        public void FilePath_DelegatesToHttpRequestMessage(string virtualPathRoot, string requestUri, string expectedPath)
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage
            {
                RequestUri = new Uri(requestUri),
            };
            HttpRequestMessageWrapper wrapper = new HttpRequestMessageWrapper(virtualPathRoot, request);

            // Act
            string actualPath = wrapper.FilePath;

            // Assert
            Assert.Equal(expectedPath, actualPath);
        }

        [Fact]
        public void HttpMethod_DelegatesToHttpRequestMessage()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage
            {
                Method = new HttpMethod("DELETE")
            };
            HttpRequestMessageWrapper wrapper = new HttpRequestMessageWrapper("/", request);

            // Act
            string actualMethod = wrapper.HttpMethod;

            // Assert
            Assert.Equal("DELETE", actualMethod);
        }

        [Fact]
        public void RequestType_DelegatesToHttpRequestMessage()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage
            {
                Method = new HttpMethod("DELETE")
            };
            HttpRequestMessageWrapper wrapper = new HttpRequestMessageWrapper("/", request);

            // Act
            string actualRequestType = wrapper.RequestType;

            // Assert
            Assert.Equal("DELETE", actualRequestType);
        }

        [Fact]
        public void Path_DelegatesToHttpRequestMessage()
        {
            // Arrange
            Uri requestUri = new Uri("http://localhost/some/path?query=value");
            HttpRequestMessage request = new HttpRequestMessage
            {
                RequestUri = requestUri
            };
            HttpRequestMessageWrapper wrapper = new HttpRequestMessageWrapper("/", request);

            // Act
            string actualPath = wrapper.Path;

            // Assert
            Assert.Equal("/some/path", actualPath);
        }

        [Fact]
        public void Path_DelegatesToHttpRequestMessage_DoesNotEncode()
        {
            // Arrange
            Uri requestUri = new Uri("http://localhost/some /path?query=value");
            HttpRequestMessage request = new HttpRequestMessage
            {
                RequestUri = requestUri
            };
            HttpRequestMessageWrapper wrapper = new HttpRequestMessageWrapper("/", request);

            // Act
            string actualPath = wrapper.Path;

            // Assert
            Assert.Equal("/some /path", actualPath);
        }

        [Theory]
        [InlineData("http://www.example.com")]
        [InlineData("http://www.example.com?query")]
        [InlineData("http://www.example.com?query=value")]
        public void QueryString_DelegatesToHttpRequestMessage(string requestUri)
        {
            // Arrange
            Uri reqUri = new Uri(requestUri);
            NameValueCollection expectedQueryString = reqUri.ParseQueryString();
            HttpRequestMessage request = new HttpRequestMessage
            {
                RequestUri = reqUri
            };
            HttpRequestMessageWrapper wrapper = new HttpRequestMessageWrapper("/", request);

            // Act
            NameValueCollection actualQuery = wrapper.QueryString;

            // Assert
            Assert.Equal(expectedQueryString.ToString(), actualQuery.ToString());
        }

        [Fact]
        public void RawUrl_DelegatesToHttpRequestMessage()
        {
            // Arrange
            Uri requestUri = new Uri("http://localhost/some/path?query=value");
            HttpRequestMessage request = new HttpRequestMessage
            {
                RequestUri = requestUri
            };
            HttpRequestMessageWrapper wrapper = new HttpRequestMessageWrapper("/", request);

            // Act
            string actualRawUrl = wrapper.RawUrl;

            // Assert
            Assert.Equal("/some/path?query=value", actualRawUrl);
        }

        [Fact]
        public void RawUrl_DelegatesToHttpRequestMessage_DoesNotEncode()
        {
            // Arrange
            Uri requestUri = new Uri("http://localhost/some /path?query=value");
            HttpRequestMessage request = new HttpRequestMessage
            {
                RequestUri = requestUri
            };
            HttpRequestMessageWrapper wrapper = new HttpRequestMessageWrapper("/", request);

            // Act
            string actualRawUrl = wrapper.RawUrl;

            // Assert
            Assert.Equal("/some /path?query=value", actualRawUrl);
        }

        [Fact]
        public void IsLocal_Call_To_HttpRequestMessageExtension_Method()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            request.SetIsLocal(new Lazy<bool>(() => true));
            HttpRequestMessageWrapper wrapper = new HttpRequestMessageWrapper("/", request);

            // Act
            bool isLocal = wrapper.IsLocal;

            // Assert
            Assert.True(isLocal);
        }
    }
}
