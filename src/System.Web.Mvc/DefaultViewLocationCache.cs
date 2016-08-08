// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Web.Caching;
using System.Web.Mvc.Properties;

namespace System.Web.Mvc
{
    public class DefaultViewLocationCache : IViewLocationCache
    {
        private static readonly TimeSpan _defaultTimeSpan = new TimeSpan(0, 15, 0);

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "The reference type is immutable. ")]
        public static readonly IViewLocationCache Null = new NullViewLocationCache();

        public DefaultViewLocationCache()
            : this(_defaultTimeSpan)
        {
        }

        public DefaultViewLocationCache(TimeSpan timeSpan)
        {
            if (timeSpan.Ticks < 0)
            {
                throw new InvalidOperationException(MvcResources.DefaultViewLocationCache_NegativeTimeSpan);
            }
            TimeSpan = timeSpan;
        }

        public TimeSpan TimeSpan { get; private set; }

        #region IViewLocationCache Members

        public string GetViewLocation(HttpContextBase httpContext, string key)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException("httpContext");
            }
            return (string)httpContext.Cache[key];
        }

        public void InsertViewLocation(HttpContextBase httpContext, string key, string virtualPath)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException("httpContext");
            }
            httpContext.Cache.Insert(key, virtualPath, null /* dependencies */, Cache.NoAbsoluteExpiration, TimeSpan);
        }

        #endregion
    }
}
