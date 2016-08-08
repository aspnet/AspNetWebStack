// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Globalization;
using System.Runtime.Caching;
using Microsoft.Internal.Web.Utils;

namespace System.Web.Helpers
{
    public static class WebCache
    {
        public static void Set(string key, object value, int minutesToCache = 20, bool slidingExpiration = true)
        {
            if (minutesToCache <= 0)
            {
                throw new ArgumentOutOfRangeException("minutesToCache",
                                                      String.Format(CultureInfo.CurrentCulture, CommonResources.Argument_Must_Be_GreaterThan, 0));
            }
            else if (slidingExpiration && (minutesToCache > 365 * 24 * 60))
            {
                // For sliding expiration policies, MemoryCache has a time limit of 365 days. 
                throw new ArgumentOutOfRangeException("minutesToCache",
                                                      String.Format(CultureInfo.CurrentCulture, CommonResources.Argument_Must_Be_LessThanOrEqualTo, 365 * 24 * 60));
            }

            CacheItemPolicy policy = new CacheItemPolicy();
            TimeSpan expireTime = new TimeSpan(0, minutesToCache, 0);

            if (slidingExpiration)
            {
                policy.SlidingExpiration = expireTime;
            }
            else
            {
                policy.AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(minutesToCache);
            }

            MemoryCache.Default.Set(key, value, policy);
        }

        public static dynamic Get(string key)
        {
            return MemoryCache.Default.Get(key);
        }

        public static dynamic Remove(string key)
        {
            return MemoryCache.Default.Remove(key);
        }
    }
}
