// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.TestCommon;

namespace Microsoft.Web.Mvc.Test
{
    public class ModelCopierTest
    {
        [Fact]
        public void CopyCollection_FromIsNull_DoesNothing()
        {
            // Arrange
            int[] from = null;
            List<int> to = new List<int> { 1, 2, 3 };

            // Act
            ModelCopier.CopyCollection(from, to);

            // Assert
            Assert.Equal(new[] { 1, 2, 3 }, to.ToArray());
        }

        [Fact]
        public void CopyCollection_ToIsImmutable_DoesNothing()
        {
            // Arrange
            List<int> from = new List<int> { 1, 2, 3 };
            ICollection<int> to = new ReadOnlyCollection<int>(new[] { 4, 5, 6 });

            // Act
            ModelCopier.CopyCollection(from, to);

            // Assert
            Assert.Equal(new[] { 1, 2, 3 }, from.ToArray());
            Assert.Equal(new[] { 4, 5, 6 }, to.ToArray());
        }

        [Fact]
        public void CopyCollection_ToIsMmutable_ClearsAndCopies()
        {
            // Arrange
            List<int> from = new List<int> { 1, 2, 3 };
            ICollection<int> to = new List<int> { 4, 5, 6 };

            // Act
            ModelCopier.CopyCollection(from, to);

            // Assert
            Assert.Equal(new[] { 1, 2, 3 }, from.ToArray());
            Assert.Equal(new[] { 1, 2, 3 }, to.ToArray());
        }

        [Fact]
        public void CopyCollection_ToIsNull_DoesNothing()
        {
            // Arrange
            List<int> from = new List<int> { 1, 2, 3 };
            List<int> to = null;

            // Act
            ModelCopier.CopyCollection(from, to);

            // Assert
            Assert.Equal(new[] { 1, 2, 3 }, from.ToArray());
        }

        [Fact]
        public void CopyModel_ExactTypeMatch_Copies()
        {
            // Arrange
            GenericModel<int> from = new GenericModel<int> { TheProperty = 21 };
            GenericModel<int> to = new GenericModel<int> { TheProperty = 42 };

            // Act
            ModelCopier.CopyModel(from, to);

            // Assert
            Assert.Equal(21, from.TheProperty);
            Assert.Equal(21, to.TheProperty);
        }

        [Fact]
        public void CopyModel_FromIsNull_DoesNothing()
        {
            // Arrange
            GenericModel<int> from = null;
            GenericModel<int> to = new GenericModel<int> { TheProperty = 42 };

            // Act
            ModelCopier.CopyModel(from, to);

            // Assert
            Assert.Equal(42, to.TheProperty);
        }

        [Fact]
        public void CopyModel_LiftedTypeMatch_ActualValueIsNotNull_Copies()
        {
            // Arrange
            GenericModel<int?> from = new GenericModel<int?> { TheProperty = 21 };
            GenericModel<int> to = new GenericModel<int> { TheProperty = 42 };

            // Act
            ModelCopier.CopyModel(from, to);

            // Assert
            Assert.Equal(21, from.TheProperty);
            Assert.Equal(21, to.TheProperty);
        }

        [Fact]
        public void CopyModel_LiftedTypeMatch_ActualValueIsNull_DoesNothing()
        {
            // Arrange
            GenericModel<int?> from = new GenericModel<int?> { TheProperty = null };
            GenericModel<int> to = new GenericModel<int> { TheProperty = 42 };

            // Act
            ModelCopier.CopyModel(from, to);

            // Assert
            Assert.Null(from.TheProperty);
            Assert.Equal(42, to.TheProperty);
        }

        [Fact]
        public void CopyModel_NoTypeMatch_DoesNothing()
        {
            // Arrange
            GenericModel<int> from = new GenericModel<int> { TheProperty = 21 };
            GenericModel<long> to = new GenericModel<long> { TheProperty = 42 };

            // Act
            ModelCopier.CopyModel(from, to);

            // Assert
            Assert.Equal(21, from.TheProperty);
            Assert.Equal(42, to.TheProperty);
        }

        [Fact]
        public void CopyModel_SubclassedTypeMatch_Copies()
        {
            // Arrange
            string originalModel = "Hello, world!";

            GenericModel<string> from = new GenericModel<string> { TheProperty = originalModel };
            GenericModel<object> to = new GenericModel<object> { TheProperty = 42 };

            // Act
            ModelCopier.CopyModel(from, to);

            // Assert
            Assert.Same(originalModel, from.TheProperty);
            Assert.Same(originalModel, to.TheProperty);
        }

        [Fact]
        public void CopyModel_ToDoesNotContainProperty_DoesNothing()
        {
            // Arrange
            GenericModel<int> from = new GenericModel<int> { TheProperty = 21 };
            OtherGenericModel<int> to = new OtherGenericModel<int> { SomeOtherProperty = 42 };

            // Act
            ModelCopier.CopyModel(from, to);

            // Assert
            Assert.Equal(21, from.TheProperty);
            Assert.Equal(42, to.SomeOtherProperty);
        }

        [Fact]
        public void CopyModel_ToIsNull_DoesNothing()
        {
            // Arrange
            GenericModel<int> from = new GenericModel<int> { TheProperty = 21 };
            GenericModel<int> to = null;

            // Act
            ModelCopier.CopyModel(from, to);

            // Assert
            Assert.Equal(21, from.TheProperty);
        }

        [Fact]
        public void CopyModel_ToIsReadOnly_DoesNothing()
        {
            // Arrange
            GenericModel<int> from = new GenericModel<int> { TheProperty = 21 };
            ReadOnlyGenericModel<int> to = new ReadOnlyGenericModel<int>(42);

            // Act
            ModelCopier.CopyModel(from, to);

            // Assert
            Assert.Equal(21, from.TheProperty);
            Assert.Equal(42, to.TheProperty);
        }

        private class GenericModel<T>
        {
            public T TheProperty { get; set; }
        }

        private class OtherGenericModel<T>
        {
            public T SomeOtherProperty { get; set; }
        }

        private class ReadOnlyGenericModel<T>
        {
            public ReadOnlyGenericModel(T propertyValue)
            {
                TheProperty = propertyValue;
            }

            public T TheProperty { get; private set; }
        }
    }
}
