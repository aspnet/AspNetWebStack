// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Web.Http.Dispatcher;
using System.Web.Http.Services;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Tracing.Tracers
{
    public class DefaultHttpControllerTypeResolverTracerTest
    {
        private static readonly string ExceptionMessage = "Remember my name";
        private readonly List<Type> ExpectedTypes;
        private readonly List<TraceRecord> ExpectedTraces;

        private readonly Mock<MockableAssembly>[] Assemblies;
        private readonly Mock<IAssembliesResolver> AssembliesResolver;
        private readonly Mock<DefaultHttpControllerTypeResolver> HttpControllerTypeResolver;

        public DefaultHttpControllerTypeResolverTracerTest()
        {
            ExpectedTypes = new List<Type>
            {
                typeof(ValidController), typeof(UsersRpcController), typeof(UsersController),
            };

            Type[] fine1Types = new Type[]
            {
                typeof(DefaultHttpControllerTypeResolverTracerTest),
                null,
                typeof(ValidController),
            };
            Type[] poorTypes = new Type[]
            {
                typeof(string),
                null,
                typeof(Action),
                null,
                typeof(UsersRpcController),
            };
            Type[] fine2Types = new Type[]
            {
                typeof(DefaultHttpControllerTypeResolver),
                null,
                typeof(DefaultHttpControllerTypeResolverTracer),
                null,
                typeof(UsersController)
            };

            Exception worseException = new Exception(ExceptionMessage);
            Exception poorException = new ReflectionTypeLoadException(poorTypes, new Exception[] { worseException });
            ExpectedTraces = new List<TraceRecord>
            {
                new TraceRecord(null, TraceCategories.ControllersCategory, TraceLevel.Debug)
                {
                    Kind = TraceKind.Begin,
                    Operator = "DefaultHttpControllerTypeResolverProxy", // Moq type name
                    Operation = "GetControllerTypes",
                },
                new TraceRecord(null, TraceCategories.ControllersCategory, TraceLevel.Warn)
                {
                    Kind = TraceKind.Trace,
                    Exception = poorException,
                    Message = "Exception thrown while getting types from 'PoorAssembly'.",
                },
                new TraceRecord(null, TraceCategories.ControllersCategory, TraceLevel.Warn)
                {
                    Kind = TraceKind.Trace,
                    Exception = worseException,
                    Message = "Exception thrown while getting types from 'WorseAssembly'.",
                },
                new TraceRecord(null, TraceCategories.ControllersCategory, TraceLevel.Debug)
                {
                    Kind = TraceKind.End,
                    Operator = "DefaultHttpControllerTypeResolverProxy",
                    Operation = "GetControllerTypes",
                },
            };

            Mock<MockableAssembly> fine1Assembly = new Mock<MockableAssembly>(MockBehavior.Strict);
            fine1Assembly.Setup(assembly => assembly.GetTypes()).Returns(fine1Types);
            fine1Assembly.SetupGet(assembly => assembly.IsDynamic).Returns(false);

            Exception[] exceptions = new Exception[] { new Exception(ExceptionMessage), };
            Mock<MockableAssembly> poorAssembly = new Mock<MockableAssembly>(MockBehavior.Strict);
            poorAssembly.Setup(assembly => assembly.GetTypes()).Throws(poorException);
            poorAssembly.SetupGet(assembly => assembly.IsDynamic).Returns(false);
            poorAssembly.SetupGet(assembly => assembly.FullName).Returns("PoorAssembly");

            Mock<MockableAssembly> worseAssembly = new Mock<MockableAssembly>(MockBehavior.Strict);
            worseAssembly.Setup(assembly => assembly.GetTypes()).Throws(worseException);
            worseAssembly.SetupGet(assembly => assembly.IsDynamic).Returns(false);
            worseAssembly.SetupGet(assembly => assembly.FullName).Returns("WorseAssembly");

            Mock<MockableAssembly> fine2Assembly = new Mock<MockableAssembly>(MockBehavior.Strict);
            fine2Assembly.Setup(assembly => assembly.GetTypes()).Returns(fine2Types);
            fine2Assembly.SetupGet(assembly => assembly.IsDynamic).Returns(false);

            Mock<MockableAssembly> dynamicAssembly = new Mock<MockableAssembly>(MockBehavior.Strict);
            dynamicAssembly.SetupGet(assembly => assembly.IsDynamic).Returns(true);

            Assemblies = new Mock<MockableAssembly>[]
            {
                fine1Assembly,
                poorAssembly,
                worseAssembly,
                fine2Assembly,
                dynamicAssembly,
            };

            AssembliesResolver = new Mock<IAssembliesResolver>(MockBehavior.Strict);
            AssembliesResolver.Setup(resolver => resolver.GetAssemblies()).Returns(
                Assemblies.Select(assembly => (Assembly)assembly.Object).AsCollection());

            HttpControllerTypeResolver =
                new Mock<DefaultHttpControllerTypeResolver>() { CallBase = true, };
        }

        [Fact]
        public void GetControllerTypes_CallsMocks()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            DefaultHttpControllerTypeResolverTracer tracer =
                new DefaultHttpControllerTypeResolverTracer(HttpControllerTypeResolver.Object, traceWriter);

            // Act
            tracer.GetControllerTypes(AssembliesResolver.Object);

            // Assert (particularly important tracer delegates to original DefaultHttpControllerTypeResolver)
            HttpControllerTypeResolver.Verify(
                controller => controller.GetControllerTypes(AssembliesResolver.Object), Times.Once());

            // Predicate is not called on null entries or internal types in the Type arrays (see TypeIsVisible)
            HttpControllerTypeResolver.VerifyGet(controller => controller.IsControllerTypePredicate, Times.Exactly(7));

            AssembliesResolver.Verify(resolver => resolver.GetAssemblies(), Times.Once());
            foreach (Mock<MockableAssembly> mock in Assemblies)
            {
                mock.VerifyGet(assembly => assembly.IsDynamic, Times.Once());
                mock.Verify(assembly => assembly.GetTypes(), mock.Object.IsDynamic ? Times.Never() : Times.Once());
            }
        }

        [Fact]
        public void GetControllerTypes_ReturnsExpectedTypes()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            DefaultHttpControllerTypeResolverTracer tracer =
                new DefaultHttpControllerTypeResolverTracer(HttpControllerTypeResolver.Object, traceWriter);

            // Act
            ICollection<Type> types = tracer.GetControllerTypes(AssembliesResolver.Object);

            // Assert
            Assert.NotNull(types);
            Assert.NotEmpty(types);
            Assert.Equal(ExpectedTypes, types);
        }

        [Fact]
        public void GetControllerTypes_TracesAsExpected()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            DefaultHttpControllerTypeResolverTracer tracer =
                new DefaultHttpControllerTypeResolverTracer(HttpControllerTypeResolver.Object, traceWriter);

            // Act
            ICollection<Type> types = tracer.GetControllerTypes(AssembliesResolver.Object);

            // Assert
            Assert.NotNull(traceWriter.Traces);
            Assert.Equal(ExpectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public void InnerProperty_ReturnsOriginal()
        {
            // Arrange
            DefaultHttpControllerTypeResolver expectedResolver = HttpControllerTypeResolver.Object;
            TestTraceWriter traceWriter = new TestTraceWriter();
            DefaultHttpControllerTypeResolverTracer tracer =
                new DefaultHttpControllerTypeResolverTracer(HttpControllerTypeResolver.Object, traceWriter);

            // Act
            DefaultHttpControllerTypeResolver resolver = tracer.Inner;

            // Assert
            Assert.NotNull(resolver);
            Assert.Same(expectedResolver, resolver);
        }

        [Fact]
        public void Decorator_GetInner_ReturnsOriginal()
        {
            // Arrange
            DefaultHttpControllerTypeResolver expectedResolver = HttpControllerTypeResolver.Object;
            TestTraceWriter traceWriter = new TestTraceWriter();
            DefaultHttpControllerTypeResolverTracer tracer =
                new DefaultHttpControllerTypeResolverTracer(HttpControllerTypeResolver.Object, traceWriter);

            // Act
            DefaultHttpControllerTypeResolver resolver = Decorator.GetInner(tracer as DefaultHttpControllerTypeResolver);

            // Assert
            Assert.NotNull(resolver);
            Assert.Same(expectedResolver, resolver);
        }

        [Fact]
        public void IsControllerTypePredicateProperty_SameAsInInner()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            DefaultHttpControllerTypeResolverTracer tracer =
                new DefaultHttpControllerTypeResolverTracer(HttpControllerTypeResolver.Object, traceWriter);

            // Act
            Predicate<Type> innerPredicate = HttpControllerTypeResolver.Object.IsControllerTypePredicate;
            Predicate<Type> tracerPredicate = tracer.IsControllerTypePredicate;

            // Assert
            Assert.NotNull(tracerPredicate);
            Assert.Same(innerPredicate, tracerPredicate);
        }

        [Fact]
        public void IsControllerTypePredicateProperty_RoundTrips()
        {
            // Arrange
            Predicate<Type> expectedPredicate = type => type != null;
            DefaultHttpControllerTypeResolver resolver = new DefaultHttpControllerTypeResolver(expectedPredicate);

            TestTraceWriter traceWriter = new TestTraceWriter();
            DefaultHttpControllerTypeResolverTracer tracer =
                new DefaultHttpControllerTypeResolverTracer(resolver, traceWriter);

            // Act
            Predicate<Type> predicate = tracer.IsControllerTypePredicate;

            // Assert
            Assert.NotNull(predicate);
            Assert.Same(expectedPredicate, predicate);
        }

        // Workaround Moq / Castle requirement that ISerializable object must have the ISerializable constructor.
        // Assembly implements ISerializable but lacks the constructor.
        public abstract class MockableAssembly : Assembly
        {
            public MockableAssembly()
            {
            }

            public MockableAssembly(SerializationInfo info, StreamingContext context)
            {
                throw new NotImplementedException();
            }
        }
    }
}
