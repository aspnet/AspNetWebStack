// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Helpers.Test
{
    public class PreComputedGridDataSourceTest
    {
        [Fact]
        public void PreSortedDataSourceReturnsRowCountItWasSpecified()
        {
            // Arrange
            int rows = 20;
            var dataSource = new PreComputedGridDataSource(new WebGrid(GetContext()), values: Enumerable.Range(0, 10).Cast<dynamic>(), totalRows: rows);

            // Act and Assert
            Assert.Equal(rows, dataSource.TotalRowCount);
        }

        [Fact]
        public void PreSortedDataSourceReturnsAllRows()
        {
            // Arrange
            var grid = new WebGrid(GetContext());
            var dataSource = new PreComputedGridDataSource(grid: grid, values: Enumerable.Range(0, 10).Cast<dynamic>(), totalRows: 10);

            // Act
            var rows = dataSource.GetRows(new SortInfo { SortColumn = String.Empty }, 0);

            // Assert
            Assert.Equal(10, rows.Count);
            Assert.Equal(0, rows.First().Value);
            Assert.Equal(9, rows.Last().Value);
        }

        private HttpContextBase GetContext()
        {
            return new Mock<HttpContextBase>().Object;
        }
    }
}
