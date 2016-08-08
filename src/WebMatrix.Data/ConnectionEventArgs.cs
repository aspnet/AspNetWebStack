// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;

namespace WebMatrix.Data
{
    public class ConnectionEventArgs : EventArgs
    {
        public ConnectionEventArgs(DbConnection connection)
        {
            Connection = connection;
        }

        public DbConnection Connection { get; private set; }
    }
}
