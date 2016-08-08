// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.Web.Mvc.Properties;

namespace Microsoft.Web.Mvc
{
    internal class DynamicReflectionObject : DynamicObject
    {
        private readonly object _realObject;

        private DynamicReflectionObject(object realObject)
        {
            _realObject = realObject;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            PropertyInfo propInfo = _realObject.GetType().GetProperty(binder.Name);

            if (propInfo == null)
            {
                PropertyInfo[] properties = _realObject.GetType().GetProperties();
                if (properties.Length == 0)
                {
                    throw new InvalidOperationException(
                        String.Format(CultureInfo.CurrentCulture,
                                      MvcResources.DynamicViewPage_NoProperties,
                                      binder.Name));
                }

                string propNames = properties.Select(p => p.Name)
                    .OrderBy(name => name)
                    .Aggregate((left, right) => left + ", " + right);

                throw new InvalidOperationException(
                    String.Format(CultureInfo.CurrentCulture,
                                  MvcResources.DynamicViewPage_PropertyDoesNotExist,
                                  binder.Name,
                                  propNames));
            }

            result = Wrap(propInfo.GetValue(_realObject, null));
            return true;
        }

        public static dynamic Wrap(object obj)
        {
            // We really only want to wrap anonymous objects, but there's no surefire way to determine
            // that an object is anonymous. We'll use the best metrics we can (internal non-nested type
            // that derives directly from Object).
            if (obj != null)
            {
                Type type = obj.GetType();
                if (!type.IsPublic && type.BaseType == typeof(Object) && type.DeclaringType == null)
                {
                    return new DynamicReflectionObject(obj);
                }
            }

            return obj;
        }
    }
}
