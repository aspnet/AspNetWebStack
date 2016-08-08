// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Microsoft.TestCommon;

namespace System.Web.Mvc.ExpressionUtil.Test
{
    public class LambdaExpressionFingerprintTest
    {
        [Fact]
        public void Properties()
        {
            // Arrange
            ExpressionType expectedNodeType = ExpressionType.Lambda;
            Type expectedType = typeof(Action<object>);

            // Act
            LambdaExpressionFingerprint fingerprint = new LambdaExpressionFingerprint(expectedNodeType, expectedType);

            // Assert
            Assert.Equal(expectedNodeType, fingerprint.NodeType);
            Assert.Equal(expectedType, fingerprint.Type);
        }

        [Fact]
        public void Comparison_Equality()
        {
            // Arrange
            ExpressionType nodeType = ExpressionType.Lambda;
            Type type = typeof(Action<object>);

            // Act
            LambdaExpressionFingerprint fingerprint1 = new LambdaExpressionFingerprint(nodeType, type);
            LambdaExpressionFingerprint fingerprint2 = new LambdaExpressionFingerprint(nodeType, type);

            // Assert
            Assert.Equal(fingerprint1, fingerprint2);
            Assert.Equal(fingerprint1.GetHashCode(), fingerprint2.GetHashCode());
        }

        [Fact]
        public void Comparison_Inequality_FingerprintType()
        {
            // Arrange
            ExpressionType nodeType = ExpressionType.Lambda;
            Type type = typeof(Action<object>);

            // Act
            LambdaExpressionFingerprint fingerprint1 = new LambdaExpressionFingerprint(nodeType, type);
            DummyExpressionFingerprint fingerprint2 = new DummyExpressionFingerprint(nodeType, type);

            // Assert
            Assert.NotEqual<ExpressionFingerprint>(fingerprint1, fingerprint2);
        }

        [Fact]
        public void Comparison_Inequality_NodeType()
        {
            // Arrange
            ExpressionType nodeType = ExpressionType.Lambda;
            Type type = typeof(Action<object>);

            // Act
            LambdaExpressionFingerprint fingerprint1 = new LambdaExpressionFingerprint(nodeType, type);
            LambdaExpressionFingerprint fingerprint2 = new LambdaExpressionFingerprint(nodeType, typeof(Action));

            // Assert
            Assert.NotEqual(fingerprint1, fingerprint2);
        }
    }
}
