// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Dynamic;
using Microsoft.TestCommon;

namespace System.Web.Helpers.Test
{
    public class JsonTest
    {
        [Fact]
        public void EncodeWithDynamicObject()
        {
            // Arrange
            dynamic obj = new DummyDynamicObject();
            obj.Name = "Hello";
            obj.Age = 1;
            obj.Grades = new[] { "A", "B", "C" };

            // Act
            string json = Json.Encode(obj);

            // Assert
            Assert.Equal("{\"Name\":\"Hello\",\"Age\":1,\"Grades\":[\"A\",\"B\",\"C\"]}", json);
        }

        [Fact]
        public void EncodeArray()
        {
            // Arrange
            object input = new string[] { "one", "2", "three", "4" };

            // Act
            string json = Json.Encode(input);

            // Assert
            Assert.Equal("[\"one\",\"2\",\"three\",\"4\"]", json);
        }

        [Fact]
        public void EncodeDynamicJsonArrayEncodesAsArray()
        {
            // Arrange
            dynamic array = Json.Decode("[1,2,3]");

            // Act
            string json = Json.Encode(array);

            // Assert
            Assert.Equal("[1,2,3]", json);
        }

        [Fact]
        public void EncodeObjectWithArrayProperty()
        {
            // Arrange
            dynamic personJson = Json.Decode("{name:'George', aliases: ['Peter', 'David']}");

            // Act
            string json = Json.Encode(personJson);

            // Assert
            Assert.Equal("{\"name\":\"George\",\"aliases\":[\"Peter\",\"David\"]}", json);
        }

        [Fact]
        public void DecodeDynamicObject()
        {
            // Act
            var obj = Json.Decode("{\"Name\":\"Hello\",\"Age\":1,\"Grades\":[\"A\",\"B\",\"C\"]}");

            // Assert
            Assert.Equal("Hello", obj.Name);
            Assert.Equal(1, obj.Age);
            Assert.Equal(3, obj.Grades.Length);
            Assert.Equal("A", obj.Grades[0]);
            Assert.Equal("B", obj.Grades[1]);
            Assert.Equal("C", obj.Grades[2]);
        }

        [Fact]
        public void DecodeDynamicObjectImplicitConversionToDictionary()
        {
            // Act
            IDictionary<string, object> values = Json.Decode("{\"Name\":\"Hello\",\"Age\":1}");

            // Assert
            Assert.Equal("Hello", values["Name"]);
            Assert.Equal(1, values["Age"]);
        }

        [Fact]
        public void DecodeArrayImplicitConversionToArrayAndObjectArray()
        {
            // Act
            Array array = Json.Decode("[1,2,3]");
            object[] objArray = Json.Decode("[1,2,3]");
            IEnumerable<dynamic> dynamicEnumerable = Json.Decode("[{a:1}]");

            // Assert
            Assert.NotNull(array);
            Assert.NotNull(objArray);
            Assert.NotNull(dynamicEnumerable);
        }

        [Fact]
        public void DecodeArrayImplicitConversionToArrayArrayValuesAreDynamic()
        {
            // Act            
            dynamic[] objArray = Json.Decode("[{\"A\":1}]");

            // Assert
            Assert.NotNull(objArray);
            Assert.Equal(1, objArray[0].A);
        }

        [Fact]
        public void DecodeDynamicObjectAccessPropertiesByIndexer()
        {
            // Arrange
            var obj = Json.Decode("{\"Name\":\"Hello\",\"Age\":1,\"Grades\":[\"A\",\"B\",\"C\"]}");

            // Assert
            Assert.Equal("Hello", obj["Name"]);
            Assert.Equal(1, obj["Age"]);
            Assert.Equal(3, obj["Grades"].Length);
            Assert.Equal("A", obj["Grades"][0]);
            Assert.Equal("B", obj["Grades"][1]);
            Assert.Equal("C", obj["Grades"][2]);
        }

        [Fact]
        public void DecodeDynamicObjectAccessPropertiesByNullIndexerReturnsNull()
        {
            // Arrange
            var obj = Json.Decode("{\"Name\":\"Hello\",\"Age\":1,\"Grades\":[\"A\",\"B\",\"C\"]}");

            // Assert
            Assert.Null(obj[null]);
        }

        [Fact]
        public void DecodeDateTime()
        {
            // Act
            DateTime dateTime = Json.Decode("\"\\/Date(940402800000)\\/\"");

            // Assert
            Assert.Equal(1999, dateTime.Year);
            Assert.Equal(10, dateTime.Month);
            Assert.Equal(20, dateTime.Day);
        }

        [Fact]
        public void DecodeNumber()
        {
            // Act
            int number = Json.Decode("1");

            // Assert
            Assert.Equal(1, number);
        }

        [Fact]
        public void DecodeString()
        {
            // Act
            string @string = Json.Decode("\"1\"");

            // Assert
            Assert.Equal("1", @string);
        }

        [Fact]
        public void DecodeArray()
        {
            // Act
            var values = Json.Decode("[11,12,13,14,15]");

            // Assert            
            Assert.Equal(5, values.Length);
            Assert.Equal(11, values[0]);
            Assert.Equal(12, values[1]);
            Assert.Equal(13, values[2]);
            Assert.Equal(14, values[3]);
            Assert.Equal(15, values[4]);
        }

        [Fact]
        public void DecodeObjectWithArrayProperty()
        {
            // Act
            var obj = Json.Decode("{\"A\":1,\"B\":[1,3,4]}");
            object[] bValues = obj.B;

            // Assert
            Assert.Equal(1, obj.A);
            Assert.Equal(1, bValues[0]);
            Assert.Equal(3, bValues[1]);
            Assert.Equal(4, bValues[2]);
        }

        [Fact]
        public void DecodeArrayWithObjectValues()
        {
            // Act
            var obj = Json.Decode("[{\"A\":1},{\"B\":3, \"C\": \"hello\"}]");

            // Assert
            Assert.Equal(2, obj.Length);
            Assert.Equal(1, obj[0].A);
            Assert.Equal(3, obj[1].B);
            Assert.Equal("hello", obj[1].C);
        }

        [Fact]
        public void DecodeArraySetValues()
        {
            // Arrange
            var values = Json.Decode("[1,2,3,4,5]");
            for (int i = 0; i < values.Length; i++)
            {
                values[i]++;
            }

            // Assert
            Assert.Equal(5, values.Length);
            Assert.Equal(2, values[0]);
            Assert.Equal(3, values[1]);
            Assert.Equal(4, values[2]);
            Assert.Equal(5, values[3]);
            Assert.Equal(6, values[4]);
        }

        [Fact]
        public void DecodeArrayPassToMethodThatTakesArray()
        {
            // Arrange
            var values = Json.Decode("[3,2,1]");

            // Act
            int index = Array.IndexOf(values, 2);

            // Assert
            Assert.Equal(1, index);
        }

        [Fact]
        public void DecodeArrayGetEnumerator()
        {
            // Arrange
            var values = Json.Decode("[1,2,3]");

            // Assert
            int val = 1;
            foreach (var value in values)
            {
                Assert.Equal(val, val);
                val++;
            }
        }

        [Fact]
        public void DecodeObjectPropertyAccessIsSameObjectInstance()
        {
            // Arrange
            var obj = Json.Decode("{\"Name\":{\"Version:\":4.0, \"Key\":\"Key\"}}");

            // Assert
            Assert.Same(obj.Name, obj.Name);
        }

        [Fact]
        public void DecodeArrayAccessingMembersThatDontExistReturnsNull()
        {
            // Act
            var obj = Json.Decode("[\"a\", \"b\"]");

            // Assert
            Assert.Null(obj.PropertyThatDoesNotExist);
        }

        [Fact]
        public void DecodeObjectSetProperties()
        {
            // Act
            var obj = Json.Decode("{\"A\":{\"B\":100}}");
            obj.A.B = 20;

            // Assert
            Assert.Equal(20, obj.A.B);
        }

        [Fact]
        public void DecodeObjectSettingObjectProperties()
        {
            // Act
            var obj = Json.Decode("{\"A\":1}");
            obj.A = new { B = 1, D = 2 };

            // Assert
            Assert.Equal(1, obj.A.B);
            Assert.Equal(2, obj.A.D);
        }

        [Fact]
        public void DecodeObjectWithArrayPropertyPassPropertyToMethodThatTakesArray()
        {
            // Arrange
            var obj = Json.Decode("{\"A\":[3,2,1]}");

            // Act
            Array.Sort(obj.A);

            // Assert
            Assert.Equal(1, obj.A[0]);
            Assert.Equal(2, obj.A[1]);
            Assert.Equal(3, obj.A[2]);
        }

        [Fact]
        public void DecodeObjectAccessingMembersThatDontExistReturnsNull()
        {
            // Act
            var obj = Json.Decode("{\"A\":1}");

            // Assert
            Assert.Null(obj.PropertyThatDoesntExist);
        }

        [Fact]
        public void DecodeObjectWithSpecificType()
        {
            // Act
            var person = Json.Decode<Person>("{\"Name\":\"David\", \"Age\":2}");

            // Assert
            Assert.Equal("David", person.Name);
            Assert.Equal(2, person.Age);
        }

        [Fact]
        public void DecodeObjectWithImplicitConversionToNonDynamicTypeThrows()
        {
            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => { Person person = Json.Decode("{\"Name\":\"David\", \"Age\":2, \"Address\":{\"Street\":\"Bellevue\"}}"); }, "Unable to convert to \"System.Web.Helpers.Test.JsonTest+Person\". Use Json.Decode<T> instead.");
        }

        private class DummyDynamicObject : DynamicObject
        {
            private IDictionary<string, object> _values = new Dictionary<string, object>();

            public override IEnumerable<string> GetDynamicMemberNames()
            {
                return _values.Keys;
            }

            public override bool TrySetMember(SetMemberBinder binder, object value)
            {
                _values[binder.Name] = value;
                return true;
            }

            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                return _values.TryGetValue(binder.Name, out result);
            }
        }

        private class Person
        {
            public string Name { get; set; }
            public int Age { get; set; }
            public int GPA { get; set; }
            public Address Address { get; set; }
        }

        private class Address
        {
            public string Street { get; set; }
        }
    }
}
