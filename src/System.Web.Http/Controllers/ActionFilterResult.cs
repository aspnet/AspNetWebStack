// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;

namespace System.Web.Http.Controllers
{
    internal class ActionFilterResult : IHttpActionResult
    {
        private readonly HttpActionBinding _binding;
        private readonly HttpActionContext _context;
        private readonly ServicesContainer _services;
        private readonly IActionFilter[] _filters;

        public ActionFilterResult(HttpActionBinding binding, HttpActionContext context, ServicesContainer services,
            IActionFilter[] filters)
        {
            Contract.Assert(binding != null);
            Contract.Assert(context != null);
            Contract.Assert(services != null);
            Contract.Assert(filters != null);

            _binding = binding;
            _context = context;
            _services = services;
            _filters = filters;
        }

        public async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            await _binding.ExecuteBindingAsync(_context, cancellationToken);

            ActionInvoker actionInvoker = new ActionInvoker(_context, cancellationToken, _services);
            // Empty filters is the default case so avoid delegates
            // Ensure empty case remains the same as the filtered case
            if (_filters.Length == 0)
            {
                return await actionInvoker.InvokeActionAsync();
            }
            // Ensure delegate continues to use the C# Compiler static delegate caching optimization
            Func<ActionInvoker, Task<HttpResponseMessage>> invokeCallback = (innerInvoker) =>
                innerInvoker.InvokeActionAsync();
            return await InvokeActionWithActionFilters(_context, cancellationToken, _filters, invokeCallback,
                actionInvoker)();
        }

        public static Func<Task<HttpResponseMessage>> InvokeActionWithActionFilters(HttpActionContext actionContext,
            CancellationToken cancellationToken, IActionFilter[] filters, Func<Task<HttpResponseMessage>> innerAction)
        {
            Contract.Assert(actionContext != null);
            Contract.Assert(filters != null);
            Contract.Assert(innerAction != null);

            // Because the continuation gets built from the inside out we need to reverse the filter list so that least
            // specific filters (Global) get run first and the most specific filters (Action) get run last.
            Func<Task<HttpResponseMessage>> result = innerAction;
            for (int i = filters.Length - 1; i >= 0; i--)
            {
                IActionFilter filter = filters[i];
                Func<Func<Task<HttpResponseMessage>>, IActionFilter, Func<Task<HttpResponseMessage>>>
                    chainContinuation = (continuation, innerFilter) =>
                    {
                        return () => innerFilter.ExecuteActionFilterAsync(actionContext, cancellationToken,
                            continuation);
                    };
                result = chainContinuation(result, filter);
            }

            return result;
        }

        private static Func<Task<HttpResponseMessage>> InvokeActionWithActionFilters<T>(
            HttpActionContext actionContext, CancellationToken cancellationToken, IActionFilter[] filters,
            Func<T, Task<HttpResponseMessage>> innerAction, T state)
        {
            return InvokeActionWithActionFilters(actionContext, cancellationToken, filters, () => innerAction(state));
        }

        // Keep as struct to avoid allocation
        private struct ActionInvoker
        {
            private readonly HttpActionContext _context;
            private readonly CancellationToken _cancellationToken;
            private readonly ServicesContainer _controllerServices;

            public ActionInvoker(HttpActionContext context, CancellationToken cancellationToken,
                ServicesContainer controllerServices)
            {
                Contract.Assert(controllerServices != null);

                _context = context;
                _cancellationToken = cancellationToken;
                _controllerServices = controllerServices;
            }

            public Task<HttpResponseMessage> InvokeActionAsync()
            {
                return _controllerServices.GetActionInvoker().InvokeActionAsync(_context, _cancellationToken);
            }
        }
    }
}
