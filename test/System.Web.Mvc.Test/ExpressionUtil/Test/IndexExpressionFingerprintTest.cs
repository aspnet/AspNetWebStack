// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using System.Reflection;
using Microsoft.TestCommon;

namespace System.Web.Mvc.ExpressionUtil.Test
{
    public class IndexExpressionFingerprintTest
    {
        [Fact]
        public void Properties()
        {
            // Arrange
            ExpressionType expectedNodeType = ExpressionType.Index;
            Type expectedType = typeof(char);
            PropertyInfo expectedIndexer = typeof(string).GetProperty("Chars");

            // Act
            IndexExpressionFingerprint fingerprint = new IndexExpressionFingerprint(expectedNodeType, expectedType, expectedIndexer);

            // Assert
            Assert.Equal(expectedNodeType, fingerprint.NodeType);
            Assert.Equal(expectedType, fingerprint.Type);
            Assert.Equal(expectedIndexer, fingerprint.Indexer);
        }

        [Fact]
        public void Comparison_Equality()
        {
            // Arrange
            ExpressionType nodeType = ExpressionType.Index;
            Type type = typeof(char);
            PropertyInfo indexer = typeof(string).GetProperty("Chars");

            // Act
            IndexExpressionFingerprint fingerprint1 = new IndexExpressionFingerprint(nodeType, type, indexer);
            IndexExpressionFingerprint fingerprint2 = new IndexExpressionFingerprint(nodeType, type, indexer);

            // Assert
            Assert.Equal(fingerprint1, fingerprint2);
            Assert.Equal(fingerprint1.GetHashCode(), fingerprint2.GetHashCode());
        }

        [Fact]
        public void Comparison_Inequality_FingerprintType()
        {
            // Arrange
            ExpressionType nodeType = ExpressionType.Index;
            Type type = typeof(char);
            PropertyInfo indexer = typeof(string).GetProperty("Chars");

            // Act
            IndexExpressionFingerprint fingerprint1 = new IndexExpressionFingerprint(nodeType, type, indexer);
            DummyExpressionFingerprint fingerprint2 = new DummyExpressionFingerprint(nodeType, type);

            // Assert
            Assert.NotEqual<ExpressionFingerprint>(fingerprint1, fingerprint2);
        }

        [Fact]
        public void Comparison_Inequality_Indexer()
        {
            // Arrange
            ExpressionType nodeType = ExpressionType.Index;
            Type type = typeof(char);
            PropertyInfo indexer = typeof(string).GetProperty("Chars");

            // Act
            IndexExpressionFingerprint fingerprint1 = new IndexExpressionFingerprint(nodeType, type, indexer);
            IndexExpressionFingerprint fingerprint2 = new IndexExpressionFingerprint(nodeType, type, null /* indexer */);

            // Assert
            Assert.NotEqual(fingerprint1, fingerprint2);
        }

        [Fact]
        public void Comparison_Inequality_Type()
        {
            // Arrange
            ExpressionType nodeType = ExpressionType.Index;
            Type type = typeof(char);
            PropertyInfo indexer = typeof(string).GetProperty("Chars");

            // Act
            IndexExpressionFingerprint fingerprint1 = new IndexExpressionFingerprint(nodeType, type, indexer);
            IndexExpressionFingerprint fingerprint2 = new IndexExpressionFingerprint(nodeType, typeof(object), indexer);

            // Assert
            Assert.NotEqual(fingerprint1, fingerprint2);
        }
    }
}
