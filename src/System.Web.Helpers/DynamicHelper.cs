// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Microsoft.CSharp.RuntimeBinder;

namespace Microsoft.Internal.Web.Utils
{
    /// <summary>
    /// Helper to evaluate different method on dynamic objects
    /// </summary>
    internal static class DynamicHelper
    {
        // We must pass in "object" instead of "dynamic" for the target dynamic object because if we use dynamic, the compiler will
        // convert the call to this helper into a dynamic expression, even though we don't need it to be.  Since this class is internal,
        // it cannot be accessed from a dynamic expression and thus we get errors.

        // Dev10 Bug 914027 - Changed the first parameter from dynamic to object, see comment at top for details
        public static bool TryGetMemberValue(object obj, string memberName, out object result)
        {
            try
            {
                result = GetMemberValue(obj, memberName);
                return true;
            }
            catch (RuntimeBinderException)
            {
            }
            catch (RuntimeBinderInternalCompilerException)
            {
            }

            // We catch the C# specific runtime binder exceptions since we're using the C# binder in this case
            result = null;
            return false;
        }

        // Dev10 Bug 914027 - Changed the first parameter from dynamic to object, see comment at top for details
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We want to swallow exceptions that happen during runtime binding")]
        public static bool TryGetMemberValue(object obj, GetMemberBinder binder, out object result)
        {
            try
            {
                // VB us an instance of GetBinderAdapter that does not implement FallbackGetMemeber. This causes lookup of property expressions on dynamic objects to fail.
                // Since all types are private to the assembly, we assume that as long as they belong to CSharp runtime, it is the right one. 
                if (typeof(Binder).Assembly.Equals(binder.GetType().Assembly))
                {
                    // Only use the binder if its a C# binder.
                    result = GetMemberValue(obj, binder);
                }
                else
                {
                    result = GetMemberValue(obj, binder.Name);
                }
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        // Dev10 Bug 914027 - Changed the first parameter from dynamic to object, see comment at top for details
        public static object GetMemberValue(object obj, string memberName)
        {
            var callSite = GetMemberAccessCallSite(memberName);
            return callSite.Target(callSite, obj);
        }

        // Dev10 Bug 914027 - Changed the first parameter from dynamic to object, see comment at top for details
        public static object GetMemberValue(object obj, GetMemberBinder binder)
        {
            var callSite = GetMemberAccessCallSite(binder);
            return callSite.Target(callSite, obj);
        }

        // dynamic d = new object();
        // object s = d.Name;
        // The following code gets generated for this expression:
        // callSite = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "Name", typeof(Program), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
        // callSite.Target(callSite, d);
        // typeof(Program) is the containing type of the dynamic operation.
        // Dev10 Bug 914027 - Changed the callsite's target parameter from dynamic to object, see comment at top for details
        public static CallSite<Func<CallSite, object, object>> GetMemberAccessCallSite(string memberName)
        {
            var binder = Binder.GetMember(CSharpBinderFlags.None, memberName, typeof(DynamicHelper), new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) });
            return GetMemberAccessCallSite(binder);
        }

        // Dev10 Bug 914027 - Changed the callsite's target parameter from dynamic to object, see comment at top for details
        public static CallSite<Func<CallSite, object, object>> GetMemberAccessCallSite(CallSiteBinder binder)
        {
            return CallSite<Func<CallSite, object, object>>.Create(binder);
        }

        // Dev10 Bug 914027 - Changed the first parameter from dynamic to object, see comment at top for details
        public static IEnumerable<string> GetMemberNames(object obj)
        {
            var provider = obj as IDynamicMetaObjectProvider;
            Debug.Assert(provider != null, "obj doesn't implement IDynamicMetaObjectProvider");

            Expression parameter = Expression.Parameter(typeof(object));
            return provider.GetMetaObject(parameter).GetDynamicMemberNames();
        }
    }
}
