// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Mvc
{
    public class RazorViewEngine : BuildManagerViewEngine
    {
        internal static readonly string ViewStartFileName = "_ViewStart";

        public RazorViewEngine()
            : this(null)
        {
        }

        public RazorViewEngine(IViewPageActivator viewPageActivator)
            : base(viewPageActivator)
        {
            AreaViewLocationFormats = new[]
            {
                string.Format("~/{3}/{2}/{4}/{1}/{0}.cshtml","{0}","{1}","{2}",DefaultAreaFolder,DefaultViewFolder),
                string.Format("~/{3}/{2}/{4}/{1}/{0}.vbhtml","{0}","{1}","{2}",DefaultAreaFolder,DefaultViewFolder),
                string.Format("~/{3}/{2}/{4}/{5}/{1}/{0}.cshtml","{0}","{1}","{2}",DefaultAreaFolder,DefaultViewFolder, DefaultSharedViewFolder),
                string.Format("~/{3}/{2}/{4}/{5}/{1}/{0}.vbhtml","{0}","{1}","{2}",DefaultAreaFolder,DefaultViewFolder, DefaultSharedViewFolder)
            };
            AreaMasterLocationFormats = new[]
            {
                string.Format("~/{3}/{2}/{4}/{1}/{0}.cshtml","{0}","{1}","{2}",DefaultAreaFolder,DefaultViewFolder),
                string.Format("~/{3}/{2}/{4}/{1}/{0}.vbhtml","{0}","{1}","{2}",DefaultAreaFolder,DefaultViewFolder),
                string.Format("~/{3}/{2}/{4}/{5}/{1}/{0}.cshtml","{0}","{1}","{2}",DefaultAreaFolder,DefaultViewFolder, DefaultSharedViewFolder),
                string.Format("~/{3}/{2}/{4}/{5}/{1}/{0}.vbhtml","{0}","{1}","{2}",DefaultAreaFolder,DefaultViewFolder, DefaultSharedViewFolder)
            };
            AreaPartialViewLocationFormats = new[]
            {
                string.Format("~/{3}/{2}/{4}/{1}/{0}.cshtml","{0}","{1}","{2}",DefaultAreaFolder,DefaultViewFolder),
                string.Format("~/{3}/{2}/{4}/{1}/{0}.vbhtml","{0}","{1}","{2}",DefaultAreaFolder,DefaultViewFolder),
                string.Format("~/{3}/{2}/{4}/{5}/{1}/{0}.cshtml","{0}","{1}","{2}",DefaultAreaFolder,DefaultViewFolder, DefaultSharedViewFolder),
                string.Format("~/{3}/{2}/{4}/{5}/{1}/{0}.vbhtml","{0}","{1}","{2}",DefaultAreaFolder,DefaultViewFolder, DefaultSharedViewFolder)
            };

            ViewLocationFormats = new[]
            {
                string.Format("~/{3}/{2}/{1}/{0}.cshtml","{0}","{1}","{2}",DefaultViewFolder),
                string.Format("~/{3}/{2}/{1}/{0}.vbhtml","{0}","{1}","{2}",DefaultViewFolder),
                string.Format("~/{1}/{2}/{0}.cshtml","{0}",DefaultViewFolder, DefaultSharedViewFolder),
                string.Format("~/{1}/{2}/{0}.vbhtml","{0}",DefaultViewFolder, DefaultSharedViewFolder),
            };
            MasterLocationFormats = new[]
            {
                string.Format("~/{3}/{2}/{1}/{0}.cshtml","{0}","{1}","{2}",DefaultViewFolder),
                string.Format("~/{3}/{2}/{1}/{0}.vbhtml","{0}","{1}","{2}",DefaultViewFolder),
                string.Format("~/{1}/{2}/{0}.cshtml","{0}",DefaultViewFolder, DefaultSharedViewFolder),
                string.Format("~/{1}/{2}/{0}.vbhtml","{0}",DefaultViewFolder, DefaultSharedViewFolder),
            };
            PartialViewLocationFormats = new[]
            {
                string.Format("~/{3}/{2}/{1}/{0}.cshtml","{0}","{1}","{2}",DefaultViewFolder),
                string.Format("~/{3}/{2}/{1}/{0}.vbhtml","{0}","{1}","{2}",DefaultViewFolder),
                string.Format("~/{1}/{2}/{0}.cshtml","{0}",DefaultViewFolder, DefaultSharedViewFolder),
                string.Format("~/{1}/{2}/{0}.vbhtml","{0}",DefaultViewFolder, DefaultSharedViewFolder),
            };

            FileExtensions = new[]
            {
                "cshtml",
                "vbhtml",
            };
        }

        protected override IView CreatePartialView(ControllerContext controllerContext, string partialPath)
        {
            return new RazorView(controllerContext, partialPath,
                                 layoutPath: null, runViewStartPages: false, viewStartFileExtensions: FileExtensions, viewPageActivator: ViewPageActivator)
            {
                DisplayModeProvider = DisplayModeProvider
            };
        }

        protected override IView CreateView(ControllerContext controllerContext, string viewPath, string masterPath)
        {
            var view = new RazorView(controllerContext, viewPath,
                                     layoutPath: masterPath, runViewStartPages: true, viewStartFileExtensions: FileExtensions, viewPageActivator: ViewPageActivator)
            {
                DisplayModeProvider = DisplayModeProvider
            };
            return view;
        }
    }
}
