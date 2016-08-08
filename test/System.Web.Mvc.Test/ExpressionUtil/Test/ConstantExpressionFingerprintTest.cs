// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Microsoft.TestCommon;

namespace System.Web.Mvc.ExpressionUtil.Test
{
    public class ConstantExpressionFingerprintTest
    {
        [Fact]
        public void Properties()
        {
            // Arrange
            ExpressionType expectedNodeType = ExpressionType.Constant;
            Type expectedType = typeof(object);

            // Act
            ConstantExpressionFingerprint fingerprint = new ConstantExpressionFingerprint(expectedNodeType, expectedType);

            // Assert
            Assert.Equal(expectedNodeType, fingerprint.NodeType);
            Assert.Equal(expectedType, fingerprint.Type);
        }

        [Fact]
        public void Comparison_Equality()
        {
            // Arrange
            ExpressionType nodeType = ExpressionType.Constant;
            Type type = typeof(object);

            // Act
            ConstantExpressionFingerprint fingerprint1 = new ConstantExpressionFingerprint(nodeType, type);
            ConstantExpressionFingerprint fingerprint2 = new ConstantExpressionFingerprint(nodeType, type);

            // Assert
            Assert.Equal(fingerprint1, fingerprint2);
            Assert.Equal(fingerprint1.GetHashCode(), fingerprint2.GetHashCode());
        }

        [Fact]
        public void Comparison_Inequality_FingerprintType()
        {
            // Arrange
            ExpressionType nodeType = ExpressionType.Constant;
            Type type = typeof(object);

            // Act
            ConstantExpressionFingerprint fingerprint1 = new ConstantExpressionFingerprint(nodeType, type);
            DummyExpressionFingerprint fingerprint2 = new DummyExpressionFingerprint(nodeType, type);

            // Assert
            Assert.NotEqual<ExpressionFingerprint>(fingerprint1, fingerprint2);
        }

        [Fact]
        public void Comparison_Inequality_Type()
        {
            // Arrange
            ExpressionType nodeType = ExpressionType.Constant;
            Type type = typeof(object);

            // Act
            ConstantExpressionFingerprint fingerprint1 = new ConstantExpressionFingerprint(nodeType, type);
            ConstantExpressionFingerprint fingerprint2 = new ConstantExpressionFingerprint(nodeType, typeof(string));

            // Assert
            Assert.NotEqual(fingerprint1, fingerprint2);
        }
    }
}
