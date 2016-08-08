// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace System.Web.Mvc
{
    public class ResultExecutingContext : ControllerContext
    {
        // parameterless constructor used for mocking
        public ResultExecutingContext()
        {
        }

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "The virtual property setters are only to support mocking frameworks, in which case this constructor shouldn't be called anyway.")]
        public ResultExecutingContext(ControllerContext controllerContext, ActionResult result)
            : base(controllerContext)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            Result = result;
        }

        public bool Cancel { get; set; }

        public virtual ActionResult Result { get; set; }
    }
}
