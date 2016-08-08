// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Web.Http.Dispatcher;
using System.Web.Http.Properties;
using System.Web.Http.Services;

namespace System.Web.Http.Tracing.Tracers
{
    /// <summary>
    /// Internal class to trace use of <see cref="DefaultHttpControllerTypeResolver"/>.
    /// </summary>
    internal class DefaultHttpControllerTypeResolverTracer
        : DefaultHttpControllerTypeResolver, IDecorator<DefaultHttpControllerTypeResolver>
    {
        private readonly DefaultHttpControllerTypeResolver _innerResolver;
        private readonly ITraceWriter _traceWriter;
        private readonly string _innerTypeName;

        public DefaultHttpControllerTypeResolverTracer(DefaultHttpControllerTypeResolver innerResolver,
            ITraceWriter traceWriter)
        {
            Contract.Assert(innerResolver != null);
            Contract.Assert(traceWriter != null);

            _innerResolver = innerResolver;
            _traceWriter = traceWriter;
            _innerTypeName = _innerResolver.GetType().Name;

            // Update inner resolver to call back here. Works around fact GetControllerTypes swallows exceptions and
            // improves tracing.
            _innerResolver.SetGetTypesFunc(GetTypesAndTrace);
        }

        public DefaultHttpControllerTypeResolver Inner
        {
            get { return _innerResolver; }
        }

        protected internal override Predicate<Type> IsControllerTypePredicate
        {
            get { return _innerResolver.IsControllerTypePredicate; }
        }

        // Trace beginning and end of GetControllerTypes() invocations. Error tracing should never occur since base
        // implementation swallows all exceptions.
        public override ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver)
        {
            ICollection<Type> result = null;
            _traceWriter.TraceBeginEnd(request: null,
                category: TraceCategories.ControllersCategory,
                level: TraceLevel.Debug,
                operatorName: _innerTypeName,
                operationName: "GetControllerTypes",
                beginTrace: null,
                execute: () => { result = _innerResolver.GetControllerTypes(assembliesResolver); },
                endTrace: null,
                errorTrace: null);

            return result;
        }

        // Warn about any exceptions encountered in GetTypes() invocations. Do not trace beginning or end since that
        // could be quite noisy.
        private Type[] GetTypesAndTrace(Assembly assembly)
        {
            try
            {
                return DefaultHttpControllerTypeResolver.GetTypes(assembly);
            }
            catch (Exception exception)
            {
                _traceWriter.Warn(request: null,
                    category: TraceCategories.ControllersCategory,
                    exception: exception,
                    messageFormat: SRResources.TraceHttpControllerTypeResolverError,
                    messageArguments: assembly.FullName);

                throw;
            }
        }
    }
}
