// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Microsoft.Design", "CA2210:AssembliesShouldHaveValidStrongNames", Justification = "These assemblies are delay-signed.")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Justification = "Classes are grouped logically for user clarity.", Scope = "Namespace", Target = "System.Net.Http")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Justification = "Classes are grouped logically for user clarity.", Scope = "Namespace", Target = "System.Web.Http.WebHost")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Justification = "Classes are here so that they're shared with the main DLL's namespace", Scope = "Namespace", Target = "System.Web.Http")]
[assembly: SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Scope = "member", Target = "System.Web.Http.GlobalConfiguration.#.cctor()")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "System.Web.Http.WebHost.Routing", Justification = "This is the most logical namespace for this type.")]
[assembly: SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Scope = "member", Target = "System.Web.Http.WebHost.HttpControllerHandler.#.cctor()", Justification = "HttpServer is disposed by HttpMessageInvoker.")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId = "Bufferless", Scope = "resource", Target = "System.Web.Http.WebHost.Properties.SRResources.resources", Justification = "The term Bufferless comes from the API")]
