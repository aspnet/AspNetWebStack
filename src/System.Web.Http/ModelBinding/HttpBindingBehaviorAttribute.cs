// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace System.Web.Http.ModelBinding
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "This class is designed to be overridden")]
    public class HttpBindingBehaviorAttribute : Attribute
    {
        private static readonly object _typeId = new object();

        public HttpBindingBehaviorAttribute(HttpBindingBehavior behavior)
        {
            Behavior = behavior;
        }

        public HttpBindingBehavior Behavior { get; private set; }

        public override object TypeId
        {
            get { return _typeId; }
        }
    }
}
