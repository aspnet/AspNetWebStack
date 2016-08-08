// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;

namespace WebMatrix.Data.Test.Mocks
{
    // Needs to be public for Moq to work
    public abstract class MockDbProviderFactory : IDbProviderFactory
    {
        public abstract DbConnection CreateConnection(string connectionString);
    }
}
