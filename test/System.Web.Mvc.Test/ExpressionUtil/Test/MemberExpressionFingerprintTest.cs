// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using System.Reflection;
using Microsoft.TestCommon;

namespace System.Web.Mvc.ExpressionUtil.Test
{
    public class MemberExpressionFingerprintTest
    {
        [Fact]
        public void Properties()
        {
            // Arrange
            ExpressionType expectedNodeType = ExpressionType.MemberAccess;
            Type expectedType = typeof(int);
            MemberInfo expectedMember = typeof(TimeSpan).GetProperty("Seconds");

            // Act
            MemberExpressionFingerprint fingerprint = new MemberExpressionFingerprint(expectedNodeType, expectedType, expectedMember);

            // Assert
            Assert.Equal(expectedNodeType, fingerprint.NodeType);
            Assert.Equal(expectedType, fingerprint.Type);
            Assert.Equal(expectedMember, fingerprint.Member);
        }

        [Fact]
        public void Comparison_Equality()
        {
            // Arrange
            ExpressionType nodeType = ExpressionType.MemberAccess;
            Type type = typeof(int);
            MemberInfo member = typeof(TimeSpan).GetProperty("Seconds");

            // Act
            MemberExpressionFingerprint fingerprint1 = new MemberExpressionFingerprint(nodeType, type, member);
            MemberExpressionFingerprint fingerprint2 = new MemberExpressionFingerprint(nodeType, type, member);

            // Assert
            Assert.Equal(fingerprint1, fingerprint2);
            Assert.Equal(fingerprint1.GetHashCode(), fingerprint2.GetHashCode());
        }

        [Fact]
        public void Comparison_Inequality_FingerprintType()
        {
            // Arrange
            ExpressionType nodeType = ExpressionType.MemberAccess;
            Type type = typeof(int);
            MemberInfo member = typeof(TimeSpan).GetProperty("Seconds");

            // Act
            MemberExpressionFingerprint fingerprint1 = new MemberExpressionFingerprint(nodeType, type, member);
            DummyExpressionFingerprint fingerprint2 = new DummyExpressionFingerprint(nodeType, type);

            // Assert
            Assert.NotEqual<ExpressionFingerprint>(fingerprint1, fingerprint2);
        }

        [Fact]
        public void Comparison_Inequality_Member()
        {
            // Arrange
            ExpressionType nodeType = ExpressionType.MemberAccess;
            Type type = typeof(int);
            MemberInfo member = typeof(TimeSpan).GetProperty("Seconds");

            // Act
            MemberExpressionFingerprint fingerprint1 = new MemberExpressionFingerprint(nodeType, type, member);
            MemberExpressionFingerprint fingerprint2 = new MemberExpressionFingerprint(nodeType, type, null /* member */);

            // Assert
            Assert.NotEqual(fingerprint1, fingerprint2);
        }

        [Fact]
        public void Comparison_Inequality_Type()
        {
            // Arrange
            ExpressionType nodeType = ExpressionType.MemberAccess;
            Type type = typeof(int);
            MemberInfo member = typeof(TimeSpan).GetProperty("Seconds");

            // Act
            MemberExpressionFingerprint fingerprint1 = new MemberExpressionFingerprint(nodeType, type, member);
            MemberExpressionFingerprint fingerprint2 = new MemberExpressionFingerprint(nodeType, typeof(object), member);

            // Assert
            Assert.NotEqual(fingerprint1, fingerprint2);
        }
    }
}
