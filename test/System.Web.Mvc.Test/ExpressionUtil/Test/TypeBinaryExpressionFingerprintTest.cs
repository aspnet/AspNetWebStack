// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Microsoft.TestCommon;

namespace System.Web.Mvc.ExpressionUtil.Test
{
    public class TypeBinaryExpressionFingerprintTest
    {
        [Fact]
        public void Properties()
        {
            // Arrange
            ExpressionType expectedNodeType = ExpressionType.TypeIs;
            Type expectedType = typeof(bool);
            Type expectedTypeOperand = typeof(object);

            // Act
            TypeBinaryExpressionFingerprint fingerprint = new TypeBinaryExpressionFingerprint(expectedNodeType, expectedType, expectedTypeOperand);

            // Assert
            Assert.Equal(expectedNodeType, fingerprint.NodeType);
            Assert.Equal(expectedType, fingerprint.Type);
            Assert.Equal(expectedTypeOperand, fingerprint.TypeOperand);
        }

        [Fact]
        public void Comparison_Equality()
        {
            // Arrange
            ExpressionType nodeType = ExpressionType.TypeIs;
            Type type = typeof(bool);
            Type typeOperand = typeof(object);

            // Act
            TypeBinaryExpressionFingerprint fingerprint1 = new TypeBinaryExpressionFingerprint(nodeType, type, typeOperand);
            TypeBinaryExpressionFingerprint fingerprint2 = new TypeBinaryExpressionFingerprint(nodeType, type, typeOperand);

            // Assert
            Assert.Equal(fingerprint1, fingerprint2);
            Assert.Equal(fingerprint1.GetHashCode(), fingerprint2.GetHashCode());
        }

        [Fact]
        public void Comparison_Inequality_FingerprintType()
        {
            // Arrange
            ExpressionType nodeType = ExpressionType.TypeIs;
            Type type = typeof(bool);
            Type typeOperand = typeof(object);

            // Act
            TypeBinaryExpressionFingerprint fingerprint1 = new TypeBinaryExpressionFingerprint(nodeType, type, typeOperand);
            DummyExpressionFingerprint fingerprint2 = new DummyExpressionFingerprint(nodeType, type);

            // Assert
            Assert.NotEqual<ExpressionFingerprint>(fingerprint1, fingerprint2);
        }

        [Fact]
        public void Comparison_Inequality_TypeOperand()
        {
            // Arrange
            ExpressionType nodeType = ExpressionType.TypeIs;
            Type type = typeof(bool);
            Type typeOperand = typeof(object);

            // Act
            TypeBinaryExpressionFingerprint fingerprint1 = new TypeBinaryExpressionFingerprint(nodeType, type, typeOperand);
            TypeBinaryExpressionFingerprint fingerprint2 = new TypeBinaryExpressionFingerprint(nodeType, type, typeof(string) /* typeOperand */);

            // Assert
            Assert.NotEqual(fingerprint1, fingerprint2);
        }

        [Fact]
        public void Comparison_Inequality_Type()
        {
            // Arrange
            ExpressionType nodeType = ExpressionType.TypeIs;
            Type type = typeof(bool);
            Type typeOperand = typeof(object);

            // Act
            TypeBinaryExpressionFingerprint fingerprint1 = new TypeBinaryExpressionFingerprint(nodeType, type, typeOperand);
            TypeBinaryExpressionFingerprint fingerprint2 = new TypeBinaryExpressionFingerprint(nodeType, typeof(object), typeOperand);

            // Assert
            Assert.NotEqual(fingerprint1, fingerprint2);
        }
    }
}
