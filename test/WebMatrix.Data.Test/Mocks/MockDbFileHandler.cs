// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace WebMatrix.Data.Test.Mocks
{
    public abstract class MockDbFileHandler : IDbFileHandler
    {
        IConnectionConfiguration IDbFileHandler.GetConnectionConfiguration(string fileName)
        {
            return GetConnectionConfiguration(fileName);
        }

        public abstract MockConnectionConfiguration GetConnectionConfiguration(string fileName);
    }
}
