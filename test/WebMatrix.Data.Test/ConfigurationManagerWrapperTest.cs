// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.TestCommon;
using Moq;
using WebMatrix.Data.Test.Mocks;

namespace WebMatrix.Data.Test
{
    public class ConfigurationManagerWrapperTest
    {
        [Fact]
        public void GetConnectionGetsConnectionFromConfig()
        {
            // Arrange            
            var configManager = new ConfigurationManagerWrapper(new Dictionary<string, IDbFileHandler>(), "DataDirectory");
            Func<string, bool> fileExists = path => false;
            Func<string, IConnectionConfiguration> getFromConfig = name => new MockConnectionConfiguration("connection string");

            // Act
            IConnectionConfiguration configuration = configManager.GetConnection("foo", getFromConfig, fileExists);

            // Assert
            Assert.NotNull(configuration);
            Assert.Equal("connection string", configuration.ConnectionString);
        }

        [Fact]
        public void GetConnectionGetsConnectionFromDataDirectoryIfFileWithSupportedExtensionExists()
        {
            // Arrange   
            var mockHandler = new Mock<MockDbFileHandler>();
            mockHandler.Setup(m => m.GetConnectionConfiguration(@"DataDirectory\Bar.foo")).Returns(new MockConnectionConfiguration("some file based connection"));
            var handlers = new Dictionary<string, IDbFileHandler>
            {
                { ".foo", mockHandler.Object }
            };
            var configManager = new ConfigurationManagerWrapper(handlers, "DataDirectory");
            Func<string, bool> fileExists = path => path.Equals(@"DataDirectory\Bar.foo");
            Func<string, IConnectionConfiguration> getFromConfig = name => null;

            // Act
            IConnectionConfiguration configuration = configManager.GetConnection("Bar", getFromConfig, fileExists);

            // Assert
            Assert.NotNull(configuration);
            Assert.Equal("some file based connection", configuration.ConnectionString);
        }

        [Fact]
        public void GetConnectionSdfAndMdfFile_MdfFileWins()
        {
            // Arrange
            var mockSdfHandler = new Mock<MockDbFileHandler>();
            mockSdfHandler.Setup(m => m.GetConnectionConfiguration(@"DataDirectory\Bar.sdf")).Returns(new MockConnectionConfiguration("sdf connection"));
            var mockMdfHandler = new Mock<MockDbFileHandler>();
            mockMdfHandler.Setup(m => m.GetConnectionConfiguration(@"DataDirectory\Bar.mdf")).Returns(new MockConnectionConfiguration("mdf connection"));
            var handlers = new Dictionary<string, IDbFileHandler>
            {
                { ".sdf", mockSdfHandler.Object },
                { ".mdf", mockMdfHandler.Object },
            };
            var configManager = new ConfigurationManagerWrapper(handlers, "DataDirectory");
            Func<string, bool> fileExists = path => path.Equals(@"DataDirectory\Bar.mdf") ||
                                                    path.Equals(@"DataDirectory\Bar.sdf");
            Func<string, IConnectionConfiguration> getFromConfig = name => null;

            // Act
            IConnectionConfiguration configuration = configManager.GetConnection("Bar", getFromConfig, fileExists);

            // Assert
            Assert.NotNull(configuration);
            Assert.Equal("mdf connection", configuration.ConnectionString);
        }

        [Fact]
        public void GetConnectionReturnsNullIfNoConnectionFound()
        {
            // Act
            var configManager = new ConfigurationManagerWrapper(new Dictionary<string, IDbFileHandler>(), "DataDirectory");
            Func<string, bool> fileExists = path => false;
            Func<string, IConnectionConfiguration> getFromConfig = name => null;

            // Act
            IConnectionConfiguration configuration = configManager.GetConnection("test", getFromConfig, fileExists);

            // Assert
            Assert.Null(configuration);
        }
    }
}
