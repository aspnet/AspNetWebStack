// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Microsoft.TestCommon;

namespace System.Web.Mvc.ExpressionUtil.Test
{
    public class ParameterExpressionFingerprintTest
    {
        [Fact]
        public void Properties()
        {
            // Arrange
            ExpressionType expectedNodeType = ExpressionType.Parameter;
            Type expectedType = typeof(object);
            int expectedParameterIndex = 1;

            // Act
            ParameterExpressionFingerprint fingerprint = new ParameterExpressionFingerprint(expectedNodeType, expectedType, expectedParameterIndex);

            // Assert
            Assert.Equal(expectedNodeType, fingerprint.NodeType);
            Assert.Equal(expectedType, fingerprint.Type);
            Assert.Equal(expectedParameterIndex, fingerprint.ParameterIndex);
        }

        [Fact]
        public void Comparison_Equality()
        {
            // Arrange
            ExpressionType nodeType = ExpressionType.Parameter;
            Type type = typeof(object);
            int parameterIndex = 1;

            // Act
            ParameterExpressionFingerprint fingerprint1 = new ParameterExpressionFingerprint(nodeType, type, parameterIndex);
            ParameterExpressionFingerprint fingerprint2 = new ParameterExpressionFingerprint(nodeType, type, parameterIndex);

            // Assert
            Assert.Equal(fingerprint1, fingerprint2);
            Assert.Equal(fingerprint1.GetHashCode(), fingerprint2.GetHashCode());
        }

        [Fact]
        public void Comparison_Inequality_FingerprintType()
        {
            // Arrange
            ExpressionType nodeType = ExpressionType.Parameter;
            Type type = typeof(object);
            int parameterIndex = 1;

            // Act
            ParameterExpressionFingerprint fingerprint1 = new ParameterExpressionFingerprint(nodeType, type, parameterIndex);
            DummyExpressionFingerprint fingerprint2 = new DummyExpressionFingerprint(nodeType, type);

            // Assert
            Assert.NotEqual<ExpressionFingerprint>(fingerprint1, fingerprint2);
        }

        [Fact]
        public void Comparison_Inequality_Method()
        {
            // Arrange
            ExpressionType nodeType = ExpressionType.Parameter;
            Type type = typeof(object);
            int parameterIndex = 1;

            // Act
            ParameterExpressionFingerprint fingerprint1 = new ParameterExpressionFingerprint(nodeType, type, parameterIndex);
            ParameterExpressionFingerprint fingerprint2 = new ParameterExpressionFingerprint(nodeType, type, -1 /* parameterIndex */);

            // Assert
            Assert.NotEqual(fingerprint1, fingerprint2);
        }

        [Fact]
        public void Comparison_Inequality_Type()
        {
            // Arrange
            ExpressionType nodeType = ExpressionType.Parameter;
            Type type = typeof(object);
            int parameterIndex = 1;

            // Act
            ParameterExpressionFingerprint fingerprint1 = new ParameterExpressionFingerprint(nodeType, type, parameterIndex);
            ParameterExpressionFingerprint fingerprint2 = new ParameterExpressionFingerprint(nodeType, typeof(string), parameterIndex);

            // Assert
            Assert.NotEqual(fingerprint1, fingerprint2);
        }
    }
}
