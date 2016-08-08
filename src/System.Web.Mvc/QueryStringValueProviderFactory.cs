// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Helpers;

namespace System.Web.Mvc
{
    public sealed class QueryStringValueProviderFactory : ValueProviderFactory
    {
        private readonly UnvalidatedRequestValuesAccessor _unvalidatedValuesAccessor;

        public QueryStringValueProviderFactory()
            : this(null)
        {
        }

        // For unit testing
        internal QueryStringValueProviderFactory(UnvalidatedRequestValuesAccessor unvalidatedValuesAccessor)
        {
            _unvalidatedValuesAccessor = unvalidatedValuesAccessor ?? (cc => new UnvalidatedRequestValuesWrapper(cc.HttpContext.Request.Unvalidated));
        }

        public override IValueProvider GetValueProvider(ControllerContext controllerContext)
        {
            if (controllerContext == null)
            {
                throw new ArgumentNullException("controllerContext");
            }

            return new QueryStringValueProvider(controllerContext, _unvalidatedValuesAccessor(controllerContext));
        }
    }
}
