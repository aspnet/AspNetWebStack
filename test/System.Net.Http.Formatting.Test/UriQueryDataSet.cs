// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Net.Http
{
    public class UriQueryTestData
    {
        public static TheoryDataSet<string, string, string> UriQueryData
        {
            get
            {
                return new TheoryDataSet<string, string, string>
                {
                    { "=", "", "" },
                    { "N=", "N", "" },
                    { "=N", "", "N" },
                    { "N=V", "N", "V" },
                    { "%26=%26", "&", "&" },
                    { "%3D=%3D", "=", "=" },
                    { "N=A%2BC", "N", "A+C" },
                    { "N=100%25AA%21", "N", "100%AA!"},
                    { "N=%7E%21%40%23%24%25%5E%26%2A%28%29_%2B","N","~!@#$%^&*()_+"},
                    { "N=%601234567890-%3D", "N", "`1234567890-="},
                    { "N=%60%31%32%33%34%35%36%37%38%39%30%2D%3D","N", "`1234567890-="},
                    { "N=%E6%BF%80%E5%85%89%E9%80%99%E5%85%A9%E5%80%8B%E5%AD%97%E6%98%AF%E7%94%9A%E9%BA%BC%E6%84%8F%E6%80%9D", "N", "激光這兩個字是甚麼意思" },
                    { "N=%C3%A6%C3%B8%C3%A5","N","æøå"},
                    { "N=%C3%A6+%C3%B8+%C3%A5","N","æ ø å"},
                };
            }
        }
    }
}