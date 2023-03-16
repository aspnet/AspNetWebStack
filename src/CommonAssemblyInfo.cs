// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

#if !BUILD_GENERATED_VERSION
[assembly: AssemblyCompany(".NET Foundation")]
[assembly: AssemblyCopyright("Copyright © .NET Foundation. All rights reserved.")]
#endif
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyTrademark("")]
[assembly: ComVisible(false)]
#if !NOT_CLS_COMPLIANT
[assembly: CLSCompliant(true)]
#endif
[assembly: NeutralResourcesLanguage("en-US")]
[assembly: AssemblyMetadata("Serviceable", "True")]

// ===================================================================================
//  TAKE CARE WHEN EDITING OR REMOVING ANYTHING BELOW THIS COMMENT.
//  BUILD_GENERATED_VERSION will be set in any CI build. Versions below are not used.
// ===================================================================================

#if (ASPNETMVC && (ASPNETWEBPAGES || ASPNETFACEBOOK || ASPNETHTTPFORMATTING)) || (ASPNETWEBPAGES && (ASPNETFACEBOOK || ASPNETHTTPFORMATTING)) || (ASPNETFACEBOOK && ASPNETHTTPFORMATTING)
#error Runtime projects cannot define more than one of ASPNETMVC, ASPNETWEBPAGES, ASPNETFACEBOOK, or ASPNETHTTPFORMATTING
#elif ASPNETHTTPFORMATTING
#if !BUILD_GENERATED_VERSION
[assembly: AssemblyVersion("6.0.0.0")] // ASPNETHTTPFORMATTING
[assembly: AssemblyFileVersion("6.0.0.0")] // ASPNETHTTPFORMATTING
#endif
#elif ASPNETMVC
#if !BUILD_GENERATED_VERSION
[assembly: AssemblyVersion("5.3.0.0")] // ASPNETMVC
[assembly: AssemblyFileVersion("5.3.0.0")] // ASPNETMVC
#endif
[assembly: AssemblyProduct("Microsoft ASP.NET MVC")]
#elif ASPNETWEBPAGES
#if !BUILD_GENERATED_VERSION
[assembly: AssemblyVersion("3.0.0.0")] // ASPNETWEBPAGES
[assembly: AssemblyFileVersion("3.0.0.0")] // ASPNETWEBPAGES
#endif
[assembly: AssemblyProduct("Microsoft ASP.NET Web Pages")]
#elif ASPNETFACEBOOK
#if !BUILD_GENERATED_VERSION
[assembly: AssemblyVersion("1.1.0.0")] // ASPNETFACEBOOK
[assembly: AssemblyFileVersion("1.1.0.0")] // ASPNETFACEBOOK
#endif
[assembly: AssemblyProduct("Microsoft ASP.NET Facebook")]
#else
#error Runtime projects must define ASPNETMVC, ASPNETWEBPAGES or ASPNETFACEBOOK
#endif
