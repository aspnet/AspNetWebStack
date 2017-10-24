// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Web.WebPages.TestUtils;
using Microsoft.Internal.Web.Utils;
using Microsoft.TestCommon;

namespace System.Web.WebPages.Test
{
    public class PathUtilTest
    {
        [Fact]
        public void IsSimpleNameTest()
        {
            Assert.True(PathUtil.IsSimpleName("Test.cshtml"));
            Assert.True(PathUtil.IsSimpleName("Test.Hello.cshtml"));
            Assert.False(PathUtil.IsSimpleName("~/myapp/Test/Hello.cshtml"));
            Assert.False(PathUtil.IsSimpleName("../Test/Hello.cshtml"));
            Assert.False(PathUtil.IsSimpleName("../../Test/Hello.cshtml"));
            Assert.False(PathUtil.IsSimpleName("/Test/Hello.cshtml"));
        }

        [Fact]
        public void GetExtensionForNullPathsReturnsNull()
        {
            // Arrange
            string path = null;

            // Act
            string extension = PathUtil.GetExtension(path);

            // Assert
            Assert.Null(extension);
        }

        [Fact]
        public void GetExtensionForEmptyPathsReturnsEmptyString()
        {
            // Arrange
            string path = String.Empty;

            // Act
            string extension = PathUtil.GetExtension(path);

            // Assert
            Assert.Equal(0, extension.Length);
        }

        [Fact]
        public void GetExtensionReturnsEmptyStringForPathsThatDoNotContainExtension()
        {
            // Arrange
            string[] paths = new[] { "SomePath", "SomePath/", "SomePath/MorePath", "SomePath/MorePath/" };

            // Act
            var extensions = paths.Select(PathUtil.GetExtension);

            // Assert
            Assert.All(extensions, ext => Assert.Empty(ext));
        }

        [Fact]
        public void GetExtensionReturnsEmptyStringForPathsContainingPathInfo()
        {
            // Arrange
            string[] paths = new[] { "SomePath.cshtml/", "SomePath.html/path/info" };

            // Act
            var extensions = paths.Select(PathUtil.GetExtension);

            // Assert
            Assert.All(extensions, ext => Assert.Empty(ext));
        }

        [Fact]
        public void GetExtensionReturnsEmptyStringForPathsTerminatingWithADot()
        {
            // Arrange
            string[] paths = new[] { "SomePath.", "SomeDirectory/SomePath/SomePath.", "SomeDirectory/SomePath.foo." };

            // Act
            var extensions = paths.Select(PathUtil.GetExtension);

            // Assert
            Assert.All(extensions, ext => Assert.Empty(ext));
        }

        [Fact]
        public void GetExtensionReturnsExtensionsForPathsTerminatingInExtension()
        {
            // Arrange
            string path1 = "SomePath.cshtml";
            string path2 = "SomeDir/SomePath.txt";

            // Act
            string ext1 = PathUtil.GetExtension(path1);
            string ext2 = PathUtil.GetExtension(path2);

            // Assert
            Assert.Equal(".cshtml", ext1);
            Assert.Equal(".txt", ext2);
        }

        [Fact]
        public void GetExtensionDoesNotThrowForPathsWithInvalidCharacters()
        {
            // Arrange
            // Repro from test case in Bug 93828
            string path = "Insights/110786998958803%7C2.d24wA6Y3MiT2w8p3OT4yTw__.3600.1289415600-708897727%7CRLN-t1w9bXtKWZ_11osz15Rk_jY";

            // Act
            string extension = PathUtil.GetExtension(path);

            // Assert
            Assert.Equal(".1289415600-708897727%7CRLN-t1w9bXtKWZ_11osz15Rk_jY", extension);
        }

        [Fact]
        public void IsWithinAppRootNestedTest()
        {
            AppDomainUtils.RunInSeparateAppDomain(() =>
            {
                var root = "/subfolder1/website1";
                using (Utils.CreateHttpRuntime(root))
                {
                    Assert.True(PathUtil.IsWithinAppRoot(root, "~/"));
                    Assert.True(PathUtil.IsWithinAppRoot(root, "~/default.cshtml"));
                    Assert.True(PathUtil.IsWithinAppRoot(root, "~/test/default.cshtml"));
                    Assert.True(PathUtil.IsWithinAppRoot(root, "/subfolder1/website1"));
                    Assert.True(PathUtil.IsWithinAppRoot(root, "/subfolder1/website1/"));
                    Assert.True(PathUtil.IsWithinAppRoot(root, "/subfolder1/website1/default.cshtml"));
                    Assert.True(PathUtil.IsWithinAppRoot(root, "/subfolder1/website1/test/default.cshtml"));

                    Assert.False(PathUtil.IsWithinAppRoot(root, "/"));
                    Assert.False(PathUtil.IsWithinAppRoot(root, "/subfolder1"));
                    Assert.False(PathUtil.IsWithinAppRoot(root, "/subfolder1/"));
                    Assert.False(PathUtil.IsWithinAppRoot(root, "/subfolder1/website2"));
                    Assert.False(PathUtil.IsWithinAppRoot(root, "/subfolder2"));
                }
            });
        }

        [Fact]
        public void IsWithinAppRootTest()
        {
            AppDomainUtils.RunInSeparateAppDomain(() =>
            {
                var root = "/website1";
                using (Utils.CreateHttpRuntime(root))
                {
                    Assert.True(PathUtil.IsWithinAppRoot(root, "~/"));
                    Assert.True(PathUtil.IsWithinAppRoot(root, "~/default.cshtml"));
                    Assert.True(PathUtil.IsWithinAppRoot(root, "~/test/default.cshtml"));
                    Assert.True(PathUtil.IsWithinAppRoot(root, "/website1"));
                    Assert.True(PathUtil.IsWithinAppRoot(root, "/website1/"));
                    Assert.True(PathUtil.IsWithinAppRoot(root, "/website1/default.cshtml"));
                    Assert.True(PathUtil.IsWithinAppRoot(root, "/website1/test/default.cshtml"));

                    Assert.False(PathUtil.IsWithinAppRoot(root, "/"));
                    Assert.False(PathUtil.IsWithinAppRoot(root, "/website2"));
                    Assert.False(PathUtil.IsWithinAppRoot(root, "/subfolder1/"));
                }
            });
        }

        private class TestVirtualPathUtility : IVirtualPathUtility
        {
            public string Combine(string basePath, string relativePath)
            {
                return basePath + "/" + relativePath;
            }

            public string ToAbsolute(string virtualPath)
            {
                return virtualPath;
            }
        }
    }
}
