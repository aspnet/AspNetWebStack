// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
#if !NETFX_CORE
using System.Net.Http.Formatting.Internal;
#endif
using System.Web.WebPages.TestUtils;
using Microsoft.TestCommon;

namespace System.Net.Http.Internal
{
    public class HttpValueCollectionTest
    {
        private static readonly int _maxCollectionKeys = 1000;

        private static HttpValueCollection CreateInstance()
        {
#if NETFX_CORE
            return new HttpValueCollection();
#else
            return HttpValueCollection.Create();
#endif
        }

        private static void RunInIsolation(Action action)
        {
            AppDomainUtils.RunInSeparateAppDomain(action);
        }

        public static TheoryDataSet<IEnumerable<KeyValuePair<string, string>>> KeyValuePairs
        {
            get
            {
                return new TheoryDataSet<IEnumerable<KeyValuePair<string, string>>>()
                {
                    new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string,string>(null, null),
                        new KeyValuePair<string,string>("n0", ""),
                        new KeyValuePair<string,string>("n1", "v1"),
                        new KeyValuePair<string,string>("n@2", "v@2"),
                        new KeyValuePair<string,string>("n 3", "v 3"),
                        new KeyValuePair<string,string>("n+4", "v+4"),
                        new KeyValuePair<string,string>("n;5", "v;5"),
                        new KeyValuePair<string,string>("n=5", "v=5"),
                    }
                };
            }
        }

        internal class TestPropertyHolder
        {
            public static TheoryDataSet<HttpValueCollection, string> ToStringTestData
            {
                get
                {
                    TheoryDataSet<HttpValueCollection, string> dataSet = new TheoryDataSet<HttpValueCollection, string>();

                    var hvc1 = CreateInstance();
                    hvc1.Add(null, null);
                    dataSet.Add(hvc1, "");

                    var hvc2 = CreateInstance();
                    hvc2.Add("name", null);
                    dataSet.Add(hvc2, "name");

                    var hvc3 = CreateInstance();
                    hvc3.Add("name", "");
                    dataSet.Add(hvc3, "name");

                    var hvc4 = CreateInstance();
                    hvc4.Add("na me", "");
                    dataSet.Add(hvc4, "na+me");

#if NETFX_CORE
                    string encoded5 = "n%22%2C%3B%5Cn";
#else
                    string encoded5 = "n%22%2c%3b%5cn";
#endif

                    var hvc5 = CreateInstance();
                    hvc5.Add("n\",;\\n", "");
                    dataSet.Add(hvc5, encoded5);

                    var hvc6 = CreateInstance();
                    hvc6.Add("", "v1");
                    hvc6.Add("", "v2");
                    hvc6.Add("", "v3");
                    hvc6.Add("", "v4");
                    dataSet.Add(hvc6, "=v1&=v2&=v3&=v4");

                    var hvc7 = CreateInstance();
                    hvc7.Add("n1", "v1");
                    hvc7.Add("n2", "v2");
                    hvc7.Add("n3", "v3");
                    hvc7.Add("n4", "v4");
                    dataSet.Add(hvc7, "n1=v1&n2=v2&n3=v3&n4=v4");

#if NETFX_CORE
                    string encoded8 = "n%2C1=v%2C1&n%3B2=v%3B2";
#else
                    string encoded8 = "n%2c1=v%2c1&n%3b2=v%3b2";
#endif

                    var hvc8 = CreateInstance();
                    hvc8.Add("n,1", "v,1");
                    hvc8.Add("n;2", "v;2");
                    dataSet.Add(hvc8, encoded8);

#if NETFX_CORE
                    string encoded9 = "n1=%26&n2=%3B&n3=%26&n4=%2B&n5=%26&n6=%3D&n7=%26";
#else
                    string encoded9 = "n1=%26&n2=%3b&n3=%26&n4=%2b&n5=%26&n6=%3d&n7=%26";
#endif

                    var hvc9 = CreateInstance();
                    hvc9.Add("n1", "&");
                    hvc9.Add("n2", ";");
                    hvc9.Add("n3", "&");
                    hvc9.Add("n4", "+");
                    hvc9.Add("n5", "&");
                    hvc9.Add("n6", "=");
                    hvc9.Add("n7", "&");
                    dataSet.Add(hvc9, encoded9);

                    var hvc10 = CreateInstance();
                    hvc10.Add("n1", "&");
                    hvc10.Add("n2", null);
                    hvc10.Add("n3", "null");
                    dataSet.Add(hvc10, "n1=%26&n2&n3=null");

                    return dataSet;
                }
            }
        }

        [Fact]
        public void Create_CreatesEmptyCollection()
        {
            var nvc = CreateInstance();

            Assert.IsType<HttpValueCollection>(nvc);
            Assert.Empty(nvc);
        }

        // This set of tests requires running on a separate appdomain so we don't
        // touch the static property MediaTypeFormatter.MaxHttpCollectionKeys.
        [Fact]
        public void Create_CreateTooManyKeysThrows()
        {
            RunInIsolation(Create_CreateTooManyKeysThrowsPrivate);
        }

        private static void Create_CreateTooManyKeysThrowsPrivate()
        {
            // Arrange
            MediaTypeFormatter.MaxHttpCollectionKeys = _maxCollectionKeys;

            List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();

            for (int i = 0; i < _maxCollectionKeys + 1; i++)
            {
                list.Add(new KeyValuePair<string, string>(i.ToString(), i.ToString()));
            }

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => HttpValueCollection.Create(list), TooManyKeysError);
        }

        private static string TooManyKeysError
        {
            get
            {
                return "The number of keys in a NameValueCollection has exceeded the limit of '" + _maxCollectionKeys + "'. You can adjust it by modifying the MaxHttpCollectionKeys property on the 'System.Net.Http.Formatting.MediaTypeFormatter' class.";
            }
        }

        [Fact]
        public void Create_CreateDoesntThrowTooManyValues()
        {
            RunInIsolation(Create_CreateDoesntThrowTooManyValuesPrivate);
        }

        private static void Create_CreateDoesntThrowTooManyValuesPrivate()
        {
            // note this is static, but also the expected type in a real run.
            MediaTypeFormatter.MaxHttpCollectionKeys = _maxCollectionKeys;

            List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();

            for (int i = 0; i < _maxCollectionKeys + 1; i++)
            {
                list.Add(new KeyValuePair<string, string>("key", i.ToString()));
            }

            Assert.DoesNotThrow(() => HttpValueCollection.Create(list));
        }

        [Fact]
        public void AddTooManyKeysThrows()
        {
            RunInIsolation(Create_CreateDoesntThrowTooManyValuesPrivate);
        }

        private void AddTooManyKeysThrowsPrivate()
        {
            // Note this is static, but also the expected type in a real run.
            MediaTypeFormatter.MaxHttpCollectionKeys = _maxCollectionKeys;

            HttpValueCollection collection = CreateInstance();

            int i = 0;

            // Act && Assert
            Assert.Throws<InvalidOperationException>(() =>
            {
                for (; i < 1001; i++)
                {
                    collection.Add(i.ToString(), i.ToString());
                }
            }, TooManyKeysError);


            Assert.Equal(1000, i);
        }

        [Fact]
        public void AddDoesntThrowTooManyValues()
        {
            RunInIsolation(AddDoesntThrowTooManyValuesPrivate);
        }

        private static void AddDoesntThrowTooManyValuesPrivate()
        {
            // Note this is static, but also the expected type in a real run.
            MediaTypeFormatter.MaxHttpCollectionKeys = _maxCollectionKeys;

            HttpValueCollection collection = CreateInstance();

            // Act && Assert
            Assert.DoesNotThrow(() =>
            {
                for (int i = 0; i < 1001; i++)
                {
                    collection.Add("key", i.ToString());
                }
            });
        }

        [Theory]
        [PropertyData("KeyValuePairs")]
        public void Create_InitializesCorrectly(IEnumerable<KeyValuePair<string, string>> input)
        {
            var nvc = HttpValueCollection.Create(input);

            int count = input.Count();
            Assert.IsType<HttpValueCollection>(nvc);
            Assert.Equal(count, nvc.Count);

            int index = 0;

            foreach (KeyValuePair<string, string> kvp in input)
            {
                string expectedKey = kvp.Key ?? String.Empty;
                string expectedValue = kvp.Value ?? String.Empty;

#if NETFX_CORE
                KeyValuePair<string, string> actualKvp = nvc.List[index];
                string actualKey = actualKvp.Key;
                string actualValue = actualKvp.Value;
#else
                string actualKey = nvc.AllKeys[index];
                string actualValue = nvc[index];
#endif
                index++;

                Assert.Equal(expectedKey, actualKey);
                Assert.Equal(expectedValue, actualValue);
            }
        }

        [Theory]
        [PropertyData("KeyValuePairs")]
        public void GetIsEquivalentToIndexerProperty(IEnumerable<KeyValuePair<string, string>> input)
        {
            var nvc = HttpValueCollection.Create(input);

            int count = input.Count();
            Assert.IsType<HttpValueCollection>(nvc);
            Assert.Equal(count, nvc.Count);

            foreach (KeyValuePair<string, string> kvp in input)
            {
                Assert.Equal(nvc[kvp.Key], nvc.Get(kvp.Key));
            }
        }

        [Theory]
        [PropertyData("ToStringTestData", PropertyType = typeof(TestPropertyHolder))]
        internal void ToString_GeneratesCorrectOutput(HttpValueCollection input, string expectedOutput)
        {
            string actualOutput = input.ToString();
            Assert.Equal(expectedOutput, actualOutput);
        }
    }
}
