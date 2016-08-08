// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;
using System.Web.SessionState;
using Microsoft.Web.Infrastructure.DynamicValidationHelper;

namespace System.Web.WebPages
{
    public class WebPageHttpHandler : IHttpHandler, IRequiresSessionState
    {
        internal const string StartPageFileName = "_PageStart";
        public static readonly string WebPagesVersionHeaderName = "X-AspNetWebPages-Version";
        private static string[] _supportedExtensions = Empty<string>.Array;
        internal static readonly string WebPagesVersion = GetVersionString();
        private readonly WebPage _webPage;
        private readonly Lazy<WebPageRenderingBase> _startPage;

        public WebPageHttpHandler(WebPage webPage)
            : this(webPage, new Lazy<WebPageRenderingBase>(() => System.Web.WebPages.StartPage.GetStartPage(webPage, StartPageFileName, GetRegisteredExtensions())))
        {
        }

        internal WebPageHttpHandler(WebPage webPage, Lazy<WebPageRenderingBase> startPage)
        {
            if (webPage == null)
            {
                throw new ArgumentNullException("webPage");
            }
            _webPage = webPage;
            _startPage = startPage;
        }

        public static bool DisableWebPagesResponseHeader { get; set; }

        public virtual bool IsReusable
        {
            get { return false; }
        }

        internal WebPage RequestedPage
        {
            get { return _webPage; }
        }

        internal WebPageRenderingBase StartPage
        {
            get { return _startPage.Value; }
        }

        internal static string[] SupportedExtensions
        {
            get { return _supportedExtensions; }
        }

        internal static void AddVersionHeader(HttpContextBase httpContext)
        {
            if (!DisableWebPagesResponseHeader)
            {
                httpContext.Response.AppendHeader(WebPagesVersionHeaderName, WebPagesVersion);
            }
        }

        public static IHttpHandler CreateFromVirtualPath(string virtualPath)
        {
            return CreateFromVirtualPath(virtualPath, VirtualPathFactoryManager.Instance);
        }

        internal static IHttpHandler CreateFromVirtualPath(string virtualPath, IVirtualPathFactory virtualPathFactory)
        {
            // We will try to create a WebPage from our factory. If this fails, we assume that the virtual path maps to an IHttpHandler.
            // Instantiate the page from the virtual path
            WebPage page = virtualPathFactory.CreateInstance<WebPage>(virtualPath);

            // If it's not a page, assume it's a regular handler
            if (page == null)
            {
                return virtualPathFactory.CreateInstance<IHttpHandler>(virtualPath);
            }

            // Mark it as a 'top level' page (as opposed to a user control or master)
            page.TopLevelPage = true;

            // Give it its virtual path
            page.VirtualPath = virtualPath;

            // Assign it the object factory
            page.VirtualPathFactory = virtualPathFactory;

            // Return a handler over it
            return new WebPageHttpHandler(page);
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "We don't want a property")]
        public static ReadOnlyCollection<string> GetRegisteredExtensions()
        {
            return new ReadOnlyCollection<string>(_supportedExtensions);
        }

        private static string GetVersionString()
        {
            // DevDiv 216459:
            // This code originally used Assembly.GetName(), but that requires FileIOPermission, which isn't granted in
            // medium trust. However, Assembly.FullName *is* accessible in medium trust.
            return new AssemblyName(typeof(WebPageHttpHandler).Assembly.FullName).Version.ToString(2);
        }

        private static bool HandleError(Exception e)
        {
            // This method is similar to System.Web.UI.Page.HandleError

            // Don't touch security exception
            if (e is SecurityException)
            {
                return false;
            }

            throw new HttpUnhandledException(null, e);
        }

        internal static void GenerateSourceFilesHeader(WebPageContext context)
        {
            if (context.SourceFiles.Any())
            {
                var files = String.Join("|", context.SourceFiles);
                // Since the characters in the value are files that may include characters outside of the ASCII set, header encoding as specified in RFC2047. 
                // =?<charset>?<encoding>?...?= 
                // In the following case, UTF8 is used with base64 encoding 
                var encodedText = "=?UTF-8?B?" + Convert.ToBase64String(Encoding.UTF8.GetBytes(files)) + "?=";
                context.HttpContext.Response.AddHeader("X-SourceFiles", encodedText);
            }
        }

        public virtual void ProcessRequest(HttpContext context)
        {
            ProcessRequestInternal(context);
        }

        internal void ProcessRequestInternal(HttpContext context)
        {
            // enable dynamic validation for this request
            ValidationUtility.EnableDynamicValidation(context);
            context.Request.ValidateInput();

            HttpContextBase contextBase = new HttpContextWrapper(context);
            ProcessRequestInternal(contextBase);
        }

        internal void ProcessRequestInternal(HttpContextBase httpContext)
        {
            try
            {
                //WebSecurity.Context = contextBase;
                AddVersionHeader(httpContext);

                // This is also the point where a Plan9 request truly begins execution

                // We call ExecutePageHierarchy on the requested page, passing in the possible initPage, so that
                // the requested page can take care of the necessary push/pop context and trigger the call to
                // the initPage.
                _webPage.ExecutePageHierarchy(new WebPageContext { HttpContext = httpContext }, httpContext.Response.Output, StartPage);

                if (ShouldGenerateSourceHeader(httpContext))
                {
                    GenerateSourceFilesHeader(_webPage.PageContext);
                }
            }
            catch (Exception e)
            {
                if (!HandleError(e))
                {
                    throw;
                }
            }
        }

        public static void RegisterExtension(string extension)
        {
            // Note: we don't lock or check for duplicates because we only expect this method to be called during PreAppStart
            // Long lived data with few writes and many reads, so reallocate the array each time.
            _supportedExtensions = _supportedExtensions.AppendAndReallocate(extension);
        }

        internal static bool ShouldGenerateSourceHeader(HttpContextBase context)
        {
            return context.Request.IsLocal;
        }
    }
}
