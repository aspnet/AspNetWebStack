// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace System.Web.Mvc.Async
{
    public class ReflectedAsyncControllerDescriptor : ControllerDescriptor
    {
        internal static readonly Func<Type, ControllerDescriptor> DefaultDescriptorFactory =
            (type) => new ReflectedAsyncControllerDescriptor(type);

        private static readonly ActionDescriptor[] _emptyCanonicalActions = new ActionDescriptor[0];

        private readonly Type _controllerType;
        private readonly AsyncActionMethodSelector _selector;

        public ReflectedAsyncControllerDescriptor(Type controllerType)
        {
            if (controllerType == null)
            {
                throw new ArgumentNullException("controllerType");
            }

            _controllerType = controllerType;
            bool allowLegacyAsyncActions = AllowLegacyAsyncActions(_controllerType);
            _selector = new AsyncActionMethodSelector(_controllerType, allowLegacyAsyncActions);
        }

        public sealed override Type ControllerType
        {
            get { return _controllerType; }
        }

        internal AsyncActionMethodSelector Selector
        {
            get { return _selector; }
        }

        /// <summary>
        /// Determines if we should bind "Foo" to FooAsync/FooCompleted pattern. 
        /// </summary>
        /// <param name="controllerType"></param>
        /// <returns></returns>
        private static bool AllowLegacyAsyncActions(Type controllerType)
        {
            if (typeof(AsyncController).IsAssignableFrom(controllerType))
            {
                return true;
            }
            if (typeof(Controller).IsAssignableFrom(controllerType))
            {
                // for backwards compat. Controller now supports IAsyncController, 
                // but still use synchronous bindings patterns. 
                return false;
            }
            if (!typeof(IAsyncController).IsAssignableFrom(controllerType))
            {
                return false;
            }
            return true;
        }

        public override ActionDescriptor FindAction(ControllerContext controllerContext, string actionName)
        {
            if (controllerContext == null)
            {
                throw new ArgumentNullException("controllerContext");
            }
            if (String.IsNullOrEmpty(actionName))
            {
                throw Error.ParameterCannotBeNullOrEmpty("actionName");
            }

            ActionDescriptorCreator creator = _selector.FindAction(controllerContext, actionName);
            if (creator == null)
            {
                return null;
            }

            return creator(actionName, this);
        }

        public override ActionDescriptor[] GetCanonicalActions()
        {
            // everything is looked up dymanically, so there are no 'canonical' actions
            return _emptyCanonicalActions;
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return ControllerType.GetCustomAttributes(inherit);
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return ControllerType.GetCustomAttributes(attributeType, inherit);
        }

        public override IEnumerable<FilterAttribute> GetFilterAttributes(bool useCache)
        {
            if (useCache && GetType() == typeof(ReflectedAsyncControllerDescriptor))
            {
                // Do not look at cache in types derived from this type because they might incorrectly implement GetCustomAttributes
                return ReflectedAttributeCache.GetTypeFilterAttributes(ControllerType);
            }
            return base.GetFilterAttributes(useCache);
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return ControllerType.IsDefined(attributeType, inherit);
        }
    }
}
