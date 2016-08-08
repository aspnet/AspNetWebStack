// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Microsoft.Design", "CA2210:AssembliesShouldHaveValidStrongNames",
    Justification = "The assembly is delay signed")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace",
    Target = "System.Net.Http",
    Justification = "Functionality logically belongs in a namespace defined in a different binary")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace",
    Target = "System.Net.Http.Formatting", Justification = "Namespace follows folder structure")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace",
    Target = "System.Web.Http.Batch", Justification = "Namespace follows folder structure")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace",
    Target = "System.Web.Http.Dependencies", Justification = "Namespace follows folder structure")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace",
    Target = "System.Web.Http.Hosting", Justification = "Removing this namespace now would be a breaking change.")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace",
    Target = "System.Web.Http.Metadata")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace",
    Target = "System.Web.Http.Services")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace",
    Target = "System.Web.Http.Validation.Validators", Justification = "Namespace follows folder structure")]
[assembly: SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Scope = "member",
    Target = "System.Web.Http.Filters.ActionFilterAttribute.#System.Web.Http.Filters.IActionFilter.ExecuteActionFilterAsync(System.Web.Http.Controllers.HttpActionContext,System.Threading.CancellationToken,System.Func`1<System.Threading.Tasks.Task`1<System.Net.Http.HttpResponseMessage>>)")]
[assembly: SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Scope = "member",
    Target = "System.Web.Http.Filters.AuthorizationFilterAttribute.#System.Web.Http.Filters.IAuthorizationFilter.ExecuteAuthorizationFilterAsync(System.Web.Http.Controllers.HttpActionContext,System.Threading.CancellationToken,System.Func`1<System.Threading.Tasks.Task`1<System.Net.Http.HttpResponseMessage>>)")]
[assembly: SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Scope = "member",
    Target = "System.Web.Http.Filters.ExceptionFilterAttribute.#System.Web.Http.Filters.IExceptionFilter.ExecuteExceptionFilterAsync(System.Web.Http.Filters.HttpActionExecutedContext,System.Threading.CancellationToken)")]
