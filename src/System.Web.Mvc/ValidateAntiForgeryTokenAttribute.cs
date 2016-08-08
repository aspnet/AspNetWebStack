// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Web.Helpers;

namespace System.Web.Mvc
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class ValidateAntiForgeryTokenAttribute : FilterAttribute, IAuthorizationFilter
    {
        private string _salt;

        public ValidateAntiForgeryTokenAttribute()
            : this(AntiForgery.Validate)
        {
        }

        internal ValidateAntiForgeryTokenAttribute(Action validateAction)
        {
            Debug.Assert(validateAction != null);
            ValidateAction = validateAction;
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "AdditionalDataProvider", Justification = "API name.")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "AntiForgeryConfig", Justification = "API name.")]
        [Obsolete("The 'Salt' property is deprecated. To specify custom data to be embedded within the token, use the static AntiForgeryConfig.AdditionalDataProvider property.", error: true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string Salt
        {
            get { return _salt; }
            set
            {
                if (!String.IsNullOrEmpty(value))
                {
                    throw new NotSupportedException("The 'Salt' property is deprecated. To specify custom data to be embedded within the token, use the static AntiForgeryConfig.AdditionalDataProvider property.");
                }
                _salt = value;
            }
        }

        internal Action ValidateAction { get; private set; }

        public void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }

            ValidateAction();
        }
    }
}
