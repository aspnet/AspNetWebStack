// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Web.Razor;
using System.Web.SessionState;
using System.Web.WebPages.Resources;

namespace System.Web.WebPages
{
    internal static class SessionStateUtil
    {
        private static readonly ConcurrentDictionary<Type, SessionStateBehavior?> _sessionStateBehaviorCache = new ConcurrentDictionary<Type, SessionStateBehavior?>();

        internal static void SetUpSessionState(HttpContextBase context, IHttpHandler handler)
        {
            SetUpSessionState(context, handler, _sessionStateBehaviorCache);
        }

        internal static void SetUpSessionState(HttpContextBase context, IHttpHandler handler, ConcurrentDictionary<Type, SessionStateBehavior?> cache)
        {
            WebPageHttpHandler webPageHandler = handler as WebPageHttpHandler;
            Debug.Assert(handler != null);
            SessionStateBehavior? sessionState = GetSessionStateBehavior(webPageHandler.RequestedPage, cache);

            if (sessionState != null)
            {
                // If the page explicitly specifies a session state value, return since it has the most priority.
                context.SetSessionStateBehavior(sessionState.Value);
                return;
            }

            WebPageRenderingBase page = webPageHandler.StartPage;
            StartPage startPage = null;
            do
            {
                // Drill down _AppStart and _PageStart.
                startPage = page as StartPage;
                if (startPage != null)
                {
                    sessionState = GetSessionStateBehavior(page, cache);
                    page = startPage.ChildPage;
                }
            }
            while (startPage != null);

            if (sessionState != null)
            {
                context.SetSessionStateBehavior(sessionState.Value);
            }
        }

        private static SessionStateBehavior? GetSessionStateBehavior(WebPageExecutingBase page, ConcurrentDictionary<Type, SessionStateBehavior?> cache)
        {
            return cache.GetOrAdd(page.GetType(), type =>
            {
                SessionStateBehavior sessionStateBehavior = SessionStateBehavior.Default;
                var attributes = (RazorDirectiveAttribute[])type.GetCustomAttributes(typeof(RazorDirectiveAttribute), inherit: false);
                var directiveAttributes = attributes.Where(attr => StringComparer.OrdinalIgnoreCase.Equals("sessionstate", attr.Name))
                    .ToList();

                if (!directiveAttributes.Any())
                {
                    return null;
                }
                if (directiveAttributes.Count > 1)
                {
                    throw new InvalidOperationException(WebPageResources.SessionState_TooManyValues);
                }
                var directiveAttribute = directiveAttributes[0];
                if (!Enum.TryParse<SessionStateBehavior>(directiveAttribute.Value, ignoreCase: true, result: out sessionStateBehavior))
                {
                    var values = Enum.GetValues(typeof(SessionStateBehavior)).Cast<SessionStateBehavior>().Select(s => s.ToString());
                    throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, WebPageResources.SessionState_InvalidValue,
                                                              directiveAttribute.Value, page.VirtualPath, String.Join(", ", values)));
                }
                return sessionStateBehavior;
            });
        }
    }
}
