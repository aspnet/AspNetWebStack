// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Helpers.Test
{
    public class WebGridDataSourceTest
    {
        [Fact]
        public void WebGridDataSourceReturnsNumberOfItemsAsTotalRowCount()
        {
            // Arrange
            var rows = GetValues();
            var dataSource = new WebGridDataSource(new WebGrid(GetContext()), values: GetValues(), elementType: typeof(Person), canPage: false, canSort: false);

            // Act and Assert
            Assert.Equal(rows.Count(), dataSource.TotalRowCount);
        }

        [Fact]
        public void WebGridDataSourceReturnsUnsortedListIfSortColumnIsNull()
        {
            // Arrange
            var values = GetValues();
            var dataSource = new WebGridDataSource(new WebGrid(GetContext()), values: GetValues(), elementType: typeof(Person), canPage: false, canSort: true);

            // Act 
            var rows = dataSource.GetRows(new SortInfo { SortColumn = null }, 0);

            // Assert
            Assert.True(Enumerable.SequenceEqual<object>(values.ToList(), rows.Select(r => r.Value).ToList(), new PersonComparer()));
        }

        [Fact]
        public void WebGridDataSourceReturnsUnsortedListIfSortColumnIsEmpty()
        {
            // Arrange
            var values = GetValues();
            var dataSource = new WebGridDataSource(new WebGrid(GetContext()), values: GetValues(), elementType: typeof(Person), canPage: false, canSort: true);

            // Act 
            var rows = dataSource.GetRows(new SortInfo { SortColumn = String.Empty }, 0);

            // Assert
            Assert.True(Enumerable.SequenceEqual<object>(values.ToList(), rows.Select(r => r.Value).ToList(), new PersonComparer()));
        }

        [Fact]
        public void WebGridDataSourceReturnsUnsortedListIfSortCannotBeInferred()
        {
            // Arrange
            var values = GetValues();
            var dataSource = new WebGridDataSource(new WebGrid(GetContext()), values: GetValues(), elementType: typeof(Person), canPage: false, canSort: true);

            // Act 
            var rows = dataSource.GetRows(new SortInfo { SortColumn = "Does-not-exist" }, 0);

            // Assert
            Assert.True(Enumerable.SequenceEqual<object>(values.ToList(), rows.Select(r => r.Value).ToList(), new PersonComparer()));
        }

        [Fact]
        public void WebGridDataSourceReturnsUnsortedListIfDefaultSortCannotBeInferred()
        {
            // Arrange
            var values = GetValues();
            var defaultSort = new SortInfo { SortColumn = "cannot-be-inferred" };
            var dataSource = new WebGridDataSource(new WebGrid(GetContext()), values: GetValues(), elementType: typeof(Person), canSort: true, canPage: false) { DefaultSort = defaultSort };

            // Act 
            var rows = dataSource.GetRows(new SortInfo { SortColumn = "Does-not-exist" }, 0);

            // Assert
            Assert.True(Enumerable.SequenceEqual<object>(values.ToList(), rows.Select(r => r.Value).ToList(), new PersonComparer()));
        }

        [Fact]
        public void WebGridDataSourceUsesDefaultSortWhenCurrentSortCannotBeInferred()
        {
            // Arrange
            var values = GetValues();
            var defaultSort = new SortInfo { SortColumn = "FirstName" };
            var dataSource = new WebGridDataSource(new WebGrid(GetContext()), values: GetValues(), elementType: typeof(Person), canSort: true, canPage: false) { DefaultSort = defaultSort };

            // Act 
            var rows = dataSource.GetRows(new SortInfo { SortColumn = "Does-not-exist" }, 0);

            // Assert
            Assert.True(Enumerable.SequenceEqual<object>(values.OrderBy(p => p.FirstName).ToList(), rows.Select(r => r.Value).ToList(), new PersonComparer()));
        }

        [Fact]
        public void WebGridDataSourceSortsUsingSpecifiedSort()
        {
            // Arrange
            var defaultSort = new SortInfo { SortColumn = "FirstName", SortDirection = SortDirection.Ascending };
            IEnumerable<dynamic> values = new[] { new Person { LastName = "Z" }, new Person { LastName = "X" }, new Person { LastName = "Y" } };
            var dataSource = new WebGridDataSource(new WebGrid(GetContext()), values: values, elementType: typeof(Person), canSort: true, canPage: false) { DefaultSort = defaultSort };

            // Act 
            var rows = dataSource.GetRows(new SortInfo { SortColumn = "LastName" }, 0);

            // Assert
            Assert.Equal(rows.ElementAt(0).Value.LastName, "X");
            Assert.Equal(rows.ElementAt(1).Value.LastName, "Y");
            Assert.Equal(rows.ElementAt(2).Value.LastName, "Z");
        }

        [Fact]
        public void WebGridDataSourceSortsDynamicType()
        {
            // Arrange
            IEnumerable<dynamic> values = new[] { new TestDynamicType("col", "val1"), new TestDynamicType("col", "val2"), new TestDynamicType("col", "val3") };
            var dataSource = new WebGridDataSource(new WebGrid(GetContext()), values: values, elementType: typeof(TestDynamicType), canSort: true, canPage: false);

            // Act 
            var rows = dataSource.GetRows(new SortInfo { SortColumn = "col", SortDirection = SortDirection.Descending }, 0);

            // Assert
            Assert.Equal(rows.ElementAt(0).Value.col, "val3");
            Assert.Equal(rows.ElementAt(1).Value.col, "val2");
            Assert.Equal(rows.ElementAt(2).Value.col, "val1");
        }

        [Fact]
        public void WebGridDataSourceWithNestedPropertySortsCorrectly()
        {
            // Arrange
            var element1 = new { Foo = new { Bar = "val2" } };
            var element2 = new { Foo = new { Bar = "val1" } };
            var element3 = new { Foo = new { Bar = "val3" } };
            IEnumerable<dynamic> values = new[] { element1, element2, element3 };

            var dataSource = new WebGridDataSource(new WebGrid(GetContext()), values: values, elementType: element1.GetType(), canSort: true, canPage: false);

            // Act 
            var rows = dataSource.GetRows(new SortInfo { SortColumn = "Foo.Bar", SortDirection = SortDirection.Descending }, 0);

            // Assert
            Assert.Equal(rows.ElementAt(0).Value.Foo.Bar, "val3");
            Assert.Equal(rows.ElementAt(1).Value.Foo.Bar, "val2");
            Assert.Equal(rows.ElementAt(2).Value.Foo.Bar, "val1");
        }

        [Fact]
        public void WebGridDataSourceSortsDictionaryBasedDynamicType()
        {
            // Arrange
            var value1 = new DynamicDictionary();
            value1["col"] = "val1";
            var value2 = new DynamicDictionary();
            value2["col"] = "val2";
            var value3 = new DynamicDictionary();
            value3["col"] = "val3";
            IEnumerable<dynamic> values = new[] { value1, value2, value3 };
            var dataSource = new WebGridDataSource(new WebGrid(GetContext()), values: values, elementType: typeof(TestDynamicType), canSort: true, canPage: false);

            // Act 
            var rows = dataSource.GetRows(new SortInfo { SortColumn = "col", SortDirection = SortDirection.Descending }, 0);

            // Assert
            Assert.Equal(rows.ElementAt(0).Value.col, "val3");
            Assert.Equal(rows.ElementAt(1).Value.col, "val2");
            Assert.Equal(rows.ElementAt(2).Value.col, "val1");
        }

        [Fact]
        public void WebGridDataSourceReturnsOriginalDataSourceIfValuesCannotBeSorted()
        {
            // Arrange
            IEnumerable<dynamic> values = new object[] { new TestDynamicType("col", "val1"), new TestDynamicType("col", "val2"), new TestDynamicType("col", DBNull.Value) };
            var dataSource = new WebGridDataSource(new WebGrid(GetContext()), values: values, elementType: typeof(TestDynamicType), canSort: true, canPage: false);

            // Act 
            var rows = dataSource.GetRows(new SortInfo { SortColumn = "col", SortDirection = SortDirection.Descending }, 0);

            // Assert
            Assert.Equal(rows.ElementAt(0).Value.col, "val1");
            Assert.Equal(rows.ElementAt(1).Value.col, "val2");
            Assert.Equal(rows.ElementAt(2).Value.col, DBNull.Value);
        }

        [Fact]
        public void WebGridDataSourceReturnsPagedResultsIfRowsPerPageIsSpecified()
        {
            // Arrange
            IEnumerable<dynamic> values = GetValues();
            var dataSource = new WebGridDataSource(new WebGrid(GetContext()), values: values, elementType: typeof(Person), canSort: false, canPage: true) { RowsPerPage = 2 };

            // Act 
            var rows = dataSource.GetRows(new SortInfo(), 0);

            // Assert
            Assert.Equal(2, rows.Count);
            Assert.Equal(rows.ElementAt(0).Value.LastName, "B2");
            Assert.Equal(rows.ElementAt(1).Value.LastName, "A2");
        }

        [Fact]
        public void WebGridDataSourceReturnsPagedSortedResultsIfRowsPerPageAndSortAreSpecified()
        {
            // Arrange
            IEnumerable<dynamic> values = GetValues();
            var dataSource = new WebGridDataSource(new WebGrid(GetContext()), values: values, elementType: typeof(Person), canSort: true, canPage: true) { RowsPerPage = 2 };

            // Act 
            var rows = dataSource.GetRows(new SortInfo { SortColumn = "LastName", SortDirection = SortDirection.Descending }, 0);

            // Assert
            Assert.Equal(2, rows.Count);
            Assert.Equal(rows.ElementAt(0).Value.LastName, "E2");
            Assert.Equal(rows.ElementAt(1).Value.LastName, "D2");
        }

        [Fact]
        public void WebGridDataSourceReturnsFewerThanRowsPerPageIfNumberOfItemsIsInsufficient()
        {
            // Arrange
            IEnumerable<dynamic> values = GetValues();
            var dataSource = new WebGridDataSource(new WebGrid(GetContext()), values: values, elementType: typeof(Person), canSort: true, canPage: true) { RowsPerPage = 3 };

            // Act 
            var rows = dataSource.GetRows(new SortInfo(), 1);

            // Assert
            Assert.Equal(2, rows.Count);
            Assert.Equal(rows.ElementAt(0).Value.LastName, "C2");
            Assert.Equal(rows.ElementAt(1).Value.LastName, "E2");
        }

        [Fact]
        public void WebGridDataSourceDoesNotThrowIfValuesAreNull()
        {
            // Arrange
            IEnumerable<dynamic> values = new object[] { String.Empty, null, DBNull.Value, null };
            var dataSource = new WebGridDataSource(new WebGrid(GetContext()), values: values, elementType: typeof(object), canSort: true, canPage: true) { RowsPerPage = 2 };

            // Act 
            var rows = dataSource.GetRows(new SortInfo(), 0);

            // Assert
            Assert.Equal(2, rows.Count);
            Assert.Equal(rows.ElementAt(0).Value, String.Empty);
            Assert.Null(rows.ElementAt(1).Value);
        }

        private IEnumerable<Person> GetValues()
        {
            return new[]
            {
                new Person { FirstName = "B1", LastName = "B2" },
                new Person { FirstName = "A1", LastName = "A2" },
                new Person { FirstName = "D1", LastName = "D2" },
                new Person { FirstName = "C1", LastName = "C2" },
                new Person { FirstName = "E1", LastName = "E2" },
            };
        }

        private class PersonComparer : IEqualityComparer<object>
        {
            public new bool Equals(object x, object y)
            {
                dynamic xDynamic = x;
                dynamic yDynamic = y;
                return (String.Equals(xDynamic.FirstName, yDynamic.FirstName, StringComparison.OrdinalIgnoreCase)
                        && String.Equals(xDynamic.LastName, yDynamic.LastName, StringComparison.OrdinalIgnoreCase));
            }

            public int GetHashCode(dynamic obj)
            {
                return 4; // Random dice roll
            }
        }

        private class TestDynamicType : DynamicObject
        {
            public Dictionary<string, object> _values = new Dictionary<string, object>();

            public TestDynamicType(string a, object b)
            {
                _values[a] = b;
            }

            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                return _values.TryGetValue(binder.Name, out result);
            }
        }

        private class Person
        {
            public string FirstName { get; set; }

            public string LastName { get; set; }
        }

        private HttpContextBase GetContext()
        {
            return new Mock<HttpContextBase>().Object;
        }
    }
}
