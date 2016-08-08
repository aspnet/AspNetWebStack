// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Http.Controllers
{
    internal static class HttpControllerDescriptorExtensions
    {
        private const string AttributeRoutedPropertyKey = "MS_IsAttributeRouted";

        public static bool IsAttributeRouted(this HttpControllerDescriptor controllerDescriptor)
        {
            if (controllerDescriptor == null)
            {
                throw new ArgumentNullException("controllerDescriptor");
            }

            object value;
            controllerDescriptor.Properties.TryGetValue(AttributeRoutedPropertyKey, out value);
            return value as bool? ?? false;
        }

        public static void SetIsAttributeRouted(this HttpControllerDescriptor controllerDescriptor, bool value)
        {
            if (controllerDescriptor == null)
            {
                throw new ArgumentNullException("controllerDescriptor");
            }

            controllerDescriptor.Properties[AttributeRoutedPropertyKey] = value;
        }
    }
}
