// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.Mvc;

namespace Microsoft.Web.Mvc
{
    internal static class TypeDescriptorHelper
    {
        private static readonly MockMetadataProvider _mockMetadataProvider = new MockMetadataProvider();

        public static ICustomTypeDescriptor Get(Type type)
        {
            return _mockMetadataProvider.GetTypeDescriptor(type);
        }

        // System.Web.Mvc.TypeDescriptorHelpers is internal, so this mock subclassed type provides
        // access to it via the GetTypeDescriptor() virtual method.
        private sealed class MockMetadataProvider : AssociatedMetadataProvider
        {
            protected override ModelMetadata CreateMetadata(IEnumerable<Attribute> attributes, Type containerType, Func<object> modelAccessor, Type modelType, string propertyName)
            {
                throw new NotImplementedException();
            }

            public new ICustomTypeDescriptor GetTypeDescriptor(Type type)
            {
                return base.GetTypeDescriptor(type);
            }
        }
    }
}
