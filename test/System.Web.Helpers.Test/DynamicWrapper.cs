// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Web.Helpers.Test
{
    /// <summary>
    /// Dynamic object implementation over a regualar CLR object. Getmember accesses members through reflection.
    /// </summary>
    public class DynamicWrapper : IDynamicMetaObjectProvider
    {
        private object _object;

        public DynamicWrapper(object obj)
        {
            _object = obj;
        }

        public DynamicMetaObject GetMetaObject(Expression parameter)
        {
            return new DynamicWrapperMetaObject(parameter, this);
        }

        private class DynamicWrapperMetaObject : DynamicMetaObject
        {
            public DynamicWrapperMetaObject(Expression expression, object value)
                : base(expression, BindingRestrictions.Empty, value)
            {
            }

            private object WrappedObject
            {
                get { return ((DynamicWrapper)Value)._object; }
            }

            private Expression GetDynamicExpression()
            {
                return Expression.Convert(Expression, typeof(DynamicWrapper));
            }

            private Expression GetWrappedObjectExpression()
            {
                FieldInfo fieldInfo = typeof(DynamicWrapper).GetField("_object", BindingFlags.NonPublic | BindingFlags.Instance);
                Debug.Assert(fieldInfo != null);
                return Expression.Convert(
                    Expression.Field(GetDynamicExpression(), fieldInfo),
                    WrappedObject.GetType());
            }

            private Expression GetMemberAccessExpression(string memberName)
            {
                return Expression.Property(
                    GetWrappedObjectExpression(),
                    memberName);
            }

            public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
            {
                var binderDefault = binder.FallbackGetMember(this);

                var expression = Expression.Convert(GetMemberAccessExpression(binder.Name), typeof(object));

                var dynamicSuggestion = new DynamicMetaObject(expression, BindingRestrictions.GetTypeRestriction(Expression, LimitType)
                                                                              .Merge(binderDefault.Restrictions));

                return binder.FallbackGetMember(this, dynamicSuggestion);
            }

            public override IEnumerable<string> GetDynamicMemberNames()
            {
                return (from p in WrappedObject.GetType().GetProperties()
                        orderby p.Name
                        select p.Name).ToArray();
            }
        }
    }
}
