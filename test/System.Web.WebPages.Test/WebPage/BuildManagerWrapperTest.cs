// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Hosting;
using Microsoft.Internal.Web.Utils;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.WebPages.Test
{
    public class BuildManagerWrapperTest
    {
        private const string _precompileConfigFileName = "~/PrecompiledApp.config";

        [Fact]
        public void CanCreateObjectFactoryReturnsFalseIfExtensionIsNotRegistered()
        {
            // Arrange
            var buildManagerWrapper = CreateWrapperInstance();

            // Act
            bool supported = buildManagerWrapper.IsPathExtensionSupported("~/styles/index.css");

            // Assert
            Assert.False(supported);
        }

        [Fact]
        public void CanCreateObjectFactoryReturnsFalseIfVirtualPathIsExtensionless()
        {
            // Arrange
            var buildManagerWrapper = CreateWrapperInstance();

            // Act
            bool supported = buildManagerWrapper.IsPathExtensionSupported("~/default");

            // Assert
            Assert.False(supported);
        }

        [Fact]
        public void CanCreateObjectFactoryReturnsFalseIfVirtualPathExtensionIsEmpty()
        {
            // Arrange
            var buildManagerWrapper = CreateWrapperInstance();

            // Act
            bool supported = buildManagerWrapper.IsPathExtensionSupported("~/default.");

            // Assert
            Assert.False(supported);
        }

        [Fact]
        public void CanCreateObjectFactoryReturnsTrueIfVirtualPathExtensionIsRegistered()
        {
            // Arrange
            var buildManagerWrapper = CreateWrapperInstance();

            // Act
            bool supported = buildManagerWrapper.IsPathExtensionSupported("~/default.cshtml");

            // Assert
            Assert.True(supported);
        }

        [Fact]
        public void CanCreateObjectFactoryPerformsCaseInsenitiveComparison()
        {
            // Arrange
            var buildManagerWrapper = CreateWrapperInstance();

            // Act
            bool supported = buildManagerWrapper.IsPathExtensionSupported("~/default.CShTml");

            // Assert
            Assert.True(supported);
        }

        [Fact]
        public void CanCreateObjectFactoryWorksForAllRegisteredExtensions()
        {
            // Arrange
            var buildManagerWrapper = CreateWrapperInstance();

            // Act
            bool supported = buildManagerWrapper.IsPathExtensionSupported("~/default.vbHtml");

            // Assert
            Assert.True(supported);
        }

        [Fact]
        public void IsNonPrecompiledAppReturnsFalseIfPrecompiledConfigFileDoesNotExist()
        {
            // Arrange
            var vpp = new Mock<VirtualPathProvider>();
            vpp.Setup(c => c.FileExists(It.IsAny<string>())).Returns(false);
            var buildManagerWrapper = new BuildManagerWrapper(vpp.Object, GetVirtualPathUtility());

            // Act
            var isPrecompiled = buildManagerWrapper.IsNonUpdatablePrecompiledApp();

            // Assert
            Assert.False(isPrecompiled);
            vpp.Verify();
        }

        [Fact]
        public void IsNonPrecompiledAppReturnsFalseIfPrecompiledConfigFileIsNotValidXml()
        {
            // Arrange
            var vpp = new Mock<VirtualPathProvider>();
            vpp.Setup(c => c.FileExists(It.Is<string>(p => p.Equals(_precompileConfigFileName)))).Returns(true).Verifiable();
            var file = new Mock<VirtualFile>();
            vpp.Setup(c => c.GetFile(It.Is<string>(p => p.Equals(_precompileConfigFileName)))).Returns(GetFile("some random text that is clearly not xml!"));
            var buildManagerWrapper = new BuildManagerWrapper(vpp.Object, GetVirtualPathUtility());

            // Act
            var isPrecompiled = buildManagerWrapper.IsNonUpdatablePrecompiledApp();

            // Assert
            Assert.False(isPrecompiled);
            vpp.Verify();
        }

        [Fact]
        public void IsNonPrecompiledAppReturnsFalseIfConfigFileDoesNotContainExpectedElements()
        {
            // Arrange
            var fileContent = @"<?xml version=""1.0""?><configuration><appSettings></appSettings></configuration>";
            var vpp = new Mock<VirtualPathProvider>();
            vpp.Setup(c => c.FileExists(It.Is<string>(p => p.Equals(_precompileConfigFileName)))).Returns(true).Verifiable();
            vpp.Setup(c => c.GetFile(It.Is<string>(p => p.Equals(_precompileConfigFileName)))).Returns(GetFile(fileContent)).Verifiable();
            var buildManagerWrapper = new BuildManagerWrapper(vpp.Object, GetVirtualPathUtility());

            // Act
            var isPrecompiled = buildManagerWrapper.IsNonUpdatablePrecompiledApp();

            // Assert
            Assert.False(isPrecompiled);
            vpp.Verify();
        }

        [Fact]
        public void IsNonPrecompiledAppReturnsFalseIfAppIsUpdatable()
        {
            // Arrange
            var fileContent = @"<precompiledApp version=""2"" updatable=""true""/>";
            var vpp = new Mock<VirtualPathProvider>();
            vpp.Setup(c => c.FileExists(It.Is<string>(p => p.Equals(_precompileConfigFileName)))).Returns(true).Verifiable();
            vpp.Setup(c => c.GetFile(It.Is<string>(p => p.Equals(_precompileConfigFileName)))).Returns(GetFile(fileContent)).Verifiable();
            var buildManagerWrapper = new BuildManagerWrapper(vpp.Object, GetVirtualPathUtility());

            // Act
            var isPrecompiled = buildManagerWrapper.IsNonUpdatablePrecompiledApp();

            // Assert
            Assert.False(isPrecompiled);
            vpp.Verify();
        }

        [Fact]
        public void IsNonPrecompiledAppReturnsTrueIfConfigFileIsValidAndIsAppIsNotUpdatable()
        {
            // Arrange
            var fileContent = @"<precompiledApp version=""2"" updatable=""false""/>";
            var vpp = new Mock<VirtualPathProvider>();
            vpp.Setup(c => c.FileExists(It.Is<string>(p => p.Equals(_precompileConfigFileName)))).Returns(true).Verifiable();
            vpp.Setup(c => c.GetFile(It.Is<string>(p => p.Equals(_precompileConfigFileName)))).Returns(GetFile(fileContent)).Verifiable();
            var buildManagerWrapper = new BuildManagerWrapper(vpp.Object, GetVirtualPathUtility());

            // Act
            var isPrecompiled = buildManagerWrapper.IsNonUpdatablePrecompiledApp();

            // Assert
            Assert.True(isPrecompiled);
            vpp.Verify();
        }

        [Fact]
        public void ExistsUsesVppIfSiteIfNotPrecompiled()
        {
            // Arrange
            var virtualPath = "~/default.cshtml";
            var vpp = new Mock<VirtualPathProvider>();
            vpp.Setup(c => c.FileExists(It.Is<string>(p => p.Equals(_precompileConfigFileName)))).Returns(false).Verifiable();
            vpp.Setup(c => c.FileExists(It.Is<string>(p => p.Equals(virtualPath)))).Returns(true).Verifiable();
            var buildManagerWrapper = new BuildManagerWrapper(vpp.Object, GetVirtualPathUtility());

            // Act
            var exists = buildManagerWrapper.Exists(virtualPath);

            // Assert
            vpp.Verify();
            Assert.True(exists);
        }

        [Fact]
        public void ExistsUsesVppIfSiteIfSiteIsPrecompiledButUpdateable()
        {
            // Arrange
            var virtualPath = "~/default.cshtml";
            var fileContent = @"<precompiledApp version=""2"" updatable=""true""/>";
            var vpp = new Mock<VirtualPathProvider>();
            vpp.Setup(c => c.FileExists(It.Is<string>(p => p.Equals(_precompileConfigFileName)))).Returns(true).Verifiable();
            vpp.Setup(c => c.GetFile(It.Is<string>(p => p.Equals(_precompileConfigFileName)))).Returns(GetFile(fileContent)).Verifiable();
            vpp.Setup(c => c.FileExists(It.Is<string>(p => p.Equals(virtualPath)))).Returns(true).Verifiable();
            var buildManagerWrapper = new BuildManagerWrapper(vpp.Object, GetVirtualPathUtility());

            // Act
            var exists = buildManagerWrapper.Exists(virtualPath);

            // Assert
            vpp.Verify();
            Assert.True(exists);
        }

        /// <remarks>
        /// This method adds items to HttpRuntime.Cache. 
        /// </summary>
        [Fact]
        public void ExistsInPrecompiledReturnsFalseIfExtensionIsUnsupported()
        {
            // Arrange
            var virtualPath = "~/ExistsInPrecompiledReturnsFalseIfExtensionIsUnsupported.jpeg";
            var fileContent = @"<precompiledApp version=""2"" updatable=""false""/>";
            var vpp = new Mock<VirtualPathProvider>();
            vpp.Setup(c => c.FileExists(It.Is<string>(p => p.Equals(_precompileConfigFileName)))).Returns(true).Verifiable();
            vpp.Setup(c => c.GetFile(It.Is<string>(p => p.Equals(_precompileConfigFileName)))).Returns(GetFile(fileContent)).Verifiable();
            var buildManagerWrapper = new BuildManagerWrapper(vpp.Object, GetVirtualPathUtility());

            // Act
            var exists = buildManagerWrapper.Exists(virtualPath);

            // Assert
            vpp.Verify();
            Assert.False(exists);
            object cachedValue = HttpRuntime.Cache.Get(BuildManagerWrapper.KeyGuid + "_" + virtualPath);
            Assert.NotNull(cachedValue);
            Assert.False((bool)cachedValue.GetType().GetProperty("Exists").GetValue(cachedValue, null));
        }

        [Fact]
        public void IsNonPrecompiledAppReturnsTrue_VPPRegistrationChanging()
        {
            // Arrange
            string fileContent = @"<precompiledApp version=""2"" updatable=""false""/>";
            IVirtualPathUtility pathUtility = GetVirtualPathUtility();

            Mock<VirtualPathProvider> mockProvider1 = new Mock<VirtualPathProvider>();
            mockProvider1.Setup(c => c.FileExists(It.Is<string>(p => p.Equals(_precompileConfigFileName)))).Returns(true).Verifiable();
            mockProvider1.Setup(c => c.GetFile(It.Is<string>(p => p.Equals(_precompileConfigFileName)))).Returns(GetFile(fileContent)).Verifiable();

            Mock<VirtualPathProvider> mockProvider2 = new Mock<VirtualPathProvider>();
            mockProvider2.Setup(c => c.FileExists(It.Is<string>(p => p.Equals(_precompileConfigFileName)))).Returns(true).Verifiable();
            mockProvider2.Setup(c => c.GetFile(It.Is<string>(p => p.Equals(_precompileConfigFileName)))).Returns(GetFile(fileContent)).Verifiable();

            VirtualPathProvider provider = mockProvider1.Object;

            // Act; uses one VirtualPathProvider in constructor and the other when IsNonUpdatablePrecompiledApp() is called directly
            BuildManagerWrapper buildManagerWrapper = new BuildManagerWrapper(() => provider, pathUtility);

            // The moral equivalent of HostingEnvironment.RegisterVirtualPathProvider(provider2.Object)
            provider = mockProvider2.Object;

            bool isPrecompiled = buildManagerWrapper.IsNonUpdatablePrecompiledApp();

            // Assert
            Assert.True(isPrecompiled);
            mockProvider1.Verify();
            mockProvider1.Verify(vpp => vpp.FileExists(It.IsAny<string>()), Times.Once());
            mockProvider1.Verify(vpp => vpp.GetFile(It.IsAny<string>()), Times.Once());
            mockProvider2.Verify();
            mockProvider2.Verify(vpp => vpp.FileExists(It.IsAny<string>()), Times.Once());
            mockProvider2.Verify(vpp => vpp.GetFile(It.IsAny<string>()), Times.Once());
        }

        private static BuildManagerWrapper CreateWrapperInstance(IEnumerable<string> supportedExtensions = null)
        {
            return new BuildManagerWrapper(new Mock<VirtualPathProvider>().Object, GetVirtualPathUtility()) { SupportedExtensions = supportedExtensions ?? new[] { "cshtml", "vbhtml" } };
        }

        private static VirtualFile GetFile(string content)
        {
            var file = new Mock<VirtualFile>("test file");
            file.Setup(f => f.Open()).Returns(() => new MemoryStream(Encoding.UTF8.GetBytes(content)));
            return file.Object;
        }

        private static IVirtualPathUtility GetVirtualPathUtility()
        {
            var utility = new Mock<VirtualPathUtilityBase>();
            utility.Setup(c => c.ToAbsolute(It.IsAny<string>())).Returns<string>(c => c);

            return utility.Object;
        }
    }

    public abstract class VirtualPathUtilityBase : IVirtualPathUtility
    {
        public virtual string Combine(string basePath, string relativePath)
        {
            throw new NotImplementedException();
        }

        public virtual string ToAbsolute(string virtualPath)
        {
            throw new NotImplementedException();
        }
    }
}
