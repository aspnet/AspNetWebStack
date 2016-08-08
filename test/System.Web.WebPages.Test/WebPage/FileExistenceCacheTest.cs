// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading;
using System.Web.Hosting;
using System.Web.WebPages.TestUtils;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.WebPages.Test
{
    public class FileExistenceCacheTest
    {
        [Fact]
        public void ConstructorTest()
        {
            var ms = 1000;
            FileExistenceCache cache;

            var vpp = new Mock<VirtualPathProvider>().Object;
            cache = new FileExistenceCache(vpp);
            Assert.Equal(vpp, cache.VirtualPathProvider);
            Assert.Equal(ms, cache.MilliSecondsBeforeReset);

            ms = 9999;
            cache = new FileExistenceCache(vpp, ms);
            Assert.Equal(vpp, cache.VirtualPathProvider);
            Assert.Equal(ms, cache.MilliSecondsBeforeReset);
        }

        // Not a valid production scenario -- no HostingEnvironment
        [Fact]
        public void ConstructorTestWithNull()
        {
            // Arrange & Act
            FileExistenceCache cache = new FileExistenceCache(() => null);

            // Assert
            Assert.Null(cache.VirtualPathProvider);
        }

        [Fact]
        public void ConstructorTest_VPPRegistrationChanging()
        {
            // Arrange
            Mock<VirtualPathProvider> mockProvider = new Mock<VirtualPathProvider>(MockBehavior.Strict);
            VirtualPathProvider provider = null;

            // Act
            FileExistenceCache cache = new FileExistenceCache(() => provider);

            // The moral equivalent of HostingEnvironment.RegisterVirtualPathProvider(mockProvider.Object)
            provider = mockProvider.Object;

            // Assert
            Assert.Equal(provider, cache.VirtualPathProvider);
            mockProvider.Verify();
        }

        [Fact]
        public void FileExistsTest_VPPRegistrationChanging()
        {
            // Arrange
            string path = "~/Index.cshtml";
            Mock<VirtualPathProvider> mockProvider = new Mock<VirtualPathProvider>(MockBehavior.Strict);
            mockProvider.Setup(c => c.FileExists(It.IsAny<string>())).Returns<string>(p => p.Equals(path)).Verifiable();
            VirtualPathProvider provider = null;

            // Act
            FileExistenceCache cache = new FileExistenceCache(() => provider);

            // The moral equivalent of HostingEnvironment.RegisterVirtualPathProvider(mockProvider.Object)
            provider = mockProvider.Object;

            bool createExists = cache.FileExists("~/Create.cshtml");
            bool indexExists = cache.FileExists(path);

            // Assert
            Assert.False(createExists);
            Assert.True(indexExists);
            mockProvider.Verify();
            mockProvider.Verify(vpp => vpp.FileExists(It.IsAny<string>()), Times.Exactly(2));
        }

        [Fact]
        public void TimeExceededFalseTest()
        {
            var ms = 100000;
            var cache = new FileExistenceCache(GetVpp(), ms);
            Assert.False(cache.TimeExceeded);
        }

        [Fact]
        public void TimeExceededTrueTest()
        {
            var ms = 5;
            var cache = new FileExistenceCache(GetVpp(), ms);
            Thread.Sleep(300);
            Assert.True(cache.TimeExceeded);
        }

        [Fact]
        public void ResetTest()
        {
            var cache = new FileExistenceCache(GetVpp());
            var cacheInternal = cache.CacheInternal;
            cache.Reset();
            Assert.NotSame(cacheInternal, cache.CacheInternal);
        }

        [Fact]
        public void FileExistsTest()
        {
            var path = "~/index.cshtml";
            var cache = new FileExistenceCache(GetVpp(path));
            Assert.True(cache.FileExists(path));
            Assert.False(cache.FileExists("~/test.cshtml"));
        }

        [Fact]
        public void FileExistsVppLaterTest()
        {
            var path = "~/index.cshtml";
            var cache = new FileExistenceCache(GetVpp(path));
            Assert.True(cache.FileExists(path));
            Assert.False(cache.FileExists("~/test.cshtml"));
        }

        [Fact]
        public void FileExistsTimeExceededTest()
        {
            AppDomainUtils.RunInSeparateAppDomain(() =>
            {
                var path = "~/index.cshtml";
                Utils.SetupVirtualPathInAppDomain(path, "");

                var cache = new FileExistenceCache(GetVpp(path));
                var cacheInternal = cache.CacheInternal;
                cache.MilliSecondsBeforeReset = 5;
                Thread.Sleep(300);
                Assert.True(cache.FileExists(path));
                Assert.False(cache.FileExists("~/test.cshtml"));
                Assert.NotEqual(cacheInternal, cache.CacheInternal);
            });
        }

        private static VirtualPathProvider GetVpp(params string[] files)
        {
            var vpp = new Mock<VirtualPathProvider>();
            vpp.Setup(c => c.FileExists(It.IsAny<string>())).Returns<string>(p => files.Contains(p, StringComparer.OrdinalIgnoreCase));
            return vpp.Object;
        }
    }
}
