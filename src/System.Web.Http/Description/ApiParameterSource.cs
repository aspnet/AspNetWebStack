// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Http.Description
{
    /// <summary>
    /// Describes where the parameter come from.
    /// </summary>
    public enum ApiParameterSource
    {
        FromUri = 0,
        FromBody,
        Unknown
    }
}
