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
        // ------------------- Branch: support_generic_models_in_views (start) -------------------
        private Type[] _genericTypes;
        // ------------------- Branch: support_generic_models_in_views ( end ) -------------------
        // we need to give access to _controllerContext to BuildManagerCompiledView<T>
        // so that it can use it in its Render() method.
        // we add the following protected readonly property for this reason
        protected ControllerContext ControllerContext
        {
            get { return _controllerContext; }
        }

        protected BuildManagerCompiledView(ControllerContext controllerContext, string viewPath)
            : this(controllerContext, viewPath, null)
        {
        }
        // ------------------- Branch: support_generic_models_in_views (start) -------------------
        protected BuildManagerCompiledView(ControllerContext controllerContext, string viewPath, IViewPageActivator viewPageActivator)
            : this(controllerContext: controllerContext, viewPath: viewPath, viewPageActivator: viewPageActivator, genericTypes: null)
        {
        }
        protected BuildManagerCompiledView(ControllerContext controllerContext, string viewPath, IViewPageActivator viewPageActivator, Type[] genericTypes)
            : this(controllerContext, viewPath, viewPageActivator, null, genericTypes)
        {
        }
        // ------------------- Branch: support_generic_models_in_views ( end ) -------------------
        internal BuildManagerCompiledView(ControllerContext controllerContext, string viewPath, IViewPageActivator viewPageActivator, IDependencyResolver dependencyResolver, Type[] genericTypes)
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
            _genericTypes = genericTypes;

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
                if (_genericTypes != null)
                    instance = ViewPageActivator.Create(_controllerContext, type, _genericTypes);
                else
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
