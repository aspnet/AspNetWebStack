// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace System.Web.WebPages
{
    public static class RequestExtensions
    {
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "request", Justification = "The request parameter is no longer being used but we do not want to break legacy callers.")]
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "1#", Justification = "Response.Redirect() takes its URI as a string parameter.")]
        public static bool IsUrlLocalToHost(this HttpRequestBase request, string url)
        {
            return !url.IsEmpty() &&
                   ((url[0] == '/' && (url.Length == 1 || (url[1] != '/' && url[1] != '\\'))) || // "/" or "/foo" but not "//" or "/\"
                    (url.Length > 1 && url[0] == '~' && url[1] == '/')); // "~/" or "~/foo"
        }
    }
}
