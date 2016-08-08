// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.CSharp.RuntimeBinder;
using Binder = Microsoft.CSharp.RuntimeBinder.Binder;

namespace System.Web.Helpers
{
    /// <summary>
    /// Default data source that sorts results if a sort column is specified.
    /// </summary>
    internal sealed class WebGridDataSource : IWebGridDataSource
    {
        private static readonly MethodInfo SortGenericExpressionMethod = typeof(WebGridDataSource).GetMethod("SortGenericExpression", BindingFlags.Static | BindingFlags.NonPublic);

        private readonly WebGrid _grid;
        private readonly Type _elementType;
        private readonly IEnumerable<dynamic> _values;
        private readonly bool _canPage;
        private readonly bool _canSort;

        public WebGridDataSource(WebGrid grid, IEnumerable<dynamic> values, Type elementType, bool canPage, bool canSort)
        {
            Debug.Assert(grid != null);
            Debug.Assert(values != null);

            _grid = grid;
            _values = values;
            _elementType = elementType;
            _canPage = canPage;
            _canSort = canSort;
        }

        public SortInfo DefaultSort { get; set; }

        public int RowsPerPage { get; set; }

        public int TotalRowCount
        {
            get { return _values.Count(); }
        }

        public IList<WebGridRow> GetRows(SortInfo sortInfo, int pageIndex)
        {
            IEnumerable<dynamic> rowData = _values;

            if (_canSort)
            {
                rowData = Sort(_values.AsQueryable(), sortInfo);
            }

            rowData = Page(rowData, pageIndex);

            try
            {
                // Force compile the underlying IQueryable
                rowData = rowData.ToList();
            }
            catch (ArgumentException)
            {
                // The OrderBy method uses a generic comparer which fails when the collection contains 2 or more 
                // items that cannot be compared (e.g. DBNulls, mixed types such as strings and ints et al) with the exception
                // System.ArgumentException: At least one object must implement IComparable.
                // Silently fail if this exception occurs and declare that the two items are equivalent
                rowData = Page(_values.AsQueryable(), pageIndex);
            }
            return rowData.Select((value, index) => new WebGridRow(_grid, value: value, rowIndex: index)).ToList();
        }

        private IQueryable<dynamic> Sort(IQueryable<dynamic> data, SortInfo sortInfo)
        {
            if (!String.IsNullOrEmpty(sortInfo.SortColumn) || ((DefaultSort != null) && !String.IsNullOrEmpty(DefaultSort.SortColumn)))
            {
                return Sort(data, _elementType, sortInfo);
            }
            return data;
        }

        private IEnumerable<dynamic> Page(IEnumerable<dynamic> data, int pageIndex)
        {
            if (_canPage)
            {
                Debug.Assert(RowsPerPage > 0);
                return data.Skip(pageIndex * RowsPerPage).Take(RowsPerPage);
            }
            return data;
        }

        private IQueryable<dynamic> Sort(IQueryable<dynamic> data, Type elementType, SortInfo sort)
        {
            Debug.Assert(data != null);

            if (typeof(IDynamicMetaObjectProvider).IsAssignableFrom(elementType))
            {
                // IDynamicMetaObjectProvider properties are only available through a runtime binder, so we
                // must build a custom LINQ expression for getting the dynamic property value.
                // Lambda: o => o.Property (where Property is obtained by runtime binder)
                // NOTE: lambda must not use internals otherwise this will fail in partial trust when Helpers assembly is in GAC
                var binder = Binder.GetMember(CSharpBinderFlags.None, sort.SortColumn, typeof(WebGrid), new[]
                {
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
                });
                var param = Expression.Parameter(typeof(IDynamicMetaObjectProvider), "o");
                var getter = Expression.Dynamic(binder, typeof(object), param);
                return SortGenericExpression<IDynamicMetaObjectProvider, object>(data, getter, param, sort.SortDirection);
            }

            Expression sorterFunctionBody;
            ParameterExpression sorterFunctionParameter;

            Expression sorter;
            if (_grid.CustomSorters.TryGetValue(sort.SortColumn, out sorter))
            {
                var lambda = sorter as LambdaExpression;
                Debug.Assert(lambda != null);

                sorterFunctionBody = lambda.Body;
                sorterFunctionParameter = lambda.Parameters[0];
            }
            else
            {
                // The IQueryable<dynamic> data source is cast as IQueryable<object> at runtime. We must call
                // SortGenericExpression using reflection so that the LINQ expressions use the actual element type.
                // Lambda: o => o.Property[.NavigationProperty,etc]
                sorterFunctionParameter = Expression.Parameter(elementType, "o");
                Expression member = sorterFunctionParameter;
                var type = elementType;
                var sorts = sort.SortColumn.Split('.');
                foreach (var name in sorts)
                {
                    PropertyInfo prop = type.GetProperty(name);
                    if (prop == null)
                    {
                        // no-op in case navigation property came from querystring (falls back to default sort)
                        if ((DefaultSort != null) && !sort.Equals(DefaultSort) && !String.IsNullOrEmpty(DefaultSort.SortColumn))
                        {
                            return Sort(data, elementType, DefaultSort);
                        }
                        return data;
                    }
                    member = Expression.Property(member, prop);
                    type = prop.PropertyType;
                }
                sorterFunctionBody = member;
            }

            var actualSortMethod = SortGenericExpressionMethod.MakeGenericMethod(elementType, sorterFunctionBody.Type);
            return (IQueryable<dynamic>)actualSortMethod.Invoke(null, new object[] { data, sorterFunctionBody, sorterFunctionParameter, sort.SortDirection });
        }

        private static IQueryable<TElement> SortGenericExpression<TElement, TProperty>(IQueryable<dynamic> data, Expression body,
                                                                                       ParameterExpression param, SortDirection sortDirection)
        {
            Debug.Assert(data != null);
            Debug.Assert(body != null);
            Debug.Assert(param != null);

            // The IQueryable<dynamic> data source is cast as an IQueryable<object> at runtime.  We must cast
            // this to an IQueryable<TElement> so that the reflection done by the LINQ expressions will work.
            IQueryable<TElement> data2 = data.Cast<TElement>();
            Expression<Func<TElement, TProperty>> lambda = Expression.Lambda<Func<TElement, TProperty>>(body, param);
            if (sortDirection == SortDirection.Descending)
            {
                return data2.OrderByDescending(lambda);
            }
            else
            {
                return data2.OrderBy(lambda);
            }
        }
    }
}
