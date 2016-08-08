// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Http.Tracing
{
    /// <summary>
    /// Category names traced by the default tracing implementation.
    /// </summary>
    /// <remarks>
    /// The list of permitted category names is open-ended, and users may define their own.
    /// It is recommended that category names reflect the namespace of their
    /// respective area.  This prevents name conflicts and allows external
    /// logging tools to enable or disable tracing by namespace.
    /// </remarks>
    public static class TraceCategories
    {
        public static readonly string ActionCategory = "System.Web.Http.Action";
        public static readonly string ControllersCategory = "System.Web.Http.Controllers";
        public static readonly string FiltersCategory = "System.Web.Http.Filters";
        public static readonly string FormattingCategory = "System.Net.Http.Formatting";
        public static readonly string MessageHandlersCategory = "System.Web.Http.MessageHandlers";
        public static readonly string ModelBindingCategory = "System.Web.Http.ModelBinding";
        public static readonly string RequestCategory = "System.Web.Http.Request";
        public static readonly string RoutingCategory = "System.Web.Http.Routing";
    }
}
