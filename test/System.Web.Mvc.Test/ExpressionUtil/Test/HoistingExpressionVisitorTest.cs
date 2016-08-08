// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.TestCommon;

namespace System.Web.Mvc.ExpressionUtil.Test
{
    public class HoistingExpressionVisitorTest
    {
        [Fact]
        public void Hoist()
        {
            // Arrange
            Expression<Func<string, int>> expr = s => (2 * s.Length) + 1;

            // Act
            Expression<Hoisted<string, int>> hoisted = HoistingExpressionVisitor<string, int>.Hoist(expr);

            // Assert
            // new expression should be (s, capturedConstants) => (int)(capturedConstants[0]) * s.Length + (int)(capturedConstants[1])
            // with fingerprint [ LAMBDA:Hoisted<string, int>, OP_ADD:int, OP_MULTIPLY:int, OP_CAST:int, INDEX(List<object>.get_Item):object, PARAM(0):List<object>, CONST:int, MEMBER(String.Length):int, PARAM(1):string, OP_CAST:int, INDEX(List<object>.get_Item):object, PARAM(0):List<object>, CONST:int, PARAM(1):string, PARAM(0):List<object> ]

            List<object> capturedConstants;
            ExpressionFingerprintChain fingerprint = FingerprintingExpressionVisitor.GetFingerprintChain(hoisted, out capturedConstants);

            Assert.Equal(new object[] { 0, 1 }, capturedConstants.ToArray()); // these are constants from the hoisted expression (array indexes), not the original expression
            FingerprintingExpressionVisitorTest.AssertChainEquals(
                fingerprint,
                new LambdaExpressionFingerprint(ExpressionType.Lambda, typeof(Hoisted<string, int>)),
                new BinaryExpressionFingerprint(ExpressionType.Add, typeof(int), null /* method */),
                new BinaryExpressionFingerprint(ExpressionType.Multiply, typeof(int), null /* method */),
                new UnaryExpressionFingerprint(ExpressionType.Convert, typeof(int), null /* method */),
                new IndexExpressionFingerprint(ExpressionType.Index, typeof(object), typeof(List<object>).GetProperty("Item")),
                new ParameterExpressionFingerprint(ExpressionType.Parameter, typeof(List<object>), 0 /* parameterIndex */),
                new ConstantExpressionFingerprint(ExpressionType.Constant, typeof(int)),
                new MemberExpressionFingerprint(ExpressionType.MemberAccess, typeof(int), typeof(string).GetProperty("Length")),
                new ParameterExpressionFingerprint(ExpressionType.Parameter, typeof(string), 1 /* parameterIndex */),
                new UnaryExpressionFingerprint(ExpressionType.Convert, typeof(int), null /* method */),
                new IndexExpressionFingerprint(ExpressionType.Index, typeof(object), typeof(List<object>).GetProperty("Item")),
                new ParameterExpressionFingerprint(ExpressionType.Parameter, typeof(List<object>), 0 /* parameterIndex */),
                new ConstantExpressionFingerprint(ExpressionType.Constant, typeof(int)),
                new ParameterExpressionFingerprint(ExpressionType.Parameter, typeof(string), 1 /* parameterIndex */),
                new ParameterExpressionFingerprint(ExpressionType.Parameter, typeof(List<object>), 0 /* parameterIndex */));
        }
    }
}
