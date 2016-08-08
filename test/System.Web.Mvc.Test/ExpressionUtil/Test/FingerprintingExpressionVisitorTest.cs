// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Microsoft.TestCommon;

namespace System.Web.Mvc.ExpressionUtil.Test
{
    public class FingerprintingExpressionVisitorTest
    {
        private const ExpressionFingerprint _nullFingerprint = null;

        [Fact]
        public void TypeOverridesAllMethods()
        {
            // Ensures that the FingerprintingExpressionVisitor type overrides all VisitXxx methods so that
            // it can properly set the "I gave up" flag when it encounters an Expression it's not familiar
            // with.

            var methodsOnExpressionVisitorRequiringOverride = typeof(ExpressionVisitor).GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).Where(mi => mi.IsVirtual).Select(mi => mi.GetBaseDefinition()).Where(mi => mi.DeclaringType == typeof(ExpressionVisitor));
            var methodsOnFingerprintingExpressionVisitor = typeof(FingerprintingExpressionVisitor).GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).Where(mi => mi.DeclaringType == typeof(FingerprintingExpressionVisitor));

            var missingMethods = methodsOnExpressionVisitorRequiringOverride.Except(methodsOnFingerprintingExpressionVisitor.Select(mi => mi.GetBaseDefinition())).ToArray();
            if (missingMethods.Length != 0)
            {
                StringBuilder sb = new StringBuilder("The following methods are declared on ExpressionVisitor and must be overridden on FingerprintingExpressionVisitor:");
                foreach (MethodInfo method in missingMethods)
                {
                    sb.AppendLine();
                    sb.Append(method);
                }
                Assert.True(false, sb.ToString());
            }
        }

        [Fact]
        public void Visit_Null()
        {
            // Arrange

            // fingerprints as [ NULL ]
            Expression expr = null;

            // Act
            List<object> capturedConstants;
            ExpressionFingerprintChain fingerprint = FingerprintingExpressionVisitor.GetFingerprintChain(expr, out capturedConstants);

            // Assert
            Assert.Empty(capturedConstants);
            AssertChainEquals(fingerprint, _nullFingerprint);
        }

        [Fact]
        public void Visit_Unknown()
        {
            // Arrange

            // if we fingerprinted ctors, would fingerprint as [ NEW(StringBuilder(int)):StringBuilder, PARAM(0):int ]
            // but since we don't fingerprint ctors, should just return null (signaling failure)
            Expression expr = (Expression<Func<int, StringBuilder>>)(capacity => new StringBuilder(capacity));

            // Act
            List<object> capturedConstants;
            ExpressionFingerprintChain fingerprint = FingerprintingExpressionVisitor.GetFingerprintChain(expr, out capturedConstants);

            // Assert
            Assert.Null(fingerprint); // Can't fingerprint ctor
            Assert.Null(capturedConstants); // Can't fingerprint ctor
        }

        [Fact]
        public void VisitBinary()
        {
            // Arrange

            // fingerprints as [ OP_GREATERTHAN:bool, CONST:int, CONST:int ]
            Expression expr = Expression.MakeBinary(ExpressionType.GreaterThan, Expression.Constant(42), Expression.Constant(84));

            // Act
            List<object> capturedConstants;
            ExpressionFingerprintChain fingerprint = FingerprintingExpressionVisitor.GetFingerprintChain(expr, out capturedConstants);

            // Assert
            Assert.Equal(new object[] { 42, 84 }, capturedConstants.ToArray());
            AssertChainEquals(fingerprint,
                              new BinaryExpressionFingerprint(ExpressionType.GreaterThan, typeof(bool), null /* method */),
                              new ConstantExpressionFingerprint(ExpressionType.Constant, typeof(int)),
                              new ConstantExpressionFingerprint(ExpressionType.Constant, typeof(int)));
        }

        [Fact]
        public void VisitConditional()
        {
            // Arrange

            // fingerprints as [ CONDITIONAL:int, CONST:bool, CONST:int, CONST:int ]
            Expression expr = Expression.Condition(Expression.Constant(true), Expression.Constant(42), Expression.Constant(84));

            // Act
            List<object> capturedConstants;
            ExpressionFingerprintChain fingerprint = FingerprintingExpressionVisitor.GetFingerprintChain(expr, out capturedConstants);

            // Assert
            Assert.Equal(new object[] { true, 42, 84 }, capturedConstants.ToArray());
            AssertChainEquals(fingerprint,
                              new ConditionalExpressionFingerprint(ExpressionType.Conditional, typeof(int)),
                              new ConstantExpressionFingerprint(ExpressionType.Constant, typeof(bool)),
                              new ConstantExpressionFingerprint(ExpressionType.Constant, typeof(int)),
                              new ConstantExpressionFingerprint(ExpressionType.Constant, typeof(int)));
        }

        [Fact]
        public void VisitConstant()
        {
            // Arrange

            // fingerprints as [ CONST:int ]
            Expression expr = Expression.Constant(42);

            // Act
            List<object> capturedConstants;
            ExpressionFingerprintChain fingerprint = FingerprintingExpressionVisitor.GetFingerprintChain(expr, out capturedConstants);

            // Assert
            Assert.Equal(new object[] { 42 }, capturedConstants.ToArray());
            AssertChainEquals(fingerprint,
                              new ConstantExpressionFingerprint(ExpressionType.Constant, typeof(int)));
        }

        [Fact]
        public void VisitDefault()
        {
            // Arrange

            // fingerprints as [ DEFAULT:int ]
            Expression expr = Expression.Default(typeof(int));

            // Act
            List<object> capturedConstants;
            ExpressionFingerprintChain fingerprint = FingerprintingExpressionVisitor.GetFingerprintChain(expr, out capturedConstants);

            // Assert
            Assert.Empty(capturedConstants);
            AssertChainEquals(fingerprint, new DefaultExpressionFingerprint(ExpressionType.Default, typeof(int)));
        }

        [Fact]
        public void VisitIndex()
        {
            // Arrange

            // fingerprints as [ INDEX:object, PARAM(0):object[], CONST:int ]
            Expression expr = Expression.MakeIndex(Expression.Parameter(typeof(object[])), null /* indexer */, new Expression[] { Expression.Constant(42) });

            // Act
            List<object> capturedConstants;
            ExpressionFingerprintChain fingerprint = FingerprintingExpressionVisitor.GetFingerprintChain(expr, out capturedConstants);

            // Assert
            Assert.Equal(new object[] { 42 }, capturedConstants.ToArray());
            AssertChainEquals(fingerprint,
                              new IndexExpressionFingerprint(ExpressionType.Index, typeof(object), null /* indexer */),
                              new ParameterExpressionFingerprint(ExpressionType.Parameter, typeof(object[]), 0 /* parameterIndex */),
                              new ConstantExpressionFingerprint(ExpressionType.Constant, typeof(int)));
        }

        [Fact]
        public void VisitLambda()
        {
            // Arrange

            // fingerprints as [ LAMBDA:Func<string, int>, CONST:int, PARAM(0):string ]
            Expression expr = (Expression<Func<string, int>>)(x => 42);

            // Act
            List<object> capturedConstants;
            ExpressionFingerprintChain fingerprint = FingerprintingExpressionVisitor.GetFingerprintChain(expr, out capturedConstants);

            // Assert
            Assert.Equal(new object[] { 42 }, capturedConstants.ToArray());
            AssertChainEquals(fingerprint,
                              new LambdaExpressionFingerprint(ExpressionType.Lambda, typeof(Func<string, int>)),
                              new ConstantExpressionFingerprint(ExpressionType.Constant, typeof(int)),
                              new ParameterExpressionFingerprint(ExpressionType.Parameter, typeof(string), 0 /* parameterIndex */));
        }

        [Fact]
        public void VisitMember()
        {
            // Arrange

            // fingerprints as [ MEMBER(String.Empty):string, NULL ]
            Expression expr = Expression.Field(null, typeof(string), "Empty");

            // Act
            List<object> capturedConstants;
            ExpressionFingerprintChain fingerprint = FingerprintingExpressionVisitor.GetFingerprintChain(expr, out capturedConstants);

            // Assert
            Assert.Empty(capturedConstants);
            AssertChainEquals(fingerprint,
                              new MemberExpressionFingerprint(ExpressionType.MemberAccess, typeof(string), typeof(string).GetField("Empty")),
                              _nullFingerprint);
        }

        [Fact]
        public void VisitMethodCall()
        {
            // Arrange

            // fingerprints as [ CALL(GC.KeepAlive):void, NULL, PARAM(0):object ]
            Expression expr = Expression.Call(typeof(GC).GetMethod("KeepAlive"), Expression.Parameter(typeof(object)));

            // Act
            List<object> capturedConstants;
            ExpressionFingerprintChain fingerprint = FingerprintingExpressionVisitor.GetFingerprintChain(expr, out capturedConstants);

            // Assert
            Assert.Empty(capturedConstants);
            AssertChainEquals(fingerprint,
                              new MethodCallExpressionFingerprint(ExpressionType.Call, typeof(void), typeof(GC).GetMethod("KeepAlive")),
                              _nullFingerprint,
                              new ParameterExpressionFingerprint(ExpressionType.Parameter, typeof(object), 0 /* parameterIndex */));
        }

        [Fact]
        public void VisitParameter()
        {
            // Arrange

            // fingerprints as [ LAMBDA:Func<int, int, int>, OP_ADD:int, OP_ADD:int, OP_ADD:int, PARAM(0):int, PARAM(0):int, PARAM(1):int, PARAM(0):int, PARAM(1):int, PARAM(0):int ]
            // (note that the parameters are out of order since 'y' is used first, but this is ok due
            // to preservation of alpha equivalence within the VisitParameter method.)
            Expression expr = (Expression<Func<int, int, int>>)((x, y) => y + y + x + y);

            // Act
            List<object> capturedConstants;
            ExpressionFingerprintChain fingerprint = FingerprintingExpressionVisitor.GetFingerprintChain(expr, out capturedConstants);

            // Assert
            Assert.Empty(capturedConstants);
            AssertChainEquals(fingerprint,
                              new LambdaExpressionFingerprint(ExpressionType.Lambda, typeof(Func<int, int, int>)),
                              new BinaryExpressionFingerprint(ExpressionType.Add, typeof(int), null /* method */),
                              new BinaryExpressionFingerprint(ExpressionType.Add, typeof(int), null /* method */),
                              new BinaryExpressionFingerprint(ExpressionType.Add, typeof(int), null /* method */),
                              new ParameterExpressionFingerprint(ExpressionType.Parameter, typeof(int), 0 /* parameterIndex */),
                              new ParameterExpressionFingerprint(ExpressionType.Parameter, typeof(int), 0 /* parameterIndex */),
                              new ParameterExpressionFingerprint(ExpressionType.Parameter, typeof(int), 1 /* parameterIndex */),
                              new ParameterExpressionFingerprint(ExpressionType.Parameter, typeof(int), 0 /* parameterIndex */),
                              new ParameterExpressionFingerprint(ExpressionType.Parameter, typeof(int), 1 /* parameterIndex */),
                              new ParameterExpressionFingerprint(ExpressionType.Parameter, typeof(int), 0 /* parameterIndex */));
        }

        [Fact]
        public void VisitTypeBinary()
        {
            // Arrange

            // fingerprints as [ TYPEIS(DateTime):bool, CONST:string ]
            Expression expr = Expression.TypeIs(Expression.Constant("hello"), typeof(DateTime));

            // Act
            List<object> capturedConstants;
            ExpressionFingerprintChain fingerprint = FingerprintingExpressionVisitor.GetFingerprintChain(expr, out capturedConstants);

            // Assert
            Assert.Equal(new object[] { "hello" }, capturedConstants.ToArray());
            AssertChainEquals(fingerprint,
                              new TypeBinaryExpressionFingerprint(ExpressionType.TypeIs, typeof(bool), typeof(DateTime)),
                              new ConstantExpressionFingerprint(ExpressionType.Constant, typeof(string)));
        }

        [Fact]
        public void VisitUnary()
        {
            // Arrange

            // fingerprints as [ OP_NOT:int, PARAM:int ]
            Expression expr = Expression.Not(Expression.Parameter(typeof(int)));

            // Act
            List<object> capturedConstants;
            ExpressionFingerprintChain fingerprint = FingerprintingExpressionVisitor.GetFingerprintChain(expr, out capturedConstants);

            // Assert
            Assert.Empty(capturedConstants);
            AssertChainEquals(fingerprint,
                              new UnaryExpressionFingerprint(ExpressionType.Not, typeof(int), null /* method */),
                              new ParameterExpressionFingerprint(ExpressionType.Parameter, typeof(int), 0 /* parameterIndex */));
        }

        internal static void AssertChainEquals(ExpressionFingerprintChain fingerprintChain, params ExpressionFingerprint[] expectedElements)
        {
            ExpressionFingerprintChain newChain = new ExpressionFingerprintChain();
            newChain.Elements.AddRange(expectedElements);
            Assert.Equal(fingerprintChain, newChain);
        }
    }
}
