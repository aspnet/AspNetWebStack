// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.SessionState;

namespace System.Web.Mvc
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class SessionStateAttribute : Attribute
    {
        public SessionStateAttribute(SessionStateBehavior behavior)
        {
            Behavior = behavior;
        }

        public SessionStateBehavior Behavior { get; private set; }
    }
}
