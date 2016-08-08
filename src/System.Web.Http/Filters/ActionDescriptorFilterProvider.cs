// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Controllers;

namespace System.Web.Http.Filters
{
    /// <summary>
    /// This <see cref="IFilterProvider"/> implementation retrieves <see cref="FilterInfo">filters</see> associated with an <see cref="HttpActionDescriptor"/>
    /// instance.
    /// </summary>
    public class ActionDescriptorFilterProvider : IFilterProvider
    {
        /// <summary>
        /// Returns the collection of filters associated with <paramref name="actionDescriptor"/>.
        /// </summary>
        /// <remarks>
        /// The implementation invokes <see cref="HttpActionDescriptor.GetFilters()"/> and <see cref="HttpControllerDescriptor.GetFilters()"/>.
        /// </remarks>
        /// <param name="configuration">The configuration. This value is not used.</param>
        /// <param name="actionDescriptor">The action descriptor.</param>
        /// <returns>A collection of filters.</returns>
        public IEnumerable<FilterInfo> GetFilters(HttpConfiguration configuration, HttpActionDescriptor actionDescriptor)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            if (actionDescriptor == null)
            {
                throw Error.ArgumentNull("actionDescriptor");
            }

            IEnumerable<FilterInfo> controllerFilters = actionDescriptor.ControllerDescriptor.GetFilters().Select(instance => new FilterInfo(instance, FilterScope.Controller));
            IEnumerable<FilterInfo> actionFilters = actionDescriptor.GetFilters().Select(instance => new FilterInfo(instance, FilterScope.Action));

            return controllerFilters.Concat(actionFilters);
        }
    }
}
