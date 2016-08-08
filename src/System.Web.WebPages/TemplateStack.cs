// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace System.Web.WebPages
{
    /// <summary>
    /// Template stacks store a stack of template files. WebPageExecutingBase implements this type, so when executing Plan9 or Mvc WebViewPage,
    /// the stack would contain instances of the page. 
    /// The stack can be queried to identify properties of the current executing file such as the virtual path of the file.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "TemplateStack is a stack")]
    public static class TemplateStack
    {
        private static readonly object _contextKey = new object();

        public static ITemplateFile GetCurrentTemplate(HttpContextBase httpContext)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException("httpContext");
            }
            return GetStack(httpContext).FirstOrDefault();
        }

        public static ITemplateFile Pop(HttpContextBase httpContext)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException("httpContext");
            }
            return GetStack(httpContext).Pop();
        }

        public static void Push(HttpContextBase httpContext, ITemplateFile templateFile)
        {
            if (templateFile == null)
            {
                throw new ArgumentNullException("templateFile");
            }
            if (httpContext == null)
            {
                throw new ArgumentNullException("httpContext");
            }
            GetStack(httpContext).Push(templateFile);
        }

        private static Stack<ITemplateFile> GetStack(HttpContextBase httpContext)
        {
            var stack = httpContext.Items[_contextKey] as Stack<ITemplateFile>;
            if (stack == null)
            {
                stack = new Stack<ITemplateFile>();
                httpContext.Items[_contextKey] = stack;
            }
            return stack;
        }
    }
}
