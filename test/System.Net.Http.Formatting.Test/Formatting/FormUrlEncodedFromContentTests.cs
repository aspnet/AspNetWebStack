// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http.Formatting.Parsers;
using System.Text;
using System.Web.Http;
using Microsoft.TestCommon;
using Newtonsoft.Json.Linq;

namespace System.Net.Http.Formatting
{
    public class FormUrlEncodedJsonFromContentTests
    {
        public static TheoryDataSet<string, string> TestEncodedNameTestData
        {
            get
            {
                // string: encoded string: result
                return new TheoryDataSet<string, string>
                {
                    { "some+thing=10", @"{""some thing"":""10""}" },
                    { "%E5%B8%A6%E4%B8%89%E4%B8%AA%E8%A1%A8=bar", @"{""带三个表"":""bar""}" },
                    { "some+thing=10&%E5%B8%A6%E4%B8%89%E4%B8%AA%E8%A1%A8=bar", @"{""some thing"":""10"",""带三个表"":""bar""}"},
                    { "a[0\r\n][b]=1", "{\"a\":{\"0\\r\\n\":{\"b\":\"1\"}}}" },
                    { "a[0%0d\n][b]=1", "{\"a\":{\"0\\r\\n\":{\"b\":\"1\"}}}" },
                    { "a[0%0d%0a][b]=1", "{\"a\":{\"0\\r\\n\":{\"b\":\"1\"}}}" },
                    { "a[0\0]=1", "{\"a\":{\"0\\u0000\":\"1\"}}" },
                    { "a[0%00]=1", "{\"a\":{\"0\\u0000\":\"1\"}}" },
                    { "a[\00]=1", "{\"a\":{\"\\u00000\":\"1\"}}" },
                    { "a[%000]=1", "{\"a\":{\"\\u00000\":\"1\"}}" },
                };
            }
        }

        public static TheoryDataSet<string, string> TestObjectTestData
        {
            get
            {
                // string: encoded string: result
                return new TheoryDataSet<string, string>
                {
                    { "a[]=4&a[]=5&b[x][]=7&b[y]=8&b[z][]=9&b[z][]=true&b[z][]=undefined&b[z][]=&c=1&f=",
                     @"{""a"":[""4"",""5""],""b"":{""x"":[""7""],""y"":""8"",""z"":[""9"",""true"",""undefined"",""""]},""c"":""1"",""f"":""""}" },

                    { "customer[Name]=Pete&customer[Address]=Redmond&customer[Age][0][]=23&customer[Age][0][]=24&customer[Age][1][]=25&" +
                      "customer[Age][1][]=26&customer[Phones][]=425+888+1111&customer[Phones][]=425+345+7777&customer[Phones][]=425+888+4564&" +
                      "customer[EnrolmentDate]=%22%5C%2FDate(1276562539537)%5C%2F%22&role=NewRole&changeDate=3&count=15",
                      @"{""customer"":{""Name"":""Pete"",""Address"":""Redmond"",""Age"":[[""23"",""24""],[""25"",""26""]]," +
                      @"""Phones"":[""425 888 1111"",""425 345 7777"",""425 888 4564""],""EnrolmentDate"":""\""\\/Date(1276562539537)\\/\""""},""role"":""NewRole"",""changeDate"":""3"",""count"":""15""}" },

                    { "customers[0][Name]=Pete2&customers[0][Address]=Redmond2&customers[0][Age][0][]=23&customers[0][Age][0][]=24&" +
                      "customers[0][Age][1][]=25&customers[0][Age][1][]=26&customers[0][Phones][]=425+888+1111&customers[0][Phones][]=425+345+7777&" +
                      "customers[0][Phones][]=425+888+4564&customers[0][EnrolmentDate]=%22%5C%2FDate(1276634840700)%5C%2F%22&customers[1][Name]=Pete3&" +
                      "customers[1][Address]=Redmond3&customers[1][Age][0][]=23&customers[1][Age][0][]=24&customers[1][Age][1][]=25&customers[1][Age][1][]=26&" +
                      "customers[1][Phones][]=425+888+1111&customers[1][Phones][]=425+345+7777&customers[1][Phones][]=425+888+4564&customers[1][EnrolmentDate]=%22%5C%2FDate(1276634840700)%5C%2F%22",
                      @"{""customers"":[{""Name"":""Pete2"",""Address"":""Redmond2"",""Age"":[[""23"",""24""],[""25"",""26""]]," +
                      @"""Phones"":[""425 888 1111"",""425 345 7777"",""425 888 4564""],""EnrolmentDate"":""\""\\/Date(1276634840700)\\/\""""}," +
                      @"{""Name"":""Pete3"",""Address"":""Redmond3"",""Age"":[[""23"",""24""],[""25"",""26""]],""Phones"":[""425 888 1111"",""425 345 7777"",""425 888 4564""],""EnrolmentDate"":""\""\\/Date(1276634840700)\\/\""""}]}" },

                    { "ab%5B%5D=hello", @"{""ab"":[""hello""]}" },

                    { "123=hello", @"{""123"":""hello""}" },

                    { "a%5B%5D=1&a", @"{""a"":[""1"",""""]}" },

                    { "a=1&a", @"{""a"":[""1"",""""]}" }
                };
            }
        }

        [Theory,
            InlineData("abc", "{\"abc\":\"\"}"),
            InlineData("%2eabc%2e", "{\".abc.\":\"\"}"),
            InlineData("", "{}"),
            InlineData("a=1", "{\"a\":\"1\"}")]
        public void SimpleStringsTest(string encoded, string expectedResult)
        {
            ValidateFormUrlEncoded(encoded, expectedResult);

        }

        [Theory,
            InlineData("a=2", "{\"a\":\"2\"}"),
            InlineData("b=true", "{\"b\":\"true\"}"),
            InlineData("c=hello", "{\"c\":\"hello\"}"),
            InlineData("d=", "{\"d\":\"\"}"),
            InlineData("e=null", "{\"e\":\"null\"}")]
        public void SimpleObjectsTest(string encoded, string expectedResult)
        {
            ValidateFormUrlEncoded(encoded, expectedResult);

        }

        [Fact]
        public void LegacyArraysTest()
        {
            ValidateFormUrlEncoded("a=1&a=hello&a=333", "{\"a\":[\"1\",\"hello\",\"333\"]}");

            // Only valid in shallow serialization
            ParseInvalidFormUrlEncoded("a[z]=2&a[z]=3");
        }

        [Theory,
            InlineData("a[]=1&a[]=hello&a[]=333", "{\"a\":[\"1\",\"hello\",\"333\"]}"),
            InlineData("a[b][]=1&a[b][]=hello&a[b][]=333", "{\"a\":{\"b\":[\"1\",\"hello\",\"333\"]}}"),
            InlineData("a[]=", "{\"a\":[\"\"]}"),
            InlineData("a%5B%5D=2", @"{""a"":[""2""]}"),
            InlineData("a[x][0]=1&a[x][]=2", @"{""a"":{""x"":[""1"",""2""]}}")]
        public void ArraysTest(string encoded, string expectedResult)
        {
            ValidateFormUrlEncoded(encoded, expectedResult);
        }

        [Theory,
            InlineData("a[0][]=1&a[0][]=hello&a[1][]=333", "{\"a\":[[\"1\",\"hello\"],[\"333\"]]}"),
            InlineData("a[b][0][]=1&a[b][1][]=hello&a[b][1][]=333", "{\"a\":{\"b\":[[\"1\"],[\"hello\",\"333\"]]}}"),
            InlineData("a[0][0][0][]=1", "{\"a\":[[[[\"1\"]]]]}")]
        public void MultidimensionalArraysTest(string encoded, string expectedResult)
        {
            ValidateFormUrlEncoded(encoded, expectedResult);
        }

        [Theory,
            InlineData("a[0][]=hello&a[2][]=333", "{\"a\":{\"0\":[\"hello\"],\"2\":[\"333\"]}}"),
            InlineData("a[0]=hello", "{\"a\":[\"hello\"]}"),
            InlineData("a[1][]=hello", "{\"a\":{\"1\":[\"hello\"]}}"),
            InlineData("a[1][0]=hello", "{\"a\":{\"1\":[\"hello\"]}}")]
        public void SparseArraysTest(string encoded, string expectedResult)
        {
            ValidateFormUrlEncoded(encoded, expectedResult);
        }

        [Theory,
            InlineData("b[]=2&b[1][c]=d", "{\"b\":[\"2\",{\"c\":\"d\"}]}")]
        public void ArraysWithMixedMembers(string encoded, string expectedResult)
        {
            ValidateFormUrlEncoded(encoded, expectedResult);
        }

        [Theory,
            InlineData("=3", "{\"\":\"3\"}"),
            InlineData("a=1&=3", "{\"a\":\"1\",\"\":\"3\"}"),
            InlineData("=3&b=2", "{\"\":\"3\",\"b\":\"2\"}")]
        public void EmptyKeyTest(string encoded, string expectedResult)
        {
            ValidateFormUrlEncoded(encoded, expectedResult);
        }

        [Theory,
            InlineData("a[b]=1&a=2"),
            InlineData("a[b]=1&a[b][]=2"),
            InlineData("a[x][]=1&a[x][0]=2"),
            InlineData("a=2&a[b]=1"),
            InlineData("[]=1"),
            InlineData("a[][]=0"),
            InlineData("a[][x]=0"),
            InlineData("a&a[b]=1")]
        public void InvalidObjectGraphsTest(string encoded)
        {
            ParseInvalidFormUrlEncoded(encoded);
        }

        [Theory,
            InlineData("a[b=2"),
            InlineData("a[[b]=2"),
            InlineData("a[b]]=2")]
        public void InvalidFormUrlEncodingTest(string encoded)
        {
            ParseInvalidFormUrlEncoded(encoded);
        }

        /// <summary>
        /// Tests for parsing form-urlencoded data originated from JS primitives.
        /// </summary>
        [Theory,
            InlineData("abc", @"{""abc"":""""}"),
            InlineData("123", @"{""123"":""""}"),
            InlineData("true", @"{""true"":""""}"),
            InlineData("", "{}"),
            InlineData("%2fabc%2f", @"{""/abc/"":""""}")]
        public void TestJValue(string encoded, string expectedResult)
        {
            ValidateFormUrlEncoded(encoded, expectedResult);
        }

        /// <summary>
        /// Negative tests for parsing form-urlencoded data originated from JS primitives.
        /// </summary>
        [Theory,
            InlineData("a[b]=1&a=2"),
            InlineData("a=2&a[b]=1"),
            InlineData("[]=1")]
        public void TestJValueNegative(string encoded)
        {
            ParseInvalidFormUrlEncoded(encoded);
        }

        /// <summary>
        /// Tests for parsing form-urlencoded data originated from JS objects.
        /// </summary>
        [Theory,
            InlineData("a=NaN", @"{""a"":""NaN""}"),
            InlineData("a=false", @"{""a"":""false""}"),
            InlineData("a=foo", @"{""a"":""foo""}"),
            InlineData("1=1", "{\"1\":\"1\"}")]
        public void TestObjects(string encoded, string expectedResult)
        {
            ValidateFormUrlEncoded(encoded, expectedResult);
        }

        /// <summary>
        /// Tests for parsing form-urlencoded data originated from JS arrays.
        /// </summary>
        [Theory,
            InlineData("a[]=2", @"{""a"":[""2""]}"),
            InlineData("a[]=", @"{""a"":[""""]}"),
            InlineData("a[0][0][]=1", @"{""a"":[[[""1""]]]}"),
            InlineData("z[]=9&z[]=true&z[]=undefined&z[]=", @"{""z"":[""9"",""true"",""undefined"",""""]}"),
            InlineData("z[]=9&z[]=true&z[]=undefined&z[]=null", @"{""z"":[""9"",""true"",""undefined"",""null""]}"),
            InlineData("z[0][]=9&z[0][]=true&z[1][]=undefined&z[1][]=null", @"{""z"":[[""9"",""true""],[""undefined"",""null""]]}"),
            InlineData("a[0][x]=2", @"{""a"":[{""x"":""2""}]}"),
            InlineData("a%5B%5D=2", @"{""a"":[""2""]}"),
            InlineData("a%5B%5D=", @"{""a"":[""""]}"),
            InlineData("z%5B%5D=9&z%5B%5D=true&z%5B%5D=undefined&z%5B%5D=", @"{""z"":[""9"",""true"",""undefined"",""""]}"),
            InlineData("z%5B%5D=9&z%5B%5D=true&z%5B%5D=undefined&z%5B%5D=null", @"{""z"":[""9"",""true"",""undefined"",""null""]}"),
            InlineData("z%5B0%5D%5B%5D=9&z%5B0%5D%5B%5D=true&z%5B1%5D%5B%5D=undefined&z%5B1%5D%5B%5D=null", @"{""z"":[[""9"",""true""],[""undefined"",""null""]]}")]
        public void TestArray(string encoded, string expectedResult)
        {
            ValidateFormUrlEncoded(encoded, expectedResult);
        }

        /// <summary>
        /// Tests for parsing form-urlencoded data originated from JS arrays, using the jQuery 1.3 format (no []'s).
        /// </summary>
        [Theory,
            InlineData("z=9&z=true&z=undefined&z=", @"{""z"":[""9"",""true"",""undefined"",""""]}"),
            InlineData("z=9&z=true&z=undefined&z=null", @"{""z"":[""9"",""true"",""undefined"",""null""]}"),
            InlineData("z=9&z=true&z=undefined&z=null&a=hello", @"{""z"":[""9"",""true"",""undefined"",""null""],""a"":""hello""}")]
        public void TestArrayCompat(string encoded, string expectedResult)
        {
            ValidateFormUrlEncoded(encoded, expectedResult);
        }

        /// <summary>
        /// Negative tests for parsing form-urlencoded data originated from JS arrays.
        /// </summary>
        [Theory,
            InlineData("a[z]=2&a[z]=3")]
        public void TestArrayCompatNegative(string encoded)
        {
            ParseInvalidFormUrlEncoded(encoded);
        }

        /// <summary>
        /// Tests for form-urlencoded data originated from sparse JS arrays.
        /// </summary>
        [Theory,
            InlineData("a[2]=hello", @"{""a"":{""2"":""hello""}}"),
            InlineData("a[x][0]=2", @"{""a"":{""x"":[""2""]}}"),
            InlineData("a[x][1]=2", @"{""a"":{""x"":{""1"":""2""}}}"),
            InlineData("a[x][0]=0&a[x][1]=1", @"{""a"":{""x"":[""0"",""1""]}}"),
            InlineData("a[0][0][0]=hello&a[1][0][0][0][]=hello", @"{""a"":[[[""hello""]],[[[[""hello""]]]]]}"),
            InlineData("a[0][0][0]=hello&a[1][0][0][0]=hello", @"{""a"":[[[""hello""]],[[[""hello""]]]]}"),
            InlineData("a[1][0][]=1", @"{""a"":{""1"":[[""1""]]}}"),
            InlineData("a[1][1][]=1", @"{""a"":{""1"":{""1"":[""1""]}}}"),
            InlineData("a[1][1][0]=1", @"{""a"":{""1"":{""1"":[""1""]}}}"),
            InlineData("a[0][]=2&a[0][]=3&a[2][]=1", "{\"a\":{\"0\":[\"2\",\"3\"],\"2\":[\"1\"]}}"),
            InlineData("a[x][]=1&a[x][1]=2", @"{""a"":{""x"":[""1"",""2""]}}"),
            InlineData("a[x][0]=1&a[x][]=2", @"{""a"":{""x"":[""1"",""2""]}}")]
        public void TestArraySparse(string encoded, string expectedResult)
        {
            ValidateFormUrlEncoded(encoded, expectedResult);
        }

        /// <summary>
        /// Negative tests for parsing form-urlencoded arrays.
        /// </summary>
        [Theory,
            InlineData("a[x]=2&a[x][]=3"),
            InlineData("a[]=1&a[0][]=2"),
            InlineData("a[]=1&a[0][0][]=2"),
            InlineData("a[x][]=1&a[x][0]=2"),
            InlineData("a[][]=0"),
            InlineData("a[][x]=0")]
        public void TestArrayIndexNegative(string encoded)
        {
            ParseInvalidFormUrlEncoded(encoded);
        }

        /// <summary>
        /// Tests for parsing complex object graphs form-urlencoded.
        /// </summary>
        [Theory]
        [PropertyData("TestObjectTestData")]
        public void TestObject(string encoded, string expectedResult)
        {
            ValidateFormUrlEncoded(encoded, expectedResult);
        }

        /// <summary>
        /// Tests for parsing form-urlencoded data with encoded names.
        /// </summary>
        [Theory]
        [PropertyData("TestEncodedNameTestData")]
        public void TestEncodedName(string encoded, string expectedResult)
        {
            ValidateFormUrlEncoded(encoded, expectedResult);
        }

        /// <summary>
        /// Tests for malformed form-urlencoded data.
        /// </summary>
        [Theory,
            InlineData("a[b=2"),
            InlineData("a[[b]=2"),
            InlineData("a[b]]=2")]
        public void TestNegative(string encoded)
        {
            ParseInvalidFormUrlEncoded(encoded);
        }

        private static void BuildParams(string prefix, JToken jsonValue, List<string> results)
        {
            if (jsonValue is JValue)
            {
                JValue jsonPrimitive = jsonValue as JValue;
                if (jsonPrimitive != null)
                {
                    if (jsonPrimitive.Type == JTokenType.String && String.IsNullOrEmpty(jsonPrimitive.Value.ToString()))
                    {
                        results.Add(prefix + "=" + String.Empty);
                    }
                    else
                    {
                        if (jsonPrimitive.Value is DateTime || jsonPrimitive.Value is DateTimeOffset)
                        {
                            string dateStr = jsonPrimitive.ToString();
                            if (!String.IsNullOrEmpty(dateStr) && dateStr.StartsWith("\""))
                            {
                                dateStr = dateStr.Substring(1, dateStr.Length - 2);
                            }
                            results.Add(prefix + "=" + WebUtility.UrlEncode(dateStr));
                        }
                        else
                        {
                            results.Add(prefix + "=" + WebUtility.UrlEncode(jsonPrimitive.Value.ToString()));
                        }
                    }
                }
                else
                {
                    results.Add(prefix + "=" + String.Empty);
                }
            }
            else if (jsonValue is JArray)
            {
                for (int i = 0; i < ((JArray)jsonValue).Count; i++)
                {
                    if (jsonValue[i] is JArray || jsonValue[i] is JObject)
                    {
                        BuildParams(prefix + "[" + i + "]", jsonValue[i], results);
                    }
                    else
                    {
                        BuildParams(prefix + "[]", jsonValue[i], results);
                    }
                }
            }
            else
            {
                //jsonValue is JObject
                foreach (KeyValuePair<string, JToken> item in (JObject)jsonValue)
                {
                    BuildParams(prefix + "[" + item.Key + "]", item.Value, results);
                }
            }
        }

        private static void ParseInvalidFormUrlEncoded(string encoded)
        {
            byte[] data = Encoding.UTF8.GetBytes(encoded);
            for (var cnt = 1; cnt <= data.Length; cnt++)
            {
                ICollection<KeyValuePair<string, string>> collection;
                FormUrlEncodedParser parser = FormUrlEncodedParserTests.CreateParser(data.Length + 1, out collection);
                Assert.NotNull(parser);

                int totalBytesConsumed;
                ParserState state = FormUrlEncodedParserTests.ParseBufferInSteps(parser, data, cnt, out totalBytesConsumed);
                Assert.Equal(ParserState.Done, state);
                Assert.Equal(data.Length, totalBytesConsumed);

                Assert.ThrowsArgument(() => { FormUrlEncodedJson.Parse(collection); }, null);
            }
        }

        private static void ValidateFormUrlEncoded(string encoded, string expectedResult)
        {
            byte[] data = Encoding.UTF8.GetBytes(encoded);
            for (var cnt = 1; cnt <= data.Length; cnt++)
            {
                ICollection<KeyValuePair<string, string>> collection;
                FormUrlEncodedParser parser = FormUrlEncodedParserTests.CreateParser(data.Length + 1, out collection);
                Assert.NotNull(parser);

                int totalBytesConsumed;
                ParserState state = FormUrlEncodedParserTests.ParseBufferInSteps(parser, data, cnt, out totalBytesConsumed);
                Assert.Equal(ParserState.Done, state);
                Assert.Equal(data.Length, totalBytesConsumed);

                JObject result = FormUrlEncodedJson.Parse(collection);
                Assert.NotNull(result);
                Assert.Equal(expectedResult, result.ToString(Newtonsoft.Json.Formatting.None));
            }
        }
    }
}
