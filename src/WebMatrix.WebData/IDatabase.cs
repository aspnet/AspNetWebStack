// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace WebMatrix.WebData
{
    internal interface IDatabase : IDisposable
    {
        dynamic QuerySingle(string commandText, params object[] args);

        IEnumerable<dynamic> Query(string commandText, params object[] parameters);

        dynamic QueryValue(string commandText, params object[] parameters);

        int Execute(string commandText, params object[] args);
    }
}
