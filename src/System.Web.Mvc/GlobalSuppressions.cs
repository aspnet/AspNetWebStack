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

[assembly: SuppressMessage("Microsoft.Design", "CA2210:AssembliesShouldHaveValidStrongNames", Justification = "Assembly is delay-signed.")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "System.Web.Mvc.Ajax", Justification = "Helpers reside within a separate namespace to support alternate helper classes.")]
[assembly: SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Scope = "member", Target = "System.Web.Mvc.TempDataDictionary.#System.Collections.Generic.ICollection`1<System.Collections.Generic.KeyValuePair`2<System.String,System.Object>>.Contains(System.Collections.Generic.KeyValuePair`2<System.String,System.Object>)", Justification = "There are no defined scenarios for wanting to derive from this class, but we don't want to prevent it either.")]
[assembly: SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Scope = "member", Target = "System.Web.Mvc.TempDataDictionary.#System.Collections.Generic.ICollection`1<System.Collections.Generic.KeyValuePair`2<System.String,System.Object>>.CopyTo(System.Collections.Generic.KeyValuePair`2<System.String,System.Object>[],System.Int32)", Justification = "There are no defined scenarios for wanting to derive from this class, but we don't want to prevent it either.")]
[assembly: SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Scope = "member", Target = "System.Web.Mvc.TempDataDictionary.#System.Collections.Generic.ICollection`1<System.Collections.Generic.KeyValuePair`2<System.String,System.Object>>.IsReadOnly", Justification = "There are no defined scenarios for wanting to derive from this class, but we don't want to prevent it either.")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId = "Param", Scope = "resource", Target = "System.Web.Mvc.Properties.MvcResources.resources", Justification = "This is the name that matches ASP.NET")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "System.Web.Mvc.Razor", Justification = "This is a grouping of functionally similar components, thus a namespace is a valid way to group them.")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "System.Web.Mvc.Filters", Justification = "Starting to move away from one big namespace. We have to start (small) somewhere.")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "System.Web.Mvc.Routing", Justification = "Starting to move away from one big namespace. We have to start (small) somewhere.")]