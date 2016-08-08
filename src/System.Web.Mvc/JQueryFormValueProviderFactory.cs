// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Mvc
{
    /// <summary>
    /// Provides the necessary ValueProvider to handle JQuery Form data.
    /// </summary>
    public sealed class JQueryFormValueProviderFactory : ValueProviderFactory
    {
        private readonly UnvalidatedRequestValuesAccessor _unvalidatedValuesAccessor;

        /// <summary>
        /// Constructs a new instance of the factory which provides JQuery form ValueProviders.
        /// </summary>
        public JQueryFormValueProviderFactory()
            : this(unvalidatedValuesAccessor: null)
        {
        }

        // For unit testing
        internal JQueryFormValueProviderFactory(UnvalidatedRequestValuesAccessor unvalidatedValuesAccessor)
        {
            _unvalidatedValuesAccessor = unvalidatedValuesAccessor ??
                                       (cc => new UnvalidatedRequestValuesWrapper(cc.HttpContext.Request.Unvalidated));
        }

        /// <summary>
        /// Returns the suitable ValueProvider.
        /// </summary>
        /// <param name="controllerContext">The context on which the ValueProvider should operate.</param>
        /// <returns></returns>
        public override IValueProvider GetValueProvider(ControllerContext controllerContext)
        {
            if (controllerContext == null)
            {
                throw new ArgumentNullException("controllerContext");
            }

            return new JQueryFormValueProvider(controllerContext, _unvalidatedValuesAccessor(controllerContext));
        }
    }
}
