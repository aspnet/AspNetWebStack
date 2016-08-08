// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web.Mvc.Properties;
using System.Web.UI;

namespace System.Web.Mvc
{
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "Unsealed so that subclassed types can set properties in the default constructor.")]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class OutputCacheAttribute : ActionFilterAttribute, IExceptionFilter
    {
        private const string CacheKeyPrefix = "_MvcChildActionCache_";
        private static readonly char[] _splitParameter = new[] { ';' };
        private static ObjectCache _childActionCache;
        private static object _childActionFilterFinishCallbackKey = new object();
        private readonly Func<string[]> _splitVaryByParamThunk;
        private OutputCacheParameters _cacheSettings = new OutputCacheParameters { VaryByParam = "*" };
        private Func<ObjectCache> _childActionCacheThunk = () => ChildActionCache;
        private bool _locationWasSet;
        private bool _noStoreWasSet;
        private string[] _tokenizedVaryByParams;

        public OutputCacheAttribute()
        {
            _splitVaryByParamThunk = () => GetTokenizedVaryByParam(VaryByParam);
        }

        internal OutputCacheAttribute(ObjectCache childActionCache)
            : this()
        {
            _childActionCacheThunk = () => childActionCache;
        }

        public string CacheProfile
        {
            get { return _cacheSettings.CacheProfile ?? String.Empty; }
            set { _cacheSettings.CacheProfile = value; }
        }

        internal OutputCacheParameters CacheSettings
        {
            get { return _cacheSettings; }
        }

        public static ObjectCache ChildActionCache
        {
            get { return _childActionCache ?? MemoryCache.Default; }
            set { _childActionCache = value; }
        }

        private ObjectCache ChildActionCacheInternal
        {
            get { return _childActionCacheThunk(); }
        }

        public int Duration
        {
            get { return _cacheSettings.Duration; }
            set { _cacheSettings.Duration = value; }
        }

        public OutputCacheLocation Location
        {
            get { return _cacheSettings.Location; }
            set
            {
                _cacheSettings.Location = value;
                _locationWasSet = true;
            }
        }

        public bool NoStore
        {
            get { return _cacheSettings.NoStore; }
            set
            {
                _cacheSettings.NoStore = value;
                _noStoreWasSet = true;
            }
        }

        public string SqlDependency
        {
            get { return _cacheSettings.SqlDependency ?? String.Empty; }
            set { _cacheSettings.SqlDependency = value; }
        }

        public string VaryByContentEncoding
        {
            get { return _cacheSettings.VaryByContentEncoding ?? String.Empty; }
            set { _cacheSettings.VaryByContentEncoding = value; }
        }

        public string VaryByCustom
        {
            get { return _cacheSettings.VaryByCustom ?? String.Empty; }
            set { _cacheSettings.VaryByCustom = value; }
        }

        public string VaryByHeader
        {
            get { return _cacheSettings.VaryByHeader ?? String.Empty; }
            set { _cacheSettings.VaryByHeader = value; }
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Param", Justification = "Matches the @ OutputCache page directive. Suppressed in source because this is a special case suppression.")]
        public string VaryByParam
        {
            get { return _cacheSettings.VaryByParam ?? String.Empty; }
            set
            {
                // Reset the tokenized values if the property gets modified.
                _tokenizedVaryByParams = null;
                _cacheSettings.VaryByParam = value;
            }
        }

        private static void ClearChildActionFilterFinishCallback(ControllerContext controllerContext)
        {
            controllerContext.HttpContext.Items.Remove(_childActionFilterFinishCallbackKey);
        }

        private static void CompleteChildAction(ControllerContext filterContext, bool wasException)
        {
            Action<bool> callback = GetChildActionFilterFinishCallback(filterContext);

            if (callback != null)
            {
                ClearChildActionFilterFinishCallback(filterContext);
                callback(wasException);
            }
        }

        private static Action<bool> GetChildActionFilterFinishCallback(ControllerContext controllerContext)
        {
            return controllerContext.HttpContext.Items[_childActionFilterFinishCallbackKey] as Action<bool>;
        }

        internal string GetChildActionUniqueId(ActionExecutingContext filterContext)
        {
            StringBuilder uniqueIdBuilder = new StringBuilder();

            // Start with a prefix, presuming that we share the cache with other users
            uniqueIdBuilder.Append(CacheKeyPrefix);

            // Unique ID of the action description
            uniqueIdBuilder.Append(filterContext.ActionDescriptor.UniqueId);

            // Unique ID from the VaryByCustom settings, if any
            DescriptorUtil.AppendUniqueId(uniqueIdBuilder, VaryByCustom);
            if (!String.IsNullOrEmpty(VaryByCustom))
            {
                string varyByCustomResult = filterContext.HttpContext.ApplicationInstance.GetVaryByCustomString(HttpContext.Current, VaryByCustom);
                uniqueIdBuilder.Append(varyByCustomResult);
            }

            // Unique ID from the VaryByParam settings, if any
            BuildUniqueIdFromActionParameters(uniqueIdBuilder, filterContext);

            // The key is typically too long to be useful, so we use a cryptographic hash
            // as the actual key (better randomization and key distribution, so small vary
            // values will generate dramtically different keys).
            using (SHA256Cng sha = new SHA256Cng())
            {
                return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(uniqueIdBuilder.ToString())));
            }
        }

        internal void BuildUniqueIdFromActionParameters(
            StringBuilder builder,
            ActionExecutingContext filterContext)
        {
            if (String.Equals(VaryByParam, "none", StringComparison.OrdinalIgnoreCase))
            {
                // nothing to do
                return;
            }

            if (String.Equals(VaryByParam, "*", StringComparison.Ordinal))
            {
                // use all available key/value pairs. Keys need to be sorted so we end up with a stable identifier.
                IEnumerable<KeyValuePair<string, object>> orderedParameters = filterContext
                                                                .ActionParameters
                                                                .OrderBy(k => k.Key, StringComparer.OrdinalIgnoreCase);

                foreach (KeyValuePair<string, object> item in orderedParameters)
                {
                    DescriptorUtil.AppendUniqueId(builder, item.Key.ToUpperInvariant());
                    DescriptorUtil.AppendUniqueId(builder, item.Value);
                }

                return;
            }

            LazyInitializer.EnsureInitialized(ref _tokenizedVaryByParams,
                                              _splitVaryByParamThunk);

            // By default, filterContext.ActionParameter is a case insensitive (StringComparer.OrdinalIgnoreCase)
            // dictionary. If the user has replaced it with a dictionary that uses a different comparer, we'll
            // wrap it in a Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            // This allows us to lookup values for action parameters when the VaryByParam token key differs in case.
            Dictionary<string, object> keyValues = GetCaseInsensitiveActionParametersDictionary(
                                                        filterContext.ActionParameters);
            for (int i = 0; i < _tokenizedVaryByParams.Length; i++)
            {
                string key = _tokenizedVaryByParams[i];
                DescriptorUtil.AppendUniqueId(builder, key);

                object value;
                keyValues.TryGetValue(key, out value);
                DescriptorUtil.AppendUniqueId(builder, value);
            }
        }

        public static bool IsChildActionCacheActive(ControllerContext controllerContext)
        {
            return GetChildActionFilterFinishCallback(controllerContext) != null;
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }

            // Complete the request if the child action threw an exception
            if (filterContext.IsChildAction && filterContext.Exception != null)
            {
                CompleteChildAction(filterContext, wasException: true);
            }
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }

            if (filterContext.IsChildAction)
            {
                // Skip validation and caching if there's no caching on the server for this action. It's ok to 
                // explicitly disable caching for a child action, but it will be ignored if the parent action
                // is using caching.
                if (IsServerSideCacheDisabled())
                {
                    return;
                }

                ValidateChildActionConfiguration();

                // Already actively being captured? (i.e., cached child action inside of cached child action)
                // Realistically, this needs write substitution to do properly (including things like authentication)
                if (GetChildActionFilterFinishCallback(filterContext) != null)
                {
                    throw new InvalidOperationException(MvcResources.OutputCacheAttribute_CannotNestChildCache);
                }

                // Already cached?
                string uniqueId = GetChildActionUniqueId(filterContext);
                string cachedValue = ChildActionCacheInternal.Get(uniqueId) as string;
                if (cachedValue != null)
                {
                    filterContext.Result = new ContentResult() { Content = cachedValue };
                    return;
                }

                // Swap in a new TextWriter so we can capture the output
                StringWriter cachingWriter = new StringWriter(CultureInfo.InvariantCulture);
                TextWriter originalWriter = filterContext.HttpContext.Response.Output;
                filterContext.HttpContext.Response.Output = cachingWriter;

                // Set a finish callback to clean up
                SetChildActionFilterFinishCallback(filterContext, wasException =>
                {
                    // Restore original writer
                    filterContext.HttpContext.Response.Output = originalWriter;

                    // Grab output and write it
                    string capturedText = cachingWriter.ToString();
                    filterContext.HttpContext.Response.Write(capturedText);

                    // Only cache output if this wasn't an error
                    if (!wasException)
                    {
                        ChildActionCacheInternal.Add(uniqueId, capturedText, DateTimeOffset.UtcNow.AddSeconds(Duration));
                    }
                });
            }
        }

        public void OnException(ExceptionContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }

            if (filterContext.IsChildAction)
            {
                CompleteChildAction(filterContext, wasException: true);
            }
        }

        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }

            if (!filterContext.IsChildAction)
            {
                // we need to call ProcessRequest() since there's no other way to set the Page.Response intrinsic
                using (OutputCachedPage page = new OutputCachedPage(_cacheSettings))
                {
                    page.ProcessRequest(HttpContext.Current);
                }
            }
        }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }

            if (filterContext.IsChildAction)
            {
                CompleteChildAction(filterContext, wasException: filterContext.Exception != null);
            }
        }

        private static void SetChildActionFilterFinishCallback(ControllerContext controllerContext, Action<bool> callback)
        {
            controllerContext.HttpContext.Items[_childActionFilterFinishCallbackKey] = callback;
        }

        private void ValidateChildActionConfiguration()
        {
            if (!String.IsNullOrWhiteSpace(CacheProfile) ||
                !String.IsNullOrWhiteSpace(SqlDependency) ||
                !String.IsNullOrWhiteSpace(VaryByContentEncoding) ||
                !String.IsNullOrWhiteSpace(VaryByHeader) ||
                _locationWasSet || _noStoreWasSet)
            {
                throw new InvalidOperationException(MvcResources.OutputCacheAttribute_ChildAction_UnsupportedSetting);
            }

            if (Duration <= 0)
            {
                throw new InvalidOperationException(MvcResources.OutputCacheAttribute_InvalidDuration);
            }

            if (String.IsNullOrWhiteSpace(VaryByParam))
            {
                throw new InvalidOperationException(MvcResources.OutputCacheAttribute_InvalidVaryByParam);
            }
        }

        private bool IsServerSideCacheDisabled()
        {
            switch (Location)
            {
                case OutputCacheLocation.None:
                case OutputCacheLocation.Client:
                case OutputCacheLocation.Downstream:
                    return true;

                default:
                    return false;
            }
        }

        private static string[] GetTokenizedVaryByParam(string varyByParam)
        {
            // Vary by specific parameters
            IEnumerable<string> splitTokens = from part in varyByParam.Split(_splitParameter)
                                              let trimmed = part.Trim()
                                              where !String.IsNullOrEmpty(trimmed)
                                              select trimmed.ToUpperInvariant();
            return splitTokens.ToArray();
        }

        private static Dictionary<string, object> GetCaseInsensitiveActionParametersDictionary(
            IDictionary<string, object> actionParameters)
        {
            // The ControllerActionInvoker starts off with a Dictionary<string, object> with
            // StringComparer.OrdinalIgnoreCase. Check if we are working with this type to start with.
            // If not produce a new dictionary from the existing one.

            var dictionary = actionParameters as Dictionary<string, object>;
            if (dictionary != null && dictionary.Comparer == StringComparer.OrdinalIgnoreCase)
            {
                return dictionary;
            }

            return new Dictionary<string, object>(actionParameters, StringComparer.OrdinalIgnoreCase);
        }

        [SuppressMessage("ASP.NET.Security", "CA5328:ValidateRequestShouldBeEnabled", Justification = "Instances of this type are not created in response to direct user input.")]
        private sealed class OutputCachedPage : Page
        {
            private OutputCacheParameters _cacheSettings;

            public OutputCachedPage(OutputCacheParameters cacheSettings)
            {
                // Tracing requires Page IDs to be unique.
                ID = Guid.NewGuid().ToString();
                _cacheSettings = cacheSettings;
            }

            protected override void FrameworkInitialize()
            {
                // when you put the <%@ OutputCache %> directive on a page, the generated code calls InitOutputCache() from here
                base.FrameworkInitialize();
                InitOutputCache(_cacheSettings);
            }
        }
    }
}
