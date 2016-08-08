// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Mvc.Filters
{
    /// <summary>
    /// Represents an authentication challenge context containing information for executing an authentication
    /// challenge.
    /// </summary>
    public class AuthenticationChallengeContext : ControllerContext
    {
        private ActionDescriptor _actionDescriptor;
        private ActionResult _result;

        /// <summary>Initializes a new instance of the <see cref="AuthenticationChallengeContext"/> class.</summary>
        /// <remarks>This constructor should only be used for unit testing purposes.</remarks>
        public AuthenticationChallengeContext()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="AuthenticationChallengeContext"/> class.</summary>
        /// <param name="controllerContext">The controller context.</param>
        /// <param name="actionDescriptor">The action descriptor.</param>
        /// <param name="result">The current action result.</param>
        public AuthenticationChallengeContext(ControllerContext controllerContext, ActionDescriptor actionDescriptor,
            ActionResult result)
            : base(controllerContext)
        {
            if (actionDescriptor == null)
            {
                throw new ArgumentNullException("actionDescriptor");
            }

            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            _actionDescriptor = actionDescriptor;
            _result = result;
        }

        /// <summary>Gets or sets the action descriptor.</summary>
        public ActionDescriptor ActionDescriptor
        {
            get
            {
                return _actionDescriptor;
            }
            set
            {
                _actionDescriptor = value;
            }
        }

        /// <summary>Gets or sets the action result to execute.</summary>
        /// <remarks>
        /// Authentication filters wishing to add an authentication challenge will often chain the result (set the
        /// value to an action result that executes the previous value and then does something additional, such as add
        /// a WWW-Authenticate header).
        /// </remarks>
        public ActionResult Result
        {
            get
            {
                return _result;
            }
            set
            {
                _result = value;
            }
        }
    }
}
