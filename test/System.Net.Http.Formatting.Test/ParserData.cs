// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http.Headers;
using Microsoft.TestCommon;

namespace System.Net.Http
{
    internal static class ParserData
    {
        public const int MinHeaderSize = 2;

        public const int MinMessageSize = 10;

        public const int MinRequestLineSize = 14;

        public const int MinStatusLineSize = 15;

        public const int MinBufferSize = 256;

        public static TheoryDataSet<Version> Versions
        {
            get
            {
                return new TheoryDataSet<Version>
                {
                    Version.Parse("1.0"),
                    Version.Parse("1.1"),
                    Version.Parse("1.2"),
                    Version.Parse("2.0"),
                    Version.Parse("10.0"),
                    Version.Parse("1.15"),
                };
            }
        }

        public static TheoryDataSet<string> InvalidVersions
        {
            get
            {
                return new TheoryDataSet<string>
                {
                    "",
                    "http/1.1",
                    "HTTP/a.1",
                    "HTTP/1.a",
                    "HTTP 1.1",
                    "HTTP\t1.1",
                    "HTTP 1 1",
                    "\0",
                    "HTTP\01.1",
                    "HTTP/4294967295.4294967295",
                    "æææøøøååå",
                    "HTTP/æææøøøååå",
                    "いくつかのテキスト",
                    "HTTP/いくつかのテキスト",
                };
            }
        }

        public static TheoryDataSet<string> InvalidMethods
        {
            get
            {
                return new TheoryDataSet<string>
                {
                    "",
                    "G\tT",
                    "G E T",
                    "\0",
                    "G\0T",
                    "GET\n",
                    "æææøøøååå",
                    "いくつかのテキスト",
                };
            }
        }

        public static TheoryDataSet<string> ValidReasonPhrases
        {
            get
            {
                return new TheoryDataSet<string>
                {
                    "",
                    "Ok",
                    "public Server Error",
                    "r e a s o n",
                    "reason ",
                    " reason ",
                };
            }
        }

        public static TheoryDataSet<string> InvalidReasonPhrases
        {
            get
            {
                return new TheoryDataSet<string>
                {
                    "\0",
                    "\t",
                    "reason\n",
                    "æææøøøååå",
                    "いくつかのテキスト",
                };
            }
        }

        // This deliberately only checks for syntactic boundaries of the URI, not its content
        public static TheoryDataSet<string> InvalidRequestUris
        {
            get
            {
                return new TheoryDataSet<string>
                {
                    "",
                    "p a t h",
                    "path ",
                    " path ",
                    "æææø ø øååå",
                    "いくつか の テキスト"
                };
            }
        }

        public static TheoryDataSet<string> InvalidStatusCodes
        {
            get
            {
                return new TheoryDataSet<string>
                {
                    "0",
                    "99",
                    "1a1",
                    "abc",
                    "1001",
                    "2000",
                    Int32.MinValue.ToString(),
                    Int32.MaxValue.ToString(),
                    "æææøøøååå",
                    "いくつかのテキスト"
                };
            }
        }

        public static readonly Dictionary<string, string> ValidHeaders = new Dictionary<string, string>
        {
            { "N0", "V0"},
            { "N1", "V1"},
            { "N2", "V2"},
            { "N3", "V3"},
            { "N4", "V4"},
            { "N5", "V5"},
            { "N6", "V6"},
            { "N7", "V7"},
            { "N8", "V8"},
            { "N9", "V9"},
        };

        public static readonly string HttpMethod = "TEG";
        public static readonly HttpStatusCode HttpStatus = HttpStatusCode.Created;
        public static readonly string HttpReasonPhrase = "ReasonPhrase";
        public static readonly string HttpHostName = "example.com";
        public static readonly int HttpHostPort = 1234;
        public static readonly string HttpMessageEntity = "1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890";

        public static readonly Uri HttpRequestUri = new Uri("http://" + HttpHostName + "/some/path");
        public static readonly Uri HttpRequestUriWithPortAndQuery = new Uri("http://" + HttpHostName + ":" + HttpHostPort + "/some/path?%C3%A6%C3%B8%C3%A5");
        public static readonly Uri HttpsRequestUri = new Uri("https://" + HttpHostName + "/some/path");

        public static readonly string TextContentType = "text/plain; charset=utf-8";

        public static readonly MediaTypeHeaderValue HttpRequestMediaType = MediaTypeHeaderValue.Parse("application/http; msgtype=request");
        public static readonly MediaTypeHeaderValue HttpResponseMediaType = MediaTypeHeaderValue.Parse("application/http; msgtype=response");

        public static readonly string HttpRequest =
            HttpMethod +
            " /some/path HTTP/1.2\r\nHost: " +
            HttpHostName +
            "\r\nN1: V1a, V1b, V1c, V1d, V1e\r\nN2: V2\r\n\r\n";

        public static readonly string HttpRequestWithHost =
            HttpMethod +
            " /some/path HTTP/1.2\r\n" +
            "N1: V1a, V1b, V1c, V1d, V1e\r\nN2: V2\r\nHost: " +
            HttpHostName + "\r\n\r\n";

        public static readonly string HttpRequestWithPortAndQuery =
            HttpMethod +
            " /some/path?%C3%A6%C3%B8%C3%A5 HTTP/1.2\r\nHost: " +
            HttpHostName + ":" + HttpHostPort.ToString() +
            "\r\nN1: V1a, V1b, V1c, V1d, V1e\r\nN2: V2\r\n\r\n";

        public static readonly string HttpResponse =
            "HTTP/1.2 " +
            ((int)HttpStatus).ToString() +
            " " +
            HttpReasonPhrase +
            "\r\nN1: V1a, V1b, V1c, V1d, V1e\r\nN2: V2\r\n\r\n";

        public static readonly string HttpRequestWithEntity =
            HttpMethod +
            " /some/path HTTP/1.2\r\nHost: " +
            HttpHostName +
            "\r\nN1: V1a, V1b, V1c, V1d, V1e\r\nN2: V2\r\nContent-Type: " +
            TextContentType +
            "\r\n\r\n" +
            HttpMessageEntity;

        public static readonly string HttpResponseWithEntity =
            "HTTP/1.2 " +
            ((int)HttpStatus).ToString() +
            " " +
            HttpReasonPhrase +
            "\r\nN1: V1a, V1b, V1c, V1d, V1e\r\nN2: V2\r\nContent-Type: " +
            TextContentType +
            "\r\n\r\n" +
            HttpMessageEntity;
    }
}