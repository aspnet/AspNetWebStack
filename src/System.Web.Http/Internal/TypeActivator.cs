// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Linq.Expressions;

namespace System.Web.Http.Internal
{
    internal static class TypeActivator
    {
        public static Func<TBase> Create<TBase>(Type instanceType) where TBase : class
        {
            Contract.Assert(instanceType != null);
            NewExpression newInstanceExpression = Expression.New(instanceType);
            return Expression.Lambda<Func<TBase>>(newInstanceExpression).Compile();
        }

        public static Func<TInstance> Create<TInstance>() where TInstance : class
        {
            return Create<TInstance>(typeof(TInstance));
        }

        public static Func<object> Create(Type instanceType)
        {
            Contract.Assert(instanceType != null);
            return Create<object>(instanceType);
        }
    }
}
