// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Services
{
    public class DecoratorTests
    {
        [Fact]
        public void GetInner_Returns_Inner_Object_From_ObjectWrapper()
        {
            // Arrange
            DummyAggregatedClass dummyInnerObject = new DummyAggregatedClass();
            DummyObjectWrapper dummyObjectWrapper = new DummyObjectWrapper(dummyInnerObject);
            IBaseInterface dummyBase = dummyObjectWrapper as IBaseInterface;

            // Act
            DummyAggregatedClass innerObject = Decorator.GetInner(dummyBase) as DummyAggregatedClass;

            // Assert
            Assert.Same(dummyInnerObject, innerObject);
        }

        [Fact]
        public void GetInner_Returns_InnerMost_Object_From_ObjectWrapper()
        {
            // Arrange
            DummyAggregatedClass dummyInnerObject = new DummyAggregatedClass();
            DummyObjectWrapper dummyObjectWrapper = new DummyObjectWrapper(dummyInnerObject);
            DummyDoubleLayerWrapper dummyDoubleLayerWrapper = new DummyDoubleLayerWrapper(dummyObjectWrapper);
            IBaseInterface dummyBase = dummyDoubleLayerWrapper as IBaseInterface;

            // Act
            DummyAggregatedClass innerObject = Decorator.GetInner(dummyBase) as DummyAggregatedClass;

            // Assert
            Assert.Same(dummyInnerObject, innerObject);
        }

        [Fact]
        public void GetInner_Call_On_InnerObject_Returns_InnerObject()
        {
            // Arrange
            DummyAggregatedClass dummyInnerObject = new DummyAggregatedClass();
            IBaseInterface dummyBase = dummyInnerObject as IBaseInterface;

            // Act
            DummyAggregatedClass innerObject = Decorator.GetInner(dummyBase) as DummyAggregatedClass;

            // Assert
            Assert.Same(dummyInnerObject, innerObject);
        }

        [Fact]
        public void GetInner_Call_On_Wrapper_Which_Does_Not_Contain_InnerObject_Returns_Wrapper()
        {
            // Arrange
            DummyWrapper dummyWrapper = new DummyWrapper();
            IBaseInterface dummyBase = dummyWrapper as IBaseInterface;

            // Act
            DummyWrapper innerObject = Decorator.GetInner(dummyBase) as DummyWrapper;

            // Assert
            Assert.Same(dummyWrapper, innerObject);
        }

        [Fact]
        public void GetInner_Returns_Itself_From_AnyClass_Which_Does_Not_Implement_Base_Interface()
        {
            // Arrange
            Mock<object> dummyClass = new Mock<object>();

            // Act
            object innerObject = Decorator.GetInner(dummyClass.Object);

            // Assert
            Assert.Same(dummyClass.Object, innerObject);
        }

        [Fact]
        public void GetInner_Returns_Null_When_Null_Is_Passed()
        {
            // Arrange
            object nullValue = null;

            // Act
            object innerObject = Decorator.GetInner(nullValue);

            // Assert
            Assert.Null(innerObject);
        }

        [Fact]
        public void GetInner_Returns_Inner_Object_From_ObjectWrapper_Which_Wraps_Itself()
        {
            // Arrange
            DummyAggregatedClass dummyInnerObject = new DummyAggregatedClass();
            DummyBaseWrapper dummyBaseWrapper = new DummyBaseWrapper(dummyInnerObject);
            IBaseInterface dummyBase = dummyBaseWrapper as IBaseInterface;

            // Act
            DummyBaseWrapper innerObject = Decorator.GetInner(dummyBase) as DummyBaseWrapper;

            // Assert
            Assert.Same(dummyBaseWrapper, innerObject);
        }

        private interface IBaseInterface { }

        private class DummyAggregatedClass : IBaseInterface { }

        private class DummyWrapper : IBaseInterface { }

        private class DummyObjectWrapper : IBaseInterface, IDecorator<DummyAggregatedClass>
        {
            private readonly DummyAggregatedClass _inner;

            public DummyObjectWrapper(DummyAggregatedClass innerObject)
            {
                _inner = innerObject;
            }

            public DummyAggregatedClass Inner
            {
                get { return _inner; }
            }
        }

        private class DummyDoubleLayerWrapper : IBaseInterface, IDecorator<DummyObjectWrapper>
        {
            private readonly DummyObjectWrapper _inner;

            public DummyDoubleLayerWrapper(DummyObjectWrapper innerObject)
            {
                _inner = innerObject;
            }

            public DummyObjectWrapper Inner
            {
                get { return _inner; }
            }
        }

        private class DummyBaseWrapper : IBaseInterface, IDecorator<IBaseInterface>
        {
            private readonly IBaseInterface _inner;

            public DummyBaseWrapper(IBaseInterface innerObject)
            {
                _inner = innerObject;
            }

            public IBaseInterface Inner
            {
                get { return this; }
            }
        }
    }
}
