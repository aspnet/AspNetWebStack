// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using Microsoft.TestCommon;
using Moq;

namespace WebMatrix.Data.Test
{
    public class DynamicRecordTest
    {
        [Fact]
        public void GetFieldValueByNameAccessesUnderlyingRecordForValue()
        {
            // Arrange
            var mockRecord = new Mock<IDataRecord>();
            mockRecord.SetupGet(m => m["A"]).Returns(1);
            mockRecord.SetupGet(m => m["B"]).Returns(2);

            dynamic record = new DynamicRecord(new[] { "A", "B" }, mockRecord.Object);

            // Assert
            Assert.Equal(1, record.A);
            Assert.Equal(2, record.B);
        }

        [Fact]
        public void GetFieldValueByIndexAccessesUnderlyingRecordForValue()
        {
            // Arrange
            var mockRecord = new Mock<IDataRecord>();
            mockRecord.SetupGet(m => m[0]).Returns(1);
            mockRecord.SetupGet(m => m[1]).Returns(2);

            dynamic record = new DynamicRecord(new[] { "A", "B" }, mockRecord.Object);

            // Assert
            Assert.Equal(1, record[0]);
            Assert.Equal(2, record[1]);
        }

        [Fact]
        public void GetFieldValueByNameReturnsNullIfValueIsDbNull()
        {
            // Arrange
            var mockRecord = new Mock<IDataRecord>();
            mockRecord.SetupGet(m => m["A"]).Returns(DBNull.Value);

            dynamic record = new DynamicRecord(new[] { "A" }, mockRecord.Object);

            // Assert
            Assert.Null(record.A);
        }

        [Fact]
        public void GetFieldValueByIndexReturnsNullIfValueIsDbNull()
        {
            // Arrange
            var mockRecord = new Mock<IDataRecord>();
            mockRecord.SetupGet(m => m[0]).Returns(DBNull.Value);

            dynamic record = new DynamicRecord(new[] { "A" }, mockRecord.Object);

            // Assert
            Assert.Null(record[0]);
        }

        [Fact]
        public void GetInvalidFieldValueThrows()
        {
            // Arrange
            var mockRecord = new Mock<IDataRecord>();
            dynamic record = new DynamicRecord(Enumerable.Empty<string>(), mockRecord.Object);

            // Assert
            Assert.Throws<InvalidOperationException>(() => { var value = record.C; }, "Invalid column name \"C\".");
        }

        [Fact]
        public void VerfiyCustomTypeDescriptorMethods()
        {
            // Arrange
            var mockRecord = new Mock<IDataRecord>();
            mockRecord.SetupGet(m => m["A"]).Returns(1);
            mockRecord.SetupGet(m => m["B"]).Returns(2);

            // Act
            ICustomTypeDescriptor record = new DynamicRecord(new[] { "A", "B" }, mockRecord.Object);

            // Assert
            Assert.Equal(AttributeCollection.Empty, record.GetAttributes());
            Assert.Null(record.GetClassName());
            Assert.Null(record.GetConverter());
            Assert.Null(record.GetDefaultEvent());
            Assert.Null(record.GetComponentName());
            Assert.Null(record.GetDefaultProperty());
            Assert.Null(record.GetEditor(null));
            Assert.Equal(EventDescriptorCollection.Empty, record.GetEvents());
            Assert.Equal(EventDescriptorCollection.Empty, record.GetEvents(null));
            Assert.Same(record, record.GetPropertyOwner(null));
            Assert.Equal(2, record.GetProperties().Count);
            Assert.Equal(2, record.GetProperties(null).Count);
            Assert.NotNull(record.GetProperties()["A"]);
            Assert.NotNull(record.GetProperties()["B"]);
        }

        [Fact]
        public void VerifyPropertyDescriptorProperties()
        {
            // Arrange
            var mockRecord = new Mock<IDataRecord>();
            mockRecord.SetupGet(m => m["A"]).Returns(1);
            mockRecord.Setup(m => m.GetOrdinal("A")).Returns(0);
            mockRecord.Setup(m => m.GetFieldType(0)).Returns(typeof(string));

            // Act
            ICustomTypeDescriptor record = new DynamicRecord(new[] { "A" }, mockRecord.Object);

            // Assert
            var aDescriptor = record.GetProperties().Find("A", ignoreCase: false);

            Assert.NotNull(aDescriptor);
            Assert.Null(aDescriptor.GetValue(null));
            Assert.Equal(1, aDescriptor.GetValue(record));
            Assert.True(aDescriptor.IsReadOnly);
            Assert.Equal(typeof(string), aDescriptor.PropertyType);
            Assert.Equal(typeof(DynamicRecord), aDescriptor.ComponentType);
            Assert.False(aDescriptor.ShouldSerializeValue(record));
            Assert.False(aDescriptor.CanResetValue(record));
        }

        [Fact]
        public void SetAndResetValueOnPropertyDescriptorThrows()
        {
            // Arrange
            var mockRecord = new Mock<IDataRecord>();
            mockRecord.SetupGet(m => m["A"]).Returns(1);

            // Act
            ICustomTypeDescriptor record = new DynamicRecord(new[] { "A" }, mockRecord.Object);

            // Assert
            var aDescriptor = record.GetProperties().Find("A", ignoreCase: false);
            Assert.NotNull(aDescriptor);
            Assert.Throws<InvalidOperationException>(() => aDescriptor.SetValue(record, 1), "Unable to modify the value of column \"A\" because the record is read only.");
            Assert.Throws<InvalidOperationException>(() => aDescriptor.ResetValue(record), "Unable to modify the value of column \"A\" because the record is read only.");
        }
    }
}
