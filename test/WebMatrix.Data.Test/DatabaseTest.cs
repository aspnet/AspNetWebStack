// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using Microsoft.TestCommon;
using Moq;
using WebMatrix.Data.Test.Mocks;

namespace WebMatrix.Data.Test
{
    public class DatabaseTest
    {
        [Fact]
        public void OpenWithNullConnectionStringNameThrowsException()
        {
            Assert.ThrowsArgumentNullOrEmptyString(() => Database.Open(null), "name");
        }

        [Fact]
        public void OpenConnectionStringWithNullConnectionStringThrowsException()
        {
            Assert.ThrowsArgumentNullOrEmptyString(() => Database.OpenConnectionString(null), "connectionString");
        }

        [Fact]
        public void OpenConnectionStringWithEmptyConnectionStringThrowsException()
        {
            Assert.ThrowsArgumentNullOrEmptyString(() => Database.OpenConnectionString(String.Empty), "connectionString");
        }

        [Fact]
        public void OpenNamedConnectionUsesConnectionStringFromConfigurationIfExists()
        {
            // Arrange
            MockConfigurationManager mockConfigurationManager = new MockConfigurationManager();
            Mock<DbConnection> mockConnection = new Mock<DbConnection>();
            mockConnection.Setup(m => m.ConnectionString).Returns("connection string");
            Mock<MockDbProviderFactory> mockProviderFactory = new Mock<MockDbProviderFactory>();
            mockProviderFactory.Setup(m => m.CreateConnection("connection string")).Returns(mockConnection.Object);
            mockConfigurationManager.AddConnection("foo", new ConnectionConfiguration(mockProviderFactory.Object, "connection string"));

            // Act            
            Database db = Database.OpenNamedConnection("foo", mockConfigurationManager);

            // Assert
            Assert.Equal("connection string", db.Connection.ConnectionString);
        }

        [Fact]
        public void OpenNamedConnectionThrowsIfNoConnectionFound()
        {
            // Arrange
            IConfigurationManager mockConfigurationManager = new MockConfigurationManager();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => Database.OpenNamedConnection("foo", mockConfigurationManager), "Connection string \"foo\" was not found.");
        }

        [Fact]
        public void GetConnectionConfigurationGetConnectionForFileHandlersIfRegistered()
        {
            // Arrange
            var mockHandler = new Mock<MockDbFileHandler>();
            mockHandler.Setup(m => m.GetConnectionConfiguration("filename.foo")).Returns(new MockConnectionConfiguration("some file based connection"));
            var handlers = new Dictionary<string, IDbFileHandler>
            {
                { ".foo", mockHandler.Object }
            };

            // Act
            IConnectionConfiguration configuration = Database.GetConnectionConfiguration("filename.foo", handlers);

            // Assert
            Assert.NotNull(configuration);
            Assert.Equal("some file based connection", configuration.ConnectionString);
        }

        [Fact]
        public void GetConnectionThrowsIfNoHandlersRegisteredForExtension()
        {
            // Arrange
            var handlers = new Dictionary<string, IDbFileHandler>();

            // Act
            Assert.Throws<InvalidOperationException>(() => Database.GetConnectionConfiguration("filename.foo", handlers), "Unable to determine the provider for the database file \"filename.foo\".");
        }
    }
}
