// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Runtime.CompilerServices;

namespace System.Web.Http
{
    /// <summary>
    /// Represents an <see cref="IQueryable{T}"/> containing zero or one entities. Use together with an
    /// <c>[EnableQuery]</c> from the System.Web.Http.OData or System.Web.OData namespace.
    /// </summary>
    /// <typeparam name="T">The type of the data in the data source.</typeparam>
    [TypeForwardedFrom("System.Web.Http.OData, Version=5.1.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public sealed class SingleResult<T> : SingleResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SingleResult{T}"/> class.
        /// </summary>
        /// <param name="queryable">The <see cref="IQueryable{T}"/> containing zero or one entities.</param>
        public SingleResult(IQueryable<T> queryable)
            : base(queryable)
        {
        }

        /// <summary>
        /// The <see cref="IQueryable{T}"/> containing zero or one entities.
        /// </summary>
        public new IQueryable<T> Queryable
        {
            get
            {
                return base.Queryable as IQueryable<T>;
            }
        }
    }
}
