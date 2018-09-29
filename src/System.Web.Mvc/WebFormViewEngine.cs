﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Mvc
{
    public class WebFormViewEngine : BuildManagerViewEngine
    {
        public WebFormViewEngine()
            : this(null)
        {
        }

        public WebFormViewEngine(IViewPageActivator viewPageActivator)
            : base(viewPageActivator)
        {
            MasterLocationFormats = new[]
            {
                "~/Views/{1}/{0}.master",
                "~/Views/Shared/{0}.master"
            };

            AreaMasterLocationFormats = new[]
            {
                "~/Areas/{2}/Views/{1}/{0}.master",
                "~/Areas/{2}/Views/Shared/{0}.master",
            };

            ViewLocationFormats = new[]
            {
                "~/Views/{1}/{0}.aspx",
                "~/Views/{1}/{0}.ascx",
                "~/Views/Shared/{0}.aspx",
                "~/Views/Shared/{0}.ascx"
            };

            AreaViewLocationFormats = new[]
            {
                "~/Areas/{2}/Views/{1}/{0}.aspx",
                "~/Areas/{2}/Views/{1}/{0}.ascx",
                "~/Areas/{2}/Views/Shared/{0}.aspx",
                "~/Areas/{2}/Views/Shared/{0}.ascx",
            };

            PartialViewLocationFormats = ViewLocationFormats;
            AreaPartialViewLocationFormats = AreaViewLocationFormats;

            FileExtensions = new[]
            {
                "aspx",
                "ascx",
                "master",
            };
        }

        protected override IView CreatePartialView(ControllerContext controllerContext, string partialPath)
        {
            return new WebFormView(controllerContext, partialPath, null, ViewPageActivator);
        }
        protected override IView CreateView(ControllerContext controllerContext, string viewPath, string masterPath)
        {
            return new WebFormView(controllerContext, viewPath, masterPath, ViewPageActivator);
        }
        // ------------------- Branch: support_generic_models_in_views (start) -------------------
        protected override IView CreatePartialView(ControllerContext controllerContext, string partialPath, Type[] genericTypes)
        {
            return new WebFormView(controllerContext, partialPath, null, ViewPageActivator, genericTypes);
        }
        protected override IView CreateView(ControllerContext controllerContext, string viewPath, string masterPath, Type[] genericTypes)
        {
            return new WebFormView(controllerContext, viewPath, masterPath, ViewPageActivator, genericTypes);
        }
        // ------------------- Branch: support_generic_models_in_views ( end ) -------------------
    }
}
