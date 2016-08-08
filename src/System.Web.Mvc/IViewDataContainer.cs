// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace System.Web.Mvc
{
    public interface IViewDataContainer
    {
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "This is the mechanism by which the ViewPage / ViewUserControl get their ViewDataDictionary objects.")]
        ViewDataDictionary ViewData { get; set; }
    }
}
