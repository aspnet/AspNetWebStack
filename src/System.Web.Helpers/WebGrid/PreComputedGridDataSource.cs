// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace System.Web.Helpers
{
    /// <summary>
    /// Source wrapper for data provided by the user that is already sorted and paged. The user provides the WebGrid the rows to bind and additionally the total number of rows that 
    /// are available.
    /// </summary>
    internal sealed class PreComputedGridDataSource : IWebGridDataSource
    {
        private readonly int _totalRows;
        private readonly IList<WebGridRow> _rows;

        public PreComputedGridDataSource(WebGrid grid, IEnumerable<dynamic> values, int totalRows)
        {
            Debug.Assert(grid != null);
            Debug.Assert(values != null);

            _totalRows = totalRows;
            _rows = values.Select((value, index) => new WebGridRow(grid, value: value, rowIndex: index)).ToList();
        }

        public int TotalRowCount
        {
            get { return _totalRows; }
        }

        public IList<WebGridRow> GetRows(SortInfo sortInfo, int pageIndex)
        {
            // Data is already sorted and paged. Ignore parameters.
            return _rows;
        }
    }
}
