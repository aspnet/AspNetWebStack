// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;

namespace System.Web.Mvc
{
    public abstract class ParameterDescriptor : ICustomAttributeProvider
    {
        private static readonly EmptyParameterBindingInfo _emptyBindingInfo = new EmptyParameterBindingInfo();

        public abstract ActionDescriptor ActionDescriptor { get; }

        public virtual ParameterBindingInfo BindingInfo
        {
            get { return _emptyBindingInfo; }
        }

        public virtual object DefaultValue
        {
            get { return null; }
        }

        public abstract string ParameterName { get; }

        public abstract Type ParameterType { get; }

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

        public virtual bool IsDefined(Type attributeType, bool inherit)
        {
            if (attributeType == null)
            {
                throw new ArgumentNullException("attributeType");
            }

            return false;
        }

        private sealed class EmptyParameterBindingInfo : ParameterBindingInfo
        {
        }
    }
}
