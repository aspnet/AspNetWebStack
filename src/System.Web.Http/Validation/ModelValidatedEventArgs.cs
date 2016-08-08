// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Http.Controllers;

namespace System.Web.Http.Validation
{
    public sealed class ModelValidatedEventArgs : EventArgs
    {
        public ModelValidatedEventArgs(HttpActionContext actionContext, ModelValidationNode parentNode)
        {
            if (actionContext == null)
            {
                throw Error.ArgumentNull("actionContext");
            }

            ActionContext = actionContext;
            ParentNode = parentNode;
        }

        public HttpActionContext ActionContext { get; private set; }

        public ModelValidationNode ParentNode { get; private set; }
    }
}
