// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using System.Reflection;
using Microsoft.TestCommon;

namespace System.Web.Mvc.ExpressionUtil.Test
{
    public class UnaryExpressionFingerprintTest
    {
        [Fact]
        public void Properties()
        {
            // Arrange
            ExpressionType expectedNodeType = ExpressionType.Not;
            Type expectedType = typeof(int);
            MethodInfo expectedMethod = typeof(object).GetMethod("GetHashCode");

            // Act
            UnaryExpressionFingerprint fingerprint = new UnaryExpressionFingerprint(expectedNodeType, expectedType, expectedMethod);

            // Assert
            Assert.Equal(expectedNodeType, fingerprint.NodeType);
            Assert.Equal(expectedType, fingerprint.Type);
            Assert.Equal(expectedMethod, fingerprint.Method);
        }

        [Fact]
        public void Comparison_Equality()
        {
            // Arrange
            ExpressionType nodeType = ExpressionType.Not;
            Type type = typeof(int);
            MethodInfo method = typeof(object).GetMethod("GetHashCode");

            // Act
            UnaryExpressionFingerprint fingerprint1 = new UnaryExpressionFingerprint(nodeType, type, method);
            UnaryExpressionFingerprint fingerprint2 = new UnaryExpressionFingerprint(nodeType, type, method);

            // Assert
            Assert.Equal(fingerprint1, fingerprint2);
            Assert.Equal(fingerprint1.GetHashCode(), fingerprint2.GetHashCode());
        }

        [Fact]
        public void Comparison_Inequality_FingerprintType()
        {
            // Arrange
            ExpressionType nodeType = ExpressionType.Not;
            Type type = typeof(int);
            MethodInfo method = typeof(object).GetMethod("GetHashCode");

            // Act
            UnaryExpressionFingerprint fingerprint1 = new UnaryExpressionFingerprint(nodeType, type, method);
            DummyExpressionFingerprint fingerprint2 = new DummyExpressionFingerprint(nodeType, type);

            // Assert
            Assert.NotEqual<ExpressionFingerprint>(fingerprint1, fingerprint2);
        }

        [Fact]
        public void Comparison_Inequality_Method()
        {
            // Arrange
            ExpressionType nodeType = ExpressionType.Not;
            Type type = typeof(int);
            MethodInfo method = typeof(object).GetMethod("GetHashCode");

            // Act
            UnaryExpressionFingerprint fingerprint1 = new UnaryExpressionFingerprint(nodeType, type, method);
            UnaryExpressionFingerprint fingerprint2 = new UnaryExpressionFingerprint(nodeType, type, null /* method */);

            // Assert
            Assert.NotEqual(fingerprint1, fingerprint2);
        }

        [Fact]
        public void Comparison_Inequality_Type()
        {
            // Arrange
            ExpressionType nodeType = ExpressionType.Not;
            Type type = typeof(int);
            MethodInfo method = typeof(object).GetMethod("GetHashCode");

            // Act
            UnaryExpressionFingerprint fingerprint1 = new UnaryExpressionFingerprint(nodeType, type, method);
            UnaryExpressionFingerprint fingerprint2 = new UnaryExpressionFingerprint(nodeType, typeof(object), method);

            // Assert
            Assert.NotEqual(fingerprint1, fingerprint2);
        }
    }
}
