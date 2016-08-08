// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Filters
{
    public class HttpFilterCollectionTest
    {
        private readonly IFilter _filter = new Mock<IFilter>().Object;
        private readonly HttpFilterCollection _collection = new HttpFilterCollection();

        [Fact]
        public void Add_WhenFilterParameterIsNull_Throws()
        {
            Assert.ThrowsArgumentNull(() => _collection.Add(filter: null), "filter");
        }

        [Fact]
        public void Add_AddsFilterWithGlobalScope()
        {
            _collection.Add(_filter);

            Assert.Same(_filter, _collection.First().Instance);
            Assert.Equal(FilterScope.Global, _collection.First().Scope);
        }

        [Fact]
        public void Add_AllowsAddingSameInstanceMultipleTimes()
        {
            _collection.Add(_filter);
            _collection.Add(_filter);

            Assert.Equal(2, _collection.Count);
        }

        [Fact]
        public void AddRange_AddsAllFilters()
        {
            IFilter[] filters = { _filter, _filter };

            _collection.AddRange(filters);

            Assert.Equal(filters.Length, _collection.Count);
            Assert.True(filters.All(_collection.Contains));
        }

        [Fact]
        public void AddRange_AddsAllFiltersWithGlobalScope()
        {
            IFilter[] filters = { _filter, _filter };

            _collection.AddRange(filters);

            Assert.True(_collection.All(f => FilterScope.Global == f.Scope));
        }

        [Fact]
        public void AddRange_ValidatesNotNull()
        {
            IFilter[] filters = { _filter, null };

            Assert.Throws<ArgumentException>(
                () => _collection.AddRange(filters),
                "The parameter 'filters' cannot contain a null element." + Environment.NewLine +
                "Parameter name: filters");

            Assert.Equal(0, _collection.Count);
        }

        [Fact]
        public void Clear_EmptiesCollection()
        {
            _collection.Add(_filter);

            _collection.Clear();

            Assert.Equal(0, _collection.Count);
        }

        [Fact]
        public void Contains_WhenFilterNotInCollection_ReturnsFalse()
        {
            Assert.False(_collection.Contains(_filter));
        }

        [Fact]
        public void Contains_WhenFilterInCollection_ReturnsTrue()
        {
            _collection.Add(_filter);

            Assert.True(_collection.Contains(_filter));
        }

        [Fact]
        public void Count_WhenCollectionIsEmpty_ReturnsZero()
        {
            Assert.Equal(0, _collection.Count);
        }

        [Fact]
        public void Count_WhenItemAddedToCollection_ReturnsOne()
        {
            _collection.Add(_filter);

            Assert.Equal(1, _collection.Count);
        }

        [Fact]
        public void Remove_WhenCollectionDoesNotHaveFilter_DoesNothing()
        {
            _collection.Remove(_filter);

            Assert.Equal(0, _collection.Count);
        }

        [Fact]
        public void Remove_WhenCollectionHasFilter_RemovesIt()
        {
            _collection.Add(_filter);
            _collection.Add(_filter);

            _collection.Remove(_filter);

            Assert.Equal(0, _collection.Count);
        }
    }
}
