// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Http.Controllers
{
    internal static class HttpActionDescriptorExtensions
    {
        private const string AttributeRoutedPropertyKey = "MS_IsAttributeRouted";

        public static bool IsAttributeRouted(this HttpActionDescriptor actionDescriptor)
        {
            if (actionDescriptor == null)
            {
                throw new ArgumentNullException("actionDescriptor");
            }

            object value;
            actionDescriptor.Properties.TryGetValue(AttributeRoutedPropertyKey, out value);
            return value as bool? ?? false;
        }

        public static void SetIsAttributeRouted(this HttpActionDescriptor actionDescriptor, bool value)
        {
            if (actionDescriptor == null)
            {
                throw new ArgumentNullException("actionDescriptor");
            }

            actionDescriptor.Properties[AttributeRoutedPropertyKey] = value;
        }
    }
}
