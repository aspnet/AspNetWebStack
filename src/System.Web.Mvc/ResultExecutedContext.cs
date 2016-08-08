// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace System.Web.Mvc
{
    public class ResultExecutedContext : ControllerContext
    {
        // parameterless constructor used for mocking
        public ResultExecutedContext()
        {
        }

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "The virtual property setters are only to support mocking frameworks, in which case this constructor shouldn't be called anyway.")]
        public ResultExecutedContext(ControllerContext controllerContext, ActionResult result, bool canceled, Exception exception)
            : base(controllerContext)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            Result = result;
            Canceled = canceled;
            Exception = exception;
        }

        public virtual bool Canceled { get; set; }

        public virtual Exception Exception { get; set; }

        public bool ExceptionHandled { get; set; }

        public virtual ActionResult Result { get; set; }
    }
}
