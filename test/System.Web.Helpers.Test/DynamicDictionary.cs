// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Web.Helpers.Test
{
    /// <summary>
    /// Dynamic object implementation over a dictionary that doesn't implement anything but the interface.
    /// Used for testing our types that consume dynamic objects to make sure they don't make any assumptions on the implementation.
    /// </summary>
    public class DynamicDictionary : IDynamicMetaObjectProvider
    {
        private readonly Dictionary<string, object> _values = new Dictionary<string, object>();

        public object this[string name]
        {
            get
            {
                object result;
                _values.TryGetValue(name, out result);
                return result;
            }
            set { _values[name] = value; }
        }

        public DynamicMetaObject GetMetaObject(Expression parameter)
        {
            return new DynamicDictionaryMetaObject(parameter, this);
        }

        private class DynamicDictionaryMetaObject : DynamicMetaObject
        {
            private static readonly PropertyInfo ItemPropery = typeof(DynamicDictionary).GetProperty("Item");

            public DynamicDictionaryMetaObject(Expression expression, object value)
                : base(expression, BindingRestrictions.Empty, value)
            {
            }

            private IDictionary<string, object> WrappedDictionary
            {
                get { return ((DynamicDictionary)Value)._values; }
            }

            private Expression GetDynamicExpression()
            {
                return Expression.Convert(Expression, typeof(DynamicDictionary));
            }

            private Expression GetIndexExpression(string key)
            {
                return Expression.MakeIndex(
                    GetDynamicExpression(),
                    ItemPropery,
                    new[]
                    {
                        Expression.Constant(key)
                    }
                    );
            }

            private Expression GetSetValueExpression(string key, object value)
            {
                return Expression.Assign(
                    GetIndexExpression(key),
                    Expression.Convert(Expression.Constant(value),
                                       typeof(object))
                    );
            }

            public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
            {
                var binderDefault = binder.FallbackGetMember(this);

                var expression = Expression.Convert(GetIndexExpression(binder.Name),
                                                    typeof(object));

                var dynamicSuggestion = new DynamicMetaObject(expression, BindingRestrictions.GetTypeRestriction(Expression, LimitType)
                                                                              .Merge(binderDefault.Restrictions));

                return binder.FallbackGetMember(this, dynamicSuggestion);
            }

            public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
            {
                var binderDefault = binder.FallbackSetMember(this, value);

                Expression expression = GetSetValueExpression(binder.Name, value.Value);

                var dynamicSuggestion = new DynamicMetaObject(expression, BindingRestrictions.GetTypeRestriction(Expression, LimitType)
                                                                              .Merge(binderDefault.Restrictions));

                return binder.FallbackSetMember(this, value, dynamicSuggestion);
            }

            public override IEnumerable<string> GetDynamicMemberNames()
            {
                return WrappedDictionary.Keys;
            }
        }
    }
}
