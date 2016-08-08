// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Web.WebPages.Instrumentation;
using Microsoft.TestCommon;

namespace System.Web.WebPages.Test.Instrumentation
{
    public class InstrumentationServiceTest
    {
        [Fact]
        public void BeginContextDelegatesToRegisteredListeners()
        {
            // Arrange
            dynamic listener1 = CreateListener();
            dynamic listener2 = CreateListener();
            InstrumentationService inst = CreateInstrumentationService(listener1, listener2);
            TextWriter mockWriter = new StringWriter();

            // Act
            inst.BeginContext(null, "Foo.cshtml", mockWriter, 42, 24, isLiteral: false);

            // Assert
            Assert.Equal(1, listener1.BeginContextCalls.Count);
            Assert.Equal(0, listener1.EndContextCalls.Count);
            Assert.Equal(1, listener2.BeginContextCalls.Count);
            Assert.Equal(0, listener2.EndContextCalls.Count);

            AssertContext("Foo.cshtml", mockWriter, 42, 24, false, listener1.BeginContextCalls[0]);
            AssertContext("Foo.cshtml", mockWriter, 42, 24, false, listener2.BeginContextCalls[0]);
        }

        [Fact]
        public void EndContextDelegatesToRegisteredListeners()
        {
            // Arrange
            dynamic listener1 = CreateListener();
            dynamic listener2 = CreateListener();
            InstrumentationService inst = CreateInstrumentationService(listener1, listener2);
            TextWriter mockWriter = new StringWriter();

            // Act
            inst.EndContext(null, "Foo.cshtml", mockWriter, 42, 24, isLiteral: false);

            // Assert
            Assert.Equal(1, listener1.EndContextCalls.Count);
            Assert.Equal(0, listener1.BeginContextCalls.Count);
            Assert.Equal(1, listener2.EndContextCalls.Count);
            Assert.Equal(0, listener2.BeginContextCalls.Count);

            AssertContext("Foo.cshtml", mockWriter, 42, 24, false, listener1.EndContextCalls[0]);
            AssertContext("Foo.cshtml", mockWriter, 42, 24, false, listener2.EndContextCalls[0]);
        }

        private void AssertContext(string virtualPath, TextWriter writer, int startPosition, int length, bool isLiteral, dynamic context)
        {
            PageExecutionContextAdapter ctx = new PageExecutionContextAdapter(context);
            Assert.Equal(virtualPath, ctx.VirtualPath);
            Assert.Same(writer, ctx.TextWriter);
            Assert.Equal(startPosition, ctx.StartPosition);
            Assert.Equal(length, ctx.Length);
            Assert.Equal(isLiteral, ctx.IsLiteral);
        }

        private InstrumentationService CreateInstrumentationService(params dynamic[] listeners)
        {
            dynamic service = new ExpandoObject();
            service.ExecutionListeners = new List<dynamic>(listeners);
            InstrumentationService inst = new InstrumentationService();
            inst.IsAvailable = true;
            inst.ExtractInstrumentationService = _ => new PageInstrumentationServiceAdapter(service);
            inst.CreateContext = CreateExpandoContext;
            return inst;
        }

        private dynamic CreateListener()
        {
            dynamic listener = new ExpandoObject();
            listener.BeginContextCalls = new List<dynamic>();
            listener.EndContextCalls = new List<dynamic>();
            listener.BeginContext = (Action<dynamic>)(d => { listener.BeginContextCalls.Add(d); });
            listener.EndContext = (Action<dynamic>)(d => { listener.EndContextCalls.Add(d); });
            return listener;
        }

        private PageExecutionContextAdapter CreateExpandoContext(string virtualPath, TextWriter writer, int startPosition, int length, bool isLiteral)
        {
            dynamic ctx = new ExpandoObject();
            ctx.VirtualPath = virtualPath;
            ctx.TextWriter = writer;
            ctx.StartPosition = startPosition;
            ctx.Length = length;
            ctx.IsLiteral = isLiteral;
            return new PageExecutionContextAdapter(ctx);
        }
    }
}
