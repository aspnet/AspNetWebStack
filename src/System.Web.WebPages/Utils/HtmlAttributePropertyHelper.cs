// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace System.Web.WebPages
{
    internal class HtmlAttributePropertyHelper : PropertyHelper
    {
        private static ConcurrentDictionary<Type, PropertyHelper[]> _reflectionCache = new ConcurrentDictionary<Type, PropertyHelper[]>();

        public static new PropertyHelper[] GetProperties(object instance)
        {
            return GetProperties(instance, CreateInstance, _reflectionCache);
        }

        private static PropertyHelper CreateInstance(PropertyInfo property)
        {
            return new HtmlAttributePropertyHelper(property);
        }

        public HtmlAttributePropertyHelper(PropertyInfo property)
            : base(property)
        {
        }

        public override string Name
        {
            get
            {
                return base.Name;
            }

            protected set
            {
                base.Name = value == null ? null : value.Replace('_', '-');
            }
        }
    }
}
