// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Globalization;
using System.IO;
using System.Web.Mvc.Properties;

namespace System.Web.Mvc
{
    public abstract class BuildManagerCompiledView : IView
    {
        internal IViewPageActivator ViewPageActivator;
        private IBuildManager _buildManager;
        private ControllerContext _controllerContext;

        protected BuildManagerCompiledView(ControllerContext controllerContext, string viewPath)
            : this(controllerContext, viewPath, null)
        {
        }

        protected BuildManagerCompiledView(ControllerContext controllerContext, string viewPath, IViewPageActivator viewPageActivator)
            : this(controllerContext, viewPath, viewPageActivator, null)
        {
        }

        internal BuildManagerCompiledView(ControllerContext controllerContext, string viewPath, IViewPageActivator viewPageActivator, IDependencyResolver dependencyResolver)
        {
            if (controllerContext == null)
            {
                throw new ArgumentNullException("controllerContext");
            }
            if (String.IsNullOrEmpty(viewPath))
            {
                throw new ArgumentException(MvcResources.Common_NullOrEmpty, "viewPath");
            }

            _controllerContext = controllerContext;

            ViewPath = viewPath;

            ViewPageActivator = viewPageActivator ?? new BuildManagerViewEngine.DefaultViewPageActivator(dependencyResolver);
        }

        internal IBuildManager BuildManager
        {
            get
            {
                if (_buildManager == null)
                {
                    _buildManager = new BuildManagerWrapper();
                }
                return _buildManager;
            }
            set { _buildManager = value; }
        }

        public string ViewPath { get; protected set; }

        public virtual void Render(ViewContext viewContext, TextWriter writer)
        {
            if (viewContext == null)
            {
                throw new ArgumentNullException("viewContext");
            }

            object instance = null;

            Type type = BuildManager.GetCompiledType(ViewPath);
            if (type != null)
            {
                instance = ViewPageActivator.Create(_controllerContext, type);
            }

            if (instance == null)
            {
                throw new InvalidOperationException(
                    String.Format(
                        CultureInfo.CurrentCulture,
                        MvcResources.CshtmlView_ViewCouldNotBeCreated,
                        ViewPath));
            }

            RenderView(viewContext, writer, instance);
        }

        protected abstract void RenderView(ViewContext viewContext, TextWriter writer, object instance);
    }
}
