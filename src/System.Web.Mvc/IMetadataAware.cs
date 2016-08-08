// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Mvc
{
    // This interface is implemented by attributes which wish to contribute to the
    // ModelMetadata creation process without needing to write a custom metadata
    // provider. It is consumed by AssociatedMetadataProvider, so this behavior is
    // automatically inherited by all classes which derive from it (notably, the
    // DataAnnotationsModelMetadataProvider).
    public interface IMetadataAware
    {
        void OnMetadataCreated(ModelMetadata metadata);
    }
}
