// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web.Helpers.Resources;
using System.Web.WebPages;
using Microsoft.Internal.Web.Utils;

namespace System.Web.Helpers
{
    public class WebGrid
    {
        private const string AjaxUpdateScript = "$({1}).swhgLoad({0},{1}{2});";
        private readonly HttpContextBase _context;
        private readonly bool _canPage;
        private readonly bool _canSort;
        private readonly string _ajaxUpdateContainerId;
        private readonly string _ajaxUpdateCallback;
        private readonly string _defaultSort;
        private readonly string _pageFieldName = "page";
        private readonly string _sortDirectionFieldName = "sortdir";
        private readonly string _selectionFieldName = "row";
        private readonly string _sortFieldName = "sort";
        private readonly string _fieldNamePrefix;
        private int _pageIndex = -1;
        private bool _pageIndexSet;
        private int _rowsPerPage;
        private int _selectedIndex = -1;
        private bool _selectedIndexSet;
        private string _sortColumn;
        private bool _sortColumnSet;
        private bool _sortColumnExplicitlySet;
        private SortDirection _sortDirection;
        private bool _sortDirectionSet;
        private IWebGridDataSource _dataSource;
        private bool _dataSourceBound;
        private bool _dataSourceMaterialized;
        private IEnumerable<string> _columnNames;
        private Type _elementType;
        private IList<WebGridRow> _rows;

        /// <param name="source">Data source</param>
        /// <param name="columnNames">Data source column names. Auto-populated by default.</param>
        /// <param name="defaultSort">Default sort column.</param>
        /// <param name="rowsPerPage">Number of rows per page.</param>
        /// <param name="canPage"></param>
        /// <param name="canSort"></param>
        /// <param name="ajaxUpdateContainerId">ID for the grid's container element. This enables AJAX support.</param>
        /// <param name="ajaxUpdateCallback">Callback function for the AJAX functionality once the update is complete</param>
        /// <param name="fieldNamePrefix">Prefix for query string fields to support multiple grids.</param>
        /// <param name="pageFieldName">Query string field name for page number.</param>
        /// <param name="selectionFieldName">Query string field name for selected row number.</param>
        /// <param name="sortFieldName">Query string field name for sort column.</param>
        /// <param name="sortDirectionFieldName">Query string field name for sort direction.</param>
#if CODE_COVERAGE 
        [ExcludeFromCodeCoverage]
#endif
        public WebGrid(
            IEnumerable<dynamic> source = null,
            IEnumerable<string> columnNames = null,
            string defaultSort = null,
            int rowsPerPage = 10,
            bool canPage = true,
            bool canSort = true,
            string ajaxUpdateContainerId = null,
            string ajaxUpdateCallback = null,
            string fieldNamePrefix = null,
            string pageFieldName = null,
            string selectionFieldName = null,
            string sortFieldName = null,
            string sortDirectionFieldName = null)
            : this(new HttpContextWrapper(Web.HttpContext.Current), defaultSort: defaultSort, rowsPerPage: rowsPerPage, canPage: canPage,
                   canSort: canSort, ajaxUpdateContainerId: ajaxUpdateContainerId, ajaxUpdateCallback: ajaxUpdateCallback, fieldNamePrefix: fieldNamePrefix, pageFieldName: pageFieldName,
                   selectionFieldName: selectionFieldName, sortFieldName: sortFieldName, sortDirectionFieldName: sortDirectionFieldName)
        {
            if (source != null)
            {
                Bind(source, columnNames);
            }
        }

        // NOTE: WebGrid uses an IEnumerable<dynamic> data source instead of IEnumerable<T> to avoid generics in the syntax.
        internal WebGrid(
            HttpContextBase context,
            string defaultSort = null,
            int rowsPerPage = 10,
            bool canPage = true,
            bool canSort = true,
            string ajaxUpdateContainerId = null,
            string ajaxUpdateCallback = null,
            string fieldNamePrefix = null,
            string pageFieldName = null,
            string selectionFieldName = null,
            string sortFieldName = null,
            string sortDirectionFieldName = null)
        {
            Debug.Assert(context != null);

            if (rowsPerPage < 1)
            {
                throw new ArgumentOutOfRangeException("rowsPerPage", String.Format(CultureInfo.CurrentCulture,
                                                                                   CommonResources.Argument_Must_Be_GreaterThanOrEqualTo, 1));
            }

            _context = context;
            _defaultSort = defaultSort;
            _rowsPerPage = rowsPerPage;
            _canPage = canPage;
            _canSort = canSort;
            _ajaxUpdateContainerId = ajaxUpdateContainerId;
            _ajaxUpdateCallback = ajaxUpdateCallback;

            _fieldNamePrefix = fieldNamePrefix;

            if (!String.IsNullOrEmpty(pageFieldName))
            {
                _pageFieldName = pageFieldName;
            }
            if (!String.IsNullOrEmpty(selectionFieldName))
            {
                _selectionFieldName = selectionFieldName;
            }
            if (!String.IsNullOrEmpty(sortFieldName))
            {
                _sortFieldName = sortFieldName;
            }
            if (!String.IsNullOrEmpty(sortDirectionFieldName))
            {
                _sortDirectionFieldName = sortDirectionFieldName;
            }

            CustomSorters = new Dictionary<string, Expression>(StringComparer.OrdinalIgnoreCase);
        }

        public IEnumerable<string> ColumnNames
        {
            get
            {
                // Review: Assuming that the users always binds the source and provides column names / we infer the default columns names on binding
                // Would not work if we want to allow column names to be independently set.
                EnsureDataBound();
                return _columnNames;
            }
        }

        public bool CanSort
        {
            get { return _canSort; }
        }

        public string AjaxUpdateContainerId
        {
            get { return _ajaxUpdateContainerId; }
        }

        public bool IsAjaxEnabled
        {
            get { return !String.IsNullOrEmpty(_ajaxUpdateContainerId); }
        }

        public string AjaxUpdateCallback
        {
            get { return _ajaxUpdateCallback; }
        }

        public string FieldNamePrefix
        {
            get { return _fieldNamePrefix ?? String.Empty; }
        }

        public bool HasSelection
        {
            get { return SelectedIndex >= 0; }
        }

        public int PageCount
        {
            get
            {
                if (!_canPage)
                {
                    return 1;
                }
                return (int)Math.Ceiling((double)TotalRowCount / RowsPerPage);
            }
        }

        public string PageFieldName
        {
            get { return FieldNamePrefix + _pageFieldName; }
        }

        public int PageIndex
        {
            get
            {
                if (!_canPage)
                {
                    //Default page index is 0
                    return 0;
                }
                if (!_pageIndexSet)
                {
                    int page;
                    if (!_canPage || !Int32.TryParse(QueryString[PageFieldName], out page) || (page < 1))
                    {
                        page = 1;
                    }

                    if (_dataSourceBound && page > PageCount)
                    {
                        page = PageCount;
                    }

                    _pageIndex = page - 1;
                    _pageIndexSet = true;
                }
                return _pageIndex;
            }
            set
            {
                if (!_canPage)
                {
                    throw new NotSupportedException(HelpersResources.WebGrid_NotSupportedIfPagingIsDisabled);
                }

                if (!_dataSourceBound)
                {
                    // Allow the user to specify arbitrary non-negative values before data binding
                    if (value < 0)
                    {
                        throw new ArgumentOutOfRangeException("value", String.Format(CultureInfo.CurrentCulture,
                                                                                     CommonResources.Argument_Must_Be_GreaterThanOrEqualTo, 0));
                    }
                    else
                    {
                        _pageIndex = value;
                        _pageIndexSet = true;
                    }
                }
                else
                {
                    // Once data bound, perform bounds check on the PageIndex. Also ensure the data source has not been materialized.
                    if ((value < 0) || (value >= PageCount))
                    {
                        throw new ArgumentOutOfRangeException("value", String.Format(CultureInfo.CurrentCulture,
                                                                                     CommonResources.Argument_Must_Be_Between, 0, (PageCount - 1)));
                    }
                    else if (value != _pageIndex)
                    {
                        EnsureDataSourceNotMaterialized();
                        _pageIndex = value;
                        _pageIndexSet = true;
                    }
                }
            }
        }

        public IList<WebGridRow> Rows
        {
            get
            {
                EnsureDataBound();
                if (!_dataSourceMaterialized)
                {
                    _rows = _dataSource.GetRows(SortInfo, PageIndex);
                    _dataSourceMaterialized = true;
                }
                return _rows;
            }
        }

        public int RowsPerPage
        {
            get { return _rowsPerPage; }
        }

        public WebGridRow SelectedRow
        {
            get
            {
                if ((SelectedIndex >= 0) && (SelectedIndex < Rows.Count))
                {
                    return Rows[SelectedIndex];
                }
                return null;
            }
        }

        public int SelectedIndex
        {
            get
            {
                if (!_selectedIndexSet)
                {
                    int row;
                    // Range checking should not use Rows.Count since this will cause paging and sorting.
                    // Review: side effect is that HasSelection will return true if Rows.Count (current page's
                    // row count) is less than both SelectedIndex and RowsPerPage. This scenario should only
                    // happen if someone manually modifies the query string.
                    // If paging isn't enabled, this getter isn't doing a upper bounds check on the value.
                    if ((!Int32.TryParse(QueryString[SelectionFieldName], out row)) || (row < 1) || (_canPage && (row > RowsPerPage)))
                    {
                        row = 0;
                    }
                    _selectedIndex = row - 1;
                    _selectedIndexSet = true;
                }
                return _selectedIndex;
            }
            set
            {
                if (_selectedIndex != value)
                {
                    EnsureDataSourceNotMaterialized();
                    _selectedIndex = value;
                }
                _selectedIndexSet = true;
            }
        }

        public string SelectionFieldName
        {
            get { return FieldNamePrefix + _selectionFieldName; }
        }

        public string SortColumn
        {
            get
            {
                if (!_sortColumnSet)
                {
                    string sortColumn = QueryString[SortFieldName];
                    if (!_dataSourceBound || ValidateSortColumn(sortColumn))
                    {
                        _sortColumn = sortColumn;
                        _sortColumnSet = true;
                    }
                }
                if (String.IsNullOrEmpty(_sortColumn))
                {
                    return _defaultSort ?? String.Empty;
                }
                return _sortColumn;
            }
            set
            {
                EnsureDataBound();
                if (!SortColumn.Equals(value, StringComparison.OrdinalIgnoreCase))
                {
                    EnsureDataSourceNotMaterialized();
                    _sortColumn = value;
                }
                _sortColumnSet = true;
                _sortColumnExplicitlySet = true;
            }
        }

        public SortDirection SortDirection
        {
            get
            {
                if (!_sortDirectionSet)
                {
                    string sortDirection = QueryString[SortDirectionFieldName];
                    if (sortDirection != null)
                    {
                        if (sortDirection.Equals("DESC", StringComparison.OrdinalIgnoreCase) ||
                            sortDirection.Equals("DESCENDING", StringComparison.OrdinalIgnoreCase))
                        {
                            _sortDirection = SortDirection.Descending;
                        }
                    }
                    _sortDirectionSet = true;
                }
                return _sortDirection;
            }
            set
            {
                if (!_dataSourceBound)
                {
                    _sortDirection = value;
                }
                else if (_sortDirection != value)
                {
                    EnsureDataSourceNotMaterialized();
                    _sortDirection = value;
                }
                _sortDirectionSet = true;
            }
        }

        private SortInfo SortInfo
        {
            get { return new SortInfo { SortColumn = SortColumn, SortDirection = SortDirection }; }
        }

        public string SortDirectionFieldName
        {
            get { return FieldNamePrefix + _sortDirectionFieldName; }
        }

        public string SortFieldName
        {
            get { return FieldNamePrefix + _sortFieldName; }
        }

        internal IDictionary<string, Expression> CustomSorters { get; private set; }

        public int TotalRowCount
        {
            get
            {
                EnsureDataBound();
                return _dataSource.TotalRowCount;
            }
        }

        private HttpContextBase HttpContext
        {
            get { return _context; }
        }

        private NameValueCollection QueryString
        {
            get { return HttpContext.Request.QueryString; }
        }

        internal static Type GetElementType(IEnumerable<dynamic> source)
        {
            Debug.Assert(source != null, "source cannot be null");
            Type sourceType = source.GetType();

            if (source.FirstOrDefault() is IDynamicMetaObjectProvider)
            {
                return typeof(IDynamicMetaObjectProvider);
            }
            else if (sourceType.IsArray)
            {
                return sourceType.GetElementType();
            }
            Type elementType = sourceType.GetInterfaces().Select(GetGenericEnumerableType).FirstOrDefault(t => t != null);

            Debug.Assert(elementType != null);
            return elementType;
        }

        private static Type GetGenericEnumerableType(Type type)
        {
            Type enumerableType = typeof(IEnumerable<>);
            if (type.IsGenericType && enumerableType.IsAssignableFrom(type.GetGenericTypeDefinition()))
            {
                return type.GetGenericArguments()[0];
            }
            return null;
        }

        public WebGrid Bind(IEnumerable<dynamic> source, IEnumerable<string> columnNames = null, bool autoSortAndPage = true, int rowCount = -1)
        {
            if (_dataSourceBound)
            {
                throw new InvalidOperationException(HelpersResources.WebGrid_DataSourceBound);
            }
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (!autoSortAndPage && _canPage && rowCount == -1)
            {
                throw new ArgumentException(HelpersResources.WebGrid_RowCountNotSpecified, "rowCount");
            }

            _elementType = GetElementType(source);
            if (_columnNames == null)
            {
                _columnNames = columnNames ?? GetDefaultColumnNames(source, elementType: _elementType);
            }

            if (!autoSortAndPage)
            {
                _dataSource = new PreComputedGridDataSource(grid: this, values: source, totalRows: rowCount);
            }
            else
            {
                WebGridDataSource dataSource = new WebGridDataSource(grid: this, values: source, elementType: _elementType, canPage: _canPage, canSort: _canSort);
                dataSource.DefaultSort = new SortInfo { SortColumn = _defaultSort, SortDirection = SortDirection.Ascending };
                dataSource.RowsPerPage = _rowsPerPage;
                _dataSource = dataSource;
            }
            _dataSourceBound = true;
            ValidatePreDataBoundValues();
            return this;
        }

        // todo: add templating from file support
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Non-static for syntax, and in case we want to check column existence.")]
        public WebGridColumn Column(string columnName = null, string header = null, Func<dynamic, object> format = null, string style = null,
                                    bool canSort = true)
        {
            if (String.IsNullOrEmpty(columnName))
            {
                if (format == null)
                {
                    throw new ArgumentException(HelpersResources.WebGrid_ColumnNameOrFormatRequired, "columnName");
                }
            }

            return new WebGridColumn { ColumnName = columnName, Header = header, Format = format, Style = style, CanSort = canSort };
        }

        // Should we keep this no-op API for improved WebGrid syntax? Alternatives are:
        // 1. columns: grid.Columns(
        //        grid.Column(...), grid.Column(...)
        //    )
        // 2. columns: new[] {
        //        grid.Column(...), grid.Column(...)
        //    }
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Review: No-op API for syntax simplification?")]
        public WebGridColumn[] Columns(params WebGridColumn[] columnSet)
        {
            return columnSet;
        }

        public IHtmlString GetContainerUpdateScript(string path)
        {
            var script = String.Format(CultureInfo.InvariantCulture, AjaxUpdateScript,
                                       HttpUtility.JavaScriptStringEncode(path, addDoubleQuotes: true),
                                       HttpUtility.JavaScriptStringEncode('#' + AjaxUpdateContainerId, addDoubleQuotes: true),
                                       !String.IsNullOrEmpty(AjaxUpdateCallback) ? ',' + HttpUtility.JavaScriptStringEncode(AjaxUpdateCallback) : String.Empty);

            return new HtmlString(HttpUtility.HtmlAttributeEncode(script));
        }

        /// <summary>
        /// Gets the HTML for a table with a pager.
        /// </summary>
        /// <param name="tableStyle">Table class for styling.</param>
        /// <param name="headerStyle">Header row class for styling.</param>
        /// <param name="footerStyle">Footer row class for styling.</param>
        /// <param name="rowStyle">Row class for styling (odd rows only).</param>
        /// <param name="alternatingRowStyle">Row class for styling (even rows only).</param>
        /// <param name="selectedRowStyle">Selected row class for styling.</param>
        /// <param name="displayHeader">Whether the header row should be displayed.</param>
        /// <param name="caption">The string displayed as the table caption</param>
        /// <param name="fillEmptyRows">Whether the table can add empty rows to ensure the rowsPerPage row count.</param>
        /// <param name="emptyRowCellValue">Value used to populate empty rows. This property is only used when <paramref name="fillEmptyRows"/> is set</param>
        /// <param name="columns">Column model for customizing column rendering.</param>
        /// <param name="exclusions">Columns to exclude when auto-populating columns.</param>
        /// <param name="mode">Modes for pager rendering.</param>
        /// <param name="firstText">Text for link to first page.</param>
        /// <param name="previousText">Text for link to previous page.</param>
        /// <param name="nextText">Text for link to next page.</param>
        /// <param name="lastText">Text for link to last page.</param>
        /// <param name="numericLinksCount">Number of numeric links that should display.</param>
        /// <param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param>
        public IHtmlString GetHtml(
            string tableStyle = null,
            string headerStyle = null,
            string footerStyle = null,
            string rowStyle = null,
            string alternatingRowStyle = null,
            string selectedRowStyle = null,
            string caption = null,
            bool displayHeader = true,
            bool fillEmptyRows = false,
            string emptyRowCellValue = null,
            IEnumerable<WebGridColumn> columns = null,
            IEnumerable<string> exclusions = null,
            WebGridPagerModes mode = WebGridPagerModes.NextPrevious | WebGridPagerModes.Numeric,
            string firstText = null,
            string previousText = null,
            string nextText = null,
            string lastText = null,
            int numericLinksCount = 5,
            object htmlAttributes = null)
        {
            Func<dynamic, object> footer = null;
            if (_canPage && (PageCount > 1))
            {
                footer = item => Pager(mode, firstText, previousText, nextText, lastText, numericLinksCount, explicitlyCalled: false);
            }

            return Table(tableStyle, headerStyle, footerStyle, rowStyle, alternatingRowStyle, selectedRowStyle, caption, displayHeader,
                         fillEmptyRows, emptyRowCellValue, columns, exclusions, footer: footer,
                         htmlAttributes: htmlAttributes);
        }

        [SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings", Justification = "Strings are easier for Plan9 developer to work with")]
        public string GetPageUrl(int pageIndex)
        {
            if (!_canPage)
            {
                throw new NotSupportedException(HelpersResources.WebGrid_NotSupportedIfPagingIsDisabled);
            }
            if ((pageIndex < 0) || (pageIndex >= PageCount))
            {
                throw new ArgumentOutOfRangeException("pageIndex", String.Format(CultureInfo.CurrentCulture,
                                                                                 CommonResources.Argument_Must_Be_Between, 0, (PageCount - 1)));
            }

            NameValueCollection queryString = new NameValueCollection(1);
            queryString[PageFieldName] = (pageIndex + 1L).ToString(CultureInfo.CurrentCulture);
            return GetPath(queryString, SelectionFieldName);
        }

        [SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings", Justification = "Strings are easier for Plan9 developer to work with")]
        public string GetSortUrl(string column)
        {
            if (!_canSort)
            {
                throw new NotSupportedException(HelpersResources.WebGrid_NotSupportedIfSortingIsDisabled);
            }
            if (String.IsNullOrEmpty(column))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "column");
            }

            var sort = SortColumn;
            var sortDir = SortDirection.Ascending;
            if (column.Equals(sort, StringComparison.OrdinalIgnoreCase))
            {
                if (SortDirection == SortDirection.Ascending)
                {
                    sortDir = SortDirection.Descending;
                }
            }

            NameValueCollection queryString = new NameValueCollection(2);
            queryString[SortFieldName] = column;
            queryString[SortDirectionFieldName] = GetSortDirectionString(sortDir);
            return GetPath(queryString, PageFieldName, SelectionFieldName);
        }

        /// <summary>
        /// Gets the HTML for a pager.
        /// </summary>
        /// <param name="mode">Modes for pager rendering.</param>
        /// <param name="firstText">Text for link to first page.</param>
        /// <param name="previousText">Text for link to previous page.</param>
        /// <param name="nextText">Text for link to next page.</param>
        /// <param name="lastText">Text for link to last page.</param>
        /// <param name="numericLinksCount">Number of numeric links that should display.</param>
        public HelperResult Pager(
            WebGridPagerModes mode = WebGridPagerModes.NextPrevious | WebGridPagerModes.Numeric,
            string firstText = null,
            string previousText = null,
            string nextText = null,
            string lastText = null,
            int numericLinksCount = 5)
        {
            return Pager(mode, firstText, previousText, nextText, lastText, numericLinksCount, explicitlyCalled: true);
        }

        /// <param name="mode">Modes for pager rendering.</param>
        /// <param name="firstText">Text for link to first page.</param>
        /// <param name="previousText">Text for link to previous page.</param>
        /// <param name="nextText">Text for link to next page.</param>
        /// <param name="lastText">Text for link to last page.</param>
        /// <param name="numericLinksCount">Number of numeric links that should display.</param>
        /// <param name="explicitlyCalled">The Pager can be explicitly called by the public API or is called by the WebGrid when no footer is provided.
        /// In the explicit scenario, we would need to render a container for the pager to allow identifying the pager links.
        /// In the implicit scenario, the grid table would be the container.
        /// </param>
        private HelperResult Pager(
            WebGridPagerModes mode,
            string firstText,
            string previousText,
            string nextText,
            string lastText,
            int numericLinksCount,
            bool explicitlyCalled)
        {
            if (!_canPage)
            {
                throw new NotSupportedException(HelpersResources.WebGrid_NotSupportedIfPagingIsDisabled);
            }
            if (!ModeEnabled(mode, WebGridPagerModes.FirstLast) && (firstText != null))
            {
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture,
                                                          HelpersResources.WebGrid_PagerModeMustBeEnabled, "FirstLast"), "firstText");
            }
            if (!ModeEnabled(mode, WebGridPagerModes.NextPrevious) && (previousText != null))
            {
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture,
                                                          HelpersResources.WebGrid_PagerModeMustBeEnabled, "NextPrevious"), "previousText");
            }
            if (!ModeEnabled(mode, WebGridPagerModes.NextPrevious) && (nextText != null))
            {
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture,
                                                          HelpersResources.WebGrid_PagerModeMustBeEnabled, "NextPrevious"), "nextText");
            }
            if (!ModeEnabled(mode, WebGridPagerModes.FirstLast) && (lastText != null))
            {
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture,
                                                          HelpersResources.WebGrid_PagerModeMustBeEnabled, "FirstLast"), "lastText");
            }
            if (numericLinksCount < 0)
            {
                throw new ArgumentOutOfRangeException("numericLinksCount",
                                                      String.Format(CultureInfo.CurrentCulture, CommonResources.Argument_Must_Be_GreaterThanOrEqualTo, 0));
            }

            return WebGridRenderer.Pager(this, HttpContext, mode: mode, firstText: firstText, previousText: previousText, nextText: nextText, lastText: lastText,
                                         numericLinksCount: numericLinksCount, renderAjaxContainer: explicitlyCalled);
        }

        /// <summary>
        /// Gets the HTML for a table with a pager.
        /// </summary>
        /// <param name="tableStyle">Table class for styling.</param>
        /// <param name="headerStyle">Header row class for styling.</param>
        /// <param name="footerStyle">Footer row class for styling.</param>
        /// <param name="rowStyle">Row class for styling (odd rows only).</param>
        /// <param name="alternatingRowStyle">Row class for styling (even rows only).</param>
        /// <param name="selectedRowStyle">Selected row class for styling.</param>
        /// <param name="caption">The table caption</param>
        /// <param name="displayHeader">Whether the header row should be displayed.</param>
        /// <param name="fillEmptyRows">Whether the table can add empty rows to ensure the rowsPerPage row count.</param>
        /// <param name="emptyRowCellValue">Value used to populate empty rows. This property is only used when <paramref name="fillEmptyRows"/> is set</param>
        /// <param name="columns">Column model for customizing column rendering.</param>
        /// <param name="exclusions">Columns to exclude when auto-populating columns.</param>
        /// <param name="footer">Table footer template.</param>
        /// <param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param>
        public IHtmlString Table(
            string tableStyle = null,
            string headerStyle = null,
            string footerStyle = null,
            string rowStyle = null,
            string alternatingRowStyle = null,
            string selectedRowStyle = null,
            string caption = null,
            bool displayHeader = true,
            bool fillEmptyRows = false,
            string emptyRowCellValue = null,
            IEnumerable<WebGridColumn> columns = null,
            IEnumerable<string> exclusions = null,
            Func<dynamic, object> footer = null,
            object htmlAttributes = null)
        {
            if (columns == null)
            {
                columns = GetDefaultColumns(exclusions);
            }
            // In order of precedence, the parameters that affect the visibility of columns in WebGrid - 
            // (1) "columns" argument of this method 
            // (2) "exclusion" argument of this method 
            // (3) "columnNames" argument of the constructor. 
            // At the time of binding we can verify if a simple property specified in the query string is a column that would be visible to the user. 
            // However, for complex properties or if either of (1) or (2) arguments are specified, we can only verify at this point. 
            EnsureColumnIsSortable(columns);

            if (emptyRowCellValue == null)
            {
                emptyRowCellValue = "&nbsp;";
            }

            return WebGridRenderer.Table(this, HttpContext, tableStyle: tableStyle, headerStyle: headerStyle, footerStyle: footerStyle, rowStyle: rowStyle,
                                         alternatingRowStyle: alternatingRowStyle, selectedRowStyle: selectedRowStyle, caption: caption, displayHeader: displayHeader, fillEmptyRows: fillEmptyRows,
                                         emptyRowCellValue: emptyRowCellValue, columns: columns, exclusions: exclusions, footer: footer, htmlAttributes: htmlAttributes);
        }

        /// <summary>
        /// Adds a specific sort function for a given column.
        /// </summary>
        /// <typeparam name="TElement">The type of elements in the grid's source.</typeparam>
        /// <typeparam name="TProperty">The column type, usually inferred from the keySelector function's return type.</typeparam>
        /// <param name="columnName">The column name (as used for sorting)</param>
        /// <param name="keySelector">The function used to select a key to sort by, for each element in the grid's source.</param>
        /// <returns>The current grid, with the new custom sorter applied.</returns>
        /// <example>
        /// <code>
        /// var grid = new WebGrid(items)
        ///     .AddSorter("Manager.Name", (Employee x) => (x == null || x.Manager == null) ? null : x.Manager.Name);
        /// </code>
        /// </example>
        [SuppressMessage("Microsoft.Design", "CA1006", Justification = "This design make sense, and is a reasonable user experience for users of the helpers")]
        public WebGrid AddSorter<TElement, TProperty>(string columnName, Expression<Func<TElement, TProperty>> keySelector)
        {
            CustomSorters[columnName] = keySelector;
            return this;
        }

        /// <param name="columns">The set of columns that are rendered to the client.</param>
        private void EnsureColumnIsSortable(IEnumerable<WebGridColumn> columns)
        {
            // Fix for bug 941102
            // The ValidateSortColumn can validate a few regular cases for sorting and reset those values to default. However, for sort columns that are complex expressions,
            // or if the user specifies a subset of columns in the GetHtml method (via columns / exclusions), the method is ineffective. 
            // Review: Should this method not throw if the data was not explicitly sorted and paged by the user
            if (_canSort && !_sortColumnExplicitlySet && !String.IsNullOrEmpty(SortColumn) && !StringComparer.OrdinalIgnoreCase.Equals(_defaultSort, SortColumn)
                && !columns.Select(c => c.ColumnName).Contains(SortColumn, StringComparer.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, HelpersResources.WebGrid_ColumnNotFound, SortColumn));
            }
        }

        internal static dynamic GetMember(WebGridRow row, string name)
        {
            object result;
            if (row.TryGetMember(name, out result))
            {
                return result;
            }
            throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,
                                                              HelpersResources.WebGrid_ColumnNotFound, name));
        }

        // review: make sure this is ordered
        internal string GetPath(NameValueCollection queryString, params string[] exclusions)
        {
            NameValueCollection temp = new NameValueCollection(QueryString);
            // update current query string in case values were set programmatically
            if (temp.AllKeys.Contains(PageFieldName))
            {
                temp.Set(PageFieldName, (PageIndex + 1L).ToString(CultureInfo.CurrentCulture));
            }
            if (temp.AllKeys.Contains(SelectionFieldName))
            {
                if (SelectedIndex < 0)
                {
                    temp.Remove(SelectionFieldName);
                }
                else
                {
                    temp.Set(SelectionFieldName, (SelectedIndex + 1L).ToString(CultureInfo.CurrentCulture));
                }
            }
            if (temp.AllKeys.Contains(SortFieldName))
            {
                if (String.IsNullOrEmpty(SortColumn))
                {
                    temp.Remove(SortFieldName);
                }
                else
                {
                    temp.Set(SortFieldName, SortColumn);
                }
            }
            if (temp.AllKeys.Contains(SortDirectionFieldName))
            {
                temp.Set(SortDirectionFieldName, GetSortDirectionString(SortDirection));
            }

            // remove fields from exclusions list
            foreach (var key in exclusions)
            {
                temp.Remove(key);
            }
            // replace with new field values
            foreach (string key in queryString.Keys)
            {
                temp.Set(key, queryString[key]);
            }
            queryString = temp;

            StringBuilder sb = new StringBuilder(HttpContext.Request.Path);

            sb.Append("?");
            for (int i = 0; i < queryString.Count; i++)
            {
                if (i > 0)
                {
                    sb.Append("&");
                }
                sb.Append(HttpUtility.UrlEncode(queryString.Keys[i]));
                sb.Append("=");
                sb.Append(HttpUtility.UrlEncode(queryString[i]));
            }
            return sb.ToString();
        }

        internal static string GetSortDirectionString(SortDirection sortDir)
        {
            return (sortDir == SortDirection.Ascending) ? "ASC" : "DESC";
        }

        private void EnsureDataBound()
        {
            if (!_dataSourceBound)
            {
                throw new InvalidOperationException(HelpersResources.WebGrid_NoDataSourceBound);
            }
        }

        private void EnsureDataSourceNotMaterialized()
        {
            if (_dataSourceMaterialized)
            {
                throw new InvalidOperationException(HelpersResources.WebGrid_PropertySetterNotSupportedAfterDataBound);
            }
        }

        private void ValidatePreDataBoundValues()
        {
            if (_canPage && _pageIndexSet && PageIndex > PageCount)
            {
                PageIndex = PageCount;
            }
            else if (_canSort && _sortColumnSet && !ValidateSortColumn(SortColumn))
            {
                SortColumn = _defaultSort;
            }
        }

        private bool ValidateSortColumn(string value)
        {
            Debug.Assert(ColumnNames != null);

            // Navigation columns that contain '.' will be validated during the Sort operation
            // Validate other properties up-front and ignore any bad columns passed via the query string
            return _sortColumnExplicitlySet
                   || String.IsNullOrEmpty(value)
                   || StringComparer.OrdinalIgnoreCase.Equals(_defaultSort, value)
                   || ColumnNames.Contains(value, StringComparer.OrdinalIgnoreCase)
                   || value.Contains('.');
        }

        private static IEnumerable<string> GetDefaultColumnNames(IEnumerable<dynamic> source, Type elementType)
        {
            var dynObj = source.FirstOrDefault() as IDynamicMetaObjectProvider;
            if (dynObj != null)
            {
                return DynamicHelper.GetMemberNames(dynObj);
            }
            else
            {
                return (from p in elementType.GetProperties()
                        where IsBindableType(p.PropertyType) && (p.GetIndexParameters().Length == 0)
                        select p.Name).OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToArray();
            }
        }

        private IEnumerable<WebGridColumn> GetDefaultColumns(IEnumerable<string> exclusions)
        {
            IEnumerable<string> names = ColumnNames;
            if (exclusions != null)
            {
                names = names.Except(exclusions);
            }
            return (from n in names
                    select new WebGridColumn { ColumnName = n, CanSort = true }).ToArray();
        }

        // see: DataBoundControlHelper.IsBindableType
        private static bool IsBindableType(Type type)
        {
            Debug.Assert(type != null);

            Type underlyingType = Nullable.GetUnderlyingType(type);
            if (underlyingType != null)
            {
                type = underlyingType;
            }
            return (type.IsPrimitive ||
                    type.Equals(typeof(string)) ||
                    type.Equals(typeof(DateTime)) ||
                    type.Equals(typeof(Decimal)) ||
                    type.Equals(typeof(Guid)) ||
                    type.Equals(typeof(DateTimeOffset)) ||
                    type.Equals(typeof(TimeSpan)));
        }

        private static bool ModeEnabled(WebGridPagerModes mode, WebGridPagerModes modeCheck)
        {
            return (mode & modeCheck) == modeCheck;
        }
    }
}
