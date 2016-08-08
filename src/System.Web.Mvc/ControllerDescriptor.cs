// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace System.Web.Mvc
{
    public abstract class ControllerDescriptor : ICustomAttributeProvider, IUniquelyIdentifiable
    {
        private readonly Lazy<string> _uniqueId;

        protected ControllerDescriptor()
        {
            _uniqueId = new Lazy<string>(CreateUniqueId);
        }

        public virtual string ControllerName
        {
            get
            {
                string typeName = ControllerType.Name;
                if (typeName.EndsWith("Controller", StringComparison.OrdinalIgnoreCase))
                {
                    return typeName.Substring(0, typeName.Length - "Controller".Length);
                }

                return typeName;
            }
        }

        public abstract Type ControllerType { get; }

        [SuppressMessage("Microsoft.Security", "CA2119:SealMethodsThatSatisfyPrivateInterfaces", Justification = "This is overridden elsewhere in System.Web.Mvc")]
        public virtual string UniqueId
        {
            get { return _uniqueId.Value; }
        }

        private string CreateUniqueId()
        {
            return DescriptorUtil.CreateUniqueId(GetType(), ControllerName, ControllerType);
        }

        public abstract ActionDescriptor FindAction(ControllerContext controllerContext, string actionName);

        public abstract ActionDescriptor[] GetCanonicalActions();

        public virtual object[] GetCustomAttributes(bool inherit)
        {
            return GetCustomAttributes(typeof(object), inherit);
        }

        public virtual object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            if (attributeType == null)
            {
                throw new ArgumentNullException("attributeType");
            }

            return (object[])Array.CreateInstance(attributeType, 0);
        }

        public virtual IEnumerable<FilterAttribute> GetFilterAttributes(bool useCache)
        {
            return GetCustomAttributes(typeof(FilterAttribute), inherit: true).Cast<FilterAttribute>();
        }

        public virtual bool IsDefined(Type attributeType, bool inherit)
        {
            if (attributeType == null)
            {
                throw new ArgumentNullException("attributeType");
            }

            return false;
        }
    }
}
