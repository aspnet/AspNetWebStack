// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace System.Web.Http.Controllers
{
    /// <summary>
    /// Represents a container for services that can be specific to a controller. 
    /// This shadows the services from its parent <see cref="ServicesContainer"/>. A controller can either set a service here, or fall through 
    /// to the more global set of services. 
    /// </summary>
    public class ControllerServices : ServicesContainer
    {
        // This lists specific services that have been over ridden for the controller.
        // Anything missing means just fall through and ask the _parent. 
        // This dictionary is only written at initialization time, and then read-only during steady state.
        // So it can be safely read from multiple threads after initialization.
        private Dictionary<Type, object> _overrideSingle;
        private Dictionary<Type, List<object>> _overrideMulti;
        private readonly ServicesContainer _parent;

        public ControllerServices(ServicesContainer parent)
        {
            if (parent == null)
            {
                throw Error.ArgumentNull("parent");
            }
            _parent = parent;
        }

        public override bool IsSingleService(Type serviceType)
        {
            return _parent.IsSingleService(serviceType);
        }

        public override object GetService(Type serviceType)
        {
            if (_overrideSingle != null)
            {
                object item;
                if (_overrideSingle.TryGetValue(serviceType, out item))
                {
                    return item;
                }
            }
            return _parent.GetService(serviceType);
        }

        public override IEnumerable<object> GetServices(Type serviceType)
        {
            if (_overrideMulti != null)
            {
                List<object> list;
                if (_overrideMulti.TryGetValue(serviceType, out list))
                {
                    return list;
                }
            }
            return _parent.GetServices(serviceType);
        }

        /// <inheritdoc/>
        protected override void ReplaceSingle(Type serviceType, object service)
        {
            if (_overrideSingle == null)
            {
                _overrideSingle = new Dictionary<Type, object>();
            }
            _overrideSingle[serviceType] = service;
        }

        /// <inheritdoc/>
        protected override void ClearSingle(Type serviceType)
        {
            if (_overrideSingle == null)
            {
                return;
            }
            _overrideSingle.Remove(serviceType);
        }

        // This is called to request a mutation to services.
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "want a mutable list")]
        protected override List<object> GetServiceInstances(Type serviceType)
        {
            if (_overrideMulti == null)
            {
                _overrideMulti = new Dictionary<Type, List<object>>();
            }
            List<object> list;
            if (!_overrideMulti.TryGetValue(serviceType, out list))
            {
                // Copy parents list. 
                list = new List<object>(_parent.GetServices(serviceType));

                // Copy into per-controller. If they're asking for the list, the expectation is that it's going to get mutated.
                _overrideMulti[serviceType] = list;
            }
            return list;
        }
    }
}