// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Dynamic;
using System.Globalization;
using System.Reflection;

namespace System.Web.WebPages
{
    // Allows dynamic access over a CLR object via private reflection
    internal sealed class ReflectionDynamicObject : DynamicObject
    {
        private object RealObject { get; set; }

        public static object WrapObjectIfInternal(object o)
        {
            // If it's null, don't try to wrap it
            if (o == null)
            {
                return null;
            }

            // If it's public, leave it alone since the standard dynamic binder will work. Well, it won't work for
            // internal properties, but we're mostly concerned about supporting anonymous objects, which are never public
            if (o.GetType().IsPublic)
            {
                return o;
            }

            return new ReflectionDynamicObject() { RealObject = o };
        }

        // Called when a property is accessed
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            // Get the property info
            PropertyInfo propInfo = RealObject.GetType().GetProperty(
                binder.Name,
                BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public);

            if (propInfo == null)
            {
                // If there is no such property, return null instead of failing. This allows optional parameters
                result = null;
            }
            else
            {
                // Get the property value
                result = propInfo.GetValue(RealObject, null);

                // Wrap the sub object if necessary. This allows nested anonymous objects to work.
                result = WrapObjectIfInternal(result);
            }

            return true;
        }

        // Called when a method is called
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            result = RealObject.GetType().InvokeMember(
                binder.Name,
                BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                RealObject,
                args,
                CultureInfo.InvariantCulture);

            return true;
        }

        // Called when the dynamic object needs to be converted to a non dynamic object
        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            result = RealObject;
            return true;
        }

        public override string ToString()
        {
            // Just return the original object's display string
            return RealObject.ToString();
        }
    }
}
