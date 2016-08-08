// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Formatting;
using System.Runtime.Serialization;
using Microsoft.TestCommon;

namespace System.Net.Formatting.Tests
{
    // Tests for ensuring the serializers behave consistently in various cases. 
    // This is important for conneg. 
    public class SerializerConsistencyTests
    {
        [Fact]
        public void PartialContract()
        {
            var c = new PartialDataContract { PropertyWithAttribute = "one", PropertyWithoutAttribute = "false" };
            SerializerConsistencyHepers.Test(c);
        }

        [Fact]
        public void ClassWithFields()
        {
            var c1 = new ClassWithFields { Property = "prop" };
            c1.SetField("field");
            SerializerConsistencyHepers.Test(c1);
        }

        [Fact]
        public void PrivateProperty()
        {
            var source2 = new PrivateProperty { FirstName = "John", LastName = "Smith" };
            source2.SetItem("shoes");
            SerializerConsistencyHepers.Test(source2);
        }

        [Fact]
        public void NormalClass()
        {
            var source = new NormalClass { FirstName = "John", LastName = "Smith", Item = "Socks" };
            SerializerConsistencyHepers.Test(source);
        }

        [Fact]
        public void InheritedProperties()
        {
            // Will we pick up inherited properties from a base object?
            BaseClass source = new DerivedClass { Property = "base", DerivedProperty = "derived" };
            source.SetField("private");
            SerializerConsistencyHepers.Test(source, typeof(DerivedClass));
        }

        [Fact]
        public void NullEmptyWhitespaceString()
        {
            NormalClass source = new NormalClass { FirstName = string.Empty, LastName = null, Item = "   " };

            SerializerConsistencyHepers.Test(source);
        }

        [Fact]
        public void Dictionary()
        {
            var dict = new Dictionary<string, int>();
            dict["one"] = 1;
            dict["two"] = 2;

            SerializerConsistencyHepers.Test(dict);
        }

        [Fact]
        public void Array()
        {
            string[] array = new string[] { "First", "Second", "Last" };

            SerializerConsistencyHepers.Test(array);
        }

        [Fact]
        public void ArrayInterfaces()
        {
            string[] array = new string[] { "First", "Second", "Last" };

            SerializerConsistencyHepers.Test(array, typeof(IList<string>));
            SerializerConsistencyHepers.Test(array, typeof(ICollection<string>));
            SerializerConsistencyHepers.Test(array, typeof(IEnumerable<string>));
        }

        [Fact]
        public void Linq()
        {
            var l = from i in Enumerable.Range(1, 10) where i > 5 select i * i;

            // Runtime type of a linq expression is some derived Linq type which we can't deserialize to. 
            // So explicitly call out IEnumerable<T>
            SerializerConsistencyHepers.Test(l, typeof(IEnumerable<int>));
        }

        [Fact]
        public void StaticProps()
        {
            ClassWithStaticProperties source = new ClassWithStaticProperties();

            SerializerConsistencyHepers.Test(source);
        }
    }

    // public class, public properties
    public class NormalClass
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Item { get; set; }
    }

    public class ClassWithStaticProperties
    {
        public string InstanceProp { get; set; }
        public static string StaticProp
        {
            get
            {
                Assert.True(false, "serializers should never call static properties");
                return string.Empty;
            }
            set
            {
                Assert.True(false, "serializers should never call static properties");
                throw new InvalidOperationException(); // assert already threw
            }
        }
    }

    [DataContract]
    public class PartialDataContract
    {
        [DataMember]
        public string PropertyWithAttribute { get; set; }

        // no attribute here
        public string PropertyWithoutAttribute { get; set; }
    }

    public class PrivateProperty // with private field
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        private string Item { get; set; }

        public void SetItem(string item)
        {
            this.Item = item;
        }
    }

    public class ClassWithFields
    {
        public string Property { get; set; }
        private string Field;

        public void SetField(string field)
        {
            this.Field = field;
        }
    }

    public class BaseClass
    {
        private string PrivateField;
        public string Property { get; set; }

        public void SetField(string field)
        {
            PrivateField = field;
        }
    }

    public class DerivedClass : BaseClass
    {
        public string DerivedProperty { get; set; }
    }

    // Helpers for performing consistency checks with the serializers.
    class SerializerConsistencyHepers
    {
        // Exercise the various serialization paths to verify that the default serializers behave consistently.
        public static void Test(object source)
        {
            Type tSource = source.GetType();
            Test(source, tSource);
        }

        // Allow explicitly passing in the type that gets passed to the serializer. 
        // The expectation is that the type can be read and written with both serializers. 
        public static void Test(object source, Type tSource)
        {
            Test(source, tSource, tSource);
        }

        // tSourceWrite - the type we use for the initial write.  This can be specific, and a 1-way serializable type (eg, a linq expression). 
        // tSourceRead - the type that we read back as. This should be more general because we need to instantiate it.
        public static void Test(object source, Type tSourceWrite, Type tSourceRead)
        {
            // Apply consistency chceks. This interleaves the results between the formatters. 
            // It doesn't actually matter specifically what the formatter does, it just matters that they're consistent.
            // This will test various transitions between C#->JSON, JSON->C#, C#->XML, and XML->C#. 
            // We can't compare C# objects, but we can compare the textual representation from XML and JSON. 
            MediaTypeFormatter xmlFormatter = new MediaTypeFormatterCollection().XmlFormatter;
            MediaTypeFormatter jsonFor = new MediaTypeFormatterCollection().JsonFormatter;

            MemoryStream blobJson = Write(source, tSourceWrite, jsonFor); // C# --> JSON
            MemoryStream blobXml = Write(source, tSourceWrite, xmlFormatter); // C# --> XML

            object obj2 = Read(blobJson, tSourceRead, jsonFor); // C# --> JSON --> C#
            object obj1 = Read(blobXml, tSourceRead, xmlFormatter); // C# --> XML --> C#            

            // We were able to round trip the source object through both formatters.
            // Now see if the resulting object is the same.

            // Check C# --> XML --> C#

            var blobXml2 = Write(obj1, tSourceRead, xmlFormatter);  // C# --> XML --> C# --> XML
            var blobJson2 = Write(obj1, tSourceRead, jsonFor); // C# --> XML --> C# --> JSON

            // Ensure that C#->XMl and  C#->XML->C#->XML give us the same result..
            Compare(blobXml, blobXml2);

            // Ensure that C#->Json and C#->XML->C#->Json give us the same result 
            Compare(blobJson, blobJson2);

            // Check C# --> JSON --> C#

            var blobXml3 = Write(obj2, tSourceRead, xmlFormatter);  // C# --> JSON --> C# --> XML
            var blobJson3 = Write(obj2, tSourceRead, jsonFor); // C# --> JSON --> C# --> JSON

            // Ensure that C#->XML and C#->JSON->C#->XML are the same
            Compare(blobXml, blobXml3);

            // Ensure that C#->JSon and C#->JSON->C#->JSON are the same.
            Compare(blobJson, blobJson3);
        }

        // Compare if 2 streams have the same contents. 
        private static void Compare(MemoryStream ms1, MemoryStream ms2)
        {
            string s1 = ToString(ms1);
            string s2 = ToString(ms2);

            Assert.Equal(s1, s2);
        }

        // Given a memory stream (which is representing a textual serialization format), get the string.
        private static string ToString(MemoryStream ms)
        {
            byte[] b = ms.GetBuffer();
            return System.Text.Encoding.UTF8.GetString(b, 0, (int)ms.Length);
        }

        private static object Read(MemoryStream ms, Type tSource, MediaTypeFormatter formatter)
        {
            bool f = formatter.CanReadType(tSource);
            Assert.True(f);

            object o = formatter.ReadFromStreamAsync(tSource, ms, content: null, formatterLogger: null).Result;
            Assert.True(tSource.IsAssignableFrom(o.GetType()));

            return o;
        }

        private static MemoryStream Write(object obj, Type tSource, MediaTypeFormatter formatter)
        {
            bool f = formatter.CanWriteType(tSource);
            Assert.True(f);

            MemoryStream ms = new MemoryStream();

            formatter.WriteToStreamAsync(tSource, obj, ms, content: null, transportContext: null).Wait();

            ms.Position = 0;
            return ms;
        }
    }
}
