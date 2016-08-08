// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;

namespace System.Web.Mvc.ExpressionUtil.Test
{
    // Represents an ExpressionFingerprint that is of the wrong type.
    internal sealed class DummyExpressionFingerprint : ExpressionFingerprint
    {
        public DummyExpressionFingerprint(ExpressionType nodeType, Type type)
            : base(nodeType, type)
        {
        }
    }
}
