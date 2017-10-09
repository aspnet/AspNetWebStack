// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

// Portions copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TestCommon;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace System.Net.Http.Formatting
{
    public class JsonNetSerializationTest
    {
        public static TheoryDataSet<object, string> SerializedJson
        {
            get
            {
                return new TheoryDataSet<object, string>()
                {
                    // Primitives
                    { 'f', "\"f\"" },
                    { "abc", "\"abc\"" },
                    { "\"\\", @"""\""\\""" },
                    { 256, "256" },
                    { (ulong)long.MaxValue, long.MaxValue.ToString() },
                    { 45.78m, "45.78" },
                    { .00000457823432, "4.57823432E-06" },
                    { (byte)24, "24" },
                    { false, "false" },
                    { AttributeTargets.Assembly | AttributeTargets.Constructor, "33" },
                    { ConsoleColor.DarkCyan, "3" },
                    { new DateTimeOffset(1999, 5, 27, 4, 34, 45, TimeSpan.Zero), "\"1999-05-27T04:34:45+00:00\"" },
                    { new TimeSpan(5, 30, 0), "\"05:30:00\"" },
                    { new Uri("http://www.bing.com"), @"""http://www.bing.com""" },
                    { new Uri("http://www.bing.com/"), @"""http://www.bing.com/""" },
                    { new Uri("http://www.bing.com/foo"), @"""http://www.bing.com/foo""" },
                    { new Uri("http://www.bing.com/foo/"), @"""http://www.bing.com/foo/""" },
                    { new Guid("4ed1cd44-11d7-4b27-b623-0b8b553c8906"), "\"4ed1cd44-11d7-4b27-b623-0b8b553c8906\"" },

                    // Structs
                    { new Point() { x = 45, Y = -5}, "{\"x\":45,\"Y\":-5}" },

                    // Arrays
                    { new object[] {}, "[]" },
                    { new int[] { 1, 2, 3}, "[1,2,3]" },
                    { new string[] { "a", "b"}, "[\"a\",\"b\"]" },
                    { new Point[] { new Point() { x = 10, Y = 10}, new Point() { x = 20, Y = 20}}, "[{\"x\":10,\"Y\":10},{\"x\":20,\"Y\":20}]" },

                    // Collections
                    { new List<int> { 1, 2, 3}, "[1,2,3]" },
                    { new List<string> { "a", "b"}, "[\"a\",\"b\"]" },
                    { new List<Point> { new Point() { x = 10, Y = 10}, new Point() { x = 20, Y = 20}}, "[{\"x\":10,\"Y\":10},{\"x\":20,\"Y\":20}]" },
                    { new MyList<int> { 1, 2, 3}, "[1,2,3]" },
                    { new MyList<string> { "a", "b"}, "[\"a\",\"b\"]" },
                    { new MyList<Point> { new Point() { x = 10, Y = 10}, new Point() { x = 20, Y = 20}}, "[{\"x\":10,\"Y\":10},{\"x\":20,\"Y\":20}]" },

                    // Dictionaries

                    { new Dictionary<string, string> { { "k1", "v1" }, { "k2", "v2" } }, "{\"k1\":\"v1\",\"k2\":\"v2\"}" },
                    { new Dictionary<int, string> { { 1, "v1" }, { 2, "v2" } }, "{\"1\":\"v1\",\"2\":\"v2\"}" },

                    // Anonymous types
                    { new { Anon1 = 56, Anon2 = "foo"}, "{\"Anon1\":56,\"Anon2\":\"foo\"}" },

                    // Classes
                    { new DataContractType() { s = "foo", i = 49, NotAMember = "Error" }, "{\"s\":\"foo\",\"i\":49}" },
                    { new POCOType() { s = "foo", t = "Error"}, "{\"s\":\"foo\"}" },
#if !NETFX_CORE // Only publics are serialized in portable library
                    { new SerializableType("protected") { publicField = "public", protectedInternalField = "protected internal", internalField = "internal", PublicProperty = "private", nonSerializedField = "Error" }, "{\"publicField\":\"public\",\"internalField\":\"internal\",\"protectedInternalField\":\"protected internal\",\"protectedField\":\"protected\",\"privateField\":\"private\"}" },
#else
                    { new SerializableType("protected") { publicField = "public", protectedInternalField = "protected internal", internalField = "internal", PublicProperty = "private", nonSerializedField = "Error" }, "{\"publicField\":\"public\",\"PublicProperty\":\"private\"}" },
#endif
                    { new { field1 = "x", field2 = (string)null, field3 = "y" }, "{\"field1\":\"x\",\"field2\":null,\"field3\":\"y\"}" },

                    // Generics
                    { new KeyValuePair<string, bool>("foo", false), "{\"Key\":\"foo\",\"Value\":false}" },

                    // ISerializable types
                    { new ISerializableType() { Property = "Value" }, "{\"SomeProperty\":\"Value\"}" },

                    // JSON Values
                    { new JValue(false), "false" },
                    { new JValue(54), "54" },
                    { new JValue("s"), "\"s\"" },
                    { new JArray() { new JValue(1), new JValue(2) }, "[1,2]" },
                    { new JObject() { { "k1", new JValue("v1") }, { "k2", new JValue("v2") } }, "{\"k1\":\"v1\",\"k2\":\"v2\"}" },
                    { new KeyValuePair<JToken, JToken>(new JValue("k"), new JArray() { new JValue("v1"), new JValue("v2") }), "{\"Key\":\"k\",\"Value\":[\"v1\",\"v2\"]}" },
                };
            }
        }

        public static TheoryDataSet<object, string, Type> TypedSerializedJson
        {
            get
            {
                return new TheoryDataSet<object, string, Type>()
                {
                    // Null
                    { null, "null", typeof(POCOType) },
                    { JValue.CreateNull(), "null", typeof(JToken) },

                    // Nullables
                    { new int?(), "null", typeof(int?) },
                    { new Point?(), "null", typeof(Point?) },
                    { new ConsoleColor?(), "null", typeof(ConsoleColor?) },
                    { new int?(45), "45", typeof(int?) },
                    { new Point?(new Point() { x = 45, Y = -5 }), "{\"x\":45,\"Y\":-5}", typeof(Point?) },
                    { new ConsoleColor?(ConsoleColor.DarkMagenta), "5", typeof(ConsoleColor?)},
                };
            }
        }

        [Theory]
        [PropertyData("SerializedJson")]
        public Task ObjectsSerializeToExpectedJson(object o, string expectedJson)
        {
            return ObjectsSerializeToExpectedJsonWithProvidedType(o, expectedJson, o.GetType());
        }

        [Theory]
        [PropertyData("SerializedJson")]
        public Task JsonDeserializesToExpectedObject(object expectedObject, string json)
        {
            return JsonDeserializesToExpectedObjectWithProvidedType(expectedObject, json, expectedObject.GetType());
        }

        [Theory]
        [PropertyData("TypedSerializedJson")]
        public async Task ObjectsSerializeToExpectedJsonWithProvidedType(object o, string expectedJson, Type type)
        {
            Assert.Equal(expectedJson, await SerializeAsync(o, type));
        }

        [Theory]
        [PropertyData("TypedSerializedJson")]
        public async Task JsonDeserializesToExpectedObjectWithProvidedType(object expectedObject, string json, Type type)
        {
            if (expectedObject == null)
            {
                Assert.Null(await DeserializeAsync(json, type));
            }
            else
            {
                object o = await DeserializeAsync(json, type);
                Assert.Equal(expectedObject, o, new ObjectComparer());
            }
        }

        [Fact]
        public async Task CallbacksGetCalled()
        {
            TypeWithCallbacks o = new TypeWithCallbacks();

            string json = await SerializeAsync(o, typeof(TypeWithCallbacks));
            Assert.Equal("12", o.callbackOrder);

            TypeWithCallbacks deserializedObject = (await DeserializeAsync(json, typeof(TypeWithCallbacks))) as TypeWithCallbacks;
            Assert.Equal("34", deserializedObject.callbackOrder);
        }

        [Fact]
        public async Task DerivedTypesArePreserved()
        {
            JsonMediaTypeFormatter formatter = new JsonMediaTypeFormatter();
            formatter.SerializerSettings.TypeNameHandling = TypeNameHandling.Objects;
            string json = await SerializeAsync(new Derived(), typeof(Base), formatter);
            object deserializedObject = await DeserializeAsync(json, typeof(Base), formatter);
            Assert.IsType<Derived>(deserializedObject);
        }

        [Fact]
        public async Task ArbitraryTypesArentDeserializedByDefault()
        {
            string json = "{\"$type\":\"" + typeof(DangerousType).AssemblyQualifiedName + "\"}";
            object deserializedObject = await DeserializeAsync(json, typeof(object));
            Assert.IsNotType<DangerousType>(deserializedObject);
        }

        [Fact]
        public async Task ReferencesArePreserved()
        {
            Ref ref1 = new Ref();
            Ref ref2 = new Ref();
            ref1.Reference = ref2;
            ref2.Reference = ref1;

            string json = await SerializeAsync(ref1, typeof(Ref));
            Ref deserializedObject = (await DeserializeAsync(json, typeof(Ref))) as Ref;

            Assert.ReferenceEquals(deserializedObject, deserializedObject.Reference.Reference);
        }

        [Fact]
        public async Task MissingMemberGetsDeserializedToNull()
        {
            string json = "{}";

            POCOType deserializedObject = (await DeserializeAsync(json, typeof(POCOType))) as POCOType;

            Assert.Null(deserializedObject.s);
        }

        [Fact]
        public Task DeserializingDeepArraysThrows()
        {
            StringBuilder sb = new StringBuilder();
            int depth = 500;
            for (int i = 0; i < depth; i++)
            {
                sb.Append("[");
            }
            sb.Append("null");
            for (int i = 0; i < depth; i++)
            {
                sb.Append("]");
            }
            string json = sb.ToString();

            return Assert.ThrowsAsync<JsonReaderException>(() => DeserializeAsync(json, typeof(object)));
        }

        [Theory]
        // existing good surrogate pair
        [InlineData("ABC \\ud800\\udc00 DEF", "ABC \ud800\udc00 DEF")]
        // invalid surrogates (two high back-to-back)
        [InlineData("ABC \\ud800\\ud800 DEF", "ABC \ufffd\ufffd DEF")]
        // invalid high surrogate at end of string
        [InlineData("ABC \\ud800", "ABC \ufffd")]
        // high surrogate not followed by low surrogate
        [InlineData("ABC \\ud800 DEF", "ABC \ufffd DEF")]
        // low surrogate not preceded by high surrogate
        [InlineData("ABC \\udc00\\ud800 DEF", "ABC \ufffd\ufffd DEF")]
        // make sure unencoded invalid surrogate characters don't make it through
        [InlineData("\udc00\ud800\ud800", "??????")]
        public async Task InvalidUnicodeStringsAreFixedUp(string input, string expectedString)
        {
            string json = "\"" + input + "\"";
            string deserializedString = (await DeserializeAsync(json, typeof(string))) as string;

            Assert.Equal(expectedString, deserializedString);

        }

        private static async Task<string> SerializeAsync(object o, Type type, MediaTypeFormatter formatter = null)
        {
            formatter = formatter ?? new JsonMediaTypeFormatter();
            MemoryStream ms = new MemoryStream();
            await formatter.WriteToStreamAsync(type, o, ms, null, null);
            ms.Flush();
            ms.Position = 0;
            return new StreamReader(ms).ReadToEnd();
        }

        internal static Task<object> DeserializeAsync(string json, Type type, MediaTypeFormatter formatter = null, IFormatterLogger formatterLogger = null)
        {
            formatter = formatter ?? new JsonMediaTypeFormatter();
            MemoryStream ms = new MemoryStream();
            byte[] bytes = Encoding.Default.GetBytes(json);
            ms.Write(bytes, 0, bytes.Length);
            ms.Flush();
            ms.Position = 0;

            return formatter.ReadFromStreamAsync(type, ms, content: null, formatterLogger: formatterLogger);
        }
    }

    public class ObjectComparer : IEqualityComparer<object>
    {
        bool IEqualityComparer<object>.Equals(object x, object y)
        {
            Type xType = x.GetType();

            if (xType == y.GetType())
            {
                if (typeof(JToken).IsAssignableFrom(xType) || xType == typeof(ArgumentNullException) || xType == typeof(KeyValuePair<JToken, JToken>))
                {
                    return x.ToString() == y.ToString();
                }
                if (xType == typeof(DataContractType))
                {
                    return Equals<DataContractType>(x, y);
                }
                if (xType == typeof(POCOType))
                {
                    return Equals<POCOType>(x, y);
                }

                if (xType == typeof(SerializableType))
                {
                    return Equals<SerializableType>(x, y);
                }

                if (xType == typeof(ISerializableType))
                {
                    return Equals<ISerializableType>(x, y);
                }

                if (xType == typeof(Point))
                {
                    return Equals<Point>(x, y);
                }

                if (typeof(IEnumerable).IsAssignableFrom(xType))
                {
                    IEnumerator xEnumerator = ((IEnumerable)x).GetEnumerator();
                    IEnumerator yEnumerator = ((IEnumerable)y).GetEnumerator();
                    while (xEnumerator.MoveNext())
                    {
                        // if x is longer than y
                        if (!yEnumerator.MoveNext())
                        {
                            return false;
                        }
                        else
                        {
                            if (!xEnumerator.Current.Equals(yEnumerator.Current))
                            {
                                return false;
                            }
                        }
                    }
                    // if y is longer than x
                    if (yEnumerator.MoveNext())
                    {
                        return false;
                    }
                    return true;
                }
            }

            return x.Equals(y);
        }

        int IEqualityComparer<object>.GetHashCode(object obj)
        {
            throw new NotImplementedException();
        }

        bool Equals<T>(object x, object y) where T : IEquatable<T>
        {
            IEquatable<T> yEquatable = (IEquatable<T>)y;
            return yEquatable.Equals((T)x);
        }
    }

    // Marked as [Serializable] to check that [DataContract] takes precedence over [Serializable]
    [DataContract]
    [Serializable]
    public class DataContractType : IEquatable<DataContractType>
    {
        [DataMember]
        public string s;

        [DataMember]
        internal int i;

        public string NotAMember;

        public bool Equals(DataContractType other)
        {
            return this.s == other.s && this.i == other.i;
        }
    }

    public class POCOType : IEquatable<POCOType>
    {
        public string s;
        internal string t;

        public bool Equals(POCOType other)
        {
            return this.s == other.s;
        }
    }

    public class MyList<T> : ICollection<T>
    {
        List<T> innerList = new List<T>();

        public IEnumerator<T> GetEnumerator()
        {
            return innerList.GetEnumerator();
        }

        public void Add(T item)
        {
            innerList.Add(item);
        }

        public void Clear()
        {
            innerList.Clear();
        }

        public bool Contains(T item)
        {
            return innerList.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            innerList.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return innerList.Count; }
        }

        public bool IsReadOnly
        {
            get { return ((ICollection<T>)innerList).IsReadOnly; }
        }

        public bool Remove(T item)
        {
            return innerList.Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return innerList.GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return innerList.GetEnumerator();
        }
    }

    [Serializable]
    class SerializableType : IEquatable<SerializableType>
    {
        public SerializableType(string protectedFieldValue)
        {
            this.protectedField = protectedFieldValue;
        }

        public string publicField;
        internal string internalField;
        protected internal string protectedInternalField;
        protected string protectedField;
        private string privateField;

        public string PublicProperty
        {
            get
            {
                return privateField;
            }
            set
            {
                this.privateField = value;
            }
        }

        [NonSerialized]
        public string nonSerializedField;

        public bool Equals(SerializableType other)
        {
#if !NETFX_CORE // Only publics are serialized in portable library. privateField is serialized through PublicProperty
            return this.publicField == other.publicField &&
                this.internalField == other.internalField &&
                this.protectedInternalField == other.protectedInternalField &&
                this.protectedField == other.protectedField &&
                this.privateField == other.privateField;
#else
            // this.privateField is serialized through this.PublicProperty, thus the comparison here
            return this.publicField == other.publicField &&
                this.privateField == other.privateField;
#endif
        }
    }

    public struct Point : IEquatable<Point>
    {
        public int x;
        public int Y { get; set; }

        public bool Equals(Point other)
        {
            return this.x == other.x && this.Y == other.Y;
        }
    }

    [DataContract(IsReference = true)]
    public class Ref
    {
        [DataMember]
        public Ref Reference;
    }

    public class Base
    {
    }

    public class Derived : Base
    {
    }

    [DataContract]
    public class TypeWithCallbacks
    {
        public string callbackOrder = "";

        [OnSerializing]
        public void OnSerializing(StreamingContext c)
        {
            callbackOrder += "1";
        }

        [OnSerialized]
        public void OnSerialized(StreamingContext c)
        {
            callbackOrder += "2";
        }

        [OnDeserializing]
        public void OnDeserializing(StreamingContext c)
        {
            callbackOrder += "3";
        }

        [OnDeserialized]
        public void OnDeserialized(StreamingContext c)
        {
            callbackOrder += "4";
        }
    }

    public class DangerousType
    {
    }

    [Serializable]
    public class ISerializableType : ISerializable, IEquatable<ISerializableType>
    {
        public ISerializableType()
        {
        }

        protected ISerializableType(SerializationInfo info, StreamingContext context)
        {
            // The key here for GetValue/SetValue is intentionally different from the property name.
            // This tests that the serializer is using the result of GetObjectData, rather than the property
            // name.
            Property = (string)info.GetValue("SomeProperty", typeof(String));
        }

        public string Property
        {
            get;
            set;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("SomeProperty", this.Property, typeof(String));
        }

        public bool Equals(ISerializableType other)
        {
            return String.Equals(Property, other.Property, StringComparison.Ordinal);
        }
    }
}
