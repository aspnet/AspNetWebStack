// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Helpers;

namespace System.Web.Mvc
{
    public sealed class FormValueProviderFactory : ValueProviderFactory
    {
        private readonly UnvalidatedRequestValuesAccessor _unvalidatedValuesAccessor;

        public FormValueProviderFactory()
            : this(null)
        {
        }

        // For unit testing
        internal FormValueProviderFactory(UnvalidatedRequestValuesAccessor unvalidatedValuesAccessor)
        {
            _unvalidatedValuesAccessor = unvalidatedValuesAccessor ?? (cc => new UnvalidatedRequestValuesWrapper(cc.HttpContext.Request.Unvalidated));
        }

        public override IValueProvider GetValueProvider(ControllerContext controllerContext)
        {
            if (controllerContext == null)
            {
                throw new ArgumentNullException("controllerContext");
            }

            return new FormValueProvider(controllerContext, _unvalidatedValuesAccessor(controllerContext));
        }
    }
}
