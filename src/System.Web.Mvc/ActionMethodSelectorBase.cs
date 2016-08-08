// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc.Async;
using System.Web.Mvc.Properties;
using System.Web.Mvc.Routing;
using System.Web.Routing;

namespace System.Web.Mvc
{
    // Common base class for Async and Sync action selectors
    internal abstract class ActionMethodSelectorBase
    {
        private StandardRouteActionMethodCache _standardRouteCache;

        protected void Initialize(Type controllerType)
        {
            ControllerType = controllerType;

            var allMethods = ControllerType.GetMethods(BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public);
            ActionMethods = Array.FindAll(allMethods, IsValidActionMethod);

            // The attribute routing mapper will remove methods from this set as they are mapped.
            // The lookup tables are initialized lazily to ensure that direct routing's changes are respected.
            StandardRouteMethods = new HashSet<MethodInfo>(ActionMethods);
        }

        public Type ControllerType { get; private set; }

        /// <summary>
        /// All action methods.
        /// </summary>
        public MethodInfo[] ActionMethods { get; private set; }

        /// <summary>
        /// Methods with no direct route, reachable via standard routing only
        /// </summary>
        public HashSet<MethodInfo> StandardRouteMethods { get; private set; }

        /// <summary>
        /// Methods which have ActionNameSelectorAttributes - these methods have dynamic functionality
        /// and might choose to opt-in to any request.
        /// </summary>
        public MethodInfo[] AliasedMethods 
        { 
            get 
            {
                return StandardRouteCache.AliasedMethods;
            }
        }

        /// <summary>
        /// Methods which do not have ActionNameSelectorAttributes - these are selected statically by name.
        /// </summary>
        public ILookup<string, MethodInfo> NonAliasedMethods
        {
            get
            {
                return StandardRouteCache.NonAliasedMethods;
            }
        }

        private StandardRouteActionMethodCache StandardRouteCache
        {
            get
            {
                if (_standardRouteCache == null)
                {
                    // This data structure is immutable, so it's safe for multiple threads to race to create it.
                    _standardRouteCache = CreateStandardRouteCache();
                }

                return _standardRouteCache;
            }
        }

        protected AmbiguousMatchException CreateAmbiguousActionMatchException(IEnumerable<MethodInfo> ambiguousMethods, string actionName)
        {
            string ambiguityList = CreateAmbiguousMatchList(ambiguousMethods);
            string message = String.Format(CultureInfo.CurrentCulture, MvcResources.ActionMethodSelector_AmbiguousMatch,
                                           actionName, ControllerType.Name, ambiguityList);
            return new AmbiguousMatchException(message);
        }

        protected AmbiguousMatchException CreateAmbiguousMethodMatchException(IEnumerable<MethodInfo> ambiguousMethods, string methodName)
        {
            string ambiguityList = CreateAmbiguousMatchList(ambiguousMethods);
            string message = String.Format(CultureInfo.CurrentCulture, MvcResources.AsyncActionMethodSelector_AmbiguousMethodMatch,
                                           methodName, ControllerType.Name, ambiguityList);
            return new AmbiguousMatchException(message);
        }

        protected static string CreateAmbiguousMatchList(IEnumerable<MethodInfo> ambiguousMethods)
        {
            StringBuilder exceptionMessageBuilder = new StringBuilder();
            foreach (MethodInfo methodInfo in ambiguousMethods)
            {
                string controllerAction = Convert.ToString(methodInfo, CultureInfo.CurrentCulture);
                string controllerType = methodInfo.DeclaringType.FullName;

                exceptionMessageBuilder.AppendLine();
                exceptionMessageBuilder.AppendFormat(CultureInfo.CurrentCulture, MvcResources.ActionMethodSelector_AmbiguousMatchType, controllerAction, controllerType);
            }

            return exceptionMessageBuilder.ToString();
        }

        private static bool IsMethodDecoratedWithAliasingAttribute(MethodInfo methodInfo)
        {
            return methodInfo.IsDefined(typeof(ActionNameSelectorAttribute), true /* inherit */);
        }

        protected abstract bool IsValidActionMethod(MethodInfo methodInfo);

        // Get the method name (before applying Aliasing attributes).
        protected virtual string GetCanonicalMethodName(MethodInfo methodInfo)
        {
            string methodName = methodInfo.Name;
            return methodName;
        }

        private StandardRouteActionMethodCache CreateStandardRouteCache()
        {
            var cache = new StandardRouteActionMethodCache();
            cache.AliasedMethods = StandardRouteMethods.Where(IsMethodDecoratedWithAliasingAttribute).ToArray();
            cache.NonAliasedMethods = StandardRouteMethods
                .Except(cache.AliasedMethods)
                .ToLookup(GetCanonicalMethodName, StringComparer.OrdinalIgnoreCase);

            return cache;
        }

        protected List<MethodInfo> FindActionMethods(ControllerContext controllerContext, string actionName)
        {
            List<MethodInfo> matches = new List<MethodInfo>();

            var cache = StandardRouteCache;

            // Performance sensitive, so avoid foreach
            for (int i = 0; i < cache.AliasedMethods.Length; i++)
            {
                MethodInfo method = cache.AliasedMethods[i];
                if (IsMatchingAliasedMethod(method, controllerContext, actionName))
                {
                    matches.Add(method);
                }
            }
            matches.AddRange(cache.NonAliasedMethods[actionName]);
            RunSelectionFilters(controllerContext, matches);
            return matches;
        }

        protected static bool IsMatchingAliasedMethod(MethodInfo method, ControllerContext controllerContext, string actionName)
        {
            // return if aliased method is opting in to this request
            // to opt in, all attributes defined on the method must return true
            ReadOnlyCollection<ActionNameSelectorAttribute> attributes = ReflectedAttributeCache.GetActionNameSelectorAttributes(method);
            // Caching count is faster for ReadOnlyCollection
            int attributeCount = attributes.Count;
            // Performance sensitive, so avoid foreach
            for (int i = 0; i < attributeCount; i++)
            {
                if (!attributes[i].IsValidName(controllerContext, actionName, method))
                {
                    return false;
                }
            }
            return true;
        }

        protected static bool IsValidMethodSelector(ReadOnlyCollection<ActionMethodSelectorAttribute> attributes, ControllerContext controllerContext, MethodInfo method)
        {
            int attributeCount = attributes.Count;
            Contract.Assert(attributeCount > 0);
            for (int i = 0; i < attributeCount; i++)
            {
                if (!attributes[i].IsValidForRequest(controllerContext, method))
                {
                    return false;
                }
            }
            return true;
        }

        protected static void RunSelectionFilters(ControllerContext controllerContext, List<MethodInfo> methodInfos)
        {
            // Filter depending on the selection attribute.
            // Methods with valid selection attributes override all others.
            // Methods with one or more invalid selection attributes are removed.

            bool hasValidSelectionAttributes = false;
            // loop backwards for fastest removal
            for (int i = methodInfos.Count - 1; i >= 0; i--)
            {
                MethodInfo methodInfo = methodInfos[i];
                ReadOnlyCollection<ActionMethodSelectorAttribute> attrs = ReflectedAttributeCache.GetActionMethodSelectorAttributesCollection(methodInfo);
                if (attrs.Count == 0)
                {
                    // case 1: this method does not have a MethodSelectionAttribute

                    if (hasValidSelectionAttributes)
                    {
                        // if there is already method with a valid selection attribute, remove method without one
                        methodInfos.RemoveAt(i);
                    }
                }
                else if (IsValidMethodSelector(attrs, controllerContext, methodInfo))
                {
                    // case 2: this method has MethodSelectionAttributes that are all valid

                    // if a matching action method had a selection attribute, consider it more specific than a matching action method
                    // without a selection attribute
                    if (!hasValidSelectionAttributes)
                    {
                        // when the first selection attribute is discovered, remove any items later in the list without selection attributes
                        if (i + 1 < methodInfos.Count)
                        {
                            methodInfos.RemoveFrom(i + 1);
                        }
                        hasValidSelectionAttributes = true;
                    }
                }
                else
                {
                    // case 3: this method has a method selection attribute but it is not valid

                    // remove the method since it is opting out of this request
                    methodInfos.RemoveAt(i);
                }
            }
        }

        // Get the action name for the method.         
        public string GetActionName(MethodInfo methodInfo)
        {
            // Check for ActionName attribute
            object[] nameAttributes = methodInfo.GetCustomAttributes(typeof(ActionNameAttribute), inherit: true);
            if (nameAttributes.Length > 0)
            {
                ActionNameAttribute nameAttribute = nameAttributes[0] as ActionNameAttribute;
                if (nameAttribute != null)
                {
                    return nameAttribute.Name;
                }
            }

            return GetCanonicalMethodName(methodInfo);
        }

        public MethodInfo FindActionMethod(ControllerContext controllerContext, string actionName)
        {
            if (controllerContext == null)
            {
                throw Error.ArgumentNull("controllerContext");
            }

            if (actionName == null)
            {
                throw Error.ArgumentNull("actionName");
            }

            List<MethodInfo> finalMethods = FindActionMethods(controllerContext, actionName);

            switch (finalMethods.Count)
            {
                case 0:
                    return null;

                case 1:
                    return finalMethods[0];

                default:
                    throw CreateAmbiguousActionMatchException(finalMethods, actionName);
            }
        }

        private class StandardRouteActionMethodCache
        {
            public MethodInfo[] AliasedMethods { get; set; }

            public ILookup<string, MethodInfo> NonAliasedMethods { get; set; }
        }
    }
}