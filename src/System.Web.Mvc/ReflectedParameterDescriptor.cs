// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;

namespace System.Web.Mvc
{
    public class ReflectedParameterDescriptor : ParameterDescriptor
    {
        private readonly ActionDescriptor _actionDescriptor;
        private readonly ReflectedParameterBindingInfo _bindingInfo;

        public ReflectedParameterDescriptor(ParameterInfo parameterInfo, ActionDescriptor actionDescriptor)
        {
            if (parameterInfo == null)
            {
                throw new ArgumentNullException("parameterInfo");
            }
            if (actionDescriptor == null)
            {
                throw new ArgumentNullException("actionDescriptor");
            }

            ParameterInfo = parameterInfo;
            _actionDescriptor = actionDescriptor;
            _bindingInfo = new ReflectedParameterBindingInfo(parameterInfo);
        }

        public override ActionDescriptor ActionDescriptor
        {
            get { return _actionDescriptor; }
        }

        public override ParameterBindingInfo BindingInfo
        {
            get { return _bindingInfo; }
        }

        public override object DefaultValue
        {
            get
            {
                object value;
                if (ParameterInfoUtil.TryGetDefaultValue(ParameterInfo, out value))
                {
                    return value;
                }
                else
                {
                    return base.DefaultValue;
                }
            }
        }

        public ParameterInfo ParameterInfo { get; private set; }

        public override string ParameterName
        {
            get { return ParameterInfo.Name; }
        }

        public override Type ParameterType
        {
            get { return ParameterInfo.ParameterType; }
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return ParameterInfo.GetCustomAttributes(inherit);
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return ParameterInfo.GetCustomAttributes(attributeType, inherit);
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return ParameterInfo.IsDefined(attributeType, inherit);
        }
    }
}
