// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Microsoft.TestCommon;

namespace System.Web.Mvc.ExpressionUtil.Test
{
    public class CachedExpressionCompilerTest
    {
        private delegate Func<TIn, TOut> Compiler<TIn, TOut>(Expression<Func<TIn, TOut>> expr);

        [Fact]
        public void Compiler_CompileFromConstLookup()
        {
            // Arrange
            Expression<Func<string, int>> expr = model => 42;
            var compiler = GetCompilerMethod<string, int>("CompileFromConstLookup");

            // Act
            var func = compiler(expr);
            int result = func("any model");

            // Assert
            Assert.Equal(42, result);
        }

        [Fact]
        public void Compiler_CompileFromFingerprint()
        {
            // Arrange
            Expression<Func<string, int>> expr = s => 20 * s.Length;
            var compiler = GetCompilerMethod<string, int>("CompileFromFingerprint");

            // Act
            var func = compiler(expr);
            int result = func("hello");

            // Assert
            Assert.Equal(100, result);
        }

        [Fact]
        public void Compiler_CompileFromIdentityFunc()
        {
            // Arrange
            Expression<Func<string, string>> expr = model => model;
            var compiler = GetCompilerMethod<string, string>("CompileFromIdentityFunc");

            // Act
            var func = compiler(expr);
            string result = func("hello");

            // Assert
            Assert.Equal("hello", result);
        }

        [Fact]
        public void Compiler_CompileFromMemberAccess_CapturedLocal()
        {
            // Arrange
            string capturedLocal = "goodbye";
            Expression<Func<string, string>> expr = _ => capturedLocal;
            var compiler = GetCompilerMethod<string, string>("CompileFromMemberAccess");

            // Act
            var func = compiler(expr);
            string result = func("hello");

            // Assert
            Assert.Equal("goodbye", result);
        }

        [Fact]
        public void Compiler_CompileFromMemberAccess_ParameterInstanceMember()
        {
            // Arrange
            Expression<Func<string, int>> expr = s => s.Length;
            var compiler = GetCompilerMethod<string, int>("CompileFromMemberAccess");

            // Act
            var func = compiler(expr);
            int result = func("hello");

            // Assert
            Assert.Equal(5, result);
        }

        [Fact]
        public void Compiler_CompileFromMemberAccess_StaticMember()
        {
            // Arrange
            Expression<Func<string, string>> expr = _ => String.Empty;
            var compiler = GetCompilerMethod<string, string>("CompileFromMemberAccess");

            // Act
            var func = compiler(expr);
            string result = func("hello");

            // Assert
            Assert.Equal("", result);
        }

        [Fact]
        public void Compiler_CompileSlow()
        {
            // Arrange
            Expression<Func<string, string>> expr = s => new StringBuilder(s).ToString();
            var compiler = GetCompilerMethod<string, string>("CompileSlow");

            // Act
            var func = compiler(expr);
            string result = func("hello");

            // Assert
            Assert.Equal("hello", result);
        }

        [Fact]
        public void Process()
        {
            // Arrange
            Expression<Func<string, string>> expr = s => new StringBuilder(s).ToString();

            // Act
            var func = CachedExpressionCompiler.Process(expr);
            string result = func("hello");

            // Assert
            Assert.Equal("hello", result);
        }

        // helper to create a delegate to a private method on the compiler
        private static Compiler<TIn, TOut> GetCompilerMethod<TIn, TOut>(string methodName)
        {
            Type openCompilerType = typeof(CachedExpressionCompiler).GetNestedType("Compiler`2", BindingFlags.NonPublic);
            Type closedCompilerType = openCompilerType.MakeGenericType(typeof(TIn), typeof(TOut));
            MethodInfo targetMethod = closedCompilerType.GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic);
            return (Compiler<TIn, TOut>)Delegate.CreateDelegate(typeof(Compiler<TIn, TOut>), targetMethod);
        }
    }
}
