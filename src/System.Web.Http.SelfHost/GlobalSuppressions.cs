// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Microsoft.Design", "CA2210:AssembliesShouldHaveValidStrongNames", Justification = "These assemblies are delay-signed.")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Justification = "Classes are grouped logically for user clarity.", Scope = "Namespace", Target = "System.Net.Http")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Justification = "Classes are grouped logically for user clarity.", Scope = "Namespace", Target = "System.Web.Http.SelfHost")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Justification = "Classes are grouped logically for user clarity.", Scope = "Namespace", Target = "System.Web.Http.SelfHost.Activation")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Justification = "Classes are grouped logically for user clarity.", Scope = "Namespace", Target = "System.Web.Http.SelfHost.Channels")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1701:ResourceStringCompoundWordsShouldBeCasedCorrectly", MessageId = "URIs", Scope = "resource", Target = "System.Web.Http.SelfHost.Properties.SRResources.resources", Justification = "FxCop does not seem to allow adding an exception to this term in its dictionary.")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId = "URIs", Scope = "resource", Target = "System.Web.Http.SelfHost.Properties.SRResources.resources", Justification = "FxCop does not seem to allow adding an exception to this term in its dictionary.")]
