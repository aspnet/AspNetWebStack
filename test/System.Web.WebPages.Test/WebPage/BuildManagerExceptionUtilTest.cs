// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Globalization;
using System.Runtime.CompilerServices;
using System.Web.WebPages.Resources;
using Microsoft.TestCommon;

namespace System.Web.WebPages.Test
{
    public class UtilTest
    {
        [Fact]
        public void IsUnsupportedExtensionError()
        {
            Assert.False(BuildManagerExceptionUtil.IsUnsupportedExtensionError(new HttpException("The following file could not be rendered because its extension \".txt\" might not be supported: \"myfile.txt\".")));

            var e = CompilationUtil.GetBuildProviderException(".txt");
            Assert.NotNull(e);
            Assert.True(BuildManagerExceptionUtil.IsUnsupportedExtensionError(e));
        }

        [Fact]
        public void IsUnsupportedExtensionThrowsTest()
        {
            var extension = ".txt";
            var virtualPath = "Layout.txt";
            var e = CompilationUtil.GetBuildProviderException(extension);

            Assert.Throws<HttpException>(
                () => { BuildManagerExceptionUtil.ThrowIfUnsupportedExtension(virtualPath, e); }, String.Format(CultureInfo.CurrentCulture, WebPageResources.WebPage_FileNotSupported, extension, virtualPath));
        }

        [Fact]
        public void CodeDomDefinedExtensionThrowsTest()
        {
            var extension = ".js";
            var virtualPath = "Layout.js";

            Assert.Throws<HttpException>(
                () => { BuildManagerExceptionUtil.ThrowIfCodeDomDefinedExtension(virtualPath, new HttpCompileException()); }, String.Format(CultureInfo.CurrentCulture, WebPageResources.WebPage_FileNotSupported, extension, virtualPath));
        }

        [Fact]
        public void CodeDomDefinedExtensionDoesNotThrowTest()
        {
            var virtualPath = "Layout.txt";
            // Should not throw an exception
            BuildManagerExceptionUtil.ThrowIfCodeDomDefinedExtension(virtualPath, new HttpCompileException());
        }
    }

    // Dummy class to simulate exception from CompilationUtil.GetBuildProviderTypeFromExtension
    internal class CompilationUtil : IVirtualPathFactory
    {
        /// <remarks>
        /// The method that consumes this exception walks the stack trace and uses the class name and method name to uniquely identify an exception.
        /// In release build, the method is inlined causing the call site to appear as the method GetBuildProviderException which causes the test to fail.
        /// These attributes prevent the compiler from inlining this method.
        /// </remarks>
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void GetBuildProviderTypeFromExtension(string extension)
        {
            throw new HttpException(extension);
        }

        public static HttpException GetBuildProviderException(string extension)
        {
            try
            {
                GetBuildProviderTypeFromExtension(extension);
            }
            catch (HttpException e)
            {
                return e;
            }
            return null;
        }

        public bool Exists(string virtualPath)
        {
            string extension = PathUtil.GetExtension(virtualPath);
            GetBuildProviderTypeFromExtension(extension);
            return false;
        }

        public object CreateInstance(string virtualPath)
        {
            throw new NotSupportedException();
        }
    }
}
