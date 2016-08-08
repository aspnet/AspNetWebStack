// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace WebMatrix.Data.Test
{
    public class FileHandlerTest
    {
        [Fact]
        public void SqlCeFileHandlerReturnsDataDirectoryRelativeConnectionStringIfPathIsNotRooted()
        {
            // Act
            string connectionString = SqlCeDbFileHandler.GetConnectionString("foo.sdf");

            // Assert
            Assert.NotNull(connectionString);
            Assert.Equal(@"Data Source=|DataDirectory|\foo.sdf;File Access Retry Timeout=10", connectionString);
        }

        [Fact]
        public void SqlCeFileHandlerReturnsFullPathConnectionStringIfPathIsNotRooted()
        {
            // Act
            string connectionString = SqlCeDbFileHandler.GetConnectionString(@"c:\foo.sdf");

            // Assert
            Assert.NotNull(connectionString);
            Assert.Equal(@"Data Source=c:\foo.sdf;File Access Retry Timeout=10", connectionString);
        }

        [Fact]
        public void SqlServerFileHandlerReturnsDataDirectoryRelativeConnectionStringIfPathIsNotRooted()
        {
            // Act           
            string connectionString = SqlServerDbFileHandler.GetConnectionString("foo.mdf", "datadir");

            // Assert
            Assert.NotNull(connectionString);
            Assert.Equal(@"Data Source=.\SQLEXPRESS;AttachDbFilename=|DataDirectory|\foo.mdf;Initial Catalog=datadir\foo.mdf;Integrated Security=True;User Instance=True;MultipleActiveResultSets=True",
                         connectionString);
        }

        [Fact]
        public void SqlServerFileHandlerReturnsFullPathConnectionStringIfPathIsNotRooted()
        {
            // Act
            string connectionString = SqlServerDbFileHandler.GetConnectionString(@"c:\foo.mdf", "datadir");

            // Assert
            Assert.NotNull(connectionString);
            Assert.Equal(@"Data Source=.\SQLEXPRESS;AttachDbFilename=c:\foo.mdf;Initial Catalog=c:\foo.mdf;Integrated Security=True;User Instance=True;MultipleActiveResultSets=True", connectionString);
        }
    }
}
