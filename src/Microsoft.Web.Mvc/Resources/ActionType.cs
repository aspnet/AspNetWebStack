// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Web.Mvc.Resources
{
    /// <summary>
    /// This enum is used by the UrlHelper extension methods to create links within resource controllers
    /// </summary>
    public enum ActionType
    {
        Create,
        GetCreateForm,
        Index,
        Retrieve,
        Update,
        GetUpdateForm,
        Delete,
    }
}
