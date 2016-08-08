// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Web.Caching;
using System.Web.Hosting;
using Microsoft.Web.Infrastructure;

namespace System.Web.WebPages
{
    public abstract class ApplicationStartPage : WebPageExecutingBase
    {
        private static readonly Action<Action> _safeExecuteStartPageThunk = GetSafeExecuteStartPageThunk();
        public static readonly string StartPageVirtualPath = "~/_appstart.";
        public static readonly string CacheKeyPrefix = "__AppStartPage__";

        public HttpApplication Application { get; internal set; }

        public override HttpContextBase Context
        {
            get { return new HttpContextWrapper(Application.Context); }
        }

        public static HtmlString Markup { get; private set; }

        internal static Exception Exception { get; private set; }

        public TextWriter Output { get; internal set; }

        public override string VirtualPath
        {
            get { return StartPageVirtualPath; }
            set
            {
                // The virtual path for the start page is fixed for now.
                throw new NotSupportedException();
            }
        }

        internal void ExecuteInternal()
        {
            // See comments in GetSafeExecuteStartPageThunk().
            _safeExecuteStartPageThunk(() =>
            {
                Output = new StringWriter(CultureInfo.InvariantCulture);
                Execute();
                Markup = new HtmlString(Output.ToString());
            });
        }

        internal static void ExecuteStartPage(HttpApplication application)
        {
            ExecuteStartPage(application,
                             vpath => MonitorFile(vpath),
                             VirtualPathFactoryManager.Instance,
                             WebPageHttpHandler.GetRegisteredExtensions());
        }

        internal static void ExecuteStartPage(HttpApplication application, Action<string> monitorFile, IVirtualPathFactory virtualPathFactory, IEnumerable<string> supportedExtensions)
        {
            try
            {
                ExecuteStartPageInternal(application, monitorFile, virtualPathFactory, supportedExtensions);
            }
            catch (Exception e)
            {
                // Throw it as a HttpException so as to
                // display the original stack trace information.
                Exception = e;
                throw new HttpException(null, e);
            }
        }

        internal static void ExecuteStartPageInternal(HttpApplication application, Action<string> monitorFile, IVirtualPathFactory virtualPathFactory, IEnumerable<string> supportedExtensions)
        {
            ApplicationStartPage startPage = null;

            foreach (var extension in supportedExtensions)
            {
                var vpath = StartPageVirtualPath + extension;

                // We need to monitor regardless of existence because the user could add/remove the
                // file at any time.
                monitorFile(vpath);
                if (!virtualPathFactory.Exists(vpath))
                {
                    continue;
                }

                if (startPage == null)
                {
                    startPage = virtualPathFactory.CreateInstance<ApplicationStartPage>(vpath);
                    startPage.Application = application;
                    startPage.VirtualPathFactory = virtualPathFactory;
                    startPage.ExecuteInternal();
                }
            }
        }

        private static Action<Action> GetSafeExecuteStartPageThunk()
        {
            // Programmatically detect if this version of System.Web.dll suffers from a bug in
            // which HttpUtility.HtmlEncode can't be called from Application_Start, and if so
            // set the current HttpContext to null to work around it.
            //
            // See Dev10 #906296 and Dev10 #898600 for more information.

            if (typeof(HttpResponse).GetProperty("DisableCustomHttpEncoder", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly) != null)
            {
                // this version suffers from the bug
                return HttpContextHelper.ExecuteInNullContext;
            }
            else
            {
                // this version does not suffer from the bug
                return action => action();
            }
        }

        private static void InitiateShutdown(string key, object value, CacheItemRemovedReason reason)
        {
            // Only handle case when the dependency has changed.
            if (reason != CacheItemRemovedReason.DependencyChanged)
            {
                return;
            }

            ThreadPool.QueueUserWorkItem(new WaitCallback(ShutdownCallBack));
        }

        private static void MonitorFile(string virtualPath)
        {
            var virtualPathDependencies = new List<string>();
            virtualPathDependencies.Add(virtualPath);
            CacheDependency cacheDependency = HostingEnvironment.VirtualPathProvider.GetCacheDependency(
                virtualPath, virtualPathDependencies, DateTime.UtcNow);
            var key = CacheKeyPrefix + virtualPath;

            HttpRuntime.Cache.Insert(key, virtualPath, cacheDependency,
                                     Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration,
                                     CacheItemPriority.NotRemovable, new CacheItemRemovedCallback(InitiateShutdown));
        }

        private static void ShutdownCallBack(object state)
        {
            InfrastructureHelper.UnloadAppDomain();
        }

        public override void Write(HelperResult result)
        {
            if (result != null)
            {
                result.WriteTo(Output);
            }
        }

        public override void WriteLiteral(object value)
        {
            Output.Write(value);
        }

        public override void Write(object value)
        {
            Output.Write(HttpUtility.HtmlEncode(value));
        }

        protected internal override TextWriter GetOutputWriter()
        {
            return Output;
        }
    }
}
