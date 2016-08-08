// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Description;
using System.Web.Mvc;
using Microsoft.TestCommon;
using ROOT_PROJECT_NAMESPACE.Areas.HelpPage;

namespace WebApiHelpPageWebHost.UnitTest
{
    public class ObjectGeneratorTest
    {
        [Theory]
        [InlineData(typeof(bool))]
        [InlineData(typeof(byte))]
        [InlineData(typeof(char))]
        [InlineData(typeof(DBNull))]
        [InlineData(typeof(Decimal))]
        [InlineData(typeof(double))]
        [InlineData(typeof(Guid))]
        [InlineData(typeof(int))]
        [InlineData(typeof(string))]
        [InlineData(typeof(float))]
        [InlineData(typeof(long))]
        [InlineData(typeof(uint))]
        [InlineData(typeof(ulong))]
        [InlineData(typeof(sbyte))]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(DateTimeOffset))]
        [InlineData(typeof(TimeSpan))]
        [InlineData(typeof(UInt16))]
        [InlineData(typeof(Int16))]
        [InlineData(typeof(Guid))]
        [InlineData(typeof(Uri))]
        public void GenerateObject_SimpleTypes(Type type)
        {
            ObjectGenerator objectGenerator = new ObjectGenerator();
            object instance = objectGenerator.GenerateObject(type);
            Assert.NotNull(instance);
            Assert.IsType(type, instance);
        }

        [Theory]
        [InlineData(typeof(KeyValuePair<string, int>))]
        [InlineData(typeof(KeyValuePair<string, KeyValuePair<int, DateTime>>))]
        [InlineData(typeof(Tuple<string>))]
        [InlineData(typeof(Tuple<string, int>))]
        [InlineData(typeof(Tuple<string, int, double, DateTime>))]
        [InlineData(typeof(Tuple<string, int, double, DateTime, Tuple<int?>>))]
        [InlineData(typeof(Tuple<string, int, double, DateTime, Tuple<int?>, Tuple<double, string>>))]
        [InlineData(typeof(Tuple<string, int, double, Type, DateTimeOffset, byte, sbyte>))]
        [InlineData(typeof(Tuple<string, int, double, Type, DateTimeOffset, byte, sbyte, Tuple<char, uint>>))]
        [InlineData(typeof(MyGenericType<int, string>))]
        [InlineData(typeof(MyGenericType<int, Tuple<string, DateTime>>))]
        [InlineData(typeof(MyGenericType<MyGenericType<DateTime, HttpVerbs>, string>))]
        public void GenerateObject_Generic(Type genericType)
        {
            ObjectGenerator objectGenerator = new ObjectGenerator();
            object instance = objectGenerator.GenerateObject(genericType);
            Assert.NotNull(instance);
            Assert.IsType(genericType, instance);
        }

        [Theory]
        [InlineData(typeof(IEnumerable<string>), typeof(string))]
        [InlineData(typeof(IEnumerable<List<string>>), typeof(List<string>))]
        [InlineData(typeof(IEnumerable<int>), typeof(int))]
        [InlineData(typeof(IEnumerable<Nullable<double>>), typeof(double))]
        [InlineData(typeof(IEnumerable<DateTime>), typeof(DateTime))]
        [InlineData(typeof(ICollection<string>), typeof(string))]
        [InlineData(typeof(ICollection<int>), typeof(int))]
        [InlineData(typeof(ICollection<Nullable<double>>), typeof(double))]
        [InlineData(typeof(ICollection<DateTime>), typeof(DateTime))]
        [InlineData(typeof(IList<string>), typeof(string))]
        [InlineData(typeof(IList<int>), typeof(int))]
        [InlineData(typeof(IList<Nullable<double>>), typeof(double))]
        [InlineData(typeof(IList<DateTime>), typeof(DateTime))]
        [InlineData(typeof(List<string>), typeof(string))]
        [InlineData(typeof(List<int>), typeof(int))]
        [InlineData(typeof(List<Nullable<double>>), typeof(double))]
        [InlineData(typeof(List<DateTime>), typeof(DateTime))]
        [InlineData(typeof(IEnumerable<KeyValuePair<string, int>>), typeof(KeyValuePair<string, int>))]
        [InlineData(typeof(IEnumerable<KeyValuePair<DateTime, int?>>), typeof(KeyValuePair<DateTime, int?>))]
        [InlineData(typeof(IEnumerable), typeof(object))]
        [InlineData(typeof(ICollection), typeof(object))]
        [InlineData(typeof(IList), typeof(object))]
        [InlineData(typeof(ArrayList), typeof(object))]
        [InlineData(typeof(HashSet<string>), typeof(string))]
        public void GenerateObject_Collection(Type collectionType, Type itemType)
        {
            ObjectGenerator objectGenerator = new ObjectGenerator();
            IEnumerable collection = objectGenerator.GenerateObject(collectionType) as IEnumerable;
            Assert.NotEmpty(collection);
            foreach (var item in collection)
            {
                Assert.NotNull(item);
                Assert.IsType(itemType, item);
            }
        }

        [Theory]
        [InlineData(typeof(IQueryable<string>), typeof(string))]
        [InlineData(typeof(IQueryable<Customer>), typeof(Customer))]
        [InlineData(typeof(IQueryable<HttpVerbs[]>), typeof(HttpVerbs[]))]
        [InlineData(typeof(IQueryable<List<DateTime>>), typeof(List<DateTime>))]
        [InlineData(typeof(IQueryable), typeof(object))]
        public void GenerateObject_IQueryable(Type collectionType, Type itemType)
        {
            ObjectGenerator objectGenerator = new ObjectGenerator();
            IQueryable collection = objectGenerator.GenerateObject(collectionType) as IQueryable;
            Assert.NotEmpty(collection);
            foreach (var item in collection)
            {
                Assert.NotNull(item);
                Assert.IsType(itemType, item);
            }
        }

        [Theory]
        [InlineData(typeof(string[]), typeof(string))]
        [InlineData(typeof(int[]), typeof(int))]
        [InlineData(typeof(double?[]), typeof(double))]
        [InlineData(typeof(DateTime[]), typeof(DateTime))]
        [InlineData(typeof(Customer[]), typeof(Customer))]
        [InlineData(typeof(HttpVerbs[]), typeof(HttpVerbs))]
        public void GenerateObject_Array(Type arrayType, Type elementType)
        {
            ObjectGenerator objectGenerator = new ObjectGenerator();
            Array array = objectGenerator.GenerateObject(arrayType) as Array;
            Assert.NotEmpty(array);
            foreach (var item in array)
            {
                Assert.NotNull(item);
                Assert.IsType(elementType, item);
            }
        }

        [Theory]
        [InlineData(typeof(Dictionary<string, int>), typeof(string), typeof(int))]
        [InlineData(typeof(Dictionary<string, Dictionary<Customer, Order>>), typeof(string), typeof(Dictionary<Customer, Order>))]
        [InlineData(typeof(Hashtable), typeof(object), typeof(object))]
        [InlineData(typeof(IDictionary), typeof(object), typeof(object))]
        [InlineData(typeof(IDictionary<string, DateTime>), typeof(string), typeof(DateTime))]
        [InlineData(typeof(SortedDictionary<string, Guid>), typeof(string), typeof(Guid))]
        [InlineData(typeof(OrderedDictionary), typeof(object), typeof(object))]
        [InlineData(typeof(ConcurrentDictionary<Guid, DateTime>), typeof(Guid), typeof(DateTime))]
        public void GenerateObject_Dictionary(Type dictionaryType, Type keyType, Type valueType)
        {
            ObjectGenerator objectGenerator = new ObjectGenerator();
            IDictionary dictionary = objectGenerator.GenerateObject(dictionaryType) as IDictionary;
            Assert.NotEmpty(dictionary);
            foreach (var key in dictionary.Keys)
            {
                Assert.NotNull(key);
                Assert.IsType(keyType, key);
            }
            foreach (var value in dictionary.Values)
            {
                Assert.NotNull(value);
                Assert.IsType(valueType, value);
            }
        }

        [Theory]
        [InlineData(typeof(HttpStatusCode))]
        [InlineData(typeof(HttpVerbs))]
        [InlineData(typeof(ApiParameterSource))]
        [InlineData(typeof(SampleDirection))]
        [InlineData(typeof(ConsoleColor))]
        [InlineData(typeof(HttpCompletionOption))]
        public void GenerateObject_Enum(Type type)
        {
            ObjectGenerator objectGenerator = new ObjectGenerator();
            object enumValue = objectGenerator.GenerateObject(type);
            Assert.NotNull(enumValue);
            Assert.True(Enum.IsDefined(type, enumValue));
        }

        [Theory]
        [InlineData(typeof(HttpResponseMessage))]
        [InlineData(typeof(ComplexTypeWithPublicFields))]
        [InlineData(typeof(ComplexStruct))]
        [InlineData(typeof(ApiDescription))]
        [InlineData(typeof(ApiParameterDescription))]
        [InlineData(typeof(Customer))] // Circular reference
        [InlineData(typeof(Order))] // Circular reference
        [InlineData(typeof(Item))] // Circular reference
        public void GenerateObject_ComplexType(Type type)
        {
            ObjectGenerator objectGenerator = new ObjectGenerator();
            object instance = objectGenerator.GenerateObject(type);
            Assert.NotNull(instance);
            Assert.IsType(type, instance);
        }

        [Theory]
        [InlineData(typeof(TypeWithNoDefaultConstructor))]
        [InlineData(typeof(NonPublicType))]
        [InlineData(typeof(EmptyEnum))]
        [InlineData(typeof(HttpRequestMessage))]
        [InlineData(typeof(KeyValuePair<EmptyEnum, NonPublicType>))]
        [InlineData(typeof(Tuple<EmptyEnum, NonPublicType, TypeWithNoDefaultConstructor>))]
        [InlineData(typeof(List<EmptyEnum>))]
        [InlineData(typeof(Dictionary<EmptyEnum, NonPublicType>))]
        [InlineData(typeof(EmptyEnum[]))]
        [InlineData(typeof(IQueryable<EmptyEnum>))]
        public void GenerateObject_ReturnsNull_WhenInstanceCannotBeCreated(Type type)
        {
            ObjectGenerator objectGenerator = new ObjectGenerator();
            Assert.Null(objectGenerator.GenerateObject(type));
        }
    }
}
