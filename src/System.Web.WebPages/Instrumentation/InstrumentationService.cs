// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.IO;

namespace System.Web.WebPages.Instrumentation
{
    public class InstrumentationService
    {
        private static readonly bool _isAvailable = HttpContextAdapter.IsInstrumentationAvailable;
        private bool _localIsAvailable = _isAvailable && PageInstrumentationServiceAdapter.IsEnabled;

        private PageInstrumentationServiceAdapter _instrumentationServiceAdapter;
        private bool _isInstrumentationServiceAdapterInitialized;

        public InstrumentationService()
        {
            ExtractInstrumentationService = GetInstrumentationServiceUncached;
            CreateContext = CreateSystemWebContext;
        }

        public bool IsAvailable
        {
            get { return _localIsAvailable; }
            internal set { _localIsAvailable = value; }
        }

        internal Func<HttpContextBase, PageInstrumentationServiceAdapter> ExtractInstrumentationService { get; set; }
        internal Func<string, TextWriter, int, int, bool, PageExecutionContextAdapter> CreateContext { get; set; }

        public void BeginContext(HttpContextBase context, string virtualPath, TextWriter writer, int startPosition, int length, bool isLiteral)
        {
            if (IsAvailable)
            {
                PageInstrumentationServiceAdapter instrumentationService = GetInstrumentationService(context);
                if (instrumentationService != null && instrumentationService.ExecutionListeners.Count > 0)
                {
                    var instrumentationContext = CreateContext(virtualPath, writer, startPosition, length, isLiteral);
                    foreach (PageExecutionListenerAdapter listener in instrumentationService.ExecutionListeners)
                    {
                        listener.BeginContext(instrumentationContext);
                    }
                }
            }
        }

        public void EndContext(HttpContextBase context, string virtualPath, TextWriter writer, int startPosition, int length, bool isLiteral)
        {
            if (IsAvailable)
            {
                PageInstrumentationServiceAdapter instrumentationService = GetInstrumentationService(context);
                if (instrumentationService != null && instrumentationService.ExecutionListeners.Count > 0)
                {
                    var instrumentationContext = CreateContext(virtualPath, writer, startPosition, length, isLiteral);
                    foreach (PageExecutionListenerAdapter listener in instrumentationService.ExecutionListeners)
                    {
                        listener.EndContext(instrumentationContext);
                    }
                }
            }
        }

        private static PageExecutionContextAdapter CreateSystemWebContext(string virtualPath, TextWriter writer, int startPosition, int length, bool isLiteral)
        {
            return new PageExecutionContextAdapter()
            {
                VirtualPath = virtualPath,
                TextWriter = writer,
                StartPosition = startPosition,
                Length = length,
                IsLiteral = isLiteral
            };
        }

        private PageInstrumentationServiceAdapter GetInstrumentationService(HttpContextBase context)
        {
            // There seems to be the potential for the adapter to be null.
            if (!_isInstrumentationServiceAdapterInitialized)
            {
                _instrumentationServiceAdapter = ExtractInstrumentationService(context);

                _isInstrumentationServiceAdapterInitialized = true;
            }

            return _instrumentationServiceAdapter;
        }

        private PageInstrumentationServiceAdapter GetInstrumentationServiceUncached(HttpContextBase context)
        {
            HttpContextAdapter ctx = new HttpContextAdapter(context);
            return ctx.PageInstrumentation;
        }
    }
}
