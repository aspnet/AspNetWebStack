// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Microsoft.Design", "CA2210:AssembliesShouldHaveValidStrongNames")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "System.Net.Http.Headers", Justification = "We follow the layout of System.Net.Http.")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "System.Net.Http.Handlers", Justification = "Handlers provide an extensibility hook which we want to keep in a separate namespace.")]
// Some resources are specific to the netstandard1.3 assembly
[assembly: SuppressMessage("Microsoft.Web.FxCop", "MW1000:UnusedResourceUsageRule", Justification = "There are a few unused resources due to missing netstandard1.3 features.")]
