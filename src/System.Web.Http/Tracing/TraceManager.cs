// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.Services;
using System.Web.Http.Tracing.Tracers;

namespace System.Web.Http.Tracing
{
    internal class TraceManager : ITraceManager
    {
        public void Initialize(HttpConfiguration configuration)
        {
            ITraceWriter traceWriter = configuration.Services.GetTraceWriter();
            if (traceWriter != null)
            {
                // Install tracers only when a custom trace writer has been registered
                CreateAllTracers(configuration, traceWriter);
            }
        }

        private static void CreateAllTracers(HttpConfiguration configuration, ITraceWriter traceWriter)
        {
            CreateActionInvokerTracer(configuration, traceWriter);
            CreateActionSelectorTracer(configuration, traceWriter);
            CreateActionValueBinderTracer(configuration, traceWriter);
            CreateContentNegotiatorTracer(configuration, traceWriter);
            CreateControllerActivatorTracer(configuration, traceWriter);
            CreateControllerSelectorTracer(configuration, traceWriter);
            CreateHttpControllerTypeResolverTracer(configuration, traceWriter);
            CreateMessageHandlerTracers(configuration, traceWriter);
            CreateMediaTypeFormatterTracers(configuration, traceWriter);
        }

        // Get services from the global config. These are normally per-controller services, but we're getting the global fallbacks.
        private static TService GetService<TService>(ServicesContainer services)
        {
            return (TService)services.GetService(typeof(TService));
        }

        private static void CreateActionInvokerTracer(HttpConfiguration configuration, ITraceWriter traceWriter)
        {
            IHttpActionInvoker invoker = GetService<IHttpActionInvoker>(configuration.Services);
            if (invoker != null && !(invoker is HttpActionInvokerTracer))
            {
                HttpActionInvokerTracer tracer = new HttpActionInvokerTracer(invoker, traceWriter);
                configuration.Services.Replace(typeof(IHttpActionInvoker), tracer);
            }
        }

        private static void CreateActionSelectorTracer(HttpConfiguration configuration, ITraceWriter traceWriter)
        {
            IHttpActionSelector selector = GetService<IHttpActionSelector>(configuration.Services);
            if (selector != null && !(selector is HttpActionSelectorTracer))
            {
                HttpActionSelectorTracer tracer = new HttpActionSelectorTracer(selector, traceWriter);
                configuration.Services.Replace(typeof(IHttpActionSelector), tracer);
            }
        }

        private static void CreateActionValueBinderTracer(HttpConfiguration configuration, ITraceWriter traceWriter)
        {
            IActionValueBinder binder = GetService<IActionValueBinder>(configuration.Services);
            if (binder != null && !(binder is ActionValueBinderTracer))
            {
                ActionValueBinderTracer tracer = new ActionValueBinderTracer(binder, traceWriter);
                configuration.Services.Replace(typeof(IActionValueBinder), tracer);
            }
        }

        private static void CreateContentNegotiatorTracer(HttpConfiguration configuration, ITraceWriter traceWriter)
        {
            IContentNegotiator negotiator = configuration.Services.GetContentNegotiator();
            if (negotiator != null && !(negotiator is ContentNegotiatorTracer))
            {
                ContentNegotiatorTracer tracer = new ContentNegotiatorTracer(negotiator, traceWriter);
                configuration.Services.Replace(typeof(IContentNegotiator), tracer);
            }
        }

        private static void CreateControllerActivatorTracer(HttpConfiguration configuration, ITraceWriter traceWriter)
        {
            IHttpControllerActivator activator = GetService<IHttpControllerActivator>(configuration.Services);
            if (activator != null && !(activator is HttpControllerActivatorTracer))
            {
                HttpControllerActivatorTracer tracer = new HttpControllerActivatorTracer(activator, traceWriter);
                configuration.Services.Replace(typeof(IHttpControllerActivator), tracer);
            }
        }

        private static void CreateControllerSelectorTracer(HttpConfiguration configuration, ITraceWriter traceWriter)
        {
            IHttpControllerSelector controllerSelector = configuration.Services.GetHttpControllerSelector();
            if (controllerSelector != null && !(controllerSelector is HttpControllerSelectorTracer))
            {
                HttpControllerSelectorTracer tracer = new HttpControllerSelectorTracer(controllerSelector, traceWriter);
                configuration.Services.Replace(typeof(IHttpControllerSelector), tracer);
            }
        }

        private static void CreateHttpControllerTypeResolverTracer(HttpConfiguration configuration, ITraceWriter traceWriter)
        {
            DefaultHttpControllerTypeResolver resolver =
                configuration.Services.GetHttpControllerTypeResolver() as DefaultHttpControllerTypeResolver;
            if (resolver != null)
            {
                IHttpControllerTypeResolver tracer = new DefaultHttpControllerTypeResolverTracer(resolver, traceWriter);
                configuration.Services.Replace(typeof(IHttpControllerTypeResolver), tracer);
            }
        }

        private static void CreateMediaTypeFormatterTracers(HttpConfiguration configuration, ITraceWriter traceWriter)
        {
            for (int i = 0; i < configuration.Formatters.Count; i++)
            {
                MediaTypeFormatter formatter = configuration.Formatters[i];
                if (!(formatter is IFormatterTracer))
                {
                    configuration.Formatters[i] = MediaTypeFormatterTracer.CreateTracer(
                                                    configuration.Formatters[i],
                                                    traceWriter,
                                                    request: null);
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Will be disposed when pipeline is disposed.")]
        private static void CreateMessageHandlerTracers(HttpConfiguration configuration, ITraceWriter traceWriter)
        {
            int handlerCount = configuration.MessageHandlers.Count;

            // If message handlers have already been wired into the pipeline,
            // we do not install tracing message handlers. This scenario occurs
            // when initialization is attempted twice, such as per-controller configuration. 
            if (handlerCount > 0 && configuration.MessageHandlers[0].InnerHandler != null)
            {
                return;
            }

            // RequestMessageHandlerTracer will be the first tracer to get executed and each messageHandlerTracer
            // will execute before its respective handler. For the MessageHandlerTracers to be registered, 
            // the message handler list must be of the form:
            // requestMessageHandlerTracer
            // messageHandler1Tracer
            // messageHandler1
            // ...
            // ...
            // messageHandlerNTracer
            // messageHandlerN
            // Where "N" is a non-negative integer. That is, there could be zero or more pairs of handlers/tracers, plus a 
            // request tracer at the beginning. If the state does not match this pattern, the tracer list is recreated.
            if (!AreMessageHandlerTracersRegistered(configuration.MessageHandlers))
            {
                // Removing the MessageHandlerTracer and RequestMessageHandlerTracer in the reverse order.
                for (int i = handlerCount - 1; i >= 0; i--)
                {
                    if (configuration.MessageHandlers[i] is RequestMessageHandlerTracer || configuration.MessageHandlers[i] is MessageHandlerTracer)
                    {
                        configuration.MessageHandlers.RemoveAt(i);
                    }
                }
                handlerCount = configuration.MessageHandlers.Count;

                // Insert a tracing handler before each existing message handler (in execution order)
                for (int i = 0; i < handlerCount * 2; i += 2)
                {
                    DelegatingHandler innerHandler = configuration.MessageHandlers[i];
                    DelegatingHandler handlerTracer = new MessageHandlerTracer(innerHandler, traceWriter);
                    configuration.MessageHandlers.Insert(i, handlerTracer);
                }

                configuration.MessageHandlers.Insert(0, new RequestMessageHandlerTracer(traceWriter));
            }
        }

        private static bool AreMessageHandlerTracersRegistered(Collection<DelegatingHandler> messageHandlers)
        {
            int handlerCount = messageHandlers.Count;
            
            // if the handler count is zero, exit early.
            if (handlerCount == 0)
            {
                return false;
            }

            // if RequestMessageHandlerTracer is absent, exit early.
            if (!(messageHandlers[0] is RequestMessageHandlerTracer))
            {
                return false;
            }

            // Message handler list must be an odd number (2*N+1) for N message handlers.
            if (handlerCount % 2 != 1)
            {
                return false;
            }

            // Check if all odd positions have tracers and even positions have their corresponding handlers.
            for (int i = 2; i < handlerCount; i += 2)
            {
                DelegatingHandler tracer = messageHandlers[i - 1];
                DelegatingHandler messageHandler = messageHandlers[i];
                if (!(tracer is MessageHandlerTracer))
                {
                    return false;
                }

                DelegatingHandler innerHandler = Decorator.GetInner(tracer);
                if (innerHandler != messageHandler)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
