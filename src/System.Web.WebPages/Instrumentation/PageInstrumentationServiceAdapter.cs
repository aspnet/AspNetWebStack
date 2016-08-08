// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Web.WebPages.Instrumentation
{
    internal partial class PageInstrumentationServiceAdapter
    {
        private static readonly Type _targetType = typeof(HttpContext).Assembly.GetType("System.Web.Instrumentation.PageInstrumentationService");
        private IReadOnlyList<PageExecutionListenerAdapter> _listenerAdapters;

        internal PageInstrumentationServiceAdapter()
        {
            Adaptee = _CallSite_ctor_2.Site();
        }

        internal PageInstrumentationServiceAdapter(object existing)
        {
            Adaptee = existing;
        }

        internal IReadOnlyList<PageExecutionListenerAdapter> ExecutionListeners
        {
            get
            {
                if (_listenerAdapters == null)
                {
                    IEnumerable<dynamic> inner = Adaptee.ExecutionListeners;
                    // Bug 235916: If we pass the type as an object, the callsite is limited to wherever the object is assigned to 
                    // dynamic which avoids private reflection issues in partial trust.
                    _listenerAdapters = inner.Select(listener => new PageExecutionListenerAdapter((object)listener)).ToList();
                }

                return _listenerAdapters;
            }
        }

        internal static bool IsEnabled
        {
            get { return _CallSite_IsEnabled_1.Getter(); }
            set { _CallSite_IsEnabled_1.Setter(value); }
        }

        internal dynamic Adaptee { get; private set; }

        private static class _CallSite_IsEnabled_1
        {
            public static Func<bool> Getter;
            public static Action<bool> Setter;

            [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Fields cannot be initialized at declaration")]
            static _CallSite_IsEnabled_1()
            {
                PropertyInfo prop = null;
                if (_targetType != null)
                {
                    prop = _targetType.GetProperty("IsEnabled", BindingFlags.Static | BindingFlags.Public, Type.DefaultBinder, typeof(bool), Type.EmptyTypes, new ParameterModifier[0]);
                }
                if (prop != null)
                {
                    Getter = Expression.Lambda<Func<bool>>(Expression.Property(null, prop)).Compile();
                    ParameterExpression value = Expression.Parameter(typeof(bool));
                    Setter = Expression.Lambda<Action<bool>>(
                        Expression.Assign(Expression.Property(null, prop), value), value).Compile();
                }
                else
                {
                    Getter = () => false;
                    Setter = _ =>
                    {
                    };
                }
            }
        }

        private static class _CallSite_ctor_2
        {
            public static Func<object> Site;

            [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Fields cannot be initialized at declaration")]
            static _CallSite_ctor_2()
            {
                if (_targetType != null)
                {
                    Site = Expression.Lambda<Func<object>>(
                        Expression.New(
                            _targetType.GetConstructor(new Type[] { })))
                        .Compile();
                }
                else
                {
                    Site = () => null;
                }
            }
        }

        // END Adaptor Infrastructure Code
    }
}
