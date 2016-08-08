// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class DependencyResolverTest
    {
        [Fact]
        public void GuardClauses()
        {
            // Arrange
            var resolver = new DependencyResolver();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => resolver.InnerSetResolver((IDependencyResolver)null),
                "resolver");
            Assert.ThrowsArgumentNull(
                () => resolver.InnerSetResolver((object)null),
                "commonServiceLocator");
            Assert.ThrowsArgumentNull(
                () => resolver.InnerSetResolver(null, type => null),
                "getService");
            Assert.ThrowsArgumentNull(
                () => resolver.InnerSetResolver(type => null, null),
                "getServices");
        }

        [Fact]
        public void DefaultServiceLocatorBehaviorTests()
        {
            // Arrange
            var resolver = new DependencyResolver();

            // Act & Assert
            Assert.NotNull(resolver.InnerCurrent.GetService<object>()); // Concrete type
            Assert.Null(resolver.InnerCurrent.GetService<ModelMetadataProvider>()); // Abstract type
            Assert.Null(resolver.InnerCurrent.GetService<IDisposable>()); // Interface
            Assert.Null(resolver.InnerCurrent.GetService(typeof(List<>))); // Open generic
        }

        [Fact]
        public void DefaultServiceLocatorResolvesNewInstances()
        {
            // Arrange
            var resolver = new DependencyResolver();

            // Act
            object obj1 = resolver.InnerCurrent.GetService<object>();
            object obj2 = resolver.InnerCurrent.GetService<object>();

            // Assert
            Assert.NotSame(obj1, obj2);
        }

        public class MockableResolver
        {
            public virtual object Get(Type type)
            {
                throw new NotImplementedException();
            }

            public virtual IEnumerable<object> GetAll(Type type)
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public void ResolverPassesCallsToDelegateBasedResolver()
        {
            // Arrange
            var resolver = new DependencyResolver();
            var mockResolver = new Mock<MockableResolver>();
            resolver.InnerSetResolver(mockResolver.Object.Get, mockResolver.Object.GetAll);

            // Act & Assert
            resolver.InnerCurrent.GetService(typeof(object));
            mockResolver.Verify(r => r.Get(typeof(object)));

            resolver.InnerCurrent.GetServices(typeof(string));
            mockResolver.Verify(r => r.GetAll(typeof(string)));
        }

        public class MockableCommonServiceLocator
        {
            public virtual object GetInstance(Type type)
            {
                throw new NotImplementedException();
            }

            public virtual IEnumerable<object> GetAllInstances(Type type)
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public void ResolverPassesCallsToICommonServiceLocator()
        {
            // Arrange
            var resolver = new DependencyResolver();
            var mockResolver = new Mock<MockableCommonServiceLocator>();
            resolver.InnerSetResolver(mockResolver.Object);

            // Act & Assert
            resolver.InnerCurrent.GetService(typeof(object));
            mockResolver.Verify(r => r.GetInstance(typeof(object)));

            resolver.InnerCurrent.GetServices(typeof(string));
            mockResolver.Verify(r => r.GetAllInstances(typeof(string)));
        }

        class MissingGetInstance
        {
            public IEnumerable<object> GetAllInstances(Type type)
            {
                return null;
            }
        }

        class MissingGetAllInstances
        {
            public object GetInstance(Type type)
            {
                return null;
            }
        }

        class GetInstanceHasWrongSignature
        {
            public string GetInstance(Type type)
            {
                return null;
            }

            public IEnumerable<object> GetAllInstances(Type type)
            {
                return null;
            }
        }

        class GetAllInstancesHasWrongSignature
        {
            public object GetInstance(Type type)
            {
                return null;
            }

            public IEnumerable<string> GetAllInstances(Type type)
            {
                return null;
            }
        }

        [Fact]
        public void ValidationOfCommonServiceLocatorTests()
        {
            // Arrange
            var resolver = new DependencyResolver();

            // Act & Assert
            Assert.Throws<ArgumentException>(
                () => resolver.InnerSetResolver(new MissingGetInstance()),
                "The type System.Web.Mvc.Test.DependencyResolverTest+MissingGetInstance does not appear to implement Microsoft.Practices.ServiceLocation.IServiceLocator." + Environment.NewLine
              + "Parameter name: commonServiceLocator");
            Assert.Throws<ArgumentException>(
                () => resolver.InnerSetResolver(new MissingGetAllInstances()),
                "The type System.Web.Mvc.Test.DependencyResolverTest+MissingGetAllInstances does not appear to implement Microsoft.Practices.ServiceLocation.IServiceLocator." + Environment.NewLine
              + "Parameter name: commonServiceLocator");
            Assert.Throws<ArgumentException>(
                () => resolver.InnerSetResolver(new GetInstanceHasWrongSignature()),
                "The type System.Web.Mvc.Test.DependencyResolverTest+GetInstanceHasWrongSignature does not appear to implement Microsoft.Practices.ServiceLocation.IServiceLocator." + Environment.NewLine
              + "Parameter name: commonServiceLocator");
            Assert.Throws<ArgumentException>(
                () => resolver.InnerSetResolver(new GetAllInstancesHasWrongSignature()),
                "The type System.Web.Mvc.Test.DependencyResolverTest+GetAllInstancesHasWrongSignature does not appear to implement Microsoft.Practices.ServiceLocation.IServiceLocator." + Environment.NewLine
              + "Parameter name: commonServiceLocator");
        }



        [Fact]
        public void DependencyResolverCache()
        {
            // Verify that when we ask for an interface twice, it only queries the underlying resolver once.
            var resolver = new DependencyResolver();

            Mock<IDependencyResolver> resolverMock = new Mock<IDependencyResolver>();
            resolverMock.Setup(r => r.GetService(typeof(object))).Returns(() => new object());
            resolverMock.Setup(r => r.GetService(typeof(int))).Returns(15);

            resolver.InnerSetResolver(resolverMock.Object);

            object result1 = resolver.InnerCurrentCache.GetService(typeof(object)); // 1st call
            object otherResult = resolver.InnerCurrentCache.GetService(typeof(int)); // 2nd call
            object result2 = resolver.InnerCurrentCache.GetService(typeof(object)); // Cached result from 1st call


            resolverMock.Verify(r => r.GetService(typeof(object)), Times.Once());
            resolverMock.Verify(r => r.GetService(typeof(int)), Times.Once());
            Assert.Same(result1, result2);
            Assert.Equal(15, otherResult);
        }

        [Fact]
        public void ClearDependencyResolverCache()
        {
            // Verify that when we ask for an interface twice, it only queries the underlying resolver once.
            var resolver = new DependencyResolver();

            Mock<IDependencyResolver> resolverMock = new Mock<IDependencyResolver>();
            resolverMock.Setup(r => r.GetService(typeof(object))).Returns(() => new object());
            resolverMock.Setup(r => r.GetService(typeof(int))).Returns(15);

            resolver.InnerSetResolver(resolverMock.Object);

            object result1 = resolver.InnerCurrentCache.GetService(typeof(object)); // 1st call
            object otherResult = resolver.InnerCurrentCache.GetService(typeof(int)); // 2nd call
            resolver.InnerSetResolver(resolverMock.Object); // This will clear the cache
            object result2 = resolver.InnerCurrentCache.GetService(typeof(object)); // 3rd call


            resolverMock.Verify(r => r.GetService(typeof(object)), Times.Exactly(2));
            resolverMock.Verify(r => r.GetService(typeof(int)), Times.Once());
            Assert.NotSame(result1, result2);
            Assert.Equal(15, otherResult);
        }
    }
}
