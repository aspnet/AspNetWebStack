// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using System.Reflection;
using Microsoft.TestCommon;

namespace System.Web.Mvc.ExpressionUtil.Test
{
    public class MethodCallExpressionFingerprintTest
    {
        [Fact]
        public void Properties()
        {
            // Arrange
            ExpressionType expectedNodeType = ExpressionType.Call;
            Type expectedType = typeof(string);
            MethodInfo expectedMethod = typeof(string).GetMethod("Intern");

            // Act
            MethodCallExpressionFingerprint fingerprint = new MethodCallExpressionFingerprint(expectedNodeType, expectedType, expectedMethod);

            // Assert
            Assert.Equal(expectedNodeType, fingerprint.NodeType);
            Assert.Equal(expectedType, fingerprint.Type);
            Assert.Equal(expectedMethod, fingerprint.Method);
        }

        [Fact]
        public void Comparison_Equality()
        {
            // Arrange
            ExpressionType nodeType = ExpressionType.Call;
            Type type = typeof(string);
            MethodInfo method = typeof(string).GetMethod("Intern");

            // Act
            MethodCallExpressionFingerprint fingerprint1 = new MethodCallExpressionFingerprint(nodeType, type, method);
            MethodCallExpressionFingerprint fingerprint2 = new MethodCallExpressionFingerprint(nodeType, type, method);

            // Assert
            Assert.Equal(fingerprint1, fingerprint2);
            Assert.Equal(fingerprint1.GetHashCode(), fingerprint2.GetHashCode());
        }

        [Fact]
        public void Comparison_Inequality_FingerprintType()
        {
            // Arrange
            ExpressionType nodeType = ExpressionType.Call;
            Type type = typeof(string);
            MethodInfo method = typeof(string).GetMethod("Intern");

            // Act
            MethodCallExpressionFingerprint fingerprint1 = new MethodCallExpressionFingerprint(nodeType, type, method);
            DummyExpressionFingerprint fingerprint2 = new DummyExpressionFingerprint(nodeType, type);

            // Assert
            Assert.NotEqual<ExpressionFingerprint>(fingerprint1, fingerprint2);
        }

        [Fact]
        public void Comparison_Inequality_Method()
        {
            // Arrange
            ExpressionType nodeType = ExpressionType.Call;
            Type type = typeof(string);
            MethodInfo method = typeof(string).GetMethod("Intern");

            // Act
            MethodCallExpressionFingerprint fingerprint1 = new MethodCallExpressionFingerprint(nodeType, type, method);
            MethodCallExpressionFingerprint fingerprint2 = new MethodCallExpressionFingerprint(nodeType, type, null /* method */);

            // Assert
            Assert.NotEqual(fingerprint1, fingerprint2);
        }

        [Fact]
        public void Comparison_Inequality_Type()
        {
            // Arrange
            ExpressionType nodeType = ExpressionType.Call;
            Type type = typeof(string);
            MethodInfo method = typeof(string).GetMethod("Intern");

            // Act
            MethodCallExpressionFingerprint fingerprint1 = new MethodCallExpressionFingerprint(nodeType, type, method);
            MethodCallExpressionFingerprint fingerprint2 = new MethodCallExpressionFingerprint(nodeType, typeof(object), method);

            // Assert
            Assert.NotEqual(fingerprint1, fingerprint2);
        }
    }
}
