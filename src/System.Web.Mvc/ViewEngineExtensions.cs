using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Web.Mvc
{
    // ------------------- Branch: support_generic_models_in_views (start) -------------------
    public static class ViewEngineExtensions
    {
        public static ViewEngineResult FindPartialView<T>(this IViewEngine viewEngine, ControllerContext controllerContext, string partialViewName, bool useCache)
        {
            return viewEngine.FindPartialView(controllerContext, partialViewName, useCache, new Type[] { typeof(T) });
        }
        public static ViewEngineResult FindView<T>(this IViewEngine viewEngine, ControllerContext controllerContext, string viewName, string masterName, bool useCache)
        {
            return viewEngine.FindView(controllerContext, viewName, masterName, useCache, new Type[] { typeof(T) });
        }
        public static ViewEngineResult FindPartialView<T1, T2>(this IViewEngine viewEngine, ControllerContext controllerContext, string partialViewName, bool useCache)
        {
            return viewEngine.FindPartialView(controllerContext, partialViewName, useCache, new Type[] { typeof(T1), typeof(T2) });
        }
        public static ViewEngineResult FindView<T1, T2>(this IViewEngine viewEngine, ControllerContext controllerContext, string viewName, string masterName, bool useCache)
        {
            return viewEngine.FindView(controllerContext, viewName, masterName, useCache, new Type[] { typeof(T1), typeof(T2) });
        }
        public static ViewEngineResult FindPartialView<T1, T2, T3>(this IViewEngine viewEngine, ControllerContext controllerContext, string partialViewName, bool useCache)
        {
            return viewEngine.FindPartialView(controllerContext, partialViewName, useCache, new Type[] { typeof(T1), typeof(T2), typeof(T3) });
        }
        public static ViewEngineResult FindView<T1, T2, T3>(this IViewEngine viewEngine, ControllerContext controllerContext, string viewName, string masterName, bool useCache)
        {
            return viewEngine.FindView(controllerContext, viewName, masterName, useCache, new Type[] { typeof(T1), typeof(T2), typeof(T3) });
        }
        public static ViewEngineResult FindPartialView<T1, T2, T3, T4>(this IViewEngine viewEngine, ControllerContext controllerContext, string partialViewName, bool useCache)
        {
            return viewEngine.FindPartialView(controllerContext, partialViewName, useCache, new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) });
        }
        public static ViewEngineResult FindView<T1, T2, T3, T4>(this IViewEngine viewEngine, ControllerContext controllerContext, string viewName, string masterName, bool useCache)
        {
            return viewEngine.FindView(controllerContext, viewName, masterName, useCache, new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) });
        }
        public static ViewEngineResult FindPartialView<T1, T2, T3, T4, T5>(this IViewEngine viewEngine, ControllerContext controllerContext, string partialViewName, bool useCache)
        {
            return viewEngine.FindPartialView(controllerContext, partialViewName, useCache, new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) });
        }
        public static ViewEngineResult FindView<T1, T2, T3, T4, T5>(this IViewEngine viewEngine, ControllerContext controllerContext, string viewName, string masterName, bool useCache)
        {
            return viewEngine.FindView(controllerContext, viewName, masterName, useCache, new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) });
        }
    }
    // ------------------- Branch: support_generic_models_in_views ( end ) -------------------
}
