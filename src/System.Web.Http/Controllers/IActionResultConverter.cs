// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Threading;

namespace System.Web.Http.Controllers
{
    /// <summary>
    /// A contract for a conversion routine that can take the result of an action returned from
    /// <see cref="HttpActionDescriptor.ExecuteAsync(HttpControllerContext, IDictionary{string, object}, CancellationToken)"/>
    /// and synchronously convert it to an instance of <see cref="HttpResponseMessage"/>.
    /// </summary>
    /// <remarks>
    /// This interface is not used when the runtime return value of an action is an <see cref="IHttpActionResult"/>.
    /// </remarks>
    public interface IActionResultConverter
    {
        HttpResponseMessage Convert(HttpControllerContext controllerContext, object actionResult);
    }
}