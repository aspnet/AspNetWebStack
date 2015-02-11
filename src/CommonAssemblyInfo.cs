// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

#if !BUILD_GENERATED_VERSION
[assembly: AssemblyCompany("Microsoft Open Technologies, Inc.")]
[assembly: AssemblyCopyright("© Microsoft Open Technologies, Inc. All rights reserved.")]
#endif
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyTrademark("")]
[assembly: ComVisible(false)]
#if !NOT_CLS_COMPLIANT
[assembly: CLSCompliant(true)]
#endif
[assembly: NeutralResourcesLanguage("en-US")]
[assembly: AssemblyMetadata("Serviceable", "True")]

// ===========================================================================
//  DO NOT EDIT OR REMOVE ANYTHING BELOW THIS COMMENT.
//  Version numbers are automatically generated based on regular expressions.
// ===========================================================================

#if ASPNETMVC && ASPNETWEBPAGES
#error Runtime projects cannot define both ASPNETMVC and ASPNETWEBPAGES
#elif ASPNETMVC
#if !BUILD_GENERATED_VERSION
[assembly: AssemblyVersion("5.2.4.0")] // ASPNETMVC
[assembly: AssemblyFileVersion("5.2.4.0")] // ASPNETMVC
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
#error Runtime projects must define either ASPNETMVC or ASPNETWEBPAGES
#endif
