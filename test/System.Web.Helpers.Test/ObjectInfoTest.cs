// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Dynamic;
using System.Linq;
using System.Web.TestUtil;
using Microsoft.TestCommon;

namespace System.Web.Helpers.Test
{
    public class ObjectInfoTest
    {
        [Fact]
        public void PrintWithNegativeDepthThrows()
        {
            // Act & Assert
            Assert.ThrowsArgumentGreaterThanOrEqualTo(() => ObjectInfo.Print(null, depth: -1), "depth", "0");
        }

        [Fact]
        public void PrintWithInvalidEnumerationLength()
        {
            // Act & Assert
            Assert.ThrowsArgumentGreaterThan(() => ObjectInfo.Print(null, enumerationLength: -1), "enumerationLength", "0");
        }

        [Fact]
        public void PrintWithNull()
        {
            // Arrange
            MockObjectVisitor visitor = CreateObjectVisitor();

            // Act
            visitor.Print(null);

            // Assert
            Assert.Equal(1, visitor.Values.Count);
            Assert.Equal("null", visitor.Values[0]);
        }

        [Fact]
        public void PrintWithEmptyString()
        {
            // Arrange
            MockObjectVisitor visitor = CreateObjectVisitor();

            // Act
            visitor.Print(String.Empty);

            // Assert
            Assert.Equal(1, visitor.Values.Count);
            Assert.Equal(String.Empty, visitor.Values[0]);
        }

        [Fact]
        public void PrintWithInt()
        {
            // Arrange
            MockObjectVisitor visitor = CreateObjectVisitor();

            // Act
            visitor.Print(404);

            // Assert
            Assert.Equal(1, visitor.Values.Count);
            Assert.Equal("404", visitor.Values[0]);
        }

        [Fact]
        public void PrintWithIDictionary()
        {
            // Arrange
            MockObjectVisitor visitor = CreateObjectVisitor();
            IDictionary dict = new OrderedDictionary();
            dict.Add("foo", "bar");
            dict.Add("abc", 500);

            // Act
            visitor.Print(dict);

            // Assert
            Assert.Equal("foo = bar", visitor.KeyValuePairs[0]);
            Assert.Equal("abc = 500", visitor.KeyValuePairs[1]);
        }

        [Fact]
        public void PrintWithIEnumerable()
        {
            // Arrange
            MockObjectVisitor visitor = CreateObjectVisitor();
            var values = Enumerable.Range(0, 10);

            // Act
            visitor.Print(values);

            // Assert
            foreach (var num in values)
            {
                Assert.True(visitor.Values.Contains(num.ToString()));
            }
        }

        [Fact]
        public void PrintWithGenericIListPrintsIndex()
        {
            // Arrange
            MockObjectVisitor visitor = CreateObjectVisitor();
            var values = Enumerable.Range(0, 10).ToList();

            // Act
            visitor.Print(values);

            // Assert
            for (int i = 0; i < values.Count; i++)
            {
                Assert.True(visitor.Values.Contains(values[i].ToString()));
                Assert.True(visitor.Indexes.Contains(i));
            }
        }

        [Fact]
        public void PrintWithArrayPrintsIndex()
        {
            // Arrange
            MockObjectVisitor visitor = CreateObjectVisitor();
            var values = Enumerable.Range(0, 10).ToArray();

            // Act
            visitor.Print(values);

            // Assert
            for (int i = 0; i < values.Length; i++)
            {
                Assert.True(visitor.Values.Contains(values[i].ToString()));
                Assert.True(visitor.Indexes.Contains(i));
            }
        }

        [Fact]
        public void PrintNameValueCollectionPrintsKeysAndValues()
        {
            // Arrange
            MockObjectVisitor visitor = CreateObjectVisitor();
            var values = new NameValueCollection();
            values["a"] = "1";
            values["b"] = null;

            // Act            
            visitor.Print(values);

            // Assert
            Assert.Equal("a = 1", visitor.KeyValuePairs[0]);
            Assert.Equal("b = null", visitor.KeyValuePairs[1]);
        }

        [Fact]
        public void PrintDateTime()
        {
            using (new CultureReplacer("en-US"))
            {
                // Arrange
                MockObjectVisitor visitor = CreateObjectVisitor();
                var dt = new DateTime(2001, 11, 20, 10, 30, 1);

                // Act            
                visitor.Print(dt);

                // Assert
                Assert.Equal("11/20/2001 10:30:01 AM", visitor.Values[0]);
            }
        }

        [Fact]
        public void PrintCustomObjectPrintsMembers()
        {
            // Arrange
            MockObjectVisitor visitor = CreateObjectVisitor();
            var person = new Person
            {
                Name = "David",
                Age = 23.3,
                Dob = new DateTime(1986, 11, 19),
                LongType = 1000000000,
                Type = 1
            };

            using (new CultureReplacer("en-US"))
            {
                // Act            
                visitor.Print(person);

                // Assert
                Assert.Equal(9, visitor.Members.Count);
                Assert.True(visitor.Members.Contains("double Age = 23.3"));
                Assert.True(visitor.Members.Contains("string Name = David"));
                Assert.True(visitor.Members.Contains("DateTime Dob = 11/19/1986 12:00:00 AM"));
                Assert.True(visitor.Members.Contains("short Type = 1"));
                Assert.True(visitor.Members.Contains("float Float = 0"));
                Assert.True(visitor.Members.Contains("byte Byte = 0"));
                Assert.True(visitor.Members.Contains("decimal Decimal = 0"));
                Assert.True(visitor.Members.Contains("bool Bool = False"));
            }
        }

        [Fact]
        public void PrintShowsVisitedWhenCircularReferenceInObjectGraph()
        {
            // Arrange
            MockObjectVisitor visitor = CreateObjectVisitor();
            PersonNode node = new PersonNode
            {
                Person = new Person
                {
                    Name = "David",
                    Age = 23.3
                }
            };
            node.Next = node;

            // Act
            visitor.Print(node);

            // Assert            
            Assert.True(visitor.Members.Contains("string Name = David"));
            Assert.True(visitor.Members.Contains(String.Format("double Age = {0}", 23.3)));
            Assert.True(visitor.Members.Contains("PersonNode Next = Visited"));
        }

        [Fact]
        public void PrintShowsVisitedWhenCircularReferenceIsIEnumerable()
        {
            // Arrange
            MockObjectVisitor visitor = CreateObjectVisitor();
            List<object> values = new List<object>();
            values.Add(values);

            // Act
            visitor.Print(values);

            // Assert            
            Assert.Equal("Visited", visitor.Values[0]);
            Assert.Equal("Visited " + values.GetHashCode(), visitor.Visited[0]);
        }

        [Fact]
        public void PrintShowsVisitedWhenCircularReferenceIsIDictionary()
        {
            // Arrange
            MockObjectVisitor visitor = CreateObjectVisitor();
            OrderedDictionary values = new OrderedDictionary();
            values[values] = values;

            // Act
            visitor.Print(values);

            // Assert            
            Assert.Equal("Visited", visitor.Values[0]);
            Assert.Equal("Visited " + values.GetHashCode(), visitor.Visited[0]);
        }

        [Fact]
        public void PrintShowsVisitedWhenCircularReferenceIsNameValueCollection()
        {
            // Arrange
            MockObjectVisitor visitor = CreateObjectVisitor();
            NameValueCollection nameValues = new NameValueCollection();
            nameValues["id"] = "1";
            List<NameValueCollection> values = new List<NameValueCollection>();
            values.Add(nameValues);
            values.Add(nameValues);

            // Act
            visitor.Print(values);

            // Assert            
            Assert.True(visitor.Values.Contains("Visited"));
            Assert.True(visitor.Visited.Contains("Visited " + nameValues.GetHashCode()));
        }

        [Fact]
        public void PrintExcludesWriteOnlyProperties()
        {
            // Arrange
            MockObjectVisitor visitor = CreateObjectVisitor();
            ClassWithWriteOnlyProperty cls = new ClassWithWriteOnlyProperty();

            // Act
            visitor.Print(cls);

            // Assert
            Assert.Equal(0, visitor.Members.Count);
        }

        [Fact]
        public void PrintWritesEnumeratedElementsUntilLimitIsReached()
        {
            // Arrange
            MockObjectVisitor visitor = CreateObjectVisitor();
            var enumeration = Enumerable.Range(0, 2000);

            // Act
            visitor.Print(enumeration);

            // Assert
            for (int i = 0; i <= 2000; i++)
            {
                if (i < 1000)
                {
                    Assert.True(visitor.Values.Contains(i.ToString()));
                }
                else
                {
                    Assert.False(visitor.Values.Contains(i.ToString()));
                }
            }
            Assert.True(visitor.Values.Contains("Limit Exceeded"));
        }

        [Fact]
        public void PrintWithAnonymousType()
        {
            // Arrange
            MockObjectVisitor visitor = CreateObjectVisitor();
            var value = new { Name = "John", X = 1 };

            // Act
            visitor.Print(value);

            // Assert
            Assert.True(visitor.Members.Contains("string Name = John"));
            Assert.True(visitor.Members.Contains("int X = 1"));
        }

        [Fact]
        public void PrintClassWithPublicFields()
        {
            // Arrange
            MockObjectVisitor visitor = CreateObjectVisitor();
            ClassWithFields value = new ClassWithFields();
            value.Foo = "John";
            value.Bar = 1;

            // Actt
            visitor.Print(value);

            // Assert            
            Assert.True(visitor.Members.Contains("string Foo = John"));
            Assert.True(visitor.Members.Contains("int Bar = 1"));
        }

        [Fact]
        public void PrintClassWithDynamicMembersPrintsMembersIfGetDynamicMemberNamesIsImplemented()
        {
            // Arrange
            MockObjectVisitor visitor = CreateObjectVisitor();
            dynamic d = new DynamicDictionary();
            d.Cycle = d;
            d.Name = "Foo";
            d.Value = null;

            // Act
            visitor.Print(d);

            // Assert                                   
            Assert.True(visitor.Members.Contains("DynamicDictionary Cycle = Visited"));
            Assert.True(visitor.Members.Contains("string Name = Foo"));
            Assert.True(visitor.Members.Contains("Value = null"));
        }

        [Fact]
        public void PrintClassWithDynamicMembersReturningNullPrintsNoMembers()
        {
            // Arrange
            MockObjectVisitor visitor = CreateObjectVisitor();
            dynamic d = new ClassWithDynamicAnNullMemberNames();
            d.Cycle = d;
            d.Name = "Foo";
            d.Value = null;

            // Act
            visitor.Print(d);

            // Assert                                   
            Assert.False(visitor.Members.Any());
        }

        [Fact]
        public void PrintUsesToStringOfIConvertibleObjects()
        {
            // Arrange
            MockObjectVisitor visitor = CreateObjectVisitor();
            ConvertibleClass cls = new ConvertibleClass();

            // Act
            visitor.Print(cls);

            // Assert
            Assert.Equal("Test", visitor.Values[0]);
        }

        [Fact]
        public void PrintConvertsTypeToString()
        {
            // Arrange
            MockObjectVisitor visitor = CreateObjectVisitor();

            // Act
            visitor.Print(typeof(string));

            // Assert
            Assert.Equal("typeof(string)", visitor.Values[0]);
        }

        [Fact]
        [ReplaceCulture]
        public void PrintClassWithPropertyThatThrowsExceptionPrintsException()
        {
            // Arrange
            MockObjectVisitor visitor = CreateObjectVisitor();
            ClassWithPropertyThatThrowsException value = new ClassWithPropertyThatThrowsException();

            // Act
            visitor.Print(value);

            // Assert            
            Assert.Equal("int MyProperty = Property accessor 'MyProperty' on object 'System.Web.Helpers.Test.ObjectInfoTest+ClassWithPropertyThatThrowsException' threw the following exception:'Property that shows an exception'", visitor.Members[0]);
        }

        [Fact]
        public void ConvertEscapeSequencesPrintsStringEscapeSequencesAsLiterals()
        {
            // Act
            string value = HtmlObjectPrinter.ConvertEscapseSequences("\\\'\"\0\a\b\f\n\r\t\v");

            // Assert
            Assert.Equal("\\\\'\\\"\\0\\a\\b\\f\\n\\r\\t\\v", value);
        }

        [Fact]
        public void ConvertEscapeSequencesDoesNotEscapeUnicodeSequences()
        {
            // Act
            string value = HtmlObjectPrinter.ConvertEscapseSequences("\u1023\x2045");

            // Assert
            Assert.Equal("\u1023\x2045", value);
        }

        [Fact]
        public void PrintCharPrintsQuotedString()
        {
            // Arrange
            HtmlObjectPrinter printer = new HtmlObjectPrinter(100, 100);
            HtmlElement element = new HtmlElement("span");
            printer.PushElement(element);

            // Act            
            printer.VisitConvertedValue('x', "x");

            // Assert
            Assert.Equal(1, element.Children.Count);
            HtmlElement child = element.Children[0];
            Assert.Equal("'x'", child.InnerText);
            Assert.Equal("quote", child["class"]);
        }

        [Fact]
        public void PrintEscapeCharPrintsEscapedCharAsLiteral()
        {
            // Arrange
            HtmlObjectPrinter printer = new HtmlObjectPrinter(100, 100);
            HtmlElement element = new HtmlElement("span");
            printer.PushElement(element);

            // Act            
            printer.VisitConvertedValue('\t', "\t");

            // Assert
            Assert.Equal(1, element.Children.Count);
            HtmlElement child = element.Children[0];
            Assert.Equal("'\\t'", child.InnerText);
            Assert.Equal("quote", child["class"]);
        }

        [Fact]
        public void GetTypeNameConvertsGenericTypesToCsharpSyntax()
        {
            // Act
            string value = ObjectVisitor.GetTypeName(typeof(Func<Func<Func<int, int, object>, Action<int>>>));

            // Assert
            Assert.Equal("Func<Func<Func<int, int, object>, Action<int>>>", value);
        }

        private class ConvertibleClass : IConvertible
        {
            public TypeCode GetTypeCode()
            {
                throw new NotImplementedException();
            }

            public bool ToBoolean(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public byte ToByte(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public char ToChar(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public DateTime ToDateTime(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public decimal ToDecimal(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public double ToDouble(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public short ToInt16(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public int ToInt32(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public long ToInt64(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public sbyte ToSByte(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public float ToSingle(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public string ToString(IFormatProvider provider)
            {
                return "Test";
            }

            public object ToType(Type conversionType, IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public ushort ToUInt16(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public uint ToUInt32(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public ulong ToUInt64(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }
        }

        private class ClassWithPropertyThatThrowsException
        {
            public int MyProperty
            {
                get { throw new InvalidOperationException("Property that shows an exception"); }
            }
        }

        private class ClassWithDynamicAnNullMemberNames : DynamicObject
        {
            public override IEnumerable<string> GetDynamicMemberNames()
            {
                return null;
            }

            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                result = null;
                return true;
            }

            public override bool TrySetMember(SetMemberBinder binder, object value)
            {
                return true;
            }
        }

        private class Person
        {
            public string Name { get; set; }
            public double Age { get; set; }
            public DateTime Dob { get; set; }
            public short Type { get; set; }
            public long LongType { get; set; }
            public float Float { get; set; }
            public byte Byte { get; set; }
            public decimal Decimal { get; set; }
            public bool Bool { get; set; }
        }

        private class ClassWithFields
        {
            public string Foo;
            public int Bar = 13;
        }

        private class ClassWithWriteOnlyProperty
        {
            public int Value
            {
                set { }
            }
        }

        private class PersonNode
        {
            public Person Person { get; set; }
            public PersonNode Next { get; set; }
        }

        private MockObjectVisitor CreateObjectVisitor(int recursionLimit = 10, int enumerationLimit = 1000)
        {
            return new MockObjectVisitor(recursionLimit, enumerationLimit);
        }

        private class MockObjectVisitor : ObjectVisitor
        {
            public MockObjectVisitor(int recursionLimit, int enumerationLimit)
                : base(recursionLimit, enumerationLimit)
            {
                Values = new List<string>();
                KeyValuePairs = new List<string>();
                Members = new List<string>();
                Indexes = new List<int>();
                Visited = new List<string>();
            }

            public List<string> Values { get; set; }
            public List<string> KeyValuePairs { get; set; }
            public List<string> Members { get; set; }
            public List<int> Indexes { get; set; }
            public List<string> Visited { get; set; }

            public void Print(object value)
            {
                Visit(value, 0);
            }

            public override void VisitObjectVisitorException(ObjectVisitorException exception)
            {
                Values.Add(exception.InnerException.Message);
            }

            public override void VisitStringValue(string stringValue)
            {
                Values.Add(stringValue);
                base.VisitStringValue(stringValue);
            }

            public override void VisitVisitedObject(string id, object value)
            {
                Visited.Add(String.Format("Visited {0}", id));
                Values.Add("Visited");
                base.VisitVisitedObject(id, value);
            }

            public override void VisitIndexedEnumeratedValue(int index, object item, int depth)
            {
                Indexes.Add(index);
                base.VisitIndexedEnumeratedValue(index, item, depth);
            }

            public override void VisitEnumeratonLimitExceeded()
            {
                Values.Add("Limit Exceeded");
                base.VisitEnumeratonLimitExceeded();
            }

            public override void VisitMember(string name, Type type, object value, int depth)
            {
                base.VisitMember(name, type, value, depth);
                type = type ?? (value != null ? value.GetType() : null);
                if (type == null)
                {
                    Members.Add(String.Format("{0} = null", name));
                }
                else
                {
                    Members.Add(String.Format("{0} {1} = {2}", GetTypeName(type), name, Values.Last()));
                }
            }

            public override void VisitNull()
            {
                Values.Add("null");
                base.VisitNull();
            }

            public override void VisitKeyValue(object key, object value, int depth)
            {
                base.VisitKeyValue(key, value, depth);
                KeyValuePairs.Add(String.Format("{0} = {1}", Values[Values.Count - 2], Values[Values.Count - 1]));
            }
        }
    }
}
