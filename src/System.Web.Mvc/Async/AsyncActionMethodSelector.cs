// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace System.Web.Mvc.Async
{
    internal sealed class AsyncActionMethodSelector : ActionMethodSelectorBase
    {
        // This flag controls async action binding for backwards compat since Controller now supports async. 
        // Set to true for classes that derive from AsyncController. In this case, FooAsync/FooCompleted is 
        // bound as a single async action pair "Foo". If false, they're bound as 2 separate sync actions. 
        // Practically, if this is false, then IsAsyncSuffixedMethod and IsCompeltedSuffixedMethod return false.
        private readonly bool _allowLegacyAsyncActions;

        public AsyncActionMethodSelector(Type controllerType, bool allowLegacyAsyncActions = true)
        {
            _allowLegacyAsyncActions = allowLegacyAsyncActions;
            Initialize(controllerType);
        }
        
        public ActionDescriptorCreator FindAction(ControllerContext controllerContext, string actionName)
        {
            MethodInfo method = FindActionMethod(controllerContext, actionName);

            if (method == null)
            {
                return null;
            }

            return GetActionDescriptorDelegate(method);
        }

        internal bool AllowLegacyAsyncActions
        {
            get { return _allowLegacyAsyncActions; }
        }

        // This method and GetMethodInfo need to stay in sync, we need to be able to
        // get a method info from each type of action descriptor we create.
        internal ActionDescriptorCreator GetActionDescriptorDelegate(MethodInfo entryMethod)
        {
            // Does the action return a Task?
            if (entryMethod.ReturnType != null && typeof(Task).IsAssignableFrom(entryMethod.ReturnType))
            {
                return (actionName, controllerDescriptor) => new TaskAsyncActionDescriptor(entryMethod, actionName, controllerDescriptor);
            }

            // Is this the FooAsync() / FooCompleted() pattern?
            if (IsAsyncSuffixedMethod(entryMethod))
            {
                string completionMethodName = entryMethod.Name.Substring(0, entryMethod.Name.Length - "Async".Length) + "Completed";
                MethodInfo completionMethod = GetMethodByName(completionMethodName);
                if (completionMethod != null)
                {
                    return (actionName, controllerDescriptor) => new ReflectedAsyncActionDescriptor(entryMethod, completionMethod, actionName, controllerDescriptor);
                }
                else
                {
                    throw Error.AsyncActionMethodSelector_CouldNotFindMethod(completionMethodName, ControllerType);
                }
            }

            // Fallback to synchronous method
            return (actionName, controllerDescriptor) => new ReflectedActionDescriptor(entryMethod, actionName, controllerDescriptor);
        }

        protected override bool IsValidActionMethod(MethodInfo methodInfo)
        {
            return IsValidActionMethod(methodInfo, true /* stripInfrastructureMethods */);
        }

        private bool IsValidActionMethod(MethodInfo methodInfo, bool stripInfrastructureMethods)
        {
            if (methodInfo.IsSpecialName)
            {
                // not a normal method, e.g. a constructor or an event
                return false;
            }

            if (methodInfo.GetBaseDefinition().DeclaringType.IsAssignableFrom(typeof(AsyncController)))
            {
                // is a method on Object, ControllerBase, Controller, or AsyncController
                return false;
            }

            if (stripInfrastructureMethods)
            {
                if (IsCompletedSuffixedMethod(methodInfo))
                {
                    // do not match FooCompleted() methods, as these are infrastructure methods
                    return false;
                }
            }

            return true;
        }

        protected override string GetCanonicalMethodName(MethodInfo methodInfo)
        {
            string methodName = methodInfo.Name;
            return (IsAsyncSuffixedMethod(methodInfo))
                       ? methodName.Substring(0, methodName.Length - "Async".Length)
                       : methodName;
        }

        private bool IsAsyncSuffixedMethod(MethodInfo methodInfo)
        {
            return _allowLegacyAsyncActions && methodInfo.Name.EndsWith("Async", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsCompletedSuffixedMethod(MethodInfo methodInfo)
        {
            return _allowLegacyAsyncActions && methodInfo.Name.EndsWith("Completed", StringComparison.OrdinalIgnoreCase);
        }

        private MethodInfo GetMethodByName(string methodName)
        {
            List<MethodInfo> methods = (from MethodInfo methodInfo in ControllerType.GetMember(methodName, MemberTypes.Method, BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod | BindingFlags.IgnoreCase)
                                        where IsValidActionMethod(methodInfo, false /* stripInfrastructureMethods */)
                                        select methodInfo).ToList();

            switch (methods.Count)
            {
                case 0:
                    return null;

                case 1:
                    return methods[0];

                default:
                    throw CreateAmbiguousMethodMatchException(methods, methodName);
            }
        }
    }
}
