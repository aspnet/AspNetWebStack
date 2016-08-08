// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace WebMatrix.WebData.Test
{
    public abstract class MockDatabase : IDatabase
    {
        public abstract dynamic QuerySingle(string commandText, params object[] args);

        public abstract IEnumerable<dynamic> Query(string commandText, params object[] parameters);

        public abstract dynamic QueryValue(string commandText, params object[] parameters);

        public abstract int Execute(string commandText, params object[] args);

        public void Dispose()
        {
            // Do nothing.
        }
    }
}
