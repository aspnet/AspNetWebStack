// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Reflection;
using System.Web.Mvc.Properties;

namespace System.Web.Mvc
{
    internal class ReflectedParameterBindingInfo : ParameterBindingInfo
    {
        private readonly ParameterInfo _parameterInfo;
        private ICollection<string> _exclude = new string[0];
        private ICollection<string> _include = new string[0];
        private string _prefix;

        public ReflectedParameterBindingInfo(ParameterInfo parameterInfo)
        {
            _parameterInfo = parameterInfo;
            ReadSettingsFromBindAttribute();
        }

        public override IModelBinder Binder
        {
            get
            {
                IModelBinder binder = ModelBinders.GetBinderFromAttributes(
                    _parameterInfo,
                    (ICustomAttributeProvider errorArg) =>
                    {
                        ParameterInfo parameterInfo = (ParameterInfo)errorArg;
                        throw new InvalidOperationException(
                            String.Format(
                                CultureInfo.CurrentCulture,
                                MvcResources.ReflectedParameterBindingInfo_MultipleConverterAttributes,
                                parameterInfo.Name,
                                parameterInfo.Member));
                    });

                return binder;
            }
        }

        public override ICollection<string> Exclude
        {
            get { return _exclude; }
        }

        public override ICollection<string> Include
        {
            get { return _include; }
        }

        public override string Prefix
        {
            get { return _prefix; }
        }

        private void ReadSettingsFromBindAttribute()
        {
            BindAttribute attr = (BindAttribute)Attribute.GetCustomAttribute(_parameterInfo, typeof(BindAttribute));
            if (attr == null)
            {
                return;
            }

            _exclude = new ReadOnlyCollection<string>(AuthorizeAttribute.SplitString(attr.Exclude));
            _include = new ReadOnlyCollection<string>(AuthorizeAttribute.SplitString(attr.Include));
            _prefix = attr.Prefix;
        }
    }
}
