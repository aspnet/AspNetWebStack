// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace System.Web.Mvc
{
    public abstract class ModelMetadataProvider
    {
        public abstract IEnumerable<ModelMetadata> GetMetadataForProperties(object container, Type containerType);

        public abstract ModelMetadata GetMetadataForProperty(Func<object> modelAccessor, Type containerType, string propertyName);

        public abstract ModelMetadata GetMetadataForType(Func<object> modelAccessor, Type modelType);
    }
}
