// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace WebMatrix.Data
{
    internal interface IConfigurationManager
    {
        IDictionary<string, string> AppSettings { get; }
        IConnectionConfiguration GetConnection(string name);
    }
}
