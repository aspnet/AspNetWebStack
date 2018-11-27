using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Web.Mvc
{
    public static class ViewPageActivatorExtensions
    {
        public static object Create<T>(this IViewPageActivator viewPageActivator, ControllerContext controllerContext, Type type)
        {
            return viewPageActivator.Create(controllerContext, type, new Type[] { typeof(T) });
        }
        public static object Create<T1, T2>(this IViewPageActivator viewPageActivator, ControllerContext controllerContext, Type type)
        {
            return viewPageActivator.Create(controllerContext, type, new Type[] { typeof(T1), typeof(T2) });
        }
        public static object Create<T1, T2, T3>(this IViewPageActivator viewPageActivator, ControllerContext controllerContext, Type type)
        {
            return viewPageActivator.Create(controllerContext, type, new Type[] { typeof(T1), typeof(T2), typeof(T3) });
        }
        public static object Create<T1, T2, T3, T4>(this IViewPageActivator viewPageActivator, ControllerContext controllerContext, Type type)
        {
            return viewPageActivator.Create(controllerContext, type, new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) });
        }
        public static object Create<T1, T2, T3, T4, T5>(this IViewPageActivator viewPageActivator, ControllerContext controllerContext, Type type)
        {
            return viewPageActivator.Create(controllerContext, type, new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) });
        }

    }
}
