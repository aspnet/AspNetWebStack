// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.
//
// To add a suppression to this file, right-click the message in the 
// Error List, point to "Suppress Message(s)", and click 
// "In Project Suppression File".
// You do not need to add suppressions to this file manually.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "System.Web.WebPages.Html", Justification = "The namespace contains types specific to Razor. It allows a way for MVC Razor host to identify and remove the namespace")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "System.Web.Mvc", Justification = "This namespace contains TagBuilder and other types forwarded from System.Web.Mvc. The namespace must stay the way it is for type forwarding to work")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "System.Web.WebPages.Instrumentation", Justification = "This namespace contains Instrumentation types and represents an isolated set of functionality.")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId = "accesscontrolservice", Scope = "resource", Target = "System.Web.WebPages.Resources.WebPageResources.resources", Justification = "This is part of a URL.")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId = "identityprovider", Scope = "resource", Target = "System.Web.WebPages.Resources.WebPageResources.resources", Justification = "This is part of a URL.")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId = "nameidentifier", Scope = "resource", Target = "System.Web.WebPages.Resources.WebPageResources.resources", Justification = "This is part of a URL.")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId = "xmlsoap", Scope = "resource", Target = "System.Web.WebPages.Resources.WebPageResources.resources", Justification = "This is part of a URL.")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces", Scope = "type", Target = "System.Web.Helpers.Validation")]
