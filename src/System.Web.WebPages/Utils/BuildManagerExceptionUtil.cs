// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Globalization;
using System.IO;
using System.Web.WebPages.Resources;
using Microsoft.Web.Infrastructure;

namespace System.Web.WebPages
{
    internal static class BuildManagerExceptionUtil
    {
        // Checks the exception to see if it is from CompilationUtil.GetBuildProviderTypeFromExtension, which will throw
        // an exception about an unsupported extension. 
        // Actual error format: There is no build provider registered for the extension '.txt'. You can register one in the <compilation><buildProviders> section in machine.config or web.config. Make sure is has a BuildProviderAppliesToAttribute attribute which includes the value 'Web' or 'All'. 
        internal static bool IsUnsupportedExtensionError(HttpException e)
        {
            Exception exception = e;

            // Go through the layers of exceptions to find if any of them is from GetBuildProviderTypeFromExtension
            while (exception != null)
            {
                var site = exception.TargetSite;
                if (site != null && site.Name == "GetBuildProviderTypeFromExtension" && site.DeclaringType != null && site.DeclaringType.Name == "CompilationUtil")
                {
                    return true;
                }
                exception = exception.InnerException;
            }
            return false;
        }

        internal static void ThrowIfUnsupportedExtension(string virtualPath, HttpException e)
        {
            if (IsUnsupportedExtensionError(e))
            {
                var extension = Path.GetExtension(virtualPath);
                throw new HttpException(String.Format(CultureInfo.CurrentCulture, WebPageResources.WebPage_FileNotSupported, extension, virtualPath));
            }
        }

        internal static void ThrowIfCodeDomDefinedExtension(string virtualPath, HttpException e)
        {
            if (e is HttpCompileException)
            {
                var extension = Path.GetExtension(virtualPath);
                if (InfrastructureHelper.IsCodeDomDefinedExtension(extension))
                {
                    throw new HttpException(String.Format(CultureInfo.CurrentCulture, WebPageResources.WebPage_FileNotSupported, extension, virtualPath));
                }
            }
        }
    }
}
