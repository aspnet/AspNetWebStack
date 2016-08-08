// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace System.Web.Mvc
{
    [AttributeUsage(ValidTargets, AllowMultiple = false, Inherited = false)]
    public abstract class CustomModelBinderAttribute : Attribute
    {
        internal const AttributeTargets ValidTargets = AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Interface | AttributeTargets.Parameter | AttributeTargets.Struct;

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "This method can potentially perform a non-trivial amount of work.")]
        public abstract IModelBinder GetBinder();
    }
}
