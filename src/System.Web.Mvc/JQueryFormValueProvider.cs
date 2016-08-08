// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Globalization;

namespace System.Web.Mvc
{
    /// <summary>
    /// The JQuery Form Value provider is used to handle JQuery formatted data in
    /// request Forms.
    /// </summary>
    public class JQueryFormValueProvider : NameValueCollectionValueProvider
    {
        /// <summary>
        /// Constructs a new instance of the JQuery form ValueProvider
        /// </summary>
        /// <param name="controllerContext">The context on which the ValueProvider operates.</param>
        public JQueryFormValueProvider(
                    ControllerContext controllerContext)
                : this(controllerContext,
                        new UnvalidatedRequestValuesWrapper(
                                controllerContext.HttpContext.Request.Unvalidated))
        {
        }

        // For unit testing
        internal JQueryFormValueProvider(
                        ControllerContext controllerContext,
                        IUnvalidatedRequestValues unvalidatedValues)
            : base(controllerContext.HttpContext.Request.Form, 
                        unvalidatedValues.Form,
                        CultureInfo.CurrentCulture,
                        jQueryToMvcRequestNormalizationRequired: true)
        {
        }
    }
}
